using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using BudEngine.NetEngine;
using BudEngine.NetEngine.src;
using BudEngine.NetEngine.src.Net;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SavingData;

public partial class ClientManager : MonoBehaviour
{
    public enum LoadingState
    {
        Start,//unity被拉起
        SessionInRequest,//请求匹配信息中
        SessionRequestCompleted,//请求匹配信息完成
        SverConnecting,//连接服务器中
        SverConnected,//连接服务器完成
    }
    public static ClientManager Inst;
    public PlayerBaseControl selfPlayerCom;
    public PlayerEmojiControl selfPlayerEmoji;
    private GameObject OtherPlayerNode;
    private GameObject OtherPlayerPrefab;
    #region FrameStepData
    private Vector3 PlayerPos = new Vector3();
    private Quaternion PlayerRot = new Quaternion();
    private bool IsMoving;
    private bool IsGround;
    private bool IsFlying;
    private bool IsFastRun;
    private bool IsInWater;
    private bool IsSwimming;
    private int AnimType;
    private int StateType;
    private string selfDiyMapId = "";
    #endregion
    private ServerPacket serRep = null;

    private EnterRoomMode rMode = EnterRoomMode.Public;

    #region FrameStepConst
    public int MAX_PLAYER = 8;
    public bool isOnline = true;//用以记录房间socket是否断线过,慎用
    private bool lastFrameOnline = true;//用以记录帧socket是否断线过,慎用
    private bool isStartFrameSyncSuccess = false;//用以StartFrameSync是否成功,慎用
    private object roomUpdateLock = new object();//双心跳修改后，RoomUpdate锁防止多次调用断线重连
    private int StartFrameRetryTimes = 0;
    private int LEAVE_TYR_MAX_TIMES = 3;
    private int leaveTryTime = 0;
    private bool isForceExit = false;//是否强制退出
    private bool isEnterRoom = false;//是否已经执行了进房操作
    private bool isCalledShowFrame = false;//是否唤起第一帧
    private float callFrameTimeOut = 10;
    private SessionInfo SessionInfo = new SessionInfo();
    #endregion

    public string SessionId = "";
    private string IpAddress = "";
    public string roomLang = "";
    public string CountryCode = "";
    private int Port;
    private int FramePort;
    private const int MAX_RETRY_TIMES = 2;
    private int CurRetryTime = 0;
    private string ServerRoomId; //GetSession获取到的RoomId

    private LoadingState mLoadingState= LoadingState.Start;
    public Dictionary<string, OtherPlayerCtr> otherPlayerDataDic = new Dictionary<string, OtherPlayerCtr>();
    #region ClientData
    [HideInInspector]
    public string roomCode = "";
    #endregion

    #region RoomUpdateParam
    private int LostTime = 0;
    private const int MaxLostTime = 5;
    private TimeSpan CurTime;
    private TimeSpan DeltaTime;
    #endregion

    #region 帧数据统计上报

    private Int64 frameSendedCount = 0; //发出了多少帧
    private Int64 frameRecvCount = 0; //收到了多少帧

    #endregion

    public bool IsBackground = false;

    private void Awake()
    {
        Inst = this;
        mLoadingState = LoadingState.Start;
        // 清除所有牵手队组
        MutualManager.Inst.ClearHoldingHandsPlayers();
        MainThreadDispatcher.Init();
        PlayerLatestPosMgr.Inst.Init();
        PlayerNetworkManager.Inst.Init();
    }

    private void AddAction(Action cb)
    {
        MainThreadDispatcher.Enqueue(cb);
    }

    private void ClearActionList()
    {

    }

    /// <summary>
    /// 游玩模式初始化联机相关信息
    /// </summary>
    public void InitSyncData()
    {
        OtherPlayerNode = new GameObject("OtherPlayerNode");
        OtherPlayerPrefab = ResManager.Inst.LoadRes<GameObject>("MMO/OtherPlayerPrefab");
        GameManager.Inst.LoadMapAsyncCount++;
        if(GameManager.Inst.LoadMapAsyncCount >= 2){
            InitRoom();
        }    
    }
    
    
    public void RetryEnterRoom()
    {
        LoggerUtils.LogReport("ClientManager RetryEnterRoom","RetryEnterRoom");
        GetSessionInfo((isSuccess) =>
        {
            if (isSuccess)
            {
                InitSDK();
            }
            else
            {
                EnterOfflineMode();
            }
        });
    }

    /// <summary>
    /// Get Session 自动重试三次
    /// </summary>
    /// <param name="callback"></param>
    public void GetSessionInfo(Action<bool> callback = null){
        //所需参数
#if !UNITY_EDITOR
        string mapId = GameManager.Inst.gameMapInfo.mapId;
        string appVersion = GameManager.Inst.unityConfigInfo.appVersion;
        string rCode = GameManager.Inst.onLineDataInfo.roomCode;
        int isPrivate = GameManager.Inst.onLineDataInfo.isPrivate;
        MAX_PLAYER = GameManager.Inst.gameMapInfo.maxPlayer;
        GlobalFieldController.pvpData = default;
        if (!string.IsNullOrEmpty(GameManager.Inst.gameMapInfo.pvpData))
        {
            GlobalFieldController.pvpData = JsonConvert.DeserializeObject<PVPData>(GameManager.Inst.gameMapInfo.pvpData);
        }
        string isPvp = (GlobalFieldController.pvpData.pvpMode == 1) ? "1" : "0";

        switch ((ROOM_MODE)GameManager.Inst.onLineDataInfo.roomMode)
        {
            case ROOM_MODE.MATCH:
                rMode = EnterRoomMode.Public;
                break;
            case ROOM_MODE.CREATE:
                rMode = EnterRoomMode.Private;
                break;
            case ROOM_MODE.JOIN:
                rMode = isPrivate == 1 ? EnterRoomMode.Private : EnterRoomMode.Public;
                break;
        }
        GetSessionReq getSessionReq = new GetSessionReq()
        {
            roomType = mapId + "|" + appVersion,
            isPrivate = (int)rMode,
            roomCode = rCode,
            maxPlayerCount = MAX_PLAYER,
            isPvp = isPvp,
        };

        DataLogUtils.LogUnityGetGameSessionReq(CurRetryTime);

        mLoadingState = LoadingState.SessionInRequest;

        LoggerUtils.Log("######进房流程 GetSessionInfo start");
        LoggerUtils.LogReport("GetSessionInfo getGameSession Request","Unity_getGameSession_Req");
        HttpUtils.MakeHttpRequest("/engine-match/getGameSession", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(getSessionReq), (content) =>
        {
            LoggerUtils.Log("######进房流程 异步！： GetSessionInfo Success:"+content);
            HttpResponDataStruct httpResponData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
            SessionInfo = JsonConvert.DeserializeObject<SessionInfo>(httpResponData.data);
            IpAddress = SessionInfo.ipAddress;
            SessionId = SessionInfo.sessionId;
            Port = SessionInfo.port;
            FramePort = SessionInfo.framePort;
            ServerRoomId = SessionInfo.roomId;
            roomLang = SessionInfo.roomLang;
            CountryCode = SessionInfo.countryCode;
            LoggerUtils.LogReport("GetSessionInfo getGameSession CurRetryTime ="+CurRetryTime.ToString(),"Unity_getGameSession_Rep");
            LoggerUtils.Log("IpAddress = " + IpAddress + " SessionInfo = " + SessionId + " Port = " + Port + " FramePort = " + FramePort + " RoomId = " + ServerRoomId + "roomLang = " + roomLang + "CountryCode = " + CountryCode);
            AddAction(()=>
            {
                GameManager.Inst.PlayerSpawnId = SessionInfo.spawnIdx;
                CurRetryTime = 0;
                DataLogUtils.LogUnityGetGameSessionRsp("0", CurRetryTime);
                mLoadingState = LoadingState.SessionRequestCompleted;
                callback?.Invoke(true);
            }); 

    
        }, (content) =>
        {
            AddAction(()=>{
                mLoadingState = LoadingState.SessionRequestCompleted;
                SavingData.HttpResponseRaw httpResponseRaw = GameUtils.GetHttpResponseRaw(content);
                DataLogUtils.LogUnityGetGameSessionRsp(httpResponseRaw.result.ToString(), CurRetryTime);
                LoggerUtils.LogError("Script:ClientManager Get SessionInfo error = " + content);
                // RetryEnterRoom();
                if (CurRetryTime >= MAX_RETRY_TIMES)
                {
                    CurRetryTime = 0;
                    LoggerUtils.Log("******************RetryEnterRoom End, EnterOfflineMode******************");
                    callback?.Invoke(false);
                }
                else
                {
                    CurRetryTime++;
                    LoggerUtils.Log(string.Format("******************RetryEnterRoom CurRetryTime:{0}******************", CurRetryTime));
                    GetSessionInfo(callback);
                }
            });
           
        });     
#else
        UnityLocalTest_GetTestNetSession(callback);
#endif
    }


    //进入离线单机模式
    public void EnterOfflineMode()
    {
        LoggerUtils.LogReport("ClientManager EnterOfflineMode","EnterOfflineMode");
        // 单机模式再次拉取人物形象
        PlayerBaseControl.Inst.InitUserInfo();
        if(isCalledShowFrame == false){
            StopCoroutine("CallShowFrame");
            StartCoroutine("CallShowFrame");
        }
    }

    public void InitRoom()
    {
        isCalledShowFrame = false;
        LoggerUtils.Log("######进房流程 InitRoom");
        LoggerUtils.LogReport("ClientMgr Initialization succeeded","Unity_InitRoom");
        DataLogUtils.LogUnityInitSdkSuccess(CurRetryTime);
        Global.Room = new Room(null);
        Listener.Add(Global.Room);
        InitBroadcast();
        EnterRoom();
        StartEnterTimer();
    }

    public void InitSDK()
    {
        DataLogUtils.LogUnityInitSdkStart();
        LoggerUtils.Log("######进房流程 InitSDK");
        LoggerUtils.LogReport("Start InitSDK","InitSDK");
        if (Global.Room != null)
        {
            LoggerUtils.Log("ClientMgr already init");
            AddAction(() => { DestroyAllOtherPlayer(); });
            Release();
        }
        leaveTryTime = 0;
        isForceExit = false;
        isEnterRoom = false;
#if !UNITY_EDITOR
        LoggerUtils.Log("userInfo.uid = " + GameManager.Inst.baseGameJsonData.baseInfo.uid);
        GameInfoPara gamecfg = new GameInfoPara
        {
            OpenId = GameManager.Inst.baseGameJsonData.baseInfo.uid
        };
        selfDiyMapId = GameManager.Inst.gameMapInfo.mapId;
#else
        GameInfoPara gamecfg = new GameInfoPara
        {
            // OpenId = UnityEngine.Random.Range(1, 10000).ToString()
            OpenId = TestNetParams.testHeader.uid
        };
        selfDiyMapId = TestNetParams.Inst.CurrentConfig.testMapId;
#endif
        ConfigPara config = new ConfigPara
        {
            Url = IpAddress,
            ReconnectMaxTimes = 5,
            ReconnectInterval = 3000,
            ResendInterval = 1000000,
            ResendTimeout = 10000000,
            Port = Port,
            FramePort = FramePort,
        };
        LoggerUtils.LogReport("InitSDK Listener.Init","InitListener");
        // 初始化监听器 Listener
        Listener.Init(gamecfg, config, (ResponseEvent eve) =>
        {
            LoggerUtils.Log("######进房流程 InitSDK success");
            LoggerUtils.LogReport("Listener.Init Success eve.Code=" + eve.Code,"InitSDK_Callback");
            if (eve.Code == 0)
            {
                AddAction(() =>
                {  
                    GameManager.Inst.LoadMapAsyncCount++;
                    LoggerUtils.LogReport("Listener.Init Success GameManager.Inst.LoadMapAsyncCount=" +
                                          GameManager.Inst.LoadMapAsyncCount.ToString(),"InitSDK_Callback_AddAction");
                    if(GameManager.Inst.LoadMapAsyncCount >= 2){
                        InitRoom();
                    }
                });
            }
            else
            {
                AddAction(() =>
                {
                    LoggerUtils.Log("ClientMgr Initialization Fail:" + eve.Code);
                    //ForceExitPanel.Show();
                    RetryEnterRoom();
                });
            }
        });
    }

    public void SendGetItems()
    {
#if !UNITY_EDITOR
        if (!ClientManager.Inst.isOnline || GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            LoggerUtils.LogError("[SendGetItems] not in room or guest mode");
            return;
        }
#else
        if (!ClientManager.Inst.isOnline)
        {
            LoggerUtils.Log("[SendGetItems] not in room");
            return;
        }
#endif

        GetItemsReq getItemsReq = new GetItemsReq();
#if UNITY_EDITOR
        getItemsReq.mapId = TestNetParams.Inst.CurrentConfig.testMapId;
        getItemsReq.bigMap = GlobalFieldController.IsDowntownEnter ? 1 : 0;
#else
        if (GlobalFieldController.CurMapInfo == null)
        {
            LoggerUtils.LogError("[SendGetItems] GlobalFieldController.CurMapInfo is null");
            return;
        }
        getItemsReq.mapId = GlobalFieldController.CurMapInfo.mapId;
        getItemsReq.bigMap = GlobalFieldController.IsDowntownEnter ? 1 : 0;
#endif

        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.GetItems,
            data = JsonConvert.SerializeObject(getItemsReq),
        };
        LoggerUtils.LogReport("[SendGetItems] SendRequest =>" + JsonConvert.SerializeObject(roomChatData),"EnterRoom_SendItem");
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    
    }


    public void SendRequest(string sendData, Action<int, string> callBack = null)
    {
        if (isEnterRoom == false)
        {
            return;
        }
        List<string> recvPlayerList = new List<string>();

        for (int i = 0; i < Global.Room.RoomInfo.PlayerList.Count; i++)
        {
            recvPlayerList.Add(Global.Room.RoomInfo.PlayerList[i].Id);
        }

        var roomChatData = JsonConvert.DeserializeObject<RoomChatData>(sendData);
        var requestReq = string.Format("{0}_{1}_{2}", Global.Room.RoomInfo.Id, GameUtils.GetTimeStamp(), Player.Id);
        roomChatData.requestSeq = requestReq;
        
        SendToClientPara para = new SendToClientPara()
        {
            PlayerId = Player.Id,
            RoomId = Global.Room.RoomInfo.Id,
            Msg = JsonConvert.SerializeObject(roomChatData),
            RecvPlayerList = recvPlayerList
        };

        if (!PlayerNetworkManager.Inst.CheckPlayerCanSendReq(roomChatData))
        {
            return;
        }

        #region RoomChat埋点

        if ((RecChatType)roomChatData.msgType == RecChatType.TextChat || (RecChatType)roomChatData.msgType == RecChatType.Emo)
        {
            MobileInterface.Instance.LogRoomChatEventByEventName(LogEventData.unity_roomchat_req, requestReq, roomChatData.msgType);
        }
        
        #endregion

        Global.Room.RoomChat(para, eve =>
        {
            LoggerUtils.LogReport("[SendGetItems] SendRequest Rsp =>" + JsonConvert.SerializeObject(para));
            AddAction(() =>
            {
                callBack?.Invoke(eve.Code, eve.Data.ToString());
                OnServerRsp(eve.Code,eve);
            });        
        });
    }


    private void OnServerRsp(int code,ResponseEvent eve)
    {
        LoggerUtils.LogReport("OnServerRsp" + eve.Data?.ToString());
        LoggerUtils.Log("OnServerRsp" + eve.Data?.ToString());
        if (eve.Code != 0)
        {
            LoggerUtils.Log("OnServerRsp error " + eve.Code);
            return;
        }

        NetMessageHelper.Inst.RoomChatCallBack(eve);
        SendToClientRsp rsp = (SendToClientRsp)eve.Data;

        #region RoomChat埋点
        ServerRspLogFirebase(eve);
        #endregion


        if (rsp != null && (RecChatType)rsp.MsgType == RecChatType.GetItems)
        {
            try
            {
                if (IsBackground)
                {
                    IsBackground = false;
                }
                LoggerUtils.Log("#####GetItems 成功" + eve.Data?.ToString());
                var time1 = GameUtils.GetSystemTime();
                GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(rsp.Data);
                OnGetItemsRsp(getItemsRsp);
                var time2 = GameUtils.GetSystemTime();
                LoggerUtils.Log("#####解析GetItems 耗时：" + (time2 - time1));
            }
            catch (Exception error)
            {
                LoggerUtils.LogError("Crash => |uid = " + GameManager.Inst.ugcUserInfo.uid + " |Time = " + GameUtils.GetTimeStamp() + " |rsp.Data = " + rsp.Data + " |error = " + error);
                return;
            }
        }
        
    }
    
    //聊天和表情埋点，此处服务器返回的数据格式不一致，待后续版本统一。
    private void ServerRspLogFirebase(ResponseEvent eve){
        SendToClientRsp rsp = (SendToClientRsp)eve.Data;
        if (rsp != null && (RecChatType)rsp.MsgType == RecChatType.TextChat)
        {
            LoggerUtils.Log("RoomChat text rsp:--->" + rsp.Data);
            if (!string.IsNullOrEmpty(rsp.Data))
            {
                MobileInterface.Instance.LogRoomChatEventByEventName(LogEventData.unity_roomchat_rsp,rsp.Data,rsp.MsgType,eve.Code.ToString());
            }
        }
        else if (rsp != null && (RecChatType)rsp.MsgType == RecChatType.Emo)
        {
            LoggerUtils.Log("RoomChat emo rsp:--->" + rsp.Data);
            
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(rsp.Data);
            if (getItemsRsp != null && !string.IsNullOrEmpty(getItemsRsp.requestSeq))
            {
                MobileInterface.Instance.LogRoomChatEventByEventName(LogEventData.unity_roomchat_rsp,getItemsRsp.requestSeq,rsp.MsgType,eve.Code.ToString());
            }
        }
    }

    //获取地图逻辑信息回包
    private void OnGetItemsRsp(GetItemsRsp getItemsRsp)
    {
        LoggerUtils.LogReport("OnGetItemsRsp getItemsRsp="+(getItemsRsp == null).ToString(),"EnterRoom_OnGetItemsRsp_Start");
        if (getItemsRsp == null) return;
#if !UNITY_EDITOR
        if (getItemsRsp.mapId != GlobalFieldController.CurMapInfo.mapId)
        {
            LoggerUtils.LogError(string.Format("getItemsRsp.mapId is not Current Map id. GetItem Mapid =>{0} , CurrentMapId=>{1}", getItemsRsp.mapId, GlobalFieldController.CurMapInfo.mapId));
            return;
        }
#endif
        
        InitPlayerData(getItemsRsp.playerCustomDatas);
        InitRoomData(getItemsRsp.roomAttrs);
        LoggerUtils.LogReport("OnGetItemsRsp isCalledShowFrame="+isCalledShowFrame,"EnterRoom_OnGetItemsRsp_End");
        if (isCalledShowFrame == false)
        {
            StopCoroutine("CallShowFrame");
            StartCoroutine("CallShowFrame");
            MessageHelper.Broadcast(MessageName.StartGameOnLine);
        }
    }

    private void InitPlayerData(PlayerCustomData[] playerCustomData)
    {
        if(playerCustomData == null || playerCustomData.Length <= 0) return;
        for (int i = 0; i < playerCustomData.Length; i++)
        {
            PlayerCustomData data = playerCustomData[i];
            if (data == null || string.IsNullOrEmpty(data.playerId)) break;
            IPlayerController iPlayerController = data.playerId == Player.Id ?
                (IPlayerController)PlayerEmojiControl.Inst : GetOtherPlayerComById(data.playerId);

            if (iPlayerController != null)
                iPlayerController.OnGetPlayerCustomData(data);
        }   
    }

    private void InitRoomData(RoomAttr[] roomAttrs)
    {
        if(roomAttrs == null) return;
        
        for (int i = 0; i < roomAttrs.Length; i++)
        {
            RoomAttr item = roomAttrs[i];
            string itemDataJson = item.data;
            if(item.data == null){
                continue;
            }
      
            if(item.type == (int)RoomAttrType.SPAWN){//出生点序号
                IndexData itemData = JsonConvert.DeserializeObject<IndexData>(item.data);
                if (itemData.spawnType == 1 && !string.IsNullOrEmpty(itemData.position))
                {
                    var pos = JsonConvert.DeserializeObject<Vec3>(itemData.position);
                    SetPlayerLastPosition(pos);
                }
                else
                {
                    LoggerUtils.Log("出生点序号：" + itemData.index);
                    if (GameManager.Inst.PlayerSpawnId == 0)
                    {
                        LoggerUtils.Log("修改出生点序号：" + itemData.index);
                        GameManager.Inst.PlayerSpawnId = itemData.index;
                        SetSelfPlayerPosition();
                    }
                }
            }
        }
    }

    private void SetSelfPlayerPosition()
    {
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null)
        {
            PlayerBaseControl.Inst.SetPlayerPosAndRot(PVPWaitAreaManager.Inst.PVPBehaviour.transform.position, Quaternion.identity);
            PlayerBaseControl.Inst.DontHandleMove();
        }
        else
        {
            PlayerBaseControl.Inst.SetPosToSpawnPoint();
        }
    }

    private void SetPlayerLastPosition(Vector3 pos)
    {
        if(PlayerBaseControl.Inst != null)
        {
            PlayerBaseControl.Inst.SetPosToNewPoint(pos, Quaternion.identity);
        }
    }

    public void TestRoomChat()
    {
        RoomChatData roomchatdata = new RoomChatData()
        {
            msgType = (int)RecChatType.TextChat,
            data = "Hello Hello Mingo"
        };

        SendRequest(JsonConvert.SerializeObject(roomchatdata));
    }

    //切后台时刷一下玩家位置，防止冰方块等玩法玩家切后台后还在持续移动
    private void OnBackGroundPlayerReset()
    {
        if (PlayerBaseControl.Inst)
        {
            PlayerBaseControl.Inst.Move(Vector3.zero);
        }
    }

    public void OnApplicationPause(bool pauseStatus)
    {
        RealTimeTalkManager.Inst.OnPause(pauseStatus);
        string code = isEnterRoom ? "1" : "0";
        if (pauseStatus)
        {
            //切换到后台时执行
             LoggerUtils.Log("切换到后台 ");
            if (GlobalFieldController.CurGameMode == GameMode.Guest)
            {
                LoggerUtils.LogReport("OnApplicationPause Background", "Unity_Background");
                MobileInterface.Instance.LogEventByEventName(LogEventData.unity_background, code);
                OnBackGroundPlayerReset();
                SendBackgroundAction();
            }
            IsBackground = true;
        }
        else
        {
            //切换到前台时执行，游戏启动时执行一次
            LoggerUtils.Log("切换到前台 ");
            MessageHelper.Broadcast(MessageName.OnForeground);
            
            if (GlobalFieldController.CurGameMode == GameMode.Guest)
            {
                LoggerUtils.LogReport("OnApplicationPause Foreground isEnterRoom="+isEnterRoom.ToString(), "Unity_Foreground");
                MobileInterface.Instance.LogEventByEventName(LogEventData.unity_foreground, code);
                SendForegroundAction();
            }

            //切换到前台时执行，游戏启动时执行一次
            //强制清零心跳重试次数，用于快速重连
            if (isEnterRoom)
            {
                LoggerUtils.Log("切换到前台时执行 ");
                if (PVPWaitAreaManager.Inst.PVPBehaviour != null)
                {
                    //切后台回来避免PVP重置，需要丢弃PVP信息，通过GetItem重置地图
                    PVPManager.Inst.IsBackgroundMsg = true;
                    //避免PVP消息锁住
                    Invoke("OnForceBackgroundChange",5);
                }

                Core.Pinger1.Retry = 0;
                var para = new GetRoomByRoomIdPara
                {
                    RoomId = roomCode,
                    SessionId = SessionId,
                    PlayerId = Player.Id
                };
                Room.GetMyRoom(para, eve =>
                {
                    if (eve.Code == 0)
                    {
                        AddAction(() =>
                        {
                            var data = (GetRoomByRoomIdRsp)eve.Data;
                            Global.Room.InitRoom(data.RoomInfo);
                            UpdatePlayers();
                            //PVP状态切换会重置地图，GetItem需要在重置地图后执行
                            if (PVPWaitAreaManager.Inst.PVPBehaviour == null)
                            {
                                SendGetItems();
                            }
                            else
                            {
                                PVPManager.Inst.PVPGetGameInfo(SendGetItems);
                            }
                            ChatPanelManager.Inst.SendGetMsgs();//拉取聊天信息
                        });
                    }
                });
            }
        }
    }

    private void OnForceBackgroundChange()
    {
        PVPManager.Inst.IsBackgroundMsg = false;
    }

    private IEnumerator CallShowFrame()
    {
        LoggerUtils.LogReport("ClientManager CallShowFrame","CallShowFrame");
        isCalledShowFrame = true;
        yield return FindObjectOfType<GameController>()?.WaitForFrameAndShow(true);
    }

    private void StartCollectFPS()
    {
        FPSController.Inst.StartCollectFPS();
    }

    private void EndCollectFPS()
    {
        float fps = FPSController.Inst.GetAverageFPS();
        GlobalFieldController.isOpenPostProcess = fps > GameConsts.averageFPS;
        SceneBuilder.Inst.PostProcessBehaviour.SetPostProcessActive(GlobalFieldController.isOpenPostProcess);
    }

    public void StartEnterTimer(){
        LoggerUtils.Log("#######ClientMgr StartEnterTimer");
        Invoke("EnterOfflineMode", callFrameTimeOut);
    }
    
    private void Release()
    {
        StopClientSendFrame();
        Global.UnInit();
        Global.Room = null;
    }

    private void OnDestroy()
    {
        Release();//多释放一次，确保释放成功
        GameServer.Release();
        Inst = null;
    }

    #region Unity测试使用
#if UNITY_EDITOR
    public void UnityLocalTest_GetTestNetSession(Action<bool> callback)
    {
        LoggerUtils.Log("Get Game Session Start");
        rMode = (EnterRoomMode)TestNetParams.Inst.CurrentConfig.isPrivate;
        MAX_PLAYER = GameManager.Inst.gameMapInfo.maxPlayer;
        string isPvp = "";
        if (GameManager.Inst.gameMapInfo != null)
        {
            if (!string.IsNullOrEmpty(GameManager.Inst.gameMapInfo.pvpData))
            {
                GlobalFieldController.pvpData = JsonConvert.DeserializeObject<PVPData>(GameManager.Inst.gameMapInfo.pvpData);
                isPvp = (GlobalFieldController.pvpData.pvpMode == 1) ? "1" : "0";
            }
        }

        GetSessionReq getSessionReq = new GetSessionReq()
        {
            roomType = string.Format("{0}|{1}", TestNetParams.Inst.CurrentConfig.testMapId, TestNetParams.testHeader.version),
            isPrivate = TestNetParams.Inst.CurrentConfig.isPrivate,   // 0-公共 1-私人 暂未使用
            roomCode = TestNetParams.Inst.CurrentConfig.roomCode,
            maxPlayerCount = MAX_PLAYER,
            isPvp = isPvp
        };
        
        LoggerUtils.Log("##getGameSession start");
        // HttpUtils.UnityLocalTest_MakeTestHttpRequest("/engine-match/getGameSession", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(getSessionReq), (content) =>
        HttpUtils.UnityLocalTest_MakeTestHttpRequest("/engine-match/getGameSession", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(getSessionReq), (content) =>
        {
            LoggerUtils.Log("##getGameSession success:"+content);
            HttpResponDataStruct roleResponseData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
            SessionInfo = JsonConvert.DeserializeObject<SessionInfo>(roleResponseData.data);
            IpAddress = string.IsNullOrEmpty(TestNetParams.Inst.GetConnectIp()) ? SessionInfo.ipAddress : TestNetParams.Inst.GetConnectIp();
            SessionId = SessionInfo.sessionId;
            Port = SessionInfo.port;
            FramePort = SessionInfo.framePort;
            ServerRoomId = SessionInfo.roomId;
            roomLang = SessionInfo.roomLang;
            CountryCode = SessionInfo.countryCode;
            LoggerUtils.Log("IpAddress = " + IpAddress + " SessionInfo = " + SessionId + " Port = " + Port + " FramePort = " + FramePort + " RoomId = " + ServerRoomId + "roomLang = " + roomLang + "CountryCode = " + CountryCode);
            AddAction(() =>
            {
                GameManager.Inst.PlayerSpawnId = SessionInfo.spawnIdx;
                callback?.Invoke(true);
            });
        }, (content) =>
        {
            AddAction(() =>
            {
                LoggerUtils.LogError("Script:ClientManager Get SessionInfo error = " + content);
                if (CurRetryTime >= MAX_RETRY_TIMES)
                {
                    CurRetryTime = 0;
                    LoggerUtils.Log("******************RetryEnterRoom End, EnterOfflineMode******************");
                    callback?.Invoke(false);
                }
                else
                {
                    CurRetryTime++;
                    LoggerUtils.Log(string.Format("******************RetryEnterRoom CurRetryTime:{0}******************", CurRetryTime));
                    UnityLocalTest_GetTestNetSession(callback);
                }
            });
  
        });
    }

    private void OnApplicationQuit()
    {
        LoggerUtils.LogReport("OnApplicationQuit");
        BudEngine.NetEngine.src.Util.Debugger.SaveLogFile();
    }
#endif
    #endregion

    #region LoadingReturn
    public void RequestCloseLoading()
    {
        LoggerUtils.Log($"RequestCloseLoading   {mLoadingState}");
        switch (mLoadingState)
        {
            case LoadingState.Start:
                break;
            case LoadingState.SessionInRequest:
            case LoadingState.SessionRequestCompleted:
            case LoadingState.SverConnecting:
                SendMsg2ClientWithCloseSeesion();
                break;
            case LoadingState.SverConnected:
                LeaveRoom();
                break;
            default:
                break;
        }
        OfflineResManager.Inst.StopLoader();
        MobileInterface.Instance.Quit();
    }
    private void SendMsg2ClientWithCloseSeesion()
    {
        string mapId = GameManager.Inst.gameMapInfo.mapId;
        string appVersion = GameManager.Inst.unityConfigInfo.appVersion;
        string rCode = GameManager.Inst.onLineDataInfo.roomCode;
        int isPrivate = GameManager.Inst.onLineDataInfo.isPrivate;
        MAX_PLAYER = GameManager.Inst.gameMapInfo.maxPlayer;
        GlobalFieldController.pvpData = default;
        if (!string.IsNullOrEmpty(GameManager.Inst.gameMapInfo.pvpData))
        {
            GlobalFieldController.pvpData = JsonConvert.DeserializeObject<PVPData>(GameManager.Inst.gameMapInfo.pvpData);
        }
        string isPvp = (GlobalFieldController.pvpData.pvpMode == 1) ? "1" : "0";
        closeSessionParam param = new closeSessionParam()
        {
            roomType = mapId + "|" + appVersion,
            isPrivate = (int)rMode,
            roomCode = rCode,
            maxPlayerCount = MAX_PLAYER,
            isPvp = isPvp,
            sessionId = SessionId,
        };
        MobileInterface.Instance.CloseSession(JsonConvert.SerializeObject(param));
        OnLeaveRoomRedundancy();
    }
    private void OnLeaveRoomRedundancy()
    {
        LeaveRoomParam leaveRoomParam = new LeaveRoomParam()
        {
            roomCode = roomCode,
            timestamp = enterRoomTimstamp,
            seq = GameManager.Inst.unityConfigInfo.seq,
        };
        string jsonStr = JsonConvert.SerializeObject(leaveRoomParam);
        MobileInterface.Instance.LeaveRoomRedundancy(jsonStr);
    }
    #endregion

    #region BUD-Downtown
    public void SendPlayerLastPos()
    {
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.PlayerPos,
            data = GetPlayerLastPosData(),
        };
        LoggerUtils.Log("SendPlayerLastPos SendMsgToSever =>" + JsonConvert.SerializeObject(roomChatData));
        SendRequest(JsonConvert.SerializeObject(roomChatData));
    }

    private string GetPlayerLastPosData()
    {
        DowntownSpawnData data = new DowntownSpawnData();
        data.playerId = GameManager.Inst.ugcUserInfo.uid;
        data.mapId = GameManager.Inst.gameMapInfo.mapId;
        data.roomId = roomCode;
        Vec3 pos = PlayerBaseControl.Inst.transform.position;
        data.position = JsonConvert.SerializeObject(pos);
        data.area = 11;
        return JsonConvert.SerializeObject(data);
    }
    #endregion
}

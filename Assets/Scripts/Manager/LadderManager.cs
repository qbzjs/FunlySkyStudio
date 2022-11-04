/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/9/1 11:52:26
/// </summary>
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using BudEngine.NetEngine;
using BudEngine.NetEngine.src;

public class LadderManager : ManagerInstance<LadderManager>, IManager
{
    private const int MaxCount = 99;
    public const string MAX_COUNT_TIP = "Up to 99 Ladders can be set.";
    private string Tips = "Please quit ladder first.";
    private Dictionary<int, LadderBehaviour> laddersDict = new Dictionary<int, LadderBehaviour>();//当前地图全部梯子
    private Dictionary<LadderPlayerInfo, LadderInfo> curLadderDict = new Dictionary<LadderPlayerInfo, LadderInfo>();//当前与玩家绑定梯子
    private Dictionary<string, int> dataDict = new Dictionary<string, int>();//服务器下发与玩家绑定梯子
    PlayerBaseControl player;
    //动作名称
    private string Anim_Str_Down_In = "climbing_down_in";
    private string Anim_Str_Up_In = "climbing_up_in";
    private string Anim_Str_Down_Out = "climbing_down_out";
    private string Anim_Str_Up_Out = "climbing_up_out";
    private string Anim_Str_Up = "climbing_up";
    private string Anim_Str_Down = "climbing_down";
    public string Anim_Str_Idle = "climbing_idle";
    private string Anim_Str_None = "idle";
    public void Init(GameController controller)
    {
        player = controller.playerCom;
        laddersDict.Clear();
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
        MessageHelper.AddListener<bool>(MessageName.PosMove, PlayerForceDownLadder);
        MessageHelper.AddListener<string>(MessageName.PlayerLeave, PlayerLeaveRoom);
        MessageHelper.AddListener(MessageName.PlayerCreate, PlayerOnCreate);
    }
    public void OnChangeMode(GameMode mode)
    {
        if (laddersDict.Count > 0)
        {
            foreach (var ladderBehaviour in laddersDict.Values)
            {
                if (ladderBehaviour != null)
                {
                    ladderBehaviour.OnModeChange(mode);
                }
            }
            dataDict.Clear();
            curLadderDict.Clear();
        }
      
    }
   
    private void HandlePackPanelShow(bool isShow)
    {
        if (laddersDict!=null)
        {
            foreach (var item in laddersDict.Values)
            {
                if (item!=null&&item.model!=null&&item.boxCollider!=null)
                {
                    item.model.SetActive(!isShow);
                    item.boxCollider.enabled = !isShow;
                }
                
            }
        }
       
     
    }
    public void PlayerForceDownLadder(bool isTrap = false)
    {
        if (player == null)
        {
            return;
        }
        var info = GetLadderInfo(player.transform);
        if (info!=null)
        {
            LadderBehaviour bv = info.ladderBehaviour;
            int uid = bv.entity.Get<GameObjectComponent>().uid;
            PlayerDownLadder();
            if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
            {
                SendUnBindLadder(uid);
            }
        }

    }
    private void PlayerLeaveRoom(string id)
    {
        OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(id);
        if (otherCtr != null)
        {
            DownLadder(otherCtr.transform);
            if (dataDict.ContainsKey(id))
            {
                dataDict.Remove(id);
            }
        }


    }
    private void PlayerOnCreate()
    {
        if (Global.Room!=null&&Global.Room.RoomInfo.PlayerList != null)
        {
            for (int i = 0; i < Global.Room.RoomInfo.PlayerList.Count; i++)
            {
                string id = Global.Room.RoomInfo.PlayerList[i].Id;
                if (dataDict.ContainsKey(id))
                {
                    if (id != Player.Id)
                    {
                        OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(id);
                        if (otherCtr != null)
                        {
                            if (!ContainsPlayerTrans(otherCtr.transform))
                            {
                                otherCtr.avoidLerpCount = 1;
                                OnLadder(dataDict[id], otherCtr.transform);
                            }
                        }
                    }
                    else
                    {
                        if (!ContainsPlayerTrans(player.transform))
                        {
                            PlayerOnLadder(dataDict[id]);
                        }
                    }
                }
                else
                {
                    if (Player.Id == id)
                    {
                        if (ContainsPlayerTrans(player.transform))
                        {
                            PlayerDownLadder();
                        }
                    }
                    else
                    {
                        OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(id);
                        if (otherCtr != null)
                        {
                            if (ContainsPlayerTrans(otherCtr.transform))
                            {
                                otherCtr.avoidLerpCount = 1;
                                DownLadder(otherCtr.transform);
                            }
                        }
                    }
                }
            }

        }
    }
    public void Update()
    {
        if (curLadderDict.Count > 0)
        {
            foreach (var item in curLadderDict)
            {
                if (item.Key.isSelf && PlayerLadderControl.Inst != null && PlayerLadderControl.Inst.isOnLadder)
                {
                    if (item.Value.carryTrans != null)
                    {
                        item.Value.carryTrans.Rotate(Vector3.up, offset, Space.Self);
                        offset = 0;
                        item.Value.ladderBehaviour.SetPlayerCarryNodeMove(curStatus, item.Value);
                    }
                    item.Key.player.position = item.Value.carryTrans.position;
                    item.Key.player.rotation = item.Value.carryTrans.rotation;
                }
                else
                {
                    item.Value.carryTrans.localPosition = item.Key.otherPlayerCtr.m_PlayerPos;
                    if (item.Key.otherPlayerCtr.m_StateType >= (int)FrameStateType.LadderUpIn && item.Key.otherPlayerCtr.m_StateType <= (int)FrameStateType.LadderIdel)
                    {
                        item.Key.player.position = item.Value.carryTrans.position;
                        item.Key.player.rotation = item.Value.carryTrans.rotation;
                    }
                }


            }

        }

    }
    public void ForceSetRot()
    {
        var info = GetLadderInfo(player.transform);
        if (info!=null)
        {
            info.carryTrans.localRotation = new Quaternion(0, 0, 0, 0);
            if (PlayerLadderControl.Inst!=null)
            {
                PlayerLadderControl.Inst.SetRot();
            }
        }

    } 



    private float offset;
    public void SetOffset(float set)
    {
        offset = set;
    }
    public void AddLadder(LadderBehaviour behaviour)
    {
        int hashCode = behaviour.GetHashCode();
      
        if (!laddersDict.ContainsKey(hashCode))
        {
            laddersDict.Add(hashCode, behaviour);
        }
    }

    private void SendBindLadder(LadderBehaviour bv)
    {

        SendLadder(bv.entity.Get<GameObjectComponent>().uid, (int)LadderState.On);
    }
    private void SendUnBindLadder(LadderBehaviour bv,bool isDownLadderAbove)
    {
        SendLadder(bv.entity.Get<GameObjectComponent>().uid, (int)LadderState.Down, isDownLadderAbove);
    }
    private void SendUnBindLadder(int uid)
    {
        SendLadder(uid, (int)LadderState.Down);
    }
    private void SendLadder(int uid, int isOnLadder,bool isDownLadderAbove = false)
    {

        LadderSendData s = new LadderSendData
        {
            status = isOnLadder,
            playerId = Player.Id,
            opType = isDownLadderAbove ? 1 : 0,
        };
        Item itemData = new Item()
        {
            id = uid,
            type = (int)ItemType.Ladder,
            data = JsonConvert.SerializeObject(s),
        };
        Item[] itemsArray = { itemData };
        SyncItemsReq itemsReq = new SyncItemsReq()
        {
            mapId = GlobalFieldController.CurMapInfo.mapId,
            items = itemsArray,
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Items,
            data = JsonConvert.SerializeObject(itemsReq),
        };
        LoggerUtils.Log("Ladder SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), SendBoardCallBack);
    }
    private void SendBoardCallBack(int code, string data)
    {
        if (code != 0)//底层错误，业务层不处理
        {
            return;
        }
        SyncItemsReq ret = JsonConvert.DeserializeObject<SyncItemsReq>(data);
        int errorCode = 0;
        if (int.TryParse(ret.retcode, out errorCode))
        {
            if (errorCode == (int)PropOptErrorCode.Exception)
            {
                LoggerUtils.Log("SendBoardCallBack =>" + ret.retcode);
                TipPanel.ShowToast("The adhesive surface has been occupied by other player");
            }
        }
    }

    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("Ladder OnReceiveServer==>" + msg);
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                if (item.type == (int)ItemType.Ladder)
                {
                    LadderSendData ladder = JsonConvert.DeserializeObject<LadderSendData>(item.data);
                    SetLadder(item.id,ladder.playerId,ladder.status,ladder.opType);
                }
                
            }
        }

        return true;

    }
    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========LadderManager===>OnGetItems:" + dataJson);

        if (!string.IsNullOrEmpty(dataJson))
        {
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
            {
                LoggerUtils.Log("[LadderManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
                return;
            }

            PlayerCustomData[] playerCustomDatas = getItemsRsp.playerCustomDatas;
            if (playerCustomDatas!=null)
            {
                for (int i = 0; i < playerCustomDatas.Length; i++)
                {
                    PlayerCustomData playerData = playerCustomDatas[i];
                    
                    ActivityData[] activitiesData = playerData.activitiesData;
                    if (activitiesData == null)
                    {
                        continue;
                    }
                    for (int n = 0; n < activitiesData.Length; n++)
                    {
                        ActivityData activeData = activitiesData[n];
                        if (activeData != null&&activeData.activityId == ActivityID.LadderItem)
                        {
                            LadderGetItemInfo info = JsonConvert.DeserializeObject<LadderGetItemInfo>(activeData.data);
                            if (info != null&&info.mapId == GlobalFieldController.CurMapInfo.mapId)
                            {
                                SetLadder(info.id, playerData.playerId, (int)LadderState.On);
                            }
                        }
                    }
                }
            }
            
        }
    }
    public void OnLadderDisable(LadderBehaviour bv,int uid)
    {
        if (curLadderDict != null && player != null)
        {
            var info = GetLadderInfo(player.transform);
            if (info != null&&info.ladderBehaviour == bv)
            {
                PlayerDownLadder();
                if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
                {
                    SendUnBindLadder(uid);
                }
            }
        }

    }
    private void SetLadder(int id,string playerId,int status,int outPos = 0)
    {
        int hashCode = GetLadderHashCode(id);
        if (hashCode != 0)
        {
            //Debug.Log("ladder OnReceiveServer==>  id " + id+ " playerId " + playerId+ " status "+ status);
            bool isOnLadder = status == (int)LadderState.On;
            SetDataLadder(status, playerId, hashCode);
            if (isOnLadder)//上板子
            {
                if (Player.Id == playerId)
                {
                    if (ContainsPlayerTrans(player.transform))
                    {
                        PlayerDownLadder();
                    }
                    PlayerOnLadder(hashCode);
                }
                else
                {
                    OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(playerId);
                    if (otherCtr != null)
                    {
                        Transform otherplayertrans = otherCtr.transform;
                        if (ContainsOtherCtr(otherCtr))
                        {
                            DownLadder(otherplayertrans);
                        }
                        
                        otherplayertrans.GetComponent<OtherPlayerCtr>().avoidLerpCount = 1;
                        OnLadder(hashCode, otherplayertrans);

                    }
                }
            }
            else if (!isOnLadder)//下板子
            {
                if (Player.Id == playerId)
                {
                    PlayerDownLadder(outPos==1);
                }
                else
                {
                    OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(playerId);
                    if (otherCtr != null)
                    {
                        Transform otherplayertrans = otherCtr.transform;
                        otherCtr.animCon.PlayLadderAnim(outPos == 1);
                        otherplayertrans.GetComponent<OtherPlayerCtr>().avoidLerpCount = 1;
                        DownLadder(otherplayertrans);

                    }

                }
            }
        }
    }

    public int GetLadderHashCode(int uid)
    {
        foreach (var sid in laddersDict)
        {
            if (uid == sid.Value.entity.Get<GameObjectComponent>().uid)
            {
                return sid.Key;
            }
        }
        return 0;
    }

    public void PlayerSendOnLadder(int hashCode)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            if (laddersDict.ContainsKey(hashCode) && laddersDict[hashCode] != null)
            {
                SendBindLadder(laddersDict[hashCode]);
            }
        }
        else
        {
            PlayerOnLadder(hashCode);
        }
    }
    public void PlayerSendDownLadder()
    {
        PlayerSendDownLadder(false);
    }
    public void PlayerSendDownLadder(bool isDownLadderAbove = false)
    {
        //冻结状态不允许下梯子
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            var info = GetLadderInfo(player.transform);
            if (info!=null)
            {
                SendUnBindLadder(info.ladderBehaviour,isDownLadderAbove);
            }
        }
        else
        {
            PlayerDownLadder(isDownLadderAbove);
        }

    }
    public void PlayerOnLadder(int hashCode)
    {
        var playerOnLadderCtrl = PlayerControlManager.Inst.playerControlNode.GetComponent<PlayerLadderControl>();
        if (!playerOnLadderCtrl)
        {
            // 添加磁力板控制脚本
            playerOnLadderCtrl = PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerLadderControl>();
        }
        OnLadder(hashCode, player.transform);
        if (PlayerLadderControl.Inst)
        {
            PlayerLadderControl.Inst.OnLadder();
            var info = GetLadderInfo(player.transform);
            SetAnim(info.isAboveLadder?Anim_Str_Up_In: Anim_Str_Down_In);
            AKSoundManager.Inst.PlayLadderSound(info.isAboveLadder ? "Play_Ladder_HighPlace_Down" : "Play_Ladder_LowPlace_Up",player.gameObject);
            PlayerLadderControl.Inst.SetFrameState(info.isAboveLadder ? FrameStateType.LadderUpIn : FrameStateType.LadderDownIn);
        }
        PlayModePanel.Instance.SetOnLadderMode(true);

      
    }


    public void OnLadder(int hashCode, Transform transform)
    {
        if (transform != null)
        {
            LadderBehaviour bv = laddersDict[hashCode];
            if (bv != null)
            {

                OtherPlayerCtr otherplayerctr = transform.GetComponent<OtherPlayerCtr>();

                LadderPlayerInfo player = new LadderPlayerInfo()
                {
                    player = transform,
                    otherPlayerCtr = otherplayerctr,
                    isSelf = otherplayerctr == null,
                };
                LadderInfo ladder = bv.GetFreeCarryNodeInfo(transform);
                curLadderDict.Add(player, ladder);
                if (transform == this.player.transform)
                {
                    bv.OnLadderSuccess();
                }
               
            }
        }
    }
    public void PlayerDownLadder(bool isDownLadderAbove = false)
    {
       
        if (!ContainsPlayerTrans(player.transform))
        {
            return;
        }
        if (PlayModePanel.Instance!=null)
        {
            PlayModePanel.Instance.SetOnLadderMode(false);
        }
        if (PlayerLadderControl.Inst)
        {
            SetAnim(isDownLadderAbove ? Anim_Str_Up_Out : Anim_Str_Down_Out);
            AKSoundManager.Inst.PlayLadderSound(isDownLadderAbove ? "Play_Ladder_HighPlace_Up" : "Play_Ladder_LowPlace_Down", player.gameObject);
            PlayerLadderControl.Inst.SetFrameState(isDownLadderAbove ? FrameStateType.LadderUpOut : FrameStateType.LadderDownOut);
            PlayerLadderControl.Inst.UnBindLadder();
           
        }
        curStatus = OnLadderMoveStatus.Stay;
        DownLadder(player.transform);
    }
    public void DownLadder(Transform transform)
    {
        if (transform != null && ContainsPlayerTrans(transform))
        {
            var ladderplayer = GetLadderPlayerInfo(transform);
            LadderBehaviour ladderBehaviour = curLadderDict[ladderplayer].ladderBehaviour;
            if (ladderBehaviour != null)
            {
                ladderBehaviour.DownLadderSuccess(curLadderDict[ladderplayer].carryTrans,ladderplayer.player == player.transform);
            }
            curLadderDict.Remove(ladderplayer);
        }
    }

    #region 人物上下
    public enum OnLadderMoveStatus
    {
        Stay,
        Up,
        Down,
    }
    private OnLadderMoveStatus curStatus = OnLadderMoveStatus.Stay;
    public void SetPlayerOnLadderMove(OnLadderMoveStatus status)
    {
        if (ContainsPlayerTrans(player.transform))
        {
            switch (status)
            {
                case OnLadderMoveStatus.Up:
                    if (curStatus != status)
                    {
                        SetAnim(Anim_Str_Up);
                        PlayerLadderControl.Inst.SetFrameState(FrameStateType.LadderUp);

                    }
                    break;
                case OnLadderMoveStatus.Down:
                    if (curStatus != status)
                    {
                    
                        SetAnim(Anim_Str_Down);
                        PlayerLadderControl.Inst.SetFrameState(FrameStateType.LadderDown);
                    }
                    break;
                case OnLadderMoveStatus.Stay:
                    if (curStatus != status)
                    {
                        SetAnim(Anim_Str_Idle);
                        PlayerLadderControl.Inst.SetFrameState(FrameStateType.LadderIdel);
                    }
                    break;
            }
            curStatus = status;
            if (curStatus!= OnLadderMoveStatus.Stay)
            {
                if (PlayerLadderControl.Inst)
                {
                    PlayerLadderControl.Inst.PlayUpDownSound();
                }
            }
        }
        else
        {
            curStatus = OnLadderMoveStatus.Stay;
        }
        
    }

    public void PlayUpDownVoice()
    {
        switch (curStatus) {
            case OnLadderMoveStatus.Up:
               
                AKSoundManager.Inst.PlayLadderSound("Play_Ladder_Climb_Up_1P",player.gameObject);
                if (PlayerLadderControl.Inst) 
                {
                    PlayerLadderControl.Inst.PlayInvokeUpDownSound();
                }
                break;
            case OnLadderMoveStatus.Down:
                AKSoundManager.Inst.PlayLadderSound("Play_Ladder_Climb_Down_1P", player.gameObject);
                if (PlayerLadderControl.Inst)
                {
                    PlayerLadderControl.Inst.PlayInvokeUpDownSound();
                }
                break;
            case OnLadderMoveStatus.Stay:
                break;
        }
    }
    public void SetAnim(string newState)
    {
        PlayerLadderControl.Inst.SetAnim(newState);
    }
    public void PlayerMoveOn()
    {
        GetLadderInfo(player.transform);

    }



    #endregion


    public bool ContainsCarryTrans(Transform trans)
    {
        foreach (var item in curLadderDict)
        {
            if (item.Value.carryTrans == trans)
            {
                return true;
            }
        }
        return false;
    }
    public bool ContainsOtherCtr(OtherPlayerCtr ctr)
    {
        if (curLadderDict.Count==0)
        {
            return false;
        }
        foreach (var item in curLadderDict.Keys)
        {
            if (item.otherPlayerCtr == ctr)
            {
                return true;
            }
        }
        return false;
    }
    public bool ContainsPlayerTrans(Transform trans)
    {
        foreach (var item in curLadderDict)
        {
            if (item.Key.player == trans)
            {
                return true;
            }
        }
        return false;
    }
    public LadderInfo GetLadderInfo(Transform trans)
    {
        foreach (var item in curLadderDict)
        {
            if (item.Key.player == trans)
            {
                return item.Value;
            }
            
        }
        return null;
    }
    public LadderPlayerInfo GetLadderPlayerInfo(OtherPlayerCtr ctr)
    {
        foreach (var item in curLadderDict.Keys)
        {
            if (item.otherPlayerCtr == ctr)
            {
                return item;
            }

        }
        return null;
    }
    public LadderPlayerInfo GetLadderPlayerInfo(Transform trans)
    {
        foreach (var item in curLadderDict)
        {
            if (item.Key.player == trans)
            {
                return item.Key;
            }

        }
        return null;
    }
    public void SetLadderState(Animator playerAnim, string stateStr)
    {
        if (playerAnim == null)
        {
            return;
        }
        playerAnim.CrossFadeInFixedTime(stateStr, 0.2f);
    }

    #region 动作
    public void HandleFrameState(OtherPlayerCtr otherPlayerCtr, OtherPlayerAnimStateManager animStateManager, Animator playerAnim, FrameStateType stateType)
    {
       
        if (otherPlayerCtr == null || animStateManager == null || playerAnim == null)
        {
            return;
        }
        var info = GetLadderPlayerInfo(otherPlayerCtr);
    
        if (info!=null)
        {
            switch (stateType)
            {
                case FrameStateType.NoState:
                    SetLadderState(playerAnim, Anim_Str_None);
                    break;
                case FrameStateType.LadderIdel:
                    SetLadderState(playerAnim, Anim_Str_Idle);
                    break;
                case FrameStateType.LadderDown:
                    SetLadderState(playerAnim, Anim_Str_Down);

                    break;
                case FrameStateType.LadderDownIn:
                    SetLadderState(playerAnim, Anim_Str_Down_In);
                    AKSoundManager.Inst.PlayLadderSound("Play_Ladder_LowPlace_Up", playerAnim.gameObject);
                    break;
                case FrameStateType.LadderDownOut:
                    SetLadderState(playerAnim, Anim_Str_Down_Out);
                    AKSoundManager.Inst.PlayLadderSound("Play_Ladder_LowPlace_Down", playerAnim.gameObject);
                    break;
                case FrameStateType.LadderUp:
                    SetLadderState(playerAnim, Anim_Str_Up);

                    break;
                case FrameStateType.LadderUpIn:
                    SetLadderState(playerAnim, Anim_Str_Up_In);
                    AKSoundManager.Inst.PlayLadderSound("Play_Ladder_HighPlace_Down", playerAnim.gameObject);
                    break;
                case FrameStateType.LadderUpOut:
                    SetLadderState(playerAnim, Anim_Str_Up_Out);
                    AKSoundManager.Inst.PlayLadderSound("Play_Ladder_HighPlace_Up", playerAnim.gameObject);
                    break;
            }
        }
       
    }
    #endregion


    public void SetDataLadder(int state ,string playerId, int Code)
    {
        if (state == (int)LadderState.On && !dataDict.ContainsKey(playerId))//上板子
        {
            dataDict.Add(playerId, Code);
        }
        else if (state == (int)LadderState.Down && dataDict.ContainsKey(playerId))//下板子
        {
            dataDict.Remove(playerId);
        }
    }
    public Vector3 GetPlayerCarryNodePos()
    {
        var info = GetLadderInfo(player.transform);
        
        return info.ladderBehaviour.SetOffsetNodePos(info);
    }
    public void OnReset()
    {

        PlayModePanel.Instance.SetOnLadderMode(false);

        if (PlayerLadderControl.Inst)
        {
            PlayerLadderControl.Inst.UnBindLadder();
        }
        AllDownLadder();
        dataDict.Clear();
        curLadderDict.Clear();
    }
    private void AllDownLadder()
    {
        foreach (var pBoard in curLadderDict)
        {
            if (pBoard.Value.ladderBehaviour != null)
            {
                pBoard.Value.ladderBehaviour.DownLadderSuccess(pBoard.Key.player,true);
            }
        }
        curLadderDict.Clear();
        curStatus = OnLadderMoveStatus.Stay;
    }
    public override void Release()
    {
        LoggerUtils.Log($"LadderManager : Release");
        curLadderDict.Clear();
        laddersDict.Clear();
        dataDict.Clear();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
        MessageHelper.RemoveListener<bool>(MessageName.PosMove, PlayerForceDownLadder);
        MessageHelper.RemoveListener<string>(MessageName.PlayerLeave, PlayerLeaveRoom);
        MessageHelper.RemoveListener(MessageName.PlayerCreate, PlayerOnCreate);
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.Ladder)
        {
            laddersDict.Remove(behaviour.GetHashCode());
        }
    }
    public bool IsOverMaxCount()//最大开关数量
    {
        if (laddersDict.Count >= MaxCount)
        {
            return true;
        }
        return false;
    }
    public bool IsCanClone(GameObject curTarget)
    {
        if (curTarget.GetComponentInChildren<LadderBehaviour>() != null)
        {
            int CombineCount = curTarget.GetComponentsInChildren<LadderBehaviour>().Length;
            if (CombineCount > 1)
            {
                if (CombineCount + laddersDict.Count > MaxCount)
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
            else
            {
                if (IsOverMaxCount())
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
        }

        return true;
    }
    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.Ladder)
        {
            if (!laddersDict.ContainsValue((LadderBehaviour)behaviour))
            {

                laddersDict.Add(behaviour.GetHashCode(), behaviour as LadderBehaviour);
            }
        }
    }
    public void OnHandleClone(NodeBaseBehaviour sourceBev, NodeBaseBehaviour newBev)
    {
        var com = newBev.entity.Get<LadderComponent>();
        if (com != null)
        {
            var be = newBev as LadderBehaviour;
            be.SetMatetial(com.mat);
            AddLadder(be);
        }
    }
    public void Clear()
    {
        
    }
    public void ShowTips()
    {
        TipPanel.ShowToast(Tips);
    }
}
public class LadderPlayerInfo
{
    public Transform player;
    public bool isSelf;
    public OtherPlayerCtr otherPlayerCtr;
}
public class LadderInfo
{
    public Transform carryTrans;
    public LadderBehaviour ladderBehaviour;
    public bool isAboveLadder;//判断玩家在上梯子时的位置，用于上梯子动作的判断
    public bool isTurn;//判断玩家的方位是否相对梯子反转
}
public class LadderGetItemInfo
{
    public int id;
    public int type;
    public string mapId;
}
public enum LadderState
{
    Down = 0,
    On = 1
}
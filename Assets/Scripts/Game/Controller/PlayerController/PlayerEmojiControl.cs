
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;
using static EmoMenuPanel;

/// <summary>
/// Author:WenJia
/// Description:Player Emoji控制
/// 主要用于管理玩家的聊天和 Emoji 表情
/// Date: 2022/3/31 11:14:20
/// </summary>
public class PlayerEmojiControl : MonoBehaviour, IPlayerController, IPlayerCtrlMgr
{
    public PlayerBaseControl playerBase;
    [HideInInspector]
    public static PlayerEmojiControl Inst;
    public AnimationController animCon;
    public TextChatBehaviour textCharBev;
    public bool IsStateEmoTipShow; //是否正在显示加好友emo的tips，期间屏蔽所有好友相遇模块弹窗
    public Vector3 v3OutScreen = new Vector3(9999, 9999, 9999);

    private bool emoInterState;
    private EmoMsgHandlerBase emoMsgHandler;
    public EmoIconData mCurEmoData = null;//双人动作和StateEmo之间的互斥记录
    public EmoIconData mCurEmoDataNormal = null;//记录本地玩家当前进行中的Emo数据

    private void SetEmoMsgHandler(EmoMsgHandlerBase newMsgHandler)
    {
        this.emoMsgHandler = newMsgHandler;
    }

    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.Emoji, Inst);
        
    }

    private void OnDestroy()
    {
        Inst = null;
    }

    public void OnRoomChat(RoomChatResp resp)
    {
        var roomChatData = JsonConvert.DeserializeObject<RoomChatData>(resp.Msg);

        LoggerUtils.Log("OnRoomChat--self--->" + resp.Msg);

        switch ((RecChatType)roomChatData.msgType)
        {
            case RecChatType.TextChat:
                OnTextChatHandle(resp);
                break;
            case RecChatType.Emo:
                OnEmoHandle(resp, roomChatData);
                break;
            default:
                OnNormalHandle(resp, roomChatData);
                break;
        }
    }
    #region 根据聊天类型处理
    private void OnNormalHandle(RoomChatResp resp, RoomChatData roomChatData)
    {
        var playerDataCom = playerBase.transform.GetComponent<PlayerData>();
        var syncPlayerInfo = playerDataCom.syncPlayerInfo;
        RoomChatPanel.Instance.SetRecChat((RecChatType)roomChatData.msgType, syncPlayerInfo.userName, roomChatData.data);

        //场景内聊天气泡框
        if (textCharBev != null)
        {
            textCharBev.OnRecChat(resp);
        }
    }
    private void OnEmoHandle(RoomChatResp resp, RoomChatData roomChatData)
    {
        var itemData = JsonConvert.DeserializeObject<Item>(roomChatData.data);
        var playEmoData = JsonConvert.DeserializeObject<EmoItemData>(itemData.data);

        SetEmoMsgHandler(EmoMsgManager.Inst.GetEmoMsgHandler(true, this, playerBase, itemData, playEmoData, animCon, textCharBev));
        var emoResult = EmoMsgManager.Inst.CallEmoMsgHandler(emoMsgHandler, (OptType)playEmoData.opt);
        bool isNeedShowInChatWnd = emoMsgHandler.IsNeedShowInChatWnd();
        if (emoResult == false|| !isNeedShowInChatWnd)
        {
            return;
        }

        var playerDataCom = playerBase.transform.GetComponent<PlayerData>();
        var syncPlayerInfo = playerDataCom.syncPlayerInfo;
        RoomChatPanel.Instance.SetRecChat((RecChatType)roomChatData.msgType, syncPlayerInfo.userName, roomChatData.data);

        //场景内聊天气泡框
        if (textCharBev != null)
        {
            textCharBev.OnRecChat(resp);
        }
    }
    private void OnTextChatHandle(RoomChatResp resp)
    {
        //场景内聊天气泡框
        if (textCharBev != null)
        {
            textCharBev.OnRecChat(resp);
        }
    }
    #endregion

    public void OnRoomCustom(string playerId, RoomChatCustomData customData)
    {


    }

    public void OnGetPlayerCustomData(PlayerCustomData playerCustomData)
    {
        if (animCon)
        {
            EmoMsgManager.Inst.OnPlayerCustomData(playerCustomData, animCon,playerBase.IsTps);
        }
    }
    public void SetCurEmoData(int id)
    {
        EmoIconData data= MoveClipInfo.GetAnimName(id);
        if (data == null)
        {
            return;
        }
        mCurEmoDataNormal = data;

        if (data.emoType == (int)EmoTypeEnum.DOUBLE_EMO|| data.emoType == (int)EmoTypeEnum.STATE_EMO)
        {
            mCurEmoData = data; //过滤记录
        }
    }
    public void CancelInteractEmo()
    {
        if (animCon.isLooping && animCon.loopingInfo.emoType == (int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO)
        {
            animCon.StopLoop();
        }
    }

    public void SetEmoInteractState(bool state)
    {
        emoInterState = state;
    }

    public bool GetEmoInteractState()
    {
        return emoInterState;
    }

    //判断是否正处于动作播放中
    public bool GetIsInInteracting()
    {
        if (animCon != null)
        {
            return animCon.isInteracting;
        }
        return false;
    }

    //判断是否正处于状态中
    public bool IsInStateEmo()
    {
        if (animCon != null)
        {
            return animCon.curStateEmo != EmoName.None;
        }

        return false;
    }

    public EmoName GetCurStateEmoName()
    {
        if (animCon != null)
        {
            return animCon.curStateEmo;
        }
        return EmoName.None;
    }

    //public bool IsInStateEmoInteracting()
    //{
    //    if (animCon != null)
    //    {
    //        return animCon.curStateEmo;
    //    }
    //    return false;
    //}

    public void PlayMove(int i)
    {
        var emoData = MoveClipInfo.GetAnimName(i);

        int random = emoData.randomCount;
        int randomResult = random == 0 ? 0 : Random.Range(1, random + 1);
        LoggerUtils.Log("ActionName=>" + emoData.spriteName + " ,random=>" + randomResult);

        SendEmoChat(i, Player.Id, randomResult);

        EmoType type = emoData.GetEmoType();
        if ((type == EmoType.Mutual || type == EmoType.LoopMutual) && Global.Room != null && !Global.Room.GetNetworkState(ConnectionType.Common))
        {
            //网络未连接情况：不发起双人动作
            return;
        }
        animCon.PlayAnim(i, randomResult);
        if (PlayerMutualControl.Inst)
        {
            // 发起牵手时，玩家进入牵手待机状态
            PlayerMutualControl.Inst.isWaitingHands = (i == (int)EmoName.EMO_JOIN_HAND);
        }
        SetCurEmoData(i);
    }
    public void CallStateEmo(int i)
    {
        var emoData = MoveClipInfo.GetAnimName(i);
        if (emoData == null)
        {
            return;
        }
        int random = emoData.randomCount;
        int randomResult = random == 0 ? 0 : Random.Range(1, random + 1);
        LoggerUtils.Log("ActionName=>" + emoData.spriteName + " ,random=>" + randomResult);
        SendStateEmoChat(i, Player.Id, randomResult);
    }

    /**
    * 发起表情聊天
    */
    public void SendEmoChat(int emoId, string playerId, int random)
    {
        var emoData = MoveClipInfo.GetAnimName(emoId);
        if (emoData == null)
        {
            return;
        }
        EmoItemData emoItemData = new EmoItemData()
        {
            random = random,
            opt = (int)OptType.Release,
            startPlayerId = playerId,
           
        };
        Item item = new Item()
        {
            id = emoData.id,
            type = (int)emoData.GetEmoType(),
            data = JsonConvert.SerializeObject(emoItemData)
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Emo,
            data = JsonConvert.SerializeObject(item),
        };
        LoggerUtils.Log("SendEmoChat -----" + JsonConvert.SerializeObject(roomChatData));

        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }
    public void SendStateEmoChat(int emoId, string playerId, int random)
    {
        var emoData = MoveClipInfo.GetAnimName(emoId);
        if (emoData == null)
        {
            return;
        }

        EmoItemDataExtra extraData = new EmoItemDataExtra()
        { 
            actionProtect = emoData.actionProtect,
            doAction = 0,
            lastActionTime = 0,
            isFriend = 0,
            token = GetToken(),
        };

        EmoItemData emoItemData = new EmoItemData()
        {
            random = random,
            opt = (int)OptType.Release,
            startPlayerId = playerId,
            extraData = JsonConvert.SerializeObject(extraData),
        };
        Item item = new Item()
        {
            id = emoData.id,
            type = (int)emoData.GetEmoType(),
            data = JsonConvert.SerializeObject(emoItemData)
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Emo,
            data = JsonConvert.SerializeObject(item),
        };
        LoggerUtils.Log("SendStateEmoChat -----" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), SendStateEmoChatCallBack);
    }
    private string GetToken()
    {
#if !UNITY_EDITOR
            return HttpUtils.tokenInfo.token;
#else
        return TestNetParams.testHeader.token;
#endif
    }
    //发送请求加好友回包
    private void SendStateEmoChatCallBack(int code,string content)
    {
        if (EmoMenuPanel.Instance)
        {
            EmoMenuPanel.Instance.StopClickStateEmoCor();
        }
        if (code != 0)//底层错误，业务层不处理
        {
            return;
        }
        SyncItemsReq ret = JsonConvert.DeserializeObject<SyncItemsReq>(content);
        int errorCode = 0;
        if (int.TryParse(ret.retcode, out errorCode))
        {
           
        }
    }
    public void CancelStateEmo(int emoId)
    {
        EmoItemData emoItemData = new EmoItemData()
        {
            opt = (int)OptType.Cancel,
            startPlayerId = Player.Id,
        };
        Item item = new Item()
        {
            id = emoId,
            type = (int)EmoTypeEnum.STATE_EMO,
            data = JsonConvert.SerializeObject(emoItemData)
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Emo,
            data = JsonConvert.SerializeObject(item),
        };

        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }
    public void OnReset()
    {
        //pvp模式下结算时刻服务器把玩家状态清除了，需要直接关闭加好友发起界面、清除数据
        if (animCon.curStateEmo != EmoName.EMO_ADD_FRIEND)
        {
            return;
        }
        //清除自己
        if (StateEmoPanel.Instance != null)
        {
            StateEmoPanel.Hide();
        }
        //移除stateEmoHandle的事件
        if (emoMsgHandler != null && emoMsgHandler is EmoMsgStateEmoHandler)
        {
            EmoMsgStateEmoHandler handler = emoMsgHandler as EmoMsgStateEmoHandler;
            playerBase.RemovePlayerStateChangedObserver(handler.PlayerStateChanged);
        }
        //清理当前的Emo数据,如果已经被替换则不处理，
        CleanCurEmoData((int)EmoName.EMO_ADD_FRIEND);
        animCon.SetStateEmo(EmoName.None);
    }
    public void CleanCurEmoData(int id)
    {
        EmoIconData emoData = MoveClipInfo.GetAnimName(id);
        if (emoData == null)
        {
            return;
        }
        if (mCurEmoDataNormal != null
            && mCurEmoDataNormal.emoType == emoData.emoType)
        {
            mCurEmoDataNormal = null;
            mCurEmoData = null;
        }
    }
}
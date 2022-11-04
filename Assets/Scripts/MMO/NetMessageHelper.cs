using BudEngine.NetEngine;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description:联机业务层消息分发工具
/// Date: 2022-3-30 19:43:08
/// </summary>
public class NetMessageHelper : CInstance<NetMessageHelper>
{

    public enum RoomProcessMsgType
    {
        Reconnect = 9999,
    }

    public enum BattleGameBstType
    {
        PVP = 1,
        LeaderBoard = 3,
    }
    
    #region Delegates
    private delegate void CallbackHandler<T>(T arg); //RoomChat直接回调
    private delegate bool BroadcastHandler<T1, T2>(T1 arg1, T2 arg2); //RoomChat广播消息，return false:同时在聊天框显示消息
    private delegate void RoomProcessHandler(); //联机流程消息，例如断线重连等
    private delegate void BattleGameBstHandler<T>(T arg); //联机游戏对战广播

    private static Dictionary<RecChatType, Delegate> _broadcastHandlers = new Dictionary<RecChatType, Delegate>();
    private static Dictionary<RecChatType, Delegate> _callbackHandlers = new Dictionary<RecChatType, Delegate>();
    private static Dictionary<RecChatType, Delegate> _callbackSpecHandlers = new Dictionary<RecChatType, Delegate>();
    private static Dictionary<RoomProcessMsgType, Delegate> _roomProcessHandler = new Dictionary<RoomProcessMsgType, Delegate>();
    private static Dictionary<BattleGameBstType, Delegate> _battleGameBstHandlers = new Dictionary<BattleGameBstType, Delegate>();

    private static List<INetMessageHandler> _netMessageHandlers = new List<INetMessageHandler>();

    public override void Release()
    {
        base.Release();
        _broadcastHandlers.Clear();
        _callbackHandlers.Clear();
        _callbackSpecHandlers.Clear();
        _roomProcessHandler.Clear();
        _battleGameBstHandlers.Clear();
        _netMessageHandlers.Clear();
    }
    #endregion

    private static bool PreBroadcasting<T>(T type, Dictionary<T, Delegate> _messageTable)
    {
        bool flag = true;
        if (!_messageTable.ContainsKey(type))
        {
            flag = false;
        }
        return flag;
    }

    private static bool PreListenerAdding<T>(T type, Delegate listenerForAdding, Dictionary<T, Delegate> _messageTable)
    {
        if (null == listenerForAdding)
        {
            return false;
        }

        bool flag = true;
        if (!_messageTable.ContainsKey(type))
        {
            _messageTable.Add(type, null);
        }
        Delegate delegate2 = _messageTable[type];
        if ((delegate2 != null) && (delegate2.GetType() != listenerForAdding.GetType()))
        {
            flag = false;
        }

        if (null != delegate2)
        {
            foreach (Delegate delegateCur in delegate2.GetInvocationList())
            {
                if (listenerForAdding == delegateCur)
                {
                    //已添加过，无需重复添加
                    return false;
                }
            }
        }

        return flag;
    }

    public void OnPlayerCreated()
    {
        CoroutineManager.Inst.StartCoroutine(DoPlayerOnCreate());
    }

    //等一帧保证Animator初始化完
    private IEnumerator DoPlayerOnCreate()
    {
        yield return null;
        if (_netMessageHandlers != null && _netMessageHandlers.Count > 0)
        {
            foreach (var handler in _netMessageHandlers)
            {
                if (handler != null)
                {
                    handler.HandlePlayerCreated();
                }
            }
        }
    }
    
    private void AddCallbackListenerWithHandler<T>(RecChatType type, INetMessageHandler iNetMessageHandler, CallbackHandler<T> handler)
    {
        if (iNetMessageHandler == null)
        {
            return;
        }

        if (!_netMessageHandlers.Contains(iNetMessageHandler))
        {
            _netMessageHandlers.Add(iNetMessageHandler);
        }
        if (PreListenerAdding(type, handler, _callbackSpecHandlers))
        {
            _callbackSpecHandlers[type] = (CallbackHandler<T>)Delegate.Combine((CallbackHandler<T>)_callbackSpecHandlers[type], handler);
        }
    }

    private void SendCallbackRoomChatMsgWithHandler<T>(RecChatType type, T arg)
    {
        if (PreBroadcasting(type, _callbackSpecHandlers) && (_callbackSpecHandlers[type] != null))
        {
            CallbackHandler<T> source = _callbackSpecHandlers[type] as CallbackHandler<T>;
            if (source != null)
            {
                source(arg);
            }
        }
    }

    private void AddCallbackListener<T>(RecChatType type, CallbackHandler<T> handler)
    {
        if (PreListenerAdding(type, handler, _callbackHandlers))
        {
            _callbackHandlers[type] = (CallbackHandler<T>)Delegate.Combine((CallbackHandler<T>)_callbackHandlers[type], handler);
        }
    }

    private void SendCallbackRoomChatMsg<T>(RecChatType type, T arg)
    {
        if (PreBroadcasting(type, _callbackHandlers) && (_callbackHandlers[type] != null))
        {
            CallbackHandler<T> source = _callbackHandlers[type] as CallbackHandler<T>;
            if (source != null)
            {
                source(arg);
            }
        }
    }

    private void AddBroadcastListener<T1, T2>(RecChatType type, BroadcastHandler<T1, T2> handler)
    {
        if (PreListenerAdding(type, handler, _broadcastHandlers))
        {
            _broadcastHandlers[type] = (BroadcastHandler<T1, T2>)Delegate.Combine((BroadcastHandler<T1, T2>)_broadcastHandlers[type], handler);
        }
    }

    private bool BroadCastRoomChatMsg<T1, T2>(RecChatType type, T1 arg1, T2 arg2)
    {
        if (PreBroadcasting(type, _broadcastHandlers) && (_broadcastHandlers[type] != null))
        {
            BroadcastHandler<T1, T2> source = _broadcastHandlers[type] as BroadcastHandler<T1, T2>;
            if (source != null)
            {
                return source(arg1, arg2);
            }
        }

        return false;
    }

    #region Room Process
    private void AddRoomProcessListener(RoomProcessMsgType type, RoomProcessHandler handler)
    {
        if (PreListenerAdding(type, handler, _roomProcessHandler))
        {
            _roomProcessHandler[type] = (RoomProcessHandler)Delegate.Combine((RoomProcessHandler)_roomProcessHandler[type], handler);
        }
    }
    
    private void BroadCastRoomProcessMsg(RoomProcessMsgType type)
    {
        if (PreBroadcasting(type, _roomProcessHandler) && (_roomProcessHandler[type] != null))
        {
            RoomProcessHandler source = _roomProcessHandler[type] as RoomProcessHandler;
            if (source != null)
            {
                source();
            }
        }
    }
    #endregion

    #region BattleGame

    private void AddBattleGameBstListener<T>(BattleGameBstType bstType, BattleGameBstHandler<T> handler)
    {
        if (PreListenerAdding(bstType, handler, _battleGameBstHandlers))
        {
            _battleGameBstHandlers[bstType] = (BattleGameBstHandler<T>)Delegate.Combine((BattleGameBstHandler<T>)_battleGameBstHandlers[bstType], handler);
        }
    }

    private void BroadCastBattleGameBst<T>(BattleGameBstType type, T arg)
    {
        if (PreBroadcasting(type, _battleGameBstHandlers) && (_battleGameBstHandlers[type] != null))
        {
            BattleGameBstHandler<T> source = _battleGameBstHandlers[type] as BattleGameBstHandler<T>;
            if (source != null)
            {
                source(arg);
            }
        }
    }

    #endregion

    #region Broadcast and callbacks
    /// <summary>
    /// 广播消息
    /// 如果需要聊天面板显示消息，return false;
    /// </summary>
    public bool BroadCastRoomChatDataLocal(string playerId, RoomChatData roomChatData)
    {
        LoggerUtils.Log("[NetMessageHelper][BroadCastRoomChatDataLocal]=>" + roomChatData.data);
        return BroadCastRoomChatMsg<string, string>((RecChatType)roomChatData.msgType, playerId, roomChatData.data);
    }

    /// <summary>
    /// 非广播消息，直接在ClientManager.RoomChat里返回
    /// </summary>
    public void RoomChatCallBack(ResponseEvent eve)
    {
        if (eve.Data == null)
        {
            LoggerUtils.Log("[NetMessageHelper]RoomChatCallBack eve data is null=>" + eve.Code);
            return;
        }
        LoggerUtils.Log("[NetMessageHelper]RoomChatCallBack test=>" + eve.Data.GetType());
        SendToClientRsp rsp = (SendToClientRsp)eve.Data;
        // LoggerUtils.Log("[NetMessageHelper]RoomChatCallBack eve data=>" + eve.Data.ToString());
        // ServerPacket serverPack = (ServerPacket)eve.Data;

        try
        {
            if((RecChatType)rsp.MsgType == RecChatType.GetItems)
            {
                GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(rsp.Data);
            }
        }
        catch(Exception error)
        {
            LoggerUtils.LogError("Crash => |uid = " + GameManager.Inst.ugcUserInfo.uid + " |Time = " + GameUtils.GetTimeStamp() + " |rsp.Data = " + rsp.Data + " |error = " + error);
            return;
        }

        if (rsp != null)
        {

            LoggerUtils.Log("[NetMessageHelper][RoomChatCallBack] SendCallbackRoomChatMsg:" + rsp.Data);
            SendCallbackRoomChatMsg<string>((RecChatType)rsp.MsgType, rsp.Data);
            
            SendCallbackRoomChatMsgWithHandler<string>((RecChatType)rsp.MsgType, rsp.Data);
        }
        else
        {
            LoggerUtils.Log("[NetMessageHelper][RoomChatCallBack] serverpack is null");
        }
    }
        
    public void BroadCastRoomProcess(RoomProcessMsgType roomProcessType)
    {
        LoggerUtils.Log("[NetMessageHelper][BroadCastRoomProcess]=>" + roomProcessType);
        BroadCastRoomProcessMsg(roomProcessType);
    }
    
    public void BroadCastBattleGameBst(BattleGameBstType type, SendGameBst bst)
    {
        LoggerUtils.Log("[NetMessageHelper][BroadCastBattleGameBst]=>" + bst);
        BroadCastBattleGameBst<SendGameBst>(type, bst);
    }
    #endregion

    /// <summary>
    /// 网络监听初始化
    /// </summary>
    public void InitListener()
    {
        /////////// 各道具广播(return false:同时在聊天框显示消息) ///////////
        AddBroadcastListener<string, string>(RecChatType.Items, BloodPropManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Items, WeaponSystemController.Inst.HandleWeaponAttackBroadcast);
        AddBroadcastListener<string, string>(RecChatType.Items, ShootWeaponManager.Inst.OnRecvOperateMsgFromSever);
        AddBroadcastListener<string, string>(RecChatType.Items, SwitchManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Items, MagneticBoardManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Items, SensorBoxManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Items, PickabilityManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Items, SteeringWheelManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Items, FreezePropsManager.Inst.OnReceiveServerFreeze);
        AddBroadcastListener<string, string>(RecChatType.Items, FirePropManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Items, SlidePipeManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Firework, FireworkManager.Inst.OnReceiveServer);
        AddBroadcastListener<string,string>(RecChatType.PVPBoardCast,OnBroadCastChatMsg);
        AddBroadcastListener<string, string>(RecChatType.HitTrap, OnHitTrapChatMsg);
        AddBroadcastListener<string, string>(RecChatType.Custom, InitCustomListener);
        AddBroadcastListener<string, string>(RecChatType.ChangeCloth, OnChangeCloth);
        AddBroadcastListener<string, string>(RecChatType.PVPRoomState, SocialNotificationManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.SwitchHoldItem, BaggageManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Items, TrapSpawnManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Portal, PortalGateManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Items, EdibilitySystemController.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.ServerNotification, EdibilitySystemController.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Promote, PromoteManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Items, LadderManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Seesaw, SeesawManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Swing, SwingManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Fishing, FishingManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.VIPZone, VIPZoneManager.Inst.OnReceiveServer);
        AddBroadcastListener<string, string>(RecChatType.Items, SwordManager.Inst.OnReceiveServer);
        /////////// SEND_ROOM_ATTRS 广播消息
        AddBroadcastListener<string, string>(RecChatType.RoomAttrs, CollectControlManager.Inst.OnReceivedRoomAttr);
//        AddCallbackListener<string>(RecChatType.PVP, StartPVPGame);//开始对局模式

        /////////// GetItems回调(非广播消息) -- 消息直接分发，不依赖人物节点 ///////////

        AddCallbackListener<string>(RecChatType.GetItems, SwitchManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, CollectControlManager.Inst.OnGetItemsCallBack);
        AddCallbackListener<string>(RecChatType.GetItems, SensorBoxManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, MagneticBoardManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetMsgs, ChatPanelManager.Inst.OnGetMsgCallback);
        AddCallbackListener<string>(RecChatType.GetItems, PlayerLatestPosMgr.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, PickabilityManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, PVPManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, BloodPropManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, EdibilitySystemController.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, PromoteManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, FreezePropsManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, FishingManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.Emo, MutualManager.Inst.OnGetServerRspCallback);
        AddCallbackListener<string>(RecChatType.GetItems, LadderManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, SeesawManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, SwingManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, SlidePipeManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, VIPZoneManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.GetItems, SwordManager.Inst.OnGetItemsCallback);
        AddCallbackListener<string>(RecChatType.IceGem, CrystalStoneManager.Inst.OnReceiveServer);
        AddCallbackListener<string>(RecChatType.GetItems, CrystalStoneManager.Inst.OnGetItemsCallback);
        /////////// GetItems回调(非广播消息) -- 消息分发，依赖人物的道具 ///////////
        AddCallbackListenerWithHandler<string>(RecChatType.GetItems, SteeringWheelManager.Inst, SteeringWheelManager.Inst.OnGetItemsCallback);
        AddCallbackListenerWithHandler<string>(RecChatType.GetItems, BaggageManager.Inst, BaggageManager.Inst.OnGetItemsCallback);

        /////////// 房间内联机流程广播消息分发(例如断下重连等进房流程相关) ///////////
        // AddRoomProcessListener(RoomProcessMsgType.Reconnect, PVPManager.Inst.PVPGetGameInfoByReconnect);

        /////////// 游戏对战广播(PVP) ///////////
        AddBattleGameBstListener<SendGameBst>(BattleGameBstType.PVP, PVPManager.Inst.OnReceiveServer);
        AddBattleGameBstListener<SendGameBst>(BattleGameBstType.LeaderBoard, LeaderBoardManager.Inst.OnReceiveServer);
    }
    
    /// <summary>
    /// custom透传字段的事件注册
    /// </summary>
    public bool InitCustomListener(string senderPlayerId, string msg)
    {
        RoomChatCustomData data = JsonConvert.DeserializeObject<RoomChatCustomData>(msg);
        switch (data.type)
        {
            case (int)ChatCustomType.Keyboard:
                var iPlayerController = senderPlayerId == Player.Id ?
                    (IPlayerController)ClientManager.Inst.selfPlayerEmoji : ClientManager.Inst.GetOtherPlayerComById(senderPlayerId);
                if (iPlayerController!=null)
                {
                    iPlayerController.OnRoomCustom(senderPlayerId, data);
                }
                return false;

            case (int)ChatCustomType.JumpOnBoard:
                MagneticBoardManager.Inst.OnPlayerJumpOnBoard(senderPlayerId, msg);
                return false;

            // case (int)ChatCustomType.ChangeCloth:
            //     UgcClothItemManager.Inst.OnHandleChangeClothBst(senderPlayerId, msg);
            //     return false;

            case (int)ChatCustomType.Talk:
                if (RealTimeTalkManager.Inst != null)
                {
                    RealTimeTalkManager.Inst.OnPlayerTalking(senderPlayerId, msg);
                }
                return false;
            case (int)ChatCustomType.DCClothSoldOut:
                if (UgcClothItemManager.Inst != null)
                {
                    UgcClothItemManager.Inst.OnDcSoldOut(msg);
                }
                return false;
            case (int)ChatCustomType.DCResSoldOut:
                if (UGCBehaviorManager.Inst != null)
                {
                    UGCBehaviorManager.Inst.OnDcSoldOut(msg);
                }
                return false;
            case (int)ChatCustomType.PGCResSoldOut:
                if (PGCBehaviorManager.Inst != null)
                {
                    PGCBehaviorManager.Inst.OnDcSoldOut(msg);
                }
                return false;
            case (int)ChatCustomType.Bounceplank:
                if (BounceplankManager.Inst != null)
                {
                    BounceplankManager.Inst.OnBouncePlankJump(senderPlayerId, data.data);
                }
                return false;
            case (int)ChatCustomType.DowntownTransfer:
                if (DowntownTransferManager.Inst != null)
                {
                    DowntownTransferManager.Inst.OnReceiveServer(senderPlayerId, data.data);
                }
                break;
        }
        return false;
    }
    //PVP击败
    public bool  OnBroadCastChatMsg(string senderPlayerId, string msg)
    {
        CommonSetRecChat(RecChatType.PVPBoardCast, msg);
        return false;
    }
    //陷阱盒
    public bool OnHitTrapChatMsg(string senderPlayerId, string msg)
    {
        CommonSetRecChat(RecChatType.HitTrap, msg);
        return false;
    }
    //服务器主动推送的聊天消息展示在聊天窗口
    public void CommonSetRecChat(RecChatType msgtype, string msg)
    {
        DefeatMsg defeatMsg = JsonConvert.DeserializeObject<DefeatMsg>(msg);
        if (defeatMsg != null)
        {
            RoomChatPanel.Instance.SetRecChat(msgtype, "", defeatMsg.msg);
            ChatPanelManager.Inst.UpdataeLastChatId(defeatMsg.chatId);
        }
    }


    /// <summary>
    /// 场景内换装
    /// </summary>
    public bool OnChangeCloth(string senderPlayerId, string msg)
    {
        RoomChatCustomData data = JsonConvert.DeserializeObject<RoomChatCustomData>(msg);
        switch (data.type)
        {
            case (int)ChatCustomType.ChangeCloth:
                UgcClothItemManager.Inst.OnHandleChangeClothBst(senderPlayerId, msg);
                return false;
            case (int)ChatCustomType.ChangeImage:
                ClosetClientManager.Inst.OnHandleChangeImageBst(senderPlayerId, msg);
                return false;
            default:
                return false;
        }
    }
}

//依赖人物节点的道具，需实现此接口并重写
public interface INetMessageHandler
{
    public void HandlePlayerCreated();
}

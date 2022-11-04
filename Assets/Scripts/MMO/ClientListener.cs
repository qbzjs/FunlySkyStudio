using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using BudEngine.NetEngine.src;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public partial class ClientManager : MonoBehaviour
{
    public void InitBroadcast()
    {
        LoggerUtils.Log("########InitBroadcast Success");
        Global.Room.OnJoinRoom = OnBroadcastJoinRoom;
        Global.Room.OnLeaveRoom = OnBroadLeaveRoom;
        Global.Room.OnDismissRoom = OnDismis;
        Global.Room.OnRemovePlayer = OnRemovePlayer;
        Global.Room.OnUpdate = OnRoomUpdate;
        Global.Room.OnChangePlayerNetworkState = OnChangePlayerNetworkState;
        Global.Room.OnRecvFromClient = OnRecvFromClient;
        Global.Room.OnStartFrameSync = OnStartFrameSync;
        Global.Room.OnStopFrameSync = OnStopFrameSync;
        Global.Room.OnRecvFrame = OnRecvFrameStep;
        Global.Room.OnRecvFromGameSvr = OnRecvFromGameSvr;
        Global.Room.OnBattleGameBst = OnBattleGameBst;
        // match
        Room.OnMatch = OnMatch;
    }

    //新玩家加入房间广播
    public void OnBroadcastJoinRoom(BroadcastEvent eve)
    {
        var data = (JoinRoomBst)eve.Data;
        AddAction(() => {
            UpdatePlayers();
            var curJoinPlayerId = data.JoinPlayerId;
            if(data.JoinPlayerId != Player.Id)
            {
                //进房 更新lastChatId
                ChatPanelManager.Inst.UpdataeLastChatId(data.ChatId);
                if (data.PlayerName != null)
                {
                    RoomChatPanel.Instance.SetRecChat(RecChatType.JoinRoom, data.PlayerName);
                }
            }
        });

    }

    public void OnBroadcastJoinRoomOnMainThread(object obj)
    {
        var eve = (BroadcastEvent)obj;
        var data = (JoinRoomBst)eve.Data;
    }

    //Broadcast player leave the room
    public void OnBroadLeaveRoom(BroadcastEvent eve)
    {
        var data = (LeaveRoomBst)eve.Data;
        MainThreadDispatcher.Enqueue(new TaskRunner(eve, OnBroadLeaveRoomOnMainThread));
    }

    //Broadcast player leave the room OnMainThread
    public void OnBroadLeaveRoomOnMainThread(object obj)
    {
        var eve = (BroadcastEvent)obj;
        var data = (LeaveRoomBst)eve.Data;
        MessageHelper.Broadcast(MessageName.PlayerLeave, data.LeavePlayerId);
        LeaveRoomMsgOnClient(data.LeavePlayerId);
        DestroyLeftPlayer(data.LeavePlayerId);
        // 退房 更新lastChatId
        ChatPanelManager.Inst.UpdataeLastChatId(data.ChatId);
    }

    private void LeaveRoomMsgOnClient(string playId)
    {
        LoggerUtils.Log("############LeaveRoomMsgOnClient");
        if (otherPlayerDataDic.ContainsKey(playId))
        {
            var otherPlayerCom = otherPlayerDataDic[playId];
            var playerData = otherPlayerCom.GetComponent<PlayerData>();
            if (playerData.syncPlayerInfo != null && playId != Player.Id)
            {
                var userName = playerData.syncPlayerInfo.userName;
                RoomChatPanel.Instance.SetRecChat(RecChatType.LeaveRoom, userName);
                LoggerUtils.Log(userName + ":LeaveRoom");
            }
        }
    }

    public void OnDismis(BroadcastEvent eve)
    {
        var data = (JoinRoomBst)eve.Data;
        LoggerUtils.Log("ClientMgr Room Dismiss");
    }

    public void OnRemovePlayer(BroadcastEvent eve)
    {
        var data = (RemovePlayerBst)eve.Data;
        //LoggerUtils.Log("ClientMgr:The player is kicked out of the room" + data.RemovePlayerId);
        MainThreadDispatcher.Enqueue(new TaskRunner(eve, OnRemovePlayerOnMainThree));
    }

    public void OnRemovePlayerOnMainThree(object obj)
    {
        var eve = (BroadcastEvent)obj;
        var data = (RemovePlayerBst)eve.Data;
        if (data.RemovePlayerId == Player.Id)
        {
            //LoggerUtils.Log("ClientMgr 我自己被踢了!!");
            //ShowForceExitPopu("kick");
        }
        else
        {
            DestroyLeftPlayer(data.RemovePlayerId);
        }
    }

    private void ShowPingTime(ResponseEvent eve)
    {
        if (eve?.Msg == "pingSend")
        {
            //心跳发包
            CurTime = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        }
        else if (eve?.Msg == "pongResposne")
        {
            //心跳回包正常，用于统计网络延迟
            DeltaTime = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0) - CurTime;
            AddAction(() => {
                FPSPanel.Instance.SetPingText((float)DeltaTime.TotalMilliseconds);
                SendLatencyMsgToSever((float)DeltaTime.TotalMilliseconds);
            });
            //统计回包网络延迟
            DataLogUtils.ResponseCount((float)DeltaTime.TotalMilliseconds);
            LostTime = 0;
        }
        else if (eve?.Msg == "pongTimeout")
        {
            ////心跳超时，用于计数，超过阈值后做异常处理，例如退房
            LostTime++;
            if (LostTime > MaxLostTime)
            {
                LoggerUtils.Log("pongTimeout over MaxLostTime!");
            }
        }
    }



    private void OnRoomUpdate(Room room, ResponseEvent eve)
    {
        ShowPingTime(eve); //TODO:目前只统计了Socket1的ping时延，待统计socket2的
        //还没完走进房流程
        if (isEnterRoom == false)
        {
            return;
        }

        LoggerUtils.Log("ClientListener OnRoomUpdate Common:" + room.GetNetworkState(ConnectionType.Common) + " Relay:" + room.GetNetworkState(ConnectionType.Relay)+ "   isOnline:" + isOnline);//获取网络状态


        //断网/弱网监控
        PlayerNetworkManager.Inst.NetChangeDetect();
        PlayerNetworkManager.Inst.WeakNetDetect(room);
        
#if UNITY_EDITOR
        if (room.GetNetworkState(ConnectionType.Relay) == true && room.GetNetworkState(ConnectionType.Common) == false)
        {
            LoggerUtils.Log("[OnRoomUpdate] Socket2 is on ,Socket1 is off");
        }
        
        if (room.GetNetworkState(ConnectionType.Relay) == false && room.GetNetworkState(ConnectionType.Common) == true)
        {
            LoggerUtils.Log("[OnRoomUpdate] Socket1 is on ,Socket2 is off");
        }
#endif
        lock (roomUpdateLock)
        {
            //////////////////  帧同步链接恢复  ///////////////////
            if(room.GetNetworkState(ConnectionType.Relay) == false)
            {
                lastFrameOnline = false;
                StopClientSendFrame();
                LoggerUtils.Log("[OnRoomUpdate]*************  帧同步断开");
            }
            else if (room.GetNetworkState(ConnectionType.Relay) == true)
            {
                if (!isStartFrameSyncSuccess)
                {
                    StartFramStep(true);
                    LoggerUtils.Log("[OnRoomUpdate]*************  帧同步补发StartFrameSync");
                }
                
                if (!lastFrameOnline)
                {
                    StartClientSendFrame();
                    LoggerUtils.Log("[OnRoomUpdate]*************  帧同步恢复");
                }
                lastFrameOnline = true;
            }

            ///////////////////  房间链接恢复  ///////////////////
            if (room.GetNetworkState(ConnectionType.Common) == false)
            {
                isOnline = false;
                LoggerUtils.Log("[OnRoomUpdate]*************  房间链接断开");
            }
            else if (room.GetNetworkState(ConnectionType.Common) == true)
            {
                if (isOnline == false)
                {
                    LoggerUtils.Log("[OnRoomUpdate]*************  房间链接恢复");

                    var para = new GetRoomByRoomIdPara
                    {
                        RoomId = Global.Room.RoomInfo.Id,
                        SessionId = SessionInfo.sessionId,
                        PlayerId = Player.Id
                    };

                    Room.GetMyRoom(para, eve =>
                    {
                        if (eve.Code != 0)
                        {
                            AddAction(() => {
                                LoggerUtils.Log("重连 ForceExitPanel.Show();");
                                OnReConnectError();
                            });
                        }
                        else
                        {
                            //重连时重新设置下PlayerInfo
                            var data = (GetRoomByRoomIdRsp)eve.Data;
                            Global.Room.InitRoom(data.RoomInfo);
                            AddAction(() => {
                                OnReconnect();
                            });
                        }
                    });
                }
                isOnline = true;
            }
        }
    }
    
    private void OnReconnect()
    {
        LoggerUtils.Log("[OnRoomUpdate]*************  OnReconnect");
        // StartFramStep(true);
        UpdatePlayers();
        if (PVPWaitAreaManager.Inst.PVPBehaviour == null)
        {
            SendGetItems();
        }
        else
        {
            PVPManager.Inst.PVPGetGameInfo(SendGetItems);
        }
        ChatPanelManager.Inst.SendGetMsgs();//聊天窗口断线重连拉取聊天信息
    }

    private void OnReConnectError()
    {
        //长时间断线(超过后端断线阈值)，回来时提示并退回端上
        LoggerUtils.Log("*************  OnReConnectError And Quit");
        ForceExitPanel.Show();
        PlayerNetworkManager.Inst.isForceExitPanelShow = true;
    }

    private void OnDisconnect()
    {

    }

    //广播玩家在线状态
    public void OnChangePlayerNetworkState(BroadcastEvent eve)
    {
        MainThreadDispatcher.Enqueue(new TaskRunner(eve, OnChangePlayerNetworkStateOnMainThread));
    }

    public void OnChangePlayerNetworkStateOnMainThread(object obj)
    {
        var eve = (BroadcastEvent)obj;
        var data = (ChangePlayerNetworkStateBst)eve.Data;
        PlayerNetworkManager.Inst.HandleChangePlayerNetworkStateBst(data);
    }

    public void OnRecvFromClient(BroadcastEvent eve)
    {
        MainThreadDispatcher.Enqueue(new TaskRunner(eve, OnRecvFromGameSvrOnMainThread));
    }

    public void OnRecvFromGameSvrOnMainThread(object obj)
    {
        var eve = (BroadcastEvent)obj;

        RoomChatResp roomChatResp = JsonConvert.DeserializeObject<RoomChatResp>(eve.Data.ToString());
        RoomChatData roomChatData = JsonConvert.DeserializeObject<RoomChatData>(roomChatResp.Msg);

        LoggerUtils.Log("[OnRecvFromGameSvrOnMainThread]=>" + eve.Data.ToString());
        
        
        #region RoomChat埋点

        if (roomChatData != null && !string.IsNullOrEmpty(roomChatData.requestSeq)) 
        {
            if ((RecChatType)roomChatData.msgType == RecChatType.TextChat || (RecChatType)roomChatData.msgType == RecChatType.Emo)
            {
                MobileInterface.Instance.LogRoomChatEventByEventName(LogEventData.unity_roomchat_broadcast,roomChatData.requestSeq,roomChatData.msgType);
            }
        }

        #endregion
        
        if (NetMessageHelper.Inst.BroadCastRoomChatDataLocal(roomChatResp.SendPlayerId, roomChatData))
        {
            return;
        }


        var iPlayerController = roomChatResp.SendPlayerId == Player.Id ?
            (IPlayerController)PlayerEmojiControl.Inst : GetOtherPlayerComById(roomChatResp.SendPlayerId);

        if (iPlayerController != null)
        {
            iPlayerController.OnRoomChat(roomChatResp);
            //if (roomChatData.msgType == (int)RecChatType.Custom)
            //{
            //    iPlayerController.OnRoomCustom(roomChatResp.SendPlayerId, JsonConvert.DeserializeObject<RoomChatCustomData>(roomChatData.data));
            //}
        }

    }

    public void OnStartFrameSync(BroadcastEvent eve)
    {
        LoggerUtils.Log("OnStartFrameSync!!!!!");
        StartClientSendFrame();
    }

    public void OnStopFrameSync(BroadcastEvent eve)
    {
        LoggerUtils.Log("OnStopFrameSync!!!!");
        StopClientSendFrame();
    }

    private void OnRecvFromGameSvr(BroadcastEvent eve)
    {
        LoggerUtils.Log("#####OnRecvFromGameSvr=>"+ eve.Data.ToString());
        MainThreadDispatcher.Enqueue(new TaskRunner(eve, RecvFromGameSvrOnMainThread));
    }

    private void RecvFromGameSvrOnMainThread(object obj)
    {
        var eve = (BroadcastEvent)obj;
        var recData = (RecvFromGameSvrBst)eve.Data;
        serRep = JsonConvert.DeserializeObject<ServerPacket>(recData.Data);
        if (serRep != null)
        {
            var action = GameServer.Instance.GetGameResq(serRep.msgType);
            if (action != null)
            {
                action.Invoke(serRep);
            }
        }
    }

    //游戏对局广播
    private void OnBattleGameBst(BroadcastEvent eve)
    {
        LoggerUtils.Log("#####OnBattleGameBst=>"+ eve.Data.ToString());
        MainThreadDispatcher.Enqueue(new TaskRunner(eve, RecvBattleGameBstOnMainThread));
    }
    
    private void RecvBattleGameBstOnMainThread(object obj)
    {
        var eve = (BroadcastEvent)obj;
        var sendGameBst = (SendGameBst) eve.Data;
        NetMessageHelper.Inst.BroadCastBattleGameBst((NetMessageHelper.BattleGameBstType) sendGameBst.ReqType, sendGameBst);
    }

    private Dictionary<string, string> lastPlayerPortal = new Dictionary<string, string>();
    private Dictionary<string, string> curPlayerPortal = new Dictionary<string, string>();
    private float voiceLocateDeletTime = 0;
    public void OnRecvFrameStep(BroadcastEvent eve)
    {
        //LoggerUtils.Log("OnRecvFrameStep  收到帧数据包");
        RecvFrameBst bst = ((RecvFrameBst)eve.Data);
        var fr = bst.Frame;
        curPlayerPortal.Clear();
        var frameItems = fr.Items;
        if (frameItems == null)
        {
            return;
        }

        frameRecvCount++;
        for (int i = 0; i < frameItems.Count; i++)
        {
            var playerData = frameItems[i];
            UgcFrameData ugcFrameData = handleFrameData((string)playerData.Data);
            if(ugcFrameData == null)
            {
                continue;
            }
            string CurDiyMapId = ugcFrameData.mapId;
            if (Player.Id != playerData.PlayerId)
            {
                if (otherPlayerDataDic.ContainsKey(playerData.PlayerId))
                {
                    otherPlayerDataDic[playerData.PlayerId].OnFrame(ugcFrameData);

                    if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isFollowPlayer && PlayerMutualControl.Inst.startPlayerId == playerData.PlayerId)
                    {
                        PlayerMutualControl.Inst.followerFrameData = ugcFrameData;

                    }
                    //设置其他玩家的动画播放
                    MutualManager.Inst.PlayOtherPlayersAnim(playerData.PlayerId);
                }
            }

            if (curPlayerPortal.ContainsKey(playerData.PlayerId))
            {
                curPlayerPortal[playerData.PlayerId] = ugcFrameData.mapId;
            }
            else
            {
                curPlayerPortal.Add(playerData.PlayerId, ugcFrameData.mapId);
            }
        }

        if (!IsDictEquals(curPlayerPortal, lastPlayerPortal))
        {
            AddAction(() =>
            {
                DealProtal(bst);
            });

            lastPlayerPortal.Clear();
            foreach (var data in curPlayerPortal)
            {
                lastPlayerPortal.Add(data.Key, data.Value);
            }
        }
        AddAction(() =>
        {
            RealTimeTalkManager.Inst.SetRemoteVoicePosition();
        });
      
        // SendStep();
    }

    public void OnMatch(BroadcastEvent eve)
    {
        var data = (MatchBst)eve.Data;
        if (data.ErrCode == 0)
        {
            LoggerUtils.Log("ClientMgr onMatch匹配成功");
        }
        else
        {
            LoggerUtils.Log("ClientMgr onMatch匹配失败");
        }
    }

    public bool IsDictEquals(Dictionary<string, string> aDict, Dictionary<string, string> bDict)
    {
        if (aDict.Count != bDict.Count)
            return false;

        foreach (var data in aDict)
        {
            string bValue;
            if (!bDict.TryGetValue(data.Key, out bValue))
                return false;
            if (!Equals(data.Value, bValue))
                return false;
        }
        return true;
    }


    /// <summary>
    ///  延时 ping 值上报联机端
    /// </summary>
    /// <param name="pingValue">当前 ping 值</param>
    private void SendLatencyMsgToSever(float pingValue)
    {
        LatencyData pingData = new LatencyData()
        {
            latency = (int)pingValue
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.SendLatency,
            data = JsonConvert.SerializeObject(pingData),
        };
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }
}

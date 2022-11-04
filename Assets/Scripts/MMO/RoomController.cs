using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

public partial class ClientManager : MonoBehaviour
{
    private string enterRoomTimstamp;

    /// <summary>
    /// 简化后进房流程
    /// </summary>
    public void EnterRoom()
    {
        LoggerUtils.LogReport("Do EnterRoom","Unity_EnterRoom");
        LoggerUtils.Log("###### Do EnterRoom");
        EnterRoomPara para = GetEnterRoomPara();
        DataLogUtils.LogUnityEnterRoomReq(CurRetryTime);
        UnityLoadingstage unityLoadingstage = new UnityLoadingstage()
        {
            stage = (int)LoadingstageType.EnteringServer
        };
        mLoadingState = LoadingState.SverConnecting;
        MobileInterface.Instance.NotifyLoadingDialog(JsonConvert.SerializeObject(unityLoadingstage));
        Global.Room.EnterRoom(para, eve =>
        {
            LoggerUtils.LogReport("Do EnterRoom eve.Code="+eve.Code,"EnterRoom_CallBack");
            AddAction(() =>
            {
                DataLogUtils.LogUnityEnterRoomRsp(eve.Code.ToString(), CurRetryTime);
            });
            if (eve.Code != 0)
            {
                AddAction(() =>
                {
                    LoggerUtils.Log("ClientMgr EnterRoom Fail" + eve.Code + "eve.Msg"+eve.Msg);
                    RetryEnterRoom();
                });
            }
            else
            {
                mLoadingState = LoadingState.SverConnected;
                AddAction(() =>
                {
                    var rsp = (EnterRoomRsp)eve.Data;
                    enterRoomTimstamp = rsp.Timestamp;
                    LoggerUtils.Log(string.Format("ClientMgr EnterRoom Success RoomId:{0}, Timestamp:{1} ", Global.Room.RoomInfo.Id, rsp.Timestamp));

                    OnEnterRoom();
                });
            }
        });
    }

    private void OnEnterRoom()
    {
        LoggerUtils.LogReport("ClientMgr OnEnterRoom roomCode:" + Global.Room.RoomInfo.Id, "Unity_OnEnterRoom");
        //实时语音开启

        roomCode = Global.Room.RoomInfo.Id;
        isEnterRoom = true;
        UnityLoadingstage unityLoadingstage = new UnityLoadingstage() {
            stage = (int)LoadingstageType.EnteringExperience
        };
        MobileInterface.Instance.NotifyLoadingDialog(JsonConvert.SerializeObject(unityLoadingstage));
        StartFramStep();
        UpdatePlayers();
        BindSettingEvents();
        SendGetItems();
        RealTimeTalkManager.Inst.Init();
    }

    //开始帧同步
    public void StartFramStep(bool isReconnect = false)
    {
        AddAction(() =>
        {
            DataLogUtils.LogUnityStartFrameStepSend(CurRetryTime);
        });

        Global.Room.StartFrameSync(eve => {
            AddAction(() =>
            {
                DataLogUtils.LogUnityStartFrameStepRecv(eve.Code.ToString(), CurRetryTime);
            });
            if (eve.Code == 0)
            {
                LoggerUtils.Log("ClientManager StartFramStep Success");
                lastFrameOnline = true;
                StartFrameRetryTimes = 0;
                isStartFrameSyncSuccess = true;
                StartClientSendFrame();
            }
            else
            {
                AddAction(() =>
                {
                    LoggerUtils.Log("ClientManager StartFramStep fail code:" + eve.Code + "  Seq:" + eve.Seq);
                    isStartFrameSyncSuccess = false;
                    //断线重连时则不重新进房
                    if (!isReconnect)
                    {
                        RetryEnterRoom();
                    }
                });
            }
        });
    }

    //离开房间
    public void LeaveRoom()
    {
        UnBindSettingEvents();
        StopClientSendFrame();

        DataLogUtils.LogUnityPingTimeSend();
        DataLogUtils.LogTotalPlayTime(1);
        DataLogUtils.LogMapInfo();

        LeaveRoomPara para = new LeaveRoomPara()
        {
            PlayerId = Player.Id,
            RoomId = Global.Room.RoomInfo.Id
        };

        if (UgcClothItemManager.Inst != null)
        {
            UgcClothItemManager.Inst.NotifyRefreshPlayerData();
        }
        ClosetClientManager.Inst.NotifyRefreshPlayerData();

        MobileInterface.Instance.LogFrameEventByEventName(LogEventData.unity_frame_sended, frameSendedCount.ToString());
        MobileInterface.Instance.LogFrameEventByEventName(LogEventData.unity_frame_broadcast, frameRecvCount.ToString());
        
        Global.Room.LeaveRoom(para, eve =>
        {
        });
        
        //端上再调用一次LeaveRoom
        LeaveRoomParam leaveRoomParam = new LeaveRoomParam()
        {
            roomCode = roomCode,
            timestamp = enterRoomTimstamp,
            seq = GameManager.Inst.unityConfigInfo.seq,
                
        };
        var jsonStr = JsonConvert.SerializeObject(leaveRoomParam);
        MobileInterface.Instance.LeaveRoomRedundancy(jsonStr);
        
       // 退出语音房间
        RealTimeTalkManager.Inst.LeaveRoom();
        Release();
    }
}

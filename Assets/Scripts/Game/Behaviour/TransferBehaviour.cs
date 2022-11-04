/// <summary>
/// Author:Mingo-LiZongMing
/// Description:Downtown传送点Bev
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using HLODSystem;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

public class TransferBehaviour : BaseTransferBehaviour
{
    private MapInfo _lastMapInfo = null;
    private string _subMapJsonContent;

    public override void StartTransfer()
    {
        base.StartTransfer();

        switch (GetTransferType())
        {
            case TransferType.DowntownTransfer:
                DowntownLoadingPanel.Show();
                TransferToSubMap();
                break;
            case TransferType.UGCMapTransfer:
                if (GlobalFieldController.CurGameMode == GameMode.Guest)
                {
                    DowntownLoadingPanel.Show();
                    var curSubMapId = GlobalFieldController.CurMapInfo.mapId;
                    DowntownTransferManager.Inst.TransferToDowntown(curSubMapId);
                    DataLogUtils.subMapEndPlayTime = GameUtils.GetUtcTimeStampAsSpan();
                }
                else
                {
                    if (PlayModePanel.Instance)
                    {
                        PlayModePanel.Instance.OnEdit?.Invoke();
                    }
                }
                break;
        }
    }

    public TransferType GetTransferType()
    {
        var transComp = entity.Get<TransferComponent>();
        return (TransferType)transComp.transType;
    }

    //开始传送
    public void TransferToSubMap()
    {
        DataLogUtils.startRestoreTime = GameUtils.GetUtcTimeStampAsSpan();
        var subMapId = GetSubMapId();
        LoggerUtils.Log("StartTransfer subMapId = " + subMapId);
        //当前状态是否可以进行传送
        if (string.IsNullOrEmpty(subMapId))
        {
            DowntownLoadingPanel.Hide();
            TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
            return;
        }
        var mInfo = new UgcUntiyMapDataInfo
        {
            mapId = subMapId
        };
        MapLoadManager.Inst.GetMapInfo(mInfo, getMapInfo =>
        {
            float avgFps = FPSController.Inst.GetAverageFPS();
            MobileInterface.Instance.LogEvent(LogEventData.unity_avg_fps, new LogEventAvgFpsParam()
            {
                fps = Mathf.FloorToInt(avgFps),
            });
            QualityManager.Inst.SetAvgFps(GlobalFieldController.CurMapInfo?.mapId, avgFps);
            var mapInfo = getMapInfo.mapInfo;
            _lastMapInfo = GlobalFieldController.CurMapInfo;
            string curMapId = GlobalFieldController.CurMapInfo.mapId;
            GlobalFieldController.CurMapInfo = mapInfo.Clone();
            var nextMapId = GlobalFieldController.CurMapInfo.mapId;
            LoadMapPipeline(curMapId, nextMapId);
        }, error =>
        {
            DowntownLoadingPanel.Hide();
            HttpResponseRaw responseDataRaw = GameUtils.GetHttpResponseRaw(error);
            if (responseDataRaw.result >= 1 || responseDataRaw.result <= 10000)
            {
                TipPanel.ShowToast("Sorry, the experience has been deleted:(");
                return;
            }
            TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        });
    }

    private void LoadMapPipeline(string curMapId, string nextMapId)
    {
        var mapInfo = GlobalFieldController.CurMapInfo;
        MapLoadManager.Inst.LoadMapJson(mapInfo.mapJson, mapContent =>
        {
            _subMapJsonContent = mapContent;
            SendMsgToSever(curMapId, nextMapId, SendCallBack);
        }, GetMapJsonFail);
    }

    private void SendMsgToSever(string curMapId, string nextMapId, Action<int, string> callBack)
    {
        MapData mData = JsonConvert.DeserializeObject<MapData>(_subMapJsonContent);
        TargetMapData targetMapData = new TargetMapData()
        {
            mapId = nextMapId,
            isOpenBlood = mData.setHP == 0 ? false : true,
            hasLeaderBoard = mData.setLeaderBoard == 0 ? false : true,
            isOpenBaggage = mData.setBaggage == 0 ? false : true,
            initBlood = mData.customHP,
        };
        PortalDataReq portalData = new PortalDataReq()
        {
            curMapId = curMapId,
            targetMap = targetMapData,
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Portal,
            data = JsonConvert.SerializeObject(portalData),
        };
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), callBack);
    }

    private void SendCallBack(int code, string content)
    {
        SyncItemsReq mData = JsonConvert.DeserializeObject<SyncItemsReq>(content);
        if (string.IsNullOrEmpty(mData.retcode) || mData.retcode.Equals("0"))
        {
            LoggerUtils.Log("Successfully entered the portal " + code + " " + content);
            OnLoadMapSuccess(_subMapJsonContent);
        }
        else
        {
            GetMapJsonFail(content);
            LoggerUtils.Log("Failed to enter the portal " + code + " " + content);
        }
    }

    private void OnLoadMapSuccess(string mapContent)
    {
        var mapId = GameManager.Inst.gameMapInfo.mapId;
        HLOD.Inst.LoadMapOfflineData(mapId, () =>
        {
            var preloadUGCs = OfflineResManager.Inst.PreDealWithOfflineRes(mapContent);
            LoggerUtils.LogReport("OfflineResManager.PreDealWithOfflineRes", "PreloadAssetBundle_Start");
            OfflineResManager.Inst.PreloadAssetBundle(preloadUGCs, () =>
            {
                GetMapJsonSuccess(mapContent);
            });
        });
    }

    private void GetMapJsonSuccess(string content)
    {
        GlobalFieldController.curMapMode = MapMode.NormalMap;
        GameManager.Inst.curDiyMapId = GlobalFieldController.CurMapInfo.mapId;
        SceneBuilder.Inst.BgBehaviour.Stop();
        SceneBuilder.Inst.BgBehaviour.StopEnr();
        WeatherManager.Inst.PauseShowWeather();
        SceneSystem.Inst.StopSystem();
        SceneBuilder.Inst.DestroyScene();
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        PortalPlayPanel.Hide();
        SceneBuilder.Inst.ParseAndBuild(content);
        SceneBuilder.Inst.SpawnPoint.SetActive(false);
        SceneBuilder.Inst.BgBehaviour.Play(true);
        SceneBuilder.Inst.BgBehaviour.PlayEnr();
        WeatherManager.Inst.ShowCurrentWeather();
        SceneBuilder.Inst.SetEntityMeshsVisibleByMode(false);
        SceneSystem.Inst.StartSystem();
        MessageHelper.Broadcast(MessageName.ChangeMode, GlobalFieldController.CurGameMode);
        ClientManager.Inst.SendGetItems();
        PlayModePanel.Instance.EntryPortalMode(true);
        PlayModePanel.Instance.SetFlyButtonVisibleByPortal();
        SetPlayerPosAndRot();
        DataLogUtils.endRestoreTime = GameUtils.GetUtcTimeStampAsSpan();
        DataLogUtils.subMapStartPlayTime = GameUtils.GetUtcTimeStampAsSpan();
        DataLogUtils.LogRestoreSubMapTime();
        DowntownLoadingPanel.Hide();
        PlayTransferEffect(TransAnimType.End);
        TimerManager.Inst.RunOnce("TransferAnim", 1f, () => {
            PlayerBaseControl.Inst.PlayerResetIdle();
            StopTransferEffect();
        });
        AudioController.Inst.StopDowntownBGM();
        MessageHelper.Broadcast(MessageName.OnLeaveSnowfield);
    }

    private void GetMapJsonFail(string error)
    {
        // 获取地图Json失败，将CurMapInfo恢复到lastMapInfo
        GlobalFieldController.CurMapInfo = _lastMapInfo;
        DowntownLoadingPanel.Hide();
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }

    private string GetSubMapId()
    {
        var transComp = entity.Get<TransferComponent>();
        var subMapId = transComp.subMapId;
        return subMapId;
    }
}

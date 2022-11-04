using System;
using System.Collections;
using System.Collections.Generic;
using HLODSystem;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Networking;

public class PortalGateData
{
    public string mapId;
    public string mapName;
    public string pngUrl;
}

public class PortalGateBehaviour : NodeBaseBehaviour
{
    private Color[] colors;
    private string mapJson;
    private MapInfo lastMapInfo = null;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
    }

    public override void OnRayEnter()
    {
        base.OnRayEnter();
        PortalPlayPanel.Show();
        PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Portal);
        PortalPlayPanel.Instance.AddButtonClick(StartTransfer, true);
        PortalPlayPanel.Instance.SetTransform(transform);
    }

    public override void OnRayExit()
    {
        base.OnRayExit();
        PortalPlayPanel.Hide();
    }


    public void StartTransfer()
    {
        if (!StateManager.Inst.CheckCanTransfer())
        {
            return;
        }

        var portalGate = entity.Get<PortalGateComponent>();
        PortalGateAnimPanel.Show();
       // StartCoroutine(ResManager.Inst.GetContent("https://cdn.joinbudapp.com/UgcJson/1450306028838199296_1636648745.json", GetMapJsonSuccess, GetMapJsonFail));
        if (string.IsNullOrEmpty(portalGate.diyMapId))
        {
            PortalGateAnimPanel.Instance.StartBlackAnim(false, SetPlayerPosAndRot);
            return;
        }

        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.SetButtonVisible(false);
        }
        if (PlayerControlManager.Inst.isPickedProp)
        {
            PickabilityManager.Inst.OnPlayerLeaveCurMap();
        }
        if (PlayerSnowSkateControl.Inst)
        {
            PlayerSnowSkateControl.Inst.LeaveSnowCube();
        }
        PortalGateAnimPanel.Instance.StartShow(() => { PortalGateTransfer(portalGate.mapName, portalGate.diyMapId); });
    }

    private void SendMsgToSever(string curMapId, string nextMapId, Action<int, string> callBack)
    {
        MapData mData = JsonConvert.DeserializeObject<MapData>(mapJson);
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

    private void SendCallBack(int code , string content)
    {
        SyncItemsReq mData = JsonConvert.DeserializeObject<SyncItemsReq>(content);
        if (string.IsNullOrEmpty(mData.retcode) || mData.retcode.Equals("0"))
        {
            LoggerUtils.Log("Successfully entered the portal " + code + " " + content);
            OnLoadMapSuccess(mapJson);
        }
        else
        {
            GetMapJsonFail(content);
            LoggerUtils.Log("Failed to enter the portal " + code + " " + content);
        }
    }

    private void PortalGateTransfer(string mName, string mId)
    {
        
        LoggerUtils.Log("PortalGateTransfer:" + mName + mId);
        var mInfo = new UgcUntiyMapDataInfo
        {
            mapName = mName,
            mapId = mId
        };
        MapLoadManager.Inst.GetMapInfo(mId, mName, getMapInfo =>
        {
            float avgFps = FPSController.Inst.GetAverageFPS();
            MobileInterface.Instance.LogEvent(LogEventData.unity_avg_fps, new LogEventAvgFpsParam()
            {
                fps = Mathf.FloorToInt(avgFps),
            });
            if (GlobalFieldController.CurGameMode == GameMode.Guest)
            {
                QualityManager.Inst.SetAvgFps(GlobalFieldController.CurMapInfo?.mapId, avgFps);
            }
            var mapInfo = getMapInfo.mapInfo;
            lastMapInfo = GlobalFieldController.CurMapInfo;
            string curMapId = GlobalFieldController.CurMapInfo.mapId;
            GlobalFieldController.CurMapInfo = mapInfo.Clone();
            var nextMapId = GlobalFieldController.CurMapInfo.mapId;
            if (!CheckVersionCanTransfer(mapInfo.editorVersion))
            {
                if (CatchPanel.Instance)
                {
                    CatchPanel.Instance.SetButtonVisible(true);
                }
                TipPanel.ShowToast("You cannot teleport to higher version experience.");
                PortalGateAnimPanel.Hide();
                return;
            }
            if (!CheckHasGrantedCanTransfer(getMapInfo.perm))
            {
                if (getMapInfo.perm.groups != null)
                {
                    string toast = "Only owners of {0} can join";
                    string formatArgs = getMapInfo.perm.groups[0].name;//后续可能会有多个权限
                    TipPanel.ShowToast(toast, formatArgs);
                }
                PortalGateAnimPanel.Hide();
                return;
            }

            LoadMapPipeline(curMapId, nextMapId);
        }, error =>
        {
            if (CatchPanel.Instance)
            {
                CatchPanel.Instance.SetButtonVisible(true);
            }
            HttpResponseRaw responseDataRaw = GameUtils.GetHttpResponseRaw(error);
            if (responseDataRaw.result >= 1 || responseDataRaw.result <= 10000)
            {
                TipPanel.ShowToast("Sorry, the experience has been deleted:(");
                PortalGateAnimPanel.Hide();
                return;
            }
            TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
            PortalGateAnimPanel.Hide();
        });
    }

    /// <summary>
    ///  校验当前 app 版本是否能通过传送门进入地图
    ///  app 版本低于地图版本不能通过传送门
    /// </summary>
    /// <param name="targetVersion">目标地图版本值</param>
    /// <returns>校验结果</returns>
    private bool CheckVersionCanTransfer(int targetVersion)
    {
        string version = "";
#if UNITY_EDITOR
        version = TestNetParams.testHeader.version;
#else
        version = GameManager.Inst.unityConfigInfo.appVersion;
#endif
        int appVersion = GameUtils.GetVersionIntByStr(version);

        if (appVersion < targetVersion)
        {
            return false;
        }
        return true;
    }
    /// <summary>
    ///  校验当前用户是否有进入该地图的权限
    /// </summary>
    /// <param name="targetVersion">当前地图的权限信息</param>
    /// <returns>校验结果</returns>
    private bool CheckHasGrantedCanTransfer(Perm perm)
    {
        if (perm != null)
        {
            return perm.granted;
        }
        return true;
    }

    private void GetMapJsonSuccess(string content)
    {
        MapData mData = JsonConvert.DeserializeObject<MapData>(content);
        if (mData.pvpData != null)
        {
            PortalGateAnimPanel.Hide();
            TipPanel.ShowToast("You could not be teleported to an experience with game mode");
            return;
        }
        GameManager.Inst.curDiyMapId = GlobalFieldController.CurMapInfo.mapId;


        PlayerManager.Inst.ExitShowPlayerState();
        SceneBuilder.Inst.BgBehaviour.Stop();
        SceneBuilder.Inst.BgBehaviour.StopEnr();
        WeatherManager.Inst.PauseShowWeather();
        //背包模式过传送门时不执行拾取的逻辑
        if(SceneParser.Inst.GetBaggageSet() != 1)
        {
            PickabilityManager.Inst.OnDealPortal();
        }
        else
        {
            PickabilityManager.Inst.OnBaggageDealPortal();
        }

        SceneSystem.Inst.StopSystem();
        SceneBuilder.Inst.DestroyScene();
        BloodPropManager.Inst.ClearBloodPropDict();
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        PortalPlayPanel.Hide();
        SceneBuilder.Inst.ParseAndBuild(content);
        SceneBuilder.Inst.SpawnPoint.SetActive(false);
        SceneBuilder.Inst.BgBehaviour.Play(true);
        SceneBuilder.Inst.BgBehaviour.PlayEnr();
        WeatherManager.Inst.ShowCurrentWeather();
        FollowModeManager.Inst.OnChangeMode(GameMode.Guest);
        DisplayBoardManager.Inst.BatchRequestPlayerInfo();
        ShotPhotoManager.Inst.InitPhotosVisiable();
        BaggageManager.Inst.InitBaggageVisiable();
        SceneBuilder.Inst.SetEntityMeshsVisibleByMode(false);
        SceneSystem.Inst.StartSystem();
        PlayerManager.Inst.StartShowPlayerState();
        MessageHelper.Broadcast(MessageName.ChangeMode, GlobalFieldController.CurGameMode);
        ClientManager.Inst.SendGetItems();
        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            PlayModePanel.Instance.OnEdit = EnterEditMode;
            PlayModePanel.Instance.EntryPortalMode(false);
            PlayModePanel.Instance.SetFlyButtonVisibleByPortal();
            SetPlayerPosAndRot();
            PortalGateAnimPanel.Instance.StartBlackAnim(true, null);
        }
        else if(GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            PlayModePanel.Instance.EntryPortalMode(true);
            PlayModePanel.Instance.SetFlyButtonVisibleByPortal();
            SetPlayerPosAndRot();
            HLOD.Inst.ResetController();
            PortalGateAnimPanel.Instance.StartBlackAnim(true, null);
        }
    }

    private void LoadMapPipeline(string curMapId, string nextMapId)
    {
        var mapInfo = GlobalFieldController.CurMapInfo;
        MapLoadManager.Inst.LoadMapJson(mapInfo.mapJson, mapContent => 
        {
            mapJson = mapContent;
            MapData mData = JsonConvert.DeserializeObject<MapData>(mapJson);
            if (mData.pvpData != null)
            {
                GetMapJsonFail("The map that travels through the past is PVP map");
                return;
            }
            if (GlobalFieldController.CurGameMode == GameMode.Guest)
            {
                SendMsgToSever(curMapId, nextMapId, SendCallBack);
            }
            else
            {
                OnLoadMapSuccess(mapContent);
            }
        } , GetMapJsonFail);
    }
    
    private void OnLoadMapSuccess(string mapContent)
    {
        var mapId = GlobalFieldController.CurMapInfo.mapId;
        HLOD.Inst.LoadMapOfflineData(mapId, () =>
        {
            GetMapJsonSuccess(mapContent);
        });
    }

    private void SetPlayerPosAndRot()
    {
        var pos = SpawnPointManager.Inst.GetSpawnPoint().transform.localPosition;
        var rot = SpawnPointManager.Inst.GetSpawnPoint().transform.localRotation;

        PlayerBaseControl.Inst.SetPlayerPosAndRot(pos, rot);
    }

    private void EnterEditMode()
    {
        PickabilityManager.Inst.OnDealPortal();
        SeesawManager.Inst.PlayerLeaveSeesaw(true);
        SceneBuilder.Inst.BgBehaviour.Stop();
        SceneBuilder.Inst.BgBehaviour.StopEnr();
        WeatherManager.Inst.PauseShowWeather();
        SceneSystem.Inst.StopSystem();
        SceneBuilder.Inst.DestroyScene();

        GlobalFieldController.CurMapInfo = GlobalFieldController.OrgMapInfo.Clone();
        SceneBuilder.Inst.ParseAndBuild(GlobalFieldController.orgMapContent);
        DisplayBoardManager.Inst.BatchRequestPlayerInfo();
        CoroutineManager.Inst.StartCoroutine(VideoNodeManager.Inst.WaitStartLoadAllVideoUrl());
        SceneBuilder.Inst.PostProcessBehaviour.SetPostProcessActive(GlobalFieldController.isOpenPostProcess);
        MessageHelper.Broadcast(MessageName.EnterEdit);
    }

    private void GetMapJsonFail(string error)
    {
        // 获取地图Json失败，将CurMapInfo恢复到lastMapInfo
        GlobalFieldController.CurMapInfo = lastMapInfo;
        PortalGateAnimPanel.Hide();
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.SetButtonVisible(true);
        }
    }
    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh,gameObject,ref colors);
    }
}

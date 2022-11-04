using System.Collections.Generic;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using Amazon;
using Cinemachine;
using DG.Tweening;
using HLODSystem;
using Leopotam.Ecs;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

//client enter Unity editor mode
public enum EnterGameMode
{
    CreateEmptyScene = 1,
    GuestScene = 2,
    ContinueEditScene = 3,
    LocalScene = 4,
    Downtown = 5,
}

public enum GameMode
{
    Play,//试玩
    Guest,
    Edit
}

public enum PVPGameMode
{
    Normal,//正常游戏模式
    Race,//争夺游戏模式
}

public enum MapMode
{
    NormalMap = 1,
    Downtown = 2,
}

public class GameController:MonoBehaviour
{
    public GameObject UIMask;//在端上loading页关闭之前，放置一个Mask遮罩拦截点击事件
    public EnterGameMode curGameMode = EnterGameMode.CreateEmptyScene;

    public CinemachineVirtualCamera EditVirCamera;
    public CinemachineVirtualCamera PlayVirCamera;
    public PlayerBaseControl playerCom;
    public SceneEditModeController editController;
    public PlayModeController playController;
    private BaseModeController curModeController;
    private Camera mainCamera;
    private FPSController fpsController;

#if UNITY_EDITOR
    private void Awake()
    {
        TestEnterSceneManager.InitSceneData();
        UIMask.SetActive(false);
    }
#endif

    private void Start()
    {
        this.gameObject.DontDestroy();
        MobileInterface.Instance.LogCustomEventData(LogEventData.UNITY_MAINSCENE_START, (int)Log_Platform.Firebase, null);
        StartGame();
    }

    private void FixedUpdate()
    {
        DataLogUtils.UpdateTotalPlayTime();
        if (DataLogUtils.IsOpenTotalPlayTimeLog())
        {
            RealTimeTalkManager.Inst.FixedUpdate();
        }
    }

    private void AddFpsController()
    {
        if (!gameObject.TryGetComponent(out fpsController))
        {
            fpsController = gameObject.AddComponent<FPSController>();
        }
    }
    

    public void StartGame()
    {
        GlobalSettingManager.Inst.Init();
        LoggerUtils.Log("Unity-GameController-StartGame");
        AddFpsController();
        QualityManager.Inst.SetFps(GlobalSettingManager.Inst.GetFps());
        QualityManager.Inst.SetTargetQualityShadow(GlobalSettingManager.Inst.IsShadowOpen());
        DOTween.useSafeMode = false; //不使用安全模式，防止DOtween底层截获回调方法的报错
        UnityInitializer.AttachToGameObject(this.gameObject);
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        CameraUtils.Inst.SetMainCamera(mainCamera);
        GameManager.Inst.Init();
        UIControlManager.Inst.Init();
        UIConfigManager.Inst.Init();
        QualityManager.Inst.Init();
        MagneticBoardManager.Inst.Init(this);
        LadderManager.Inst.Init(this);
        BounceplankManager.Inst.Init(this);
        SteeringWheelManager.Inst.Init();
        SocialNotificationManager.Inst.Init();
        UIManager.Inst.Init();
        SceneBuilder.Inst.Init();
        SceneSystem.Inst.Init();
        MovePathManager.Inst.Init();
        CollectControlManager.Inst.Init();
        RoleConfigDataManager.Inst.LoadRoleConfig();
        //LoadUGCClothConfig
        UGCClothesDataManager.Inst.Init();
        NetMessageHelper.Inst.InitListener();
        UGCBehaviorManager.Inst.Init();
        OfflineResManager.Inst.Init();
        LRUManager<MeshLRUInfo>.Inst.Init(MeshLRUInfo.MaxSize);
        LRUManager<MapOfflineLRUInfo>.Inst.Init(MapOfflineLRUInfo.MaxSize);
        LRUManager<UGCTexLRUInfo>.Inst.Init(UGCTexLRUInfo.MaxSize);
        VideoNodeManager.Inst.Init();
        PVPWaitAreaManager.Inst.Init();
        HTTPAsyncHelper.Init();
        WeaponSystemController.Inst.Init();
        BloodPropManager.Inst.Init();
        FireworkManager.Inst.Init();
        FreezePropsManager.Inst.Init();
        ParachuteManager.Inst.Init();
        UgcClothItemManager.Inst.Init();
        CameraZoomManager.Inst.Init();
        StateManager.Inst.Init();
        CameraModeManager.Inst.Init();
        WeatherManager.Inst.Init();
        SnowCubeManager.Inst.Init();
        DataUtils.InitEditSaveInfo();
        ParticleObjPool.Inst.Init();
        ParticleManager.Inst.Init();
        SeesawManager.Inst.Init();
        VIPZoneManager.Inst.Init();
        SkyboxManager.Inst.Init();
        FishingEditManager.Inst.Init();
        SpawnPointManager.Inst.Init();
        FlashLightManager.Inst.Init();
        CrystalStoneManager.Inst.Init();
        //SwordManager.Inst.Init(playerCom);
        //SwingManager.Inst.Init();

        MessageHelper.AddListener(MessageName.EnterEdit, EnterEditMode);
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, HandleChangeMode);
#if !UNITY_EDITOR
        curGameMode = GetCurGameMode();
        LoggerUtils.Log("curGameMode" + (int)curGameMode);
#endif
        EnterGameByClient(curGameMode);
        SceneBuilder.Inst.PostProcessBehaviour.SetPostProcessActive(GlobalSettingManager.Inst.IsBloomOpen());
        //FBI WARNING !!! :所有Listener监听请勿放在EnterGameByClient之后！！！
        LoggerUtils.Log($"add closeLoadingListener");
        MobileInterface.Instance.AddClientRespose(MobileInterface.closeLoading, OnCloseLoading);
        //场景重建完成后再初始化音量
        GlobalSettingManager.Inst.InitSoundVolume();
    }

    private EnterGameMode GetCurGameMode()
    {
        if (GlobalFieldController.IsDowntownEnter)
        {
            return EnterGameMode.Downtown;
        }
        return (EnterGameMode)GameManager.Inst.engineEntry.subType;
    }

    private void EnterGameByLocal()
    {
        string jsonPath = Application.streamingAssetsPath + "/123.json";
        InitPlayMode();
        SceneBuilder.Inst.InitSceneParent();
        BasePrimitivePanel.SetResIDs(GameConsts.GameEditIds);
        SceneBuilder.Inst.ParseAndBuild(File.ReadAllText(jsonPath));
        DisplayBoardManager.Inst.BatchRequestPlayerInfo();
        InitEditMode();
        EnterEditMode(); 
    }

    private void EnterGameByClient(EnterGameMode curMode)
    {
        LoggerUtils.LogReport("EnterGameByClient curMode=" + curMode.ToString(), "EnterGameByClient");
        InitPlayMode();
        SceneBuilder.Inst.InitSceneParent();
        BasePrimitivePanel.SetResIDs(GameConsts.GameEditIds);
        
        switch (curMode)
        {
            case EnterGameMode.CreateEmptyScene:
#if !UNITY_EDITOR
                GameManager.Inst.gameMapInfo = new MapInfo();
                GameManager.Inst.gameMapInfo.mapName = GameManager.Inst.ugcUntiyMapDataInfo.mapName;
                GameManager.Inst.gameMapInfo.dataType = (int)MapSaveType.Map;
#else
                GameManager.Inst.gameMapInfo = new MapInfo();
                GameManager.Inst.gameMapInfo.mapName = "Ted";
                GameManager.Inst.gameMapInfo.dataType = (int)MapSaveType.Map;
#endif
                // Debug.Log("GameManager.Inst.unityConfigInfo.templateId:"+GameManager.Inst.unityConfigInfo.templateId);
                // Debug.Log("GameManager.Inst.unityConfigInfo.templateUrl:"+GameManager.Inst.unityConfigInfo.templateUrl);
                if(GameManager.Inst.unityConfigInfo != null && GameManager.Inst.unityConfigInfo.templateId != 0 
                    && !string.IsNullOrEmpty(GameManager.Inst.unityConfigInfo.templateUrl)){
                    InitTemplateScene();
                }else{
                    SceneBuilder.Inst.CreateEmptyScene();
                    SpawnPointManager.Inst.OnCreateEmptyScene();
                    SceneEditModeController.SaveMapByFirst();
                    DowntownTransferManager.Inst.CreateDefaultTransferPoint();
                    InitEditMode();
                    EnterEditMode();
                    StartCoroutine(WaitForFrameAndShow(true));
                }
                break;

            case EnterGameMode.ContinueEditScene:
                //GameManager.Inst.ugcUntiyMapDataInfo = new UgcUntiyMapDataInfo()
                //{
                //    mapId = "1460170748341456896_1638510428_0",
                //    mapName = "987654321"
                //};
                DataLogUtils.LogUnityGetMapInfoReq();
                MapLoadManager.Inst.GetMapInfo(GameManager.Inst.ugcUntiyMapDataInfo, (getInfo) =>
                {
                    if(!GlobalFieldController.isGameProcessing) return;
                    DataLogUtils.LogUnityGetMapInfoRsp("0");
                    HandleMapInfo(getInfo.mapInfo);
                }, (error) =>
                {
                    SavingData.HttpResponseRaw httpResponseRaw = GameUtils.GetHttpResponseRaw(error);
                    DataLogUtils.LogUnityGetMapInfoRsp(httpResponseRaw.result.ToString());
                    HandleFailure(error);
                });
                break;

            case EnterGameMode.GuestScene:
#if UNITY_EDITOR 
                GameManager.Inst.ugcUntiyMapDataInfo = new UgcUntiyMapDataInfo()
                {
                   mapId = "1490472853555482624_1664868873_1",
                   mapName = "111"
                };
                
                // GameManager.Inst.ugcUntiyMapDataInfo = new UgcUntiyMapDataInfo()
                // {
                //     mapId = "1524100248937689088_1652208832_7",
                //     mapName = "111"
                // };


                MapLoadManager.Inst.GetMapInfo(GameManager.Inst.ugcUntiyMapDataInfo, (getInfo) =>
                {
                    if(!GlobalFieldController.isGameProcessing) return;
                    GameManager.Inst.gameMapInfo = getInfo.mapInfo;
                    GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
                    InitGuestScene();
                }, (error) => {
                    SavingData.HttpResponseRaw httpResponseRaw = GameUtils.GetHttpResponseRaw(error);
                    LoggerUtils.LogError("GetMapInfo Fail" + error);
                    MobileInterface.Instance.Quit();
                });
#else
                LoggerUtils.Log("whiteListMask:" + GameManager.Inst.isInWhiteList);
                GlobalFieldController.whiteListMask = new WhiteListMask(GameManager.Inst.isInWhiteList);
                if (GlobalFieldController.whiteListMask.IsInWhiteList(WhiteListMask.WhiteListType.DevInfo))
                {
                    FPSPanel.Instance.gameObject.SetActive(true);
                }
                if(GameManager.Inst.gameMapInfo != null)
                {
                    GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
                    InitGuestScene();
                }
                else {
                    LoggerUtils.LogError("EnterGameByClient GetMapInfo Fail");
                    DataLogUtils.LogUnityGetMapInfoReq();
                    MapLoadManager.Inst.GetMapInfo(GameManager.Inst.ugcUntiyMapDataInfo, (getInfo) => {
                        DataLogUtils.LogUnityGetMapInfoRsp("0");
                        GameManager.Inst.gameMapInfo = getInfo.mapInfo;
                        GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
                        InitGuestScene();
                    }, (error) => {
                        SavingData.HttpResponseRaw httpResponseRaw = GameUtils.GetHttpResponseRaw(error);
                        DataLogUtils.LogUnityGetMapInfoRsp(httpResponseRaw.result.ToString());
                        LoggerUtils.LogError("GetMapInfo Fail" + error);
                        MobileInterface.Instance.Quit();
                    });
                }
#endif
                break;
            case EnterGameMode.LocalScene:
                
                EnterGameByLocal();
                break;
            case EnterGameMode.Downtown:
                LoggerUtils.Log("whiteListMask:" + GameManager.Inst.isInWhiteList);
                GlobalFieldController.whiteListMask = new WhiteListMask(GameManager.Inst.isInWhiteList);
                if (GlobalFieldController.whiteListMask.IsInWhiteList(WhiteListMask.WhiteListType.DevInfo))
                {
                    FPSPanel.Instance.gameObject.SetActive(true);
                }
                if (GameManager.Inst.gameMapInfo != null)
                {
                    GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
                    InitDowntownScene();
                }
                else
                {
                    DataLogUtils.LogUnityGetMapInfoReq();
                    var downtownId = GameManager.Inst.ugcUntiyMapDataInfo.mapId;
#if UNITY_EDITOR
                    downtownId = "teambud_downtown1_1";
#endif
                    MapLoadManager.Inst.GetDowntownInfo(downtownId, (downtownInfo) => {
                        GameManager.Inst.gameMapInfo = MapInfoConvertManager.Inst.ConvertDowntownInfoToMapInfo(downtownInfo);
                        GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
                        InitDowntownScene();
                    }, (error) => {
                        SavingData.HttpResponseRaw httpResponseRaw = GameUtils.GetHttpResponseRaw(error);
                        DataLogUtils.LogUnityGetMapInfoRsp(httpResponseRaw.result.ToString());
                        LoggerUtils.LogError("GetMapInfo Downtown Fail" + error);
                        MobileInterface.Instance.Quit();
                    });
                }
                break;
        }
    }

    private void InitContinueEditScene()
    {
        string mapUrl = GameManager.Inst.gameMapInfo.mapJson;
        if (mapUrl.Contains("ZipFile/") && mapUrl.Contains(".zip"))
        {
            MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_req);
            StartCoroutine(GetByte(mapUrl, (content) =>
            {
                if(!GlobalFieldController.isGameProcessing) return;
                MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_rsp, "0");
                string jsonStr = ZipUtils.SaveZipFromByte(content);
                if (string.IsNullOrEmpty(jsonStr))
                {
                    LoggerUtils.LogError("UnZip Failed");
                    OnGetFail();
                    return;
                }
                OfflineResManager.Inst.PreloadAssetBundle(() =>
                {
                    OnGetSuccessEd(jsonStr);
                });
            }, (error) =>
            {
                MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_rsp, "-1");
                OnGetFail();
            }));
        }
        else
        {
            MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_req);
            StartCoroutine(GetText(mapUrl, (content) =>
            {
                if(!GlobalFieldController.isGameProcessing) return;
                MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_rsp, "0");
                OfflineResManager.Inst.PreloadAssetBundle(() =>
                {
                    OnGetSuccessEd(content);
                });
            }, (error) =>
            {
                MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_rsp, "-1");
                OnGetFail();
            }));
        }
    }

    private void InitContinueEditByLocal()
    {
        string filePath = DataUtils.DraftPath + Data_Type.Map.ToString().ToLower() + ".zip";
        if (!File.Exists(filePath))
        {
            LoggerUtils.LogError("local map json not exist! => " + filePath);
            OnGetFail();
            return;
        }
        byte[] content = File.ReadAllBytes(filePath);
        string jsonStr = ZipUtils.SaveZipFromByte(content);
        if (string.IsNullOrEmpty(jsonStr))
        {
            LoggerUtils.LogError("local map json unzip failed! => " + filePath);
            OnGetFail();
            return;
        }
        LoggerUtils.Log("LocalRead -- mapJsonStr = " + jsonStr);
        OfflineResManager.Inst.PreloadAssetBundle(() =>
        {
            OnGetSuccessEd(jsonStr);
        });
    }

    private PreloadGameInfo preloadInfo;
    private void InitGuestScene()
    {
        string mapUrl = GameManager.Inst.gameMapInfo.mapJson;
        var mapId = GameManager.Inst.gameMapInfo.mapId;
        LoggerUtils.LogReport("InitGuestScene mapId ="+mapId,"InitGuestScene");
        string mapName = Path.GetFileNameWithoutExtension(mapUrl).Replace(".zip", "");
        GameManager.Inst.curDiyMapId = GameManager.Inst.gameMapInfo.mapId;
        GameManager.Inst.LoadMapAsyncCount = 0;
        preloadInfo = new PreloadGameInfo(3, () =>
        {
            if(!GlobalFieldController.isGameProcessing) return;
            var preloadUGCs = OfflineResManager.Inst.PreDealWithOfflineRes(preloadInfo.mapContent);
            LoggerUtils.LogReport("OfflineResManager.PreDealWithOfflineRes", "PreloadAssetBundle_Start");
            OfflineResManager.Inst.PreloadAssetBundle(preloadUGCs, () =>
            {
                if(!GlobalFieldController.isGameProcessing) return;
                LoggerUtils.LogReport(
                    "InitSDK preloadInfo.isSessionSuccess =" + preloadInfo.isSessionSuccess.ToString(),
                    "PreloadAssetBundle_callback");
                // Init SDK 前置
                if (preloadInfo.isSessionSuccess)
                {
                    ClientManager.Inst.InitSDK();
                }

                LoggerUtils.LogReport("InitSDK Success", "InitSDK_Success");
                OnGetSuccessGu(preloadInfo.mapContent);
                // 场景重建之后， 判断若获取Session失败，则进入单机模式
                if (!preloadInfo.isSessionSuccess)
                {
                    LoggerUtils.LogReport("preloadInfo fail", "PreloadInfo_GetSession_Fail");
                    ClientManager.Inst.EnterOfflineMode();
                }
            });
        });
        LoggerUtils.LogReport("Start GetSessionInfo","GetSessionInfo_Start");
        ClientManager.Inst.GetSessionInfo((isSuccess) =>
        {
            if(!GlobalFieldController.isGameProcessing) return;
            LoggerUtils.LogReport(string.Format("GetSessionInfo isSuc={0} count={1}", isSuccess.ToString(),
                preloadInfo.PreloadCount), "GetSessionInfo_PreEnd");
            preloadInfo.isSessionSuccess = isSuccess;
            preloadInfo.PreloadCount++;
            LoggerUtils.LogReport("GetSessionInfo PreloadCount", "GetSessionInfo_End");
        });
        GlobalFieldController.CurGameMode = GameMode.Guest;
        MapLoadManager.Inst.LoadMapJson(mapUrl, (mapContent) =>
        {
            if(!GlobalFieldController.isGameProcessing) return;
            preloadInfo.mapContent = mapContent;
            LoggerUtils.LogReport(string.Format("LoadMapJson PreloadCount={0}",  (preloadInfo.PreloadCount + 1).ToString(),
                preloadInfo.PreloadCount), "LoadMapJson_End");
            preloadInfo.PreloadCount++;
           
        }, OnLoadMapFailure);
        HLOD.Inst.LoadMapOfflineData(mapId, () =>
        {
            if(!GlobalFieldController.isGameProcessing) return;
            LoggerUtils.LogReport(string.Format("LoadMapOfflineData PreloadCount={0}",  preloadInfo.PreloadCount.ToString(),
                preloadInfo.PreloadCount), "LoadMapOfflineData_End");
            preloadInfo.PreloadCount++;
        });
    }

    private void OnLoadMapFailure(string error)
    {
        LoggerUtils.LogError("LoadMapJson Failed:" + error);
        OnGetFail();
    }
 

    private void InitTemplateScene()
    {
        string templateUrl = GameManager.Inst.unityConfigInfo.templateUrl;
        LoggerUtils.LogReport("InitTemplateScene templateUrl="+templateUrl.ToString(), "unity_downTempJson_req");
        MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downTempJson_req);
        StartCoroutine(GetText(templateUrl, (content) =>
        {
            MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downTempJson_rsp);
            LoggerUtils.LogReport("InitTemplateScene Success", "unity_downTempJson_rsp");
            OnGetSuccessTemp(content);
        }, (error) =>
        {
            OnGetFail();
        }));
    }

    private void OnGetSuccessEd(string content)
    {
        DataLogUtils.LogUnityRestoreJsonStart();
        SceneBuilder.Inst.ParseAndBuild(content);
        DataLogUtils.LogUnityRestoreJsonEnd();
        LoggerUtils.Log("#######进房流程 解析完json");
        LoggerUtils.LogReport("OnGetSuccessEd Parsing JSON", "OnGetSuccessEd");
        //初始化时，写入本地配置文件
        DataUtils.SetConfigLocal(CoverType.JPG);
        DisplayBoardManager.Inst.BatchRequestPlayerInfo();
        InitEditMode();
        EnterEditMode();
        StartCoroutine(WaitForFrameAndShow(true));
    }


    private void OnGetSuccessGu(string content)
    {
        LoggerUtils.LogReport("OnGetSuccessGu Start Parse Json","OnGetSuccessGu_Start");
        DataLogUtils.LogUnityRestoreJsonStart();
        SceneBuilder.Inst.ParseAndBuild(content);
        DataLogUtils.LogUnityRestoreJsonEnd();
        DataLogUtils.LogUnityMapLight();
        DisplayBoardManager.Inst.BatchRequestPlayerInfo();
        LoggerUtils.Log("#######进房流程 解析完json");
        LoggerUtils.LogReport("OnGetSuccessGu Parse JSON Success", "OnGetSuccessGu_Success");
        EnterPlayModel(GameMode.Guest);
        LoggerUtils.LogReport("OnGetSuccessGu EnterPlayModel", "OnGetSuccessGu_EnterPlayModel");
    }

    private void OnGetSuccessTemp(string content)
    {
        LoggerUtils.LogReport("OnGetSuccessTemp start Parse JSON", "OnGetSuccessTemp_Success_Start");
        MobileInterface.Instance.LogEventByEventName(LogEventData.unity_restoreTemp_start);
        SceneBuilder.Inst.ParseAndBuild(content);
        MobileInterface.Instance.LogEventByEventName(LogEventData.unity_restoreTemp_end);
        DisplayBoardManager.Inst.BatchRequestPlayerInfo();
        SceneEditModeController.SaveMapByFirst();
        InitEditMode();
        EnterEditMode();
        LoggerUtils.LogReport("OnGetSuccessTemp Parse JSON Success", "OnGetSuccessTemp_Success");
        StartCoroutine(WaitForFrameAndShow(true));
    }

    private void OnGetFail()
    {
        LoggerUtils.LogError("Get MapJson Fail");
        StartCoroutine(WaitForFrameAndShow(false));
    }

    private void InitPlayMode()
    {
        playController = new PlayModeController();
    }

    private void InitEditMode()
    {
        editController = new SceneEditModeController();
        editController.Init();
        editController.SetCamera(mainCamera, EditVirCamera);
        InvokeRepeating("AutoSaveMapOnEditMode", 300, 300);
        InvokeRepeating("EditModeTimer", 1, 1);
        GameManager.Inst.gameMapInfo.editTime = Mathf.Max(GameManager.Inst.gameMapInfo.editTime, 1);
    }

    private void AutoSaveMapOnEditMode()
    {
#if !UNITY_EDITOR
        if (GlobalFieldController.CurGameMode == GameMode.Edit)
        {
            SceneEditModeController.SaveMapJsonByAuto();
        }
#endif
    }

    private void EditModeTimer()
    {
#if !UNITY_EDITOR
        if (GlobalFieldController.CurGameMode == GameMode.Edit)
        {
            GameManager.Inst.gameMapInfo.editTime++;
        }
#endif
    }

    private void EnterEditMode()
    {
        GlobalFieldController.CurGameMode = GameMode.Edit;
        EditVirCamera.enabled = true;
        PlayVirCamera.enabled = false;
        PlayModePanel.Hide();
        RoomChatPanel.Hide();
        PVPSurvivalGamePlayPanel.Hide();
        PVPWinConditionGamePlayPanel.Hide();
        BaggagePanel.Hide();
        FPSPlayerHpPanel.Hide();
        InputReceiver.locked = false;
        if (StorePanel.Instance) 
        {
            StorePanel.Instance.RePlayerCam(); 
        }
        if (VideoFullPanel.Instance)
        {
            VideoFullPanel.Instance.RePanelState();
        }
        StorePanel.Hide();
        VideoFullPanel.Hide();
        SceneGizmoPanel.Show();
        BasePrimitivePanel.Show();
        editController.SetEditHandler();
        editController.SetPlayerState(playerCom);
        BasePrimitivePanel.Instance.OnSelect = editController.CreatePritiveModel;
        ReferPanel.Show();
        GameEditModePanel.Show();
        GameEditModePanel.Instance.SetGizmoController(editController.gController);
        GameEditModePanel.Instance.OnPlay = OnPlayClick;
        SceneBuilder.Inst.BgBehaviour.Stop();
        SceneBuilder.Inst.BgBehaviour.StopEnr();
        WeatherManager.Inst.PauseShowWeather();
        SceneBuilder.Inst.SetEntityMeshsVisibleByMode(true);
        DisplayBoardManager.Inst.BatchRequestPlayerInfo();
        SoundManager.Inst.AudioStop();
        ShotPhotoManager.Inst.InitPhotosVisiable();
        MagneticBoardManager.Inst.OnChangeMode(GameMode.Edit);
        LeaderBoardManager.Inst.OnChangeMode(GameMode.Edit);
        TrapSpawnManager.Inst.OnChangeMode(GameMode.Edit);
        SceneSystem.Inst.StopSystem();
        EdibilityManager.Inst.OnChangeMode(GameMode.Edit);
        
    }

    

    private void HandleChangeMode(GameMode gameMode)
    {
        switch (gameMode)
        {
            case GameMode.Play:
                ShowHideManager.Inst.EnterPlayMode();
                LockHideManager.Inst.EnterPlayMode();
                SensorBoxManager.Inst.EnterPlayMode();
                ParachuteManager.Inst.OnChangeMode(GameMode.Play);
                FirePropManager.Inst.OnChangeMode(GameMode.Play);
                FishingEditManager.Inst.OnModeChange(GameMode.Play);
                PGCEffectManager.Inst.OnChangeMode(GameMode.Play);
                break;
            case GameMode.Guest:
                ShowHideManager.Inst.EnterPlayMode();
                SensorBoxManager.Inst.EnterPlayMode();
                ParachuteManager.Inst.OnChangeMode(GameMode.Guest);
                FirePropManager.Inst.OnChangeMode(GameMode.Guest);
                FishingEditManager.Inst.OnModeChange(GameMode.Guest);
                PGCEffectManager.Inst.OnChangeMode(GameMode.Guest);
                break;
            case GameMode.Edit:
                ShowHideManager.Inst.EnterEditMode();
                SensorBoxManager.Inst.EnterEditMode();
                ParachuteManager.Inst.OnChangeMode(GameMode.Edit);
                BaggageManager.Inst.OnChangeMode(GameMode.Edit);
                FishingManager.Inst.OnChangeMode(GameMode.Edit);
                PickabilityManager.Inst.OnChangeMode(GameMode.Edit);
                FirePropManager.Inst.OnChangeMode(GameMode.Edit);
                FishingEditManager.Inst.OnModeChange(GameMode.Edit);
                PGCEffectManager.Inst.OnChangeMode(GameMode.Edit);
                LockHideManager.Inst.EnterEditMode();//这行代码要放到该case的最后
                break;
            default:
                break;
        }
    }

    private void OnPlayClick()
    {
        editController.gController.DisableGizmo();
        GlobalFieldController.OrgMapInfo = GlobalFieldController.CurMapInfo != null? GlobalFieldController.CurMapInfo.Clone() : null ;
        GlobalFieldController.orgMapContent = SceneParser.Inst.StageToMapJson();
        EnterPlayModel(GameMode.Play);
        ResBagManager.Inst.StopDownload();
        ParsePropWithTipsManager.Inst.DestroyGameObj();
    }

    private void EnterPlayModel(GameMode gameMode)
    {
        GlobalFieldController.CurGameMode = gameMode;
        InputReceiver.locked = false;
        UIManager.Inst.CloseAllDialog();
        MovePathManager.Inst.CloseAndSave();
        MovePathManager.Inst.ReleaseAllPoints();
        PlayModePanel.Show();
        PlayModePanel.Instance.OnEdit = EnterEditMode;
        PlayModePanel.Instance.EntryMode(gameMode);
        var joyStick = PlayModePanel.Instance.joyStick;
        playController.joyStick = joyStick;
        playController.SetCamera(mainCamera, PlayVirCamera);
        playController.SetPlayerState(playerCom,gameMode);
        EditVirCamera.enabled = false;
        PlayVirCamera.enabled = true;
        SceneBuilder.Inst.SpawnPoint.SetActive(false);
        if (gameMode != GameMode.Guest)
        {
            SceneBuilder.Inst.BgBehaviour.Play(true);
            SceneBuilder.Inst.BgBehaviour.PlayEnr();
            WeatherManager.Inst.ShowCurrentWeather();
            PlayerControlManager.Inst.ChangeAnimClips();
        }
        else
        {
#if !UNITY_EDITOR
            ClientManager.Inst.InitSyncData();
#endif
        }
        SceneBuilder.Inst.SetEntityMeshsVisibleByMode(false);
        SoundManager.Inst.AudioStop();
        ShotPhotoManager.Inst.InitPhotosVisiable();
        CrystalStoneManager.Inst.InitFirstToastPanel();
        PickabilityManager.Inst.OnChangeMode(gameMode);
        FollowModeManager.Inst.OnChangeMode(gameMode);
        LeaderBoardManager.Inst.OnChangeMode(gameMode);
        BaggageManager.Inst.OnChangeMode(gameMode);
        TrapSpawnManager.Inst.OnChangeMode(gameMode);
        EdibilityManager.Inst.RefreshFoodData();
        SceneSystem.Inst.StartSystem();
        EdibilityManager.Inst.GetParentData();
        //因为要获取游玩模式下的父节点信息
        FishingManager.Inst.OnChangeMode(gameMode);
    }

    IEnumerator GetText(string url, UnityAction<string> onSuccess, UnityAction<string> onFailure)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = 45;
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log(www.error);
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(www.downloadHandler.text);
        }
    }

    IEnumerator GetByte(string url, UnityAction<byte[]> onSuccess, UnityAction<string> onFailure)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = 45;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log(www.error);
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(www.downloadHandler.data);
        }
    }

    private void HandleMapInfo(MapInfo remote)
    {
        LoggerUtils.Log("remote mapInfo draftId == " + remote.draftId);
        MapInfo localInfo = DataUtils.GetMapInfoLocal();
        bool isLocalMode = false;
        if (localInfo != null)
        {
            isLocalMode = localInfo.draftId > remote.draftId;
            GameManager.Inst.gameMapInfo = isLocalMode ? localInfo : remote;
        }
        else
        {
            GameManager.Inst.gameMapInfo = remote;
        }
        GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();

        if (isLocalMode)
        {
            LoggerUtils.Log("Enter Local Mode");
            InitContinueEditByLocal();
        }
        else
        {
            LoggerUtils.Log("Enter Remote Mode");
            InitContinueEditScene();
        }
    }

    private void HandleFailure(string error)
    {
        MapInfo localInfo = DataUtils.GetMapInfoLocal();
        if (localInfo != null)
        {
            GameManager.Inst.gameMapInfo = localInfo;
            GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
            LoggerUtils.Log("GetMapInfo Fail -- Enter Local");
            InitContinueEditByLocal();
        }
        else
        {
            LoggerUtils.LogError("GetMapInfo Fail" + error);
            MobileInterface.Instance.Quit();
        }
    }

    private void OnDestroy()
    {
        CancelInvoke("AutoSaveMapOnEditMode");
        MessageHelper.RemoveListener(MessageName.EnterEdit, EnterEditMode);
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, HandleChangeMode);
        CInstanceManager.Release();
        NodeBehaviourManager.Release();
        GlobalFieldController.Clear();
        DataLogUtils.Clear();
        CombineUtils.Clear();
        GameUtils.Clear();
    }

    public IEnumerator WaitForFrameAndShow(bool isSuccess)
    {
        LoggerUtils.LogReport("WaitForFrameAndShow isSuccess" + isSuccess, "WaitForFrameAndShow_Start");
#if !UNITY_EDITOR
        MobileInterface.Instance.NotifyPercentFull();
#endif
        DataLogUtils.LogUnityCloseLoadingPage();
        yield return new WaitForSeconds(0.1f);  //wait for drawcall end
        StartCollectFPS();
        yield return new WaitForSeconds(0.7f);
        EndCollectFPS();
        if (GlobalFieldController.CurGameMode == GameMode.Guest && isSuccess)
        {
            // Guest 模式唤起Unity 第一帧时开始播放音乐
            SceneBuilder.Inst.BgBehaviour.Play(true);
            SceneBuilder.Inst.BgBehaviour.PlayEnr();
            WeatherManager.Inst.ShowCurrentWeather();
            QualityManager.Inst.SetQualityLevel(QualityManager.Inst.CheckQuality());
            
            //唤起第一帧后再开始播放3D视频
            VideoNodeManager.Inst.StartAllVideoPlay();
            LoggerUtils.LogReport("VideoNodeManager.Inst.StartAllVideoPlay", "3DVideo_Start");
            
            //游玩模式第一帧后刷一下昼夜天空盒
            SceneBuilder.Inst.SkyboxBev.OnWaitForFrameFinish();

            //游玩模式第一帧后刷一下手电灯
            FlashLightManager.Inst.EnterPlayMode();

            yield return null;
        }
        else if(GlobalFieldController.CurGameMode == GameMode.Edit && isSuccess)
        {
            VideoNodeManager.Inst.StartLoadAllVideoUrl();
            yield return null;
        }
        LoggerUtils.LogReport("MobileInterface.Instance.GetGameInfo Before", "GetGameInfo_Start");
#if !UNITY_EDITOR 
        GetDCListInfo();
        //关闭端上Loading页面时 再关闭UI遮罩
        UIMask.SetActive(false);
        MobileInterface.Instance.GetGameInfo();
        LoggerUtils.LogReport("MobileInterface.Instance.GetGameInfo End", "GetGameInfo_End");
        DataLogUtils.LogUnityCloseLoadingPageSuccess(isSuccess);
        if (GlobalFieldController.CurGameMode == GameMode.Guest && isSuccess)
        {
            DataLogUtils.LogEnterRoom();
        }
        // loading页面已经关闭
        GameManager.Inst.loadingPageIsClosed = true;
        if (GlobalFieldController.CurGameMode != GameMode.Edit && isSuccess && GameManager.Inst.ugcUserInfo.clothesIsBan == 1)
        {
            TipPanel.ShowToast("Your clothing was removed for violating our community guidelines.");
        }
        //fail exit
        if (!isSuccess)
        {
            TipPanel.ShowToast("Enter experience failed :(");
            yield return new WaitForSeconds(1f);
            MobileInterface.Instance.Quit();
            LoggerUtils.LogReport("Enter experience failed :(", "GetGameInfo_Fail");

        }
        else
        {
            DataLogUtils.totalPlayTime = 0;
            DataLogUtils.LogTotalPlayTime(0);
        }
#endif
        if (GlobalFieldController.CurGameMode == GameMode.Guest && isSuccess)
        {
            LoggerUtils.LogReport("ExceptionReport.RemoveLastReportOnGameSuccess","RemoveLastReportOnGameSuccess");
            ExceptionReport.RemoveLastReportOnGameSuccess();
        }
    }
    
    private void GetDCListInfo()
    {
        LoggerUtils.Log("OnDCListInfo "+ GameManager.Inst.gameMapInfo.mapId);
        var httpMapDataInfo = new HttpMapDataInfo
        {
            mapId = GameManager.Inst.gameMapInfo.mapId,
        };
        
        HttpUtils.MakeHttpRequest("/ugcmap/quoteDcInfo", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpMapDataInfo), OnDCListInfoGetSuccess, OnDCListInfoGetFail); ;
    }
    public void OnDCListInfoGetSuccess(string msg)
    {
        LoggerUtils.Log("OnDCListInfoGetSuccess   " + msg);
        HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        DcSoldOutListInfo info = JsonConvert.DeserializeObject<DcSoldOutListInfo>(hResponse.data);
        if (info!=null&&info.dcInfos!=null&&info.dcInfos.Count!=0)
        {
            UgcClothItemManager.Inst.SetSoldOutList(info.dcInfos);
        }
       
    }
    public void OnDCListInfoGetFail(string msg)
    {
        LoggerUtils.Log("OnDCListInfoGetFail   "+ msg);
    }
    private void StartCollectFPS()
    {
        fpsController.StartCollectFPS();
    }

    private void EndCollectFPS()
    {
        float fps = fpsController.GetAverageFPS();
        bool fpsEnough = fps > GameConsts.averageFPS;
        bool playerAllowBloom = GlobalSettingManager.Inst.IsBloomOpen();
        GlobalFieldController.isOpenPostProcess = fpsEnough && playerAllowBloom;
        SceneBuilder.Inst.PostProcessBehaviour.SetPostProcessActive(GlobalFieldController.isOpenPostProcess);
        WeatherManager.Inst.OnFpsCollected(fps);
    }
    public void Update()
    {
        if (FreezePropsManager.Inst!=null)
        {
            FreezePropsManager.Inst.Update(Time.deltaTime);
        }
        if (ParticleObjPool.Inst!=null)
        {
            ParticleObjPool.Inst.Update();
        }
        if (LadderManager.Inst != null)
        {
            LadderManager.Inst.Update();
        }
    }
    #region Loading阶段返回代码
    //玩家点击loading界面的return按钮响应的接口
    private void OnCloseLoading(string content)
    {
        LoggerUtils.LogReport("GameController OnCloseLoading", "OnCloseLoading_2");
        //存在多次调用的问题,避免多次点击强制退出
        if(GlobalFieldController.isGameProcessing)
        {
            GlobalFieldController.isGameProcessing = false;
            if (ClientManager.Inst != null)
            {
                ClientManager.Inst.RequestCloseLoading();
            }
        }
    }
    #endregion

    #region Snowfield 大地图新增功能

    private void InitDowntownScene()
    {

#if UNITY_EDITOR
        TestNetParams.Inst.CurrentConfig.isOpenNetTest = true;
#endif
        string downtownUrl = GameManager.Inst.gameMapInfo.mapJson;
        var downtownId = GameManager.Inst.gameMapInfo.mapId;
        //Test
        LoggerUtils.LogReport("InitDowntownScene downtownId =" + downtownId, "InitDowntownScene");
        GameManager.Inst.curDiyMapId = downtownId;
        GameManager.Inst.LoadMapAsyncCount = 0;
        preloadInfo = new PreloadGameInfo(3, () =>
        {
            var preloadUGCs = OfflineResManager.Inst.PreDealWithOfflineRes(preloadInfo.mapContent);
            LoggerUtils.LogReport("OfflineResManager.PreDealWithOfflineRes", "PreloadAssetBundle_Start");
            OfflineResManager.Inst.PreloadAssetBundle(preloadUGCs, () =>
            {
                LoggerUtils.LogReport(
                    "InitSDK preloadInfo.isSessionSuccess =" + preloadInfo.isSessionSuccess.ToString(),
                    "PreloadAssetBundle_callback");

                // Init SDK 前置
                if (preloadInfo.isSessionSuccess)
                {
                    ClientManager.Inst.InitSDK();
                }
                LoggerUtils.LogReport("InitSDK Success", "InitSDK_Success");
                OnGetSuccessDowntownGu(preloadInfo.mapContent);
                // 场景重建之后， 判断若获取Session失败，则进入单机模式
                if (!preloadInfo.isSessionSuccess)
                {
                    LoggerUtils.LogReport("preloadInfo fail", "PreloadInfo_GetSession_Fail");
                    ClientManager.Inst.EnterOfflineMode();
                }
            });
        });
        LoggerUtils.LogReport("Start GetSessionInfo", "GetSessionInfo_Start");
        ClientManager.Inst.GetSessionInfo((isSuccess) =>
        {
            LoggerUtils.LogReport(string.Format("GetSessionInfo isSuc={0} count={1}", isSuccess.ToString(),
                preloadInfo.PreloadCount), "GetSessionInfo_PreEnd");
            preloadInfo.isSessionSuccess = isSuccess;
            preloadInfo.PreloadCount++;
            LoggerUtils.LogReport("GetSessionInfo PreloadCount", "GetSessionInfo_End");
        });
        GlobalFieldController.CurGameMode = GameMode.Guest;
        GlobalFieldController.curMapMode = MapMode.Downtown;
        MapLoadManager.Inst.LoadMapJson(downtownUrl, (mapContent) =>
        {
            preloadInfo.mapContent = mapContent;
            LoggerUtils.LogReport(string.Format("LoadMapJson PreloadCount={0}", (preloadInfo.PreloadCount + 1).ToString(),
                preloadInfo.PreloadCount), "LoadMapJson_End");
            preloadInfo.PreloadCount++;

        }, OnLoadMapFailure);
        HLOD.Inst.LoadMapOfflineData(downtownId, () =>
        {
            LoggerUtils.LogReport(string.Format("LoadMapOfflineData PreloadCount={0}", preloadInfo.PreloadCount.ToString(),
                preloadInfo.PreloadCount), "LoadMapOfflineData_End");
            preloadInfo.PreloadCount++;
        });
    }

    private void OnGetSuccessDowntownGu(string content)
    {
        GameManager.Inst.downtownJson = content;
        LoggerUtils.LogReport("OnGetSuccessGu Start Parse Json", "OnGetSuccessGu_Start");
        DataLogUtils.LogUnityRestoreJsonStart();
        //SceneBuilder.Inst.DowntownParseAndBuild(content);
        DataLogUtils.LogUnityRestoreJsonEnd();
        LoggerUtils.Log("#######进房流程 解析完json");
        LoggerUtils.LogReport("OnGetSuccessDowntownGu Parse JSON Success", "OnGetSuccessGu_Success");
        EnterPlayModel(GameMode.Guest);
        LoggerUtils.LogReport("OnGetSuccessDowntownGu EnterPlayModel", "OnGetSuccessGu_EnterPlayModel");
        MessageHelper.Broadcast(MessageName.OnEnterSnowfield);
    }
    #endregion
}

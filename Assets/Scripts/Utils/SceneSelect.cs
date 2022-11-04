/// <summary>
/// Author:Mingo-LiZongMing
/// Description:场景选择大厅
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BudEngine.NetEngine.src.Util;
using BudEngine.NetEngine.src.Util.Def;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public enum SCENE_TYPE : int
{
   ROLE_SCENE = 0,//MapScene
   MAP_SCENE = 1,//RoleScene
   ResMAP_SCENE = 2,//PropScene
   DPreview = 3,// 3D Preview
   UGCClothes = 4, //UGCClothes
   CPreview = 5,// Cloth Preview
    MYSPACE_SCENE = 6, //MySpace
    UGCPatterns = 9,//UGC Pattern
    FPPreview = 10,//Pattern Preview
    UGCMaterial = 11,//UGC Material
    MPreview = 12,//UGC Material Preview
   Downtown = 13,//DowntownScene
}
public enum MAP_TYPE : int
{
    EMPTY = 1,
    PLAYMODE = 2,
    EDITMODE = 3,
}

public class SceneSelect : MonoBehaviour
{
    public static SceneSelect Inst;

    public void OnStart()
    {
        Inst = this;
        if (GlobalFieldController.isGameProcessing) return;
        GlobalFieldController.isGameProcessing = true;
        DontDestroyOnLoad(this.gameObject);
        QualitySettings.vSyncCount = 0;
        Time.maximumDeltaTime = 0.1f;
        ExceptionReport.Init();
        AssetBundleLoaderMgr.Inst.Init();
        ExceptionReport.LogPreReport("Enter Scene", "Enter_Unity");
#if UNITY_ANDROID
        QualitySettings.SetQualityLevel(0, true);
        QualitySettings.skinWeights = SkinWeights.TwoBones;
        MobileInterface.Instance.InjectStartCallBack(OnReStartUnity);
        Resources.UnloadUnusedAssets();
        GC.Collect();
        AndroidEngineEntry();
#endif

#if UNITY_IPHONE
            Resources.UnloadUnusedAssets();
            GC.Collect();
            IOSEngineEntry();
#endif
    }

#if UNITY_ANDROID
    private void OnReStartUnity()
    {
        if(GlobalFieldController.isGameProcessing) return;
        GlobalFieldController.isGameProcessing = true;
        Resources.UnloadUnusedAssets();
        GC.Collect();
        ExceptionReport.Init();
        AssetBundleLoaderMgr.Inst.Init();
        ExceptionReport.LogPreReport("Enter Scene Again", "Enter_Unity");
        AndroidEngineEntry();
    }
    public void AndroidEngineEntry()
    {
        if(!GlobalFieldController.isGameProcessing) return;
        var gameAndroidInter = AndroidInterface.GetObject();
        if (gameAndroidInter == null)
        {
            LoggerUtils.Log("Unity==== Enter Character------");  
            GameManager.Inst.nativeType = NATIVE_TYPE.ROLECALL;
            DataLogUtils.LogUnityStartGame();
            DataLogUtils.LogUnityGetEngineEntyReq();
            MobileInterface.Instance.AddClientRespose(MobileInterface.getEngineEntry, GetEngineEntry);
            MobileInterface.Instance.GetEngineEntryRole();
        }
        else
        {
            LoggerUtils.Log("Unity==== Enter Editor------");
            GameManager.Inst.nativeType = NATIVE_TYPE.MAPCALL;
            DataLogUtils.LogUnityStartGame();
            DataLogUtils.LogUnityGetEngineEntyReq();
            MobileInterface.Instance.AddClientRespose(MobileInterface.getEngineEntry, GetEngineEntry);
            MobileInterface.Instance.GetEngineEntry();
        }
    }
    

#endif

#if UNITY_IPHONE
    public void IOSEngineEntry()
    {
        GameManager.Inst.nativeType = NATIVE_TYPE.MAPCALL;
        DataLogUtils.LogUnityStartGame();
        DataLogUtils.LogUnityGetEngineEntyReq();
        MobileInterface.Instance.AddClientRespose(MobileInterface.getEngineEntry, GetEngineEntry);
        MobileInterface.Instance.GetEngineEntry();
    }
#endif

    private void InitLogView()
    {
#if !UNITY_EDITOR
        var rego = GameObject.Find("Reporter");
        var environment = GameManager.Inst.baseGameJsonData.baseInfo.environment;
        LoggerUtils.IsDebug = environment == "master" || environment == "pr";
        SdkUtil.IsDebug = environment == "master" || environment == "pr";
        FPSPanel.Instance.gameObject.SetActive(environment == "master" || environment == "pr");
        if (rego != null)
        {
            if (LoggerUtils.IsDebug)
            {
                rego.GetComponent<Reporter>().numOfCircleToShow = 5;
            }
            else
            {
                Destroy(rego);
            }
        }
#endif
    }

    private void GetEngineEntry(string content)
    {
        ExceptionReport.LogPreReport("GetEngineEntry Success", "GetEngineEntry_Success");
        var engineEntry = JsonConvert.DeserializeObject<EngineEntry>(content);
        GameManager.Inst.engineEntry = engineEntry;
        GlobalFieldController.CurSceneType = (SCENE_TYPE)engineEntry.sceneType;
        LoggerUtils.LogReport(string.Format("engineEntry.sceneType = ") + engineEntry.sceneType, "LoadScene_Success");
        DataLogUtils.LogUnityGetEngineEntyRsp();
        MobileInterface.Instance.AddClientRespose(MobileInterface.closeLoading, OnCloseLoading);
        GetGameJsonBySceneType();
    }

    private void GetGameJsonBySceneType()
    {
        var engineEntry = GameManager.Inst.engineEntry;
        switch ((SCENE_TYPE)engineEntry.sceneType)
        {
            case SCENE_TYPE.MAP_SCENE:
            case SCENE_TYPE.ROLE_SCENE:
            case SCENE_TYPE.ResMAP_SCENE:
            case SCENE_TYPE.DPreview:
            case SCENE_TYPE.UGCClothes:
            case SCENE_TYPE.UGCPatterns:
            case SCENE_TYPE.CPreview:
            case SCENE_TYPE.FPPreview:
            case SCENE_TYPE.MYSPACE_SCENE:
            case SCENE_TYPE.MPreview:
            case SCENE_TYPE.UGCMaterial:
                MobileInterface.Instance.AddClientRespose(MobileInterface.getGameJson, GetGameJson);
                MobileInterface.Instance.GetGameJson();
                break;
            case SCENE_TYPE.Downtown:
                MobileInterface.Instance.AddClientRespose(MobileInterface.getDowntownJson, GetDowntownJson);
                MobileInterface.Instance.GetDowntownJson();
                break;
        }
    }

    private void GetGameJson(string content)
    {
        if(!GlobalFieldController.isGameProcessing) return;
        GetGameJson nativeGameInfo = JsonConvert.DeserializeObject<GetGameJson>(content);
        GameManager.Inst.baseGameJsonData = nativeGameInfo;
        GameManager.Inst.unityConfigInfo = nativeGameInfo.configInfo;
        GameManager.Inst.ugcUntiyMapDataInfo = nativeGameInfo.ugcMapDataInfo;
        GameManager.Inst.onLineDataInfo = nativeGameInfo.onLineDataInfo;
        GameManager.Inst.gameMapInfo = nativeGameInfo.unityMapInfo.mapInfo;
        GameManager.Inst.isInWhiteList = nativeGameInfo.configInfo.isWhiteUser == true ? 1 : 0;
        GameManager.Inst.ugcUserInfo = nativeGameInfo.unityUserInfo;
        GameManager.Inst.ugcClothInfo = nativeGameInfo.unityUGCClothInfo;
        LocalizationConManager.Inst.Initialize(nativeGameInfo.baseInfo.lang, nativeGameInfo.baseInfo.locale);
        GameInfo.Inst.myUid = nativeGameInfo.baseInfo.uid;
        HttpUtils.tokenInfo = nativeGameInfo.baseInfo;
        HttpUtils.RequestUrl = nativeGameInfo.baseInfo.baseUrl;
        GlobalFieldController.IsDowntownEnter = false;
        AWSUtill.SetAwsSavingPath();
        DataLogUtils.LogUnityGetGameJsonRsp();

        ExceptionReport.LogPreReport("GetGameJson Success","GetGameJson_Success");

        //新手注册流程需要清除uid和token
        HandleNativeBaseInfo();

        var bundleFileListData = GameObject.Find("BundleFileListData").GetComponent<BundleFileListData>();
        CoroutineManager.Inst.StartCoroutine(ResManager.Inst.LoadConfig(GameManager.Inst.baseGameJsonData.u3dSourcesConfigVersion,
            ()=>
            {
                BundleMgr.Inst.LoadBundleList(bundleFileListData.bundleFileListAndroid.text);
          		EnterScene();
            }, 
            ()=>
            {
                LoggerUtils.LogError("SceneSelect::GetGameJson update config fail, unexpected error will be occurred");
                BundleMgr.Inst.LoadBundleList(bundleFileListData.bundleFileListAndroid.text);
         		EnterScene();	
            }));
        InitLogView();
    }

    public void GetDowntownJson(string content)
    {
        GetDowntownJson nativeGameInfo = JsonConvert.DeserializeObject<GetDowntownJson>(content);

        GameManager.Inst.baseGameJsonData = nativeGameInfo;
        GameManager.Inst.unityConfigInfo = nativeGameInfo.configInfo;
        GameManager.Inst.ugcUntiyMapDataInfo = MapInfoConvertManager.Inst.ConvertDowntownDataInfoToMapDataInfo(nativeGameInfo.downtownDataInfo);
        GameManager.Inst.isInWhiteList = nativeGameInfo.configInfo.isWhiteUser == true ? 1 : 0;
        GameManager.Inst.onLineDataInfo = nativeGameInfo.onLineDataInfo;
        GameManager.Inst.ugcUserInfo = nativeGameInfo.unityUserInfo;
        GameManager.Inst.downtownInfo = nativeGameInfo.unityDowntownInfo.downtownInfo;
        GameManager.Inst.gameMapInfo = MapInfoConvertManager.Inst.ConvertDowntownInfoToMapInfo(nativeGameInfo.unityDowntownInfo.downtownInfo);
        GlobalFieldController.IsDowntownEnter = true;
        LocalizationConManager.Inst.Initialize(nativeGameInfo.baseInfo.lang, nativeGameInfo.baseInfo.locale);
        GameInfo.Inst.myUid = nativeGameInfo.baseInfo.uid;
        HttpUtils.tokenInfo = nativeGameInfo.baseInfo;
        HttpUtils.RequestUrl = nativeGameInfo.baseInfo.baseUrl;

        AWSUtill.SetAwsSavingPath();
        DataLogUtils.LogUnityGetGameJsonRsp();

        ExceptionReport.LogPreReport("GetDowntown Success", "GetDowntown_Success");

        //新手注册流程需要清除uid和token
        HandleNativeBaseInfo();

        var bundleFileListData = GameObject.Find("BundleFileListData").GetComponent<BundleFileListData>();
        CoroutineManager.Inst.StartCoroutine(ResManager.Inst.LoadConfig(GameManager.Inst.baseGameJsonData.u3dSourcesConfigVersion,
            () =>
            {
                BundleMgr.Inst.LoadBundleList(bundleFileListData.bundleFileListAndroid.text);
                EnterScene();
            },
            () =>
            {
                LoggerUtils.LogError("SceneSelect::GetGameJson update config fail, unexpected error will be occurred");
                BundleMgr.Inst.LoadBundleList(bundleFileListData.bundleFileListAndroid.text);
                EnterScene();
            }));
        InitLogView();

    }
    
    private void EnterScene()
    {
        var engineEntry = GameManager.Inst.engineEntry;
        if (GameManager.Inst.unityConfigInfo != null && GameManager.Inst.unityConfigInfo.featSwitch != null)
        {
            if (((SCENE_TYPE)engineEntry.sceneType == SCENE_TYPE.MAP_SCENE
                 || (SCENE_TYPE)engineEntry.sceneType == SCENE_TYPE.MAP_SCENE)
                && (EnterGameMode)engineEntry.subType == EnterGameMode.GuestScene)
            {
                ExceptionReport.IsReportLog = GameManager.Inst.unityConfigInfo.featSwitch.enableLogUpload;
                ExceptionReport.CreateReportLogOnGameStart();
            }
        }

        switch ((SCENE_TYPE)engineEntry.sceneType)
        {
            case SCENE_TYPE.MAP_SCENE:
                GameManager.Inst.sceneType = SCENE_TYPE.MAP_SCENE;
                SceneManager.LoadScene(2);
                break;
            case SCENE_TYPE.ROLE_SCENE:
                GameManager.Inst.sceneType = SCENE_TYPE.ROLE_SCENE;
                Application.targetFrameRate = 60;
                SceneManager.LoadScene(1);
                break;
            case SCENE_TYPE.ResMAP_SCENE:
                GameManager.Inst.sceneType = SCENE_TYPE.ResMAP_SCENE;
                Application.targetFrameRate = 60;
                SceneManager.LoadScene(3);
                break;
            case SCENE_TYPE.DPreview:
                GameManager.Inst.sceneType = SCENE_TYPE.DPreview;
                Application.targetFrameRate = 60;
                SceneManager.LoadScene(4);
                break;
            case SCENE_TYPE.UGCClothes:
                GameManager.Inst.sceneType = SCENE_TYPE.UGCClothes;
                Application.targetFrameRate = 60;
                SceneManager.LoadScene(5);
                break;
            case SCENE_TYPE.UGCPatterns:
                GameManager.Inst.sceneType = SCENE_TYPE.UGCPatterns;
                SceneManager.LoadScene(5);
                break;
            case SCENE_TYPE.CPreview:
                GameManager.Inst.sceneType = SCENE_TYPE.CPreview;
                Application.targetFrameRate = 60;
                SceneManager.LoadScene(6);
                break;
            case SCENE_TYPE.FPPreview:
                GameManager.Inst.sceneType = SCENE_TYPE.FPPreview;
                SceneManager.LoadScene(6);
                break;
            case SCENE_TYPE.MYSPACE_SCENE:
                GameManager.Inst.sceneType = SCENE_TYPE.MYSPACE_SCENE;
                SceneManager.LoadScene(2);
                break;
            case SCENE_TYPE.Downtown:
                GameManager.Inst.sceneType = SCENE_TYPE.Downtown;
                SceneManager.LoadScene(2);
 				break;
            case SCENE_TYPE.UGCMaterial:
                GameManager.Inst.sceneType = SCENE_TYPE.UGCMaterial;
                Application.targetFrameRate = 60;
                SceneManager.LoadScene(5);
                break;
            case SCENE_TYPE.MPreview:
                GameManager.Inst.sceneType = SCENE_TYPE.MPreview;
                Application.targetFrameRate = 60;
                SceneManager.LoadScene(4);
                break;
        }
    }

    private void HandleNativeBaseInfo()
    {
        var entry = GameManager.Inst.engineEntry;
        //新手注册流程需要清除uid和token
        if ((SCENE_TYPE)entry.sceneType == SCENE_TYPE.ROLE_SCENE && (ROLE_TYPE)entry.subType == ROLE_TYPE.FIRST_ENTRY)
        {
            GameInfo.Inst.myUid = string.Empty;
            HttpUtils.tokenInfo.uid = string.Empty;
            HttpUtils.tokenInfo.token = string.Empty;
        }
    }
    private void OnCloseLoading(string content)
    {
        LoggerUtils.LogReport("SceneSelect OnCloseLoading", "OnCloseLoading_1");
        if(GlobalFieldController.isGameProcessing)
        {
            GlobalFieldController.isGameProcessing = false;
            var androidClass = AndroidInterface.GetObject();
            if (androidClass == null)
            {
                MobileInterface.Instance.QuitRole();
            }
            else
            {
                MobileInterface.Instance.Quit();
            }
        }
    }

    private void OnDestroy()
    {
        GlobalFieldController.isGameProcessing = false;
        DontDestroyUtils.Dispose();
        MobileInterface.Instance.DelClientResponse(MobileInterface.closeLoading);
        Inst = null;
    }
}

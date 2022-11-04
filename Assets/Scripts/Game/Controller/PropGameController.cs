using System;
using System.Collections;
using System.IO;
using Amazon;
using Cinemachine;
using Leopotam.Ecs;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

//client enter Unity editor mode
public enum EnterResEditMode
{
    CreateEmptyScene = 1,
    ContinueEditScene = 2
}



public class PropGameController:MonoBehaviour
{
    public GameObject UIMask;//在端上loading页关闭之前，放置一个Mask遮罩拦截点击事件
    public EnterResEditMode curGameMode = EnterResEditMode.CreateEmptyScene;
    public CinemachineVirtualCamera EditVirCamera;
    private EditModeController editController;
    private Camera mainCamera;

#if UNITY_EDITOR
    private void Awake()
    {
        UIMask.SetActive(false);
    }
#endif

    private void Start()
    {
        MainThreadDispatcher.Init();
        StartGame();
    }

    public void StartGame()
    {
        QualitySettings.vSyncCount = 0;
        UnityInitializer.AttachToGameObject(this.gameObject);
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        CameraUtils.Inst.SetMainCamera(mainCamera);
        GameManager.Inst.Init();
        UIManager.Inst.Init();
        SceneBuilder.Inst.Init();
        DataUtils.InitEditSaveInfo();
#if !UNITY_EDITOR
        curGameMode = (EnterResEditMode)GameManager.Inst.engineEntry.subType;
        LoggerUtils.Log("curGameMode" + (int)curGameMode);
#endif
        EnterGameByClient(curGameMode);
    }

    private void EnterGameByClient(EnterResEditMode curMode)
    {
        SceneBuilder.Inst.InitSceneParent();
        BasePrimitivePanel.SetResIDs(GameConsts.ResEditIds);
        switch (curMode)
        {
            case EnterResEditMode.CreateEmptyScene:
#if !UNITY_EDITOR
                GameManager.Inst.gameMapInfo = new MapInfo();
                GameManager.Inst.gameMapInfo.mapName = GameManager.Inst.ugcUntiyMapDataInfo.mapName;
                GameManager.Inst.gameMapInfo.dataType = (int)MapSaveType.Prop;
#else
                GameManager.Inst.gameMapInfo = new MapInfo();
                GameManager.Inst.gameMapInfo.mapName = "Ted";
                GameManager.Inst.gameMapInfo.dataType = (int)MapSaveType.Prop;
#endif
                SceneBuilder.Inst.CreateEmptyScene();
                EditModeController.SavePropByFirst();
                InitEditMode();
                EnterEditMode();
                StartCoroutine(WaitForFrameAndShow(true));
                break;
            case EnterResEditMode.ContinueEditScene:
                DataLogUtils.LogUnityGetMapInfoReq();
                MapLoadManager.Inst.GetMapInfo(GameManager.Inst.ugcUntiyMapDataInfo,getMapInfo => {
                    DataLogUtils.LogUnityGetMapInfoRsp("0");
                    HandleMapInfo(getMapInfo.mapInfo);
                }, (error) => {
                    SavingData.HttpResponseRaw httpResponseRaw = GameUtils.GetHttpResponseRaw(error);
                    DataLogUtils.LogUnityGetMapInfoRsp(httpResponseRaw.result.ToString());
                    HandleFailure(error);
                });
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
                MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_rsp, "0");
                string jsonStr = ZipUtils.SaveZipFromByte(content);
                if(string.IsNullOrEmpty(jsonStr))
                {
                    LoggerUtils.LogError("UnZip Failed");
                    OnInitFail();
                    return;
                }
                OnInitSuccess(jsonStr);
            }, (error) =>
            {
                MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_rsp, "-1");
                OnInitFail();
            }));
        }
        else
        {
            MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_req);
            StartCoroutine(GetText(mapUrl, (content) =>
            {
                MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_rsp, "0");
                OnInitSuccess(content);
            }, (error) =>
            {
                MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_rsp, "-1");
                OnInitFail();
            }));
        }
    }

    private void InitContinueEditByLocal()
    {
        string filePath = DataUtils.DraftPath + Data_Type.Map.ToString().ToLower() + ".zip";
        if (!File.Exists(filePath))
        {
            LoggerUtils.LogError("local map json not exist! => " + filePath);
            OnInitFail();
            return;
        }
        byte[] content = File.ReadAllBytes(filePath);
        string jsonStr = ZipUtils.SaveZipFromByte(content);
        if (string.IsNullOrEmpty(jsonStr))
        {
            LoggerUtils.LogError("local map json unzip failed! => " + filePath);
            OnInitFail();
            return;
        }
        LoggerUtils.Log("LocalRead -- mapJsonStr = " + jsonStr);
        OnInitSuccess(jsonStr);
    }

    private void OnInitSuccess(string jsonStr)
    {
        SceneBuilder.Inst.ParseAndBuild(jsonStr);
        SceneBuilder.Inst.SpawnPoint.SetActive(false);
        SceneBuilder.Inst.PostProcessBehaviour.gameObject.SetActive(false);
        //初始化时，写入本地配置文件
        DataUtils.SetConfigLocal(CoverType.PNG);
        InitEditMode();
        EnterEditMode();
        StartCoroutine(WaitForFrameAndShow(true));
    }

    private void OnInitFail()
    {
        LoggerUtils.LogError("Get MapJson Fail");
        StartCoroutine(WaitForFrameAndShow(false));
    }

    private void InitEditMode()
    {
        editController = new EditModeController();
        editController.Init();
        editController.SetCamera(mainCamera, EditVirCamera);
        InvokeRepeating("AutoSaveMapOnEditMode", 300, 300);
    }

    private void AutoSaveMapOnEditMode()
    {
        EditModeController.SavePropJsonByAuto();
    }


    private void EnterEditMode()
    {
        EditVirCamera.enabled = true;
        InputReceiver.locked = false;
        PlayModePanel.Hide();
        BasePrimitivePanel.Show();
        BasePrimitivePanel.Instance.isGameEditScene = false;
        BasePrimitivePanel.Instance.UpdateUI();
        SceneGizmoPanel.Show();
        editController.SetEditHandler();
        BasePrimitivePanel.Instance.OnSelect = editController.CreatePritiveModel;

        PropEditModePanel.Show();
        PropEditModePanel.Instance.SetGizmoController(editController.gController);
        SceneBuilder.Inst.SetEntityMeshsVisibleByMode(true);
    }

    
    IEnumerator GetText(string url, UnityAction<string> onSuccess, UnityAction<string> onFailure)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
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
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
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
            StartCoroutine(WaitForFrameAndShow(false));
        }
    }

    IEnumerator WaitForFrameAndShow(bool isSuccess)
    {
#if !UNITY_EDITOR
        MobileInterface.Instance.NotifyPercentFull();
#endif
        yield return new WaitForSeconds(0.8f);
        //关闭端上Loading页面时 再关闭UI遮罩
        UIMask.SetActive(false);
        MobileInterface.Instance.GetGameInfo();
        //fail exit
        if (!isSuccess)
        {
            TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
            LoggerUtils.LogError("Enter Map Fail");
            yield return new WaitForSeconds(1f);
            BlackPanel.Show();
            yield return new WaitForSeconds(0.1f);
            MobileInterface.Instance.Quit();
        }
    }

    private void OnDestroy()
    {
        CancelInvoke("AutoSaveMapOnEditMode");
        CInstanceManager.Release();
        NodeBehaviourManager.Release();
        GlobalFieldController.Clear();
    }
}
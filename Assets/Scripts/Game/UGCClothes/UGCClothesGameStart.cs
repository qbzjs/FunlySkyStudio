using System.Collections;
using System.Collections.Generic;
using Amazon;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using SavingData;

public enum EnterUGCClothesMode
{
    CreateEmptyScene = 1,
    ContinueEditScene = 2,
}

public class UGCClothesGameStart : MonoBehaviour
{
    public GameObject UIMask;
    [HideInInspector]
    public MainUGCResPanel mainPanel;
    public MainUGCPanelResHandler resHandler;
    public EnterUGCClothesMode EnterMode = EnterUGCClothesMode.ContinueEditScene;
    SCENE_TYPE sceneType = SCENE_TYPE.UGCClothes;
    public int tempId = 1;
#if UNITY_EDITOR
    private void Awake()
    {
        UIMask.SetActive(false);
    }
#endif

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.DontDestroy();
        MainThreadDispatcher.Init();
        DataUtils.InitEditSaveInfo();
        UGCClothesDataManager.Inst.Init();
        RoleConfigDataManager.Inst.LoadRoleConfig();
        UnityInitializer.AttachToGameObject(this.gameObject);
#if !UNITY_EDITOR
        EnterMode = (EnterUGCClothesMode)GameManager.Inst.engineEntry.subType;
        LoggerUtils.Log("EnterMode" + (int)EnterMode);
        tempId = GameManager.Inst.unityConfigInfo.templateId;
#endif
        SetSceneType();
        GetMainPanel();
        EnterGameByClient(EnterMode);

    }

    private void EnterGameByClient(EnterUGCClothesMode mode)
    {
        switch (mode)
        {
            case EnterUGCClothesMode.CreateEmptyScene:

                UGCClothesDataManager.Inst.InitDataByCreate(tempId);
                mainPanel.OnInit(resHandler);
                mainPanel.GenerateUGCClothes();
                mainPanel.OnSaveUGCResByFirst();
                StartCoroutine(WaitForFrameAndShow(true));
                break;
            case EnterUGCClothesMode.ContinueEditScene:
                //OnGetSuccessEd(File.ReadAllText(DataUtils.ugcClothesDataDir+ "_1645562276_json_clothes.json"));
                MapLoadManager.Inst.GetMapInfo(GameManager.Inst.ugcUntiyMapDataInfo, (getInfo) => {
                    HandleMapInfo(getInfo.mapInfo);
                }, (error) => {
                    SavingData.HttpResponseRaw httpResponseRaw = GameUtils.GetHttpResponseRaw(error);
                    HandleFailure(error);
                });
                break;
        }
    }
    private void SetSceneType()
    {
#if !UNITY_EDITOR
            sceneType = (SCENE_TYPE)GameManager.Inst.engineEntry.sceneType;
#else
        if (tempId < 1000)
        {
            sceneType = SCENE_TYPE.UGCClothes;
        }
        else if (tempId > 1000 && tempId < 10000)
        {
            sceneType = SCENE_TYPE.UGCPatterns;
        }
        else if (tempId >= 10000)
        {
            sceneType = SCENE_TYPE.UGCMaterial;
        }
#endif
    }
    private void GetMainPanel()
    {
        if (mainPanel == null)
        {
            switch (sceneType)
            {
                case SCENE_TYPE.UGCClothes:
                case SCENE_TYPE.UGCPatterns:
                    mainPanel = resHandler.gameObject.AddComponent<MainUGCClothPanel>();
                    break;
                case SCENE_TYPE.UGCMaterial:
                    mainPanel = resHandler.gameObject.AddComponent<MainUGCMaterialPanel>();
                    break;
            }
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
            LoggerUtils.Log("GetMapInfo Fail -- Enter Local");
            InitContinueEditByLocal();
        }
        else
        {
            LoggerUtils.LogError("GetMapInfo Fail" + error);
            MobileInterface.Instance.Quit();
        }
    }

    private void InitContinueEditByLocal()
    {
        string filePath = "";
        switch (sceneType)
        {
            case SCENE_TYPE.UGCClothes:
            case SCENE_TYPE.UGCPatterns:
                filePath = DataUtils.DraftPath + "clothJson.json";
                break;
            case SCENE_TYPE.UGCMaterial:
                filePath = DataUtils.DraftPath + "dataJson.json";
                break;
        }
       
        if (!File.Exists(filePath))
        {
            LoggerUtils.LogError("local cloth json not exist! => " + filePath);
            MobileInterface.Instance.Quit();
            return;
        }
        string content = File.ReadAllText(filePath);
        if (string.IsNullOrEmpty(content))
        {
            LoggerUtils.LogError("local cloth json is empty! => " + filePath);
            MobileInterface.Instance.Quit();
            return;
        }
        OnGetSuccessEd(content);
        LoggerUtils.Log("LocalRead -- clothJsonStr = " + content);
    }

    private void InitContinueEditScene()
    {
        Debug.Log(sceneType+"     "+ GameManager.Inst.gameMapInfo.jsonUrl);
        string clothJsonUrl = "";
        switch (sceneType)
        {
            case SCENE_TYPE.UGCClothes:
            case SCENE_TYPE.UGCPatterns:
                clothJsonUrl =  GameManager.Inst.gameMapInfo.clothesJson;
                break;
            case SCENE_TYPE.UGCMaterial:
             
                clothJsonUrl = GameManager.Inst.gameMapInfo.jsonUrl;
                break;
        }
        StartCoroutine(GetText(clothJsonUrl, (content) =>
        {
            OnGetSuccessEd(content);
        }, (error) =>
        {
            LoggerUtils.Log("InitContinueEditScene DownLoadJsonFail ");
            MobileInterface.Instance.Quit();
        }));
    }

    private void OnGetSuccessEd(string content)
    {
        UGCClothesDataManager.Inst.InitDataByCreate(content);
        DataUtils.SetConfigLocal(CoverType.PNG);
        mainPanel.OnInit(resHandler);
        mainPanel.GenerateUGCClothes();
#if !UNITY_EDITOR
        StartCoroutine(WaitForFrameAndShow(true));
#endif
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

    public IEnumerator WaitForFrameAndShow(bool isSuccess)
    {
#if !UNITY_EDITOR
        MobileInterface.Instance.NotifyPercentFull();
#endif
        yield return new WaitForSeconds(0.8f);
#if !UNITY_EDITOR
        UIMask.SetActive(false);
        MobileInterface.Instance.GetGameInfo();
        if (!isSuccess)
        {
            yield return new WaitForSeconds(1f);
            MobileInterface.Instance.Quit();
        }
#endif
    }

    private void OnDestroy()
    {
        CInstanceManager.Release();
    }

}

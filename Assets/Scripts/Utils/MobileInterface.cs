using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using UnityEngine;
using SavingData;
using UnityEditor;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public enum ReturnType
{
#if UNITY_ANDROID
    Return = 1,
    Go = 2,
    Next = 5,
    Search = 3,
    Send = 4,
    Done = 6,
#endif

#if UNITY_IPHONE
    Return = 0,
    Go = 1,
    Next = 4,
    Search = 6,
    Send = 7,
    Done = 9,
#endif
}

public class MobileInterface : MonoBehaviour
{
    private static MobileInterface instance;

    public static MobileInterface Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject(typeof(MobileInterface).Name);
                instance = go.AddComponent<MobileInterface>();
            }
            return instance;
        }
    }

    public const string showKeyboard = "showKeyboard";
    public const string hideKeyboard = "hideKeyboard";
    public const string getGameJson = "getGameJson";
    public const string getDowntownJson = "getDowntownJson";
    public const string getGameInfo = "getGameInfo";
    public const string finishUnityHeadUrlSetup = "finishUnityHeadUrlSetup";
    public const string getEngineEntry = "getEngineEntry";
    public const string openResourceStore = "openResourceStore";
    public const string uploadResource = "uploadResource";
    public const string updateSelfResourceStore = "updateSelfResourceStore";
    public const string openSystemAlbum = "openSystemAlbum";
    public const string skipNative = "skipNative";
    public const string uploadImage = "uploadImage";
    public const string openNativeShareDialog = "openNativeShareDialog";
    public const string openPersonalHomePage = "openPersonalHomePage";
    public const string onUpdateDataAction = "UpdateDataAction";
    public const string saveMediaToLocal = "saveMediaToLocal";
    public const string openProfilePage = "openProfilePage";
    public const string openNativeDetailPage = "openNativeDetailPage";
    public const string getProfilePath = "getProfilePath";
    public const string checkWallet = "checkWallet";
    public const string createOrImportWallet = "createOrImportWallet";
    public const string createWalletInterface = "createWalletInterface";
    public const string importWalletInterface = "importWalletInterface";
    public const string exceptionReport = "exceptionReport";
    public const string logEvent = "logEvent";
    public const string leaveRoomRedundancy = "leaveRoomRedundancy";
    public const string openClothStore = "openClothStore";
    public const string updateClothList = "updateClothList";
    public const string openLandH5Page = "openLandH5Page";
    public const string uploadToAws = "uploadToAws";
    public const string uploadToAlbum = "uploadToAlbum";
    public const string refreshAvatarRedDot = "refreshAvatarRedDot"; 
    public const string notifyPercentFull = "notifyPercentFull";   
    public const string openUgcClothPage = "openUgcClothPage";
    public const string requestNativePermission = "requestNativePermission";
    public const string openNativeSettingsPage = "openNativeSettingsPage";
    public const string refreshPlayerInfo = "refreshPlayerInfo";
    public const string updateWearClothsResource = "updateWearClothsResource";
    public const string loadingDialog = "loadingDialog";
    public const string closeLoading = "closeLoading";
    public const string closeSession = "closeSession";
    public const string getDCResSoldNum = "getDCResSoldNum";
    public const string openUgcResPage = "openUgcResPage";
    public const string openDcResPage = "openDcPage";
    public const string openPromotePage = "openPromotePage";
    public const string updateDownloadProgress = "updateDownloadProgress";
    public const string newUserRegistrationCompleted = "newUserRegistrationCompleted";
    public const string exitGame = "exitGame";
    public const string getPublicAvatarUserInfo = "getPublicAvatarUserInfo";
    public const string openAvatarItemDetail = "openAvatarItemDetail";
    public const string openUnityTryOnPage = "openUnityTryOnPage";
    public const string openUnityWearPage = "openUnityWearPage";
    public const string viewWillAppear = "viewWillAppear";
    public const string openCheckInRewardsPage = "openCheckInRewardsPage";
    public const string openStorePage = "openStorePage";
    public const string closeNativePage = "closeNativePage";
    [HideInInspector]
    public UnityEvent<string> onGetGameJson;
    [HideInInspector]
    public UnityEvent<string> onGetPeopleImage;
    
    private enum SceneMode
    {
        EditScene,
        GuestScene
    }
    
    private enum HVMode
    {
        Horizontal, 
        Vertical 
    }
    
    [HideInInspector]
    public Dictionary<string, UnityAction<string>>  onClientRespose = new Dictionary<string, UnityAction<string>>();
    public Dictionary<string, UnityAction<string>> onClientFail = new Dictionary<string, UnityAction<string>>();
    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void AddClientRespose(string key, UnityAction<string> callback)
    {
        if (onClientRespose.ContainsKey(key))
        {
            onClientRespose.Remove(key);
        }
        onClientRespose.Add(key, callback);
    }

    public void DelClientResponse(string key)
    {
        if (onClientRespose.ContainsKey(key))
        {
            onClientRespose.Remove(key);
        }
    }


    public void AddClientFail(string key, UnityAction<string> callback)
    {
        if (onClientFail.ContainsKey(key))
        {
            onClientFail.Remove(key);
        }
        onClientFail.Add(key, callback);
    }

    public void Notify(string funcName, IMobileNotify notifyData = null)
    {
        Notify(funcName, notifyData != null ? JsonConvert.SerializeObject(notifyData) : "");
    }
    
    public void NotifyRole(string funcName, IMobileNotify notifyData = null)
    {
        NotifyRole(funcName, notifyData != null ? JsonConvert.SerializeObject(notifyData) : "");
    }

    public void Notify(string funcName, string data)
    {
#if UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        AndroidInterface.Call(funcName, data);
#elif UNITY_IPHONE
        IOSInterface.sendMessageToClient(funcName, data);
#endif
    }
    
    public void NotifyRole(string funcName, string data)
    {
#if UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        AndroidInterface.RoleCall(funcName, data);
#elif UNITY_IPHONE
        IOSInterface.sendMessageToClient(funcName, data);
#endif
    }



    /*** interfaces ***/
#if UNITY_ANDROID
    //Notifies the client to Show (MapScene)
    //目前android和Unity交互存在多次点击问题
    private bool _openAvoidMultClick = true;

    private bool openAvoidMultClick
    {
        get => _openAvoidMultClick;
        set
        {
            if (!value)
            {
                CancelInvoke("AvoidMultClickActive");
                Invoke("AvoidMultClickActive", 1);
            }

            _openAvoidMultClick = value;
        }
    }

    private Action RestartUnity;
    public void StartUnity(string msg)
    {
        ClientResponse response = JsonUtility.FromJson<ClientResponse>(msg);
        LoggerUtils.LogError("StartUnity msg = " + msg);
        if (response.isSuccess == 1)
        {
            RestartUnity?.Invoke();
        }
    }

    public void InjectStartCallBack(Action act)
    {
        RestartUnity = act;
    }
    public void GetGameInfo()
    {
        AndroidInterface.Call(getGameInfo, "");
    }

    //Notifies the client to Show (RoleScene)
    public void GetGameInfoRole()
    {
        AndroidInterface.RoleCall(getGameInfo, "");
    }

    public void GetGameJson()
    {
        var gameAndroidInter = AndroidInterface.GetObject();
        if (gameAndroidInter == null)
        {
            AndroidInterface.RoleCall(getGameJson, "");
        }
        else
        {
            AndroidInterface.Call(getGameJson, "");
        }
    }

    public void GetGameJsonRole()
    {
        AndroidInterface.RoleCall(getGameJson, "");
    }

    public void FinishUnityHeadUrlSetup(string UnityImageBean)
    {
        AndroidInterface.RoleCall(finishUnityHeadUrlSetup, UnityImageBean);
    }

    public void Quit()
    {
        if (openAvoidMultClick)
        {
            openAvoidMultClick = false;
            StartCoroutine(QuitEditAndShowBlackPanel(null, SceneMode.GuestScene));
        }
    }

    public void Quit(string exitEditParams)
    {
        if (openAvoidMultClick)
        {
            openAvoidMultClick = false;
            StartCoroutine(QuitEditAndShowBlackPanel(exitEditParams, SceneMode.EditScene));
        }
    }

    private void AvoidMultClickActive()
    {
        if (!openAvoidMultClick)
        {
            openAvoidMultClick = true;
        }
    }

    public void QuitRole()
    {
        if (openAvoidMultClick)
        {
            openAvoidMultClick = false;
            StartCoroutine(QuitRoleAndShowBlackPanel());
        }
    }

    public void GetEngineEntryRole()
    {
        AndroidInterface.RoleCall(getEngineEntry, "");
    }

    public void GetEngineEntry()
    {
        AndroidInterface.Call(getEngineEntry, "");
    }

    public void OpenResourceStore()
    {
        AndroidInterface.Call(openResourceStore, "");
    }

    public void UploadResource(string ResourceInfo)
    {
        AndroidInterface.Call(uploadResource, ResourceInfo);
    }

    public void ShowKeyboard(string info)
    {
        AndroidInterface.Call(showKeyboard, info);
    }

    public void OpenSystemAlbum(string info)
    {
        AndroidInterface.Call(openSystemAlbum, info);
    }

    public void SkipNative(int skipType)
    {
        AndroidInterface.Call(skipNative, skipType.ToString());
    }

    public void UploadImage(string imageUrl)
    {
        AndroidInterface.Call(uploadImage, imageUrl);
    }

    public void OpenNativeShareDialog(string nativeShareParams)
    {
        AndroidInterface.Call(openNativeShareDialog, nativeShareParams);
    }

    public void OpenPersonalHomePage(string uid)
    {
        AndroidInterface.Call(openPersonalHomePage, uid);
    }

    public void SaveMediaToLocal(string data)
    {
        AndroidInterface.RoleCall(saveMediaToLocal, data);
    }

    public void OpenProfilePage(string albumParams)
    {
        AndroidInterface.Call(openProfilePage, albumParams);
    }

    public void OpenNativeDetailPage(string info, bool isRole = false)
    {
        if (isRole)
        {
            AndroidInterface.RoleCall(openNativeDetailPage, info);
            return;
        }
        AndroidInterface.Call(openNativeDetailPage, info);
    }
    public void GetProfilePath()
    {
        AndroidInterface.RoleCall(getProfilePath, "");
    }
    
    public void ExceptionReport(string str)
    {
        if (Screen.orientation == ScreenOrientation.Portrait)
        {
            AndroidInterface.RoleCall(exceptionReport, str);
        }
        else
        {
            AndroidInterface.Call(exceptionReport, str);
        }
    }

    public void RoleLogEvent(string data)
    {
        AndroidInterface.RoleCall(logEvent, data);
    }

    public void LogEvent(string data)
    {
        AndroidInterface.Call(logEvent, data);
    }

    public void LeaveRoomRedundancy(string data)
    {
        AndroidInterface.Call(leaveRoomRedundancy, data);
    }

    public void OpenClothStore()
    {
        AndroidInterface.RoleCall(openClothStore,"");
    }
    public void RefreshAvatarRedDot()
    {
        AndroidInterface.RoleCall(refreshAvatarRedDot, "");
    }

    public void OpenLandH5Page(string data)
    {
        AndroidInterface.Call(openLandH5Page, data);
    }

    public void UploadToAws(string data, bool isRole = false)
    {
        if (isRole)
        {
            AndroidInterface.RoleCall(uploadToAws, data);
            return;
        }
        AndroidInterface.Call(uploadToAws, data);
    }

    public void UploadToAlbum(string data)
    {
        AndroidInterface.Call(uploadToAlbum,data);
    }

    public void NotifyPercentFull()
    {
        AndroidInterface.Call(notifyPercentFull, "");
    }
    
    public void OpenUgcClothPage()
    {
        AndroidInterface.Call(openUgcClothPage, "");
    }
    public void OpenUgcResPage()
    {
        AndroidInterface.Call(openUgcResPage, "");
    }
    public void OpenStorePage(string type)
    {
        AndroidInterface.Call(openStorePage, type);
    }
    public void OpenDcResPage()
    {
        AndroidInterface.Call(openDcResPage, "");
    }

    public void OpenPromotePage(string data)
    {
        AndroidInterface.Call(openPromotePage, data);
    }

    public void RequestNativePermission(string data)
    {
        AndroidInterface.Call(requestNativePermission, data);
    }
    public void OpenNativeSettingsPage()
    {
        AndroidInterface.Call(openNativeSettingsPage,"");
    }
    
    public void NotifyRefreshPlayerInfo()
    {
        AndroidInterface.Call(refreshPlayerInfo, "");
    }

    public void NotifyRefreshPlayerInfoRoleCall()
    {
        AndroidInterface.RoleCall(refreshPlayerInfo, "");
    }

    public void NotifyLoadingDialog(string data)
    {
        AndroidInterface.Call(loadingDialog, data);
    }
    public void CloseSession(string data)
    {
        AndroidInterface.Call(closeSession, data);
    }
    public void GetDCResSoldNum(string data)
    {
        AndroidInterface.Call(getDCResSoldNum, data);
    }
    public void NewUserRegistrationCompleted(string roleUpLoadBody)
    {
        if (openAvoidMultClick)
        {
            openAvoidMultClick = false;
            StartCoroutine(QuitNewUse(roleUpLoadBody));
        }
    }
    
    private IEnumerator QuitNewUse(string msg)
    {
        SetBlackPanelByPreChange();
        yield return new WaitForSeconds(0.1f);
        openAvoidMultClick = true;
        ChangeLoddyAndDispose();
        ExitGame(HVMode.Vertical,newUserRegistrationCompleted,msg);
    }

    
    
    public void GetPublicAvatarUserInfo()
    {
        AndroidInterface.RoleCall(getPublicAvatarUserInfo, "");
    }
    public void OpenAvatarItemDetail(string data)
    {
        AndroidInterface.RoleCall(openAvatarItemDetail, data);
    }
    public void OpenUnityTryOnPage(string data)
    {
        AndroidInterface.RoleCall(openUnityTryOnPage, data);
    }
    public void OpenUnityWearPage(string data)
    {
        AndroidInterface.RoleCall(openUnityWearPage, data);
    }
    public void OpenCheckInRewardsPage()
    {
        AndroidInterface.RoleCall(openCheckInRewardsPage, "");
    }
    public void GetDowntownJson()
    {
        AndroidInterface.Call(getDowntownJson, "");
    }

#endif

#if UNITY_IPHONE
    public void GetGameInfo()
    {
        IOSInterface.sendMessageToClient(getGameInfo, null);
    }
    //Notifies the client to Show (MapScene)
    public void GetGameInfoRole()
    {
        IOSInterface.sendMessageToClient(getGameInfo, null);
    }

    public void GetGameJson()
    {
        IOSInterface.sendMessageToClient(getGameJson, null);
    }

    public void Quit()
    {
        StartCoroutine(QuitEditAndShowBlackPanel(null,SceneMode.GuestScene));
    }

    public void Quit(string exitEditParams)
    {
        StartCoroutine(QuitEditAndShowBlackPanel(exitEditParams,SceneMode.EditScene));
    }

    public void QuitRole()
    {
        StartCoroutine(QuitRoleAndShowBlackPanel());
    }

    public void GetGameJsonRole() {
        IOSInterface.sendMessageToClient(getGameJson, null);
    }

    public void FinishUnityHeadUrlSetup(string unityImageBean)
    {
        IOSInterface.sendMessageToClient(finishUnityHeadUrlSetup, unityImageBean);
    }

    private void SendMessageToClient(string key, string msg)
    {
         IOSInterface.sendMessageToClient(key,msg);
    }
    public void GetEngineEntry() {
        SendMessageToClient(getEngineEntry, null);
    }

    public void OpenResourceStore()
    {
        IOSInterface.sendMessageToClient(openResourceStore, "");
    }

    public void UploadResource(string ResourceInfo)
    {
        IOSInterface.sendMessageToClient(uploadResource, ResourceInfo);
    }

     public void ShowKeyboard(string info)
    {
        SendMessageToClient(showKeyboard, info);
    }

    public void OpenSystemAlbum(string info)
    {
        SendMessageToClient(openSystemAlbum, info);
    }

    public void SkipNative(int skipType)
    {
        SendMessageToClient(skipNative, skipType.ToString());
    }

    public void UploadImage(string imageUrl)
    {
        SendMessageToClient(uploadImage, imageUrl);
    }

    public void OpenNativeShareDialog(string nativeShareParams)
    {
        SendMessageToClient(openNativeShareDialog, nativeShareParams);
    }

    public void OpenPersonalHomePage(string uid)
    {
        SendMessageToClient(openPersonalHomePage, uid);
    }

    public void SaveMediaToLocal(string data)
    {
        SendMessageToClient(saveMediaToLocal, data);
    }

    public void OpenProfilePage(string albumParams)
    {
        SendMessageToClient(openProfilePage, albumParams);
    }
    public void OpenNativeSettingsPage()
    {
       SendMessageToClient(openNativeSettingsPage,"");
    }
    
    public void NotifyRefreshPlayerInfo()
    {
        SendMessageToClient(refreshPlayerInfo,"");
    }

    public void OpenNativeDetailPage(string info, bool isRole = false)
    {
        SendMessageToClient(openNativeDetailPage, info);
	}
    public void OpenUgcResPage()
    {
        SendMessageToClient(openUgcResPage, "");
    }
    public void OpenStorePage(string type)
    {
        SendMessageToClient(openStorePage, type);
    }

    public void OpenDcResPage()
    {
        SendMessageToClient(openDcResPage, "");
    }

    public void OpenPromotePage(string data)
    {
        SendMessageToClient(openPromotePage, data);
    }

    public void GetProfilePath()
    {
        SendMessageToClient(getProfilePath, "");
    }
    public void ExceptionReport(string str)
    {
        SendMessageToClient(exceptionReport, str);
    }

    public void LogEvent(string data)
    {
        SendMessageToClient(logEvent,data);
    }

    public void RoleLogEvent(string data)
    {
        SendMessageToClient(logEvent, data);
    }

    public void LeaveRoomRedundancy(string data)
    {
        SendMessageToClient(leaveRoomRedundancy, data);
    }

    public void OpenClothStore()
    {
        SendMessageToClient(openClothStore,"");
    }
    
    public void OpenLandH5Page(string data)
    {
        SendMessageToClient(openLandH5Page, data);
    }
    public void RequestNativePermission(string data)
    {
        SendMessageToClient(requestNativePermission, data);
    }
    public void RefreshAvatarRedDot()
    {
        SendMessageToClient(refreshAvatarRedDot, "");
    }

    public void UploadToAws(string data, bool isRole = false)
    {
        SendMessageToClient(uploadToAws, data);
    }

    public void UploadToAlbum(string data)
    {
        SendMessageToClient(uploadToAlbum,data);
    }

    public void NotifyPercentFull()
    {
        SendMessageToClient(notifyPercentFull, "");
    }
    

    public void OpenUgcClothPage()
    {
        SendMessageToClient(openUgcClothPage, "");
    }

    public void NotifyRefreshPlayerInfoRoleCall()
    {
        SendMessageToClient(refreshPlayerInfo, "");
    }

    public void NotifyLoadingDialog(string data)
    {
        SendMessageToClient(loadingDialog, data);
    }
    public void CloseSession(string data)
    {
        SendMessageToClient(closeSession,data);
    }
    public void GetDCResSoldNum(string data)
    {
        SendMessageToClient(getDCResSoldNum, data);
    }
    public void NewUserRegistrationCompleted(string roleUpLoadBody)
    {
        SendMessageToClient(newUserRegistrationCompleted, roleUpLoadBody);
    }
    public void GetPublicAvatarUserInfo()
    {
        SendMessageToClient(getPublicAvatarUserInfo, "");
    }
    public void OpenAvatarItemDetail(string data)
    {
        SendMessageToClient(openAvatarItemDetail, data);
    }
    public void OpenUnityTryOnPage(string data)
    {
        SendMessageToClient(openUnityTryOnPage, data);
    }
    public void OpenUnityWearPage(string data)
    {
        SendMessageToClient(openUnityWearPage, data);
    }
    public void OpenCheckInRewardsPage()
    {
        SendMessageToClient(openCheckInRewardsPage, "");
    }
    public void GetDowntownJson()
    {
        SendMessageToClient(getDowntownJson, "");
    }
#endif

    public void ReceiveMessageFromClient(string msg)
    {
        ClientResponse response = JsonUtility.FromJson<ClientResponse>(msg);
        LoggerUtils.Log("ReceiveMessageFromClient msg = "+ msg);
        if (response.isSuccess == 1)
        {
            if (onClientRespose.ContainsKey(response.funcName))
            {
                onClientRespose[response.funcName]?.Invoke(response.data);
            }
        }
        else
        {
            if (onClientFail.ContainsKey(response.funcName))
            {
                onClientFail[response.funcName]?.Invoke(response.data);
            }
        }
        ReceiveAwsMessageFromClient(response);
    }

    private void ReceiveAwsMessageFromClient(ClientResponse response)
    {
        if (response.funcName != uploadToAws)
        {
            return;
        }

        LoggerUtils.Log("OnAwsUploadCallback --> resp == " + response.data);
        AWSResponse awsResp = JsonConvert.DeserializeObject<AWSResponse>(response.data);
        if (response.isSuccess == 1)
        {
            if (onClientRespose.ContainsKey(awsResp.filePath))
            {
                onClientRespose[awsResp.filePath]?.Invoke(response.data);
            }
        }
        else
        {
            if (onClientFail.ContainsKey(awsResp.filePath))
            {
                onClientFail[awsResp.filePath]?.Invoke(response.data);
            }
        }
    }

    /*** callbacks ***/
    public void GetGameJsonReceive(string gameJson)
    {
        onGetGameJson.Invoke(gameJson);
    }
    
    private void OnDestroy()
    {
        instance = null;
    }

    IEnumerator QuitRoleAndShowBlackPanel()
    {
        SetBlackPanelByPreChange();
        yield return new WaitForSeconds(0.1f);
        ChangeLoddyAndDispose();
        yield return new WaitForSeconds(0.5f);
        ExitGame(HVMode.Vertical,exitGame);
    }
    
    IEnumerator QuitEditAndShowBlackPanel(string exitEditParams,SceneMode mode)
    {
        AkSoundEngine.StopAll();
        if (mode == SceneMode.GuestScene)
        {
            if (SceneBuilder.Inst.BgBehaviour!=null)
            {
                SceneBuilder.Inst.BgBehaviour.Stop();
                SceneBuilder.Inst.BgBehaviour.StopEnr();
            }
        }
        SetBlackPanelByPreChange();
        yield return new WaitForSeconds(0.1f);
        ChangeLoddyAndDispose();
        yield return new WaitForSeconds(0.5f);
        ExitGame(HVMode.Horizontal,"exitGame",exitEditParams);
    }
    //避免看到人物衣服释放现象
    private void SetBlackPanelByPreChange()
    {
        BlackPanel.Show();
        BlackPanel.Instance.PlayBlackBg();
    }

    private void ChangeLoddyAndDispose()
    {
        SceneManager.LoadScene("Lobby");
        DontDestroyUtils.Dispose();
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    private void ExitGame(HVMode mode,string funName,string exitEditParams = null)
    {
        GlobalFieldController.isGameProcessing = false;
#if UNITY_ANDROID
        openAvoidMultClick = true;
        HttpUtils.Release();
        if (mode == HVMode.Horizontal)
        {
            AndroidInterface.Call(funName, exitEditParams ?? "");
        }
        else
        {
            AndroidInterface.RoleCall(funName, exitEditParams ?? "");
        }
        onClientRespose.Clear();
        onClientFail.Clear();
        AndroidInterface.Inst.Release();
        CInstanceManager.Release();
#endif
#if UNITY_IPHONE
        IOSInterface.sendMessageToClient("exitGame", exitEditParams);
#endif
    }
    
    public void LogEventByEventName(string eventName,string code = null,int retry = 0)
    {
        var LogEventParam = GetLogEventParam(code, retry);
        LogEventParam.eventName = eventName;
#if !UNITY_EDITOR
        string paramStr = JsonConvert.SerializeObject(LogEventParam);
        switch (GameManager.Inst.nativeType)
        {
            case NATIVE_TYPE.MAPCALL:
                LogEvent(paramStr);
                break;
            case NATIVE_TYPE.ROLECALL:
                RoleLogEvent(paramStr);
                break;
        }
        LoggerUtils.Log("LogEvent paramStr = " + paramStr);
#endif
    }

    public void LogRoomChatEventByEventName(string eventName, string requestSeq, int msgType, string code = null)
    {
        var LogEventParam = GetLogEventParam(code);

        var roomchatEventParams = new RoomChatEventParam
        {
            requestSeq = requestSeq,
            msgType = msgType,

            scene = LogEventParam.parameters.scene,
            subType= LogEventParam.parameters.subType,
            roomMode = LogEventParam.parameters.roomMode,
            roomCode= LogEventParam.parameters.roomCode,
            mapId= LogEventParam.parameters.mapId,
            seq= LogEventParam.parameters.seq,
            code = LogEventParam.parameters.code,
            sessionId = LogEventParam.parameters.sessionId,
            retry = LogEventParam.parameters.retry
        };

        var roomChatEventParam = new RoomChatLogEventParam
        {
            parameters = roomchatEventParams,
            eventName = eventName,
        };

#if !UNITY_EDITOR
        string paramStr = JsonConvert.SerializeObject(roomChatEventParam);
        switch (GameManager.Inst.nativeType)
        {
            case NATIVE_TYPE.MAPCALL:
                LogEvent(paramStr);
                break;
            case NATIVE_TYPE.ROLECALL:
                RoleLogEvent(paramStr);
                break;
        }
        LoggerUtils.Log("LogEvent paramStr = " + paramStr);
#else
        LoggerUtils.Log("RoomChatLog:" + JsonConvert.SerializeObject(roomChatEventParam));
#endif
    }
    
    public void LogFrameEventByEventName(string eventName, string frameCount, string code = null)
    {
        var LogEventParam = GetLogEventParam(code);

        var frameCountEventParam = new FrameCountEventParam
        {
            frameCount = frameCount,

            scene = LogEventParam.parameters.scene,
            subType= LogEventParam.parameters.subType,
            roomMode = LogEventParam.parameters.roomMode,
            roomCode= LogEventParam.parameters.roomCode,
            mapId= LogEventParam.parameters.mapId,
            seq= LogEventParam.parameters.seq,
            code = LogEventParam.parameters.code,
            sessionId = LogEventParam.parameters.sessionId,
            retry = LogEventParam.parameters.retry
        };

        var roomChatEventParam = new FrameCountLogEventParam
        {
            parameters = frameCountEventParam,
            eventName = eventName,
        };

#if !UNITY_EDITOR
        string paramStr = JsonConvert.SerializeObject(roomChatEventParam);
        switch (GameManager.Inst.nativeType)
        {
            case NATIVE_TYPE.MAPCALL:
                LogEvent(paramStr);
                break;
            case NATIVE_TYPE.ROLECALL:
                RoleLogEvent(paramStr);
                break;
        }
        LoggerUtils.Log("LogEvent paramStr = " + paramStr);
#else
        LoggerUtils.Log("LogFrameEvent:" + JsonConvert.SerializeObject(roomChatEventParam));
#endif
    }

    public void LogPingTimeEventByEventName(string eventName, string region, string maxPing, string averagePing, string code = null)
    {
        var LogEventParam = GetLogEventParam(code);

        var pingTimeEventParam = new PingTimeEventParam
        {
            region = region,
            maxPing = maxPing,
            averagePing = averagePing,

            scene = LogEventParam.parameters.scene,
            subType = LogEventParam.parameters.subType,
            roomMode = LogEventParam.parameters.roomMode,
            roomCode = LogEventParam.parameters.roomCode,
            mapId = LogEventParam.parameters.mapId,
            seq = LogEventParam.parameters.seq,
            code = LogEventParam.parameters.code,
            sessionId = LogEventParam.parameters.sessionId,
            retry = LogEventParam.parameters.retry
        };

        var pingTimeLogEventParam = new PingTimeLogEventParam
        {
            parameters = pingTimeEventParam,
            eventName = eventName,
        };

#if !UNITY_EDITOR
        string paramStr = JsonConvert.SerializeObject(pingTimeLogEventParam);
        switch (GameManager.Inst.nativeType)
        {
            case NATIVE_TYPE.MAPCALL:
                LogEvent(paramStr);
                break;
            case NATIVE_TYPE.ROLECALL:
                RoleLogEvent(paramStr);
                break;
        }
        LoggerUtils.Log("LogEvent paramStr = " + paramStr);
#else
        LoggerUtils.Log("LogPingTime:" + JsonConvert.SerializeObject(pingTimeLogEventParam));
#endif
    }

    private LogEventParam GetLogEventParam(string code = null, int retry = 0)
    {
        LogEventParam LogEventParam = new LogEventParam();
        if (code != null)
        {
            LogEventParam.parameters.code = code;
        }
        if (GameManager.Inst.ugcUntiyMapDataInfo != null)
        {
            if (!string.IsNullOrEmpty(GameManager.Inst.ugcUntiyMapDataInfo.mapId))
            {
                LogEventParam.parameters.mapId = GameManager.Inst.ugcUntiyMapDataInfo.mapId;
            }
        }
        if (GameManager.Inst.unityConfigInfo != null)
        {
            if (!string.IsNullOrEmpty(GameManager.Inst.unityConfigInfo.seq))
            {
                LogEventParam.parameters.seq = GameManager.Inst.unityConfigInfo.seq;
            }
        }
        if (GameManager.Inst.engineEntry != null)
        {
            LogEventParam.parameters.scene = GameManager.Inst.engineEntry.sceneType;
            LogEventParam.parameters.subType = GameManager.Inst.engineEntry.subType;
        }
        if (GameManager.Inst.onLineDataInfo != null)
        {
            if (!string.IsNullOrEmpty(GameManager.Inst.onLineDataInfo.roomCode))
            {
                LogEventParam.parameters.roomCode = GameManager.Inst.onLineDataInfo.roomCode;
            }
            LogEventParam.parameters.roomMode = GameManager.Inst.onLineDataInfo.roomMode;
        }
        if (ClientManager.Inst != null)
        {
            if (!string.IsNullOrEmpty(ClientManager.Inst.SessionId))
            {
                string sessionId = ClientManager.Inst.SessionId.Replace("arn:aws:gamelift:us-west-1::gamesession/", "");
                LogEventParam.parameters.sessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(ClientManager.Inst.roomCode))
            {
                LogEventParam.parameters.roomCode = ClientManager.Inst.roomCode;
            }
        }

        LogEventParam.parameters.retry = retry;
        return LogEventParam;
    }


    public void LogEvent(string eventName, LogEventBaseParam param)
    {
        var eventData = new LogEventInfo()
        {
            eventName = eventName,
            parameters = param,
        };
        string paramStr = JsonConvert.SerializeObject(eventData);
        LoggerUtils.Log("paramStr:" + paramStr);
#if !UNITY_EDITOR
        switch (GameManager.Inst.nativeType)
        {
            case NATIVE_TYPE.MAPCALL:
                LogEvent(paramStr);
                break;
            case NATIVE_TYPE.ROLECALL:
                RoleLogEvent(paramStr);
                break;
        }
#endif

    }

    public void LogCustomEventData(string eventName, int platform, Dictionary<string, object> param)
    {
        CustomEventParam customEvent = new CustomEventParam()
        {
            platform = platform,
            eventName = eventName,
            parameters = param
        };
        string paramStr = JsonConvert.SerializeObject(customEvent);
        LoggerUtils.Log("LogCustomEventData paramStr = " + paramStr);
#if !UNITY_EDITOR
        switch (GameManager.Inst.nativeType)
        {
            case NATIVE_TYPE.MAPCALL:
                LogEvent(paramStr);
                break;
            case NATIVE_TYPE.ROLECALL:
                RoleLogEvent(paramStr);
                break;
        }
#endif
    }

    public void MobileSendMsgBridge(string funcName, string data)
    {
#if UNITY_ANDROID
        if (Screen.orientation == ScreenOrientation.Portrait)
        {
            AndroidInterface.RoleCall(funcName, data);
        }
        else
        {
            AndroidInterface.Call(funcName, data);
        }
#elif UNITY_IPHONE
        IOSInterface.sendMessageToClient(funcName, data);
#endif
    }
    
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GRTools.Localization;
using Newtonsoft.Json;
using RTG;
using SavingData;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditModePanel<T> : BasePanel<T> where T:BasePanel<T>
{
    public Button PlayBtn;
    public Button GlobalBtn;
    public Button PackBtn;
    public Button SaveBtn;
    public Button ResStoreBtn;
    public Button MenuBtn;
    public Button MenuSaveBtn;
    public Button MenuPhotoBtn;
    public Button MenuCloseBtn;
    public Button EmptyBtn;
    public Button UndoBtn;
    public Button RedoBtn;
    public GameObject MaskGo;
    public GameObject MenuPanel;
    public Animator saveAnim;
    public Animator mainSaveAnim;
    private GameObject saveIcon;
    private GameObject mainSavaIcon;
    public Action OnPlay;
    public Text AppVersion;
    public GizmoController gController;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        PlayBtn.onClick.AddListener(OnPlayClick);
        ResStoreBtn.onClick.AddListener(OnResStoreClick);
        PackBtn.onClick.AddListener(OnPackClick);
        MenuBtn.onClick.AddListener(OnMenuClick);
        GlobalBtn.onClick.AddListener(OnGlobalClick);
        SaveBtn.onClick.AddListener(OnSaveClick);
        MenuSaveBtn.onClick.AddListener(OnMenuSaveClick);
        MenuPhotoBtn.onClick.AddListener(OnPhotoClick);
        MenuCloseBtn.onClick.AddListener(OnCloseClick);
        EmptyBtn.onClick.AddListener(OnEmptyClick);
        UndoBtn.onClick.AddListener(OnUndoClick);
        RedoBtn.onClick.AddListener(OnRedoClick);
        saveIcon = SaveBtn.transform.GetChild(0).gameObject;
        mainSavaIcon = MenuSaveBtn.transform.GetChild(0).gameObject;
        MessageHelper.AddListener(MessageName.UpdateUndoView,UpdateUndoBtnView);
#if !UNITY_EDITOR
        LocalizationConManager.Inst.SetLocalizedContent(AppVersion, "Version  {0}", GameManager.Inst.unityConfigInfo.appVersion);
#endif
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        MenuPanel.SetActive(false);
        MaskGo.SetActive(false);
        UpdateUndoBtnView();
    }


    public void SetGizmoController(GizmoController gCtr)
    {
        gController = gCtr;
    }

    public void UpdateUndoBtnView()
    {
        LoggerUtils.Log("UpdateUndoBtnView");
        bool hasUndo = (UndoRecordPool.Inst.GetUndoCount() > 0);
        UndoBtn.transform.GetChild(0).gameObject.SetActive(hasUndo);
        UndoBtn.transform.GetChild(1).gameObject.SetActive(!hasUndo);

        bool hasRedo = (UndoRecordPool.Inst.GetRedoCount() > 0);
        RedoBtn.transform.GetChild(0).gameObject.SetActive(hasRedo);
        RedoBtn.transform.GetChild(1).gameObject.SetActive(!hasRedo);
    }

    private void OnPlayClick()
    {
        ReferManager.Inst.OnReferPlay();
        OnPlay?.Invoke();
    }

    private void OnGlobalClick()
    {
        UIManager.Inst.uiCanvas.gameObject.SetActive(false);
        GlobalEyePanel.Show(true);
    }

    private void OnPackClick()
    {
        gController.DisableGizmo();
        MovePathManager.Inst.CloseAndSave();
        BasePrimitivePanel.DisSelect();
        UIManager.Inst.CloseAllDialog();
        PackPanel.Show();
        PackPanel.Instance.SetReturnClick(() => Show());
        if (ReferManager.Inst.isRefer)
        {
            ReferPanel.Instance.playerCom.transform.gameObject.SetActive(false);
        }
    }

    private void OnSaveClick()
    {
        InputReceiver.locked = true;
        MobileInterface.Instance.LogEventByEventName(LogEventData.unity_clickSave);

        saveIcon.SetActive(false);
        saveAnim.gameObject.SetActive(true);
        saveAnim.Play("SaveAnimtion", 0, 0);

        SaveMapAndCover();


    }

    private void OnMenuSaveClick()
    {
        MobileInterface.Instance.LogEventByEventName(LogEventData.unity_clickSave);

        mainSavaIcon.SetActive(false);
        mainSaveAnim.gameObject.SetActive(true);
        mainSaveAnim.Play("SaveAnimtion",0,0);
        SaveMapAndCover();
    }

    private void OnResStoreClick()
    {
#if UNITY_EDITOR
        UIManager.Inst.CloseAllDialog();
        ResStorePanel.Show();
#else
        EditModeController.ClearBehav?.Invoke();
        EditModeController.curPos = Vector3.zero;
        ResBagManager.Inst.OpenResPage(null);
#endif

        //if (ReferManager.Inst.isRefer)
        //{
        //    ReferPanel.Instance.EnterReferMode();
        //    ReferManager.Inst.isHafeRefer = true;
        //}
    }
   
    
    private void SaveMapAndCover()
    {
        MaskGo.gameObject.SetActive(true);
        this.transform.SetAsLastSibling();
#if UNITY_EDITOR
        var content = SceneParser.Inst.StageToMapJson();
        File.WriteAllText(Application.streamingAssetsPath +"/123.json", content);
#endif
        SaveMapCover(SaveMapInfoSuccess, OnMapInfoFail);

    }

    public virtual void OnSaveAndQuit()
    {
        ComfirmPanel.SetAnim(true);
        this.transform.SetAsLastSibling();
        MaskGo.gameObject.SetActive(true);
        CancelInvoke("CanRetrySave");
        Invoke("CanRetrySave", 90);
        SaveMapCover(SaveMapInfoSuccessAndQuit, OnSaveCoverFailAndQuit);
    }
    


    private void SaveMapInfoSuccess(string content)
    {
        TipPanel.ShowToast("Saved successfully:D");
        CloseSaveAnim();
        InputReceiver.locked = false;
        ReferManager.Inst.OnSaveChangeReferState();
    }

    private void OnMapInfoFail(string content)
    {
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        LoggerUtils.LogError("Save Map Fail");
        CloseSaveAnim();
        InputReceiver.locked = false;
        ReferManager.Inst.OnSaveChangeReferState();
    }


    private void SaveMapInfoSuccessAndQuit(string content)
    {
        TipPanel.ShowToast("Saved successfully:D");
        ComfirmPanel.SetAnim(false);
        ComfirmPanel.Hide();
        gController.DisableGizmo();

        ExitEditParams exitEditParams = new ExitEditParams()
        {
            mapId = GameManager.Inst.ugcUntiyMapDataInfo.mapId,
            draftPath = GameManager.Inst.ugcUntiyMapDataInfo.draftPath,
        };
        string quitPara = JsonConvert.SerializeObject(exitEditParams);
        LoggerUtils.Log("exitEditParams == " + quitPara);
        MobileInterface.Instance.Quit(quitPara);
    }

    private void OnSaveCoverFailAndQuit(string err)
    {
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        LoggerUtils.Log("Save Fail");
        ComfirmPanel.SetAnim(false);
        MaskGo.gameObject.SetActive(false);
        ReferManager.Inst.OnSaveChangeReferState();
    }

    protected void CloseSaveAnim()
    {
        MaskGo.gameObject.SetActive(false);
        MenuPanel.SetActive(false);
        saveIcon.SetActive(true);
        mainSavaIcon.SetActive(true);
        saveAnim.gameObject.SetActive(false);
        mainSaveAnim.gameObject.SetActive(false);
    }


    private void OnPhotoClick()
    {
        ReferManager.Inst.OnReferPlay();
        switch (GlobalFieldController.CurSceneType)
        {
            case SCENE_TYPE.MAP_SCENE:
            case SCENE_TYPE.MYSPACE_SCENE:
                OnPhotoClickInMapScene();
                break;
            case SCENE_TYPE.ResMAP_SCENE:
                OnPhotoClickInResScene();
                break;
        }
    }

    private void OnPhotoClickInMapScene()
    {
        MenuPanel.SetActive(false);
        gController.DisableGizmo();
        MovePathManager.Inst.CloseAndSave();
        BasePrimitivePanel.DisSelect();
        UIManager.Inst.CloseAllDialog();
        RTGApp.Get.enabled = false;
        GlobalFieldController.isScreenShoting = true;
        CoverPanel.Show();
        MessageHelper.Broadcast(MessageName.SaveCoverStateChange);
        CoverPanel.Instance.SetReturnClick(() => {
            Show();
            RTGApp.Get.enabled = true;
            GlobalFieldController.isScreenShoting = false;
            MessageHelper.Broadcast(MessageName.SaveCoverStateChange);
        });
    }

    private void OnPhotoClickInResScene()
    {
        MenuPanel.SetActive(false);
        gController.DisableGizmo();
        BasePrimitivePanel.DisSelect();
        UIManager.Inst.CloseAllDialog();
        RTGApp.Get.enabled = false;
        GlobalFieldController.isScreenShoting = true;

        Camera.main.RemoveLayer(LayerMask.NameToLayer("Terrain"));
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Color oriBgColor = Camera.main.backgroundColor;
        Camera.main.backgroundColor = new Color(0, 0, 0, 0);

        ResSceneCoverPanel.Show();
        ResSceneCoverPanel.Instance.SetReturnClick(() => {
            Show();
            RTGApp.Get.enabled = true;
            GlobalFieldController.isScreenShoting = false;
            Camera.main.AddLayer(LayerMask.NameToLayer("Terrain"));
            Camera.main.clearFlags = CameraClearFlags.Skybox;
            Camera.main.backgroundColor = oriBgColor;
        });
    }

    protected virtual void OnCloseClick()
    {
        MenuPanel.SetActive(false);
        string content = GlobalFieldController.CurSceneType == SCENE_TYPE.MYSPACE_SCENE ? "space" : "experience";
        content = string.Format("Do you want to save this {0}?", content);
        ComfirmPanel.Show();
        ComfirmPanel.Instance.SetText(content);
        ComfirmPanel.Instance.OnSaveClick = OnSaveAndQuit;
        ComfirmPanel.Instance.OnDontClick = OnQuit;
    }

    public virtual void OnQuit()
    {
        gController.DisableGizmo();
        ExitEditParams exitEditParams = new ExitEditParams()
        {
            mapId = GameManager.Inst.ugcUntiyMapDataInfo.mapId,
            draftPath = GameManager.Inst.ugcUntiyMapDataInfo.draftPath,
        };
        string quitPara = JsonConvert.SerializeObject(exitEditParams);
        LoggerUtils.Log("exitEditParams == " + quitPara);
        MobileInterface.Instance.Quit(quitPara);
        //StartCoroutine("OnQuitGame");
    }



    private void CanRetrySave()
    {
        MaskGo.gameObject.SetActive(false);
    }

    //IEnumerator OnQuitGame()
    //{
    //    gController.DisableGizmo();
    //    BlackPanel.Show();
    //    yield return new WaitForSeconds(0.5f);
    //    MobileInterface.Instance.Quit();
    //}

    private void OnEmptyClick()
    {
        MenuPanel.SetActive(false);
    }

    private void OnMenuClick()
    {
        MenuPanel.SetActive(!MenuPanel.activeSelf);
        transform.SetAsLastSibling();
    }

    private void OnUndoClick()
    {
        UndoRedoManager.Inst.Undo();
        UpdateUndoBtnView();
    }

    private void OnRedoClick()
    {
        UndoRedoManager.Inst.Redo();
        UpdateUndoBtnView();
    }

    protected virtual void SaveMapCover(Action<string> success, Action<string> fail)
    {
        if (ReferPanel.Instance && ReferManager.Inst.isRefer)
        {
            ReferPanel.Instance.OnSaveMapCloseRefer();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        MessageHelper.RemoveListener(MessageName.UpdateUndoView,UpdateUndoBtnView);
    }
}

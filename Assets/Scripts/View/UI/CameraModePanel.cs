using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using RedDot;
using OperationRedDotSystem;

/// <summary>
/// Author:Shaocheng
/// Description:相机模式UI
/// Date: 2022-6-10 10:46:10
/// </summary>
public class CameraModePanel : BasePanel<CameraModePanel>
{
    public Button CloseCameraModeBtn;
    public Button CloseTipBtn;
    public Button TakePhotoBtn;
    public Button CloseAreaBtn;
    public Button EnterSelfieBtn, ExitSelfieBtn, CloseSelfieTipBtn, CloseSelfieAreaBtn;

    public GameObject TipUiGo, SelfieTip;
    public GameObject selfieBtnPanel;
    public VNode mVNode;

    public const string CAMERA_MODE_TIP_KEY = "CAMERA_MODE_TIP_KEY";
    public const string SELFIE_MODE_TIP_KEY = "SELFIE_MODE_TIP_KEY";

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        CloseCameraModeBtn.onClick.AddListener(OnCloseCameraModeBtnClick);
        TakePhotoBtn.onClick.AddListener(OnTakePhotoBtnClick);
        CloseTipBtn.onClick.AddListener(OnCloseTipBtnClick);
        CloseAreaBtn.onClick.AddListener(OnCloseAreaClick);
        EnterSelfieBtn.onClick.AddListener(OnEnterSelfieClick);
        ExitSelfieBtn.onClick.AddListener(OnExitSelfieClick);
        CloseSelfieTipBtn.onClick.AddListener(OnCloseSelfieTip);
        CloseSelfieAreaBtn.onClick.AddListener(OnCloseSelfieTip);
        InitTipUi();
        InitSelfieUI();
        if (PlayModePanel.Instance.operationRedDotManager.IsInited)
        {
            RedDotInitedCallBack(true);
        }
        else
        {
            PlayModePanel.Instance.operationRedDotManager.AddListener(RedDotInitedCallBack);
        }
    }

    private void InitSelfieUI()
    {
        SelfieTip.SetActive(false);
        ExitSelfieBtn.gameObject.SetActive(false);
    }

    private void InitTipUi()
    {
        // 0-显示 1-隐藏
        TipUiGo.SetActive(PlayerPrefs.GetInt(CAMERA_MODE_TIP_KEY, 0) == 0);
    }


    private void OnCloseCameraModeBtnClick()
    {
        //冻结中不能退出自拍模式
        if (StateManager.IsInSelfieMode && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.SelfieMode))
        {
            TipPanel.ShowToast("Selfie mode cannot be quit while freezing.");
            return;
        }
        CameraModeManager.Inst.EnterMode(CameraModeEnum.NormalGuestCamera);
        PlayerBaseControl.Inst.isInSelfieMode = false;
    }

    private void OnTakePhotoBtnClick()
    {
        PhotoType photoType = StateManager.IsInSelfieMode ? PhotoType.SelfieMode : PhotoType.CameraMode;
        DataLogUtils.LogTakePhoto("shot_start", (int)photoType);
        StartCoroutine(ShotAnimation());
        AudioController.Inst.PlayShotAudio();

        var bytes = ScreenShotUtils.ScreenShot(GameManager.Inst.MainCamera, new Rect(0, 0, Screen.width, Screen.height), true);
        if (bytes.Length == 0)
        {
            OnFail();
            return;
        }

        string fileName = DataUtils.SaveImg(bytes);
        PhotoBusData photoBusData = new PhotoBusData();
        if (GlobalFieldController.IsDowntownEnter)
        {
            photoBusData = new PhotoBusData()
            {
                downtownId = GameManager.Inst.gameMapInfo.mapId,
                downtownName = GameManager.Inst.gameMapInfo.mapName,
                downtownDesc = GameManager.Inst.gameMapInfo.mapDesc,
                downtownCover = GameManager.Inst.gameMapInfo.mapCover
            };
        }
        else
        {
            photoBusData = new PhotoBusData() { ugcId = GameManager.Inst.gameMapInfo.mapId };
        }
        string busData = JsonConvert.SerializeObject(photoBusData);
        LoggerUtils.Log("OnShotBtnClick -- PhotoBusData : " + busData);
        AWSUtill.UpLoadToAlbum(fileName, busData, OnSuccess, OnFail, (int)photoType);
    }

    private IEnumerator ShotAnimation()
    {
        BlackPanel.Show();
        RawImage image = BlackPanel.Instance.GetComponent<RawImage>();
        Image blackImage = BlackPanel.Instance.BlackImage;
        GameObject black = BlackPanel.Instance.Black;

        image.CrossFadeAlpha(0, 0, false);
        image.color = new Color(1, 1, 1, 1);
        blackImage.color = new Color(1, 1, 1, 0);
        black.SetActive(false);

        blackImage.DOFade(1, 0.4f).SetEase(Ease.InExpo).onComplete += () => 
        { 
            blackImage.DOFade(0, 0.4f).SetEase(Ease.OutExpo).onComplete += () =>
            {
                StartCoroutine(WaitShotStop(image));
            };
        };

        yield return new WaitForEndOfFrame();
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();
        image.texture = screenShot;
        image.CrossFadeAlpha(1, 0, false);
    }

    IEnumerator WaitShotStop(RawImage image)
    {
        yield return new WaitForSeconds(0.5f);
        Object.Destroy(image.texture);
        image.texture = null;
        image.CrossFadeAlpha(0, 0, false);
        BlackPanel.Hide();
    }

    private void OnSuccess(string content = "")
    {
        TipPanel.ShowToast("Added photo to album!");
    }

    private void OnFail(string content = "")
    {
        PhotoType photoType = StateManager.IsInSelfieMode ? PhotoType.SelfieMode : PhotoType.CameraMode;
        DataLogUtils.LogTakePhoto("shot_fail", (int)photoType);
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }

    private void OnCloseTipBtnClick()
    {
        TipUiGo.SetActive(false);
        PlayerPrefs.SetInt(CAMERA_MODE_TIP_KEY, 1);
        PlayerPrefs.Save();
    }

    private void OnCloseAreaClick()
    {
        TipUiGo.SetActive(false);
        PlayerPrefs.SetInt(CAMERA_MODE_TIP_KEY, 1);
        PlayerPrefs.Save();
    }

    private void OnCloseSelfieTip()
    {
        SelfieTip.SetActive(false);
        PlayerPrefs.SetInt(SELFIE_MODE_TIP_KEY, 1);
        PlayerPrefs.Save();
    }

    private void OnEnterSelfieClick()
    {
        if (!StateManager.Inst.CheckCanEnterSelfieMode())
        {
            return;
        }
        //进入自拍模式
        PlayerBaseControl.Inst.isInSelfieMode = true;
        RequestCleanRedDot((int)ENodeType.SelfieMode);
        SelfieTip.SetActive(PlayerPrefs.GetInt(SELFIE_MODE_TIP_KEY, 0) == 0);
        RefreshUI();

        if (CameraModeManager.Inst.FreePhotoCameraMode != null)
        {
            var handler = CameraModeManager.Inst.FreePhotoCameraMode.Handler as CameraModeHandler;
            handler.ResetJoyStick();
        }

        SelfieModeManager.Inst.EnterSelfieMode();
    }
    private void OnExitSelfieClick()
    {
        //冻结中不能退出自拍模式
        if (PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.SelfieMode))
        {
            TipPanel.ShowToast("Selfie mode cannot be quit while freezing.");
            return;
        }
        //退出自拍模式
        SelfieModeManager.Inst.ExitSelfieMode();
        RefreshUI();
    }

    public void OnEmoPanelShow(bool isActive)
    {
        TakePhotoBtn.gameObject.SetActive(isActive);
        selfieBtnPanel.SetActive(isActive);
    }

    public void RefreshUI()
    {
        ExitSelfieBtn.gameObject.SetActive(StateManager.IsInSelfieMode);
        EnterSelfieBtn.gameObject.SetActive(!StateManager.IsInSelfieMode);
    }

    private void RequestCleanRedDot(int btnId)
    {
        if (mVNode != null && mVNode.mLogic.Count > 0)
        {
            int oldValue = mVNode.mLogic.Count;
            mVNode.mLogic.ChangeCount(oldValue - 1);
        }
        PlayModePanel.Instance.operationRedDotManager.RequestCleanOptRedDot(btnId);
    }

    private void RedDotInitedCallBack(bool isInited)
    {
        if (isInited)
        {
            List<int> redDotId = PlayModePanel.Instance.operationRedDotManager.optBtnIds;
            for (int i = 0; i < redDotId.Count; i++)
            {
                int id = redDotId[i];
                if (id == (int)ENodeType.SelfieMode)
                {
                    AttachOptRedDot();
                }
            }
        }
    }

    public void AttachOptRedDot()
    {
        mVNode = InternalAttachRedDotNode();
        if (mVNode != null && mVNode.mLogic != null)
        {
            mVNode.mLogic.AddListener(ChangedCountCallBack);
            mVNode.mLogic.ChangeCount(mVNode.mLogic.Count);
        }
    }

    private void ChangedCountCallBack(int count)
    {

    }

    private VNode InternalAttachRedDotNode()
    {
        RedDotTree tree = PlayModePanel.Instance.operationRedDotManager.Tree;
        VNode dot = tree.CreateAndBindViewRedDot(EnterSelfieBtn.gameObject, (int)ENodeType.SelfieMode, ERedDotPrefabType.Type4);
        return dot;
    }
    private void Update()
    {
        if (PlayModePanel.Instance.operationRedDotManager!=null 
            && PlayModePanel.Instance.operationRedDotManager.IsInited
            && PlayModePanel.Instance.operationRedDotManager.Tree!=null)
        {
            PlayModePanel.Instance.operationRedDotManager.Tree.Update();
        } 
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (mVNode)
        {
            mVNode.Destroy(false);
        }
    }
}
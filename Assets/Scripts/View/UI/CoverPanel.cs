using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoverPanel : BasePanel<CoverPanel>
{
    public Button ReturnBtn;
    public Button ConfirmBtn;
    public Animator Anim;
    public GameObject Mask;
    public GameObject AnimText;
    public Image refImg;
    private Action returnClick;
    private bool isSpawnVisible = true;

    private const float oriMaskW = 3186;
    private const float oriMaskH = 1125;

    private const int screenShotW = 1740;
    private const int screenShotH = 1113;

    private float curRefWidth = 0;
    private float curRefHeight = 0;
    private Rect screenShotRec = new Rect();

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        ReturnBtn.onClick.AddListener(OnReturnClick);
        ConfirmBtn.onClick.AddListener(OnComfirmClick);
        isSpawnVisible = SceneBuilder.Inst.SpawnPoint.activeSelf;

        curRefHeight = (Screen.height / oriMaskH) * screenShotH;
        curRefWidth = (Screen.height / oriMaskH) * screenShotW;
        refImg.rectTransform.sizeDelta = new Vector2(curRefWidth, curRefHeight);
        screenShotRec = new Rect(Screen.width / 2 + refImg.rectTransform.rect.x, Screen.height / 2 + refImg.rectTransform.rect.y, refImg.rectTransform.rect.width, refImg.rectTransform.rect.height);
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        SceneBuilder.Inst.SpawnPoint.SetActive(false);
    }
    public override void OnBackPressed()
    {
        base.OnBackPressed();
        SceneBuilder.Inst.SpawnPoint.SetActive(isSpawnVisible);
    }

    public void SetReturnClick(Action ret)
    {
        returnClick = ret;
    }

    public void OnReturnClick()
    {
        if(GameManager.Inst.sceneType == SCENE_TYPE.MAP_SCENE || GameManager.Inst.sceneType == SCENE_TYPE.MYSPACE_SCENE) {
            SceneBuilder.Inst.SpawnPoint.SetActive(true);
        }
        SceneGizmoPanel.Show();
        BasePrimitivePanel.Show();
        ReferManager.Inst.EnterReferPlay();
        returnClick?.Invoke();
        Hide();
    }

    public void OnComfirmClick()
    {
        CloseMask(false);
        Anim.Play("SaveAnimtion", 0, 0);
        var bytes = ScreenShotUtils.ScreenShot(GameManager.Inst.MainCamera, screenShotRec);
        if (bytes.Length == 0)
        {
            OnFail();
            return;
        }
        //本地保存封面
        DataUtils.SaveCoverLocal(bytes, CoverType.JPG);
        GameManager.Inst.gameMapInfo.mapStatus.isSetCover = true;
        //保存封面的时候，也需要发送给服务器素材列表
        SceneParser.Inst.GetResList();
        //保存封面的时候，也需要发送给服务器DC列表
        SceneParser.Inst.GetDcList();
        //保存封面的时候，也需要发送给服务器
        SceneParser.Inst.GetUnitySaveData();
        //更新配置信息
        var optType = string.IsNullOrEmpty(GameManager.Inst.gameMapInfo.mapId) ? OperationType.ADD : OperationType.UPDATE;
        DataUtils.SetMapInfoLocal(optType);
        DataUtils.SetConfigLocal(CoverType.JPG);
        //保存成功
        OnSuccess(null);
    }

    

    private void OnSuccess(string content)
    {
        TipPanel.ShowToast("Cover photo saved successfully!");
        CloseMask(true);
        OnReturnClick();
    }

    private void OnFail(string content = "")
    {
        TipPanel.ShowToast("Oops! Failed to save the cover photo :(");
        LoggerUtils.LogError("Cover Save Fail");
        CloseMask(true);
    }

    private void CloseMask(bool isClose)
    {
        Mask.SetActive(!isClose);
        Anim.gameObject.SetActive(!isClose);
        AnimText.SetActive(isClose);
    }
}

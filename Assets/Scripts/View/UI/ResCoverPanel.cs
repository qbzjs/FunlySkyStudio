using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public class ResCoverPanel : BasePanel<ResCoverPanel>
{
    public Button ReturnBtn;
    public Button ConfirmBtn;
    public Animator Anim;
    public GameObject Mask;
    public GameObject AnimText;
    public Image refImg;
    private Action returnClick;
    private string PropFileName;
    private string[] highestResIds;

    private const float oriMaskW = 3186;
    private const float oriMaskH = 1125;

    private const int screenShotW = 846;
    private const int screenShotH = 846;

    private float curRefWidth = 0;
    private float curRefHeight = 0;
    private Rect screenShotRec = new Rect();

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        ReturnBtn.onClick.AddListener(OnReturnClick);
        ConfirmBtn.onClick.AddListener(OnComfirmClick);

        curRefHeight = (Screen.height / oriMaskH) * screenShotH;
        curRefWidth = curRefHeight;
        refImg.rectTransform.sizeDelta = new Vector2(curRefWidth, curRefHeight);
        screenShotRec = new Rect(Screen.width / 2 + refImg.rectTransform.rect.x, Screen.height / 2 + refImg.rectTransform.rect.y, refImg.rectTransform.rect.width, refImg.rectTransform.rect.height);
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        SceneBuilder.Inst.SpawnPoint.SetActive(false);
        ReferManager.Inst.OnReferPlay();
    }
    public override void OnBackPressed()
    {
        base.OnBackPressed();
        SceneBuilder.Inst.SpawnPoint.SetActive(true);
    }

    public void SetReturnClick(Action ret)
    {
        returnClick = ret;
    }

    public void SetPropFileName(string fileName)
    {
        PropFileName = fileName;
    }

    public void SetHighestResIds(string[] strArray)
    {
        this.highestResIds = strArray;
    }

    public void OnReturnClick()
    {
        SceneBuilder.Inst.SpawnPoint.SetActive(true);
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
        AWSUtill.UpLoadPropZipRes(PropFileName, (propJsonUrl) =>
        {
            var bytes = ScreenShotUtils.ResScreenShot(GameManager.Inst.MainCamera, screenShotRec);
            if (bytes.Length == 0)
            {
                OnFail();
                return;
            }
            string fileName = DataUtils.SaveResImg(bytes);
            AWSUtill.UpLoadImage(fileName, (imageUrl) =>
            {
                MapInfo resMapInfo = GameManager.Inst.gameMapInfo.Clone();
                resMapInfo.mapCover = imageUrl;
                resMapInfo.propsJson = propJsonUrl;
                resMapInfo.dataType = 1;
                resMapInfo.highestResIds = highestResIds;
                LoggerUtils.Log("JsonConvert.SerializeObject(resMapInfo) = " + JsonConvert.SerializeObject(resMapInfo));
#if !UNITY_EDITOR
                MobileInterface.Instance.UploadResource(JsonConvert.SerializeObject(resMapInfo));
#endif
                TipPanel.ShowToast("Upload successfully:D");
                CloseMask(true);
                OnReturnClick();
            }, (err) =>
            {
                OnFail();
            });
        }, (err) =>
        {
            OnFail();
        });
    }



    private void OnSuccess(string content)
    {
        TipPanel.ShowToast("Saved successfully:D");
        CloseMask(true);
        OnReturnClick();
    }

    private void OnFail(string content = "")
    {
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        CloseMask(true);
    }

    private void CloseMask(bool isClose)
    {
        Mask.SetActive(!isClose);
        Anim.gameObject.SetActive(!isClose);
        AnimText.SetActive(isClose);
    }
}

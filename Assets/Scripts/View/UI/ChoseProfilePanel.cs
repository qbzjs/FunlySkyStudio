using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChoseProfilePanel : BasePanel<ChoseProfilePanel>
{
    public Button BtnReturn;
    public Button BtnChoseProfile;
    public Button SaveProfile;
    public RawImage ProfileImg;
    [Header("ActionSheet")]
    public GameObject ChoosePanel;
    public Button BtnChosePose;
    public Button BtnChoseAlbum;
    public Button BtnCancel;
    public GameObject LoadingPanel;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        InitButtonAction();
        var portraitUrl = GameManager.Inst.ugcUserInfo.portraitUrl;
        LoadProfile(portraitUrl);
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        ChoosePanel.SetActive(false);
    }

    private void InitButtonAction()
    {
        BtnReturn.onClick.AddListener(OnReturnBtnClick);
        BtnChoseProfile.onClick.AddListener(OnChooseBtnClick);
        SaveProfile.onClick.AddListener(OnSaveProfileBtnClick);
        BtnChosePose.onClick.AddListener(OnBtnChosePoseClick);
        BtnChoseAlbum.onClick.AddListener(OnBtnChoseAlbumClick);
        BtnCancel.onClick.AddListener(OnBtnCancelClick);
    }

    private void OnReturnBtnClick()
    {
        Hide();
        RoleUIManager.Inst.RoleMenuPanel.SetActive(true);
    }

    #region Actionsheet
    private void OnChooseBtnClick()
    {
        ChoosePanel.SetActive(true);
    }

    private void OnBtnChosePoseClick()
    {
        Hide();
        RoleUIManager.Inst.RoleMenuPanel.SetActive(false);
        RoleUIManager.Inst.ProfilePanelActivity(true);
    }

    private void OnBtnCancelClick()
    {
        ChoosePanel.SetActive(false);
    }
    #endregion

    #region SaveProfileBtn Event
    private void OnSaveProfileBtnClick()
    {
        SaveMediaParams saveMediaParams = new SaveMediaParams()
        {
            mediaType = 1,
            mediaUrl = GameManager.Inst.ugcUserInfo.portraitUrl
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.saveMediaToLocal, OnSaveProfileSuccess);
        MobileInterface.Instance.AddClientFail(MobileInterface.saveMediaToLocal, OnSaveProfileFail);
        MobileInterface.Instance.SaveMediaToLocal(JsonConvert.SerializeObject(saveMediaParams));
    }

    private void OnSaveProfileSuccess(string content)
    {
        TipPanel.ShowToast("Saved successfully:D");
    }

    private void OnSaveProfileFail(string content)
    {
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }
    #endregion

    private void OnBtnChoseAlbumClick()
    {
        OpenProfilePageParams albumParams = new OpenProfilePageParams()
        {
            albumType = 0
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.openProfilePage, OnOpenProfilePageSuccess);
        MobileInterface.Instance.AddClientFail(MobileInterface.openProfilePage, OnOpenProfilePageFail);
        MobileInterface.Instance.OpenProfilePage(JsonConvert.SerializeObject(albumParams));
    }

    private void OnOpenProfilePageSuccess(string content)
    {
        OnProfilePageResp onProfilePageResp = JsonConvert.DeserializeObject<OnProfilePageResp>(content);
        if (!string.IsNullOrEmpty(onProfilePageResp.portraitUrl))
        {
            LoadingPanel.SetActive(true);
            LoadProfile(onProfilePageResp.portraitUrl);
            GameManager.Inst.ugcUserInfo.portraitUrl = onProfilePageResp.portraitUrl;
            //Saving Profile
            RoleLoadManager.Inst.SaveUserInfo((content)=> {
                TipPanel.ShowToast("Saved successfully:D");
                LoadingPanel.SetActive(false);
            },
            (err)=> {
                TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
                LoadingPanel.SetActive(false);
            });
        }
    }

    private void OnOpenProfilePageFail(string content)
    {
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }

    private void LoadProfile(string portraitUrl)
    {
        LoadingPanel.SetActive(true);
        StartCoroutine(LoadSprite(portraitUrl, ProfileImg));
    }

    IEnumerator LoadSprite(string url, RawImage image)
    {
        UnityWebRequest wr = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        wr.downloadHandler = texDl;
        wr.timeout = 45;
        yield return wr.SendWebRequest();
        if (!wr.isNetworkError)
        {
            image.texture = texDl.texture;
        }
        else
        {
            TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        }
        texDl.Dispose();
        wr.Dispose();
        LoadingPanel.SetActive(false);
    }
}

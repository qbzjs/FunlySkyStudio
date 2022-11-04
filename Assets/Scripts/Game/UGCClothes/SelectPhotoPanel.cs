using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPhotoPanel : MonoBehaviour
{
    public Button album;
    public Button phoneLibrary;
    public Button closeBtn;
    private UGCClothesPhotoBehaviour curBehav;

    void Start()
    {
        album.onClick.AddListener(OnAlbumClick);
        phoneLibrary.onClick.AddListener(OnPhoneLibraryClick);
        closeBtn.onClick.AddListener(ClosePanel);
    }

    public void OnAlbumClick()
    {
#if UNITY_EDITOR
        TestButton(SavePhotoType.CheckInPhoto);
#else
        OpenProfilePageParams albumParams = new OpenProfilePageParams()
        {
            albumType = 1
        };
        UGCClothesPhotoManager.Inst.targetBehav.photoType = SavePhotoType.CheckInPhoto;
        MobileInterface.Instance.AddClientRespose(MobileInterface.openProfilePage, OnOpenProfilePageSuccess);
        MobileInterface.Instance.OpenProfilePage(JsonConvert.SerializeObject(albumParams));
#endif
        ClosePanel();
    }
    public void OnPhoneLibraryClick()
    {
#if UNITY_EDITOR
        TestButton(SavePhotoType.SystemPhoto);
#else
        //此处打开系统相册，跟之前打开视频使用同一接口只是type不同，故使用BGMusicInfo
        AudioClientArg albumParams = new AudioClientArg()
        {
            albumType = 1
        };
        UGCClothesPhotoManager.Inst.targetBehav.photoType = SavePhotoType.SystemPhoto;
        MobileInterface.Instance.AddClientRespose(MobileInterface.openSystemAlbum, OnOpenProfilePageSuccess);
        MobileInterface.Instance.OpenSystemAlbum(JsonConvert.SerializeObject(albumParams));
#endif
        ClosePanel();
    }

    private void TestButton(SavePhotoType type)
    {
        string content = String.Empty;
        string urlR = "https://i.ibb.co/RcQ3pJP/v2-3942bc6160c1cbc84216731fe935f9f4-1440w.jpg";
        string urlL = "https://cdn.joinbudapp.com/TestFolder/UgcImage/_1644358089.jpg";
        UGCClothesPhotoManager.Inst.targetBehav.photoType = type;
        if (type == SavePhotoType.CheckInPhoto)
        {
            OnProfilePageResp data = new OnProfilePageResp()
            {
                compressUrl = urlL
            };
            content = JsonConvert.SerializeObject(data);
        }
        else
        {
            BGMusicInfo data = new BGMusicInfo()
            {
                resPath = urlR
            };
            content = JsonConvert.SerializeObject(data);
        }
        OnOpenProfilePageSuccess(content);
    }

    private void OnOpenProfilePageSuccess(string content)
    {
        string url = String.Empty;
        if (UGCClothesPhotoManager.Inst.targetBehav.photoType == SavePhotoType.CheckInPhoto)
        {
            OnProfilePageResp dataUrl = JsonConvert.DeserializeObject<OnProfilePageResp>(content);
            url = dataUrl.compressUrl;
        }
        else
        {
            //此处打开系统相册，跟之前打开视频使用同一接口只是type不同，故使用BGMusicInfo
            var info = JsonConvert.DeserializeObject<BGMusicInfo>(content);
            url = info.resPath;
        }
        if (string.IsNullOrEmpty(url))
        {
            TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
            return;
        }
        curBehav.data.photoUrl = url;
        curBehav.LoadPhoto();
    }

    public void ShowPanel(UGCClothesPhotoBehaviour behav)
    {
        curBehav = behav;
        gameObject.SetActive(true);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}

using System;
using Newtonsoft.Json;
using SavingData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum PanelType
{
    AddPhoto,
    Loading,
    ChangePhoto
}

/// <summary>
/// Author: 熊昭
/// Description: 3D相册道具操作面板类
/// Date: 2022-02-06 18:00:27
/// </summary>
public class ShotPhotoPanel : InfoPanel<ShotPhotoPanel>
{
    public GameObject NoPanel;
    public GameObject HasPanel;
    public GameObject AddPanel;
    public GameObject LoadPanel;
    public Button AddButton;
    public Button ChangeButton;
    public RawImage PhotoImage;
    public Button AddPhotoButton;

    private SceneEntity sEntity;
    private ShotPhotoBehaviour sBehv;
    private Vector2 orginSize;
    private float ratio = 1.78f;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        AddButton.onClick.AddListener(OnButtonClick);
        ChangeButton.onClick.AddListener(OnChangeButtonClick);
        AddPhotoButton.onClick.AddListener(OnPhotoButtonClick);
        orginSize = PhotoImage.rectTransform.sizeDelta;
    }

    public void SetEntity(SceneEntity entity)
    {
        sEntity = entity;
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        sBehv = bindGo.GetComponent<ShotPhotoBehaviour>();
        var tex = sBehv.GetCurrentTexture();
        RefreshPanel(sBehv, tex);

        sBehv.onLoadSuc = OnLoadSuccess;
        sBehv.onLoadFai = OnLoadFail;
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        if (sBehv)
        {
            sBehv.onLoadSuc = null;
            sBehv.onLoadFai = null;
        }
    }

    private void OnDisable()
    {
        OnBackPressed();
    }

    private void RefreshPanel(ShotPhotoBehaviour behav, Texture tex = null)
    {
        PanelType type;
        if (behav.isHasPhoto)
        {
            PhotoImage.texture = tex;
            type = PanelType.ChangePhoto;
            if (sEntity.Get<ShotPhotoComponent>().type == SavePhotoType.CheckInPhoto)
            {
                PhotoImage.rectTransform.sizeDelta = orginSize;
            }
            else
            {
                var size = orginSize;
                var nr = (float)tex.width / tex.height;
                if (nr >= ratio)
                {
                    size.y = orginSize.x / nr;
                }
                else
                {
                    size.x = orginSize.y * nr;
                }
                PhotoImage.rectTransform.sizeDelta = size;
            }
        }
        else if (behav.isLoading)
        {
            type = PanelType.Loading;
        }
        else
        {
            type = PanelType.AddPhoto;
        }
        ShowPanel(type);
    }

    private void DisableAllPanel()
    {
        NoPanel.SetActive(false);
        AddPanel.SetActive(false);
        LoadPanel.SetActive(false);
        HasPanel.SetActive(false);
    }

    private void ShowPanel(PanelType type)
    {
        DisableAllPanel();
        switch (type)
        {
            case PanelType.AddPhoto:
                NoPanel.gameObject.SetActive(true);
                AddPanel.gameObject.SetActive(true);
                break;
            case PanelType.Loading:
                NoPanel.gameObject.SetActive(true);
                LoadPanel.gameObject.SetActive(true);
                break;
            case PanelType.ChangePhoto:
                HasPanel.gameObject.SetActive(true);
                break;
        }
    }

    private void OnButtonClick()
    {
#if UNITY_EDITOR
        TestButton(SavePhotoType.CheckInPhoto);
#else
        OpenProfilePageParams albumParams = new OpenProfilePageParams()
        {
            albumType = 1
        };
        sEntity.Get<ShotPhotoComponent>().type = SavePhotoType.CheckInPhoto;
        MobileInterface.Instance.AddClientRespose(MobileInterface.openProfilePage, OnOpenProfilePageSuccess);
        MobileInterface.Instance.OpenProfilePage(JsonConvert.SerializeObject(albumParams));
#endif
    }
    
    private void OnPhotoButtonClick()
    {
#if UNITY_EDITOR
        TestButton(SavePhotoType.SystemPhoto);
#else
        //此处打开系统相册，跟之前打开视频使用同一接口只是type不同，故使用BGMusicInfo
        AudioClientArg albumParams = new AudioClientArg()
        {
            albumType = 1
        };
        sEntity.Get<ShotPhotoComponent>().type = SavePhotoType.SystemPhoto;
        MobileInterface.Instance.AddClientRespose(MobileInterface.openSystemAlbum, OnOpenProfilePageSuccess);
        MobileInterface.Instance.OpenSystemAlbum(JsonConvert.SerializeObject(albumParams));
#endif
    }
    
    private void OnChangeButtonClick()
    {
        switch (sEntity.Get<ShotPhotoComponent>().type)
        {
            case SavePhotoType.CheckInPhoto:
                OnButtonClick();
                break;
            case SavePhotoType.SystemPhoto:
                OnPhotoButtonClick();
                break;
            default:
                OnButtonClick();
                break;
        }
        
    }

    private void TestButton(SavePhotoType type)
    {
        // type = (SavePhotoType)Random.Range(0, 2);
        string content = String.Empty;
        string urlR = "https://i.peo.pw/2022/08/17/62fc7a5293524.jpg";
        string urlL = "https://cdn.joinbudapp.com/TestFolder/UgcImage/_1644358089.jpg";
        sEntity.Get<ShotPhotoComponent>().type = type;
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
        LoggerUtils.Log("ShotPhotoPanel OnOpenProfilePageSuccess Content is -- " + content);
        string url = String.Empty;
        if (sEntity.Get<ShotPhotoComponent>().type == SavePhotoType.CheckInPhoto)
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
        sEntity.Get<ShotPhotoComponent>().photoUrl = url;
        sBehv.LoadPhoto();
        RefreshPanel(sBehv, sBehv.GetCurrentTexture());
    }

    private void OnLoadSuccess(Texture2D tex)
    {
        RefreshPanel(sBehv, tex);
    }

    private void OnLoadFail()
    {
        RefreshPanel(sBehv);
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }
}
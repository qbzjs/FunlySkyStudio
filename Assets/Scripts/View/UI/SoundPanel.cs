/// <summary>
/// Author:MeiMei—LiMei
/// Description:音效道具的设置Panel
/// Date: 2022-01-13
/// </summary>
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public struct SoundInfo
{
    public string resName;
    public string resPath;
}
public class AudioClientArg
{
    public int propType;// 0 Bgm背景音 1声音交互按钮
    public int albumType;
}

public class SoundPanel : InfoPanel<SoundPanel>
{

    [Header("Panel")]
    public GameObject MaskPanel;
    public GameObject AddPanel;
    public GameObject HasPanel;
    public PropertySwitchPanel switchPanel;
    public PropertyCollectiblesPanel collectiblesPanel;

    public PropertySensorBoxPanel sensorBoxPanel;

    [Header("Button")]
    public Button AddButton;
    public Button CloseButton;
    public Button HasButton;
    // public Button NoButton;//"no mosic" buttom

    [Header("GameObject")]
    public GameObject HasSelect;
    public GameObject AddSelect;
    public GameObject NoSelect;

    [HideInInspector]
    public GameObject LastSelect;

    [Header("Text")]
    public Text BgNameText;

    [Header("ScrollRect")]
    public ScrollRect scrollRect;

    [Header("Toggle")]
    public Toggle tapToggle;

    [Header("  ")]
    private SoundInfo sInfo;
    private SceneEntity sEntity;
    private SoundButtonBehaviour sbBehv;
    protected static BasePanel inst;
    string fullPath = "";  //????url
    private bool isForceHide;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        AddButton.onClick.AddListener(OnAddClick);
        // NoButton.onClick.AddListener(OnNoClick);
        CloseButton.onClick.AddListener(OnCloseClick);
        HasButton.onClick.AddListener(OnHasClick);
        tapToggle.onValueChanged.AddListener(OnTapClick);
        InitControlPanel();
    }

    private void OnTapClick(bool isOn)
    {
        var ctrl = SoundControl.SUPPORT_CTRL_MUSIC;
        if (isOn)
        {
            ctrl = SoundControl.NOT_SUPPORT;
        }
        tapToggle.isOn = isOn;
        sEntity.Get<SoundComponent>().isControl = (int)ctrl;
        sbBehv.RefreshButtonCanTouch(isOn);
    }

    private void InitControlPanel()
    {
        switchPanel.CtrlType = SwitchControlType.SOUNDPLAY_CONTROL;
        switchPanel.Init();
        collectiblesPanel.CtrlType = CollectControlType.SOUNDPLAY_CONTROL;
        collectiblesPanel.Init();

        sensorBoxPanel.CtrlType = PropControlType.SOUNDPLAY_CONTROL;
        sensorBoxPanel.Init();
    }

    public void SetOppositePanelHide(bool isHide)
    {
        if (isHide)
        {
            if (gameObject.activeInHierarchy)
            {
                Hide();
                isForceHide = true;
            }
        }
        else
        {
            if (isForceHide)
            {
                Show();
                isForceHide = false;
            }
        }
    }
    public void ResetForceHidePanel()
    {
        isForceHide = false;
    }    
    public  void BecameVisible()//按钮状态切换
    {
        var bindGo = sEntity.Get<GameObjectComponent>().bindGo;
        sbBehv = bindGo.GetComponent<SoundButtonBehaviour>();
        var comp = sEntity.Get<SoundComponent>();
        switch (comp.musicType)
        {
            case musicType.importMusic:
                if (!string.IsNullOrEmpty(comp.soundName) && !string.IsNullOrEmpty(comp.soundUrl)&&sbBehv.importASource.clip!=null)
                {
                    BgNameText.text = comp.soundName;
                    ShowSelect(HasSelect);
                    ShowPanel(false);                 
                }
                else
                {
                    ShowPanel(true);
                    ShowSelect(AddSelect);
                }
                break;
            case musicType.noMusic:
                BgNameText.text = comp.soundName;
                OnNoClick();
                break;
        }
    }
    public void SetEntity(SceneEntity entity)
    {
        sEntity = entity;
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        sbBehv = bindGo.GetComponent<SoundButtonBehaviour>();
        var comp = entity.Get<SoundComponent>();
        ShowPanel(string.IsNullOrEmpty(comp.soundName));
        BecameVisible();
        switchPanel.SetEntity(entity);
        collectiblesPanel.SetEntity(entity);
        sensorBoxPanel.SetEntity(entity);
        var isOn = comp.isControl == (int)SoundControl.NOT_SUPPORT;
        OnTapClick(isOn);
    }
    public void InitData()
    {
        // ShowSelect(NoSelect);
        ShowSelect(AddSelect);
        ShowPanel(true);
        OnNoClick();
        MaskPanel.SetActive(false);
    }

    private void ShowPanel(bool isAdd)
    {
        AddPanel.SetActive(isAdd);
        HasPanel.SetActive(!isAdd);
    }

    private void GetSoundInfo(string content)
    {
        sInfo = JsonConvert.DeserializeObject<SoundInfo>(content);
        sEntity.Get<SoundComponent>().soundName = sInfo.resName;
        sEntity.Get<SoundComponent>().soundUrl = sInfo.resPath;
        MaskPanel.SetActive(true);
        string audioName = sEntity.Get<SoundComponent>().soundName;
        if (!audioName.Contains(".mp3"))
        {
            audioName += ".mp3";
        }
        string resName = GameUtils.GetTimeStamp() + "_" + audioName;
        AWSUtill.UpLoadRes(resName, sInfo.resPath, AWSUtill.videoPath, OnUploadSuccess, OnUploadFail);
    }
    private void OnUploadSuccess(string url)
    {
        Debug.Log("resPath  :" + url);//test
        sEntity.Get<SoundComponent>().soundUrl = url;
        if (!File.Exists(sInfo.resPath))
        {
            LoggerUtils.Log(sInfo.resPath + " is not exist");
        }
        if (Application.platform == RuntimePlatform.Android)
        {
            fullPath = "jar:file://" + sInfo.resPath;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            fullPath = "file://" + sInfo.resPath;
        }
        StartCoroutine(ResManager.Inst.GetAudioClip(fullPath, GetAudioClipSuccess, GetAudioClipFail));
    }

    private void OnUploadFail(string error)
    {
        MaskPanel.SetActive(false);
        TipPanel.ShowToast("Try again:(");
    }

    private void GetAudioClipSuccess(AudioClip clip)
    {
        MaskPanel.SetActive(false);
        BgNameText.text = sInfo.resName;
        ShowPanel(false);
        ShowSelect(HasSelect);
        sbBehv.SetAudio(clip);
        sbBehv.Play2DAudio();
        File.Delete(sInfo.resPath);
        Debug.LogError("Get Audio Clip Success");//test
    }
    private void GetAudioClipFail()
    {
        Debug.LogError("Get Audio Clip Fail");//test
        MaskPanel.SetActive(false);
        TipPanel.ShowToast("Try again:(");
        File.Delete(sInfo.resPath);
    }
    private void ShowSelect(GameObject curSelect)
    {
        if (curSelect == null)
        {
            return;
        }
        if (LastSelect != null)
        {
            LastSelect.SetActive(false);
        }
        LastSelect = curSelect;
        curSelect.SetActive(true);
    }

    private void OnAddClick()
    {
        sbBehv.Stop();
        ShowSelect(AddSelect);
        AudioClientArg tempArg = new AudioClientArg();
        tempArg.albumType = 0;
        tempArg.propType = 1;
        sEntity.Get<SoundComponent>().musicType = musicType.importMusic;
        MobileInterface.Instance.AddClientRespose(MobileInterface.openSystemAlbum, GetSoundInfo);
        MobileInterface.Instance.OpenSystemAlbum(JsonConvert.SerializeObject(tempArg));
    }
    private void OnCloseClick()
    {
        sbBehv.Stop();
        bool isShowPanel = true;
        ShowPanel(isShowPanel);
        AddSelect.SetActive(true);
        sbBehv.SetAudio(null);
        var comp = sEntity.Get<SoundComponent>();
        comp.soundName = null;
        comp.soundUrl = null;
        comp.soundUrl = null;
        fullPath = null;
    }

    private void OnHasClick()
    {
        sbBehv.Stop();
        sEntity.Get<SoundComponent>().musicType = musicType.importMusic;
        ShowSelect(HasSelect);
        sbBehv.Play2DAudio();
    }
    private void OnNoClick()
    {
        // ShowSelect(NoSelect);
        ShowSelect(AddSelect);
        sEntity.Get<SoundComponent>().musicType = musicType.noMusic;
        sbBehv.Stop();
    }
    public override void OnBackPressed()
    {
        base.OnBackPressed();
        if (sbBehv!=null)
        {
            sbBehv.Stop();
        } 
    }
}

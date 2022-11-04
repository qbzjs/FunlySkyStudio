using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Entitas;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public struct BGMusicInfo
{
    public string resName;
    public string resPath;
}

public class BGClientArg
{
    public int propType;// 0 Bgm背景音 1声音交互按钮
    public int albumType;
}

public class BGMusicPanel : InfoPanel<BGMusicPanel>
{
    public Transform ItemParent;
    public GameObject itemPrefab;
    public Sprite[] iconSprites;
    public List<CommonSelectItem> allSelectItem;
    public GameObject HasSelect;
    public GameObject AddSelect;
    public Button AddButton;
    public Button CloseButton;
    public Button HasButton;

    [Header("Panel")]
    public GameObject MaskPanel;
    public GameObject AddPanel;
    public GameObject HasPanel;
    private GameObject LastSelect;
    private Animator LastAnimator;
    private Image LastIconImg;
    private Animator CurAnimator;


    [Header("Text")]
    public Text BgNameText;
    private BGMusicInfo mInfo;
    private SceneEntity entity;
    private BGMusicBehaviour bgBehv;
    //protected static BGMusicItem inst;
    protected static BasePanel inst;
    string fullPath = "";  //????url


    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        AddButton.onClick.AddListener(OnAddClick);
        CloseButton.onClick.AddListener(OnCloseClick);
        HasButton.onClick.AddListener(OnHasClick);
        allSelectItem = new List<CommonSelectItem>();
        entity = SceneBuilder.Inst.BGMusicEntity;
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        bgBehv = bindGo.GetComponent<BGMusicBehaviour>();
        for (int i = 0; i < GameManager.Inst.bgmConfigDatas.Count; i++)
        {
            int index = i;
            var itemGo = GameObject.Instantiate(itemPrefab, ItemParent);
            var itemScript = itemGo.GetComponent<CommonSelectItem>();
            itemScript.SetText(GameManager.Inst.bgmConfigDatas[i].showName);
            itemScript.SetIcon(i == 0 ? iconSprites[0] : iconSprites[1]);
            itemScript.AddClick(() => OnSelect(index));
            allSelectItem.Add(itemScript);
        }
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        entity = SceneBuilder.Inst.BGMusicEntity;
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        bgBehv = bindGo.GetComponent<BGMusicBehaviour>();
        var comp = entity.Get<BGMusicComponent>();
        var bgmData = GameManager.Inst.bgmConfigDataDics[comp.musicId];
        int index = Array.FindIndex(GameManager.Inst.bgmConfigDatas.ToArray(), x => x == bgmData);
        switch (comp.musicType)
        {
            case 0:
                if (!string.IsNullOrEmpty(comp.bgName) && !string.IsNullOrEmpty(comp.bgUrl))
                {
                    BgNameText.text = mInfo.resName;
                    ShowSelect(HasSelect);
                    ShowPanel(false);
                }
                else {
                    ShowPanel(true);
                    ShowSelect(AddSelect);
                }
                break;
            case 1:
            case 2:
                ShowSelect(allSelectItem[index].SelectGo);
                break;
        }
        SwitchWithoutAnim();
    }
    private void OnSelect(int index)
    {
        bgBehv.Stop();
        OnUISelect(index);
        int id = GameManager.Inst.bgmConfigDatas[index].id;
        entity.Get<BGMusicComponent>().musicId = id;
        if (id == 0)
        {
            entity.Get<BGMusicComponent>().musicType = 1;
        }
        else
        {
            entity.Get<BGMusicComponent>().musicType = 2;
            bgBehv.Play(false, SwitchWithoutAnim);
        }
    }
    private void OnUISelect(int index)
    {
        if (index == 0)
        {
            CurAnimator = null;
        }
        else
        {
            PlayIconAnim(allSelectItem[index].Anim, allSelectItem[index].Icon);
        }
        SwitchWithoutAnim();
        ShowSelect(allSelectItem[index].SelectGo);
    }

    public void InitData()
    {
        var comp = entity.Get<BGMusicComponent>();
        ShowPanel(string.IsNullOrEmpty(comp.bgName)||string.IsNullOrEmpty(comp.bgUrl));
        BgNameText.text = comp.bgName;
        //MaskPanel.SetActive(false);
    }

    private void ShowPanel(bool isAdd)
    {
        AddPanel.SetActive(isAdd);
        HasPanel.SetActive(!isAdd);
    }

    private void GetBGMusicInfo(string content)
    {
        mInfo = JsonConvert.DeserializeObject<BGMusicInfo>(content);
        BgNameText.text = mInfo.resName;
        entity.Get<BGMusicComponent>().bgName = mInfo.resName;
        entity.Get<BGMusicComponent>().bgPath = mInfo.resPath;
        MaskPanel.SetActive(true);
        string audioName = mInfo.resName;
        if (!audioName.Contains(".mp3"))
        {
            audioName += ".mp3";
        }
        string resName = GameUtils.GetTimeStamp() + "_"+ audioName;
        AWSUtill.UpLoadRes(resName, mInfo.resPath, AWSUtill.videoPath, OnUploadSuccess, OnUploadFail);
    }

    private void OnUploadSuccess(string url)
    {
        LoggerUtils.Log("resPath  :" + url);//test
        entity.Get<BGMusicComponent>().bgUrl = url;
        if (!File.Exists(mInfo.resPath))
        {
            LoggerUtils.Log(mInfo.resPath+ " is not exist");
        }
        if (Application.platform == RuntimePlatform.Android)
        {
            fullPath = "jar:file://" + mInfo.resPath;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            fullPath = "file://" + mInfo.resPath;
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
        LoggerUtils.Log("Get Audio Clip Success");//test
        MaskPanel.SetActive(false);
        ShowPanel(false);
        ShowSelect(HasSelect);
        bgBehv.SetAudio(clip);
        bgBehv.Play(false);
        File.Delete(mInfo.resPath);  
    }

    private void GetAudioClipFail()
    {
        LoggerUtils.Log("Get Audio Clip Fail");//test
        MaskPanel.SetActive(false);
        TipPanel.ShowToast("Try again:(");
        File.Delete(mInfo.resPath);
    }
    private void ShowSelect(GameObject curSelect)
    {
        if(curSelect == null)
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

    private void PlayIconAnim(Animator curAnimator, Image curIconImg)
    {
        if (curAnimator == null || curIconImg == null)
        {
            return;
        }

        if (LastAnimator != null)
        {
            //LastAnimator.enabled = false;
            LastAnimator.gameObject.SetActive(false);
        }
        LastAnimator = curAnimator;
        CurAnimator = curAnimator;
        curAnimator.gameObject.SetActive(true);
        curAnimator.Play("bgmusic");

        if (LastIconImg != null)
        {
            LastIconImg.gameObject.SetActive(true);
        }
        LastIconImg = curIconImg;
        curIconImg.gameObject.SetActive(false);

    }
    private void SwitchWithoutAnim()
    {
        if(CurAnimator == LastAnimator)
        {
            return;
        }
        if (LastAnimator != null)
        {
            LastAnimator.gameObject.SetActive(false);
            LastAnimator = null;
        }
        if (LastIconImg != null)
        {
            LastIconImg.gameObject.SetActive(true);
            LastIconImg = null;
        }
    }
    private void OnAddClick()
    {
        CurAnimator = null;
        bgBehv.Stop();
        SwitchWithoutAnim();
        ShowSelect(AddSelect);
        entity.Get<BGMusicComponent>().musicType = 0;
        entity.Get<BGMusicComponent>().musicId = 0;
        AudioClientArg tempArg = new AudioClientArg();
        tempArg.albumType = 0;
        tempArg.propType = 0;
        MobileInterface.Instance.AddClientRespose(MobileInterface.openSystemAlbum, GetBGMusicInfo);//调用安卓端的方法
        MobileInterface.Instance.OpenSystemAlbum(JsonConvert.SerializeObject(tempArg));
    }
    private void OnCloseClick()
    {
        bool isShowPanel = true;

        ShowPanel(isShowPanel);
        AddSelect.SetActive(false);
        bgBehv.SetAudio(null);
        var comp = entity.Get<BGMusicComponent>();
        comp.bgName = null;
        comp.bgPath = null;
        comp.bgUrl = null;
        fullPath = null;
    }
    private void OnHasClick()
    {
        bgBehv.Stop();
        entity.Get<BGMusicComponent>().musicType = 0;
        entity.Get<BGMusicComponent>().musicId = 0;
        ShowSelect(HasSelect);
        CurAnimator = null;
        SwitchWithoutAnim();
        bgBehv.Play(false);
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        bgBehv.Stop();
        CurAnimator = null;
    }

    private void OnDisable()
    {
        if(bgBehv != null)
        {
            bgBehv.Stop();
        }
        CurAnimator = null;
    }
}

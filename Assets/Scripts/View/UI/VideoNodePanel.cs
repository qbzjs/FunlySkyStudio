using UnityEngine;
using UnityEngine.UI;

public enum VideoSoundRange
{
    Near,
    Medium,
    Far,
    Infinite
}
/// <summary>
/// Author:Shaocheng
/// Description:视频道具UI
/// Date: 2022-3-30 19:43:08
/// </summary>
public class VideoNodePanel : InfoPanel<VideoNodePanel>
{
    #region UI

    public Button btnClearUrl;
    public Button btnDone;
    public Button btnInput;

    public Button closeLoadingBtn, closeReadyBtn;
    public Text textUrl;
    public Toggle togNear;
    public Toggle togMed;
    public Toggle togFar;
    public Toggle togInfi;

    public GameObject editUI;
    public GameObject loadingUI;
    public GameObject readyUI;

    public Text ready_text;
    public Button ready_clearBtn;

    private int maxLength = 80;

    #endregion

    private const string urlPlaceHolder = "Enter YouTube link...";
    private VideoNodeBehaviour vBehav;
    private string URLPlaceHolder { get { return LocalizationConManager.Inst.GetLocalizedText(urlPlaceHolder); } }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        btnClearUrl.onClick.AddListener(OnClearUrlClick);
        ready_clearBtn.onClick.AddListener(OnClearUrlClick);
        btnDone.onClick.AddListener(OnDoneClick);
        btnInput.onClick.AddListener(OnInputUrlClick);
        closeLoadingBtn.onClick.AddListener(OnCloseBtnClick);
        closeReadyBtn.onClick.AddListener(OnCloseBtnClick);
        togNear.onValueChanged.AddListener((isOn) => { OnToggleRange(isOn, VideoSoundRange.Near); });
        togMed.onValueChanged.AddListener((isOn) => { OnToggleRange(isOn, VideoSoundRange.Medium); });
        togFar.onValueChanged.AddListener((isOn) => { OnToggleRange(isOn, VideoSoundRange.Far); });
        togInfi.onValueChanged.AddListener((isOn) => { OnToggleRange(isOn, VideoSoundRange.Infinite); });
    }

    public void OnCloseBtnClick()
    {
        var opanel = UIManager.Inst.uiCanvas.GetComponentsInChildren<IPanelOpposable>(true);
        for (int i = 0; i < opanel.Length; i++)
        {
            opanel[i].SetGlobalHide(true);
        }
        this.gameObject.SetActive(false);
    }

    public void SetEntity(SceneEntity entity)
    {
        vBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<VideoNodeBehaviour>();
        RefreshPanel();
    }

    public void RefreshPanel()
    {
        if (vBehav == null || editUI == null || loadingUI == null) return;

        var status = vBehav.currentStatus;
        var url = vBehav.entity.Get<VideoNodeComponent>().videoUrl;
        var range = vBehav.entity.Get<VideoNodeComponent>().soundRange;
        LocalizationConManager.Inst.SetSystemTextFont(textUrl);
        textUrl.text = string.IsNullOrEmpty(url)? URLPlaceHolder : url;
        LoggerUtils.Log($"[Video]-->RefreshPanel-->status:{status}-->url:{url}");

        switch (status)
        {
            case VideoLoadStatus.Empty:
                editUI.gameObject.SetActive(true);
                loadingUI.gameObject.SetActive(false);
                readyUI.gameObject.SetActive(false);
                break;
                
            case VideoLoadStatus.UrlReady:
            case VideoLoadStatus.ReadyToPlay:
                editUI.gameObject.SetActive(false);
                loadingUI.gameObject.SetActive(false);
                readyUI.gameObject.SetActive(true);

                ready_text.text = url;
                RefreshPanelRange(range);
                break;

            case VideoLoadStatus.UrlLoading:
                editUI.gameObject.SetActive(false);
                loadingUI.gameObject.SetActive(true);
                readyUI.gameObject.SetActive(false);
                break;
        }
    }

    private void RefreshPanelRange(int range)
    {
        togNear.isOn = range == (int)VideoSoundRange.Near;
        togMed.isOn = range == (int)VideoSoundRange.Medium;
        togFar.isOn = range == (int)VideoSoundRange.Far;
        togInfi.isOn = range == (int)VideoSoundRange.Infinite;
    }

    private void OnToggleRange(bool isOn, VideoSoundRange range)
    {
        if (isOn)
        {
            vBehav.entity.Get<VideoNodeComponent>().soundRange = (int)range;
            vBehav.InitSoundRange();
            VideoNodeManager.Inst.SetRangeEffectSize((int)range);
        }
    }

    private void OnClearUrlClick()
    {
        LoggerUtils.Log($"[Video]-->OnClearUrlClick");
        textUrl.text = URLPlaceHolder;
        vBehav.StopPlayVideo();
        vBehav.entity.Get<VideoNodeComponent>().videoUrl = string.Empty;
        vBehav.entity.Get<VideoNodeComponent>().soundRange = (int)VideoSoundRange.Near;
        vBehav.SetVideoStatus(VideoLoadStatus.Empty);
        VideoNodeManager.Inst.SetRangeEffectVisible(false);
        RefreshPanel();
    }

    private void OnDoneClick()
    {
        LoggerUtils.Log($"[Video]-->OnDoneClick-->textUrl.text:{textUrl.text}");

        if (string.IsNullOrEmpty(textUrl.text) || textUrl.text.Equals(URLPlaceHolder))
        {
            return;
        }

        vBehav.StartLoadVideoUrl(textUrl.text);
        RefreshPanel();
    }


    #region Keyboard Things

    public void OnInputUrlClick()
    {
#if !UNITY_EDITOR
        var str = textUrl.text.Trim();
        str = str.Equals(URLPlaceHolder) ? "" : str;
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = URLPlaceHolder,
            inputMode = 2,
            inputFlag = 0,
            defaultText = str,
            returnKeyType = (int)ReturnType.Done
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowKeyBoard);
        //LoggerUtils.Log("JsonUtility.ToJson(keyBoardInfo)==="+ JsonUtility.ToJson(keyBoardInfo));
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
#else
        UnityLocalTest_InputYoutubeUrl();
#endif
    }

    public void ShowKeyBoard(string str)
    {
        LoggerUtils.Log($"[Video]-->ShowKeyBoard-->{str}");

        if (string.IsNullOrEmpty(str.Trim()))
            str = URLPlaceHolder;

        textUrl.text = str;
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }

    #endregion


    #region Unity测试使用

#if UNITY_EDITOR
    private void UnityLocalTest_InputYoutubeUrl()
    {
        //TODO:部分视频链接加载会报错
        // var url = "https://www.youtube.com/shorts/hLJ-6xdMfBA";
        // var url = "https://www.youtube.com/watch?v=kgx4WGK0oNU"; //问题视频---右侧带评论区--无法解析视频内容一直重试
        var url = "https://www.youtube.com/watch?v=tcTF_ag_wWU";
        //var url = "https://www.youtube.com/watch?v=XL85PAIsOZc";
         // var url = "asdfsaf";
        // var url = "https://www.bilibili.com/video/BV1bT4y1S7Sp?spm_id_from=333.851.b_7265636f6d6d656e64.2";
        ShowKeyBoard(url);
    }


#endif

    #endregion
}
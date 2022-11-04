using System;
using System.Collections;
using LightShaft.Scripts;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Video;

public enum VideoLoadStatus
{
    Empty,
    UrlLoading,
    UrlReady,
    PreLoading,
    ReadyToPlay,
}

public enum VideoScreenIcon
{
    PlayRender,
    ic_play,
    ic_pause,
    ic_fullscreen,
    Video_link,
}
/// <summary>
/// Author:Shaocheng
/// Description:视频道具行为
/// Date: 2022-3-30 19:43:08
/// </summary>
//TODO:UndoRedo支持
public class VideoNodeBehaviour : NodeBaseBehaviour
{
    private static MaterialPropertyBlock mpb;
    private Color[] oldColor;
    private bool isCanClick = true;

    private AudioSource aSource;
    private VideoPlayer videoPlayer;
    private YoutubePlayer player;
    private YoutubeVideoEvents youtubeVideoEvents;

    private GameObject youtubeLinkBtn;
    private MeshRenderer playRender;
    private MeshRenderer thumbnailRenderer;
    private GameObject controlPanel;
    private GameObject iconPlay;
    private GameObject iconPause;

    public VideoLoadStatus currentStatus;
    public int curRetryTimes;
    private const string PLAY_IMG_PATH = "Texture/ModelTexture/VideoNode/img_video_play";
    private const string EDIT_IMG_PATH = "Texture/ModelTexture/VideoNode/img_video";
    private Texture editTexture;
    private Texture playTexture;

    private const float orgMaxRng = 15; //默认最大声音范围

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        if (mpb == null) mpb = new MaterialPropertyBlock();

        aSource = GetComponent<AudioSource>();
        videoPlayer = GetComponentInChildren<VideoPlayer>();
        player = GetComponentInChildren<YoutubePlayer>();
        youtubeVideoEvents = GetComponentInChildren<YoutubeVideoEvents>();
        videoPlayer.targetCamera = Camera.main;
        player.videoPlayer = videoPlayer;

        // playRender = transform.Find("PlayRender").GetComponent<MeshRenderer>();
        // thumbnailRenderer = transform.Find("ThumbnailRenderer").GetComponent<MeshRenderer>();

        SwitchRenderTexture();

        youtubeLinkBtn = transform.Find("Video_link").gameObject;
        controlPanel = transform.Find("ControllMainUI").gameObject;
        iconPlay = controlPanel.transform.Find("ic_play").gameObject;
        iconPause = controlPanel.transform.Find("ic_pause").gameObject;
        ShowOrHideLinkBtn(false);

        StopPlayVideo();
        InitYoutubePlayer();
        SetVideoStatus(VideoLoadStatus.Empty);
        SetAudioSourceVolume(VideoNodeManager.Inst.videoVolume);
    }
    
    private void OnEnable()
    {
        LoggerUtils.Log($"[Video]-->OnEnable");
        if (GlobalFieldController.CurGameMode == GameMode.Guest || GlobalFieldController.CurGameMode == GameMode.Play)
        {
            if (VideoNodeManager.Inst.isFinishEnterRoom) StartLoadVideo();
        }
    }

    private void OnDisable()
    {
        LoggerUtils.Log($"[Video]-->OnDisable");
        if (GlobalFieldController.CurGameMode == GameMode.Guest || GlobalFieldController.CurGameMode == GameMode.Play)
        {
            if (VideoNodeManager.Inst.isFinishEnterRoom) StopPlayVideo();
        }
    }

    public void ClearVideoInfo()
    {
        currentStatus = VideoLoadStatus.Empty;
        curRetryTimes = 0; 
    }

    public override void OnReset()
    {
        base.OnReset();
    }

    public override void OnDestroy()
    {
        curRetryTimes = 0;
        StopPlayVideo();
        SwitchRenderTexture();
    }

    public void SetVideoStatus(VideoLoadStatus status)
    {
        currentStatus = status;
        if (VideoNodePanel.Instance) VideoNodePanel.Instance.RefreshPanel();
        LoggerUtils.Log($"[Video]-->SetVideoStatus-->{currentStatus}");
    }

    public void InitSoundRange()
    {
        int range = entity.Get<VideoNodeComponent>().soundRange;
        //3d音效
        aSource.spatialBlend = range == (int)VideoSoundRange.Infinite ? 0 : 1; //全场音效
        //声音范围
        aSource.minDistance = 0; //需要先重置最短距离
        aSource.maxDistance = orgMaxRng * Mathf.Pow(2, range); //2^x
        aSource.minDistance = aSource.maxDistance - 5; //2^x - 5
    }

    private void SwitchRenderTexture()
    {
        // LoggerUtils.Log($"[Video]-->SwitchRenderTexture");

        //TODO:后续再考虑替换占位图
        //编辑模式用蓝图，游玩/试玩模式用灰色占位图
        // if (GlobalFieldController.CurGameMode == GameMode.Guest || GlobalFieldController.CurGameMode == GameMode.Play)
        // {
        //     if (playTexture == null) playTexture = ResManager.Inst.LoadRes<Texture>(PLAY_IMG_PATH);
        //     playRender.material.mainTexture = playTexture;
        //     thumbnailRenderer.material.mainTexture = playTexture;
        // }
        // else
        // {
        //     if (editTexture == null) editTexture = ResManager.Inst.LoadRes<Texture>(EDIT_IMG_PATH);
        //     playRender.material.mainTexture = editTexture;
        //     thumbnailRenderer.material.mainTexture = editTexture;
        // }
    }

    public Renderer GetThumbnailObject()
    {
        return player.thumbnailObject;
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref oldColor);
    }

    private void ShowOrHideLinkBtn(bool isShow)
    {
        if (youtubeLinkBtn)
        {
            youtubeLinkBtn.SetActive(isShow);
        }
    }

    #region Video Control And Callbacks

    private void InitYoutubePlayer()
    {
        player.videoQuality = YoutubeSettings.YoutubeVideoQuality.STANDARD;
        player.videoPlayer.isLooping = true;
        youtubeVideoEvents.OnYoutubeUrlAreReady.AddListener(OnUrlLoadReady);
        youtubeVideoEvents.OnVideoReadyToStart.AddListener(OnVideoReadyToStart);
        youtubeVideoEvents.OnVideoLoadError.AddListener(OnVideoLoadError);
        youtubeVideoEvents.OnUrlError.AddListener(OnUrlError);
        //other settings in future ...
    }

    private void OnUrlLoadReady(string url)
    {
        LoggerUtils.Log($"[Video]-->OnUrlLoadReady-->{url}");
        SetVideoStatus(VideoLoadStatus.UrlReady);
        //加载完成显示范围光圈
        if (VideoNodeManager.Inst.currentSelectVideo == gameObject)
        {
            VideoNodeManager.Inst.SetRangeEffectVisible(true);
            VideoNodeManager.Inst.SetRangeEffectSize(entity.Get<VideoNodeComponent>().soundRange);
            VideoNodeManager.Inst.SetRangeEffectPos(gameObject);
        }
    }

    private void OnUrlError()
    {
        LoggerUtils.Log($"[Video]-->OnUrlError");
        SetVideoStatus(VideoLoadStatus.Empty);
        ShowOrHideLinkBtn(false);
        if (GlobalFieldController.CurGameMode == GameMode.Edit)
        {
            TipPanel.ShowToast(VideoNodeManager.Inst.URL_PARSE_ERROR_TIP);
        }
    }

    private void OnVideoReadyToStart()
    {
        LoggerUtils.Log($"[Video]-->OnVideoReadyToStart");
        SetVideoStatus(VideoLoadStatus.ReadyToPlay);
        if (GlobalFieldController.CurGameMode == GameMode.Play || GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            ShowOrHideLinkBtn(true);
            player.Play();
            RefreshPlayUI();
        }
    }

    private void OnVideoLoadError()
    {
        //TODO:输入错误URL时，会一直Loading而不返回VideoLoadError
        LoggerUtils.Log($"[Video]-->OnVideoLoadError");

        if (GlobalFieldController.CurGameMode == GameMode.Edit)
        {
            if (curRetryTimes < VideoNodeManager.RetryTipTime)
            {
                TipPanel.ShowToast(VideoNodeManager.Inst.LOADING_FAILED_TIP);
                curRetryTimes++;
            }
        }

        SetVideoStatus(VideoLoadStatus.Empty);
        ShowOrHideLinkBtn(false);
    }

    #endregion

    #region Call outside...

    public void OnChangeMode(GameMode mode)
    {
        LoggerUtils.Log($"[Video]-->OnChangeMode==>{mode}==>{entity.Get<VideoNodeComponent>().videoUrl}");

        StopPlayVideo();
        SwitchRenderTexture();
        var vUrl = entity.Get<VideoNodeComponent>().videoUrl;
        if (string.IsNullOrEmpty(vUrl)) return;

        curRetryTimes = 0;
        if (mode == GameMode.Edit)
        {
            //编辑模式只恢复视频封面，不加载
            if (player) player.EnableThumbnailObject();
            ShowOrHideLinkBtn(false);
            if (VideoNodePanel.Instance)
            {
                VideoNodePanel.Instance.RefreshPanel();
            }
        }
        else if (mode == GameMode.Play || mode == GameMode.Guest)
        {
            //试玩模式 加载并prepare视频
            if (VideoNodeManager.Inst.isFinishEnterRoom)
            {
                StartLoadVideo();
            }
        }
    }

    public void StartLoadVideoUrl(string url)
    {
        LoggerUtils.Log($"[Video]-->StartLoadVideoUrl==>{url}");

        entity.Get<VideoNodeComponent>().videoUrl = url;
        if (string.IsNullOrEmpty(entity.Get<VideoNodeComponent>().videoUrl)) return;
        if (!gameObject.activeInHierarchy) return;

        curRetryTimes = 0;
        StopPlayVideo();
        SetVideoStatus(VideoLoadStatus.UrlLoading); //TODO:输入正确格式但是错误Url时，插件会一直重试
        player.LoadUrl(entity.Get<VideoNodeComponent>().videoUrl);
    }

    public void StartLoadVideo()
    {
        if (entity == null) return;
        LoggerUtils.Log($"[Video]-->StartLoadVideo==>{entity.Get<VideoNodeComponent>().videoUrl}");
        if (string.IsNullOrEmpty(entity.Get<VideoNodeComponent>().videoUrl)) return;

        StopPlayVideo();
        player.loadYoutubeUrlsOnly = false;
        player.PreLoadVideo(entity.Get<VideoNodeComponent>().videoUrl);
        SetVideoStatus(VideoLoadStatus.PreLoading);
    }

    public void StopPlayVideo()
    {
        LoggerUtils.Log($"[Video]-->StopPlayVideo");
        if (player)
        {
            player.Stop();
            player.DisableThumbnailObject();
            player.StopAllCoroutines();
            curRetryTimes = 0;
        }
        //刷新模型UI
        CloseCtrlPanel();
        //全屏时停止
        if (videoPlayer.renderMode == VideoRenderMode.CameraNearPlane)
        {
            videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            //退出全屏面板
            VideoFullPanel.Hide();
        }
    }

    public void PlayPauseVideo()
    {
        LoggerUtils.Log($"[Video]-->PlayPauseVideo");
        if (player)
        {
            player.PlayPause();
            RefreshPlayUI();
        }
        if (VideoFullPanel.Instance)
        {
            VideoFullPanel.Instance.RefreshPlayBtn();
        }
    }

    public bool GetPauseState()
    {
        return player.pauseCalled;
    }

    public void SetAudioMute(bool isMute)
    {
        aSource.mute = isMute;
    }

    public void ForceSound2D(bool isForce)
    {
        aSource.spatialBlend = isForce ? 0 : (entity.Get<VideoNodeComponent>().soundRange == (int)VideoSoundRange.Infinite ? 0 : 1);
    }

    #endregion

    #region UI Control

    public void ChangeCtrlPanelVisible(bool isShow)
    {
        if (currentStatus == VideoLoadStatus.ReadyToPlay)
        {
            controlPanel.SetActive(isShow);
            AutoHideAfterAction();
        }
    }

    private void AutoHideAfterAction()
    {
        //3s自动隐藏
        CancelInvoke("CloseCtrlPanel");
        if (controlPanel.activeInHierarchy)
        {
            Invoke("CloseCtrlPanel", 3);
        }
    }

    private void CloseCtrlPanel()
    {
        controlPanel.SetActive(false);
    }

    private void RefreshPlayUI()
    {
        //刷新模型UI
        iconPlay.SetActive(GetPauseState());
        iconPause.SetActive(!GetPauseState());
    }

    public void SwitchFullScreen(Action action)
    {
        BlackPanel.Show();
        BlackPanel.Instance.PlayTransitionAnimAct(() =>
        {
            if (GetPauseState())
            {
                StartCoroutine("SwitchFullScreenWhenPause");
            }
            else
            {
                videoPlayer.renderMode = videoPlayer.renderMode == VideoRenderMode.MaterialOverride ?
                VideoRenderMode.CameraNearPlane : VideoRenderMode.MaterialOverride;
            }
            action.Invoke();
            BlackPanel.Instance.transform.SetAsLastSibling();
        });
    }

    private IEnumerator SwitchFullScreenWhenPause()
    {
        PlayPauseVideo();
        videoPlayer.renderMode = videoPlayer.renderMode == VideoRenderMode.MaterialOverride ?
        VideoRenderMode.CameraNearPlane : VideoRenderMode.MaterialOverride;
        //暂停状态全屏特殊处理
        yield return new WaitForSeconds(0.04f);
        PlayPauseVideo();
    }

    public void ShowVideoLinkPage()
    {
        var link = entity.Get<VideoNodeComponent>().videoUrl;
        LoggerUtils.Log($"[Video]-->ShowVideoLinkPage-->{link}");

        bool needResume = false;
        if (!string.IsNullOrEmpty(link))
        {
            H5Params param = new H5Params()
            {
                url = link
            };
            var jsonStr = JsonConvert.SerializeObject(param);
            MobileInterface.Instance.AddClientRespose(MobileInterface.openLandH5Page, (ret) =>
            {
                LoggerUtils.Log($"[Video]-->OnH5Callback-->{ret}");
                if (needResume)
                {
                    //恢复播放视频
                    PlayPauseVideo();
                }
            });
            MobileInterface.Instance.OpenLandH5Page(jsonStr);
            if (!GetPauseState())
            {
                //暂停视频
                PlayPauseVideo();
                needResume = true;
            }
        }
    }

    public void ControlMainVideo(VideoScreenIcon controlType)
    {
        switch (controlType)
        {
            case VideoScreenIcon.PlayRender:
                ChangeCtrlPanelVisible(!controlPanel.activeInHierarchy);
                break;
            case VideoScreenIcon.ic_play:
            case VideoScreenIcon.ic_pause:
                PlayPauseVideo();
                AutoHideAfterAction();
                break;
            case VideoScreenIcon.ic_fullscreen:
                SwitchFullScreen(() =>
                {
                    //打开全屏面板
                    VideoFullPanel.Show();
                    VideoFullPanel.Instance.SetEntity(entity);
                });
                break;
            case VideoScreenIcon.Video_link:
                ShowVideoLinkPage();
                break;
        }
    }

    public void SetAudioSourceVolume(float v)
    {
        aSource.volume = v;
    }

    #endregion
}
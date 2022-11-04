using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description:视频道具管理
/// Date: 2022-3-30 19:43:08
/// </summary>
public class VideoNodeManager : ManagerInstance<VideoNodeManager>, IManager
{
    #region TOAST

    public string MAX_NUM_TIP = "Only {0} video players can be added in the experience.";
    public string URL_PARSE_ERROR_TIP = "Only supports YouTube video links. Live and Shorts are not supported.";
    public string LOADING_FAILED_TIP = "Loading failed...please try again.";

    #endregion

    public const int MAX_COUNT = 5;
    public const int RetryTipTime = 1;
    public int CurrentNum = 0;

    public List<VideoNodeBehaviour> videoBehavs = new List<VideoNodeBehaviour>();
    public GameObject currentSelectVideo;

    public bool isFinishEnterRoom = false; //是否完成进房(关闭第一帧)

    private GameObject rangeEffectBuilder;
    private GameObject soundRange;
    private const float orgEffectSize = 43; //默认特效大小
    private const float inifEffectSize = 2027; //全局特效大小
    public float videoVolume;

    public void Init()
    {
        ShowHideManager.Inst.afterSwitchClick += OnSwitchClicked;
        SensorBoxManager.Inst.afterSwitchClick += OnSwitchClicked;
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        LoggerUtils.Log($"[Video]-->OnChangeMode==>Init");
    }

    public override void Release()
    {
        base.Release();
        Clear();
        if (ShowHideManager.Inst.afterSwitchClick != null)
        {
            ShowHideManager.Inst.afterSwitchClick -= OnSwitchClicked;
        }
        LoggerUtils.Log($"[Video]-->OnChangeMode==>Release");
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    private void OnChangeMode(GameMode mode)
    {
        LoggerUtils.Log($"[Video]-->Manager OnChangeMode----->{mode}, videoBehavs:{videoBehavs.Count}");

        foreach (var vb in videoBehavs)
        {
            vb.OnChangeMode(mode);
        }
    }

    public bool IsOverMaxCount()
    {
        return CurrentNum >= MAX_COUNT;
    }

    public bool IsCanClone(int count)
    {
        if (CurrentNum + count > MAX_COUNT)
        {
            return false;
        }

        return true;
    }

    public void AddVideoNode(VideoNodeBehaviour behaviour)
    {
        if (videoBehavs.Contains(behaviour))
        {
            LoggerUtils.LogError("AddVideoNode --> videoBehavs contains this");
            return;
        }
        CurrentNum++;
        videoBehavs.Add(behaviour);
        LoggerUtils.Log($"[Video]-->AddVideoNode==>videoBehavs:{videoBehavs.Count}");
    }

    public void StartAllVideoPlay()
    {
        isFinishEnterRoom = true;
        foreach (var vBev in videoBehavs)
        {
            if (vBev != null)
            {
                vBev.StartLoadVideo();
            }
        }
    }

    public void StartLoadAllVideoUrl()
    {
        isFinishEnterRoom = true;
        foreach (var vBev in videoBehavs)
        {
            if (vBev != null)
            {
                vBev.StartLoadVideoUrl(vBev.entity.Get<VideoNodeComponent>().videoUrl);
            }
        }
    }

    public void StopAllVideoPlay()
    {
        foreach (var vBev in videoBehavs)
        {
            if (vBev != null)
            {
                vBev.StopPlayVideo();
            }
        }
    }

    public void AllVideoPlayPause()
    {
        foreach (var vBev in videoBehavs)
        {
            if (vBev != null)
            {
                vBev.PlayPauseVideo();
            }
        }
    }

    //从传送门返回编辑模式
    public IEnumerator WaitStartLoadAllVideoUrl()
    {
        yield return null;
        foreach (var vBev in videoBehavs)
        {
            if (vBev != null)
            {
                vBev.StartLoadVideoUrl(vBev.entity.Get<VideoNodeComponent>().videoUrl);
            }
        }
    }

    public void OnSwitchClicked(GameObject go)
    {
        // 2022-3-25 15:44:12 通过Behavior显隐监听实现，避免在其他模块频繁添加回调
        
        // //视频道具控制,被隐藏后VideoPlayer会清空，无法暂停继续播放，只能重新开始播放
        // var videoNodes = go.GetComponentsInChildren<VideoNodeBehaviour>(true);
        // var isShow = go.activeInHierarchy;
        // LoggerUtils.Log("VideoNodeManager--->OnSwitchClicked--->" + isShow);
        //
        // foreach (var videoNode in videoNodes)
        // {
        //     if (videoNode && videoBehavs.Contains(videoNode))
        //     {
        //         if (isShow)
        //         {
        //             videoNode.StartLoadVideo();
        //         }
        //         else
        //         {
        //             videoNode.StopPlayVideo();
        //         }
        //     }
        // }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        // LoggerUtils.Log("VideoNodeManager--->RemoveNode--->" + CurrentNum);
        if (behaviour is VideoNodeBehaviour v)
        {
            if (CurrentNum > 0) CurrentNum--;
            if (videoBehavs.Contains(v))
            {
                videoBehavs.Remove(v);

                v.entity.Get<VideoNodeComponent>().videoUrl = string.Empty;
                v.StopPlayVideo();
                v.ClearVideoInfo();
            }
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour is VideoNodeBehaviour v)
        {
            if (!videoBehavs.Contains(v))
            {
                videoBehavs.Add(v);
                CurrentNum++;
            }
        }
        
        if (VideoNodePanel.Instance)
        {
            VideoNodePanel.Instance.SetEntity(behaviour.entity);
        }
    }

    public void Clear()
    {
        CurrentNum = 0;
        videoBehavs?.Clear();
        LoggerUtils.Log("VideoNodeManager--->Clear()");
    }

    public void OnHitScreen(GameObject hitGo)
    {
        string hitName = hitGo.name;

        VideoScreenIcon controlType;
        bool result = Enum.TryParse(hitName, out controlType);
        if (result)
        {
            if (GlobalFieldController.CurGameMode == GameMode.Guest
                || GlobalFieldController.CurGameMode == GameMode.Play)
            {
                var videoBev = hitGo.GetComponentInParent<VideoNodeBehaviour>();
                if (videoBev != null)
                {
                    videoBev.ControlMainVideo(controlType);
                }
            }
        }
    }

    public void OnOpenCloseFull(VideoNodeBehaviour cVideo, bool isOpen)
    {
        foreach (var vBev in videoBehavs)
        {
            if (vBev != null && vBev != cVideo)
            {
                vBev.SetAudioMute(isOpen);
            }
        }
        cVideo.ForceSound2D(isOpen);
    }

    #region Range Effect
    public void OnSelectNode(GameObject currentVideo)
    {
        var behav = currentVideo.GetComponent<VideoNodeBehaviour>();
        if (behav == null)
        {
            currentSelectVideo = null;
            return;
        }
        //记录当前选中Video
        currentSelectVideo = currentVideo;
        if (behav.currentStatus == VideoLoadStatus.UrlReady || behav.currentStatus == VideoLoadStatus.PreLoading || behav.currentStatus == VideoLoadStatus.ReadyToPlay)
        {
            SetRangeEffectVisible(true);
            SetRangeEffectSize(behav.entity.Get<VideoNodeComponent>().soundRange);
            SetRangeEffectPos(currentVideo);
        }
    }

    public void OnDisSelectNode(GameObject currentVideo)
    {
        if (currentVideo != null && currentVideo.GetComponent<VideoNodeBehaviour>())
        {
            SetRangeEffectVisible(false);
            currentSelectVideo = null;
        }
    }

    public void SetRangeEffectVisible(bool isVisible)
    {
        if (isVisible && rangeEffectBuilder == null)
        {
            rangeEffectBuilder = new GameObject("RangeEffectBuilder");
            var assetPrefab = ResManager.Inst.LoadResNoCache<GameObject>("Prefabs/UI/Effect/SoundRange");
            soundRange = GameObject.Instantiate(assetPrefab, rangeEffectBuilder.transform);
            soundRange.name = assetPrefab.name;
        }
        if (soundRange == null) return;
        soundRange.SetActive(isVisible);
    }

    public void SetRangeEffectSize(int range)
    {
        var scale = Vector3.one;
        scale.x = range == (int)VideoSoundRange.Infinite ? inifEffectSize : orgEffectSize * Mathf.Pow(2, range);
        scale.z = scale.x;
        if (soundRange == null) return;
        soundRange.transform.localScale = scale;
    }

    public void SetRangeEffectPos(GameObject target)
    {
        if (soundRange != null && soundRange.activeInHierarchy && target.GetComponent<VideoNodeBehaviour>())
        {
            soundRange.transform.position = target.transform.position;
        }
    }

    public void SetVideoVolume(float volume)
    {
        videoVolume = volume;
        foreach (var video in videoBehavs)
        {
            video.SetAudioSourceVolume(volume);
        }
    }
    #endregion
}
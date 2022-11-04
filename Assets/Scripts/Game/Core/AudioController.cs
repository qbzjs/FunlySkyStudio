using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum MoveAudioState
{
    None,
    Jump,
    Move,
    Ground,
    Fly,
}

public class BGMConfig
{
    public static Dictionary<int, int> tempBgmDict = new Dictionary<int, int>()
    {
        { 0,1001},//3D Empty
        { 1,1002},//Night
        { 2,1003},
        { 3,1004},
        { 4,1005},
        { 5,1006},
        { 6,1007},
    };
}


public class AudioController : BMonoBehaviour<AudioController>
{
    public float deltaTime = 0.26f;
    private int groundLength = 3;
    private int maxAudioLength = 8;
    private Action bgComplete;
    public MoveAudioState audioState = MoveAudioState.None;
    private string playingMusicName = "";
    private GameObject ambientNode;

    private Coroutine loadBgAudioCoroutine;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        ambientNode = new GameObject("ambientNode");
        ambientNode.transform.parent = transform;
    }

    public void StopStepAudio()
    {
        // CancelInvoke("PlayStepAudio");
        // for (int i = 0; i < aSource.Length - 3; i++)
        // {
        //     aSource[i].Stop();
        // }
    }

    public void CloseAudio(bool isToogle)
    {
        // for (int i = 0; i < aSource.Length - 1; i++)
        // {
        //     aSource[i].enabled = isToogle;
        // }
    }
    public void PlayJumpAudio()
    {
        AKSoundManager.Inst.PostEvent("play_default_jump", PlayerBaseControl.Inst.gameObject);
    }

    public void PlayFlyAudio()
    {
        AKSoundManager.Inst.flySound(PlayerBaseControl.Inst.gameObject, true);
    }

    public void PlayShotAudio()
    {
        AKSoundManager.Inst.PostEvent("play_screenshot", PlayerBaseControl.Inst.gameObject);
    }

    public void StopFlyAudio()
    {
        AKSoundManager.Inst.flySound(PlayerBaseControl.Inst.gameObject, false);
    }

    private void PlayComplete()
    {
        bgComplete?.Invoke();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        AudioListener.pause = pauseStatus;
    }

    public void PlayerBGAudio(bool isLoop, int musicId, Action complete)
    {
        AkSoundEngine.StopAll(this.gameObject);
        var bgmData = GameManager.Inst.bgmConfigDataDics[musicId];
        string musicEventName = bgmData.wwiseName;
        string bgmEvent = "play_music";
        string bgmSwitch = "play_bgm";
        if (isLoop)
        {
            bgmEvent = "play_music_loop";
            bgmSwitch = "play_bgm_loop";
        }

        AkSoundEngine.SetSwitch(bgmSwitch, musicEventName, this.gameObject);
        AkSoundEngine.PostEvent(bgmEvent, this.gameObject, (uint)AkCallbackType.AK_EndOfEvent, OnMusicEndCallBack, musicEventName);
        playingMusicName = musicEventName;
        bgComplete = complete;
    }

    public void LoadStreamBGAudio(string url, UnityAction<AudioClip> onLoaded, UnityAction onFailure, UnityAction<AudioClip> onFinish)
    {
        StopStreamBGAudio();
        LoggerUtils.Log("LoadStreamBGAudio:" + url);
        
        loadBgAudioCoroutine = CoroutineManager.Inst.StartCoroutine(StreamAudioLoader.Inst.LoadAudio(url,
            tmpClip =>
            {
                LoggerUtils.Log("onLoaded");
                onLoaded?.Invoke(tmpClip);
                
            }, () =>
            {
                LoggerUtils.Log("onFailure");
                loadBgAudioCoroutine = null;
                onFailure?.Invoke();
            }, audioClip =>
            {
                LoggerUtils.Log("onFinish");
                loadBgAudioCoroutine = null;
                audioClip.name = url.Split('\\', '/').LastOrDefault() ?? "ugcAudio.mp3";
                onFinish?.Invoke(audioClip);
            }));
    }

    private int ambientId;
    //正在播放特殊白噪音时不可播放
    private bool isPlaying;
    public void PlayAmbientMusic()
    {
        PlayAmbientMusic(ambientId);
    }
    public void PlayAmbientMusic(int ambientId)
    {
        
        if (ambientId == 0)
        {
            return;
        }
        string eventName = "Play_white_noise_day";
        if (GameConsts.ambientEventDict.ContainsKey(ambientId))
        {
            eventName = GameConsts.ambientEventDict[ambientId];
            this.ambientId = ambientId;
        }
        if (isPlaying)
        {
            return;
        }
        StopAmbientMusic();
        AkSoundEngine.PostEvent(eventName, ambientNode);
    }

    public void PlayWaterAmbientMusic()
    {
        StopAmbientMusic();
        AkSoundEngine.PostEvent("Play_White_noise_underwater", ambientNode);
        isPlaying = true;


    }

    private float currentVolume = 100;

    public void SetBGAudioVolume(float value)
    {
        float v = Mathf.Min(value, 100);
        v = Mathf.Max(v, 0);
        //调整总音量大小(0 - 100)：0 - 静音 ；100 - 默认大小
        AkSoundEngine.SetRTPCValue("Master_Volume", v);
        currentVolume = v;
    }

    //BGM 音量设置
    public void SetBGMAudioVolume(float value)
    {
        float v = Mathf.Min(value, 100);
        v = Mathf.Max(v, 0);
        //调整音量大小(0 - 100)：0 - 静音
        AkSoundEngine.SetRTPCValue("Music_Volume", v);
    }

    //游戏音效音量设置
    public void SetSFXAudioVolume(float value)
    {
        float v = Mathf.Min(value, 100);
        v = Mathf.Max(v, 0);
        //调整音量大小(0 - 100)：0 - 静音
        AkSoundEngine.SetRTPCValue("SFX_Volume", v);
    }

    public float GetBGAudioVolume()
    {
        //获取当前总音量大小(0 - 100)
        return currentVolume;
    }

    public void PlayDowntownBGM()
    {
        AkSoundEngine.StopAll(this.gameObject);
        AKSoundManager.Inst.PlayAttackSound("bgm_great_snowfield", "play_music_loop", "play_bgm_loop", gameObject);
    }

    public void StopDowntownBGM()
    {
        AkSoundEngine.StopAll(this.gameObject);
    }

    public void StopAmbientMusic()
    {
        isPlaying = false;
        AkSoundEngine.StopAll(ambientNode);
    }

    public void StopStreamBGAudio()
    {
        LoggerUtils.Log("StopStreamBGAudio");
        if (loadBgAudioCoroutine == null) return;
        CoroutineManager.Inst.StopCoroutine(loadBgAudioCoroutine);
        loadBgAudioCoroutine = null;
    }

    public void StopBGAudio()
    {
        AkSoundEngine.StopAll(this.gameObject);
    }

    private void OnMusicEndCallBack(object in_cookie, AkCallbackType in_type, object in_info)
    {
        if (in_cookie.ToString() == playingMusicName)
        {
            bgComplete?.Invoke();
        }
    }
}

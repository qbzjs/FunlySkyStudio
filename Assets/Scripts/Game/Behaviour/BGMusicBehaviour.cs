using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class BGMusicBehaviour : NodeBaseBehaviour
{
    public bool isLoading = false;
    private AudioSource aSource;
    private string nowMusicUrl;
    private enum GameState {
        Play,
        Stop
    }
    private GameState pState = GameState.Stop;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        aSource = this.GetComponent<AudioSource>();
    }

    public void SetAudio(AudioClip clip)
    {
        LoggerUtils.Log("BGMusicBehaviour SetAudio:" + clip);
        aSource.clip = clip;
    }
    public void SetAudioVolume(float level)
    {
        aSource.volume = level;
    }
    public void SetAudioMute(bool isMute)
    {
        aSource.mute = isMute;
    }
    public bool GetAudioMuteState()
    {
        return aSource.mute;
    }
    public void LoadOuterMusic()
    {
        string url = entity.Get<BGMusicComponent>().bgUrl;
        if (!string.IsNullOrEmpty(url))
        {
            url = url.Replace("https://buddy-app-bucket.s3-accelerate.amazonaws.com/", "https://cdn.joinbudapp.com/");
            url = url.Replace("https://buddy-app-bucket.s3.us-west-1.amazonaws.com/", "https://cdn.joinbudapp.com/");
            isLoading = true;
            if (url.StartsWith("https://") || url.StartsWith("http://"))
            {
                if (nowMusicUrl == url && aSource.clip != null)
                {
                    GetAudioClipSuccess(aSource.clip);
                }
                else {
                    nowMusicUrl = null;
                    aSource.clip = null;
                    AudioController.Inst.LoadStreamBGAudio(url, GetAudioClipSliceLoaded, GetAudioClipFail, GetAudioClipSuccess);
                }
            }
            else
            {
                StartCoroutine(ResManager.Inst.GetAudioClip(url, GetAudioClipSuccess, GetAudioClipFail));
            }
        }
    }
    
    /// <summary>
    /// 流式加载中一部分音频被加载成功
    /// </summary>
    /// <param name="clip"></param>
    private void GetAudioClipSliceLoaded(AudioClip clip)
    {
        AudioLoadingPanel.Hide();
        var audioType = entity.Get<BGMusicComponent>().musicType;
        aSource.clip = clip;
        if (pState == GameState.Play && audioType == 0)
        {
            aSource.time = 0;
            Play(GlobalFieldController.CurGameMode != GameMode.Edit);
        }
    }

    private void GetAudioClipSuccess(AudioClip clip)
    {
        nowMusicUrl = entity.Get<BGMusicComponent>().bgUrl;
        isLoading = false;
        AudioLoadingPanel.Hide();
        var audioType = entity.Get<BGMusicComponent>().musicType;
        float timer = 0;
        if (aSource.clip != null)
        {
            timer = aSource.time;
        }
        aSource.clip = clip;
        if (pState == GameState.Play && audioType == 0)
        {
            aSource.time = timer;
            Play(GlobalFieldController.CurGameMode != GameMode.Edit);
        }
    }

    private void GetAudioClipFail()
    {
        nowMusicUrl = null;
        aSource.clip = null;
        isLoading = false;
        AudioLoadingPanel.Hide();
        LoggerUtils.Log("Oops! Something Wrong: (");
    }

    public void Play(bool isLoop, Action callBack = null)
    {
        pState = GameState.Play;
        var comp = entity.Get<BGMusicComponent>();
        if (comp.musicType == 0)
        {
            aSource.loop = isLoop;
            if (aSource.clip != null)
            {
                if (GlobalFieldController.CurGameMode == GameMode.Edit)
                {
                    aSource.time = 0;
                }
                aSource.Play();
            }
            else
            {
                if (isLoading || string.IsNullOrEmpty(comp.bgUrl) || !comp.bgUrl.StartsWith("https://") && !comp.bgUrl.StartsWith("http://") ) return;
                isLoading = true;
                AudioController.Inst.LoadStreamBGAudio(comp.bgUrl, GetAudioClipSliceLoaded, GetAudioClipFail, GetAudioClipSuccess);
                LoggerUtils.Log("Play:" + JsonConvert.SerializeObject(comp) );
            }
        }
        else if (comp.musicType == 2)
        {
            AudioController.Inst.PlayerBGAudio(isLoop, comp.musicId, callBack);
        }
    }

    public void PlayEnr()
    {
        var comp = entity.Get<BGEnrMusicComponent>();
        AudioController.Inst.PlayAmbientMusic(comp.enrMusicId);
    }

    public void StopEnr()
    {
        AudioController.Inst.StopAmbientMusic();
    }

    public void Stop()
    {

        
        pState = GameState.Stop;
        aSource.Stop();
        if (AudioController.Inst != null)
        {
            AudioController.Inst.StopBGAudio();
            AudioController.Inst.StopStreamBGAudio();
        }

        
        var comp = entity.Get<BGMusicComponent>();
        if (!isLoading || string.IsNullOrEmpty(comp.bgUrl) || !comp.bgUrl.StartsWith("https://") && !comp.bgUrl.StartsWith("http://"))
            return;
        nowMusicUrl = null;
        aSource.clip = null;
        isLoading = false;
    }
}

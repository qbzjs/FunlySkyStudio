/// <summary>
/// Author:MeiMei—LiMei
/// Description:音效道具Behavior
/// Date: 2022-01-13
/// </summary>
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SoundButtonBehaviour : NodeBaseBehaviour
{
    public bool isLoading = false;
    private AudioSource[] arrayMusic;
    public AudioSource importASource;//导入的音效
    private Animator mAnimator;

    private Color[] oldColor;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        arrayMusic = gameObject.GetComponentsInChildren<AudioSource>();
        importASource = arrayMusic[1];
        mAnimator = this.GetComponentInChildren<Animator>();
        mAnimator.Play("Inacbtn", 0, 0);
        RefreshButtonCanTouch(true);
        SetSoundVolume(SoundManager.Inst.soundVolume);
    }

    public void RefreshButtonCanTouch(bool state)
    {
        string layer = state ? "Touch" : "Model";
        mAnimator.gameObject.layer = LayerMask.NameToLayer(layer);
    }
    public void SetAudio(AudioClip clip)//导入的音效
    {
        importASource.clip = clip;
    }
    public void LoadOuterMusic()
    {
        string url = entity.Get<SoundComponent>().soundUrl;
        if (!string.IsNullOrEmpty(url))
        {
            isLoading = true;
            CoroutineManager.Inst.StartCoroutine(ResManager.Inst.GetAudioClip(url, GetAudioClipSuccess, GetAudioClipFail));
        }
    }
    private void GetAudioClipSuccess(AudioClip clip)
    {
        isLoading = false;
        importASource.clip = clip;
        LoggerUtils.Log("GetAudioClipSuccess");
    }
    private void GetAudioClipFail()
    {
        importASource.clip = null;
        isLoading = false;
        LoggerUtils.Log("Oops! Something Wrong: (");
    }
    public void Play2DAudio()
    {
        importASource.spatialBlend = 0.0f;
        importASource.Play();
    }
    public override void OnRayEnter()//试玩模式声音按钮显示
    {
        HighLight(true);
        var cmp = entity.Get<SoundComponent>();
        PortalPlayPanel.Show();
        PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Sound);
        PortalPlayPanel.Instance.AddButtonClick(OnClickSound);
        PortalPlayPanel.Instance.SetTransform(transform);
    }
    public override void OnRayExit()
    {
        HighLight(false);
        PortalPlayPanel.Hide();
    }
    public void OnClickSound()//游玩模式下点击音效按钮
    {
        mAnimator.Play("Inacbtn", 0, 0);
        var comp = entity.Get<SoundComponent>();
        if (comp.musicType==musicType.noMusic)
        {
            AKSoundManager.Inst.PostEvent("play_button", gameObject);
            return;
        }
        if (importASource.clip == null&&isLoading==false)
        {
            LoadOuterMusic();
            if (isLoading == true)
            {
                TipPanel.ShowToast("Audio is still downloading");
            }
        }
        else
        {
            importASource.spatialBlend = 1.0f;
            importASource.Play();
        }
    }
    public void Stop()
    {
        isLoading = false;
        importASource.Stop();
    }
    public override void HighLight(bool isHigh)//高亮
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref oldColor);
    }

    public void SetSoundVolume(float volume)
    {
        importASource.volume = volume;
    }
}

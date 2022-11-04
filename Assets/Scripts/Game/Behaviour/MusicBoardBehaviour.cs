using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: 熊昭
/// Description: 音乐板道具行为功能类
/// Date: 2021-12-03 14:51:34
/// </summary>
public class MusicBoardBehaviour : NodeBaseBehaviour
{
    private static MaterialPropertyBlock mpb;
    private Color[] oldColor;
    private List<Renderer> blockRenders = new List<Renderer>();

    private bool isPlaying = false;
    private float playTime = 1.672f;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }

        var musMod = this.transform.GetChild(0);
        for (int i = 0; i < musMod.childCount; i++)
        {
            var block = musMod.GetChild(i).gameObject;
            blockRenders.Add(block.GetComponent<Renderer>());
        }
        //初始化复位高亮状态
        CloseEmission();
    }

    public void SetColorInit()
    {
        MusicBoardComponent mComp = entity.Get<MusicBoardComponent>();
        for (int i = 0; i < mComp.audioIDs.Length; i++)
        {
            SetColor(i, mComp.audioIDs[i]);
        }
    }

    public void SetColor(int area, int audioID)
    {
        MusicBoardData data = GameManager.Inst.musicBoardDatas[audioID];
        var dColor = DataUtils.DeSerializeColorByHex(data.darkColor);
        blockRenders[area].GetPropertyBlock(mpb);
        mpb.SetColor("_Color", dColor);
        blockRenders[area].SetPropertyBlock(mpb);
    }

    private void SetEmissionColor(int area, int audioID)
    {
        MusicBoardData data = GameManager.Inst.musicBoardDatas[audioID];
        var lColor = DataUtils.DeSerializeColorByHex(data.lightColor);
        blockRenders[area].GetPropertyBlock(mpb);
        mpb.SetColor("_EmissionColor", lColor);
        blockRenders[area].SetPropertyBlock(mpb);
    }

    public override void OnColliderHit()
    {
        base.OnColliderHit();

        //if (!isWiseControl)
        //{
        //    for (int i = 0; i < blockCount; i++)
        //    {
        //        if (blockAudios[i].clip == null)
        //            continue;
        //        if (!blockAudios[i].isPlaying)
        //            blockAudios[i].Play();
        //        blockRenders[i].material.EnableKeyword("_EMISSION");
        //    }
        //    Invoke("CloseEmission", 0.5f);
        //}

        int[] audioNums = entity.Get<MusicBoardComponent>().audioIDs;

        //if all audio numbers equal to 0
        if (Array.FindIndex(audioNums, a => a > 0) == -1)
            return;

        for (int i = 0; i < audioNums.Length; i++)
        {
            if (audioNums[i] == 0)
                continue;
            SetEmissionColor(i, audioNums[i]);
            blockRenders[i].material.EnableKeyword("_EMISSION");
        }
        Invoke("CloseEmission", 0.5f);

        if (isPlaying)
            return;

        PlayWiseAudio();
        StartCoroutine(PlayAudioTimer());
    }

    public void PlayWiseAudio()
    {
        int[] audioNums = entity.Get<MusicBoardComponent>().audioIDs;
        for (int i = 0; i < audioNums.Length; i++)
        {
            if (audioNums[i] != 0)
            {
                string boardEvent = GameManager.Inst.musicBoardDatas[audioNums[i]].wiseEvent;
                AkSoundEngine.PostEvent(boardEvent, blockRenders[i].gameObject);
                LoggerUtils.Log("MusicBoard：" + i + " -- " + boardEvent);
            }
        }
    }

    private IEnumerator PlayAudioTimer()
    {
        isPlaying = true;
        //timer start
        yield return new WaitForSeconds(playTime);
        isPlaying = false;
    }

    private void CloseEmission()
    {
        var oColor = new Color(0, 0, 0);
        for (int i = 0; i < blockRenders.Count; i++)
        {
            blockRenders[i].GetPropertyBlock(mpb);
            mpb.SetColor("_EmissionColor", oColor);
            blockRenders[i].SetPropertyBlock(mpb);
        }
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLight(isHigh, mpb, ref oldColor, blockRenders.ToArray());
    }
}
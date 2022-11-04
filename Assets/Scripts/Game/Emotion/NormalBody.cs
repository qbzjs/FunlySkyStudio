using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
public class NormalBody : BaseBody
{

    public override void OnStop()
    {
        base.OnStop();
        animCon.OnEmoKill();
    }
    protected IEnumerator PlayMove()
    {
        if (nextEmoInfo != null)
        {
            yield return new WaitForSeconds(emoIconData.moveEndTime);
            BodyArgs bodyArgs = new BodyArgs(animCon, nextEmoInfo);
            OnPlay(bodyArgs);
        }
        else
        {
            yield return new WaitForSeconds(emoIconData.moveEndTime - 0.15f);
            anim.CrossFadeInFixedTime("idle", 0.15f);
            yield return new WaitForSeconds(0.15f);
            OnStop();
        }

    }
    protected IEnumerator PlayLoopMove()
    {
        yield return new WaitForSeconds(emoIconData.moveEndTime);
        anim.Play(emoIconData.name, 0, 0f);
        PlayLoopEmo();
    }

    public override void PlayLoopEmo()
    {

        anim.Play(emoIconData.name, 0, 0f);
        PlayingMove = CoroutineManager.Inst.StartCoroutine(PlayLoopMove());
    }

    public override void PlayEmo()
    {
        if (emoIconData.isAbRes)
        {
            emoIconData.moveEndTime = animCon.PlayAni(emoIconData.name, 0, 0f);
        }
        else
        {
            anim.Play(emoIconData.name, 0, 0f);
        }

        PlayingMove = CoroutineManager.Inst.StartCoroutine(PlayMove());
    }


}

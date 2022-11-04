using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class SpecialBody : BaseBody
{
    public Action<string> specialAct;
    public string playerId;

    public override void OnStop()
    {
        base.OnStop();
        animCon.OnEmoKill();
    }

    public override void PlayEmo()
    {
        anim.Play(emoIconData.name, 0, 0f);
        PlayingMove = CoroutineManager.Inst.StartCoroutine(PlaySpecialMove());
    }

    protected IEnumerator PlaySpecialMove()
    {
        yield return new WaitForSeconds(emoIconData.moveEndTime - 0.15f);
#if UNITY_EDITOR
        if(GlobalFieldController.CurGameMode == GameMode.Play)
        {
            playerId = TestNetParams.testHeader.uid;
        }
#endif
        specialAct?.Invoke(playerId);
        anim.CrossFadeInFixedTime("idle", 0.15f);
        yield return new WaitForSeconds(0.15f);
        OnStop();
    }

    public void Init(Action<string> act, string pId)
    {
        specialAct = act;
        playerId = pId;
    }

    public override void OnKill()
    {
        base.OnKill();
#if UNITY_EDITOR
        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            playerId = TestNetParams.testHeader.uid;
        }
#endif
        specialAct?.Invoke(playerId);
        specialAct = null;
        playerId = "";
    }
}

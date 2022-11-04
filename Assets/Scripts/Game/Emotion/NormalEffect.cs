using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEffect : BaseEffect
{

    public override void OnStop()
    {
        base.OnStop();
        if (emoIconData.nextEmoInfo != null)
        {
            EffectArgs effectArgs = new EffectArgs(anim, emoIconData.nextEmoInfo, playerModle, roleCon);
            OnPlay(effectArgs);
        }
        else
        {
            OnKill();
        }

    }
    private IEnumerator PlayEffect()
    {
        CreateExpression();
        yield return new WaitForSeconds(emoIconData.moveEndTime);
        OnStop();
    }
    protected IEnumerator PlayLoopEffect()
    {
        CreateExpression();
        yield return new WaitForSeconds(emoIconData.moveEndTime);
        PlayLoopEmo();
    }
    public override void PlayEmo()
    {
        PlayingEffect = CoroutineManager.Inst.StartCoroutine(PlayEffect());
    }

    public override void PlayLoopEmo()
    {
        PlayingEffect = CoroutineManager.Inst.StartCoroutine(PlayLoopEffect());
    }
}

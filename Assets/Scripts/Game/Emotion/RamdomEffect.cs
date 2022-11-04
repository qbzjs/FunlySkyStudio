using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RamdomEffect : BaseEffect
{
    private int randomId;
    public override void OnPlay(EffectArgs args)
    {
        randomId = args.randomId;
        base.OnPlay(args);
      
    }
    private IEnumerator PlayEffect()
    {
       
        yield return new WaitForSeconds(emoIconData.moveEndTime);
        OnStop();
    }
    public override void OnStop()
    {
        base.OnStop();
        OnKill();
    }
    private void SetRandomMove(string name, int id)
    {
        if (expressionGameObject != null && expressionGameObject.Count > 0)
        {
            for (int i = 0; i < expressionGameObject.Count; i++)
            {
                RandomMoveGameObject[] ran = expressionGameObject[i].GetComponentsInChildren<RandomMoveGameObject>();
                if (ran != null && ran.Length > 0)
                {
                    for (int j = 0; j < ran.Length; j++)
                    {
                        if (ran[j].isChangeTexture)
                        {
                            ran[j].ChangeTexture(name, id);
                        }
                    }
                }
            }
        }
    }
    public override void PlayEmo()
    {
        CreateExpression();
        SetRandomMove(emoIconData.name,randomId);
        PlayingEffect =CoroutineManager.Inst.StartCoroutine(PlayEffect());
       
    }

    public override void PlayLoopEmo()
    {
       
    }
}

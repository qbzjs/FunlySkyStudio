using System.Collections.Generic;
using UnityEngine;
//特效管理器，后续优化：放置到玩家身上统一管理玩家的特效
public class EffectManger
{
    public FreezePropsManager mManager;
    public Dictionary<string, FreezeEffectObj> mEffectObjs;
    public EffectManger(FreezePropsManager manager)
    {
        mManager = manager;
        mEffectObjs = new Dictionary<string, FreezeEffectObj>();
    }
    public void PlayEffect(string playerId,Transform parent)
    {
        FreezeEffectObj effectObj = null;
        if (!mEffectObjs.TryGetValue(playerId, out effectObj))
        {
            effectObj = new FreezeEffectObj();
            effectObj.Init(parent);
            mEffectObjs.Add(playerId, effectObj);
        }
        effectObj.Play();
    }
    public void StopEffect(string playerId)
    {
        FreezeEffectObj effectObj = null;
        if (mEffectObjs.TryGetValue(playerId,out effectObj))
        {
            effectObj.Stop();
        }
    }
    public void PlayEndEffect(string playerId)
    {
        FreezeEffectObj effectObj = null;
        if (mEffectObjs.TryGetValue(playerId, out effectObj))
        {
            effectObj.PlayEndEffect();
        }
    }
    public void PlayContinueEffect(string playerId)
    {
        FreezeEffectObj effectObj = null;
        if (mEffectObjs.TryGetValue(playerId, out effectObj))
        {
            effectObj.PlayContinueEffect();
        }
    }
}

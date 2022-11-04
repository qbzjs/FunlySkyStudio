using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:烟花特效池子
/// Date: 2022/7/29 1:35:35
/// </summary>
public class FireworkPool : CInstance<FireworkPool>
{
    private List<GameObject> fireEffectsPool = new List<GameObject>();
    public Transform FireworkNode;
    public GameObject GetFireWorkEffect()
    {
        if (FireworkNode == null)
        {
            FireworkNode = new GameObject("FireWorkNode").transform;
        }
        GameObject effect;
        if (fireEffectsPool != null && fireEffectsPool.Count > 0)
        {
            effect = fireEffectsPool[0];
            fireEffectsPool.RemoveAt(0);
        }
        else
        {
            var effectObj = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/Effect/FireworkEffect");
            effect = UnityEngine.Object.Instantiate(effectObj);
        }
        return effect;
    }
    public void RemoveGameobject(GameObject gameObject)
    {
        if (fireEffectsPool.Contains(gameObject))
        {
            fireEffectsPool.Remove(gameObject);
        }
    }
    public void AddEffectToPool(GameObject gameObject)
    {
        if (!fireEffectsPool.Contains(gameObject))
        {
            fireEffectsPool.Add(gameObject);
        }
    }
}

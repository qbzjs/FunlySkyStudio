using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class FirePropData
{
    public int id;
    public int flare;//是否开启光照  0 关闭，1 开启
    public float intensity;//光照强度
    public int collision;//是否开启碰撞  0，关闭 1,开启
    public int doDamage;//是否开启伤害     0 不开启，1 开启
    public int hpDamage;//伤害值
    public float lightRange;//光照范围
}

public class FirePropComponent : IComponent
{
    public int id;
    public int flare;
    public float intensity;
    public int collision;
    public int doDamage;
    public int hpDamage;
    public float lightRange;

    public IComponent Clone()
    {
        FirePropComponent comp = new FirePropComponent
        {
            id = id,
            flare = flare,
            intensity = intensity,
            collision = collision,
            doDamage = doDamage,
            hpDamage = hpDamage,
            lightRange = lightRange
        };

        return comp;
    }

    public BehaviorKV GetAttr()
    {
        FirePropData data = new FirePropData
        {
            id = id,
            flare = flare,
            intensity = intensity,
            collision = collision,
            doDamage = doDamage,
            hpDamage = hpDamage,
            lightRange = lightRange
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.FireProp,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

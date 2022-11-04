using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:PGC特效Component
/// Date: 2022/10/25 14:37:26
/// </summary>

[Serializable]
public class PGCEffectData
{
    public string col;
    public int sound;
    public int def;
}

[Serializable]
public class PGCEffectComponent : IComponent
{
    public string effectColor = "";
    public int playSound = 1;
    public int useDefColor = 1; // 是否使用默认特效颜色 0：不使用默认，1：使用默认

    public IComponent Clone()
    {
        PGCEffectComponent component = new PGCEffectComponent();
        component.effectColor = effectColor;
        component.playSound = playSound;
        component.useDefColor = useDefColor;
        return component;
    }
    
    public BehaviorKV GetAttr()
    {
        PGCEffectData data = new PGCEffectData
        {
            col = effectColor,
            sound = playSound,
            def = useDefColor
    };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.PGCEffect,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

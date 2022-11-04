using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Author:JayWill
/// Description:滑梯Component
/// </summary>

[Serializable]
public class SlidePipeData
{
    public int waytype;//one way  or  tow way
    public int hide;
}

public class SlidePipeComponent : IComponent
{
    public int WayType;
    public int HideModel;//0:默认显示 1:隐藏模型

    public IComponent Clone()
    {
        var comp = new SlidePipeComponent();
        comp.WayType = WayType;
        comp.HideModel = HideModel;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        SlidePipeData data = new SlidePipeData
        {
            waytype = WayType,
            hide = HideModel,
            
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.SlidePipe,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
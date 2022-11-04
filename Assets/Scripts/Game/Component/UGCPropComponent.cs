using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public struct UGCPropData
{
    public int isTradable;
}

/// <summary>
/// Author: 熊昭
/// Description: 场景内UGC素材可购买属性数据
/// Date: 2021-12-11 23:26:09
/// </summary>
public class UGCPropComponent : IComponent
{
    public int isTradable = 0;
    public IComponent Clone()
    {
        var comp = new UGCPropComponent();
        comp.isTradable = this.isTradable;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        UGCPropData data = new UGCPropData
        {
            isTradable = this.isTradable,
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.UGCProp,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:PGC植物Component
/// Date: 2022/8/4 15:7:26
/// </summary>

[Serializable]
public class PGCPlantData
{
    public string plantColor;
}

[Serializable]
public class PGCPlantComponent : IComponent
{
    public string plantColor = "FFFFFF";
    public IComponent Clone()
    {
        PGCPlantComponent component = new PGCPlantComponent();
        component.plantColor = plantColor;
        return component;
    }
    public BehaviorKV GetAttr()
    {
        PGCPlantData data = new PGCPlantData
        {
            plantColor = plantColor
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.PGCPlant,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

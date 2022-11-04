using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct IceCubeData
{
    public string tile;
}

/// <summary>
/// Author: Lishuzhan
/// Description: 
/// Date: 2022-07-14
/// </summary>
public class IceCubeComponent : IComponent
{
    public Vec2 tile;

    public IComponent Clone()
    {
        IceCubeComponent component = new IceCubeComponent();
        component.tile = tile;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        IceCubeData data = new IceCubeData
        {
            tile = DataUtils.Vector2ToString(tile)
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.IceCube,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SnowCubeData
{
    public int s; // 形状
    public string col; // 颜色
    public string tile; // Tiling
}

public enum SnowShape
{
    Cube,
    Cylinder
}

/// <summary>
/// Author: LiShuzhan
/// Description: 
/// Date: 2022-08-16
/// </summary>
public class SnowCubeComponent : IComponent
{
    public int shape;
    public string color;
    public Vector2 tiling;

    public IComponent Clone()
    {
        SnowCubeComponent component = new SnowCubeComponent();
        component.shape = shape;
        component.color = color;
        component.tiling = tiling;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        SnowCubeData data = new SnowCubeData
        {
            s = shape,
            col = color,
            tile = DataUtils.Vector2ToString(tiling)
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.SnowCube,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
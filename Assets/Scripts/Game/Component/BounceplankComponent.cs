/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/7/26 21:19:54
/// </summary>
using System;
using Newtonsoft.Json;
using UnityEngine;
public enum BounceShape
{
    Round,
    Square
}
public enum BounceHeight
{
    L = 8,
    M = 12,
    H = 16
}
[Serializable]
public class BounceplankData
{
    public int s;
    public string h;
    public string col;
    public int mat;
    public string tile;
}
[Serializable]
public class BounceplankComponent : IComponent
{
    public int shape;
    public string BounceHeight;
    public string color;
    public int mat;
    public Vector2 tile;
    public IComponent Clone()
    {
        BounceplankComponent component = new BounceplankComponent();
        component.shape = shape;
        component.BounceHeight = BounceHeight;
        component.color = color;
        component.mat = mat;
        component.tile = tile;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        BounceplankData data = new BounceplankData
        {
            s = shape,
            h = BounceHeight,
            col = color,
            mat = mat,
            tile = DataUtils.Vector2ToString(tile)
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.Bounceplank,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/8/31 13:16:4
/// </summary>
using System;
using Newtonsoft.Json;
using UnityEngine;
[Serializable]
public class LadderData
{
    public string col;
    public int mat;
    public int act;//梯子隐藏模型：0:不隐藏，1:隐藏
    public string tile;
}
[Serializable]
public class LadderComponent : IComponent
{
    public string color;
    public int mat;
    public int active;//梯子隐藏模型：0:不隐藏，1:隐藏
    public Vector2 tile;
    public IComponent Clone()
    {
        LadderComponent component = new LadderComponent();
        component.color = color;
        component.mat = mat;
        component.active = active;
        component.tile = tile;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        LadderData data = new LadderData
        {
            col = color,
            mat = mat,
            act = active,
            tile = DataUtils.Vector2ToString(tile)
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.Ladder,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

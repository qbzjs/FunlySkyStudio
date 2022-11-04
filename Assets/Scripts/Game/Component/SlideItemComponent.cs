using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Author:JayWill
/// Description:滑梯Component
/// </summary>

[Serializable]
public class SlideItemData
{
    public int index;
    public string color;
    public int mat;
    public string tile;
    public int speedtype;
}

public class SlideItemComponent : IComponent
{
    public int ItemIndex;
    public Color Color;
    public int MatId;
    public Vector2 Tile;
    public int SpeedType;
    public IComponent Clone()
    {
        var comp = new SlideItemComponent();
        comp.ItemIndex = ItemIndex;
        comp.Color = Color;
        comp.MatId = MatId;
        comp.Tile = Tile;
        comp.SpeedType = SpeedType;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        SlideItemData data = new SlideItemData
        {
            index = ItemIndex,
            color = DataUtils.ColorToString(Color),
            mat = MatId,
            tile = DataUtils.Vector2ToString(Tile),
            speedtype = SpeedType,
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.SlideItem,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
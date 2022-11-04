using System;
using Newtonsoft.Json;
using UnityEngine;


[Serializable]
public class MaterialComponent:IComponent 
{
    public string umat = "";
    public string uurl = "";
    public int matId = 0;
    public int colorId = 0;
    public Color color;
    public Vector2 tile;
    public IComponent Clone()
    {
        MaterialComponent component = new MaterialComponent();
        component.umat = umat;
        component.uurl = uurl;
        component.matId = matId;
        component.colorId = colorId;
        component.color = color;
        component.tile = tile;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        ColorMatData data = new ColorMatData
        {
            cols = DataUtils.ColorToString(color),
            mat = matId,
            tile = DataUtils.Vector2ToString(tile),
            umat = umat,
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.ColorMaterial,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

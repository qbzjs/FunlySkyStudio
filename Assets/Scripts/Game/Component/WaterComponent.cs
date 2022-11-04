using System;
using Newtonsoft.Json;
using UnityEngine;
[System.Serializable]
public class WaterData
{
    public int id;//对应json配置id
    public string tiling;//纹理缩放
    public float v;//水流速度
}
[Serializable]
public class WaterComponent : IComponent
{
    public int id;//对应json配置id
    public Vector2 tiling;
    public float v = 1;
    public IComponent Clone ()
    {
        WaterComponent component = new WaterComponent();
        component.id = id;
        component.tiling = tiling;
        component.v = v;
        return component;
    }

    public BehaviorKV GetAttr ()
    {
        WaterData data = new WaterData
        {
            id = id,
            tiling = DataUtils.Vector2ToString(tiling),
            v = v,
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.WaterData,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

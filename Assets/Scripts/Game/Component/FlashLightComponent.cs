using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public enum FlashLightType
{
    Directional = 0,
    SpotLight = 1
}

public enum FlashLightMode
{
    Queue = 0,
    Random = 1
}

[System.Serializable]
public class FlashLightData
{
    public int id; //uid
    public int type; //0 光柱；1 聚光灯
    public float range; //范围
    public float inten; //强度
    public float radius; //半径   
    public int isReal; //0 无实时光；1 有实时光
    public int mode; //0 顺序；1 随机
    public int time; //播放间隔
    public List<string> colors; //颜色
}

public class FlashLightComponent : IComponent
{
    public int id; //uid
    public int type; //0 光柱；1 聚光灯
    public float range; //范围
    public float inten; //强度
    public float radius; //半径  
    public int isReal; //0 无实时光；1 有实时光
    public int mode; //0 顺序；1 随机
    public int time; //播放间隔
    public List<Color> colors; //颜色

    public IComponent Clone()
    {
        return new FlashLightComponent
        {
            id = id,
            type = type,
            range = range,
            inten = inten,
            radius = radius,
            colors = new List<Color>(colors),
            mode = mode,
            time = time,
            isReal = isReal
        };
    }

    public BehaviorKV GetAttr()
    {
        FlashLightData data = new FlashLightData
        {
            id = id,
            type = type,
            range = range,
            inten = inten,
            radius = radius,
            colors = ColorToStringList(),
            mode = mode,
            time = time,
            isReal = isReal
        };
        return new BehaviorKV { k = (int)BehaviorKey.FlashLight, v = JsonConvert.SerializeObject(data) };
    }

    public List<string> ColorToStringList()
    {
        List<string> list = new List<string>();
        for(int i = 0; i < colors.Count; ++i)
        {
            list.Add(DataUtils.ColorToString(colors[i]));
        }
        return list;
    }
}

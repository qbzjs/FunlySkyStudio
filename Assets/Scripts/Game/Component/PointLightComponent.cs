using System;
using Newtonsoft.Json;
using UnityEngine;
[Serializable]
public class PointLightData
{
    public float inte;
    public float rng;
    public string lico;
}
[Serializable]
public class PointLightComponent : IComponent
{
    public float Intensity;
    public float Range;
    public Color color;
    public int colorId = 0;
    public IComponent Clone()
    {
        PointLightComponent component = new PointLightComponent();
        component.Intensity = Intensity;
        component.color = color;
        component.colorId = colorId;
        component.Range = Range;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        PointLightData data = new PointLightData
        {
            lico = DataUtils.ColorToString(color),
            inte = Intensity,
            rng = Range
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.PointLight,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
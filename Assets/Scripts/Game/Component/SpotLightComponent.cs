using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class SpotLightComponent : IComponent
{
    public float Intensity;
    public float Range;
    public float SpotAngle;
    public Color color;
    public int colorId;
    public IComponent Clone()
    {
        SpotLightComponent component = new SpotLightComponent();
        component.Intensity = Intensity;
        component.color = color;
        component.colorId = colorId;
        component.Range = Range;
        component.SpotAngle = SpotAngle;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        SpotLightData data = new SpotLightData
        {
            lico = DataUtils.ColorToString(color),
            inte = Intensity,
            rng = Range,
            spoa = SpotAngle
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.SpotLight,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
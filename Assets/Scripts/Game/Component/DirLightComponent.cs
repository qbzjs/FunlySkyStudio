using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DirLightComponent:IComponent
{
    public float anglex;
    public float angley;
    public float intensity;
    public Color color;

    public IComponent Clone()
    {
        DirLightComponent component = new DirLightComponent();
        component.anglex = anglex;
        component.angley = angley;
        component.intensity = intensity;
        component.color = color;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PostProcessComponent :IComponent
{
    public int bloomActive;
    public float bloomIntensity;
//    public int amActive;

    public IComponent Clone()
    {
        PostProcessComponent component = new PostProcessComponent();
        component.bloomActive = bloomActive;
        component.bloomIntensity = bloomIntensity;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}
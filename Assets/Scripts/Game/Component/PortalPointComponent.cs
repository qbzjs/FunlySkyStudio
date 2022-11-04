using Newtonsoft.Json;
using System;

[Serializable]
public class PortalPointComponent : IComponent
{
    public int pid;

    public IComponent Clone()
    {
        PortalPointComponent component = new PortalPointComponent();
        component.pid = pid;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        PortalPointData data = new PortalPointData
        {
            id = pid
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.PortalPoint,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
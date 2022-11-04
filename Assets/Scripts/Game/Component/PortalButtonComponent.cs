using Newtonsoft.Json;

public class PortalButtonComponent : IComponent
{
    public int pid;

    public IComponent Clone()
    {
        PortalButtonComponent component = new PortalButtonComponent();
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

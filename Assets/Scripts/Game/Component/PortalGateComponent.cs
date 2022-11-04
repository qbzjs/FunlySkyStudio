using Newtonsoft.Json;

public class PortalGateComponent : IComponent
{
    public string mapName;
    public string pngUrl;
    public string diyMapId;
    public IComponent Clone()
    {
        PortalGateComponent component = new PortalGateComponent();
        component.diyMapId = diyMapId;
        component.mapName = mapName;
        component.pngUrl = pngUrl;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        PortalGateData data = new PortalGateData
        {
            mapId = diyMapId,
            mapName =  mapName,
            pngUrl =  pngUrl
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.PortalGate,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
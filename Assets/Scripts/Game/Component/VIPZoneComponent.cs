using Newtonsoft.Json;

public class VIPZoneComponent : IComponent
{
    public string passId;
    public string dcItemId;
    public int isEdit;

    public IComponent Clone()
    {
        VIPZoneComponent copy = new VIPZoneComponent();
        copy.passId = passId;
        copy.dcItemId = dcItemId;
        copy.isEdit = isEdit;
        return copy;
    }

    public BehaviorKV GetAttr()
    {
        VIPZoneData data = new VIPZoneData()
        {
            passId = passId,
            dcItemId = dcItemId,
            isEdit = isEdit
        };
        return new BehaviorKV()
        {
            k = (int)BehaviorKey.VIPZone,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

public class VIPZoneData
{
    public string passId;
    public string dcItemId;
    public int isEdit;
}
using Newtonsoft.Json;

public class VIPDoorComponent : IComponent
{
    public string id;

    public IComponent Clone()
    {
        VIPDoorComponent copy = new VIPDoorComponent();
        copy.id = id;
        return copy;
    }

    public BehaviorKV GetAttr()
    {
        VIPDoorData data = new VIPDoorData()
        {
            id = id
        };
        return new BehaviorKV()
        {
            k = (int)BehaviorKey.VIPDoor,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

public class VIPDoorData
{
    public string id;
}
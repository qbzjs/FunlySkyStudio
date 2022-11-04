using Newtonsoft.Json;

public class VIPCheckComponent : IComponent
{
    public string id;

    public IComponent Clone()
    {
        VIPCheckComponent copy = new VIPCheckComponent();
        copy.id = id;
        return copy;
    }

    public BehaviorKV GetAttr()
    {
        VIPCheckData data = new VIPCheckData()
        {
            id = id
        };
        return new BehaviorKV()
        {
            k = (int)BehaviorKey.VIPCheck,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

public class VIPCheckData
{
    public string id;
}
using Newtonsoft.Json;

public class VIPDoorEffectComponent : IComponent
{
    public string id;

    public IComponent Clone()
    {
        VIPDoorEffectComponent copy = new VIPDoorEffectComponent();
        copy.id = id;
        return copy;
    }

    public BehaviorKV GetAttr()
    {
        VIPDoorEffectData data = new VIPDoorEffectData()
        {
            id = id
        };
        return new BehaviorKV()
        {
            k = (int)BehaviorKey.VIPDoorEffect,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

public class VIPDoorEffectData
{
    public string id;
}
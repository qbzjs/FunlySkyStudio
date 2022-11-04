using Newtonsoft.Json;

public class SeesawSeatComponent : IComponent
{
    public int index;
    public bool isFull = false; // 临时字段，不写进 json
    public string rId = SeesawManager.SEAT_DEFAULT;
    
    public IComponent Clone()
    {
        SeesawSeatComponent copy = new SeesawSeatComponent();
        copy.index = index;
        copy.rId = rId;
        return copy;
    }

    public BehaviorKV GetAttr()
    {
        SeeSawSeatData data = new SeeSawSeatData()
        {
            index = index,
            rId = rId
        };
        return new BehaviorKV()
        {
            k = (int)BehaviorKey.SeeSawSeat,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

public class SeeSawSeatData
{
    public int index;
    public string rId;
}
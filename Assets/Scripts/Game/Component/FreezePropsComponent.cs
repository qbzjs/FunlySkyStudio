
using Newtonsoft.Json;

public class FreezePropsData
{
    public string id;//对应json配置id
    public int mFreezeTime;

    public FreezePropsData(string id,int freezeTime)
    {
        this.id = id;
        this.mFreezeTime = freezeTime;
    }
}
public class FreezePropsComponent : IComponent
{
    public string rId; // 关联的素材 Id
    public int mFreezeTime;
    public IComponent Clone()
    {
        FreezePropsComponent des = new FreezePropsComponent();
        des.mFreezeTime = mFreezeTime;
        des.rId = rId;
        return des;
    }

    public BehaviorKV GetAttr()
    {
        FreezePropsData data = new FreezePropsData(rId, mFreezeTime);
        return new BehaviorKV
        {
            k = (int)BehaviorKey.FreezeProps,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

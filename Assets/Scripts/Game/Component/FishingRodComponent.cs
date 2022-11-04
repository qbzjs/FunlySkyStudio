using Newtonsoft.Json;

/// <summary>
/// Author: Tee Li
/// 日期：2022/8/30
/// 鱼竿组件
/// </summary>

[System.Serializable]
public class FishingRodData
{
    public string rid;
    public int isCustomPos;
}

public class FishingRodComponent : IComponent
{
    public string rid;
    public int isCustomPos;

    public IComponent Clone()
    {
        return new FishingRodComponent
        {
            rid = rid,
            isCustomPos = isCustomPos
        };
    }

    public BehaviorKV GetAttr()
    {
        FishingRodData data = new FishingRodData
        {
            rid = rid,
            isCustomPos = isCustomPos
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.FishingRod,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

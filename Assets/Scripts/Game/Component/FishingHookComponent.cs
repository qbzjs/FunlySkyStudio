using Newtonsoft.Json;

/// <summary>
/// Author: Tee Li
/// 日期：2022/8/30
/// 鱼钩组件
/// </summary>

[System.Serializable]
public class FishingHookData
{
    public string rid;
    public int isCustomHook;
    public Vec3 hookPosition;
}

public class FishingHookComponent : IComponent
{
    public string rid;
    public int isCustomHook;
    public Vec3 hookPosition = new Vec3(0f,0f,0f);

    public IComponent Clone()
    {
        return new FishingHookComponent
        {
            rid = rid,
            isCustomHook = isCustomHook,
            hookPosition = hookPosition
        };
    }

    public BehaviorKV GetAttr()
    {
        FishingHookData data = new FishingHookData
        {
            rid = rid,
            isCustomHook = isCustomHook,
            hookPosition = hookPosition
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.FishingHook,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

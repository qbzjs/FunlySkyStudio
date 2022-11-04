using System;
using Newtonsoft.Json;

/// <summary>
/// Author:WenJia
/// Description: 回血道具 Cmp, UGC 素材 和 默认的回血道具 会携带此 Cmp
/// Date: 2022/5/19 13:54:19
/// </summary>

[Serializable]
public struct BloodPropData
{
    public string rId;
    public float restore;
}

public class BloodPropComponent : IComponent
{
    public string rId; // 关联的素材 Id
    public float restore; // 回血量
    public IComponent Clone()
    {
        BloodPropComponent component = new BloodPropComponent();
        component.rId = rId;
        component.restore = restore;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        BloodPropData data = new BloodPropData
        {
            rId = rId,
            restore = restore
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.BloodRestoreProp,
            v = JsonConvert.SerializeObject(data)
        };
    }
}


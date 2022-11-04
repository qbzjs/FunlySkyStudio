// EdibilityComponent.cs
// Created by xiaojl Jul/22/2022
// 可食用属性组件

using Newtonsoft.Json;

// 可食用属性数据
public class EdibilityData
{
    public int mode;
}

// 食用模式
public enum EdibilityMode
{
    None,
    Eat,
    Drink
}

public enum EateState
{
    Free = 0,
    HasEated = 1,
}

public class EdibilityComponent : IComponent
{
    private EdibilityMode _mode;

    public EdibilityMode Mode { get { return _mode; } set { _mode = value; } }

    public EateState eatState;

    public IComponent Clone()
    {
        var comp = new EdibilityComponent();
        comp._mode = _mode;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        EdibilityData data = new EdibilityData();
        data.mode = (int)_mode;

        var behaviorKV = new BehaviorKV();
        behaviorKV.k = (int)BehaviorKey.Edibility;
        behaviorKV.v = JsonConvert.SerializeObject(data);

        return behaviorKV;
    }
}

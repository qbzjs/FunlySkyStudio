/// <summary>
/// Author:Mingo-LiZongMing
/// Description:可捕捉属性
/// </summary>
public class CatchabilityComponent : IComponent
{
    public IComponent Clone()
    {
        var comp = new CatchabilityComponent();
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        return new BehaviorKV
        {
            k = (int)BehaviorKey.Catchability,
            v = ""
         };
    }
}
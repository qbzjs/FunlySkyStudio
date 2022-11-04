public class CanFlyComponent : IComponent
{
    public int canFly;
    public IComponent Clone()
    {
        CanFlyComponent component = new CanFlyComponent();
        component.canFly = canFly;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}
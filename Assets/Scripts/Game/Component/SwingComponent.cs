
using Newtonsoft.Json;

public struct SwingNodeData
{
    public string rId;
    public Vec3 ropePos;
    public Vec3 ropeRote;
    public Vec3 ropeScale;
    public Vec3 seatPos;
    public Vec3 seatRote;
    public Vec3 seatScale;
    public Vec3 sitPos;
    public bool hide;
}
public class SwingComponent : IComponent
{
    public string rId;//素材id
    public Vec3 ropePos;
    public Vec3 ropeRote;
    public Vec3 ropeScale;
    public Vec3 seatPos;
    public Vec3 seatRote;
    public Vec3 seatScale;
    public Vec3 sitPos;
    public bool hide;

    
    public IComponent Clone()
    {
        SwingComponent copy = new SwingComponent();
        copy.rId = rId;
        copy.ropePos = ropePos;
        copy.ropeRote = ropeRote;
        copy.ropeScale = ropeScale;
        copy.seatPos = seatPos;
        copy.seatRote = seatRote;
        copy.seatScale = seatScale;
        copy.sitPos = sitPos;
        copy.hide = hide;
        return copy;
    }

    public BehaviorKV GetAttr()
    {
        SwingNodeData data = new SwingNodeData
        {
            rId = rId,
            ropePos = ropePos,
            ropeRote = ropeRote,
            ropeScale = ropeScale,
            seatPos = seatPos,
            seatRote = seatRote,
            seatScale = seatScale,
            sitPos = sitPos,
            hide = hide,
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.Swing,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

using Newtonsoft.Json;
using UnityEngine;

public class SeesawComponent : IComponent
{
    public int mat;
    public string color = "FFFFFF";
    public int setLeftSitPoint;
    public Vec3 leftSitPoint;
    public int setRightSitPoint;
    public Vec3 rightSitPoint;
    public int symmetry;
    public Vec2 tiling = new Vec2(1,1);

    public int setLeftSeatPos;
    public int setRightSeatPos;
    //面板用，不保存到本地
    public int panelChooseSeatIndex = 0;

    public IComponent Clone()
    {
        SeesawComponent copy = new SeesawComponent();
        copy.mat = mat;
        copy.color = color;
        copy.setLeftSitPoint = setLeftSitPoint;
        copy.leftSitPoint = leftSitPoint;
        copy.setRightSitPoint = setRightSitPoint;
        copy.rightSitPoint = rightSitPoint;
        copy.symmetry = symmetry;
        copy.tiling = tiling;
        copy.setLeftSeatPos = setLeftSeatPos;
        copy.setRightSeatPos = setRightSeatPos;
        return copy;
    }

    public BehaviorKV GetAttr()
    {
        SeesawData seesawData = new SeesawData()
        {
            mat = mat,
            color = color,
            setLeftSitPoint = setLeftSitPoint,
            leftSitPoint = leftSitPoint,
            setRightSeatPoint = setRightSitPoint,
            rightSitPoint = rightSitPoint,
            tiling = tiling,
            setLeftSeatPos = setLeftSeatPos,
            setRightSeatPos = setRightSeatPos
        };
        return new BehaviorKV()
        {
            k = (int)BehaviorKey.SeeSaw,
            v = JsonConvert.SerializeObject(seesawData)
        };
    }
}

public class SeesawData
{
    public int mat;
    public string color;
    public int setLeftSitPoint;
    public Vec3 leftSitPoint;
    public int setRightSeatPoint;
    public Vec3 rightSitPoint;
    public Vec2 tiling;
    public int setLeftSeatPos;
    public int setRightSeatPos;
}
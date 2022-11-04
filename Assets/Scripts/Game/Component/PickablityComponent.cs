/// <summary>
/// Author:Mingo-LiZongMing
/// Description:可拾起道具的数据存储
/// </summary>
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public struct PickableData
{
    public int cp;
    public float x;
    public float y;
    public float z;
}

public enum PickableState
{
    Unpickable = 0,
    Pickable = 1,
}

public class PickablityComponent : IComponent
{
    public int canPick;
    public bool isPicked = false;
    public bool alreadyPicked = false;
    public Vector3 anchors = Vector3.zero;

    public IComponent Clone()
    {
        var comp = new PickablityComponent();
        comp.canPick = canPick;
        comp.anchors = anchors;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        PickableData data = new PickableData()
        {
            cp = canPick,
            x = anchors.x,
            y = anchors.y,
            z = anchors.z,
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.Pickablity,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

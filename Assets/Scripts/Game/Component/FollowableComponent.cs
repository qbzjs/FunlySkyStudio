using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum MoveMode
{
    Anchor,
    Follow
}
[Serializable]
public class FollowableData
{
    public int mt;
    public Vec3 size;
}
public class FollowableComponent : IComponent
{
    public int moveType = (int)MoveMode.Follow;
    public Vec3 size = new Vec3(3.5f, 1.8f, 3.5f);
    public IComponent Clone()
    {
        FollowableComponent component = new FollowableComponent();
        component.moveType = moveType;
        component.size = size;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        FollowableData data = new FollowableData
        {
            mt = moveType,
            size = size,
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.FollowBox,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

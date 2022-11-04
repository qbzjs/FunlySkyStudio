using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ParachuteBagData
{
    public string rid;
    public int paraUid;
    public int isCustomPoint;
    public Vec3 anchorsPos;
}
public class ParachuteBagComponent : IComponent
{
    public int parachuteUid;
    public string rid;
    public int isCustomPoint;
    public Vec3 anchors = Vector3.zero;

    public IComponent Clone()
    {
        ParachuteBagComponent component = new ParachuteBagComponent();
        component.parachuteUid = parachuteUid;
        component.rid = rid;
        component.isCustomPoint = isCustomPoint;
        component.anchors = anchors;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        ParachuteBagData data = new ParachuteBagData
        {
            paraUid = parachuteUid,
            rid = rid,
            isCustomPoint = isCustomPoint,
            anchorsPos = anchors,
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.ParachuteBag,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

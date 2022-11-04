using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ParachuteData
{
    public string rid;
    public int bagUid;
    public int isCustomPoint;
    public Vec3 anchorsPos;
}

/// <summary>
/// Author: LiShuzhan
/// Description: 
/// Date: 2022-08-01
/// </summary>
public class ParachuteComponent : IComponent
{
    public int parachuteBagUid;
    public string rid;
    public int isCustomPoint;
    public Vec3 anchors = Vector3.zero;

    public IComponent Clone()
    {
        ParachuteComponent component = new ParachuteComponent();
        component.parachuteBagUid = parachuteBagUid;
        component.rid = rid;
        component.isCustomPoint = isCustomPoint;
        component.anchors = anchors;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        ParachuteData data = new ParachuteData
        {
			bagUid = parachuteBagUid,
            rid = rid,
            isCustomPoint = isCustomPoint,
            anchorsPos = anchors,
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.Parachute,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
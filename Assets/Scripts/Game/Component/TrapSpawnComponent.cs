using Newtonsoft.Json;
using System;
using UnityEngine;

[Serializable]
public struct TrapSpawnData
{
    public int id;
}

/// <summary>
/// Author: 熊昭
/// Description: 陷阱盒传送点数据类
/// Date: 2022-01-03 21:24:42
/// </summary>
public class TrapSpawnComponent : IComponent
{
    public int tId = 0;   //id : 1~99

    public IComponent Clone()
    {
        TrapSpawnComponent component = new TrapSpawnComponent();
        component.tId = tId;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        TrapSpawnData data = new TrapSpawnData
        {
            id = tId
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.TrapSpawn,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
/// <summary>
/// Author:Mingo-LiZongMing
/// Description:射击道具Cmp, UGC素材 和 默认的攻击道具 会携带此Cmp
/// Date: 2022-4-25 17:44:22
/// </summary>
using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct ShootWeaponNodeData
{
    public string rId;
    public int wType;
    public float damage;
    public Vec3 anchorsPos;
    public int isCustomPoint;
    public int hasCap;
    public int capacity;
    public int curBullet;
    public int fireRate;
}
public enum CustomPointState
{
    Off,
    On
}
public enum FireRate
{
    Slow = 0,
    Medium = 1,
    Fast = 2,
}
public enum CapState
{
    NoCap = 1,
    HasCap = 2,
}
public class ShootWeaponComponent : IComponent
{
    public string rId;
    public int wType;
    public float damage;
    public Vec3 anchors = Vector3.zero;
    public int isCustomPoint;

    public int hasCap = (int)CapState.NoCap;
    public int capacity;
    public int fireRate = (int)FireRate.Medium;

    public int curBullet;

    public IComponent Clone()
    {
        ShootWeaponComponent component = new ShootWeaponComponent();
        component.rId = rId;
        component.wType = wType;
        component.damage = damage;
        component.anchors = anchors;
        component.isCustomPoint = isCustomPoint;

        component.hasCap = hasCap;
        component.capacity = capacity;
        component.fireRate = fireRate;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        ShootWeaponNodeData data = new ShootWeaponNodeData
        {
            rId = rId,
            wType = wType,
            damage = damage,
            anchorsPos = anchors,
            isCustomPoint = isCustomPoint,
            hasCap = hasCap,
            capacity = capacity,
            fireRate = fireRate,
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.ShootWeapon,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

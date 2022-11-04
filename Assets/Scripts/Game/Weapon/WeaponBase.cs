using System;
using System.Collections;
using System.Collections.Generic;using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description:武器基类
/// Date: 2022-4-14 17:44:22
/// </summary>
public abstract class WeaponBase : IWeapon
{
    public string Rid { get; set; }
    public NodeBaseBehaviour weaponBehaviour { get; set; }
    public float Damage { get; set; } //武器伤害

    public WeaponBase(NodeBaseBehaviour behv)
    {
        weaponBehaviour = behv;
    }

    public abstract void OnCreate();


    public abstract void OnAttack();
    public abstract void OnEndAttack();


}
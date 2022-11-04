/// <summary>
/// Author:Mingo-LiZongMing
/// Description:射击道具行为
/// Date: 2022-5-5 17:44:22
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeShootWeapon : WeaponBase
{
    public Action<GameObject> OnTrigger;
    public int weaponUid;
    //public Vector3 shootPoint;
    public Transform ShootPoint;

    public MeleeShootWeapon(NodeBaseBehaviour behv) : base(behv)
    {
        Rid = behv.entity.Get<ShootWeaponComponent>().rId;
        Damage = behv.entity.Get<ShootWeaponComponent>().damage;
    }

    public override void OnCreate()
    {
        if(ShootPoint == null)
        {
            OnBindWeapon();
        }
    }

    private void OnBindWeapon()
    {
        var shootComp = weaponBehaviour.entity.Get<ShootWeaponComponent>();
        var shootPoint = shootComp.anchors;
        var shootPointBindGo = new GameObject("ShootPoint");
        shootPointBindGo.transform.SetParent(weaponBehaviour.transform);
        shootPointBindGo.transform.localPosition = shootPoint;
        ShootPoint = shootPointBindGo.transform;
    }

    public void UnBindWeapon()
    {
        if(ShootPoint != null)
        {
            GameObject.Destroy(ShootPoint.gameObject);
            ShootPoint = null;
        }
    }

    public override void OnAttack()
    {

    }

    public override void OnEndAttack()
    {

    }
}

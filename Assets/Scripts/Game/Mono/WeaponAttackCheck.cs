using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:PengCheng
/// Description:攻击道具物理检测
/// Date: 2021-02-24 14:15:29
/// </summary>
public class WeaponAttackCheck : MonoBehaviour
{
    public Action<Collider> OnTrigger;

    private void OnTriggerEnter(Collider other)
    {
        OnTrigger?.Invoke(other);
    }
}

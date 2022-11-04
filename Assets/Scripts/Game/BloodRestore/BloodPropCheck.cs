using System;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:回血道具物理检测
/// Date: 2022/5/20 16:0:53
/// </summary>


public class BloodPropCheck : MonoBehaviour
{
    public Action<GameObject> OnTrigger;

    private void OnTriggerEnter(Collider other)
    {
        OnTrigger?.Invoke(other.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: pzkunn
/// Description: 动画事件控制器
/// Date: 2022/10/21 13:15:36
/// </summary>
public class AnimatorEventControl : MonoBehaviour
{
    //冰晶宝石收集动画结束事件
    public void OnIceGemCollectFin()
    {
        var behav = GetComponentInParent<CrystalStoneBehaviour>();
        if (behav)
        {
            behav.ActiveCollectEffect();
        }
        CrystalStoneManager.Inst.OnRefreshCollectCount();
    }
}

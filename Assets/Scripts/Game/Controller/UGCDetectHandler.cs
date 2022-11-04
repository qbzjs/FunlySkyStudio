using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: 熊昭
/// Description: UGC购买按钮交互范围检测
/// Date: 2021-12-15 16:40:44
/// </summary>
public class UGCDetectHandler
{
    public bool IsUGCCombine(CombineBehaviour cBehav)
    {
        bool isUGCCombine = false;
        if (cBehav)
            isUGCCombine = cBehav.entity.Get<GameObjectComponent>().type == ResType.UGC;
        return isUGCCombine;
    }

    public float GetTouchDistance(Collider col, Transform selfTF)
    {
        Vector3 targetPos = col.ClosestPointOnBounds(selfTF.position);
        return Vector3.Distance(selfTF.position, targetPos);
    }

    public void RayDetectHandler(CombineBehaviour cBehav, ref NodeBaseBehaviour nBehav, ref Transform triggerGo)
    {
        if (cBehav == null)
            return;
        if (triggerGo != cBehav.transform)
        {
            if (nBehav != null)
            {
                nBehav.OnRayExit();
            }
            triggerGo = cBehav.transform;
            nBehav = cBehav;
            nBehav.OnRayEnter();
        }
    }
}
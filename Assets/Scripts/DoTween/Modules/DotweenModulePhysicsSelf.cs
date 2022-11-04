using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public static class DotweenModulePhysicsSelf
{
    public static void DoPingPongMoveY(this Transform transform, float from, float to, float duration, Ease upEase, Ease downEase)
    {
        transform.DOLocalMoveY(to, duration).SetEase(upEase)
                .OnComplete(() => transform.DoPingPongMoveY(to, from, duration, downEase, upEase));

    }
}
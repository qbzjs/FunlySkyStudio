
using System;
using DG.Tweening;
using UnityEngine;

public class NodeBaseBehaviour:MonoBehaviour
{
    //[HideInInspector]
    public SceneEntity entity;
    public bool norScale = false;
    public bool isCanClick = true;

    public virtual void OnInitByCreate()
    {

    }

    public virtual void HighLight(bool isHigh)
    {

    }
    public virtual void OnReset()
    {

    }

    public virtual void OnRayExit()
    {


    }

    public virtual void OnRayEnter()
    {

    }

    public virtual void OnTrigEnter()
    {

    }

    public virtual void OnTrigExit()
    {

    }

    public virtual void OnColliderHit()
    {

    }

    public virtual void OnSelfRotation(Vector3 axis, float speed)
    {
        transform.Rotate(axis, speed * Time.deltaTime, Space.Self);
    }

    public virtual void OnUpDownMove(float yOffset, float duration, DG.Tweening.Ease upEase, DG.Tweening.Ease downEase)
    {
        transform.DoPingPongMoveY(transform.localPosition.y, transform.localPosition.y + yOffset, duration, upEase, downEase);
    }

    public virtual void OnDestroy()
    {
        transform.DOKill();
    }
}
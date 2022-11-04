/// <summary>
/// Author:Mingo-LiZongMing
/// Description:子弹的默认Behaviour
/// </summary>
using System;
using UnityEngine;

public class BulletDefaultBehaviour : NodeBaseBehaviour
{
    private float bulletSpeed = 50f;
    private float startTime = -1;
    private Vector3 moveDir;
    private float Gravity = 5f;
    private Vector3 startPos = new Vector3();

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
    }

    public void Initialization(Vector3 startPos, Vector3 moveDirection)
    {
        startTime = -1;
        moveDir = moveDirection;
        this.startPos = startPos;
    }

    public void OnUpdate()
    {
        if(gameObject.activeSelf)
        {
            if (startTime < 0) {
                startTime = Time.time;
            }
            //更新子弹位置
            var tmp_CurTime = Time.time - startTime;
            var tmp_CurPoint = GetBulletPoint(tmp_CurTime);
            transform.position = tmp_CurPoint;
            DetectBulletPos();
        }
    }

    private Vector3 GetBulletPoint(float _time)
    {
        //vt
        var tmp_HorizontalSpeed = _time * bulletSpeed * moveDir + transform.position;
        //gt^2
        var tmp_GravityVector = Gravity * _time * _time * Vector3.down;
        return tmp_HorizontalSpeed + tmp_GravityVector;
    }

    public void DetectBulletPos()
    {   
        Vector3 curPos = transform.position;
        if (Vector3.Distance(startPos, curPos) >= 100f)
        {
            WeaponBulletManager.Inst.PushItem(this);
        }
    }
}

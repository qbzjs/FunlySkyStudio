using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowModeBehaviour : NodeBaseBehaviour
{
    public MeshRenderer bRenderer;
    private NodeBaseBehaviour followTarget;
    private Vector3 curFollowPoint = Vector3.zero;
    private float timeLimit = 1;
    private bool isTrigger = false;
    private float posY;
    private float moveState;
    private float moveSpeed;
    private bool isLook = true;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        //boxGO = transform.GetChild(0).gameObject;
        bRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    public void SetFollowTarget(NodeBaseBehaviour target)
    {
        followTarget = target;
    }
    public override void OnTrigEnter()
    {
        base.OnTrigEnter();
        if (ReferManager.Inst.isRefer)
        {
            return;
        }
        isTrigger = true;
        StartFollowMove();
    }

    public override void OnTrigExit()
    {
        base.OnTrigExit();
        if (ReferManager.Inst.isRefer)
        {
            return;
        }
        StopFollowMove();
    }

    public void UpdateFollowYAxis()
    {
        posY = followTarget.transform.position.y;
    }

    public void StartFollowMove()
    {
        var comp =  followTarget.entity.Get<MovementComponent>();
        moveState = comp.tempMoveState;
        moveSpeed = GameConsts.moveSpeed[comp.speedId];
        if (followTarget.entity.HasComponent<RPAnimComponent>())
        {
            isLook = followTarget.entity.Get<RPAnimComponent>().rSpeed == 0;
        }

        OnFollowMove();
    }

    public void OnFollowMove()
    {
        if(moveState != 0 || !followTarget.gameObject.activeInHierarchy || !isTrigger)
            return;
        Vector3 playerPos = PlayerBaseControl.Inst.transform.position;
        curFollowPoint = new Vector3(playerPos.x, posY, playerPos.z);
        var durTime = CalculateMoveTime(followTarget.transform.parent.position, curFollowPoint, moveSpeed);
        if (durTime <= 0.1f)
        {
            durTime = 0.1f;
        }
        followTarget.transform.parent.DOMove(curFollowPoint, durTime).SetEase(Ease.Linear);
        if (isLook)
        {
            followTarget.transform.parent.DOLookAt(curFollowPoint, GameConsts.rotDelTime[1]);
        }
        float minTime = Mathf.Min(durTime, timeLimit);
        Invoke("OnFollowMove", minTime);
    }

    public void PauseFollowMove()
    {
        CancelInvoke("OnFollowMove");
        followTarget.transform.parent.DOKill();
    }

    public void StopFollowMove()
    {
        CancelInvoke("TargetMove");
        isTrigger = false;
        followTarget.transform.parent.DOKill();
    }
    

    private float CalculateMoveTime(Vector3 behavPos, Vector3 point, float speed)
    {
        float distance = 0;
        distance += (point - behavPos).magnitude;
        return distance / speed;
    }
    
}

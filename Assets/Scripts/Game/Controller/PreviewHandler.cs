using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PreviewHandler : InputHandler
{

    protected readonly ResPreviewCtr controller;
    float lastTouchSpan;

    private float rotateSpeed = 15;
    public float zoomSpeed = 5f;
    public float minCamDist = 4;
    public float maxCamDist = 150;

    public Transform pivot;

    public float min = -360;
    public float max = 360;
    
    private Vector2 lastCenter;

    protected enum MultiGesture
    {
        MoreFingers,
        TwoFingers,
        Span,
        Move
    }
    protected MultiGesture gesture;
    private Camera eCamera;
    private Transform camFollow;
    private float moveSpeed = 0.18f;
    
    public PreviewHandler(ResPreviewCtr cont)
    {
        controller = cont;
        this.pivot = GameObject.Find("Center").transform ;
    }

    public void SetCamera(Camera cam, CinemachineVirtualCamera vCam)
    {
        eCamera = cam;
        camFollow = vCam.Follow;
    }

    public override void OnMovementTouchStay(Touch touch)
    {
        Vector2 move = rotateSpeed * GameConsts.TimeScale * touch.deltaPosition;
        Rotate(-move.y, move.x);
    }
    public override void OnMultipleTouchesBegin(Touch[] touches)
    {
        lastCenter = GetCenter(touches);
        lastTouchSpan = GetTouchSpan(touches);
        if (touches.Length == 2)
        {
            gesture = MultiGesture.TwoFingers;
        }
        else
        {
            gesture = MultiGesture.MoreFingers;
        }
    }
    private Vector2 GetCenter(Touch[] touches)
    {
        Vector2 mid = Vector2.zero;
        for (int i = 0; i < touches.Length; ++i)
        {
            mid += touches[i].position;
        }
        return mid / touches.Length;
    }

    private float GetTouchSpan(Touch[] touches)
    {
        Vector2 mid = GetCenter(touches);

        float dist = 0f;

        for (int i = 0; i < touches.Length; ++i)
        {
            dist += Vector2.Distance(mid, touches[i].position);
        }
        return dist / touches.Length;
    }
    public override void OnMultipleTouchesStay(Touch[] touches)
    {
        // zoom       
        if (gesture == MultiGesture.TwoFingers)
        {
            float angle = DeltaAngle(touches[0], touches[1]);
            if (angle == 0f) 
                return;
            gesture = angle < 90f ? MultiGesture.Move : MultiGesture.Span;
        }       
        if (gesture == MultiGesture.Span)
        {
            float newTouchSpan = GetTouchSpan(touches);
            OnPinch(newTouchSpan);
        }
        else if (gesture == MultiGesture.Move)
        {
            Vector2 newCenter = GetCenter(touches);
            OnMove(newCenter);
        }
    }
    
    void OnMove(Vector2 newCenter)
    {
        Vector3 camForward = pivot.transform.up;
        Vector3 camRight = pivot.transform.right;
        camRight.y = 0;

        float dirx = lastCenter.x - newCenter.x;
        float diry = lastCenter.y - newCenter.y;
        camForward = moveSpeed * diry * GameConsts.TimeScale * camForward.normalized;
        camRight = moveSpeed * dirx * GameConsts.TimeScale * camRight.normalized;
        camFollow.transform.position = camFollow.transform.position + camForward + camRight;
        lastCenter = newCenter;
    }

    private void OnPinch(float newTouchSpan)
    {
        float offset = (newTouchSpan - lastTouchSpan) * zoomSpeed * GameConsts.TimeScale;
        float currentOffset = controller.CamTransposer.m_FollowOffset.z;
        //float currentOffset = Vector3.Distance(controller.CamAim.position, controller.mapCenter.position);
        float newOffset = currentOffset + offset;
        if (newOffset > -minCamDist)
        {
            controller.CamTransposer.m_FollowOffset = new Vector3(0, 0, -minCamDist);
            //newOffset = controller.minCamDist;
        }
        else if (newOffset < -maxCamDist)
        {
            controller.CamTransposer.m_FollowOffset = new Vector3(0, 0, -maxCamDist);
            //newOffset = controller.minCamDist;
        }
        else
        {
            controller.CamTransposer.m_FollowOffset = new Vector3(0, 0, newOffset);
        }
        lastTouchSpan = newTouchSpan;
    }

    float DeltaAngle(Touch one, Touch other)
    {
        return Mathf.Abs(Vector2.SignedAngle(one.deltaPosition, other.deltaPosition));
    }

    #region Rotate
    public void Limit()
    {
        Vector3 currentAngle = pivot.localRotation.eulerAngles;
        if (currentAngle.x < min)
        {
            pivot.localRotation = Quaternion.Euler(min, currentAngle.y, currentAngle.z);
        }
        else if (currentAngle.x > max)
        {
            pivot.localRotation = Quaternion.Euler(max, currentAngle.y, currentAngle.z);

        }
    }

    public void Rotate(float angleVertical, float angleHorizontal)
    {
        pivot.Rotate(Vector3.up, angleHorizontal, Space.World);
        RotateVertical(angleVertical);
    }

    public void RotateVertical(float angle)
    {
        Vector3 currentAngle = pivot.localRotation.eulerAngles;
        float curAngleX = currentAngle.x > 180f ? currentAngle.x - 360f : currentAngle.x;
        float targetAngle = curAngleX + angle;
        if (targetAngle < min)
        {
            targetAngle = min;
        }
        else if (targetAngle > max)
        {
            targetAngle = max;
        }

        float angleToRotate = targetAngle - curAngleX;
        pivot.Rotate(pivot.right, angleToRotate, Space.World);
        //pivot.Rotate(Vector3.right, angleToRotate, Space.World);
    }
    public void SetMinMax(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
    #endregion
}

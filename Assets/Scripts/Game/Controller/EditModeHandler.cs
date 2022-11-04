using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

public class EditModeHandler : InputHandler
{
    private Transform camFollow;
    private CinemachineVirtualCamera VirtualCam;
    private CinemachineTransposer camTransposer;
    public JoyStick joyStick;
    float moveTouchId = -1;
    float rotTouchId = -1;
    private float rotMin = 5;
    private float rotMax = 85;
    private float maxWidth = 500;
    private float maxHeight = 500;
    private float rotateSpeed = 2;
    private float moveSpeed = 0.18f;
    private float maxCamDist = 500;
    private float zoomSpeed = 0.6f;
    private float minCamAndCamFollowDist = -4f;//相机到参考点的最小距离（z轴）；
    private float minCamDist = 2;
    private float initialOffsetZ;//初始摄心距
    private float lastTouchSpan;
    private float lastCentery;
    private Vector2 lastCenter;
    private Camera eCamera;
    private bool isJoyStick;
    public Action<Touch> OnSelectTarget;
    public Action OnMouseAndKeyboardInput;
    public float ZoomSpeed
    {
        get
        {
            float scale =  camTransposer.m_FollowOffset.z / initialOffsetZ;
            scale= Mathf.Max(0.2f,scale);
            return zoomSpeed* scale;
        }
        set { zoomSpeed = value; }
    }
    
    public float OffestScale
    {
        get
        {
            float scale = camTransposer.m_FollowOffset.z / initialOffsetZ;
            scale = Mathf.Max(1.25f, scale);
            return scale;
        }
        set { }
    }
    protected enum MultiGesture
    {
        MoreFingers,
        TwoFingers,
        Span,
        Move
    }
    protected MultiGesture gesture;

    public void SetCamera(Camera cam, CinemachineVirtualCamera vCam)
    {
        eCamera = cam;
        VirtualCam = vCam;
        camFollow = vCam.Follow;
        camTransposer = VirtualCam.GetCinemachineComponent<CinemachineTransposer>();
        initialOffsetZ = camTransposer.m_FollowOffset.z;
    }

    public override void OnShortTouchEnd(Touch touch)
    {
        OnSelectTarget?.Invoke(touch);
    }

    public override void OnMovementTouchStay(Touch touch)
    {
        if (isJoyStick)
        {
            return;
        }
        if (ReferManager.Inst.isRefer && !ReferManager.Inst.isHafeRefer)
        {
            return;
        }
        Vector2 move = rotateSpeed * GameConsts.TimeScale * touch.deltaPosition;

        MouseAndKeyboardInput();
        Rotate(-move.y, move.x);
    }
    public void MouseAndKeyboardInput()
    {
        StopCamTransform();
        OnMouseAndKeyboardInput?.Invoke();
    }
    private void Rotate(float angleVertical, float angleHorizontal)
    {
        camFollow.Rotate(Vector3.up, angleHorizontal, Space.World);
        RotateVertical(angleVertical);
    }
    public bool mIsDoingProjectionSwitch = false;
    public bool mIsDoingRotationSwitch = false;
    public IEnumerator mGenricCamTransformCrtn = null;
    public void OnSceneGizmoHandlePicked(Quaternion targetRotation)
    {
        StopCamTransform();
        CoroutineManager.Inst.StartCoroutine(mGenricCamTransformCrtn = DoSmoothRotationSwitch(targetRotation));
    }
    public void StopCamTransform()
    {
        if (mGenricCamTransformCrtn != null)
        {
            CoroutineManager.Inst.StopCoroutine(mGenricCamTransformCrtn);
            mGenricCamTransformCrtn = null;
        }
        mIsDoingRotationSwitch = false;
    }
    private IEnumerator DoSmoothRotationSwitch(Quaternion targetRotation)
    {
        mIsDoingRotationSwitch = true;
        while (true)
        {
            camFollow.rotation = Quaternion.Slerp(camFollow.rotation, targetRotation, Time.deltaTime * 6);
            if (Mathf.Abs(Quaternion.Angle(camFollow.rotation, targetRotation)) < 1e-4f)
            {
                camFollow.rotation = targetRotation;
                break;
            }

            yield return null;
        }
        mIsDoingRotationSwitch = false;
    }

    private void RotateVertical(float angle)
    {
        Vector3 currentAngle = camFollow.localRotation.eulerAngles;
        float curAngleX = currentAngle.x > 180f ? currentAngle.x - 360f : currentAngle.x;
        float targetAngle = curAngleX + angle;
        //targetAngle = Mathf.Clamp(targetAngle, rotMin, rotMax);
        float angleToRotate = targetAngle - curAngleX;
        camFollow.Rotate(camFollow.right, angleToRotate, Space.World);
    }

    public override void OnMultipleTouchesBegin(Touch[] touches)
    {
        if (isJoyStick)
        {
            return;
        }
        lastCenter = GetCenter(touches);
        lastTouchSpan = GetTouchSpan(touches);
        gesture = touches.Length == 2 ? MultiGesture.TwoFingers : MultiGesture.MoreFingers;
    }

    public override void OnMultipleTouchesStay(Touch[] touches)
    {
        if (isJoyStick)
        {
            return;
        }
        if (gesture == MultiGesture.TwoFingers)
        {
            float angle = DeltaAngle(touches[0], touches[1]);
            if (angle == 0f)
                return;
            gesture = angle < 90f ? MultiGesture.Move : MultiGesture.Span;
        }
        if (isJoyStick && rotTouchId != -1)
        {
            return;
        }
        else
        {
            rotTouchId = -1;
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
        MouseAndKeyboardInput();
    }

    void OnPinch(float newTouchSpan)
    {
        float offset = (newTouchSpan - lastTouchSpan) * ZoomSpeed * GameConsts.TimeScale;
        float currentOffset = camTransposer.m_FollowOffset.z;
        float newOffset = currentOffset + offset;

        //如果相机离参考点太近，就一起移动。
        if (newOffset >= minCamAndCamFollowDist)
        {
            Vector3 followCurPos = camFollow.transform.position;
            Vector3 worldForward = camFollow.transform.TransformVector(Vector3.forward * offset);
            Vector3 detechPos = followCurPos + worldForward;
            //如果尝试移动导致参考点y轴小于0，则不允许移动。
            //09.29取消限制
            //if (detechPos.y > 0)
            camFollow.transform.position = detechPos;
        }
        else
        {
            if (newOffset < -maxCamDist)
            {
                camTransposer.m_FollowOffset = new Vector3(0, 0, -maxCamDist);
            }
            else
            {
                camTransposer.m_FollowOffset = new Vector3(0, 0, newOffset);
            }
        }
        lastTouchSpan = newTouchSpan;
    }

    void OnMove(Vector2 newCenter)
    {
        Vector3 camUp = eCamera.transform.TransformVector(Vector3.up);
        Vector3 camRight = eCamera.transform.TransformVector(Vector3.right);
        float dirx = lastCenter.x - newCenter.x;
        float diry = lastCenter.y - newCenter.y;
        float initalMoveSpeed = 0.18f;
        moveSpeed = initalMoveSpeed * OffestScale;
        LoggerUtils.Log("moveSpeed:" + moveSpeed);
        camUp = moveSpeed * diry * GameConsts.TimeScale * camUp.normalized;

        Vector3 followCurPos = camFollow.transform.position;
        Vector3 detechPos = followCurPos + camUp;
        camRight = moveSpeed * dirx * GameConsts.TimeScale * camRight.normalized;
        camFollow.transform.position = (camFollow.transform.position + camUp + camRight);
        lastCenter = newCenter;
    }

    Vector2 GetCenter(Touch[] touches)
    {
        Vector2 mid = Vector2.zero;
        for (int i = 0; i < touches.Length; ++i)
        {
            mid += touches[i].position;
        }
        return mid / touches.Length;
    }

    float GetTouchSpan(Touch[] touches)
    {
        Vector2 mid = GetCenter(touches);

        float dist = 0f;

        for (int i = 0; i < touches.Length; ++i)
        {
            dist += Vector2.Distance(mid, touches[i].position);
        }
        return dist / touches.Length;
    }

    private float DeltaAngle(Touch one, Touch other)
    {
        return Mathf.Abs(Vector2.SignedAngle(one.deltaPosition, other.deltaPosition));
    }

    private Vector3 LimitedPosition(Vector3 currentPosition)
    {
        Vector3 limited = currentPosition;
        if (currentPosition.x > maxWidth / 2)
        {
            limited.x = maxWidth / 2;
        }
        else if (currentPosition.x < -maxWidth / 2)
        {
            limited.x = -maxWidth / 2;
        }

        if (currentPosition.y > maxHeight / 2)
        {
            limited.y = maxHeight / 2;
        }
        else if (currentPosition.z < -maxHeight / 2)
        {
            limited.y = -maxHeight / 2;
        }
        return limited;
    }

    public override void OnTouchBegin(Touch touch)
    {
        if (!ReferManager.Inst.isRefer || ReferManager.Inst.isHafeRefer)
        {
            return;
        }
        ReferManager.Inst.isUndoRedo = false;
        if (ReferManager.Inst.isRefer && joyStick.InRange(touch.position))
        {
            moveTouchId = touch.fingerId;
        }
        else
        {
            rotTouchId = touch.fingerId;
        }
    }

    public override void OnTouchStay(Touch touch)
    {
        if (ReferManager.Inst.isHafeRefer || ReferManager.Inst.isUndoRedo)
        {
            if (ReferPanel.Instance)
            {
                ReferPanel.Instance.playerCom.Move(Vector3.zero);
            }
            return;
        }
        if (touch.fingerId == moveTouchId)
        {
            if (touch.phase == TouchPhase.Ended)
            {
                moveTouchId = -1;
                ReferPanel.Instance.playerCom.Move(Vector3.zero);
                joyStick.JoystickReset();
                isJoyStick = false;
            }
            else
            {
                isJoyStick = true;
                Vector3 offset = joyStick.Touch(touch.position);
                ReferPanel.Instance.playerCom.Move(offset);
            }
            return;
        }
        // Vector2 move = rotateSpeed * Time.deltaTime * touch.deltaPosition;
        if (touch.fingerId == rotTouchId)
        {
            if (touch.phase == TouchPhase.Ended)
            {
                rotTouchId = -1;
                isJoyStick = false;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 offset = rotateSpeed * GameConsts.TimeScale * touch.deltaPosition;
                ReferPanel.Instance.playerCom.transform.Rotate(Vector3.up, offset.x, Space.World);
                MouseAndKeyboardInput();
                Rotate(-offset.y, offset.x);
            }
        }
    }
}

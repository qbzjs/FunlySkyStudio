using System;
using System.Runtime.CompilerServices;
using Cinemachine;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;

public class CameraModeHandler : InputHandler
{
    private Transform camFollow;
    private CinemachineVirtualCamera VirtualCam;
    private CinemachineTransposer camTransposer;
    public JoyStick joyStick;
    float moveTouchId = -1;
    float rotTouchId = -1;
    private float maxWidth = (int)GlobalFieldController.terrainSize * 100f;//地图边界
    private float maxHeight = (int)GlobalFieldController.terrainSize * 100f;//地图边界
    private float rotateSpeed = 2;
    private float moveSpeed = 0.18f;
    private float maxCamDist = 20;//拉远最远距离
    private float zoomSpeed = 0.6f;//相机模式缩放速度
    private float selZoomSpeed = 3f;//自拍缩放速度
    private float minCamDist = -2;//拉近最近距离
    private float lastTouchSpan;
    private Vector2 lastCenter;
    private Camera eCamera;
    private bool isJoyStick;

    public Action<Touch> OnSelectTarget;

    public Action<Touch> OnClickTarget;

    public Vector3 orginPos;//进入相机模式时的初始位置
    public Vector3 orginEuler;//进入相机模式时的初始角度
    public Vector3 orginForward;//进入相机模式时的初始朝向
    private Transform mCtrCamera;

    private const float MOVE_LIMIT = 10;
    private float H_CAMERA_LIMIT = 90;//横向滑动限制
    private const float V_SELFIE_ROTATE_MIN = 0;//自拍模式纵向限制min
    private const float V_SELFIE_ROTATE_MAX = 30;//自拍模式纵向限制max
    private const float V_CAMERA_ROTATE_MIN = -60; //相机模式纵向限制min
    private const float V_CAMERA_ROTATE_MAX = 85;//相机模式纵向限制max
    private const float MOVE_THRESHOLD = 5;//移动启动阈值，超过则裁判的为开始移动
    private const float FIELD_OF_VIEW_MIN = 40;
    private const float FILED_OF_VIEW_MAX = 72;

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
        mCtrCamera = camFollow;
        OnEnter();
    }

    public void OnEnter()
    {
        var playVirCamera = CameraModeManager.Inst.GetPlayVirCamera();
        camFollow.SetPositionAndRotation(playVirCamera.Follow.position,playVirCamera.Follow.rotation);
        CameraModeManager.Inst.SetVirCameraBodyType(VirtualCam,CameraBodyType.Transposer);
        camTransposer.m_FollowOffset = new Vector3(0,0,CameraModeManager.DEFAULT_FOLLOW_Z);//默认跟随距离
        VirtualCam.m_Lens.FieldOfView = CameraModeManager.CAMERA_FIELD_OF_VIEW;
        OnCameraMove();
    }


    public void OnCameraMove()
    {   
        orginPos = eCamera.transform.position;
        orginEuler = eCamera.transform.localEulerAngles;
        orginForward = eCamera.transform.forward;
        camTransposer = VirtualCam.GetCinemachineComponent<CinemachineTransposer>();
        CoroutineManager.Inst.StartCoroutine(DelaySetCameraPos());
        
    }

    public void ResetJoyStick()
    {
        if(joyStick)
        {
            joyStick.JoystickReset();
            moveTouchId = -1;
        }
        if(PlayerControlManager.Inst != null)
        {
            PlayerControlManager.Inst.Move(Vector3.zero);
        }
    }

    public IEnumerator DelaySetCameraPos()
    {
        yield return 0;
        orginPos = eCamera.transform.position;
        orginEuler = eCamera.transform.localEulerAngles;
        orginForward = eCamera.transform.forward;
    }

    public override void OnShortTouchEnd(Touch touch)
    {
        OnSelectTarget?.Invoke(touch);
    }

    public override void OnMovementTouchStay(Touch touch)
    {
    }

    private void Rotate(float angleVertical, float angleHorizontal)
    {
        // RotateH(angleHorizontal);
    
        //自拍模式则按第一人称方式旋转人物
        if(StateManager.IsInSelfieMode)
        {
            //人物不能动的话，不允许旋转
            if(PlayerManager.Inst.selfPlayer.GetNoAbilityFlag(EObjAbilityType.Move))
            {
                return;
            }
            var player = PlayerManager.Inst.selfPlayer;
            player.transform.Rotate(Vector3.up, angleHorizontal, Space.World);
            player.playerAnim.transform.Rotate(Vector3.up, angleHorizontal, Space.World);
            RotateVertical(angleVertical,V_SELFIE_ROTATE_MIN,V_SELFIE_ROTATE_MAX);
        }
        else
        {
            RotateH(angleHorizontal);
            RotateVertical(angleVertical);
        }
    }

    //横向旋转无限制
    private void RotateH(float angleHorizontal)
    {
        mCtrCamera.Rotate(Vector3.up, angleHorizontal, Space.World);
    }

    //横向旋转及限制
    private void RotateHorizontal(float angleHorizontal)
    {
        float diffAngle = Vector3.Angle(mCtrCamera.forward, orginForward);
        Vector3 normal = Vector3.Cross(orginForward, mCtrCamera.forward);//计算法线向量
        diffAngle *= Mathf.Sign(Vector3.Dot(normal,Vector3.up));
        float targetAngle = diffAngle + angleHorizontal;
        targetAngle = Mathf.Clamp(targetAngle, -H_CAMERA_LIMIT, H_CAMERA_LIMIT);
        float angleToRotate = targetAngle - diffAngle;
        mCtrCamera.Rotate(Vector3.up, angleToRotate, Space.World);
    }

    //纵向旋转及限制
    private void RotateVertical(float angle,float rotMin = V_CAMERA_ROTATE_MIN, float rotMax = V_CAMERA_ROTATE_MAX)
    {
        Vector3 currentAngle = mCtrCamera.localRotation.eulerAngles;
        float curAngleX = currentAngle.x > 180f ? currentAngle.x - 360f : currentAngle.x;
        float targetAngle = curAngleX + angle;
        targetAngle = Mathf.Clamp(targetAngle, rotMin, rotMax);
        float angleToRotate = targetAngle - curAngleX;
        mCtrCamera.Rotate(mCtrCamera.right, angleToRotate, Space.World);       
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
    }

    //双指缩放
    void OnPinch(float newTouchSpan)
    {

        if(StateManager.IsInSelfieMode)
        {
            float offset = (newTouchSpan - lastTouchSpan) * selZoomSpeed * GameConsts.TimeScale;
            
            float currentOffset = VirtualCam.m_Lens.FieldOfView;
            float newOffset = currentOffset - offset;
            if(newOffset < FIELD_OF_VIEW_MIN)
            {
                newOffset = FIELD_OF_VIEW_MIN;
            }
            else if(newOffset > FILED_OF_VIEW_MAX)
            {
                newOffset = FILED_OF_VIEW_MAX;
            }
            
            VirtualCam.m_Lens.FieldOfView = newOffset;
            lastTouchSpan = newTouchSpan;
        }
        else
        {
            float offset = (newTouchSpan - lastTouchSpan) * ZoomSpeed * GameConsts.TimeScale;
            float currentOffset = camTransposer.m_FollowOffset.z;
            float newOffset = currentOffset + offset;
            if (newOffset > -minCamDist)
            {
                camTransposer.m_FollowOffset = new Vector3(0, 0, -minCamDist);
            }
            else if (newOffset < -maxCamDist)
            {
                camTransposer.m_FollowOffset = new Vector3(0, 0, -maxCamDist);
            }
            else
            {
                camTransposer.m_FollowOffset = new Vector3(0, 0, newOffset);
            }
            lastTouchSpan = newTouchSpan;
        }
       
    }

    public float ZoomSpeed
    {
        get { return zoomSpeed; }
        set
        {
            zoomSpeed = value;
        }
    }
    //双指平移
    void OnMove(Vector2 newCenter)
    {
        if(StateManager.IsInSelfieMode)
        {
            //自拍模式不能平移
            return;
        }
        Vector3 camForward = mCtrCamera.forward;
        Vector3 camRight = mCtrCamera.right;
        camForward.y = 0;
        camRight.y = 0;
        float dirx = lastCenter.x - newCenter.x;
        float diry = lastCenter.y - newCenter.y;
        camForward = moveSpeed * diry * GameConsts.TimeScale * camForward.normalized;
        camRight = moveSpeed * dirx * GameConsts.TimeScale * camRight.normalized;
        var movetoPos = LimitedCameraPosition(mCtrCamera.position + camForward + camRight);
        mCtrCamera.position = movetoPos;
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

    private Vector3 LimitedWroldPosition(Vector3 currentPosition)
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

        if (currentPosition.z > maxHeight / 2)
        {
            limited.z = maxHeight / 2;
        }
        else if (currentPosition.z < -maxHeight / 2)
        {
            limited.z = -maxHeight / 2;
        }
        return limited;
    }

    private Vector3 LimitedCameraPosition(Vector3 currentPosition)
    {
        Vector3 movetoPos = LimitedWroldPosition(currentPosition);
        if (movetoPos.x > (orginPos.x + MOVE_LIMIT))
        {
            movetoPos.x = orginPos.x + MOVE_LIMIT;
        }
        else if (movetoPos.x < (orginPos.x - MOVE_LIMIT))
        {
            movetoPos.x = orginPos.x - MOVE_LIMIT;
        }

        if (movetoPos.z > orginPos.z + MOVE_LIMIT)
        {
            movetoPos.z = orginPos.z + MOVE_LIMIT;
        }
        else if (movetoPos.z < orginPos.z - MOVE_LIMIT)
        {
            movetoPos.z = orginPos.z - MOVE_LIMIT;
        }
        return movetoPos;
    }

    public override void OnTouchBegin(Touch touch)
    {
        if (ReferManager.Inst.isRefer)
        {
            return;
        }
        if(joyStick.enabled)
        {
            joyStick.SetToPos(touch.position);
        }
        
        if (joyStick.enabled && joyStick.InRange(touch.position))
        {
            moveTouchId = touch.fingerId;
            GameUtils.ResetStayTime();
        }
        else
        {
            rotTouchId = touch.fingerId;
        }
        if ((PlayerManager.Inst.selfPlayer.animCon.isTyping))
        {
            // 若在打字过程中, 再次移动摇杆，则结束打字动画
            MobileInterface.Instance.ReceiveMessageFromClient(JsonConvert.SerializeObject( new SavingData.ClientResponse
            {
                isSuccess = 1,
                funcName = MobileInterface.hideKeyboard,
                data = "",
            }) );
        }
    }

    public Vector3 Vec3Rotate(Vector3 source,float angle)
    {
        Quaternion q = Quaternion.AngleAxis(angle,Vector3.forward);
        return q * source;
    }

    public override void OnTouchStay(Touch touch)
    {
        if (ReferManager.Inst.isRefer)
        {
            return;
        }
        if ((PlayerManager.Inst.selfPlayer.animCon.isTyping && moveTouchId != -1))
        {
            moveTouchId = -1;
            PlayerControlManager.Inst.Move(Vector3.zero);
            joyStick.JoystickReset();
            return;
        }
        if (StorePanel.Instance && StorePanel.Instance.gameObject.activeSelf)
        {
            PlayerControlManager.Inst.Move(Vector3.zero);
            joyStick.JoystickReset();
            return; 
        }

        if (TokenDetectionPanel.Instance && TokenDetectionPanel.Instance.gameObject.activeSelf)
        {
            PlayerControlManager.Inst.Move(Vector3.zero);
            joyStick.JoystickReset();
            return;
        }
        if (touch.fingerId == moveTouchId)
        {
            if (touch.phase == TouchPhase.Ended)
            {
                moveTouchId = -1;
                GameUtils.ResetStayTime();
                PlayerControlManager.Inst.Move(Vector3.zero);
                joyStick.JoystickReset();
                isJoyStick = false;
            }
            else
            {
                if (!joyStick.enabled) return;
                isJoyStick = true;
                GameUtils.stayTime += Time.deltaTime;
                if (GameUtils.stayTime > 2)
                {
                        PlayerBaseControl.Inst.canUseAutoMateMode = true;
                }
                else
                {
                    PlayerManager.Inst.selfPlayer.canUseAutoMateMode = false;
                }

                Vector3 offset = joyStick.Touch(touch.position);

                // float fixAngle = Vector3.Angle(orginForward, mCtrCamera.forward);//计算夹角
                // Vector3 normal = Vector3.Cross(orginForward, mCtrCamera.forward);//计算法线向量
                // fixAngle *= Mathf.Sign(Vector3.Dot(normal,Vector3.up));

                float fixAngle = VirtualCam.gameObject.transform.localEulerAngles.y - PlayerManager.Inst.selfPlayer.transform.localEulerAngles.y;
                offset = Vec3Rotate(offset,-fixAngle);

                if(Math.Abs(offset.x )< MOVE_THRESHOLD && Math.Abs(offset.y)< MOVE_THRESHOLD)
                {
                    return;
                }
                PlayerControlManager.Inst.Move(offset);


            }
            return;
        }
       
        if (touch.fingerId == rotTouchId)
        {
            if (touch.phase == TouchPhase.Ended)
            {
                rotTouchId = -1;
                isJoyStick = false;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 offset = rotateSpeed * GameConsts.TimeScale * touch.deltaPosition ;
                Rotate(-offset.y, offset.x);
            }
        }
    }
}

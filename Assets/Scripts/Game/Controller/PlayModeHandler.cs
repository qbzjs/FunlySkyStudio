using Cinemachine;
using System;
using Newtonsoft.Json;
using UnityEngine;
using ButtonType = PlayModePanel.ButtonType;

public class PlayModeHandler : InputHandler
{
    private float rotMin = -45;
    private float rotMax = 60;
    private float maxWidth = 100;
    private float maxHeight = 100;
    private float rotateSpeed = 4.4f;
    private float moveSpeed = 0.18f;
    private float maxCamDist = 350;
    private float zoomSpeed = 0.6f;
    private float minCamDist = 2;
    private float protectTouchDist = 60;

    private Transform camFollow;
    private CinemachineVirtualCamera VirtualCam;
    private CinemachineTransposer camTransposer;
    private Camera eCamera;
    private Camera uiCamera;
    private Canvas uiCanvas;

    public JoyStick joyStick;
    public PlayerBaseControl player;
    public Action<Touch> OnClickTarget;
    public static int fpvRotMax = 60;

    int moveTouchId = -1;
    int rotTouchId = -1;

    private const float MOVE_THRESHOLD = 5;//移动启动阈值，超过则裁判的为开始移动


    public void SetCamera(Camera cam, CinemachineVirtualCamera vCam)
    {
        fpvRotMax = 60;
        eCamera = cam;
        VirtualCam = vCam;
        camFollow = vCam.Follow;
        camTransposer = VirtualCam.GetCinemachineComponent<CinemachineTransposer>();
        uiCanvas = UIManager.Inst.uiCanvas.GetComponentInParent<Canvas>();
        uiCamera = uiCanvas.worldCamera;
        player = PlayerManager.Inst.selfPlayer;
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
        if ((PlayerBaseControl.Inst && PlayerBaseControl.Inst.animCon.isTyping))
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

    public override void OnTouchStay(Touch touch)
    {
        if (ReferManager.Inst.isRefer)
        {
            return;
        }
        if ((PlayerBaseControl.Inst && PlayerBaseControl.Inst.animCon.isTyping && moveTouchId != -1))
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
        
        if(TokenDetectionPanel.Instance && TokenDetectionPanel.Instance.gameObject.activeSelf)
        {
            moveTouchId = -1;
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
            }
            else
            {
                if (!joyStick.enabled) return;
               GameUtils.stayTime += Time.deltaTime;

                if (GameUtils.stayTime > 2)
                {
                    PlayerBaseControl.Inst.canUseAutoMateMode = true;
                }
                else
                {
                    PlayerBaseControl.Inst.canUseAutoMateMode = false;
                }
                Vector3 offset = joyStick.Touch(touch.position);
                if(Math.Abs(offset.x )< MOVE_THRESHOLD && Math.Abs(offset.y)< MOVE_THRESHOLD)
                {
                    return;
                }
                if (StateManager.IsOnSeesaw)
                {
                    return;
                }
                if (StateManager.IsOnSwing)
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
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                float cameraSensitive = GlobalSettingManager.Inst.GetCameraSensitive();
                Vector3 offset = rotateSpeed * GameConsts.TimeScale * touch.deltaPosition;
                
                if (!player.isMoving && (player.playerAnim.gameObject.activeSelf || !player.isTps))
                {
                    if ((PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
                    || (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel != null)
                    || StateManager.IsOnLadder)
                    {
                        player.playerAnim.transform.Rotate(Vector3.up, -offset.x * cameraSensitive, Space.Self);
                    }
                    else if(!StateManager.IsOnSeesaw && !StateManager.IsOnSwing)
                    {
                        player.playerAnim.transform.Rotate(Vector3.up, -offset.x *cameraSensitive, Space.World);
                    }
                }

                if ((PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
                    || (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel != null)
                    || StateManager.IsOnLadder)
                {
                    MagneticBoardManager.Inst.SetOffset(offset.x * cameraSensitive);
                    SteeringWheelManager.Inst.SetOffset(offset.x * cameraSensitive);
                    LadderManager.Inst.SetOffset(offset.x * cameraSensitive);
                }
                else {
                    player.transform.Rotate(Vector3.up, offset.x * cameraSensitive, Space.World);
                    if (!player.isMoving && !player.isTps)
                    {
                        player.playerAnim.transform.Rotate(Vector3.up, offset.x * cameraSensitive, Space.World);
                    }
                }

               
                RotateVertical(-offset.y * cameraSensitive);
            }
        }
    }

    private void RotateVertical(float angle)
    {
        
        Vector3 currentAngle = camFollow.localRotation.eulerAngles;
        float curAngleX = currentAngle.x > 180f ? currentAngle.x - 360f : currentAngle.x;
        float targetAngle = curAngleX + angle;
        targetAngle = Mathf.Clamp(targetAngle, rotMin, fpvRotMax);
        float angleToRotate = targetAngle - curAngleX;
        camFollow.Rotate(camFollow.right, angleToRotate, Space.World);
    }
    public override bool OnDragJoyStick(Touch touch)
    {
        if (joyStick.enabled && joyStick.InRange(touch.position))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public override void OnShortTouchEnd(Touch touch)
    {
        //目前只有游玩模式需要
        if (!InsideTouchArea(touch))
        {
            LoggerUtils.Log("InsideProtectArea -- " + Time.time);
            return;
        }
        OnClickTarget?.Invoke(touch);
    }

    private bool InsideTouchArea(Touch touch)
    {
        var actBtn = SceneBuilder.Inst.CanFlyEntity.Get<CanFlyComponent>().canFly == 0 ? ButtonType.FlyModeBtn : ButtonType.JumpBtn;
        RectTransform actBtnRTF = PlayModePanel.Instance.GetButtonRTF(actBtn);
        RectTransform joyStickRTF = PlayModePanel.Instance.GetButtonRTF(ButtonType.JoyStick);

        if (uiCamera == null)
        {
            return false;
        }
        if (joyStickRTF)
        {
            if (InsideCircleArea(touch, joyStickRTF))
            {
                return false;
            }
        }
        switch (actBtn)
        {
            case ButtonType.FlyModeBtn:
                if (InsideSquareArea(touch, actBtnRTF))
                {
                    return false;
                }
                break;
            case ButtonType.JumpBtn:
                if (InsideCircleArea(touch, actBtnRTF))
                {
                    return false;
                }
                break;
        }
        return true;
    }

    private bool InsideCircleArea(Touch touch, RectTransform btnRTF)
    {
        Vector2 btnScreenPos = uiCamera.WorldToScreenPoint(btnRTF.position);
        float protectRadius = Mathf.Max(btnRTF.sizeDelta.x, btnRTF.sizeDelta.y) / 2 + protectTouchDist;
        if (Vector2.Distance(touch.position, btnScreenPos) < protectRadius)
        {
            return true;
        }
        return false;
    }

    private bool InsideSquareArea(Touch touch, RectTransform btnRTF)
    {
        Vector2 btnScreenPos = uiCamera.WorldToScreenPoint(btnRTF.position);
        var xMin = btnScreenPos.x - btnRTF.sizeDelta.x / 2 - protectTouchDist;
        var xMax = btnScreenPos.x + btnRTF.sizeDelta.x / 2 + protectTouchDist;
        var yMin = btnScreenPos.y - btnRTF.sizeDelta.y / 2 - protectTouchDist;
        var yMax = btnScreenPos.y + btnRTF.sizeDelta.y / 2 + protectTouchDist;
        if (touch.position.x < xMax && touch.position.x > xMin && touch.position.y < yMax && touch.position.y > yMin)
        {
            return true;
        }
        return false;
    }
}

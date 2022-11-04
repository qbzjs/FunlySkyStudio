using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description:Player 跳伞控制
/// 跳伞过程的滑翔、降落阶段操作控制
/// Date: 2022-7-28 10:46:02
/// </summary>

//降落伞当前的移动状态
public enum ParachuteMoveState
{
    Ready = 0,
    Gliding = 1, //滑翔
    Falling = 2, //开伞降落
}

public enum ParachuteAnimState
{
    NoUse = 1000, //降落伞未使用

    GlidingIdle = 1001, //降落伞-滑翔静止
    GlidingMove = 1002, //降落伞-滑翔移动
    GlidingPreLand = 1003, //降落伞-滑翔落地翻滚

    FallingReady = 1004, //降落伞打开
    FallingIdle = 1005, //降落伞-降落静止状态
    FallingMoveForward = 1006, //降落伞-降落时向前移动(左上-右上之间)，人物前倾
    FallingMoveBackward = 1007, //降落伞-降落时向后移动(左下-右下之间)， 人物后倾
    FallingMoveLeft = 1008, //降落伞-降落时向左移动 (左上-左下之间)，人物朝z轴左旋转
    FallingMoveRight = 1009, //降落伞-降落时向右移动  (右下-右上之间)，人物朝z轴右旋转

    FallingPreLand = 1010, //降落伞-降落落地翻滚
}

#region 降落伞状态基类

public abstract class ParachuteStateBase
{
    [HideInInspector] public PlayerBaseControl playerBase;
    [HideInInspector] public Animator playerAnim;
    [HideInInspector] public AnimationController animCon;
    [HideInInspector] public CharacterController character;
    [HideInInspector] public Transform pTrans;

    //已滑翔时间
    public float CurGlideTime;
    public bool ctrlPanelIsShow;

    [HideInInspector] public bool IsLanding = false; //是否正在落地
    [HideInInspector] public bool IsParachuteOpened; //是否已经开伞

    public ParachuteMoveState StateName = ParachuteMoveState.Ready;

    protected ParachuteStateBase()
    {
        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        character = playerBase.Character;
        animCon = playerBase.animCon;
        pTrans = playerAnim.gameObject.transform;

        StateName = ParachuteMoveState.Ready;
    }

    #region 动画控制

    protected void ChangePlayerAnimState(ParachuteAnimState newState)
    {
        if (playerBase == null)
        {
            return;
        }

        playerBase.PlayAnimationById(AnimId.Parachute, (int) newState);
    }

    protected void CrossFadeAnim(string stateName, float duration)
    {
        if (playerAnim == null || playerAnim.gameObject.activeSelf == false)
        {
            return;
        }

        playerAnim.Update(0f);
        playerAnim.CrossFadeInFixedTime(stateName, duration);
    }

    #endregion
    
    #region UI控制

    protected void RefreshPlayModePanel()
    {
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.SetOnParachute();
        }
    }

    #endregion

    #region 工具方法

    public abstract float GetPreLandHeight();
    public abstract void StopAllTimer();

    public void ForceStopParachute(bool shouldResetIdle = true)
    {
        StopAllTimer();
        ParachuteManager.Inst.StopAllSound(playerBase.gameObject);


        ParachuteCtrlPanel.Hide();
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.BtnPanel.SetActive(true);
        }

        CurGlideTime = 0f;
        ctrlPanelIsShow = false;
        IsParachuteOpened = false;
        IsLanding = false;
        PlayerParachuteControl.Inst.SetFrameState(FrameStateType.NoState);
        PlayerParachuteControl.Inst.SwitchState(ParachuteMoveState.Ready);
        RefreshPlayModePanel();
        
        //降落伞收起
        ParachuteManager.Inst.SwitchParachute(GameManager.Inst.ugcUserInfo.uid, false);

        if (playerBase && playerBase.gameObject.activeSelf)
        {
            ChangePlayerAnimState(ParachuteAnimState.NoUse);
            playerBase.upwardVec.y = -1f;
            playerBase.gravity = Physics.gravity.y;
            playerBase.moveVec = Vector3.zero;

            if (shouldResetIdle)
            {
                playerBase.PlayerResetIdle();
            }
        }
    }

    public void OnChangeMode()
    {
        ForceStopParachute();
        RefreshPlayModePanel();
    }

    #endregion

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Move(ref Vector3 moveVec, ref Vector3 upwardVec);
    public abstract void Rotate(Vector3 screenOffset);
    public abstract void LandingStart(Action cb);
}

public class ParachuteReadyState : ParachuteStateBase
{
    public ParachuteReadyState() : base()
    {
        StateName = ParachuteMoveState.Ready;
    }

    public override float GetPreLandHeight()
    {
        return 0f;
    }

    public override void StopAllTimer()
    {
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void Move(ref Vector3 moveVec, ref Vector3 upwardVec)
    {
    }

    public override void Rotate(Vector3 screenOffset)
    {
    }

    public override void LandingStart(Action cb)
    {
    }
}

public class ParachuteGlidingState : ParachuteStateBase
{
    #region params

    //滑翔x秒后，可以开伞
    public float CanOpenParachuteUITime = 1f;

    //滑翔时，不操作摇杆，使用此加速度
    public float GlidingIdle_Y_AddSpeed_Factor = 0.008f;
    public float GlidingIdle_Y_AddSpeed;

    //滑翔时，操作摇杆，使用此加速度
    public float GlidingMove_Y_AddSpeed_Factor = 0.012f;
    public float GlidingMove_Y_AddSpeed;


    //滑翔时，操作摇杆，使用此移动速度
    public float Gliding_Move_Speed = 16f;

    //滑翔时，进入翻滚落地动作的高度
    public float Gliding_Preland_Height = 4f;

    #endregion

    private BudTimer startLandingTimer;
    private BudTimer endLandingTimer;


    public ParachuteGlidingState() : base()
    {
        StateName = ParachuteMoveState.Gliding;

        GlidingIdle_Y_AddSpeed = Physics.gravity.y * GlidingIdle_Y_AddSpeed_Factor;
        GlidingMove_Y_AddSpeed = Physics.gravity.y * GlidingMove_Y_AddSpeed_Factor;
    }

    public override float GetPreLandHeight()
    {
        return Gliding_Preland_Height;
    }

    public override void StopAllTimer()
    {
        TimerManager.Inst.Stop(startLandingTimer);
        TimerManager.Inst.Stop(endLandingTimer);
    }

    public override void Enter()
    {
        CurGlideTime = 0;
        ctrlPanelIsShow = false;
        playerBase.gravity = GlidingIdle_Y_AddSpeed;

        ChangePlayerAnimState(ParachuteAnimState.GlidingIdle);
        CrossFadeAnim(ParachuteManager.StateStr_ParachuteGlideIdle, 0.4f);

        playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.ParachuteGlideIdle);
        RefreshPlayModePanel();
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        ParachuteManager.Inst.PlaySound("Play_Parachute_Gliding_Fly", playerBase.gameObject);
        
        //滑翔后隐藏丢弃按钮
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.BtnPanel.SetActive(false);
        }
    }

    public override void Exit()
    {
        ForceStopParachute();
    }

    public override void Move(ref Vector3 moveVec, ref Vector3 upwardVec)
    {
        if (IsLanding)
        {
            moveVec = Vector3.zero;
            return;
        }

        if (playerBase == null || playerBase.isFlying || playerBase.isGround)
        {
            return;
        }

        var moveMag = moveVec.magnitude;
        if (moveMag > 0.1f)
        {
            playerBase.gravity = GlidingMove_Y_AddSpeed;
            moveVec = moveVec.normalized * Gliding_Move_Speed;
            playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.ParachuteGlideMove);

            ChangePlayerAnimState(ParachuteAnimState.GlidingMove);
            PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteGlidingMove);
        }
        else
        {
            playerBase.gravity = GlidingIdle_Y_AddSpeed;
            playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.ParachuteGlideIdle);

            ChangePlayerAnimState(ParachuteAnimState.GlidingIdle);
            PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteGlidingIdle);
        }

        //显示开伞按钮
        CurGlideTime += Time.deltaTime;
        if (!ctrlPanelIsShow && CurGlideTime >= CanOpenParachuteUITime)
        {
            ParachuteCtrlPanel.Show();
            ctrlPanelIsShow = true;
        }
    }

    public override void Rotate(Vector3 screenOffset)
    {
    }

    public override void LandingStart(Action cb)
    {
        PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteGlidingPreLand);
        ChangePlayerAnimState(ParachuteAnimState.GlidingPreLand);
        ParachuteManager.Inst.PlaySound("FrontFlip", "Play_Parachute_Ground", "Parachute_Ground", playerBase.gameObject);
        IsLanding = true;
        ParachuteCtrlPanel.Hide();

        startLandingTimer = TimerManager.Inst.RunOnce("startLandingTimer", 0.5f, () =>
        {
            // x s后开始播特效
            playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.ParachuteGlidePreLandEnd);
        });

        endLandingTimer = TimerManager.Inst.RunOnce("endLandingTimer", 1.5f, () =>
        {
            // 整个动作结束，完成落地流程
            Exit();
            RefreshPlayModePanel();
            if (cb != null)
            {
                cb.Invoke();
            }
        });
    }
}

public class ParachuteFallingState : ParachuteStateBase
{
    #region params

    private Quaternion resetQuat = Quaternion.Euler(0f, 0f, 0f);
    private float rotateSpeed = 10f;

    public float JoyStick_X_Max = 140f;
    public float JoyStick_X_Min = -140f;
    public float JoyStick_Y_Max = 140f;
    public float JoyStick_Y_Min = -140f;

    public float Falling_X_MaxAngle = 30f;
    public float Falling_X_MinAngle = -14f;
    public float Falling_Y_MaxAngle = 40f;
    public float Falling_Y_MinAngle = -40f;
    public float Falling_Z_MaxAngle = 20f;
    public float Falling_Z_MinAngle = -20f;

    //开伞后的下落速度--匀速
    public float Falling_Vertical_Idle_Speed = -2.0f;
    public float Falling_Vertical_Forward_Speed = -2.2f;

    public float Falling_Vertical_Backward_Speed = -1.8f;

    //开伞后的水平移动速度
    public float Falling_Horizontal_Forward_Speed = 8.0f;

    public float Falling_Horizontal_Backward_Speed = 5.0f;

    //降落时，进入翻滚落地动作的高度
    public float Falling_Preland_Height = 2f;

    private Vector3 v3Up = new Vector3(0f, 140f, 0f);

    #endregion

    private BudTimer ctrlPanelShowTimer;
    private BudTimer readyToFallTimer;
    private BudTimer startLandingTimer;
    private BudTimer endLandingTimer;
    private BudTimer closeParachuteTimer;


    public ParachuteFallingState() : base()
    {
        StateName = ParachuteMoveState.Falling;
    }

    public override float GetPreLandHeight()
    {
        return Falling_Preland_Height;
    }

    public override void StopAllTimer()
    {
        TimerManager.Inst.Stop(ctrlPanelShowTimer);
        TimerManager.Inst.Stop(readyToFallTimer);
        TimerManager.Inst.Stop(startLandingTimer);
        TimerManager.Inst.Stop(endLandingTimer);
        TimerManager.Inst.Stop(closeParachuteTimer);
    }

    #region 旋转计算

    private float GetXRot(float screenOffsetY)
    {
        if (screenOffsetY > 0)
        {
            var jY = Mathf.Min(screenOffsetY, JoyStick_Y_Max);
            var desXRot = Falling_X_MaxAngle * jY / JoyStick_Y_Max;
            return desXRot;
        }
        else
        {
            var jY = Mathf.Max(screenOffsetY, JoyStick_Y_Min);
            var desXRot = Falling_X_MinAngle * jY / JoyStick_Y_Min;
            return desXRot;
        }
    }

    private float GetYRot(float screenOffsetX)
    {
        if (screenOffsetX > 0)
        {
            var jX = Mathf.Min(screenOffsetX, JoyStick_X_Max);
            var retY = Falling_Y_MaxAngle * jX / JoyStick_X_Max;
            return retY;
        }
        else
        {
            var jX = Mathf.Max(screenOffsetX, JoyStick_X_Min);
            var retY = Falling_Y_MinAngle * jX / JoyStick_X_Min;
            return retY;
        }
    }

    private float GetZRot(float screenOffsetX)
    {
        if (screenOffsetX > 0)
        {
            var jZ = Mathf.Min(screenOffsetX, JoyStick_X_Max);
            var retZ = Falling_Z_MinAngle * jZ / JoyStick_X_Max;
            return retZ;
        }
        else
        {
            var jZ = Mathf.Max(screenOffsetX, JoyStick_X_Min);
            var retZ = Falling_Z_MaxAngle * jZ / JoyStick_X_Min;
            return retZ;
        }
    }

    // 开伞降落过程，松开摇杆，恢复Rotate
    public void ResetRotate()
    {
        if (pTrans)
        {
            pTrans.localRotation = Quaternion.Lerp(pTrans.localRotation, resetQuat, Time.deltaTime * rotateSpeed * 0.5f);
        }
    }

    #endregion

    #region 根据不同摇杆方向切换动作

    private void ChangeAnimStateByJoyStick(Vector3 screenOffset)
    {
        if (playerBase == null)
        {
            return;
        }

        if (screenOffset == Vector3.zero || screenOffset.magnitude < 0.1f)
        {
            ChangePlayerAnimState(ParachuteAnimState.FallingIdle);
            return;
        }

        var yAngle = Vector3.Angle(v3Up, screenOffset);
        if (screenOffset.x > 0)
        {
            if (yAngle > 0 && yAngle < 45)
            {
                //朝上
                ChangePlayerAnimState(ParachuteAnimState.FallingMoveForward);
                PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteFallingMoveForward);
            }
            else if (yAngle >= 45 && yAngle <= 135)
            {
                //朝右
                ChangePlayerAnimState(ParachuteAnimState.FallingMoveRight);
                PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteFallingMoveRight);
            }
            else if (yAngle > 135 && yAngle < 180)
            {
                //朝下
                ChangePlayerAnimState(ParachuteAnimState.FallingMoveBackward);
                PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteFallingMoveBackward);
            }
        }
        else
        {
            if (yAngle > 0 && yAngle < 45)
            {
                //朝上
                ChangePlayerAnimState(ParachuteAnimState.FallingMoveForward);
                PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteFallingMoveForward);
            }
            else if (yAngle >= 45 && yAngle <= 135)
            {
                //朝左
                ChangePlayerAnimState(ParachuteAnimState.FallingMoveLeft);
                PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteFallingMoveLeft);
            }
            else if (yAngle > 135 && yAngle < 180)
            {
                //朝下
                ChangePlayerAnimState(ParachuteAnimState.FallingMoveBackward);
                PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteFallingMoveBackward);
            }
        }
    }

    #endregion

    public override void Enter()
    {
        //开伞
        ParachuteManager.Inst.SwitchParachute(GameManager.Inst.ugcUserInfo.uid, true);
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        ParachuteManager.Inst.StopAllSound(playerBase.gameObject);
        PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteFallingReady);
        ChangePlayerAnimState(ParachuteAnimState.FallingReady);
        playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.ParachuteOpenParachute);
        ParachuteManager.Inst.PlaySound("Play_Parachute_Open", playerBase.gameObject);

        ctrlPanelShowTimer = TimerManager.Inst.RunOnce("ctrlPanelShowTimer", 0.3f, () =>
        {
            ParachuteManager.Inst.PlaySound("Play_Parachute_Fall_Fly", playerBase.gameObject);
            ParachuteCtrlPanel.Hide();
            ctrlPanelIsShow = false;
        });       
        
        readyToFallTimer = TimerManager.Inst.RunOnce("readyToFallTimer", 1.7f, () =>
        {
            PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteFallingIdle);
            ChangePlayerAnimState(ParachuteAnimState.FallingIdle);
            playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.ParachuteFalling);

            IsParachuteOpened = true;
        });
    }

    public override void Exit()
    {
        ForceStopParachute();
    }

    public override void Move(ref Vector3 moveVec, ref Vector3 upwardVec)
    {
        if (IsLanding)
        {
            // 开伞落地往前带位移翻滚效果
            moveVec = pTrans.forward.normalized * 2f;
            return;
        }

        if (IsParachuteOpened == false)
        {
            return;
        }

        if (playerBase == null || playerBase.isFlying || playerBase.isGround)
        {
            return;
        }

        var moveMag = moveVec.magnitude;
        if (moveMag > 0.1f)
        {
            if (Vector3.Angle(moveVec.normalized, playerBase.transform.forward) <= 90)
            {
                //往前移动
                upwardVec.y = Falling_Vertical_Forward_Speed;
                moveVec = moveVec.normalized * Falling_Horizontal_Forward_Speed;
            }
            else
            {
                //往后移动
                upwardVec.y = Falling_Vertical_Backward_Speed;
                moveVec = moveVec.normalized * Falling_Horizontal_Backward_Speed;
            }
        }
        else
        {
            upwardVec.y = Falling_Vertical_Idle_Speed;
            PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteFallingIdle);
            ChangePlayerAnimState(ParachuteAnimState.FallingIdle);
            ResetRotate();
        }
    }

    public override void Rotate(Vector3 screenOffset)
    {
        if (pTrans == null || IsLanding == true)
        {
            return;
        }

        //人物前倾---x轴旋转---由摇杆y轴分量确定
        //人物左右旋转---z轴旋转---由摇杆x轴分量确定
        var oriRot = pTrans.localRotation;
        pTrans.localRotation = Quaternion.Lerp(oriRot, Quaternion.Euler(GetXRot(screenOffset.y), GetYRot(screenOffset.x), GetZRot(screenOffset.x)), Time.deltaTime * rotateSpeed);

        //根据摇杆偏移切换不同动作
        ChangeAnimStateByJoyStick(screenOffset);
    }

    public override void LandingStart(Action cb)
    {
        //收伞
        PlayerParachuteControl.Inst.SetFrameState(FrameStateType.ParachuteFallingPreLand);
        ParachuteManager.Inst.SwitchParachute(GameManager.Inst.ugcUserInfo.uid, false);

        playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.ParachuteCloseParachute);
        ParachuteManager.Inst.PlaySound("Play_Parachute_Disappear", playerBase.gameObject);
        IsLanding = true;
        ParachuteCtrlPanel.Hide();
        
        if (pTrans)
        {
            pTrans.localRotation = resetQuat;
        }

        closeParachuteTimer = TimerManager.Inst.RunOnce("closeParachuteTimer", 0.3f, () =>
        {
            //收伞后翻滚
            ChangePlayerAnimState(ParachuteAnimState.FallingPreLand);
            CrossFadeAnim(ParachuteManager.StateStr_ParachuteFallPreLand, 0.25f);
            ParachuteManager.Inst.PlaySound("Roll", "Play_Parachute_Ground", "Parachute_Ground", playerBase.gameObject);
            
            startLandingTimer = TimerManager.Inst.RunOnce("startLandingTimer", 0.3f, () =>
            {
                // x s后开始播特效
                playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.ParachuteFallingPreLandEnd);
            });
            endLandingTimer = TimerManager.Inst.RunOnce("endLandingTimer", 1.0f, () =>
            {
                // 整个动作结束，完成落地流程
                ForceStopParachute();
                RefreshPlayModePanel();
                if (cb != null)
                {
                    cb.Invoke();
                }
            });
        });
    }
}

#endregion

public class PlayerParachuteControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector] public static PlayerParachuteControl Inst;

    private ParachuteStateBase ReadyState;
    private ParachuteStateBase GlidingState;
    private ParachuteStateBase FallingState;

    public ParachuteStateBase CurrentState;
    [NonSerialized] public int CurAnimStateId = (int) ParachuteAnimState.NoUse;
    public FrameStateType CurFrameState = FrameStateType.NoState;

    //垂直下落速度大于值，进入滑翔
    public float EnterGlidingYSpeed = -10;
    private int raycastLayerMask = 0;
    private bool IsGlidingToastShow = false;
    
    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.Parachute, Inst);

        raycastLayerMask = LayerMask.GetMask("Terrain", "Model", "IceCube", "SpecialModel", "OtherPlayer", "Airwall");

        ReadyState = new ParachuteReadyState();
        GlidingState = new ParachuteGlidingState();
        FallingState = new ParachuteFallingState();

        CurrentState = ReadyState;

        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.InitSetOnParachute();
        }
        
    }

    private void Update()
    {
        DetectLanding();
    }

    private void OnDestroy()
    {
        if (PlayerControlManager.Inst)
        {
            CurrentState?.ForceStopParachute(false);
            CurrentState = ReadyState;
            IsGlidingToastShow = false;
            PlayerControlManager.Inst.RemovePlayerCtrlMgr(PlayerControlType.Parachute);
        }

        Inst = null;
    }

    public void OnChangeMode(GameMode gameMode)
    {
        if (CurrentState != null)
        {
            CurrentState.OnChangeMode();
            IsGlidingToastShow = false;
        }
    }

    public void SwitchState(ParachuteMoveState newState)
    {
        switch (newState)
        {
            case ParachuteMoveState.Ready:
                CurrentState = ReadyState;
                CurrentState.Enter();
                break;
            case ParachuteMoveState.Gliding:
                CurrentState = GlidingState;
                CurrentState.Enter();
                break;
            case ParachuteMoveState.Falling:
                CurrentState = FallingState;
                CurrentState.Enter();
                break;
        }
    }

    #region 外部模块接口

    public FrameStateType GetCurFrameState()
    {
        return CurFrameState;
    }

    //丢伞
    public void DropParachute()
    {
        ForceStopParachute(false);
        DestroyImmediate(this);
    }

    public void ForceStopParachute(bool shouldResetIdle = true)
    {
        CurrentState?.ForceStopParachute(shouldResetIdle);
        SwitchState(ParachuteMoveState.Ready);
        IsGlidingToastShow = false;
    }


    /// <summary>
    /// 点击开伞，切到开伞降落状态
    /// </summary>
    public void OpenParachute()
    {
        if (PlayerBaseControl.Inst && !PlayerBaseControl.Inst.isTps)
        {
            TipPanel.ShowToast("You could not parachute in first-person perspective");
            return;
        }
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (CurrentState.StateName != ParachuteMoveState.Gliding || CurrentState.playerBase == null)
        {
            return;
        }

        if (CurrentState.IsLanding)
        {
            LoggerUtils.Log("ParachuteControl: IsLanding, switch to falling interrupt!!");
            return;
        }

        if (CurrentState.IsParachuteOpened)
        {
            LoggerUtils.Log("ParachuteControl: Parachute already opened!!");
            return;
        }

        SwitchState(ParachuteMoveState.Falling); //需要这里直接改CurState，否则会被PlayMove又修改成glide
    }

    /// <summary>
    /// 是否正在使用，滑翔和开伞状态都会标记为正在使用
    /// </summary>
    public bool IsParachuteUsing()
    {
        return CurrentState != null && CurrentState.StateName != ParachuteMoveState.Ready;
    }

    public bool IsParachuteFalling()
    {
        return CurrentState != null && CurrentState.StateName == ParachuteMoveState.Falling;
    }
    
    public bool IsParachuteGliding()
    {
        return CurrentState != null && CurrentState.StateName == ParachuteMoveState.Gliding;
    }

    // 只有开伞后，才控制旋转
    public bool IsCanHandleRotate()
    {
        if (CurrentState != null
            && CurrentState.StateName == ParachuteMoveState.Falling
            && CurrentState.IsParachuteOpened)
        {
            return true;
        }

        return false;
    }

    public void IntoWater()
    {
        ForceStopParachute();
        if (CurrentState != null && CurrentState.playerAnim)
        {
            CurrentState.playerAnim.Update(0f);
            CurrentState.playerAnim.CrossFadeInFixedTime("swimming_idle", 0.5f);
        }
    }

    public void SetFrameState(FrameStateType stateType)
    {
        CurFrameState = stateType;
    }

    #endregion

    //旋转控制
    public void OnFallingRotate(Vector3 screenOffset)
    {
        CurrentState?.Rotate(screenOffset);
    }

    //即将落地检测，即将落地播放落地翻滚动画
    private void DetectLanding()
    {
        if (CurrentState == null)
        {
            return;
        }

        if (CurrentState.StateName != ParachuteMoveState.Gliding && CurrentState.StateName != ParachuteMoveState.Falling)
        {
            return;
        }

        if (CurrentState.playerBase == null || CurrentState.playerBase.gameObject.activeSelf == false)
        {
            return;
        }

        if (CurrentState.IsLanding)
        {
            return;
        }

        var position = CurrentState.playerBase.transform.position;
        var preLandHeight = CurrentState.GetPreLandHeight();
        bool isHit = Physics.Raycast(position, -Vector3.up, out var raycastHit, preLandHeight, raycastLayerMask);
        if (isHit)
        {
            if (PlayerSwimControl.Inst && PlayerSwimControl.Inst.isInWater)
            {
                return;
            }

            if (raycastHit.collider != null && raycastHit.collider.transform.parent.name == "trapbox")
            {
                return;
            }
            CurrentState.LandingStart(() => { SwitchState(ParachuteMoveState.Ready); });
        }
    }

    //移动控制
    public void OnPlayerMove(ref Vector3 moveVec, ref Vector3 upwardVec)
    {
        if (CurrentState == null)
        {
            return;
        }
        
        //蹦床时不进入滑翔
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.isBounceplankJumping)
        {
            return;
        }
        
        //第一视角不允许进滑翔
        if (PlayerBaseControl.Inst && !PlayerBaseControl.Inst.isTps)
        {
            return;
        }

        //返回出生点等黑屏过渡期间不处理
        if (BlackPanel.Instance && BlackPanel.Instance.gameObject.activeSelf)
        {
            return;
        }
        
        //自由落体->滑翔
        if (CurrentState.StateName == ParachuteMoveState.Ready)
        {
            if (upwardVec.y > 0 || upwardVec.y >= -1f)
            {
                return;
            }
            
            //此前若打开过表情UIpanel，进入自由落体就屏蔽掉
            if (upwardVec.y < -2f && EmoMenuPanel.Instance && EmoMenuPanel.Instance.gameObject.activeSelf)
            {
                EmoMenuPanel.Hide();
                PlayModePanel.Instance.EmoMenuPanelBecameVisible(false);
            }

            if (upwardVec.y <= EnterGlidingYSpeed)
            {
                //自拍模式不能进入滑翔
                if (StateManager.IsInSelfieMode)
                {
                    if (!IsGlidingToastShow)
                    {
                        SelfieModeManager.Inst.ShowSelfieModeToast();
                        IsGlidingToastShow = true;
                    }
                    return;
                }
                
                //进入滑翔前打断滑雪动作
                if (PlayerSnowSkateControl.Inst)
                {
                    PlayerSnowSkateControl.Inst.ForceStopSkatingAndLeave();
                }
                SwitchState(ParachuteMoveState.Gliding);
                SetFrameState(FrameStateType.ParachuteGlidingIdle);
            }
            else
            {
                IsGlidingToastShow = false;
            }
        }
        else
        {
            CurrentState.Move(ref moveVec, ref upwardVec);
        }
    }
}

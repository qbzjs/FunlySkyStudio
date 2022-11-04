using System;
using RTG;
using UnityEngine;

public enum SnowAnimState
{
    ForWoard = 1000,
    Left = 1001,
    Right = 1002,
}

/// <summary>
/// Author:Shaocheng
/// Description: 滑雪主控制脚本
/// Date: 2022-8-17 10:29:45
/// </summary>
public class PlayerSnowSkateControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector] public static PlayerSnowSkateControl Inst;
    [HideInInspector] public PlayerBaseControl playerBase;
    [HideInInspector] public Animator playerAnim;
    [HideInInspector] public AnimationController animCon;
    [HideInInspector] public Transform pTrans;
    [HideInInspector] public GameObject pTrigger;

    public FrameStateType CurFrameState = FrameStateType.NoState;

    [HideInInspector] public float jumpForce = 10f;
    private float oriJumpForce = 6f;
    private Quaternion lastRotate = Quaternion.Euler(Vector3.zero);

    #region 移动参数

    public float snow_max_speed = 12f;
    public float snow_max_speed_sqr = 12f * 12f;
    public float snow_add_speed = 0.6f;
    public float snow_del_speed = 0.2f;

    #endregion

    public float maxAngle = 1.5f;

    private BudTimer setEnterSkatingTimer;
    private BudTimer setLeaveSkatingTimer;

    private bool isSkating;
    private bool isPlayEnterSkating;
    private bool isPlayLeaveSkating;

    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.SnowSkate, Inst);
        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        animCon = playerBase.animCon;
        pTrigger = GameObject.Find("PTrigger");

        pTrans = playerAnim.gameObject.transform;
    }

    private void OnDestroy()
    {
        //这里再强调用一次，防止退出滑雪的timer还没跑完就离开雪方块要destroy了
        ForceStopSkating();
        
        if (PlayerControlManager.Inst)
        {
            PlayerControlManager.Inst.RemovePlayerCtrlMgr(PlayerControlType.SnowSkate);
        }

        Inst = null;
    }

    #region PlayerBaseControl接管

    // 接收和处理摇杆输入
    public void OnJoystickMove(ref Vector3 move, ref Vector3 curMoveVec, ref Vector3 moveVec, ref Vector3 upwardVec)
    {
        if (playerBase == null)
        {
            return;
        }
        
        if (!isPlayLeaveSkating)
        {
            HandleFastRun(playerBase.isFastRun);
        }

        CalVecParallelToSnow(ref moveVec);
        CalVecParallelToSnow(ref move);
        if (isSkating)
        {
            moveVec = curMoveVec + snow_add_speed * move.normalized;
        }

        OnPlayerOffset(curMoveVec, moveVec, playerBase.isFastRun);
    }

    public void OnPlayerRotate(Vector3 curmoveVec, Vector3 moveVec)
    {
        if (pTrans == null)
        {
            return;
        }

        var curSnowBev = SnowCubeManager.Inst.GetCurStandOnSnowCubeBev();
        if (curSnowBev == null)
        {
            return;
        }

        var normal = curSnowBev.transform.up;
        if (Vector3.Angle(normal, Vector3.up) > 90)
        {
            normal = -normal;
        }

        if (moveVec != Vector3.zero)
        {
            pTrans.rotation = Quaternion.LookRotation(moveVec.normalized, normal);
            PTriggerRotate();
            lastRotate = pTrans.rotation;
        }
        else
        {
            //松开摇杆时，保持松开摇杆前的人物朝向
            pTrans.rotation = Quaternion.LookRotation(curmoveVec.normalized, normal);
            lastRotate = Quaternion.Euler(Vector3.zero);
        }
    }

    // 处理人物Character最终移动效果
    public void OnPlayerMove(ref Vector3 moveVec, ref Vector3 upwardVec)
    {
        OnPlayerMoveOnSnow(ref moveVec, ref upwardVec);
        // CalVecParallelToSnow(ref moveVec);
    }

    public void OnClickJump()
    {
        if (playerBase != null &&
            playerBase.mAnimStateManager != null &&
            playerBase.mAnimStateManager.mskateboardAnim != null)
        {
            playerBase.mAnimStateManager.mskateboardAnim.Play(PlayerSnowSkateAnim.skiing_jump, 0);

            StopSkatingSound();
        }
    }

    #endregion

    #region 人物垂直雪面计算

    //计算平行于雪面的向量
    private void CalVecParallelToSnow(ref Vector3 v)
    {
        if (PlayerStandonControl.Inst == null ||
            PlayerStandonControl.Inst.GetStandOnType() != StandOnType.SnowCube)
        {
            return;
        }

        var curSnowBev = SnowCubeManager.Inst.GetCurStandOnSnowCubeBev();
        if (curSnowBev == null)
        {
            return;
        }

        var normalVec = curSnowBev.transform.up;
        
        //Debug.DrawRay(curSnowBev.gameObject.transform.position, normalVec, Color.blue);

        if (!IsCanStepOnSnowCube(normalVec))
        {
            return;
        }
        
        //已经是平行于雪面的，不做计算
        if (IsVec3Perp(v, normalVec))
        {
            return;
        }

        //在法线上的投影   
        var moveNor = v.normalized;
        var moveP1 = Vector3.Dot(moveNor, -normalVec.normalized) * -normalVec.normalized;

        //在法线垂直的平面上的投影方向
        var moveP2 = moveNor - moveP1;
        //水平移动方向平行于雪面
        v = moveP2.normalized * v.magnitude;

// #if UNITY_EDITOR
//         var pp = pTrans.position;
//         Debug.DrawRay(curSnowBev.transform.position, normalVec, Color.red);
//         Debug.DrawRay(pp, moveVec, Color.red);
//         Debug.DrawRay(pp, moveP1, Color.yellow);
//         Debug.DrawRay(pp, moveP2, Color.magenta);
// #endif
    }

    // 判断向量是否互相垂直
    private bool IsVec3Perp(Vector3 v1, Vector3 v2)
    {
        float value = Vector3.Dot(v1.normalized, v2.normalized);
        if (Mathf.Abs(value) == 0)
        {
            return true;
        }

        return false;
    }
    
    // 判断雪方块是否超过最大角度上限
    private bool IsCanStepOnSnowCube(Vector3 normal)
    {
        if (Vector3.Angle(Vector3.up, normal) > 80f)
        {
            return false;
        }
        return true;
    }

    private void PTriggerRotate()
    {
        if (pTrigger && pTrans && playerBase && playerBase.playerCenter)
        {
            pTrigger.transform.rotation = pTrans.rotation;
            pTrigger.transform.position = playerBase.playerCenter.transform.position;
        }
    }

    private void ResetPlayerPerp()
    {
        // 回到地面，人物旋转重置

        if (PlayerBaseControl.Inst && !PlayerBaseControl.Inst.isTps && playerBase)
        {
            playerBase.transform.localRotation = Quaternion.Euler(0, playerBase.transform.localRotation.eulerAngles.y, 0);
        }

        if (pTrans)
        {
            pTrans.localEulerAngles = new Vector3(0 , pTrans.localEulerAngles.y , 0);
            // pTrans.rotation = Quaternion.LookRotation(pTrans.forward, Vector3.up);
        }

        if (pTrigger)
        {
            pTrigger.transform.localPosition = Vector3.zero;
            pTrigger.transform.localEulerAngles = Vector3.zero;
        }

        lastRotate = Quaternion.Euler(Vector3.zero);
    } 

    #endregion

    #region 人物雪面上移动效果

    private void OnPlayerMoveOnSnow(ref Vector3 moveVec, ref Vector3 upwardVec)
    {
        if (isPlayLeaveSkating)
        {
            return;
        }

        if (moveVec.magnitude > 0.1f)
        {
            moveVec -= moveVec.normalized * snow_del_speed;
            //playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.Skate);
        }
        else
        {
            moveVec = Vector3.zero;
            //playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.Idle);
        }

        if (moveVec.sqrMagnitude > snow_max_speed_sqr)
        {
            moveVec = moveVec.normalized * snow_max_speed;
        }
    }

    //人物偏移
    public void OnPlayerOffset(Vector3 curMoveVec, Vector3 moveVec, bool isFastRun)
    {
        if (!isFastRun || isPlayEnterSkating || !playerBase.isGround)
        {
            return;
        }

        Vector3 result = Vector3.Cross(moveVec.normalized, curMoveVec.normalized);
        float angle = Vector3.Angle(curMoveVec.normalized, moveVec.normalized);
        var skateboardAnim = playerBase.mAnimStateManager.mskateboardAnim;
        if (angle > maxAngle)
        {
            if (CurFrameState != FrameStateType.SnowCubeFastRunForward)
            {
                PlaySkatingSound();
                return;
            }

            if (result.y > 0)
            {
                CurFrameState = FrameStateType.SnowCubeFastRunLeft;
                ChangePlayerAnimState(SnowAnimState.Left);
                SnowCubeManager.Inst.ChangeSkateboardState(skateboardAnim, (int) SnowAnimState.Left);
            }
            else
            {
                CurFrameState = FrameStateType.SnowCubeFastRunRight;
                ChangePlayerAnimState(SnowAnimState.Right);
                SnowCubeManager.Inst.ChangeSkateboardState(skateboardAnim, (int) SnowAnimState.Right);
            }
        }
        else if (CurFrameState != FrameStateType.SnowCubeFastRunForward)
        {
            CurFrameState = FrameStateType.SnowCubeFastRunForward;
            ChangePlayerAnimState(SnowAnimState.ForWoard);
            SnowCubeManager.Inst.ChangeSkateboardState(skateboardAnim, (int) SnowAnimState.ForWoard);
        }
        
        PlaySkatingSound();
    }

    #region 声音控制

    private enum SkatingSoundType
    {
        None,
        Forward,
        LeftOrRight,
    }

    private SkatingSoundType GetSoundType(FrameStateType fType)
    {
        if (fType == FrameStateType.SnowCubeFastRunForward)
        {
            return SkatingSoundType.Forward;
        }
        else if(fType == FrameStateType.SnowCubeFastRunLeft || fType == FrameStateType.SnowCubeFastRunRight)
        {
            return SkatingSoundType.LeftOrRight;
        }
        
        return SkatingSoundType.Forward;
    }
    
    private SkatingSoundType lastSoundType;
    
    private void PlaySkatingSound()
    {
        if (isSkating == false || playerBase == null || playerBase.PlayerIsGround() == false)
        {
            return;
        }
        
        var curSoundType = GetSoundType(CurFrameState);
        if (lastSoundType == curSoundType)
        {
            return;
        }

        lastSoundType = curSoundType;
        if (curSoundType == SkatingSoundType.Forward)
        {
            SnowCubeManager.Inst.PlaySkatingSound(true, playerAnim.gameObject, true);
        }
        else if(curSoundType == SkatingSoundType.LeftOrRight)
        {
            SnowCubeManager.Inst.PlaySkatingSound(true, playerAnim.gameObject, false);
        }
    }

    private void StopSkatingSound()
    {
        SnowCubeManager.Inst.StopSkatingSound(true, playerAnim.gameObject);
        lastSoundType = SkatingSoundType.None;
    }
    
    #endregion

    protected void ChangePlayerAnimState(SnowAnimState newState)
    {
        if (playerBase == null)
        {
            return;
        }

        playerBase.PlayAnimationById(AnimId.RunOffset, (int) newState);
    }

    #endregion

    #region 动作切换&上下滑雪板

    public void HandleFastRun(bool isFastRun)
    {
        if (isFastRun)
        {
            EnterSkating();
        }
        else
        {
            ExitSkating();
        }
    }

    // 进入快跑带板滑雪
    public void EnterSkating()
    {
        if (!IsCanInSkating())
        {
            return;
        }
        isSkating = true;
        PlayerControlManager.Inst.ChangeAnimClips();
        playerAnim.Play(PlayerSnowSkateAnim.skiing_in);
        AKSoundManager.Inst.PostEvent(SnowCubeManager.SnowSound_Play_Snow_Skateboard_Appear, playerAnim.gameObject);
        playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.SnowOpenSkateboard);
        playerBase.jumpForce = jumpForce;
        CurFrameState = FrameStateType.SnowCubeGetOnBoard;
        isPlayEnterSkating = true;
        TimerManager.Inst.Stop(setEnterSkatingTimer);
        setEnterSkatingTimer = TimerManager.Inst.RunOnce("setSkatingTimer", 0.5f, () =>
        {
            isPlayEnterSkating = false;
            CurFrameState = FrameStateType.SnowCubeFastRunForward;
            PlaySkatingSound();
        });
    }

    // 退出滑雪
    // needAnim : 是否需要动画， false则强行隐藏滑雪板不做收板动作
    public void ExitSkating(bool needAnim = true, Action callback = null)
    {
        if (!isSkating)
        {
            callback?.Invoke();
            return;
        }

        if (needAnim)
        {
            CurFrameState = FrameStateType.SnowCubeGetOffBoard;
            playerAnim.Play(PlayerSnowSkateAnim.skiing_out);
            AKSoundManager.Inst.PostEvent(SnowCubeManager.SnowSound_Play_Snow_Skateboard_Disappear, playerAnim.gameObject);
            playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.SnowLeaveSkateboard);
            isPlayLeaveSkating = true;
            TimerManager.Inst.Stop(setLeaveSkatingTimer);
            setLeaveSkatingTimer = TimerManager.Inst.RunOnce("setLeaveSkatingTimer", 0.5f, () =>
            {
                isPlayLeaveSkating = false;
                isSkating = false;
                CurFrameState = FrameStateType.NoState;
                PlayerControlManager.Inst.ChangeAnimClips();
                StopSkatingSound();
                playerBase.mAnimStateManager.mskateboardObj.SetActive(false);
                callback?.Invoke();

            });
        }
        else
        {
            CurFrameState = FrameStateType.NoState;
            isPlayLeaveSkating = false;
            isSkating = false;
            if (playerBase.mAnimStateManager.mskateboardObj)
            {
                playerBase.mAnimStateManager.mskateboardObj.SetActive(false);
            }

            PlayerControlManager.Inst.ChangeAnimClips();
            StopSkatingSound();
        }

        isPlayEnterSkating = false;
        TimerManager.Inst.Stop(setEnterSkatingTimer);
        playerBase.jumpForce = oriJumpForce;
        playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.Idle);
        ChangePlayerAnimState(SnowAnimState.ForWoard);
    }

    public void EnterSnowCube()
    {
        StopAllTimer();
        ResetPlayerPerp();
        ResetSnowParams();
    }

    #endregion

    #region 外部方法

    public bool IsCanUseSnowMove()
    {
        var isStandOnSnow = PlayerStandonControl.Inst &&
                            PlayerStandonControl.Inst.CurStandOnWhat != null &&
                            PlayerStandonControl.Inst.CurStandOnWhat.StandOnType == StandOnType.SnowCube;
        var isRefer = ReferManager.Inst != null && ReferManager.Inst.isRefer;
        return isStandOnSnow && !isRefer;
    }


    public bool IsSnowSkating()
    {
        return isSkating;
    }

    public void LeaveSnowCube()
    {
        LoggerUtils.Log("PlayerSnowSkateControl: LeaveSnowCube");

        if (isSkating)
        {
            ExitSkating(true, () =>
            {
                ResetPlayerPerp();
                ResetSnowParams();
                
                DestroyImmediate(this);

            
            });
        }
        else
        {
            ResetPlayerPerp();
            ResetSnowParams();
            DestroyImmediate(this);
        }
    }

    public void OnStandAir()
    {
        // TODO:FSC 无法完全cover检测不到的情况
        // ResetPlayerPerp();
    }

    public void ResetSnowParams()
    {
        isSkating = false;
        isPlayEnterSkating = false;
        isPlayLeaveSkating = false;
        StopAllTimer();
    }

    //切回前台还在板子上，手动刷一下下板子
    public void OnForeground()
    {
        LoggerUtils.Log("PlayerSnowSkateControl: OnForeground");
        ExitSkating(false);
    }

    /// <summary>
    /// 外部强打断滑雪,不播放收板子动画强切
    /// </summary>
    public void ForceStopSkating()
    {
        LoggerUtils.Log("PlayerSnowSkateControl: ForceStopSkating");
        ExitSkating(false);
        ResetPlayerPerp();
        ResetSnowParams();
        ResetPlayer();
    }

    // 强打断并且destroy滑雪控制
    public void ForceStopSkatingAndLeave()
    {
        LoggerUtils.Log("PlayerSnowSkateControl: ForceStopSkatingAndLeave");
        ExitSkating(false);
        ResetPlayerPerp();
        ResetSnowParams();
        ResetPlayer();
        DestroyImmediate(this);
        
        if (PlayerStandonControl.Inst)
        {
            PlayerStandonControl.Inst.ResetStandOn();
        }
    }

    private void ResetPlayer()
    {
        GameUtils.ResetStayTime();
        playerBase.Move(Vector3.zero);
        // if (PlayerBaseControl.Inst)
        // {
        //     PlayerBaseControl.Inst.PlayerResetIdle();
        // }
    }

    public void StopAllTimer()
    {
        TimerManager.Inst.Stop(setEnterSkatingTimer);
        TimerManager.Inst.Stop(setLeaveSkatingTimer);
    }

    public FrameStateType GetCurFrameState()
    {
        return CurFrameState;
    }

    public void OnChangeTps()
    {
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.isTps)
        {
            if (playerBase && pTrans)
            {
                playerBase.transform.localRotation = Quaternion.Euler(0, playerBase.transform.localRotation.eulerAngles.y, 0);
                pTrans.localRotation = lastRotate;
            }
        }
    }

    public bool IsCanInSkating()
    {
        if (StateManager.IsEating)
        {
            return false; 
        }
        else if (StateManager.PlayerIsMutual)
        {
            return false;
        }
        else if (isSkating)
        {
            return false;
        }
        return true;
    }

    public bool IsPlayEnterOrLeaveAnim()
    {
        return isPlayEnterSkating || isPlayLeaveSkating;
    }

    #endregion
}
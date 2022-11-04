
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Author:WenJia
/// Description:Player 驾驶开车控制
/// 主要包含方向盘驾驶相关逻辑
/// Date: 2022/3/31 11:14:20
/// </summary>

public class PlayerDriveControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector]
    public PlayerBaseControl playerBase;
    [HideInInspector]
    public static PlayerDriveControl Inst;
    public CharacterController character;
    [HideInInspector]
    public SteeringWheelBehaviour steeringWheel;
    [HideInInspector]
    public Animator playerAnim;
    [HideInInspector]
    public AnimationController animCon;

    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.Drive, Inst);

        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        character = playerBase.Character;
        animCon = playerBase.animCon;
    }

    private void OnDestroy()
    {
        if (PlayerControlManager.Inst)
        {
            PlayerControlManager.Inst.RemovePlayerCtrlMgr(PlayerControlType.Drive);
        }
        Inst = null;
    }

    //上车
    public void OnSteering(SteeringWheelBehaviour sw)
    {
        playerBase.waitPosChange = true;
        steeringWheel = sw;
        playerBase.PlayAnimation(AnimId.IsGround, true);
        playerBase.PlayAnimation(AnimId.IsMoving, false);
        playerBase.isGround = true;
        playerBase.isMoving = false;
        playerBase.SetFly(false, true);
        playerAnim.transform.DOKill();
        AudioController.Inst.StopFlyAudio();
        character.enabled = false;
        playerBase.StopFootSound();
        if (PlayerSwimControl.Inst)
        {
            PlayerSwimControl.Inst.ForceOutWater();
        }
        if (!playerBase.isTps)
        {
            StartCoroutine(playerBase.SetPlayerRoleActive(0, true, AnimId.IsOnSteering, true));
        }
        if (playerBase.mAnimStateManager!=null)
        {
            playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.Idle);
        }
    }

    //下车
    public void GetOffSteering()
    {
        playerBase.waitPosChange = false;
        steeringWheel = null;
        playerBase.transform.rotation = Quaternion.identity;
        character.enabled = true;
        if (!playerBase.isTps && !PlayerControlManager.Inst.isPickedProp)
        {
            CoroutineManager.Inst.StartCoroutine(playerBase.SetPlayerRoleActive(0.1f, false));
        }

        // 移除驾驶控制脚本
        Destroy(this);
    }

    /**
    * 切换视角时，恢复驾驶状态
    */
    public void ChangeViewResetDriveState()
    {
        if (steeringWheel)
        {
            playerBase.PlayAnimation(AnimId.IsOnSteering, true);
        }
    }
}
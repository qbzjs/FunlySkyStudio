/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/9/4 19:22:47
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LadderAnimState
{
    NoUse = 1000,
    Down = 1001,
    Down_In = 1002,
    Down_Out = 1003,
    Up = 1004,
    Up_In = 1005,
    Up_Out = 1006,
    Idel_1 = 1007,
    Idel_2 = 1008
}
public class PlayerLadderControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector]
    public PlayerBaseControl playerBase;
    [HideInInspector]
    public static PlayerLadderControl Inst;
    [HideInInspector]
    public bool isOnLadder;
    [HideInInspector]
    public Animator playerAnim;
    [HideInInspector]
    public AnimationController animCon;
    public FrameStateType CurFrameState = FrameStateType.NoState;
    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.OnLadder, Inst);

        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        animCon = playerBase.animCon;
    }

    private void OnDestroy()
    {
        if (PlayerControlManager.Inst)
        {
            PlayerControlManager.Inst.RemovePlayerCtrlMgr(PlayerControlType.OnLadder);
        }
        CancelInvoke("PlayUpDownSound");
        Inst = null;
    }

    //上梯子
    public void OnLadder()
    {
        playerBase.waitPosChange = true;
        isOnLadder = true;
        playerBase.PlayAnimation(AnimId.IsGround, true);
        playerBase.PlayAnimation(AnimId.IsMoving, false);
        playerBase.isGround = true;
        playerBase.isMoving = false;
        playerBase.SetFly(false, true);
        if (playerBase.mAnimStateManager != null)
        {
            playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.Idle);
        }
        if (PlayerSwimControl.Inst)
        {
            playerBase.PlayAnimation(AnimId.IsInWater, false);
            PlayerSwimControl.Inst.ForceOutWater();
        }
        AudioController.Inst.StopFlyAudio();
        //取消双人动作发起状态（网络延迟处理）
        PlayerEmojiControl.Inst.CancelInteractEmo();
        AttackWeaponManager.Inst.SetAttackCtrPanelActive(false);
        ShootWeaponManager.Inst.SetAttackCtrPanelActive(false);
        StateManager.Inst.SetFishingCtrPanelVisibile(false);
        playerBase.PlayerResetIdle();
        playerAnim.transform.localRotation = new Quaternion(0, 0, 0, 0);
        

    }

    //下梯子
    public void UnBindLadder()
    {
        AttackWeaponManager.Inst.SetAttackCtrPanelActive(true);
        ShootWeaponManager.Inst.SetAttackCtrPanelActive(true);
        StateManager.Inst.SetFishingCtrPanelVisibile(true);
        playerBase.waitPosChange = false;
        isOnLadder = false;
        playerBase.transform.rotation = Quaternion.identity;
        SetRot();
        // 移除磁力板控制脚本
        DestroyImmediate(this);
    }
    public void SetRot()
    {
        if (!playerBase.isTps)
        {
            playerAnim.transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        //playerBase.PlayerResetIdle();
        playerAnim.transform.localRotation = new Quaternion(0, 0, 0, 0);
    }
    public void SetAnim(string newState)
    {
        playerAnim.CrossFadeInFixedTime(newState, 0.2f);
    }

    public FrameStateType GetCurFrameState()
    {
        return CurFrameState;
    }
    public void SetFrameState(FrameStateType stateType)
    {
        CurFrameState = stateType;
    }

    public void PlayUpDownSound()
    {
        if (IsInvoking("PlayUpDownSound"))
        {
            return;
        }
        if (LadderManager.Inst!=null&&isOnLadder)
        {
            LadderManager.Inst.PlayUpDownVoice();
        }
    }
    public void PlayInvokeUpDownSound()
    {
        Invoke("PlayUpDownSound", 0.27f);
    }


}

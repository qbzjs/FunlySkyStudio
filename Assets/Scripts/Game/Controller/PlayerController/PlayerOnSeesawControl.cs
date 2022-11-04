using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description: Player 跷跷板控制
/// 主要包含跷跷板相关逻辑
/// Date: 2022/9/9 14:24:55
/// </summary>

public class PlayerOnSeesawControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector]
    public static PlayerOnSeesawControl Inst;
    [HideInInspector]
    public PlayerBaseControl playerBase;
    [HideInInspector]
    public Animator playerAnim;
    [HideInInspector]
    public AnimationController animCon;
    [HideInInspector]
    public bool isOnSeesaw;
    int dir = -1; // 跷跷板方向 右边下压方向：-1， 左边下压方向：1
    BudTimer onSeesawTimer, leaveSeesawTimer, changeAnimTimer, resetIdleTimer;

    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.OnSeesaw, Inst);

        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        animCon = playerBase.animCon;
    }

    private void OnDestroy()
    {
        if (PlayerControlManager.Inst)
        {
            PlayerControlManager.Inst.RemovePlayerCtrlMgr(PlayerControlType.OnSeesaw);
        }
        Inst = null;
    }

    //上跷跷板
    public void OnSeesaw(bool isR)
    {
        playerBase.waitPosChange = true;
        isOnSeesaw = true;
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
        playerBase.PlayerResetIdle();
        playerAnim.transform.localRotation = new Quaternion(0, 0, 0, 0);
        PlayerBaseControl.Inst.animCon.PlayAnim((int)EmoName.EMO_SEESAW_ANIM);
        animCon.PlayEyeAnim();
        AKSoundManager.Inst.PlaySeesawSound("Play_Seesaw_Sitdown", playerAnim.gameObject);
        ClearTimer();
        changeAnimTimer = TimerManager.Inst.RunOnce("changeAnim", 1f, () =>
        {
            PlayerControlManager.Inst.ChangeAnimClips();
            playerAnim.Play("seesaw_centre", 1, 0f);
            ClearTimer();
            resetIdleTimer = TimerManager.Inst.RunOnce("resetIdle", 0.1f, ()=>{
                playerAnim.CrossFade("idle", 0.2f, 0, 0f);
            });
        });
        //上板后隐藏丢弃按钮
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.BtnPanel.SetActive(false);
        }
    }

    // 离开跷跷板，解除绑定
    public void LeaveSeesaw()
    {
        AttackWeaponManager.Inst.SetAttackCtrPanelActive(true);
        ShootWeaponManager.Inst.SetAttackCtrPanelActive(true);
        //下板后显示丢弃按钮
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.BtnPanel.SetActive(true);
        }

        ClearTimer();
        playerAnim.Play("seesaw_end", 0, 0);
        isOnSeesaw = false;
        playerBase.waitPosChange = false;
        leaveSeesawTimer = TimerManager.Inst.RunOnce("leaveSeesaw", 0.5f, () =>
        {
            AKSoundManager.Inst.PlaySeesawSound("Play_Seesaw_Standup", playerAnim.gameObject);

            PlayerControlManager.Inst.ChangeAnimClips();
            if (PlayerStandonControl.Inst)
            {
                PlayerStandonControl.Inst.ResetStandOn();
            }

            animCon.PlayEyeAnim();
            playerAnim.CrossFade("idle", 0.2f, 0, 0f);
            playerBase.animCon.ReleaseAndCancelLastEmo();
            // 移除跷跷板控制脚本
            DestroyImmediate(this);
        });
    }

    public void ForceLeaveSeesaw()
    {
        AttackWeaponManager.Inst.SetAttackCtrPanelActive(true);
        ShootWeaponManager.Inst.SetAttackCtrPanelActive(true);
        //下板后显示丢弃按钮
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.BtnPanel.SetActive(true);
        }

        playerAnim.Play("seesaw_end", 0, 0);
        isOnSeesaw = false;
        playerBase.waitPosChange = false;
        PlayerControlManager.Inst.ChangeAnimClips();
        if (PlayerStandonControl.Inst)
        {
            PlayerStandonControl.Inst.ResetStandOn();
        }

        animCon.PlayEyeAnim();
        playerAnim.CrossFade("idle", 0.2f, 0, 0f);
        playerBase.animCon.ReleaseAndCancelLastEmo();
        // 移除跷跷板控制脚本
        DestroyImmediate(this);
    }

    //下压跷跷板
    public void PushSeesaw()
    {
        PlayerBaseControl.Inst.animCon.PlayAnim((int)EmoName.EMO_SEESAW_PUSH);
        AKSoundManager.Inst.PlaySeesawSound("Play_Seesaw_Pushdown", gameObject);
    }

    void LateUpdate()
    {
        if (isOnSeesaw && !playerBase.isTps)
        {
            var playerTransPos = playerAnim.transform.parent.TransformPoint(playerAnim.transform.localPosition);
            playerBase.transform.localPosition = playerTransPos;
        }
    }

    public void ClearTimer()
    {
        TimerManager.Inst.Stop(onSeesawTimer);
        TimerManager.Inst.Stop(leaveSeesawTimer);
        TimerManager.Inst.Stop(changeAnimTimer);
        TimerManager.Inst.Stop(resetIdleTimer);
    }
}
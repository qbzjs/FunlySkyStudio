using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOnSwingControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector]
    public static PlayerOnSwingControl Inst;
    public PlayerBaseControl playerBase;
    [HideInInspector]
    public Animator playerAnim;
    [HideInInspector]
    public AnimationController animCon;
    [HideInInspector] public bool isOnSwing;
    
    BudTimer onSwingTimer, leaveSwingTimer, changeAnimTimer;


    public void OnSwing()
    {
        playerBase.waitPosChange = true;
        isOnSwing = true;
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
        PlayerEmojiControl.Inst.CancelInteractEmo();
        AttackWeaponManager.Inst.SetAttackCtrPanelActive(false);
        ShootWeaponManager.Inst.SetAttackCtrPanelActive(false);
        playerBase.PlayerResetIdle();
        playerAnim.transform.localRotation = new Quaternion(0, 0, 0, 0);
        AKSoundManager.Inst.PlaySwingSound("Play_Swing_Sitdown", gameObject);
        playerAnim.Play("swing_sitdown", 0, 0f);
        // playerAnim.Play("swing_sitdown", 1, 0f);
        ClearTimer();
        changeAnimTimer = TimerManager.Inst.RunOnce("changeAnim", 0.667f, () =>
        {
            PlayerControlManager.Inst.ChangeAnimClips();
            playerAnim.CrossFade("swing_idle", 0.2f, 0, 0f);
            animCon.PlayEyeAnim();
        });

    }
    
    public void LeaveSwing()
    {
        AttackWeaponManager.Inst.SetAttackCtrPanelActive(true);
        ShootWeaponManager.Inst.SetAttackCtrPanelActive(true);
        isOnSwing = false;
        playerBase.waitPosChange = false;
        
        ClearTimer();
        AKSoundManager.Inst.PlaySwingSound("Play_Swing_Standup", gameObject);
        playerAnim.Play("swing_getup", 0, 0);
        // playerAnim.Play("swing_getup", 1, 0);
        leaveSwingTimer = TimerManager.Inst.RunOnce("leaveSwing", 0.667f, () =>
        {
            PlayerControlManager.Inst.ChangeAnimClips();
            playerAnim.CrossFade("idle", 0.2f, 0, 0f);
            animCon.PlayEyeAnim();
            playerAnim.transform.localPosition = new Vector3(0, -0.95f, 0);
            Destroy(this);
        });
    }

    public void Playfront()
    {
        PlayerControlManager.Inst.ChangeAnimClips();
        AKSoundManager.Inst.PlaySwingSound("Play_Swing_Forward", gameObject);
        playerAnim.CrossFade("swing_front", 0.2f, 0, 0f);
        // animCon.playerAnim.Play("swing_front", 1, 0f);
    }
    
    public void PlayBack()
    {
        PlayerControlManager.Inst.ChangeAnimClips();
        AKSoundManager.Inst.PlaySwingSound("Play_Swing_Backward", gameObject);
        playerAnim.CrossFade("swing_back", 0.2f, 0, 0f);
        // animCon.playerAnim.Play("swing_back", 1, 0f);
    }
    
    public void PlayIdle()
    {
        PlayerControlManager.Inst.ChangeAnimClips();
        playerAnim.CrossFade("swing_idle", 0.2f, 0, 0f);
        // animCon.playerAnim.Play("swing_idle", 1, 0f);
    }


    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.Swing, Inst);

        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        animCon = playerBase.animCon;
    }
    
    private void OnDestroy()
    {
        if (PlayerControlManager.Inst)
        {
            PlayerControlManager.Inst.RemovePlayerCtrlMgr(PlayerControlType.Swing);
        }
        Inst = null;
    }
    
    public void ClearTimer()
    {
        TimerManager.Inst.Stop(onSwingTimer);
        TimerManager.Inst.Stop(leaveSwingTimer);
        TimerManager.Inst.Stop(changeAnimTimer);
    }
}

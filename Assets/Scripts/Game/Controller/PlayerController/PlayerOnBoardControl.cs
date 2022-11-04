using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Newtonsoft.Json;
using DG.Tweening;
using TMPro;
using Action = System.Action;
using BudEngine.NetEngine;

/// <summary>
/// Author:WenJia
/// Description:Player 磁力板控制
/// 主要包含磁力板相关逻辑
/// Date: 2022/3/31 11:14:20
/// </summary>

public class PlayerOnBoardControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector]
    public PlayerBaseControl playerBase;
    [HideInInspector]
    public static PlayerOnBoardControl Inst;
    [HideInInspector]
    public bool isOnBoard;
    [HideInInspector]
    public Animator playerAnim;
    [HideInInspector]
    public AnimationController animCon;

    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.OnBoard, Inst);

        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        animCon = playerBase.animCon;
    }

    private void OnDestroy()
    {
        if (PlayerControlManager.Inst)
        {
            PlayerControlManager.Inst.RemovePlayerCtrlMgr(PlayerControlType.OnBoard);
        }
        
        Inst = null;
    }

    //上板子（目前用于磁力版）
    public void OnBoard()
    {
        playerBase.waitPosChange = true;
        isOnBoard = true;
        playerBase.PlayAnimation(AnimId.IsGround, true);
        playerBase.PlayAnimation(AnimId.IsMoving, false);
        playerBase.isGround = true;
        playerBase.isMoving = false;
        playerBase.SetFly(false, true);
        if (playerBase.mAnimStateManager!=null)
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
    }

    //下板子（目前用于磁力版）
    public void UnBindBoard()
    {
        AttackWeaponManager.Inst.SetAttackCtrPanelActive(true);
        ShootWeaponManager.Inst.SetAttackCtrPanelActive(true);
        StateManager.Inst.SetFishingCtrPanelVisibile(true);
        playerBase.waitPosChange = false;
        isOnBoard = false;
        playerBase.transform.rotation = Quaternion.identity;
        if (!playerBase.isTps)
        {
            playerAnim.transform.rotation = new Quaternion(0, 0, 0, 0);
        }

        // 移除磁力板控制脚本
        DestroyImmediate(this);
    }

    public void PlayJumpOnBoard()
    {
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
        playerAnim.Play("jump 0");
        playerBase.PlayAnimation(AnimId.IsGround, false);
    }

    public void LandOnBoard()
    {
        playerBase.PlayAnimation(AnimId.IsGround, true);
    }
}
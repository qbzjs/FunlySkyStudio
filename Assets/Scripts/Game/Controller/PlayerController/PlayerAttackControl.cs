/// <summary>
/// Author:Mingo-LiZongMing
/// Description:玩家攻击控制
/// Date: 2022-4-14 17:44:22
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector]
    public static PlayerAttackControl Inst;
    [HideInInspector]
    public PlayerMeleeAttack curAttackPlayer;
    [HideInInspector]
    public PlayerBaseControl playerBase;
    [HideInInspector]
    public Animator playerAnim;
    [HideInInspector]
    public AnimationController animCon;

    public void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.Attack, Inst);
        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        animCon = playerBase.animCon;

        curAttackPlayer = new PlayerMeleeAttack(playerBase.gameObject);
        curAttackPlayer.animCon = animCon;
        curAttackPlayer.playerAnimator = playerAnim;
        curAttackPlayer.playerType = PlayerMeleeAttack.PlayerType.SelfPlayer;
        curAttackPlayer.PlayerId = GameManager.Inst.ugcUserInfo.uid;
        curAttackPlayer.OnEndAttackAction = playerBase.PlayerResetIdle;
        curAttackPlayer.OnBeforeUnderAttackAction = BeforeUnderAttack;
        curAttackPlayer.OnAfterUnderAttackAction = AfterUnderAttack;
    }

    public void OnDestroy()
    {
        if (PlayerControlManager.Inst)
        {
            PlayerControlManager.Inst.RemovePlayerCtrlMgr(PlayerControlType.Attack);
        }
        Inst = null;
    }

    public void OnWearWeapon(MeleeAttackWeapon weapon, int weaponUid)
    {
        curAttackPlayer.WearWeapon(weapon, weaponUid);
    }

    public void OnDropWeapon()
    {
        if (curAttackPlayer != null)
        {
            curAttackPlayer.DropWeapon();
        }
    }

    public void Attack()
    {
        LoggerUtils.Log("Player Attack");
        if (curAttackPlayer != null)
        {
            curAttackPlayer.Attack();
        }
    }

    public void UnderAttack(int dir)
    {
        LoggerUtils.Log("Player UnderAttack");
        if (curAttackPlayer != null)
        {
            curAttackPlayer.UnderAttack(1, dir, (int)HitPart.HEAD);
        }
    }


    private void BeforeUnderAttack()
    {
        AttackingPanel.Show();
        if (hideAttackForceCor != null)
        {
            CoroutineManager.Inst.StopCoroutine(hideAttackForceCor);
            hideAttackForceCor = null;
        }
        
        hideAttackForceCor = CoroutineManager.Inst.StartCoroutine(HideAttacking());
    }

    private void AfterUnderAttack()
    {
        AttackingPanel.Hide();
    }

    private Coroutine hideAttackForceCor;
    
    private IEnumerator HideAttacking()
    {
        yield return new WaitForSeconds(0.6f);
        AttackingPanel.Hide();
    }
}

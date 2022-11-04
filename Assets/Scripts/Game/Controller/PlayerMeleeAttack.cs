/// <summary>
/// Author:Mingo-LiZongMing
/// Description:玩家攻击、穿戴武器控制
/// Date: 2022-5-17 17:44:22
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using UnityEngine;

public class PlayerMeleeAttack : PlayerAttackBase
{
    private AttackData attackData;
    private string attackName = "pvp_attack";
    private string runAttackName = "pvp_runattack";
    private string underAttackName = "pvp_beattack";
    private string underAttackBackName = "pvp_beattackback";
    private string runUnderAttackName = "pvp_runbeattackback";
    public string PlayerId;
    public PlayerType playerType = PlayerType.SelfPlayer;
    public Action OnEndAttackAction;
    public Action OnBeforeUnderAttackAction;
    public Action OnAfterUnderAttackAction;
    public MeleeAttackWeapon HoldWeapon;
    public AnimationController animCon;
    public Animator playerAnimator;
    private List<GameObject> LimitTriggerGo = new List<GameObject>();
    private ParticleSystem effectPs, effectHit;
    private bool isAttacking = false;
    public Coroutine curAnimCoroutine;
    public Vec3[] hitPosList = new Vec3[]{
        new Vec3(0, 1.02f, 0), // 头部位置
        new Vec3(-0.236f, 0.503f, 0), // 左肩位置
        new Vec3(0.164f, 0.503f, 0),  // 右肩位置
    };
public PlayerMeleeAttack(GameObject player) : base(player)
    {
    }

    public void WearWeapon(MeleeAttackWeapon weapon, int uid)
    {
        HoldWeapon = weapon;
        HoldWeapon.OnTrigger = OnTriggerPlayer;
        HoldWeapon.weaponUid = uid;
        isAttacking = false;
    }

    public void DropWeapon()
    {
        HoldWeapon = null;
        isAttacking = false;
    }

    private void FinishAttackAnim()
    {
        isAttacking = false;
        HideHitEffect();
    }

    public void RefreshWeaponUI()
    {
        if (PlayerAttackControl.Inst == null || PlayerAttackControl.Inst.curAttackPlayer == null)
        {
            return;
        }
        var weapon = PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon;
        if (weapon == null)
        {
            return;
        }
        if (weapon.OpenDurability == 1)
        {
            AttackWeaponCtrlPanel.Instance.ShowWeaponHitsUI();
        }
    }
    
    //获取攻击动作真正的攻击范围判定帧区间
    //ret[0]:开始帧数 , ret[1]：x帧后结束判定
    private int[] GetAttackAnimFramePos()
    {
        if (IceCubeManager.Inst.IsPlayerStandOnIceCube(PlayerId))
        {
            return new []{26, 50}; //滑冰攻击
        }
        
        if (playerAnimator != null && playerAnimator.GetBool("IsMoving"))
        {
            return new []{0, 26}; //跑动攻击
        }
        else
        {
            return new []{26, 4}; //原地攻击
        }
    }

    public override void Attack()
    {
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
        // var isAttack = playerAnimator.GetBool("IsAttack");
        var isMoving = playerAnimator.GetBool("IsMoving");
        if (!isAttacking)
        {
            // LoggerUtils.Log("is not attacking ~~~~~~~~");

            var curAttackName = isMoving ? runAttackName : attackName;

            // 普通攻击动作 26帧 开始--attack 30帧 结束攻击
            var attackFrameRange = GetAttackAnimFramePos();
            var startAttackFrame = attackFrameRange[0];
            var endAttackFrame = attackFrameRange[1];

            //LoggerUtils.Log($"Attack()------startAttackFrame:{startAttackFrame},endAttackFrame:{endAttackFrame}");

            switch (playerType)
            {
                case PlayerType.SelfPlayer:
                    // playerAnimator.SetBool("IsAttack", true);
                    PlayAnim(playerAnimator, curAttackName, startAttackFrame, endAttackFrame, () =>
                    {
                        OnStartAttack();
                        AKSoundManager.Inst.PlayAttackSound("default", "play_pvp_attack_1p", "pvp_attack", CurPlayer);
                    }, OnEndAttack, FinishAttackAnim);
                    SendAttackMsg();
                    break;
                case PlayerType.OtherPlayer:
                    PlayAnim(playerAnimator, curAttackName, startAttackFrame, endAttackFrame, () =>
                    {
                        AKSoundManager.Inst.PlayAttackSound("default", "play_pvp_attack_3p", "pvp_attack", CurPlayer);
                    }, null, FinishAttackAnim);
                    break;
            }
        }
        else
        {
            // LoggerUtils.Log("is attacking ~~~~~~~~");
        }
    }

    public void SendAttackMsg()
    {
        if (GlobalFieldController.CurGameMode != GameMode.Guest) return;

        AttackWeaponItemData weaponItemData = new AttackWeaponItemData();
        weaponItemData.affectPlayers = new AttackWeaponAffectPlayerData[] { };
        var weaponUid = HoldWeapon.weaponUid;
        WeaponSystemController.Inst.SendWeaponAttackReq(ItemType.ATTACK_WEAPON, weaponUid, weaponItemData,
            (errCode, errMsg) => {
                LoggerUtils.Log($"SendWeaponAttackReq callback ,errorCode:{errCode}, {errMsg}");
            });
    }

    public void SetAttackCheckCanUse(bool canUse)
    {
        HoldWeapon.weaponCheck.gameObject.SetActive(canUse);
    }

    #region 普通攻击动作

    public void PlayAnim(Animator animator, string stateName, int startAttackFrameIndex, int endAttackFrameIndex, Action startAttack, Action endAttack, Action callback)
    {
        LoggerUtils.Log($"PlayAttackAnim : {stateName}, startFrame:{startAttackFrameIndex}, endFrame:{endAttackFrameIndex}");
        if (animator == null)
        {
            return;
        }

        if (animator.gameObject.activeSelf == false)
        {
            callback?.Invoke();
            return;
        }
        animCon.PlayAnim(null, stateName, -1);
        if (curAnimCoroutine != null)
        {
            CoroutineManager.Inst.StopCoroutine(curAnimCoroutine);
            curAnimCoroutine = null;
        }
        curAnimCoroutine = CoroutineManager.Inst.StartCoroutine(DelayRunEffectCallback(animator, stateName, startAttackFrameIndex, endAttackFrameIndex, startAttack, endAttack, callback));
    }

    //真正的攻击判定时间在 startAttackFrameIndex和endAttackFrameIndex之间
    //startAttack -- 开始攻击帧回调
    //endAttack -- 结束攻击帧回调
    //callback -- 结束整个攻击动画回调
    public IEnumerator DelayRunEffectCallback(Animator animator, string stateName, int startAttackFrameIndex, int endAttackFrameIndex, Action startAttack, Action endAttack, Action callback)
    {
        yield return new WaitForEndOfFrame();
        if (animator == null)
        {
            yield return null;
        }
        else
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(stateName))
            {
                isAttacking = true;

                if (startAttackFrameIndex != 0)
                {
                    yield return new WaitForSeconds(info.length * startAttackFrameIndex / 60f);
                }
                startAttack?.Invoke();

                if (endAttackFrameIndex != 0)
                {
                    yield return new WaitForSeconds(info.length * endAttackFrameIndex / 60f);
                    endAttack?.Invoke();

                    yield return new WaitForSeconds(info.length * (60 - startAttackFrameIndex - endAttackFrameIndex) / 60f);
                    callback?.Invoke();
                }
                else
                {
                    yield return new WaitForSeconds(info.length);
                    callback?.Invoke();
                }
            }   
        }
    }

    #endregion

    public void OnStartAttack()
    {
        LimitTriggerGo.Clear();
        HoldWeapon?.OnAttack();
    }

    public void OnEndAttack()
    {
        LimitTriggerGo.Clear();
        // OnEndAttackAction?.Invoke();
        HoldWeapon?.OnEndAttack();
        LoggerUtils.Log("Attack()------endAttack");
    }

    public void OnTriggerPlayer(Collider collider)
    {
        var other = collider.gameObject;
        if (playerType == PlayerType.OtherPlayer)
            return;
        if (LimitTriggerGo.Contains(other))
            return;
        LimitTriggerGo.Add(other);

        //TODO:SendMsgToCilent
        if ((other.layer == LayerMask.NameToLayer("OtherPlayer") || other.layer == LayerMask.NameToLayer("Touch")) && CanBeAttacked(other))
        {
            attackData.AttackDir = other.transform.position - CurPlayer.transform.position;
            attackData.AttackDir.y = 0;
            Vector3 playerDic = other.transform.forward;
            playerDic.y = 0;
            bool isForward = Vector3.Dot(attackData.AttackDir, playerDic) >= 0;
            attackData.AnimDir = isForward ? AttrackDirection.Forward : AttrackDirection.Back;
            OnHitOtherPlayer(other);

            var hitPos = collider.bounds.ClosestPoint(other.transform.position);
            LoggerUtils.Log("---------- OnTriggerPlayer ----------- hitPos is " + hitPos);
            // var attackPart = GetHitPart(hitPos);
            var attackPart = (int)HitPart.HEAD;
            // LoggerUtils.Log("---------- OnTriggerPlayer ----------- attackPart is " + attackPart);
            attackData.AttackPart = attackPart;
        }
    }

    public int GetHitPart(Vector3 closestPoint)
    {
        int index = 0;
        float minDis = (closestPoint - hitPosList[0]).sqrMagnitude;
        for (var i = 1; i < hitPosList.Length; i++)
        {
            var pos = hitPosList[i];
            float dis = (closestPoint - pos).sqrMagnitude;
            if (dis < minDis)
            {
                minDis = dis;
                index = i;
            }
        }

        return index;
    }

    public bool CanBeAttacked(GameObject other)
    {
        //TODO:待添加全场景玩家管理器

        var otherPlayerId = PlayerInfoManager.GetPlayerIdByObj(other);
        if (otherPlayerId == null)
        {
            return false;
        }
        var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(otherPlayerId);
        if (otherComp != null)
        {
            if (otherComp.steeringWheel != null)
            {
                return false;
            }

            if (MagneticBoardManager.Inst.IsOtherPlayerOnBoard(otherComp))
            {
                return false;
            }
            if (otherComp.mSlideMovementCompt!=null&&otherComp.mSlideMovementCompt.IsOnSlide())
            {
            	return false;
            }
            if(SeesawManager.Inst.IsOtherPlayerOnSeesaw(otherComp))
            {
                return false;
            }
        }

        return true;
    }

    public override void UnderAttack(int sendPlayer, int dir, int attackPart)
    {
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
        if (underAttackEffect) PlayUnderAttackEffect(sendPlayer, dir, attackPart);
    }

    /// <summary>
    /// 播放受击动作和音效
    /// </summary>
    /// <param name="dir">受击方向</param>
    private void PlayUnderAttackEffect(int sendPlayer, int dir, int attackPart)
    {
        var animName = string.Empty;
        switch (dir)
        {
            case (int)AttrackDirection.Forward:
                animName = underAttackBackName;
                break;
            case (int)AttrackDirection.Back:
                animName = underAttackName;
                break;
        }
        OnBeforeUnderAttackAction?.Invoke();
        var isMoving = playerAnimator.GetBool("IsMoving");
        var curAttackName = isMoving ? runUnderAttackName : animName;
        PlayAnim(playerAnimator, curAttackName, 0, 0, null, null, () =>
        {
            FinishAttackAnim();
            OnAfterUnderAttackAction?.Invoke();
        });
        PlayHitEffect(attackPart);
        switch (playerType)
        {
            case PlayerType.SelfPlayer:
                AKSoundManager.Inst.PlayAttackSound("default", "play_pvp_hit_1p", "pvp_hit", CurPlayer);
                break;
            case PlayerType.OtherPlayer:
                if ((PlayerType)sendPlayer == PlayerType.SelfPlayer)
                {
                    AKSoundManager.Inst.PlayAttackSound("default", "play_pvp_hit_1p", "pvp_hit", CurPlayer);
                }
                else
                {
                    AKSoundManager.Inst.PlayAttackSound("default", "play_pvp_hit_3p", "pvp_hit", CurPlayer);
                }
                break;
        }
    }

    public void PlayHitEffect(int hitPart = (int)HitPart.HEAD)
    {
        if (effectHit == null)
        {
            GameObject hitEffect = ResManager.Inst.LoadRes<GameObject>("Effect/pvp_beattack/pvp_beattack");
            var effect = GameObject.Instantiate(hitEffect, CurPlayer.transform);
            effectHit = effect.GetComponentInChildren<ParticleSystem>(true);
        }
        effectHit.transform.localPosition = hitPosList[hitPart];
        effectHit.gameObject.SetActive(true);
        effectHit.Play();
    }

    public void HideHitEffect()
    {
        if (effectHit != null)
        {
            effectHit.gameObject.SetActive(false);
        }
    }

    private void OnHitOtherPlayer(GameObject other)
    {
        AttackWeaponAffectPlayerData affectData = new AttackWeaponAffectPlayerData();
        affectData.PlayerId = PlayerInfoManager.GetPlayerIdByObj(other);
        affectData.AttackPlayerId = Player.Id;
        affectData.Damage = HoldWeapon.Damage;
        affectData.AnimDir = attackData.AnimDir;
        affectData.AttackDir = attackData.AttackDir;
        affectData.AttackPart = attackData.AttackPart;
        affectData.CurDurability = HoldWeapon.CurDurability;
        AttackWeaponItemData weaponItemData = new AttackWeaponItemData();
        weaponItemData.affectPlayers = new[]
        {
            affectData,
        };
        var weaponUid = HoldWeapon.weaponUid;
        WeaponSystemController.Inst.SendWeaponAttackReq(ItemType.ATTACK_WEAPON, weaponUid, weaponItemData, null);
    }

    public void OnDeath()
    {
        //玩家已死亡不处理
        if (PlayerManager.Inst.GetPlayerDeathState(PlayerId))
        {
            return;
        }
        PlayDeathPs();
        PlayerManager.Inst.OnPlayerDeath(PlayerId);
        AKSoundManager.Inst.PlayDeathSound(CurPlayer);
    }

    private void PlayDeathPs()
    {
        if (effectPs == null)
        {
            GameObject deathEffect = ResManager.Inst.LoadRes<GameObject>("Effect/death_smoke/death_smoke");
            var effect = GameObject.Instantiate(deathEffect, CurPlayer.transform);
            effectPs = effect.GetComponentInChildren<ParticleSystem>();
        }
        effectPs.Play();
    }

    private bool underAttackEffect = true;
    public void SwitchUnderAttackEffect(bool v)
    {
        underAttackEffect = v;
    }
}

public enum HitPart
{
    HEAD = 0,
    LEFT,
    RIGHT
}

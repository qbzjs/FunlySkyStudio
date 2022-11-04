using System.Collections;
﻿using System;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description:攻击道具管理
/// Date: 2022-4-14 17:44:22
/// </summary>
public class AttackWeaponManager : WeaponBaseManager<AttackWeaponManager>, IPVPManager
{
    public const float DAMAGE = 20.0f;
    public const float DURABILITY = 20;
    public Action OnHitUiEffect;

    public override void Init()
    {
        base.Init();
        LoggerUtils.Log($"AttackWeaponManager : Init");
    }

    public override void AddWeaponComponent(NodeBaseBehaviour nb, string rId)
    {
        if (nb != null)
        {
            var cmp = nb.entity.Get<AttackWeaponComponent>();
            var gComp = nb.entity.Get<GameObjectComponent>();
            gComp.modId = (int)GameResType.AttackWeapon;
            cmp.rId = rId;
            cmp.damage = DAMAGE;
            cmp.hits = DURABILITY;
            cmp.wType = (int)WeaponType.Attack;
            cmp.openDurability = 1;
            cmp.curHits = DURABILITY;
        }
    }

    public override NodeBaseBehaviour CreateDefaultNode()
    {
        var newBev = SceneBuilder.Inst.CreateSceneNode<AttackWeaponCreater, AttackWeaponDefaultBehaviour>();
        AttackWeaponCreater.SetData((AttackWeaponDefaultBehaviour)newBev, new AttackWeaponNodeData()
        {
            rId = DEFAULT_MODEL,
            damage = DAMAGE,
            oDur = 1,
            hits = DURABILITY,
        });
        return newBev;
    }


    public override void Release()
    {
        base.Release();
        LoggerUtils.Log($"AttackWeaponManager : Release");
    }
    
    #region UNDO/REDO

    public override void RevertNode(NodeBaseBehaviour behaviour)
    {
        base.RevertNode(behaviour);
        if (behaviour.entity.HasComponent<AttackWeaponComponent>())
        {
            var rid = behaviour.entity.Get<AttackWeaponComponent>().rId;
            AddUgcWeaponItem(rid, behaviour);
        }
    }

    public override void RemoveNode(NodeBaseBehaviour behaviour)
    {
        base.RemoveNode(behaviour);
    }

    #endregion


    public override void HandleWeaponBroadcast(string senderPlayerId, Item item)
    {
        base.HandleWeaponBroadcast(senderPlayerId, item);
        LoggerUtils.Log($"AttackWeapon HandleWeaponBroadcast :{senderPlayerId}=>{item.data}");

        //发起攻击方表现
        if (senderPlayerId != Player.Id)
        {
            var curAttackPlayer = GetCurAttackPlayer(senderPlayerId);
            if(curAttackPlayer != null)
            {
                curAttackPlayer.Attack();
            }
        }

        //受击方表现
        AttackWeaponItemData weaponItemData = JsonConvert.DeserializeObject<AttackWeaponItemData>(item.data);
        var affectPlayers = weaponItemData.affectPlayers;
        if (affectPlayers == null) return;
        foreach (var affectData in affectPlayers)
        {
            if (affectData == null) continue;

            LoggerUtils.Log("affectPlayerObj======>" + JsonConvert.SerializeObject(affectData));

            var playerId = affectData.PlayerId;
            if (string.IsNullOrEmpty(playerId)) continue;

            //TODO:后退方向，1.0暂不位移，只播放受击动作
            //SelfPlayer = 1, OtherPlayer = 2,
            var sendPlayer = senderPlayerId == Player.Id ? 1 : 2;
            HandleUnderAttackSync(sendPlayer,affectData);
        }
    }

    private void HandleUnderAttackSync(int sendPlayer,AttackWeaponAffectPlayerData affectData)
    {
        if (sendPlayer == 1 && PlayerParachuteControl.Inst)
        {
            //如果是降落伞使用中强打断降落伞
            PlayerParachuteControl.Inst.ForceStopParachute();
        }
        //打断钓鱼
        if (StateManager.IsFishing)
        {
            FishingManager.Inst.ForceStopFishing();
        }
        //播放受击动作
        PlayUnderAttackAnim(sendPlayer,affectData);

        var playerId = affectData.AttackPlayerId;
        var curAttackPlayer = GetCurAttackPlayer(playerId);
        // 更新耐久度
        if (curAttackPlayer != null && curAttackPlayer.HoldWeapon != null)
        {
            curAttackPlayer.HoldWeapon.CurDurability = affectData.CurDurability;
            var weapon = curAttackPlayer.HoldWeapon;
            var cmp = weapon.weaponBehaviour.entity.Get<AttackWeaponComponent>();
            cmp.curHits = weapon.CurDurability;

            if (weapon.OpenDurability == 1 && weapon.CurDurability <= 0)
            {
                CoroutineManager.Inst.StartCoroutine(weapon.PlayWeaponDestroyEffect());
                CoroutineManager.Inst.StartCoroutine(AttackWeaponManager.Inst.ShowWeaponDestroy(playerId));
            }
            // 刷新耐久度UI
            curAttackPlayer.RefreshWeaponUI();
        }

        //对局准备过程中不扣血
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && !PVPWaitAreaManager.Inst.IsPVPGameStart)
        {
            return;
        }

        //如果没有开启人物生命值 不处理扣血和死亡
        if (SceneParser.Inst.GetHPSet() != 0)
        {
            //处理玩家死亡状态
            HandlePlayerDeath(affectData);
            //处理牵手状态和非牵手状态的扣血以及死亡表现
            HandlePlayerHp(affectData);
        }
    }

    private void PlayUnderAttackAnim(int sendPlayer,AttackWeaponAffectPlayerData affectData)
    {
        var playerId = affectData.PlayerId;
        var animDir = affectData.AnimDir;
        var attackHit = affectData.AttackPart;
        var curAttackPlayer = GetCurAttackPlayer(playerId);
        if(curAttackPlayer != null)
        {
            curAttackPlayer.UnderAttack(sendPlayer, (int)animDir, attackHit);
        }
    }

    private void HandlePlayerHp(AttackWeaponAffectPlayerData affectData)
    {
        var playerId = affectData.PlayerId;
        var curBlood = affectData.CurBlood;
        PVPManager.Inst.UpdatePlayerHpShow(playerId, curBlood);
        if (playerId == Player.Id)
        {
            OnHitUiEffect?.Invoke();
        }
    }


    private void HandlePlayerDeath(AttackWeaponAffectPlayerData affectData)
    {
        var playerId = affectData.PlayerId;
        var alive = affectData.Alive;
        if(alive == 2)
        {
            var battleCtr = GetBattleCtr(playerId);
            var curAttackPlayer = GetCurAttackPlayer(playerId);
            if ((battleCtr != null) && (curAttackPlayer != null))
            {
                battleCtr.OnDeadEvent = curAttackPlayer.OnDeath;
                battleCtr.GetDeath(playerId);
                battleCtr.OnDeadEvent = null;
            }
        }
    }

    /// <summary>
    /// 获取CurAttack，武器能力 一定要判空
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    private PlayerMeleeAttack GetCurAttackPlayer(string playerId)
    {
        if (playerId == Player.Id)
        {
            if (PlayerAttackControl.Inst == null)
            {
                PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerAttackControl>();
            }
            return PlayerAttackControl.Inst.curAttackPlayer;
        }
        else
        {
            var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
            if (otherComp != null)
            {
                var attackCtr = otherComp.GetOtherPlayerAttackCtl();
                if (attackCtr != null)
                {
                    return attackCtr.curAttackPlayer;
                }
            }
        }
        return null;
    }

    private CharBattleControl GetBattleCtr(string playerId)
    {
        if (playerId == Player.Id)
        {
            return PlayerBaseControl.Inst.GetComponent<CharBattleControl>();
        }
        else
        {
            var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
            if(otherComp != null)
            {
                return otherComp.GetComponent<CharBattleControl>();
            }
        }
        return null;
    }

    protected override void OnChangeMode(GameMode mode)
    {
        base.OnChangeMode(mode);

        switch (mode)
        {
            case GameMode.Edit:
                ResetAllWeapons();
                SetAllAttackPropVisible(true);
                break;
            case GameMode.Play:
            case GameMode.Guest:
                ResetAllWeapons();
                SetAllAttackPropVisible(true);
                break;
        }
    }

    // 地图重置攻击道具信息
    public void OnReset()
    {
        ResetAllWeapons();
        SetAllAttackPropVisible(true);
    }

    public void ResetAllWeapons()
    {
        foreach (var weaponList in allWeaponsDict.Values)
        {
            foreach (var weapon in weaponList)
            {
                var cmp = weapon.entity.Get<AttackWeaponComponent>();
                cmp.curHits = cmp.hits;
            }
        }
        WeaponSystemController.Inst.OnReset();
    }

    public void SetAllAttackPropVisible(bool visible)
    {
        var allDefaultList = GetAllDefaultNodeBeav();
        foreach (var list in allWeaponsDict.Values)
        {
            foreach (var weaponBev in list)
            {
                if (weaponBev != null)
                {
                    if (allDefaultList != null && allDefaultList.Count > 0)
                    {
                        // 未添加 UGC 素材的道具的道具不处理
                        if (allDefaultList.Contains(weaponBev))
                        {
                            continue;
                        }
                    }
                    bool propIsVisible = visible;
                    // 被开关控制，且默认不可见，就隐藏
                    if (weaponBev.entity.HasComponent<ShowHideComponent>()
                    && weaponBev.entity.Get<ShowHideComponent>().defaultShow == 1)
                    {
                        propIsVisible = false;
                    }

                    weaponBev.gameObject.SetActive(propIsVisible);
                }
            }
        }
    }

    public IEnumerator ShowWeaponDestroy(string playerId)
    {
        yield return new WaitForSeconds(0.2f);
        var curAttackPlayer = GetCurAttackPlayer(playerId);
        if (curAttackPlayer != null)
        {
            curAttackPlayer.DropWeapon();
        }

        if (playerId == Player.Id)
        {
            PickabilityManager.Inst.OnAttackPropDestroy();
        }
    }

    /// <summary>
    /// 攻击道具是否脱离显隐控制
    /// 攻击道具没有添加UGC素材或道具损毁，则脱离控制
    /// </summary>
    /// <param name="behaviour"> 攻击道具 </param>
    /// <returns></returns>
    public bool IsAttackPropOutOfControl(NodeBaseBehaviour behaviour)
    {
        foreach (var weaponList in allWeaponsDict.Values)
        {
            if (weaponList.Contains(behaviour))
            {
                var cmp = behaviour.entity.Get<AttackWeaponComponent>();
                if (cmp.openDurability == 1 && cmp.curHits <= 0) //开启耐力值限制，且耐力值为0，则武器损毁,不被控制显隐
                {
                    return true;
                }
            }
        }

        // 未添加 UGC 素材的道具的显隐不应该被控制
        var defaultModels = GetAllDefaultNodeBeav();
        if (defaultModels != null && defaultModels.Count > 0)
        {
            if (defaultModels.Contains(behaviour))
            {
                return true;
            }
        }

        return false;
    }
}

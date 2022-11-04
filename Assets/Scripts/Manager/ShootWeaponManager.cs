/// <summary>
/// Author:Mingo-LiZongMing
/// Description:射击道具管理
/// Date: 2022-4-25 17:44:22
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;

public class ShootWeaponManager : WeaponBaseManager<ShootWeaponManager>, IPVPManager
{
    public const float DAMAGE = 5.0f;
    public const int cameraRotMax = 45;
    public const int DefCap = 30;
    public Action OnHitUiEffect;

    public override void Init()
    {
        base.Init();
        LoggerUtils.Log($"ShootWeaponManager : Init");
    }

    public override void AddWeaponComponent(NodeBaseBehaviour nb, string rId)
    {
        if (nb != null)
        {
            var cmp = nb.entity.Get<ShootWeaponComponent>();
            var gComp = nb.entity.Get<GameObjectComponent>();
            gComp.modId = (int)GameResType.ShootWeapon;
            cmp.rId = rId;
            cmp.damage = DAMAGE;
            cmp.wType = (int)WeaponType.Shoot;
            cmp.anchors = Vector3.zero;
            cmp.isCustomPoint = (int)CustomPointState.Off;

            cmp.hasCap = (int)CapState.HasCap;
            cmp.capacity = DefCap;
            cmp.fireRate = (int)FireRate.Medium;
            cmp.curBullet = cmp.capacity;
        }
    }

    public override NodeBaseBehaviour CreateDefaultNode()
    {
        var newBev = SceneBuilder.Inst.CreateSceneNode<ShootWeaponCreater, ShootWeaponDefaultBehaviour>();
        ShootWeaponCreater.SetData((ShootWeaponDefaultBehaviour)newBev, new ShootWeaponNodeData()
        {
            rId = DEFAULT_MODEL,
            damage = DAMAGE,
        });
        return newBev;
    }

    public override void Release()
    {
        base.Release();
        LoggerUtils.Log($"ShootWeaponManager : Release");
    }

    protected override void OnChangeMode(GameMode mode)
    {
        base.OnChangeMode(mode);
        OnReset();
    }

    public void OnReset()
    {
        foreach (var weaponList in allWeaponsDict.Values)
        {
            foreach (var weapon in weaponList)
            {
                if (weapon.entity.HasComponent<ShootWeaponComponent>())
                {
                    var cmp = weapon.entity.Get<ShootWeaponComponent>();
                    cmp.curBullet = cmp.capacity;
                }
            }
        }
        ShootWeaponFireManager.Inst.OnRest();
    }

    public Vector3 GetAnchors(SceneEntity entity)
    {
        var pCom = entity.Get<ShootWeaponComponent>();
        return pCom.anchors;
    }

    public void SetAnchors(SceneEntity entity, Vector3 pos)
    {
        if (!entity.HasComponent<ShootWeaponComponent>()) return;
        var pCom = entity.Get<ShootWeaponComponent>();
        pCom.anchors = pos;
    }

    #region UNDO/REDO

    public override void RevertNode(NodeBaseBehaviour behaviour)
    {
        base.RevertNode(behaviour);
        if (behaviour.entity.HasComponent<ShootWeaponComponent>())
        {
            var rid = behaviour.entity.Get<ShootWeaponComponent>().rId;
            AddUgcWeaponItem(rid, behaviour);
        }
    }

    public override void RemoveNode(NodeBaseBehaviour behaviour)
    {
        base.RemoveNode(behaviour);
    }

    #endregion

    public void SendOperateMsgToSever(NodeBaseBehaviour baseBev , OPERATE_TYPE opType)
    {
        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            return;
        }
        var entity = baseBev.entity;
        var gComp = entity.Get<GameObjectComponent>();
        var shootComp = entity.Get<ShootWeaponComponent>();

        PickPropSyncData pickPropSyncData = new PickPropSyncData();
        pickPropSyncData.playerId = Player.Id;
        pickPropSyncData.propType = (int)WeaponType.Shoot;
        pickPropSyncData.opType = (int)opType;
        pickPropSyncData.hasCapacity = shootComp.hasCap;
        pickPropSyncData.capacity = shootComp.capacity;
        pickPropSyncData.curBullet = shootComp.curBullet;

        Item itemData = new Item()
        {
            id = entity.Get<GameObjectComponent>().uid,
            type = (int)ItemType.WEAPON_OPERATE,
            data = JsonConvert.SerializeObject(pickPropSyncData),
        };
        Item[] itemsArray = { itemData };
        SyncItemsReq itemsReq = new SyncItemsReq()
        {
            mapId = GlobalFieldController.CurMapInfo.mapId,
            items = itemsArray,
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Items,
            data = JsonConvert.SerializeObject(itemsReq),
        };
        LoggerUtils.Log("SendOperateMsgToSever =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }

    public bool OnRecvOperateMsgFromSever(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("OnRecvOperateMsgFromSever " + msg);
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                if (item.type != (int)ItemType.WEAPON_OPERATE) { continue; }
                if (string.IsNullOrEmpty(item.data)) { continue; }
                var curAttackPlayer = GetCurAttackPlayer(senderPlayerId);
                if (curAttackPlayer == null) { continue; }
                if (senderPlayerId == Player.Id) { continue; }
                PickPropSyncData data = JsonConvert.DeserializeObject<PickPropSyncData>(item.data);
                var opType = data.opType;
                switch ((OPERATE_TYPE)opType) {
                    case OPERATE_TYPE.OnBtnDown:
                        ShootWeaponFireManager.Inst.AddPlayerInShootingList(curAttackPlayer);
                        curAttackPlayer.BulletCalibration(data.curBullet);
                        break;
                    case OPERATE_TYPE.OnBtnUp:
                        ShootWeaponFireManager.Inst.RemovePlayerInShootingList(curAttackPlayer);
                        curAttackPlayer.BulletCalibration(data.curBullet);
                        break;
                    case OPERATE_TYPE.StartReload:
                        curAttackPlayer.OnStartReload();
                        curAttackPlayer.BulletCalibration(data.curBullet);
                        break;
                    case OPERATE_TYPE.EndReload:
                        curAttackPlayer.onReloadComplete();
                        curAttackPlayer.BulletCalibration(data.curBullet);
                        break;
                    case OPERATE_TYPE.BulletCalibration:
                        curAttackPlayer.BulletCalibration(data.curBullet);
                        break;
                }
            }
        }
        return true;
    }

    public override void HandleWeaponBroadcast(string senderPlayerId, Item item)
    {
        base.HandleWeaponBroadcast(senderPlayerId, item);
        LoggerUtils.Log("senderPlayerId = " + senderPlayerId);
        LoggerUtils.Log($"ShootWeapon HandleWeaponBroadcast :{senderPlayerId}=>{item.data}");
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
            var sendPlayer = senderPlayerId == Player.Id ? 1 : 2;
            HandleUnderAttackSync(sendPlayer,affectData);
        }
    }

    private void HandleUnderAttackSync(int sendPlayer, AttackWeaponAffectPlayerData affectData)
    {
        //如果是降落伞使用中强打断降落伞
        if (affectData.PlayerId == Player.Id && StateManager.IsParachuteUsing)
        {
            PlayerParachuteControl.Inst.ForceStopParachute();
        }
        //打断钓鱼
        if (StateManager.IsFishing)
        {
            FishingManager.Inst.ForceStopFishing();
        }
        //播放受击动作
        PlayUnderAttackAnim(sendPlayer,affectData);
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
        if (alive == 2)
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

    private void PlayUnderAttackAnim(int sendPlayer, AttackWeaponAffectPlayerData affectData)
    {
        var playerId = affectData.PlayerId;
        var animDir = affectData.AnimDir;
        var curAttackPlayer = GetCurAttackPlayer(playerId);
        if (curAttackPlayer != null)
        {
            //TODO 需要对攻击人做处理
            curAttackPlayer.UnderAttack(sendPlayer, (int)animDir);
        }
    }

    /// <summary>
    /// 获取CurAttack，武器能力 一定要判空
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    private PlayerMeleeShoot GetCurAttackPlayer(string playerId)
    {
        if (playerId == Player.Id)
        {
            if (PlayerShootControl.Inst == null)
            {
                PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerShootControl>();
            }
            return PlayerShootControl.Inst.curShootPlayer;
        }
        else
        {
            var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
            if (otherComp != null)
            {
                var attackCtr = otherComp.GetOtherPlayerShootCtl();
                if (attackCtr != null)
                {
                    return attackCtr.curShootPlayer;
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
            if (otherComp != null)
            {
                return otherComp.GetComponent<CharBattleControl>();
            }
        }
        return null;
    }
}

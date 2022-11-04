using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description:各武器Manager统一管理，外部业务通过该脚本控制所有武器Manager
/// Date: 2022-4-14 17:44:22 
/// </summary>
public class WeaponSystemController : CInstance<WeaponSystemController>, IPVPManager
{
    private Dictionary<WeaponType, object> allWeaponManagers = new Dictionary<WeaponType, object>();
    private Dictionary<NodeBaseBehaviour, MeleeAttackWeapon> allWeapons = new Dictionary<NodeBaseBehaviour, MeleeAttackWeapon>();
    private Dictionary<NodeBaseBehaviour, MeleeShootWeapon> allShootWeapons = new Dictionary<NodeBaseBehaviour, MeleeShootWeapon>();


    public void AddWeaponManager<T>(WeaponType weaponType, T value) where T : WeaponBaseManager<T>, new()
    {
        if (!allWeaponManagers.ContainsKey(weaponType))
        {
            allWeaponManagers.Add(weaponType, value);
        }
    }

    public WeaponBaseManager<T> GetWeaponManager<T>(WeaponType weaponType) where T : WeaponBaseManager<T>, new()
    {
        if (allWeaponManagers.ContainsKey(weaponType))
        {
            return allWeaponManagers[weaponType] as WeaponBaseManager<T>;
        }

        return null;
    }

    public void Init()
    {
        //各武器Manager初始化
        AddWeaponManager(WeaponType.Attack, AttackWeaponManager.Inst);
        AddWeaponManager(WeaponType.Shoot, ShootWeaponManager.Inst);
        GetWeaponManager<AttackWeaponManager>(WeaponType.Attack).Init();
        GetWeaponManager<ShootWeaponManager>(WeaponType.Shoot).Init();
    }

    public override void Release()
    {
        base.Release();
        GetWeaponManager<AttackWeaponManager>(WeaponType.Attack)?.Release();
        GetWeaponManager<ShootWeaponManager>(WeaponType.Shoot)?.Release();
    }


    public void HandleWeaponClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        if (!IsWeaponNode(oBehaviour) || !IsWeaponNode(nBehaviour))
        {
            return;
        }

        var weaponType = GetWeaponTypeInEntity(nBehaviour.entity);
        if (weaponType == WeaponType.Attack)
        {
            var manager = GetWeaponManager<AttackWeaponManager>(weaponType);
            var rid = nBehaviour.entity.Get<AttackWeaponComponent>().rId;
            manager.AddUgcWeaponItem(rid, nBehaviour);
        }
        else if (weaponType == WeaponType.Shoot)
        {
            var manager = GetWeaponManager<ShootWeaponManager>(weaponType);
            var rid = nBehaviour.entity.Get<ShootWeaponComponent>().rId;
            manager.AddUgcWeaponItem(rid, nBehaviour);
        }
        //TODO:扩展其他武器类型
    }

    public WeaponType GetWeaponTypeInEntity(SceneEntity entity)
    {
        if (entity != null && entity.HasComponent<AttackWeaponComponent>())
        {
            return WeaponType.Attack;
        }
        else if (entity != null && entity.HasComponent<ShootWeaponComponent>())
        {
            return WeaponType.Shoot;
        }

        return WeaponType.NUll;
    }


    /// <summary>
    /// 通过entity是否有对应武器的Cmp,判断是否是武器节点
    /// </summary>
    /// <param name="behaviour"></param>
    public bool IsWeaponNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour == null) return false;

        //TODO:其他武器类型判断
        if (behaviour.entity.HasComponent<AttackWeaponComponent>())
        {
            return true;
        }
        else if (behaviour.entity.HasComponent<ShootWeaponComponent>())
        {
            return true;
        }
        else if (behaviour.entity.HasComponent<BloodPropComponent>())
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 进入拾取后，如果是武器道具，显示武器控制UI TODO:扩展多个武器道具
    /// </summary>
    public void HandleWeaponPick(NodeBaseBehaviour behaviour, bool isPick, string playerId)
    {
        if (!behaviour || !IsWeaponNode(behaviour))
        {
            return;
        }

        WeaponType wt = GetWeaponTypeInEntity(behaviour.entity);
        switch (wt)
        {
            case WeaponType.Attack:
                OnHandleAttackWeaponPick(behaviour, isPick, playerId);
                break;
            case WeaponType.Shoot:
                OnHandleShootWeaponPick(behaviour, isPick, playerId);
                break;
            default: break;
        }
    }

    /// <summary>
    /// 攻击道具拾起
    /// </summary>
    /// <param name="behaviour"></param>
    /// <param name="isPick"></param>
    /// <param name="playerId"></param>
    public void OnHandleAttackWeaponPick(NodeBaseBehaviour behaviour, bool isPick, string playerId)
    {
        if (isPick)
        {
            if (!allWeapons.ContainsKey(behaviour))
            {
                allWeapons.Add(behaviour, new MeleeAttackWeapon(behaviour));
            }

            MeleeAttackWeapon weapon = allWeapons[behaviour];
            weapon.OnCreate();
            int uid = behaviour.entity.Get<GameObjectComponent>().uid;
            if (playerId == GameManager.Inst.ugcUserInfo.uid)
            {
                if (PlayerAttackControl.Inst == null)
                {
                    PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerAttackControl>();
                }
                PlayerAttackControl.Inst.enabled = true;
                PlayerAttackControl.Inst.OnWearWeapon(weapon, uid);
                AttackWeaponCtrlPanel.Show();
                AttackWeaponCtrlPanel.Instance.CheckShowHide();
                PlayerControlManager.Inst.ChangeAnimClips();
            }
            else
            {
                var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
                if (otherComp != null)
                {
					var attackCtr = otherComp.GetOtherPlayerAttackCtl();
                    attackCtr.OnWearWeapon(weapon, uid);
                    otherComp.otherPlayerAttackCtr = attackCtr;
                    otherComp.SwitchAttackAnimClips();
                }
            }
        }
        else
        {
            if (allWeapons.ContainsKey(behaviour))
            {
                allWeapons[behaviour].UnBindWeapon();
            }

            if (playerId == GameManager.Inst.ugcUserInfo.uid)
            {
                if (PlayerAttackControl.Inst)
                {
                    PlayerAttackControl.Inst.OnDropWeapon();
                    AttackWeaponCtrlPanel.Hide();
                }
            }
            else
            {
                var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
                if (otherComp != null)
                {
                    var attackCtr = otherComp.GetComponentInChildren<OtherPlayerAttackCtr>();
                    if (attackCtr != null)
                    {
                        attackCtr.OnDropWeapon();
                        otherComp.SwitchNormalAnimClips();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 射击道具拾起
    /// </summary>
    /// <param name="behaviour"></param>
    /// <param name="isPick"></param>
    /// <param name="playerId"></param>
    public void OnHandleShootWeaponPick(NodeBaseBehaviour behaviour, bool isPick, string playerId)
    {
        if(!WeaponBulletManager.Inst.isInit)
        {
            LoggerUtils.Log("WeaponBulletManager 初始化子弹对象池");
            WeaponBulletManager.Inst.Init();
        }
        if (isPick)
        {
            if (!allShootWeapons.ContainsKey(behaviour))
            {
                allShootWeapons.Add(behaviour, new MeleeShootWeapon(behaviour));
            }

            MeleeShootWeapon weapon = allShootWeapons[behaviour];
            weapon.OnCreate();
            int uid = behaviour.entity.Get<GameObjectComponent>().uid;
            if (playerId == GameManager.Inst.ugcUserInfo.uid)
            {
                if (PlayerShootControl.Inst == null)
                {
                    PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerShootControl>();
                }
                PlayerShootControl.Inst.enabled = true;
                PlayerShootControl.Inst.OnWearWeapon(weapon, uid);
                ShootWeaponCtrlPanel.Show();
                ShootWeaponCtrlPanel.Instance.CheckShowHide();
                PlayerControlManager.Inst.ChangeAnimClips();
                if (CameraModeManager.Inst.GetCurrentCameraMode() == CameraModeEnum.FreePhotoCamera)
                {
                    CameraModeManager.Inst.EnterMode(CameraModeEnum.NormalGuestCamera);
                    TipPanel.ShowToast("Camera mode cannot be used in the game view of first-person.");
                }
            }
            else
            {
                var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
                if (otherComp != null)
                {
                    var shootCtr = otherComp.GetOtherPlayerShootCtl();
                    shootCtr.OnWearWeapon(weapon, uid);
                    otherComp.otherPlayerShootCtr = shootCtr;
                    otherComp.SwitchShootAnimClips();
                }
            }
        }
        else
        {
            if (allShootWeapons.ContainsKey(behaviour))
            {
                allShootWeapons[behaviour].UnBindWeapon();
            }

            if (playerId == GameManager.Inst.ugcUserInfo.uid)
            {
                if (PlayerShootControl.Inst)
                {
                    PlayerShootControl.Inst.OnDropWeapon();
                    ShootWeaponCtrlPanel.Hide();
                }
            }
            else
            {
                var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
                if (otherComp != null)
                {
                    var shootCtr = otherComp.GetComponentInChildren<OtherPlayerShootCtr>();
                    if (shootCtr != null)
                    {
                        shootCtr.OnDropWeapon();
                        otherComp.SwitchNormalAnimClips();
                    }
                }
            }
        }
    }
    #region 武器网络请求

    /// <summary>
    /// 发送武器攻击请求
    /// </summary>
    /// <param name="weaponDataType">武器消息类型,详见ItemType</param>
    /// <param name="weaponUid">武器Uid</param>
    /// <param name="weaponAttackData">武器攻击数据，不同武器攻击数据按需定义</param>
    /// <param name="callBack">回调方法，包含请求发送的错误码回包</param>
    public void SendWeaponAttackReq(ItemType weaponDataType, int weaponUid, object weaponAttackData, Action<int, string> callBack)
    {
        Item[] itemsArray =
        {
            new Item()
            {
                id = weaponUid,
                type = (int) weaponDataType,
                data = JsonConvert.SerializeObject(weaponAttackData),
            }
        };
        SyncItemsReq itemsReq = new SyncItemsReq()
        {
            mapId = GlobalFieldController.CurMapInfo.mapId,
            items = itemsArray,
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int) RecChatType.Items,
            data = JsonConvert.SerializeObject(itemsReq),
        };
        LoggerUtils.Log($"SendWeaponAttackReq => {JsonConvert.SerializeObject(roomChatData)}");
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), callBack);
    }

    /// <summary>
    /// 处理武器攻击广播
    /// </summary>
    /// <param name="senderPlayerId"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public bool HandleWeaponAttackBroadcast(string senderPlayerId, string msg)
    {
        LoggerUtils.Log($"HandleWeaponAttackBroadcast => senderPlayer:{senderPlayerId}, msg:{msg}");

        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);

#if !UNITY_EDITOR
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo == null || mapId != GlobalFieldController.CurMapInfo.mapId)
        {
            LoggerUtils.Log($"HandleWeaponAttackBroadcast , not current mapId:{mapId}  {GlobalFieldController.CurMapInfo.mapId}");
            return true;
        }
#endif

        foreach (var item in itemsReq.items)
        {
            if (item.type == (int) ItemType.ATTACK_WEAPON)
            {
                var manager = GetWeaponManager<AttackWeaponManager>(WeaponType.Attack);
                manager.HandleWeaponBroadcast(senderPlayerId, item);
            }
            //TODO:其他武器道具类型拓展
            if (item.type == (int)ItemType.SHOOT_WEAPON)
            {
                var manager = GetWeaponManager<ShootWeaponManager>(WeaponType.Shoot);
                manager.HandleWeaponBroadcast(senderPlayerId, item);
            }
        }

        return true;
    }

    public void OnReset()
    {
        ResetAllWeapons();
    }

    public void ResetAllWeapons()
    {
        foreach (var weapon in allWeapons.Values)
        {
            if (weapon == null)
            {
                continue;
            }
            var cmp = weapon.weaponBehaviour.entity.Get<AttackWeaponComponent>();
            weapon.Durability = cmp.hits;
            weapon.CurDurability = cmp.hits;
            weapon.OpenDurability = cmp.openDurability;
            weapon.Damage = cmp.damage;
        }
    }

    #endregion

    public void UpdateWeaponInfo(NodeBaseBehaviour baseBev, PickPropSyncData data)
    {
        var entity = baseBev.entity;
        // 更新攻击道具耐久度
        if (entity.HasComponent<AttackWeaponComponent>())
        {
            var attackCmp = entity.Get<AttackWeaponComponent>();
            attackCmp.curHits = data.curDurability;
            if (attackCmp.openDurability == 1 && attackCmp.curHits <= 0)
            {
                baseBev.gameObject.SetActive(false);
            }
        }

        // 更新射击道具信息
        if (entity.HasComponent<ShootWeaponComponent>())
        {
            var shootComp = entity.Get<ShootWeaponComponent>();
            shootComp.curBullet = data.curBullet;
        }
    }
}

public enum WeaponType
{
    NUll,
    Attack,
    Shoot
}
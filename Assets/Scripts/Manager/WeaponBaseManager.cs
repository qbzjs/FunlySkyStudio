using System;
using System.Collections.Generic;
using System.Linq;
using SavingData;

/// <summary>
/// Author:Shaocheng
/// Description:基础武器道具管理，提供一些武器道具的基础数据管理能力，例如攻击道具使用AttackWeaponManager继承WeaponBaseManager
/// Date: 2022-4-14 17:44:22
/// </summary>
public class WeaponBaseManager<T> : ManagerInstance<T>, IManager where T : BaseInstance, IManager, new()
{
    public const string DEFAULT_MODEL = "DEFAULT_MODEL";

    //攻击道具内存数据维护 -- 包含已替换的ugc 和 默认占位图
    //默认占位图的key--- DEFAULT_MODEL
    //UGC素材攻击道具的key--- Rid
    public Dictionary<string, List<NodeBaseBehaviour>> allWeaponsDict = new Dictionary<string, List<NodeBaseBehaviour>>();

    public UgcWeaponItem lastSelectWeapon;

    public Action HideWeaponCtrPanel;
    public Action ShowWeaponCtrPanel;

    public void SetLastSelectWeapon(UgcWeaponItem weaponItem)
    {
        if (weaponItem != null)
        {
            lastSelectWeapon = weaponItem;
            LoggerUtils.Log($"WeaponBaseManager: SetLastSelectWeapon-{weaponItem.mapInfo.mapId}");
        }
    }

    /// <summary>
    /// 获取上一次选中UGC的Rid，没有默认返回DEFAULT_MODEL
    /// </summary>
    /// <returns></returns>
    public string GetLastSelectWeaponRid()
    {
        if (lastSelectWeapon != null)
        {
            return lastSelectWeapon.mapInfo.mapId;
        }

        return DEFAULT_MODEL;
    }

    public UgcWeaponItem GetLastSelectUgcMapInfo()
    {
        return lastSelectWeapon;
    }

    /// <summary>
    /// 获取所有正在用作武器的UGC Rid集合
    /// 排除了默认占位道具
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllUgcWeaponRidList()
    {
        if (allWeaponsDict != null)
        {
            var tempList = allWeaponsDict.Keys.ToList();
            if (tempList.Contains(DEFAULT_MODEL))
            {
                tempList.Remove(DEFAULT_MODEL);
            }

            return tempList;
        }

        return null;
    }

    public WeaponType GetWeaponType(SceneEntity entity)
    {
        if (entity != null && entity.HasComponent<AttackWeaponComponent>())
        {
            return WeaponType.Attack;
        }
        if (entity != null && entity.HasComponent<ShootWeaponComponent>())
        {
            return WeaponType.Shoot;
        }

        return WeaponType.NUll;
    }

    #region Virtual Methods

    public virtual void Init()
    {
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
    }

    public virtual void AddWeaponComponent(NodeBaseBehaviour nb, string rId)
    {
    }

    public virtual NodeBaseBehaviour CreateDefaultNode()
    {
        return null;
    }

    #endregion


    public override void Release()
    {
        base.Release();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
    }

    /// <summary>
    /// 获取所有默认道具的Beav
    /// </summary>
    /// <returns></returns>
    public List<NodeBaseBehaviour> GetAllDefaultNodeBeav()
    {
        if (allWeaponsDict.ContainsKey(DEFAULT_MODEL))
        {
            return allWeaponsDict[DEFAULT_MODEL];
        }
        
        return null;
    }

    /// <summary>
    /// 根据素材id,添加UGC素材武器list
    /// </summary>
    /// <param name="rid"></param>
    public void AddUgcWeaponList(string rid)
    {
        if (!allWeaponsDict.ContainsKey(rid))
        {
            allWeaponsDict.Add(rid, new List<NodeBaseBehaviour>());
        }
    }

    /// <summary>
    /// 添加UGC武器节点
    /// </summary>
    /// <param name="rId"></param>
    /// <param name="ugcBeav"></param>
    public void AddUgcWeaponItem(string rId, NodeBaseBehaviour ugcBeav)
    {
        AddUgcWeaponList(rId);
        allWeaponsDict[rId].Add(ugcBeav);
        LoggerUtils.Log($"WeaponBaseManager: AddUgcWeaponItem --{rId}--{ugcBeav.gameObject.name}");
    }

    #region Message Listener
    /// <summary>
    /// 监听组合面板打开，打开组合，隐藏武器道具
    /// </summary>
    /// <param name="isShow"></param>
    protected virtual void HandlePackPanelShow(bool isShow)
    {
        foreach (var list in allWeaponsDict.Values)
        {
            foreach (var weaponBev in list)
            {
                if (weaponBev)
                {
                    weaponBev.gameObject.SetActive(!isShow);
                }
            }
        }
    }

    protected virtual void OnChangeMode(GameMode mode)
    {
        LoggerUtils.Log($"WeaponBaseManager : OnChangeMode --> {mode}");

        switch (mode)
        {
            case GameMode.Edit:
                SetDefaultModeShow(true);
                break;
            case GameMode.Play:
            case GameMode.Guest:
                SetDefaultModeShow(false);
                break;
        }
    }
    #endregion

    public void SetDefaultModeShow(bool isShow)
    {
        var defalutModels = GetAllDefaultNodeBeav();
        if (defalutModels != null)
        {
            foreach (var attackModel in defalutModels)
            {
                attackModel.gameObject.SetActive(isShow);
            }
        }
    }


    /// <summary>
    /// 移除武器道具，不区分默认道具和UGC
    /// </summary>
    public void RemoveWeaponItem(NodeBaseBehaviour nb)
    {
        foreach (var weapons in allWeaponsDict.Values)
        {
            if (weapons.Contains(nb))
            {
                weapons.Remove(nb);
                return;
            }
        }
    }

    /// <summary>
    /// 移除武器道具，具体到某个UGC素材
    /// </summary>
    public void RemoveWeaponItem(string rid, NodeBaseBehaviour nb)
    {
        if (allWeaponsDict.ContainsKey(rid))
        {
            var weaponList = allWeaponsDict[rid];
            if (weaponList.Contains(nb))
            {
                weaponList.Remove(nb);
            }
            else
            {
                LoggerUtils.Log($"WeaponBaseManager: RemoveWeaponItem {nb.name} not exist!");
            }
        }
    }


    #region UNDO/REDO

    public virtual void RemoveNode(NodeBaseBehaviour behaviour)
    {
        if (!behaviour || !WeaponSystemController.Inst.IsWeaponNode(behaviour))
        {
            return;
        }

        string removeIndex = "";
        foreach (var weaponKV in allWeaponsDict)
        {
            var weaponList = weaponKV.Value;
            if (weaponList.Contains(behaviour))
            {
                removeIndex = weaponKV.Key;
                break;
            }
        }

        if (!string.IsNullOrEmpty(removeIndex) && allWeaponsDict.ContainsKey(removeIndex))
        {
            var rmList = allWeaponsDict[removeIndex];
            rmList.Remove(behaviour);

            //场景内已无该UGC素材使用记录，清除数据
            if (rmList.Count <= 0)
            {
                allWeaponsDict.Remove(removeIndex);
            }
        }
    }

    public virtual void SetAttackCtrPanelActive(bool isActive)
    {
        if (isActive)
        {
            ShowWeaponCtrPanel?.Invoke();
        }
        else
        {
            HideWeaponCtrPanel?.Invoke();
        }
    }

    public virtual void RevertNode(NodeBaseBehaviour behaviour)
    {
        //TODO：undo操作
    }

    public void Clear()
    {
        allWeaponsDict.Clear();
    }

    #endregion

    #region 处理武器广播

    public virtual void HandleWeaponBroadcast(string senderPlayerId, Item item)
    {
    }

    #endregion
}
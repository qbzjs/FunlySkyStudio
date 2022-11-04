using System;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description:武器节点创建工具
/// Date: 2022-4-14 17:44:22 
/// </summary>
public class WeaponCreateUtils : CInstance<WeaponCreateUtils>
{
    /// <summary>
    /// 地图中创建武器
    /// 编辑时，又分创建默认占位道具，和创建上次选中的UGC素材
    /// </summary>
    public NodeBaseBehaviour CreateWeaponBeavInEdit<T1>(WeaponType type, Vector3 pos) where T1 : WeaponBaseManager<T1>, new()
    {
        NodeBaseBehaviour nBev = null;
        var manager = WeaponSystemController.Inst.GetWeaponManager<T1>(type);
        var lastSelectUgcInfo = manager.GetLastSelectUgcMapInfo();

        if (lastSelectUgcInfo == null || string.IsNullOrEmpty(lastSelectUgcInfo.mapJsonContent))
        {
            //创建默认武器道具
            nBev = manager.CreateDefaultNode();
            AddPickableComponent(nBev);
        }
        else
        {
            //创建上一个UGC素材并作为武器
            var mapInfo = lastSelectUgcInfo.mapInfo;
            nBev = CreateSingleUgcWeapon(pos, mapInfo, mapInfo.mapId, lastSelectUgcInfo.mapJsonContent);
            manager.AddWeaponComponent(nBev, mapInfo.mapId);
            manager.AddUgcWeaponItem(mapInfo.mapId, nBev);
        }

        return nBev;
    }


    /// <summary>
    /// 创建单个UGC武器,并挂上可拾取Cmp,也走离线渲染
    /// </summary>
    public NodeBaseBehaviour CreateSingleUgcWeapon(Vector3 pos, MapInfo mapInfo, string rId, string mapJsonContent)
    {
        // 将离线数据加入当前数据缓存
        UGCBehaviorManager.Inst.AddOfflineRenderData(mapInfo.renderList);
        LoggerUtils.Log($"CreateSingleUgcWeapon :  rid:{rId}, mapJsonContent:{mapJsonContent}");
        var nBehav = SceneBuilder.Inst.ParsePropAndBuild(mapJsonContent, pos, rId);
        nBehav.transform.parent = SceneBuilder.Inst.StageParent;
        AddConstrainer(nBehav);
        AddPickableComponent(nBehav);

        return nBehav;
    }

    public void AddPickableComponent(NodeBaseBehaviour nBev)
    {
        //添加可拾取属性
        if (nBev && !nBev.entity.HasComponent<PickablityComponent>())
        {
            var entity = nBev.entity;
            entity.Get<PickablityComponent>().canPick = 1;
            PickabilityManager.Inst.AddPickablityProp(entity, entity.Get<PickablityComponent>().anchors);
        }
    }

    public void AddWeaponComponentToUGC(NodeBaseBehaviour behaviour, NodeData data)
    {
        //攻击道具
        var attackWeaponKV = data.attr.Find(x => x.k == (int) BehaviorKey.AttackWeapon);
        if (attackWeaponKV != null)
        {
            var attackData = JsonConvert.DeserializeObject<AttackWeaponNodeData>(attackWeaponKV.v);

            var gComp = behaviour.entity.Get<GameObjectComponent>();
            gComp.modId = (int)GameResType.AttackWeapon;
            behaviour.entity.Get<AttackWeaponComponent>().rId = attackData.rId;
            behaviour.entity.Get<AttackWeaponComponent>().damage = attackData.damage;
            behaviour.entity.Get<AttackWeaponComponent>().wType = attackData.wType;
            behaviour.entity.Get<AttackWeaponComponent>().hits = attackData.hits;
            behaviour.entity.Get<AttackWeaponComponent>().curHits = attackData.hits;
            behaviour.entity.Get<AttackWeaponComponent>().openDurability = attackData.oDur;

            AttackWeaponManager.Inst.AddUgcWeaponItem(attackData.rId, behaviour);
        }

        //射击道具
        var shootWeaponKV = data.attr.Find(x => x.k == (int)BehaviorKey.ShootWeapon);
        if (shootWeaponKV != null)
        {
            var shootData = JsonConvert.DeserializeObject<ShootWeaponNodeData>(shootWeaponKV.v);
            if (shootData.hasCap == 0)
            {
                shootData.hasCap = (int)CapState.NoCap;
                shootData.fireRate = (int)FireRate.Medium;
            }
            var comp = behaviour.entity.Get<ShootWeaponComponent>();
            var gComp = behaviour.entity.Get<GameObjectComponent>();
            gComp.modId = (int) GameResType.ShootWeapon;
            comp.rId = shootData.rId;
            comp.damage = shootData.damage;
            comp.wType = shootData.wType;
            comp.isCustomPoint = shootData.isCustomPoint;
            comp.anchors = shootData.anchorsPos;
            comp.hasCap = shootData.hasCap;
            comp.capacity = shootData.capacity;
            comp.fireRate = shootData.fireRate;
            comp.curBullet = comp.capacity;
            ShootWeaponManager.Inst.AddUgcWeaponItem(shootData.rId, behaviour);
        }
        //其他....
    }

    //限制道具位置不能穿透到地底下
    public void AddConstrainer(NodeBaseBehaviour nBehav)
    {
        if (!nBehav.gameObject.TryGetComponent(out SpawnPointConstrainer adjustBehav))
        {
            adjustBehav = nBehav.gameObject.AddComponent<SpawnPointConstrainer>();
            adjustBehav.minHeight = 0;
        }
    }
}
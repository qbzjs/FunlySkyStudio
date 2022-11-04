
using System;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:回血道具节点创建工具
/// Date: 2022/5/19 16:10:35
/// </summary>


public class BloodPropCreateUtils : CInstance<BloodPropCreateUtils>
{
    /// <summary>
    /// 地图中创建回血道具
    /// 编辑时，又分创建默认占位道具，和创建上次选中的UGC素材
    /// </summary>
    public NodeBaseBehaviour CreateBloodPropBeavInEdit(Vector3 pos)
    {
        NodeBaseBehaviour nBev = null;
        var manager = BloodPropManager.Inst;
        var lastSelectUgcInfo = manager.GetLastSelectUgcMapInfo();

        if (lastSelectUgcInfo == null || string.IsNullOrEmpty(lastSelectUgcInfo.mapJsonContent))
        {
            //创建默认回血道具
            nBev = manager.CreateDefaultNode();
        }
        else
        {
            //创建上一个UGC素材并作为回血道具
            var mapInfo = lastSelectUgcInfo.mapInfo;
            nBev = CreateSingleUgcWeapon(pos, mapInfo, mapInfo.mapId, lastSelectUgcInfo.mapJsonContent);
            manager.AddWeaponComponent(nBev, mapInfo.mapId);
            manager.AddUgcWeaponItem(mapInfo.mapId, nBev);
        }
        return nBev;
    }


    /// <summary>
    /// 创建单个UGC回血道具,并挂上可拾取Cmp,也走离线渲染
    /// </summary>
    public NodeBaseBehaviour CreateSingleUgcWeapon(Vector3 pos, MapInfo mapInfo, string rId, string mapJsonContent)
    {
        // 将离线数据加入当前数据缓存
        UGCBehaviorManager.Inst.AddOfflineRenderData(mapInfo.renderList);

        LoggerUtils.Log($"CreateSingleUgcWeapon :  rid:{rId}, mapJsonContent:{mapJsonContent}");
        var nBehav = SceneBuilder.Inst.ParsePropAndBuild(mapJsonContent, pos, rId);
        nBehav.transform.parent = SceneBuilder.Inst.StageParent;
        AddConstrainer(nBehav);
        var gameComponent = nBehav.entity.Get<GameObjectComponent>();
        gameComponent.modId = (int)GameResType.BloodRestore;
        gameComponent.handleType = NodeHandleType.BloodRestore;
        gameComponent.modelType = NodeModelType.BloodRestore;

        var bloodBase = new BloodPropBase(nBehav);
        BloodPropManager.Inst.AddBloodPropBase(nBehav, bloodBase);
        bloodBase.UpdateBloodPropBehaviour(nBehav);

        return nBehav;
    }

    public void AddWeaponComponentToUGC(NodeBaseBehaviour behaviour, NodeData data)
    {
        //回血道具
        var bloodPropKV = data.attr.Find(x => x.k == (int)BehaviorKey.BloodRestoreProp);
        if (bloodPropKV != null)
        {
            var bloodData = JsonConvert.DeserializeObject<BloodPropData>(bloodPropKV.v);
            behaviour.entity.Get<BloodPropComponent>().rId = bloodData.rId;
            behaviour.entity.Get<BloodPropComponent>().restore = bloodData.restore;

            BloodPropManager.Inst.AddUgcWeaponItem(bloodData.rId, behaviour);

            var gameComponent = behaviour.entity.Get<GameObjectComponent>();
            gameComponent.modId = (int)GameResType.BloodRestore;
            gameComponent.handleType = NodeHandleType.BloodRestore;
            gameComponent.modelType = NodeModelType.BloodRestore;

            var bloodBase = BloodPropManager.Inst.GetBloodPropBase(behaviour);
            if (bloodBase == null)
            {
                bloodBase = new BloodPropBase(behaviour);
            }
            BloodPropManager.Inst.AddBloodPropBase(behaviour, bloodBase);
            bloodBase.UpdateBloodPropBehaviour(behaviour);
        }
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

using System;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description:选择ugc作为道具管理类
/// Date: 2022-7-26 15:24:01
/// </summary>
public class UgcChooseManager : CInstance<UgcChooseManager>
{
    /// <summary>
    /// 创建单个UGC道具,走离线渲染
    /// </summary>
    public NodeBaseBehaviour CreateSingleUgcAsProp(Vector3 pos, MapInfo mapInfo, string rId, string mapJsonContent)
    {
        // 将离线数据加入当前数据缓存
        UGCBehaviorManager.Inst.AddOfflineRenderData(mapInfo.renderList);
        LoggerUtils.Log($"UgcChooseManager CreateSingleUgcAsProp :  rid:{rId}, mapJsonContent:{mapJsonContent}");
        var nBehav = SceneBuilder.Inst.ParsePropAndBuild(mapJsonContent, pos, rId);
        nBehav.transform.parent = SceneBuilder.Inst.StageParent;
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
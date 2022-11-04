using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author:JayWill
/// Description:滑梯节点Creater
/// </summary>

public class SlideItemCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = new GameObject("SlideItem");
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.SlidePipe;
        gameComponent.modelType = NodeModelType.SlideItem;
        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent = null)
    {
        SlideItemBehaviour itemBehaviour = behaviour as SlideItemBehaviour;
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        var newParent = parent ?? SceneBuilder.Inst.StageParent;
        itemBehaviour.transform.SetParent(newParent);
        itemBehaviour.transform.localPosition = pos;
        itemBehaviour.transform.localEulerAngles = rot;
        itemBehaviour.transform.localScale = sca;

        itemBehaviour.mRoot = parent.GetComponent<SlidePipeBehaviour>();
        SetData(behaviour as SlideItemBehaviour, GameUtils.GetAttr<SlideItemData>((int)BehaviorKey.SlideItem, data.attr),data);
    }

    public static void SetData(SlideItemBehaviour behaviour, SlideItemData itemData,NodeData data)
    {
        var mComp = behaviour.entity.Get<SlideItemComponent>();
        behaviour.Clear();
        mComp.ItemIndex = itemData.index;
        mComp.MatId = itemData.mat;
        mComp.Color = DataUtils.DeSerializeColor(itemData.color);
        mComp.Tile = DataUtils.DeSerializeVector2(itemData.tile);
        mComp.SpeedType = itemData.speedtype;

        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        gameComp.modId = data.id;
   
        GameObject assetObj = behaviour.assetObj;
        if (assetObj == null)
        {
            assetObj = ModelCachePool.Inst.Get(data.id);
        }
        behaviour.UpdateModel(assetObj, data.id);
    }
}

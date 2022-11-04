using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author:JayWill
/// Description:滑梯Creater
/// </summary>

public class SlidePipeCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = new GameObject("SlidePipe");
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        if (!assetGo.TryGetComponent(out SpawnPointConstrainer adjustBehav))
        {
            adjustBehav = assetGo.AddComponent<SpawnPointConstrainer>();
            adjustBehav.minHeight = 0.2f;
        }
        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.SlidePipe;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.SlidePipe;
        gameComponent.modelType = NodeModelType.SlidePipe;
        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent = null)
    {
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        var newParent = parent ?? SceneBuilder.Inst.StageParent;
        behaviour.transform.SetParent(newParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;
        
        SetData(behaviour as SlidePipeBehaviour, GameUtils.GetAttr<SlidePipeData>((int)BehaviorKey.SlidePipe, data.attr),data);
    }

    public static void SetData(SlidePipeBehaviour behaviour, SlidePipeData slideData,NodeData data)
    {
        var mComp = behaviour.entity.Get<SlidePipeComponent>();
        behaviour.Clear();
        mComp.WayType = slideData.waytype;
        mComp.HideModel = slideData.hide;

        behaviour.SetVirtualModel(mComp.HideModel == 1);

        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        SlidePipeManager.Inst.AddSlidePipe(behaviour);
    }
}
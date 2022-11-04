/// <summary>
/// Author:YangJie
/// Description:
/// Date: 2022/5/16 19:36:18
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Game.Core;
using HLODSystem;
using SavingData;
using UnityEngine;
using Object = UnityEngine.Object;

public class PGCNodeCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = new GameObject("PGCNode");
        var rlRoot = new GameObject("RLRoot");
        rlRoot.transform.SetParent(assetGo.transform);
        NodeBaseBehaviour nodeBehaviour = assetGo.AddComponent<T>();
        
        nodeBehaviour.OnInitByCreate();
        nodeBehaviour.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.type = ResType.PGC;
        gameComponent.modelType = NodeModelType.PGC;
        gameComponent.handleType = NodeHandleType.PGC;
        return (T) nodeBehaviour;
    }

    public static Transform SetAsset(PGCBehaviour pgcBehaviour, GameObject assetObj, int id)
    {
        if (assetObj == null)
        {
            return null;
        }
        
        if (pgcBehaviour.isSoldOut)
        {
            ModelCachePool.Inst.Release(id, assetObj);
            return null;
        }
        
        var rlRoot = pgcBehaviour.transform.Find("RLRoot");
        if (rlRoot != null)
        {
            assetObj.transform.SetParent(rlRoot);
            assetObj.transform.localPosition = Vector3.zero;
            assetObj.transform.localEulerAngles = Vector3.zero;
            assetObj.transform.localScale = Vector3.one;
            pgcBehaviour.assetObj = assetObj;
            pgcBehaviour.OnInitByCreate();
        }

        return assetObj.transform;
    }
    
    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Transform parent = null)
    {
        
        var pgcBehaviour = behaviour as PGCBehaviour;
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        var pos = DataUtils.DeSerializeVector3(data.p);
        sca = DataUtils.LimitVector3(sca);
        pgcBehaviour.transform.SetParent(parent ? parent : SceneBuilder.Inst.StageParent);
        pgcBehaviour.transform.localPosition = pos;
        pgcBehaviour.transform.localEulerAngles = rot;
        pgcBehaviour.transform.localScale = sca;
        pgcBehaviour.data = data;
        var gameComp = pgcBehaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        gameComp.resId = data.rid;
        gameComp.modId = data.id;
        
        var pgcSceneData =  GameUtils.GetAttr<PGCSceneData>((int) BehaviorKey.PgcScene, data.attr);
        if (MapInfo.IsScenePgc(pgcSceneData.classifyID, pgcSceneData.pgcID))
        {
            var pgcSceneComponent = pgcBehaviour.entity.Get<PGCSceneComponent>();
            pgcSceneComponent.classifyID = pgcSceneData.classifyID;
            pgcSceneComponent.pgcID = pgcSceneData.pgcID;
            
            var ugcPropData =  GameUtils.GetAttr<UGCPropData>((int) BehaviorKey.UGCProp, data.attr);
            var ugcPropComponent = pgcBehaviour.entity.Get<UGCPropComponent>();
            ugcPropComponent.isTradable = ugcPropData.isTradable;
        }
        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            pgcBehaviour.SetLODStatus(HLODState.High);
        }
        PGCBehaviorManager.Inst.AddNode(behaviour);
    }

    public static void LoadAsset(PGCBehaviour pgcBehaviour, int id, Action<Transform> onSucc = null, Action onFail = null)
    {
        if (!GameManager.Inst.priConfigData.TryGetValue(id, out var modelData))
        {
            onFail?.Invoke();
            return;
        };

        ModelCachePool.Inst.GetSync(pgcBehaviour, id, (go) =>
        {
            var rl = SetAsset(pgcBehaviour, go, id);
            if (rl != null)
            {
                pgcBehaviour.SetCanBuyInMap();
                onSucc?.Invoke(rl);
            }
            else
            {
                onFail?.Invoke();
            }
        },onFail);
    }
    
    
    
    public override GameObject Clone(GameObject target)
    {
        return null;
    }
}

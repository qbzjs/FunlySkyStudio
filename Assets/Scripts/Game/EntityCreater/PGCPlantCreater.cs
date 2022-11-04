using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:PGC植物创建
/// Date: 2022/8/4 18:32:40
/// </summary>
public class PGCPlantCreater : SceneEntityCreater
{

    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = new GameObject("PGCPlant");
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Special;
        gameComponent.modelType = NodeModelType.PGCPlant;

        AddConstrainer(behav);

        return behav as T;
    }
    //限制位置不能穿透到地底下
    public void AddConstrainer(NodeBaseBehaviour nBehav)
    {
        if (!nBehav.gameObject.TryGetComponent(out SpawnPointConstrainer adjustBehav))
        {
            adjustBehav = nBehav.gameObject.AddComponent<SpawnPointConstrainer>();
            adjustBehav.minHeight = 0;
        }
    }
    public override GameObject Clone(GameObject target)
    {
        return null;
    }
    public static bool CanCloneTarget(GameObject target)
    {
        return true;
    }
    public static void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        var sBehav = nBehaviour as PGCPlantBehaviour;
        PGCPlantManager.Inst.AddItem(sBehav);
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vec3 pos, Transform parent = null)
    {
        var nodeBehaviour = behaviour as PGCPlantBehaviour;
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        var newParent = parent ? parent : SceneBuilder.Inst.StageParent;
        behaviour.transform.SetParent(newParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;

        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        var handleData = GameManager.Inst.PGCPlantDatasDic[data.id];
        gameComp.uid = UidManager.Inst.GetUid(data);
        gameComp.modId = data.id;
        gameComp.handleType = (NodeHandleType)data.type;

        if (nodeBehaviour.assetObj == null)
        {
            var assetObj = ModelCachePool.Inst.Get(data.id);
            nodeBehaviour.UpdateAssetObj(assetObj, data.id);
        }
        nodeBehaviour.OnInitByCreate();


        SetData(nodeBehaviour, GameUtils.GetAttr<PGCPlantData>((int)BehaviorKey.PGCPlant, data.attr));
    }

    public static void SetData(PGCPlantBehaviour nBehaviour, PGCPlantData data)
    {
        var mComp = nBehaviour.entity.Get<PGCPlantComponent>();
        if (data != null)
        {
            mComp.plantColor = data.plantColor;
        }
        else
        {
            mComp.plantColor = PGCPlantManager.Inst.lastChooseColor;
        }
        nBehaviour.SetColor(DataUtils.DeSerializeColor(mComp.plantColor));
    }
}

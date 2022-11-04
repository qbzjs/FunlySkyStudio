using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using UnityEngine;

public class FishingRodCreator : SceneEntityCreater
{
    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.FishingRod);
        if (! assetGo.TryGetComponent(out T behav))
            behav = assetGo.AddComponent<T>();

        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        behav.gameObject.transform.localPosition = CameraUtils.Inst.GetCreatePosition();
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.FishingRod;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Special;
        gameComponent.modelType = NodeModelType.FishingRod;
        gameComponent.uid = UidManager.Inst.GetUid();

        return behav;
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent)
    {
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        var newParent = parent.Find(FishingEditManager.Inst.RodParentPath);
        behaviour.transform.SetParent(newParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;

        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);

        var rodData = GameUtils.GetAttr<FishingRodData>((int)BehaviorKey.FishingRod, data.attr);
        AddRodComponent(behaviour, rodData.rid, rodData.isCustomPos);
    }

    public static void AddRodComponent(NodeBaseBehaviour node, string rid, int isCustomPos)
    {
        var mComp = node.entity.Get<FishingRodComponent>();
        mComp.rid = rid;
        mComp.isCustomPos = isCustomPos;
    }
}

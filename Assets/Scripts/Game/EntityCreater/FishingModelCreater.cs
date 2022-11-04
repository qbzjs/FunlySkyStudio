using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using UnityEngine;

public class FishingModelCreater : SceneEntityCreater
{
    public static void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        var nEntity = nBehaviour.entity;
        FishingEditManager.Inst.AddFishingNode(nBehaviour);
        if (nEntity.HasComponent<PickablityComponent>() && PickabilityManager.Inst.CheckCanSetPickability())
        {
            var pComp = nEntity.Get<PickablityComponent>();
            PickabilityManager.Inst.AddPickablityProp(nEntity, pComp.anchors);
        }
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.FishingModel);
        if (!assetGo.TryGetComponent(out T behav))
            behav = assetGo.AddComponent<T>();

        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        behav.gameObject.transform.localPosition = CameraUtils.Inst.GetCreatePosition();

        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.FishingModel;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Special;
        gameComponent.modelType = NodeModelType.FishingModel;
        gameComponent.uid = UidManager.Inst.GetUid();
        FishingEditManager.Inst.AddFishingNode(behav);
        return behav;
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent)
    {
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        behaviour.transform.SetParent(SceneBuilder.Inst.StageParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;

        //鱼竿默认添加可拾起属性
        var comp = behaviour.entity.Get<PickablityComponent>();
        comp.canPick = 1;
        PickabilityManager.Inst.AddPickablityProp(behaviour.entity, comp.anchors);

        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
    }
}

using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author: Tee Li
/// 日期：2022/8/30
/// 鱼钩创建
/// </summary>

public class FishingHookCreator : SceneEntityCreater
{
    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.FishingHook);
        if (! assetGo.TryGetComponent(out T behav))
            behav = assetGo.AddComponent<T>();

        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        behav.gameObject.transform.localPosition = CameraUtils.Inst.GetCreatePosition();
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.FishingHook;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Special;
        gameComponent.modelType = NodeModelType.FishingHook;
        gameComponent.uid = UidManager.Inst.GetUid();
        return behav;
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent)
    {
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        var newParent = parent.Find(FishingEditManager.Inst.HookParentPath);
        behaviour.transform.SetParent(newParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;

        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);

        var hookData = GameUtils.GetAttr<FishingHookData>((int)BehaviorKey.FishingHook, data.attr);
        AddHookComponent(behaviour, hookData.rid, hookData.hookPosition, hookData.isCustomHook);
    }

    public static void AddHookComponent(NodeBaseBehaviour node, string rid, Vec3 hookPosition, int isCustomHook)
    {
        var mComp = node.entity.Get<FishingHookComponent>();
        mComp.rid = rid;
        mComp.hookPosition = hookPosition;
        mComp.isCustomHook = isCustomHook;
    }
}

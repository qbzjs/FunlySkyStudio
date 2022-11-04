using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Game.Core;

/// <summary>
/// Author:Meimei-LiMei
/// Description:默认烟花道具的创建
/// Date: 2022/7/20 16:53:49
/// </summary>
public class FireworkCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.Firework);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        //限制道具位置不能穿透到地底下
        if (!assetGo.TryGetComponent(out SpawnPointConstrainer adjustBehav))
        {
            adjustBehav = assetGo.AddComponent<SpawnPointConstrainer>();
            adjustBehav.minHeight = 0;
        }

        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        behav.gameObject.transform.localPosition = CameraUtils.Inst.GetCreatePosition();
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.Firework;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Firework;
        gameComponent.modelType = NodeModelType.Firework;

        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static bool CanCloneTarget(GameObject target)
    {
        if (FireworkManager.Inst.IsOverMaxCount())
        {
            return false;
        }
        return true;
    }

    public static void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        var oCom = oBehaviour.entity.Get<FireworkComponent>();
        FireworkManager.Inst.AddUgcFireworkItem(oCom.rId, nBehaviour);
        if (oCom.rId == FireworkManager.DEFAULT_MODEL)
        {
            return;
        }
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

        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        SetData(behaviour as FireworkBehaviour, GameUtils.GetAttr<FireworkData>((int)BehaviorKey.Firework, data.attr));
    }

    public static void SetData(FireworkBehaviour behaviour, FireworkData data)
    {
        var mComp = behaviour.entity.Get<FireworkComponent>();
        mComp.rId = data.rId;
        mComp.fireworkcolor = data.fireworkcolor;
        mComp.fireworkHeight = data.fireworkHeight;
        mComp.anchorsPos = data.anchorsPos;
        mComp.isCustomPoint = data.isCustomPoint;
        mComp.isControl = data.isControl;

        FireworkManager.Inst.AddUgcFireworkItem(FireworkManager.DEFAULT_MODEL, behaviour);
    }
}


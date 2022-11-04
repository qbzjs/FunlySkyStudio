using Assets.Scripts.Game.Core;
using System.Collections;
using System.Collections.Generic;
using SavingData;
using UnityEngine;

/// <summary>
/// Author: 熊昭
/// Description: 3D相册道具创建器
/// Date: 2022-02-06 18:00:27
/// </summary>
public class ShotPhotoCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.ShotPhoto);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.ShotPhoto;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Special;
        gameComponent.modelType = NodeModelType.ShotPhoto;
        return behav;
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
        var sBehav = nBehaviour as ShotPhotoBehaviour;
        ShotPhotoManager.Inst.AddPhoto(sBehav);
        sBehav.OnLoadingClone();
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
        SetData(behaviour as ShotPhotoBehaviour, GameUtils.GetAttr<ShotPhotoData>((int)BehaviorKey.ShotPhoto, data.attr));
    }

    public static void SetData(ShotPhotoBehaviour behaviour, ShotPhotoData data)
    {
        var comp = behaviour.entity.Get<ShotPhotoComponent>();
        comp.photoUrl = data.pUrl;
        comp.type = (SavePhotoType)data.type;
        behaviour.LoadPhoto();
        ShotPhotoManager.Inst.AddPhoto(behaviour);
    }
}
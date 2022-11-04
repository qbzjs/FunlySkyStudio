/// <summary>
/// Author:Mingo-LiZongMing
/// Description:mo
/// </summary>
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using UnityEngine;

public class ShootWeaponCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.ShootWeapon);
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
        gameComponent.modId = (int)GameResType.ShootWeapon;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.ShootWeapon;
        gameComponent.modelType = NodeModelType.ShootWeapon;

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
        var newBev = nBehaviour as ShootWeaponDefaultBehaviour;
        ShootWeaponManager.Inst.AddUgcWeaponItem(ShootWeaponManager.DEFAULT_MODEL, newBev);
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
        SetData(behaviour as ShootWeaponDefaultBehaviour, GameUtils.GetAttr<ShootWeaponNodeData>((int)BehaviorKey.ShootWeapon, data.attr));
    }

    public static void SetData(ShootWeaponDefaultBehaviour behaviour, ShootWeaponNodeData data)
    {
        var mComp = behaviour.entity.Get<ShootWeaponComponent>();
        mComp.rId = data.rId;
        mComp.damage = data.damage;
        mComp.wType = data.wType;
        mComp.anchors = data.anchorsPos;
        mComp.isCustomPoint = data.isCustomPoint;
        mComp.hasCap = data.hasCap;
        mComp.capacity = data.capacity;
        mComp.fireRate = data.fireRate;
        mComp.curBullet = mComp.capacity;

        ShootWeaponManager.Inst.AddUgcWeaponItem(ShootWeaponManager.DEFAULT_MODEL, behaviour);
    }
}

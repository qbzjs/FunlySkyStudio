using Assets.Scripts.Game.Core;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description:默认武器道具的创建-默认小剑节点的创建
/// Date: 2022-4-14 17:44:22
/// </summary>
public class AttackWeaponCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int) GameResType.AttackWeapon);
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
        gameComponent.modId = (int) GameResType.AttackWeapon;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.AttackWeapon;
        gameComponent.modelType = NodeModelType.AttackWeapon;
        
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
        var newBev = nBehaviour as AttackWeaponDefaultBehaviour;
        AttackWeaponManager.Inst.AddUgcWeaponItem(AttackWeaponManager.DEFAULT_MODEL, newBev);
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
        SetData(behaviour as AttackWeaponDefaultBehaviour, GameUtils.GetAttr<AttackWeaponNodeData>((int) BehaviorKey.AttackWeapon, data.attr));
    }

    public static void SetData(AttackWeaponDefaultBehaviour behaviour, AttackWeaponNodeData data)
    {
        var mComp = behaviour.entity.Get<AttackWeaponComponent>();
        mComp.rId = data.rId;
        mComp.damage = data.damage;
        mComp.wType = data.wType;
        mComp.openDurability = data.oDur;
        mComp.hits = data.hits;
        mComp.curHits = data.hits;

        AttackWeaponManager.Inst.AddUgcWeaponItem(AttackWeaponManager.DEFAULT_MODEL, behaviour);
    }
}
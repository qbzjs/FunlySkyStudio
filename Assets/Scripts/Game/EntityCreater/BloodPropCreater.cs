using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Game.Core;

/// <summary>
/// Author:WenJia
/// Description:默认回血道具的创建
/// Date: 2022/5/19 13:31:14
/// </summary>


public class BloodPropCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.BloodRestore);
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
        gameComponent.modId = (int)GameResType.BloodRestore;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.BloodRestore;
        gameComponent.modelType = NodeModelType.BloodRestore;

        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static bool CanCloneTarget(GameObject target)
    {
        if (BloodPropManager.Inst.IsOverMaxCount())
        {
            return false;
        }
        return true;
    }

    public static void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        var oCom = oBehaviour.entity.Get<BloodPropComponent>();
        BloodPropManager.Inst.AddUgcWeaponItem(oCom.rId, nBehaviour);
        if (oCom.rId == BloodPropManager.DEFAULT_MODEL)
        {
            return;
        }
        var bloodBase = BloodPropManager.Inst.GetBloodPropBase(nBehaviour);
        if (bloodBase == null)
        {
            bloodBase = new BloodPropBase(nBehaviour);
        }
        BloodPropManager.Inst.AddBloodPropBase(nBehaviour, bloodBase);
        bloodBase.UpdateBloodPropBehaviour(nBehaviour, true);
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
        SetData(behaviour as BloodPropBehaviour,
        GameUtils.GetAttr<BloodPropData>((int)BehaviorKey.BloodRestoreProp, data.attr));
    }

    public static void SetData(BloodPropBehaviour behaviour, BloodPropData data)
    {
        var mComp = behaviour.entity.Get<BloodPropComponent>();
        var rid = BloodPropManager.DEFAULT_MODEL;
        mComp.rId = data.rId;
        mComp.restore = data.restore;
        rid = data.rId;

        BloodPropManager.Inst.AddUgcWeaponItem(rid, behaviour);
    }
}

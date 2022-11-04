using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author: LiShuzhan
/// Description:
/// Date: 2022-08-01
/// </summary>
public class ParachuteCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int) GameResType.Parachute);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        AddConstrainer(behav);
        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        behav.gameObject.transform.localPosition = CameraUtils.Inst.GetCreatePosition();
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int) GameResType.Parachute;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Parachute; 
        gameComponent.modelType = NodeModelType.Parachute;
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
        var rid =  nBehaviour.entity.Get<ParachuteComponent>().rid;
        ParachuteManager.Inst.AddUgcItem(rid, nBehaviour, ParaUgcType.Parachute);
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent = null)
    {
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        var newParent = parent ? parent : SceneBuilder.Inst.StageParent;
        behaviour.transform.SetParent(newParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;
        

        //Todo: add your own logic
        SetData(behaviour as ParachuteBehaviour, GameUtils.GetAttr<ParachuteData>((int) BehaviorKey.Parachute, data.attr), data);
    }

    public static void SetData(ParachuteBehaviour itemBehaviour, ParachuteData data, NodeData nodeData)
    {
        var mComp = itemBehaviour.entity.Get<ParachuteComponent>();
        mComp.parachuteBagUid = data.bagUid;
        mComp.rid = data.rid;
        mComp.isCustomPoint = data.isCustomPoint;
        mComp.anchors = data.anchorsPos;
        var gameComp = itemBehaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(nodeData);
        ParachuteManager.Inst.AddUgcItem(ParachuteManager.DEFAULT_MODEL, itemBehaviour, ParaUgcType.Parachute);
    }

    //限制道具位置不能穿透到地底下
    public static void AddConstrainer(NodeBaseBehaviour nBehav)
    {
        if (!nBehav.gameObject.TryGetComponent(out SpawnPointConstrainer adjustBehav))
        {
            adjustBehav = nBehav.gameObject.AddComponent<SpawnPointConstrainer>();
            adjustBehav.minHeight = 0;
        }
    }
}
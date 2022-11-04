using Assets.Scripts.Game.Core;
using UnityEngine;

public class ParachuteBagCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.ParachuteBag);
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
        gameComponent.modId = (int)GameResType.ParachuteBag;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.ParachuteBag;
        gameComponent.modelType = NodeModelType.ParachuteBag;
        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static void OnClone(NodeBaseBehaviour nBehaviour)
    {
        var rid = nBehaviour.entity.Get<ParachuteBagComponent>().rid;
        ParachuteManager.Inst.AddUgcItem(rid, nBehaviour, ParaUgcType.Bag);
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

        SetData(behaviour as ParachuteBagBehaviour, GameUtils.GetAttr<ParachuteBagData>((int)BehaviorKey.ParachuteBag, data.attr), data);
    }

    public static void SetData(ParachuteBagBehaviour itemBehaviour, ParachuteBagData data,NodeData nodeData)
    {
        var mComp = itemBehaviour.entity.Get<ParachuteBagComponent>();
        mComp.parachuteUid = data.paraUid;
        mComp.rid = data.rid;
        mComp.isCustomPoint = data.isCustomPoint;
        mComp.anchors = data.anchorsPos;
        var gameComp = itemBehaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(nodeData);
        ParachuteManager.Inst.AddUgcItem(ParachuteManager.DEFAULT_MODEL, itemBehaviour, ParaUgcType.Bag);
        itemBehaviour.gameObject.SetActive(false);
    }

    public static void AddConstrainer(NodeBaseBehaviour nBehav)
    {
        if (!nBehav.gameObject.TryGetComponent(out SpawnPointConstrainer adjustBehav))
        {
            adjustBehav = nBehav.gameObject.AddComponent<SpawnPointConstrainer>();
            adjustBehav.minHeight = 0;
        }
    }
}

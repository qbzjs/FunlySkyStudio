using System.Security.Cryptography;
using Assets.Scripts.Game.Core;
using UnityEngine;

public class FreezePropsCraeter : SceneEntityCreater
{
    //恢复场景时候调用
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
        SetData(behaviour, GameUtils.GetAttr<FreezePropsData>((int)BehaviorKey.FreezeProps, data.attr));
    }
    //编辑模式调用
    public static void SetData(NodeBaseBehaviour behaviour, FreezePropsData data)
    {
        FreezePropsComponent compt = behaviour.entity.Get<FreezePropsComponent>();
        compt.rId = data.id;
        compt.mFreezeTime = data.mFreezeTime;
        FreezePropsManager.Inst.AddUgcItem(data.id, behaviour);
    }
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.FreezeProps);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        //限制道具位置不能穿透到地底下
        FreezePropsManager.Inst.AddConstrainer(behav);
     
        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        behav.gameObject.transform.localPosition = CameraUtils.Inst.GetCreatePosition();
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.FreezeProps;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.FreezeProps;
        gameComponent.modelType = NodeModelType.FreezeProps;

        return behav;
    }
    public override GameObject Clone(GameObject target)
    {
        return null;
    }
    public void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        FreezePropsComponent oCom = oBehaviour.entity.Get<FreezePropsComponent>();
        FreezePropsManager.Inst.AddUgcItem(oCom.rId, nBehaviour);
        if (oCom.rId == BloodPropManager.DEFAULT_MODEL)
        {
            return;
        }
        var nodeAuxiliary = FreezePropsManager.Inst.GetNodeAuxiliary(nBehaviour);
        if (nodeAuxiliary == null)
        {
            nodeAuxiliary = new FreezePropsNodeAuxiliary(nBehaviour,FreezePropsManager.Inst);
        }
        FreezePropsManager.Inst.AddNodeAuxiliary(nBehaviour, nodeAuxiliary);
        nodeAuxiliary.UpdatePropBehaviour(nBehaviour, true);
    }
}

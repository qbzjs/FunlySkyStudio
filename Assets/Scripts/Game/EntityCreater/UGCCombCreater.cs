/// <summary>
/// Author:YangJie
/// Description: UGC Creater
/// Date: 2022/3/30 18:28:5
/// </summary>
using Assets.Scripts.Game.Core;
using HLODSystem;
using UnityEngine;

public class UGCCombCreater : SceneEntityCreater
{
    public UGCCombBehaviour Create(Vector3 pos, Transform parent = null)
    {
        var cNode = ModelCachePool.Inst.Get((int)GameResType.UGCComb);
        var newParent = parent ? parent : SceneBuilder.Inst.StageParent;
        cNode.transform.SetParent(newParent);
        cNode.transform.localPosition = pos;
        var entity = world.NewEntity();
        if (!cNode.TryGetComponent(out UGCCombBehaviour behav))
        {
            behav = cNode.AddComponent<UGCCombBehaviour>();
        }
        behav.entity = entity;
        behav.OnInitByCreate();
        var gameComp = entity.Get<GameObjectComponent>();
        gameComp.bindGo = cNode;
        gameComp.modId = (int)GameResType.UGCComb;
        gameComp.type = ResType.UGC;
        gameComp.modelType = NodeModelType.CommonCombine;
        gameComp.handleType = NodeHandleType.SpecialCombine;
        return behav;
    }

    public override T Create<T>()
    {
        return null;
    }
    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static void SetData(UGCCombBehaviour behaviour, UGCPropData data)
    {
        var comp = behaviour.entity.Get<UGCPropComponent>();
        comp.isTradable = data.isTradable;
        behaviour.SetCanBuyInMap();
    }

    public static void SetData(UGCCombBehaviour behaviour, NodeData data, bool isUgcClone = false)
    {
        UGCBehaviorManager.Inst.AddNode(behaviour, data);
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);

        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;
        behaviour.data = data;
        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        gameComp.resId = data.rid;
#if UNITY_EDITOR
        if (isUgcClone)
        {
            behaviour.name += "_ugcClone_" + gameComp.uid;
            LoggerUtils.Log($"[SceneBuilderUtils] Clone UGC rid:{data.rid}, behav:{behaviour.name}");
        }
        else
        {
            behaviour.name += "_ugc_" + gameComp.uid;
            LoggerUtils.Log($"[SceneBuilderUtils] UGC rid:{data.rid}, behav:{behaviour.name}");
        }
#endif
        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            ParsePropWithTipsManager.Inst.AddPropParse(data.rid,(lod)=>{
                HLODState state;
                switch (lod)
                {
                    case UGCModelType.High: state = HLODState.High; break;
                    case UGCModelType.Low: state = HLODState.Low; break;
                    default: state = HLODState.High; break;
                }
                behaviour.SetLODStatus(state);
            },behaviour as NodeBaseBehaviour);
        }
        SetData(behaviour, GameUtils.GetAttr<UGCPropData>((int)BehaviorKey.UGCProp, data.attr));
    }

}
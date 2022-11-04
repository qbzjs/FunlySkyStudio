using Assets.Scripts.Game.Core;
using UnityEngine;

public class SwingCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        SceneEntity entity = world.NewEntity();
        var go = ModelCachePool.Inst.Get((int) GameResType.Swing);
        if (!go.TryGetComponent(out T behav))
        {
            behav = go.AddComponent<T>();
        }

        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        behav.gameObject.transform.localPosition = CameraUtils.Inst.GetCreatePosition();
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = go;
        gameComponent.modId = (int) GameResType.Swing;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Swing;
        gameComponent.modelType = NodeModelType.Swing;

        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static bool CanCloneTarget(GameObject target)
    {
        return !SwingManager.Inst.IsOverMaxCount();
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
        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);

        SwingNodeData sd = GameUtils.GetAttr<SwingNodeData>((int) BehaviorKey.Swing, data.attr);
        SetData(behaviour as SwingBehaviour, sd);
    }

    public static void SetData(SwingBehaviour sb, SwingNodeData sd)
    {
        SwingComponent sc = sb.entity.Get<SwingComponent>();
        sc.rId = sd.rId;
        sc.ropePos = sd.ropePos;
        sc.ropeRote = sd.ropeRote;
        sc.ropeScale = sd.ropeScale;
        sc.seatPos = sd.seatPos;
        sc.seatRote = sd.seatRote;
        sc.seatScale = sd.seatScale;
        sc.sitPos = sd.sitPos;
        sc.hide = sd.hide;
        sb.InitSwing(sd);
        SwingManager.Inst.AddSwing(sb);
        if (!string.IsNullOrEmpty(sc.rId))
        {
            SwingManager.Inst.SaveRid(sc.rId);
        }
    }
}
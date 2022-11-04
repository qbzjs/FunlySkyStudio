/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/7/26 20:54:4
/// </summary>
using Assets.Scripts.Game.Core;
using UnityEngine;
using UnityEngine.EventSystems;


public class BounceplankCreater : SceneEntityCreater
{
    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.Bounceplank);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.Bounceplank;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Bounceplank;
        gameComponent.modelType = NodeModelType.Bounceplank;
        return behav;
    }
    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent = null)
    {
        LoggerUtils.Log("Bounceplank SetData:" + data);
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
        SetData(behaviour as BounceplankBehaviour, GameUtils.GetAttr<BounceplankData>((int)BehaviorKey.Bounceplank, data.attr));
    }

    public static void SetData(BounceplankBehaviour behaviour, BounceplankData data)
    {
        BounceplankComponent compt = behaviour.entity.Get<BounceplankComponent>();
        if (data != null)
        {
            compt.shape = data.s;
            compt.BounceHeight = data.h;
            compt.color = data.col;
            compt.mat = data.mat;
            compt.tile = string.IsNullOrEmpty(data.tile) ? new Vector2(1, 1) : DataUtils.DeSerializeVector2(data.tile);
        }
        else
        {
            compt.shape = (int)BounceShape.Round;
            compt.BounceHeight = BounceHeight.M.ToString();
            compt.color = DataUtils.ColorToString(AssetLibrary.Inst.colorLib.Get(0));
            compt.mat = 0;
            compt.tile = new Vector2(1, 1);
        }
        behaviour.SetColor(DataUtils.DeSerializeColor(compt.color));
        behaviour.SetMatetial(compt.mat);
        behaviour.SetShape((BounceShape)compt.shape);
        behaviour.SetTiling(compt.tile);
        BounceplankManager.Inst.AddItem(behaviour as BounceplankBehaviour);
    }
}

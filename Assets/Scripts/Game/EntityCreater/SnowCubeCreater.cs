using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author: LiShuzhan
/// Description:
/// Date: 2022-08-16
/// </summary>
public class SnowCubeCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int) GameResType.SnowCube);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int) GameResType.SnowCube;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.SnowCube; 
        gameComponent.modelType = NodeModelType.SnowCube;
        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        var sBehav = nBehaviour as SnowCubeBehaviour;
        SnowCubeManager.Inst.AddItem(sBehav);
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

        SetData(behaviour as SnowCubeBehaviour, GameUtils.GetAttr<SnowCubeData>((int) BehaviorKey.SnowCube, data.attr));
    }

    public static void SetData(SnowCubeBehaviour itemBehaviour, SnowCubeData data)
    {
        var mComp = itemBehaviour.entity.Get<SnowCubeComponent>();
        if (data != null)
        {
            mComp.shape = data.s;
            mComp.color = data.col;
            mComp.tiling = DataUtils.DeSerializeVector2(data.tile);
        }
        else
        {
            mComp.shape = (int)SnowShape.Cube;
            mComp.color = DataUtils.ColorToString(AssetLibrary.Inst.colorLib.Get(0));
            mComp.tiling = Vector2.one;
        }
        itemBehaviour.SetColor(DataUtils.DeSerializeColor(mComp.color));
        itemBehaviour.SetShape((SnowShape)mComp.shape);
        itemBehaviour.SetTiling(mComp.tiling);
        SnowCubeManager.Inst.AddItem(itemBehaviour);
    }
}
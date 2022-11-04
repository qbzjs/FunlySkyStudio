using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author: Lishuzhan
/// Description:
/// Date: 2022-07-14
/// </summary>
public class IceCubeCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int) GameResType.IceCube);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int) GameResType.IceCube;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.IceCube; 
        gameComponent.modelType = NodeModelType.IceCube;
        if (GlobalFieldController.curMapMode == MapMode.Downtown)
        {
            GameUtils.CloseAllMesh(behav);
        }
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
        if (IceCubeManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast(IceCubeManager.MAX_COUNT_TIP);
            return;
        }
        var sBehav = nBehaviour as IceCubeBehaviour;
        IceCubeManager.Inst.AddItem(sBehav);
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
        SetData(behaviour as IceCubeBehaviour, GameUtils.GetAttr<IceCubeData>((int)BehaviorKey.IceCube, data.attr));
    }

    public static void SetData(IceCubeBehaviour itemBehaviour, IceCubeData data)
    {
        var mComp = itemBehaviour.entity.Get<IceCubeComponent>();
        mComp.tile = DataUtils.DeSerializeVector2(data.tile);
        itemBehaviour.SetTiling(mComp.tile);

    }
}
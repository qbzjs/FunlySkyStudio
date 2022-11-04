using Assets.Scripts.Game.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class WaterCubeCreater : SceneEntityCreater
{
    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.WaterCube);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.WaterCube;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Base;
        gameComponent.modelType = NodeModelType.WaterCube;
        if(GlobalFieldController.curMapMode == MapMode.Downtown)
        {
            GameUtils.CloseAllMesh(behav);
        }
        return behav;
    }
    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent = null)
    {
        LoggerUtils.Log("WaterCubeCreater SetData:" + data);
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
        SetData(behaviour as WaterCubeBehaviour, GameUtils.GetAttr<WaterData>((int)BehaviorKey.WaterData, data.attr));
    }

    public static void SetData(WaterCubeBehaviour behaviour, WaterData data)
    {
        WaterComponent compt = behaviour.entity.Get<WaterComponent>();
        if (data != null)
        {
            compt.id = data.id;
            compt.tiling = DataUtils.DeSerializeVector2(data.tiling);
            compt.v = data.v;
        }
        else
        {
            compt.id = 0;
            compt.tiling = behaviour.OldTiling();
            compt.v = behaviour.OldVelocity();
        }
        behaviour.Setup();
    }
}

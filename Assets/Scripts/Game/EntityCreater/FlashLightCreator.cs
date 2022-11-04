using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author : Tee Li
/// 描述：手电灯创建
/// 日期：2022/10/08
/// </summary>

public class FlashLightCreator : SceneEntityCreater
{
    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        FlashLightManager.Inst.AddNode(nBehaviour);
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
        SetData(behaviour as FlashLightBehaviour, GameUtils.GetAttr<FlashLightData>((int)BehaviorKey.FlashLight, data.attr));
    }

    public static void SetData(FlashLightBehaviour behaviour, FlashLightData data)
    {
        FlashLightComponent compt = behaviour.entity.Get<FlashLightComponent>();
        compt.id = data.id;
        compt.type = data.type;
        compt.range = data.range;
        compt.inten = data.inten;
        compt.radius = data.radius;       
        compt.mode = data.mode;
        compt.isReal = data.isReal;
        compt.time = data.time;
        compt.colors = StringToColorList(data.colors);

        var gameComponent = behaviour.entity.Get<GameObjectComponent>();
        gameComponent.uid = UidManager.Inst.GetUid(gameComponent.uid);
        gameComponent.modId = (int)GameResType.FlashLight;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.SpotLight;
        gameComponent.modelType = NodeModelType.FlashLight;

        behaviour.SetUp();
    }

    public override T Create<T>()
    {
        SceneEntity entity = world.NewEntity();
        GameObject assetGo = ModelCachePool.Inst.Get((int)GameResType.FlashLight);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        if (!assetGo.GetComponent<SpawnPointConstrainer>())
        {
            SpawnPointConstrainer adjustBehav = assetGo.AddComponent<SpawnPointConstrainer>();
            adjustBehav.minHeight = 0;
        }

        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        behav.gameObject.transform.localPosition = CameraUtils.Inst.GetCreatePosition();
        GameObjectComponent gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        return behav;
    }

    public static List<Color> StringToColorList(List<string> colors)
    {
        List<Color> list = new List<Color>();
        for (int i = 0; i < colors.Count; ++i)
        {
            list.Add(DataUtils.DeSerializeColor(colors[i]));
        }
        return list;
    }
}

using Assets.Scripts.Game.Core;
using UnityEngine;
using HLODSystem;

/// <summary>
/// Author:Tee-Li
/// 描述：默认火焰道具创建
/// 时间：2022/08/16 13:48
/// </summary>

public class FirePropCreator : SceneEntityCreater
{
    public override GameObject Clone(GameObject target)
    {
        return null;
    }
    public void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        FirePropManager.Inst.AddNode(nBehaviour);  
    }
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
        SetData(behaviour as FirePropBehaviour, GameUtils.GetAttr<FirePropData>((int)BehaviorKey.FireProp, data.attr), data);
    }
    //编辑模式和场景还原最终都会走这个
    public static void SetData(FirePropBehaviour behaviour, FirePropData data, NodeData nodeData = null)
    {
        FirePropComponent compt = behaviour.entity.Get<FirePropComponent>();
        compt.id = data.id;
        compt.flare = data.flare;
        compt.intensity =data.intensity;
        compt.collision = data.collision;
        compt.doDamage = data.doDamage;
        compt.hpDamage = data.hpDamage;
        compt.lightRange = data.lightRange;

        var gameComponent = behaviour.entity.Get<GameObjectComponent>();
        gameComponent.uid = UidManager.Inst.GetUid(gameComponent.uid);
        gameComponent.modId = (int)GameResType.FireProp;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Special;
        gameComponent.modelType = NodeModelType.FireProp;

        if(nodeData != null)
        {
            ((FirePropBehaviour)behaviour).data = nodeData;
        }
        else
        {
            ((FirePropBehaviour)behaviour).data = new NodeData(){
                id = (int)GameResType.FireProp
            };
        }


        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            behaviour.SetLODStatus(HLODState.High);
        }
    }
    public override T Create<T>()
    {
        SceneEntity entity = world.NewEntity();
        GameObject assetGo = new GameObject("FireProp");

        if(!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        if(!assetGo.TryGetComponent(out SpawnPointConstrainer adjustBehav))
        {
            adjustBehav = assetGo.AddComponent<SpawnPointConstrainer>();
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

    public static FirePropData GetDefaultData()
    {
        return new FirePropData
        {
            flare = 0,
            intensity = 1.5f,
            collision = 1,
            doDamage = 0,
            hpDamage = 20
        };
    }
}

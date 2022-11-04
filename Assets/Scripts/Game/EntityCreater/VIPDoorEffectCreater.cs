using Assets.Scripts.Game.Core;
using UnityEngine;

public class VIPDoorEffectCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        SceneEntity entity = world.NewEntity();
        var go = new GameObject("VIPDoorEffect");
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
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.VIPZone; 
        gameComponent.modelType = NodeModelType.VIPDoorEffect;
        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent)
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
        SetData(behaviour,data);
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data,bool saveId = false)
    {
        var findResult = data.attr.Find(x => x.k == (int)BehaviorKey.VIPDoorEffect);
        if (findResult != null)
        {
            VIPDoorEffectData doorEffectData = GameUtils.GetAttr<VIPDoorEffectData>((int) BehaviorKey.VIPDoorEffect, data.attr);
            int id;
            if (int.TryParse(doorEffectData.id,out id))
            {
                UpdateModel(behaviour as VIPDoorEffectBehaviour,id);
            }
            if (saveId)
            {
                VIPZoneManager.Inst.OnUgcUseSave(VIPComponentType.DoorEffect,doorEffectData.id);
            }
        }
    }
    
    public static bool UpdateModel(NodeBaseBehaviour behaviour,string gameResId)
    {
        int id;
        if (int.TryParse(gameResId,out id))
        {
            UpdateModel(behaviour as VIPDoorEffectBehaviour,id);
            return true;
        }

        return false;
    }
    
    public static void UpdateModel(VIPDoorEffectBehaviour behaviour,int gameResId)
    {
        GameObjectComponent gameObjectComponent = behaviour.entity.Get<GameObjectComponent>();
        GameObject par = gameObjectComponent.bindGo;
        var go = ModelCachePool.Inst.Get(gameResId);
        go.transform.SetParent(par.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        behaviour.assetObj = go;
        behaviour.entity.Get<VIPDoorEffectComponent>().id = gameResId.ToString();
        gameObjectComponent.modId = gameResId;
    }

    public static void SetDefaultData(NodeBaseBehaviour behaviour)
    {
        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid();
    }
}
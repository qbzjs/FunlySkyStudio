using Assets.Scripts.Game.Core;
using UnityEngine;

public class VIPZoneCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        SceneEntity entity = world.NewEntity();
        var go = new GameObject();
        go.name = "VIPZone";
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
        gameComponent.modId = (int) GameResType.VIPZone;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.VIPZone; 
        gameComponent.modelType = NodeModelType.VIPZone;
        
        if (behav.gameObject.GetComponent<SpawnPointConstrainer>() == null)
        {
            SpawnPointConstrainer spawnPointConstrainer = behav.gameObject.AddComponent<SpawnPointConstrainer>();
            spawnPointConstrainer.minHeight = 0;
        }
        VIPZoneManager.Inst.OnCreateNewVIPZone(behav as VIPZoneBehaviour);
        
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
    

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data)
    {
        var findResult = data.attr.Find(x => x.k == (int)BehaviorKey.VIPZone);
        if (findResult != null)
        {
            VIPZoneData zoneData = GameUtils.GetAttr<VIPZoneData>((int) BehaviorKey.VIPZone, data.attr);
            VIPZoneComponent zoneComponent = behaviour.entity.Get<VIPZoneComponent>();
            zoneComponent.passId = zoneData.passId;
            zoneComponent.dcItemId = zoneData.dcItemId;
            zoneComponent.isEdit = zoneData.isEdit;
            if (zoneData.passId != null)
            {
                VIPZoneManager.Inst.OnUgcUseSave(VIPComponentType.PassDC,zoneData.passId);
            }
            VIPZoneManager.Inst.AddVIPZone(behaviour as VIPZoneBehaviour);
        }
    }

    public static void SetDefaultData(NodeBaseBehaviour behaviour)
    {
        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid();
    }
}
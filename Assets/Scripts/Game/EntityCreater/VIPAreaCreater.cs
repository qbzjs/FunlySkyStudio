using Assets.Scripts.Game.Core;
using UnityEngine;

public class VIPAreaCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        SceneEntity entity = world.NewEntity();
        var go = ModelCachePool.Inst.Get((int)GameResType.VIPArea);
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
        gameComponent.modId = (int) GameResType.VIPArea;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.VIPZone; 
        gameComponent.modelType = NodeModelType.VIPArea;
        
        if (behav.gameObject.GetComponent<SpawnPointConstrainer>() == null)
        {
            SpawnPointConstrainer spawnPointConstrainer = behav.gameObject.AddComponent<SpawnPointConstrainer>();
            spawnPointConstrainer.minHeight = 0;
        }
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
    }

    public static void SetDefaultData(NodeBaseBehaviour behaviour)
    {
        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid();
    }
}
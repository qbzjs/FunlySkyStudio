using Assets.Scripts.Game.Core;
using TMPro;
using UnityEngine;

public enum SpawnPointCreaterType
{
    EmptyData,
    ContinueLoadingData,
}

public class SpawnPointCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.BornPoint);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.BornPoint;
        gameComponent.type = ResType.Special;
        gameComponent.handleType = NodeHandleType.Born;
        gameComponent.modelType = NodeModelType.BornPoint;
        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        var sBehav = nBehaviour as SpawnPointBehaviour;
        SpawnPointManager.Inst.AddSpawnList(sBehav);
        sBehav.SetNumText(SpawnPointManager.Inst.spawnList.Count);
        GameManager.Inst.maxPlayer = SpawnPointManager.Inst.spawnList.Count;
        if (PVPTeamManager.Inst.IsTeamMode() && SpawnPointManager.Inst.spawnList.Count < GameConsts.MAX_PLAYER)
        {
            PVPTeamManager.Inst.UpdateTeamInfo();
        }
    }

    public static void SetData(SpawnPointBehaviour behaviour, SpawnData data, SpawnPointCreaterType type)
    {
        Vector3 pos = CameraUtils.Inst.GetCreatePosition();
        Vector3 rot = Vector3.zero;
        int id = 0;
        int hpValue = 0;

        switch (type)
        {
            case SpawnPointCreaterType.EmptyData:
                id = SpawnPointManager.Inst.spawnList.Count + 1;
                hpValue = 100;
                break;
            case SpawnPointCreaterType.ContinueLoadingData:
                rot = DataUtils.DeSerializeVector3(data.r);
                pos = DataUtils.DeSerializeVector3(data.p);
                id = data.id;
                hpValue = data.hp;
                //兼容旧数据
                if (data.id == 0)
                {
                    var comp = SceneBuilder.Inst.HPEntity.Get<HPControlComponent>();
                    id = SpawnPointManager.Inst.spawnList.Count+1;
                    hpValue = comp.customHP;
                }
                break;
        }
        if(SceneBuilder.Inst.SpawnPoint == null)
        {
            SceneBuilder.Inst.SpawnPoint = new GameObject("SpawnPoint");
            SceneBuilder.Inst.SpawnPoint.transform.SetParent(SceneBuilder.Inst.SceneParent);
        }
        var newParent = SceneBuilder.Inst.SpawnPoint.transform;
        behaviour.transform.SetParent(newParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = Vector3.one;
        behaviour.id = id;
        behaviour.hpValue = hpValue;
        behaviour.OnInitByCreate();
        behaviour.SetNumText(id);
        SpawnPointManager.Inst.AddSpawnList(behaviour);
    }
}

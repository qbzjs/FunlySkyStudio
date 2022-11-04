using Assets.Scripts.Game.Core;
using UnityEngine;

public class VIPCheckCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        SceneEntity entity = world.NewEntity();
        var go = new GameObject("VIPCheck");
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
        gameComponent.modelType = NodeModelType.VIPCheck;

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
        SetData(behaviour,data);
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data,bool saveId = false)
    {
        var findResult = data.attr.Find(x => x.k == (int)BehaviorKey.VIPCheck);
        if (findResult != null)
        {
            if (behaviour.GetComponent<VIPCheckBoundControl>() == null)
            {
                behaviour.gameObject.AddComponent<VIPCheckBoundControl>();
            }
            behaviour.GetComponent<VIPCheckBoundControl>().InitEffect();
            behaviour.GetComponent<VIPCheckBoundControl>().UpdateEffectShow();
            
            if (behaviour.gameObject.GetComponent<SpawnPointConstrainer>() == null)
            {
                SpawnPointConstrainer spawnPointConstrainer = behaviour.gameObject.AddComponent<SpawnPointConstrainer>();
                spawnPointConstrainer.minHeight = 0;
            }
            
            VIPCheckData checkData = GameUtils.GetAttr<VIPCheckData>((int) BehaviorKey.VIPCheck, data.attr);
            UpdateModel(behaviour as VIPCheckBehaviour,checkData.id);
            //UGC赋值
            behaviour.entity.Get<VIPCheckComponent>().id = checkData.id;
            if (saveId)
            {
                VIPZoneManager.Inst.OnUgcUseSave(VIPComponentType.Check,checkData.id);
            }
        }
    }

    public static bool UpdateModel(NodeBaseBehaviour behaviour,string key)
    {
        int id = GetModIdByKey(key);
        string texPath = GetTexByKey(key);
        if (id == -1)
        {
            return false;
        }
        UpdateModel(behaviour as VIPCheckBehaviour,key,id,texPath);
        return true;
    }

    private static int GetModIdByKey(string key)
    {
        switch (key)
        {
            case VIPZoneConstant.CHECK_ID_1:
                return (int)GameResType.VIPCheck;
            case VIPZoneConstant.CHECK_ID_2:
            case VIPZoneConstant.CHECK_ID_3:
            case VIPZoneConstant.CHECK_ID_4:
            case VIPZoneConstant.CHECK_ID_5:
                return (int)GameResType.VIPCheck2;
        }

        return -1;
    }

    private static string GetTexByKey(string key)
    {
        switch (key)
        {
            case VIPZoneConstant.CHECK_ID_1:
                return null;
            case VIPZoneConstant.CHECK_ID_2:
                return VIPZoneConstant.CHECK_TEXTURE_2;
            case VIPZoneConstant.CHECK_ID_3:
                return VIPZoneConstant.CHECK_TEXTURE_3;
            case VIPZoneConstant.CHECK_ID_4:
                return VIPZoneConstant.CHECK_TEXTURE_4;
            case VIPZoneConstant.CHECK_ID_5:
                return VIPZoneConstant.CHECK_TEXTURE_5;
        }

        return null;
    }
    
    public static void UpdateModel(VIPCheckBehaviour behaviour,string key,int gameResId,string texPath)
    {
        GameObjectComponent gameObjectComponent = behaviour.entity.Get<GameObjectComponent>();
        GameObject par = gameObjectComponent.bindGo;
        var go = ModelCachePool.Inst.Get(gameResId);
        if (texPath != null)
        {
            UpdateTex(go,texPath);
        }
        go.transform.SetParent(par.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        behaviour.assetObj = go;
        behaviour.entity.Get<VIPCheckComponent>().id = key;
        gameObjectComponent.modId = gameResId;
    }
    
    private static void UpdateTex(GameObject go, string texPath)
    {
        Material material = go.GetComponent<Renderer>().material;
        material.mainTexture = ResManager.Inst.LoadRes<Texture>(texPath);
    }
    
    public static void SetDefaultData(NodeBaseBehaviour behaviour)
    {
        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid();
    }
}
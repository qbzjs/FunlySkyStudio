using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using SavingData;
using BudEngine.NetEngine;

public class SceneParser:CInstance<SceneParser>
{
    public string StageToMapJson()
    {
        MapData mapData = new MapData();
        mapData.version = 1;
        mapData.sky = GetSkyData();
        mapData.dir = GetDirData();
        mapData.ter = GetTerrainData();
        mapData.espawn = GetEditSpawn();
        mapData.spawns = SpawnPointManager.Inst.GetSpawnsData();
        mapData.maxPlayers = GetMaxPlayers();
        mapData.bgmusic = GetBGMusic();
        mapData.canFly = GetCanFly();
        mapData.postprocess = GetPostProcessData();
        mapData.pvpData = GetPVPWaitAreaData();
        mapData.setHP = GetHPSet();
        mapData.customHP = 0;//旧数据会使用此字段，1.47.0后废弃
        mapData.setBaggage = GetBaggageSet();
        mapData.dmgSrcs = GetDamageSources();
        mapData.weather = GetWeather();
        var parent = SceneBuilder.Inst.StageParent;
        mapData.pref = GetChildrenData(parent, null);
        mapData.resList = GlobalFieldController.ugcNodeData;
        mapData.editTime = GameManager.Inst.gameMapInfo.editTime;
        mapData.defaultSpawnId = SpawnPointManager.Inst.defaultSpawnId;
        mapData.uMatList = GlobalFieldController.ugcMatData;
        return JsonConvert.SerializeObject(mapData);
    }

    private WeatherSaveParams GetWeather()
    {
        WeatherComponent weatherComponent = SceneBuilder.Inst.WeatherEntity.Get<WeatherComponent>();
        WeatherSaveParams saveParams = new WeatherSaveParams()
        {
            weatherType = weatherComponent.weatherType
        };
        return saveParams;
    }

    public string StageToPropJson()
    {
        var cloneStage = SceneBuilder.Inst.CloneTarget(SceneBuilder.Inst.StageParent.gameObject, true);
        var allNodes = GetAllNodeInFirstLayer(cloneStage.transform);
        var entityList = GetAllSceneEntity(cloneStage.transform);
        GameManager.Inst.gameMapInfo.highestResIds = GetMinimumVersion(entityList);
        var entity = SceneBuilder.Inst.CombineNode(allNodes, cloneStage.transform);
        string propJson = SceneEntityToPropJson(entity);
        cloneStage.SetActive(false);
        SceneBuilder.Inst.DestroyEntity(cloneStage);
        return propJson;
    }


    public string SceneEntityToPropJson(SceneEntity entity)
    {
        var gameComp = entity.Get<GameObjectComponent>();
        var resType = gameComp.type;
        gameComp.type = ResType.UGC;
        gameComp.uid = 0;
        var ignoreKeys = new List<int> { (int)BehaviorKey.RPAnim , (int)BehaviorKey.Movement, (int)BehaviorKey.ShowHide, (int)BehaviorKey.CollectControl};
        var nodeData = GetEntityData(entity, ignoreKeys);
        var trans = entity.Get<GameObjectComponent>().bindGo.transform;
        nodeData.prims = GetChildrenData(trans, ignoreKeys, true);
        string nodeJson = JsonConvert.SerializeObject(nodeData);
        gameComp.type = resType;
        return nodeJson;
    }

    public void GetResList()
    {
        MapData mapData = new MapData();
        var parent = SceneBuilder.Inst.StageParent;
        GameManager.Inst.ugcPropList.Clear();
        mapData.pref = GetChildrenPropData(parent, null);
        GameManager.Inst.gameMapInfo.propList = GameManager.Inst.ugcPropList.ToArray();
    }
    public void GetDcList()
    {
        List<DcSaveInfo> dcList = new List<DcSaveInfo>();
        if (UgcClothItemManager.Inst!=null)
        {
            dcList =  UgcClothItemManager.Inst.AddDCList(dcList);
        }
        if (DCManager.Inst!=null)
        {
            dcList = DCManager.Inst.AddList(dcList);
        }
        GameManager.Inst.gameMapInfo.dcList = dcList.ToArray();
    }
    public void GetUnitySaveData()
    {
        UnitySaveData unityData = new UnitySaveData();
        unityData.openBlood = GetHPSet();
        unityData.customBlood = 0;//旧数据会使用此字段，1.47.0后废弃
        unityData.hasLeaderboard = LeaderBoardManager.Inst.GetLeaderBoardSet();
        unityData.openBaggage = GetBaggageSet();
        unityData.dmgSrcs = GetDamageSources();
        unityData.crystalList = CrystalStoneManager.Inst.GetAllCrystalsList();
        GameManager.Inst.gameMapInfo.unityData = JsonConvert.SerializeObject(unityData);
    }

    private SkyData GetSkyData()
    {
        SkyData data = default;
        data.skyId = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().skyboxId;
        data.type = (int) RenderSettings.ambientMode;
        data.scol = DataUtils.ColorToString(RenderSettings.ambientSkyColor);
        data.ecol = DataUtils.ColorToString(RenderSettings.ambientEquatorColor);
        data.gcol = DataUtils.ColorToString(RenderSettings.ambientGroundColor);
        
        data.skyboxType = (int) SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().skyboxType;
        data.dayLength = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().dayLength;
        data.daytimeHour = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().daytimeHour;
        data.daytimeMin = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().daytimeMin;

        return data;
    }

    private DirLightData GetDirData()
    {
        DirLightData data = default;
        var dlComp = SceneBuilder.Inst.DirLight.entity.Get<DirLightComponent>();
        data.anx = dlComp.anglex;
        data.any = dlComp.angley;
        data.inte = dlComp.intensity;
        data.lico = DataUtils.ColorToString(dlComp.color);
        return data;
    }

    private PostProcessData GetPostProcessData()
    {
        PostProcessData data = new PostProcessData();
        var pComp = SceneBuilder.Inst.PostProcessBehaviour.entity.Get<PostProcessComponent>();
        data.bActive = pComp.bloomActive;
        data.bInte = pComp.bloomIntensity;
        return data;
    }

    private PVPWaitAreaData GetPVPWaitAreaData()
    {
        var behav = PVPWaitAreaManager.Inst.PVPBehaviour;
        GameManager.Inst.gameMapInfo.pvpData = string.Empty;
        if (behav != null)
        {
            PVPWaitAreaData data = new PVPWaitAreaData();
            data.p = DataUtils.Vector3ToString(behav.transform.localPosition);
            data.r = DataUtils.Vector3ToString(behav.transform.localEulerAngles);
            data.s = DataUtils.Vector3ToString(behav.transform.localScale);
            var comp = behav.entity.Get<PVPWaitAreaComponent>();
            RaceGameData rData = new RaceGameData();
            rData.pvpTime = comp.raceData.pvpTime;
            rData.taskArg = comp.raceData.taskArg;
            rData.taskArga = comp.raceData.taskArga;
            data.gameCondition = JsonConvert.SerializeObject(rData);
            data.gameMode = comp.gameMode;
            data.teamList = comp.teamList;
            //Save PvpData To MapInfo
            PVPData pvpData = new PVPData()
            {
                pvpMode = 1,
                winType = comp.gameMode,
                gameTime = rData.pvpTime,
                teamList = comp.teamList
            };
            GameManager.Inst.gameMapInfo.pvpData = JsonConvert.SerializeObject(pvpData);
            return data;
        }

        return null;
    }

    private GameTerrainData GetTerrainData()
    {
        GameTerrainData data = new GameTerrainData() ;
        var tComp = SceneBuilder.Inst.TerrainEntity.Get<TerrainComponent>();
        data.matId = tComp.matId;
        data.uurl = tComp.umatUrl;
        data.umat = tComp.umapId;
        data.cols = DataUtils.ColorToString(tComp.color);
        data.terrainSize = tComp.terrainSize;
        return data;
    }


    private SpawnData GetEditSpawn()
    {
        if(SceneBuilder.Inst.EditSpawn!=null){
            return SceneBuilder.Inst.EditSpawn;
        }else{
            SpawnData data = new SpawnData();
            data.p = DataUtils.Vector3ToString(Vector3.zero);
            data.r = DataUtils.Vector3ToString(new Vector3(14.9f,0,0));
            return data;
        } 
    }

    private SpawnData GetPlaySpawn()
    {
        SpawnData data = new SpawnData();
        data.p = DataUtils.Vector3ToString(SceneBuilder.Inst.SpawnPoint.transform.localPosition);
        data.r = DataUtils.Vector3ToString(SceneBuilder.Inst.SpawnPoint.transform.eulerAngles);
        return data;
    }

    private int GetMaxPlayers()
    {
        int maxPlayers = GameManager.Inst.maxPlayer;
#if !UNITY_EDITOR
        //记录房间最大人数
        GameManager.Inst.gameMapInfo.maxPlayer = maxPlayers;
#endif
        return maxPlayers;
    }

    private BGMusicData GetBGMusic()
    {
        BGMusicData data = default;
        var comp = SceneBuilder.Inst.BGMusicEntity.Get<BGMusicComponent>();
        data.bgName = comp.bgName;
        data.bgUrl = comp.bgUrl;
        data.musicId = comp.musicId;
        data.musicType = comp.musicType;
        var enrComp = SceneBuilder.Inst.BGMusicEntity.Get<BGEnrMusicComponent>();
        data.eId = enrComp.enrMusicId;
#if !UNITY_EDITOR
        GameManager.Inst.gameMapInfo.audioUrl = comp.bgUrl;
#endif
        return data;
    }

    private int GetCanFly()
    {
        var tComp = SceneBuilder.Inst.CanFlyEntity.Get<CanFlyComponent>();
        return tComp.canFly;
    }

    public int GetHPSet()
    {
        var comp = SceneBuilder.Inst.HPEntity.Get<HPControlComponent>();
        return comp.setHP;
    }

    public int GetCustomHP(string playerId)
    {
        int spawnId = SpawnPointManager.Inst.GetPlayerSpawnId(playerId);
        SpawnPointBehaviour sBehav = SpawnPointManager.Inst.GetSpawnPointBehavByGameMode(spawnId);
        return sBehav.hpValue;
    }
    public List<int> GetDamageSources()
    {
        var comp = SceneBuilder.Inst.HPEntity.Get<HPControlComponent>();
        List<int> dmgList = comp.dmgSrcs;
        if(dmgList.Count == 0)
        {
            dmgList.Add((int)DamageSource.Player);
        }
        return dmgList;
    }

    public void ResetDamageSources()
    {
        var comp = SceneBuilder.Inst.HPEntity.Get<HPControlComponent>();
        List<int> dmgList = comp.dmgSrcs;
        dmgList.Clear();
        dmgList.Add((int)DamageSource.Player);
    }

    public void UpdateDamageSources(string ctrType,int dmgSource)
    {
        var comp = SceneBuilder.Inst.HPEntity.Get<HPControlComponent>();
        List<int> dmgList = comp.dmgSrcs;
        if(ctrType == "Add")
        {
            if(!dmgList.Contains(dmgSource))
            {
                dmgList.Add(dmgSource);
            }
        }
        else if(ctrType == "Remove")
        {
            if(dmgList.Contains(dmgSource))
            {
                dmgList.Remove(dmgSource);
            }
        }
        
        //打印详情
        // string dumpListStr = "";
        // for(int i = 0;i<dmgList.Count;i++)
        // {
        //     dumpListStr = dumpListStr + dmgList[i] + "|";
        // }
        // LoggerUtils.Log("当前伤害来源列表："+dumpListStr);
	}
    public int GetBaggageSet()
    {
        return 0;
        var comp = SceneBuilder.Inst.BaggageEntity.Get<BaggageComponent>();
        return comp.openBaggage;
    }

    //isPropMode:如果是保存素材，uid清0
    //如果是UGC 素材且使用离线渲染
    public virtual List<NodeData> GetChildrenData(Transform node, List<int> ignorekeys, bool isPropMode = false)
    {
        List<NodeData> datas = new List<NodeData>();
        var entitys = GetAllNodeInFirstLayer(node);
        for (int i = 0; i < entitys.Count; i++)
        {
            var nodeData = GetEntityData(entitys[i], ignorekeys);
            var trans = entitys[i].Get<GameObjectComponent>().bindGo.transform;
            nodeData.uid = isPropMode ? 0: nodeData.uid;
            datas.Add(nodeData);
            AddUGCMatData(entitys[i]);
            // 钓鱼
            if (nodeData.id == (int)GameResType.FishingModel)
            {
                nodeData.prims = GetFishingPrims(trans, ignorekeys);
                continue;
            }

            if(trans.TryGetComponent<UGCCombBehaviour>(out var uGCCombBehaviour))
            {
                //老版本没有rid的素材
                if (string.IsNullOrEmpty(nodeData.rid))
                {
                    nodeData.prims = GetChildrenData(trans, ignorekeys, isPropMode);
                }
            }
            else
            {
                nodeData.prims = GetChildrenData(trans, ignorekeys, isPropMode);
            }
        }
        return datas;
    }
    private void AddUGCMatData(SceneEntity entity)
    {
        if (entity.Components.ContainsKey(typeof(MaterialComponent)))
        {
            var com = entity.Components[typeof(MaterialComponent)] as MaterialComponent;
            if (!string.IsNullOrEmpty(com.umat) && !GlobalFieldController.ugcMatData.ContainsKey(com.umat))
            {
                var udata = new UGCMatSaveData();
                udata.uurl = com.uurl;
                GlobalFieldController.ugcMatData.Add(com.umat, udata);
            }
        }
    }
//    public virtual List<NodeData> GetUGCData(UGCCombBehaviour uGCCombBehaviour, List<int> ignorekeys, bool isPropMode = false)
//    {
//        var rid = uGCCombBehaviour.entity.Get<GameObjectComponent>().resId;
//        if (!string.IsNullOrEmpty(rid) && GlobalFieldController.ugcNodeData.ContainsKey(rid))
//        {
//            return GlobalFieldController.ugcNodeData[rid];
//        }
//        else
//        {
//            return GetChildrenData(uGCCombBehaviour.transform, ignorekeys, isPropMode);
//        }
//    }

    private List<NodeData> GetFishingPrims(Transform trans, List<int> ignorekeys)
    {
        var prims = new List<NodeData>();
        var rodParent = trans.Find(FishingEditManager.Inst.RodParentPath);
        if (rodParent.childCount > 0)
        {
            var rod = rodParent.GetChild(0).GetComponent<NodeBaseBehaviour>();
            prims.Add(GetEntityData(rod.entity, ignorekeys));
        }

        var hookParent = trans.Find(FishingEditManager.Inst.HookParentPath);
        if (hookParent.childCount > 0)
        {
            var hook = hookParent.GetChild(0).GetComponent<NodeBaseBehaviour>();
            prims.Add(GetEntityData(hook.entity, ignorekeys));
        }

        return prims;
    }

    public virtual List<NodeData> GetChildrenPropData(Transform node, List<int> ignorekeys)
    {
        List<NodeData> datas = new List<NodeData>();
        var entitys = GetAllNodeInFirstLayer(node);
        for (int i = 0; i < entitys.Count; i++)
        {
            var nodeData = GetEntityData(entitys[i], ignorekeys);
            var trans = entitys[i].Get<GameObjectComponent>().bindGo.transform;
            nodeData.prims = GetChildrenPropData(trans, ignorekeys);
            datas.Add(nodeData);
            if (!string.IsNullOrEmpty(nodeData.rid))
            {
                GameManager.Inst.ugcPropList.Add(nodeData.rid);
            }
        }
        return datas;
    }

    public string UploadToStore(SceneEntity entity)
    {
        if (IsContainSpecialEntity(entity))
        {
            return string.Empty;
        }
        var tempEntity = entity;
        var gComp = entity.Get<GameObjectComponent>();
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        NodeData nodeData = null;
        string jsonContent = string.Empty;
        var ignoreKeys = new List<int> { (int)BehaviorKey.RPAnim, (int)BehaviorKey.Movement , (int)BehaviorKey.ShowHide,(int)BehaviorKey.CollectControl,(int)BehaviorKey.Pickablity,(int)BehaviorKey.FollowBox,(int)BehaviorKey.Edibility,(int)BehaviorKey.Catchability};
        switch (gComp.type)
        {
            case ResType.PGC:
                if (GameManager.Inst.unityConfigInfo.isQueen == (int)IsQueen.True)
                {
                    jsonContent = UpLoadSingle(bindGo, tempEntity, nodeData, ignoreKeys);
                }
                break;
            case ResType.Single:
                jsonContent = UpLoadSingle(bindGo, tempEntity, nodeData, ignoreKeys);
                break;
            case ResType.CommonCombine:
                var oldResType = tempEntity.Get<GameObjectComponent>().type;
                tempEntity.Get<GameObjectComponent>().type = ResType.UGC;
                var oldUid = tempEntity.Get<GameObjectComponent>().uid;
                tempEntity.Get<GameObjectComponent>().uid = 0;
                nodeData = GetEntityData(tempEntity, ignoreKeys);
                var ctrans = tempEntity.Get<GameObjectComponent>().bindGo.transform;
                nodeData.prims = GetChildrenData(ctrans, ignoreKeys, true);
                jsonContent = JsonConvert.SerializeObject(nodeData);
                tempEntity.Get<GameObjectComponent>().type = oldResType;
                tempEntity.Get<GameObjectComponent>().uid = oldUid;
                break;
        }

        //Convert To UGC Type
        if (string.IsNullOrEmpty(jsonContent))
        {
            LoggerUtils.Log("upload prop json content is Empty");
        }
        return jsonContent;
       
    }
    public string UpLoadSingle(GameObject bindGo,SceneEntity tempEntity, NodeData nodeData ,List<int> ignoreKeys)
    {
        Transform oldParent = bindGo.transform.parent;
        var conBehav = SceneBuilder.Inst.CreateCombineNode<UGCCombBehaviour>(null, bindGo.transform.localPosition, Vector3.zero, Vector3.one, ResType.UGC);
        conBehav.transform.SetPositionAndRotation(bindGo.transform.position, bindGo.transform.rotation);
        bindGo.transform.SetParent(conBehav.transform);
        tempEntity = conBehav.entity;
        conBehav.entity.Get<GameObjectComponent>().type = ResType.UGC;
        nodeData = GetEntityData(tempEntity, ignoreKeys);
        var trans = tempEntity.Get<GameObjectComponent>().bindGo.transform;
        nodeData.prims = GetChildrenData(trans, ignoreKeys, true);
        string jsonContent = JsonConvert.SerializeObject(nodeData);
        bindGo.transform.SetParent(oldParent);
        SceneBuilder.Inst.DestroyEntity(conBehav.gameObject);

        return jsonContent;
    }

    public bool IsContainSpecialEntity(SceneEntity entity)
    {
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        var nodeBehvs = bindGo.GetComponentsInChildren<NodeBaseBehaviour>();
        for (var i = 0; i < nodeBehvs.Length; i++)
        {
            ResType resType = nodeBehvs[i].entity.Get<GameObjectComponent>().type;
            int modId = nodeBehvs[i].entity.Get<GameObjectComponent>().modId;
            if (resType == ResType.UGC
                || (resType == ResType.PGC && GameManager.Inst.unityConfigInfo.isQueen == (int)IsQueen.False)
                || (resType == ResType.Single && GameManager.Inst.priConfigData[modId].uploadType == (int)NodeUploadType.Unable))
            {
                return true;
            }
        }
        return false;
    }

    private List<SceneEntity> GetAllNodeInFirstLayer(Transform node)
    {
        List<SceneEntity> behaviours = new List<SceneEntity>();
        for (int i = 0; i < node.childCount; i++)
        {
            var nBehaviour = node.GetChild(i).GetComponent<NodeBaseBehaviour>();
            if (nBehaviour != null)
            {
                behaviours.Add(nBehaviour.entity);
            }
        }
        return behaviours;
    }

    private List<SceneEntity> GetAllSceneEntity(Transform node)
    {
        List<SceneEntity> entityList = new List<SceneEntity>();
        var nodeBevs = node.GetComponentsInChildren<NodeBaseBehaviour>();
        foreach(var bev in nodeBevs)
        {
            entityList.Add(bev.entity);
        }
        return entityList;
    }

    private NodeData GetEntityData(SceneEntity entity, List<int> ignorekeys)
    {
        var data = new NodeData();
        var gComp = entity.Get<GameObjectComponent>();
        data.rid = gComp.resId;
        data.uid = gComp.uid;
        data.id = gComp.modId;
        data.type = (int)gComp.type;
        data.p = DataUtils.Vector3ToString(gComp.bindGo.transform.localPosition);
        data.r = DataUtils.Vector3ToString(gComp.bindGo.transform.localEulerAngles);
        var scale = DataUtils.LimitVector3(gComp.bindGo.transform.localScale);
        data.s = DataUtils.Vector3ToString(scale);
        data.attr = GetComponentsAttrByEntity(entity, ignorekeys);
        return data;
    }


    private List<BehaviorKV> GetComponentsAttrByEntity(SceneEntity entity, List<int> ignorekeys)
    {
        List<BehaviorKV> allAttrs = new List<BehaviorKV>();
        foreach (var val in entity.Components.Values)
        {
            var kv = val.GetAttr();
            if (kv != null && (ignorekeys == null || !ignorekeys.Contains(kv.k)))
            {
                allAttrs.Add(kv);
            }
        }
        return allAttrs;
    }

    public List<SceneEntity> GetAllEntity(SceneEntity entity)
    {
        var gComp = entity.Get<GameObjectComponent>();
        var target = gComp.bindGo;
        List<SceneEntity> entitys = new List<SceneEntity>();
        NodeBaseBehaviour[] behaviours = target.GetComponentsInChildren<NodeBaseBehaviour>();
        for(int i = 0; i < behaviours.Length; i++)
        {
            entitys.Add(behaviours[i].entity);
        }
        return entitys;
    }

    public string[] GetMinimumVersion(List<SceneEntity> entitiys)
    {
        int maxBaseId = 0; 
        int maxPgcId = 0;
        int maxparticularMode = 0;

        for (int i = 0; i < entitiys.Count; i++)
        {
            var gComp = entitiys[i].Get<GameObjectComponent>();
            int modId = gComp.modId;
            int type = GetResTypeByModId(modId);

            switch (type)
            {
                case (int)ModeResType.PGC:
                    if (modId > maxPgcId)
                    {
                        maxPgcId = modId;
                    }
                    break;
                case (int)ModeResType.Base:
                    if (modId > maxBaseId)
                    {
                        maxBaseId = modId;
                    }
                    break;
                case (int)ModeResType.particular:
                    if (modId > maxparticularMode)
                    {
                        maxparticularMode = modId;
                    }
                    break;
            }
        }
        List<string> resIdList = new List<string>();
        resIdList.Add(maxPgcId.ToString());
        resIdList.Add(maxBaseId.ToString());
        resIdList.Add(maxparticularMode.ToString());
        LoggerUtils.Log("resIdList = " + JsonConvert.SerializeObject(resIdList));
        return resIdList.ToArray();
    }

    private int GetResTypeByModId(int modId)
    {
        if (modId > 0 && modId <= 1000)
        {
            return (int)ModeResType.Base;
        }
        else if (modId > 2500 && modId <= 3000)
        {
            return (int)ModeResType.particular;
        }
        else if (modId > 10000)
        {
            return (int)ModeResType.PGC;
        }
        return -1;
    }
}

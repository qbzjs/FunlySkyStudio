using System.Net.Mime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Xml.Serialization;
using System.Linq;
using Assets.Scripts.Game.Core;
using DesperateDevs.Utils;
using HLODSystem;
using Leopotam.Ecs;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using Object = System.Object;

public partial class SceneBuilder : CInstance<SceneBuilder>
{
    public Transform SceneParent;
    public Transform StageParent;
    public GameObject SpawnPoint;
    public GameObject TerrainGo;
    public SceneEntity TerrainEntity;
    public SceneEntity BGMusicEntity;
    public SceneEntity CanFlyEntity;
    public SceneEntity WeatherEntity;
    public SceneEntity HPEntity;
    public SceneEntity BaggageEntity;
    public BGMusicBehaviour BgBehaviour;
    public DirLightBehaviour DirLight;
    public SkyboxBehaviour SkyboxBev;
    public SpawnData EditSpawn;
    public PostProcessBehaviour PostProcessBehaviour;
    public List<NodeBaseBehaviour> allControllerBehaviours = new List<NodeBaseBehaviour>();
    public List<NodeBaseBehaviour> hideEntityBevs = new List<NodeBaseBehaviour>();//play model need to hide objects
    
    private readonly int lightMax = 6;
    private int curLight = 0;
    private ECSWorld ecsWorld;
    private SceneEntityFactory factory;
    private Vector3 defaultTerrainPos = Vector3.zero;
    private Vector3 defaultPlayCameraPos = Vector3.zero;
    private Vector3 defaultPlayCameraRot = Vector3.zero;
    private Vector3 cloneOffset = new Vector3(-0.2f, 0.0001f, 0.0001f);

    public List<NodeBaseBehaviour> likeEntityBevs = new List<NodeBaseBehaviour>();
    public List<NodeBaseBehaviour> attentionEntityBevs = new List<NodeBaseBehaviour>();
    public List<NodeBaseBehaviour> favoriteEntityBevs = new List<NodeBaseBehaviour>();

    

    public void Init()
    {
        ecsWorld = new ECSWorld();
        factory = new SceneEntityFactory(ecsWorld);
        hideEntityBevs = new List<NodeBaseBehaviour>();
        likeEntityBevs = new List<NodeBaseBehaviour>();
        attentionEntityBevs = new List<NodeBaseBehaviour>();
        favoriteEntityBevs = new List<NodeBaseBehaviour>();
    }

    public void InitSceneParent()
    {
        SceneParent = new GameObject("Scene").transform;
        StageParent = new GameObject("Stage").transform;
        StageParent.SetParent(SceneParent);
        PostProcessBehaviour = factory.BindCreater<PostProcessCreater>().Create<PostProcessBehaviour>();
        CreateDirLight();
        CreateGameTimeManager();
        CreateSkyboxBev();
        RebuildScene();
    }

    public void InitPreviewSceneParent()
    {
        SceneParent = new GameObject("Scene").transform;
        StageParent = new GameObject("Stage").transform;
        StageParent.SetParent(SceneParent);
    }

    public void RebuildScene()
    {
        CreateTerrain();
        CreateBGMusic();
        InitEmptyWeather();
        SetCanFly(1);
        SetHasHP(0);
        SetHasBaggage(0);
    }

    private void InitEmptyWeather()
    {
        SetWeatherWithSaveParams(new WeatherSaveParams()); 
    }

    private void SetWeatherWithSaveParams(WeatherSaveParams weatherParams)
    {
        if (WeatherEntity == null)
        {
            WeatherEntity = ecsWorld.NewEntityNoRecord();
        }

        if (weatherParams == null)
        {
            weatherParams = new WeatherSaveParams();
        }
        
        WeatherEntity.Get<WeatherComponent>().weatherType = weatherParams.weatherType;
        WeatherManager.Inst.ChangeWeatherWithSaveParams(weatherParams);
    }

    public void SetWeatherType(EWeatherType weatherType)
    {
        if (WeatherEntity == null)
        {
            WeatherEntity = ecsWorld.NewEntityNoRecord();
        }
        
        WeatherEntity.Get<WeatherComponent>().weatherType = weatherType;
        WeatherManager.Inst.ChangeWeatherType(weatherType);
    }


    public void CreateEmptyScene()
    {
        GetSkyboxCreater().CreateEmptyMapSkybox(SkyboxManager.Inst.defaultSkyId);
        var data = SetDirLightInitDataByEmpty(SkyboxManager.Inst.defaultSkyId);
        SetDirLight(data);
        SetDefaultBGMusic();
        var terData = new GameTerrainData();
        terData.matId = 8;
        SetTerrain(terData, Color.white, (int)TerrainSizeConfigs.fiveTimesTerrainSize);
        SetCanFly(1);
        SetHasHP(0);
        SetHasBaggage(0);
        PostProcessCreater.SetDefault(PostProcessBehaviour);
        UGCBehaviorManager.Inst.InitLocalOfflineRes(null);
        GlobalFieldController.whiteListMask.SetInWhiteList(WhiteListMask.WhiteListType.OfflineRender);

    }

    public void ParseAndBuild(string json)
    {
        allControllerBehaviours.Clear();
        ShowHideManager.Inst.ClearBevs();
        ParsePropWithTipsManager.Inst.DestroyGameObj();
        SwitchManager.Inst.ClearBevs();
        CollectControlManager.Inst.ClearBevs();
        NodeBehaviourManager.Inst.ClearManagers();
        PortalPointManager.Inst.RefreshPortalPointManagerDic();
        DCManager.Inst.Clear();
        MapData mData = JsonConvert.DeserializeObject<MapData>(json);
        MapNodeData.SetupMap(mData);
        UGCBehaviorManager.Inst.InitLocalOfflineRes(mData);
        UGCTexManager.Inst.ParserUmatList(mData.uMatList);
        var terColor = DataUtils.DeSerializeColor(mData.ter.cols);
        var startParseJsonTime = Time.realtimeSinceStartup;
        SetTerrain(mData.ter, terColor, mData.ter.terrainSize);
        GetSkyboxCreater().CreateSkybox(mData.sky);
        SetDirLight(mData.dir);
        SetBGMusic(mData.bgmusic);
        SetEnrMusic(mData.bgmusic);
        SetPVPBehaviour(mData.pvpData);
        SetCustomHP(mData.customHP);
        SpawnPointManager.Inst.SetSpawnPoint(mData);
        SetDefaultSpawn(mData.defaultSpawnId);
        SetEditSpawnPoint(mData.espawn);
        SetWeatherWithSaveParams(mData.weather);
        BuildAllNodes(mData.pref);
        DowntownTransferManager.Inst.CreateDefaultTransferPoint();
        var overParseJsonTime = Time.realtimeSinceStartup;
        LoggerUtils.Log("overParseJsonTime:" + (overParseJsonTime - startParseJsonTime));
        FPSPanel.Instance.SetParseJsonTime(overParseJsonTime - startParseJsonTime);
        LoggerUtils.Log("Initialize Start:" + Time.realtimeSinceStartup);
        LoggerUtils.Log("Initialize Over:" + Time.realtimeSinceStartup);
        SetCanFly(mData.canFly);
        SetHasHP(mData.setHP);
        SetHasBaggage(mData.setBaggage);
        SetDamageSources(mData.dmgSrcs);
        PostProcessCreater.SetData(PostProcessBehaviour,mData.postprocess);
        SceneBuilderUtils.Inst.ClearAll();
        UGCBehaviorManager.Inst.SetAllSoldOutList();
        PGCBehaviorManager.Inst.SetAllSoldOutList();
        MapRenderManager.Inst.Init(GlobalFieldController.CurMapInfo?.renderJson);
    }

    private void SetPVPBehaviour(PVPWaitAreaData pvpData)
    {
        if (pvpData == null)
        {
           return;
        }
        CreatePVPBehaviour();
        PVPWaitAreaCreater.SetData(PVPWaitAreaManager.Inst.PVPBehaviour, pvpData);
    }

    public void CreatePVPBehaviour()
    {
        if (PVPWaitAreaManager.Inst.PVPBehaviour == null)
        {
            PVPWaitAreaManager.Inst.PVPBehaviour = CreateSceneNode<PVPWaitAreaCreater, PVPWaitAreaBehaviour>();
            PVPWaitAreaManager.Inst.PVPBehaviour.transform.SetParent(SceneParent);
        }
    }

    public S CreateSceneNode<T, S>() where T : SceneEntityCreater, new() where S : NodeBaseBehaviour
    {
        return factory.BindCreater<T>().Create<S>();
    }

    public T BindCreater<T>() where T : SceneEntityCreater, new()
    {
        return factory.BindCreater<T>();
    }

    public NodeData CrateScenePgcNodeData(DCInfo dcInfo, DCPGCItemInfo dcPgcInfo, int isDc, Vector3 pos)
    {
        var nodeData = new NodeData()
        {
            rid = dcInfo.itemId,
            id = dcPgcInfo.pgcId,
            p = DataUtils.Vector3ToString(pos),
            r = DataUtils.Vector3ToString(Vector3.zero),
            s = DataUtils.Vector3ToString(Vector3.one),
            type = (int)ResType.PGC,
        };
        
        if (dcPgcInfo != null)
        {
            PGCSceneData pgcSceneData = new PGCSceneData()
            {
                classifyID = dcPgcInfo.classifyType,
                pgcID = dcPgcInfo.pgcId
            };
            
            nodeData.attr.Add(new BehaviorKV()
            {
                k = (int)BehaviorKey.PgcScene,
                v = JsonConvert.SerializeObject(pgcSceneData)
            });
        }
        
        if (isDc > 0 && dcInfo != null)
        {
            DcData dc = new DcData()
            {
                address = dcInfo.walletAddress,
                actId = dcInfo.budActId,
                id = dcInfo.itemId,
                isDc = 1
            };
            
            nodeData.attr.Add(new BehaviorKV()
            {
                k = (int)BehaviorKey.DC,
                v = JsonConvert.SerializeObject(dc)
            });
        }
        
        UGCPropData prod = new UGCPropData();
        prod.isTradable = 1;
        var propData = new BehaviorKV
        {
            k = (int)BehaviorKey.UGCProp,
            v = JsonConvert.SerializeObject(prod)
        };
        nodeData.attr.Add(propData);
        
        return nodeData;
    }

    public void CreateDCPGC(MapInfo mapInfo)
    {
        var pos = CameraUtils.Inst.GetCreatePosition();
        var nodeData = SceneBuilder.Inst.CrateScenePgcNodeData(mapInfo.dcInfo, mapInfo.dcPgcInfo, mapInfo.isDC, pos);
        ParsePropAndBuild(JsonConvert.SerializeObject(nodeData), pos);
    }
    
    public NodeBaseBehaviour ParsePropAndBuild(string content, Vector3 pos, string rid = null)
    {
        var nodeData = JsonConvert.DeserializeObject<NodeData>(content);
        if (!string.IsNullOrEmpty(rid))
        {
            nodeData.rid = rid;
        }
		nodeData.uid = 0;
        
        var nodeBehaviour = CreateSceneNodeByNodeData(nodeData, pos);
        SceneBuilderUtils.Inst.ClearAll();
        return nodeBehaviour;
    }

    public NodeBaseBehaviour ParseClothAndBuild(string content)
    {
        var mapInfo = JsonConvert.DeserializeObject<MapInfo>(content);
        UGCClothItemData clothData = new UGCClothItemData();
        clothData.tId = mapInfo.templateId;
        clothData.cUrl = mapInfo.clothesUrl;
        clothData.cMapId = mapInfo.mapId;
        clothData.isDc = mapInfo.isDC;
        clothData.dataSubType = mapInfo.dataSubType;
        if (mapInfo.dcPgcInfo != null)
        {
            clothData.classifyType = mapInfo.dcPgcInfo.classifyType;
            clothData.pgcId = mapInfo.dcPgcInfo.pgcId;
        }
        var clothbevKey = new BehaviorKV
        {
            k = (int)BehaviorKey.UGCClothItem,
            v = JsonConvert.SerializeObject(clothData)
        };
        NodeData nodeData = new NodeData{
            uid = 0,
            id = 1041,
            type = -1,
            p = "0.0000, 0.0000, 0.0000",
            r = "0.0000, 0.0000, 0.0000",
            s = "1.0000, 1.0000, 1.0000",
        };
        nodeData.attr.Add(clothbevKey);
        var nodeBehaviour = CreateSceneNodeByNodeData(nodeData, Vector3.zero);
        return nodeBehaviour;
    }

    public void BuildAllNodes(List<NodeData> nodes, Transform parent = null)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            CreateSceneNodeByNodeData(nodes[i], parent);
        }
    }
    public int Get3DTextCount()
    {
        int count = SceneSystem.Inst.FilterNodeBehavioursCount<DTextBehaviour>(SceneBuilder.Inst.allControllerBehaviours);
        int countNew = SceneSystem.Inst.FilterNodeBehavioursCount<NewDTextBehaviour>(SceneBuilder.Inst.allControllerBehaviours);
        return count + countNew;
    }
    private void CreateSceneNodeByNodeData(NodeData data,Transform parent = null)
    {
        var pos = DataUtils.DeSerializeVector3(data.p);
        CreateSceneNodeByNodeData(data,pos, parent);
    }

    private NodeBaseBehaviour CreateSceneNodeByNodeData(NodeData data, Vector3 pos, Transform parent = null)
    {
        
        var rType = (ResType)data.type;
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        NodeBaseBehaviour behaviour = null;
    
        switch (rType)
        {
            case ResType.Single:
                behaviour = CreateSingleNode(data, pos, rot, sca, parent);
                break;
            case ResType.CommonCombine:
                behaviour = CreateCombineNode<CombineBehaviour>(data, pos, rot, sca, ResType.CommonCombine, parent);
                if (GlobalFieldController.CurGameMode == GameMode.Edit)
                {
                    BuildAllNodes(data.prims, behaviour.transform);
                }
                else
                {
                    var assetObj = CombineUtils.DeSerializeNodeData(behaviour.transform, data, false);
                    assetObj.transform.localScale = Vector3.one;
                    assetObj.transform.localEulerAngles = Vector3.zero;
                    assetObj.transform.localPosition = Vector3.zero;
                }

                SeesawManager.Inst.AddSeeSawToCombine(behaviour,data);
                break;
            case ResType.UGC:
                behaviour = factory.BindCreater<UGCCombCreater>().Create(pos, parent);
                UGCCombCreater.SetData((UGCCombBehaviour) behaviour, data);
                SceneBuilderUtils.Inst.AddToUgcBehavPool(behaviour, data);
                DCManager.Inst.AddDCComponentToUGC((UGCCombBehaviour)behaviour, data);
                WeaponCreateUtils.Inst.AddWeaponComponentToUGC(behaviour, data);
                BloodPropCreateUtils.Inst.AddWeaponComponentToUGC(behaviour, data);
                FireworkManager.Inst.AddFireworkComponentToUGC(behaviour, data);
                ParachuteManager.Inst.AddComponentToUGC(behaviour, data);
                FreezePropsManager.Inst.AddComponentToUGC(behaviour, data);
                SeesawManager.Inst.AddSeeSawSeatToUGC(behaviour,data);
                SwingManager.Inst.AddSwingSeat(behaviour,data);
                FishingEditManager.Inst.AddComponentToUGC(behaviour, data);
                VIPZoneManager.Inst.AddComponentToUGC(behaviour, data);
                break;
            case ResType.PGC:
                behaviour = IsSpecialPGCNode(data) ? factory.BindCreater<PGCNodeCreater>().Create<PGCSpecialBehaviour>() : factory.BindCreater<PGCNodeCreater>().Create<PGCBehaviour>();
                PGCNodeCreater.SetData(behaviour, data, parent);
                DCManager.Inst.AddDCComponentToPGC((PGCBehaviour)behaviour, data);
                break;
            case ResType.Downtown:
                behaviour = factory.BindCreater<DowntownCreater>().Create<DowntownNodeBehaviour>();
                DowntownCreater.SetData(behaviour, data, parent);
                break;
        }
 
        AddRPAnimAttribute(behaviour, GetAttr<RPAnimData>((int)BehaviorKey.RPAnim, data.attr));
        AddFollowableAttrbute(behaviour, GetAttr<FollowableData>((int)BehaviorKey.FollowBox, data.attr));
        AddMovementAttribute(behaviour, GetAttr<MovementPropertyData>((int)BehaviorKey.Movement, data.attr));
        AddSwitchControlAttribute(behaviour, GetAttr<SwitchControlData>((int)BehaviorKey.SwitchControl, data.attr));
        AddCollectControlAttribute(behaviour, GetAttr<CollectControlData>((int)BehaviorKey.CollectControl, data.attr));
        AddSensorControlAttribute(behaviour, GetAttr<SensorControlData>((int)BehaviorKey.SensorControl, data.attr));
        AddSwitchAttribute(data, behaviour);
        AddPickablityAttribute(behaviour, data);
        AddEdibilityAttribute(behaviour, GetAttr<EdibilityData>((int)BehaviorKey.Edibility, data.attr));
        AddCatchabilityAttribute(behaviour, data);

        //加属性往此行之上加，不可在AddHLODAttribute之下加，否则可能会造成属性丢失
        AddHLODAttribute(behaviour, data);          
        
        MapRenderManager.Inst.AddBehaviour(behaviour);
        
        if (behaviour != null && !allControllerBehaviours.Contains(behaviour))
        {
            allControllerBehaviours.Add(behaviour);
        }
        return behaviour;
    }

    private NodeBaseBehaviour CreateSingleNode(NodeData data, Vector3 pos, Vector3 rot, Vector3 sca,
        Transform parent = null)
    {
        var configData = GameManager.Inst.priConfigData[data.id];
        NodeBaseBehaviour nBehaviour = null;
        switch ((NodeModelType) configData.modType)
        {
            case NodeModelType.BaseModel:
                nBehaviour = factory.BindCreater<BaseModelCreater>().Create<NodeBehaviour>();
                BaseModelCreater.SetData(nBehaviour, data, parent);
                break;
            case NodeModelType.PointLight:
                nBehaviour = CreatePrimitive<PointLightBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType) configData.modType, parent);
                hideEntityBevs.Add(nBehaviour);
                AddPointLightAttribute(nBehaviour, GetAttr<PointLightData>((int) BehaviorKey.PointLight, data.attr));
                break;
            case NodeModelType.SpotLight:
                nBehaviour = CreatePrimitive<SpotLightBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType) configData.modType, parent);
                hideEntityBevs.Add(nBehaviour);
                AddSpotLightAttribute(nBehaviour, GetAttr<SpotLightData>((int) BehaviorKey.SpotLight, data.attr));
                break;
            case NodeModelType.PortalButton:
                nBehaviour = CreatePrimitive<PortalButtonBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType) configData.modType, parent);
                AddPortalButtonAttribute(nBehaviour,
                    GetAttr<PortalPointData>((int) BehaviorKey.PortalPoint, data.attr));
                break;
            case NodeModelType.PortalPoint:
                nBehaviour = CreatePrimitive<PortalPointBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType) configData.modType, parent);
                AddPortalPointAttribute(nBehaviour, GetAttr<PortalPointData>((int) BehaviorKey.PortalPoint, data.attr));
                break;
            case NodeModelType.DText:
                nBehaviour = CreatePrimitive<DTextBehaviour>(data.uid, data.id, pos, rot, sca, (NodeModelType) configData.modType,
                    parent); 
                AddDTextAttribute(nBehaviour, GetAttr<DTextData>((int) BehaviorKey.DText, data.attr));
                break;
            case NodeModelType.PortalGate:
                nBehaviour = CreatePrimitive<PortalGateBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType) configData.modType, parent);
                AddPortalGateAttribute(nBehaviour, GetAttr<PortalGateData>((int) BehaviorKey.PortalGate, data.attr));
                break;
            case NodeModelType.Like:
                nBehaviour = CreatePrimitive<LikeButtonBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType) configData.modType, parent);
                likeEntityBevs.Add(nBehaviour);
                break;
            case NodeModelType.Attention:
                nBehaviour = CreatePrimitive<AttentionButtonBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType) configData.modType, parent);
                attentionEntityBevs.Add(nBehaviour);
                break;
            case NodeModelType.Favorite:
                nBehaviour = CreatePrimitive<FavoriteButtonBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType)configData.modType, parent);
                favoriteEntityBevs.Add(nBehaviour);
                break;
            case NodeModelType.Switch:
                nBehaviour = CreatePrimitive<SwitchButtonBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType)configData.modType, parent);
                AddSwitchButtonAttribute(nBehaviour, GetAttr<SwitchButtonData>((int)BehaviorKey.SwitchButton, data.attr));
                break;
            case NodeModelType.PropStar:
                nBehaviour = CreatePrimitive<PropStarBehaviour>(data.uid,  data.id, pos,  rot,  sca,  (NodeModelType)configData.modType, parent);
  
                break;
            case NodeModelType.TrapBox:
                nBehaviour = CreatePrimitive<TrapBoxBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType)configData.modType, parent);
                AddTrapBoxAttribute(nBehaviour, GetAttr<TrapBoxData>((int)BehaviorKey.TrapBox, data.attr));
                break;
            case NodeModelType.TrapSpawn:
                nBehaviour = CreatePrimitive<TrapSpawnBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType)configData.modType, parent);
                AddTrapSpawnAttribute(nBehaviour, GetAttr<TrapSpawnData>((int)BehaviorKey.TrapSpawn, data.attr));
                break;
            case NodeModelType.MusicBoard:
                nBehaviour = CreatePrimitive<MusicBoardBehaviour>(data.uid, data.id, pos, rot, sca, 
                    (NodeModelType)configData.modType, parent);
                AddMusicBoardAttribute(nBehaviour, GetAttr<MusicIDData>((int)BehaviorKey.MusicBoard, data.attr));
                break;
            case NodeModelType.DisplayBoard:
                nBehaviour = CreatePrimitive<DisplayBoardBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType)configData.modType, parent);
                AddDisplayBoardAttribute(nBehaviour, GetAttr<DisplayBoardData>((int)BehaviorKey.DisplayBoard, data.attr));
                break;
            case NodeModelType.MagneticBoard:
                nBehaviour = CreatePrimitive<MagneticBoardBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType)configData.modType, parent);
               
                break;
            case NodeModelType.Sound:
                nBehaviour = CreatePrimitive<SoundButtonBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType)configData.modType, parent);
                AddSoundButtonAttribute(nBehaviour, GetAttr<SoundButtonData>((int)BehaviorKey.Sound, data.attr));
                break;
            case NodeModelType.ShotPhoto:
                nBehaviour = factory.BindCreater<ShotPhotoCreater>().Create<ShotPhotoBehaviour>();
                ShotPhotoCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.SteeringWheel:
                nBehaviour = CreatePrimitive<SteeringWheelBehaviour>(data.uid, data.id, pos, rot, sca,
                    (NodeModelType)configData.modType, parent);
                SteeringWheelManager.Inst.AddCar(data.uid, nBehaviour.GetComponent<SteeringWheelBehaviour>());
                break;
            case NodeModelType.Video:
                nBehaviour = factory.BindCreater<VideoNodeCreater>().Create<VideoNodeBehaviour>();
                VideoNodeCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.SensorBox:
                nBehaviour = factory.BindCreater<SensorBoxCreater>().Create<SensorBoxBehaviour>();
                SensorBoxCreater.SetData(nBehaviour, data, pos, parent);
                break;

            case NodeModelType.WaterCube:
                nBehaviour = factory.BindCreater<WaterCubeCreater>().Create<WaterCubeBehaviour>();
                WaterCubeCreater.SetData(nBehaviour, data, pos, parent);
                break;

            case NodeModelType.AttackWeapon:
                nBehaviour = factory.BindCreater<AttackWeaponCreater>().Create<AttackWeaponDefaultBehaviour>();
                AttackWeaponCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.LeaderBoard:
                nBehaviour = factory.BindCreater<LeaderBoardCreater>().Create<LeaderBoardBehaviour>();
                LeaderBoardCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.UgcCloth:
                nBehaviour = factory.BindCreater<UgcClothItemCreater>().Create<UgcClothItemBehaviour>();
                UgcClothItemCreater.SetData(nBehaviour, data, pos, parent);
				break;
            case NodeModelType.ShootWeapon:
                nBehaviour = factory.BindCreater<ShootWeaponCreater>().Create<ShootWeaponDefaultBehaviour>();
                ShootWeaponCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.BloodRestore:
                nBehaviour = factory.BindCreater<BloodPropCreater>().Create<BloodPropBehaviour>();
                BloodPropCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.NewDText:
                nBehaviour = factory.BindCreater<NewDTextCreater>().Create<NewDTextBehaviour>();
                NewDTextCreater.SetData(nBehaviour as NewDTextBehaviour, data, pos, parent);
                break;
            case NodeModelType.IceCube:
                nBehaviour = factory.BindCreater<IceCubeCreater>().Create<IceCubeBehaviour>();
                IceCubeCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.Bounceplank:
                nBehaviour = factory.BindCreater<BounceplankCreater>().Create<BounceplankBehaviour>();
                BounceplankCreater.SetData(nBehaviour, data, pos, parent);
				break;
            case NodeModelType.Ladder:
                nBehaviour = factory.BindCreater<LadderCreater>().Create<LadderBehaviour>();
                LadderCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.Firework:
                nBehaviour = factory.BindCreater<FireworkCreater>().Create<FireworkBehaviour>();
                FireworkCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.PGCPlant:
                nBehaviour = factory.BindCreater<PGCPlantCreater>().Create<PGCPlantBehaviour>();
                PGCPlantCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.Parachute:
                nBehaviour = factory.BindCreater<ParachuteCreater>().Create<ParachuteBehaviour>();
                ParachuteCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.ParachuteBag:
                nBehaviour = factory.BindCreater<ParachuteBagCreater>().Create<ParachuteBagBehaviour>();
                ParachuteBagCreater.SetData(nBehaviour, data, pos, parent);
                break;    
            case NodeModelType.FreezeProps:
                nBehaviour = factory.BindCreater<FreezePropsCraeter>().Create<FreezePropsBehaviour>();
                FreezePropsCraeter.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.FireProp:
                nBehaviour = factory.BindCreater<FirePropCreator>().Create<FirePropBehaviour>();
                FirePropCreator.SetData(nBehaviour, data, pos, parent);
                FirePropManager.Inst.AddNode(nBehaviour);
                break;
            case NodeModelType.SnowCube:
                nBehaviour = factory.BindCreater<SnowCubeCreater>().Create<SnowCubeBehaviour>();
                SnowCubeCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.SeeSaw:
                nBehaviour = factory.BindCreater<SeeSawCreater>().Create<SeesawBehaviour>();
                SeeSawCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.SeeSawSeat:
                nBehaviour = factory.BindCreater<SeesawSeatCreater>().Create<SeesawSeatBehaviour>();
                SeesawSeatCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.FishingModel:
                nBehaviour = factory.BindCreater<FishingModelCreater>().Create<FishingBehaviour>();
                FishingModelCreater.SetData(nBehaviour, data, pos, parent);
                BuildAllNodes(data.prims, nBehaviour.transform);
                break;
            case NodeModelType.FishingRod:
                nBehaviour = factory.BindCreater<FishingRodCreator>().Create<FishingRodBehaviour>();
                FishingRodCreator.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.FishingHook:
                nBehaviour = factory.BindCreater<FishingHookCreator>().Create<FishingHookBehaviour>();
                FishingHookCreator.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.SlidePipe:
                nBehaviour = factory.BindCreater<SlidePipeCreater>().Create<SlidePipeBehaviour>();
                SlidePipeCreater.SetData(nBehaviour, data, pos, parent);
                if(data.prims!=null && data.prims.Count > 0)
                {
                    BuildAllNodes(data.prims, nBehaviour.transform);
                }
                break;
            case NodeModelType.SlideItem:
                nBehaviour = factory.BindCreater<SlideItemCreater>().Create<SlideItemBehaviour>();
                SlideItemCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.VIPZone:
                nBehaviour = factory.BindCreater<VIPZoneCreater>().Create<VIPZoneBehaviour>();
                VIPZoneCreater.SetData(nBehaviour, data, pos, parent);
                if(data.prims!=null && data.prims.Count > 0)
                {
                    BuildAllNodes(data.prims, nBehaviour.transform);
                }

                break;
            case NodeModelType.VIPArea:
                nBehaviour = factory.BindCreater<VIPAreaCreater>().Create<VIPAreaBehaviour>();
                VIPAreaCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.VIPDoor:
                nBehaviour = factory.BindCreater<VIPDoorCreater>().Create<VIPDoorBehaviour>();
                VIPDoorCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.VIPDoorWrap:
                nBehaviour = factory.BindCreater<VIPDoorWrapCreater>().Create<VIPDoorWrapBehaviour>();
                VIPDoorWrapCreater.SetData(nBehaviour,data,pos,parent);
                break;
            case NodeModelType.VIPCheck:
                nBehaviour = factory.BindCreater<VIPCheckCreater>().Create<VIPCheckBehaviour>();
                VIPCheckCreater.SetData(nBehaviour, data, pos, parent);
                nBehaviour.GetComponent<VIPCheckBoundControl>().UpdateEffectShow();
                break;
            case NodeModelType.VIPDoorEffect:
                nBehaviour = factory.BindCreater<VIPDoorEffectCreater>().Create<VIPDoorEffectBehaviour>();
                VIPDoorEffectCreater.SetData(nBehaviour, data, pos, parent);
                break;
            case NodeModelType.FlashLight:
                nBehaviour = factory.BindCreater<FlashLightCreator>().Create<FlashLightBehaviour>();
                FlashLightCreator.SetData(nBehaviour, data, pos, parent);
                FlashLightManager.Inst.AddNode(nBehaviour);
                break;
            case NodeModelType.Swing:
                nBehaviour = factory.BindCreater<SwingCreater>().Create<SwingBehaviour>();
                if(data.prims!=null && data.prims.Count > 0)
                {
                    BuildAllNodes(data.prims, nBehaviour.transform);
                }
                SwingCreater.SetData(nBehaviour, data, pos, parent);
			    break;
            case NodeModelType.DowntownTransfer:
                nBehaviour = factory.BindCreater<TransferCreater>().Create<TransferBehaviour>();
                TransferCreater.SetData(nBehaviour, data, parent);
                break;
            case NodeModelType.PGCEffect:
                nBehaviour = factory.BindCreater<PGCEffectCreater>().Create<PGCEffectBehaviour>();
                PGCEffectCreater.SetData(nBehaviour, data, pos, parent);
                var sBehav = nBehaviour as PGCEffectBehaviour;
                PGCEffectManager.Inst.AddItem(sBehav);
                break;
            case NodeModelType.CrystalStone:
                nBehaviour = factory.BindCreater<CrystalStoneCreater>().Create<CrystalStoneBehaviour>();
                CrystalStoneCreater.SetData(nBehaviour, data, pos, parent);
                CrystalStoneManager.Inst.AddCrystalNode(nBehaviour);
                break;
        }

        return nBehaviour;
    }

    private void AddSwitchAttribute(NodeData data, NodeBaseBehaviour nBehaviour)
    {
        //开关:只对有ShowHideComp的组件添加Data
        var kv = data.attr.Find(x => x.k == (int)BehaviorKey.ShowHide);
        if (kv != null)
        {
            var showHideData = JsonConvert.DeserializeObject<ShowHideData>(kv.v);
            AddShowHideAttribute(nBehaviour, showHideData);
        }

        var ctrlKV = data.attr.Find(x => x.k == (int)BehaviorKey.SwitchControl);
        if (ctrlKV != null)
        {
            var ctrlData = JsonConvert.DeserializeObject<SwitchControlData>(ctrlKV.v);
            // 同步开关控制属性
            AddSwitchControlAttribute(nBehaviour, ctrlData);
        }
    }

    private T GetAttr<T>(int key,List<BehaviorKV> behavs)
    {
        var kv = behavs.Find(x => x.k == key);
        if (kv != null)
        {
            return JsonConvert.DeserializeObject<T>(kv.v);
        }
        return default(T);
    }

    public NodeBaseBehaviour CreatePrimitive<T>(int uid, int id, Vector3 pos,Vector3 rot,Vector3 sca, NodeModelType modType, Transform parent = null) where T : NodeBaseBehaviour
    {
        NodeBaseBehaviour nodeBehaviour;
        CreateNewModel<T>(uid, id, pos, rot, sca, parent, out nodeBehaviour);
        allControllerBehaviours.Add(nodeBehaviour);
        return nodeBehaviour;
    }

    private void AddCollectControlAttribute(NodeBaseBehaviour nBehaviour, CollectControlData collectData)
    {

        if (collectData == null)
        { 
            return;   
        }

        // if (collectData.isControl != 1) return;
        var mComp = nBehaviour.entity.Get<CollectControlComponent>();
        mComp.isControl = collectData.isControl;
        mComp.moveActive = collectData.moveActive;
        mComp.playSound = collectData.playSound;
        mComp.animActive = collectData.animActive;
        mComp.playfirework = collectData.playfirework;
    }


    public void AddColorMatAttribute(NodeBaseBehaviour nBehaviour, ColorMatData matData)
    {
        var mComp = nBehaviour.entity.Get<MaterialComponent>();
        mComp.matId = matData.mat;
        mComp.tile = DataUtils.DeSerializeVector2(matData.tile);
        mComp.color = DataUtils.DeSerializeColor(matData.cols);
        SceneObjectController.SetBaseModelAtr(nBehaviour, mComp.matId, mComp.color);
        SceneObjectController.InitBaseModelTile(nBehaviour, mComp.tile);
    }


    public void AddPointLightAttribute(NodeBaseBehaviour nBehaviour, PointLightData data)
    {
        var pBehav = nBehaviour as PointLightBehaviour;
        var mComp = nBehaviour.entity.Get<PointLightComponent>();
        mComp.Intensity = data.inte;
        mComp.Range = data.rng;
        mComp.color = DataUtils.DeSerializeColor(data.lico);
    }

    public void SetEntityMeshsVisibleByMode(bool isVisible)
    {
        for (var i = 0; i < hideEntityBevs.Count; i++)
        {
            if (hideEntityBevs[i] == null)
            {
                continue;
            }
            var entity = hideEntityBevs[i].entity;
            var modelType = entity.Get<GameObjectComponent>().modelType;
            switch (modelType)
            {
                case NodeModelType.PointLight:
                    var pbehv = hideEntityBevs[i] as PointLightBehaviour;
                    pbehv.SetMeshVisibel(isVisible);
                    break;
                case NodeModelType.SpotLight:
                    var sbehv = hideEntityBevs[i] as SpotLightBehaviour;
                    sbehv.SetMeshVisibel(isVisible);
                    break;
            }
        }
    }

    public void AddShowHideAttribute(NodeBaseBehaviour nBehaviour, ShowHideData data)
    {
        if (data != null)
        {
            var mComp = nBehaviour.entity.Get<ShowHideComponent>();
            mComp.defaultShow = data.show;
            mComp.switchUids = data.switchs;

            ShowHideManager.Inst.AddShowHideEntityToDict(nBehaviour.entity);
        }
    }

    private void AddPickablityAttribute(NodeBaseBehaviour nBehaviour, NodeData data)
    {
        if(data != null)
        {
            var kv = data.attr.Find(x => x.k == (int)BehaviorKey.Pickablity);
            if (kv != null)
            {
                var v = JsonConvert.DeserializeObject<PickableData>(kv.v);
                if(v.cp == (int)PickableState.Pickable)
                {
                    PickabilityManager.Inst.AddPickablityProp(nBehaviour.entity, new Vector3(v.x,v.y,v.z));
                }
            }
        }
    }

    private void AddEdibilityAttribute(NodeBaseBehaviour nBehaviour, EdibilityData data)
    {
        if (data != null)
        {
            var mode = (EdibilityMode)data.mode;
            if (mode != EdibilityMode.None)
            {
                EdibilityManager.Inst.AddEdibilityProp(nBehaviour.entity);
                EdibilityManager.Inst.SetEdibilityMode(nBehaviour.entity, mode);
            }
        }
    }

    private void AddCatchabilityAttribute(NodeBaseBehaviour nBehaviour, NodeData data)
    {
        if (data != null)
        {
            var kv = data.attr.Find(x => x.k == (int)BehaviorKey.Catchability);
            if (kv != null)
            {
                var entity = nBehaviour.entity;
                FishingManager.Inst.AddCatchability(entity);
            }
        }
    }

    private void AddHLODAttribute(NodeBaseBehaviour nBehaviour, NodeData data) {
        if (data != null && nBehaviour is BaseHLODBehaviour) {
            var entity = nBehaviour.entity;
            if(GlobalFieldController.CurGameMode == GameMode.Guest)
            {
                var mComp = entity.Get<HLODComponent>();
                mComp.hlodId = $"{data.uid}_{data.ToHash()}";
            }
            HLOD.Inst.AddEntity(entity, nBehaviour);
        }
        
    }

    public void AddAttackWeaponAttribute(NodeBaseBehaviour nBehaviour, AttackWeaponNodeData data)
    {
        var dComp = nBehaviour.entity.Get<AttackWeaponComponent>();
        dComp.rId = data.rId;
    }

    public void AddSwitchButtonAttribute(NodeBaseBehaviour nBehaviour, SwitchButtonData data)
    {
        var mComp = nBehaviour.entity.Get<SwitchButtonComponent>();
        mComp.switchId = data.sid;
        mComp.controllUids = data.controlls;
        mComp.moveControllUids = data.moveControlls;
        mComp.soundControllUids = data.soundControlls;
        mComp.animControllUids = data.animControlls;
        mComp.fireworkControllUids = data.fireworkControlls;
        SwitchButtonBehaviour b = nBehaviour as SwitchButtonBehaviour;
        b.ShowIndexNum();
        b.isWork = false;
        SwitchManager.Inst.UpdateMaxSwitchId(mComp.switchId);
        SwitchManager.Inst.AddSwtich(nBehaviour);
    }

    public void AddSpotLightAttribute(NodeBaseBehaviour nBehaviour, SpotLightData data)
    {
        var pBehav = nBehaviour as SpotLightBehaviour;
        var mComp = nBehaviour.entity.Get<SpotLightComponent>();
        mComp.Intensity = data.inte;
        mComp.Range = data.rng;
        mComp.SpotAngle = data.spoa;
        mComp.color = DataUtils.DeSerializeColor(data.lico);

    }

    public void AddDTextAttribute(NodeBaseBehaviour nBehaviour, DTextData data)
    {
        var dBehav = nBehaviour as DTextBehaviour;
        var dComp = dBehav.entity.Get<DTextComponent>();
        dComp.content = data.tex;
        dComp.col = DataUtils.DeSerializeColor(data.textcol);
    }

    public void AddPortalGateAttribute(NodeBaseBehaviour nBehaviour, PortalGateData data)
    {
        var dBehav = nBehaviour as PortalGateBehaviour;
        var dComp = dBehav.entity.Get<PortalGateComponent>();
        dComp.diyMapId = data.mapId;
        dComp.mapName = data.mapName;
        dComp.pngUrl = data.pngUrl;

    }

    public void AddDisplayBoardAttribute(NodeBaseBehaviour nBehaviour, DisplayBoardData data)
    {
        var dBehav = nBehaviour as DisplayBoardBehaviour;
        var dComp = dBehav.entity.Get<DisplayBoardComponent>();
        if (!string.IsNullOrEmpty(data.userId))
        {
            dComp.userId = data.userId;
            dComp.userName = data.userName;
            dComp.headUrl = data.headUrl;
            //为了批量请求展板上的用户信息，仅添加有用户信息的展板
            DisplayBoardManager.Inst.AddDisplayBev(data.userId, dBehav);
        }

    }
    public void AddSoundButtonAttribute(NodeBaseBehaviour nBehaviour, SoundButtonData data)
    {
        var sBehav = nBehaviour as SoundButtonBehaviour;
        var mComp = sBehav.entity.Get<SoundComponent>();
        mComp.soundName = data.sName;
        mComp.soundUrl = data.sUrl;
        mComp.musicType = data.musicType;
        mComp.isControl = data.isControl;
        SoundManager.Inst.AddSound(nBehaviour);
        sBehav.LoadOuterMusic();
        sBehav.RefreshButtonCanTouch(mComp.isControl == (int)SoundControl.NOT_SUPPORT);
    }

    public void AddRPAnimAttribute(NodeBaseBehaviour nBehaviour, RPAnimData data)
    {
        if (data != null)
        {
            var dComp = nBehaviour.entity.Get<RPAnimComponent>();
            dComp.rSpeed = data.rsd;
            dComp.uSpeed = data.usd;
            dComp.rAxis = data.rax;
            dComp.animState = data.aState;
            dComp.tempAnimState = data.aState;
        }
    }

    public void AddMovementAttribute(NodeBaseBehaviour nBehaviour, MovementPropertyData data)
    {
        if (data != null)
        {
            var dComp = nBehaviour.entity.Get<MovementComponent>();
            dComp.speedId = data.sd;
            dComp.turnAround = data.ta;
            dComp.pathPoints = new List<Vector3>();
            dComp.moveState = data.moveState;

            dComp.tempMoveState = dComp.moveState;

            if (data.points != null)
            {
                data.points.ForEach(x=> dComp.pathPoints.Add(DataUtils.DeSerializeVector3(x)));
            }
        }
    }

    public void AddFollowableAttrbute(NodeBaseBehaviour nBehaviour, FollowableData data)
    {
        if (data != null)
        {
            var dComp = nBehaviour.entity.Get<FollowableComponent>();
            dComp.size = data.size;
            var gComp = nBehaviour.entity.Get<GameObjectComponent>();
            if(gComp.uid == 0)
            {
                return;
            }
            if(dComp.moveType == (int)MoveMode.Follow)
            {
                FollowModeManager.Inst.BuildFollowBox(nBehaviour);
            }
        }
    }

    public void AddSwitchControlAttribute(NodeBaseBehaviour nBehaviour, SwitchControlData data)
    {
        if (data != null)
        {
            var dComp = nBehaviour.entity.Get<SwitchControlComponent>();
            dComp.switchUids = data.switchs;
            dComp.switchControlType = data.switchCtrlType;
            dComp.switchSoundUids = data.soundUids;
            dComp.switchAnimUids = data.animUids;
            dComp.controlPlaySound = data.playSound;

            SwitchControlManager.Inst.AddSwitchControlEntityToDict(nBehaviour.entity);
        }
    }

    public void AddSensorControlAttribute(NodeBaseBehaviour nBehaviour, SensorControlData data)
    {
        if (data != null)
        {
            var comp = nBehaviour.entity.Get<SensorControlComponent>();
            comp.visibleSensorUids = data.visUids;
            comp.moveSensorUids = data.moveUids;
            comp.soundSensorUids = data.soundUids;
            comp.animSensorUids = data.animUids;
            comp.fireworkSensorUids = data.fireworkUids;

            LoggerUtils.Log("####AddSensorControl  Attributecomp.visibleSensorUids:"+comp.visibleSensorUids.Count);
            if(comp.visibleSensorUids!=null && comp.visibleSensorUids.Count > 0)
            {
                SensorBoxManager.Inst.AddVisibleCtrEntity(nBehaviour.entity);
            }

            if(comp.moveSensorUids!=null && comp.moveSensorUids.Count > 0)
            {
                SensorBoxManager.Inst.AddMoveCtrEntity(nBehaviour.entity);
            }

            if(comp.soundSensorUids!=null && comp.soundSensorUids.Count > 0)
            {
                SensorBoxManager.Inst.AddSoundCtrEntity(nBehaviour.entity);
            }

            if (comp.animSensorUids != null && comp.animSensorUids.Count > 0)
            {
                SensorBoxManager.Inst.AddAnimCtrEntity(nBehaviour.entity);
            }

            if (comp.fireworkSensorUids != null && comp.fireworkSensorUids.Count > 0)
            {
                SensorBoxManager.Inst.AddFireworkCtrEntity(nBehaviour.entity);
            }
        }
    }

    public void AddPortalPointAttribute(NodeBaseBehaviour nBehaviour, PortalPointData data)
    {
        var pBehav = nBehaviour as PortalPointBehaviour;
        var mComp = nBehaviour.entity.Get<PortalPointComponent>();
        pBehav.pid = data.id;
        mComp.pid = data.id;
        pBehav.RefreshPointId();
        PortalPointManager.Inst.AddPortalPoint(data.id, pBehav.entity);
    }

    public void AddPortalButtonAttribute(NodeBaseBehaviour nBehaviour, PortalPointData data)
    {
        var pBehav = nBehaviour as PortalButtonBehaviour;
        var mComp = nBehaviour.entity.Get<PortalButtonComponent>();
        pBehav.pid = data.id;
        mComp.pid = data.id;
        pBehav.RefreshButtonId();
        PortalPointManager.Inst.AddPortalButton(data.id, pBehav.entity);
    }

    public void AddTrapBoxAttribute(NodeBaseBehaviour nBehaviour, TrapBoxData data)
    {
        var tBehav = nBehaviour as TrapBoxBehaviour;
        var tComp = nBehaviour.entity.Get<TrapBoxComponent>();
        tComp.tId = data.id == 0 ? TrapSpawnManager.Inst.GetNextId() : data.id;
        tComp.rePos = data.rPos;
        tComp.reTex = data.rTex;
        tComp.text = string.IsNullOrEmpty(data.tex) ? "" : data.tex;
        tComp.hitState = data.hitS;
        tBehav.RefreshShowId();
        tBehav.SetBoxVisiable(true);
        TrapSpawnManager.Inst.AddTrapBox(tComp.tId, nBehaviour.entity);
    }

    public void AddTrapSpawnAttribute(NodeBaseBehaviour nBehaviour, TrapSpawnData data)
    {
        var tBehav = nBehaviour as TrapSpawnBehaviour;
        var tComp = nBehaviour.entity.Get<TrapSpawnComponent>();
        tComp.tId = data.id;
        tBehav.RefreshPointId();
        TrapSpawnManager.Inst.AddTrapSpawn(data.id, nBehaviour.entity);
    }

    public void AddMusicBoardAttribute(NodeBaseBehaviour nBehaviour, MusicIDData data)
    {
        var behav = nBehaviour as MusicBoardBehaviour;
        var comp = nBehaviour.entity.Get<MusicBoardComponent>();
        comp.audioIDs[0] = data.lID;
        comp.audioIDs[1] = data.mID;
        comp.audioIDs[2] = data.rID;
        behav.SetColorInit();
    }
    
    private SceneEntity CreateNewModel<T>(int uid,int id,Vector3 pos,Vector3 rot,Vector3 sca,Transform parent,out NodeBaseBehaviour nodeBehaviour) where T:NodeBaseBehaviour
    {
        var entity = ecsWorld.NewEntity();
        var modelData = GameManager.Inst.priConfigData[id];
        GameObject assetGo = null;
        if(!typeof(BaseHLODBehaviour).IsAssignableFrom(typeof(T)))
        {
            assetGo = ModelCachePool.Inst.Get(id);
        }
        else
        {
            assetGo = new GameObject(Enum.GetName(typeof(NodeModelType),(NodeModelType)modelData.modType));
        } 
        var newParent = parent?? StageParent;
        assetGo.transform.SetParent(newParent);
        assetGo.transform.localPosition = pos;
        assetGo.transform.localEulerAngles = rot;
        assetGo.transform.localScale = sca;
        if (!assetGo.TryGetComponent(out nodeBehaviour))
        {
            nodeBehaviour = assetGo.AddComponent<T>();
        }
        nodeBehaviour.OnInitByCreate();
        nodeBehaviour.entity = entity;
        var tmpUid = UidManager.Inst.GetUid(uid);
        if(nodeBehaviour is BaseHLODBehaviour hLODBehaviour)
        {
            hLODBehaviour.data = new NodeData{
                uid = tmpUid,
                id = id,
                r = DataUtils.Vector3ToString(rot),
                p = DataUtils.Vector3ToString(pos),
                s = DataUtils.Vector3ToString(sca),
                type = (int)ResType.Single
            };
        }
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.uid = tmpUid;
        gameComponent.bindGo = assetGo;
        gameComponent.modId = id;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = (NodeHandleType)modelData.handleType;
        gameComponent.modelType = (NodeModelType)modelData.modType;

#if UNITY_EDITOR
        assetGo.name = $"{assetGo.name}_{gameComponent.uid}";
#endif

        return entity;
    }

    private DirLightData SetDirLightInitDataByEmpty(int skyId)
    {
        DirLightData data = default;
        var lData = GameConsts.settings[skyId];
        data.anx = lData.anglex;
        data.any = lData.angley;
        data.inte = lData.intensity;
        data.lico = DataUtils.ColorToString(lData.dirctional);
        return data;
    }
    

    private void CreateTerrain()
    {
        var entity = ecsWorld.NewEntityNoRecord();
        TerrainGo = ModelCachePool.Inst.Get((int)GameResType.Ground);
        TerrainGo.transform.SetParent(SceneParent);
        TerrainGo.transform.position = defaultTerrainPos;
        if (!TerrainGo.TryGetComponent(out TerrainBehaviour nBehaviour))
        {
            nBehaviour = TerrainGo.AddComponent<TerrainBehaviour>();
        }
        nBehaviour.entity = entity;
        nBehaviour.OnInitByCreate();
        var gameComp = entity.Get<GameObjectComponent>();
        gameComp.bindGo = TerrainGo;
        gameComp.modId = (int)GameResType.Ground;
        TerrainEntity = entity;
    }

    private void CreateBGMusic()
    {
        BGMusicEntity = ecsWorld.NewEntityNoRecord();
        var entityGo = ModelCachePool.Inst.Get((int)GameResType.BGMusic);
        entityGo.transform.SetParent(SceneParent);
        if (!entityGo.TryGetComponent<AudioSource>(out AudioSource source))
        {
            var aSource = entityGo.AddComponent<AudioSource>();
            aSource.playOnAwake = false;
            aSource.volume = 0.5f;
        }

        if (!entityGo.TryGetComponent<BGMusicBehaviour>(out BgBehaviour))
        {
            BgBehaviour = entityGo.AddComponent<BGMusicBehaviour>();
        }
        var gameComp = BGMusicEntity.Get<GameObjectComponent>();
        gameComp.bindGo = entityGo;
        gameComp.modId = (int)GameResType.BGMusic;
        BgBehaviour.entity = BGMusicEntity;
        BgBehaviour.OnInitByCreate();
    }
    
    private void SetTerrain(GameTerrainData data,Color color, int size)
    {
        var com = TerrainEntity.Get<TerrainComponent>();
        com.matId = data.matId;
        com.umatUrl = data.uurl;
        com.umapId = data.umat;
        com.color = color;
        //GameManager.Inst.terrainSize = (TerrainSizeConfigs)size;
        if (size == 0) size = (int)TerrainSizeConfigs.defaultTerrainSize;
        GlobalFieldController.terrainSize = (TerrainSizeConfigs)size;
        TerrainEntity.Get<TerrainComponent>().terrainSize = size;
        var nBehaviour = TerrainGo.GetComponent<TerrainBehaviour>();
        if (string.IsNullOrEmpty(com.umatUrl) && string.IsNullOrEmpty(com.umapId))
        {
            var mat = ResManager.Inst.LoadRes<Material>(GameConsts.TerrainMatPath + "Ground_" + com.matId);
            nBehaviour.SetMat(mat);
        }
        else
        {
            UGCTexManager.Inst.GetUGCTex(com.umatUrl, (tex) => {
                nBehaviour.SetUGCMat(tex);
            });
        }
        
        nBehaviour.SetColor(color);
        nBehaviour.ExpandTextureScale(size);
        TerrainGo.transform.localScale = new Vector3(1f*size, 1f, 1f*size);
    }

    private void SetDefaultBGMusic()
    {
        BGMusicData data = new BGMusicData();
        data.musicType = 2;
        data.musicId = BGMConfig.tempBgmDict[0];
        data.eId = 2001;
        SetBGMusic(data);
        SetEnrMusic(data);
    }

    private void SetBGMusic(BGMusicData data)
    {
        BGMusicEntity.Get<BGMusicComponent>().bgName = data.bgName;
        BGMusicEntity.Get<BGMusicComponent>().bgUrl = data.bgUrl;
        BGMusicEntity.Get<BGMusicComponent>().musicId = data.musicId;
        BGMusicEntity.Get<BGMusicComponent>().musicType = data.musicType;
        var bindGo = BGMusicEntity.Get<GameObjectComponent>().bindGo;
        var behev = bindGo.GetComponent<BGMusicBehaviour>();
        if(data.musicType == 0)
        {
            behev.LoadOuterMusic();
        }
    }

    public void SetEnrMusic(BGMusicData data)
    {
        BGMusicEntity.Get<BGEnrMusicComponent>().enrMusicId = data.eId;
    }

    public NodeBaseBehaviour CreateTrapSpawn(SceneEntity bEntity)
    {
        var tComp = bEntity.Get<TrapBoxComponent>();
        int rePos = tComp.rePos;
        LoggerUtils.Log("CreateTrapSpawn rePos:"+rePos);
        if (rePos == 0)
            return null;

        TrapSpawnData point_Data = new TrapSpawnData();
        point_Data.id = tComp.tId;
        var gComp = bEntity.Get<GameObjectComponent>();
        var trap_Pos = gComp.bindGo.transform.position;
        var point_Pos = new Vector3(trap_Pos.x + 1f, trap_Pos.y, trap_Pos.z);
        var point_Behav = CreatePrimitive<TrapSpawnBehaviour>(0, (int)GameResType.TrapSpawn, point_Pos, Vector3.zero, Vector3.one, NodeModelType.TrapSpawn);
        if (point_Behav != null)
        {
            tComp.pId = point_Behav.entity.Get<GameObjectComponent>().uid;
            AddTrapSpawnAttribute(point_Behav, point_Data);
        }
        return point_Behav;
    }

    public NodeBaseBehaviour CreateTrapSpawn(SceneEntity bEntity, Vector3 pos)
    {
        var tComp = bEntity.Get<TrapBoxComponent>();
        int rePos = tComp.rePos;
        if (rePos == 0)
            return null;

        TrapSpawnData point_Data = new TrapSpawnData();
        point_Data.id = tComp.tId;
        var point_Behav = CreatePrimitive<TrapSpawnBehaviour>(0, (int)GameResType.TrapSpawn, pos, Vector3.zero, Vector3.one, NodeModelType.TrapSpawn);
        if (point_Behav != null)
        {
            tComp.pId = point_Behav.entity.Get<GameObjectComponent>().uid;
            AddTrapSpawnAttribute(point_Behav, point_Data);
        }
        return point_Behav;
    }

    public FollowModeBehaviour CreateFollowBox(NodeBaseBehaviour target)
    {
        var nBehaviour = factory.BindCreater<FollowBoxCreater>().Create<FollowModeBehaviour>();
        FollowBoxCreater.SetData(nBehaviour, target);
        return nBehaviour;
    }

    private void SetEditSpawnPoint(SpawnData espawn)
    {
        if(espawn == null)return;
        EditSpawn = new SpawnData();
        EditSpawn.p = espawn.p;
        EditSpawn.r = espawn.r;
        Vector3 spawnPos = DataUtils.DeSerializeVector3(espawn.p);
        Vector3 spawnRot = DataUtils.DeSerializeVector3(espawn.r);
        var editCamera = GameObject.Find("EditCamera");
        if(editCamera != null){
            editCamera.transform.position = spawnPos;
            editCamera.transform.eulerAngles = spawnRot;
        }
        
    }

    /// <summary>
    /// 更新出生点的分队下标
    /// </summary>
    /// <param name="teamList">分队信息</param>
    public void UpdateBronPointTeamIDState(List<List<int>> teamList)
    {
        var spawnList = SpawnPointManager.Inst.spawnList;
        if (PVPTeamManager.Inst.IsTeamMode())
        {
            var texPath = "Texture/BornPoint/team_";
            for (int teamid = 0; teamid < teamList.Count; teamid++)
            {
                for (int i = 0; i < spawnList.Count; i++)
                {
                    Transform TeamIdGo = spawnList[i].transform.Find("TeamId");
                    if (teamList[teamid].Contains(i + 1))
                    {
                        TeamIdGo.gameObject.SetActive(true);
                        var teamTex = ResManager.Inst.LoadRes<Texture2D>(texPath + teamid);
                        var mat = TeamIdGo.GetComponent<MeshRenderer>().material;
                        mat.SetTexture("_main", teamTex);
                    }
                }
            }
        }
        else {
            for (int i = 0; i < spawnList.Count; i++)
            {
                var teamIcon = spawnList[i].transform.Find("TeamId").gameObject;
                teamIcon.SetActive(false);
            }
        }
    }

    public void UpdateAllLikeButton(int state)
    {
        for (int i = 0; i < likeEntityBevs.Count; i++)
        {
            LikeButtonBehaviour bev = (LikeButtonBehaviour)likeEntityBevs[i];
            if (bev != null)
            {
                bev.SetSelectState(state);
            }
        }
    }
    public void UpdateAllFavoriteButton(int state)
    {
        for (int i = 0; i < favoriteEntityBevs.Count; i++)
        {
            FavoriteButtonBehaviour bev = (FavoriteButtonBehaviour)favoriteEntityBevs[i];
            if (bev != null)
            {
                bev.SetSelectState(state);
            }
        }
    }
    public void UpdateAllAttentionButton(int state)
    {
        for (int i = 0; i < attentionEntityBevs.Count; i++)
        {
            AttentionButtonBehaviour bev = (AttentionButtonBehaviour)attentionEntityBevs[i];
            if (bev != null)
            {
                bev.SetSelectState(state);
            }
        }
    }

    public NodeBaseBehaviour CreateCombineNode<T>(NodeData data, Vector3 pos, Vector3 rot, Vector3 sca, ResType resType,
        Transform parent = null) where T : CombineBehaviour
    {
        var poolResType = resType == ResType.UGC ? GameResType.UGCComb : GameResType.CombEmpty;
        var cNode = ModelCachePool.Inst.Get((int)poolResType);
        var newParent = parent ?? StageParent;
        cNode.transform.SetParent(newParent);
        cNode.transform.localPosition = pos;
        cNode.transform.localEulerAngles = rot;
        cNode.transform.localScale = sca;
        var entity = ecsWorld.NewEntity();
        if (!cNode.TryGetComponent(out T behav))
        {
            behav = cNode.AddComponent<T>();
        }
        behav.entity = entity;
        behav.OnInitByCreate();
        behav.data = data;
        var gameComp = entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        gameComp.bindGo = cNode;
        gameComp.modId = (int)poolResType;
        gameComp.type = resType;
        gameComp.modelType = NodeModelType.CommonCombine;
        gameComp.handleType = NodeHandleType.Combine;
#if UNITY_EDITOR
        cNode.name = $"{cNode.name}_combine_{gameComp.uid}";
#endif
        return behav;
    }

    public NodeBaseBehaviour CreatePGCNode(NodeData data, Vector3 pos, Vector3 rot, Vector3 sca, Transform parent = null)
    {
        NodeBaseBehaviour nodeBehaviour;
        if (IsSpecialPGCNode(data))
        {
            nodeBehaviour = CreatePrimitive<PGCSpecialBehaviour>(data.uid, data.id, pos, rot, sca, NodeModelType.PGC, parent);
        }
        else
        {
            nodeBehaviour = CreatePrimitive<PGCBehaviour>(data.uid, data.id, pos, rot, sca, NodeModelType.PGC, parent);
        }
        GameObjectComponent gameComp = nodeBehaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        gameComp.modId = data.id;
        gameComp.type = ResType.PGC;
        gameComp.modelType = NodeModelType.PGC;
        gameComp.handleType = NodeHandleType.PGC;
        
#if UNITY_EDITOR
        nodeBehaviour.name = $"{nodeBehaviour.name}_pgc_{gameComp.uid}";
#endif
        return nodeBehaviour;
    }

    public NodeBaseBehaviour CreateMovementPoint(Vector3 pos,GameObject prefab,Transform parent)
    {
        var go = GameObject.Instantiate(prefab, parent);
        go.transform.position = pos;
        var entity = ecsWorld.NewEntityNoRecord();
        go.AddComponent<SpawnPointConstrainer>();
        var behav = go.AddComponent<NodeBaseBehaviour>();
        behav.entity = entity;
        behav.OnInitByCreate();
        var gameComp = entity.Get<GameObjectComponent>();
        gameComp.bindGo = go;
        gameComp.modId = (int)GameResType.EditMovePoint;
        gameComp.type = ResType.Special;
        gameComp.modelType = NodeModelType.Movement;
        gameComp.handleType = NodeHandleType.NudeMod;
        return behav;
    }

    public SceneEntity CombineNode(List<SceneEntity> entitys,Transform par = null)
    {
        List<SceneEntity> packEntitys = new List<SceneEntity>();
        List<SceneEntity> destoryEntitys = new List<SceneEntity>();
        for (var i = 0; i < entitys.Count; i++)
        {
            FollowModeManager.Inst.OnCombineNode(entitys[i]);
            entitys[i].Remove<RPAnimComponent>();
            entitys[i].Remove<MovementComponent>();
            entitys[i].Remove<CollectControlComponent>();
            entitys[i].Remove<FollowableComponent>();

            ShowHideManager.Inst.OnCombineNode(entitys[i]);
            SwitchControlManager.Inst.OnCombineNode(entitys[i]);
            SwitchManager.Inst.OnCombineNode(entitys[i]);
            SensorBoxManager.Inst.OnCombineNode(entitys[i]);
            LockHideManager.Inst.RefreshLockList(entitys[i].Get<GameObjectComponent>().uid, false);
            PickabilityManager.Inst.OnCombineNode(entitys[i]);
            EdibilityManager.Inst.OnCombineNode(entitys[i]);
            FishingManager.Inst.OnCombineNode(entitys[i]);

            var gComp = entitys[i].Get<GameObjectComponent>();
            switch ((ResType)gComp.type)
            {
                case ResType.Single:
                    packEntitys.Add(entitys[i]);
                    break;
                case ResType.UGC:
                case ResType.PGC:
                    packEntitys.Add(entitys[i]);
                    break;
                //TODO:project only two level
                case ResType.CommonCombine: 
                    var combineTrans = entitys[i].Get<GameObjectComponent>().bindGo.transform;
                    for (int j = 0; j < combineTrans.childCount; j++)
                    {
                        var nodeBehav = combineTrans.GetChild(j).GetComponent<NodeBaseBehaviour>();
                        if (nodeBehav != null)
                        {
                            packEntitys.Add(nodeBehav.entity);
                        }
                    }
                    destoryEntitys.Add(entitys[i]);
                    break;
            }
        }

        var centerPos = DataUtils.GetCenterPoint(packEntitys);
        NodeData data = new NodeData();
        data.uid = UidManager.Inst.GetUid();
        var conBehav = CreateCombineNode<CombineBehaviour>(data, centerPos, Vector3.zero, Vector3.one, ResType.CommonCombine, par);
        allControllerBehaviours.Add(conBehav);
        var comParent = conBehav.transform;
        packEntitys.ForEach(x =>
        {
            var entityGo = x.Get<GameObjectComponent>().bindGo;
            entityGo.transform.SetParent(comParent);
            entityGo.transform.localScale = DataUtils.LimitVector3(entityGo.transform.localScale);
        });
        destoryEntitys.ForEach(x =>
        {
            // DestroyEntity(x);
            var comp = x.Get<GameObjectComponent>();
            SecondCachePool.Inst.DestroyEntity(comp.bindGo);
        });
        SteeringWheelManager.Inst.CombineCar(conBehav.entity);
        return conBehav.entity;
    }


    private void SetCanFly(int canfly)
    {
        CanFlyEntity = ecsWorld.NewEntityNoRecord();
        CanFlyEntity.Get<CanFlyComponent>().canFly = canfly;
    }

    private void SetHasHP(int hasHP)
    {
        if(HPEntity == null)
        {
            HPEntity = ecsWorld.NewEntityNoRecord();
        }
        HPEntity.Get<HPControlComponent>().setHP = hasHP;
    }

    private void SetHasBaggage(int hasBaggage)
    {
        BaggageEntity = ecsWorld.NewEntityNoRecord();
        BaggageEntity.Get<BaggageComponent>().openBaggage = hasBaggage;
	}
    private void SetDamageSources(List<int> dmgScrs)
    {
        if(HPEntity == null)
        {
            HPEntity = ecsWorld.NewEntityNoRecord();
        }
        if(dmgScrs != null)
        {
            HPEntity.Get<HPControlComponent>().dmgSrcs = dmgScrs;
        }
    }

    private void SetCustomHP(int customHP)
    {
        if (HPEntity == null)
        {
            HPEntity = ecsWorld.NewEntityNoRecord();
        }
        int hpValue = customHP > 0 ? customHP : 100;
        HPEntity.Get<HPControlComponent>().customHP = hpValue;
    }

    private void SetDefaultSpawn(int defaultSpawnId)
    {
        defaultSpawnId = defaultSpawnId == 0 ? 1 : defaultSpawnId;
        SpawnPointManager.Inst.defaultSpawnId = defaultSpawnId;
    }

    #region Skybox

    private SkyboxCreater GetSkyboxCreater()
    {
        return factory.BindCreater<SkyboxCreater>();
    }

    private void CreateGameTimeManager()
    {
        if (GameObject.Find("GameTimeManager") == null)
        {
            var gtmGo = new GameObject("GameTimeManager");
            gtmGo.AddComponent<GameTimeManager>();
        }
    }

    private void CreateSkyboxBev()
    {
        var skyboxGo = GameObject.Find("Skybox");
        if (skyboxGo == null)
        {
            skyboxGo = new GameObject("Skybox");
        }
        
        var entity = ecsWorld.NewEntityNoRecord();
        SkyboxBev = skyboxGo.AddComponent<SkyboxBehaviour>();
        SkyboxBev.isCanClick = false;
        entity.Get<GameObjectComponent>().bindGo = SkyboxBev.gameObject;
        SkyboxBev.entity = entity;
        SkyboxBev.OnInitByCreate();
    }

    #endregion

    #region Dir Light

    private void CreateDirLight()
    {
        var dirGo = GameObject.Find("DirLight");
        var entity = ecsWorld.NewEntityNoRecord();
        DirLight = dirGo.AddComponent<DirLightBehaviour>();
        DirLight.OnInitByCreate();
        entity.Get<GameObjectComponent>().bindGo = DirLight.gameObject;
        DirLight.entity = entity;
    }

    private void SetDirLight(DirLightData data)
    {
        var dComp = DirLight.entity.Get<DirLightComponent>();
        dComp.anglex = data.anx;
        dComp.angley = data.any;
        dComp.intensity = data.inte;
        dComp.color = DataUtils.DeSerializeColor(data.lico);
        DirLight.SetIntensity(data.inte);
        DirLight.SetAngleX(data.anx);
        DirLight.SetAngleY(data.any);
        DirLight.SetColor(dComp.color);
    }

    #endregion
    
    public GameObject CloneTarget(GameObject target, bool isPropSave)
    {
        var newTarget = GameObject.Instantiate(target, target.transform.parent);
        newTarget.name = target.name;
        newTarget.transform.position = target.transform.position + cloneOffset;
        var oComps = target.GetComponentsInChildren<NodeBaseBehaviour>(true);
        var newComps = newTarget.GetComponentsInChildren<NodeBaseBehaviour>(true);
        for (var i = 0; i < newComps.Length; i++)
        {
            newComps[i].entity = ecsWorld.CloneEntity(oComps[i].entity);
            newComps[i].entity.Get<GameObjectComponent>().bindGo = newComps[i].gameObject;
            newComps[i].OnInitByCreate();
            allControllerBehaviours.Add(newComps[i]);
            HandleSpecialTargetOnClone(oComps[i], newComps[i], isPropSave);
            ParachuteManager.Inst.ParachuteClone(oComps[i], newComps[i]);
            WeaponSystemController.Inst.HandleWeaponClone(oComps[i], newComps[i]);

#if UNITY_EDITOR
            var gCmp = newComps[i].entity.Get<GameObjectComponent>();
            gCmp.bindGo.name = $"{gCmp.bindGo.name}_clone_{gCmp.uid}";
#endif
        }
        SeesawManager.Inst.OnClone(newTarget);
        CloneMatProp(target,newTarget);
        return newTarget;
    }

    public GameObject CloneTargetTemp(GameObject target)
    {
        var newTarget = GameObject.Instantiate(target, target.transform.parent);
        CloneMatProp(target, newTarget);
        var bevh = newTarget.GetComponent<NodeBaseBehaviour>();
        GameObject.Destroy(bevh);
        return newTarget;
    }

    private void CloneMatProp(GameObject org,GameObject clone)
    {
        var oRenders = org.GetComponentsInChildren<Renderer>(true);
        var cRenders = clone.GetComponentsInChildren<Renderer>(true);
        if (oRenders.Length != cRenders.Length)
        {
            LoggerUtils.Log("Clone Fail");
            return;
        }
        var matProp = new MaterialPropertyBlock();
        for (int i = 0; i < oRenders.Length; i++)
        {
            oRenders[i].GetPropertyBlock(matProp);
            cRenders[i].SetPropertyBlock(matProp);
        }
    }

    public SceneEntity GetEntityByUid(int findUid)
    {
        foreach(var nBehaviour in allControllerBehaviours){
            if(nBehaviour.entity.Get<GameObjectComponent>().uid == findUid){
                return nBehaviour.entity;
            }
        }
        return null;
    }

    public void DestroyEntity(GameObject go)
    {
        DestroyEntityInChild(go);
        if (go.GetComponent<NodeBaseBehaviour>() == null)
        {
            GameObject.Destroy(go);
        }
    }

    public void DestroyEntityInChildNotSelf(GameObject go)
    {
        var selfBehaviour = go.GetComponent<NodeBaseBehaviour>();
        var baseBehaivours = go.GetComponentsInChildren<NodeBaseBehaviour>(true);
        for (int i = 0; i < baseBehaivours.Length; i++)
        {
            if (baseBehaivours[i] != selfBehaviour)
            {
                var entity = baseBehaivours[i].entity;
                RemoveNodeBehaviour(baseBehaivours[i]);
                baseBehaivours[i].OnReset();
                ecsWorld.DestroyEntity(entity);
                GameObject.Destroy(baseBehaivours[i].gameObject);
            }
        }
    }

    public void DestroyEntityInChild(GameObject go)
    {
        var baseBehaivours = go.GetComponentsInChildren<NodeBaseBehaviour>(true);
        for (int i = 0; i < baseBehaivours.Length; i++)
        {
            DestroyEntity(baseBehaivours[i]);
        }
    }
    //代办：地形、出生点创建流程需要修改
    //Entity数据需要存储
    private void DestroyEntity(SceneEntity entity)
    {
        var comp = entity.Get<GameObjectComponent>();
        var behav = comp.bindGo.GetComponent<NodeBaseBehaviour>();
        DestroyEntity(behav);
    }

    private void DestroyEntity(NodeBaseBehaviour nBehav)
    {
        var entity = nBehav.entity;
        var comp = entity.Get<GameObjectComponent>();
        RemoveNodeBehaviour(nBehav);
        SecondCachePool.Inst.RemoveItem(nBehav.gameObject);
        nBehav.OnReset();
        ecsWorld.DestroyEntity(entity);
        if (nBehav is BaseHLODBehaviour hlodBehaviour)
        {
            if (hlodBehaviour.hlodState == HLODState.Cull || hlodBehaviour.assetObj == null)
            {
                UnityEngine.Object.Destroy(nBehav.gameObject);
            }
            else
            {
                ModelCachePool.Inst.Release(comp.modId, hlodBehaviour.assetObj);
                UnityEngine.Object.Destroy(nBehav.gameObject);
            }
        }
        else if(nBehav is ActorNodeBehaviour actorBehaviour)
        {
            if(actorBehaviour.assetObj != null)
            {
                ModelCachePool.Inst.Release(comp.modId, actorBehaviour.assetObj);
            }
            UnityEngine.Object.Destroy(nBehav.gameObject);
        }
        else
        {
            ModelCachePool.Inst.Release(comp.modId, nBehav.gameObject);
        }

    }

    public void RemoveNodeBehaviour(NodeBaseBehaviour nBehav)
    {
        hideEntityBevs.Remove(nBehav);
        likeEntityBevs.Remove(nBehav);
        favoriteEntityBevs.Remove(nBehav);
        attentionEntityBevs.Remove(nBehav);
        allControllerBehaviours.Remove(nBehav);
        // SwitchManager.Inst.OnRemoveNode(nBehav);
        // SoundManager.Inst.OnRemoveNode(nBehav);
        // ShowHideManager.Inst.OnRemoveNode(nBehav);
        NodeBehaviourManager.Inst.ClearManagerNodeBehaviour(nBehav);
    }

    public void RevertNodeBehaviour(NodeBaseBehaviour nBehaviour)
    {
        if(!allControllerBehaviours.Contains(nBehaviour))
        {
            allControllerBehaviours.Add(nBehaviour);
        }

        var modelType = nBehaviour.entity.Get<GameObjectComponent>().modelType;
        switch (modelType)
        {
            case NodeModelType.SpotLight:
            case NodeModelType.PointLight:
                if(!hideEntityBevs.Contains(nBehaviour)){
                    hideEntityBevs.Add(nBehaviour);
                } 
                break;
            case NodeModelType.Like:
                if(!likeEntityBevs.Contains(nBehaviour)){
                    likeEntityBevs.Add(nBehaviour);
                }  
                break;
            case NodeModelType.Favorite:
                if (!favoriteEntityBevs.Contains(nBehaviour))
                {
                    favoriteEntityBevs.Add(nBehaviour);
                }
                break;
            case NodeModelType.Attention:
                if (!attentionEntityBevs.Contains(nBehaviour))
                {
                    attentionEntityBevs.Add(nBehaviour);
                }
                break;
            default:
                break;
        }
        NodeBehaviourManager.Inst.RevertManagerNodeBehaviour(nBehaviour);     
    }

    public void DestroyScene()
    {
        hideEntityBevs.Clear();
        likeEntityBevs.Clear();
        favoriteEntityBevs.Clear();
        attentionEntityBevs.Clear();
        ParsePropWithTipsManager.Inst.Release();
        UndoRecordPool.Inst.ClearPool();
        SecondCachePool.Inst.ClearPool();
        DestroyEntityInChild(SceneParent.gameObject);
        RebuildScene();
        ecsWorld.DestoryAllEntity();
        Resources.UnloadUnusedAssets();
        MapRenderManager.Inst.Clear();
        GC.Collect();
    }

    private bool IsSpecialPGCNode(NodeData data)
    {
        //以后类似的PGC素材建议从11000开始记
        if (data.id >= 10009 && data.id <= 10024)
        {
            return true;
        }
        return false;
    }

    public bool CanCloneTarget(GameObject target)
    {
        var nBehavs = target.GetComponentsInChildren<NodeBaseBehaviour>();
        if (nBehavs == null)
        {
            return false;
        }

        //传送按钮
        int countButton = target.GetComponentsInChildren<PortalButtonBehaviour>().Length;
        if (PortalPointManager.Inst.GetCurMax() + countButton > PortalPointManager.MaxCount)
        { 
        
            return false;
        }
        //传送点
        int countPoint = target.GetComponentsInChildren<PortalPointBehaviour>().Length;
        if (PortalPointManager.Inst.GetCurMax() + countPoint > PortalPointManager.MaxCount)
        {
            return false;
        }
        //陷阱盒
        int countTrap = target.GetComponentsInChildren<TrapBoxBehaviour>().Length;
        if (TrapSpawnManager.Inst.IsOverMaxTrapCountWhenClone(countTrap))
        {
            return false;
        }
        //开关
        int countSwtich = target.GetComponentsInChildren<SwitchButtonBehaviour>().Length;
        if (!SwitchManager.Inst.IsCanCloneSwitch(countSwtich))
        {
            return false;
            
        }
        //3d视频
        int videoCount = target.GetComponentsInChildren<VideoNodeBehaviour>().Length;
        if (!VideoNodeManager.Inst.IsCanClone(videoCount))
        {
            return false;
        }

        //感应盒
        int sensorBoxCount = target.GetComponentsInChildren<SensorBoxBehaviour>().Length;
        if (!SensorBoxManager.Inst.IsCanClone(sensorBoxCount))
        {
            return false;
        }
        //冰冻道具
        int freezePropsCount = target.GetComponentsInChildren<FreezePropsBehaviour>().Length;
        if (!FreezePropsManager.Inst.IsCanClone(freezePropsCount))
        {
            return false;
        }
        //火焰道具
        int firePropsCount = target.GetComponentsInChildren<FirePropBehaviour>().Length;
        if (!FirePropManager.Inst.IsCanClone(firePropsCount))
        {
            return false;
        }
        return true;
    }

    private void HandleSpecialTargetOnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour, bool isPropSave)
    {
        var modelType = nBehaviour.entity.Get<GameObjectComponent>().modelType;
        switch (modelType)
        {
            case NodeModelType.PortalButton:
                int bId = PortalPointManager.Inst.GetBtnNextPid();
                var bBev = nBehaviour as PortalButtonBehaviour;
                bBev.pid = bId;
                bBev.RefreshButtonId();
                var bComp = bBev.entity.Get<PortalButtonComponent>();
                bComp.pid = bId;
                PortalPointManager.Inst.AddPortalButton(bId, nBehaviour.entity);
                break;

            case NodeModelType.PortalPoint:
                int pId = PortalPointManager.Inst.GetPointNextPid();
                var pBev = nBehaviour as PortalPointBehaviour;
                pBev.pid = pId;
                pBev.RefreshPointId();
                var pComp = pBev.entity.Get<PortalPointComponent>();
                pComp.pid = pId;
                PortalPointManager.Inst.AddPortalPoint(pId, nBehaviour.entity);
                break;
            case NodeModelType.SpotLight:
            case NodeModelType.PointLight:
                hideEntityBevs.Add(nBehaviour);
                break;
            case NodeModelType.PortalGate:
                var entity = nBehaviour.entity;
                var gateComp = entity.Get<PortalGateComponent>();
                gateComp.diyMapId = string.Empty;
                gateComp.mapName = string.Empty;
                gateComp.pngUrl = string.Empty;
                break;
            case NodeModelType.Like:
                likeEntityBevs.Add(nBehaviour);
                break;
            case NodeModelType.Favorite:
                favoriteEntityBevs.Add(nBehaviour);
                break;
            case NodeModelType.Attention:
                attentionEntityBevs.Add(nBehaviour);
                break;
            case NodeModelType.PropStar:
                break;
            case NodeModelType.Switch:
                SwitchButtonBehaviour b = nBehaviour as SwitchButtonBehaviour;
                b.ShowIndexNum();
                break;
            case NodeModelType.TrapBox:
                int tId = TrapSpawnManager.Inst.GetNextId();
                var tBehav = nBehaviour as TrapBoxBehaviour;
                var oBehav = oBehaviour as TrapBoxBehaviour;
                var tComp = nBehaviour.entity.Get<TrapBoxComponent>();
                tComp.tId = tId;
                tBehav.RefreshShowId();
                TrapSpawnManager.Inst.AddTrapBox(tId, nBehaviour.entity);
                TrapSpawnManager.Inst.OnHandleClone(oBehav.entity, tBehav.entity);
                break;
            case NodeModelType.MusicBoard:
                //play board music when copy them
                var mBehav = nBehaviour as MusicBoardBehaviour;
                mBehav.PlayWiseAudio();
                break;
            case NodeModelType.Sound:
                SoundManager.Inst.OnHandleClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.ShotPhoto:
                ShotPhotoCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.Video:
                VideoNodeCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.AttackWeapon:
                AttackWeaponCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.LeaderBoard:
                LeaderBoardCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.UgcCloth:
                UgcClothItemCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.BloodRestore:
                BloodPropCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.NewDText:
                NewDTextCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.IceCube:
                IceCubeCreater.OnClone(oBehaviour, nBehaviour);
                break;            
            case NodeModelType.Firework:
                FireworkCreater.OnClone(oBehaviour, nBehaviour);
 				break;
            case NodeModelType.Bounceplank:
                BounceplankManager.Inst.OnHandleClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.Ladder:
                LadderManager.Inst.OnHandleClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.FreezeProps:
                factory.BindCreater<FreezePropsCraeter>().OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.PGCPlant:
                PGCPlantCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.FireProp:
                factory.BindCreater<FirePropCreator>().OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.SnowCube:
                SnowCubeCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.SlidePipe:
                SlidePipeManager.Inst.OnHandleClone(oBehaviour, nBehaviour);
				break;
            case NodeModelType.FishingModel:
                FishingModelCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.BornPoint:
                SpawnPointCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.FlashLight:
                FlashLightCreator.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.PGCEffect:
                PGCEffectCreater.OnClone(oBehaviour, nBehaviour);
                break;
            case NodeModelType.DowntownTransfer:
                TransferCreater.OnClone(oBehaviour, nBehaviour);
                break;
            default:
                LoggerUtils.Log(modelType);
                break;
        }
        if(!isPropSave)
        {
            ShowHideManager.Inst.OnHandleClone(oBehaviour.entity, nBehaviour.entity);
            SwitchControlManager.Inst.OnHandleClone(oBehaviour.entity, nBehaviour.entity);
            SwitchManager.Inst.OnHandleClone(oBehaviour, nBehaviour);
            SensorBoxManager.Inst.OnHandleClone(oBehaviour, nBehaviour);
            CollectControlManager.Inst.OnHandleClone(oBehaviour.entity, nBehaviour.entity);
            UGCBehaviorManager.Inst.OnHandleClone(oBehaviour.entity, nBehaviour.entity);
            PickabilityManager.Inst.OnHandleClone(oBehaviour.entity, nBehaviour.entity);
            FollowBoxCreater.OnClone(oBehaviour.entity, nBehaviour.entity);
            EdibilityManager.Inst.OnHandleClone(oBehaviour.entity, nBehaviour.entity);
            FishingManager.Inst.OnHandleClone(oBehaviour.entity, nBehaviour.entity);
            VIPZoneManager.Inst.OnHandleClone(oBehaviour.entity, nBehaviour.entity);
        }
        
    }
}

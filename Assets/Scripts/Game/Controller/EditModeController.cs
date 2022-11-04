using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Cinemachine;
using Leopotam.Ecs;
using Newtonsoft.Json;
using RTG;
using SavingData;
using UnityEngine;

public class EditModeController : BaseModeController
{
    public static bool IsCanSelect = true;
    public static Vector3 curPos = Vector3.zero;
    public GizmoController gController { private set; get; }
    public static Action<SceneEntity> SetSelect;
    public static Action UnSelectAll;
    public static Action HideGizmo;
    public static Action restoreClick;
    public static Action ClearBehav;
    public static Action<int> SelectGround;
    public static Action<int> SelectSkybox;
    private float maxCamDist = 350;
    private Camera mainCamera;
    private EditModeHandler editHandler;
    private NodeBaseBehaviour curBehav;
    private NodeBaseBehaviour targetBehav;
    private Dictionary<GameResType, Action<int>> uiSelectDic;
    public void Init()
    {
        uiSelectDic = new Dictionary<GameResType, Action<int>>();
        uiSelectDic.Add(GameResType.BornPoint, OnSelectBornPoint);
        uiSelectDic.Add(GameResType.DowntownTransfer, OnSelectDowntownTransfer);
        uiSelectDic.Add(GameResType.Ground, OnSelectGround);
        uiSelectDic.Add(GameResType.Sky, OnSelectSkybox);
        uiSelectDic.Add(GameResType.DirLight, OnSelectDirLight);
        uiSelectDic.Add(GameResType.BaseMode, OnSelectPrimitiveMode);
        uiSelectDic.Add(GameResType.PointLight, OnSelectPointLight);
        uiSelectDic.Add(GameResType.SpotLight, OnSelectSpotLight);
        uiSelectDic.Add(GameResType.PortalPoint, OnSelectPortalPoint);
        uiSelectDic.Add(GameResType.DText, OnSelect3DText);
        uiSelectDic.Add(GameResType.NewDText, OnSelectNew3DText);
        uiSelectDic.Add(GameResType.BGMusic, OnSelectBGMusic);
        uiSelectDic.Add(GameResType.PortalGate, OnSelectPortalGate);
        uiSelectDic.Add(GameResType.Like, OnSelectLike);
        uiSelectDic.Add(GameResType.Attention, OnSelectAttention);
        uiSelectDic.Add(GameResType.CanFly, OnSelectCanFly);
        uiSelectDic.Add(GameResType.TrapBox, OnSelectTrapBox);
        uiSelectDic.Add(GameResType.Switch, OnSelectSwitch);
        uiSelectDic.Add(GameResType.PropStar, OnSelectPropStar);
        uiSelectDic.Add(GameResType.MusicBoard, OnSelectMusicBoard);
        uiSelectDic.Add(GameResType.EnrMusic, OnSelectEnrBGMusic);
        uiSelectDic.Add(GameResType.DisplayBoard, OnSelectDisplayBoard);
        uiSelectDic.Add(GameResType.PostProcess, OnSelectPostProcess);
        uiSelectDic.Add(GameResType.Favorite, OnSelectFavorite);
        uiSelectDic.Add(GameResType.Sound, OnSelectSound);
        uiSelectDic.Add(GameResType.ShotPhoto, OnSelectShotPhoto);
        uiSelectDic.Add(GameResType.Video, OnSelectVideo);
        uiSelectDic.Add(GameResType.PVPWaitArea, OnSelectPVPWaitArea);
        uiSelectDic.Add(GameResType.SensorBox, OnSelectSensorBox);
        uiSelectDic.Add(GameResType.MagneticBoard, OnSelectMagneticBoard);
        uiSelectDic.Add(GameResType.SteeringWheel, OnSelectSteeringWheel);
        uiSelectDic.Add(GameResType.WaterCube, OnSelectWaterCube);
        uiSelectDic.Add(GameResType.Bounceplank, OnSelectBounceplank);
        uiSelectDic.Add(GameResType.Ladder, OnSelectLadder);
        uiSelectDic.Add(GameResType.AttackWeapon, OnSelectAttackWeapon);
        uiSelectDic.Add(GameResType.LeaderBoard, OnSelectLeaderBoard);
		uiSelectDic.Add(GameResType.UgcCloth, OnSelectUgcCloth);
        uiSelectDic.Add(GameResType.ShootWeapon, OnSelectShootWeapon);
        uiSelectDic.Add(GameResType.BloodRestore, OnSelectBloodProp);
        uiSelectDic.Add(GameResType.IceCube, OnSelectIceCube);
        uiSelectDic.Add(GameResType.Firework, OnSelectFirework);
        uiSelectDic.Add(GameResType.PGCPlant, OnSelectPGCPlant);
        uiSelectDic.Add(GameResType.Parachute, OnSelectParachute);
        uiSelectDic.Add(GameResType.Weather, OnSelectWeather);
        uiSelectDic.Add(GameResType.FreezeProps, OnSelectFreezePprops);
        uiSelectDic.Add(GameResType.FireProp, OnSelectFireProp);
        uiSelectDic.Add(GameResType.SnowCube, OnSelectSnowCube);
        uiSelectDic.Add(GameResType.SeeSaw, OnSelectSeeSaw);
        uiSelectDic.Add(GameResType.FishingModel, OnSelectFishing);
        uiSelectDic.Add(GameResType.SlidePipe, OnSelectSlidePipe);
        uiSelectDic.Add(GameResType.VIPZone, OnSelectVIPZone);       
        uiSelectDic.Add(GameResType.FlashLight, OnSelectFlashLight);
        uiSelectDic.Add(GameResType.PGCEffect, OnSelectPGCEffect);
        uiSelectDic.Add(GameResType.Swing, OnSelectSwing);
        uiSelectDic.Add(GameResType.CrystalStone, OnSelectCrystalStone);

        SetSelect = SetSelectTarget;
        
        UnSelectAll = UnSelectAllTarget;
        SelectGround = OnSelectGround;
        SelectSkybox = OnSelectSkybox;
        HideGizmo = HideGizmos;
        restoreClick = allowClick;
        ClearBehav = ClearCurBehav;
    }


    public static void SaveMapByFirst()
    {
        var content = SceneParser.Inst.StageToMapJson();
        ZipUtils.SaveZipJsonLocal(content, (fileName) =>
        {
            LoggerUtils.Log("local save empty scene success" + fileName);
            GameManager.Inst.gameMapInfo.mapId = GameManager.Inst.ugcUntiyMapDataInfo.mapId;
            GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
            GlobalFieldController.whiteListMask.SetInWhiteList(WhiteListMask.WhiteListType.OfflineRender);
            if (string.IsNullOrEmpty(GlobalFieldController.CurMapInfo.mapId))
            {
                GlobalFieldController.CurMapInfo.mapId = GameInfo.Inst.myUid + "_" + GameUtils.GetTimeStamp() + "_map";
            }
            DataUtils.SetMapInfoLocal(OperationType.ADD);
            DataUtils.SetConfigLocal(CoverType.JPG);
        }, (err) =>
        {
            LoggerUtils.LogError("local save empty scene fail");
        });
    }

    public static void SavePropByFirst()
    {
        var content = SceneParser.Inst.StageToMapJson();
        ZipUtils.SaveZipJsonLocal(content, (fileName) =>
        {
            var propContent = SceneParser.Inst.StageToPropJson();
            ZipUtils.SavePropZipJsonLocal(propContent, (fileName) =>
            {
                LoggerUtils.Log("local save empty prop scene success" + fileName);
                GameManager.Inst.gameMapInfo.mapId = GameManager.Inst.ugcUntiyMapDataInfo.mapId;
                GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
                if (string.IsNullOrEmpty(GlobalFieldController.CurMapInfo.mapId))
                {
                    GlobalFieldController.CurMapInfo.mapId = GameInfo.Inst.myUid + "_" + GameUtils.GetTimeStamp() + "_prop";
                }
                DataUtils.SetMapInfoLocal(OperationType.ADD);
                DataUtils.SetConfigLocal(CoverType.PNG);
            }, (err) => { LoggerUtils.LogError("local save empty prop fail"); });
        }, (err) => { LoggerUtils.LogError("local save empty scene fail"); });
    }





    public static void SavePropJsonByAuto()
    {
        var content = SceneParser.Inst.StageToMapJson();
        ZipUtils.SaveZipJsonLocal(content, (fileName) =>
        {
            var propContent = SceneParser.Inst.StageToPropJson();
            ZipUtils.SavePropZipJsonLocal(propContent, (fileName) =>
            {
                LoggerUtils.Log("local auto save prop scene success" + fileName);
                //mapId为空则是创建地图
                var optType = string.IsNullOrEmpty(GameManager.Inst.gameMapInfo.mapId) ? OperationType.ADD : OperationType.UPDATE;
                DataUtils.SetMapInfoLocal(optType);
                DataUtils.SetConfigLocal(CoverType.PNG);
            }, (err) => { LoggerUtils.LogError("local auto save prop fail"); });
        }, (err) => { LoggerUtils.LogError("local auto save scene fail"); });
    }

    public static void SaveMapJsonByAuto()
    {
        GameManager.Inst.gameMapInfo.imgs = ShotPhotoManager.Inst.getUrlArray();
        //自动保存时，也需要发送给服务器素材列表
        SceneParser.Inst.GetResList();
        //保存封面的时候，也需要发送给服务器DC列表
        SceneParser.Inst.GetDcList();
        //自动保存时，也需要发送给服务器
        SceneParser.Inst.GetUnitySaveData();
        SoundManager.Inst.GetAudioList();
        LoggerUtils.Log("SaveMapJsonByAuto called");
        var content = SceneParser.Inst.StageToMapJson();
        ZipUtils.SaveZipJsonLocal(content, (fileName) =>
        {
            LoggerUtils.Log("local auto save scene success" + fileName);
            //mapId为空则是创建地图
            var optType = string.IsNullOrEmpty(GameManager.Inst.gameMapInfo.mapId) ? OperationType.ADD : OperationType.UPDATE;
            DataUtils.SetMapInfoLocal(optType);
            DataUtils.SetConfigLocal(CoverType.JPG);
        }, (err) =>
        {
            LoggerUtils.LogError("local auto save scene fail");
        });
    }



    public static void SaveMapJson(Action<string> success, Action<string> fail)
    {
        GameManager.Inst.gameMapInfo.imgs = ShotPhotoManager.Inst.getUrlArray();
        SceneParser.Inst.GetResList();
        //保存封面的时候，也需要发送给服务器DC列表
        SceneParser.Inst.GetDcList();
        SceneParser.Inst.GetUnitySaveData();
        SoundManager.Inst.GetAudioList();
        var content = SceneParser.Inst.StageToMapJson();
        MobileInterface.Instance.LogEventByEventName(LogEventData.unity_zipJson_start);
        ZipUtils.SaveZipJsonLocal(content, (fileName) =>
        {
            MobileInterface.Instance.LogEventByEventName(LogEventData.unity_zipJson_success);
            success?.Invoke(fileName);
#if UNITY_EDITOR
            File.WriteAllText(Application.streamingAssetsPath + fileName, content);
#endif
        }, (err) =>
        {
            fail?.Invoke(err);
        });
    }

    public static void SavePropJson(Action<string> success, Action<string> fail)
    {
        var content = SceneParser.Inst.StageToPropJson();
        ZipUtils.SavePropZipJsonLocal(content, (fileName) =>
        {
            success?.Invoke(fileName);
        }, (err) =>
        {
            fail?.Invoke(content);
        });
    }

    public static void SaveMapCover(Action<string> success, Action<string> fail)
    {
        float oriMaskW = 3186;
        float oriMaskH = 1125;
        const int screenShotW = 1740;
        const int screenShotH = 1113;
        float curRefWidth = 0;
        float curRefHeight = 0;
        Rect screenShotRec = new Rect();
        curRefHeight = (Screen.height / oriMaskH) * screenShotH;
        curRefWidth = (Screen.height / oriMaskH) * screenShotW;
        screenShotRec = new Rect(Screen.width / 2 - curRefWidth / 2, Screen.height / 2 - curRefHeight / 2 , curRefWidth, curRefHeight);
        RTGApp.Get.enabled = false;
        var bytes = ScreenShotUtils.ScreenShot(GameManager.Inst.MainCamera, screenShotRec);
        if (bytes == null || bytes.Length == 0)
        {
            fail?.Invoke("Save Fail");
            return;
        }
        DataUtils.SaveCoverLocal(bytes, CoverType.JPG);
        RTGApp.Get.enabled = true;
        success?.Invoke(null);
    }

    public static void SaveResMapCover(Action<string> success, Action<string> fail)
    {
        float oriMaskW = 3186;
        float oriMaskH = 1125;
        const int screenShotW = 846;
        const int screenShotH = 846;
        float curRefWidth = 0;
        float curRefHeight = 0;
        Rect screenShotRec = new Rect();
        curRefHeight = (Screen.height / oriMaskH) * screenShotH;
        curRefWidth = (Screen.height / oriMaskH) * screenShotW;
        screenShotRec = new Rect(Screen.width / 2 - curRefWidth / 2, Screen.height / 2 - curRefHeight / 2, curRefWidth, curRefHeight);

        RTGApp.Get.enabled = false;
        var bytes = ScreenShotUtils.ResScreenShot(GameManager.Inst.MainCamera, screenShotRec);
        if (bytes == null || bytes.Length == 0)
        {
            fail?.Invoke("Save Fail");
            return;
        }
        DataUtils.SaveCoverLocal(bytes, CoverType.PNG);
        RTGApp.Get.enabled = true;
        success?.Invoke(null);
    }


    public void SetCamera(Camera cam, CinemachineVirtualCamera vCam)
    {
        cam.enabled = true;
        mainCamera = cam;
        gController = new GizmoController();
        editHandler = new EditModeHandler();
        editHandler.SetCamera(cam, vCam);
        editHandler.OnSelectTarget = OnSelectTarget;
        
        var rtgApp = ResManager.Inst.LoadResNoCache<GameObject>(GameConsts.PluginPath + "RTGApp");
        var RTG = GameObject.Instantiate(rtgApp);
        var RTGCameraScript = RTG.GetComponentInChildren<RTFocusCamera>();
        RTGCameraScript.SetTargetCamera(mainCamera);
        RTG.gameObject.SetActive(true);
    }
    public void SetEditHandler()
    {
        if (editHandler != null)
        {
            if(ReferManager.Inst.isRefer)
            {
                editHandler.joyStick = ReferPanel.Instance.joyStick;
                editHandler.joyStick.JoystickReset();
            }

            InputReceiver.Inst.SetHandle(editHandler);
            SceneGizmoPanel.Instance.SetEditModeHandler(editHandler);
        }
    }

    public static void AddCreateRecord(GameObject gameObject)
    {
        LoggerUtils.Log("EditModeController AddCreateRecord");
        if(gameObject == null)
        {
            LoggerUtils.LogError("EditModeController AddCreateRecord gameObject is null!");
            return;
        }
        CreateDestroyUndoData beginData = new CreateDestroyUndoData();
        beginData.targetNode = null;
        beginData.createUndoMode = (int)CreateUndoMode.Create;

        CreateDestroyUndoData endData = new CreateDestroyUndoData();
        endData.targetNode = gameObject;
        beginData.createUndoMode = (int)CreateUndoMode.Create;

        UndoRecord record = new UndoRecord(UndoHelperName.CreateDestroyUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        UndoRecordPool.Inst.PushRecord(record);
    }

    public static void AddDestroyRecord(GameObject gameObject)
    {
        CreateDestroyUndoData beginData = new CreateDestroyUndoData();
        beginData.targetNode = gameObject;
        beginData.createUndoMode = (int)CreateUndoMode.Destroy;

        CreateDestroyUndoData endData = new CreateDestroyUndoData();
        endData.targetNode = null;
        beginData.createUndoMode = (int)CreateUndoMode.Destroy;

        UndoRecord record = new UndoRecord(UndoHelperName.CreateDestroyUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        UndoRecordPool.Inst.PushRecord(record);
    }

    private void HideGizmos()
    {
        if(gController != null)
        {
            gController.DisableGizmo();
        }
    
        if(curBehav != null)
        {
            targetBehav = curBehav;
            curPos = curBehav.transform.localPosition;
            Collider[] colliders = curBehav.gameObject.GetComponentsInChildren<Collider>(true);
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }           
        }
    }
    private void allowClick()
    {
        if(targetBehav != null)
        {
            Collider[] colliders = targetBehav.gameObject.GetComponentsInChildren<Collider>(true);
            foreach (var collider in colliders)
            {
                collider.enabled = true;
            } 
        }
    }
    private void ClearCurBehav()
    {
        curBehav = null;
    }
    private void OnSelectTarget(Touch touch)
    {
        if (GlobalFieldController.isScreenShoting || !IsCanSelect)
            return;
        Ray ray = mainCamera.ScreenPointToRay(touch.position);

        if (SeparatePartEditPanel.IsOn) //编辑道具部件面板打开
        {
            SeparatePartEditPanel.Instance.OnClick(ray, maxCamDist);
            return;
        }

        //"SpecialModel" pack model visible,camera cover invisble 
        bool isHit = Physics.Raycast(ray, out RaycastHit hit, 2 * maxCamDist,
            1 << LayerMask.NameToLayer("Model")
            | 1 << LayerMask.NameToLayer("ShotExclude")
            | 1 << LayerMask.NameToLayer("SpecialModel")
            | 1 << LayerMask.NameToLayer("TriggerModel")
            | 1 << LayerMask.NameToLayer("Touch")
            | 1 << LayerMask.NameToLayer("PVPArea")
            | 1 << LayerMask.NameToLayer("WaterCube")
            | 1 << LayerMask.NameToLayer("IceCube"));

        if (!isHit)
        {
            if (ReferPanel.Instance&&ReferManager.Inst.isRefer && ReferManager.Inst.isHafeRefer)
            {
                ReferPanel.Instance.OnReferMode();
            }
            DisableAllPanel();
            if (ReferPanel.Instance)
            {
                ReferPanel.Show();
                ReferManager.Inst.isHafeRefer = false;
            }
            BasePrimitivePanel.DisSelect();
            MovePathManager.Inst.CloseAndSave();
            gController.DisableGizmo();
            HideAccessories();
            return;
        }
        var go = hit.collider.gameObject;
        var nodeBehav = go.GetComponentInParent<NodeBaseBehaviour>();
        var entity = SceneObjectController.GetCanControllerNode(nodeBehav.gameObject);
        if (entity != null)
        {
            if (ReferPanel.Instance && ReferManager.Inst.isRefer)
            {
                ReferPanel.Hide();
                ReferManager.Inst.isHafeRefer = true;
            }
            HideAccessories();
            SetSelectTarget(entity);
        }
    }

    protected virtual void SetSelectTarget(SceneEntity entity)
    {
        var gameComp = entity.Get<GameObjectComponent>();
        curBehav = gameComp.bindGo.GetComponent<NodeBaseBehaviour>();
        DisableAllPanel();
        ShowModelPanel(entity);
        ShowWeaponModelPanel(entity);
        ShowBloodPropPanel(entity);
        ShowParachuteOrBag(entity);
        ShowSeeSawPanel(entity);
        gController.SetTarget(gameComp.bindGo);

        ShowPipePanel(entity);
        ShowVIPZonePanel(entity);
        BasePrimitivePanel.Instance.OnIconSelect(gameComp.modId);
        ShowModelHandlePanel(gameComp.handleType, entity);
        ShowFishingPanel(entity);
        ReferManager.Inst.isUndoRedo = true;
    }

    private void ShowVIPZonePanel(SceneEntity entity)
    {
        GameObjectComponent gameObjectComponent = entity.Get<GameObjectComponent>();
        
        if (VIPZoneManager.Inst.IsVIPZoneComponent(entity))
        {
            VIPZoneBehaviour vipZoneBehaviour = gameObjectComponent.bindGo.GetComponentInParent<VIPZoneBehaviour>();
            if (vipZoneBehaviour == null)
            {
                return;
            }
            
            gController.SetTarget(vipZoneBehaviour.gameObject);
            
            VIPZonePanel.Show();
            VIPZonePanel.Instance.SetEntity(vipZoneBehaviour.entity);
            
            PropTipsPanel.Show();
            PropTipsPanel.Instance.SetTipsInfo("DC Token VIP Zone","After adding the DC Token VIP Zone,players can only enter this zone if they own the DC you selected.",VIPZonePanel.Instance);
        }
        
    }

    private void ShowSeeSawPanel(SceneEntity entity)
    {
        if (entity.HasComponent<SeesawComponent>())
        {
            SeesawPanel.Show();
            SeesawPanel.Instance.SetEntity(entity);
            
            PropTipsPanel.Show();
            PropTipsPanel.Instance.SetTipsInfo("Seesaw","Place the Seesaw and two players can play together on the Seesaw.",SeesawPanel.Instance);
        }
    }

    private void UnSelectAllTarget()
    {
        MovePathManager.Inst.ReleaseAllPoints();
        DisableAllPanel();
        if (ReferPanel.Instance)
        {
            ReferManager.Inst.isHafeRefer = false;
            ReferPanel.Show();
        }
        BasePrimitivePanel.DisSelect();
    }

    /// <summary>
    /// 武器道具的面板显示
    /// </summary>
    private void ShowWeaponModelPanel(SceneEntity entity)
    {
        if (entity == null) return;
        var weaponType = WeaponSystemController.Inst.GetWeaponTypeInEntity(entity);
        switch (weaponType)
        {
            case WeaponType.Attack:
                //攻击道具
                AttackWeaponPanel.Show();
                AttackWeaponPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(AttackWeaponPanel.Instance);
                break;
            case WeaponType.Shoot:
                //射击道具
                ShootWeaponPanel.Show();
                ShootWeaponPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(ShootWeaponPanel.Instance);
                break;
            default: 
                break;
        }
    }

    /// <summary>
    /// 回血道具的面板显示
    /// </summary>
    private void ShowBloodPropPanel(SceneEntity entity)
    {
        if (entity == null) return;
        if (entity.HasComponent<BloodPropComponent>())
        {
            BloodPropPanel.Show();
            BloodPropPanel.Instance.SetEntity(entity);
            string bloodPropTitle = "HP-Restoring item";
            string bloodPropDesc = "Set up object that you want to use as HP-Restoring item. The player will gain certain health points when touching the object.";
            PropTipsPanel.Show();
            PropTipsPanel.Instance.SetTipsInfo(bloodPropTitle, bloodPropDesc, BloodPropPanel.Instance);
        }
    }

    private void ShowParachuteOrBag(SceneEntity entity)
    {
        if (entity == null) return;

        if (entity.HasComponent<ParachuteComponent>())
        {
            ShowParachutePanel(entity);
        }
        else if (entity.HasComponent<ParachuteBagComponent>())
        {
            var paraBehav = ParachuteManager.Inst.OnParachuteBagSelect(entity);
            if (paraBehav != null && paraBehav.entity != null)
            {
                ShowParachutePanel(paraBehav.entity);
            }
        }
    }

    private void ShowFishingPanel(SceneEntity entity)
    {
        if (entity == null)
            return;

        var comp = entity.Get<GameObjectComponent>();
        var isRod = entity.HasComponent<FishingRodComponent>();
        var isHook = entity.HasComponent<FishingHookComponent>();

        if (isRod || isHook)
        {
            var fishingNode = comp.bindGo.transform.GetComponentInParent<FishingBehaviour>();
            SetSelectTarget(fishingNode.entity);
            gController.SetTarget(fishingNode.entity.Get<GameObjectComponent>().bindGo);
        }
    }

    private void ShowParachutePanel(SceneEntity entity)
    {
        string parachuteTitle = "Parachute";
        string parachuteDesc = "After the player picks up the parachute and jumps into the air, he can move in the air by gliding and landing.";
        ParachutePanel.Show();
        ParachutePanel.Instance.SetParachuteEntity(entity);
        PropTipsPanel.Show();
        PropTipsPanel.Instance.SetTipsInfo(parachuteTitle, parachuteDesc, ParachutePanel.Instance);
    }

    private void ShowPipePanel(SceneEntity entity)
    {
        if (entity == null) return;
        var gameComp = entity.Get<GameObjectComponent>();

        if(gameComp.modelType == NodeModelType.SlidePipe)
        {
            var parBehav = gameComp.bindGo.GetComponent<SlidePipeBehaviour>();
            var itemBehaviour = parBehav.GetTailItem();
            if(itemBehaviour)
            {
                SlidePipePanel.Show();
                SlidePipePanel.Instance.SetEntity(entity, itemBehaviour.entity);
                string slidePipeTitle = "Slider";
                string slidePipeDesc = "Build up and place the Slider where you want to slide through.";
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(slidePipeTitle, slidePipeDesc, SlidePipePanel.Instance);
            }
        }
        else if(gameComp.modelType == NodeModelType.SlideItem)
        {
               
            var parent = gameComp.bindGo.transform.parent;
            if(parent != null)
            {
                var parBehav = parent.GetComponentInParent<SlidePipeBehaviour>();
                if (parBehav != null)
                {
                    gController.SetTarget(parBehav.gameObject);
                    SlidePipePanel.Show();
                    SlidePipePanel.Instance.SetEntity(parBehav.entity, entity);
                    string slidePipeTitle = "Slider";
                    string slidePipeDesc = "Build up and place the Slider where you want to slide through.";
                    PropTipsPanel.Show();
                    PropTipsPanel.Instance.SetTipsInfo(slidePipeTitle, slidePipeDesc, SlidePipePanel.Instance);
                }
            }

        }
    }

    private void ShowModelPanel(SceneEntity entity)
    {
        var gameComp = entity.Get<GameObjectComponent>();
        switch (gameComp.modelType)
        {
            case NodeModelType.BaseModel:
                BaseMatColorPanel.Show();
                BaseMatColorPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(BaseMatColorPanel.Instance);
                break;
            case NodeModelType.PointLight:
                PointLightPanel.Show();
                PointLightPanel.Instance.SetEntity(entity);
                PointLightPanel.Instance.SetInitArgs();
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(PointLightPanel.Instance);
                break;
            case NodeModelType.SpotLight:
                SpotLightPanel.Show();
                SpotLightPanel.Instance.SetEntity(entity);
                SpotLightPanel.Instance.SetInitArgs();
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(SpotLightPanel.Instance);
                break;
            case NodeModelType.PortalButton:
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Teleport Button", "When pressed, players will be teleported to the corresponding spawn point.");
                break;
            case NodeModelType.PortalPoint:
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Teleport Spawn Point", "When the corresponding teleport button is pressed, players will be teleported to this spawn point.");
                break;
            case NodeModelType.DText:
                DTextPanel.Show();
                DTextPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(DTextPanel.Instance);
                break;
            case NodeModelType.NewDText:
                NewDTextPanel.Show();
                NewDTextPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(NewDTextPanel.Instance);
                break;
            case NodeModelType.PortalGate:
                PortalGatePanel.Show();
                PortalGatePanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(PortalGatePanel.Instance);
                break;
            case NodeModelType.TrapBox:
                TrapBoxPanel.Show();
                TrapBoxPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Trap Box", "Place the trap box on your designed trap such as enemies or lava pits. Adjust the size of the trap box to fit your design. Players will return to the default or designated spawn point upon contact with the trap box.",TrapBoxPanel.Instance);               
                break;
            case NodeModelType.PropStar:
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Collectibles", "After placing the collectibles, select the object you want to control via the collectibles. Under the <i><b><size=40>Properties</size></b></i> setting-> <i><b><size=40>Collectibles</size></b></i> Control,  finish collecting all the collectibles will toggle the visibility or mobility of the controlled object.");
                break;
            case NodeModelType.Switch:
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("<b>Toggle Switch</b>", "After placing the switch, select the object you want to control. Under the <i><b><size=40>Properties</size></b></i> setting-> <i><b><size=40>Switch Control</size></b></i>，you can set the corresponding switch that can control the visibility or mobility of this object. Once pressed, the switch will toggle the object on and off.");
                break;
            case NodeModelType.MusicBoard:
                MusicBoardPanel.Show();
                MusicBoardPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(MusicBoardPanel.Instance);
                break;
            case NodeModelType.BornPoint:
                string title = "Spawn Point: The location when a player enters a map.";
                string desc = "Each map is set to have 16 spawn points. The player will spawn at sequential order according to the vacancy of the map.";
                SpawnPointPanel.Show();
                SpawnPointPanel.Instance.SetBehav(gameComp);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(title, desc, SpawnPointPanel.Instance);
                break;
            case NodeModelType.WaterCube:
                WaterMaterialPanel.Show();
                WaterMaterialPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Water Cube", "Resize the Water Cube and place it where you want to add moving state in water! You can enter the Water Cube to swim or walk.", WaterMaterialPanel.Instance);

                break;
            case NodeModelType.DisplayBoard:
                DisplayBoardPanel.Show();
                DisplayBoardPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(DisplayBoardPanel.Instance);
                break;
            case NodeModelType.MagneticBoard:
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Adhesive surface", "You can combine the adhesive surface with the object and set the moving track to fasten a player on the object.");
                break;
            case NodeModelType.Attention:
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Follow","Players will follow you by pressing this button.");
                break;
            case NodeModelType.Favorite:
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Favor", "Players will favorite this experience by pressing this button.");
                break;
            case NodeModelType.Sound:
                SoundPanel.Show();
                SoundPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Sound Button", "Set the sound you like, the sound will be played when you press the sound button.", SoundPanel.Instance);
                break;
            case NodeModelType.ShotPhoto:
                ShotPhotoPanel.Show();
                ShotPhotoPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(ShotPhotoPanel.Instance);

                break;
            
            case NodeModelType.SteeringWheel:
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Steering wheel", "Move objects by manipulating the steering wheel After combining objects with steering wheels, click the Drive button to enter Driving Mode.");
                break;
            case NodeModelType.Video:
                VideoNodePanel.Show();
                VideoNodePanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("<b>Video Player</b>","Only supports YouTube video links. Live and Shorts are not supported.", VideoNodePanel.Instance);
                break;
            case NodeModelType.PVPWaitArea:
                PVPWaitAreaPanel.Show();
                PVPWaitAreaPanel.Instance.SetEntity(PVPWaitAreaManager.Inst.PVPBehaviour.entity);
                PropTipsPanel.Show();
                //需要接入多语言
                PropTipsPanel.Instance.SetTipsInfo("Waiting Zone", "The Game Mode will be turned on upon adding the Waiting Zone.", PVPWaitAreaPanel.Instance);
				break;
            case NodeModelType.SensorBox:
                SensorBoxPanel.Show();
                SensorBoxPanel.Instance.SetEntity(entity);
                string sensorTitle = "Sensor Box";
                string sendorDesc = "After placing the Sensor Box, select the object you want to control. Under the Properties setting -> Sensor Box Control, you can set the corresponding Sensor Box that can control the visibility or mobility of the object. Once entered the Sensor Box, the properties of the object will be toggled.";
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(sensorTitle, sendorDesc,SensorBoxPanel.Instance);
                break;
            case NodeModelType.LeaderBoard:
                LeaderBoardPanel.Show();
                LeaderBoardPanel.Instance.SetEntity(entity);
                string leaderTitle = "Leaderboard";
                string leaderDesc = "After adding a leaderboard, the player's ranking information will be displayed in this experience and its details page.";
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(leaderTitle, leaderDesc, LeaderBoardPanel.Instance);
                break;
            case NodeModelType.UgcCloth:
                UgcClothItemBehaviour be = gameComp.bindGo.GetComponent<UgcClothItemBehaviour>();
                if (be!=null&&be.isSoldOut)
                {
                    break;
                }
                UgcClothItemPanel.Show();
                UgcClothItemPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(UgcClothItemPanel.Instance);
                break;
            case NodeModelType.IceCube:
                IceCubePanel.Show();
                IceCubePanel.Instance.SetEntity(entity);
                string iceTitle = "Tips";
                string iceDesc = "Resize the Ice Cube and place it where you want to add moving state on ice! You can skate while moving on Ice Cube.";
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(iceTitle, iceDesc, IceCubePanel.Instance);
                break;
            //烟花
            case NodeModelType.Firework:
                FireworkPanel.Show();
                FireworkPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                string fireworkTitle = "Firework";
                string fireworkDesc = "Set up objects that you want to use as fireworks. The player will launch fireworks via triggers.";
                PropTipsPanel.Instance.SetTipsInfo(fireworkTitle, fireworkDesc, FireworkPanel.Instance);
				break;
            case NodeModelType.Bounceplank:
                BounceplankPanel.Show();
                BounceplankPanel.Instance.SetEntity(entity);
                string bounceplankTitle = "Bounceplank";
                string bounceplankDesc = "Resize the Bounceplank and place it where you want player to jump! You will keep bouncing until you leave Bounceplank.";
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(bounceplankTitle, bounceplankDesc, BounceplankPanel.Instance);
                break;
            case NodeModelType.Ladder:
                LadderPanel.Show();
                LadderPanel.Instance.SetEntity(entity);
                string ladderTitle = "Ladder";
                string ladderDesc = "Resize the Ladder and place it where you want to climb! You can climb up while approaching a ladder.";
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(ladderTitle, ladderDesc, LadderPanel.Instance);
                break;
            case NodeModelType.PGCPlant:
                PGCPlantPanel.Show();
                PGCPlantPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(PGCPlantPanel.Instance);
                break;
            case NodeModelType.FreezeProps:
                FreezePropsPanel.Show();//添加一个 冰洁道具的面板 FreezePropsPanel
                FreezePropsPanel.Instance.SetEntity(entity);
                string Title = "Freeze Item";
                string Desc = "Once the player touches the frozen item, he will be frozen and unable to move until it is unfrozen.";
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(Title, Desc, FreezePropsPanel.Instance);
                break;
            case NodeModelType.FireProp:
                FirePropPanel.Show();//火焰
                FirePropPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Fire", "Place the Fire where you want to illuminate the surroundings or cause damage as a trap.", FirePropPanel.Instance);
                break;
            case NodeModelType.SnowCube:
                SnowCubePanel.Show();
                SnowCubePanel.Instance.SetEntity(entity);
                string snowCubeTitle = "Snow Cube";
                string snowCubeDesc = "Resize the Snow Cube and place it where you want to ski on snow!";
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(snowCubeTitle, snowCubeDesc, SnowCubePanel.Instance);
                break;
            case NodeModelType.FishingRod:
            case NodeModelType.FishingHook:
            case NodeModelType.FishingModel:
                string fishingTitle = "Fishing Rod";
                string fishingDesc = "After adding a fishing rod, the player can use the fishing rod to catch objects that are set in \"Catchable\" state in the Catchability of the Properties";
                FishingRodPanel.Show();
                FishingRodPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(fishingTitle, fishingDesc, FishingRodPanel.Instance);
                break;
            case NodeModelType.FlashLight:
                FlashLightPanel.Show();
                FlashLightPanel.Instance.SetEntity(entity);
                string flashlightTitle = "Flashlight";
                string flashlightDesc = "Place Flashlight to shoot a beam and illuminate a zone. Color of Flashlight can be set to change in order or randomly.";
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(flashlightTitle, flashlightDesc, FlashLightPanel.Instance);
                break;
            case NodeModelType.Swing:
                SwingPanel.Show();
                SwingPanel.Instance.SetEntity(entity);
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo("Swing","Replace the swing seat as you want. The position of the rope and seat can be adjusted separately.",SwingPanel.Instance);
                break;
            case NodeModelType.PGCEffect:
                PGCEffectPanel.Show();
                PGCEffectPanel.Instance.SetEntity(entity);
                string pgcEffectTitle = "Particle Effects";
                string pgcEffectDesc = "Place Particle Effects and select various colors to decorate experiences.";
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(pgcEffectTitle, pgcEffectDesc, PGCEffectPanel.Instance);
                break;
            case NodeModelType.CrystalStone:
                string crystalTitle = "Ice Gem";
                string crystalDesc = "After adding a Ice Gem, your experience will be selected by Team BUD to become part of Great Snowfield, and will also serve as one of 10 check-in points for BUD Treasure Hunt.";
                PropTipsPanel.Show();
                PropTipsPanel.Instance.SetTipsInfo(crystalTitle, crystalDesc);
                break;
        }
    }

    public void CreatePritiveModel(int id)
    {
        DisableAllPanel();
        var resType = GameConsts.GetResType(id);
        if (uiSelectDic.ContainsKey(resType))
        {
            uiSelectDic[resType]?.Invoke(id);
        }
    }

    public void CloseEditPanel()
    {
        DisableAllPanel();
        BasePrimitivePanel.Hide();
        InputReceiver.locked = true;
    }

    private void OnSelectBornPoint(int id)
    {
        CreateBornPoint(id);
        if (PVPTeamManager.Inst.IsTeamMode() && SpawnPointManager.Inst.spawnList.Count < GameConsts.MAX_PLAYER)
        {
            PVPTeamManager.Inst.UpdateTeamInfo();
        }
    }
    private void CreateBornPoint(int id)
    {
        if(SpawnPointManager.Inst.IsOverMaxCount())
        {
            return;
        }
        var behaviour = OnCreateBySelect<SpawnPointBehaviour>(id, NodeModelType.BornPoint);
        if (behaviour != null)
        {
            SpawnPointCreater.SetData(behaviour as SpawnPointBehaviour, null, SpawnPointCreaterType.EmptyData);
            SetSelectTarget(behaviour.entity);
            SpawnPointManager.Inst.AddSpawnList(behaviour as SpawnPointBehaviour);
            AddCreateRecord(behaviour.gameObject);
            GameManager.Inst.maxPlayer = SpawnPointManager.Inst.spawnList.Count;
        }
    }

    private void OnSelectPrimitiveMode(int id)
    {
        ColorMatData data = GameManager.Inst.colorMatData;
        var behaviour = OnCreateBaseModel(id);
        if (behaviour != null)
        {
            SceneBuilder.Inst.AddColorMatAttribute(behaviour, data);
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
    }

    private void OnSelectPointLight(int id)
    {
        PointLightData data = GameManager.Inst.pointLightData;
        var behaviour = OnCreateBySelect<PointLightBehaviour>(id, NodeModelType.PointLight);
        SceneBuilder.Inst.hideEntityBevs.Add(behaviour);
        if (behaviour != null)
        {
            SceneBuilder.Inst.AddPointLightAttribute(behaviour, data);
            OnSelectBehaviourCreated(behaviour as NodeBaseBehaviour);
        }
    }
    
    private void OnSelectBehaviourCreated(NodeBaseBehaviour behaviour)
    {
        if(behaviour is BaseHLODBehaviour hBehav)hBehav.SetLODStatus(HLODSystem.HLODState.High);
        SetSelectTarget(behaviour.entity);
        AddCreateRecord(behaviour.gameObject);
    }
    private void OnSelectSpotLight(int id)
    {
        SpotLightData data = GameManager.Inst.spotLightData;
        var behaviour = OnCreateBySelect<SpotLightBehaviour>(id, NodeModelType.SpotLight);
        SceneBuilder.Inst.hideEntityBevs.Add(behaviour);
        if (behaviour != null)
        {
            SceneBuilder.Inst.AddSpotLightAttribute(behaviour, data);
            OnSelectBehaviourCreated(behaviour as NodeBaseBehaviour);
        }
    }

    private void OnSelectPortalPoint(int id)
    {
        DisableAllPanel();
        if (IsOverMaxPortalCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }
        int curId = PortalPointManager.Inst.GetNextPid();
        PortalPointData btn_Data = new PortalPointData();
        btn_Data.id = curId;
        var btn_Pos = CameraUtils.Inst.GetCreatePosition();
        var btn_Behav = SceneBuilder.Inst.CreatePrimitive<PortalButtonBehaviour>(0, id, btn_Pos, Vector3.zero, Vector3.one, NodeModelType.PortalButton);
        if (btn_Behav != null)
        {
            SetSelectTarget(btn_Behav.entity);
            SceneBuilder.Inst.AddPortalButtonAttribute(btn_Behav, btn_Data);
            AddCreateRecord(btn_Behav.gameObject);
        }
        

        PortalPointData point_Data = new PortalPointData();
        point_Data.id = curId;
        var point_Pos = new Vector3(btn_Pos.x + 2f, btn_Pos.y, btn_Pos.z);
        var point_Behav = SceneBuilder.Inst.CreatePrimitive<PortalPointBehaviour>(0, (int)GameResType.PortalButton, point_Pos, Vector3.zero, Vector3.one, NodeModelType.PortalPoint);
        if (point_Behav != null)
        {
            SetSelectTarget(point_Behav.entity);
            SceneBuilder.Inst.AddPortalPointAttribute(point_Behav, point_Data);
            AddCreateRecord(point_Behav.gameObject);
        }
    }

    private void OnSelect3DText(int id)
    {
        var data = GameManager.Inst.dTextData;
        var behaviour = OnCreateBySelect<DTextBehaviour>(id, NodeModelType.DText);
        if (behaviour != null)
        {
            SceneBuilder.Inst.AddDTextAttribute(behaviour, data);
            OnSelectBehaviourCreated(behaviour as NodeBaseBehaviour);
        }
    }

    private void OnSelectNew3DText(int id)
    {
        var behaviour = OnCreateBySelect<NewDTextBehaviour>(id, NodeModelType.NewDText);
        if (behaviour != null)
        {
            var pos = CameraUtils.Inst.GetCreatePosition();
            NewDTextCreater.SetDefaultData(behaviour as NewDTextBehaviour, pos);
            OnSelectBehaviourCreated(behaviour as NodeBaseBehaviour);
        }
    }

    private void OnSelectLike(int id)
    {
        var btn_Pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreatePrimitive<LikeButtonBehaviour>(0, id, btn_Pos, Vector3.zero, Vector3.one, NodeModelType.Like);
        SceneBuilder.Inst.likeEntityBevs.Add(behaviour);
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
    }

    private void OnSelectAttention(int id)
    {
        var btn_Pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreatePrimitive<AttentionButtonBehaviour>(0, id, btn_Pos, Vector3.zero, Vector3.one, NodeModelType.Attention);
        SceneBuilder.Inst.attentionEntityBevs.Add(behaviour);
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
    }
    private void OnSelectSound(int id)//选择声音交互按钮
    {
        if (SoundManager.Inst.IsOverMaxSoundCount())
        {
            TipPanel.ShowToast("You can only add up to 10 sound buttons");
            return;
        }
        var btn_Pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreatePrimitive<SoundButtonBehaviour>(0, id, btn_Pos, Vector3.zero, Vector3.one, NodeModelType.Sound);
        SoundManager.Inst.AddSound(behaviour);
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
        SoundPanel.Instance.InitData();
    }
    private void OnSelectFavorite(int id)
    {
        var btn_Pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreatePrimitive<FavoriteButtonBehaviour>(0, id, btn_Pos, Vector3.zero, Vector3.one, NodeModelType.Favorite);
        SceneBuilder.Inst.favoriteEntityBevs.Add(behaviour);
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
    }
    private void OnSelectCanFly(int id)
    {
        DisableAllPanel();
        FlyPermisionPanel.Show();

        PropTipsPanel.Show();
        PropTipsPanel.Instance.SetTipsInfo(FlyPermisionPanel.Instance);
    }

    private void OnSelectSwitch(int id)
    {
        if (SwitchManager.Inst.IsOverMaxSwitchCount())
        {
            TipPanel.ShowToast("Exceed limit:(");
            return;
        }

        var btn_Pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreatePrimitive<SwitchButtonBehaviour>(0, id, btn_Pos, Vector3.zero, Vector3.one, NodeModelType.Switch);
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            ShowHideData data = new ShowHideData();
            data.show = 0;
            SceneBuilder.Inst.AddShowHideAttribute(behaviour, data);

            SwitchButtonData sbData = new SwitchButtonData();
            sbData.sid = SwitchManager.Inst.GetNewSwitchId();
            SceneBuilder.Inst.AddSwitchButtonAttribute(behaviour, sbData);

            AddCreateRecord(behaviour.gameObject);
        }
    }

    private void OnSelectTrapBox(int id)
    {
        if (TrapSpawnManager.Inst.IsOverMaxTrapCount())
        {
            DisableAllPanel();
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }

        int curId = TrapSpawnManager.Inst.GetNextId();
        TrapBoxData trap_Data = new TrapBoxData();
        trap_Data.id = curId;
        var trap_Pos = CameraUtils.Inst.GetCreatePosition();
        var trap_Behav = SceneBuilder.Inst.CreatePrimitive<TrapBoxBehaviour>(0, id, trap_Pos, Vector3.zero, Vector3.one, NodeModelType.TrapBox);
        if (trap_Behav != null)
        {
            SetSelectTarget(trap_Behav.entity);
            SceneBuilder.Inst.AddTrapBoxAttribute(trap_Behav, trap_Data);
            AddCreateRecord(trap_Behav.gameObject);
        }
    }

    private void OnSelectPropStar(int id)
    {
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreatePrimitive<PropStarBehaviour>(0,  id,  pos, Vector3.zero, Vector3.one, NodeModelType.PropStar);
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
    }

    private void OnSelectMusicBoard(int id)
    {
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreatePrimitive<MusicBoardBehaviour>(0, id, pos, Vector3.zero, Vector3.one, NodeModelType.MusicBoard);
        if (behaviour != null)
        { 
            (behaviour as MusicBoardBehaviour).SetColorInit();
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
    }

    private void OnSelectMagneticBoard(int id)
    {
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreatePrimitive<MagneticBoardBehaviour>(0, id, pos, Vector3.zero, Vector3.one, NodeModelType.MagneticBoard);
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
    }
    
    private void OnSelectSteeringWheel(int id)
    {
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreatePrimitive<SteeringWheelBehaviour>(0, id, pos, Vector3.zero, Vector3.one, NodeModelType.SteeringWheel);
        SteeringWheelManager.Inst.AddCar(behaviour.entity.Get<GameObjectComponent>().uid, behaviour.GetComponent<SteeringWheelBehaviour>());
        if (behaviour != null)
        {
            behaviour.entity.Get<SteeringWheelComponent>();

            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
    }
    private void OnSelectWaterCube(int id)
    {
        var behaviour = OnCreateBySelect<WaterCubeBehaviour>(id, NodeModelType.WaterCube);
        if (!behaviour) return;

        WaterData data = new WaterData();
        data.v = 0.05f;
        data.tiling = DataUtils.Vector2ToString(Vector2.one);
        data.id = 0;

        WaterCubeCreater.SetData(behaviour as WaterCubeBehaviour, data);
        SetSelectTarget(behaviour.entity);
        AddCreateRecord(behaviour.gameObject);
    }
    private void OnSelectBounceplank(int id)
    {
        if (BounceplankManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast(BounceplankManager.MAX_COUNT_TIP);
            return;
        }
        var behaviour = OnCreateBySelect<BounceplankBehaviour>(id, NodeModelType.Bounceplank);
        if (!behaviour) return;

        BounceplankData data = new BounceplankData();
        data.s = (int)BounceShape.Round;
        data.h = BounceHeight.M.ToString();
        data.col = DataUtils.ColorToString(AssetLibrary.Inst.colorLib.Get(0));
        data.mat = 0;
        BounceplankCreater.SetData(behaviour as BounceplankBehaviour, data);
        SetSelectTarget(behaviour.entity);
        
        AddCreateRecord(behaviour.gameObject);
    }
    private void OnSelectLadder(int id)
    {
        if (LadderManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast(LadderManager.MAX_COUNT_TIP);
            return;
        }
        var behaviour = OnCreateBySelect<LadderBehaviour>(id, NodeModelType.Ladder);
        if (!behaviour) return;

        LadderData data = new LadderData();

        data.col = DataUtils.ColorToString(AssetLibrary.Inst.colorLib.Get(0));
        data.mat = 3;
        data.tile = DataUtils.Vector2ToString(new Vector2(1,1));
        LadderCreater.SetData(behaviour as LadderBehaviour, data);
        SetSelectTarget(behaviour.entity);

        AddCreateRecord(behaviour.gameObject);
    }
    private void OnSelectBGMusic(int id)
    {
        DisableAllPanel();
        BGMusicPanel.Show();
        BGMusicPanel.Instance.InitData();
        PropTipsPanel.Show();
        PropTipsPanel.Instance.SetTipsInfo(BGMusicPanel.Instance);
    }

    private void OnSelectEnrBGMusic(int id)
    {
        DisableAllPanel();
        BGEnrMusicPanel.Show();

        PropTipsPanel.Show();
        PropTipsPanel.Instance.SetTipsInfo(BGEnrMusicPanel.Instance);
    }

    private void OnSelectDisplayBoard(int id)
    {
        if (DisplayBoardManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }
        DisableAllPanel();
        var behaviour = OnCreateBySelect<DisplayBoardBehaviour>(id, NodeModelType.DisplayBoard);
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
    }

    private void OnSelectPortalGate(int id)
    {
        var behaviour = OnCreateBySelect<PortalGateBehaviour>(id, NodeModelType.PortalGate);
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
    }

    private void OnSelectFireProp(int id)
    {
        FirePropManager manager = FirePropManager.Inst;
        if (manager.IsLimitedCount)
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }

        var behaviour = OnCreateFirePropBySelect();
        if (!behaviour) return;

        SetSelectTarget(behaviour.entity);
        AddCreateRecord(behaviour.gameObject);
    }

    private void OnSelectFishing(int id)
    {
        if (FishingEditManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }
        FishingBehaviour behaviour = OnCreateFishingBySelect();
        if (!behaviour) return;

        SetSelectTarget(behaviour.entity);
        AddCreateRecord(behaviour.gameObject);
    }

    private NodeBaseBehaviour OnCreateWeaponBySelect<T>(WeaponType type) where T :  WeaponBaseManager<T>, new()
    {
        DisableAllPanel();
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = WeaponCreateUtils.Inst.CreateWeaponBeavInEdit<T>(type, pos);
        return behaviour;
    }

    private NodeBaseBehaviour OnCreateBloodPropBySelect()
    {
        DisableAllPanel();
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = BloodPropCreateUtils.Inst.CreateBloodPropBeavInEdit(pos);
        return behaviour;
    }
    //烟花
    private NodeBaseBehaviour OnCreateFireworkBySelect()
    {
        DisableAllPanel();
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = FireworkManager.Inst.CreateFireworkBeavInEdit(pos);
        return behaviour;
    } 
    //冻结道具
    private NodeBaseBehaviour OnCreateFreezeBySelect()
    {
        DisableAllPanel();
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = FreezePropsManager.Inst.CreateBySelected(pos);
        return behaviour;
    }
    //火焰道具
    private NodeBaseBehaviour OnCreateFirePropBySelect()
    {
        DisableAllPanel();
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = FirePropManager.Inst.CreateBySelected(pos);
        SceneBuilder.Inst.allControllerBehaviours.Add(behaviour);
        return behaviour;
    }

    //降落伞
    private NodeBaseBehaviour OnCreateParachuteBySelect()
    {
        DisableAllPanel();
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = ParachuteManager.Inst.CreateParachuteBeavInEdit(pos);
        return behaviour;
    }

    private FishingBehaviour OnCreateFishingBySelect()
    {
        DisableAllPanel();
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = FishingEditManager.Inst.CreateFishingNode(pos);
        SceneBuilder.Inst.allControllerBehaviours.Add(behaviour);
        return behaviour;
    }

    private TransferBehaviour OnCreateDowntownTransferBySelect()
    {
        DisableAllPanel();
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = DowntownTransferManager.Inst.CreateDowntownTransfer(pos);
        SceneBuilder.Inst.allControllerBehaviours.Add(behaviour);
        return behaviour;
    }

    private NodeBaseBehaviour OnCreateBySelect<T>(int id, NodeModelType modeType) where T : NodeBaseBehaviour
    {
        DisableAllPanel();
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreatePrimitive<T>(0, id, pos, Vector3.zero, Vector3.one, modeType);
        return behaviour;
    }

    private NodeBaseBehaviour OnCreateBaseModel(int id) 
    {
        DisableAllPanel();
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreateSceneNode<BaseModelCreater, NodeBehaviour>();
        BaseModelCreater.SetData(behaviour, new NodeData()
        {
            uid = 0,
            id = id,
            p =  DataUtils.Vector3ToString(pos),
            r =  DataUtils.Vector3ToString(Vector3.zero),
            s =  DataUtils.Vector3ToString(Vector3.one),
            type = (int)NodeModelType.BaseModel,
        } ); 
        SceneBuilder.Inst.allControllerBehaviours.Add(behaviour);
        return behaviour;
    }
public NodeBaseBehaviour OnCreatePGCPLant()
    {
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreateSceneNode<PGCPlantCreater, PGCPlantBehaviour>();
        var lastChooseID = PGCPlantManager.Inst.GetLastChooseID();
        var handleData = GameManager.Inst.PGCPlantDatasDic[lastChooseID];
        PGCPlantCreater.SetData(behaviour, new NodeData()
        {
            uid = 0,
            id = lastChooseID,
            p = DataUtils.Vector3ToString(pos),
            r = DataUtils.Vector3ToString(Vector3.zero),
            s = DataUtils.Vector3ToString(Vector3.one),
            type = handleData.handleType,
        }, pos);
        SceneBuilder.Inst.allControllerBehaviours.Add(behaviour);
        return behaviour;
    }

    public NodeBaseBehaviour OnCreatePGCEffect()
    {
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreateSceneNode<PGCEffectCreater, PGCEffectBehaviour>();
        var defChooseID = PGCEffectManager.Inst.defChooseId;
        // var handleData = GameManager.Inst.PGCEffectDatasDic[defChooseID];
        PGCEffectCreater.SetData(behaviour, new NodeData()
        {
            uid = 0,
            id = defChooseID,
            p = DataUtils.Vector3ToString(pos),
            r = DataUtils.Vector3ToString(Vector3.zero),
            s = DataUtils.Vector3ToString(Vector3.one),
            // type = handleData.handleType,
        }, pos);
        SceneBuilder.Inst.allControllerBehaviours.Add(behaviour);
        return behaviour;
    }

    private void OnSelectGround(int id)
    {
        DisableAllPanel();
        var com = SceneBuilder.Inst.TerrainEntity.Get<TerrainComponent>();
      
        if (string.IsNullOrEmpty(com.umatUrl) && string.IsNullOrEmpty(com.umapId))
        {
            int matId = com.matId;
            var matData = GameManager.Inst.allTerrainConfigDatas.Find(x => x.id == matId);
            int selectIndex = -1;
            for (int i = 0; i < GameManager.Inst.terrainConfigDatas.Count; i++)
            {
                if (GameManager.Inst.terrainConfigDatas[i].id == matData.id)
                {
                    selectIndex = i;
                }
            }
            TerrainMaterialPanel.Show();
            TerrainMaterialPanel.Instance.SetMatSelect(selectIndex);
        }
        else
        {
            
            TerrainMaterialPanel.Show();
            TerrainMaterialPanel.Instance.SetUGCMatSelect(com.umapId);

        }
       
       
        PropTipsPanel.Show();
        PropTipsPanel.Instance.SetTipsInfo(TerrainMaterialPanel.Instance);
    }

    private void OnSelectSkybox(int id)
    {
        DisableAllPanel();
        int skyboxId = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().skyboxId;
        SkyboxStylePanel.Show();
        SkyboxStylePanel.Instance.HighLight(skyboxId);
        PropTipsPanel.Show();
        PropTipsPanel.Instance.SetTipsInfo(SkyboxStylePanel.Instance);
    }
   
    private void OnSelectDirLight(int id)
    {
        DisableAllPanel();
        DirLightPanel.Show();
        int skyboxId = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().skyboxId;
        var data = SceneBuilder.Inst.DirLight.entity.Get<DirLightComponent>();
        DirLightPanel.Instance.SetSkyID(skyboxId, data.intensity, data.anglex, data.angley);
        PropTipsPanel.Show();
        PropTipsPanel.Instance.SetTipsInfo(DirLightPanel.Instance);
    }

    private void OnSelectShotPhoto(int id)
    {
        if (ShotPhotoManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }
        var behaviour = OnCreateBySelect<ShotPhotoBehaviour>(id, NodeModelType.ShotPhoto);
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            ShotPhotoManager.Inst.AddPhoto(behaviour as ShotPhotoBehaviour);
            AddCreateRecord(behaviour.gameObject);
        }
    }

    private void OnSelectVideo(int id)
    {
        if (VideoNodeManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast(VideoNodeManager.Inst.MAX_NUM_TIP, VideoNodeManager.MAX_COUNT);
            return;
        }
        var behaviour = OnCreateBySelect<VideoNodeBehaviour>(id, NodeModelType.Video);
        if (!behaviour) return;
        
        VideoNodeData data = new VideoNodeData
        {
            vUrl = string.Empty,
            sRange = (int)VideoSoundRange.Near
        };
        VideoNodeCreater.SetData(behaviour as VideoNodeBehaviour, data);
        SetSelectTarget(behaviour.entity);
        AddCreateRecord(behaviour.gameObject);
    }
    
    private void OnSelectSensorBox(int id)
    {
        if (SensorBoxManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }
        var behaviour = OnCreateBySelect<SensorBoxBehaviour>(id, NodeModelType.SensorBox);
        if (!behaviour) return;
        
        SetSelectTarget(behaviour.entity);
        SensorBoxData sbData = new SensorBoxData();
        sbData.index = SensorBoxManager.Inst.GetNewIndex();
        SensorBoxCreater.SetData(behaviour as SensorBoxBehaviour, sbData);
        AddCreateRecord(behaviour.gameObject);
    }

    private void OnSelectPVPWaitArea(int id)
    {
        DisableAllPanel();

        if (PVPWaitAreaManager.Inst.PVPBehaviour)
        {
            var entity = PVPWaitAreaManager.Inst.PVPBehaviour.entity;
            if (LockHideManager.Inst.hideList.Contains(entity))
            {
                return;
            }
            PVPWaitAreaManager.Inst.PVPBehaviour.gameObject.SetActive(true);
        }
        else
        {
            var pos = CameraUtils.Inst.GetCreatePosition();
            SceneBuilder.Inst.CreatePVPBehaviour();
            PVPWaitAreaCreater.SetDefaultData(PVPWaitAreaManager.Inst.PVPBehaviour, pos);
            AddCreateRecord(PVPWaitAreaManager.Inst.PVPBehaviour.gameObject);

        }
        SetSelectTarget(PVPWaitAreaManager.Inst.PVPBehaviour.entity);
    }

    private void OnSelectAttackWeapon(int id)
    {
        if (!PickabilityManager.Inst.CheckCanSetPickability())
        {
            TipPanel.ShowToast(PickabilityManager.MAX_COUNT_TIP);
            return;
        }
        
        var behaviour = OnCreateWeaponBySelect<AttackWeaponManager>(WeaponType.Attack);
        if (!behaviour) return;

        SetSelectTarget(behaviour.entity);
        AddCreateRecord(behaviour.gameObject);
    }

    private void OnSelectUgcCloth(int id)
    {
        if (UgcClothItemManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast(UgcClothItemManager.MAX_COUNT_TIP);
            return;
        }
        var behaviour = OnCreateBySelect<UgcClothItemBehaviour>(id, NodeModelType.UgcCloth);
        if (behaviour != null)
        {
            UgcClothItemManager.Inst.ResetUgcClothItem(behaviour);
            SetSelectTarget(behaviour.entity);
            UgcClothItemManager.Inst.AddUgcClothItem((UgcClothItemBehaviour) behaviour);
            AddCreateRecord(behaviour.gameObject);
        }
	}

    private void OnSelectShootWeapon(int id)
    {
        if (!PickabilityManager.Inst.CheckCanSetPickability())
        {
            TipPanel.ShowToast(PickabilityManager.MAX_COUNT_TIP);
            return;
        }

        var behaviour = OnCreateWeaponBySelect<ShootWeaponManager>(WeaponType.Shoot);
        if (!behaviour) return;

        SetSelectTarget(behaviour.entity);
        AddCreateRecord(behaviour.gameObject);
    }

    private void OnSelectBloodProp(int id)
    {
        if (BloodPropManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }
        var behaviour = OnCreateBloodPropBySelect();
        if (!behaviour) return;

        SetSelectTarget(behaviour.entity);
        AddCreateRecord(behaviour.gameObject);
    }
    private void OnSelectPGCPlant(int id)
    {
        if (PGCPlantManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }
        DisableAllPanel();
        var behaviour = OnCreatePGCPLant();
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
            PGCPlantManager.Inst.AddItem(behaviour);
        }
    }

    private void OnSelectPGCEffect(int id)
    {
        if (PGCEffectManager.Inst.IsOverMaxCount())
        {
            return;
        }
        DisableAllPanel();
        var behaviour = OnCreatePGCEffect();
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
            PGCEffectManager.Inst.AddItem(behaviour);
        }
    }

    //烟花
    private void OnSelectFirework(int id)
    {
        if (FireworkManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }
        if (!PickabilityManager.Inst.CheckCanSetPickability())
        {
            TipPanel.ShowToast(PickabilityManager.MAX_COUNT_TIP);
            return;
        }
        var behaviour = OnCreateFireworkBySelect();
        if (behaviour != null)
        {
            SetSelectTarget(behaviour.entity);
            AddCreateRecord(behaviour.gameObject);
        }
    }
    private void OnSelectFreezePprops(int id)
    {
        FreezePropsManager manager = FreezePropsManager.Inst;
        if (manager.IsLimitedCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }

        var behaviour = OnCreateFreezeBySelect();
        if (!behaviour) return;

        SetSelectTarget(behaviour.entity);
        AddCreateRecord(behaviour.gameObject);       
    }
    private void OnSelectPostProcess(int id)
    {
        DisableAllPanel();
        PostProcessingPanel.Show();
        PostProcessingPanel.Instance.SetEntity(SceneBuilder.Inst.PostProcessBehaviour.entity);

        PropTipsPanel.Show();
        PropTipsPanel.Instance.SetTipsInfo(PostProcessingPanel.Instance);
    }

    private void OnSelectLeaderBoard(int id)
    {
        DisableAllPanel();
        if (LeaderBoardManager.Inst.IsOverMaxBoardCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }
        LeaderBoardPanel.Show();
        var behaviour = OnCreateBySelect<LeaderBoardBehaviour>(id,NodeModelType.LeaderBoard);
        if (!behaviour) return;

        SetSelectTarget(behaviour.entity);
        LeaderBoardData lbData = new LeaderBoardData();
        lbData.curMode = (int)LeaderBoardModeType.None;
        LeaderBoardCreater.SetData(behaviour as LeaderBoardBehaviour, lbData);
        AddCreateRecord(behaviour.gameObject);
    }

    public virtual void DisableAllPanel()
    {
        SeparatePartEditPanel.CloseNotByUser();
        gController.DisableGizmo();
        SkyboxStylePanel.Hide();
        TerrainMaterialPanel.Hide();
        ModelHandlePanel.Hide();
        BaseMaterialPanel.Hide();
        SkyboxStylePanel.Hide();
        DirLightPanel.Hide();
        PointLightPanel.Hide();
        SpotLightPanel.Hide();
        PropTipsPanel.Hide();
        PropLittleTipsPanel.Hide();
        ResStorePanel.Hide();
        DTextPanel.Hide();
        BGMusicPanel.Hide();
        PortalGatePanel.Hide();
        FlyPermisionPanel.Hide();
        MusicBoardPanel.Hide();
        BGEnrMusicPanel.Hide();
        TrapBoxPanel.Hide();
        DisplayBoardPanel.Hide();
        PostProcessingPanel.Hide();
        SoundPanel.Hide();
        ShotPhotoPanel.Hide();
        ReferPanel.Hide();
        PVPWaitAreaPanel.Hide();
        SensorBoxPanel.Hide();
        LeaderBoardPanel.Hide();
        if (ReferManager.Inst.isRefer)
        {
            ReferManager.Inst.isHafeRefer = true;
        }

		VideoNodePanel.Hide();
        SpawnPointPanel.Hide();
        AttackWeaponPanel.Hide();
        AttackWeaponCtrlPanel.Hide();
        UgcClothItemPanel.Hide();
        ShootWeaponPanel.Hide();
        ShootWeaponCtrlPanel.Hide();
        BloodPropPanel.Hide();
        FPSPlayerHpPanel.Hide();
        NewDTextPanel.Hide();
        WaterMaterialPanel.Hide();
        IceCubePanel.Hide();
        BounceplankPanel.Hide();
        LadderPanel.Hide();
        FireworkPanel.Hide();
        PGCPlantPanel.Hide();
        EatOrDrinkCtrPanel.Hide();
        ParachutePanel.Hide();
        ParachuteCtrlPanel.Hide();
        WeatherSelectPanel.Hide();
        FreezePropsPanel.Hide();
        BaseMatColorPanel.Hide();
        FirePropPanel.Hide();
        SnowCubePanel.Hide();
        SeesawPanel.Hide();
        FishingRodPanel.Hide();
        FishingCtrPanel.Hide();
        SlidePipePanel.Hide();
        VIPZonePanel.Hide();
        FlashLightPanel.Hide();
        SwingPanel.Hide();
        PGCEffectPanel.Hide();
    }


    private void ShowModelHandlePanel(NodeHandleType modelType, SceneEntity entity = null)
    {
        ModelHandlePanel.Show();
        ModelHandlePanel.Instance.SetGizmoController(gController);
        ModelHandlePanel.Instance.EnterMode(modelType);
        ModelHandlePanel.Instance.ShowUploadBtn(entity);
        ModelHandlePanel.Instance.WeaponEnterMode(entity);
        ModelHandlePanel.Instance.ParachuteEnterMode(entity);
        ModelHandlePanel.Instance.OnDestroyNode = DestroyCurNode;
        ModelHandlePanel.Instance.OnSelectTarget = SetSelectTarget;
        ModelHandlePanel.Instance.OnUnSelectAllTarget = UnSelectAllTarget;

    }

    public void SetPlayerState(PlayerBaseControl playerCom)
    {
        PVPWaitAreaManager.Inst.SetMeshAndBoxVisible(true,false);
        MessageHelper.Broadcast(MessageName.ChangeMode, GameMode.Edit);
        playerCom.gameObject.SetActive(false);
        playerCom.playerAnim.gameObject.SetActive(false);
        //进入编辑模式退出状态显示
        PlayerManager.Inst.ExitShowPlayerState();
    }

    private void DestroyCurNode()
    {
        var curTarget = gController.GetCurrentTarget();
        if (curTarget != null)
        {
            gController.DisableGizmo();
            MovePathManager.Inst.ReleaseAllPoints();
            BasePrimitivePanel.DisSelect();

            // SceneBuilder.Inst.DestroyEntity(curTarget);
            AddDestroyRecord(curTarget);
            SecondCachePool.Inst.DestroyEntity(curTarget);
            // SceneBuilder.Inst.DestroyEntity(curTarget);   
        }
        DisableAllPanel();
    }

    private bool IgnoreCantGameobject(NodeBaseBehaviour nBehaviour)
    {
        var entity = nBehaviour.entity;
        return entity.HasComponent<IgnoreCantSelectComponent>();
    }

    private bool IsOverMaxPortalCount()
    {
        int curBtnCount = PortalPointManager.Inst.GetCurMax();
        if (curBtnCount >= PortalPointManager.MaxCount)
        {
            LoggerUtils.Log("Max 99 no more");
            return true;
        }
        return false;
    }
    
    

    private void OnSelectIceCube(int id)
    {
        if (IceCubeManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast(IceCubeManager.MAX_COUNT_TIP);
            return;
        }
        var behaviour = OnCreateBySelect<IceCubeBehaviour>(id, NodeModelType.IceCube);
        if (behaviour != null)
        {
            IceCubeData data = new IceCubeData();
            data.tile = DataUtils.Vector2ToString(Vector2.one);
            IceCubeCreater.SetData(behaviour as IceCubeBehaviour, data);
            
            behaviour.transform.localScale = new Vector3(1, 0.1f, 1);
            SetSelectTarget(behaviour.entity);
            IceCubeManager.Inst.AddItem(behaviour as IceCubeBehaviour);
            AddCreateRecord(behaviour.gameObject);
        }
    }
    
    private void OnSelectSnowCube(int id)
    {
        if (SnowCubeManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast(SnowCubeManager.MAX_COUNT_TIP);
            return;
        }
        var behaviour = OnCreateBySelect<SnowCubeBehaviour>(id, NodeModelType.SnowCube);
        if (behaviour != null)
        {
            SnowCubeData data = new SnowCubeData();
            data.s = (int)SnowShape.Cube;
            data.col = DataUtils.ColorToString(AssetLibrary.Inst.colorLib.Get(0));
            data.tile = DataUtils.Vector2ToString(Vector2.one);
            SnowCubeCreater.SetData(behaviour as SnowCubeBehaviour, data);
            behaviour.transform.localScale = new Vector3(1, 0.1f, 1);
            SetSelectTarget(behaviour.entity);
            SnowCubeManager.Inst.AddItem(behaviour as SnowCubeBehaviour);
            AddCreateRecord(behaviour.gameObject);
        }
    }

    private void OnSelectParachute(int id)
    {
        if (ParachuteManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast(ParachuteManager.MAX_COUNT_TIP);
            return;
        }
        ParachuteManager.Inst.HideAllBagActive();
        var paraBehav = OnCreateParachuteBySelect();
        if (paraBehav != null)
        {
            SetSelectTarget(paraBehav.entity);
        }
    }

    private void OnSelectSeeSaw(int id)
    {
        GameObject go = SeesawManager.Inst.CreateSeeSaw();
        if (go != null)
        {
            AddCreateRecord(go);
        }
    }

    private void OnSelectVIPZone(int id)
    {
        GameObject go = VIPZoneManager.Inst.CreateVIPZoneInEdit();
        if (go != null)
        {
            AddCreateRecord(go);
        }
    }
    private void OnSelectSlidePipe(int id)
    {   
        if (SlidePipeManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast(SlidePipeManager.MAX_COUNT_TIP);
            return;
        }
        
        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreateSceneNode<SlidePipeCreater, SlidePipeBehaviour>();
        
        if (!behaviour) return;
       
        NodeData nodeData = new NodeData(){
            uid = 0,
            id = id,
        };
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = Vector3.zero;
        behaviour.transform.localScale = Vector3.one;

        SlidePipeData slideData = new SlidePipeData();
        SlidePipeCreater.SetData(behaviour as SlidePipeBehaviour, slideData,nodeData);
        AddCreateRecord(behaviour.gameObject);

        //默认创建一节滑梯
        var itemBehaviour = SlidePipeManager.Inst.CreateSlideItem(behaviour,true);
        SetSelectTarget(behaviour.entity);
    }
    
    private void OnSelectWeather(int obj)
    {
        DisableAllPanel();
        WeatherSelectPanel.Show();
        WeatherSelectPanel.Instance.OnCloseClick = UnSelectAllTarget;

        PropTipsPanel.Show();
        PropTipsPanel.Instance.SetTipsInfo(WeatherSelectPanel.Instance);
    }
    
    private void OnSelectSwing(int id)
    {
        if (SwingManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }

        var pos = CameraUtils.Inst.GetCreatePosition();
        var behaviour = SceneBuilder.Inst.CreatePrimitive<SwingBehaviour>(0, id, pos, Vector3.zero, Vector3.one, NodeModelType.Swing);
        if (behaviour != null)
        {
            AddCreateRecord(behaviour.gameObject);
            SetSelectTarget(behaviour.entity);
        }
    }

    private void OnSelectFlashLight(int id)
    {
        DisableAllPanel();

        Vector3 pos = CameraUtils.Inst.GetCreatePosition();
        NodeBaseBehaviour behav = FlashLightManager.Inst.CreateBySelected(pos);
        if (!behav) return;

        SetSelectTarget(behav.entity);
        AddCreateRecord(behav.gameObject);
    }

    private void OnSelectCrystalStone(int id)
    {
        var behav = CrystalStoneManager.Inst.GetCrystalNode();
        if (behav) //若已存在宝石, 则直接选中宝石
        {
            if (LockHideManager.Inst.hideList.Contains(behav.entity))
            {
                return;
            }
            behav.gameObject.SetActive(true);
        }
        else //否则走创建流程
        {
            behav = OnCreateBySelect<CrystalStoneBehaviour>(id, NodeModelType.CrystalStone);
            if (behav == null)
            {
                return;
            }
            AddCreateRecord(behav.gameObject);
            CrystalStoneManager.Inst.AddCrystalNode(behav);
        }
        SetSelectTarget(behav.entity);
	}

    private void OnSelectDowntownTransfer(int id)
    {
        TransferBehaviour behaviour = OnCreateDowntownTransferBySelect();
        if (!behaviour) return;

        SetSelectTarget(behaviour.entity);
        AddCreateRecord(behaviour.gameObject);
    }

    //隐藏所有附属道具
    public void HideAccessories()
    {
        ParachuteManager.Inst.HideAllBagActive();
    }
}

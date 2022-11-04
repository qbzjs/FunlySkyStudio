using System;
using System.Collections.Generic;
using System.Linq;
using RTG;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ModelHandlePanel : BasePanel<ModelHandlePanel>
{
    public Button MoveBtn;
    public Button RotateBtn;
    public Button ScaleBtn;
    public Button CloneBtn;
    public Button UnCombineBtn;
    public Button DesBtn;
    public Button UploadBtn;
    public Button HideBtn;
    public Toggle LockTog;

    public Button AddXBtn;
    public Button SubXBtn;
    public Button AddYBtn;
    public Button SubYBtn;
    public Button AddZBtn;
    public Button SubZBtn;

    public Button NorAddBtn;
    public Button NorSubBtn;

    public Toggle ScaleToggle;

    public GameObject AxisGo;
    public GameObject ExtraGo;
    public GameObject StepPanel;
    public GameObject XAxis;
    public GameObject YAxis;
    public GameObject ZAxis;
    public GameObject ScaleLabel;

    public CoToggleGroup CoGroup;

    public GameObject Mask;

    [SerializeField] private Text rotInputText;
    [SerializeField] private GameObject rotGo;

    private float moveDefSnap = 0.1f;
    private float scaleDefSnap = 0.1f;

    private float fixRotDefSnap = 45;
    private float fixScaleDefSnap = 0.1f;

    private bool fixScaEnable = false;

    //SpcialModel
    private bool isSpecialScale = false;
    private bool isHideSpecialScale = false;
    private float minScaleVal = 0.01f;
    private CoToggle PosCoToggle;
    private CoToggle RotCoToggle;
    private CoToggle ScaCoToggle;

    private NodeHandleType modelMode = NodeHandleType.Base;
    private HandleMode handleMode = HandleMode.Move;
    private GizmoController gController;
    private Vector3 targetLastScale;
    public Action OnDestroyNode;
    public Action<SceneEntity> OnSelectTarget;
    public Action OnUnSelectAllTarget;
    
    private int rotMin = 1;
    private int rotMax = 360;

    private enum Axis_Type
    {
        XYZ,
        Y,
        XZ,
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        PosCoToggle = MoveBtn.GetComponent<CoToggle>();
        RotCoToggle = RotateBtn.GetComponent<CoToggle>();
        ScaCoToggle = ScaleBtn.GetComponent<CoToggle>();
        MoveBtn.onClick.AddListener(OnMoveClick);
        RotateBtn.onClick.AddListener(OnRotateClick);
        ScaleBtn.onClick.AddListener(OnScaleClick);
        DesBtn.onClick.AddListener(OnDestoryClick);
        UploadBtn.onClick.AddListener(OnUploadClick);
        HideBtn.onClick.AddListener(OnHideBtnClick);
        LockTog.onValueChanged.AddListener(OnLockTogChange);
        AddXBtn.onClick.AddListener(() => OnChangeXYZClick(1, 0));
        SubXBtn.onClick.AddListener(() => OnChangeXYZClick(-1, 0));
        AddYBtn.onClick.AddListener(() => OnChangeXYZClick(1, 1));
        SubYBtn.onClick.AddListener(() => OnChangeXYZClick(-1, 1));
        AddZBtn.onClick.AddListener(() => OnChangeXYZClick(1, 2));
        SubZBtn.onClick.AddListener(() => OnChangeXYZClick(-1, 2));
        NorAddBtn.onClick.AddListener(OnNorAddClick);
        NorSubBtn.onClick.AddListener(OnNorSubClick);
        ScaleToggle.onValueChanged.AddListener(OnScaToggleClick);
        UnCombineBtn.onClick.AddListener(OnUnCombineClick);
        CloneBtn.onClick.AddListener(OnCloneClick);
        
        var trigger = rotInputText.GetComponent<EventTrigger>();
        EventTrigger.Entry onSelect = new EventTrigger.Entry();
        onSelect.eventID = EventTriggerType.PointerClick;
        onSelect.callback.AddListener(Select);
        trigger.triggers.Add(onSelect);

        if (PropEditModePanel.Instance)
        {
            EnterSceneType(SCENE_TYPE.ResMAP_SCENE);
        }
        else if (GameEditModePanel.Instance) {
            EnterSceneType(GlobalFieldController.CurSceneType);
        }
#if !UNITY_EDITOR
        MobileInterface.Instance.AddClientRespose(MobileInterface.uploadResource, OnUploadResource);
#endif

        UploadBtn.gameObject.SetActive(false);
    }

    private void Select(BaseEventData data)
    {
        string str = rotInputText.text;
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = "",
            inputMode = 1,
            maxLength = 3,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = str,
            returnKeyType = (int)ReturnType.Return
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowKeyBoard);
        LoggerUtils.Log("JsonUtility.ToJson(keyBoardInfo)==="+ JsonUtility.ToJson(keyBoardInfo));
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }
    
    private void ShowKeyBoard(string str)
    {
        if (string.IsNullOrEmpty(str)) return;
        int v;
        if (int.TryParse(str, out v))
        {
            if (v >= rotMin && v <= rotMax)
            {
                rotInputText.text = v.ToString();
                fixRotDefSnap = v;
            }
            else
            {
                TipPanel.ShowToast("Please enter the correct value");
            }
        }
        else
        {
            TipPanel.ShowToast("Please enter the correct value");
        }
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        OnReset();
    }

    private void OnReset()
    {
        isSpecialScale = false;
        isHideSpecialScale = false;
        fixScaEnable = false;
        AxisGo.SetActive(true);
        StepPanel.SetActive(true);
        ExtraGo.SetActive(false);
        CloneBtn.gameObject.SetActive(true);
        MoveBtn.gameObject.SetActive(true);
        RotateBtn.gameObject.SetActive(true);
        ScaleBtn.gameObject.SetActive(true);
        DesBtn.gameObject.SetActive(true);
        HideBtn.gameObject.SetActive(true);
        LockTog.gameObject.SetActive(true);
        UnCombineBtn.gameObject.SetActive(false);
        GetCurLockState();
    }

    public void EnterSceneType(SCENE_TYPE type)
    {
        switch (type) {
            case SCENE_TYPE.MAP_SCENE:
            case SCENE_TYPE.MYSPACE_SCENE:
                UploadBtn.gameObject.SetActive(true);
                break;
            case SCENE_TYPE.ResMAP_SCENE:
                UploadBtn.gameObject.SetActive(false);
                UploadBtn.onClick.RemoveAllListeners();
                break;
        }
    }

    public void WeaponEnterMode(SceneEntity entity)
    {
        if (entity == null) return;
        var weaponType = WeaponSystemController.Inst.GetWeaponTypeInEntity(entity);
        switch (weaponType)
        {
            case WeaponType.Attack:
                SetBtnActiveBySceneType(false);
                break;
            case WeaponType.Shoot:
                SetBtnActiveBySceneType(false);
                break;
            default: 
                break;
        }
    }

    public void ParachuteEnterMode(SceneEntity entity)
    {
        if (entity == null) return;
        var ugcType = ParachuteManager.Inst.GetParachuteType(entity);
        switch (ugcType)
        {
            case ParaUgcType.Parachute:
                isSpecialScale = true;
                UploadBtn.gameObject.SetActive(false);
                break;
            case ParaUgcType.Bag:
                isSpecialScale = true;
                CloneBtn.gameObject.SetActive(false);
                HideBtn.gameObject.SetActive(false);
                LockTog.gameObject.SetActive(false);
                UploadBtn.gameObject.SetActive(false);
                DesBtn.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    public void EnterMode(NodeHandleType mode)
    {
        OnReset();
        modelMode = mode;
        switch (mode)
        {
            case NodeHandleType.Base:
                SetBtnActiveBySceneType(true);
                break;
            case NodeHandleType.Born:
                ScaleBtn.gameObject.SetActive(false);
                StepPanel.gameObject.SetActive(true);
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.DowntownTransfer:
                ScaleBtn.gameObject.SetActive(false);
                RotateBtn.gameObject.SetActive(false);
                StepPanel.gameObject.SetActive(true);
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.PointLight:
                CloneBtn.gameObject.SetActive(true);
                ScaleBtn.gameObject.SetActive(false);
                RotateBtn.gameObject.SetActive(false);
                DesBtn.gameObject.SetActive(true);
                StepPanel.gameObject.SetActive(true);
                HideBtn.gameObject.SetActive(false);
                LockTog.gameObject.SetActive(false);
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.SpotLight:
                CloneBtn.gameObject.SetActive(true);
                RotateBtn.gameObject.SetActive(true);
                ScaleBtn.gameObject.SetActive(false);
                DesBtn.gameObject.SetActive(true);
                StepPanel.gameObject.SetActive(true);
                HideBtn.gameObject.SetActive(false);
                LockTog.gameObject.SetActive(false);
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.Special:
                isSpecialScale = true;
                SetBtnActiveBySceneType(true);
                break;
            case NodeHandleType.Combine:
                CloneBtn.gameObject.SetActive(true);
                ScaleBtn.gameObject.SetActive(true);
                DesBtn.gameObject.SetActive(true);
                StepPanel.gameObject.SetActive(true);
                UnCombineBtn.gameObject.SetActive(true);
                isSpecialScale = IsContainSpecialModel();
                SetBtnActiveBySceneType(true);
                break;
            case NodeHandleType.SeesawCombine:
                UploadBtn.gameObject.SetActive(false);
                isSpecialScale = true;
                break;
            case NodeHandleType.Swing:
                UploadBtn.gameObject.SetActive(false);
                isSpecialScale = true;
                break;
            case NodeHandleType.SpecialCombine:
                isSpecialScale = true;
                UnCombineBtn.gameObject.SetActive(false);
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.RotAxisY:
                //CloneBtn.gameObject.SetActive(false);
                ScaleBtn.gameObject.SetActive(false);
                StepPanel.gameObject.SetActive(true);
                HideBtn.gameObject.SetActive(false);
                LockTog.gameObject.SetActive(false);
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.NudeMod:
                RotateBtn.gameObject.SetActive(false);
                CloneBtn.gameObject.SetActive(false);
                ScaleBtn.gameObject.SetActive(false);
                DesBtn.gameObject.SetActive(false);
                StepPanel.gameObject.SetActive(false);
                HideBtn.gameObject.SetActive(false);
                LockTog.gameObject.SetActive(false);
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.TrapSpawn:
                CloneBtn.gameObject.SetActive(false);
                ScaleBtn.gameObject.SetActive(false);
                StepPanel.gameObject.SetActive(false);
                HideBtn.gameObject.SetActive(false);
                LockTog.gameObject.SetActive(false);
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.MagneticBoard:
                ScaleBtn.gameObject.SetActive(false);
                isSpecialScale = true;
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.WaterCube:
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.PGC:
                isSpecialScale = true;
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.SteeringWheel:
                isSpecialScale = true;
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.Video:
                isSpecialScale = true;
                SetBtnActiveBySceneType(false);
                //CloneBtn.gameObject.SetActive(false);  1.28可复制
                break;
            case NodeHandleType.PVP:
                CloneBtn.gameObject.SetActive(false);
                ScaleBtn.gameObject.SetActive(true);
                DesBtn.gameObject.SetActive(true);
                StepPanel.gameObject.SetActive(true);
                HideBtn.gameObject.SetActive(true);
                LockTog.gameObject.SetActive(true);
                UploadBtn.gameObject.SetActive(false);
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.BloodRestore:
                isSpecialScale = true;
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.IceCube:
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.Firework:
                isSpecialScale = true;
                UploadBtn.gameObject.SetActive(false);
                break;
            case NodeHandleType.Ladder:
                isHideSpecialScale = true;
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.Bounceplank:

                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.FreezeProps:
            	isSpecialScale = true;
                SetBtnActiveBySceneType(false);
                break;
            case NodeHandleType.SlidePipe:
                SetBtnActiveBySceneType(false);
                ScaleBtn.gameObject.SetActive(false);
                UploadBtn.gameObject.SetActive(false);
                break;
            case NodeHandleType.VIPZone:
                isSpecialScale = true;
                break;
            case NodeHandleType.CrystalStone:
                ScaleBtn.gameObject.SetActive(false); //1.49.0 暂不支持缩放
                CloneBtn.gameObject.SetActive(false);
                UploadBtn.gameObject.SetActive(false);
                break;
        }
        handleMode = HandleMode.Move;
        OnMoveClick();
    }
    
    private void SetStepPanel(NodeHandleType handleType, HandleMode handleMode)
    {
        switch (handleMode)
        {
            case HandleMode.Rotate:
                SetStepAxisByHandleType(handleType);
                break;
            case HandleMode.Scale:
                SetStepAxisByHandleType(handleType);
                break;
            default:
                SetStepPanelAxis(Axis_Type.XYZ);
                break;
        }
    }

    private void SetStepPanelAxis(Axis_Type mode) {
       
        switch (mode)
        {
            case Axis_Type.XYZ:
                XAxis.SetActive(true);
                YAxis.SetActive(true);
                ZAxis.SetActive(true);
                break;
            case Axis_Type.Y:
                XAxis.SetActive(false);
                YAxis.SetActive(true);
                ZAxis.SetActive(false);
                break;
            case Axis_Type.XZ:
                XAxis.SetActive(true);
                YAxis.SetActive(false);
                ZAxis.SetActive(true);
                break;
        }
    }

    private void SetStepAxisByHandleType(NodeHandleType handleType)
    {
        switch (handleType)
        {
            case NodeHandleType.VIPZone:
            case NodeHandleType.SeesawCombine:
            case NodeHandleType.RotAxisY:
            case NodeHandleType.Born:
            case NodeHandleType.MagneticBoard:
            case NodeHandleType.SteeringWheel:
            case NodeHandleType.CrystalStone:
                SetStepPanelAxis(Axis_Type.Y);
                break;
            case NodeHandleType.Ladder:
                if (handleMode == HandleMode.Rotate)
                {
                    SetStepPanelAxis(Axis_Type.XYZ);
                }
                else
                {
                    SetStepPanelAxis(Axis_Type.Y);
                }
                break;
            case NodeHandleType.WaterCube:
            case NodeHandleType.PVP:
                if(handleMode == HandleMode.Rotate)
                {
                    SetStepPanelAxis(Axis_Type.Y);
                }
                else
                {
                    SetStepPanelAxis(Axis_Type.XYZ);
                }
                break;
            case NodeHandleType.Bounceplank:
                if (handleMode == HandleMode.Rotate)
                {
                    SetStepPanelAxis(Axis_Type.XYZ);
                }
                else
                {
                    SetStepPanelAxis(Axis_Type.XZ);
                }
             
                break;
            default:
                SetStepPanelAxis(Axis_Type.XYZ);
                break;
        }
    }

    private void OnUploadClick()
    {
        var entity = gController.GetCurrentTarget().GetComponent<NodeBaseBehaviour>().entity;
        GameObject target = entity.Get<GameObjectComponent>().bindGo;
        var comps = target.GetComponentsInChildren<Transform>();
        var content = SceneParser.Inst.UploadToStore(entity);
        var entityList = SceneParser.Inst.GetAllEntity(entity);
        var highestResIds = SceneParser.Inst.GetMinimumVersion(entityList);
        if (string.IsNullOrEmpty(content))
        {
            TipPanel.ShowToast("Only building primitives can be uploaded as props.");
            return;
        }
        var fileName = ZipUtils.SavePropZipJson(content);
        GlobalFieldController.isScreenShoting = true;
        Mask.SetActive(true);
        gController.DisableGizmo();
        MovePathManager.Inst.CloseAndSave();
        BasePrimitivePanel.DisSelect();
        UIManager.Inst.CloseAllDialog();
        List<int> layers = new List<int>();

        for (var i = 0; i < comps.Length; i++)
        {
            layers.Add(comps[i].gameObject.layer);
            if ((comps[i].gameObject.layer == 6 || comps[i].gameObject.layer == 10))
            {
                continue;
            }
            comps[i].gameObject.layer = 11;
        }

        Camera.main.RemoveLayer(LayerMask.NameToLayer("PVPArea"));
        Camera.main.RemoveLayer(LayerMask.NameToLayer("ShotExclude"));
        Camera.main.RemoveLayer(LayerMask.NameToLayer("SpecialModel"));
        Camera.main.RemoveLayer(LayerMask.NameToLayer("TriggerModel"));
        Camera.main.RemoveLayer(LayerMask.NameToLayer("WaterCube"));
        Camera.main.RemoveLayer(LayerMask.NameToLayer("Model"));
        Camera.main.RemoveLayer(LayerMask.NameToLayer("Touch"));
        Camera.main.RemoveLayer(LayerMask.NameToLayer("Terrain"));
        Camera.main.RemoveLayer(LayerMask.NameToLayer("Ignore Raycast"));
        Camera.main.RemoveLayer(LayerMask.NameToLayer("IceCube"));
        Camera.main.AddLayer(LayerMask.NameToLayer("Prop"));
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Color oriBgColor = Camera.main.backgroundColor;
        Camera.main.backgroundColor = new Color(0, 0, 0, 0);

        ResCoverPanel.Show();
        ResCoverPanel.Instance.SetPropFileName(fileName);
        ResCoverPanel.Instance.SetHighestResIds(highestResIds);
        ResCoverPanel.Instance.SetReturnClick(() => {
            GameEditModePanel.Show();
            GlobalFieldController.isScreenShoting = false;
            for (var i = 0; i < comps.Length; i++)
            {
                comps[i].gameObject.layer = layers[i];
            }

            Camera.main.AddLayer(LayerMask.NameToLayer("PVPArea"));
            Camera.main.AddLayer(LayerMask.NameToLayer("ShotExclude"));
            Camera.main.AddLayer(LayerMask.NameToLayer("SpecialModel"));
            Camera.main.AddLayer(LayerMask.NameToLayer("TriggerModel"));
            Camera.main.AddLayer(LayerMask.NameToLayer("WaterCube"));
            Camera.main.AddLayer(LayerMask.NameToLayer("Model"));
            Camera.main.AddLayer(LayerMask.NameToLayer("Touch"));
            Camera.main.AddLayer(LayerMask.NameToLayer("Terrain"));
            Camera.main.AddLayer(LayerMask.NameToLayer("Ignore Raycast"));
            Camera.main.AddLayer(LayerMask.NameToLayer("IceCube"));
            Camera.main.RemoveLayer(LayerMask.NameToLayer("Prop"));
            Camera.main.clearFlags = CameraClearFlags.Skybox;
            Camera.main.backgroundColor = oriBgColor;

            Mask.SetActive(false);
        });
    }

    private void OnUploadResource(string content)
    {
        TipPanel.ShowToast("Upload successfully:D");
    }

    private void OnCloneClick()
    {
        if (CheckLockState()) return;
        var curTarget = gController.GetCurrentTarget();
        if (!SceneBuilder.Inst.CanCloneTarget(curTarget))
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }
        if (!DisplayBoardManager.Inst.IsCanCloneDisplayBoard(curTarget))
        {
            return;
        }
        if (!SoundManager.Inst.IsCanCloneSound(curTarget))
        {
            return;
        }
        if (!PickabilityManager.Inst.CheckCanClone())
        {
            return;
        }
        if (!LeaderBoardManager.Inst.IsCanCloneLeaderBoard(curTarget))
        {
            return;
        }
        if (!UgcClothItemManager.Inst.CheckCanClone(curTarget))
        {
            return;
        }
        if (!ParachuteManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!IceCubeManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!FireworkManager.Inst.IsCanClone(curTarget))
        {
            return;
        }

        if (!BounceplankManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!LadderManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!PGCPlantManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!ShotPhotoManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!SnowCubeManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!FishingEditManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!SeesawManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!VIPZoneManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!SlidePipeManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!SpawnPointManager.Inst.IsCanClone(curTarget))
        {
            return;
        }

        if (!PGCEffectManager.Inst.IsCanClone(curTarget))
        {
            return;
        }
        if (!SwingManager.Inst.IsCanClone(curTarget))
                {
            return;
        }
        if (curTarget.GetComponent<NodeBaseBehaviour>().entity.HasComponent<BloodPropComponent>()
        && BloodPropManager.Inst.IsOverMaxCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return;
        }

        if (curTarget != null)
        {
            var newTarget = SceneBuilder.Inst.CloneTarget(curTarget, false);
            var newEntity = newTarget.GetComponent<NodeBaseBehaviour>().entity;
            var swbs = newTarget.GetComponentsInChildren<SteeringWheelBehaviour>();
            if (swbs.Length > 0)
            {
                SteeringWheelManager.Inst.AddCar(newEntity.Get<GameObjectComponent>().uid, swbs[0]);
            }
            OnSelectTarget?.Invoke(newEntity);

            AddCloneRecord(newTarget);

        }
    }

    

    private void OnUnCombineClick()
    {
        if (CheckLockState()) return;
        var curTarget = gController.GetCurrentTarget();
        if (curTarget != null)
        {
            //解组合前
            SceneEntity entity = curTarget.GetComponent<NodeBaseBehaviour>()?.entity;
            CombineUndoData beginData = new CombineUndoData();
            beginData.combineUndoMode = (int)CombineUndoMode.UnCombine;
            beginData.InitCombinedData(entity);
            List<SceneEntity> entitys = new List<SceneEntity>();

            int childCount = curTarget.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var childNode = curTarget.transform.GetChild(0);
                childNode.SetParent(curTarget.transform.parent);
                childNode.transform.localScale = DataUtils.LimitVector3(childNode.transform.localScale);

                SceneEntity childEntity = childNode.GetComponent<NodeBaseBehaviour>()?.entity;
                if(childEntity!=null){
                    entitys.Add(childEntity);
                }
            }

            //解组合后
            CombineUndoData endData = new CombineUndoData();
            endData.combineUndoMode = (int)CombineUndoMode.UnCombine;
            endData.InitMultiData(entitys);

            AddUnCombineRecord(beginData,endData);

        }
        // OnDestroyNode?.Invoke();
        SecondCachePool.Inst.DestroyEntity(curTarget);  
        OnUnSelectAllTarget?.Invoke();
    }


    private bool IsContainSpecialModel()
    {
        var curTarget = gController.GetCurrentTarget();
        if (curTarget != null)
        {
            var behavs = curTarget.GetComponentsInChildren<NodeBaseBehaviour>();
            for (var i = 0; i < behavs.Length; i++)
            {
                var modelType = behavs[i].entity.Get<GameObjectComponent>().modelType;
                var resType = behavs[i].entity.Get<GameObjectComponent>().type;
                if (resType == ResType.UGC || (modelType != NodeModelType.BaseModel && modelType != NodeModelType.CommonCombine
                    && modelType != NodeModelType.SpotLight && modelType != NodeModelType.PointLight && modelType != NodeModelType.TrapBox
                        && modelType != NodeModelType.SensorBox))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void OnChangeXYZClick(int dir,int axis)
    {
        if (CheckLockState()) return;
        switch (handleMode)
        {
            case HandleMode.Move:
                StepMove(dir,axis);
                break;
            case HandleMode.Rotate:
                StepRotate(dir, axis);
                break;
            case HandleMode.Scale:
                StepScale(dir,axis);
                break;
        }
    }

    public void StepMove(int dir,int axis)
    {
        Vector3 moveVec = Vector3.zero;
        moveVec[axis] = moveDefSnap * dir;
        gController.MoveTarget(moveVec);
    }

    public void StepRotate(int dir, int axis)
    {
        float rotAngle = fixRotDefSnap;
        rotAngle *= dir;
        gController.RotateTarget(axis, rotAngle);
    }

    public void StepScale(int dir, int axis)
    {
        Vector3 scaleVec = Vector3.zero;
        scaleVec[axis] = scaleDefSnap * dir;
        var curTarget = gController.GetCurrentTarget();
        if (curTarget != null)
        {
            Vector3 localScale = curTarget.transform.localScale;
            Vector3 newScaled = curTarget.transform.localScale + scaleVec;
            if (newScaled[axis] < minScaleVal)
            {
                newScaled[axis] = minScaleVal;
            }
            ChangeTargetScale(curTarget,newScaled);
        }
    }

    private void ChangeTargetScale(GameObject curTarget,Vector3 newScaled)
    {
        var lastScale = curTarget.transform.lossyScale;
        gController.ScaleTarget(newScaled);
        var newScale = curTarget.transform.lossyScale;
        ChangeBaseTile(lastScale, newScale);
    }
    
    public void ChangeBaseTile(Vector3 lastScale,Vector3 newScale)
    {
        if(modelMode != NodeHandleType.Base)
            return;
        var curTarget = gController.GetCurrentTarget();
        var nodeBehav = curTarget.GetComponent<NodeBehaviour>();
        if (nodeBehav != null)
        {
            nodeBehav.SetScale(newScale,lastScale);
            SaveTiling(nodeBehav);
        }
    }

    private void OnNorAddClick()
    {
        if (CheckLockState()) return;
        var curTarget = gController.GetCurrentTarget();
        if(curTarget == null)
            return;
        Vector3 newScaled = GetUniformStepScale(curTarget.transform.localScale,1);
        ChangeTargetScale(curTarget, newScaled);
    }

    private void OnNorSubClick()
    {
        if (CheckLockState()) return;
        var curTarget = gController.GetCurrentTarget();
        if (curTarget == null)
            return;
        Vector3 newScaled = GetUniformStepScale(curTarget.transform.localScale, -1);
        for (int i = 0; i < 3; i++)
        {
            if (newScaled[i] < minScaleVal)
            {
                newScaled[i] = minScaleVal;
            }
        }
        ChangeTargetScale(curTarget, newScaled);
    }
    //the same china
    private Vector3 GetUniformStepScale(Vector3 current,float dir)
    {
        List<float> curVals = new List<float>();
        for (int i = 0; i < 3; i++)
        {
            if (current[i] > fixScaleDefSnap)
            {
                curVals.Add(current[i]);
            }
        }



        int index = 2;//default z axis
        float min = current[index];
        if (curVals.Count != 0)
        {
            min = curVals.Min();
            for (int i = 0; i < 3; i++)
            {
                if (current[i].Equals(min))
                {
                    index = i;
                    break;
                }
            }
        }
        Vector3 newVec = Vector3.zero;
        newVec[index] = min + dir * fixScaleDefSnap;
        for (int i = 0; i < 3; i++)
        {
            if (i != index)
            {
                newVec[i] = newVec[index] * current[i]/ current[index];
            }
        }
        return newVec;
    }

    private void OnScaToggleClick(bool enable)
    {
        fixScaEnable = enable;
        ScaleToggle.targetGraphic.enabled = !enable;
        AxisGo.SetActive(!fixScaEnable);
        ExtraGo.SetActive(fixScaEnable);
        ScaleLabel.SetActive(fixScaEnable);
    }


    public void SetGizmoController(GizmoController controller)
    {
        gController = controller;
        LockHideManager.Inst.gController = controller;
    }

    public void OnMoveClick()
    {
        LoggerUtils.Log("OnMoveClick");
        handleMode = HandleMode.Move;
        PosCoToggle.IsOn = true;
        gController.SetMoveCtr();
        AxisGo.gameObject.SetActive(true);
        ExtraGo.gameObject.SetActive(false);
        ScaleToggle.gameObject.SetActive(false);
        rotGo.gameObject.SetActive(false);
        SetStepPanel(modelMode, handleMode);
    }

    public void OnRotateClick()
    {
        handleMode = HandleMode.Rotate;
        RotCoToggle.IsOn = true;
        gController.SetRotateCtr();

        gController.ShowXZAxis(modelMode != NodeHandleType.SteeringWheel
            && modelMode != NodeHandleType.WaterCube
            && modelMode != NodeHandleType.MagneticBoard
            && modelMode != NodeHandleType.Born
            && modelMode != NodeHandleType.RotAxisY
            && modelMode != NodeHandleType.TrapSpawn
            && modelMode != NodeHandleType.PVP
            && modelMode != NodeHandleType.VIPZone
            && modelMode != NodeHandleType.SeesawCombine
            && modelMode != NodeHandleType.CrystalStone);
        ScaleToggle.gameObject.SetActive(false);
        rotGo.gameObject.SetActive(true);
        AxisGo.SetActive(true);
        ExtraGo.gameObject.SetActive(false);
        SetStepPanel(modelMode, handleMode);
    }

    public void OnScaleClick()
    {
        handleMode = HandleMode.Scale;
        ScaCoToggle.IsOn = true;
        gController.SetScaleCtr(OnCreateScale);
        gController.ShowScaleXZAxis(modelMode != NodeHandleType.Bounceplank);
        gController.ShowScaleYAxis(modelMode != NodeHandleType.Ladder);
        AxisGo.gameObject.SetActive(!isSpecialScale);
        ScaleToggle.gameObject.SetActive(true);
        rotGo.gameObject.SetActive(false);
        ScaleToggle.isOn = isSpecialScale || ScaleToggle.isOn;
        ScaleToggle.interactable = !isSpecialScale;
        ScaleToggle.gameObject.SetActive(!isHideSpecialScale);
        AxisGo.SetActive(!ScaleToggle.isOn);
        ExtraGo.gameObject.SetActive(ScaleToggle.isOn);
        ScaleLabel.SetActive(ScaleToggle.isOn);
        var curTarget = gController.GetCurrentTarget();
        if (curTarget != null)
        {
            var nodeBehav = curTarget.GetComponent<NodeBaseBehaviour>();
            nodeBehav.norScale = isSpecialScale;
        }
        SetStepPanel(modelMode, handleMode);
    }

    private void OnCreateScale()
    {
        gController.scaleGizmo.Gizmo.PostDragBegin += OnDragBegin;
        gController.scaleGizmo.Gizmo.PostDragUpdate += OnDragUpdate;
    }

    private void OnDragBegin(Gizmo giz, int handle)
    {
        var curTarget = gController.GetCurrentTarget();
        if (curTarget != null)
        {
            targetLastScale = curTarget.transform.lossyScale;
        }
    }

    private void OnDragUpdate(Gizmo giz, int handle)
    {
        var curTarget = gController.GetCurrentTarget();
        if (curTarget != null)
        {
            var constrain = curTarget.GetComponent<NodeBehaviour>();
            if (constrain)
            {
                constrain.SetScale(curTarget.transform.lossyScale, targetLastScale);
                SaveTiling(constrain);
            }
            targetLastScale = curTarget.transform.lossyScale;
        }
    }

    private void SetBtnActiveBySceneType(bool isActive)
    {
        // if (GameManager.Inst.sceneType == SCENE_TYPE.MAP_SCENE || GameManager.Inst.sceneType == SCENE_TYPE.MYSPACE_SCENE)
        // {
        //     UploadBtn.gameObject.SetActive(isActive);
        // }
        // else if (GameManager.Inst.sceneType == SCENE_TYPE.ResMAP_SCENE)
        // {
        //     UploadBtn.gameObject.SetActive(false);
        // }
    }

    private void OnDestoryClick()
    {
        if (CheckLockState()) return;
        var curTarget = gController.GetCurrentTarget();
        if (!IsCanDestoryTarget(curTarget))
        {
            return;
        }
        CoGroup.TurnOn(null);
        OnDestroyNode?.Invoke();
        ReferManager.Inst.EnterReferPlay();
    }

    private bool IsCanDestoryTarget(GameObject curTarget)
    {
        bool isCan = true;
        if (curTarget.GetComponent<SpawnPointBehaviour>())
        {
            isCan = SpawnPointManager.Inst.IsCanDesTarget();
        }
        if (curTarget.GetComponent<TransferBehaviour>())
        {
            isCan = DowntownTransferManager.Inst.IsCanDesTarget();
        }
        return isCan;
    }

    #region Lock Func
    private void OnLockTogChange(bool isLock)
    {
        AddLockRecord(isLock);
        SetCurLockState(isLock);
        
    }

    public void SetCurLockState(bool isLock)
    {
        LockHideManager.Inst.SetCurLockState(isLock);
        SetCurLockUI(isLock);
    }
    public void SetCurLockStateUndo(bool isLock)
    {
        LockHideManager.Inst.SetCurLockState(isLock);
        SetCurLockUI(isLock);
        LockTog.onValueChanged.RemoveAllListeners();
        LockTog.isOn = isLock;
        LockTog.onValueChanged.AddListener(OnLockTogChange);
    }
    private void SetCurLockUI(bool isLock)
    {
        var spUnLock = LockTog.transform.Find("IconUnLock").gameObject;
        var spLock = LockTog.transform.Find("IconLock").gameObject;
        if ((spUnLock != null) && (spLock != null))
        {
            spUnLock.SetActive(!isLock);
            spLock.SetActive(isLock);
        }
    }

    private bool GetCurLockState()
    {
        bool lockState = LockHideManager.Inst.GetCurLockState();
        SetCurLockState(lockState);
        return lockState;
    }

    private bool CheckLockState()
    {
        if (GetCurLockState())
        {
            TipPanel.ShowToast("Unlock it to edit");
        }
        return GetCurLockState();
    }
    #endregion

    #region Hide Func
    private void OnHideBtnClick()
    {
        if (CheckLockState()) return;
        LockHideManager.Inst.HideCurProp();
    }
    #endregion

    public void ResetGizmoState()
    {
        switch (handleMode)
        {
            case HandleMode.Move:
                gController.SetMoveCtr();
                break;
            case HandleMode.Rotate:
                gController.SetRotateCtr();
                break;
            case HandleMode.Scale:
                gController.SetScaleCtr(OnCreateScale);
                break;
        }
    }

    private void SaveTiling(NodeBehaviour nodeBehav)
    {
        var curEntity = nodeBehav.entity;
        var mpb = NodeBehaviour.mpb;
        var tiling = mpb.GetVector("_MainTex_ST");
        if (curEntity != null)
        {
            curEntity.Get<MaterialComponent>().tile = tiling;
        }
    }

    public static void AddCloneRecord(GameObject gameObject)
    {
        if(gameObject == null)
        {
            LoggerUtils.LogError("EditModeController AddCreateRecord gameObject is null!");
            return;
        }
        CreateDestroyUndoData beginData = new CreateDestroyUndoData();
        beginData.targetNode = null;
        beginData.createUndoMode = (int)CreateUndoMode.Duplicate;

        CreateDestroyUndoData endData = new CreateDestroyUndoData();
        endData.targetNode = gameObject;
        beginData.createUndoMode = (int)CreateUndoMode.Duplicate;

        UndoRecord record = new UndoRecord(UndoHelperName.CreateDestroyUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddLockRecord(bool islock)
    {
        GameObject obj = gController.GetCurrentTarget();
        if (obj == null)
        {
            return;
        }
        Transform target = obj.transform;
       
        LockHideUndoData beginData = new LockHideUndoData();
        beginData.targetNode = target;
        beginData.LockHideType = (int)LockHideType.Lock;
        beginData.isLock = gController.GetLockState();
        LockHideUndoData endData = new LockHideUndoData();
        endData.targetNode = target;
        endData.LockHideType = (int)LockHideType.Lock;
        endData.isLock = islock;
        UndoRecord record = new UndoRecord(UndoHelperName.LockHideUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddUnCombineRecord(object beginData,object endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.CombineUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;

        UndoRecordPool.Inst.PushRecord(record);
    }


    public void ShowUploadBtn(SceneEntity entity)
    {
        if (entity == null)
        {
            return;
        }
        var gameComp = entity.Get<GameObjectComponent>();
        bool isActive = !SceneParser.Inst.IsContainSpecialEntity(entity);

        if (GameManager.Inst.sceneType == SCENE_TYPE.ResMAP_SCENE)
        {
            isActive = false;
        }
        UploadBtn.gameObject.SetActive(isActive);
    }
}

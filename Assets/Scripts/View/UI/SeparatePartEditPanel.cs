using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author : Tee Li
/// 描述: 钓鱼竿需求引入，分开调整部件位置的面板
/// 日期：2022/9/1
/// </summary>

public class SeparatePartEditPanel : BasePanel<SeparatePartEditPanel>
{
    public static bool IsOn => Instance && Instance.gameObject.activeInHierarchy;

    
    public Text TitleText;
    public Action SureBtnClickAct { get; set; }
    public Action CancelBtnClickAct { get; set; }

    private GizmoController ogController;
    private GizmoController gController;
    private bool InputReceiverLocked;
    private Dictionary<GameObject, int> state;

    private string layerName = "Anchors";
    private int layer;
    private string[] ignoreLayer = new[] { "ShotExclude", "PVPArea", "SpecialModel", "Model", "TriggerModel", "Touch", "Prop", "Player", "WaterCube", "OtherPlayer", "PostProcess", "Trigger", "Head", "Ignore Raycast", "IceCube" };

    private ICollection<SceneEntity> entities;
    private SceneEntity fishingEntity;
    private SceneEntity rodEntity;
    private SceneEntity hookEntity;
    private SceneEntity curEntity;

    [SerializeField] private Button cancelBtn;
    [SerializeField] private Button sureBtn;

    [SerializeField] private Toggle moveBtn;
    [SerializeField] private Toggle rotationBtn;
    [SerializeField] private Toggle scaleBtn;

    public const int ENTITY_TYPE_FISH = 0;
    public const int ENTITY_TYPE_UGC_COMBINE = 1;
    public const int ENTITY_TYPE_VIP_ZONE = 2;
    private int entityType = ENTITY_TYPE_FISH;
    private Action<Ray,float> ClickAction;

    public Action OnMoveSelect;
    public Action OnRotateSelect;
    public Action OnScaleSelect;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        layer = LayerMask.NameToLayer(layerName);
        ogController = GameObject.Find("GameStart").GetComponent<GameController>().editController.gController;
        ogController.DisableGizmo();
        BasePrimitivePanel.DisSelect();
        UIManager.Inst.SwitchDialog();
        AddListeners();
    }

    private void CameraRemoveOtherLayer()
    {
        foreach (var s in ignoreLayer)
        {
            GameManager.Inst.MainCamera.RemoveLayer(LayerMask.NameToLayer(s));
        }
    }

    private void CameraResumeOtherLayer()
    {
        foreach (var s in ignoreLayer)
        {
            GameManager.Inst.MainCamera.AddLayer(LayerMask.NameToLayer(s));
        }
    }

    public void SetUp(SceneEntity entity)
    {
        gController = new GizmoController();
        gController.ShowScaleYAxis(true);
        gController.ShowScaleXZAxis(true);
        gController.NeedRecord = false;
        Select(entity);
        moveBtn.isOn = true;
        InputReceiverLocked = InputReceiver.locked;
        InputReceiver.locked = false;
        moveBtn.interactable = false;
    }

    public void SetEntities(SceneEntity fishingEntity, SceneEntity rodEntity, SceneEntity hookEntity)
    {
        this.fishingEntity = fishingEntity;
        this.rodEntity = rodEntity;
        this.hookEntity = hookEntity;

        this.entities = new List<SceneEntity>() { rodEntity, hookEntity };
        SetNorScale(entities);
        SwitchLayer();
        entityType = ENTITY_TYPE_FISH;
        CameraRemoveOtherLayer();
    }

    public void SetEntities(SceneEntity[] entities,int entityType)
    {
        this.entities = entities.ToList();
        this.entityType = entityType;
        if (entityType == ENTITY_TYPE_UGC_COMBINE)
        {
            SwitchLayer();
            CameraRemoveOtherLayer();
        }
        gController.ShowXZAxis();
    }

    public void SetClickAction(Action<Ray,float> clickAction)
    {
        this.ClickAction = clickAction;
    }

    public void Select(SceneEntity entity)
    {
        curEntity = entity;
        if (entity == null)
        {
            gController.SetTarget(null);
        }
        else
        {
            gController.SetTarget(entity.Get<GameObjectComponent>().bindGo);
        }
        moveBtn.isOn = true;
        OnMoveSelect?.Invoke();
        if (entity != null && entity == hookEntity)
            FixedTransform(hookEntity.Get<GameObjectComponent>().bindGo, false);
    }

    public GizmoController GetGizmoController()
    {
        return gController;
    }

    public void OnClick(Ray ray, float maxCamDist)
    {
        if (entityType == ENTITY_TYPE_FISH)
        {
            FishClick(ray,maxCamDist);
        }
        else
        {
            ClickAction?.Invoke(ray,maxCamDist);
        }
    }

    private void FishClick(Ray ray, float maxCamDist)
    {
        bool isHit = Physics.Raycast(ray, out RaycastHit hit, 2 * maxCamDist, LayerMask.GetMask(layerName));
        if (isHit)
        {
            GameObject go = hit.collider.gameObject;
            NodeBaseBehaviour nodeBehav = go.GetComponent<NodeBaseBehaviour>();
            if (nodeBehav == null)
                nodeBehav = go.GetComponentInParent<NodeBaseBehaviour>();
            SceneEntity entity = nodeBehav.entity;
            if (entity != null)
            {
                var comp = entity.Get<GameObjectComponent>();
                if (moveBtn.interactable)
                {
                    switch (comp.modelType)
                    {
                        case NodeModelType.FishingModel:
                            entity = rodEntity;
                            break;
                    }

                    Select(entity);
                }
                else
                {
                    switch (comp.modelType)
                    {
                        case NodeModelType.FishingRod:
                            entity = fishingEntity;
                            FixedTransform(hookEntity.Get<GameObjectComponent>().bindGo, true);
                            break;
                        case NodeModelType.FishingHook:
                            FixedTransform(hookEntity.Get<GameObjectComponent>().bindGo, false);
                            break;
                    }

                    Select(entity);
                }
            }
        }
    }

    public void SetTitle(string titleTxt, params object[] formatArgs)
    {
        LocalizationConManager.Inst.SetLocalizedContent(TitleText, titleTxt, formatArgs);
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        ResetGizmoType();
        if (entityType != ENTITY_TYPE_VIP_ZONE)
        {
            CameraResumeOtherLayer();
        }
    }

    private void ResetGizmoType()
    {
        if (ModelHandlePanel.Instance)
        {
            ModelHandlePanel.Instance.ResetGizmoState();
        }
    }

    private void AddListeners()
    {
        cancelBtn.onClick.AddListener(OnCancelClick);
        sureBtn.onClick.AddListener(OnSureClick);
        moveBtn.onValueChanged.AddListener(OnMoveClick);
        rotationBtn.onValueChanged.AddListener(OnRotationClick);
        scaleBtn.onValueChanged.AddListener(OnScaleClick);
    }


    private void OnCancelClick()
    { 
        OnHidePanel();
        CancelBtnClickAct?.Invoke();
    }

    private void OnSureClick()
    {
        OnHidePanel();
        SureBtnClickAct?.Invoke();        
    }

    //非玩家操作导致的关闭
    public static void CloseNotByUser()
    {
        if (Instance == null)
        {
            return;
        }

        if (Instance.gameObject.activeInHierarchy)
        {
            //就当做点了一下确定
            Instance.OnSureClick();
        }
    }

    private void OnHidePanel()
    {       
        InputReceiver.locked = InputReceiverLocked;
        BasePrimitivePanel.Show();
        LockHideManager.Inst.CheckHidePanelVisable();
        UIManager.Inst.SwitchDialog();
        gController.SetZMoveStatus(true);
        gController.DisableGizmo();
        gController = null;
        Hide();
        if (entityType != ENTITY_TYPE_VIP_ZONE)
        {
            SwitchLayer();
        }

        if (entityType == ENTITY_TYPE_FISH)
        {
            FixedTransform(hookEntity.Get<GameObjectComponent>().bindGo, false);
        }
    }



    public void OnMoveClick(bool isOn)
    {
        if (isOn)
        {
            gController.SetMoveCtr();
            OnMoveSelect?.Invoke();
        }
        moveBtn.interactable = !isOn;
    }

    public void OnRotationClick(bool isOn)
    {
        rotationBtn.interactable = !isOn;
        if (isOn)
        {
            gController.SetRotateCtr();
            OnRotateSelect?.Invoke();
            if (entityType == ENTITY_TYPE_FISH)
            {
                if (curEntity == fishingEntity)
                    Select(rodEntity);
            }
        }
    }

    public void OnScaleClick(bool isOn)
    {
        scaleBtn.interactable = !isOn;
        if (isOn)
        {
            gController.SetScaleCtr(null);
            OnScaleSelect?.Invoke();
            if (entityType == ENTITY_TYPE_FISH)
            {
                if (curEntity == fishingEntity)
                    Select(rodEntity);
            }
        }
    }

    private void SetNorScale(ICollection<SceneEntity> entities)
    {
        foreach(SceneEntity entity in entities)
        {
            NodeBaseBehaviour behav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NodeBaseBehaviour>();
            if (behav)
            {
                behav.norScale = true;
            }
        }
    }


    private void SwitchLayer()
    {
        if (state == null)
        {
            List<Transform> targets = new List<Transform>();
            foreach(SceneEntity entity in entities)
            {
                targets.AddRange(entity.Get<GameObjectComponent>().bindGo.GetComponentsInChildren<Transform>());
            }
            state = new Dictionary<GameObject, int>();
            foreach (var v in targets)
            {
                var o = v.gameObject;
                state.Add(o, o.layer);
                o.layer = layer;
            }
        }
        else
        {
            foreach (var kv in state)
            {
                if (kv.Key != null)
                {
                    kv.Key.layer = kv.Value;
                }
            }
            state = null;
        }
    }

    private void FixedTransform(GameObject go, bool enable)
    {
        var fixedCtrl = go.GetComponent<FixedCtrl>();

        if (enable)
        {
            if (fixedCtrl == null)
                go.AddComponent<FixedCtrl>();
        }
        else
        {
            if (fixedCtrl != null)
                Destroy(fixedCtrl);
        }
    }

    public string GetLayerName()
    {
        return layerName;
    }

    public SceneEntity GetCurSelectEntity()
    {
        return curEntity;
    }

    public void SetCurSelectEntity(SceneEntity entity)
    {
        curEntity = entity;
    }
}

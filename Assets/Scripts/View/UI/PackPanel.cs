using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using RTG;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PackPanel : BasePanel<PackPanel>,IUndoRecord
{
    public class EntityStateData
    {
        public SceneEntity entity;
        public bool isHigh;
    }

    public Button CancelBtn;
    public Button SureBtn;
    private float maxCamDist = 350;
    private float firstTouchTime = 0;
    private readonly float shortTouchThreshold = 0.2f;
    private Camera mainCamera;
    private Action returnClick;
    private MaterialPropertyBlock mpb;
    private Dictionary<SceneEntity, EntityStateData> selects = new Dictionary<SceneEntity, EntityStateData>();
    private bool hasRepetition = false;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        mpb = new MaterialPropertyBlock();
        mainCamera = GameManager.Inst.MainCamera;
        SureBtn.gameObject.SetActive(false);
        CancelBtn.onClick.AddListener(OnCancelClick);
        SureBtn.onClick.AddListener(OnSureClick);
        hasRepetition = false;
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        EditModeController.IsCanSelect = false;
        mainCamera.RemoveLayer(LayerMask.NameToLayer("ShotExclude"));
        mainCamera.RemoveLayer(LayerMask.NameToLayer("WaterCube"));
        mainCamera.RemoveLayer(LayerMask.NameToLayer("PVPArea"));
        mainCamera.RemoveLayer(LayerMask.NameToLayer("IceCube"));
        selects = new Dictionary<SceneEntity, EntityStateData>();
        MessageHelper.Broadcast(MessageName.OpenPackPanel, true);
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        EditModeController.IsCanSelect = true;
        mainCamera.AddLayer(LayerMask.NameToLayer("ShotExclude"));
        mainCamera.AddLayer(LayerMask.NameToLayer("WaterCube"));
        mainCamera.AddLayer(LayerMask.NameToLayer("PVPArea"));
        mainCamera.AddLayer(LayerMask.NameToLayer("IceCube"));
        MessageHelper.Broadcast(MessageName.OpenPackPanel, false);
    }

    private void OnCancelClick()
    {
        foreach (var data in selects.Values)
        {
            EntityHighLight(data.entity, false);
        }
        OnHidePanel();
    }

    public void SetReturnClick(Action ret)
    {
        returnClick = ret;
    }

    private void OnHidePanel()
    {
        SceneGizmoPanel.Show();
        BasePrimitivePanel.Show();
        LockHideManager.Inst.CheckHidePanelVisable();
        returnClick?.Invoke();
        Hide();
        if (ReferManager.Inst.isRefer)
        {
            ReferPanel.Instance.OnReferMode();
            ReferManager.Inst.isHafeRefer = false;
        }
        if (ReferPanel.Instance)
        {
            ReferPanel.Show();
        }
    }

    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }
    private void OnSureClick()
    {
        //操作前
        var entitys = new List<SceneEntity>(selects.Keys);
        CombineUndoData beginData = new CombineUndoData();
        beginData.combineUndoMode = (int)CombineUndoMode.Combine;
        beginData.InitMultiData(entitys);
       
        var entity = SceneBuilder.Inst.CombineNode(new List<SceneEntity>(selects.Keys));
        EntityHighLight(entity, false);
        OnHidePanel();
       
        //操作后
        CombineUndoData endData = new CombineUndoData();
        endData.combineUndoMode = (int)CombineUndoMode.Combine;
        endData.InitCombinedData(entity);
        
        UndoRecord record = new UndoRecord(UndoHelperName.CombineUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private bool CheckRepetition<T>(SceneEntity entity, bool isSelect)
    {
        var go = entity.Get<GameObjectComponent>().bindGo;
        var com = go.GetComponentInChildren<T>(true);
        if (com != null)
        {
            if (isSelect)
            {
                hasRepetition = false;
                return false;
            }
            if (hasRepetition)
            {
                return true;
            }else
            {
                hasRepetition = true;
            }
        }
        return false;
    }


    private void EntityHighLight(SceneEntity entity, bool isHigh)
    {
        var entityGo = entity.Get<GameObjectComponent>().bindGo;
        var nodeBehav = entityGo.GetComponentsInChildren<NodeBaseBehaviour>();
        for (int i = 0; i < nodeBehav.Length; i++)
        {
            HighLight(nodeBehav[i], isHigh);
        }
    }

    private void HighLight(NodeBaseBehaviour baseBehav,bool isHigh)
    {
        var modelType = baseBehav.entity.Get<GameObjectComponent>().modelType;
        baseBehav.HighLight(isHigh);
    }

    

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InputReceiver.locked = false;
    }

    #region Raycast

    private void Update()
    {
        if (Input.touchCount == 1)
        {
            HandleSingleTouch();
        }
    }

    void HandleSingleTouch()
    {
        var curTouch = Input.GetTouch(0);
        if (EventSystem.current.IsPointerOverGameObject(curTouch.fingerId))
        {
            return;
        }

        if (curTouch.phase == TouchPhase.Began)
        {
            firstTouchTime = Time.timeSinceLevelLoad;
        }

        if (curTouch.phase == TouchPhase.Ended)
        {
            if (Time.timeSinceLevelLoad - firstTouchTime < shortTouchThreshold)
            {
                OnSelectTarget(curTouch);
            }
        }
    }

    private void OnSelectTarget(Touch touch)
    {
        if (GlobalFieldController.isScreenShoting)
            return;

        Ray ray = mainCamera.ScreenPointToRay(touch.position);
        //| 1 << LayerMask.NameToLayer("ShotExclude") 
        bool isHit = Physics.Raycast(ray, out RaycastHit hit, 2 * maxCamDist, 
            1 << LayerMask.NameToLayer("Model") 
            | 1 << LayerMask.NameToLayer("SpecialModel")
            | 1 << LayerMask.NameToLayer("Touch"));
        if (isHit)
        {
            var go = hit.collider.gameObject;
            var nodeBehav = go.GetComponentInParent<NodeBaseBehaviour>();
            var entity = SceneObjectController.GetCanControllerNode(nodeBehav.gameObject);
            if (CheckRepetition<SteeringWheelBehaviour>(entity, selects.ContainsKey(entity)))
            {
                TipPanel.ShowToast("Only one steering wheel can be contained in a composite object");
                return;
            }
            OnHighLight(entity);
            SetSureBtnState();
        }
    }

    #endregion

    private void OnHighLight(SceneEntity entity)
    {
        if (selects.ContainsKey(entity))
        {
            EntityStateData data = selects[entity];
            data.isHigh = !data.isHigh;
            EntityHighLight(data.entity, data.isHigh);
            selects.Remove(entity);
        }
        else
        {
            EntityStateData data = new EntityStateData();
            data.entity = entity;
            data.isHigh = true;
            EntityHighLight(data.entity, data.isHigh);
            selects.Add(entity, data);
        }
    }


    private void SetSureBtnState()
    {
        int acount = 0;
        foreach (var val in selects.Values)
        {
            if (val.isHigh)
            {
                acount++;
            }
        }

        SureBtn.gameObject.SetActive(acount > 1);
    }

    
}

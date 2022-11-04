using SavingData;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author : Tee Li
/// 2022/9/1
/// </summary>

public class FishingRodPanel : InfoPanel<FishingRodPanel>
{
    public FishingUgcPanel ugcRodPanel;
    public FishingUgcPanel ugcHookPanel;

    public Button customPartBtn;
    public Button customHookBtn;
    public Button editPartBtn;
    public Button editHookBtn;

    private SceneEntity fishingEntity;
    private SceneEntity rodEntity { get { return FishingEditManager.Inst.GetRod(fishingBehav).entity; } }
    private SceneEntity hookEntity { get { return FishingEditManager.Inst.GetHook(fishingBehav).entity; } }
    private FishingBehaviour fishingBehav;

    private Vector3 rodPos;
    private Vector3 rodScale;
    private Quaternion rodRotation;
    private Vector3 hookPos;
    private Vector3 hookScale;
    private Quaternion hookRotation;

    public Button closePanelBtn;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        SetListeners();
    }

    public void SetEntity(SceneEntity entity)
    {
        var comp = entity.Get<GameObjectComponent>();
        fishingBehav = comp.bindGo.transform.GetComponentInParent<FishingBehaviour>();
        fishingEntity = fishingBehav.entity;

        ugcRodPanel.SetData(FishingEditManager.Inst.UgcRodRecords);
        ugcHookPanel.SetData(FishingEditManager.Inst.UgcHookRecords);

        SetUi(rodEntity, hookEntity);
    }

    private void SetUi(SceneEntity rod, SceneEntity hook)
    {
        FishingRodComponent rodComp = rod.Get<FishingRodComponent>();
        FishingHookComponent hookComp = hook.Get<FishingHookComponent>();

        ugcRodPanel.SelectItem(rodComp.rid);
        ugcHookPanel.SelectItem(hookComp.rid);

        ToggleBtn(customPartBtn, editPartBtn, rodComp.isCustomPos > 0);
        ToggleBtn(customHookBtn, editHookBtn, hookComp.isCustomHook > 0);
    }

    private void SetListeners()
    {
        customPartBtn.onClick.AddListener(OnCustomPartClick);
        customHookBtn.onClick.AddListener(OnCustomHookClick);
        editPartBtn.onClick.AddListener(OnEditPartClick);
        editHookBtn.onClick.AddListener(OnEditHookClick);
        closePanelBtn.onClick.AddListener(OnCloseBtnClick);

        ugcRodPanel.onSelectUgc = OnAfterUgcCreateRod;
        ugcHookPanel.onSelectUgc = OnAfterUgcCreateHook;
    }

    private void OnCloseBtnClick()
    {
        var opanel = UIManager.Inst.uiCanvas.GetComponentsInChildren<IPanelOpposable>(true);
        for (int i = 0; i < opanel.Length; i++)
        {
            opanel[i].SetGlobalHide(true);
        }
        this.gameObject.SetActive(false);
    }

    #region Callbacks
    private void OnAfterUgcCreateRod(MapInfo mapInfo, string rid, string mapJsonContent)
    {
        var beginData = new FishUndoData();
        var endData = new FishUndoData();

        var oldRod = FishingEditManager.Inst.GetRod(fishingBehav);
        beginData.uid = oldRod.entity.Get<GameObjectComponent>().uid;
        beginData.isRod = true;

        var newRod = FishingEditManager.Inst.CreateUgcRod(fishingBehav, mapInfo, rid, mapJsonContent);
        endData.uid = newRod.entity.Get<GameObjectComponent>().uid;
        endData.isRod = true;

        SecondCachePool.Inst.DestroyEntity(oldRod.gameObject);
        var record = new UndoRecord(UndoHelperName.FishingUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        UndoRecordPool.Inst.PushRecord(record);

        EnableAllCollider();
        if(PackPanel.Instance != null && PackPanel.Instance.gameObject.activeSelf)
        {
            MessageHelper.Broadcast(MessageName.OpenPackPanel, true);
            return;
        }
        EditModeController.SetSelect?.Invoke(fishingBehav.entity);
    }

    private void OnAfterUgcCreateHook(MapInfo mapInfo, string rid, string mapJsonContent)
    {
        var beginData = new FishUndoData();
        var endData = new FishUndoData();

        var oldHook = FishingEditManager.Inst.GetHook(fishingBehav);
        beginData.uid = oldHook.entity.Get<GameObjectComponent>().uid;
        beginData.isRod = false;

        var newHook = FishingEditManager.Inst.CreateUgcHook(fishingBehav, mapInfo, rid, mapJsonContent);
        endData.uid = newHook.entity.Get<GameObjectComponent>().uid;
        endData.isRod = false;

        SecondCachePool.Inst.DestroyEntity(oldHook.gameObject);
        var record = new UndoRecord(UndoHelperName.FishingUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        UndoRecordPool.Inst.PushRecord(record);

        EnableAllCollider();
        if(PackPanel.Instance != null && PackPanel.Instance.gameObject.activeSelf)
        {
            MessageHelper.Broadcast(MessageName.OpenPackPanel, true);
            return;
        }
        EditModeController.SetSelect?.Invoke(fishingBehav.entity);
    }

    private void OnCustomPartClick()
    {
        if (rodEntity == null) return;

        FishingRodComponent comp = rodEntity.Get<FishingRodComponent>();
        if (comp.isCustomPos == 0)
        {
            OnEditPartClick();
        }
        else
        {
            PartToDefault();
            SetUi(rodEntity, hookEntity);
        }
    }

    private void OnCustomHookClick()
    {
        if (hookEntity == null) return;

        FishingHookComponent comp = hookEntity.Get<FishingHookComponent>();
        if(comp.isCustomHook == 0)
        {
            OnEditHookClick();
        }
        else
        {
            HookToDefault();
            SetUi(rodEntity, hookEntity);
        }
    }

    private void OnEditPartClick()
    {
        var fishingGo = fishingEntity.Get<GameObjectComponent>().bindGo;
        var rodGo = rodEntity.Get<GameObjectComponent>().bindGo;
        var hookGo = hookEntity.Get<GameObjectComponent>().bindGo;
        rodPos = fishingGo ? fishingGo.transform.localPosition : Vector3.zero;
        rodScale = rodGo ? rodGo.transform.localScale : Vector3.zero;
        rodRotation = rodGo ? rodGo.transform.localRotation : Quaternion.identity;
        hookPos = hookGo ? hookGo.transform.localPosition : Vector3.zero;
        hookScale = hookGo ? hookGo.transform.localScale : Vector3.zero;
        hookRotation = hookGo ? hookGo.transform.localRotation : Quaternion.identity;

        SeparatePartEditPanel.Show();
        SeparatePartEditPanel.Instance.SetEntities(fishingEntity, rodEntity, hookEntity);
        SeparatePartEditPanel.Instance.SetTitle("Adjust the geometry of the rod components as you want");
        SeparatePartEditPanel.Instance.SetUp(hookEntity);   
        SeparatePartEditPanel.Instance.SureBtnClickAct = OnPartSureClick;
        SeparatePartEditPanel.Instance.CancelBtnClickAct = OnPartCancelClick;
    }
    
    private void OnEditHookClick()
    {
        fishingBehav.EnableHookEffet(true);

        CommonEditAnchorsPanel.Show();
        CommonEditAnchorsPanel.Instance.SetTitle("The creature will be hooked at the hook point you set");
        CommonEditAnchorsPanel.Instance.SetNodeName("Anchors");
        CommonEditAnchorsPanel.Instance.Init(hookEntity, hookEntity.Get<FishingHookComponent>().hookPosition);
        CommonEditAnchorsPanel.Instance.SureBtnClickAct = OnAnchorSureClick;
        CommonEditAnchorsPanel.Instance.CancelBtnClickAct = OnAnchorCancelClick;
    }

    private void OnPartSureClick()
    {
        rodEntity.Get<FishingRodComponent>().isCustomPos = 1;
        EditModeController.SetSelect?.Invoke(fishingEntity);
    }

    private void OnPartCancelClick()
    {
        var fishingGo = fishingEntity.Get<GameObjectComponent>().bindGo;
        var rodGo = rodEntity.Get<GameObjectComponent>().bindGo;
        var hookGo = hookEntity.Get<GameObjectComponent>().bindGo;
        if (fishingGo)
        {
            fishingGo.transform.localPosition = rodPos;
        }
        if (rodGo)
        {
            rodGo.transform.localPosition = Vector3.zero;
            rodGo.transform.localScale = rodScale;
            rodGo.transform.localRotation = rodRotation;
        }
        if (hookGo)
        {
            hookGo.transform.localPosition = hookPos;
            hookGo.transform.localScale = hookScale;
            hookGo.transform.localRotation = hookRotation;
        }

        EditModeController.SetSelect?.Invoke(fishingEntity);
    }

    private void OnAnchorSureClick(Vector3 pos)
    {
        fishingBehav.EnableHookEffet(false);

        var hookGo = hookEntity.Get<GameObjectComponent>().bindGo;
        if (hookGo != null)
        {
            FishingHookComponent comp = hookEntity.Get<FishingHookComponent>();
            comp.isCustomHook = 1;
            comp.hookPosition = pos;
        }

        EditModeController.SetSelect?.Invoke(fishingEntity);
    }

    private void OnAnchorCancelClick(Vector3 pos)
    {
        fishingBehav.EnableHookEffet(false);
        EditModeController.SetSelect?.Invoke(fishingEntity);
    }
    #endregion

    #region Helpers
    private void ToggleBtn(Button btn, Button edit, bool isOn)
    {
        GameObject check = btn.transform.GetChild(0).gameObject;
        check.SetActive(isOn);
        edit.gameObject.SetActive(isOn);
    }

    private void PartToDefault()
    {
        rodEntity.Get<FishingRodComponent>().isCustomPos = 0;
    }

    private void HookToDefault()
    {
        FishingHookComponent comp = hookEntity.Get<FishingHookComponent>();
        comp.isCustomHook = 0;
        comp.hookPosition = FishingEditManager.Inst.DefaultHookPoint;
    }
    #endregion

    public void RedoUndo(UndoRecord record, bool isUndo)
    {
        var data = (isUndo ? record.BeginData : record.EndData) as FishUndoData;
        var node = SecondCachePool.Inst.GetGameObjectByUid(data.uid);
        if (node != null)
        {
            var behav = node.GetComponent<NodeBaseBehaviour>();
            if (behav != null)
            {
                if (data.isRod)
                {
                    fishingBehav.transform.Find(FishingEditManager.Inst.RodParentPath).ClearChildren();
                    SecondCachePool.Inst.RevertEntity(node);
                    SetUi(behav.entity, hookEntity);
                }
                else
                {
                    fishingBehav.transform.Find(FishingEditManager.Inst.HookParentPath).ClearChildren();
                    SecondCachePool.Inst.RevertEntity(node);
                    SetUi(rodEntity, behav.entity);
                }

                EnableAllCollider();
            }
        }
    }

    private void EnableAllCollider()
    {
        var colliders = fishingBehav.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
            collider.enabled = true;
    }
}

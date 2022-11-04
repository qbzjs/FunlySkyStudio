using System;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public class SwingPanel : UgcChoosePanel<SwingPanel>, IUndoRecord
{
    private const string ropeText = "Adjust the length and spacing of the rope.";
    private const string seatText = "Adjust the seat as you want.";
    private const string spText = "Adjust the place where you want to sit.";

    private string[] ignoreLayer = new[]
    {
        "ShotExclude", "PVPArea", "SpecialModel", "Model", "TriggerModel", "Touch", "Prop", "Player", "WaterCube", "OtherPlayer",
        "PostProcess", "Trigger", "Head", "Ignore Raycast", "IceCube"
    };

    private int layer;
    private Dictionary<GameObject, int> state;

    private SceneEntity entity;
    private SwingBehaviour swingBehaviour;
    private SwingComponent swingComponent;
    private GizmoController ogController;
    private GizmoController gController;

    // [SerializeField] private Button btnChoosePropInNoPanel;
    // [SerializeField] private Button btnChoosePropInHasPanel;
    [SerializeField] private Toggle ropeTgl;
    [SerializeField] private Toggle seatTgl;
    [SerializeField] private Toggle spTgl;
    [SerializeField] private Toggle hideTgl;
    [SerializeField] private Button ropeEBtn;
    [SerializeField] private Button seatEBtn;
    [SerializeField] private Button spEBtn;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject editPanel;
    [SerializeField] private GameObject PRSPanel;
    [SerializeField] private Button editCloseBtn;
    [SerializeField] private Button editSureBtn;
    [SerializeField] private Button boardBtn;
    [SerializeField] private Button posEBtn;
    [SerializeField] private Button roteEBtn;
    [SerializeField] private Button scaleEBtn;
    [SerializeField] private Text tips;
    [SerializeField] private GameObject SelectBg;
    private UgcChooseItem lastUgcChooseItem;

    private Vector3 defaultPos;
    private Vector3 defaultRota;
    private Vector3 defaultScale;

    private Action sa, ca;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        layer = LayerMask.NameToLayer("Anchors");
        ropeTgl.onValueChanged.AddListener(OnRopeTglClick);
        seatTgl.onValueChanged.AddListener(OnSeatTglClick);
        spTgl.onValueChanged.AddListener(OnSpTglClick);
        hideTgl.onValueChanged.AddListener(OnShowTglClick);
        ropeEBtn.onClick.AddListener(OnRopeEBtnClick);
        seatEBtn.onClick.AddListener(OnSeatEBtnClick);
        spEBtn.onClick.AddListener(OnSpEBtnClick);
        editCloseBtn.onClick.AddListener(OnEditCloseClick);
        editSureBtn.onClick.AddListener(OnEditSureBtnClick);
        boardBtn.onClick.AddListener(OnBoardBtnClick);
        posEBtn.onClick.AddListener(OnPosEBtnClick);
        roteEBtn.onClick.AddListener(OnRoteEBtnClick);
        scaleEBtn.onClick.AddListener(OnScaleEBtnClick);
        ogController = GameObject.Find("GameStart").GetComponent<GameController>().editController.gController;
        gController = new GizmoController();
        gController.NeedRecord = false;
        gController.DisableGizmo();
    }

    private void InitUI()
    {
        if (!entity.HasComponent<SwingComponent>()) return;
        var com = entity.Get<SwingComponent>();
        var isOrg = isOriginVec3(com.ropePos, com.ropeRote, com.ropeScale);
        ropeTgl.SetIsOnWithoutNotify(!isOrg);
        ropeTgl.graphic.canvasRenderer.SetAlpha(isOrg ? 0f : 1f);
        ropeEBtn.gameObject.SetActive(!isOrg);

        isOrg = isOriginVec3(com.seatPos, com.seatRote, com.seatScale);
        seatTgl.SetIsOnWithoutNotify(!isOrg);
        seatTgl.graphic.canvasRenderer.SetAlpha(isOrg ? 0f : 1f);
        seatEBtn.gameObject.SetActive(!isOrg);

        isOrg = com.sitPos == null || com.sitPos == Vector3.zero;
        spTgl.SetIsOnWithoutNotify(!isOrg);
        spTgl.graphic.canvasRenderer.SetAlpha(isOrg ? 0f : 1f);
        spEBtn.gameObject.SetActive(!isOrg);

        hideTgl.SetIsOnWithoutNotify(com.hide);
        hideTgl.graphic.canvasRenderer.SetAlpha(!com.hide ? 0f : 1f);
        
        SelectBg.SetActive(true);
        if (lastUgcChooseItem != null)
        {
            lastUgcChooseItem.selectBg.gameObject.SetActive(false);
        }
    }

    private void OnScaleEBtnClick()
    {
        gController.SetScaleCtr(null, true);
    }

    private void OnRoteEBtnClick()
    {
        gController.SetRotateCtr();
    }

    private void OnPosEBtnClick()
    {
        gController.SetMoveCtr();
    }

    private void OnBoardBtnClick()
    {
        swingBehaviour.SetBoard();
        SelectBg.SetActive(true);
        if (lastUgcChooseItem != null)
        {
            lastUgcChooseItem.selectBg.gameObject.SetActive(false);
        }
    }

    private void OnEditSureBtnClick()
    {
        mainShowEditHide();
        sa?.Invoke();
        sa = null;
    }

    private void OnEditCloseClick()
    {
        mainShowEditHide();
        ca?.Invoke();
        ca = null;
    }

    private void OnSpEBtnClick()
    {
        tips.text = spText;
        var target = swingBehaviour.GetSit();
        PRSPanel.SetActive(false);
        getPRS(target);
        sa = () =>
        {
            if (!entity.HasComponent<SwingComponent>()) return;
            var com = entity.Get<SwingComponent>();
            com.sitPos = target.localPosition;
            spEBtn.gameObject.SetActive(true);
            var isOrg = isOrigin(target);
            spTgl.SetIsOnWithoutNotify(!isOrg);
            spTgl.graphic.canvasRenderer.SetAlpha(isOrg ? 0f : 1f);
            spEBtn.gameObject.SetActive(!isOrg);
            PRSPanel.SetActive(true);
        };
        ca = () =>
        {
            defaultPRS(target);
            var isOrg = isOrigin(target);
            spTgl.SetIsOnWithoutNotify(!isOrg);
            spTgl.graphic.canvasRenderer.SetAlpha(isOrg ? 0f : 1f);
            spEBtn.gameObject.SetActive(!isOrg);
            PRSPanel.SetActive(true);
        };
        editShowMainHide(target.gameObject);
    }

    private void OnSeatEBtnClick()
    {
        tips.text = seatText;
        var target = swingBehaviour.GetBoard();
        getPRS(target);
        sa = () =>
        {
            if (!entity.HasComponent<SwingComponent>()) return;
            var com = entity.Get<SwingComponent>();
            com.seatPos = target.localPosition;
            com.seatRote = target.localEulerAngles;
            com.seatScale = target.localScale;
            seatEBtn.gameObject.SetActive(true);
            var isOrg = isOrigin(target);
            seatTgl.SetIsOnWithoutNotify(!isOrg);
            seatTgl.graphic.canvasRenderer.SetAlpha(isOrg ? 0f : 1f);
            seatEBtn.gameObject.SetActive(!isOrg);
        };
        ca = () =>
        {
            defaultPRS(target);
            var isOrg = isOrigin(target);
            seatTgl.SetIsOnWithoutNotify(!isOrg);
            seatTgl.graphic.canvasRenderer.SetAlpha(isOrg ? 0f : 1f);
            seatEBtn.gameObject.SetActive(!isOrg);
        };
        editShowMainHide(target.gameObject);
    }

    private void OnRopeEBtnClick()
    {
        tips.text = ropeText;
        var target = swingBehaviour.GetRope();
        getPRS(target);
        sa = () =>
        {
            if (!entity.HasComponent<SwingComponent>()) return;
            var com = entity.Get<SwingComponent>();
            com.ropePos = target.localPosition;
            com.ropeRote = target.localEulerAngles;
            com.ropeScale = target.localScale;
            ropeEBtn.gameObject.SetActive(true);
            var isOrg = isOrigin(target);
            ropeTgl.SetIsOnWithoutNotify(!isOrg);
            ropeTgl.graphic.canvasRenderer.SetAlpha(isOrg ? 0f : 1f);
            ropeEBtn.gameObject.SetActive(!isOrg);
        };
        ca = () =>
        {
            defaultPRS(target);
            var isOrg = isOrigin(target);
            ropeTgl.SetIsOnWithoutNotify(!isOrg);
            ropeTgl.graphic.canvasRenderer.SetAlpha(isOrg ? 0f : 1f);
            ropeEBtn.gameObject.SetActive(!isOrg);
        };
        editShowMainHide(target.gameObject);
    }

    private void OnShowTglClick(bool isOn)
    {
        swingBehaviour.GetRope().gameObject.SetActive(!isOn);
        if (!entity.HasComponent<SwingComponent>()) return;
        var com = entity.Get<SwingComponent>();
        com.hide = isOn;
    }

    private void OnSpTglClick(bool isOn)
    {
        if (isOn)
        {
            OnSpEBtnClick();
        }
        else
        {
            PRSToOrigin(swingBehaviour.GetSit());
        }
    }

    private void OnSeatTglClick(bool isOn)
    {
        if (isOn)
        {
            OnSeatEBtnClick();
        }
        else
        {
            PRSToOrigin(swingBehaviour.GetBoard());
        }
    }

    private void OnRopeTglClick(bool isOn)
    {
        if (isOn)
        {
            OnRopeEBtnClick();
        }
        else
        {
            PRSToOrigin(swingBehaviour.GetRope());
        }
    }

    protected override void AfterUgcCreateFinish(NodeBaseBehaviour item, string rId)
    {
        swingBehaviour.SetBoard(item.transform);
        item.GetComponent<UGCCombBehaviour>().norScale = true;
        item.entity.Get<SwingComponent>();
        if (!entity.HasComponent<SwingComponent>()) return;
        var com = entity.Get<SwingComponent>();
        com.rId = rId;
        SwingManager.Inst.SaveRid(rId);
        SelectBg.SetActive(false);
        ChooseItem(rId);
    }
    
    protected override List<string> GetAllUgcRidList()
    {
        return SwingManager.Inst.GetAllUGC();
    }

    public override void SetLastChooseUgcItem(UgcChooseItem item)
    {
        lastUgcChooseItem = item;
    }
    
    public override void OnUgcChooseItemClick(UgcChooseItem item)
    {
        SelectBg.SetActive(false);
        base.OnUgcChooseItemClick(item);
    }

    protected override bool DestroySelf()
    {
        return false;
    }

    protected override bool SelectUgc()
    {
        return false;
    }

    public override void SetEntity(SceneEntity entity)
    {
        this.entity = entity;
        swingBehaviour = entity.Get<GameObjectComponent>().bindGo.GetComponentInChildren<SwingBehaviour>();
        swingComponent = entity.Get<SwingComponent>();
        curBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NodeBaseBehaviour>();
        InitUI();
        if (!string.IsNullOrEmpty(swingComponent.rId))
        {
            SelectBg.SetActive(false);
            ChooseItem(swingComponent.rId);
        }

        // var matData = GetMatDataById(comp.mat);
        // var matSelectIndex = matDatas.FindIndex(x => x.id == comp.mat);
        // RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, matData);
        // SetMatSelect(matSelectIndex);
        // var colorSelectIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(modelColor), colorDatas.List);
        // SetEntitySelectColor(colorSelectIndex, comp.color);
        // SetSettingData();
    }

    public override void AddTabListener()
    {
    }

    public void AddRecord(UndoRecord record)
    {
    }

    private void getPRS(Transform trs)
    {
        defaultPos = trs.localPosition;
        defaultRota = trs.localEulerAngles;
        defaultScale = trs.localScale;
    }

    private void defaultPRS(Transform trs)
    {
        trs.localPosition = defaultPos;
        trs.localEulerAngles = defaultRota;
        trs.localScale = defaultScale;
    }

    private void PRSToOrigin(Transform trs)
    {
        trs.localPosition = Vector3.zero;
        trs.localEulerAngles = Vector3.zero;
        trs.localScale = Vector3.one;
    }

    private bool isOrigin(Transform trs)
    {
        return trs.localPosition == Vector3.zero
               && trs.localEulerAngles == Vector3.zero
               && trs.localScale == Vector3.one;
    }

    private bool isOriginVec3(Vec3 p, Vec3 r, Vec3 s)
    {
        if (p == null || r == null || s == null)
        {
            return true;
        }

        return p == Vector3.zero
               && r == Vector3.zero
               && s == Vector3.one;
    }

    private void switchLayer()
    {
        if (state == null)
        {
            state = new Dictionary<GameObject, int>();
            var list = swingBehaviour.GetComponentsInChildren<Transform>(true);
            foreach (var v in list)
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
                kv.Key.layer = kv.Value;
            }

            state = null;
        }
    }

    public void editShowMainHide(GameObject obj)
    {
        mainPanel.SetActive(false);
        editPanel.SetActive(true);
        UIManager.Inst.SwitchDialog();
        switchLayer();
        Show();
        ogController.DisableGizmo();
        gController.SetTarget(obj);
        EditModeController.IsCanSelect = false;
        foreach (var s in ignoreLayer)
        {
            GameManager.Inst.MainCamera.RemoveLayer(LayerMask.NameToLayer(s));
        }
    }

    public void mainShowEditHide()
    {
        mainPanel.SetActive(true);
        editPanel.SetActive(false);
        UIManager.Inst.SwitchDialog();
        gController.DisableGizmo();
        ogController.SetTarget(swingBehaviour.gameObject);
        switchLayer();
        
        EditModeController.IsCanSelect = true;
        foreach (var s in ignoreLayer)
        {
            GameManager.Inst.MainCamera.AddLayer(LayerMask.NameToLayer(s));
        }

        if (ModelHandlePanel.Instance)
        {
            ModelHandlePanel.Instance.ResetGizmoState();
        }
    }
    
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        UpdateCustomizePanel();
    }
}
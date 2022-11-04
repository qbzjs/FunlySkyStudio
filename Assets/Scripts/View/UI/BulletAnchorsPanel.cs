using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BulletAnchorsPanel : BasePanel<BulletAnchorsPanel>
{
    public static string nodeName = "BulletAnchors";
    private GameObject obj;
    private GameObject target;
    private Transform trs;
    private GizmoController ogController;
    private GizmoController gController;
    private bool InputReceiverLocked;
    private Dictionary<GameObject, int> state;
    private int layer;
    private string[] ignoreLayer = new[] { "ShotExclude", "PVPArea", "SpecialModel", "Model", "TriggerModel", "Touch", "Prop", "Player", "WaterCube", "OtherPlayer", "PostProcess", "Trigger", "Head", "Ignore Raycast", "IceCube" };
    private SceneEntity entity;

    [SerializeField] private Button CancelBtn;
    [SerializeField] private Button SureBtn;

    public static bool IsActive = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        layer = LayerMask.NameToLayer("Anchors");
        ogController = GameObject.Find("GameStart").GetComponent<GameController>().editController.gController;
        ogController.DisableGizmo();
        BasePrimitivePanel.DisSelect();
        UIManager.Inst.SwitchDialog();
        CancelBtn.onClick.AddListener(OnCancelClick);
        SureBtn.onClick.AddListener(OnSureClick);
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        EditModeController.IsCanSelect = false;
        foreach (var s in ignoreLayer)
        {
            GameManager.Inst.MainCamera.RemoveLayer(LayerMask.NameToLayer(s));
        }
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        EditModeController.IsCanSelect = true;
        foreach (var s in ignoreLayer)
        {
            GameManager.Inst.MainCamera.AddLayer(LayerMask.NameToLayer(s));
        }
        ResetGizmoType();
    }

    private void OnCancelClick()
    {
        trs.localPosition = ShootWeaponManager.Inst.GetAnchors(entity);
        OnHidePanel();
    }

    private void OnSureClick()
    {
        ShootWeaponManager.Inst.SetAnchors(entity, trs.localPosition);
        OnHidePanel();
    }

    private void OnHidePanel()
    {
        InputReceiver.locked = InputReceiverLocked;
        ogController.SetTarget(target);
        SceneGizmoPanel.Show();
        BasePrimitivePanel.Show();
        LockHideManager.Inst.CheckHidePanelVisable();
        UIManager.Inst.SwitchDialog();
        gController.DisableGizmo();
        gController = null;
        trs.gameObject.SetActive(false);
        Hide();
        switchLayer();
    }

    private void ResetGizmoType()
    {
        if (ModelHandlePanel.Instance)
        {
            ModelHandlePanel.Instance.ResetGizmoState();
        }
    }

    private void OnEnable()
    {
        IsActive = true;
    }

    private void OnDisable()
    {
        IsActive = false;
    }

    public void Init(SceneEntity entity, Vector3 pos)
    {
        this.entity = entity;
        target = entity.Get<GameObjectComponent>().bindGo;
        trs = target.transform.Find(nodeName);
        if (trs)
        {
            obj = trs.gameObject;
        }
        else
        {
            obj = Instantiate(ShootWeaponPanel.Instance.bulletPrefab);
            obj.name = nodeName;
            trs = obj.transform;
            trs.SetParent(target.transform);
            trs.localPosition = Vector3.zero;
        }

        trs.localPosition = pos;
        trs.gameObject.SetActive(true);
        gController = new GizmoController();
        gController.NeedRecord = false;
        gController.SetTarget(obj);
        InputReceiverLocked = InputReceiver.locked;
        InputReceiver.locked = false;
        switchLayer();
    }

    private void switchLayer()
    {
        if (state == null)
        {
            state = new Dictionary<GameObject, int>();
            var list = target.GetComponentsInChildren<Transform>(true);
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
                if(kv.Key != null)
                {
                    kv.Key.layer = kv.Value;
                }
            }

            state = null;
        }
    }
}

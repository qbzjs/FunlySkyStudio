using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Author:Meimei-LiMei
/// Description:通用设置锚点panel
/// Date: 2022/7/22 16:59:53
/// </summary>
public class CommonEditAnchorsPanel : BasePanel<CommonEditAnchorsPanel>
{
    public string nodeName = "Anchors";
    public Text TitleText;
    public Action<Vector3> SureBtnClickAct;
    public Action<Vector3> CancelBtnClickAct;
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
    public void SetTitle(string titleTxt, params object[] formatArgs)
    {
        LocalizationConManager.Inst.SetLocalizedContent(TitleText, titleTxt, formatArgs);
    }
    public void SetNodeName(string nodeName)
    {
        this.nodeName = nodeName;
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
    private void ResetGizmoType()
    {
        if (ModelHandlePanel.Instance)
        {
            ModelHandlePanel.Instance.ResetGizmoState();
        }
    }

    private void OnCancelClick()
    {
        OnHidePanel();
        CancelBtnClickAct?.Invoke(trs.localPosition);
    }

    private void OnSureClick()
    {
        OnHidePanel();
        SureBtnClickAct?.Invoke(trs.localPosition);
        FireworkManager.Inst.SetAnchors(entity, trs.localPosition);
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
            obj = new GameObject(nodeName);
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
                if (kv.Key != null)
                {
                    kv.Key.layer = kv.Value;
                }
            }
            state = null;
        }
    }
}

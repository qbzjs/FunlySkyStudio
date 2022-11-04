/// <summary>
/// Author:Mingo-LiZongMing
/// Description:可捕捉属性的属性面板
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatchabilityPanel : BasePanel<CatchabilityPanel>
{
    public Toggle CatchableToggle;
    public Toggle UnCatchableToggle;
    public List<GameObject> hidePanel;

    private SceneEntity _entity;

    public void SetEntity(SceneEntity entity)
    {
        _entity = entity;
        var hasCatchability = FishingManager.Inst.HasCatchability(entity);
        CatchableToggle.isOn = hasCatchability;
        UnCatchableToggle.isOn = !hasCatchability;
        SetClickPanelState(hasCatchability);
    }

    protected override void Awake()
    {
        base.Awake();
        CatchableToggle.onValueChanged.AddListener(OnCatchableClick);
        UnCatchableToggle.onValueChanged.AddListener(OnUnCatchableClick);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CatchableToggle.onValueChanged.RemoveAllListeners();
        UnCatchableToggle.onValueChanged.RemoveAllListeners();
    }

    private void OnCatchableClick(bool isOn)
    {
        if (isOn)
        {
            if (IsConstainSpecialComp(_entity))
            {
                //TODO: 根据产品定义 - 可捕捉的优先级最高，所以打开可捕获属性之后，需要取消跟随模式
                ModelPropertyPanel.Instance.CloseFollowMode();
            }
            if (!FishingManager.Inst.CheckCurCountCanSetCatchability())
            {
                UnCatchableToggle.isOn = true;
                return;
            }

            if (!FishingManager.Inst.CheckCanSetCatchability(_entity))
            {
                UnCatchableToggle.isOn = true;
                LoggerUtils.LogError("This Prop Cant Set Catchablility ");
                return;
            }
            if (FishingManager.Inst.HasCatchability(_entity))
            {
                return;
            }
            FishingManager.Inst.AddCatchability(_entity);
            PickabilityManager.Inst.AddPickablityProp(_entity, _entity.Get<PickablityComponent>().anchors);
            ModelPropertyPanel.Instance.RefreshAttributePanel();
            SetClickPanelState(true);
        }
    }

    private void OnUnCatchableClick(bool isOn)
    {
        if (isOn)
        {
            if (!FishingManager.Inst.HasCatchability(_entity))
            {
                return;
            }
            FishingManager.Inst.RemoveCatchability(_entity);

            if (_entity.HasComponent<PickablityComponent>())
            {
                PickabilityManager.Inst.RemovePickablityProp(_entity);
            }
            ModelPropertyPanel.Instance.RefreshAttributePanel();
            SetClickPanelState(false);
        }
    }

    private bool IsConstainSpecialComp(SceneEntity entity)
    {
        if (entity.HasComponent<FollowableComponent>() && entity.Get<FollowableComponent>().moveType == (int)MoveMode.Follow)
        {
            return true;
        }
        return false;
    }

    private void SetClickPanelState(bool active)
    {
        var tempColor = !active == true ? Color.white : new Color(0.58f, 0.58f, 0.58f, 1f);
        foreach (var panel in hidePanel)
        {
            var comps = panel.GetComponentsInChildren<MaskableGraphic>(true);
            foreach (var comp in comps)
            {
                comp.color = tempColor;
            }
        }
    }
}

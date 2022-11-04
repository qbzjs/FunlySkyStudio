// EdibilityPanel.cs
// Created by xiaojl Jul/22/2022
// 可食用属性面板

using UnityEngine;
using UnityEngine.UI;

public class EdibilityPanel : BasePanel<EdibilityPanel>
{
    public Toggle EatableToggle;
    public Toggle UneatableToggle;
    public Toggle EatModeToggle;
    public Toggle DrinkModeToggle;
    public GameObject ModeGroupPanel;

    public RectTransform AttributeLayScollRect;
    private SceneEntity _entity;

    public void SetEntity(SceneEntity entity)
    {
        _entity = entity;

        var hasEdibility = EdibilityManager.Inst.HasEdibilityProp(_entity);
        ModeGroupPanel.SetActive(hasEdibility);
        EatableToggle.isOn = hasEdibility;
        UneatableToggle.isOn = !hasEdibility;
        if (hasEdibility)
        {
            // 获取食用模式
            var mode = EdibilityManager.Inst.GetEdibilityMode(_entity);
            EatModeToggle.isOn = mode == EdibilityMode.Eat;
            DrinkModeToggle.isOn = mode == EdibilityMode.Drink;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // 监听事件
        EatableToggle.onValueChanged.AddListener(OnEatableClick);
        UneatableToggle.onValueChanged.AddListener(OnUneatableClick);
        EatModeToggle.onValueChanged.AddListener(OnEatModeClick);
        DrinkModeToggle.onValueChanged.AddListener(OnDrinkModeClick);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 移除事件
        EatableToggle.onValueChanged.RemoveAllListeners();
        UneatableToggle.onValueChanged.RemoveAllListeners();
        EatModeToggle.onValueChanged.RemoveAllListeners();
        DrinkModeToggle.onValueChanged.RemoveAllListeners();
    }

    private void OnEatableClick(bool isOn)
    {
        if (isOn)
        {
            if (IsConstainSpecialComp(_entity))
            {
                UneatableToggle.isOn = true;
                return;
            }
            // 不允许食用，返回
            if (! EdibilityManager.Inst.CheckEdibility(_entity))
            {
                UneatableToggle.isOn = true;
                LoggerUtils.LogError(string.Format("Set Eatable Failed. Entity id = {0}", _entity.Id));
                return;
            }

            // 已有可食用属性，返回
            if (EdibilityManager.Inst.HasEdibilityProp(_entity))
                return;

            // 添加可食用属性
            EdibilityManager.Inst.AddEdibilityProp(_entity);

            // 显示食用模式面板
            ModeGroupPanel.SetActive(true);

            // 默认设置为吃东西模式
            EdibilityManager.Inst.SetEdibilityMode(_entity, EdibilityMode.Eat);
            EatModeToggle.isOn = true;

            UpdateAttributePanel();
        }
    }

    private void OnUneatableClick(bool isOn)
    {
        if (isOn)
        {
            // 没有可食用属性，返回
            if (! EdibilityManager.Inst.HasEdibilityProp(_entity))
                return;

            // 移除可食用属性
            EdibilityManager.Inst.RemoveEdibilityProp(_entity);

            // 隐藏食用模式面板
            ModeGroupPanel.SetActive(false);

            UpdateAttributePanel();
        }
    }

    private void OnEatModeClick(bool isOn)
    {
        if (isOn)
        {
            // 获取当前食用模式
            var mode = EdibilityManager.Inst.GetEdibilityMode(_entity);

            // 如果已经是吃东西模式，返回
            if (mode == EdibilityMode.Eat)
                return;

            // 设置为吃东西模式
            EdibilityManager.Inst.SetEdibilityMode(_entity, EdibilityMode.Eat);
        }
    }

    private void OnDrinkModeClick(bool isOn)
    {
        if (isOn)
        {
            // 获取当前食用模式
            var mode = EdibilityManager.Inst.GetEdibilityMode(_entity);

            // 如果已经是喝东西模式，返回
            if (mode == EdibilityMode.Drink)
                return;

            // 设置为喝东西模式
            EdibilityManager.Inst.SetEdibilityMode(_entity, EdibilityMode.Drink);
        }
    }

    private void UpdateAttributePanel()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(AttributeLayScollRect);
    }

    private bool IsConstainSpecialComp(SceneEntity entity)
    {
        if(entity.HasComponent<FollowableComponent>() && entity.Get<FollowableComponent>().moveType == (int)MoveMode.Follow)
        {
            return true;
        }
        return false;
    }
}

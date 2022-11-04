using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SavingData;

/// <summary>
/// Author:MeiMei
/// Description:烟花UI
/// Date: 2022/7/20 14:10:14
/// </summary>
public enum Constraint
{
    FixedColumnCount,
    FixedRowCount
}

public class FireworkPanel : UgcChoosePanel<FireworkPanel>, IUndoRecord
{
    enum PanelShowType
    {
        Color,
        Setting
    };

    private SceneEntity curEntity;
    public Button editBtn;
    public Toggle isEditToggle, tapToggle;
    public Toggle[] heightToggle;
    public PropertySwitchPanel switchPanel;
    public PropertyCollectiblesPanel collectiblesPanel;
    public PropertySensorBoxPanel sensorBoxPanel;
    private Dictionary<int, FireworkHeight> fireworkHeightDics = new Dictionary<int, FireworkHeight>()
    {
        {3,FireworkHeight.Low},
        {7,FireworkHeight.Medium},
        {12,FireworkHeight.Heigh}
    };

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        GetColorDatas();
        CreatePanel();
        CreateColorItems();
        UpdateCustomizePanel();
        AddCommonListener();
        InitControlPanel();
        InitToggle();
    }

    public override void GetColorDatas()
    {
        colorDatas = ResManager.Inst.LoadRes<ColorLibrary>("ConfigAssets/UGCClothesColorLibrary");
    }

    private void CreatePanel()
    {
        tabGroup = transform.GetComponentInChildren<TabSelectGroup>(true);
        tabGroup.isExpand = true;//默认展开
        var tab = tabGroup.GetTab((int)PanelShowType.Color);
        colorNormalScroll = tab.normalPanel.GetComponentInChildren<ScrollRect>(true);
        ScrollRect rect = tab.expandPanel.GetComponentInChildren<ScrollRect>(true);
        colorExpandScroll = rect.GetComponentInChildren<ScrollRect>(true);
        Button[] customizeBtns = tab.expandPanel.GetComponentsInChildren<Button>(true);
        delectBtn = customizeBtns[0];
        confirmBtn = customizeBtns[1];
        GridLayoutGroup[] grids = tab.expandPanel.GetComponentsInChildren<GridLayoutGroup>(true);
        GridLayoutGroup colorCustomizeGrid = grids[1];
        for (int i = 0; i < colorCustomizeGrid.transform.childCount; i++)
        {
            customizeItems.Add(colorCustomizeGrid.transform.GetChild(i).gameObject);
        }
    }

    public override void SetColor(int index, ColorSelectType type)
    {
        var beginData = CreateUndoData(TextSelectType.Color, colorType, (int)PanelShowType.Color, tabGroup.isExpand);
        Color color = GetCurColor(type, index);
        SetSliderColor(color);
        SetColorSelect(type, index);
        SetColor(color);
        colorStr = DataUtils.ColorToString(color);
        colorType = type;
        colorId = index;
        var endData = CreateUndoData(TextSelectType.Color, colorType, (int)PanelShowType.Color, tabGroup.isExpand);
        AddRecord(beginData, endData);
    }

    public override void SetColor(Color color)
    {
        if (curEntity.HasComponent<FireworkComponent>())
        {
            var comp = curEntity.Get<FireworkComponent>();
            comp.fireworkcolor = DataUtils.ColorToString(color);
        }
    }

    public void SelectColorUndo(int type, Color color, int tabId, bool isExpand)
    {
        colorStr = DataUtils.ColorToString(color);
        SetSelectColor(color, (ColorSelectType)type);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
        RefreshScrollPanel(ShowType.Color, isExpand);
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        tabGroup.OpenPreTab();
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
        UpdateCustomizePanel();
    }

    public override void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        RefreshUI();
        curBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NodeBaseBehaviour>();
        if (curBehav is FireworkBehaviour)
        {
            //默认占位回血道具
            ChooseItem(string.Empty);
        }
        else if (curBehav is UGCCombBehaviour && entity.HasComponent<FireworkComponent>())
        {
            //UGC回血道具
            string rId = entity.Get<FireworkComponent>().rId;
            ChooseItem(rId);
        }
        if (entity.HasComponent<FireworkComponent>())
        {
            var comp = curEntity.Get<FireworkComponent>();
            InitPanelStateByComp(comp);
        }
        InitTriggerEntity();
    }

    #region  UI处理
    private void InitPanelStateByComp(FireworkComponent comp)
    {
        int colorIndex = DataUtils.GetColorSelect((comp.fireworkcolor), colorDatas.List);
        if (colorIndex < 0)
        {
            colorIndex = GetColorIndex(Color.white);
        }
        colorId = colorIndex;
        var color = colorDatas.Get(colorId);
        colorStr = DataUtils.ColorToString(color);
        SetEntitySelectColor(colorId, DataUtils.ColorToString(color));
        SetFireworkColor(colorIndex);
        //Trigger
        var isOn = comp.isControl == (int)FireworkControl.NOT_SUPPORT;
        tapToggle.isOn = isOn;
        //point
        var isSelect = comp.isCustomPoint == (int)FireworkPointState.Off ? false : true;
        isEditToggle.isOn = isSelect;
        editBtn.gameObject.SetActive(isSelect);
    }

    //设置烟花颜色
    private void SetFireworkColor(int index)
    {
        colorId = index;
        var color = colorDatas.Get(colorId);
        curEntity.Get<FireworkComponent>().fireworkcolor = DataUtils.ColorToString(color);
    }

    //初始化Toggle
    private void InitToggle()
    {
        isEditToggle.onValueChanged.AddListener(OnFireworkPointTogClick);
        tapToggle.onValueChanged.AddListener(OnTapClick);
        editBtn.onClick.AddListener(OnEditClick);
    }

    //设置烟花高度
    private void SetFirworkHeight(int index)
    {
        var comp = curEntity.Get<FireworkComponent>();
        switch (index)
        {
            case (int)FireworkHeight.Low:
                comp.fireworkHeight = 3;
                break;
            case (int)FireworkHeight.Medium:
                comp.fireworkHeight = 7;
                break;
            case (int)FireworkHeight.Heigh:
                comp.fireworkHeight = 12;
                break;
        }
    }

    //烟花发射点Toggle事件
    private void OnFireworkPointTogClick(bool isOn)
    {
        if (!curEntity.HasComponent<FireworkComponent>())
        {
            return;
        }
        var comp = curEntity.Get<FireworkComponent>();
        if (comp.rId == FireworkManager.DEFAULT_MODEL)
        {
            TipPanel.ShowToast("Please set the object first");
            return;
        }
        if (isOn)
        {
            if (comp.isCustomPoint == (int)FireworkPointState.Off)
            {
                OnEditClick();
            }
            comp.isCustomPoint = (int)FireworkPointState.On;
        }
        else
        {
            FireworkManager.Inst.SetAnchors(curEntity, Vector3.zero);
            comp.isCustomPoint = (int)FireworkPointState.Off;

        }
        editBtn.gameObject.SetActive(isOn);
    }

    //编辑发射点处理
    private void OnEditClick()
    {
        CommonEditAnchorsPanel.Show();
        CommonEditAnchorsPanel.Instance.SetNodeName("FireworkAnchros");
        CommonEditAnchorsPanel.Instance.SetTitle("Fireworks will be launched at the launch point you set");
        var pos = FireworkManager.Inst.GetAnchors(curEntity);
        CommonEditAnchorsPanel.Instance.Init(curEntity, pos);
        CommonEditAnchorsPanel.Instance.SureBtnClickAct = OnSureClick;
        CommonEditAnchorsPanel.Instance.CancelBtnClickAct = OnCancelClick;
    }

    public void OnSureClick(Vector3 pos)
    {
        FireworkManager.Inst.SetAnchors(curEntity, pos);
    }

    public void OnCancelClick(Vector3 pos)
    {
        pos = FireworkManager.Inst.GetAnchors(curEntity);
    }

    private void InitTriggerEntity()
    {
        switchPanel.SetEntity(curEntity);
        collectiblesPanel.SetEntity(curEntity);
        sensorBoxPanel.SetEntity(curEntity);
    }

    private void OnTapClick(bool isOn)
    {
        var ctrl = FireworkControl.SUPPORT_CTRL_Firework;
        if (isOn)
        {
            ctrl = FireworkControl.NOT_SUPPORT;
        }
        curEntity.Get<FireworkComponent>().isControl = (int)ctrl;
    }

    private void InitControlPanel()
    {
        switchPanel.CtrlType = SwitchControlType.FIREWORK_CONTROL;
        switchPanel.Init();
        collectiblesPanel.CtrlType = CollectControlType.FIREWORK_CONTROL;
        collectiblesPanel.Init();
        sensorBoxPanel.CtrlType = PropControlType.FIREWORK_CONTROL;
        sensorBoxPanel.Init();
    }

    #endregion

    #region  UGC素材选择处理
    /// <summary>
    /// 选择了UGC素材后回调
    /// </summary>
    protected override void ChooseUgcCallback(MapInfo mapInfo)
    {
        base.ChooseUgcCallback(mapInfo);
    }

    /// <summary>
    /// 点击Item，切换UGC素材，用新素材来展现道具
    /// </summary>
    public override void OnUgcChooseItemClick(UgcChooseItem fireworkItem)
    {
        base.OnUgcChooseItemClick(fireworkItem);
    }

    protected override List<string> GetAllUgcRidList()
    {
        return FireworkManager.Inst.GetAllUgcRidList();
    }

    //添加ugc素材后添加烟花组件
    protected override void AfterUgcCreateFinish(NodeBaseBehaviour nBehav, string rId)
    {
        FireworkManager.Inst.AddFireworkComponent(nBehav, rId);
        FireworkManager.Inst.AddUgcFireworkItem(rId, nBehav);
    }

    //设置最后选择的ugc
    public override void SetLastChooseUgcItem(UgcChooseItem fireworkItem)
    {
        FireworkManager.Inst.SetLastSelectFirework(fireworkItem);
    }

    #endregion
    //刷新UI
    protected override void RefreshUI()
    {
        base.RefreshUI();
        var allUsedUgcs = GetAllUgcRidList();
        if (allUsedUgcs != null)
        {
            goNoPanel.SetActive(allUsedUgcs.Count > 0 == false);
            goHasPanel.SetActive(allUsedUgcs.Count > 0);
        }
    }

    public override void AddTabListener()
    {
        var tab = tabGroup.GetTab((int)PanelShowType.Setting);
        tab.AddClickListener(OnSettingClick);
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        tab.AddClickListener(OnColorClick);
    }

    public void OnSettingClick()
    {
        OpenShowPanel(PanelShowType.Setting);
    }

    public void OnColorClick()
    {
        OpenShowPanel(PanelShowType.Color);
    }

    private void OpenShowPanel(PanelShowType type)
    {
        var isExpand = tabGroup.isExpand;
        if (type == PanelShowType.Color)
        {
            if (isExpand)
            {
                UpdateScrollPanel(colorItems, colorExpandScroll.content);
            }
            else
            {
                UpdateScrollPanel(colorItems, colorNormalScroll.content);
            }
        }
    }

    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }

    public void AddRecord(FireworkUndoData beginData, FireworkUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.FireworkUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private FireworkUndoData CreateUndoData(TextSelectType type, ColorSelectType colorType, int tabId = -1, bool isExpand = false)
    {
        var comp = curEntity.Get<FireworkComponent>();
        FireworkUndoData data = new FireworkUndoData();
        Color color = DataUtils.DeSerializeColor(comp.fireworkcolor);
        data.colorIndex = colorId;
        data.color = color;
        data.colorType = (int)colorType;
        data.targetEntity = curEntity;
        data.TextType = (int)type;
        data.tabId = tabId;
        data.isExpand = isExpand;
        return data;
    }
}

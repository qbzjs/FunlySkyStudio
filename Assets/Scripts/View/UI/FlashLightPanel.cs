using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author : Tee Li
/// 描述：手电灯面板
/// 日期：2022/10/08
/// </summary>

public class FlashLightPanel : CommonMatColorPanel<FlashLightPanel>, IUndoRecord
{
    enum FlashShowType
    {
        Color = 0,
        Settings = 1
    }

    public GameObject colorPanel;
    public GameObject settingPanel;
   
    private readonly string customGridPath = "ColorPanel/Customize/LayoutGrids";

    private SceneEntity curEntity;
    private FlashLightBehaviour curBehav;
    private FlashLightComponent curComp;

    private ToggleGroupItem typeItem;
    private ToggleGroupItem orderItem;
    private NumberInputItem timeItem;
    private SliderItem intenItem;
    private SliderItem rangeItem;
    private SliderItem radiusItem;
    private SwitchItem realLightItem;
    private Button hintBtn;
    private GameObject hintBox;

    private ColorListItemGroup colorListGroup;

    private readonly int maxTime = 60;
    private readonly int minTime = 1;
    private readonly Vector2 intenRange = new Vector2(0.1f, 0.8f);
    private readonly Vector2 rangeRange = new Vector2(1f, 10f);
    private readonly Vector2 radiusRange = new Vector2(0.2f, 15f);
    private readonly int colorMaxCount = 20;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        CreateColorPanel();
        CreateSettingPanel();
        GetColorData();
        CreateColorItems();
        CreateColorListPanel();
        UpdateCustomizePanel();

        AddCommonListener();
        AddListener();

        InitAfterCreation();
    }

    public override void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        curComp = curEntity.Get<FlashLightComponent>();
        curBehav = curEntity.Get<GameObjectComponent>().bindGo.GetComponent<FlashLightBehaviour>();
        RefreshUI();
    }

    private void RefreshUI()
    {
        switch ((FlashLightType)curComp.type)
        {
            case FlashLightType.Directional:
                typeItem.SetValueWithoutNotify("DirLight", true);
                break;
            case FlashLightType.SpotLight:
                typeItem.SetValueWithoutNotify("SpotLight", true);
                break;
        }

        switch ((FlashLightMode)curComp.mode)
        {
            case FlashLightMode.Queue:
                orderItem.SetValueWithoutNotify("Queue", true);
                break;
            case FlashLightMode.Random:
                orderItem.SetValueWithoutNotify("Random", true);
                break;
        }

        timeItem.SetValueWithoutNotify(curComp.time);

        intenItem.SetValueWithoutNotify(DataUtils.GetProgress(curComp.inten, intenRange.x, intenRange.y, intenItem.slideBar.minValue, intenItem.slideBar.maxValue));
        rangeItem.SetValueWithoutNotify(DataUtils.GetProgress(curComp.range, rangeRange.x, rangeRange.y, rangeItem.slideBar.minValue, rangeItem.slideBar.maxValue));
        radiusItem.SetValueWithoutNotify(DataUtils.GetProgress(curComp.radius, radiusRange.x, radiusRange.y, radiusItem.slideBar.minValue, radiusItem.slideBar.maxValue));

        realLightItem.SetValueWithoutNotify(curComp.isReal > 0);

        colorListGroup.RefreshList(curComp.colors);

        SetColorListSelect(curBehav.editModeColor);
    }

    #region Creation
    private void CreateColorPanel()
    {
        tabGroup = transform.GetComponentInChildren<TabSelectGroup>();
        tabGroup.isExpand = true;
        var tab = tabGroup.GetTab((int)FlashShowType.Color);
        colorNormalScroll = tab.normalPanel.GetComponentInChildren<ScrollRect>();
        Button[] customizeBtns = tab.normalPanel.GetComponentsInChildren<Button>();
        delectBtn = customizeBtns[0];
        confirmBtn = customizeBtns[1];
        Transform gridTrans = tab.normalPanel.transform.Find(customGridPath);
        GridLayoutGroup colorCustomizeGrid = gridTrans.GetComponentInChildren<GridLayoutGroup>();
        for (int i = 0; i < colorCustomizeGrid.transform.childCount; i++)
        {
            customizeItems.Add(colorCustomizeGrid.transform.GetChild(i).gameObject);
        }
    }

    private void CreateColorListPanel()
    {
        Transform content = colorPanel.transform.Find("ColorList").GetComponentInChildren<ScrollRect>().content;
        colorListGroup = new ColorListItemGroup(content);

        GameObject item = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "ColorListItem");
        colorListGroup.Create(item, colorMaxCount);

        colorListGroup.ApplyDefaultSelectListener();
        colorListGroup.AddSelectListener(OnColorSelect);
        colorListGroup.AddLastListener(OnColorLast);
        colorListGroup.AddNextListener(OnColorNext);
        colorListGroup.AddDelListener(OnColorDelete);        
    }

    private void CreateSettingPanel()
    {
        Transform content = settingPanel.GetComponentInChildren<ScrollRect>().content;
        typeItem = content.Find("Type").GetComponent<ToggleGroupItem>();
        orderItem = content.Find("Order").GetComponent<ToggleGroupItem>();
        timeItem = content.Find("Interval").GetComponent<NumberInputItem>();
        intenItem = content.Find("Intensity").GetComponent<SliderItem>();
        rangeItem = content.Find("Range").GetComponent<SliderItem>();
        radiusItem = content.Find("Radius").GetComponent<SliderItem>();
        realLightItem = content.Find("RealTime").GetComponent<SwitchItem>();
        hintBtn = realLightItem.transform.Find("Group/HintBtn/HintBtn").GetComponent<Button>();
        hintBox = content.Find("Hint").gameObject;
    }



    private void GetColorData()
    {
        colorDatas = AssetLibrary.Inst.colorLib;
    }

    public override void AddTabListener()
    {
        TabSelect tab = tabGroup.GetTab((int)FlashShowType.Color);
        tab.AddClickListener(() => {});
        tab = tabGroup.GetTab((int)FlashShowType.Settings);
        tab.AddClickListener(() => {});
    }

    private void AddListener()
    {
        typeItem.AddListener("DirLight", OnSelectDirlight);
        typeItem.AddListener("SpotLight", OnSelectSpotlight);
        orderItem.AddListener("Queue", OnSelectQueue);
        orderItem.AddListener("Random", OnSelectRandom);
        timeItem.AddListener(OnSetTime);
        timeItem.AddInputVerify(TestTime);
        timeItem.AddInvalidListener(OnInvalidTime);
        intenItem.AddListener(OnIntenChange);
        rangeItem.AddListener(OnRangeChange);
        radiusItem.AddListener(OnRadiusChange);
        realLightItem.AddListener(OnRealTime);
        hintBtn.onClick.AddListener(OnShowHintClick);
    }

    private void InitAfterCreation()
    {
        tabGroup.ResetTab();
        hintBox.gameObject.SetActive(false);
    }

    #endregion


    #region UI Callbacks

    private void OnSelectDirlight(bool isOn)
    {
        if (isOn)
        {
            curComp.type = (int)FlashLightType.Directional;
            curBehav.SetType(curComp.type);
        }      
    }

    private void OnSelectSpotlight(bool isOn)
    {
        if (isOn)
        {
            curComp.type = (int)FlashLightType.SpotLight;
            curBehav.SetType(curComp.type);
        }
    }

    private void OnSelectQueue(bool isOn)
    {
        if (isOn)
        {
            curComp.mode = (int)FlashLightMode.Queue;
        }
    }

    private void OnSelectRandom(bool isOn)
    {
        if (isOn)
        {
            curComp.mode = (int)FlashLightMode.Random;
        }
    }

    private void OnSetTime(int value)
    {
        curComp.time = value;
    }

    private bool TestTime(int value)
    {
        return (value >= minTime) && (value <= maxTime);
    }

    private void OnInvalidTime(int value)
    {
        if(value < minTime)
        {
            TipPanel.ShowToast("At least 1 second");
        }
        if(value > maxTime)
        {
            TipPanel.ShowToast("Up to 60 seconds");
        }
    }

    private void OnIntenChange(float value)
    {
        float realVal = DataUtils.GetRealValue(value, intenRange.x, intenRange.y, intenItem.slideBar.minValue, intenItem.slideBar.maxValue);
        curComp.inten = realVal;
        curBehav.SetIntensity(realVal);
    }

    private void OnRangeChange(float value)
    {
        float realVal = DataUtils.GetRealValue(value, rangeRange.x, rangeRange.y, rangeItem.slideBar.minValue, rangeItem.slideBar.maxValue);
        curComp.range = realVal;
        curBehav.SetRange(realVal);
    }

    private void OnRadiusChange(float value)
    {
        float realVal = DataUtils.GetRealValue(value, radiusRange.x, radiusRange.y, radiusItem.slideBar.minValue, radiusItem.slideBar.maxValue);
        curComp.radius = realVal;
        curBehav.SetRadius(realVal);
    }

    private void OnRealTime(bool isOn)
    {
        curComp.isReal = isOn ? 1 : 0;
        curBehav.SetIsReal(isOn);
    }

    private void OnShowHintClick()
    {
        bool isOn = !hintBox.gameObject.activeInHierarchy;
        hintBox.gameObject.SetActive(isOn);
        if (isOn)
        {
            settingPanel.GetComponentInChildren<ScrollRect>().velocity = Vector2.up * 500f;
        }
    }

    public override void SetColor(int index, ColorSelectType type)
    {
        base.SetColor(index, type);
        AddColor(GetCurColor(type, index));
    }

    public override void SetColor(Color color)
    {
        base.SetColor(color);
        ColorListItem item = colorListGroup.GetCurrentItem();
        if (item)
        {
            item.SetColor(color);
            curComp.colors[colorListGroup.CurItemIndex] = color;
            curBehav.SetColor(color);
            curBehav.editModeColor = color;
        }       
    }

    private void AddColor(Color color)
    {
        if (curComp.colors.Count < colorMaxCount)
        {
            FlashlightUndoData beginData = GetUndoData();

            curComp.colors.Add(color);
            colorListGroup.RefreshList(curComp.colors);
            colorListGroup.SelectOnly(curComp.colors.Count - 1);

            FlashlightUndoData endData = GetUndoData();
            AddRecord(beginData, endData);
        }
        else
        {
            TipPanel.ShowToast("Up to 20 colors can be selected");
        }
    }

    private void OnColorSelect(int i)
    {
        Color curColor = curComp.colors[i];
        curBehav.SetColor(curColor);
        curBehav.editModeColor = curColor;
        SetSliderColor(curColor);
    }

    private void SetColorListSelect(Color color)
    {
        SetSliderColor(color);
        colorListGroup.SelectColorWithoutNotify(color);
    }

    private void OnColorNext(int i)
    {
        if(i + 1 < curComp.colors.Count)
        {
            FlashlightUndoData beginData = GetUndoData();

            SwitchColor(curComp.colors, i, i + 1);
            colorListGroup.RefreshList(curComp.colors);
            colorListGroup.SelectOnly(i+1);

            FlashlightUndoData endData = GetUndoData();
            AddRecord(beginData, endData);
        }
    }

    private void OnColorLast(int i)
    {
        if (i > 0)
        {
            FlashlightUndoData beginData = GetUndoData();

            SwitchColor(curComp.colors, i, i - 1);
            colorListGroup.RefreshList(curComp.colors);
            colorListGroup.SelectOnly(i-1);

            FlashlightUndoData endData = GetUndoData();
            AddRecord(beginData, endData);
        }
    }

    private void OnColorDelete(int i)
    {
        if(curComp.colors.Count > 1)
        {
            FlashlightUndoData beginData = GetUndoData();

            curComp.colors.RemoveAt(i);
            colorListGroup.RefreshList(curComp.colors);
            int selectIndex = Mathf.Min(i, curComp.colors.Count - 1);
            colorListGroup.SelectOnly(selectIndex);

            FlashlightUndoData endData = GetUndoData();
            AddRecord(beginData, endData);
        }
        else
        {
            TipPanel.ShowToast("At least 1 color needed");
        }
    }



    #endregion

    #region UndoRedo

    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }

    public void SetColorUndo(List<Color> colors, int select)
    {
        curComp.colors.Clear();
        curComp.colors.AddRange(colors);
        colorListGroup.RefreshList(curComp.colors);
        colorListGroup.SelectOnly(select);
    }

    public void SelectUndoTab()
    {
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(0), false);
    }

    private FlashlightUndoData GetUndoData()
    {
        return new FlashlightUndoData
        {
            targetEntity = curEntity,
            colors = new List<Color>(curComp.colors),
            select = colorListGroup.CurItemIndex
        };
    }

    private void AddRecord(FlashlightUndoData beginData, FlashlightUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.FlashlightUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    #endregion

    private void SwitchColor(List<Color> colors, int i, int o)
    {
        Color temp = colors[i];
        colors[i] = colors[o];
        colors[o] = temp;
    }

}

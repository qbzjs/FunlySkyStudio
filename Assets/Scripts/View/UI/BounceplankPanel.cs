using System;
using System.Collections;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class BounceplankPanel : CommonMatColorPanel<BounceplankPanel>
{
    enum PanelShowType
    {
        Material = 0,
        Color = 1,
        Setting = 2,
    };
    public ToggleGroup shapeGroup;
    public ToggleGroup heightGroup;

    public Button AddTileBtn;
    public Button SubTileBtn;
    private float tileIncrement = 0.1f;
    private Color modelColor;
    private SceneEntity curEntity;
    private BounceplankBehaviour nBehaviour;
    private Dictionary<Toggle, BounceShape> shapeToggles = new Dictionary<Toggle, BounceShape>();
    private Dictionary<Toggle, BounceHeight> heightToggles = new Dictionary<Toggle, BounceHeight>();

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        CreatePanel();
        GetMatDatas();
        GetColorDatas();
        CreateColorItems();
        CreateMatItems();
        UpdateCustomizePanel();
        AddCommonListener();
        AddMatTypeListener();
        AddListener();
    }

    private void CreatePanel()
    {
        tabGroup = transform.GetComponentInChildren<TabSelectGroup>();
        tabGroup.isExpand = true;//默认展开
        var tab = tabGroup.GetTab((int)PanelShowType.Material);
        matNormalScroll = tab.normalPanel.GetComponentInChildren<ScrollRect>();
        matExpandScroll = tab.expandPanel.GetComponentInChildren<ScrollRect>();
        matTypeGroup = tab.expandPanel.GetComponentInChildren<ToggleGroup>();
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        colorNormalScroll = tab.normalPanel.GetComponentInChildren<ScrollRect>();
        ScrollRect rect = tab.expandPanel.GetComponentInChildren<ScrollRect>();
        colorExpandScroll = rect.GetComponentInChildren<ScrollRect>();
        Button[] customizeBtns = tab.expandPanel.GetComponentsInChildren<Button>();
        delectBtn = customizeBtns[0];
        confirmBtn = customizeBtns[1];
        GridLayoutGroup[] grids = tab.expandPanel.GetComponentsInChildren<GridLayoutGroup>();
        GridLayoutGroup colorCustomizeGrid = grids[1];
        for (int i = 0; i < colorCustomizeGrid.transform.childCount; i++)
        {
            customizeItems.Add(colorCustomizeGrid.transform.GetChild(i).gameObject);
        }
    }

    private void AddListener()
    {
        AddTileBtn.onClick.AddListener(OnAddTileClick);
        SubTileBtn.onClick.AddListener(OnSubTileClick);
        Toggle[] toggles = shapeGroup.GetComponentsInChildren<Toggle>();
        int index = 0;
        foreach (BounceShape shape in Enum.GetValues(typeof(BounceShape)))
        {
            shapeToggles.Add(toggles[index], shape);
            toggles[index].onValueChanged.AddListener(OnShapeValueChanged);
            index++;
        }
        toggles = heightGroup.GetComponentsInChildren<Toggle>();
        index = 0;
        foreach (BounceHeight height in Enum.GetValues(typeof(BounceHeight)))
        {
            heightToggles.Add(toggles[index], height);
            toggles[index].onValueChanged.AddListener(OnHeightValueChanged);
            index++;
        }
    }
    private void OnAddTileClick()
    {
        var tiling = curEntity.Get<BounceplankComponent>().tile;
        tiling.y -= tileIncrement;
        if (tiling.y < 0) tiling.y = 0;
        curEntity.Get<MaterialComponent>().tile = tiling;

        nBehaviour.SetTiling(tiling);
    }
    private void OnSubTileClick()
    {
        var tiling = curEntity.Get<BounceplankComponent>().tile;
        tiling.y += tileIncrement;
        curEntity.Get<MaterialComponent>().tile = tiling;

        nBehaviour.SetTiling(tiling);
    }

    private void OnSettingClick()
    {
        OpenShowPanel(PanelShowType.Setting);
    }
    private void OnMaterialClick()
    {
        var matData = GetMatDataById(nBehaviour.entity.Get<BounceplankComponent>().mat);
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, matData);
    }
    private void OnColorClick()
    {
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
    }
    private void OnShapeValueChanged(bool isOn)
    {
        if (!isOn) return;
        foreach (var toggle in shapeToggles)
        {
            if (toggle.Key.isOn == true)
            {
                nBehaviour.SetShape(toggle.Value);
                break;
            }
        }
    }
    private void OnHeightValueChanged(bool isOn)
    {
        if (!isOn) return;
        foreach (var toggle in heightToggles)
        {
            if (toggle.Key.isOn == true)
            {
                nBehaviour.SetHeight(toggle.Value);
                break;
            }
        }
    }
    private void OpenShowPanel(PanelShowType type)
    {
        if (type == PanelShowType.Setting)
        {
            RefreshSettingPanel();
        }
    }
    public override void AddTabListener()
    {
        var tab = tabGroup.GetTab((int)PanelShowType.Material);
        tab.AddClickListener(OnMaterialClick);
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        tab.AddClickListener(OnColorClick);
        tab = tabGroup.GetTab((int)PanelShowType.Setting);
        tab.AddClickListener(OnSettingClick);
    }

    public override void GetMatDatas()
    {
        matDatas.Clear();
        matDatas.AddRange(GameManager.Inst.matConfigDatas);
        // 屏蔽透明材质
        HideSpecialMat(GameConsts.TRANSPARENT_MAT_ID);
        //TODO: FEAT 发光材质
        //屏蔽发光材质
        // HideSpecialMat(GameConsts.EMISSION_MAT_ID);
    }
    public override void GetColorDatas()
    {
        colorDatas = ResManager.Inst.LoadRes<ColorLibrary>("ConfigAssets/UGCClothesColorLibrary");
    }

    public override void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        var entityGo = entity.Get<GameObjectComponent>().bindGo;
        nBehaviour = entityGo.GetComponent<BounceplankBehaviour>();
        var comp = entity.Get<BounceplankComponent>();
        modelColor = DataUtils.DeSerializeColor(comp.color);
        colorStr = DataUtils.ColorToString(modelColor);

        var matData = GetMatDataById(comp.mat);
        var matSelectIndex = matDatas.FindIndex(x => x.id == comp.mat);
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, matData);
        SetMatSelect(matSelectIndex);
        var colorSelectIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(modelColor), colorDatas.List);
        SetEntitySelectColor(colorSelectIndex,comp.color);
    }

    public override void SetMaterial(int index)
    {
        nBehaviour.SetMatetial(matDatas[index].id);
    }

    public override void SetColor(int index, ColorSelectType type)
    {
        Color color = GetCurColor(type, index);
        SetSliderColor(color);
        SetColorSelect(type, index);
        SetColor(color);
        colorStr = DataUtils.ColorToString(color);
        colorType = type;
        colorId = index;
    }
    public override void SetColor(Color color)
    {
        modelColor = color;
        nBehaviour.SetColor(modelColor);
    }
    public void RefreshSettingPanel()
    {
        var shape = curEntity.Get<BounceplankComponent>().shape;
        var height = curEntity.Get<BounceplankComponent>().BounceHeight;
        foreach (var toggle in shapeToggles)
        {
            if (shape == (int)toggle.Value)
            {
                toggle.Key.SetIsOnWithoutNotify(true);
                break;
            }
        }
        foreach (var toggle in heightToggles)
        {
            if (height == toggle.Value.ToString())
            {
                toggle.Key.SetIsOnWithoutNotify(true);
                break;
            }
        }
    }
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        tabGroup.OpenPreTab();
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand);
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
        UpdateCustomizePanel();
    }
}

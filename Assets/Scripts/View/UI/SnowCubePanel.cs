using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnowCubePanel : CommonMatColorPanel<SnowCubePanel>, IUndoRecord
{
    enum PanelShowType
    {
        Color = 0,
        Setting = 1,
    };
    public Button AddTileBtn;
    public Button SubTileBtn;
    public ToggleGroup shapeGroup;
    private float tileIncrement = 1f;
    private Color modelColor;
    private SceneEntity curEntity;
    private SnowCubeBehaviour nBehaviour;
        private Dictionary<Toggle, SnowShape> shapeToggles = new Dictionary<Toggle, SnowShape>();

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        CreatePanel();
        GetColorDatas();
        CreateColorItems();
        UpdateCustomizePanel();
        AddCommonListener();
        AddListener();
    }

    private void CreatePanel()
    {
        tabGroup = transform.GetComponentInChildren<TabSelectGroup>();
        tabGroup.isExpand = true;
        var tab = tabGroup.GetTab((int)PanelShowType.Color);
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
        foreach (SnowShape shape in Enum.GetValues(typeof(SnowShape)))
        {
            shapeToggles.Add(toggles[index], shape);
            toggles[index].onValueChanged.AddListener(OnShapeValueChanged);
            index++;
        }
    }

    private void OnSettingClick()
    {
        OpenShowPanel(PanelShowType.Setting);
    }
    private void OnColorClick()
    {
        OpenShowPanel(PanelShowType.Color);
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
    }

    private void OpenShowPanel(PanelShowType type)
    {
        if (type == PanelShowType.Setting)
        {
            RefreshSettingPanel();
        }
        else
        {
            RefreshScrollPanel((int)type);
        }
    }
    public override void AddTabListener()
    {
        var tab = tabGroup.GetTab((int)PanelShowType.Setting);
        tab.AddClickListener(OnSettingClick);
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        tab.AddClickListener(OnColorClick);
    }

    public override void GetColorDatas()
    {
        colorDatas = AssetLibrary.Inst.colorLib;
    }

    public override void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        var entityGo = entity.Get<GameObjectComponent>().bindGo;
        nBehaviour = entityGo.GetComponent<SnowCubeBehaviour>();
        var comp = entity.Get<SnowCubeComponent>();
        modelColor = DataUtils.DeSerializeColor(comp.color);

        var colorSelectIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(modelColor), colorDatas.List);
        SetEntitySelectColor(colorSelectIndex, comp.color);
    }

    public override void SetColor(int index, ColorSelectType type)
    {
        var beginData = CreateUndoData(SnowCubeType.Color, colorType, colorStr, (int)PanelShowType.Color, tabGroup.isExpand);
        Color color = GetCurColor(type, index);
        SetSliderColor(color);
        SetColorSelect(type, index);
        SetColor(color);
        colorStr = DataUtils.ColorToString(color);
        colorType = type;
        colorId = index;
        var endData = CreateUndoData(SnowCubeType.Color, colorType, colorStr, (int)PanelShowType.Color, tabGroup.isExpand);
        AddRecord(beginData, endData);
    }

    public override void SetColor(Color color)
    {
        modelColor = color;
        nBehaviour.SetColor(modelColor);
    }
    public void RefreshScrollPanel(int type)
    {
        var isExpand = tabGroup.isExpand;
        if (type == (int)PanelShowType.Color)
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
    private void OnSubTileClick()
    {
        var beginData = CreateUndoData(SnowCubeType.Tile);
        var tiling = curEntity.Get<SnowCubeComponent>().tiling;
        tiling.x -= tileIncrement;
        tiling.y -= tileIncrement;
        if (tiling.y < 0) tiling.y = 0;
        if (tiling.x < 0) tiling.x = 0;
        curEntity.Get<SnowCubeComponent>().tiling = tiling;
        var endData = CreateUndoData(SnowCubeType.Tile);
        AddRecord(beginData, endData);
        nBehaviour.SetTiling(tiling);
    }
    private void OnAddTileClick()
    {
        var beginData = CreateUndoData(SnowCubeType.Tile);
        var tiling = curEntity.Get<SnowCubeComponent>().tiling;
        tiling.x += tileIncrement;
        tiling.y += tileIncrement;
        curEntity.Get<SnowCubeComponent>().tiling = tiling;
        var endData = CreateUndoData(SnowCubeType.Tile);
        AddRecord(beginData, endData);
        nBehaviour.SetTiling(tiling);
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

    public void RefreshSettingPanel()
    {
        var shape = curEntity.Get<SnowCubeComponent>().shape;
        foreach (var toggle in shapeToggles)
        {
            if (shape == (int)toggle.Value)
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
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
    }

    public void TileClickUndo(Vector2 tile)
    {
        curEntity.Get<SnowCubeComponent>().tiling = tile;
        nBehaviour.SetTiling(tile);
    }

    public void SetColorUndo(int index, int type, string str, int tabId, bool isExpand)
    {
        colorStr = str;
        SetSelectColor(index, (ColorSelectType)type);
        var color = colorDatas.Get(colorId);
        nBehaviour.SetColor(color);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
        RefreshScrollPanel(ShowType.Color, isExpand);
    }

    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord(SnowCubeUndoData beginData, SnowCubeUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.SnowCubeUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private SnowCubeUndoData CreateUndoData(SnowCubeType type, ColorSelectType colorType = ColorSelectType.Normal, string str = "", int tabId = -1, bool isExpand = false)
    {
        SnowCubeComponent component = curEntity.Get<SnowCubeComponent>();
        SnowCubeUndoData data = new SnowCubeUndoData();
        data.colorId = colorId;
        data.colorType = (int)colorType;
        data.colorStr = str;
        data.tile = component.tiling;
        data.targetEntity = curEntity;
        data.undoType = (int)type;
        data.tabId = tabId;
        data.isExpand = isExpand;
        return data;
    }
}

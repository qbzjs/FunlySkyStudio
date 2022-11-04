using System;
using System.Collections;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class BaseMatColorPanel : CommonMatColorPanel<BaseMatColorPanel>, IUndoRecord
{
    enum PanelShowType
    {
        Material = 0,
        Color = 1,
    };
    public Button AddTileBtn;
    public Button SubTileBtn;

    private int matId;
    private string ugcMatMapId;
    private float tileIncrement = 0.1f;
    private Color modelColor;
    private SceneEntity curEntity;
    private NodeBehaviour nBehaviour;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        SetHasUGCMatPanel();
        CreatePanel();
        GetMatDatas();
        GetColorDatas();
        CreateMatItems();
        CreateColorItems();
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
        matExpandScroll = tab.expandPanel.transform.Find("MaterialScroll/ScrollView").GetComponent<ScrollRect>();
        ugcMatExpandScroll = tab.expandPanel.transform.Find("MaterialScroll/UGCMatScrollView").GetComponent<ScrollRect>();
     
        ugcMatView = ugcMatExpandScroll.GetComponentInChildren<UGCMatView>(true);
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
      
    }

    private void OnMaterialClick()
    {
        var matData = GetMatDataById(matId);
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, matData);
    }
    private void OnColorClick()
    {
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
    }
    public override void AddTabListener()
    {
        var tab = tabGroup.GetTab((int)PanelShowType.Material);
        tab.AddClickListener(OnMaterialClick);
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        tab.AddClickListener(OnColorClick);
    }

    public override void GetMatDatas()
    {
        matDatas.Clear();
        matDatas.AddRange(GameManager.Inst.matConfigDatas);
    }
    public override void GetColorDatas()
    {
        colorDatas = ResManager.Inst.LoadRes<ColorLibrary>("ConfigAssets/UGCClothesColorLibrary");
    }

    public override void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        var entityGo = entity.Get<GameObjectComponent>().bindGo;
        nBehaviour = entityGo.GetComponent<NodeBehaviour>();
        var matComp = entity.Get<MaterialComponent>();
        modelColor = matComp.color;
        colorStr = DataUtils.ColorToString(modelColor);
        var index = GetColorIndex(modelColor);
        if (string.IsNullOrEmpty(matComp.umat))
        {
            var matSelectIndex = matDatas.FindIndex(x => x.id == matComp.matId);
            matId = matComp.matId;
            var matData = GetMatDataById(matComp.matId);
            RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, matData);
            SetMatSelect(matSelectIndex);
        }
        else
        {
            RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, (int)OpenUGCPage.UGCMaterial);
            SetUGCMatSelect(matComp.umat);

        }
       
        var colorSelectIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(modelColor), colorDatas.List);
        SetEntitySelectColor(colorSelectIndex, DataUtils.ColorToString(modelColor));
    }

    public override void SetMaterial(int index)
    {
        SetMatSelect(index);
        var beginData = CreateUndoData(MaterialSelectType.Material,colorType, (int)PanelShowType.Material, tabGroup.isExpand);
        var matData = matDatas[index];
        matId = matData.id;
        curEntity.Get<MaterialComponent>().matId = matData.id;
        curEntity.Get<MaterialComponent>().umat = null;
        curEntity.Get<MaterialComponent>().uurl = null;
        var endData = CreateUndoData(MaterialSelectType.Material,colorType,(int)PanelShowType.Material, tabGroup.isExpand);
        AddRecord(beginData, endData);
        SceneObjectController.SetBaseModelAtr(nBehaviour, matId, modelColor);
    }
    public override void SetUGCMaterial(UGCMatData data)
    {
        var beginData = CreateUndoData(MaterialSelectType.Material, colorType, (int)PanelShowType.Material, tabGroup.isExpand);
        curEntity.Get<MaterialComponent>().umat = data.mapInfo.mapId;
        curEntity.Get<MaterialComponent>().uurl = data.matUrl;
        var endData = CreateUndoData(MaterialSelectType.Material, colorType, (int)PanelShowType.Material, tabGroup.isExpand);
        AddRecord(beginData, endData);
        SceneObjectController.SetUGCBaseModelAtr(nBehaviour, matId, modelColor, data.matUrl);
    }
    public override void SetColor(int index, ColorSelectType type)
    {
        var beginData = CreateUndoData(MaterialSelectType.Color,colorType,(int)PanelShowType.Color,tabGroup.isExpand);
        Color color = GetCurColor(type, index);
        SetSliderColor(color);
        SetColorSelect(type, index);
        SetColor(color);
        colorStr = DataUtils.ColorToString(color);
        colorType = type;
        var endData = CreateUndoData(MaterialSelectType.Color,colorType,(int)PanelShowType.Color, tabGroup.isExpand);
        AddRecord(beginData, endData);
    }
    public override void SetColor(Color color)
    {
        modelColor = color;
        curEntity.Get<MaterialComponent>().color = color;
        SceneObjectController.SetBaseModelColor(nBehaviour, color);
    }
    private void OnAddTileClick()
    {
        var beginData = CreateUndoData(MaterialSelectType.Tile);
        var tiling = curEntity.Get<MaterialComponent>().tile;
        tiling.x -= tileIncrement;
        tiling.y -= tileIncrement;
        if (tiling.y < 0) tiling.y = 0;
        if (tiling.x < 0) tiling.x = 0;
        curEntity.Get<MaterialComponent>().tile = tiling;

        var endData = CreateUndoData(MaterialSelectType.Tile);
        AddRecord(beginData, endData);
        nBehaviour.SetTiling(tiling);
    }
    public void TileClickUndo(Vector2 tile)
    {
        curEntity.Get<MaterialComponent>().tile = tile;
        nBehaviour.SetTiling(tile);
    }

    private void OnSubTileClick()
    {
        var beginData = CreateUndoData(MaterialSelectType.Tile);
        var tiling = curEntity.Get<MaterialComponent>().tile;
        tiling.x += tileIncrement;
        tiling.y += tileIncrement;
        curEntity.Get<MaterialComponent>().tile = tiling;
        var endData = CreateUndoData(MaterialSelectType.Tile);
        AddRecord(beginData, endData);
        nBehaviour.SetTiling(tiling);
    }
    public void SetMaterialUndo(int matid,string ugcMapId, string ugcUrl, int tabId,bool isExpand)
    {
        if (string.IsNullOrEmpty(ugcUrl) && string.IsNullOrEmpty(ugcMapId))
        {
            for (int i = 0; i < GameManager.Inst.matConfigDatas.Count; i++)
            {
                if (GameManager.Inst.matConfigDatas[i].id == matid)
                {
                    SetMatSelect(i);
                    break;
                }
            }
            matId = matid;
            var matData = GameManager.Inst.matConfigDatas.Find(x => x.id == matid);
            SceneObjectController.SetBaseModelAtr(nBehaviour, matId, modelColor);
            tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
            RefreshScrollPanel(ShowType.Material, isExpand, matData);
        }
        else
        {
            SetUGCMatSelect(ugcMapId);
            SceneObjectController.SetUGCBaseModelAtr(nBehaviour, matId, modelColor, ugcUrl);
            RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, (int)OpenUGCPage.UGCMaterial);
            tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
           

        }
        curEntity.Get<MaterialComponent>().matId = matid;
        curEntity.Get<MaterialComponent>().umat = ugcMapId;
        curEntity.Get<MaterialComponent>().uurl = ugcUrl;

    }
    public void SetColorUndo(int type,Color color,int tabId,bool isExpand)
    {
        colorStr = DataUtils.ColorToString(color);
        SetSelectColor(color,(ColorSelectType)type);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
        RefreshScrollPanel(ShowType.Color, isExpand);
    }
    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord(BaseMatColorUndoData beginData, BaseMatColorUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.BaseMatColorUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private BaseMatColorUndoData CreateUndoData(MaterialSelectType type,ColorSelectType colorType=ColorSelectType.Normal,int tabId = -1,bool isExpand=false)
    {
        MaterialComponent component = curEntity.Get<MaterialComponent>();
        BaseMatColorUndoData data = new BaseMatColorUndoData();
        data.matId = component.matId;
        data.ugcMatMapId = component.umat;
        data.ugcMatUrl = component.uurl;
        data.colorType = (int)colorType;
        data.color = component.color;
        data.tile = component.tile;
        data.targetEntity = curEntity;
        data.baseMaterialType = (int)type;
        data.tabId = tabId;
        data.isExpand = isExpand;
        return data;
    }
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        tabGroup.OpenPreTab();
        var matData = GetMatDataById(matId);
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, matData);
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
        UpdateCustomizePanel();
    }
}

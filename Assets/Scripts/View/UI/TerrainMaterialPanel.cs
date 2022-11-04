using System;
using System.Collections;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class TerrainMaterialPanel : CommonMatColorPanel<TerrainMaterialPanel>,IUndoRecord
{
    enum PanelShowType
    {
        Material,
        Color,
    };
    private TerrainBehaviour terBehav;
    private int matId;
    private static int firstColorNum = 0;
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
        terBehav = SceneBuilder.Inst.TerrainGo.GetComponentInChildren<TerrainBehaviour>();
        matId = terBehav.entity.Get<TerrainComponent>().matId;
        colorId = GetColorIndex(terBehav.entity.Get<TerrainComponent>().color);
        colorStr = DataUtils.ColorToString(terBehav.entity.Get<TerrainComponent>().color);
        SetCurColorSelect(DataUtils.DeSerializeColor(colorStr));
        SetSliderColor(DataUtils.DeSerializeColor(colorStr));
    }
    private void CreatePanel()
    {

        tabGroup = transform.GetComponentInChildren<TabSelectGroup>();
        tabGroup.isExpand = true;//默认展开
        var tab = tabGroup.GetTab((int)PanelShowType.Material);
        ugcMatExpandScroll = tab.expandPanel.transform.Find("MaterialScroll/UGCMatScrollView").GetComponent<ScrollRect>();
        ugcMatView = ugcMatExpandScroll.GetComponentInChildren<UGCMatView>(true);
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

    public override void SetMaterial(int index)
    {
        var beginData = CreateUndoData(MaterialSelectType.Material,colorType,colorStr, (int)PanelShowType.Material,tabGroup.isExpand);
        var matData = matDatas[index];
        SetMatSelect(index);
        matId = matData.id;
        var mat = ResManager.Inst.LoadRes<Material>(GameConsts.TerrainMatPath + "Ground_"+ matData.id);
        terBehav.SetMat(mat);
        SetTerrainColor(colorStr);
        terBehav.entity.Get<TerrainComponent>().matId = matId;
        terBehav.entity.Get<TerrainComponent>().umatUrl = null;
        terBehav.entity.Get<TerrainComponent>().umapId = null;
        var endData = CreateUndoData(MaterialSelectType.Material,colorType, colorStr, (int)PanelShowType.Material,tabGroup.isExpand);
        AddRecord(beginData, endData);
        var size = terBehav.entity.Get<TerrainComponent>().terrainSize;
        terBehav.ExpandTextureScale(size);
    }
    public void SetMaterialUndo(int matUndoId,string ugcMatUrl,string ugcMatMapId,int tabId,bool isExpand)
    {
        if (string.IsNullOrEmpty(ugcMatUrl) && string.IsNullOrEmpty(ugcMatMapId))
        {
            var matData = GameManager.Inst.allTerrainConfigDatas.Find(x => x.id == matUndoId);
            int selectIndex = 0;
            for (int i = 0; i < matDatas.Count; i++)
            {
                if (matDatas[i].id == matData.id)
                {
                    selectIndex = i;
                }
            }
            SetMatSelect(selectIndex);
            matId = matData.id;
            var mat = ResManager.Inst.LoadRes<Material>(GameConsts.TerrainMatPath + "Ground_" + matData.id);
            terBehav.SetMat(mat);
            RefreshScrollPanel(ShowType.Material, isExpand, matData);
        }
        else
        {
            UGCTexManager.Inst.GetUGCTex(ugcMatUrl, (tex) => {
                terBehav.SetUGCMat(tex);
            });
            SetUGCMatSelect(ugcMatMapId);
            RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, (int)OpenUGCPage.UGCMaterial);
        }
        SetTerrainColor(colorStr);
        terBehav.entity.Get<TerrainComponent>().matId = matId;
        terBehav.entity.Get<TerrainComponent>().umatUrl = ugcMatUrl;
        terBehav.entity.Get<TerrainComponent>().umapId = ugcMatMapId;
        var size = terBehav.entity.Get<TerrainComponent>().terrainSize;
        terBehav.ExpandTextureScale(size);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
        
    }
    public override void SetUGCMaterial(UGCMatData data)
    {
        var beginData = CreateUndoData(MaterialSelectType.Material, colorType, colorStr, (int)PanelShowType.Material, tabGroup.isExpand);
        UGCTexManager.Inst.GetUGCTex(data.matUrl, (tex) => {
            terBehav.SetUGCMat(tex);
        });
        SetTerrainColor(colorStr);
        terBehav.entity.Get<TerrainComponent>().umatUrl = data.matUrl;
        terBehav.entity.Get<TerrainComponent>().umapId = data.mapId;
        var endData = CreateUndoData(MaterialSelectType.Material, colorType, colorStr, (int)PanelShowType.Material, tabGroup.isExpand);
        AddRecord(beginData, endData);
        var size = terBehav.entity.Get<TerrainComponent>().terrainSize;
        terBehav.ExpandTextureScale(size);
    }
    public override void SetColor(int index,ColorSelectType type)
    {
        var beginData =  CreateUndoData(MaterialSelectType.Color,colorType, colorStr, (int)PanelShowType.Color,tabGroup.isExpand);
        Color color = GetCurColor(type, index);
        SetSliderColor(color);
        SetColorSelect(type, index);
        SetColor(color);
        colorStr = DataUtils.ColorToString(color);
        colorType = type;
        colorId = index;
        var endData = CreateUndoData(MaterialSelectType.Color,colorType, colorStr, (int)PanelShowType.Color,tabGroup.isExpand);
        AddRecord(beginData,endData);
    }
    public override void SetColor(Color color)
    {
        terBehav.SetColor(color);
        terBehav.entity.Get<TerrainComponent>().color = color;
    }
    public void SetColorUndo(int index,int type,string str,int tabId,bool isExpand)
    {
        colorStr = str;
        SetSelectColor(index, (ColorSelectType)type);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
        RefreshScrollPanel(ShowType.Color, isExpand);
    }
    public override void SetEntity(SceneEntity entity)
    {
        
    }
    public override void AddTabListener()
    {
        var tab = tabGroup.GetTab((int)PanelShowType.Material);
        tab.AddClickListener(OnMaterialClick);
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        tab.AddClickListener(OnColorClick);
    }
    private void OnMaterialClick()
    {
        var matData = GetMatDataById(matId);
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, matData);
    }
    private void OnColorClick()
    {
        RefreshScrollPanel(ShowType.Color,tabGroup.isExpand);
    }

    public override void GetMatDatas()
    {
        matDatas.Clear();
        matDatas.AddRange(GameManager.Inst.terrainConfigDatas);
    }
    public override void GetColorDatas()
    {
        colorDatas = ResManager.Inst.LoadRes<ColorLibrary>("ConfigAssets/UGCClothesColorLibrary");
    }

    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord(TerrainMaterialUndoData beginData, TerrainMaterialUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.TerrainMaterialUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private TerrainMaterialUndoData CreateUndoData(MaterialSelectType type, ColorSelectType colorType, string str="",int tabId = -1, bool isExpand = false)
    {
        TerrainMaterialUndoData data = new TerrainMaterialUndoData();
        data.matId = SceneBuilder.Inst.TerrainEntity.Get<TerrainComponent>().matId;
        data.ugcMatMapId = SceneBuilder.Inst.TerrainEntity.Get<TerrainComponent>().umapId;
        data.ugcMatUrl = SceneBuilder.Inst.TerrainEntity.Get<TerrainComponent>().umatUrl;
        data.colorId = colorId;
        data.colorStr = str;
        data.colorType = (int)colorType;
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
    private void SetTerrainColor(string str)
    {
        var color = DataUtils.DeSerializeColor(str);
        terBehav.SetColor(color);
    }
}

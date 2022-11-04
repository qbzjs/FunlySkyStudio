using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Game.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:Meimei-LiMei
/// Description:PGC植物专属属性设置界面
/// Date: 2022/8/4 14:10:3
/// </summary>

public class PGCPlantPanel : CommonMatColorPanel<PGCPlantPanel>, IUndoRecord
{
    enum PanelShowType
    {
        Type = 0,
        Color = 1,
    };
    protected SceneEntity curEntity;
    protected PGCPlantBehaviour curBehav;
    protected PGCPlantComponent curComp;
    public Transform colorParent;
    public Transform plantParent;
    public ScrollRect typeView, colorView;
    public Toggle colorToggle, typeToggle;
    public GameObject plantItem;
    public GameObject colorItem;
    private List<GameObject> allPlantSelect = new List<GameObject>();
    private List<PGCPlantConfig> PGCPlantConfigs = new List<PGCPlantConfig>();


    public override void SetColor(int index, ColorSelectType type)
    {
        var beginData = CreateUndoData(PGCSelectType.Color,colorType,(int)PanelShowType.Color, tabGroup.isExpand);
        Color color = GetCurColor(type, index);
        SetSliderColor(color);
        SetColorSelect(type, index);
        SetColor(color);
        colorStr = DataUtils.ColorToString(color);
        colorType = type;
        colorId = index;
        var endData = CreateUndoData(PGCSelectType.Color,colorType,(int)PanelShowType.Color, tabGroup.isExpand);
        AddRecord(beginData, endData);
    }
    public override void SetColor(Color color)
    {
        var colorStr = DataUtils.ColorToString(color);
        curComp.plantColor = colorStr;
        curBehav.SetColor(color);
    }
    public override void AddTabListener()
    {
        var tab = tabGroup.GetTab((int)PanelShowType.Type);
        tab.AddClickListener(OnTypeClick);
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        tab.AddClickListener(OnColorClick);
    }
    public void OnTypeClick()
    {

    }
    public void OnColorClick()
    {
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
    }
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        tabGroup.OpenPreTab();
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
        UpdateCustomizePanel();
    }
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        PGCPlantConfigs = PGCPlantManager.Inst.GetPGCPlantConfigList();
        GetColorDatas();
        CreatePanel();
        CreateColorItems();
        InitPGCPlantView();
        UpdateCustomizePanel();
        AddCommonListener();

        SetPGCPlantSelect(0);
        SetColorSelect(ColorSelectType.Normal, 11);
        tabGroup.ResetTab();
    }
    public override void GetColorDatas()
    {
        colorDatas = ResManager.Inst.LoadRes<ColorLibrary>("ConfigAssets/UGCClothesColorLibrary");
    }
    public override void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        curBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<PGCPlantBehaviour>();
        if (entity.HasComponent<PGCPlantComponent>())
        {
            curComp = entity.Get<PGCPlantComponent>();
            InitPanelStateByComp(curComp);
        }
    }
    private void CreatePanel()
    {
        tabGroup = transform.GetComponentInChildren<TabSelectGroup>();
        tabGroup.isExpand = true;//默认展开
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
    private void InitPanelStateByComp(PGCPlantComponent comp)
    {
        //类型
        var gocomp = curEntity.Get<GameObjectComponent>();
        var index = GetSelectIndex(gocomp.modId);
        SetPGCPlantSelect(index);

        //颜色
        colorStr = comp.plantColor;
        var color = DataUtils.DeSerializeColor(colorStr);
        int colorIndex = DataUtils.GetColorSelect((comp.plantColor), colorDatas.List);
        colorId = colorIndex;
        SetSliderColor(color);
        SetEntitySelectColor(colorIndex, comp.plantColor);
    }
    private int GetSelectIndex(int plantId)
    {
        for (int i = 0; i < GameManager.Inst.PGCPlantDatas.Count; i++)
        {
            if (GameManager.Inst.PGCPlantDatas[i].id == plantId)
            {
                return i;
            }
        }
        return 0;
    }
    private void InitPGCPlantView()
    {
        for (int i = 0; i < GameManager.Inst.PGCPlantDatas.Count; i++)
        {
            //11015对应的大草坪由于性能原因在1.41版本进行删除，新用户无法使用
            if (GameManager.Inst.PGCPlantDatas[i].id == 11015)
            {
                continue;
            }
            int index = i;
            var itemData = GameManager.Inst.PGCPlantDatas[i];
            var itemGo = Instantiate(plantItem, plantParent);
            var cover = itemGo.transform.GetChild(0).GetComponent<Image>();
            var button = itemGo.GetComponentInChildren<Button>();
            var select = itemGo.transform.GetChild(1).gameObject;
            cover.sprite = ResManager.Inst.GetGameAtlasSprite(itemData.iconName);
            button.onClick.AddListener(() => OnClickPGCPlant(index));
            allPlantSelect.Add(select);
        }
    }
    public void UpdatePGCPlant(int plantID)
    {
        var curId = PGCPlantManager.Inst.GetLastChooseID();
        DestroyCurNode(curId);
        curBehav.UpdateNodeHandleType(plantID);
        GetNewObj(plantID);
        var color = DataUtils.DeSerializeColor(curComp.plantColor);
        curBehav.SetColor(color);
    }
    public void OnClickPGCPlant(int index)
    {
        var plantData = PGCPlantConfigs[index];
        var curId = PGCPlantManager.Inst.GetLastChooseID();
        if (plantData.id == curId)
        {
            return;
        }
        var beginData = CreateUndoData(PGCSelectType.Type,colorType,(int)PanelShowType.Type);
        UpdatePGCPlant(plantData.id);
        Color curColor = DataUtils.DeSerializeColor(colorStr);
        SetColor(curColor);
        SetSliderColor(curColor);
        SetCurColorSelect(curColor);
        SetPGCPlantSelect(index);
        var endData = CreateUndoData(PGCSelectType.Type, colorType,(int)PanelShowType.Type);
        AddRecord(beginData, endData);
    }
    public void SetPGCPlantUndo(int plantID,int type,int tabId,bool isExpand,int colorIndex,string srcColor)
    {
        UpdatePGCPlant(plantID);
        var index = GetSelectIndex(plantID);
        SetPGCPlantSelect(index);
        Color color = DataUtils.DeSerializeColor(srcColor);
        SetSelectColor(color, (ColorSelectType)type);
        colorStr = srcColor;
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
    }
    private void SetPGCPlantSelect(int index)
    {
        allPlantSelect.ForEach(x => x.SetActive(false));
        if (index < allPlantSelect.Count)
        {
            allPlantSelect[index].SetActive(true);
            var plantData = PGCPlantConfigs[index];
            PGCPlantManager.Inst.UpdateLastChooseID(plantData.id);
        }
    }
    private void DestroyCurNode(int id)
    {
        ModelCachePool.Inst.Release(id, curBehav.assetObj);
    }
    private void GetNewObj(int id)
    {
        var go = ModelCachePool.Inst.Get(id);
        curBehav.UpdateAssetObj(go, id);
    }
    public void SetPGCPlantColorUndo(int index,int type,string str,int tabId,bool isExpand)
    {
        colorStr = str;
        SetSelectColor(index, (ColorSelectType)type);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
        RefreshScrollPanel(ShowType.Color, isExpand);
    }
    #region 撤销功能
    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord(PGCPlantUndoData beginData, PGCPlantUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.PGCPlantUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }
    private PGCPlantUndoData CreateUndoData(PGCSelectType type,ColorSelectType colorType,int tabId=-1,bool isExpand = false)
    {
        var pgcComp = curEntity.Get<PGCPlantComponent>();
        var goComp = curEntity.Get<GameObjectComponent>();
        PGCPlantUndoData data = new PGCPlantUndoData();
        data.plantID = goComp.modId;
        int colorIndex = DataUtils.GetColorSelect(pgcComp.plantColor, colorDatas.List);
        if (colorIndex < 0)
            colorIndex = GetCustomizeColorIndex(DataUtils.DeSerializeColor(pgcComp.plantColor));
        data.colorIndex = colorId;
        data.colorType = (int)colorType;
        data.colorStr = pgcComp.plantColor;
        data.targetEntity = curEntity;
        data.undoType = (int)type;
        data.tabId = tabId;
        data.isExpand = isExpand;
        return data;
    }
    #endregion
}

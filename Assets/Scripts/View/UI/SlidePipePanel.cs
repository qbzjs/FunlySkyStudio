using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SlidePipePanel;

public class SlidePipePanel : CommonMatColorPanel<SlidePipePanel>
{
    enum PanelShowType
    {
        Material = 0,
        Color = 1,
        Setting = 2,
    };
    public enum EWayType
    {
        One,
        Tow,
    }
    public enum EShapeType
    {
        Forward,
        Left,
        Right,
        Up,
        Down,
    }
    public  enum ESpeedType
    {
        ExtraSlow,
        Slow,
        Medium,
        Fast,
        ExtraFast
    }
    public  class Item<T>
    {
        public GameObject mGameObject;
        public Toggle mToggle;
        public T mSpeedType;
        public Action<T> mSelectCallBack;
        public void Init(GameObject go, T speedType, Action<T> selectedCallBack)
        {
            mGameObject = go;
            mSpeedType = speedType;
            mSelectCallBack = selectedCallBack;
            mToggle = mGameObject.GetComponent<Toggle>();
            mToggle.onValueChanged.AddListener(OnSelectedToggle);
        }

        public void OnSelectedToggle(bool isOn)
        {
            if (isOn) mSelectCallBack?.Invoke(mSpeedType);
        }
        public void OnSelectedToggleWithoutNotify(bool isOn)
        {
            mToggle.SetIsOnWithoutNotify(isOn);
        }
    }
    private SceneEntity mRootEntity;
    private SceneEntity mCurEntity;
    private SlidePipeBehaviour nRootBehaviour;
    private SlideItemBehaviour nItemBehaviour;
    private Color modelColor;
    public GameObject mInnerTipsPanel;
    public Button mCloseTipsBtn;
    public Button mOpenTipsBtn;
    public Button AddTileBtn;
    public Button SubTileBtn;
    private float tileIncrement = 0.1f;
    //direction
    public ToggleGroup mWayTypeGroup;
    public Toggle[] mWayItems;
    public Toggle mHideModeSelected;
    //amount
    public Button mAmountMinus;
    public Button mAmountAdd;
    public Text mAmountText;
    //shape
    public Toggle[] mShapeItems;
    //speed
    public Toggle[] mSpeedItems;
    public Dictionary<EShapeType, SlideResType> mMapping = new Dictionary<EShapeType, SlideResType>()
    {
        { EShapeType.Forward,SlideResType.Forward},
        { EShapeType.Left,SlideResType.Left},
        { EShapeType.Right,SlideResType.Right},
        { EShapeType.Up,SlideResType.Up},
        { EShapeType.Down,SlideResType.Down},
    };
    public Dictionary<EShapeType, Item<EShapeType>> mShapeTypeItems = new Dictionary<EShapeType, Item<EShapeType>>();
    public Dictionary<ESpeedType, Item<ESpeedType>> mSpeedTypeItems = new Dictionary<ESpeedType, Item<ESpeedType>>();
    public Dictionary<EWayType, Item<EWayType>> mWayTypeItems = new Dictionary<EWayType, Item<EWayType>>();
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
        InitToggle();
    }

    private void AddListener()
    {
        AddTileBtn.onClick.AddListener(OnAddTileClick);
        SubTileBtn.onClick.AddListener(OnSubTileClick);
        mOpenTipsBtn.onClick.AddListener(OnOpenTipsClick);
        mCloseTipsBtn.onClick.AddListener(OnCloseTipsClick);
    }
    private void InitToggle()
    {
        for (int i = 0; i < mWayItems.Length; i++)
        {
            Toggle obj = mWayItems[i];
            EWayType eType = (EWayType)i;
            Item<EWayType> item = new Item<EWayType>();
            item.Init(obj.gameObject, eType, OnSelectedWayTypeToggle);
            mWayTypeItems.Add(eType, item);
        }
       
        mHideModeSelected.onValueChanged.AddListener(OnSelectedHideMode);
        //
        mAmountAdd.onClick.AddListener(OnClickAmountAddCallBack);
        mAmountMinus.onClick.AddListener(OnClickAmountMinusCallBack);
        //
        for (int i = 0; i < mShapeItems.Length; i++)
        {
            Toggle obj = mShapeItems[i];
            EShapeType eType = (EShapeType)i;
            Item<EShapeType> item = new Item<EShapeType>();
            item.Init(obj.gameObject, eType, OnSelectedShapeToggle);
            mShapeTypeItems.Add(eType, item);
        }
        for (int i = 0; i < mSpeedItems.Length; i++)
        {
            Toggle obj = mSpeedItems[i];
            ESpeedType eType = (ESpeedType)i;
            Item<ESpeedType> item = new Item<ESpeedType>();
            item.Init(obj.gameObject, eType, OnSelectedSpeedToggle);
            mSpeedTypeItems.Add(eType, item);
        }
        SetDefaultItem(mWayTypeItems, EWayType.One);
        SetDefaultItem(mSpeedTypeItems, ESpeedType.Medium);
        SetDefaultItem(mShapeTypeItems, EShapeType.Forward);
    }
    private void OnAddTileClick()
    {
        var beginData = CreateUndoData(MaterialSelectType.Tile);
        var tiling = mCurEntity.Get<SlideItemComponent>().Tile;
        tiling.x -= tileIncrement;
        tiling.y -= tileIncrement;
        if (tiling.y < 0) tiling.y = 0;
        if (tiling.x < 0) tiling.x = 0;
        mCurEntity.Get<SlideItemComponent>().Tile = tiling;

        var endData = CreateUndoData(MaterialSelectType.Tile);
        AddRecord(beginData, endData);
        nItemBehaviour.SetTiling(tiling);
    }
    private void OnSubTileClick()
    {
        var beginData = CreateUndoData(MaterialSelectType.Tile);
        var tiling = mCurEntity.Get<SlideItemComponent>().Tile;
        tiling.x += tileIncrement;
        tiling.y += tileIncrement;
        mCurEntity.Get<SlideItemComponent>().Tile = tiling;
        var endData = CreateUndoData(MaterialSelectType.Tile);
        AddRecord(beginData, endData);
        nItemBehaviour.SetTiling(tiling);
    }
    public void SetDefaultItem<T>(Dictionary<T,Item<T>> items,T defaultType)
    {
        Item<T> item = null;
        if (items.TryGetValue(defaultType, out item))
        {
            item.OnSelectedToggleWithoutNotify(true);
        }
    }

    public void SetItemEnable<T>(Dictionary<T,Item<T>> items,T defaultType,bool isEnbale)
    {
        Item<T> item = null;
        if (items.TryGetValue(defaultType, out item))
        {
            item.mGameObject.SetActive(isEnbale);
        }
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
    public override void AddTabListener()
    {
        var tab = tabGroup.GetTab((int)PanelShowType.Material);
        tab.AddClickListener(OnMaterialClick);
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        tab.AddClickListener(OnColorClick);
        tab = tabGroup.GetTab((int)PanelShowType.Setting);
        tab.AddClickListener(OnSettingClick);
    }
    private void OnSettingClick()
    {
        OpenShowPanel(PanelShowType.Setting);
    }
    private void OpenShowPanel(PanelShowType type)
    {
        if (type == PanelShowType.Setting)
        {
            RefreshSettingPanel();
        }
    }
    public void RefreshSettingPanel()
    {
    }
    private void OnMaterialClick()
    {
        var matData = GetMatDataById(nRootBehaviour.entity.Get<BounceplankComponent>().mat);
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, matData);
        mInnerTipsPanel.SetActive(false);
    }
    private void OnColorClick()
    {
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
        mInnerTipsPanel.SetActive(false);
    }
    public override void SetEntity(SceneEntity entity)
    {

    }
    public override void SetEntity(SceneEntity rootEntity,SceneEntity entity)
    {
        mCurEntity = entity;
        mRootEntity = rootEntity;
        GameObject rootEntityGo = mRootEntity.Get<GameObjectComponent>().bindGo;
        nRootBehaviour = rootEntityGo.GetComponent<SlidePipeBehaviour>();

        GameObject itemEntityGO = mCurEntity.Get<GameObjectComponent>().bindGo;
        nItemBehaviour = itemEntityGO.GetComponent<SlideItemBehaviour>();
        nItemBehaviour.SetSelect(true);
        
        SlidePipeComponent pipeComp = rootEntity.Get<SlidePipeComponent>();
        SlideItemComponent comp = entity.Get<SlideItemComponent>();
        modelColor = comp.Color;
        colorStr = DataUtils.ColorToString(modelColor);

        var matData = GetMatDataById(comp.MatId);
        var matSelectIndex = matDatas.FindIndex(x => x.id == comp.MatId);
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand, matData);
        SetMatSelect(matSelectIndex);
        var colorSelectIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(modelColor), colorDatas.List);
        SetEntitySelectColor(colorSelectIndex, DataUtils.ColorToString(comp.Color));

        SlidePipeManager.Inst.CurSelectItemBehaviour = nItemBehaviour;

        //显示数量
        RefreshAmount(nRootBehaviour.GetItemCount());

        //显示选中形状
        int modId = mCurEntity.Get<GameObjectComponent>().modId;
        EShapeType shapeType = GetShapeTypeByModId(modId);
        SetDefaultItem(mShapeTypeItems, shapeType);

        //显示方向：单/双
        SetDefaultItem(mWayTypeItems, (EWayType)pipeComp.WayType);

        mHideModeSelected.SetIsOnWithoutNotify(pipeComp.HideModel == 1);
        //设置速度
        SetDefaultItem(mSpeedTypeItems,(ESpeedType)comp.SpeedType);
    }

    public EShapeType GetShapeTypeByModId(int modId)
    {
        EShapeType shapeType = EShapeType.Forward;
        foreach(var key in mMapping.Keys)
        {
            if((int)mMapping[key] == modId)
            {
                shapeType = key;
                break;
            }
        }
        return shapeType;
    }

    public override void SetMaterial(int index)
    {
        SetMatSelect(index);
        var beginData = CreateUndoData(MaterialSelectType.Material,colorType, (int)PanelShowType.Material, tabGroup.isExpand);
        var matData = matDatas[index];
        mCurEntity.Get<SlideItemComponent>().MatId = matData.id;
        var endData = CreateUndoData(MaterialSelectType.Material,colorType,(int)PanelShowType.Material, tabGroup.isExpand);
        AddRecord(beginData, endData);
        nItemBehaviour.SetMatetial(matData.id);
    }

    public override void SetColor(int index, ColorSelectType type)
    {
        var beginData = CreateUndoData(MaterialSelectType.Color,colorType,(int)PanelShowType.Color,tabGroup.isExpand);
        Color color = GetCurColor(type, index);
        SetSliderColor(color);
        SetColorSelect(type, index);
        SetColor(color);
        nItemBehaviour.RefreshHighLight();
        colorStr = DataUtils.ColorToString(color);
        colorType = type;
        var endData = CreateUndoData(MaterialSelectType.Color,colorType,(int)PanelShowType.Color, tabGroup.isExpand);
        AddRecord(beginData, endData);
    }
    public override void SetColor(Color color)
    {
        modelColor = color;
        nItemBehaviour.SetColor(color);
    }

    private SlideItemUndoData CreateUndoData(MaterialSelectType type,ColorSelectType colorType=ColorSelectType.Normal,int tabId = -1,bool isExpand=false)
    {
        SlideItemComponent component = mCurEntity.Get<SlideItemComponent>();
        SlideItemUndoData data = new SlideItemUndoData();
        data.matId = component.MatId;
        data.colorType = (int)colorType;
        data.color = component.Color;
        data.tile = component.Tile;
        data.targetEntity = mCurEntity;
        data.baseMaterialType = (int)type;
        data.tabId = tabId;
        data.isExpand = isExpand;
        return data;
    }

    public void SetMaterialUndo(int matId,int tabId,bool isExpand)
    {
        for (int i = 0; i < GameManager.Inst.matConfigDatas.Count; i++)
        {
            if (GameManager.Inst.matConfigDatas[i].id == matId)
            {
                SetMatSelect(i);
                break;
            }
        }
        nItemBehaviour.SetMatetial(matId);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
        var matData = GameManager.Inst.matConfigDatas.Find(x => x.id == matId);
        RefreshScrollPanel(ShowType.Material, isExpand, matData);
    }
    public void SetColorUndo(int type,Color color,int tabId,bool isExpand)
    {
        colorStr = DataUtils.ColorToString(color);
        SetSelectColor(color,(ColorSelectType)type);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
        RefreshScrollPanel(ShowType.Color, isExpand);
    }

    public void TileClickUndo(Vector2 tile)
    {
        nItemBehaviour.SetTiling(tile);
    }

    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord(SlideItemUndoData beginData, SlideItemUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.SlideItemUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }
    #region Direction
    public void OnSelectedWayTypeToggle(EWayType wayType)
    {
        SlidePipeComponent compt = nRootBehaviour.entity.Get<SlidePipeComponent>();
        compt.WayType = (int)wayType;
    }
    public void OnSelectedHideMode(bool isOn)
    {
        if (isOn)
        {
            
        }
        else
        {

        }
        nRootBehaviour.SetVirtualModel(isOn);
    }
    #endregion
    #region Shape
    public void OnClickAmountAddCallBack()
    {
        if(nRootBehaviour.GetItemCount() >= SlidePipeManager.MaxItemCount)
        {
            TipPanel.ShowToast(SlidePipeManager.MAX_ITEM_TIP);
            return;
        }
        var itemBehaviour = SlidePipeManager.Inst.CreateSlideItem(nRootBehaviour);
        //自动选中字后一节
        if(itemBehaviour == null) return;
        nRootBehaviour.UnSelectAllItem();
        SetEntity(nRootBehaviour.entity,itemBehaviour.entity);
       
        RefreshAmount(nRootBehaviour.GetItemCount());
    }
    public void OnClickAmountMinusCallBack()
    {
        if(nRootBehaviour.GetItemCount() <= 1)
        {
            TipPanel.ShowToast(SlidePipeManager.MIN_ITEM_TIP);
            return;
        }
        SlidePipeManager.Inst.RemoveSlideItem(nRootBehaviour);
        var itemBehaviour = nRootBehaviour.GetTailItem();
        if(itemBehaviour == null) return;
        GameObjectComponent goComp = itemBehaviour.entity.Get<GameObjectComponent>();
        SlidePipeManager.Inst.CurSlideType = goComp.modId;
        nRootBehaviour.UnSelectAllItem();
        SetEntity(nRootBehaviour.entity,itemBehaviour.entity);

        RefreshAmount(nRootBehaviour.GetItemCount());
    }
    public void OnSelectedShapeToggle(EShapeType shapeType)
    {
        SlideResType resType = mMapping[shapeType];
        SlideItemBehaviour itemBehaviour = nRootBehaviour.GetTailItem();
        if(SlidePipeManager.Inst.CurSelectItemBehaviour != itemBehaviour)
        {
            TipPanel.ShowToast(SlidePipeManager.CHANGE_SHAPE_TIP);
            return;
        }
        SlidePipeManager.Inst.CurSlideType = (int)resType;
        SlidePipeManager.Inst.ChangeSlideItemModel(itemBehaviour);
    }

    public void OnOpenTipsClick()
    {
        mInnerTipsPanel.SetActive(true);
    }

    public void OnCloseTipsClick()
    {
        mInnerTipsPanel.SetActive(false);
    }

    public void RefreshAmount(int amount)
    {
        SetItemEnable(mWayTypeItems, EWayType.Tow,amount > 1);
        if(amount <= 1)
        {
            SetDefaultItem(mWayTypeItems, EWayType.One);
            OnSelectedWayTypeToggle(EWayType.One);
        }
        RefreshAmountText(amount);
    }
    public void RefreshAmountText(int amount)
    {
        mAmountText.text = amount.ToString();
    }
    #endregion
    #region Speed
    public void OnSelectedSpeedToggle(ESpeedType speedType)
    {
        mCurEntity.Get<SlideItemComponent>().SpeedType = (int)speedType;
        SlidePipeManager.Inst.CurSpeedType = (int)speedType;
    }
    
    #endregion
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        tabGroup.OpenPreTab();
        RefreshScrollPanel(ShowType.Material, tabGroup.isExpand);
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
        UpdateCustomizePanel();
        mInnerTipsPanel.SetActive(false);
    }
}

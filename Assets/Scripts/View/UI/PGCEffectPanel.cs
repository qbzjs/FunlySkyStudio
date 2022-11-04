using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Game.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:WenJia
/// Description:PGC特效专属属性设置界面
/// Date: 2022/10/25 14:22:32
/// </summary>

public class PGCEffectPanel : CommonMatColorPanel<PGCEffectPanel>, IUndoRecord
{
    enum PanelShowType
    {
        Type = 0,
        Color = 1,
        Setting = 2,
    };

    protected SceneEntity curEntity;
    protected PGCEffectBehaviour curBehav;
    protected PGCEffectComponent curComp;
    public Transform effectParent;
    public Toggle soundToggle;
    public GameObject effectItem;
    private List<GameObject> allEffectSelect = new List<GameObject>();
    private List<PGCEffectConfig> PGCEffectConfigs = new List<PGCEffectConfig>();

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        PGCEffectConfigs = PGCEffectManager.Inst.GetPGCEffectConfigList();
        GetColorDatas();
        CreatePanel();
        CreateColorItems();
        InitPGCEffectView();
        UpdateCustomizePanel();
        AddCommonListener();

        SetPGCEffectSelect(0);
        SetColorSelect(ColorSelectType.Normal, 11);
        tabGroup.ResetTab();
        AddListener();
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

    private void InitPanelStateByComp(PGCEffectComponent comp)
    {
        //类型
        var gocomp = curEntity.Get<GameObjectComponent>();
        var index = GetSelectIndex(gocomp.modId);
        SetPGCEffectSelect(index);

        //颜色
        colorStr = comp.effectColor;
        var color = DataUtils.DeSerializeColor(colorStr);
        int colorIndex = DataUtils.GetColorSelect((comp.effectColor), colorDatas.List);
        colorId = colorIndex;
        SetSliderColor(color);
        SetEntitySelectColor(colorIndex, comp.effectColor);

        if (comp.useDefColor != 1)
        {
            curBehav.ChooseColor = colorStr;
        }
        // 设置
        RefreshSettingPanel();
    }

    private void InitPGCEffectView()
    {
        for (int i = 0; i < PGCEffectConfigs.Count; i++)
        {
            int index = i;
            var itemData = PGCEffectConfigs[i];
            var itemGo = Instantiate(effectItem, effectParent);
            var cover = itemGo.transform.GetChild(0).GetComponent<Image>();
            var button = itemGo.GetComponentInChildren<Button>();
            var select = itemGo.transform.GetChild(1).gameObject;
            cover.sprite = ResManager.Inst.GetGameAtlasSprite(itemData.iconName);
            button.onClick.AddListener(() => OnClickPGCEffect(index));
            allEffectSelect.Add(select);
        }
    }

    public override void AddTabListener()
    {
        var tab = tabGroup.GetTab((int)PanelShowType.Type);
        tab.AddClickListener(OnTypeClick);
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        tab.AddClickListener(OnColorClick);
        tab = tabGroup.GetTab((int)PanelShowType.Setting);
        tab.AddClickListener(OnSettingClick);
    }

    public void AddListener()
    {
        soundToggle.onValueChanged.AddListener(OnSoundChanged);
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        tabGroup.OpenPreTab();
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
        UpdateCustomizePanel();
    }

    public override void SetColor(int index, ColorSelectType type)
    {
        var beginData = CreateUndoData(PGCSelectType.Color, colorType, (int)PanelShowType.Color, tabGroup.isExpand);
        Color color = GetCurColor(type, index);
        SetSliderColor(color);
        SetColorSelect(type, index);
        SetColor(color);
        colorStr = DataUtils.ColorToString(color);
        colorType = type;
        colorId = index;
        curBehav.ChooseColor = colorStr;
        var pgcComp = curEntity.Get<PGCEffectComponent>();
        pgcComp.useDefColor = 0;
        var endData = CreateUndoData(PGCSelectType.Color, colorType, (int)PanelShowType.Color, tabGroup.isExpand);
        AddRecord(beginData, endData);
    }

    public override void SetColor(Color color)
    {
        var colorStr = DataUtils.ColorToString(color);
        curComp.effectColor = colorStr;
        curBehav.SetColor(color);
    }

    public void OnTypeClick()
    {

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
        var id = curEntity.Get<GameObjectComponent>().modId;
        var effectConfig = PGCEffectManager.Inst.GetPGCEffectConfigData(id);
        var useSound = effectConfig.useSound;
        float alpha = 1;
        if (useSound == 0)
        {
            alpha = 0.5f;
            soundToggle.SetIsOnWithoutNotify(false);
        }
        else
        {
            var playSound = curEntity.Get<PGCEffectComponent>().playSound;
            soundToggle.SetIsOnWithoutNotify(playSound == 1);
        }
        soundToggle.targetGraphic.color = new Color(1, 1, 1, alpha);
        soundToggle.interactable = useSound == 1;
    }

    public void OnColorClick()
    {
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
    }

    private void OnSoundChanged(bool isOn)
    {
        var id = curEntity.Get<GameObjectComponent>().modId;
        var effectConfig = PGCEffectManager.Inst.GetPGCEffectConfigData(id);
        var useSound = effectConfig.useSound;
        if (useSound == 0)
        {
            soundToggle.isOn = false;
            return;
        }

        int playSound = isOn ? 1 : 0;
        curEntity.Get<PGCEffectComponent>().playSound = playSound;
    }

    public override void GetColorDatas()
    {
        colorDatas = ResManager.Inst.LoadRes<ColorLibrary>("ConfigAssets/UGCClothesColorLibrary");
    }

    public override void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        curBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<PGCEffectBehaviour>();
        if (entity.HasComponent<PGCEffectComponent>())
        {
            curComp = entity.Get<PGCEffectComponent>();
            InitPanelStateByComp(curComp);
        }
    }

    private int GetSelectIndex(int EffectId)
    {
        for (int i = 0; i < PGCEffectConfigs.Count; i++)
        {
            if (PGCEffectConfigs[i].id == EffectId)
            {
                return i;
            }
        }
        return 0;
    }

    public void UpdatePGCEffect(int EffectID)
    {
        var curId = PGCEffectManager.Inst.GetLastChooseID();
        DestroyCurNode(curId);
        GetNewObj(EffectID);
        var color = DataUtils.DeSerializeColor(curComp.effectColor);
        curBehav.SetColor(color);
        RefreshSettingPanel();
    }

    public void OnClickPGCEffect(int index)
    {
        var EffectData = PGCEffectConfigs[index];
        var curId = PGCEffectManager.Inst.GetLastChooseID();
        if (EffectData.id == curId)
        {
            return;
        }
        var beginData = CreateUndoData(PGCSelectType.Type, colorType, (int)PanelShowType.Type);
        UpdatePGCEffect(EffectData.id);
        if (string.IsNullOrEmpty(curBehav.ChooseColor))
        {
            colorStr = EffectData.defColor;
        }
        Color curColor = DataUtils.DeSerializeColor(colorStr);
        SetColor(curColor);
        SetSliderColor(curColor);
        SetCurColorSelect(curColor);
        SetPGCEffectSelect(index);
        var endData = CreateUndoData(PGCSelectType.Type, colorType, (int)PanelShowType.Type);
        AddRecord(beginData, endData);
    }

    public void SetPGCEffectUndo(int EffectID, int type, int tabId, bool isExpand, int colorIndex, string srcColor)
    {
        UpdatePGCEffect(EffectID);
        var index = GetSelectIndex(EffectID);
        SetPGCEffectSelect(index);
        Color color = DataUtils.DeSerializeColor(srcColor);
        SetSelectColor(color, (ColorSelectType)type);
        colorStr = srcColor;
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
    }

    private void SetPGCEffectSelect(int index)
    {
        allEffectSelect.ForEach(x => x.SetActive(false));
        if (index < allEffectSelect.Count)
        {
            allEffectSelect[index].SetActive(true);
            var EffectData = PGCEffectConfigs[index];
            PGCEffectManager.Inst.UpdateLastChooseID(EffectData.id);
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

    public void SetPGCEffectColorUndo(int index, int type, string str, int tabId, bool isExpand)
    {
        colorStr = str;
        var EffectData = PGCEffectConfigs[index];
        colorStr = EffectData.defColor;
        SetSelectColor(index, (ColorSelectType)type);

        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
        RefreshScrollPanel(ShowType.Color, isExpand);
    }

    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }

    public void AddRecord(PGCEffectUndoData beginData, PGCEffectUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.PGCEffectUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }
    private PGCEffectUndoData CreateUndoData(PGCSelectType type, ColorSelectType colorType, int tabId = -1, bool isExpand = false)
    {
        var pgcComp = curEntity.Get<PGCEffectComponent>();
        var goComp = curEntity.Get<GameObjectComponent>();
        PGCEffectUndoData data = new PGCEffectUndoData();
        data.effectID = goComp.modId;
        int colorIndex = DataUtils.GetColorSelect(pgcComp.effectColor, colorDatas.List);
        if (colorIndex < 0)
        {
            colorIndex = GetCustomizeColorIndex(DataUtils.DeSerializeColor(pgcComp.effectColor));
        }

        data.colorIndex = colorId;
        data.colorType = (int)colorType;
        data.colorStr = pgcComp.effectColor;
        data.targetEntity = curEntity;
        data.undoType = (int)type;
        data.tabId = tabId;
        data.isExpand = isExpand;
        return data;
    }
}

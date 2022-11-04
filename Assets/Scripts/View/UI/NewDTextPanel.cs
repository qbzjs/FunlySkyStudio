using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 修改此脚本时要考虑旧版3d文字DTextPanel
/// </summary>
public class NewDTextPanel : CommonMatColorPanel<NewDTextPanel>, IUndoRecord
{
    enum PanelShowType
    {
        Text,
        Color,
    };
    public Transform LightParent;
    public RectTransform InputRect;
    private Text inputText;
    private EventTrigger trigger;
    private int maxLength = 80;
    private NewDTextBehaviour dBehav;
    private int matId;
    private string LastStr;
    private List<GameObject> allColors = new List<GameObject>();
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        GetColorDatas();
        CreatePanel();
        CreateColorItems();
        UpdateCustomizePanel();
        AddCommonListener();

        inputText = InputRect.GetComponent<Text>();
        trigger = GetComponentInChildren<EventTrigger>();
        EventTrigger.Entry onSelect = new EventTrigger.Entry();
        onSelect.eventID = EventTriggerType.PointerClick;
        onSelect.callback.AddListener(Select);
        trigger.triggers.Add(onSelect);
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
    public override void GetColorDatas()
    {
        colorDatas = ResManager.Inst.LoadRes<ColorLibrary>("ConfigAssets/UGCClothesColorLibrary");
    }
    public override void SetColor(int index, ColorSelectType type)
    {
        var beginData = CreateUndoData(TextSelectType.Color,colorType,(int)PanelShowType.Color,tabGroup.isExpand);
        Color color = GetCurColor(type, index);
        SetSliderColor(color);
        SetColorSelect(type, index);
        SetColor(color);
        colorStr = DataUtils.ColorToString(color);
        colorType = type;
        colorId = index;
        dBehav.entity.Get<NewDTextComponent>().colorId = colorId;
        var endData = CreateUndoData(TextSelectType.Color,colorType,(int)PanelShowType.Color, tabGroup.isExpand);
        AddRecord(beginData, endData);
    }
    public override void SetColor(Color color)
    {
        dBehav.entity.Get<NewDTextComponent>().col = color;
        dBehav.SetColor(color);
    }
    public void SelectColorUndo(int type,Color color,int tabId,bool isExpand)
    {
        colorStr = DataUtils.ColorToString(color);
        SetSelectColor(color, (ColorSelectType)type);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
        RefreshScrollPanel(ShowType.Color, isExpand);
    }
    private string enterText = "Enter text...";
    private string EnterText { get { return LocalizationConManager.Inst.GetLocalizedText(enterText); } }
    public override void SetEntity(SceneEntity entity)
    {
        dBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NewDTextBehaviour>();
        string content = entity.Get<NewDTextComponent>().content.Trim();
        LastStr = content;
        if (content.Length > maxLength)
        {
            content = content.Substring(0, maxLength) + "...";
        }
        string conStr = string.IsNullOrEmpty(content) ? EnterText : content;
        inputText.text = conStr;
        var matComp = entity.Get<MaterialComponent>();
        matId = matComp.matId;
        var color = dBehav.entity.Get<NewDTextComponent>().col;
        colorStr = DataUtils.ColorToString(color);
        colorId = GetColorIndex(color);
        if (colorId >= 0)
        {
            dBehav.entity.Get<NewDTextComponent>().colorId = colorId;
        }
        SetEntitySelectColor(colorId, DataUtils.ColorToString(color));
    }
    private void Select(BaseEventData data)
    {
        string str = inputText.text.Trim();
        str = str.Equals(EnterText) ? "" : LastStr;
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = EnterText,
            inputMode = 2,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = str,
            returnKeyType = (int)ReturnType.Done
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowKeyBoard);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
        //ShowKeyBoard("asda");
    }

    public void ShowKeyBoard(string str)
    {
        string tempStr = DataUtils.FilterNonStandardText(str);
        var beginData = CreateUndoData(TextSelectType.Text,ColorSelectType.Normal,(int)PanelShowType.Text);
        if (string.IsNullOrEmpty(tempStr.Trim()))
            tempStr = EnterText;
        dBehav.entity.Get<NewDTextComponent>().content = tempStr;
        dBehav.SetContent(tempStr);
        LastStr = tempStr;
        NewTextUndoData endData = CreateUndoData(TextSelectType.Text,ColorSelectType.Normal,(int)PanelShowType.Text);
        AddRecord(beginData, endData);
        SetTextContent(tempStr);
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }
    public void TextUndo(string str,int tabId,bool isExpand)
    {
        if (string.IsNullOrEmpty(str.Trim()))
            str = EnterText;
        dBehav.entity.Get<NewDTextComponent>().content = str;
        dBehav.SetContent(str);
        LastStr = str;
        SetTextContent(str);
        tabGroup.SelectCurUndoTab(tabGroup.GetTab(tabId), isExpand);
    }

    private void SetTextContent(string str)
    {
        if (str.Length > maxLength)
        {
            string temp = str.Substring(0, maxLength);
            inputText.text = temp + "...";
            return;
        }
        inputText.text = str;
    }
    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord(NewTextUndoData beginData, NewTextUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.NewTextUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private NewTextUndoData CreateUndoData(TextSelectType type,ColorSelectType colorType,int tabId=-1,bool isExpand=false)
    {
        NewDTextComponent textComp = dBehav.entity.Get<NewDTextComponent>();
        NewTextUndoData data = new NewTextUndoData();
        data.colorType = (int)colorType;
        data.color = textComp.col;
        data.targetEntity = dBehav.entity;
        data.text = LastStr;
        data.TextType = (int)type;
        data.tabId = tabId;
        data.isExpand = isExpand;
        return data;
    }
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        tabGroup.OpenPreTab();
        RefreshScrollPanel(ShowType.Color, tabGroup.isExpand);
        UpdateCustomizePanel();
    }
    public override void AddTabListener()
    {
        var tab = tabGroup.GetTab((int)PanelShowType.Text);
        tab.AddClickListener(OnTextClick);
        tab = tabGroup.GetTab((int)PanelShowType.Color);
        tab.AddClickListener(OnColorClick);
    }
    public void OnTextClick()
    {
        OpenShowPanel(PanelShowType.Text);
    }
    public void OnColorClick()
    {
        OpenShowPanel(PanelShowType.Color);
    }
    private void OpenShowPanel(PanelShowType type)
    {
        var isExpand = tabGroup.isExpand;
        if(type == PanelShowType.Color)
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
}
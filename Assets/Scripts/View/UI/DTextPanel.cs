using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public struct KeyBoardInfo
{
    public int type;
    public string placeHolder;
    public int inputMode;
    public int maxLength;
    public int inputFlag;
    public string lengthTips;
    public string defaultText;
    public int returnKeyType;
}

public class DTextPanel : InfoPanel<DTextPanel>,IUndoRecord
{
    public Transform LightParent;
    public RectTransform InputRect;
    private Text inputText;
    private EventTrigger trigger;
    private int maxLength = 80;
    private DTextBehaviour dBehav;
    private int matId;
    private int colorId;
    private string LastStr;
    private List<GameObject> allColors = new List<GameObject>();
    private static int firstColorNum = 0;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        InitLightColor();
        inputText = InputRect.GetComponent<Text>();
        trigger = GetComponentInChildren<EventTrigger>();
        EventTrigger.Entry onSelect = new EventTrigger.Entry();
        onSelect.eventID = EventTriggerType.PointerClick;
        onSelect.callback.AddListener(Select);
        trigger.triggers.Add(onSelect);
    }

    private void InitLightColor()
    {
        var itemPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "DirLightColorItem");
        for (int i = 0; i < AssetLibrary.Inst.colorLib.Size(); i++)
        {
            int index = i;
            var itemGo = GameObject.Instantiate(itemPrefab, LightParent);
            itemGo.GetComponent<Image>().color = AssetLibrary.Inst.colorLib.Get(i);
            itemGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectColor(index);
            });
            var colorm = itemGo.transform.Find("sel").gameObject;
            allColors.Add(colorm);
        }
    }
    public void SelectColor(int index)
    {
        if(index== dBehav.entity.Get<DTextComponent>().colorId)
        {
            return;
        }
        var beginData = CreateUndoData(TextSelectType.Color);
        var col = AssetLibrary.Inst.colorLib.Get(index);
        dBehav.entity.Get<DTextComponent>().col = col;
        dBehav.entity.Get<DTextComponent>().colorId = index;
        var endData = CreateUndoData(TextSelectType.Color);
        AddRecord(beginData, endData);
        dBehav.SetColor(col);
        SetColorSelect(index);
    }
    public void SelectColotUndo(int index)
    {
        var col = AssetLibrary.Inst.colorLib.Get(index);
        dBehav.entity.Get<DTextComponent>().col = col;
        dBehav.entity.Get<DTextComponent>().colorId = index;
        dBehav.SetColor(col);
        SetColorSelect(index);
    }
    string enterText = "Enter text...";
    public void SetEntity(SceneEntity entity)
    {
        dBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<DTextBehaviour>();
        string content = entity.Get<DTextComponent>().content.Trim();
        LastStr = content;
        if (content.Length > maxLength)
        {
            content = content.Substring(0, maxLength) + "...";
        }
        string conStr = string.IsNullOrEmpty(content) ? enterText : content;
        LocalizationConManager.Inst.SetLocalizedContent(inputText, conStr);
        var matComp = entity.Get<MaterialComponent>();
        matId = matComp.matId;
        colorId = dBehav.entity.Get<DTextComponent>().colorId;
        var selectIndex = GameManager.Inst.matConfigDatas.FindIndex(x => x.id == matId);
        int selIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(dBehav.entity.Get<DTextComponent>().col), AssetLibrary.Inst.colorLib.List);
        SetColorSelect(selIndex);
    }
    private void Select(BaseEventData data)
    {
        string str = inputText.text.Trim();
        str = str.Equals(LocalizationConManager.Inst.GetLocalizedText(enterText)) ? "" : LastStr;
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = LocalizationConManager.Inst.GetLocalizedText(enterText),
            inputMode = 2,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = str,
            returnKeyType = (int)ReturnType.Done
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowKeyBoard);
        LoggerUtils.Log("JsonUtility.ToJson(keyBoardInfo)==="+ JsonUtility.ToJson(keyBoardInfo));
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
        //ShowKeyBoard("asda");
    }

    public void ShowKeyBoard(string str)
    {
        var beginData = CreateUndoData(TextSelectType.Text);
        if (string.IsNullOrEmpty(str.Trim()))
            str = enterText;
        dBehav.entity.Get<DTextComponent>().content = str;
        dBehav.SetContent(str);
        LastStr = str;
        TextUndoData endData = CreateUndoData(TextSelectType.Text);
        AddRecord(beginData, endData);
        SetTextContent(str);
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }
    public void TextUndo(string str)
    {
        if (string.IsNullOrEmpty(str.Trim()))
            str = enterText;
        dBehav.entity.Get<DTextComponent>().content = str;
        dBehav.SetContent(str);
        LastStr = str;
        SetTextContent(str);
    }

    private void SetTextContent(string str)
    {
        LocalizationConManager.Inst.SetSystemTextFont(inputText);
        if (str.Length > maxLength)
        {
            string temp = str.Substring(0, maxLength);
            inputText.text = temp + "...";
            return;
        }
        if (str == enterText)
        {
            inputText.text = LocalizationConManager.Inst.GetLocalizedText(enterText);
            return;
        }
        inputText.text = str;
    }
    private void SetColorSelect(int index)
    {
        if (index < 0 || index >= allColors.Count)
        {
            LoggerUtils.LogError("Mat ID is Error");
            return;
        }
        allColors.ForEach(x => x.SetActive(false));
        allColors[index].SetActive(true);
    }
    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord(TextUndoData beginData, TextUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.TextUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private TextUndoData CreateUndoData(TextSelectType type)
    {
        TextUndoData data = new TextUndoData();
        data.colorId = dBehav.entity.Get<DTextComponent>().colorId;
        data.targetEntity = dBehav.entity;
        data.text = LastStr;
        data.TextType = (int)type;
        return data;
    }
}
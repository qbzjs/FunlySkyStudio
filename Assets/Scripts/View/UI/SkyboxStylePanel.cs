using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class SkyboxStylePanel : InfoPanel<SkyboxStylePanel>, IUndoRecord
{
    enum SkyboxTabType
    {
        Type,
        Settings
    }

    private SpriteAtlas priAtlas;
    private GameObject priPrefab;
    private Dictionary<int, GameObject> allSelect = new Dictionary<int, GameObject>();
    private static int selectIndex = -1;

    public Transform PriParent;
    public TabSelectGroup tabGroup;
    private TabSelect typeTab;
    private TabSelect settingsTab;

    public Button addDayLengthBtn;
    public Button subDayLengthBtn;
    public Button dayLengthTxtInput;
    public Button dayTimeHourTxtInput;
    public Button dayTimeMinTxtInput;
    public Text dayLengthTxt;
    public Text dayTimeHourTxt;
    public Text dayTimeMinTxt;

    private int MinDayLength = 1;
    private int MaxDayLength = 30;
    private int MinDayTimeHour = 0;
    private int MaxDayTimeHour = 23;
    private int MinDayTimeMinitue = 0;
    private int MaxDayTimeMinitue = 59;

    private int DefaultDayLength = 12;
    private int DefaultDayTimeHour = 6;
    private int DefaultDayTimeMin = 0;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        tabGroup.isExpand = false;
        typeTab = tabGroup.GetTab((int) SkyboxTabType.Type);
        settingsTab = tabGroup.GetTab((int) SkyboxTabType.Settings);
        typeTab.AddClickListener(() => { });
        settingsTab.AddClickListener(() => { });

        addDayLengthBtn.onClick.AddListener(OnAddDayLengthBtnClicked);
        subDayLengthBtn.onClick.AddListener(OnSubDayLengthBtnClicked);
        dayLengthTxtInput.onClick.AddListener(OnInputDayLengthClicked);
        dayTimeHourTxtInput.onClick.AddListener(OnInputInitTimeHourClicked);
        dayTimeMinTxtInput.onClick.AddListener(OnInputInitTimeMinClicked);

        priPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "SkyboxStyleItem");
        priAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/GameAtlas");

        InitDayNightSkybox();
        InitNormalSkybox();

        RefreshSettingTab();
    }

    private void InitDayNightSkybox()
    {
        for (int i = 0; i < GameManager.Inst.skyboxDayNightDatas.Count; i++)
        {
            var skybox = GameManager.Inst.skyboxDayNightDatas[i];
            if (skybox == null || string.IsNullOrEmpty(skybox.iconName))
            {
                continue;
            }

            var itemGo = Instantiate(priPrefab, PriParent);
            itemGo.SetActive(true);
            var selectGo = itemGo.transform.GetChild(1).gameObject;
            selectGo.gameObject.SetActive(false);
            var itemScript = itemGo.transform.GetChild(0).GetComponent<Image>();
            var itemBtn = itemGo.transform.GetChild(0).GetComponent<Button>();
            itemScript.sprite = priAtlas.GetSprite(skybox.iconName);
            itemBtn.onClick.AddListener(() => SelectDayNightSkybox(skybox.id));
            allSelect.Add(skybox.id, selectGo);
        }
    }

    private void InitNormalSkybox()
    {
        for (int i = 0; i < GameManager.Inst.skyboxDatas.Count; i++)
        {
            var skybox = GameManager.Inst.skyboxDatas[i];
            if (skybox == null || string.IsNullOrEmpty(GameManager.Inst.skyboxDatas[i].iconName))
            {
                continue;
            }

            var itemGo = Instantiate(priPrefab, PriParent);
            itemGo.SetActive(true);
            var selectGo = itemGo.transform.GetChild(1).gameObject;
            selectGo.gameObject.SetActive(false);
            allSelect.Add(skybox.id, selectGo);
            var itemScript = itemGo.transform.GetChild(0).GetComponent<Image>();
            var itemBtn = itemGo.transform.GetChild(0).GetComponent<Button>();
            itemScript.sprite = priAtlas.GetSprite(skybox.iconName);
            itemBtn.onClick.AddListener(() => SelectSky(skybox.id));
        }
    }

    private void SelectDayNightSkybox(int skyboxId)
    {
        var begin = CreateUndoData(GetCurSkyboxType());
        HighLight(skyboxId);

        //恢复到默认天空盒参数
        SkyboxCreater.SetSkyboxData(new SkyData()
        {
            skyId = skyboxId,
            skyboxType = (int) SkyboxType.DayNight,
            dayLength = DefaultDayLength,
            daytimeHour = DefaultDayTimeHour,
            daytimeMin = DefaultDayTimeMin
        });

        SceneBuilder.Inst.SkyboxBev.SetDayNightSkybox(skyboxId);
        RefreshSettingTab();
        var end = CreateUndoData(SkyboxType.DayNight);
        AddRecord(begin, end);
    }

    public void SelectSkyUndo(SkyboxUndoData undoData)
    {
        HighLight(undoData.skyboxId);

        if (undoData.skyboxType == SkyboxType.Normal)
        {
            var skyboxId = undoData.skyboxId;
            SkyboxCreater.SetSkyboxData(new SkyData()
            {
                skyId = skyboxId,
                skyboxType = (int) SkyboxType.Normal,
                dayLength = 0,
                daytimeHour = 0,
                daytimeMin = 0
            });

            SceneBuilder.Inst.SkyboxBev.SetNormalSky(skyboxId);
            
        }
        else if (undoData.skyboxType == SkyboxType.DayNight)
        {
            var skyboxId = undoData.skyboxId;
            SkyboxCreater.SetSkyboxData(new SkyData()
            {
                skyId = skyboxId,
                skyboxType = (int) SkyboxType.DayNight,
                dayLength = DefaultDayLength,
                daytimeHour = DefaultDayTimeHour,
                daytimeMin = DefaultDayTimeMin
            });

            SceneBuilder.Inst.SkyboxBev.SetDayNightSkybox(skyboxId);
        }
        
        tabGroup.SelectCurTab(typeTab);
        RefreshSettingTab();
    }

    public void SelectSky(int skyboxId)
    {
        var begin = CreateUndoData(GetCurSkyboxType());
        HighLight(skyboxId);
        SkyboxCreater.SetSkyboxData(new SkyData()
        {
            skyId = skyboxId,
            skyboxType = (int) SkyboxType.Normal,
            dayLength = 0,
            daytimeHour = 0,
            daytimeMin = 0
        });

        SceneBuilder.Inst.SkyboxBev.SetNormalSky(skyboxId);

        RefreshSettingTab();
        var end = CreateUndoData(SkyboxType.Normal);
        AddRecord(begin, end);
    }

    public void HighLight(int pid)
    {
        if (pid != selectIndex)
        {
            DisHighlight();
            selectIndex = pid;
        }

        if (allSelect.ContainsKey(selectIndex))
        {
            allSelect[selectIndex].SetActive(true);
        }
    }

    public virtual void DisHighlight()
    {
        if (allSelect.ContainsKey(selectIndex))
        {
            allSelect[selectIndex].SetActive(false);
        }

        selectIndex = -1;
    }

    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }

    public void AddRecord(SkyboxUndoData beginData, SkyboxUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.SkyboxUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private SkyboxUndoData CreateUndoData(SkyboxType skyboxType)
    {
        SkyboxUndoData data = new SkyboxUndoData();
        data.skyboxId = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().skyboxId;
        data.skyboxType = skyboxType;
        return data;
    }

    private SkyboxType GetCurSkyboxType()
    {
        return SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().skyboxType;
    }
    
    
    #region Settings Tab

    private void RefreshSettingTab()
    {
        dayLengthTxt.text = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().dayLength.ToString();
        var hour = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().daytimeHour;
        var min = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().daytimeMin;
        dayTimeHourTxt.text = (hour >= 10 ? "" : "0") + hour;
        dayTimeMinTxt.text = (min >= 10 ? "" : "0") + min;

        settingsTab.gameObject.SetActive(GetCurSkyboxType() == SkyboxType.DayNight);
    }

    private void OnAddDayLengthBtnClicked()
    {
        var oldV = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().dayLength;
        if (oldV + 1 > 30)
        {
            TipPanel.ShowToast("Up to {0}", 30);
        }

        var newV = Mathf.Clamp(++oldV, MinDayLength, MaxDayLength);
        SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().dayLength = newV;
        RefreshSettingTab();
    }

    private void OnSubDayLengthBtnClicked()
    {
        var oldV = SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().dayLength;
        if (oldV - 1 <= 0)
        {
            TipPanel.ShowToast("At least {0}", 1);
        }

        var newV = Mathf.Clamp(--oldV, MinDayLength, MaxDayLength);
        SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().dayLength = newV;
        RefreshSettingTab();
    }

    private void OnInputDayLengthClicked()
    {
        string str = dayLengthTxt.text.Trim();
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = str,
            inputMode = 1,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = str,
            returnKeyType = (int) ReturnType.Return
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, HandleDayLengthInput);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }

    private void OnInputInitTimeHourClicked()
    {
#if UNITY_EDITOR
        HandleDayTimeHourInput("26");
#else
        string str = dayTimeHourTxt.text.Trim();
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = str,
            inputMode = 1,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = str,
            returnKeyType = (int) ReturnType.Return
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, HandleDayTimeHourInput);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
#endif
    }

    private void OnInputInitTimeMinClicked()
    {
#if UNITY_EDITOR
        HandleDayTimeMinInput("99");
#else
        string str = dayTimeMinTxt.text.Trim();
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = str,
            inputMode = 1,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = str,
            returnKeyType = (int) ReturnType.Return
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, HandleDayTimeMinInput);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
#endif
    }

    #endregion

    #region Input Handler

    private void HandleDayLengthInput(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        //记录玩家输入的数量
        int pointNum;
        //判断如果输入不是整数则跳过并且弹tips
        if (!int.TryParse(str, out pointNum))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }

        //最大最小值限制
        pointNum = Mathf.Clamp(pointNum, MinDayLength, MaxDayLength);
        dayLengthTxt.text = pointNum.ToString();
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
        SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().dayLength = pointNum;

        RefreshSettingTab();
    }

    private void HandleDayTimeHourInput(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        //记录玩家输入的数量
        int pointNum;
        //判断如果输入不是整数则跳过并且弹tips
        if (!int.TryParse(str, out pointNum))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }

        //最大最小值限制
        pointNum = Mathf.Clamp(pointNum, MinDayTimeHour, MaxDayTimeHour);
        dayTimeHourTxt.text = pointNum.ToString();
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);

        SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().daytimeHour = pointNum;
        RefreshSettingTab();
        
        SceneBuilder.Inst.SkyboxBev.SetSkyboxTime();
    }

    private void HandleDayTimeMinInput(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        //记录玩家输入的数量
        int pointNum;
        //判断如果输入不是整数则跳过并且弹tips
        if (!int.TryParse(str, out pointNum))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }

        //最大最小值限制
        pointNum = Mathf.Clamp(pointNum, MinDayTimeMinitue, MaxDayTimeMinitue);
        dayTimeMinTxt.text = pointNum.ToString();
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);

        SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().daytimeMin = pointNum;
        RefreshSettingTab();
        
        SceneBuilder.Inst.SkyboxBev.SetSkyboxTime();
    }

    #endregion
}
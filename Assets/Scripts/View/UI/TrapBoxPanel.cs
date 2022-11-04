using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Author: 熊昭
/// Description: 陷阱盒道具面板操作类
/// Date: 2021-11-26 14:50:28
/// </summary>
public class TrapBoxPanel : InfoPanel<TrapBoxPanel>
{
    [SerializeField]
    private Toggle textToggle;

    [SerializeField]
    private Toggle customSpawnToggle;

    [SerializeField]
    private Button backToSpawnBtn;

    [SerializeField]
    private Button reduceHpBtn;

    [SerializeField]
    private Text customText;
    [SerializeField]
    private Text numText;
    [SerializeField]
    private Image textLine;

    private EventTrigger trigger;
    private SceneEntity curEntity;

    // private string defaultText = "Oops! You triggered a trap! Return to spawn point.";
    private string defaultText = "Oops! You triggered a trap!";
    private int maxNum = 60;
    private Color enColor = Color.white;
    private Color disColor = new Color(1, 1, 1, 0.38f);

    private bool backToSpawnState = false;
    private bool reduceHpState = false;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        backToSpawnBtn.onClick.AddListener(OnBackToSpawnClick);
        customSpawnToggle.onValueChanged.AddListener(OnCustomSpawnSelect);
        textToggle.onValueChanged.AddListener(OnTextSelect);
        reduceHpBtn.onClick.AddListener(OnReduceHpClick);

        trigger = GetComponentInChildren<EventTrigger>();
        EventTrigger.Entry onSelect = new EventTrigger.Entry();
        onSelect.eventID = EventTriggerType.PointerClick;
        onSelect.callback.AddListener(SelectText);
        trigger.triggers.Add(onSelect);
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        var tComp = entity.Get<TrapBoxComponent>();
        LoggerUtils.Log("SetEntity getBackSpawnBtnState  tComp.rePos = " +  tComp.rePos);
        SetBackSpawnBtnState(tComp.rePos != (int)TrapBoxTrans.NoTrans);
        customSpawnToggle.isOn = (tComp.rePos == (int)TrapBoxTrans.CustomSpawn);
        customSpawnToggle.gameObject.SetActive(tComp.rePos != (int)TrapBoxTrans.NoTrans);
        textToggle.isOn = tComp.reTex == 1;
        SetReduceHpBtnState(tComp.hitState == 1);
        SetTextEnable(tComp.reTex == 1);
        SetContent(tComp.text);

        
    }

    public void SetContent(string content)
    {
        var tComp = curEntity.Get<TrapBoxComponent>();
        if (tComp.reTex == 1 && !string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(content.Trim()))
        {
            string str = content.TrimStart().TrimEnd();
            SetTextContent(str, str.Length, enColor);
        }
        else
        {
            string defStr = LocalizationConManager.Inst.GetLocalizedText(defaultText);
            SetTextContent(defStr, 0, disColor);
        }
    }

    private void SetTextContent(string content, int conNum, Color textColor)
    {
        LocalizationConManager.Inst.SetLocalizedContent(customText, "{0}", content);
        numText.text = string.Format("{0}/{1}", Mathf.Min(conNum, maxNum), maxNum);

        customText.color = textColor;
        numText.color = textColor;
    }

    private void OnBackToSpawnClick()
    {
        var tComp = curEntity.Get<TrapBoxComponent>();
        if(tComp.rePos != (int)TrapBoxTrans.NoTrans)
        {
            if(reduceHpState == false)
            {
                ShowLimitSelectToast();
                return;
            }
            customSpawnToggle.gameObject.SetActive(false);
            customSpawnToggle.isOn = false;
            SetBackSpawnBtnState(false);
            tComp.rePos = (int)TrapBoxTrans.NoTrans;
            LoggerUtils.Log("SetBackSpawnBtnState  tComp.rePos = 2");
        }
        else
        {
            customSpawnToggle.gameObject.SetActive(true);
            customSpawnToggle.isOn = false;
            SetBackSpawnBtnState(true);
            tComp.rePos = (int)TrapBoxTrans.MapSpawn;
            LoggerUtils.Log("SetBackSpawnBtnState  tComp.rePos = 0");
        }
    }

    private void OnReduceHpClick()
    {
        var tComp = curEntity.Get<TrapBoxComponent>();
        if (tComp.hitState == 0)
        {
            SetReduceHpBtnState(true);
            tComp.hitState = 1;
        }
        else
        {
            if(backToSpawnState == false)
            {
                ShowLimitSelectToast();
                return;
            }
            SetReduceHpBtnState(false);
            tComp.hitState = 0;
        }
    }

    private void OnCustomSpawnSelect(bool isOn)
    {
        var tComp = curEntity.Get<TrapBoxComponent>();
        if (tComp.rePos == (int)TrapBoxTrans.CustomSpawn && isOn == true)
        {
            return;
        }

        if(isOn){
            tComp.rePos = (int)TrapBoxTrans.CustomSpawn; 
        }
        else
        {
            tComp.rePos = (int)TrapBoxTrans.MapSpawn;
        }
        LoggerUtils.Log("OnCustomSpawnSelect  tComp.rePos = " + tComp.rePos);
        CustomSpawnPoint(isOn);
    }

    private void CustomSpawnPoint(bool isSelect)
    {
        var gComp = curEntity.Get<GameObjectComponent>();
        var tBehav = gComp.bindGo.GetComponent<TrapBoxBehaviour>();
        if (isSelect)
        {
            var pointBehv= SceneBuilder.Inst.CreateTrapSpawn(tBehav.entity);
            EditModeController.AddCreateRecord(pointBehv.gameObject);
        }
        else
        {
            
            var tComp = curEntity.Get<TrapBoxComponent>();
            var pointEntity = TrapSpawnManager.Inst.GetPointGo(tComp.tId);
            if (pointEntity != null)
            { 
                // SceneBuilder.Inst.DestroyEntity(pointEntity.Get<GameObjectComponent>().bindGo);
                var pointTarget = pointEntity.Get<GameObjectComponent>().bindGo;
                TrapSpawnManager.Inst.RemoveTrapSpawn(tComp.tId);
                EditModeController.AddDestroyRecord(pointTarget);
                SecondCachePool.Inst.DestroyEntity(pointTarget);
            }
        }
        tBehav.RefreshShowId();
    }

    private void OnTextSelect(bool isOn)
    {
        int state = isOn ? 1 : 0;
        var tComp = curEntity.Get<TrapBoxComponent>();
        if (tComp.reTex == state)
        {
            return;
        }
        tComp.reTex = state;
        SetTextEnable(isOn);

        if (!isOn)
        {
            tComp.text = "";
            SetContent(tComp.text);
        }
    }

   
    private void SetTextEnable(bool state)
    {
        trigger.enabled = state;
        textLine.color = state ? enColor : disColor;
    }

    private void SelectText(BaseEventData data)
    {
        var tComp = curEntity.Get<TrapBoxComponent>();
        string str = string.IsNullOrEmpty(tComp.text) ? "" : tComp.text.TrimStart().TrimEnd();
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = "",
            inputMode = 0,
            maxLength = maxNum,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = str,
            returnKeyType = (int)ReturnType.Done
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowKeyBoard);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }

    //必须至少勾选一个选项
    private void ShowLimitSelectToast()
    {
        string tipsText = LocalizationConManager.Inst.GetLocalizedText("Please select at least one option");
        TipPanel.ShowToast("{0}", tipsText);
    }

    public void ShowKeyBoard(string str)
    {
        var tComp = curEntity.Get<TrapBoxComponent>();
        string content = string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str.Trim()) ? "" : str;
        content = content.TrimStart().TrimEnd();
        tComp.text = content;
        SetContent(content);
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }

    public void SetBackSpawnBtnState(bool isSelected)
    {
        SetBtnState(backToSpawnBtn,isSelected);
        backToSpawnState = isSelected;
    }

    public void SetReduceHpBtnState(bool isSelected)
    {
        SetBtnState(reduceHpBtn,isSelected);
        reduceHpState = isSelected;
    }

    public void SetBtnState(Button btn,bool isSelected)
    {
        Transform checkImg = btn.gameObject.transform.Find("Background/Checkmark");
        if(checkImg)
        {
            checkImg.gameObject.SetActive(isSelected);
        }
    }


}
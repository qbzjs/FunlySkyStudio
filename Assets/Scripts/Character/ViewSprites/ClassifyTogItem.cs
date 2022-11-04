using System;
using System.Collections;
using System.Collections.Generic;
using RedDot;
using UnityEngine;
using UnityEngine.UI;
using GRTools.Localization;
using System.Text;

public class ClassifyTogItem : MonoBehaviour
{
    public Toggle[] SubToggles;
    public GameObject[] Panels;
    public Action<int> UpdateOnSelectSub;
    public Action<int> ClearRed;
    private string OriginRed, MarketRed, DcRed;
    public Text originalText, MarketText, DCText;
    #region 本地化翻译截取字符
    private void Awake()
    {
        originalText.GetComponent<LocalizationComponent>().OnChangeContent = OnLocalOrContent;
        if (MarketText)
        {
            MarketText.GetComponent<LocalizationComponent>().OnChangeContent = OnLocalMarketContent;
        }
        DCText.GetComponent<LocalizationComponent>().OnChangeContent = OnLocalDCContent;
    }
    public void OnLocalOrContent(LanguageCode languageCode, Component component, string text)
    {
        var otiginal = GameUtils.SubStringByBytes(text, 16, Encoding.Unicode);
        component.GetComponent<Text>().text = otiginal;
    }
    public void OnLocalMarketContent(LanguageCode languageCode, Component component, string text)
    {
        var marktetPlace = GameUtils.SubStringByBytes(text, 22, Encoding.Unicode);
        component.GetComponent<Text>().text = marktetPlace;
    }
    public void OnLocalDCContent(LanguageCode languageCode, Component component, string text)
    {
        var Dc = GameUtils.SubStringByBytes(text, 40, Encoding.Unicode);
        component.GetComponent<Text>().text = Dc;
    }
    #endregion
    public void Start()
    {
        InitToggle();
    }
    private void InitToggle()
    {
        for (var i = 0; i < SubToggles.Length; i++)
        {
            int index = i;
            SubToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    SelectSub(index);
                }
            });
        } 
    }
    /// <summary>
    /// 选中对应Tog(Wear,公共avatar)
    /// </summary>
    /// <param name="index"></param>
    public void SetSelectTogByIndex(int index)
    {
        if (SubToggles[index].isOn)
        {
            SelectSub(index);
        }
        else
        {
            SubToggles[index].isOn = true;
        }
    }
    /// <summary>
    /// 同步选中当前三级tab
    /// </summary>
    public void UpdateSelectTab()
    {
        int curIndex = Array.FindIndex(SubToggles, (tog) => tog.isOn);
        SetSelectTogByIndex(curIndex);
    }
    /// <summary>
    /// Tab点击事件
    /// </summary>
    /// <param name="index"></param>
    public void SelectSub(int index)
    {
        for (var i = 0; i < Panels.Length; i++)
        {
            Panels[i].SetActive(false);
            SubToggles[i].GetComponent<Text>().color = new Color32(151, 151, 151, 255);
        }
        Panels[index].SetActive(true);
        SubToggles[index].GetComponent<Text>().color = new Color32(0, 0, 0, 255);
        ClearRed?.Invoke(index);
        UpdateOnSelectSub?.Invoke(index);
    }
    public void SetSelectAction(Action<int> selectaction, Action<int> clearRed)
    {
        this.UpdateOnSelectSub = selectaction;
        this.ClearRed = clearRed;
    }
    public void NewUserUiSetting()
    {
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
        {
            this.gameObject.SetActive(false);
            for (int i = 0; i < Panels.Length; i++)
            {
                var rectTrans = Panels[i].GetComponent<RectTransform>();
                rectTrans.offsetMin = Vector2.zero;
                rectTrans.offsetMax = Vector2.zero;
            }
        }
    }

}

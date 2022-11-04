using System;
using System.Text;
using GRTools.Localization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:WenJia
/// Description: Avatar 操作确认通用弹窗
/// Date: 2022/5/6 17:18:28
/// </summary>


public class CharacterTipDialogPanel : BasePanel<CharacterTipDialogPanel>
{
    public Button CancelBtn;
    public Button RightBtn;
    public Text ContentText, RightBtnText, CancelBtnText;
    public Action RightBtnClickAct;
    protected override void Awake()
    {
        CancelBtnText.GetComponent<LocalizationComponent>().OnChangeContent = OnLocalizationContent;
    }

    public void OnLocalizationContent(LanguageCode languageCode, Component component, string text)
    {
        var cancelText = GameUtils.SubStringByBytes(text, 16, Encoding.Unicode);
        component.GetComponent<Text>().text = cancelText;
    }
    private void Start()
    {
        CancelBtn.onClick.AddListener(OnCancelBtnClick);
        RightBtn.onClick.AddListener(OnRightBtnClick);
    }

    public void OnCancelBtnClick()
    {
        Hide();
    }

    public void OnRightBtnClick()
    {
        RightBtnClickAct?.Invoke();
        Hide();
    }

    public void SetTitle(string content, string RBtnTxt, params object[] formatArgs)
    {
        LocalizationConManager.Inst.SetLocalizedContent(ContentText, content, formatArgs);
        LocalizationConManager.Inst.SetLocalizedContent(RightBtnText, RBtnTxt, formatArgs);
        //本地化截取字符
        var rightBtnText = GameUtils.SubStringByBytes(RightBtnText.text, 16, Encoding.Unicode);
        RightBtnText.text = rightBtnText;
    }
}

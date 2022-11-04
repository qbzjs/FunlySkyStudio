using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:Meimei-LiMei
/// Description:二次确认弹窗
/// Date: 2022/6/23 19:59:21
/// </summary>
public class SecondComfirmPanel : BasePanel<SecondComfirmPanel>
{
    public Button LeftBtn;
    public Button RightBtnBtn;
    public Text TitleText;
    public Text LeftBtnText;
    public Text RightBtnText;
    public Action LeftBthClickAct;
    public Action RightBtnClickAct;
     public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        LeftBtn.onClick.AddListener(OnLestBtnClick);
        RightBtnBtn.onClick.AddListener(OnRightBtnClick);
    }
    public void OnLestBtnClick()
    {
        LeftBthClickAct?.Invoke();
        Hide();
    }

    public void OnRightBtnClick()
    {
        RightBtnClickAct?.Invoke();
        Hide();
    }
    /// <summary>
    /// 设置文案
    /// </summary>
    /// <param name="titleTxt">Tips</param>
    /// <param name="LBtnTxt">左边按钮文案</param>
    /// <param name="RBtnTxt">右边按钮文案</param>
    public void SetTitle(string titleTxt, string LBtnTxt, string RBtnTxt, params object[] formatArgs)
    {
        LocalizationConManager.Inst.SetLocalizedContent(TitleText, titleTxt, formatArgs);
        LocalizationConManager.Inst.SetLocalizedContent(RightBtnText, RBtnTxt, formatArgs);
        LocalizationConManager.Inst.SetLocalizedContent(LeftBtnText, LBtnTxt, formatArgs);
        //本地化截取字符
        LoggerUtils.Log("secondConfiemPanel+leftBtnText--before:" + LeftBtnText.text);
        LoggerUtils.Log("secondConfiemPanel+rightBtnText--before:" + RightBtnText.text);
        var leftBtnText = GameUtils.SubStringByBytes(LeftBtnText.text, 20, Encoding.Unicode);
        var rightBtnText = GameUtils.SubStringByBytes(RightBtnText.text, 20, Encoding.Unicode);
        LeftBtnText.text = leftBtnText;
        RightBtnText.text = rightBtnText;
        LoggerUtils.Log("secondConfiemPanel+leftBtnText:" + leftBtnText);
        LoggerUtils.Log("secondConfiemPanel+rightBtnText:" + rightBtnText);
    }
}

/// <summary>
/// Author:Mingo-LiZongMing
/// Description:分队需求-Button
/// Date: 2022-6-24 14:08:22
/// </summary>
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MemberButtonItem : MonoBehaviour
{
    private bool isSelected = false;
    private Image ButtonBg;
    private Text btnText;
    private Button comBtn;
    private GameObject selectGo;

    private Color SelectBtnColor = new Color(255, 255, 255, 255);
    private Color UnSelectBtnColor = new Color(255, 255, 255, 0);
    private Color SelectTxtColor = new Color(0.59f, 0.59f, 0.59f, 1);
    private Color UnSelectTxtColor = new Color(255, 255, 255, 255);

    public void Init()
    {
        ButtonBg = this.GetComponent<Image>();
        comBtn = this.GetComponent<Button>();
        btnText = this.transform.GetChild(0).GetComponent<Text>();
        selectGo = btnText.transform.GetChild(0).gameObject;
    }

    public void SetSelectState(bool isSelect)
    {
        if (isSelect)
        {
            isSelected = true;
            ButtonBg.color = SelectBtnColor;
            btnText.color = SelectTxtColor;
        }
        else
        {
            isSelected = false;
            ButtonBg.color = UnSelectBtnColor;
            btnText.color = UnSelectTxtColor;
        }
        selectGo.SetActive(!isSelect);
    }

    public void SetText(string content)
    {
        LocalizationConManager.Inst.SetLocalizedContent(btnText, content);
    }

    public string GetText()
    {
        return btnText.text;
    }

    public void AddClick(UnityAction act)
    {
        comBtn.onClick.AddListener(act);
    }

    public bool GetSelectState()
    {
        return isSelected;
    }
}

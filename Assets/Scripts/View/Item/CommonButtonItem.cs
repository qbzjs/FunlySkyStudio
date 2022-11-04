
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CommonButtonItem : MonoBehaviour
{
    private Text btnText;
    private Button comBtn;
    private GameObject selectGo;

    public void Init()
    {
        comBtn = this.GetComponent<Button>();
        btnText = this.transform.GetChild(0).GetComponent<Text>();
        selectGo = btnText.transform.GetChild(0).gameObject;
    }

    public void SetSelectState(bool isSelect)
    {
        selectGo.SetActive(isSelect);
    }

    public bool GetSelectState()
    {
        return selectGo.activeSelf;
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
}

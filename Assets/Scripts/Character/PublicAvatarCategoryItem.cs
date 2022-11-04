/// <summary>
/// Author:Mingo-LiZongMing
/// Description:
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PublicAvatarCategoryItem : MonoBehaviour
{
    private Text btnText;
    private Button comBtn;
    private GameObject selectGo;
    private Image iconImg;

    public void Init()
    {
        comBtn = this.GetComponent<Button>();
        iconImg = this.transform.GetChild(0).GetComponent<Image>();
        btnText = this.transform.GetChild(1).GetComponent<Text>();
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

    public void SetIconImage(Sprite sprite)
    {
        if (sprite != null)
        {
            iconImg.sprite = sprite;
        }
    }

    public Image GetIconImage()
    {
        return iconImg;
    }
}

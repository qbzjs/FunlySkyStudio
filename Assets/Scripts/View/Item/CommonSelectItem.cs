using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CommonSelectItem : MonoBehaviour
{
    public Image Icon;
    public Text BtnText;
    public Button ComBtn;
    public GameObject SelectGo;
    public Animator Anim;

    public void Init()
    {
        ComBtn = this.GetComponent<Button>();
        BtnText = this.transform.GetChild(0).GetComponent<Text>();
        SelectGo = this.transform.GetChild(1).gameObject;
    }

    public void SetSelectState(bool isSelect)
    {
        SelectGo.SetActive(isSelect);
    }

    public void SetAnim(bool isPlay)
    {
        if (isPlay)
        {
            Anim.Play("bgmusic");
        }
        Icon.gameObject.SetActive(!isPlay);
        Anim.gameObject.SetActive(isPlay);
    }

    public void SetIcon(Sprite spr)
    {
        Icon.sprite = spr;
    }

    public void SetText(string content)
    {
        LocalizationConManager.Inst.SetLocalizedContent(BtnText, content);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetChild(0).GetComponent<RectTransform>());
        transform.GetComponent<RectTransform>().sizeDelta = transform.GetChild(0).GetComponent<RectTransform>().sizeDelta;   
    }

    public void AddClick(UnityAction act)
    {
        ComBtn.onClick.AddListener(act);
    }
}

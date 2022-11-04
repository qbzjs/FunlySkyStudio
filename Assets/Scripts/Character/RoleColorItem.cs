using System;
using GameData;
using UnityEngine;
using UnityEngine.UI;
public class RoleColorItem : MonoBehaviour
{
    public Button ColorBtn;
    public Image colorImg;
    public GameObject colorSelectGo;
    public string rcData;
    private Action<RoleColorItem> OnSelect;
    private Vector2 originalVec
    {
        get{return this.GetComponent<RectTransform>().sizeDelta;}
    }
    void Start()
    {
        ColorBtn.onClick.AddListener(OnSelectClick);
    }
    public void Init(string color, Action<RoleColorItem> select)
    {
        rcData = color;
        OnSelect = select;
        colorImg.color = DataUtils.DeSerializeColorByHex(rcData);
        colorSelectGo.SetActive(false);
    }

    public void SetSelectState(bool isVisible)
    {
        colorSelectGo.SetActive(isVisible);
        colorSelectGo.GetComponent<Image>().color = ColorBtn.GetComponent<Image>().color;
        this.ColorBtn.GetComponent<RectTransform>().sizeDelta=isVisible? originalVec*0.75f:originalVec;
    }

    private void OnSelectClick()
    {
        OnSelect?.Invoke(this);
    }

}
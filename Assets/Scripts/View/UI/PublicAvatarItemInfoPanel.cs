/// <summary>
/// Author:Mingo-LiZongMing
/// Description:
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PublicAvatarItemInfoPanel : MonoBehaviour
{
    public Image _iconImg;
    public Text _txtTittle;
    public Text _txtDesc;
    public Button _btnInfo;
    public Image _iconLabel;
    public RectTransform content;
    private Action action;

    private const int MaxTittleLength = 20;
    private const int MaxDescLength = 60;

    private void Awake()
    {
        _btnInfo.onClick.AddListener(OnBtnInfoClick);
    }

    public void SetIconImage(Sprite sprite)
    {
        if (sprite != null)
        {
            _iconImg.sprite = sprite;
        }
    }

    public void SetTittle(string txt)
    {
        txt = DataUtils.FilterNonStandardText(txt);
        LocalizationConManager.Inst.SetLocalizedContent(_txtTittle, txt);
        if (_txtTittle.text.Length > MaxTittleLength)
        {
            _txtTittle.text = _txtTittle.text.Substring(0, MaxTittleLength) + "...";
        }
    }

    public void SetDesc(string txt)
    {
        txt = DataUtils.FilterNonStandardText(txt);
        LocalizationConManager.Inst.SetLocalizedContent(_txtDesc, txt);
        if (_txtDesc.text.Length > MaxDescLength)
        {
            _txtDesc.text = _txtDesc.text.Substring(0, MaxDescLength) + "...";
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public void SetLabelIcon(LabelType labelType)
    {
        if (labelType == LabelType.NONE)
        {
            _iconLabel.gameObject.SetActive(false);
        }
        else
        {
            string iconName = string.Format("ic_{0}Icon", labelType.ToString().ToLower());
            _iconLabel.sprite = SpriteAtlasManager.Inst.GetAvatarCommonSprite(iconName);
            _iconLabel.gameObject.SetActive(true);
        }
    }

    public void SetOnClickAction(Action act)
    {
        action = act;
    }

    private void OnBtnInfoClick()
    {
        action?.Invoke();
    }
}

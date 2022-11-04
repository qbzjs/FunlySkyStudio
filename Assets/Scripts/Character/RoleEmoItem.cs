using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleEmoItem : MonoBehaviour
{
    private Button EmoBtn;
    private Image EmoImg;
    private GameObject SelectImg;
    private string EmoName;
    private int PoseId;
    private Action<string,int> selectClick;
    private static RoleEmoItem curItem;

    void Awake()
    {
        EmoBtn = gameObject.GetComponent<Button>();
        EmoImg = gameObject.transform.Find("Icon_Emo").GetComponent<Image>();
        SelectImg = gameObject.transform.Find("IsSelected").gameObject;
        EmoBtn.onClick.AddListener(OnSelectClick);
    }

    public void SetSprite(Sprite sprite)
    {
        EmoImg.sprite = sprite;
    }

    public void SetEmoName(string emoName,int poseId, Action<string,int> act)
    {
        EmoName = emoName;
        PoseId = poseId;
        selectClick = act;
    }

    public void SetSelect(bool isSelected)
    {
        curItem = this;
        SelectImg.SetActive(isSelected);
    }

    public void OnSelectClick()
    {
        if (curItem != null)
        {
            curItem.SetSelect(false);
        }
        SetSelect(true);
        selectClick?.Invoke(EmoName, PoseId);
    }

}

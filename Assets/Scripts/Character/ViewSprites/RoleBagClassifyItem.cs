using System;
using System.Collections;
using System.Collections.Generic;
using AvatarRedDotSystem;
using RedDot;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RoleBagClassifyItem : MonoBehaviour
{
    public Text BtnText;
    public Image SelectImg;
    public Button ClassifyBtn;
    private Action<BagCompType> selectClick;
    private Color defColor = new Color32(151, 151, 151, 255);
    private Color selectColor = new Color32(30, 31, 36, 255);
    [HideInInspector]
    public VNode mVNode;
    public BagCompTypeData data;
    public void Start()
    {
        ClassifyBtn.onClick.AddListener(OnSelectClick);
    }
    public void SetData(BagCompTypeData data)
    {
        this.data = data;
        LocalizationConManager.Inst.SetLocalizedContent(BtnText, data.showName);
    }
    public void SetAction(Action<BagCompType> act)
    {
        this.selectClick = act;
    }
    public void OnSelectClick()
    {
        selectClick?.Invoke((BagCompType)data.bagCompType);
    }
    public void SetSelectState(bool isVisible)
    {
        SelectImg.gameObject.SetActive(isVisible);
        BtnText.color = isVisible ? selectColor : defColor;
        if (isVisible)
        {
            ClearRed(data.redDotsKind);
        }
    }
    public void AttachRedDot()
    {
        var redDotMan = RoleMenuView.Ins.mAvatarRedDotManager;
        foreach (var item in redDotMan.mAvatarRedInfos)
        {
            if (item.resourceKind.Equals(data.redDotsKind))
            {
                mVNode = redDotMan.Tree.AddRedDot(gameObject, data.parentNode, data.selfNode, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                mVNode.mLogic.ChangeCount(1);
            }
        }
    }
    public void ClearRed(string redstring)
    {
        if (mVNode != null && mVNode.mLogic.Count > 0)
        {
            mVNode.mLogic.ChangeCount(mVNode.mLogic.Count - 1);
            RoleMenuView.Ins.mAvatarRedDotManager.ReqCleanRedDot(redstring);
        }
    }
}

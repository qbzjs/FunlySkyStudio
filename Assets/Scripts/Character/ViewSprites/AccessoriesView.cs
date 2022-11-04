using AvatarRedDotSystem;
using RedDot;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象—配饰UI显示
/// Date: 2022/4/1 16:48:33
/// </summary>
public class AccessoriesView : BaseView
{
    public RoleStyleView[] iconView;
    public Toggle[] SubToggles;
    public GameObject[] Panels;
    public GameObject[] newImage;
    public GameObject toggleParent;
    public VNode mDCRedDotNode;
    public VNode mOriginalRedDotNode;
    private int curSelectTogIndex = 0;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitAccessoriesView);
    }

    public void InitAccessoriesView()
    {
        this.bodyPart = BodyPartType.body;
        this.classifyType = ClassifyType.accessories;
        iconView[0].part = bodyPart;
        iconView[0].type = classifyType;
        NewUserUiSetting(toggleParent, Panels);
        InitToggle();
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetAccessoriesStylesDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectAccessorIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.acId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var AccessorStyleData = RoleConfigDataManager.Inst.GetAccessoriesStylesDataById(itemId);
        if (AccessorStyleData != null)
        {
            AccessorStyleData.rc = roleStyleItem;
            OnSelectAccessorIcon(AccessorStyleData);
        }
    }

    public override void OnSelect()
    {
        UpdateSelectState();
        SelectSub(curSelectTogIndex);
    }

    public override void UpdateSelectState()
    {
        int index = Array.FindIndex(SubToggles, (tog) => tog.isOn);
        UpdateOnSelectSub(index);
    }

    private void InitToggle()
    {
        for (var i = 0; i < SubToggles.Length; i++)
        {
            int index = i;
            SubToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    SelectSub(index);
                    UpdateOnSelectSub(index);
                }
            });
        }
    }

    private void UpdateOnSelectSub(int index)
    {
        iconView[index].GetAllItemList(InitListAction);
        iconView[index].curItem = null;
        iconView[index].SetSelect(roleData.acId);
    }

    private void SelectSub(int index)
    {
        for (var i = 0; i < Panels.Length; i++)
        {
            Panels[i].SetActive(false);
            SubToggles[i].GetComponent<Text>().color = new Color32(151, 151, 151, 255);
        }
        Panels[index].SetActive(true);
        SubToggles[index].GetComponent<Text>().color = new Color32(0, 0, 0, 255);
        newImage[index].gameObject.SetActive(false);
        curSelectTogIndex = index;
        if (index == 0)
        {
            ClearRed(mOriginalRedDotNode, "acoriginal");
        }
        if (index == 1)
        {
            ClearRed(mDCRedDotNode, "dcac");
        }
    }

    private void OnSelectAccessorIcon(RoleIconData data)
    {
        roleData.acId = data.id;
        PlayLoadingAnime(data, true);
        rController.SetStyle(BundlePart.Accessoies, data.texName, () => PlayLoadingAnime(data, false), () => PlayLoadingAnime(data, false));
    }

    protected override void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        tree.AddRedDot(rootItem.gameObject, (int)ENodeType.Body, (int)ENodeType.accessories, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        foreach (var item in datas)
        {
            if (item.resourceKind.Equals("acoriginal"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[0].gameObject, (int)ENodeType.accessories, (int)ENodeType.acoriginal, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mOriginalRedDotNode = vNode;
            }
            if (item.resourceKind.Equals("dcac"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[1].gameObject, (int)ENodeType.accessories, (int)ENodeType.digitalCollect, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mDCRedDotNode = vNode;
            }
        }
    }
}


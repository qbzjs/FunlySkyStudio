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
/// Description:人物形象—鞋子UI显示
/// Date: 2022/4/1 16:48:5
/// </summary>
public class ShoesView : BaseView
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
        RoleMenuView.Ins.SetAction(InitShoesView);
    }
    public void InitShoesView()
    {
        this.bodyPart = BodyPartType.body;
        this.classifyType = ClassifyType.shoes;
        iconView[0].part = bodyPart;
        iconView[0].type = classifyType;
        NewUserUiSetting(toggleParent, Panels);
        InitToggle();
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetShoeStylesDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectShoesIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.shoeId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var shoeStyleData = RoleConfigDataManager.Inst.GetShoeStylesDataById(itemId);
        if (shoeStyleData != null)
        {
            shoeStyleData.rc = roleStyleItem;
            OnSelectShoesIcon(shoeStyleData);
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
        iconView[index].SetSelect(roleData.shoeId);
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
            ClearRed(mOriginalRedDotNode, "shoesoriginal");
        }
        if (index == 1)
        {
            ClearRed(mDCRedDotNode, "dcshoes");
        }
    }

    private void OnSelectShoesIcon(RoleIconData data)
    {
        roleData.shoeId = data.id;
        PlayLoadingAnime(data, true);
        rController.SetStyle(BundlePart.Shoe, data.texName, ()=>PlayLoadingAnime(data, false), () => PlayLoadingAnime(data, false));
    }

    protected override void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        tree.AddRedDot(rootItem.gameObject, (int)ENodeType.Body, (int)ENodeType.shoes, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        foreach (var item in datas)
        {
            if (item.resourceKind.Equals("shoesoriginal"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[0].gameObject, (int)ENodeType.shoes, (int)ENodeType.shoesoriginal, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mOriginalRedDotNode = vNode;
            }
            if (item.resourceKind.Equals("dcshoes"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[1].gameObject, (int)ENodeType.shoes, (int)ENodeType.digitalCollect, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mDCRedDotNode = vNode;
            }
        }
    }
}

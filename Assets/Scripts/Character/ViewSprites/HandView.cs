using AvatarRedDotSystem;
using Google.Protobuf.WellKnownTypes;
using RedDot;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: pzkunn
/// Description: 人物形象 — 手部挂饰分类
/// Date: 2022/8/9 20:31:46
/// </summary>
public class HandView : BaseView
{
    public RoleHandAdjustView adjustView;
    public RoleStyleView[] iconView;

    public Toggle[] subToggles;
    public GameObject[] panels;
    public GameObject[] newImage;
    public GameObject toggleParent;
    public VNode mDCRedDotNode;
    public VNode mOriginalRedDotNode;
    private int curSelectTogIndex = 0;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitHandView);
    }
    public void InitHandView()
    {
        this.bodyPart = BodyPartType.body;
        this.classifyType = ClassifyType.hand;
        BindHandAdjust();
        iconView[0].part = bodyPart;
        iconView[0].type = classifyType;
        NewUserUiSetting(toggleParent, panels);
        InitToggle();
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetHandStyleDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectHandIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.hdId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var handStyleData = RoleConfigDataManager.Inst.GetHandStyleDataById(itemId);
        if (handStyleData != null)
        {
            handStyleData.rc = roleStyleItem;
            OnSelectHandIcon(handStyleData);
        }
    }
    public void AdjustViewShowOrHideCallBack(bool isShow)
    {
        if (isShow)
        {
            rController.animCom.Play("interface_idle", 0, 0);
            rController.animCom.Update(0);
            rController.animCom.speed = 0;

            SetSwitchHandVisible();
        }
        else
        {
            rController.animCom.Play("interface_idle", 0, 0);
            rController.animCom.Update(0);
            rController.animCom.speed = 1;
        }
        
    }
    public override void OnSelect()
    {
        UpdateSelectState();
        SelectSub(curSelectTogIndex);
    }
    
    public override void UpdateSelectState()
    {
        int index = Array.FindIndex(subToggles, (tog) => tog.isOn);
        UpdateOnSelectSub(index);
    }

    private void InitToggle()
    {
        for (var i = 0; i < subToggles.Length; i++)
        {
            int index = i;
            subToggles[i].onValueChanged.AddListener((isOn) =>
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
        iconView[index].SetSelect(roleData.hdId);
    }

    private void SelectSub(int index)
    {
        for (var i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(false);
            subToggles[i].GetComponent<Text>().color = new Color32(151, 151, 151, 255);
        }
        panels[index].SetActive(true);
        subToggles[index].GetComponent<Text>().color = new Color32(0, 0, 0, 255);
        newImage[index].gameObject.SetActive(false);
        curSelectTogIndex = index;
        if (index == 0)
        {
            ClearRed(mOriginalRedDotNode, "handoriginal");
        }
        if (index == 1)
        {
            ClearRed(mDCRedDotNode, "dchand");
        }
    }

    private void BindHandAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Up_down);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Left_right);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Front_back);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.X_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Y_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Z_Rotation);
        adjustView.Init(mAdjustItemContexts, AdjustItemValueChanged,OnAdjustViewResetCallBack, AdjustViewShowOrHideCallBack);
        adjustView.SetDefalutValue();
        adjustView.SetOnSwitch(SwitchHand);
    }
    public void OnAdjustViewResetCallBack(int id)
    {
        HandStyleData data = RoleConfigDataManager.Inst.GetHandStyleDataById(id);
        SetAdjustView2Normal(adjustView,data);
    }
    public void SetAdjustView2RoleData(RoleAdjustView adjustView, HandStyleData data, RoleData roleData)
    {
        adjustView.SetSliderValue( EAdjustItemType.Size,
            GetSliderValue(data.scaleLimit, roleData.hdS));
        adjustView.SetSliderValue( EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, roleData.hdP, GetVecAxis(data.handBipType, EAdjustItemType.Up_down)));
        adjustView.SetSliderValue( EAdjustItemType.Left_right,
            GetSliderValue(data.hLimit, roleData.hdP, GetVecAxis(data.handBipType, EAdjustItemType.Left_right)));
        adjustView.SetSliderValue( EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, roleData.hdP, GetVecAxis(data.handBipType, EAdjustItemType.Front_back)));
        adjustView.SetSliderValue( EAdjustItemType.X_Rotation,
            GetSliderValue(data.xrotLimit, roleData.hdR, GetVecAxis(data.handBipType, EAdjustItemType.X_Rotation)));
        adjustView.SetSliderValue( EAdjustItemType.Y_Rotation,
            GetSliderValue(data.yrotLimit, roleData.hdR, GetVecAxis(data.handBipType, EAdjustItemType.Y_Rotation)));
        adjustView.SetSliderValue( EAdjustItemType.Z_Rotation,
            GetSliderValue(data.zrotLimit, roleData.hdR, GetVecAxis(data.handBipType, EAdjustItemType.Z_Rotation)));
    }
    private VecAxis GetVecAxis(int handType, EAdjustItemType itemtype)
    {
        if (handType == (int)HandBipType.Arm)
        {
            switch (itemtype)
            {
                case EAdjustItemType.Up_down:
                    return VecAxis.Z;
                case EAdjustItemType.Front_back:
                    return VecAxis.Y;
                case EAdjustItemType.Left_right:
                    return VecAxis.X;
                case EAdjustItemType.X_Rotation:
                    return VecAxis.X;
                case EAdjustItemType.Y_Rotation:
                    return VecAxis.Z;
                case EAdjustItemType.Z_Rotation:
                    return VecAxis.Y;
            }
        }
        if (handType == (int)HandBipType.Glove)
        {
            switch (itemtype)
            {
                case EAdjustItemType.Up_down:
                    return VecAxis.Z;
                case EAdjustItemType.Front_back:
                    return VecAxis.Y;
                case EAdjustItemType.Left_right:
                    return VecAxis.X;
                case EAdjustItemType.Z_Rotation:
                    return VecAxis.Y;
                case EAdjustItemType.Y_Rotation:
                    return VecAxis.Z;
                case EAdjustItemType.X_Rotation:
                    return VecAxis.X;
            }
        }
        return VecAxis.None;
    }
    public void SetAdjustView2Normal(RoleAdjustView adjustView, HandStyleData data)
    {
        adjustView.SetSliderValue(EAdjustItemType.Size, GetSliderValue(data.scaleLimit, data.sDef));
        adjustView.SetSliderValue(EAdjustItemType.Up_down, GetSliderValue(data.vLimit, data.pDef, GetVecAxis(data.handBipType, EAdjustItemType.Up_down)));
        adjustView.SetSliderValue(EAdjustItemType.Left_right, GetSliderValue(data.hLimit, data.pDef, GetVecAxis(data.handBipType, EAdjustItemType.Left_right)));
        adjustView.SetSliderValue(EAdjustItemType.Front_back, GetSliderValue(data.fLimit, data.pDef, GetVecAxis(data.handBipType, EAdjustItemType.Front_back)));
        adjustView.SetSliderValue(EAdjustItemType.X_Rotation, GetSliderValue(data.xrotLimit, data.rDef, GetVecAxis(data.handBipType, EAdjustItemType.X_Rotation)));
        adjustView.SetSliderValue(EAdjustItemType.Y_Rotation, GetSliderValue(data.yrotLimit, data.rDef, GetVecAxis(data.handBipType, EAdjustItemType.Y_Rotation)));
        adjustView.SetSliderValue(EAdjustItemType.Z_Rotation, GetSliderValue(data.zrotLimit, data.rDef, GetVecAxis(data.handBipType, EAdjustItemType.Z_Rotation)));
    }
    public void AdjustItemValueChanged(EAdjustItemType itemType, float value)
    {
        HandStyleData data = RoleConfigDataManager.Inst.GetHandStyleDataById(roleData.hdId);
        switch (itemType)
        {
            case EAdjustItemType.Size:
                roleData.hdS = GetValueBySlider(roleData.hdS, data.scaleLimit, value);
                rController.SetHandScale(roleData.hdS, data.handBipType);
                break;
            case EAdjustItemType.Up_down:
                roleData.hdP = GetValueBySlider(roleData.hdP, data.vLimit, value, GetVecAxis(data.handBipType, EAdjustItemType.Up_down));
                rController.SetHandPos(roleData.hdP, data.handBipType);
                break;
            case EAdjustItemType.Left_right:
                roleData.hdP = GetValueBySlider(roleData.hdP, data.hLimit, value, GetVecAxis(data.handBipType, EAdjustItemType.Left_right));
                rController.SetHandPos(roleData.hdP, data.handBipType);
                break;
            case EAdjustItemType.Front_back:
                roleData.hdP = GetValueBySlider(roleData.hdP, data.fLimit, value, GetVecAxis(data.handBipType, EAdjustItemType.Front_back));
                rController.SetHandPos(roleData.hdP, data.handBipType);
                break;
            case EAdjustItemType.X_Rotation:
                roleData.hdR = GetValueBySlider(roleData.hdR, data.xrotLimit, value, GetVecAxis(data.handBipType, EAdjustItemType.X_Rotation));
                rController.SetHandRot(roleData.hdR, data.handBipType);
                break;
            case EAdjustItemType.Y_Rotation:
                roleData.hdR = GetValueBySlider(roleData.hdR, data.yrotLimit, value, GetVecAxis(data.handBipType, EAdjustItemType.Y_Rotation));
                rController.SetHandRot(roleData.hdR, data.handBipType);
                break;
            case EAdjustItemType.Z_Rotation:
                roleData.hdR = GetValueBySlider(roleData.hdR, data.zrotLimit, value, GetVecAxis(data.handBipType, EAdjustItemType.Z_Rotation));
                rController.SetHandRot(roleData.hdR, data.handBipType);
                break;
            default:
                break;
        }
    }
    private void OnSelectHandIcon(RoleIconData obj)
    {
        HandStyleData data = obj as HandStyleData;
        adjustView.SetCurrentId(data.id);
        if (roleData.hdId == data.id)
        {
            SetAdjustView2RoleData(adjustView,data,roleData);
        }

        PlayLoadingAnime(data, true);
        rController.SetStyle(BundlePart.Hand, data.texName, ()=>PlayLoadingAnime(data, false), ()=> PlayLoadingAnime(data, false));
        if (roleData.hdId != data.id)
        {
            var handData = RoleConfigDataManager.Inst.GetHandStyleDataById(data.id);
            roleData.hdS = handData.sDef;
            roleData.hdR = handData.rDef;
            roleData.hdP = handData.pDef;
            roleData.hdId = data.id;
            SetAdjustView2Normal(adjustView,data);
            SetHandDefault(data.id);
        }
    }
    protected override void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        tree.AddRedDot(rootItem.gameObject, (int)ENodeType.Body, (int)ENodeType.hand, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        foreach (var item in datas)
        {
            if (item.resourceKind.Equals("handoriginal"))
            {
                VNode vNode = tree.AddRedDot(subToggles[0].gameObject, (int)ENodeType.hand, (int)ENodeType.handoriginal, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mOriginalRedDotNode = vNode;
            }
            if (item.resourceKind.Equals("dchand"))
            {
                VNode vNode = tree.AddRedDot(subToggles[1].gameObject, (int)ENodeType.hand, (int)ENodeType.digitalCollect, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mDCRedDotNode = vNode;
            }
        }
    }
    public void OnDisable()
    {
        if (rController!=null&&rController.animCom!=null)
        {
            rController.animCom.speed = 1;
        }
    }


    private void SwitchHand()
    {
        OnAdjustViewResetCallBack(roleData.hdId);
        SetOldVersionLeftRight();

        if(roleData.hdLR == (int)HandLRType.Left)
        {
            roleData.hdLR = (int)HandLRType.Right;
        }
        else if(roleData.hdLR == (int)HandLRType.Right)
        {
            roleData.hdLR = (int)HandLRType.Left;
        }
        rController.SwitchHand(roleData.hdId);
    }

    private void SetHandDefault(int handId)
    {
        HandStyleData data = RoleConfigDataManager.Inst.GetHandStyleDataById(roleData.hdId);
        roleData.hdLR = data.leftRightType;
        rController.SetHandLeftRight(handId, (HandLRType)data.leftRightType);       
    }

    private void SetSwitchHandVisible()
    {
        HandStyleData data = RoleConfigDataManager.Inst.GetHandStyleDataById(roleData.hdId);
        bool isSingleHand = data.leftRightType == (int)HandLRType.Left || data.leftRightType == (int)HandLRType.Right;
        adjustView.SetBtnActive(isSingleHand);
    }

    private void SetOldVersionLeftRight()
    {
        HandStyleData data = RoleConfigDataManager.Inst.GetHandStyleDataById(roleData.hdId);
        if (data == null) return;
        
        if(roleData.hdLR <= (int)HandLRType.Default)
        {
            roleData.hdLR = data.leftRightType;
        }
    }
}
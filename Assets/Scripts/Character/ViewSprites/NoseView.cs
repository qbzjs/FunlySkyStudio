using SavingData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象—鼻子UI显示
/// Date: 2022/4/1 16:50:17
/// </summary>
public class NoseView : BaseView
{
    private RoleAdjustView noseAdjustView;
    public RoleCustomStyleView iconView;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitNoseView);
    }
    public void InitNoseView()
    {
        this.bodyPart = BodyPartType.face;
        this.classifyType = ClassifyType.nose;
        noseAdjustView = transform.GetChild(1).GetComponent<RoleAdjustView>();
        BindNoseAdjust();
        iconView.part = bodyPart;
        iconView.type = classifyType;
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetNoseStyleDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectNoseIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.nId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var noseStyleData = RoleConfigDataManager.Inst.GetNoseStyleDataById(itemId);
        if (noseStyleData != null)
        {
            noseStyleData.rc = roleStyleItem;
            OnSelectNoseIcon(noseStyleData);
        }
    }

    public override void OnSelect()
    {
        UpdateSelectState();
    }

    public override void UpdateSelectState()
    {
        iconView.GetAllItemList(InitListAction);
        iconView.SetSelect(roleData.nId);
    }

    private void OnSelectNoseIcon(RoleIconData obj)
    {
        NoseStyleData data = obj as NoseStyleData;
        noseAdjustView.SetCurrentId(data.id);
        if (roleData.nId == data.id)
        {
            SetAdjustView2RoleData(noseAdjustView,data,roleData);
        }
        rController.SetNoseStyle(data.texName);
        if (roleData.nId != data.id)
        {
            var noseData = RoleConfigDataManager.Inst.GetNoseStyleDataById(data.id);
            roleData.nP = noseData.pDef;
            roleData.nCS = noseData.childSDef;
            roleData.nPS = noseData.parentSDef;
            roleData.nId = data.id;
           SetAdjustView2Normal(noseAdjustView,data);
        }
    }

    private void BindNoseAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Vertical);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.HorizontalStretch);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.VerticalStretch);
        noseAdjustView.Init(mAdjustItemContexts, AdjustItemValueChanged,OnAdjustViewResetCallBack);
        noseAdjustView.SetDefalutValue();
    }
    public void OnAdjustViewResetCallBack(int id)
    {
        NoseStyleData data = RoleConfigDataManager.Inst.GetNoseStyleDataById(id);
        SetAdjustView2Normal(noseAdjustView,data);
    }
    public void SetAdjustView2RoleData(RoleAdjustView adjustView, NoseStyleData data, RoleData roleData)
    {
        adjustView.SetSliderValue( EAdjustItemType.Size,
            GetSliderValue(data.parentScaleLimit, roleData.nPS));
        adjustView.SetSliderValue( EAdjustItemType.Vertical,
            GetSliderValue(data.vLimit, roleData.nP));
        adjustView.SetSliderValue( EAdjustItemType.HorizontalStretch,
            GetSliderValue(data.childHScaleLimit, roleData.nCS, GetVecAxis(EAdjustItemType.HorizontalStretch)));
        adjustView.SetSliderValue(EAdjustItemType.VerticalStretch,
            GetSliderValue(data.childVScaleLimit, roleData.nCS, GetVecAxis(EAdjustItemType.VerticalStretch)));
    }
    public void SetAdjustView2Normal(RoleAdjustView adjustView, NoseStyleData data)
    {
        noseAdjustView.SetSliderValue(EAdjustItemType.Size,
            GetSliderValue(data.parentScaleLimit, data.parentSDef));
        noseAdjustView.SetSliderValue(EAdjustItemType.Vertical,
            GetSliderValue(data.vLimit, data.pDef));
        noseAdjustView.SetSliderValue(EAdjustItemType.HorizontalStretch,
            GetSliderValue(data.childHScaleLimit, data.childSDef, GetVecAxis(EAdjustItemType.HorizontalStretch)));
        noseAdjustView.SetSliderValue(EAdjustItemType.VerticalStretch,
            GetSliderValue(data.childVScaleLimit, data.childSDef, GetVecAxis(EAdjustItemType.VerticalStretch)));
    }
    public void AdjustItemValueChanged(EAdjustItemType itemType, float value)
    {
        NoseStyleData noseData = null;
        switch (itemType)
        {
            case EAdjustItemType.Size:
                noseData = RoleConfigDataManager.Inst.GetNoseStyleDataById(roleData.nId);
                roleData.nPS = GetValueBySlider(roleData.nPS, noseData.parentScaleLimit, value);
                rController.SetNoseParentScale(roleData.nPS);
                break;
            case EAdjustItemType.Vertical:
                noseData = RoleConfigDataManager.Inst.GetNoseStyleDataById(roleData.nId);
                roleData.nP = GetValueBySlider(roleData.nP, noseData.vLimit, value);
                rController.SetNosePos(roleData.nP);
                break;
            case EAdjustItemType.HorizontalStretch:
                noseData = RoleConfigDataManager.Inst.GetNoseStyleDataById(roleData.nId);
                roleData.nCS = GetValueBySlider(roleData.nCS, noseData.childHScaleLimit, value, GetVecAxis(EAdjustItemType.HorizontalStretch));
                rController.SetNoseChildScale(roleData.nCS);
                break;
            case EAdjustItemType.VerticalStretch:
                noseData = RoleConfigDataManager.Inst.GetNoseStyleDataById(roleData.nId);
                roleData.nCS = GetValueBySlider(roleData.nCS, noseData.childVScaleLimit, value, GetVecAxis(EAdjustItemType.VerticalStretch));
                rController.SetNoseChildScale(roleData.nCS);
                break;
            default:
                break;
        }
    }
    public VecAxis GetVecAxis(EAdjustItemType itemType)
    {
        switch (itemType)
        {
            case EAdjustItemType.HorizontalStretch:
                return VecAxis.X;
            case EAdjustItemType.VerticalStretch:
                return VecAxis.Z;
            default:
                break;
        }
        return VecAxis.None;
    }
}

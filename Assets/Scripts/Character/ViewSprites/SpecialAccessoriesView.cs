using SavingData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:Avatar 特殊挂饰选择界面
/// Date: 2022/6/23 18:26:55
/// </summary>


public class SpecialAccessoriesView : BaseView
{
    private RoleAdjustView adjustView;
    public RoleCustomStyleView iconView;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitSpecialView);
    }

    public void InitSpecialView()
    {
        this.bodyPart = BodyPartType.body;
        this.classifyType = ClassifyType.special;
        adjustView = transform.GetChild(1).GetComponent<RoleAdjustView>();
        BindPatternAdjust();
        iconView.part = bodyPart;
        iconView.type = classifyType;
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetSpecialStylesDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectSpecialIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.saId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var specialStyleData = RoleConfigDataManager.Inst.GetSpecialStylesDataById(itemId);
        if (specialStyleData != null)
        {
            specialStyleData.rc = roleStyleItem;
            OnSelectSpecialIcon(specialStyleData);
        }
    }

    public override void OnSelect()
    {
        UpdateSelectState();
    }

    public override void UpdateSelectState()
    {
        iconView.GetAllItemList(InitListAction);
        iconView.SetSelect(roleData.saId);
    }

    private void OnSelectSpecialIcon(RoleIconData obj)
    {
        SpecialStyleData data = obj as SpecialStyleData;
        adjustView.SetCurrentId(data.id);
        if (roleData.saId == data.id)
        {
            SetAdjustView2RoleData(adjustView,data,roleData);
        }
        var gView = data.rc;
        PlayLoadingAnime(data, true);
        rController.SetStyle(BundlePart.Special, data.texName, ()=>PlayLoadingAnime(data, false), ()=>PlayLoadingAnime(data, false));
        if (roleData.saId != data.id)
        {
            var pData = RoleConfigDataManager.Inst.GetSpecialStylesDataById(data.id);
            roleData.saP = pData.pDef;
            roleData.saS = pData.sDef;
            roleData.saR = pData.rDef;
            roleData.saId = data.id;
            SetAdjustView2Normal(adjustView,data);
        }
    }

    private void BindPatternAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Up_down);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Left_right);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Front_back);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.X_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Y_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Z_Rotation);
        adjustView.Init(mAdjustItemContexts, AdjustItemValueChanged,OnAdjustViewResetCallBack);
        adjustView.SetDefalutValue();
    }
    public void OnAdjustViewResetCallBack(int id)
    {
        SpecialStyleData data = RoleConfigDataManager.Inst.GetSpecialStylesDataById(id);
        SetAdjustView2Normal(adjustView,data);
    }
    public void SetAdjustView2RoleData(RoleAdjustView adjustView, SpecialStyleData data, RoleData roleData)
    {
        adjustView.SetSliderValue( EAdjustItemType.Size,
            GetSliderValue(data.scaleLimit, roleData.saS,GetVecAxis(EAdjustItemType.Size)));
        adjustView.SetSliderValue( EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, roleData.saP, GetVecAxis(EAdjustItemType.Up_down)));
        adjustView.SetSliderValue( EAdjustItemType.Left_right,
            GetSliderValue(data.hLimit, roleData.saP, GetVecAxis(EAdjustItemType.Left_right)));
        adjustView.SetSliderValue( EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, roleData.saP, GetVecAxis(EAdjustItemType.Front_back)));
        adjustView.SetSliderValue( EAdjustItemType.X_Rotation,
            GetSliderValue(data.xrotLimit, roleData.saR, GetVecAxis(EAdjustItemType.X_Rotation)));
        adjustView.SetSliderValue( EAdjustItemType.Y_Rotation,
            GetSliderValue(data.zrotLimit, roleData.saR, GetVecAxis(EAdjustItemType.Y_Rotation)));
        adjustView.SetSliderValue( EAdjustItemType.Z_Rotation,
            GetSliderValue(data.yrotLimit, roleData.saR, GetVecAxis(EAdjustItemType.Z_Rotation)));

    }
    public void SetAdjustView2Normal(RoleAdjustView adjustView, SpecialStyleData data)
    {
        adjustView.SetSliderValue(EAdjustItemType.Size,
            GetSliderValue(data.scaleLimit, data.sDef, GetVecAxis(EAdjustItemType.Size)));
        adjustView.SetSliderValue(EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, data.pDef, GetVecAxis(EAdjustItemType.Up_down)));
        adjustView.SetSliderValue(EAdjustItemType.Left_right,
            GetSliderValue(data.hLimit, data.pDef, GetVecAxis(EAdjustItemType.Left_right)));
        adjustView.SetSliderValue(EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, data.pDef, GetVecAxis(EAdjustItemType.Front_back)));
        adjustView.SetSliderValue(EAdjustItemType.X_Rotation,
            GetSliderValue(data.xrotLimit, data.rDef, GetVecAxis(EAdjustItemType.X_Rotation)));
        adjustView.SetSliderValue(EAdjustItemType.Y_Rotation,
            GetSliderValue(data.zrotLimit, data.rDef, GetVecAxis(EAdjustItemType.Y_Rotation)));
        adjustView.SetSliderValue(EAdjustItemType.Z_Rotation,
            GetSliderValue(data.yrotLimit, data.rDef, GetVecAxis(EAdjustItemType.Z_Rotation)));
    }
    public void AdjustItemValueChanged(EAdjustItemType itemType, float value)
    {
        SpecialStyleData data = RoleConfigDataManager.Inst.GetSpecialStylesDataById(roleData.saId);
        switch (itemType)
        {
            case EAdjustItemType.Size:
                roleData.saS = GetValueBySlider(roleData.saS, data.scaleLimit, value);
                rController.SetSpecialScale(roleData.saS);
                break;
            case EAdjustItemType.Up_down:
                roleData.saP = GetValueBySlider(roleData.saP, data.vLimit, value, VecAxis.X);
                rController.SetSpecialPos(roleData.saP);
                break;
            case EAdjustItemType.Left_right:
                roleData.saP = GetValueBySlider(roleData.saP, data.hLimit, value, VecAxis.Z);
                rController.SetSpecialPos(roleData.saP);
                break;
            case EAdjustItemType.Front_back:
                roleData.saP = GetValueBySlider(roleData.saP, data.fLimit, value, VecAxis.Y);
                rController.SetSpecialPos(roleData.saP);
                break;
            case EAdjustItemType.X_Rotation:
                roleData.saR = GetValueBySlider(roleData.saR, data.xrotLimit, value, VecAxis.X);
                rController.SetSpecialRot(roleData.saR);
                break;
            case EAdjustItemType.Y_Rotation:
                roleData.saR = GetValueBySlider(roleData.saR, data.zrotLimit, value, VecAxis.Z);
                rController.SetSpecialRot(roleData.saR);
                break;
            case EAdjustItemType.Z_Rotation:
                roleData.saR = GetValueBySlider(roleData.saR, data.yrotLimit, value, VecAxis.Y);
                rController.SetSpecialRot(roleData.saR);
                break;
            default:
                break;
        }
    }
    public VecAxis GetVecAxis(EAdjustItemType itemType)
    {
        switch (itemType)
        {
            case EAdjustItemType.Up_down:
                return VecAxis.X;
            case EAdjustItemType.Front_back:
                return VecAxis.Y;
            case EAdjustItemType.Left_right:
                return VecAxis.Z;
            case EAdjustItemType.X_Rotation:
                return VecAxis.X;
            case EAdjustItemType.Y_Rotation:
                return VecAxis.Z;
            case EAdjustItemType.Z_Rotation:
                return VecAxis.Y;
            default:
                break;
        }
        return VecAxis.None;
    }
}

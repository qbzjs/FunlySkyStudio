using SavingData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象—嘴巴UI显示
/// Date: 2022/4/1 16:50:32
/// </summary>
public class MouthView : BaseView
{
    private RoleAdjustView mouseAdjuestView;
    public RoleCustomStyleView iconView;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitMouthView);
    }
    public void InitMouthView()
    {
        this.bodyPart = BodyPartType.face;
        this.classifyType = ClassifyType.mouth;
        mouseAdjuestView = transform.GetChild(1).GetComponent<RoleAdjustView>();
        BindMouseAdjust();
        iconView.part = bodyPart;
        iconView.type = classifyType;
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetMouseStyleDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectMouseIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.mId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var mouseStyleData = RoleConfigDataManager.Inst.GetMouseStyleDataById(itemId);
        if (mouseStyleData != null)
        {
            mouseStyleData.rc = roleStyleItem;
            OnSelectMouseIcon(mouseStyleData);
        }
    }

    public override void OnSelect()
    {
        UpdateSelectState();
    }

    public override void UpdateSelectState()
    {
        iconView.GetAllItemList(InitListAction);
        iconView.SetSelect(roleData.mId);
    }
    private void BindMouseAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Up_down);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Left_right);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Front_back);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Rotation);
        mouseAdjuestView.Init(mAdjustItemContexts, AdjustItemValueChanged,OnAdjustViewResetCallBack);
        mouseAdjuestView.SetDefalutValue();
    }
    public void OnAdjustViewResetCallBack(int id)
    {
        MouseStyleData data = RoleConfigDataManager.Inst.GetMouseStyleDataById(id);
        SetAdjustView2Normal(mouseAdjuestView,data);
    }

    public void SetAdjustView2RoleData(RoleAdjustView adjustView, MouseStyleData data, RoleData roleData)
    {
        adjustView.SetSliderValue( EAdjustItemType.Size,
            GetSliderValue(data.scaLimit, roleData.mS, GetVecAxis(EAdjustItemType.Size)));
        adjustView.SetSliderValue( EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, roleData.mP, GetVecAxis(EAdjustItemType.Up_down)));
        adjustView.SetSliderValue( EAdjustItemType.Left_right,
            GetSliderValue(data.hLimit, roleData.mP, GetVecAxis(EAdjustItemType.Left_right)));
        adjustView.SetSliderValue( EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, roleData.mP, GetVecAxis(EAdjustItemType.Front_back)));
        adjustView.SetSliderValue( EAdjustItemType.Rotation,
            GetSliderValue(data.rotLimit, roleData.mR, GetVecAxis(EAdjustItemType.Rotation)));
    }
    public void SetAdjustView2Normal(RoleAdjustView adjustView, MouseStyleData data)
    {
        adjustView.SetSliderValue(EAdjustItemType.Size,
            GetSliderValue(data.scaLimit, data.sDef, GetVecAxis(EAdjustItemType.Size)));
        adjustView.SetSliderValue(EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, data.pDef, GetVecAxis(EAdjustItemType.Up_down)));
        adjustView.SetSliderValue(EAdjustItemType.Left_right,
            GetSliderValue(data.hLimit, data.pDef, GetVecAxis(EAdjustItemType.Left_right)));
        adjustView.SetSliderValue(EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, data.pDef, GetVecAxis(EAdjustItemType.Front_back)));
        adjustView.SetSliderValue(EAdjustItemType.Rotation,
            GetSliderValue(data.rotLimit, data.rDef, GetVecAxis(EAdjustItemType.Rotation)));
    }
    public void AdjustItemValueChanged(EAdjustItemType itemType, float value)
    {
        MouseStyleData data = RoleConfigDataManager.Inst.GetMouseStyleDataById(roleData.mId);
        switch (itemType)
        {
            case EAdjustItemType.Size:
                roleData.mS = GetValueBySlider(roleData.mS, data.scaLimit, value);
                rController.SetMouthSca(roleData.mS);
                break;
            case EAdjustItemType.Up_down:
                roleData.mP = GetValueBySlider(roleData.mP, data.vLimit, value, VecAxis.X);
                rController.SetMouthPos(roleData.mP);
                break;
            case EAdjustItemType.Left_right:
                roleData.mP = GetValueBySlider(roleData.mP, data.hLimit, value, VecAxis.Z);
                rController.SetMouthPos(roleData.mP);
                break;
            case EAdjustItemType.Front_back:
                roleData.mP = GetValueBySlider(roleData.mP, data.fLimit, value, VecAxis.Y);
                rController.SetMouthPos(roleData.mP);
                break;
            case EAdjustItemType.Rotation:
                roleData.mR = GetValueBySlider(roleData.mR, data.rotLimit, value);
                rController.SetMouthRot(roleData.mR);
                break;
            default:
                break;
        }
    }
    private void OnSelectMouseIcon(RoleIconData obj)
    {
        MouseStyleData data = obj as MouseStyleData;
        mouseAdjuestView.SetCurrentId(data.id);
        if (roleData.mId == data.id)
        {
            SetAdjustView2RoleData(mouseAdjuestView,data,roleData);
        }
        rController.SetMouthStyle(data.texName);
        if (roleData.mId != data.id)
        {
            var mouseData = RoleConfigDataManager.Inst.GetMouseStyleDataById(data.id);
            roleData.mP = mouseData.pDef;
            roleData.mR = mouseData.rDef;
            roleData.mS = mouseData.sDef;
            roleData.mId = data.id;
           
            SetAdjustView2Normal(mouseAdjuestView,data);
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
            default:
                break;
        }
        return VecAxis.None;
    }
}

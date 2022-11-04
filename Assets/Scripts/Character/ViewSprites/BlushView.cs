using SavingData;
using System;
using System.Collections.Generic;

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象—腮红UI显示
/// Date: 2022/4/1 16:50:44
/// </summary>
public class BlushView : BaseView
{
    private RoleAdjustView BlushAdjuestView;
    public RoleCustomStyleView iconView;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitBlushStyleView);
    }
    public void InitBlushStyleView()
    {
        this.bodyPart = BodyPartType.face;
        this.classifyType = ClassifyType.blush;
        BlushAdjuestView = transform.GetChild(1).GetComponent<RoleAdjustView>();
        BindBlushAdjust();
        var colorView = GetComponentInChildren<RoleColorView>();
        colorView.Init(roleColorConfigData.faceStyleColors.commonColors, OnSelectFaceStyleColor);

        iconView.part = bodyPart;
        iconView.type = classifyType;
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetBlusherStyleDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectFaceStyleIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.bluId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    private void BindBlushAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts,EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts,EAdjustItemType.Spacing);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Up_down);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Front_back);
        BlushAdjuestView.Init(mAdjustItemContexts, AdjustItemValueChanged,OnAdjustViewResetCallBack);
        BlushAdjuestView.SetDefalutValue();
    }
    public void AdjustItemValueChanged(EAdjustItemType itemType, float value)
    {
        BlushStyleData data = RoleConfigDataManager.Inst.GetBlusherStyleDataById(roleData.bluId);
        switch (itemType)
        {
            case EAdjustItemType.Size:
                roleData.bluS = GetValueBySlider(roleData.bluS, data.scaLimit, value);
                rController.SetBlushSca(roleData.bluS);
                break;
            case EAdjustItemType.Up_down:
                roleData.bluP = GetValueBySlider(roleData.bluP, data.vLimit, value, GetVecAxis(EAdjustItemType.Up_down));
                rController.SetBlushPos(roleData.bluP);
                break;
            case EAdjustItemType.Front_back:
                roleData.bluP = GetValueBySlider(roleData.bluP, data.fLimit, value, GetVecAxis(EAdjustItemType.Front_back));
                rController.SetBlushPos(roleData.bluP);
                break;
            case EAdjustItemType.Spacing:
                roleData.bluP = GetValueBySlider(roleData.bluP, data.hLimit, value, GetVecAxis(EAdjustItemType.Spacing));
                rController.SetBlushPos(roleData.bluP);
                break;
            default:
                break;
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var faceStyleData = RoleConfigDataManager.Inst.GetBlusherStyleDataById(itemId);
        if (faceStyleData != null)
        {
            faceStyleData.rc = roleStyleItem;
            OnSelectFaceStyleIcon(faceStyleData);
        }
    }

    public override void OnSelect()
    {
        UpdateSelectState();
    }

    public override void UpdateSelectState()
    {
        iconView.GetAllItemList(InitListAction);
        iconView.SetSelect(roleData.bluId);
        var colorView = GetComponentInChildren<RoleColorView>(true);
        colorView.SetSelect(roleData.bluCr);
    }

    private void OnSelectFaceStyleColor(string colorData)
    {
        roleData.bluCr = colorData;
        rController.SetBlusherColor(colorData);
    }

    private void OnSelectFaceStyleIcon(RoleIconData data)
    {
        BlushStyleData bludata = data as BlushStyleData;
        BlushAdjuestView.SetCurrentId(bludata.id);
        if (roleData.bluId == bludata.id)
        {
            SetAdjustView2RoleData(BlushAdjuestView,bludata,roleData);
        }
        rController.SetBlusherStyle(bludata.texName);
        if (roleData.bluId != bludata.id)
        {
            var blushData = RoleConfigDataManager.Inst.GetBlusherStyleDataById(data.id);
            roleData.bluP = blushData.pDef;
            roleData.bluS = blushData.sDef;
            roleData.bluId = bludata.id;
            SetAdjustView2Normal(BlushAdjuestView,blushData);
        }
    }
    public void OnAdjustViewResetCallBack(int id)
    {
        BlushStyleData data = RoleConfigDataManager.Inst.GetBlusherStyleDataById(id);
        SetAdjustView2Normal(BlushAdjuestView,data);
    }
    public void SetAdjustView2Normal(RoleAdjustView blushAdjuestView, BlushStyleData data)
    {
        blushAdjuestView.SetSliderValue(EAdjustItemType.Size, 
            GetSliderValue(data.scaLimit, data.sDef, GetVecAxis(EAdjustItemType.Size)));
        blushAdjuestView.SetSliderValue(EAdjustItemType.Spacing, 
            GetSliderValue(data.hLimit, data.pDef, GetVecAxis(EAdjustItemType.Spacing)));
        blushAdjuestView.SetSliderValue(EAdjustItemType.Up_down, 
            GetSliderValue(data.vLimit, data.pDef, GetVecAxis(EAdjustItemType.Up_down)));
        blushAdjuestView.SetSliderValue(EAdjustItemType.Front_back, 
            GetSliderValue(data.fLimit, data.pDef, GetVecAxis(EAdjustItemType.Front_back)));
    }

    public void SetAdjustView2RoleData(RoleAdjustView blushAdjuestView, BlushStyleData data, RoleData roleData)
    {
        blushAdjuestView.SetSliderValue(EAdjustItemType.Size, 
            GetSliderValue(data.scaLimit, roleData.bluS));
        blushAdjuestView.SetSliderValue(EAdjustItemType.Spacing,
            GetSliderValue(data.hLimit, roleData.bluP, GetVecAxis(EAdjustItemType.Spacing)));
        blushAdjuestView.SetSliderValue(EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, roleData.bluP, GetVecAxis(EAdjustItemType.Up_down)));
        blushAdjuestView.SetSliderValue(EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, roleData.bluP, GetVecAxis(EAdjustItemType.Front_back)));
    }
    public VecAxis GetVecAxis(EAdjustItemType itemType)
    {
        switch (itemType)
        {
            case EAdjustItemType.Up_down:
                return VecAxis.X;
            case EAdjustItemType.Front_back:
                return VecAxis.Y;
            case EAdjustItemType.Spacing:
                return VecAxis.Z;
            default:
                break;
        }
        return VecAxis.None;
    }
}

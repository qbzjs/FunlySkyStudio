using SavingData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象—眉毛UI显示
/// Date: 2022/4/1 16:49:7
/// </summary>
public class BrowsView : BaseView
{
    private RoleAdjustView browAdjuestView;
    public RoleCustomStyleView iconView;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitBrowView);
    }
    public void InitBrowView()
    {
        this.bodyPart = BodyPartType.face;
        this.classifyType = ClassifyType.brows;
        browAdjuestView = transform.GetChild(1).GetComponent<RoleAdjustView>();
        BindBrowAdjust();
        var colorView = GetComponentInChildren<RoleColorView>();
        colorView.Init(roleColorConfigData.browColors.commonColors, OnSelectBrowColor);
         var paletteColorView=this.GetComponentInChildren<PaletteColorView>(true);
        paletteColorView.InitPaletteView(roleColorConfigData.browColors.allColors,OnSelectBrowColor);
        var hsvColorView = this.GetComponentInChildren<HsvColorView>(true);
        hsvColorView.InitHsvView(OnSelectBrowColor);
        iconView.part = bodyPart;
        iconView.type = classifyType;
        hsvColorView.SetGetCurrentTarget(() => roleData.bCr);
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetBrowStyleDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectBrowIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.bId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var browStyleData = RoleConfigDataManager.Inst.GetBrowStyleDataById(itemId);
        if (browStyleData != null)
        {
            browStyleData.rc = roleStyleItem;
            OnSelectBrowIcon(browStyleData);
        }
    }

    public override void OnSelect()
    {
        UpdateSelectState();
    }

    public override void UpdateSelectState()
    {
        var colorView = GetComponentInChildren<RoleColorView>(true);
        var paletteview=this.GetComponentInChildren<PaletteColorView>(true);
        var hsvView = GetComponentInChildren<HsvColorView>(true);
        iconView.GetAllItemList(InitListAction);
        iconView.SetSelect(roleData.bId);
        colorView.SetSelect(roleData.bCr);
        paletteview.SetSelect(roleData.bCr);
        paletteview.gameObject.SetActive(false);
        hsvView.gameObject.SetActive(false);
    }

    private void BindBrowAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Spacing);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Up_down);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Front_back);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Rotation);
        browAdjuestView.Init(mAdjustItemContexts, AdjustItemValueChanged,OnAdjustViewResetCallBack);
    }
    public void AdjustItemValueChanged(EAdjustItemType itemType, float value)
    {
        BrowStyleData data = RoleConfigDataManager.Inst.GetBrowStyleDataById(roleData.bId);
        switch (itemType)
        {
            case EAdjustItemType.Size:
                roleData.bS = GetValueBySlider(roleData.bS, data.scaLimit, value);
                rController.SetBrowSca(roleData.bS);
                break;
            case EAdjustItemType.Spacing:
                roleData.bP = GetValueBySlider(roleData.bP, data.hLimit, value,GetVecAxis(EAdjustItemType.Spacing));
                rController.SetBrowPos(roleData.bP);
                break;
            case EAdjustItemType.Up_down:
                roleData.bP = GetValueBySlider(roleData.bP, data.vLimit, value, GetVecAxis(EAdjustItemType.Up_down));
                rController.SetBrowPos(roleData.bP);
                break;
            case EAdjustItemType.Front_back:
                roleData.bP = GetValueBySlider(roleData.bP, data.fLimit, value, GetVecAxis(EAdjustItemType.Front_back));
                rController.SetBrowPos(roleData.bP);
                break;
            case EAdjustItemType.Rotation:
                roleData.bR = GetValueBySlider(roleData.bR, data.rotateLimit, value);
                rController.SetBrowRot(roleData.bR);
                break;
            default:
                break;
        }
    }
    private void OnSelectBrowColor(string colorData)
    {
        roleData.bCr = colorData;
        rController.SetBrowColor(colorData);
    }

    private void OnSelectBrowIcon(RoleIconData obj)
    {
        BrowStyleData data = obj as BrowStyleData;
        browAdjuestView.SetCurrentId(data.id);
        if (roleData.bId == data.id)
        {
            SetAdjustView2RoleData(browAdjuestView,data,roleData);
        }

        rController.SetBrowStyle(data.texName);
        if (roleData.bId != data.id)
        {
            var browData = RoleConfigDataManager.Inst.GetBrowStyleDataById(data.id);
            roleData.bP = browData.pDef;
            roleData.bR = browData.rDef;
            roleData.bS = browData.sDef;
            roleData.bId = data.id;
            SetAdjustView2Normal(browAdjuestView,data);
        }
    }
    public void SetAdjustView2Normal(RoleAdjustView adjustView, BrowStyleData data)
    {
        adjustView.SetSliderValue(EAdjustItemType.Size, 
            GetSliderValue(data.scaLimit, data.sDef));
        adjustView.SetSliderValue(EAdjustItemType.Spacing, 
            GetSliderValue(data.hLimit, data.pDef,GetVecAxis(EAdjustItemType.Spacing)));
        adjustView.SetSliderValue(EAdjustItemType.Up_down, 
            GetSliderValue(data.vLimit, data.pDef, GetVecAxis(EAdjustItemType.Up_down)));
        adjustView.SetSliderValue(EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, data.pDef, GetVecAxis(EAdjustItemType.Front_back)));
        adjustView.SetSliderValue(EAdjustItemType.Rotation,
            GetSliderValue(data.rotateLimit, data.rDef, GetVecAxis(EAdjustItemType.Rotation)));
    }

    public void SetAdjustView2RoleData(RoleAdjustView adjustView, BrowStyleData data, RoleData roleData)
    {
        adjustView.SetSliderValue( EAdjustItemType.Size,
            GetSliderValue(data.scaLimit, roleData.bS));
        adjustView.SetSliderValue( EAdjustItemType.Spacing,
            GetSliderValue(data.hLimit, roleData.bP, GetVecAxis(EAdjustItemType.Spacing)));
        adjustView.SetSliderValue( EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, roleData.bP, GetVecAxis(EAdjustItemType.Up_down)));
        adjustView.SetSliderValue( EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, roleData.bP, GetVecAxis(EAdjustItemType.Front_back)));
        adjustView.SetSliderValue( EAdjustItemType.Rotation,
            GetSliderValue(data.rotateLimit, roleData.bR, GetVecAxis(EAdjustItemType.Rotation)));
    }
    public void OnAdjustViewResetCallBack(int id)
    {
        BrowStyleData data = RoleConfigDataManager.Inst.GetBrowStyleDataById(id);
        SetAdjustView2Normal(browAdjuestView,data);
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

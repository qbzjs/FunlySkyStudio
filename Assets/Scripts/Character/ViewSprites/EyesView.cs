using AvatarRedDotSystem;
using RedDot;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象—眼睛UI显示
/// Date: 2022/4/1 16:48:45
/// </summary>
public class EyesView : BaseView
{
    public RoleColorAdjustView eyeAdjuestView;
    private const string defaultEyeColor = "#f4f4f4";
    public ClassifyTogItem classifyTogItem;
    public RoleStyleView[] iconView;
    [HideInInspector]
    public VNode mOriginalRedDotNode;
    [HideInInspector]
    public VNode mDCRedDotNode;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitEyesView);
    }
    public void InitEyesView()
    {
        this.bodyPart = BodyPartType.face;
        this.classifyType = ClassifyType.eyes;
        //Tab分类初始
        classifyTogItem.NewUserUiSetting();
        classifyTogItem.SetSelectAction(UpdateOnSelectSub, OnClearRed);
        BindEyeAdjust();
        InitViewType();

        var colorView = this.GetComponentInChildren<RoleColorView>(true);
        colorView.Init(roleColorConfigData.eyeColors.commonColors, OnSelectEyeColor);
        var paletteColorView = this.GetComponentInChildren<PaletteColorView>(true);
        paletteColorView.InitPaletteView(roleColorConfigData.eyeColors.allColors, OnSelectEyeColor);
        var hsvColorView = this.GetComponentInChildren<HsvColorView>(true);
        hsvColorView.InitHsvView(OnSelectEyeColor);
        hsvColorView.SetGetCurrentTarget(() => roleData.eCr);
    }

    private void InitViewType()
    {
        for (int i = 0; i < iconView.Length; i++)
        {
            iconView[i].type = classifyType;
            iconView[i].part = bodyPart;
        }
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetEyeStyleDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectEyeIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.eId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    private void OnSelectEyeColor(string colorData)
    {
        var eyeData = RoleConfigDataManager.Inst.GetEyeStyleDataById(roleData.eId);

        //更换瞳孔颜色
        rController.SetEyePupilColor(eyeData.texName, colorData);
        roleData.eCr = colorData;
    }

    /// <summary>
    /// 恢复默认瞳孔颜色
    /// </summary>
    private void ResetDefaultEyeColor()
    {
        rController.SetEyePupilColor("eye_18", defaultEyeColor);
        roleData.eCr = defaultEyeColor;
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var eyeStyleData = RoleConfigDataManager.Inst.GetEyeStyleDataById(itemId);
        if (eyeStyleData != null)
        {
            eyeStyleData.rc = roleStyleItem;
            OnSelectEyeIcon(eyeStyleData);
        }
    }

    public override void OnSelect()
    {
        classifyTogItem.UpdateSelectTab();//同步选中三级Tab
        UpdateSelectState();
    }

    public override void UpdateSelectState()
    {
        var colorView = this.GetComponentInChildren<RoleColorView>(true);
        var paletteColorView = this.GetComponentInChildren<PaletteColorView>(true);
        int index = Array.FindIndex(classifyTogItem.SubToggles, (tog) => tog.isOn);
        UpdateOnSelectSub(index);
        colorView.SetSelect(roleData.eCr);
        paletteColorView.SetSelect(roleData.eCr);
        var eyeData = RoleConfigDataManager.Inst.GetEyeStyleDataById(roleData.eId);
        rController.SetEyePupilColor(eyeData.texName, roleData.eCr);
    }
    
    private void UpdateOnSelectSub(int index)
    {
        iconView[index].GetAllItemList(InitListAction);
        iconView[index].curItem = null;
        iconView[index].SetSelect(roleData.hdId);
    }

    protected override void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        tree.AddRedDot(rootItem.gameObject, (int)ENodeType.eyes, (int)ENodeType.eyes, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        foreach (var item in datas)
        {
            switch (item.resourceKind)
            {
                case "eyeoriginal":
                    mOriginalRedDotNode = tree.AddRedDot(classifyTogItem.SubToggles[0].gameObject, (int)ENodeType.eyes, (int)ENodeType.eyesoriginal, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                    mOriginalRedDotNode.mLogic.ChangeCount(1);
                    break;
                case "dceye":
                    mDCRedDotNode = tree.AddRedDot(classifyTogItem.SubToggles[1].gameObject, (int)ENodeType.eyes, (int)ENodeType.dceyes, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                    mDCRedDotNode.mLogic.ChangeCount(1);
                    break;
            }
        }

    }

    public void OnClearRed(int index)
    {
        if (index == 0)
        {
            ClearRed(mOriginalRedDotNode, "eyeoriginal");
        }
        if (index == 2)
        {
            ClearRed(mDCRedDotNode, "dceye");
        }
    }

    private void BindEyeAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Spacing);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Up_down);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Front_back);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Rotation);
        eyeAdjuestView.Init(mAdjustItemContexts,AdjustItemValueChanged,OnAdjustViewResetCallBack);
        eyeAdjuestView.SetDefalutValue(roleColorConfigData.eyeColors.commonColors[1]);
    }
    public void OnAdjustViewResetCallBack(int id)
    {
        EyeStyleData data = RoleConfigDataManager.Inst.GetEyeStyleDataById(id);
        SetAdjustView2Normal(eyeAdjuestView,data);
    }
    public void SetAdjustView2Normal(RoleAdjustView adjustView, EyeStyleData data)
    {
        adjustView.SetSliderValue(EAdjustItemType.Size,
            GetSliderValue(data.scaleLimit, data.sDef, GetVecAxis(EAdjustItemType.Size)));
        adjustView.SetSliderValue(EAdjustItemType.Spacing,
            GetSliderValue(data.hLimit, data.pDef, GetVecAxis(EAdjustItemType.Spacing)));
        adjustView.SetSliderValue(EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, data.pDef, GetVecAxis(EAdjustItemType.Up_down)));
        adjustView.SetSliderValue(EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, data.pDef, GetVecAxis(EAdjustItemType.Front_back)));
        adjustView.SetSliderValue(EAdjustItemType.Rotation,
            GetSliderValue(data.rotateLimit, data.rDef, GetVecAxis(EAdjustItemType.Rotation)));
    }

    public void SetAdjustView2RoleData(RoleAdjustView adjustView, EyeStyleData data, RoleData roleData)
    {
        adjustView.SetSliderValue( EAdjustItemType.Size,
            GetSliderValue(data.scaleLimit, roleData.eS, GetVecAxis(EAdjustItemType.Size)));
        adjustView.SetSliderValue( EAdjustItemType.Spacing,
            GetSliderValue(data.hLimit, roleData.eP, GetVecAxis(EAdjustItemType.Spacing)));
        adjustView.SetSliderValue( EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, roleData.eP, GetVecAxis(EAdjustItemType.Up_down)));
        adjustView.SetSliderValue( EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, roleData.eP, GetVecAxis(EAdjustItemType.Front_back)));
        adjustView.SetSliderValue( EAdjustItemType.Rotation,
            GetSliderValue(data.rotateLimit, roleData.eR, GetVecAxis(EAdjustItemType.Rotation)));
    }
    public VecAxis GetVecAxis(EAdjustItemType itemType)
    {
        switch (itemType)
        {
            case EAdjustItemType.Spacing:
                return VecAxis.Z;
            case EAdjustItemType.Front_back:
                return VecAxis.Y;
            case EAdjustItemType.Up_down:
                return VecAxis.X;
            default:
                break;
        }
        return VecAxis.None;
    }
    public void AdjustItemValueChanged(EAdjustItemType itemType, float value)
    {
        EyeStyleData data = RoleConfigDataManager.Inst.GetEyeStyleDataById(roleData.eId);
        switch (itemType)
        {
            case EAdjustItemType.Size:
                roleData.eS = GetValueBySlider(roleData.eS, data.scaleLimit, value);
                rController.SetEyesScale(roleData.eS);
                break;
            case EAdjustItemType.Spacing:
                roleData.eP = GetValueBySlider(roleData.eP, data.hLimit, value, GetVecAxis(EAdjustItemType.Spacing));
                rController.SetEyesPos(roleData.eP);
                break;
            case EAdjustItemType.Up_down:
                roleData.eP = GetValueBySlider(roleData.eP, data.vLimit, value, GetVecAxis(EAdjustItemType.Up_down));
                rController.SetEyesPos(roleData.eP);
                break;
            case EAdjustItemType.Front_back:
                roleData.eP = GetValueBySlider(roleData.eP, data.fLimit, value, GetVecAxis(EAdjustItemType.Front_back));
                rController.SetEyesPos(roleData.eP);
                break;
            case EAdjustItemType.Rotation:
                roleData.eR = GetValueBySlider(roleData.eR, data.rotateLimit, value);
                rController.SetEyesRot(roleData.eR);
                break;
            default:
                break;
        }
    }

    private void OnSelectEyeIcon(RoleIconData obj)
    {
        EyeStyleData data = obj as EyeStyleData;
        eyeAdjuestView.SetCurrentId(data.id);
        if (roleData.eId == data.id)
        {
            SetAdjustView2RoleData(eyeAdjuestView,data,roleData);
        }
        rController.StartEyeAnimation(data.id);
        rController.SetEyesStyle(data.texName);
        rController.SetSpecialEyesStyle(data.texName);
        rController.SetEyePupilColor(data.texName, roleData.eCr);
        if (roleData.eId != data.id)
        {
            var eyeData = RoleConfigDataManager.Inst.GetEyeStyleDataById(data.id);
            roleData.eP = eyeData.pDef;
            roleData.eS = eyeData.sDef;
            roleData.eR = eyeData.rDef;
            roleData.eId = data.id;
            SetAdjustView2Normal(eyeAdjuestView,data);
        }

        if (data.CantSetColor)
        {
            eyeAdjuestView.ShowColorView(true);
        }
        else
        {
            eyeAdjuestView.ShowColorView(false);
        }
    }
}

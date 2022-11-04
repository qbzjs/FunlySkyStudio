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
/// Description:人物形象—头饰UI显示
/// Date: 2022/4/1 16:47:31
/// </summary>
public class HeadwearView : BaseView
{
    private RoleColorAdjustView hatAdjustView;
    public RoleStyleView[] iconView;
    private RoleColorView colorView;
    private PaletteColorView paletteColorView;
    private bool isUpdate = false;

    public Toggle[] SubToggles;
    public GameObject[] Panels;
    public GameObject[] newImage;
    public GameObject toggleParent;
    public VNode mDCRedDotNode;
    public VNode mOriginalRedDotNode;
    private int curSelectTogIndex = 0;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitHeadwearView);
    }
    public void InitHeadwearView()
    {
        this.bodyPart = BodyPartType.body;
        this.classifyType = ClassifyType.headwear;
        hatAdjustView = GetComponentInChildren<RoleColorAdjustView>(true);
        colorView = hatAdjustView.ColorView.GetComponentInChildren<RoleColorView>();
        colorView.Init(roleColorConfigData.hatColors.commonColors, OnSelectHatColor);
        BindHatAdjust();
        paletteColorView =this.GetComponentInChildren<PaletteColorView>(true);
        paletteColorView.InitPaletteView(roleColorConfigData.hatColors.allColors,OnSelectHatColor);
        var hsvColorView = this.GetComponentInChildren<HsvColorView>(true);
        hsvColorView.InitHsvView(OnSelectHatColor);
        iconView[0].part = bodyPart;
        iconView[0].type = classifyType;
        hsvColorView.SetGetCurrentTarget(()=>roleData.hatCr);
        NewUserUiSetting(toggleParent, Panels);
        InitToggle();
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetHatStyleDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectHatIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.hatId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var hatStyleData = RoleConfigDataManager.Inst.GetHatStyleDataById(itemId);
        if (hatStyleData != null)
        {
            hatStyleData.rc = roleStyleItem;
            OnSelectHatIcon(hatStyleData);
        }
    }

    public override void OnSelect()
    {
        UpdateSelectState();
        SelectSub(curSelectTogIndex);
    }

    public override void UpdateSelectState()
    {
        if (hatAdjustView)
        {
            isUpdate = true;
            int index = Array.FindIndex(SubToggles, (tog) => tog.isOn);
            UpdateOnSelectSub(index);
            colorView.SetSelect(roleData.hatCr);
            paletteColorView.SetSelect(roleData.hatCr);
        }

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
        iconView[index].SetSelect(roleData.hatId);
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
            ClearRed(mOriginalRedDotNode, "headweardoriginal");
        }
        if (index == 1)
        {
            ClearRed(mDCRedDotNode, "dcheadweard");
        }
    }

    private void BindHatAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Up_down);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Left_right);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Front_back);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.X_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Y_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Z_Rotation);
        hatAdjustView.Init(mAdjustItemContexts, AdjustItemValueChanged,OnAdjustViewResetCallBack);
        hatAdjustView.SetDefalutValue(roleColorConfigData.hairColors.commonColors[0]);
    }
    public void OnAdjustViewResetCallBack(int id)
    {
        HatStyleData data = RoleConfigDataManager.Inst.GetHatStyleDataById(id);
        SetAdjustView2Normal(hatAdjustView,data);
    }
    public void SetAdjustView2RoleData(RoleAdjustView adjustView, HatStyleData data, RoleData roleData)
    {
        adjustView.SetSliderValue( EAdjustItemType.Size,
            GetSliderValue(data.scaleLimit, roleData.hatS,GetVecAxis(EAdjustItemType.Size)));
        adjustView.SetSliderValue( EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, roleData.hatP, GetVecAxis(EAdjustItemType.Up_down)));
        adjustView.SetSliderValue( EAdjustItemType.Left_right,
            GetSliderValue(data.hLimit, roleData.hatP, GetVecAxis(EAdjustItemType.Left_right)));
        adjustView.SetSliderValue( EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, roleData.hatP, GetVecAxis(EAdjustItemType.Front_back)));
        adjustView.SetSliderValue( EAdjustItemType.X_Rotation,
            GetSliderValue(data.xrotLimit, roleData.hatR, GetVecAxis(EAdjustItemType.X_Rotation)));
        adjustView.SetSliderValue( EAdjustItemType.Y_Rotation,
            GetSliderValue(data.zrotLimit, roleData.hatR, GetVecAxis(EAdjustItemType.Y_Rotation)));
        adjustView.SetSliderValue( EAdjustItemType.Z_Rotation,
            GetSliderValue(data.yrotLimit, roleData.hatR, GetVecAxis(EAdjustItemType.Z_Rotation)));
    }
    public void SetAdjustView2Normal(RoleAdjustView adjustView, HatStyleData data)
    {
        hatAdjustView.SetSliderValue(EAdjustItemType.Size,
            GetSliderValue(data.scaleLimit, data.sDef, GetVecAxis(EAdjustItemType.Size)));
        hatAdjustView.SetSliderValue(EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, data.pDef, GetVecAxis(EAdjustItemType.Up_down)));
        hatAdjustView.SetSliderValue(EAdjustItemType.Left_right,
            GetSliderValue(data.hLimit, data.pDef, GetVecAxis(EAdjustItemType.Left_right)));
        hatAdjustView.SetSliderValue(EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, data.pDef, GetVecAxis(EAdjustItemType.Front_back)));
        hatAdjustView.SetSliderValue(EAdjustItemType.X_Rotation,
            GetSliderValue(data.xrotLimit, data.rDef, GetVecAxis(EAdjustItemType.X_Rotation)));
        hatAdjustView.SetSliderValue(EAdjustItemType.Y_Rotation,
            GetSliderValue(data.zrotLimit, data.rDef, GetVecAxis(EAdjustItemType.Y_Rotation)));
        hatAdjustView.SetSliderValue(EAdjustItemType.Z_Rotation,
            GetSliderValue(data.yrotLimit, data.rDef, GetVecAxis(EAdjustItemType.Z_Rotation)));
    }
    public void AdjustItemValueChanged(EAdjustItemType itemType, float value)
    {
        HatStyleData data = RoleConfigDataManager.Inst.GetHatStyleDataById(roleData.hatId);
        switch (itemType)
        {
            case EAdjustItemType.Size:
                roleData.hatS = GetValueBySlider(roleData.hatS, data.scaleLimit, value);
                rController.SetHatSca(roleData.hatS);
                break;
            case EAdjustItemType.Up_down:
                roleData.hatP = GetValueBySlider(roleData.hatP, data.vLimit, value, VecAxis.X);
                rController.SetHatPos(roleData.hatP);
                break;
            case EAdjustItemType.Left_right:
                roleData.hatP = GetValueBySlider(roleData.hatP, data.hLimit, value, VecAxis.Z);
                rController.SetHatPos(roleData.hatP);
                break;
            case EAdjustItemType.Front_back:
                roleData.hatP = GetValueBySlider(roleData.hatP, data.fLimit, value, VecAxis.Y);
                rController.SetHatPos(roleData.hatP);
                break;
            case EAdjustItemType.X_Rotation:
                roleData.hatR = GetValueBySlider(roleData.hatR, data.xrotLimit, value, VecAxis.X);
                rController.SetHatRot(roleData.hatR);
                break;
            case EAdjustItemType.Y_Rotation:
                roleData.hatR = GetValueBySlider(roleData.hatR, data.zrotLimit, value, VecAxis.Z);
                rController.SetHatRot(roleData.hatR);
                break;
            case EAdjustItemType.Z_Rotation:
                roleData.hatR = GetValueBySlider(roleData.hatR, data.yrotLimit, value, VecAxis.Y);
                rController.SetHatRot(roleData.hatR);
                break;
            default:
                break;
        }
    }
    private void OnSelectHatColor(string colorData)
    {
        roleData.hatCr = colorData;
        rController.SetHatColor(colorData);
    }

    private void OnSelectHatIcon(RoleIconData obj)
    {
        HatStyleData data = obj as HatStyleData;
        hatAdjustView.SetCurrentId(data.id);
        if (roleData.hatId == data.id)
        {
            SetAdjustView2RoleData(hatAdjustView,data,roleData);
        }

        PlayLoadingAnime(obj, true);
        rController.SetStyle(BundlePart.Hats, data.texName, () => PlayLoadingAnime(obj, false), () => PlayLoadingAnime(obj, false));
 
        if (!isUpdate)
        {
            colorView.SetSelect(roleColorConfigData.hatColors.allColors[0]);
        }
        isUpdate = false;
        if (roleData.hatId != data.id)
        {
            var hatData = RoleConfigDataManager.Inst.GetHatStyleDataById(data.id);
            roleData.hatP = hatData.pDef;
            roleData.hatS = hatData.sDef;
            roleData.hatR = hatData.rDef;
            roleData.hatId = data.id;
            SetAdjustView2Normal(hatAdjustView,data);
            
            
        }
        if (data.CantSetColor)
        {
            hatAdjustView.ShowColorView(true);
        }
        else
        {
            hatAdjustView.ShowColorView(false);
        }
    }
    protected override void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        tree.AddRedDot(rootItem.gameObject, (int)ENodeType.Body, (int)ENodeType.headwear, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        foreach (var item in datas)
        {
            if (item.resourceKind.Equals("headweardoriginal"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[0].gameObject, (int)ENodeType.headwear, (int)ENodeType.headweardoriginal, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mOriginalRedDotNode = vNode;
            }
            if (item.resourceKind.Equals("dcheadweard"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[1].gameObject, (int)ENodeType.headwear, (int)ENodeType.digitalCollect, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mDCRedDotNode = vNode;
            }
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

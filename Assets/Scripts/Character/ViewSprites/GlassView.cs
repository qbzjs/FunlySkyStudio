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
/// Description:人物形象—面饰UI显示
/// Date: 2022/4/1 16:47:46
/// </summary>
public class GlassView : BaseView
{
    private RoleColorAdjustView glassesAdjustView;
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
        RoleMenuView.Ins.SetAction(InitGlassesView);
    }
    public void InitGlassesView()
    {
        this.bodyPart = BodyPartType.body;
        this.classifyType = ClassifyType.glasses;
        glassesAdjustView = GetComponentInChildren<RoleColorAdjustView>(true);
        colorView = glassesAdjustView.ColorView.GetComponentInChildren<RoleColorView>();
        BindGlassesAdjust();
        colorView.Init(roleColorConfigData.glassesColors.commonColors, OnSelectGlassesColor);
        paletteColorView=this.GetComponentInChildren<PaletteColorView>(true);
        paletteColorView.InitPaletteView(roleColorConfigData.glassesColors.allColors,OnSelectGlassesColor);
        var hsvColorView = this.GetComponentInChildren<HsvColorView>(true);
        hsvColorView.InitHsvView(OnSelectGlassesColor);
        iconView[0].part = bodyPart;
        iconView[0].type = classifyType;
        hsvColorView.SetGetCurrentTarget(()=>roleData.glCr);
        NewUserUiSetting(toggleParent, Panels);
        InitToggle();
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetGlassesStyleDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectGlassesIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.glId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var glassStyleData = RoleConfigDataManager.Inst.GetGlassesStyleDataById(itemId);
        if (glassStyleData != null)
        {
            glassStyleData.rc = roleStyleItem;
            OnSelectGlassesIcon(glassStyleData);
        }
    }

    public override void OnSelect()
    {
        UpdateSelectState();
        SelectSub(curSelectTogIndex);
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
            ClearRed(mOriginalRedDotNode, "glassoriginal");
        }
        if (index == 1)
        {
            ClearRed(mDCRedDotNode, "dcglasses");
        }
    }

    public override void UpdateSelectState()
    {
        if (glassesAdjustView)
        {
            isUpdate = true;
            int index = Array.FindIndex(SubToggles, (tog) => tog.isOn);
            UpdateOnSelectSub(index);
            colorView.SetSelect(roleData.glCr);
            paletteColorView.SetSelect(roleData.glCr);
        }
    }

    private void UpdateOnSelectSub(int index)
    {
        iconView[index].GetAllItemList(InitListAction);
        iconView[index].curItem = null;
        iconView[index].SetSelect(roleData.glId);
    }

    private void BindGlassesAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Up_down);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Left_right);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Front_back);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.X_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Y_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Z_Rotation);
        glassesAdjustView.Init(mAdjustItemContexts, AdjustItemValueChanged,OnAdjustViewResetCallBack);
        glassesAdjustView.SetDefalutValue( roleColorConfigData.glassesColors.commonColors[0]);
    }
    public void OnAdjustViewResetCallBack(int id)
    {
        GlassesStyleData data = RoleConfigDataManager.Inst.GetGlassesStyleDataById(id);
        SetAdjustView2Normal(glassesAdjustView,data);
    }
    public void SetAdjustView2RoleData(RoleAdjustView adjustView, GlassesStyleData data, RoleData roleData)
    {
        adjustView.SetSliderValue( EAdjustItemType.Size,
            GetSliderValue(data.scaLimit, roleData.glS));
        adjustView.SetSliderValue( EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, roleData.glP,GetVecAxis(EAdjustItemType.Up_down)));
        adjustView.SetSliderValue( EAdjustItemType.Left_right,
            GetSliderValue(data.hLimit, roleData.glP, GetVecAxis(EAdjustItemType.Left_right)));
        adjustView.SetSliderValue( EAdjustItemType.Front_back,
            GetSliderValue(data.fLimit, roleData.glP, GetVecAxis(EAdjustItemType.Front_back)));
        adjustView.SetSliderValue( EAdjustItemType.X_Rotation,
            GetSliderValue(data.xrotLimit, roleData.glR, GetVecAxis(EAdjustItemType.X_Rotation)));
        adjustView.SetSliderValue( EAdjustItemType.Y_Rotation,
            GetSliderValue(data.zrotLimit, roleData.glR, GetVecAxis(EAdjustItemType.Y_Rotation)));
        adjustView.SetSliderValue( EAdjustItemType.Z_Rotation,
            GetSliderValue(data.yrotLimit, roleData.glR, GetVecAxis(EAdjustItemType.Z_Rotation)));
    }
    public void SetAdjustView2Normal(RoleAdjustView adjustView, GlassesStyleData data)
    {
        adjustView.SetSliderValue(EAdjustItemType.Size,
            GetSliderValue(data.scaLimit, data.sDef,GetVecAxis(EAdjustItemType.Size)));
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
        GlassesStyleData data = RoleConfigDataManager.Inst.GetGlassesStyleDataById(roleData.glId);
        switch (itemType)
        {
            case EAdjustItemType.Size:
                roleData.glS = GetValueBySlider(roleData.glS, data.scaLimit, value);
                rController.SetGlassesSca(roleData.glS);
                break;
            case EAdjustItemType.Up_down:
                roleData.glP = GetValueBySlider(roleData.glP, data.vLimit, value, VecAxis.X);
                rController.SetGlassesPos(roleData.glP);
                break;
            case EAdjustItemType.Left_right:
                roleData.glP = GetValueBySlider(roleData.glP, data.hLimit, value, VecAxis.Z);
                rController.SetGlassesPos(roleData.glP);
                break;
            case EAdjustItemType.Front_back:
                roleData.glP = GetValueBySlider(roleData.glP, data.fLimit, value, VecAxis.Y);
                rController.SetGlassesPos(roleData.glP);
                break;
            case EAdjustItemType.X_Rotation:
                roleData.glR = GetValueBySlider(roleData.glR, data.xrotLimit, value, VecAxis.X);
                rController.SetGlassesRot(roleData.glR);
                break;
            case EAdjustItemType.Y_Rotation:
                roleData.glR = GetValueBySlider(roleData.glR, data.zrotLimit, value, VecAxis.Z);
                rController.SetGlassesRot(roleData.glR);
                break;
            case EAdjustItemType.Z_Rotation:
                roleData.glR = GetValueBySlider(roleData.glR, data.yrotLimit, value, VecAxis.Y);
                rController.SetGlassesRot(roleData.glR);
                break;

            default:
                break;
        }
    }
    private void OnSelectGlassesColor(string colorData)
    {
        roleData.glCr = colorData;
        rController.SetGlassesColor(colorData);
    }

    private void OnSelectGlassesIcon(RoleIconData obj)
    {
        GlassesStyleData data = obj as GlassesStyleData;
        glassesAdjustView.SetCurrentId(data.id);
        if (roleData.glId == data.id)
        {
            SetAdjustView2RoleData(glassesAdjustView,data,roleData);
        }
        
        PlayLoadingAnime(obj, true);
        rController.SetStyle(BundlePart.Glasses, data.texName, () => PlayLoadingAnime(obj, false), ()=> PlayLoadingAnime(obj, false));
        if (!isUpdate)
        {
            colorView.SetSelect(roleColorConfigData.glassesColors.allColors[0]);
        }
        isUpdate = false;
        if (roleData.glId != data.id)
        {
            //TODO 这和外部传进来的Data是相同的数据
            var glData = RoleConfigDataManager.Inst.GetGlassesStyleDataById(data.id);
            roleData.glP = glData.pDef;
            roleData.glR = glData.rDef;
            roleData.glS = glData.sDef;
            roleData.glId = data.id;
            SetAdjustView2Normal(glassesAdjustView,data);
        }
        if (data.CantSetColor)
        {
            glassesAdjustView.ShowColorView(true);
        }
        else
        {
            glassesAdjustView.ShowColorView(false);
        }
    }
    protected override void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        tree.AddRedDot(rootItem.gameObject, (int)ENodeType.Body, (int)ENodeType.glasses, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        foreach (var item in datas)
        {
            if (item.resourceKind.Equals("glassoriginal"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[0].gameObject, (int)ENodeType.glasses, (int)ENodeType.glassoriginal, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                vNode.mLogic.ChangeCount(1);
                mOriginalRedDotNode = vNode;
            }
            if (item.resourceKind.Equals("dcglasses"))
            {
                VNode vNode = tree.AddRedDot(SubToggles[1].gameObject, (int)ENodeType.glasses, (int)ENodeType.digitalCollect, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
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

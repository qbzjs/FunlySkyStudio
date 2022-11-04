using System;
using System.Collections;
using System.Collections.Generic;
using AvatarRedDotSystem;
using RedDot;
using SavingData;
using ThirdParty.iOS4Unity;
using UnityEngine;
using static RTG.CameraFocus;

/// <summary>
/// Author: pzkunn
/// Description: 人物形象—面部彩绘UI
/// Date: 2022-06-13 18:47:00
/// </summary>
public class PatternView : BaseView
{
    public RoleAdjustView adjustView;
    public ClassifyTogItem classifyTogItem;
    public RoleDCPatternView digitalView;
    public RoleCustomStyleView iconView;
    public RoleUgcPatternView ugcView;
    [HideInInspector]
    public VNode mMarketplaceRedDotNode;
    [HideInInspector]
    public VNode mOriginalRedDotNode;
    [HideInInspector]
    public VNode mDCRedDotNode;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitPatternView);
    }

    public void InitPatternView()
    {
        this.bodyPart = BodyPartType.face;
        this.classifyType = ClassifyType.patterns;
        BindPatternAdjust();
        InitWearUgcPattern();
        InitCurUgcPattern();
        //Tab分类初始
        classifyTogItem.NewUserUiSetting();
        classifyTogItem.SetSelectAction(UpdateOnSelectSub, OnClearRed);
        //官方资源
        iconView.part = bodyPart;
        iconView.type = classifyType;
        //UGC
        ugcView.SetPgcAct(OnSelectPgcIconById);
        ugcView.InitExperienceList(ClassifyType.ugcPatterns);
        //DC
        digitalView.InitParams(OnSelectPgcIconById, ClassifyType.ugcPatterns);
        digitalView.InitDigitalViewList();
        //初始时设置调整轴Value
        SetAdjustView2RoleData(roleData.fpId);
        SetWearTabState();
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetPatternStylesDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectPatternIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.fpId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    private void SetWearTabState()
    {
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.SET_WEAR)
        {
            SetSelectTog(1);
        }
    }

    private void InitCurUgcPattern()
    {
        //当前设置的面部彩绘违规了,那默认不穿戴面部彩绘
        bool isUgc = RoleConfigDataManager.Inst.CurPatternIsUgc(roleData.fpId);
        if (isUgc && GameManager.Inst.ugcUserInfo != null && GameManager.Inst.ugcUserInfo.facePaintingIsBan == 1)
        {
            roleData.fpId = 0;
            roleData.ugcFPData = new UgcResData();
        }
    }
    /// <summary>
    /// 通过ID选中对应PGCItem
    /// </summary>
    /// <param name="id"></param>
    public void OnSelectPgcIconById(int id)
    {
        RoleIconData data = RoleConfigDataManager.Inst.GetPatternStylesDataById(id);
        OnSelectPatternIcon(data);
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        //仅支持PGC部件的选中
        var patternStyleData = RoleConfigDataManager.Inst.GetPatternStylesDataById(itemId);
        if (patternStyleData != null && patternStyleData.IsPGC())
        {
            patternStyleData.rc = roleStyleItem;
            OnSelectPatternIcon(patternStyleData);
        }
    }
    /// <summary>
    /// 选中对应Tog(Wear,公共avatar)
    /// </summary>
    /// <param name="index"></param>
    public void SetSelectTog(int index)
    {
        classifyTogItem.SetSelectTogByIndex(index);
    }
    /// <summary>
    /// 选中二级Tab
    /// </summary>
    public override void OnSelect()
    {
        classifyTogItem.UpdateSelectTab();//同步选中三级Tab
    }
    /// <summary>
    /// 更新选中态
    /// </summary>
    public override void UpdateSelectState()
    {
        int index = Array.FindIndex(classifyTogItem.SubToggles, (tog) => tog.isOn);
        UpdateOnSelectSub(index);
    }
    /// <summary>
    /// 同步对应三级TabItem选中态
    /// </summary>
    /// <param name="index">三级Tab Index</param>
    private void UpdateOnSelectSub(int index)
    {
        if (index == 0)
        {
            iconView.GetAllItemList(InitListAction);
            iconView.curItem = null;
            iconView.SetSelect(roleData.fpId);
        }
        if (index == 1)
        {
            ugcView.OnSelectItemByID(roleData.ugcFPData.ugcMapId, roleData.fpId);
        }
        if (index == 2)
        {
            digitalView.OnSelectItemByID(roleData.ugcFPData.ugcMapId, roleData.fpId);
        }
    }
    private void InitWearUgcPattern()
    {
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.SET_WEAR)
        {
            var ugcClothInfo = GameManager.Inst.ugcClothInfo;
            if (ugcClothInfo != null && ugcClothInfo.dataSubType == (int)DataSubType.Patterns)
            {
                PatternStyleData patterndata = RoleConfigDataManager.Inst.GetPatternByTemplateId(ugcClothInfo.templateId);
                roleData.fpId = patterndata.id;
                roleData.ugcFPData = new UgcResData
                {
                    ugcJson = ugcClothInfo.clothesJson,
                    ugcMapId = ugcClothInfo.mapId,
                    ugcUrl = ugcClothInfo.clothesUrl,
                    ugcType = (int)UGCClothesResType.UGC
                };
                roleData.fpP = patterndata.pDef;
                roleData.fpS = patterndata.sDef;
                RoleClassifiyView.Ins.SetClassifyItemSelect(ClassifyType.patterns, -1);
                DataLogUtils.AVatarUGCWear(ugcClothInfo.mapId, (int)RoleResGrading.Normal, ClassifyType.ugcPatterns);//DC不可wear
            }
        }
    }
    public void OnClearRed(int index)
    {
        if (index == 0)
        {
            ClearRed(mOriginalRedDotNode, "patternoriginal");
        }
        if (index == 1)
        {
            ClearRed(mMarketplaceRedDotNode, "ugcpattern");
        }
        if (index == 2)
        {
            ClearRed(mDCRedDotNode, "dcpattern");
        }
    }
    protected override void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        tree.AddRedDot(rootItem.gameObject, (int)ENodeType.Face, (int)ENodeType.patterns, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        foreach (var item in datas)
        {
            switch (item.resourceKind)
            {
                case "patternoriginal":
                    mOriginalRedDotNode = tree.AddRedDot(classifyTogItem.SubToggles[0].gameObject, (int)ENodeType.patterns, (int)ENodeType.patternoriginal, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                    mOriginalRedDotNode.mLogic.ChangeCount(1);
                    break;
                case "ugcpattern":
                    mMarketplaceRedDotNode = tree.AddRedDot(classifyTogItem.SubToggles[1].gameObject, (int)ENodeType.patterns, (int)ENodeType.ugcpattern, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                    mMarketplaceRedDotNode.mLogic.ChangeCount(1);
                    break;
                case "dcpattern":
                    mDCRedDotNode = tree.AddRedDot(classifyTogItem.SubToggles[2].gameObject, (int)ENodeType.patterns, (int)ENodeType.dcpattern, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                    mDCRedDotNode.mLogic.ChangeCount(1);
                    break;
            }
        }

    }

    private void OnSelectPatternIcon(RoleIconData obj)
    {
        PatternStyleData data = obj as PatternStyleData;
        adjustView.SetCurrentId(data.id);
        if (roleData.fpId == data.id)
        {
            SetAdjustView2RoleData(adjustView,data,roleData);
        }
        rController.SetPatternStyle(data.texName);
        if (roleData.fpId != data.id)
        {
            var pData = RoleConfigDataManager.Inst.GetPatternStylesDataById(data.id);
            roleData.fpP = pData.pDef;
            roleData.fpS = pData.sDef;
            roleData.fpId = data.id;
            SetAdjustView2Normal(adjustView,data);
        }
        RoleMenuView.Ins.roleData.ugcFPData = new UgcResData
        {
            ugcJson = "",
            ugcMapId = "",
            ugcUrl = "",
            ugcType = (int)UGCClothesResType.PGC
        };
}
    #region 调整
    private void BindPatternAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Up_down);
        adjustView.Init(mAdjustItemContexts, AdjustItemValueChanged,OnAdjustViewResetCallBack);
        adjustView.SetDefalutValue();
    }
    public void OnAdjustViewResetCallBack(int id)
    {
        PatternStyleData data = RoleConfigDataManager.Inst.GetPatternStylesDataById(id);
        SetAdjustView2Normal(adjustView,data);
    }
    /// <summary>
    /// 通过id设置调整轴Value与人物形象对齐(仅改变UI,不改变人物形象)
    /// </summary>
    /// <param name="id"></param>
    public void SetAdjustView2RoleData(int id)
    {
        PatternStyleData data = RoleConfigDataManager.Inst.GetPatternStylesDataById(id);
        adjustView.SetSliderValueWithoutNotify(EAdjustItemType.Size,
           GetSliderValue(data.scaleLimit, roleData.fpS));
        adjustView.SetSliderValueWithoutNotify(EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, roleData.fpP));
    }
    /// <summary>
    /// 设置调整轴Value(改变UI及人物形象)
    /// </summary>
    /// <param name="adjustView"></param>
    /// <param name="data"></param>
    /// <param name="roleData"></param>
    public void SetAdjustView2RoleData(RoleAdjustView adjustView, PatternStyleData data, RoleData roleData)
    {
        adjustView.SetSliderValue( EAdjustItemType.Size,
            GetSliderValue(data.scaleLimit, roleData.fpS));
        adjustView.SetSliderValue( EAdjustItemType.Up_down,
            GetSliderValue(data.vLimit, roleData.fpP));
    }
    public void SetAdjustView2Normal(RoleAdjustView adjustView, PatternStyleData data)
    {
        adjustView.SetSliderValue(EAdjustItemType.Size, GetSliderValue(data.scaleLimit, data.sDef));
        adjustView.SetSliderValue(EAdjustItemType.Up_down, GetSliderValue(data.vLimit, data.pDef));
    }
    public void AdjustItemValueChanged(EAdjustItemType itemType, float value)
    {
        PatternStyleData data = RoleConfigDataManager.Inst.GetPatternStylesDataById(roleData.fpId);
            switch (itemType)
            {
                case EAdjustItemType.Up_down:
                roleData.fpP = GetValueBySlider(roleData.fpP, data.vLimit, value);
                rController.SetPatternPos(roleData.fpP, data.IsPGC());
                break;
            case EAdjustItemType.Size:
                roleData.fpS = GetValueBySlider(roleData.fpS, data.scaleLimit, value);
                rController.SetPatternScale(roleData.fpS, data.IsPGC());
                break;
            default:
                break;
        }

    }
    #endregion
}
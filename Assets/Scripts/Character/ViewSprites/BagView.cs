using AvatarRedDotSystem;
using RedDot;
using System;
using System.Collections;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public enum BagCompType
{
    Backpack = 1,
    Crossbody = 2,
}
public enum BagSubType
{
    Original = 0,
    DigitalC = 1,
}
public class BagCompTypeData
{
    public string showName;
    public int bagCompType;
    public int parentNode;
    public int selfNode;
    public string redDotsKind;
    public int subType;
}

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象—背饰UI显示
/// Date: 2022/4/8 11:34:32
/// </summary>
public class BagView : BaseView
{
    public RoleColorAdjustView bagAdjustView;
    public RoleStyleView[] iconView;
    public RoleBagClassifyView[] classifyViews;
    public RoleColorView colorView;
    public PaletteColorView paletteColorView;
    private bool isUpdate = false;
    public ClassifyTogItem classifyTogItem;
    public List<BagCompTypeData> bagCompTypeConfig;
    Dictionary<BagCompType, List<BagStyleData>> bagDatasDic = new Dictionary<BagCompType, List<BagStyleData>>()
    {
        {BagCompType.Backpack,new List<BagStyleData>()},
        {BagCompType.Crossbody,new List<BagStyleData>()},
    };
    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitBagView);
    }

    public void InitBagView()
    {
        this.bodyPart = BodyPartType.body;
        this.classifyType = ClassifyType.bag;

        InitBagDatas();
        InitConfigData();
        InitViewType();
        InitClassifyView();

        //Tab分类初始
        classifyTogItem.NewUserUiSetting();
        classifyTogItem.SetSelectAction(UpdateOnSelectSecSub, null);

        colorView.Init(roleColorConfigData.bagColors.commonColors, OnSelectBagColor);
        BindBagAdjust();

        paletteColorView.InitPaletteView(roleColorConfigData.bagColors.allColors, OnSelectBagColor);
        var hsvColorView = this.GetComponentInChildren<HsvColorView>(true);
        hsvColorView.InitHsvView(OnSelectBagColor);
        hsvColorView.SetGetCurrentTarget(() => roleData.bagCr);
    }
    private void InitConfigData()
    {
        bagCompTypeConfig = ResManager.Inst.LoadJsonRes<List<BagCompTypeData>>("Configs/RoleData/BagCompTypeConfig");
    }
    private void InitBagDatas()
    {
        foreach (var item in roleConfigData.bagStyles)
        {
            if (item.bagCompType == 0)
            {
                bagDatasDic[BagCompType.Backpack].Add(item);
                bagDatasDic[BagCompType.Crossbody].Add(item);
                continue;
            }
            bagDatasDic[(BagCompType)item.bagCompType].Add(item);
        }
    }
    private void InitClassifyView()
    {
        for (int i = 0; i < classifyViews.Length; i++)
        {
            classifyViews[i].SetReqAction(UpdateOnSelectThreeSub);
            classifyViews[i].InitClassifyItem(GetClassifyDatas(i));
        }
    }
    private void InitViewType()
    {
        for (int i = 0; i < iconView.Length; i++)
        {
            iconView[i].type = classifyType;
            iconView[i].part = bodyPart;
            iconView[i].componentType = bagCompTypeConfig[i].bagCompType;
        }
    }
    private List<BagCompTypeData> GetClassifyDatas(int subType)
    {
        List<BagCompTypeData> datas = new List<BagCompTypeData>();
        return bagCompTypeConfig.FindAll(x => x.subType == subType);
    }
    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetBagStylesDataById(itemInfo.pgcId);
        if (data != null)
        {
            int curId = 0;
            switch (view.componentType)
            {
                case (int)BagCompType.Backpack:
                    view.Init(data, OnSelectBagIcon, sprite, itemInfo);
                    curId = roleData.bagId;
                    break;
                case (int)BagCompType.Crossbody:
                    view.Init(data, OnSelectCrossbodyIcon, sprite, itemInfo);
                    curId = roleData.cbId;
                    break;
            }
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == curId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var bagStyleData = RoleConfigDataManager.Inst.GetBagStylesDataById(itemId);
        if (bagStyleData != null)
        {
            bagStyleData.rc = roleStyleItem;
            switch (bagStyleData.bagCompType)
            {
                case (int)BagCompType.Backpack:
                    OnSelectBagIcon(bagStyleData);
                    break;
                case (int)BagCompType.Crossbody:
                    OnSelectCrossbodyIcon(bagStyleData);
                    break;
            }
            Array.ForEach<RoleBagClassifyView>(classifyViews, (x) => x.SetSelectByType((BagCompType)bagStyleData.bagCompType));
        }
    }
    /// <summary>
    /// 获取对应Item（兼容收藏临时处理）
    /// </summary>
    /// <param name="roleIconData"></param>
    /// <returns></returns>
    public override GameObject GetIconItem(RoleIconData roleIconData, BaseView view)
    {
        var data = RoleConfigDataManager.Inst.GetBagStylesDataById(roleIconData.id);
        if (data != null)
        {
            var iconview = Array.Find<RoleStyleView>(iconView, x => x.componentType == data.bagCompType);
            if (iconview)
            {
                return iconview.IconItem;
            }
        }
        return null;
    }
    public override void OnSelect()
    {
        classifyTogItem.UpdateSelectTab();
    }
    /// <summary>
    /// 设置当前界面的icon选中态
    /// </summary>
    /// <param name="view"></param>
    private void SetIconSelectByView(RoleStyleView view)
    {
        view.curItem = null;
        isUpdate = true;
        switch (view.componentType)
        {
            case (int)BagCompType.Backpack:
                view.SetSelect(roleData.bagId);
                break;
            case (int)BagCompType.Crossbody:
                view.SetSelect(roleData.cbId);
                break;
        }
    }
    private void SetIconSelect()
    {
        Array.ForEach<RoleStyleView>(iconView, x => SetIconSelectByView(x));
    }
    public override void UpdateSelectState()
    {
        if (bagAdjustView)
        {
            SetIconSelect();
            colorView.SetSelect(roleData.bagCr);
            paletteColorView.SetSelect(roleData.bagCr);
        }
    }

    /// <summary>
    /// 选中二级分类
    /// </summary>
    /// <param name="index"></param>
    private void UpdateOnSelectSecSub(int index)
    {
        classifyViews[index].UpdateSelectTab();
    }
    /// <summary>
    /// 选中三级分类请求回调
    /// </summary>
    /// <param name="view"></param>
    private void UpdateOnSelectThreeSub(RoleStyleView view)
    {
        view.GetAllItemList(InitListAction);
        SetIconSelectByView(view);
    }
    #region Adjust
    private void BindBagAdjust()
    {
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Size);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Up_down);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Left_right);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Front_back);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.X_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Y_Rotation);
        AdjustItemContextFactory.Create(mAdjustItemContexts, EAdjustItemType.Z_Rotation);
        bagAdjustView.Init(mAdjustItemContexts, AdjustItemValueChanged, OnAdjustViewResetCallBack);
        bagAdjustView.SetDefalutValue(roleColorConfigData.bagColors.commonColors[0]);
    }

    public void OnAdjustViewResetCallBack(int id)
    {
        BagStyleData bagData = RoleConfigDataManager.Inst.GetBagStylesDataById(id);
        SetAdjustView2Normal(bagAdjustView, bagData);
    }

    public void SetAdjustView2RoleData(RoleAdjustView adjustView, BagStyleData bagData, RoleData roleData)
    {
        bagAdjustView.SetSliderValue(EAdjustItemType.Size,
            GetSliderValue(bagData.scaLimit, roleData.bagS, GetVecAxis(EAdjustItemType.Size)));
        bagAdjustView.SetSliderValue(EAdjustItemType.Up_down,
            GetSliderValue(bagData.vLimit, roleData.bagP, GetVecAxis(EAdjustItemType.Up_down)));
        bagAdjustView.SetSliderValue(EAdjustItemType.Left_right,
            GetSliderValue(bagData.hLimit, roleData.bagP, GetVecAxis(EAdjustItemType.Left_right)));
        bagAdjustView.SetSliderValue(EAdjustItemType.Front_back,
            GetSliderValue(bagData.fLimit, roleData.bagP, GetVecAxis(EAdjustItemType.Front_back)));
        bagAdjustView.SetSliderValue(EAdjustItemType.X_Rotation,
            GetSliderValue(bagData.xrotLimit, roleData.bagR, GetVecAxis(EAdjustItemType.X_Rotation)));
        bagAdjustView.SetSliderValue(EAdjustItemType.Y_Rotation,
            GetSliderValue(bagData.zrotLimit, roleData.bagR, GetVecAxis(EAdjustItemType.Y_Rotation)));
        bagAdjustView.SetSliderValue(EAdjustItemType.Z_Rotation,
            GetSliderValue(bagData.yrotLimit, roleData.bagR, GetVecAxis(EAdjustItemType.Z_Rotation)));
    }

    public void SetAdjustView2Normal(RoleAdjustView adjustView, BagStyleData bagData)
    {
        bagAdjustView.SetSliderValue(EAdjustItemType.Size,
            GetSliderValue(bagData.scaLimit, bagData.sDef, GetVecAxis(EAdjustItemType.Size)));
        bagAdjustView.SetSliderValue(EAdjustItemType.Up_down,
            GetSliderValue(bagData.vLimit, bagData.pDef, GetVecAxis(EAdjustItemType.Up_down)));
        bagAdjustView.SetSliderValue(EAdjustItemType.Left_right,
            GetSliderValue(bagData.hLimit, bagData.pDef, GetVecAxis(EAdjustItemType.Left_right)));
        bagAdjustView.SetSliderValue(EAdjustItemType.Front_back,
            GetSliderValue(bagData.fLimit, bagData.pDef, GetVecAxis(EAdjustItemType.Front_back)));
        bagAdjustView.SetSliderValue(EAdjustItemType.X_Rotation,
            GetSliderValue(bagData.xrotLimit, bagData.rDef, GetVecAxis(EAdjustItemType.X_Rotation)));
        bagAdjustView.SetSliderValue(EAdjustItemType.Y_Rotation,
            GetSliderValue(bagData.zrotLimit, bagData.rDef, GetVecAxis(EAdjustItemType.Y_Rotation)));
        bagAdjustView.SetSliderValue(EAdjustItemType.Z_Rotation,
            GetSliderValue(bagData.yrotLimit, bagData.rDef, GetVecAxis(EAdjustItemType.Z_Rotation)));
    }

    public void AdjustItemValueChanged(EAdjustItemType itemType, float value)
    {
        BagStyleData bagData = RoleConfigDataManager.Inst.GetBagStylesDataById(roleData.bagId);
        switch (itemType)
        {
            case EAdjustItemType.Size:
                roleData.bagS = GetValueBySlider(roleData.bagS, bagData.scaLimit, value);
                rController.SetBagSca(roleData.bagS);
                break;
            case EAdjustItemType.Up_down:
                roleData.bagP = GetValueBySlider(roleData.bagP, bagData.vLimit, value, GetVecAxis(EAdjustItemType.Up_down));
                rController.SetBagPos(roleData.bagP);
                break;
            case EAdjustItemType.Left_right:
                roleData.bagP = GetValueBySlider(roleData.bagP, bagData.hLimit, value, GetVecAxis(EAdjustItemType.Left_right));
                rController.SetBagPos(roleData.bagP);
                break;
            case EAdjustItemType.Front_back:
                roleData.bagP = GetValueBySlider(roleData.bagP, bagData.fLimit, value, GetVecAxis(EAdjustItemType.Front_back));
                rController.SetBagPos(roleData.bagP);
                break;
            case EAdjustItemType.X_Rotation:
                roleData.bagR = GetValueBySlider(roleData.bagR, bagData.xrotLimit, value, GetVecAxis(EAdjustItemType.X_Rotation));
                rController.SetBagRot(roleData.bagR);
                break;
            case EAdjustItemType.Y_Rotation:
                roleData.bagR = GetValueBySlider(roleData.bagR, bagData.zrotLimit, value, GetVecAxis(EAdjustItemType.Y_Rotation));
                rController.SetBagRot(roleData.bagR);
                break;
            case EAdjustItemType.Z_Rotation:
                roleData.bagR = GetValueBySlider(roleData.bagR, bagData.yrotLimit, value, GetVecAxis(EAdjustItemType.Z_Rotation));
                rController.SetBagRot(roleData.bagR);
                break;
            default:
                break;
        }
    }
    #endregion

    private void OnSelectBagColor(string colorData)
    {
        roleData.bagCr = colorData;
        rController.SetBagColor(colorData);
    }

    private void OnSelectBagIcon(RoleIconData obj)
    {
        BagStyleData data = obj as BagStyleData;
        bagAdjustView.SetCurrentId(data.id);
        if (roleData.bagId == data.id)
        {
            SetAdjustView2RoleData(bagAdjustView, data, roleData);
        }
        PlayLoadingAnime(data, true);
        rController.SetStyle(BundlePart.Bag, data.texName, () => PlayLoadingAnime(data, false), () => PlayLoadingAnime(data, false));
        if (!isUpdate)
        {
            var colorView = bagAdjustView.ColorView.GetComponentInChildren<RoleColorView>();
            colorView.SetSelect(roleColorConfigData.bagColors.allColors[0]);
        }
        isUpdate = false;

        if (roleData.bagId != data.id)
        {
            var bagData = RoleConfigDataManager.Inst.GetBagStylesDataById(data.id);
            roleData.bagP = bagData.pDef;
            roleData.bagS = bagData.sDef;
            roleData.bagR = bagData.rDef;
            roleData.bagId = data.id;
            SetAdjustView2Normal(bagAdjustView, bagData);
        }
        bagAdjustView.ShowColorView(data.CantSetColor);
    }

    private void OnSelectCrossbodyIcon(RoleIconData data)
    {
        roleData.cbId = data.id;
        PlayLoadingAnime(data, true);
        rController.SetStyle(BundlePart.Crossbody, data.texName, () => PlayLoadingAnime(data, false), () => PlayLoadingAnime(data, false));
    }
    protected override void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        //二级红点  
        tree.AddRedDot(rootItem.gameObject, (int)ENodeType.Body, (int)ENodeType.bag, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        //三级红点
        tree.AddRedDot(classifyTogItem.SubToggles[0].gameObject, (int)ENodeType.bag, (int)ENodeType.bagoriginal, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        tree.AddRedDot(classifyTogItem.SubToggles[1].gameObject, (int)ENodeType.bag, (int)ENodeType.dcbag, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        //四级红点
        Array.ForEach<RoleBagClassifyView>(classifyViews, (x) => x.AttachRedDot());
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

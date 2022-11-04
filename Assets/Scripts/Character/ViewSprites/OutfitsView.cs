using GRTools.Localization;
using RedDot;
using SavingData;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using AvatarRedDotSystem;
using System;

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象—衣服UI显示
/// Date: 2022/4/1 16:47:7
/// </summary>
public class OutfitsView : BaseView
{
    public ClassifyTogItem classifyTogItem;
    public RoleClothStyleView clothView;
    public RoleClothDigitalView digitalView;
    public RoleStyleView pgcView;
    [HideInInspector]
    public VNode mMarketplaceRedDotNode;
    [HideInInspector]
    public VNode mOriginalRedDotNode;
    [HideInInspector]
    public VNode mDCRedDotNode;
    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitOutfitsView);
    }
    public void InitOutfitsView()
    {
        this.bodyPart = BodyPartType.body;
        this.classifyType = ClassifyType.outfits;
        InitWearUgcCloth();
        InitCurUgcCloth();
        //Tab分类初始
        classifyTogItem.NewUserUiSetting();
        classifyTogItem.SetSelectAction(UpdateOnSelectSub, OnClearRed);
        //UGC
        clothView.InitExperienceList(ClassifyType.ugcCloth);
        //DC
        digitalView.InitParams(OnScelctPgcIconById, ClassifyType.ugcCloth);
        digitalView.InitDigitalViewList();

        pgcView.part = bodyPart;
        pgcView.type = classifyType;
        SetWearTabState();
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetClothesById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectClothesIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.cloId)
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
    public void OnScelctPgcIconById(int id)
    {
        RoleIconData data = RoleConfigDataManager.Inst.GetClothesById(id);
        OnSelectClothesIcon(data);
    }
    /// <summary>
    /// 清红点
    /// </summary>
    /// <param name="index"></param>
    public void OnClearRed(int index)
    {
        if (index == 0)
        {
            ClearRed(mOriginalRedDotNode, "outfitsoriginal");
        }
        if (index == 1)
        {
            ClearRed(mMarketplaceRedDotNode, "ugcclothes");
        }
        if (index == 2)
        {
            ClearRed(mDCRedDotNode, "dcclothes");
        }
    }
    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        //仅支持PGC部件的选中
        var clothesStyleData = RoleConfigDataManager.Inst.GetClothesById(itemId);
        if (clothesStyleData != null && clothesStyleData.IsPGC())
        {
            clothesStyleData.rc = roleStyleItem;
            OnSelectClothesIcon(clothesStyleData);
        }
    }
    /// <summary>
    /// 选中二级Tab
    /// </summary>
    public override void OnSelect()
    {
        classifyTogItem.UpdateSelectTab();//同步选中三级Tab
    }

    /// <summary>
    /// 更新选中态（重置人物形象）
    /// </summary>
    public override void UpdateSelectState()
    {
        int index = Array.FindIndex(classifyTogItem.SubToggles, (tog) => tog.isOn);
        UpdateOnSelectSub(index);
    }

    /// <summary>
    /// 供外部调用选中对应Tog(Wear,公共avatar)
    /// </summary>
    /// <param name="index"></param>
    public void SetSelectTog(int index)
    {
        classifyTogItem.SetSelectTogByIndex(index);
    }
    /// <summary>
    /// 更新Item选中态
    /// </summary>
    /// <param name="index"></param>
    private void UpdateOnSelectSub(int index)
    {
        if (index == 0)
        {
            pgcView.GetAllItemList(InitListAction);
            pgcView.curItem = null;
            pgcView.SetSelect(roleData.cloId);
        }
        if (index == 1)
        {
            clothView.OnSelectItemByID(roleData.clothMapId, roleData.cloId);
        }
        if (index == 2)
        {
            digitalView.OnSelectItemByID(roleData.clothMapId, roleData.cloId);
        }
    }

    protected override void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        tree.AddRedDot(rootItem.gameObject, (int)ENodeType.Body, (int)ENodeType.outfits, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
        foreach (var item in datas)
        {
            switch (item.resourceKind)
            {
                case "outfitsoriginal":
                    mOriginalRedDotNode = tree.AddRedDot(classifyTogItem.SubToggles[0].gameObject, (int)ENodeType.outfits, (int)ENodeType.outfitsoriginal, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                    mOriginalRedDotNode.mLogic.ChangeCount(1);
                    break;
                case "ugcclothes":
                    mMarketplaceRedDotNode = tree.AddRedDot(classifyTogItem.SubToggles[1].gameObject, (int)ENodeType.outfits, (int)ENodeType.ugcCloth, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                    mMarketplaceRedDotNode.mLogic.ChangeCount(1);
                    break;
                case "dcclothes":
                    mDCRedDotNode = tree.AddRedDot(classifyTogItem.SubToggles[2].gameObject, (int)ENodeType.outfits, (int)ENodeType.digitalCollect, ERedDotPrefabType.Type3, ERedDotPos.TopRight);
                    mDCRedDotNode.mLogic.ChangeCount(1);
                    break;
            }
        }

    }
    //PGC衣服Item的OnClick事件
    private void OnSelectClothesIcon(RoleIconData data)
    {
        roleData.cloId = data.id;
        var roleItem = data.rc;
        PlayLoadingAnime(data, true);
        rController.SetStyle(BundlePart.Clothes, data.texName, ()=>PlayLoadingAnime(data, false), ()=>PlayLoadingAnime(data, false));
        RoleMenuView.Ins.roleData.ugcClothType = (int)UGCClothesResType.PGC;
        
        //清空UGC衣服数据
        roleData.clothesJson = "";
        roleData.clothesUrl = "";
        roleData.clothMapId = "";
    }

    private void InitWearUgcCloth()
    {
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.SET_WEAR)
        {
            var ugcClothInfo = GameManager.Inst.ugcClothInfo;
            if (ugcClothInfo != null && ugcClothInfo.dataSubType == (int)DataSubType.Clothes)
            {
                ClothStyleData clothesData = RoleConfigDataManager.Inst.GetClothesByTemplateId(ugcClothInfo.templateId);
                roleData.cloId = clothesData.id;
                roleData.clothesJson = ugcClothInfo.clothesJson;
                roleData.clothesUrl = ugcClothInfo.clothesUrl;
                roleData.clothMapId = ugcClothInfo.mapId;
                roleData.ugcClothType = (int)UGCClothesResType.UGC;
                RoleClassifiyView.Ins.SetClassifyItemSelect(ClassifyType.outfits, -1);
                DataLogUtils.AVatarUGCWear(ugcClothInfo.mapId, (int)RoleResGrading.Normal, ClassifyType.ugcCloth);//DC不可wear
            }
        }
    }

    private void InitCurUgcCloth()
    {
        //当前设置的衣服违规了,那默认换成第一套衣服
        bool isUgc = RoleConfigDataManager.Inst.CurClothesIsUgc(roleData.cloId);
        if (isUgc && GameManager.Inst.ugcUserInfo != null && GameManager.Inst.ugcUserInfo.clothesIsBan == 1)
        {
            roleData.cloId = 1;
        }
    }
}

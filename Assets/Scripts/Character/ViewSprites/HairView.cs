using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using SavingData;

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象—头发UI显示
/// Date: 2022/4/1 10:29:14
/// </summary>

public class HairView : BaseView
{
    public RoleStyleView iconView;

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitHairView);
    }
    public void InitHairView()
    {
        this.bodyPart = BodyPartType.body;
        this.classifyType = ClassifyType.hair;
        var colorView = this.GetComponentInChildren<RoleColorView>(true);
        colorView.Init(roleColorConfigData.hairColors.commonColors, OnSelectHairColor);
        iconView.part = bodyPart;
        iconView.type = classifyType;
        var paletteColorView=this.GetComponentInChildren<PaletteColorView>(true);
        paletteColorView.InitPaletteView(roleColorConfigData.hairColors.allColors,OnSelectHairColor);
        var hsvColorView = this.GetComponentInChildren<HsvColorView>(true);
        hsvColorView.InitHsvView(OnSelectHairColor);
        hsvColorView.SetGetCurrentTarget(()=>roleData.hCr);
    }

    private void InitListAction(RoleStyleView view, RoleItemInfo itemInfo)
    {
        var data = RoleConfigDataManager.Inst.GetHairDataById(itemInfo.pgcId);
        if (data != null)
        {
            view.Init(data, OnSelectHairIcon, sprite, itemInfo);
            //创建完成 --> 选中形象当前部件
            if (itemInfo.pgcId == roleData.hId)
            {
                view.curItem = itemInfo.item;
                view.curItem.SetSelectState(true);
            }
        }
    }

    public override void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {
        var hairStyleData = RoleConfigDataManager.Inst.GetHairDataById(itemId);
        if (hairStyleData != null)
        {
            hairStyleData.rc = roleStyleItem;
            OnSelectHairIcon(hairStyleData);
        }
    }

    public override void OnSelect()
    {
        UpdateSelectState();
    }
    public override void UpdateSelectState()
    {
        iconView.GetAllItemList(InitListAction);
        iconView.SetSelect(roleData.hId);
        var colorView = this.GetComponentInChildren<RoleColorView>(true);
        var paletteview=this.GetComponentInChildren<PaletteColorView>(true);
        var hsvView = GetComponentInChildren<HsvColorView>(true);
        colorView.SetSelect(roleData.hCr);
        paletteview.SetSelect(roleData.hCr);
        paletteview.gameObject.SetActive(false);
        hsvView.gameObject.SetActive(false);
    }
    private void OnSelectHairColor(string colorData)
    {
        roleData.hCr = colorData;
        rController.SetHairColor(colorData);
    }
    private void OnSelectHairIcon(RoleIconData data)
    {
        roleData.hId = data.id;
        PlayLoadingAnime(data, true);
        rController.SetStyle(BundlePart.Hair, data.texName, ()=>PlayLoadingAnime(data, false), ()=>PlayLoadingAnime(data, false));
    }
}

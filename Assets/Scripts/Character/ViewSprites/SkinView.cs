using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象—肤色UI显示
/// Date: 2022/4/1 16:46:45
/// </summary>
public class SkinView : BaseView
{
    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitSkinView);
    }
    public void InitSkinView()
    {
        this.classifyType = ClassifyType.skin;
        var colorView = GetComponentInChildren<RoleColorView>();
        colorView.Init(roleColorConfigData.skinColors.commonColors, OnSelectSkinIcon);
        var hsvView = GetComponentInChildren<HsvColorView>(true);
        hsvView.InitHsvView(OnSelectSkinIcon);
        hsvView.SetGetCurrentTarget(()=>roleData.sCr);
    }

    public override void OnSelect()
    {
        UpdateSelectState();
    }

    public override void UpdateSelectState()
    {
        var colorView = GetComponentInChildren<RoleColorView>(true);
        colorView.SetSelect(roleData.sCr);
        var hsvView = GetComponentInChildren<HsvColorView>(true);
        hsvView.gameObject.SetActive(false);
    }

    private void OnSelectSkinIcon(string colorData)
    {
        roleData.sCr = colorData;
        rController.SetSkinColor(colorData);
    }
}

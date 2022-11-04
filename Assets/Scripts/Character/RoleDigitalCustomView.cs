using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Author: pzkunn
/// Description: DC分类调节界面管理
/// Date: 2022/8/5 10:42:35
/// </summary>
public class RoleDigitalCustomView : RoleDigitalView
{
    public GameObject StyleView;
    public RoleAdjustView AdjuestView;

    public override void Init<T>(T data, Action<RoleIconData> select, SpriteAtlas spriteAtlas, RoleItemInfo info)
    {
        base.Init(data, select, spriteAtlas, info);
        var cItem = info.item as RoleCustomStyleItem;
        cItem.SetCustomView(ShowAdjustView);
    }

    public void ShowAdjustView()
    {
        AdjuestView.Show(StyleView);
    }
}
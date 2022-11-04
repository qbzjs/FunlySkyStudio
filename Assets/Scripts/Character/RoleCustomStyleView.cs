using SavingData;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class RoleCustomStyleView : RoleStyleView
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
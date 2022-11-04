using System;
using System.Collections;
using System.Collections.Generic;
using SuperScrollView;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:面部彩绘DC界面
/// Date: 2022/9/23
/// </summary>
public class RoleUgcPatternView : RoleUgcBaseView
{
    public Texture CancelTexture;//取消按钮贴图
    public RoleAdjustView AdjustView;
    private int specilCount = 1;
    public Action<int> pgcSelectAct;

    public override void OnSelectItemByID(string mapId, int pgcId)
    {
        int index = allUgcClothesInfos.FindIndex(x => x.mapId.Equals(mapId));
        if (index > -1)
        {
            var item = mLoopGridView.GetShownItemByItemIndex(index + 2);
            if (item != null)
            {
                var itemScript = item.GetComponent<RoleStyleUgcItem>();
                OnItemSelectState(itemScript);
            }
        }
        else if (string.IsNullOrEmpty(mapId) && pgcId == 0)//选中取消按钮
        {
            var item = mLoopGridView.GetShownItemByItemIndex(1);
            if (item != null)
            {
                var itemScript = item.GetComponent<RoleStyleUgcItem>();
                OnItemSelectState(itemScript);
            }
        }
        else
        {
            SetUgcItemUnSelect();
        }
    }
    public void SetPgcAct(Action<int> pgcAct)
    {
        this.pgcSelectAct = pgcAct;
    }
    public override void UpdateHttpRequestArg(int pageSize, string cookie = "")
    {
        base.UpdateHttpRequestArg(pageSize, cookie);
        httpRequest.dataSubType = (int)DataSubType.Patterns;
    }
    public override void OnInitClothesResListSuccess(string content)
    {
        base.OnInitClothesResListSuccess(content);
        mLoopGridView.InitGridView(allUgcClothesInfos.Count + 2, OnGetItemByRowColumn);
    }
    public override void OnGetClothesResListSuccess(string content)
    {
        base.OnGetClothesResListSuccess(content);
        mLoopGridView.RefreshGridView(allUgcClothesInfos.Count + 2);
    }
    public void ShowAdjustView()
    {
        if (AdjustView != null)
        {
            AdjustView.Show(this.gameObject);
        }
    }
    public override LoopGridViewItem OnGetItemByRowColumn(LoopGridView gridView, int itemIndex, int row, int column, ScrollDirection sdir)
    {
        LoopGridViewItem item = gridView.GetItemByPool();
        item.gameObject.name = itemIndex.ToString();
        var ugcItem = item.GetComponent<RoleStyleUgcItem>();
        //first store
        if (itemIndex == 0)
        {
            ugcItem.SetStoreTexture(StoreTexture);
            ugcItem.type = type;
            return item;
        }
        //取消按钮
        if (itemIndex == 1)
        {
            ugcItem.type = type;
            ugcItem.SetCancelBtn(CancelTexture, () =>
            {
                pgcSelectAct?.Invoke(0);
                OnItemSelectState(ugcItem);
            });
            if (RoleMenuView.Ins.roleData.fpId == 0)
            {
                OnItemSelectState(ugcItem);
            }
            return item;
        }
        var itemData = allUgcClothesInfos[itemIndex - 2];
        if (itemData == null || itemData.coverUrl == null)
        {
            ugcItem.spriteImg.gameObject.SetActive(false);
            ugcItem.wearLoader.SetActive(true);
            return item;
        }
        ugcItem.Init(itemData, OnUgcItemSelect);
        ugcItem.CanAdjust = true;
        ugcItem.SetCustomView(ShowAdjustView);
        AddItemList(ugcItem);
        textureBatchLoader.m_OnImageLoadError = (err, detail) =>
        {
            Debug.LogError("Error url " + err + "----------" + detail.m_URL);
        };
        textureBatchLoader.m_OnImageLoaded = (result) =>
        {
            if (result.m_Texture)
            {
                //快速翻页会出现查找不到情况
                var item = gridView.GetShownItemByItemIndex(result.tempArg);
                if (item != null)
                {
                    var itemScript = item.GetComponent<RoleStyleUgcItem>();
                    itemScript.SetItemTexture(result.m_Texture);
                }
            }
        };
        var tex = textureBatchLoader.GetImageByUrl(itemData.coverUrl, itemIndex, sdir, loadIfNotFound: true);
        if (tex)
        {
            ugcItem.SetItemTexture(tex);
        }
        if (!string.IsNullOrEmpty(RoleMenuView.Ins.roleData.ugcFPData.ugcMapId))
        {
            if (itemData.mapId.Equals(RoleMenuView.Ins.roleData.ugcFPData.ugcMapId))
            {
                OnItemSelectState(ugcItem);
            }
        }
        return item;
    }
    public override void OnUgcItemSelect(RoleStyleUgcItem ugcItem)
    {
        base.OnUgcItemSelect(ugcItem);
        var curRoleData = RoleMenuView.Ins.roleData;
        if (ugcItem.rcData.mapId == curRoleData.ugcFPData.ugcMapId)//避免重置调整参数
        {
            return;
        }
        var templateId = ugcItem.rcData.templateId;
        PatternStyleData patternData = RoleConfigDataManager.Inst.GetPatternByTemplateId(templateId);
        if (patternData != null)
        {
            patternData.patternJson = ugcItem.rcData.jsonUrl;
            patternData.patternUrl = ugcItem.rcData.zipUrl;
            patternData.patternMapId = ugcItem.rcData.mapId;
            curRoleData.fpId = patternData.id;
            curRoleData.ugcFPData = new UgcResData
            {
                ugcMapId = patternData.patternMapId,
                ugcJson = patternData.patternJson,
                ugcUrl = patternData.patternUrl,
                ugcType = (int)UGCClothesResType.UGC
            };
            var roleComp = RoleMenuView.Ins.rController;
            ugcItem.PlayLoadTexAnim(true);
            roleComp.SetUgcPatternStyle(patternData, () => { ugcItem.PlayLoadTexAnim(false); }, () => { ugcItem.PlayLoadTexAnim(false); });
            var patternview = RoleMenuView.Ins.GetView<PatternView>();
            patternview.SetAdjustView2Normal(AdjustView, patternData);
        }
    }
    public override void ReqCleanRedDot()
    {
        base.ReqCleanRedDot();
        RoleMenuView.Ins.mAvatarRedDotManager.ReqCleanRedDot("ugcpattern");
    }
}

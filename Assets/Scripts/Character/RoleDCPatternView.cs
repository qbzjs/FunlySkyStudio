using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;
using UnityEngine.U2D;
using System;
/// <summary>
/// Author:Meimei-LiMei
/// Description:面部彩绘UGC界面
/// Date: 2022/9/23
/// </summary>
public class RoleDCPatternView : RoleDCBaseView
{
    public Texture CancelTexture;
    public RoleAdjustView AdjustView;
    public override void OnSelectItemByID(string mapId, int pgcId)
    {
        int index = allUgcClothesInfos.FindIndex(x => (!string.IsNullOrEmpty(mapId) && x.mapId.Equals(mapId)) || x.pgcId.Equals(pgcId));
        if (index > -1)
        {
            var item = mLoopGridView.GetShownItemByItemIndex(index + 1);
            if (item != null)
            {
                var itemScript = item.GetComponent<RoleStyleUgcItem>();
                OnItemSelectState(itemScript);
            }
        }
        //选中取消按钮
        if (string.IsNullOrEmpty(mapId) && RoleMenuView.Ins.roleData.fpId == 0)
        {
            var item = mLoopGridView.GetShownItemByItemIndex(0);
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
    public override void SetInitNFTClothesInfos()
    {
        InitNFTClothesInfos(() =>
      {
          mLoopGridView.InitGridView(allUgcClothesInfos.Count + 1, OnGetItemByRowColumn);//TODO
      });
    }
    public override void UpdateResListSuccess(string content)
    {
        base.UpdateResListSuccess(content);
        mLoopGridView.SetListItemCount(allUgcClothesInfos.Count + 1, false);
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
        //取消按钮
        if (itemIndex == 0)
        {
            ugcItem.type = pgctype;
            ugcItem.SetCancelBtn(CancelTexture, () =>
            {
                pgcItemSclect?.Invoke(ugcItem.rcData.pgcId);
                OnItemSelectState(ugcItem);
            });
            return item;
        }
        var itemData = allUgcClothesInfos[itemIndex - 1];
        if (itemData == null)
        {
            return item;
        }

        if (itemIndex < nftCount)
        {
            //nft流程
            var patternData = RoleConfigDataManager.Inst.GetPatternByTemplateId(itemData.pgcId);
            if (patternData != null)
            {
                if (nftSprite == null)
                {
                    nftSprite = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/AtlasAB/Pattern");
                }
                ugcItem.Init(itemData, OnNftItemSelect);
                ugcItem.CanAdjust = true;
                ugcItem.SetCustomView(ShowAdjustView);
                AddItemList(UGCClothesResType.PGC, ugcItem);
                ugcItem.SetItemTexture(patternData.spriteName, nftSprite);
                if (RoleMenuView.Ins.roleData.fpId == itemData.pgcId)
                {
                    OnItemSelectState(ugcItem);
                }
            }
            return item;
        }

        //ugc流程
        ugcItem.Init(itemData, OnUgcItemSelect);
        ugcItem.CanAdjust = true;
        ugcItem.SetCustomView(ShowAdjustView);
        AddItemList(UGCClothesResType.UGC, ugcItem);
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
                ugcJson = patternData.patternJson,
                ugcMapId = patternData.patternMapId,
                ugcUrl = patternData.patternUrl,
                ugcType = (int)UGCClothesResType.DC
            };
            var roleComp = RoleMenuView.Ins.rController;
            ugcItem.PlayLoadTexAnim(true);
            roleComp.SetUgcPatternStyle(patternData, () => { ugcItem.PlayLoadTexAnim(false); }, () => { ugcItem.PlayLoadTexAnim(false); });
            var patternview = RoleMenuView.Ins.GetView<PatternView>();
            patternview.SetAdjustView2Normal(AdjustView, patternData);
        }
    }
    public override void OnNftItemSelect(RoleStyleUgcItem ugcItem)
    {
        base.OnNftItemSelect(ugcItem);
        RoleMenuView.Ins.roleData.ugcFPData.ugcType = (int)UGCClothesResType.PGC;
    }
}

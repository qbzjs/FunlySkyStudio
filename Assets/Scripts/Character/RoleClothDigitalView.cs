using System;
using System.Collections;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using SuperScrollView;
using UnityEngine.UI;
using UnityEngine.U2D;
using DG.Tweening;

/// <summary>
/// Author:Meimei-LiMei
/// Description:Digital Collectibles分类界面管理
/// Date: 2022/6/30 21:3:11
/// </summary>
public class RoleClothDigitalView : RoleDCBaseView
{
    public override void OnSelectItemByID(string mapId, int pgcId)
    {
        base.OnSelectItemByID(mapId, pgcId);
    }
    public override void SetInitNFTClothesInfos()
    {
        base.SetInitNFTClothesInfos();
    }
    public override void UpdateResListSuccess(string content)
    {
        base.UpdateResListSuccess(content);
        mLoopGridView.SetListItemCount(allUgcClothesInfos.Count, false);
    }
    public override LoopGridViewItem OnGetItemByRowColumn(LoopGridView gridView, int itemIndex, int row, int column, ScrollDirection sdir)
    {
       
        LoopGridViewItem item = gridView.GetItemByPool();
        item.gameObject.name = itemIndex.ToString();
        var ugcItem = item.GetComponent<RoleStyleUgcItem>();
        var itemData = allUgcClothesInfos[itemIndex];
        if (itemData == null)
        {
            return item;
        }

        if (itemIndex < nftCount)
        {
            //nft流程
            var clothesData = RoleConfigDataManager.Inst.GetClothesById(itemData.pgcId);
            if (nftSprite == null)
            {
                nftSprite = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/AtlasAB/Outfits");
            }
            ugcItem.Init(itemData, OnNftItemSelect);
            AddItemList(UGCClothesResType.PGC, ugcItem);
            ugcItem.SetItemTexture(clothesData.spriteName, nftSprite);
            if (RoleMenuView.Ins.roleData.cloId == itemData.pgcId)
            {
                OnItemSelectState(ugcItem);
            }
            return item;
        }

        //ugc流程
        ugcItem.Init(itemData, OnUgcItemSelect);
        AddItemList(UGCClothesResType.UGC, ugcItem);
        textureBatchLoader.m_OnImageLoadError = (err, detail) =>
        {
            LoggerUtils.LogError("Error url " + err + "----------"+detail.m_URL);
        };
        textureBatchLoader.m_OnImageLoaded = (result) =>
        {
            if (result.m_Texture)
            {
                //快速翻页会出现查找不到情况
                var item = gridView.GetShownItemByItemIndex(result.tempArg);
                if (item != null)
                {
                    var itemScript  = item.GetComponent<RoleStyleUgcItem>();
                    itemScript.SetItemTexture(result.m_Texture);
                }
            }
        };
        var tex = textureBatchLoader.GetImageByUrl(itemData.coverUrl,itemIndex,sdir, loadIfNotFound: true);
        if (tex)
        {
            ugcItem.SetItemTexture(tex);
        }
        if (itemData.mapId.Equals(RoleMenuView.Ins.roleData.clothMapId))
        {
            OnItemSelectState(ugcItem);
        }
        return item;
    }
    public override void OnUgcItemSelect(RoleStyleUgcItem ugcItem)
    {
        base.OnUgcItemSelect(ugcItem);
        var templateId = ugcItem.rcData.templateId;
        ClothStyleData clothesData = RoleConfigDataManager.Inst.GetClothesByTemplateId(templateId);
        clothesData.clothesJson = ugcItem.rcData.jsonUrl;
        clothesData.clothesUrl = ugcItem.rcData.zipUrl;
        clothesData.clothMapId = ugcItem.rcData.mapId;
        var curRoleData = RoleMenuView.Ins.roleData;
        curRoleData.cloId = clothesData.id;
        curRoleData.clothesJson = clothesData.clothesJson;
        curRoleData.clothesUrl = clothesData.clothesUrl;
        curRoleData.clothMapId = clothesData.clothMapId;
        curRoleData.ugcClothType = (int)UGCClothesResType.DC;
        var roleComp = RoleMenuView.Ins.rController;
        ugcItem.PlayLoadTexAnim(true);
        ClothLoadManager.Inst.LoadUGCClothRes(clothesData, roleComp, () => { ugcItem.PlayLoadTexAnim(false); }, () => { ugcItem.PlayLoadTexAnim(false); });
    }
    public override void OnNftItemSelect(RoleStyleUgcItem ugcItem)
    {
        base.OnNftItemSelect(ugcItem);
        RoleMenuView.Ins.roleData.ugcClothType = (int)UGCClothesResType.PGC;
    }
}

using System;
using SuperScrollView;
using UnityEngine;

public class RoleClothStyleView : RoleUgcBaseView
{
    public override void OnSelectItemByID(string mapId, int pgcId = 0)
    {
        int index = allUgcClothesInfos.FindIndex(x => x.mapId.Equals(mapId));
        if (index > -1)
        {
            var item = mLoopGridView.GetShownItemByItemIndex(index + 1);//TODO
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
    public override void UpdateHttpRequestArg(int pageSize, string cookie = "")
    {
        base.UpdateHttpRequestArg(pageSize, cookie);
        httpRequest.dataSubType = (int)DataSubType.Clothes;
    }
    public override void OnInitClothesResListSuccess(string content)
    {
        base.OnInitClothesResListSuccess(content);
        mLoopGridView.InitGridView(allUgcClothesInfos.Count + 1, OnGetItemByRowColumn);
    }
    public override void OnGetClothesResListSuccess(string content)
    {
        base.OnGetClothesResListSuccess(content);
        mLoopGridView.RefreshGridView(allUgcClothesInfos.Count + 1);
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
            ugcItem.type = ClassifyType.ugcCloth;
            return item;
        }
        var itemData = allUgcClothesInfos[itemIndex - 1];
        if (itemData == null || itemData.coverUrl == null)
        {
            ugcItem.spriteImg.gameObject.SetActive(false);
            ugcItem.wearLoader.SetActive(true);
            return item;
        }
        ugcItem.Init(itemData, OnUgcItemSelect);
        AddItemList(ugcItem);
        textureBatchLoader.m_OnImageLoadError = (err, detail) =>
        {
            LoggerUtils.LogError("Error url " + err + "----------" + detail.m_URL);
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
        curRoleData.ugcClothType = (int)UGCClothesResType.UGC;
        var roleComp = RoleMenuView.Ins.rController;
        ugcItem.PlayLoadTexAnim(true);
        ClothLoadManager.Inst.LoadUGCClothRes(clothesData, roleComp, () => { ugcItem.PlayLoadTexAnim(false); }, () => { ugcItem.PlayLoadTexAnim(false); });
    }
    public override void ReqCleanRedDot()
    {
        base.ReqCleanRedDot();
        RoleMenuView.Ins.mAvatarRedDotManager.ReqCleanRedDot("ugcclothes");
    }

}

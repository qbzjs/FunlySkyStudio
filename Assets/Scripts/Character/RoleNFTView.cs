using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: pzkunn
/// Description: 官方NFT-Item列表页面
/// Date: 2022/10/18 20:8:37
/// </summary>
public class RoleNFTView : MonoBehaviour
{
    public DigitalCollectView parentView;
    public Image bannerImg;
    public GameObject bannerloader;
    public Transform iconParent;
    public RectTransform iconViewRTF;

    private int seriesId;
    private string seriesName;
    private RoleStyleItem curItem;
    private NFTHttpReqQuerry reqQuerry = new NFTHttpReqQuerry();
    private List<NFTItemInfo> itemInfos = new List<NFTItemInfo>();

    //页面初始化
    public void GetItemListInfo(int seriesId, string seriesName)
    {
        this.seriesId = seriesId;
        this.seriesName = seriesName;
        reqQuerry.seriesName = seriesName;
        reqQuerry.pageSize = 50;
        reqQuerry.cookie = "";
        reqQuerry.toUid = GameManager.Inst.ugcUserInfo.uid;
        RefreshItemList();
    }

    private void RefreshItemList()
    {
        HttpUtils.MakeHttpRequest("/other/getOfficialNfts", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(reqQuerry), OnGetItemListSuccess, OnGetItemListFail);
    }

    private void OnGetItemListSuccess(string content)
    {
        LoggerUtils.Log($"GetDigitalCollectInfo:{seriesName} Success --> {content}");
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data))
        {
            LoggerUtils.LogError($"GetDigitalCollectInfo:{seriesName} Failed --> repData.data == null");
            return;
        }
        AvatarClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<AvatarClothesRepInfo>(repData.data);
        reqQuerry.cookie = resourceInfo.cookie;
        if (resourceInfo.resources != null)
        {
            foreach (var item in resourceInfo.resources)
            {
                if (item.IsIllegal())
                {
                    continue;
                }
                //创建本页面Items
                CreateResourceItem(item);
            }
        }
        if (resourceInfo.isEnd != 1)
        {
            RefreshItemList();
        }
    }

    private void OnGetItemListFail(string error)
    {
        LoggerUtils.LogError($"GetDigitalCollectInfo:{seriesName} Failed --> {error}");
    }

    private void CreateResourceItem(AvatarClothesInfo item)
    {
        var dcItem = parentView.CreateNFTItem((ClassifyType)item.resourceType, item.id, iconParent, OnSelectClick);
        if (dcItem == null)
        {
            LoggerUtils.LogError($"CreateDigitalCollectItem: {item.resourceType} - {item.id} Failed");
            return;
        }
        //放入DCView集合
        var itemType = seriesId > 0 ? ItemDictType.DCView : ItemDictType.DCViewAll;
        BaseView.Ins.AddItemList(itemType, (ClassifyType)item.resourceType, dcItem);
        //更新收藏标识
        dcItem.UpdateItemCollect(item.isFavorites == 1);
        //更新isNew标识
        dcItem.UpdateItemIsNew(item.isNew == 1);
        //将item关联info信息
        var info = CreateInfo(item, dcItem);
        //顺序排列并放入列表
        SortWhenCreateItem(dcItem, info);
    }

    private NFTItemInfo CreateInfo(AvatarClothesInfo item, RoleStyleItem styleItem)
    {
        return new NFTItemInfo()
        {
            sort = item.sort,
            detailsType = item.detailsType,
            shadowUrl = item.shadowUrl,
            bannerUrl = item.bannerUrl,
            backgroundUrl = item.backgroundUrl,
            itemId = item.itemId,
            budActId = item.budActId,
            item = styleItem
        };
    }

    private void SortWhenCreateItem(RoleStyleItem roleItem, NFTItemInfo info)
    {
        var refIndex = itemInfos.FindIndex(x => x.sort < info.sort);
        if (refIndex >= 0)
        {
            int sibling = itemInfos[refIndex].item.transform.GetSiblingIndex();
            roleItem.transform.SetSiblingIndex(sibling);
            itemInfos.Insert(refIndex, info);
        }
        else
        {
            itemInfos.Add(info);
        }
    }

    private void OnSelectClick(RoleStyleItem item)
    {
        if (curItem == item)
        {
            return;
        }

        if (curItem != null)
        {
            curItem.SetSelectState(false);
        }
        curItem = item;
        curItem.SetSelectState(true);
        parentView.detailsBtn.gameObject.SetActive(seriesId > 0); //当前页不是All
        RefreshBannerBySelectItem(); //47开始支持单独为Icon配置Banner背景等的能力
    }

    public NFTItemInfo GetCurrentItemInfo()
    {
        return itemInfos.Find(x => x.item == curItem);
    }

    public void RefreshBannerBySelectItem()
    {
        //允许单独为Item配置背景等
        var cInfo = GetCurrentItemInfo();
        if (seriesId > 0 && cInfo != null)
        {
            RoleConfigDataManager.Inst.SetAvatarIconDynamic(bannerImg, cInfo.bannerUrl, parentView.sprite, SetBannerState);
            RoleConfigDataManager.Inst.SetAvatarIconDynamic(RoleClassifiyView.Ins.backgroundImage, cInfo.backgroundUrl, parentView.sprite);
            RoleConfigDataManager.Inst.SetAvatarIconDynamic(RoleClassifiyView.Ins.shadowImg, cInfo.shadowUrl, parentView.sprite);
            parentView.ChangeDetailsBtnStyle(cInfo.detailsType);
        }
    }

    public void SetBannerState(ImgLoadState state)
    {
        switch (state)
        {
            case ImgLoadState.Loading:
                bannerImg.sprite = parentView.sprite.GetSprite("banner_loading");
                break;
            case ImgLoadState.Failed:
                bannerImg.sprite = parentView.sprite.GetSprite("banner_loadingfail");
                break;
        }
        bannerloader.SetActive(state == ImgLoadState.Loading);
    }

    public void SetBannerVisiable(bool state)
    {
        bannerImg.gameObject.SetActive(state);
        var offset = iconViewRTF.offsetMax;
        offset.y = state ? -220 : -5;
        iconViewRTF.offsetMax = offset;
    }

    public void ClearSelectState()
    {
        if (curItem != null)
        {
            curItem.SetSelectState(false);
            curItem = null;
            parentView.detailsBtn.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        ClearSelectState();
    }
}

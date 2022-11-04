using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SavingData;
using UnityEngine.Events;
using Newtonsoft.Json;
using SuperScrollView;
using UnityEngine.UI;
using UnityEngine.U2D;
using DG.Tweening;
/// <summary>
/// Author:Meimei-LiMei
/// Description:DC列表基类（提供一些通用函数：请求、刷新等）
/// Date: 2022/9/20
/// </summary>
public class RoleDCBaseView : MonoBehaviour
{
    private DCHttpReqQuerry httpRequest = new DCHttpReqQuerry();
    private DCHttpReqQuerry httpNFTRequest = new DCHttpReqQuerry();
    public LoopGridView mLoopGridView;
    [HideInInspector]
    public List<RoleUGCIconData> allUgcClothesInfos = new List<RoleUGCIconData>();
    [HideInInspector]
    public TextureQueuedLoader textureBatchLoader;
    private bool isLock = true;
    private int pageSize = 32;
    private bool isEnd = false;
    // 保存所有Ugc衣服数据 key:mapId  value:ugc衣服数据
    private List<RoleStyleUgcItem> ugcItemList = new List<RoleStyleUgcItem>();
    public SpriteAtlas nftSprite;
    private Action onGetNFTFinish;
    [HideInInspector]
    public int nftCount;
    private RoleStyleUgcItem curSelectUGCItem;
    public Action<int> pgcItemSclect;
    public ClassifyType pgctype, ugctype;//pgc类型

    public virtual void OnSelectItemByID(string mapId, int pgcId)
    {
        int index = allUgcClothesInfos.FindIndex(x => (!string.IsNullOrEmpty(mapId) && x.mapId.Equals(mapId)) || x.pgcId.Equals(pgcId));
        if (index > -1)
        {
            var item = mLoopGridView.GetShownItemByItemIndex(index);
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
    /// <summary>
    /// 初始化所需参数
    /// </summary>
    /// <param name="pgcSelect">pgc按钮选中事件</param>
    /// <param name="ugcType"></param>
    public virtual void InitParams(Action<int> pgcAct, ClassifyType ugcType)
    {
        pgcItemSclect = pgcAct;
        this.ugctype = ugcType;
        this.pgctype = RoleConfigDataManager.Inst.GetPGCTypeByUGCType(ugctype);
    }

    /// <summary>
    /// 实例化Digital衣服列表
    /// </summary>
    public void InitDigitalViewList()
    {
        textureBatchLoader = TextureQueuedLoader.Create(maxLoaderNum: 3, maxMemoryCacheNum: 300, maxQueueSize: 30);
        mLoopGridView.OnPreLoadEvent = () => RefreshUgcClothesRes(UpdateResListSuccess, OnGetClothesResListFail);
        mLoopGridView.OnOverBottomEvent = () => RefreshUgcClothesRes(UpdateResListSuccess, OnGetClothesResListFail);
        InitUgcClothesRes(OnGetClothesResListSuccess, OnGetClothesResListFail);
    }

    private void UpdateHttpRequestArg(string cookie = "")
    {
        httpRequest.classifyType = (int)ugctype;
        httpRequest.itemType = (int)DCItemType.Clothes;
        httpRequest.listType = (int)DCUGCCloResType.Owned;
        httpRequest.toUid = GameManager.Inst.ugcUserInfo.uid;
        httpRequest.cookie = cookie;
    }

    private void InitUgcClothesRes(UnityAction<string> onSuccess, UnityAction<string> onFail)
    {
        allUgcClothesInfos.Clear();
        UpdateHttpRequestArg();
        HttpUtils.MakeHttpRequest("/ugcmap/userItemList", (int)HTTP_METHOD.GET,
            JsonConvert.SerializeObject(httpRequest),
            (content) => { onSuccess?.Invoke(content); },
            (error) => { onFail?.Invoke(error); });
    }

    private void UnLockRefresh()
    {
        isLock = true;
    }

    private void RefreshUgcClothesRes(UnityAction<string> onSuccess, UnityAction<string> onFail)
    {
        if (isLock && !isEnd)
        {
            isLock = false;
            Invoke("UnLockRefresh", 5);
            HttpUtils.MakeHttpRequest("/ugcmap/userItemList", (int)HTTP_METHOD.GET,
                JsonConvert.SerializeObject(httpRequest),
                (content) => { onSuccess?.Invoke(content); },
                (error) => { onFail?.Invoke(error); });
        }
    }

    private void OnGetClothesResListSuccess(string content)
    {
        //继续请求NFT资源
        SetInitNFTClothesInfos();
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data))
        {
            LoggerUtils.LogError("UGCClothes ResList Data is Null");
            return;
        }
        DCUGCClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<DCUGCClothesRepInfo>(repData.data);
        isEnd = resourceInfo.isEnd == 1;
        if (resourceInfo.itemList == null)
        {
            LoggerUtils.Log("UGCClothes resourceInfo.mapInfos is Null");
            return;
        }
        UpdateHttpRequestArg(resourceInfo.cookie);
        UpdateUgcClothesInfos(resourceInfo.itemList);
    }
    public virtual void SetInitNFTClothesInfos()
    {
        InitNFTClothesInfos(() =>
      {
          mLoopGridView.InitGridView(allUgcClothesInfos.Count, OnGetItemByRowColumn);
      });
    }

    public void InitNFTClothesInfos(Action onFinish)
    {
        onGetNFTFinish = onFinish;
        httpNFTRequest.classifyType = (int)pgctype;
        httpNFTRequest.itemType = (int)DCItemType.Clothes;
        httpNFTRequest.listType = (int)DCUGCCloResType.Owned;
        httpNFTRequest.toUid = GameManager.Inst.ugcUserInfo.uid;
        httpNFTRequest.cookie = "";
        HttpUtils.MakeHttpRequest("/ugcmap/userItemList", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpNFTRequest),
            OnGetNFTInfoSuccess, OnGetClothesResListFail);
    }

    private void OnGetNFTInfoSuccess(string content)
    {
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data))
        {
            LoggerUtils.LogError("OnGetNFTList : repData.data == null");
            return;
        }
        DCUGCClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<DCUGCClothesRepInfo>(repData.data);
        httpNFTRequest.cookie = resourceInfo.cookie;
        if (resourceInfo.itemList != null)
        {
            UpdateNftClothesInfos(resourceInfo.itemList);
        }
        if (resourceInfo.isEnd != 1)
        {
            HttpUtils.MakeHttpRequest("/ugcmap/userItemList", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpNFTRequest),
                OnGetNFTInfoSuccess, OnGetClothesResListFail);
        }
        else
        {
            onGetNFTFinish?.Invoke();
        }
    }

    private void UpdateUgcClothesInfos(List<DCUGCClothesInfo> mapInfos)
    {
        for (var i = 0; i < mapInfos.Count; i++)
        {
            RoleUGCIconData data = new RoleUGCIconData()
            {
                classifyType = (int)ugctype,
                coverUrl = mapInfos[i].mapCover,
                jsonUrl = mapInfos[i].clothesJson,
                zipUrl = mapInfos[i].clothesUrl,
                templateId = mapInfos[i].templateId,
                mapId = mapInfos[i].mapId,
                isNew = mapInfos[i].mapStatus.isNew,
                isFavorites = mapInfos[i].mapStatus.isFavorites,
                grading = (int)RoleResGrading.DC,
                origin = mapInfos[i].dcInfo.nftType == (int)NftType.Airdrop ? (int)RoleOriginType.Airdrop : (int)RoleOriginType.Normal,
            };
            allUgcClothesInfos.Add(data);
        }
    }

    private void UpdateNftClothesInfos(List<DCUGCClothesInfo> mapInfos)
    {
        for (var i = 0; i < mapInfos.Count; i++)
        {
            if (mapInfos[i].dcPgcInfo != null && mapInfos[i].dcPgcInfo.hasCount > 0)
            {
                RoleUGCIconData data = new RoleUGCIconData()
                {
                    classifyType = (int)pgctype,
                    mapId = mapInfos[i].mapId,
                    isNew = mapInfos[i].mapStatus.isNew,
                    isFavorites = mapInfos[i].mapStatus.isFavorites,
                    pgcId = mapInfos[i].dcPgcInfo.pgcId,
                    grading = (int)RoleResGrading.DC,
                    origin = mapInfos[i].dcInfo.nftType == (int)NftType.Airdrop ? (int)RoleOriginType.Airdrop : (int)RoleOriginType.Normal,
                };
                nftCount++;
                allUgcClothesInfos.Insert(0, data);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    public virtual void UpdateResListSuccess(string content)
    {
        isLock = true;
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);

        if (string.IsNullOrEmpty(repData.data))
        {
            LoggerUtils.LogError("UGCClothes ResList Data is Null");
            return;
        }
        DCUGCClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<DCUGCClothesRepInfo>(repData.data);
        isEnd = resourceInfo.isEnd == 1;
        if (resourceInfo.itemList == null)
        {
            LoggerUtils.Log("UGCClothes resourceInfo.mapInfos is Null");
            return;
        }
        UpdateHttpRequestArg(resourceInfo.cookie);
        UpdateUgcClothesInfos(resourceInfo.itemList);
    }
    /// <summary>
    /// 刷新Item（!!!每个子类单独实现具体逻辑）
    /// </summary>
    /// <param name="gridView"></param>
    /// <param name="itemIndex"></param>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="sdir"></param>
    /// <returns></returns>
    public virtual LoopGridViewItem OnGetItemByRowColumn(LoopGridView gridView, int itemIndex, int row, int column, ScrollDirection sdir)
    {
        return null;
    }

    private void OnGetClothesResListFail(string error)
    {
        isLock = true;
    }

    public virtual void OnUgcItemSelect(RoleStyleUgcItem ugcItem)
    {
        OnItemSelectState(ugcItem);
    }

    public virtual void OnNftItemSelect(RoleStyleUgcItem ugcItem)
    {
        OnItemSelectState(ugcItem);
        pgcItemSclect?.Invoke(ugcItem.rcData.pgcId);
    }

    public RoleStyleUgcItem GetPgcItem(int id)
    {
        return ugcItemList.Find(x => x.rcData.pgcId.Equals(id));
    }

    public RoleStyleUgcItem GetUgcItem(string mapId)
    {
        return ugcItemList.Find(x => x.rcData.mapId.Equals(mapId));
    }

    public void UpdateItemCollect(UGCClothesResType type, int pgcId, string mapId, bool isCollect)
    {
        RoleStyleUgcItem item = null;
        switch (type)
        {
            case UGCClothesResType.PGC:
                item = ugcItemList.Find(x => x.rcData.pgcId.Equals(pgcId));
                break;
            case UGCClothesResType.UGC:
                item = ugcItemList.Find(x => x.rcData.mapId.Equals(mapId));
                break;
        }

        if (item != null)
        {
            item.UpdateItemCollect(isCollect);
        }
    }

    public void AddItemList(UGCClothesResType type, RoleStyleUgcItem item)
    {
        RoleStyleUgcItem oItem = null;
        switch (type)
        {
            case UGCClothesResType.PGC:
                if (item.rcData.pgcId == default(int))
                {
                    return;
                }
                oItem = ugcItemList.Find(x => x.rcData.pgcId.Equals(item.rcData.pgcId));
                break;
            case UGCClothesResType.UGC:
                if (string.IsNullOrEmpty(item.rcData.mapId))
                {
                    return;
                }
                oItem = ugcItemList.Find(x => x.rcData.mapId.Equals(item.rcData.mapId));
                break;
        }
        if (oItem == null)
        {
            ugcItemList.Add(item);
        }
    }

    public void OnItemSelectState(RoleStyleUgcItem ugcItem)
    {
        if (curSelectUGCItem != null)
        {
            curSelectUGCItem.SetSelectState(false);
        }
        curSelectUGCItem = ugcItem;
        curSelectUGCItem.SetSelectState(true);
    }
    public void SetUgcItemUnSelect()
    {
        if (curSelectUGCItem != null)
        {
            curSelectUGCItem.SetSelectState(false);
            curSelectUGCItem = null;
        }
    }
}

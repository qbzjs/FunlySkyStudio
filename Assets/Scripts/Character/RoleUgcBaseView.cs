using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using SavingData;
using SuperScrollView;
using UnityEngine.Events;

public struct UpDateClothList
{
    public int resType;
    public int needRefresh;
}
/// <summary>
/// Author:Meimei-LiMei
/// Description:UGC列表基类（提供一些通用函数：请求、刷新等）
/// Date: 2022/9/20 13:36:51
/// </summary>
public class RoleUgcBaseView : MonoBehaviour
{
    public Texture StoreTexture;
    public HttpReqQuerry httpRequest = new HttpReqQuerry();
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
    private RoleStyleUgcItem curSelectUGCItem;
    public ClassifyType type;//分签页类型，用于UI交互(通过DataSubType获得)

    public virtual void OnSelectItemByID(string mapId, int pgcId = 0)
    {
        int index = allUgcClothesInfos.FindIndex(x => x.mapId.Equals(mapId));
        if (index > -1)
        {
            var item = mLoopGridView.GetShownItemByItemIndex(index);
            if (item != null)
            {
                var itemScript = item.GetComponent<RoleStyleUgcItem>();
                OnItemSelectState(itemScript);
            }
        }
    }
    /// <summary>
    /// 实例化购买列表
    /// </summary>
    public void InitExperienceList(ClassifyType classifyType)
    {
        LoggerUtils.Log("InitExperienceList");
        this.type = classifyType;
        textureBatchLoader = TextureQueuedLoader.Create(maxLoaderNum: 3, maxMemoryCacheNum: 300, maxQueueSize: 30);
        mLoopGridView.OnPreLoadEvent = () => RefreshUgcRes(UpdateResListSuccess, OnGetClothesResListFail);
        mLoopGridView.OnOverBottomEvent = () => RefreshUgcRes(UpdateResListSuccess, OnGetClothesResListFail);
        InitUgcRes(OnInitClothesResListSuccess, OnGetClothesResListFail);
    }

    public virtual void UpdateHttpRequestArg(int pageSize, string cookie = "")
    {
        httpRequest.dataType = (int)Data_Type.Cloth;
        httpRequest.pageSize = pageSize.ToString();
        httpRequest.cookie = cookie;
    }

    private void InitUgcRes(UnityAction<string> onSuccess, UnityAction<string> onFail)
    {
        allUgcClothesInfos.Clear();
        if (isLock && !isEnd)
        {
            isLock = false;
            Invoke("UnLockRefresh", 5);
            UpdateHttpRequestArg(pageSize - 1);
            HttpUtils.MakeHttpRequest("/ugcmap/experienceList", (int)HTTP_METHOD.GET,
                JsonConvert.SerializeObject(httpRequest),
                (content) => { onSuccess?.Invoke(content); },
                (error) => { onFail?.Invoke(error); });
        }
    }

    private void UnLockRefresh()
    {
        isLock = true;
    }

    private void RefreshUgcRes(UnityAction<string> onSuccess, UnityAction<string> onFail)
    {
        if (isLock && !isEnd)
        {
            isLock = false;
            Invoke("UnLockRefresh", 5);
            HttpUtils.MakeHttpRequest("/ugcmap/experienceList", (int)HTTP_METHOD.GET,
                JsonConvert.SerializeObject(httpRequest),
                (content) => { onSuccess?.Invoke(content); },
                (error) => { onFail?.Invoke(error); });
        }
    }

    private void OnGetUgcResListUpdate(string content)
    {
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data))
        {
            LoggerUtils.LogError("UGCClothes ResList Data is Null");
            return;
        }
        ResourceInfo resourceInfo = JsonConvert.DeserializeObject<ResourceInfo>(repData.data);
        isEnd = resourceInfo.isEnd == 1;
        if (resourceInfo.mapInfos == null)
        {
            LoggerUtils.Log("UGCClothes resourceInfo.mapInfos is Null");
            return;
        }
        UpdateHttpRequestArg(pageSize, resourceInfo.cookie);
        UpdateUgcClothesInfos(resourceInfo.mapInfos);
    }

    public virtual void OnInitClothesResListSuccess(string content)
    {
        isLock = true;
        OnGetUgcResListUpdate(content);

    }

    public virtual void OnGetClothesResListSuccess(string content)
    {
        isLock = true;
        OnGetUgcResListUpdate(content);
    }

    private void UpdateUgcClothesInfos(List<MapInfo> mapInfos)
    {
        for (var i = 0; i < mapInfos.Count; i++)
        {
            if (mapInfos[i].isDC > 0)
            {
                continue;
            }
            RoleUGCIconData data = new RoleUGCIconData()
            {
                classifyType = (int)type,
                coverUrl = mapInfos[i].mapCover,
                jsonUrl = mapInfos[i].clothesJson,
                zipUrl = mapInfos[i].clothesUrl,
                templateId = mapInfos[i].templateId,
                mapId = mapInfos[i].mapId,
                isNew = mapInfos[i].mapStatus.isNew,
                isFavorites = mapInfos[i].mapStatus.isFavorites,
            };
            allUgcClothesInfos.Add(data);
        }
    }

    private void UpdateResListSuccess(string content)
    {
        isLock = true;
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);

        if (string.IsNullOrEmpty(repData.data))
        {
            LoggerUtils.LogError("UGCClothes ResList Data is Null");
            return;
        }
        ResourceInfo resourceInfo = JsonConvert.DeserializeObject<ResourceInfo>(repData.data);
        isEnd = resourceInfo.isEnd == 1;
        if (resourceInfo.mapInfos == null)
        {
            LoggerUtils.Log("UGCClothes resourceInfo.mapInfos is Null");
            return;
        }
        UpdateHttpRequestArg(pageSize, resourceInfo.cookie);
        UpdateUgcClothesInfos(resourceInfo.mapInfos);
        mLoopGridView.SetListItemCount(allUgcClothesInfos.Count, false);
    }
    /// <summary>
    /// 刷新Item（每个子类单独实现具体逻辑）
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

    private void GetClothListFromStore()
    {
        InitUgcRes(OnGetClothesResListSuccess, OnGetClothesResListFail);
        ReqCleanRedDot();
    }
    public virtual void ReqCleanRedDot()
    {

    }

    public void OnUpdateClothList(string content)
    {
        UpDateClothList upDateClothList = JsonConvert.DeserializeObject<UpDateClothList>(content);
        if (upDateClothList.needRefresh == 1)
        {
            UpdateHttpRequestArg(pageSize - 1);
            isEnd = false;
            GetClothListFromStore();
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

    public virtual void OnUgcItemSelect(RoleStyleUgcItem ugcItem)
    {
        OnItemSelectState(ugcItem);
    }

    public void UpdateUgcDataCollect(string mapId, bool isCollect)
    {
        var item = ugcItemList.Find(x => x.rcData.mapId.Equals(mapId));
        if (item != null)
        {
            item.UpdateItemCollect(isCollect);
        }
    }

    public void AddItemList(RoleStyleUgcItem item)
    {
        if (string.IsNullOrEmpty(item.rcData.mapId))
        {
            return;
        }
        var oItem = ugcItemList.Find(x => x.rcData.mapId.Equals(item.rcData.mapId));
        if (oItem == null)
        {
            ugcItemList.Add(item);
        }
    }
    public RoleStyleUgcItem GetUgcItem(string mapId)
    {
        return ugcItemList.Find(x => x.rcData.mapId.Equals(mapId));
    }
}

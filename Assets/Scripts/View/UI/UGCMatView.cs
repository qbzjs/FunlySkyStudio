using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SavingData;
using SuperScrollView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UGCMatView : MonoBehaviour
{
  
    public List<UGCMatData> ugcMatDatas = new List<UGCMatData>();
    public List<MapInfo> ugcMatInfos = new List<MapInfo>();
    private HttpReqQuerry httpRequest = new HttpReqQuerry();
    protected List<BaseMaterialItem> ugcItemList = new List<BaseMaterialItem>();
    private bool isLock = true;
    private int pageSize = 32;
    private bool isEnd = false;
    private Action<UGCMatData> btnClickAction;
    [HideInInspector]
    public TextureQueuedLoader textureBatchLoader;
    public LoopGridView mLoopGridView;
    private BaseMaterialItem curSelectUGCItem;
    private bool isInit;
    /// <summary>
    /// 实例化材质列表
    /// </summary>
    public void InitExperienceList(Action<UGCMatData> btnClickAction)
    {
        
        LoggerUtils.Log("InitExperienceList   "+ isInit);
        if (isInit)
        {
            return;
        }
        isInit = true;
        this.btnClickAction = btnClickAction;
        textureBatchLoader = TextureQueuedLoader.Create(maxLoaderNum: 3, maxMemoryCacheNum: 300, maxQueueSize: 51);
        mLoopGridView.OnPreLoadEvent = () => RefreshUgcRes(UpdateResListSuccess, OnGetResListFail);
        mLoopGridView.OnOverBottomEvent = () => RefreshUgcRes(UpdateResListSuccess, OnGetResListFail);
        InitUgcRes(OnInitResListSuccess, OnGetResListFail);
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
        mLoopGridView.SetListItemCount(ugcMatDatas.Count, false);
    }
    private void InitUgcRes(UnityAction<string> onSuccess, UnityAction<string> onFail)
    {
        ugcMatDatas.Clear();
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
    private void OnGetResListFail(string error)
    {
        isLock = true;
    }
    public virtual void UpdateHttpRequestArg(int pageSize, string cookie = "")
    {
        httpRequest.dataType = (int)Data_Type.Material;
        httpRequest.pageSize = pageSize.ToString();
        httpRequest.cookie = cookie;
    }
    public virtual void OnInitResListSuccess(string content)
    {
        isLock = true;
        OnGetUgcResListUpdate(content);
      
        mLoopGridView.InitGridView(ugcMatDatas.Count + 1, OnGetItemByRowColumn);
        
    }
    public LoopGridViewItem OnGetItemByRowColumn(LoopGridView gridView, int itemIndex, int row, int column, ScrollDirection sdir)
    {
     
        LoopGridViewItem item = gridView.GetItemByPool();
        item.gameObject.name = itemIndex.ToString();
        var ugcItem = item.GetComponent<BaseMaterialItem>();
        //first store
        if (itemIndex == 0)
        {
            ugcItem.SetStore(OnMatStoreBack);

            return item;
        }
        var itemData = ugcMatDatas[itemIndex - 1];
        
        if (itemData == null || itemData.coverUrl == null)
        {
            return item;
        }
        ugcItem.Init(btnClickAction, itemData);
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
                    var itemScript = item.GetComponent<BaseMaterialItem>();
                    itemScript.SetItemTexture(result.m_Texture);
                }
            }
        };
       
        var tex = textureBatchLoader.GetImageByUrl(itemData.coverUrl, itemIndex, sdir, loadIfNotFound: true);
        if (tex)
        {
            ugcItem.SetItemTexture(tex);
        }
        if (!string.IsNullOrEmpty(curMapId)&& itemData.mapId.Equals(curMapId))
        {
            OnItemSelectState(ugcItem);
        }

        return item;
    }
    public void AddItemList(BaseMaterialItem item)
    {
        if (string.IsNullOrEmpty(item.uData.mapId))
        {
            return;
        }
        var oItem = ugcItemList.Find(x => x.uData.mapId.Equals(item.uData.mapId));
        if (oItem == null)
        {
            ugcItemList.Add(item);
        }
    }
    public BaseMaterialItem GetUgcItem(string mapId)
    {
        return ugcItemList.Find(x => x.uData.mapId.Equals(mapId));
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
    private void UpdateUgcClothesInfos(List<MapInfo> mapInfos)
    {
      
        for (var i = 0; i < mapInfos.Count; i++)
        {
            UGCMatData uData = new UGCMatData();
            uData.mapId = mapInfos[i].mapId;
            uData.coverUrl = mapInfos[i].mapCover;
            uData.mapInfo = mapInfos[i];
            uData.matUrl = mapInfos[i].dataUrl;
            ugcMatDatas.Add(uData);
        }
    }
    private void UnLockRefresh()
    {
        isLock = true;
    }
    public void SetAllItemHide()
    {
        if (curSelectUGCItem != null)
        {
            curSelectUGCItem.SetSelectState(false);
        }
    }
    public void OnItemSelectState(BaseMaterialItem ugcItem)
    {
        if (curSelectUGCItem != null)
        {
            curSelectUGCItem.SetSelectState(false);
        }
        curSelectUGCItem = ugcItem;
        curSelectUGCItem.SetSelectState(true);
    }
    private string curMapId;
    public void SetItemShow(string mapid)
    {
        curMapId = mapid;
        var item = GetUgcItem(mapid);
        if (item!=null)
        {
            curSelectUGCItem = item;
        }
        curSelectUGCItem.SetSelectState(true);
    }

    public void OnMatStoreBack(string content)
    {
        TipPanel.ShowToast("OnMatStoreBack");
        Debug.Log("OnMatStoreBack   " + content);
        OnUpdateMatList(content);
    }
    public void OnUpdateMatList(string content)
    {
        UpdateHttpRequestArg(pageSize - 1);
        isEnd = false;
        GetClothListFromStore();
    }
    private void GetClothListFromStore()
    {
        InitUgcRes(OnGetResListSuccess, OnGetResListFail);
    }
    public void OnGetResListSuccess(string content)
    {
        isLock = true;
        OnGetUgcResListUpdate(content);
        mLoopGridView.RefreshGridView(ugcMatDatas.Count + 1);
    }
}

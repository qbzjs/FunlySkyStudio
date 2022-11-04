/// <summary>
/// Author:Mingo-LiZongMing
/// Description:素材Item的资源管理器
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Networking;

public enum OpenType
{
    UGC,
    DC
}

public class ResBagManager : InstMonoBehaviour<ResBagManager>
{
    public List<GameObject> itemList = new List<GameObject>();

    public List<MapInfo> mapInfos = new List<MapInfo>();

    public List<MapInfo> srchMapInfos = new List<MapInfo>();

    public List<string> AlreadyShowResItem = new List<string>();

    public Dictionary<string, ResItemData> resItemDataDic = new Dictionary<string, ResItemData>();

    public int isEnd = 1;

    private HttpReqQuerry httpReqQuerry = new HttpReqQuerry();

    private SearchRepQuerry searchRepQuerry = new SearchRepQuerry();

    private bool canRefreshResList = true;

    private Coroutine iEnum;

    private Dictionary<string, MapInfo> releaseResDataDic = new Dictionary<string, MapInfo>();

    private Action<MapInfo> outItemClick; //外部调用素材背包，设置特殊的点击事件

    public void OpenResPage(Action<MapInfo> itemClick,OpenType type = OpenType.UGC)
    {
        if(ParsePropWithTipsManager.Inst.GetIsLoading())
        {
            TipPanel.ShowToast("Please wait for the last resource to finish loading.");
            return;
        }
        outItemClick = itemClick;
        if (outItemClick != null)
        {
#if UNITY_EDITOR
            if (type == OpenType.DC)
            {
                OnOutResDCCreateCallBack(File.ReadAllText(Application.streamingAssetsPath + "/111.json"));
            }
            else
            {
                OnOutResCreateCallBack(File.ReadAllText(Application.streamingAssetsPath + "/111.json"));
            }
#else
            if (type == OpenType.DC)
            {
                MobileInterface.Instance.AddClientRespose(MobileInterface.openDcResPage, OnOutResDCCreateCallBack);
                MobileInterface.Instance.OpenDcResPage(); 
            }
            else
            {
                MobileInterface.Instance.AddClientRespose(MobileInterface.openUgcResPage, OnOutResCreateCallBack);
                MobileInterface.Instance.OpenUgcResPage(); 
            }           
#endif
        }
        else
        {
#if UNITY_EDITOR
            OnResCreateCallBack(File.ReadAllText(Application.streamingAssetsPath + "/111.json"));
#else
            if (type == OpenType.DC)
            {
                MobileInterface.Instance.AddClientRespose(MobileInterface.openDcResPage, OnResCreateCallBack);
                MobileInterface.Instance.OpenDcResPage(); 
            }
            else
            {
                MobileInterface.Instance.AddClientRespose(MobileInterface.openUgcResPage, OnResCreateCallBack);
                MobileInterface.Instance.OpenUgcResPage();
            }
#endif
        }
    }

    public void OnOutResCreateCallBack(string content)
    {
        MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(content);
        //如果是外部的调用到pgc素材暂时屏蔽
        if (mapInfo.IsScenePgc()) return; 
        ParsePropWithTipsManager.Inst.InitTipGameObject();
        iEnum = StartCoroutine(LoadSingleTexture(mapInfo));
    }

    public void OnOutResDCCreateCallBack(string content)
    {
        MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(content);
        ParsePropWithTipsManager.Inst.InitTipGameObject();
        iEnum = StartCoroutine(LoadSingleTexture(mapInfo,OpenType.DC));
    }
    
    
    public void OnResCreateCallBack(string content)
    {
        MapInfo mapInfo = JsonConvert.DeserializeObject<MapInfo>(content);
        ParsePropWithTipsManager.Inst.InitTipGameObject();
        if (mapInfo.IsScenePgc())
        {
            SceneBuilder.Inst.CreateDCPGC(mapInfo);
        }
        else
        {
            
            iEnum = StartCoroutine(LoadSingleContent(mapInfo));
        }
    }

    public void StopDownload()
    {
        if(iEnum != null)StopCoroutine(iEnum);
    }

    public void OnResCreate(MapInfo mapInfo)
    {
        if(!ParsePropWithTipsManager.Inst.AllowIsLoading())
        {
            return;
        }
        
        if (resItemDataDic.ContainsKey(mapInfo.mapId) && resItemDataDic[mapInfo.mapId].mapJsonContent != null)
        {
            SetDcMapInfo(mapInfo);
            //外部调用素材背包要进行的操作
            if (outItemClick != null)
            {
                outItemClick.Invoke(mapInfo);
                return;
            }

            var rId = resItemDataDic[mapInfo.mapId].mapInfo.mapId;
            var pos = CameraUtils.Inst.GetCreatePosition();
            // 将离线数据加入当前数据缓存
            UGCBehaviorManager.Inst.AddOfflineRenderData(resItemDataDic[mapInfo.mapId].mapInfo.renderList);
            var nBehav = SceneBuilder.Inst.ParsePropAndBuild(resItemDataDic[mapInfo.mapId].mapJsonContent, pos, rId);
            if(PackPanel.Instance != null && PackPanel.Instance.gameObject.activeSelf)
            {
                MessageHelper.Broadcast(MessageName.OpenPackPanel, true);
                return;
            }
            EditModeController.SetSelect?.Invoke(nBehav.entity);
        }
    }

  

 
    
    private void SetDcMapInfo(MapInfo mapInfo)
    {
        if (mapInfo.isDC == (int)IsDC.True&&mapInfo.dcInfo!=null)
        {
            NodeData data = JsonConvert.DeserializeObject<NodeData>(resItemDataDic[mapInfo.mapId].mapJsonContent);
            if (data != null)
            {
                DcData dcData = new DcData
                {
                    isDc = mapInfo.isDC,
                    id = mapInfo.dcInfo.itemId,
                    address = mapInfo.dcInfo.walletAddress,
                    actId = mapInfo.dcInfo.budActId
                };
                BehaviorKV kV = new BehaviorKV()
                {
                    k = (int)BehaviorKey.DC,
                    v = JsonConvert.SerializeObject(dcData)
                };
            
                data.attr.Add(kV);
                resItemDataDic[mapInfo.mapId] = new ResItemData()
                {
                    mapJsonContent = JsonConvert.SerializeObject(data),
                    mapInfo = resItemDataDic[mapInfo.mapId].mapInfo,
                    mapCover = resItemDataDic[mapInfo.mapId].mapCover
                };
            }
        }
    }
    private string resJson;
    public void RefreshResList()
    {
        if (!canRefreshResList)
            return;
        canRefreshResList = false;
        if (ResStorePanel.Instance)
        {
            ResStorePanel.Instance.LoadingPanel.SetActive(true);
        }
        httpReqQuerry.dataType = 1;
        httpReqQuerry.cookie = "";
        HttpUtils.MakeHttpRequest("/ugcmap/experienceList", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpReqQuerry), OnGetResListSuccess, OnGetResListFail);
    }

    public void UpdateSelfResList()
    {
#if UNITY_EDITOR
        canRefreshResList = true;
#endif
        if (!canRefreshResList)
            return;
        canRefreshResList = false;
        if (ResStorePanel.Instance)
        {
            ResStorePanel.Instance.LoadingPanel.SetActive(true);
        }
        HttpReqQuerry TempQuerry = new HttpReqQuerry();
        TempQuerry.dataType = 1;
        TempQuerry.cookie = "";
        HttpUtils.MakeHttpRequest("/ugcmap/experienceList", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(TempQuerry), UpdateSelfResListSuccess, OnGetResListFail);
    }

    public void GetNextPageResList()
    {
        if (!canRefreshResList)
            return;
        canRefreshResList = false;
        ResStorePanel.Instance.LoadingPanel.SetActive(true);
        HttpReqQuerry TempQuerry = new HttpReqQuerry();
        TempQuerry.dataType = 1;
        TempQuerry.cookie = httpReqQuerry.cookie;
        HttpUtils.MakeHttpRequest("/ugcmap/experienceList", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(TempQuerry), GetNextPageSuccess, OnGetResListFail);
    }

    public void RefreshSearchResList(string str)
    {
        if (!canRefreshResList)
            return;
        canRefreshResList = false;
        ResStorePanel.Instance.LoadingPanel.SetActive(true);
        searchRepQuerry.searchWord = str;
        searchRepQuerry.cookie = "";
        HttpUtils.MakeHttpRequest("/search/propInventory", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(searchRepQuerry), OnGetSearchResSuccess, OnGetSearchResFail);
    }

    public void GetNextPageSearchResList()
    {
        if (!canRefreshResList)
            return;
        canRefreshResList = false;
        ResStorePanel.Instance.LoadingPanel.SetActive(true);
        HttpUtils.MakeHttpRequest("/search/propInventory", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(searchRepQuerry), GetNextSearchSuccess, OnGetSearchResFail);
    }

    public void GetNextPageSuccess(string content)
    {
        LoggerUtils.Log("GetNextPageSuccess");
        canRefreshResList = true;
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data) || repData.data == "null")
        {
            return;
        }
        ResourceInfo resourceInfo = JsonConvert.DeserializeObject<ResourceInfo>(repData.data);
        isEnd = resourceInfo.isEnd;
        httpReqQuerry.cookie = resourceInfo.cookie;
        resourceInfo.mapInfos = ClearEmptyMapInfo(resourceInfo.mapInfos);
        List<MapInfo> tempMapInfos = new List<MapInfo>();
        foreach (var mapInfo in resourceInfo.mapInfos)
        {
            var curMapInfo = mapInfos.Find(x => x.mapId == mapInfo.mapId);
            if (curMapInfo == null)
            {
                tempMapInfos.Add(mapInfo);
            }
            else
            {
                break;
            }
        }
        mapInfos = mapInfos.Concat(tempMapInfos).ToList<MapInfo>();
        StopBothCoroute();
        CheckRemoveCache(tempMapInfos.Count);
        ActiveLoadAllCoroute(true);
        //ResStorePanel.Instance.RefreshContentSize();
        //ResStorePanel.Instance.CheckShowOrHide();
        ResStorePanel.Instance.IsHadProp(true);
        ResStorePanel.Instance.LoadingPanel.SetActive(false);
    }

    public void UpdateSelfResListSuccess(string content)
    {
        LoggerUtils.Log("UpdateSelfResListSuccess");
        canRefreshResList = true;
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data) || repData.data == "null")
        {
            return;
        }
        ResourceInfo resourceInfo = JsonConvert.DeserializeObject<ResourceInfo>(repData.data);
        isEnd = resourceInfo.isEnd;
        httpReqQuerry.cookie = resourceInfo.cookie;
        List<MapInfo> tempMapInfos = new List<MapInfo>();
        if (resourceInfo.mapInfos != null)
        {
            foreach (var mapInfo in resourceInfo.mapInfos)
            {
                var curMapInfo = mapInfos.Find(x => x.mapId == mapInfo.mapId);
                if (curMapInfo == null)
                {
                    tempMapInfos.Add(mapInfo);
                }
                else
                {
                    break;
                }
            }
        }
        mapInfos = tempMapInfos.Concat(mapInfos).ToList<MapInfo>();

        ResStorePanel.Instance.GoToContentTop();
        ResStorePanel.Instance.IsHadProp(true);
        StopBothCoroute();
        CheckRemoveCache(tempMapInfos.Count);
        ActiveLoadAllCoroute(true);
        //ResStorePanel.Instance.RefreshContentSize();
        //ResStorePanel.Instance.CheckShowOrHide();
    }

    public void OnGetResListSuccess(string content)
    {
        canRefreshResList = true;
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        ResourceInfo resourceInfo = JsonConvert.DeserializeObject<ResourceInfo>(repData.data);

        if (string.IsNullOrEmpty(repData.data) || resourceInfo.mapInfos == null)
        {
            ResStorePanel.Instance.IsHadProp(false);
        }
        else
        {
            isEnd = resourceInfo.isEnd;
            httpReqQuerry.cookie = resourceInfo.cookie;
            mapInfos.Clear();
            resourceInfo.mapInfos = ClearEmptyMapInfo(resourceInfo.mapInfos);
            mapInfos = mapInfos.Concat(resourceInfo.mapInfos).ToList<MapInfo>();
            LoggerUtils.Log("mapInfos Count = " + mapInfos.Count);
            if (resourceInfo.mapInfos == null || resourceInfo.mapInfos.Count <= 0)
            {
                return;
            }
            ResStorePanel.Instance.GoToContentTop();
            ResStorePanel.Instance.IsHadProp(true);
            StopBothCoroute();
            CheckRemoveCache(mapInfos.Count);
            ActiveLoadAllCoroute(true);
            //ResStorePanel.Instance.RefreshContentSize();
            //ResStorePanel.Instance.CheckShowOrHide();
        }
        ResStorePanel.Instance.LoadingPanel.SetActive(false);
    }

    public void GetNextSearchSuccess(string content)
    {
        LoggerUtils.Log("GetNextSearchSuccess");
        canRefreshResList = true;
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data) || repData.data == "null")
        {
            return;
        }
        ResourceInfo resourceInfo = JsonConvert.DeserializeObject<ResourceInfo>(repData.data);
        isEnd = resourceInfo.isEnd;
        searchRepQuerry.cookie = resourceInfo.cookie;
        List<MapInfo> tempMapInfos = new List<MapInfo>();
        foreach (var mapInfo in resourceInfo.mapInfos)
        {
            var curMapInfo = srchMapInfos.Find(x => x.mapId == mapInfo.mapId);
            if (curMapInfo == null)
            {
                tempMapInfos.Add(mapInfo);
            }
            else
            {
                break;
            }
        }
        srchMapInfos = srchMapInfos.Concat(tempMapInfos).ToList<MapInfo>();
        StopBothCoroute();
        CheckRemoveCache(tempMapInfos.Count);
        ActiveLoadSrchCoroute(true);
        ResStorePanel.Instance.IsHadProp(true);
    }

    public void OnGetSearchResSuccess(string content)
    {
        canRefreshResList = true;
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        ResourceInfo resourceInfo = JsonConvert.DeserializeObject<ResourceInfo>(repData.data);

        if (string.IsNullOrEmpty(repData.data) || resourceInfo.mapInfos == null)
        {
            ResStorePanel.Instance.IsHadProp(false);
        }
        else
        {
            isEnd = resourceInfo.isEnd;
            searchRepQuerry.cookie = resourceInfo.cookie;
            srchMapInfos.Clear();
            srchMapInfos = srchMapInfos.Concat(resourceInfo.mapInfos).ToList<MapInfo>();
            LoggerUtils.Log("searchMapInfos Count = " + srchMapInfos.Count);
            if (resourceInfo.mapInfos == null || resourceInfo.mapInfos.Count <= 0)
            {
                return;
            }

            ResStorePanel.Instance.GoToContentTop();
            ResStorePanel.Instance.IsHadProp(true);
            StopBothCoroute();
            CheckRemoveCache(srchMapInfos.Count);
            ActiveLoadSrchCoroute(true);
        }
        ResStorePanel.Instance.LoadingPanel.SetActive(false);
    }

    public void OnGetResListFail(string content)
    {
        LoggerUtils.Log("OnGetResListFail");
        canRefreshResList = true;
        ResStorePanel.Instance.LoadingPanel.SetActive(false);
    }

    public void OnGetSearchResFail(string content)
    {
        LoggerUtils.Log("OnGetSearchResFail");
        canRefreshResList = true;
        ResStorePanel.Instance.LoadingPanel.SetActive(false);
    }

    public void RefreshMapInfos()
    {
        List<MapInfo> tempMapInfos = new List<MapInfo>();

        for (int i = 12; i < 300; i++)
        {
            MapInfo testMapInfo = new MapInfo();
            testMapInfo.mapId = i.ToString();
            testMapInfo.mapJson = "https://buddy-app-bucket.s3.us-west-1.amazonaws.com/UgcJson/1449969322318503936_1634915725.json";
            testMapInfo.mapCover = "https://buddy-app-bucket.s3.us-west-1.amazonaws.com/UgcImage/1447519844756664320_1634205651.jpg";
            ResItemData resItemData = new ResItemData();
            resItemData.mapInfo = testMapInfo;
            tempMapInfos.Add(testMapInfo);
        }
        mapInfos = mapInfos.Concat(tempMapInfos).ToList<MapInfo>();
        ResStorePanel.Instance.IsHadProp(true);
        ResStorePanel.Instance.RefreshContentSize();
        ResStorePanel.Instance.CheckShowOrHide();
    }

    public GameObject GetItem()
    {
        GameObject item = null;
        if (itemList == null)
        {
            itemList = new List<GameObject>();
        }
        if (itemList.Count > 0)
        {
            item = itemList[0];
            itemList.RemoveAt(0);
        }
        else
        {
            var ItemPrefab = ResManager.Inst.LoadResNoCache<GameObject>("Prefabs/UI/Panel/ResItem");
            item = GameObject.Instantiate(ItemPrefab);
        }
        item.SetActive(true);
        return item;
    }

    public void PushItem(GameObject item)
    {
        var itemCom = item.GetComponent<ResBagItem>();
        if (itemCom != null)
        {
            itemCom.CoverImg.texture = null;
        }
        item.SetActive(false);
        itemList.Add(item);
    }

    public IEnumerator LoadTexture(string url, Action<Texture> onSuccess, Action<string> onFailure)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }
        UnityWebRequest www = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        www.downloadHandler = texDl;
        www.timeout = 45;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log("LoadSpriteError" + www.error);
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(texDl.texture);
        }
        texDl.Dispose();
        www.Dispose();
    }

    public IEnumerator GetText(string url, Action<string> onSuccess, Action<string> onFailure)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = 45;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log(www.error);
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(www.downloadHandler.text);
        }
    }

    public IEnumerator GetByte(string url, Action<byte[]> onSuccess, Action<string> onFailure)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = 45;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log(www.error);
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(www.downloadHandler.data);
        }
    }

    public void CheckResItemDataRelease()
    {
        if (resItemDataDic.Count < 100)
        {
            return;
        }
        else
        {
            for (int i = 0; i < 100; i++)
            {
                resItemDataDic.Remove(mapInfos[i].mapId);
            }
        }
    }

    public void LoadSingleResItemData(MapInfo mapInfo)
    {
        StartCoroutine(LoadSingleResData(mapInfo));
    }

    private void RefreshContent()
    {
        ResStorePanel.Instance.RefreshContentSize();
        ResStorePanel.Instance.CheckShowOrHide();
    }

    public void ActiveLoadAllCoroute(bool state)
    {
        if (state)
        {
            StartCoroutine("LoadAllResData");
            return;
        }
        StopCoroutine("LoadAllResData");
    }

    public void ActiveLoadSrchCoroute(bool state)
    {
        if (state)
        {
            StartCoroutine("LoadSrchResData");
            return;
        }
        StopCoroutine("LoadSrchResData");
    }

    private void StopBothCoroute()
    {
        StopCoroutine("LoadAllResData");
        StopCoroutine("LoadSrchResData");
    }

    private IEnumerator LoadAllResData()
    {
        yield return LoadBatchResData(mapInfos);
    }

    private IEnumerator LoadSrchResData()
    {
        yield return LoadBatchResData(srchMapInfos);
    }

    private void CheckRemoveCache(int count)
    {
        if (resItemDataDic.Count >= 100)
        {
            for (int i = 0; i < count; i++)
            {
                resItemDataDic.Remove(AlreadyShowResItem[i]);
            }
        }
    }

    private IEnumerator LoadSingleTexture(MapInfo mapInfo,OpenType type = OpenType.UGC)
    {
        if (string.IsNullOrEmpty(mapInfo.mapCover))
        {
            yield break;
        }
        UnityWebRequest www = new UnityWebRequest(mapInfo.mapCover);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        www.downloadHandler = texDl;
        www.timeout = 45;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log("LoadSpriteError" + www.error);
            LoggerUtils.LogError("Get ResData Fail");
        }
        else
        {
            ResItemData rData = new ResItemData();
            rData.mapCover = texDl.texture;
            rData.mapInfo = mapInfo;
            string jsonUrl = mapInfo.propsJson;
            if (type == OpenType.DC)
            {
                OnLoadTextureSuccess(mapInfo, "", rData);
            }
            else
            {
                if (jsonUrl.Contains("ZipFile/") && jsonUrl.Contains(".zip"))
                {
                    yield return GetByte(jsonUrl, (content) =>
                    {
                        string jsonStr = ZipUtils.SaveZipFromByte(content);
                        OnLoadTextureSuccess(mapInfo, jsonStr, rData);
                    }, (error) =>
                    {
                        LoggerUtils.LogError("Get ResMapJson Fail");
                    });
                }
                else
                {
                    yield return GetText(jsonUrl, (content) =>
                    {
                        OnLoadTextureSuccess(mapInfo, content, rData);
                    }, (error) =>
                    {
                        LoggerUtils.LogError("Get ResMapJson Fail");
                    });
                }
            }
        }
        texDl.Dispose();
        www.Dispose();
        iEnum = null;
    }
    private IEnumerator LoadSingleContent(MapInfo mapInfo)
    {
        ResItemData rData = new ResItemData();
        rData.mapInfo = mapInfo;
        string jsonUrl = mapInfo.propsJson;
        
        if (jsonUrl.Contains("ZipFile/") && jsonUrl.Contains(".zip"))
        {
            yield return GetByte(jsonUrl, (content) =>
            {
                string jsonStr = ZipUtils.SaveZipFromByte(content);
                OnLoadTextureSuccess(mapInfo, jsonStr, rData);
            }, (error) =>
            {
                LoggerUtils.LogError("Get ResMapJson Fail");
            });
        }
        else
        {
            yield return GetText(jsonUrl, (content) =>
            {
                OnLoadTextureSuccess(mapInfo, content, rData);
            }, (error) =>
            {
                LoggerUtils.LogError("Get ResMapJson Fail");
            });
        }
        iEnum = null;
    }
    private void OnLoadTextureSuccess(MapInfo mapInfo, string content, ResItemData rData)
    {
        rData.mapJsonContent = content;
        if (resItemDataDic.ContainsKey(mapInfo.mapId))
        {
            resItemDataDic.Remove(mapInfo.mapId);
            resItemDataDic.Add(mapInfo.mapId, rData);
        }
        else
        {
            resItemDataDic.Add(mapInfo.mapId, rData);
        }
        OnResCreate(mapInfo);
    }

    IEnumerator LoadBatchResData(List<MapInfo> refreshMapInfos)
    {
        foreach (var mapInfo in refreshMapInfos)
        {
            if (resItemDataDic.ContainsKey(mapInfo.mapId))
                continue;
            yield return LoadSingleTexture(mapInfo);
        }
        RefreshContent();
    }

    IEnumerator LoadSingleResData(MapInfo mapInfo)
    {
        if (resItemDataDic.Count >= 100)
        {
            resItemDataDic.Remove(AlreadyShowResItem[0]);
        }
        yield return LoadTexture(mapInfo.mapCover, (texture) =>
        {
            ResItemData resItemData = new ResItemData();
            resItemData.mapCover = texture;
            resItemData.mapInfo = mapInfo;
            string jsonUrl = mapInfo.propsJson;
            if (jsonUrl.Contains("ZipFile/") && jsonUrl.Contains(".zip"))
            {
                StartCoroutine(GetByte(jsonUrl, (content) =>
                {
                    string jsonStr = ZipUtils.SaveZipFromByte(content);
                    OnLoadResDataSuccess(mapInfo, jsonStr, resItemData);
                }, (error) =>
                {
                    LoggerUtils.LogError("Get ResMapJson Fail");
                }));
            }
            else
            {
                StartCoroutine(GetText(jsonUrl, (content) =>
                {
                    OnLoadResDataSuccess(mapInfo, content, resItemData);
                }, (error) =>
                {
                    LoggerUtils.LogError("Get ResMapJson Fail");
                }));
            }
        }, (error) =>
        {
            LoggerUtils.LogError("Get ResData Fail");
        });
        RefreshContent();
    }

    private void OnLoadResDataSuccess(MapInfo mapInfo, string content, ResItemData resItemData)
    {
        resItemData.mapJsonContent = content;
        if (resItemDataDic.ContainsKey(mapInfo.mapId))
        {
            resItemDataDic.Remove(mapInfo.mapId);
            resItemDataDic.Add(mapInfo.mapId, resItemData);
        }
        else
        {
            resItemDataDic.Add(mapInfo.mapId, resItemData);
        }
        try
        {
            RefreshContent();
        }
        catch
        {
            LoggerUtils.Log("Refresh Content Fail");
        }
    }

    private List<MapInfo> ClearEmptyMapInfo(List<MapInfo> mapInfos)
    {
        for(int i = 0;i < mapInfos.Count; i++)
        {
            if (string.IsNullOrEmpty(mapInfos[i].propsJson))
            {
                mapInfos.RemoveAt(i);
            }
        }
        return mapInfos;
    }

    private void OnDestroy()
    {
        inst = null;
    }
}

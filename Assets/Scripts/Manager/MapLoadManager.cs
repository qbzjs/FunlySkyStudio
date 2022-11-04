using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Events;

public enum OperationType
{
    ADD = 0,
    DELETE = 1,
    PUBLISH = 2,
    UPDATE = 3
};

public class MapLoadManager : CInstance<MapLoadManager>
{
    public void GetMapInfo(string info, UnityAction<string> onSuccess, UnityAction<string> onFail)
    {
        HttpUtils.MakeHttpRequest("/ugcmap/info", (int)HTTP_METHOD.GET, info, onSuccess, onFail);
    }


    public void GetMapInfo(HttpMapDataInfo httpMapDataInfo, UnityAction<GetMapInfo> onSuccess, UnityAction<string> onFail)
    {
        HttpUtils.MakeHttpRequest("/ugcmap/info", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpMapDataInfo),
            (content) =>
            {
              
                try
                {
                    var mapInfo = JsonConvert.DeserializeObject<HttpResponse>(content);
                    var getMapInfo = JsonConvert.DeserializeObject<GetMapInfo>(mapInfo.data);
                    LoggerUtils.Log("whiteListMask:" + getMapInfo.isInWhiteList);
                    GlobalFieldController.whiteListMask = new WhiteListMask(getMapInfo.isInWhiteList);
                    if (GlobalFieldController.whiteListMask.IsInWhiteList(WhiteListMask.WhiteListType.DevInfo))
                    {
                        FPSPanel.Instance.gameObject.SetActive(true);
                    }
                    onSuccess?.Invoke(getMapInfo);
                }
                catch (Exception e)
                {
                    LoggerUtils.LogError(e.StackTrace);
                    var responseDataRaw = new HttpResponseRaw
                    {
                        result = -1,
                        rmsg = e.Message
                    };
                    onFail?.Invoke(JsonConvert.SerializeObject(responseDataRaw));
                }
            }, onFail);
        
    }

    public void GetMapInfo(UgcUntiyMapDataInfo ugcUnityMapDataInfo, UnityAction<GetMapInfo> onSuccess, UnityAction<string> onFail)
    {
        var httpMapDataInfo = new HttpMapDataInfo()
        {
            mapId = ugcUnityMapDataInfo.mapId,
            mapName = ugcUnityMapDataInfo.mapName
        };
        GetMapInfo(httpMapDataInfo, onSuccess, onFail);
    }

    public void GetMapInfo(string mapId, string mapName, UnityAction<GetMapInfo> onSuccess, UnityAction<string> onFail)
    {
        var httpMapDataInfo = new HttpMapDataInfo
        {
            mapId = mapId,
            mapName = mapName
        };
        GetMapInfo(httpMapDataInfo, onSuccess, onFail);
    }

    public void SetMapInfo(OperationType operationType, UnityAction<string> onSuccess, UnityAction<string> onFail)
    {
        UpLoadMapBody upLoadMapBody = new UpLoadMapBody
        {
            mapInfo = GameManager.Inst.gameMapInfo,
            operationType = (int)operationType,
            templateId = GameManager.Inst.unityConfigInfo.templateId
        };
        LoggerUtils.Log("JsonConvert.SerializeObject(upLoadMapBody) = " + JsonConvert.SerializeObject(upLoadMapBody));
        HttpUtils.MakeHttpRequest("/ugcmap/set", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(upLoadMapBody), onSuccess, onFail);
    }

    public void SetMapInfo<T>(OperationType operationType, UnityAction<T> onSuccess, UnityAction<string> onFail)
    {
        UpLoadMapBody upLoadMapBody = new UpLoadMapBody
        {
            mapInfo = GameManager.Inst.gameMapInfo,
            operationType = (int)operationType,
            templateId = GameManager.Inst.unityConfigInfo.templateId
        };
        LoggerUtils.Log("JsonConvert.SerializeObject(upLoadMapBody) = " + JsonConvert.SerializeObject(upLoadMapBody));
        HttpUtils.MakeHttpRequest("/ugcmap/set", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(upLoadMapBody),
            (content) =>
            {
                try
                {
                    var roleResponseData = JsonUtility.FromJson<HttpResponDataStruct>(content);
                    var tmpObj = JsonConvert.DeserializeObject<T>(roleResponseData.data);
                    if (tmpObj is CreateMapInfo createMapInfo)
                    {
                        GlobalFieldController.whiteListMask = new WhiteListMask(createMapInfo.isInWhiteList);
                        LoggerUtils.Log("whiteListMask:" + createMapInfo.isInWhiteList);
                        if (GlobalFieldController.whiteListMask.IsInWhiteList(WhiteListMask.WhiteListType.DevInfo))
                        {
                            FPSPanel.Instance.gameObject.SetActive(true);
                        }
                    }
                    onSuccess?.Invoke(tmpObj);
                }
                catch (Exception e)
                {
                    var responseDataRaw = new HttpResponseRaw
                    {
                        result = -1,
                        rmsg = e.Message
                    };
                    onFail?.Invoke(JsonConvert.SerializeObject(responseDataRaw));
                }
         
                
            }, onFail);
    }

    // 加载地图Json
    public void LoadMapJson(string mapUrl, Action<string> onSuccess = null, Action<string> onFailure  = null)
    {
        string mapName = Path.GetFileNameWithoutExtension(mapUrl).Replace(".zip", "");
        SceneCacheUtils.CheckSceneCacheNum();
        string cont = SceneCacheUtils.LoadSceneCacheJson(mapName);
        if (string.IsNullOrEmpty(cont))
        {
            cont = SceneCacheUtils.LoadNativeCacheJson(mapName);
            if (string.IsNullOrEmpty(cont))
            {   
                MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_req);
                IEnumerator mapLoader = null;

                void LoadSuccess(string content)
                {
                    MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_rsp, "0");
                    SceneCacheUtils.SaveSceneCacheJson(mapName, content);
                    onSuccess?.Invoke(content);
                }
                
                void LoadFail(string error)
                {
                    MobileInterface.Instance.LogEventByEventName(LogEventData.unity_downloadJson_rsp, "1");
                    onFailure?.Invoke(error);
                }

                if (mapUrl.Contains("ZipFile/") && mapUrl.Contains(".zip"))
                {
                    mapLoader = ResManager.Inst.GetZipContent(mapUrl, LoadSuccess, LoadFail);
                }
                else
                {
                    mapLoader = ResManager.Inst.GetContent(mapUrl, LoadSuccess, LoadFail);
                }
                CoroutineManager.Inst.StartCoroutine(mapLoader);
                return;
            }
        }
        onSuccess?.Invoke(cont);
    }
    
    
    //批量获取MapInfo
    public void GetBatchMapInfo(HttpBatchMapDataInfo httpMapDataInfo, UnityAction<GetBatchMapInfo> onSuccess, UnityAction<string> onFail)
    {
        HttpUtils.MakeHttpRequest("/ugcmap/batchInfo", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(httpMapDataInfo),
            (content) =>
            {
              
                try
                {
                    var mapInfo = JsonConvert.DeserializeObject<HttpResponse>(content);
                    var getMapInfo = JsonConvert.DeserializeObject<GetBatchMapInfo>(mapInfo.data);
                    
                    onSuccess?.Invoke(getMapInfo);
                }
                catch (Exception e)
                {
                    var responseDataRaw = new HttpResponseRaw
                    {
                        result = -1,
                        rmsg = e.Message
                    };
                    onFail?.Invoke(JsonConvert.SerializeObject(responseDataRaw));
                }
            }, onFail);
        
    }

    public void GetDowntownInfo(string downtownId, UnityAction<DowntownInfo> onSuccess, UnityAction<string> onFail)
    { 
        HttpDowntownDataInfo data = new HttpDowntownDataInfo() {
            downtownId = downtownId,
        };
        HttpUtils.MakeHttpRequest("/downtown/info", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(data), (content)=> {
            var responseData = JsonUtility.FromJson<HttpResponse>(content);
            var getDowntownInfo = JsonConvert.DeserializeObject<GetDowntownInfo>(responseData.data);
            onSuccess?.Invoke(getDowntownInfo.downtownInfo);
        }, onFail);
    }
}

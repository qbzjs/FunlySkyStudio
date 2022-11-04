/// <summary>
/// Author:YangJie
/// Description: OfflineResManager
/// Date: 2022/3/30 18:28:5
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BestHTTP;
using HLODSystem;
using HLODSystem.Trees;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class OfflineResManager : CInstance<OfflineResManager>
{
    
    [Serializable]
    public class OfflineResInfo
    {
        public string rid;
        public string name;
        public ulong size;
        public ulong time;
        public string hash;

        public OfflineResInfo()
        {
            time = GetTimeStamp();
        }
        private ulong GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToUInt64(ts.TotalSeconds);
        }
    }
    
    public class ABLoadInfo : IComparable
    {
        // 0, 未加载
        // 1，加载中
        // 2，加载成功
        public int status = 0;
        public float dis = 0;
        public Action<bool> callBack;
        public OfflineRenderData renderData;
        public UGCModelType modelType;

        public int CompareTo(object obj)
        { 
            var other = (ABLoadInfo) obj;
            if (!other.renderData.IsCache(other.modelType) && renderData.IsCache(modelType))
            {
                return -1;
            } else if (other.renderData.IsCache(other.modelType) && !renderData.IsCache(modelType))
            {
                return 1;
            } else
            {
                return dis.CompareTo(other.dis);
            }
        }
    }
    public Dictionary<string, Dictionary<UGCModelType, ABLoadInfo>> loadingDic =
        new Dictionary<string, Dictionary<UGCModelType, ABLoadInfo>>();

    private string localSavePath => Application.persistentDataPath + "/" + GameConsts.OfflineCachePath;
    private string cacheFilePath => Application.persistentDataPath + "/" + GameConsts.OfflineCachePath + "CacheFile.json";
    private string fileName => LRUManager<FileLRUInfo>.Inst.LRUFilePath();
    public string nativeCacheSavePath => Application.persistentDataPath + "/" + GameConsts.NativeCachePath;
    
#if UNITY_EDITOR
    private ulong maxCacheSize = 500 * 1024 * 1024;
#else
    private ulong maxCacheSize = 500*1024*1024;
#endif

    // 最大同时加载AB
    public void Init()
    {
        OfflineLoader.Init();
        // HTTPManager.Logger.Level = Loglevels.All;
        HTTPManager.MaxConnectionPerServer = (byte) (SystemInfo.processorCount + 1);
        BestHTTP.PlatformSupport.Memory.BufferPool.IsEnabled = false;
        Clear();
        LRUManager<FileLRUInfo>.Inst.Init(FileLRUInfo.MaxSize);
        LoadCacheInfoFile();
    }

    public void LoadCacheInfoFile()
    {
        if (File.Exists(Path.Combine(DataUtils.dataDir,fileName)))       //新json文件存在
        {
            try
            {
                if(File.Exists(cacheFilePath))File.Delete(cacheFilePath);       //删除旧json文件
            }
            catch (Exception e)
            {
                LoggerUtils.LogError("LoadCacheFile:" + e.Message);
            }
        }
        else
        {   
            var directoryInfo = new DirectoryInfo(localSavePath);
            if (directoryInfo.Exists)
            {
                var fileInfos = directoryInfo.GetFiles("*.ab");
                foreach (var fileInfo in fileInfos)
                {
                    var resId = GetKeyRid(fileInfo.Name);
                    var cacheInfo = new FileLRUInfo
                    {
                        key = resId,
                        cacheFilePath = fileInfo.Name,
                        size =(ulong)fileInfo.Length
                    };
                    LRUManager<FileLRUInfo>.Inst.Put(cacheInfo);
                }
                LRUManager<FileLRUInfo>.Inst.SaveJson();
            }
            else
            {
                Directory.CreateDirectory(localSavePath);
            }
        }
    }

    public string GetFileName(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return null;
        }
        return url.Substring(url.LastIndexOf("/", StringComparison.Ordinal) + 1);
    }

    public string GetKeyRid(string name)                        //获取ab文件的 lruID，v3 ab 1460975788223664128_1643439899_3-20220804_112606 resId为1460975788223664128_1643439899_3 
    {                                                           // v4 ab 1460975788223664128_1643439899_3-20220804_112606_1 resId为1460975788223664128_1643439899_31 
        var paramArr = name.Split('-');                         //多出的1用于标记不同程度的模型
        var strs = name.Split('.')[0].Split('_');
        var resId = paramArr[0];
        if(strs.Length > 4)resId = resId + strs[4];
        else resId =resId + "0";
        return resId;
    }
    public string GetRid(string url)
    {
        var fileName = GetFileName(url);
        var paramArr = fileName.Split('-');
        if (paramArr.Length > 1)
        {
            return paramArr[0];
        }
        return null;
    }

    public void SaveCacheInfoFile()
    {
        LRUManager<FileLRUInfo>.Inst.SaveJson();
    }

    public void Clear()
    {
        StopLoader();
    }

    public void StopLoader()
    {
        preloadAssetBundleFinalCallBack = null;
        loadingParam = null;
        loadingDic?.Clear();
        PriorityManager<OfflineLoader>.Inst.Clear();
        if (preloadTimeOutCoroutine != null && CoroutineManager.Inst)
        {
            CoroutineManager.Inst.StopCoroutine(preloadTimeOutCoroutine);
        }

        preloadTimeOutCoroutine = null;
    }



    private void LoadAssetsCallBack(OfflineLoader resLoader, bool isSuccess)
    {
        if (loadingDic.TryGetValue(resLoader.RenderData.mapId, out var loadingInfos))
        {
            var abLoadingInfo = loadingInfos.Values.FirstOrDefault(tmp => tmp.renderData.GetRenderUrl(tmp.modelType) == resLoader.RenderData.GetRenderUrl(resLoader.ModelType));
            if (abLoadingInfo != null)
            {
                abLoadingInfo.callBack?.Invoke(isSuccess);
                loadingDic[resLoader.RenderData.mapId].Remove(abLoadingInfo.modelType);
                if (loadingDic[resLoader.RenderData.mapId].Count == 0)
                {
                    loadingDic.Remove(resLoader.RenderData.mapId);
                }
            }
        }
        resLoader.Dispose();
    }

    public void LoadFileAsync(string resId, UGCModelType modelType, System.Action<bool> callBack, float dis = 100, int priority = 0)
    {
        if (string.IsNullOrEmpty(resId))
        {
            callBack?.Invoke(false);
            return;
        }
        GlobalFieldController.offlineRenderDataDic.TryGetValue(resId, out var renderData);
        if (renderData != null)
        {
            LoadFileAsync(renderData, modelType, callBack, dis, priority);
        }
        else
        {
            callBack?.Invoke(false);
        }
    }

    public void LoadFileAsync(OfflineRenderData renderData, UGCModelType modelType, System.Action<bool> callBack, float dis = 100, int priority = 0)
    {
        AddLoadingDic(renderData.mapId, modelType, callBack, dis, priority);
    }
    public void AddLoadingDic(string resId, UGCModelType modelType, System.Action<bool> callBack, float dis = 100, int priority = 0)
    {
        GlobalFieldController.offlineRenderDataDic.TryGetValue(resId, out var renderData);
        if (renderData == null)
        {
            callBack?.Invoke(false);
            return;
        }

        if (UGCModelCachePool.Inst.IsContains(resId, modelType))
        {
            callBack?.Invoke(true);
            return;
        }
        
        ABLoadInfo abLoadingInfo = null;
        if (!loadingDic.ContainsKey(resId))
        {
            loadingDic.Add(resId, new Dictionary<UGCModelType, ABLoadInfo>());
        }

        var loadingInfos = loadingDic[resId];
        abLoadingInfo = loadingInfos.Values.FirstOrDefault(tmp => tmp.renderData.GetRenderUrl(tmp.modelType) == renderData.GetRenderUrl(modelType));
        if (abLoadingInfo == null) {
            abLoadingInfo = new ABLoadInfo()
            {
                status = 0,
                modelType = modelType,
                callBack = callBack,
                renderData = renderData,
                dis = dis
            };
            loadingDic[resId].Add(modelType, abLoadingInfo); 
        } else {
            if (callBack != null)
            {
                abLoadingInfo.callBack += callBack;
            }
        }
        if (abLoadingInfo.status != 0) return;
        abLoadingInfo.status = 1;
        var loader = OfflineLoader.Get(renderData, modelType, LoadAssetsCallBack, priority);
        PriorityManager<OfflineLoader>.Inst.Do(loader);
    }

    public override void Release()
    {
        Clear();
        AssetBundle.UnloadAllAssetBundles(true);
    }

    private ulong GetTimeDuration(DateTime overTime)
    {
        TimeSpan ts = overTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToUInt64(ts.TotalSeconds);
    }


    public void AddAbFile(FileLRUInfo fileLRUInfo)
    {
        LRUManager<FileLRUInfo>.Inst.Put(fileLRUInfo);
        DeleteDeprecatedAbFile();
    }

    private void DeleteDeprecatedAbFile()
    {
        SaveCacheInfoFile();
    }
    

    #region 预下载离线资源
    private LoadProgressParams loadingParam;
    private float startDownloadTime;
    private Action preloadAssetBundleFinalCallBack;
    private Coroutine preloadTimeOutCoroutine;
    private readonly int preloadTimeOut = 40;
    private int preloadCacheCount = 0;
    private int preloadCount = 0;
    private int preloadErrCount = 0;
    private IEnumerator PreloadTimeOut()
    {
        yield return new WaitForSeconds(preloadTimeOut);
        loadingParam = null;
        preloadTimeOutCoroutine = null;
        preloadErrCount += (PriorityManager<OfflineLoader>.Inst.GetDoingCount() + PriorityManager<OfflineLoader>.Inst.GetWaitingCount());
        PriorityManager<OfflineLoader>.Inst.Clear();
        loadingDic.Clear();
        preloadAssetBundleFinalCallBack?.Invoke();
    }

    public List<string> PreDealWithOfflineRes(string mapDataContent)
    {
        var mapData = JsonConvert.DeserializeObject<MapData>(mapDataContent);
        GlobalFieldController.ugcNodeData = UGCBehaviorManager.Inst.GetAllUGCData(mapData);
        if (mapData.pref == null || mapData.pref.Count == 0)
        {
            return null;
        }
        SpawnPointManager.Inst.SetSpawnPoint(mapData);
        return HLOD.Inst.GetHighUGCRids(mapData);
    }

    public void PreloadAssetBundle(List<string> rids, System.Action callBack = null)
    {
        LoggerUtils.Log("PreloadAssetBundle Start:" + rids?.Count);
        startDownloadTime = Time.realtimeSinceStartup;
        preloadAssetBundleFinalCallBack = () =>
        {
            UnityLoadingstage unityLoadingstage = new UnityLoadingstage()
            {
                stage = (int) LoadingstageType.BuildingExperience
            };
            MobileInterface.Instance.Notify(MobileInterface.loadingDialog, JsonConvert.SerializeObject(unityLoadingstage));

            MobileInterface.Instance.LogEvent(LogEventData.unity_downLoadABFinish, new LogEventDownLoadABFinish()
            {
                cache = preloadCacheCount,
                error = preloadErrCount,
                total = preloadCount,
                useTime = (int) ((Time.realtimeSinceStartup - startDownloadTime) * 1000)
            });
            callBack?.Invoke();
            preloadAssetBundleFinalCallBack = null;
        };
        preloadTimeOutCoroutine = CoroutineManager.Inst.StartCoroutine(PreloadTimeOut());
        loadingParam = new LoadProgressParams();
        UGCBehaviorManager.Inst.InitOfflineRenderData();
        if (GlobalFieldController.offlineRenderDataDic.Count == 0)
        {
            preloadAssetBundleFinalCallBack?.Invoke();
            return;
        }

        preloadCacheCount = 0;
        preloadCount = 0;
        preloadCacheCount = 0;
        
        var tmpValues = GlobalFieldController.offlineRenderDataDic.Values.ToList();
        var loaders = new List<OfflineLoader>();
        foreach (var tmpRenderData in tmpValues)
        {
            if (rids != null && !rids.Contains(tmpRenderData.mapId))
            {
                continue;
            }

            preloadCount++;
            bool isCache = false;
            // 游玩模式下 优先加载Low 状态 UGC 模型, 其他状态下 加载High 状态 UGC 模型
            var modeType = Object.FindObjectOfType<GameController>()?.curGameMode == EnterGameMode.GuestScene ?  UGCModelType.Low : UGCModelType.High;
            if (UGCModelCachePool.Inst.IsContains(tmpRenderData.mapId, modeType))
            {
                // 已经加载到内存中了
                isCache = true;
            }
            if (tmpRenderData.IsCache(UGCModelType.High))
            {
                isCache = true;
                modeType = UGCModelType.High;
            }
            else if(tmpRenderData.IsCache(UGCModelType.Low))
            {
                isCache = true;
                modeType = UGCModelType.Low;
            }

            if (isCache)
            {
                preloadCacheCount++;
                loadingParam.now++;
                continue;
            }
            var tmpLoader = OfflineLoader.Get(tmpRenderData, modeType, PreloadCallBack);
            loaders.Add(tmpLoader);
        }
        loadingParam.total = preloadCount;
        if (loadingParam.now >= loadingParam.total)
        {
            MobileInterface.Instance.Notify(MobileInterface.updateDownloadProgress, loadingParam);
            LoggerUtils.Log("updateDownloadProgress:" + loadingParam.now + "/" + loadingParam.total);
            LoggerUtils.Log("PreloadAssetBundle Over:" + (Time.realtimeSinceStartup - startDownloadTime));
            preloadAssetBundleFinalCallBack?.Invoke();
            loadingParam = null;
            if (preloadTimeOutCoroutine != null)
            {
                CoroutineManager.Inst.StopCoroutine(preloadTimeOutCoroutine);
                preloadTimeOutCoroutine = null;
            }
        }
        foreach (var loader in loaders)
        {
            PriorityManager<OfflineLoader>.Inst.Do(loader);
        }
    }
    
    /// <summary>
    /// 预下载AssetBundle 资源
    /// </summary>
    /// <param name="callBack"></param>
    public void PreloadAssetBundle(System.Action callBack = null)
    {
        PreloadAssetBundle(new List<string>(), callBack);
    }

    private void PreloadCallBack(OfflineLoader loader, bool isSuccess)
    {
        if (!isSuccess)
        {
            preloadErrCount++;
        }

        if (loader.IsUseCache)
        {
            preloadCacheCount++;
        }
        loadingParam.now++;
        MobileInterface.Instance.Notify(MobileInterface.updateDownloadProgress, loadingParam);
        LoggerUtils.Log("updateDownloadProgress:" + loadingParam.now + "/" + loadingParam.total);
        if (loadingParam.now >= loadingParam.total)
        {
            LoggerUtils.Log("PreloadAssetBundle Over:" + (Time.realtimeSinceStartup - startDownloadTime));
            preloadAssetBundleFinalCallBack?.Invoke();
            loadingParam = null;
            if (preloadTimeOutCoroutine != null)
            {
                CoroutineManager.Inst.StopCoroutine(preloadTimeOutCoroutine);
                preloadTimeOutCoroutine = null;
            }
        }
        loader.Dispose();
    }
    #endregion
}

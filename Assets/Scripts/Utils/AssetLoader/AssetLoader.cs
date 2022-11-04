using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public abstract class AssetLoader : BaseAction
{
    private Coroutine loadCoroutine;
    private HttpDownloader httpDownloader;

    protected bool isCached;
    protected string cachePath;

    public string FileName { get; set; }
    public List<string> CacheFolders { get; set; }
    public string SaveFolder { get; set; }
    public bool DisableCache { get; set; }

    public bool IsUseCache { get; private set; }

    public AssetLoader(string url, Action<BaseAction, object, string> callBack, int priority = 0): base(callBack, priority)
    {
        Url = url;
    }


    public string Url { get; private set; }

    public bool IsCached
    {
        get
        {
            if (isCached || !string.IsNullOrEmpty(cachePath))
            {
                return true;
            }
            if (string.IsNullOrEmpty(FileName))
            {
                FileName = GetLocalFileName();
            }

            if (CacheFolders == null)
            {
                if (SaveFolder == null)
                {
                    SaveFolder = Application.persistentDataPath;
                }
                CacheFolders = new List<string>() { SaveFolder };
            }
            foreach (var cacheFolder in CacheFolders)
            {
                var tmpPath = Path.Combine(cacheFolder, FileName);
                if (!File.Exists(tmpPath)) continue;
                isCached = true;
                cachePath = tmpPath;
                return true;
            }
            return false;
        }
    }

    public override void Do()
    {
        if (!isUsed)
        {
            return;
        }

        if (!CheckUrl())
        {
            OnAssetCallBack(default, "URL ERROR");
            return;
        }
        
        if (IsCached)
        {
            IsUseCache = true;
            LoadLocalAsset();
        }
        else
        {
            LoadRemoteAsset();
        }
    }

    protected virtual bool CheckUrl()
    {
        Uri uri = null;
        try
        {
            uri = new Uri(Url);
        }
        catch (Exception e)
        {
            uri = null;
            Debug.LogError("URL Error:" + Url);
        }
        return uri != null;
    }

    private void LoadLocalAsset()
    {
        var data = File.ReadAllBytes(cachePath);
        OnDataLoadCallBack(data);
    }


    private void LoadRemoteAsset()
    {
        if (Url.StartsWith("https://") || Url.StartsWith("http://"))
        {
            // 所有 http 协议的资源都使用 HttpDownloader 加载
            httpDownloader = new HttpDownloader(Url, (downloader, data, err) =>
            {
                var bytes = data as byte[];
                if (bytes != null)
                {
                    OnDownloadEnd(GetLocalFileName(), (ulong) bytes.Length);
                }
                else
                {
                    OnDownloadErr(GetLocalFileName(), err);
                }
                httpDownloader?.Dispose();
                httpDownloader = null;
                OnDataLoadCallBack(bytes, err);
            }, Priority);
            httpDownloader.onStarted += OnDownloadStart;
            httpDownloader = PriorityManager<HttpDownloader>.Inst.Do(httpDownloader) as HttpDownloader;
        }
        else
        {
            // 其他文件属于本地资源都使用 UnityWebRequest 加载
            var request = UnityWebRequest.Get(Url);
            request.timeout = 45;
            request.SendWebRequest();
            loadCoroutine = CoroutineManager.Inst.CallBack(request.SendWebRequest(), operation =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    OnDataLoadCallBack(null, request.error);
                }
                else
                {
                    OnDataLoadCallBack(request.downloadHandler.data);
                }
                request.Dispose();
                request = null;
                loadCoroutine = null;
            });
        }
    }


    private void OnDataLoadCallBack(byte[] data, string err = null)
    {
        if (!isUsed)
        {
            return;
        }
        if (data == null && string.IsNullOrEmpty(err))
        {
            err = "Data is null:" + Url;
        }
        if (!string.IsNullOrEmpty(err))
        {
            OnAssetCallBack(default, err);
        }
        else
        {
            OnDataHandler(data,  (tAsset, handlerData, error) =>
            {
                if (!isCached && !DisableCache)
                {
                    SaveToCache(handlerData);
                }
                OnAssetCallBack(tAsset, err);
            });
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        Url = null;
        FileName = null;
        isCached = false;
        CacheFolders = null;
        SaveFolder = null;
        if (loadCoroutine != null && CoroutineManager.IsInit())
        {
            CoroutineManager.Inst.StopCoroutine(loadCoroutine);
        }
        loadCoroutine = null;
        httpDownloader?.Dispose();
        httpDownloader = null;
    }

    protected virtual string GetLocalFileName()
    {
        return Path.GetFileName(Url);
    }

    protected virtual bool SaveToCache(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            return false;
        }
        if (SaveFolder == null)
        {
            SaveFolder = Application.persistentDataPath;
        }

        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
        }
        var localFileName = GetLocalFileName();
        var savePath = Path.Combine(SaveFolder, GetLocalFileName());
        try
        {
            File.WriteAllBytes(savePath, data);
            OnSaveCallBack(localFileName, (ulong)data.Length);
            return true;
        }
        catch (System.Exception err)
        {
            LoggerUtils.LogError(err.Message+" Url =: "+Url);
            return false;
        }
        
    }
    
    protected virtual void OnDataHandler(byte[] data, Action<object, byte[], string> callBack)
    {
        callBack?.Invoke(data, data, null);
    }

    protected virtual void OnSaveCallBack(string fileName, ulong dataLength)
    {
        
    }

    protected virtual void OnDownloadStart(string fileName)
    {
        
    }

    /// <summary>
    /// 若资源需要下载，下载成功回调
    /// </summary>
    protected virtual void OnDownloadEnd(string fileName, ulong length)
    {
        
    }

    /// <summary>
    /// 若资源需要下载，下载失败回调
    /// </summary>
    protected virtual void OnDownloadErr(string fileName, string err)
    {
        
    }

}

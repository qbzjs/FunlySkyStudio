using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UGCResourcePool : CInstance<UGCResourcePool>
{
    public Dictionary<string, UGCDownloader> allTexturePool = new Dictionary<string, UGCDownloader>();
    private int poolCapacity = 5000;//缓存池大小待优化
    
    public enum DownloadState
    {
        Pre,
        Loading,
        Complete
    }
    
    public class UGCDownloader
    {
        public Texture tex;
        public Action<Texture> onComplete;
        public Action onFailed;
        public string texPath;
        public DownloadState state = DownloadState.Pre;
    }
    
    public void DownloadAndGet(string path, Action<Texture> OnComplete, Action OnFailed = null)
    {
        if (allTexturePool.ContainsKey(path))
        {
            if (allTexturePool[path].state == DownloadState.Loading)
            {
                allTexturePool[path].onComplete += OnComplete;
                allTexturePool[path].onFailed += OnFailed;
                //complete?.Invoke(allTexturePool[path]);
                return;
            }
            else if (allTexturePool[path].state == DownloadState.Complete)
            {
                allTexturePool[path].onComplete = OnComplete;
                OnComplete?.Invoke(allTexturePool[path].tex);
                return;
            }
        }
        
        UGCDownloader downloader = new UGCDownloader();
        downloader.state = DownloadState.Loading;
        downloader.onComplete = OnComplete;
        downloader.onFailed = OnFailed;
        downloader.texPath = path;
        downloader.tex = null;
        allTexturePool[path] =  downloader;
        CoroutineManager.Inst.StartCoroutine(GameUtils.LoadTexture(path,
            (tex) =>
            {
                if (allTexturePool.Count > poolCapacity)
                {
                    allTexturePool.Remove(allTexturePool.Keys.First());
                }
                downloader.tex = tex;
                allTexturePool[downloader.texPath] = downloader;
                downloader.state = DownloadState.Complete;
                downloader.onComplete?.Invoke(tex);
            }, (x) =>
            {
                allTexturePool.Remove(downloader.texPath);
                downloader.onFailed?.Invoke();
            }));
    }
}
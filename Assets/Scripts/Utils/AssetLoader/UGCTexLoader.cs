using System;
using UnityEngine;

/// <summary>
/// Author:YangJie
/// Description:
/// Date: 2022/10/12 17:52:18
/// </summary>
public class UGCTexLoader: AssetLoader
{
    private Coroutine loadAssetBundleCoroutine;
    public UGCTexLoader(string url, Action<BaseAction, object, string> callBack, int priority = 0) : base(url, callBack, priority)
    {
        SaveFolder = Application.persistentDataPath + "/Offline/UGCTex/";
    }

    protected override void OnDataHandler(byte[] data, Action<object, byte[], string> callBack)
    {
        base.OnDataHandler(data,callBack);
        // if(data.Length <= 0)
        // {
        //     callBack?.Invoke(null, data, "Data length less than zero Error:" + Url);
        //     return;
        // } 
        
        // loadAssetBundleCoroutine = CoroutineManager.Inst.CallBack(AssetBundle.LoadFromMemoryAsync(data), request =>
        //     {
        //         if (request.assetBundle != null)
        //         {      
        //             callBack?.Invoke(request.assetBundle, data, null);
        //         }
        //         else
        //         {
        //             callBack?.Invoke(null, data, "LoadFromMemoryAsync Error:" + Url);
        //             loadAssetBundleCoroutine = null;
        //         }
        //     });
    }

    protected override void OnSaveCallBack(string fileName, ulong dataLength)
    {
        var resId = UGCTexManager.Inst.GetUGCTexID(fileName);
        LRUManager<UGCTexLRUInfo>.Inst.Put(new UGCTexLRUInfo{key = resId,cacheFilePath = fileName,size = dataLength});
    }

    public override void Dispose()
    {
        base.Dispose();
        // if (CoroutineManager.IsInit() && loadAssetBundleCoroutine != null)
        // {
        //     CoroutineManager.Inst.StopCoroutine(loadAssetBundleCoroutine);
        // }
        // loadAssetBundleCoroutine = null;
    }
}

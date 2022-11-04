using System;
using UnityEngine;

/// <summary>
/// Author:YangJie
/// Description:
/// Date: #CreateTime#
/// </summary>
public class AssetBundleLoader: AssetLoader
{
    private Coroutine loadAssetBundleCoroutine;
    public AssetBundleLoader(string url, Action<BaseAction, object, string> callBack, int priority = 0) : base(url, callBack, priority)
    {
        
    }

    protected override void OnDataHandler(byte[] data, Action<object, byte[], string> callBack)
    {
        if(data.Length <= 0)
        {
            callBack?.Invoke(null, data, "Data length less than zero Error:" + Url);
            return;
        } 

        loadAssetBundleCoroutine = CoroutineManager.Inst.CallBack(AssetBundle.LoadFromMemoryAsync(data), request =>
            {
                if (request.assetBundle != null)
                {      
                    callBack?.Invoke(request.assetBundle, data, null);
                }
                else
                {
                    callBack?.Invoke(null, data, "LoadFromMemoryAsync Error:" + Url);
                    loadAssetBundleCoroutine = null;
                }
            });
    }

    public override void Dispose()
    {
        base.Dispose();
        if (CoroutineManager.IsInit() && loadAssetBundleCoroutine != null)
        {
            CoroutineManager.Inst.StopCoroutine(loadAssetBundleCoroutine);
        }
        loadAssetBundleCoroutine = null;
    }
}

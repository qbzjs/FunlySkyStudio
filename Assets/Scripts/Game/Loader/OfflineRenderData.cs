using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

/// <summary>
/// Author:YangJie
/// Description:
/// Date: #CreateTime#
/// </summary>

[Serializable]
public class OfflineRenderData
{
    
    public class ModelInfo
    {
        public string fileName;
        public string url;
        public bool isCache;
        public string cachePath;

        public ModelInfo(string _fileName, string _url)
        {
            fileName = _fileName;
            url = _url;
            cachePath = GetLocalCachePath();
            isCache = !string.IsNullOrEmpty(cachePath);
        }

        public string GetLocalCachePath()
        {
            var resId = OfflineResManager.Inst.GetKeyRid(fileName);
            var targetPath = Application.persistentDataPath + "/" + GameConsts.OfflineCachePath + fileName;

            if (!File.Exists(targetPath))
            {   
                targetPath = Application.persistentDataPath + "/" + GameConsts.NativeCachePath + fileName;
                if (!File.Exists(targetPath))
                {
                    return null;
                }
            }

            LRUManager<FileLRUInfo>.Inst.Get(resId);
            return targetPath;
        }
    }

    private bool isInit = false;
    private bool isSameModel = false;
    public static readonly string V1 = "1.0";
    public static readonly string V2 = "2.0";
    public static readonly string V3 = "3.0";
    public static readonly string V4 = "4.0";
    public static readonly string V41 = "4.1";
    
    public string mapId;
    public string renderUrl;
    public string version = "1.0";

    /// <summary>
    /// 分高低模型 渲染Url
    /// </summary>
    private Dictionary<UGCModelType, ModelInfo> modelInfos = null;
    
    public void Init()
    {
        if (isInit)
        {
            return;
        }
        modelInfos = new Dictionary<UGCModelType, ModelInfo>();
        if (version == V4 || version == V41)
        {
            List<string> urls = JsonConvert.DeserializeObject<List<string>>(renderUrl);
            foreach (var url in urls)
            {
                var fileName = Path.GetFileName(url);
                var tmpModelString = Path.GetFileNameWithoutExtension(url).Substring(fileName.LastIndexOf("_", StringComparison.Ordinal) + 1);
                if (int.TryParse(tmpModelString,  out var tmpModelNumber))
                {
                    var tmpModelInfo = new ModelInfo(fileName.Replace(".zip", ".ab"), url);
                    modelInfos.Add(((UGCModelType)tmpModelNumber), tmpModelInfo);
                }
            }
        }
        else
        {
            var fileName = renderUrl.Substring(renderUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);
            var tmpModelInfo = new ModelInfo(fileName, renderUrl);
            modelInfos.Add(UGCModelType.Low, tmpModelInfo);
        }
        
        isInit = true;
    }

    /// <summary>
    /// 判断高低模AB文件是否一致
    /// </summary>
    /// <returns></returns>
    public bool IsSameFile()
    {
        return modelInfos.Count == 1;
    }

    public bool IsSameModel()
    {
        return isSameModel;
    }

    public void SetSameModel(bool isSame)
    {
        isSameModel = isSame;
    }

    public bool IsCache(UGCModelType modelType)
    {
        var tmpModelInfo = GetModelInfo(modelType);
        
        return  tmpModelInfo != null && tmpModelInfo.isCache;
    }
    
    public bool IsCache()
    {
        return Enum.GetValues(typeof(UGCModelType)).Cast<UGCModelType>().Any(IsCache);
    }

    public void SetCache(UGCModelType modelType , bool isCache)
    {
        var tmpModelInfo = GetModelInfo(modelType);
        if (tmpModelInfo != null)
        {
            tmpModelInfo.isCache = isCache;
            if (isCache)
            {
                tmpModelInfo.cachePath = tmpModelInfo.GetLocalCachePath();
            }
        }
    }
    
    public void SetCache(bool isCache)
    {
        foreach (UGCModelType tmpModelType in Enum.GetValues(typeof(UGCModelType)))
        {
            if (modelInfos.ContainsKey(tmpModelType))
            {
                SetCache(tmpModelType, isCache);
                break;
            }
        }
    }
    
    public string GetCachePath(UGCModelType modelType = UGCModelType.Low)
    {
        var tmpModelInfo = GetModelInfo(modelType);
        if (tmpModelInfo != null)
        {
            return tmpModelInfo.cachePath;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 获取对于某个等级的渲染Url，如果没有则返回高等级的渲染Url
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    public string GetRenderUrl(UGCModelType modelType = UGCModelType.Low)
    {
        var tmpModelInfo = GetModelInfo(modelType);
        return tmpModelInfo?.url;
    }

    public string GetFileName(UGCModelType modelType = UGCModelType.Low)
    {
        var tmpModelInfo = GetModelInfo(modelType);
        return tmpModelInfo?.fileName;
    }

    public void DeleteCacheFile(string fileName)
    {
        foreach (var tmpModelInfo in modelInfos)
        {
            if (tmpModelInfo.Value.fileName == fileName)
            {
                tmpModelInfo.Value.cachePath = null;
                tmpModelInfo.Value.isCache = false;
            }
        }
    }

    private ModelInfo GetModelInfo(UGCModelType modelType = UGCModelType.Low)
    {
        ModelInfo tmpModelInfo = null;
        if (modelInfos.ContainsKey(modelType))
        {
            tmpModelInfo = modelInfos[modelType];
        }
        else
        {
            foreach (UGCModelType tmpModelType in Enum.GetValues(typeof(UGCModelType)))
            {
                if (modelInfos.ContainsKey(tmpModelType))
                {
                    tmpModelInfo = modelInfos[tmpModelType];
                    break;
                }
            }
        }
        return tmpModelInfo;
    }
}

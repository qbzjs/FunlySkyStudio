using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Author:YangJie
/// Description:
/// Date: #CreateTime#
/// </summary>
public class OfflineLoader : AssetBundleLoader
{
    struct UGCMatName
    {
        public string umat;
        public int matId;
        public string tiling;
        public string color;

        public UGCMatName(string matName)
        {
            var names = matName.Split('_');
            color = names[1];
            tiling = names[2];
            int.TryParse(names[3],out matId);
            if(names.Length > 4)
            {
                umat = names[4];
            }
            else
            {
                umat = null;
            }
        } 
    }
    private Action<OfflineLoader, bool> loadedCallBack;
    private OfflineRenderData renderData;
    private UGCModelType modelType;
    private static Dictionary<string,List<string>> fileDir;

    private static Light light;
    private static Shader alphaShader;
    private static Shader opaqueShader;
    private static Shader normalShader;

    public OfflineRenderData RenderData => renderData;
    public UGCModelType ModelType => modelType;

    private OfflineLoader(OfflineRenderData data, UGCModelType type, Action<OfflineLoader, bool> callBack, int priority = 0) : base(
        data.GetRenderUrl(type), OnAssetBundleCallBack, priority)
    {
        SaveFolder = Application.persistentDataPath + "/" + GameConsts.OfflineCachePath;
        CacheFolders = new List<string>()
        {
            SaveFolder, Application.persistentDataPath + "/" + GameConsts.NativeCachePath
        };
        renderData = data;
        modelType = type;
        loadedCallBack = callBack;
    }


    #region Static Methods

    public static void Init()
    {
        light = GameObject.Find("DirLight").GetComponent<Light>();
        alphaShader = Shader.Find("Custom/CustomDiffuseAlpha");
        opaqueShader = Shader.Find("Custom/CustomDiffuse");
        normalShader = Shader.Find("Custom/CustomDiffuseWithNormal");
        fileDir = new Dictionary<string,List<string>>();
        InitFileInfos();
    }

    public static OfflineLoader Get(OfflineRenderData renderData, UGCModelType modelType, Action<OfflineLoader, bool> callBack,
        int priority = 0)
    {
        var loader = new OfflineLoader(renderData, modelType, callBack, priority);
        return loader;
    }

    private static void OnAssetBundleCallBack(BaseAction action, object data, string err)
    {
        AssetLoader loader = action as AssetLoader;
        AssetBundle assetBundle = data as AssetBundle;
        var tmpLoader = (OfflineLoader) loader;
        GameObject[] assets = null;
        if (assetBundle != null)
        {
            assets = assetBundle.LoadAllAssets<GameObject>();
            assetBundle.Unload(false);
        }
        else
        {
            LoggerUtils.Log($"Asset Bundle Error: {tmpLoader?.Url}" + err);
        }
        tmpLoader?.OnLoadedSuccess(assets);
    }

    #endregion


    #region CallBack

    protected override void OnDownloadStart(string fileName)
    {
        DataLogUtils.LogUnityDownLoadABStart(fileName);
    }

    protected override void OnDownloadEnd(string fileName, ulong length)
    {
        DataLogUtils.LogUnityDownLoadABEnd(fileName, (int)length);
    }

    protected override void OnDownloadErr(string fileName, string err)
    {
        DataLogUtils.LogUnityDownLoadABError(fileName, err);
    }

    protected override void OnSaveCallBack(string fileName, ulong dataLength)
    {
        DeleteNativeABFile(fileName);
        var resId = OfflineResManager.Inst.GetKeyRid(fileName);
        OfflineResManager.Inst.AddAbFile(new FileLRUInfo{key = resId,cacheFilePath = fileName,size = dataLength});
    }
    private void DeleteNativeABFile(string fileName)
    {
        var rid = fileName.Split('-');

        if(fileDir.ContainsKey(rid[0]))
        {
            foreach (var item in fileDir[rid[0]])
            {
                if(File.Exists(Application.persistentDataPath + "/" + GameConsts.NativeCachePath + item))
                {
                    File.Delete(Application.persistentDataPath + "/" + GameConsts.NativeCachePath + item);
                }
            }
            fileDir.Remove(rid[0]);
        }
    }
    private static void InitFileInfos()
    {
        var fileJsonPath = Application.persistentDataPath + "/" + GameConsts.OfflineCachePath+"Fileinfos.json";
        if(File.Exists(fileJsonPath)){
            string str = File.ReadAllText(fileJsonPath);
            fileDir = JsonConvert.DeserializeObject<Dictionary<string,List<string>>>(str);
        }
        else
        {
            var directoryInfo = new DirectoryInfo(Application.persistentDataPath + "/" + GameConsts.NativeCachePath);
            if (directoryInfo.Exists)
            {
                var fileInfos = directoryInfo.GetFiles("*.ab");
                foreach (var fileInfo in fileInfos)
                {
                    var rid = fileInfo.Name.Split('-');
                    if(!fileDir.ContainsKey(rid[0]))
                    {
                        fileDir.Add(rid[0], new List<string>() {fileInfo.Name});
                    }
                    else
                    {
                        fileDir[rid[0]].Add(fileInfo.Name);
                    }
                }
                if(!Directory.Exists(Application.persistentDataPath + "/" + GameConsts.OfflineCachePath))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + "/" + GameConsts.OfflineCachePath);
                }
                File.WriteAllText(fileJsonPath,JsonConvert.SerializeObject(fileDir));
            }
        }
       
    }
    private void OnLoadedSuccess(GameObject[] objs)
    {
        if (objs == null)
        {
            loadedCallBack?.Invoke(this, false);
        }
        else
        {
            ReplaceLocalMat(objs);
            PreloadDealUGCModel(objs);
            loadedCallBack?.Invoke(this, true);
        }
    }
    
    protected override void OnDataHandler(byte[] data, Action<object, byte[], string> callBack)
    {
        if (Url.EndsWith(".zip") && !isCached)
        {
            data = ZipUtils.Unzip(data);
        }
        base.OnDataHandler(data, callBack);
    }

    #endregion

    
    protected override string GetLocalFileName()
    {
        var tmpLocalFileName = base.GetLocalFileName();
        if (tmpLocalFileName.EndsWith(".zip"))
        {
            tmpLocalFileName = tmpLocalFileName.Replace(".zip", ".ab");
        }
        return tmpLocalFileName;
    }
    


    private void PreloadDealUGCModel(GameObject[] objs)
    {
        var rid = renderData.mapId;
        if (objs == null || UGCModelCachePool.Inst.IsContains(renderData.mapId, modelType))
        {
            return;
        }

        LODGroup lodGroup = null;
        foreach (var tmpObj in objs)
        {
            lodGroup = tmpObj.GetComponentInChildren<LODGroup>(true);
            if (lodGroup != null)
            {
                break;
            }
        }

        if (lodGroup == null)
        {
            
            if (renderData.IsSameFile())
            {
                renderData.SetSameModel(true);
                UGCModelCachePool.Inst.SetOriginObj(rid, new List<UGCModelType>() {UGCModelType.Low, UGCModelType.High}, objs);
                renderData.SetCache(true);
            }
            else
            {
                UGCModelCachePool.Inst.SetOriginObj(rid, modelType, objs);
                renderData.SetCache(modelType,true);
            }
            
        }
        else
        {
            List<GameObject> lowObjs = new List<GameObject>();
            List<GameObject> highObjs = new List<GameObject>();
            var lods = lodGroup.GetLODs();
            if (lods.Length >= 1)
            {
                var highRenders = lods[0].renderers;
                foreach (var tmpRender in highRenders)
                {
                    if (!tmpRender.TryGetComponent<MeshCollider>(out _))
                    {
                        tmpRender.gameObject.AddComponent<MeshCollider>();
                    }

                    highObjs.Add(tmpRender.gameObject);
                }
            }

            if (highObjs.Count > 0)
            {
                var highObj = UGCModelCachePool.Inst.SetOriginObj(rid, UGCModelType.High, highObjs.ToArray());
                highObj.name = "LODHighObj";
                renderData.SetCache(UGCModelType.High,true);
            }

            if (lods.Length >= 2)
            {
                var lowRenders = lods[1].renderers;
                foreach (var tmpRender in lowRenders)
                {
                    if (!tmpRender.TryGetComponent<MeshCollider>(out var _))
                    {
                        tmpRender.gameObject.AddComponent<MeshCollider>();
                    }

                    lowObjs.Add(tmpRender.gameObject);
                }
            }

            if (lowObjs.Count > 0)
            {
                var lodLow = UGCModelCachePool.Inst.SetOriginObj(rid, UGCModelType.Low, lowObjs.ToArray());
                lodLow.name = "LODLowObj";
                renderData.SetCache(UGCModelType.Low,true);
            }
        }
    }

    private void ReplaceLocalMat(GameObject[] objs)
    {
        foreach (var tmpObj in objs)
        {
            var renders = tmpObj.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var tmpRender in renders)
            {
                var mats = tmpRender.sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    Material tmpMat = mats[j];
                    if(tmpMat.name.Contains("SDF"))  //SDF是3d文字的材质球就不走shader替换
                    {
                        mats[j].shader = Shader.Find("TextMeshPro/Mobile/Distance Field");
                        continue; 
                    }
#if UNITY_ANDROID
                    tmpMat = ReplaceMobileShader(tmpMat);
#endif
                    FillMaterialTexture(tmpMat);
                    mats[j] = tmpMat;
                }

                tmpRender.sharedMaterials = mats;
            }
        }
    }

    private Material ReplaceMobileShader(Material mat)
    {
        var tag = mat.GetTag("RenderType", false);
        var tmpColor = mat.GetColor("_Color");
        Shader tmpShader = opaqueShader;
        if (mat.name.Contains("Simplygon"))
        {
            tmpShader = normalShader;
        }
        if (tag == "Transparent") tmpShader = alphaShader;
        var tmpMat = new Material(tmpShader);
        tmpMat.name = mat.name;
        tmpMat.SetTexture("_MainTex", mat.mainTexture);
        tmpMat.SetColor("_Color", tmpColor);
        tmpMat.SetTexture("_BumpMap", mat.GetTexture("_BumpMap"));
        tmpMat.SetFloat("_Glossiness", mat.GetFloat("_Glossiness"));
        tmpMat.SetFloat("_Metallic", mat.GetFloat("_Metallic"));
        tmpMat.SetTextureScale("_MainTex", mat.GetTextureScale("_MainTex"));
        var intensity = 0.82f;
        if (light && light.color == Color.white)
        {
            intensity = 0.75f;
        }
        tmpMat.SetFloat("_Intensity", intensity);
        return tmpMat;
    }

    /// <summary>
    /// 通过材质球的贴图ID 获取贴图
    /// </summary>
    /// <param name="mat"></param>
    private void FillMaterialTexture(Material mat)
    {
        var matNames = mat.name.Split('_');
        UGCMatName uGCMat = new UGCMatName(mat.name);
        if (matNames.Length == 4 && matNames[0] == "output" && int.TryParse(matNames[3], out var matId))
        {
            var matData = GameManager.Inst.matConfigDatas?.Find(x => x.id == matId);
            if (matData != null)
            {
                var tex = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + matData.texName);
                var normalMap =
                    ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + matData.texName + "_normal");
                if (normalMap == null)
                {
                    normalMap = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + "default_normal");
                }
                mat.SetTexture("_MainTex", tex);
                mat.SetTexture("_BumpMap", normalMap);
            }
        }
        else if (string.IsNullOrEmpty(uGCMat.umat))
        {
            UGCTexManager.Inst.ReplaceUGCTex(mat);
        }
    }
}

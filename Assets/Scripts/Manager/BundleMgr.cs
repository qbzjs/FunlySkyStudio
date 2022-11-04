
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SavingData;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class BundleMappingInfo
{
    public string bundleName;
    public string fileName;
}

//这里比较坑 因为目前的classifyType有很多冗余字段 服务器字段没有一一对应 也不好新增类型
//所以用这个枚举来代替classifyType
public enum BundlePart
{
    Clothes = 10,
    Hats = 11,
    Glasses = 12,
    Bag = 15,
    Hand = 18,
    Special = 21,
    Shoe = 13,
    Hair = 8,
    Eyes = 3,
    Brow = 4,
    Nose = 5,
    Mouse = 6,
    Face = 7,
    Accessoies = 14,
    Pattern = 17,
    Effect = 28,
    Crossbody = 27,
    Respgc = 101,
    PartCount
}

public enum BundleLoadStatus
{
    Success,
    Error,
    Idle
}

public class ConfigLoadingTask
{
    public ConfigVersion cv;
    public bool loadSuccess;
}

public class LoadingTask
{
    public BundlePart cp;
    public string bundleName;
    public string templateName;
    public Action<BundleWrapper> succ;
    public Action fail;
    public BundleLoadStatus bundleLoadStatus = BundleLoadStatus.Idle;
    public string url;
    public BundleWrapper bw;
    public bool localLoadSucc;
}

public class FileExist
{
    public bool res;
}

public class BundleMgr : InstMonoBehaviour<BundleMgr>
{
    public static string bundleListJson = "BundleFileList.json";
    public static string AbDownloadInfoFile = "ABDownloadInfo.json";
    private string _cacheFileListJson = "CacheFileList.json";

    private string _characterResPath = "Bundle/CharacterResPath";
    private string _editorCharacterResPath = "Assets/Res/Bundle";
    private string _localResPath = "";
    private string _romoteUrl = "";

    private Dictionary<string, List<BundleMappingInfo>> _bundleFileList =
        new Dictionary<string, List<BundleMappingInfo>>();

    private Dictionary<string, LoadingTask> _loadTasksHandles = new Dictionary<string, LoadingTask>();
    private Dictionary<string, BundleWrapper> _bundleCache = new Dictionary<string, BundleWrapper>(512);
    private List<string> _fileInfos = new List<string>();
    private int _maxCacheBundleLen = 300;


    public void LoadBundleList(string androidBundleFileList)
    {
        LoadBundleList_Internal(bundleListJson, androidBundleFileList);
    }

    void Awake()
    {
        //DontDestroyOnLoad(this);
        this.gameObject.DontDestroy();
        _bundleCache.Clear();
        _loadTasksHandles.Clear();
        try
        {

            var bDir = Path.Combine(Application.persistentDataPath, _characterResPath);
            if (!Directory.Exists(bDir)) Directory.CreateDirectory(bDir);
            var bFile = Path.Combine(bDir, _cacheFileListJson);
            if (File.Exists(bFile))
            {
                _fileInfos = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(bFile));
            }
            else
            {
                File.WriteAllText(bFile, JsonConvert.SerializeObject(_fileInfos));
            }

            _romoteUrl = ResPathTool.GetBundleUrlPrefix();
            _localResPath =
                Path.Combine(Application.streamingAssetsPath, "assetbundle", "Res",
                    ResPathTool.GetPlatform());

        }
        catch (Exception e)
        {
            LoggerUtils.LogError($"CharacterResLoader::Awake {e}");
        }
        LoggerUtils.Log($"bundle localUrl:{_localResPath}");
        LoggerUtils.Log($"bundle romoteUrl:{_romoteUrl}");
    }

    private void InjectBundleList(string content)
    {
        var jobj = (JObject)JsonConvert.DeserializeObject(content);
        foreach (var it in jobj)
        {
            if (!_bundleFileList.ContainsKey(it.Key))
            {
                var lst = new List<BundleMappingInfo>();
                lst.AddRange(it.Value.Select(s => new BundleMappingInfo()
                {
                    bundleName = s["bundleName"].ToString(),
                    fileName = s["fileName"].ToString()
                }));
                _bundleFileList.Add(it.Key, lst);
            }
        }
    }

    private void LoadBundleList_Internal(string rPath, string androidFileListString = null)
    {
        _bundleFileList.Clear();
        var bundleListFile = Path.Combine(_localResPath, rPath);
        LoggerUtils.Log($"Load bundleList in {bundleListFile}");
        string bundleJsonString = "";

        var k = ResPathTool.GetBundleFileList();
        if (ResManager.Inst.IsResInConfigInPersistent(k))
        {
            var file = ResManager.Inst.GetConfigInPersistentPath(k);
            if (File.Exists(file))
            {
                bundleJsonString = File.ReadAllText(file);
            }
        }
        else
        {
#if UNITY_EDITOR || UNITY_IOS
            if (File.Exists(bundleListFile))
            {
                bundleJsonString = File.ReadAllText(bundleListFile);
            }
            else
            {
                LoggerUtils.LogError($"Load {bundleListJson} fail file not found in {bundleListFile}");
            }
#else
            bundleJsonString = androidFileListString;
#endif
        }

        if (!string.IsNullOrEmpty(bundleJsonString))
        {
            InjectBundleList(bundleJsonString);
            LoggerUtils.Log($"Load {bundleListJson} success!");
        }
        else
        {
            LoggerUtils.LogError($"Load bundleFileList fail bundleJsonString is null");
        }
    }


    public void LoadBundle(BundlePart cp, string templateName, Action<BundleWrapper> succ = null,
        Action fail = null, string url = null)
    {
        var task = new LoadingTask()
        {
            cp = cp,
            succ = succ,
            fail = fail,
            templateName = templateName,
            url = url
        };

        if (TaskIsLoading(task)) return;
        _loadTasksHandles.Add(task.templateName, task);

        if (_bundleCache.TryGetValue(task.templateName, out var ab))
        {
            LoggerUtils.Log($"bundle {task.templateName} load from cache!");
            try
            {
                task.bw = ab;
                OnLoadFinish(true, task);
            }
            catch (Exception e)
            {
                LoggerUtils.LogError($"CharacterResLoader::LoadCharacterRes {e}");
            }
            return;
        }


#if UNITY_EDITOR
        StartCoroutine(LoadResInEditor(task));
#else
        StartCoroutine(LoadResInTerminal(task));
#endif
    }

    public BundleWrapper LoadBundle(BundlePart cp, string templateName)
    {
        if (_bundleCache.TryGetValue(templateName, out var ab))
        {
            return ab;
        }

        var task = new LoadingTask()
        {
            cp = cp,
            templateName = templateName,
        };

#if UNITY_EDITOR
#if !LOAD_FROM_BUNDLE
            var bdWrapper = new BundleWrapper(task.cp, task.templateName);
            task.bw = bdWrapper;
            OnLoadFinish(true, task);
            return task.bw;
#else
        if (_bundleFileList.Count <= 0)
        {
            var bundleListFile = Path.Combine(_localResPath, bundleListJson);
            if (File.Exists(bundleListFile))
            {
                var bundleJsonString = File.ReadAllText(bundleListFile);
                InjectBundleList(bundleJsonString);
            }
        }
        var bundleInfo = GetBundleInfo(task);
        task.bundleName = bundleInfo.bundleName;
        var bundleFile = Path.Combine(_localResPath, task.bundleName);
        var assetBundle = AssetBundle.LoadFromFile(bundleFile);
        task.bw = new BundleWrapper(assetBundle);
        OnLoadFinish(true, task);
        return task.bw;
#endif

#else
        var bundleInfo = GetBundleInfo(task);
        task.bundleName = bundleInfo.bundleName;
        var bundleFile = Path.Combine(_localResPath, task.bundleName);
        var assetBundle = AssetBundle.LoadFromFile(bundleFile);
        task.bw = new BundleWrapper(assetBundle);
        OnLoadFinish(true, task);
        return task.bw;
#endif
    }

    private IEnumerator LoadResInEditor(LoadingTask task)
    {
#if LOAD_FROM_BUNDLE
        if (_bundleFileList.Count == 0) LoadBundleList_Internal(bundleListJson);
        yield return LoadResInTerminal(task);
#else
        var bdWrapper = new BundleWrapper(task.cp, task.templateName);
        task.bw = bdWrapper;
        OnLoadFinish(true, task);
        yield break;
#endif
    }

    private IEnumerator LoadResInTerminal(LoadingTask task)
    {
        var bundleInfo = GetBundleInfo(task);
        if (bundleInfo == null)
        {
            OnLoadFinish(false, task);
            yield break;
        }
        task.bundleName = bundleInfo.bundleName;
        
        yield return TryLoadFromLocal(task);

        if (task.localLoadSucc)
        {
            OnLoadFinish(true, task);
            yield break;
        }

        LoggerUtils.Log(
            $"not found bundle {task.bundleName}|{task.templateName} in local, download from remote!");
        yield return StartCoroutine(LoadRemoteResBundle(task));
    }

    private bool TaskIsLoading(LoadingTask task)
    {
        if (_loadTasksHandles.TryGetValue(task.templateName, out var v))
        {
            v.succ += task.succ;
            v.fail += task.fail;
            LoggerUtils.Log($"{task.templateName} is loading...");
            return true;
        }

        return false;
    }

    private IEnumerator TryLoadFromLocal(LoadingTask task)
    {
        task.localLoadSucc = false;
        //从streamingassets加载
        var bundleFile = Path.Combine(_localResPath, task.bundleName);
        var res = new FileExist();
        yield return CheckIfFileExist(res, bundleFile);
        AssetBundle assetBundle = null;
        if (res.res)
        {
            LoggerUtils.Log($"bundle {task.bundleName}|{task.templateName} load from streamingassets!");
            try
            {
                assetBundle = AssetBundle.LoadFromFile(bundleFile);
            }
            catch (Exception e)
            {
                LoggerUtils.LogError($"CharacterResLoader::TryLoadFromLocal {e}");
            }

            if (assetBundle != null)
            {
                task.bw = new BundleWrapper(assetBundle);
                task.localLoadSucc = true;
                yield break;
            }
        }

        //从持久化目录加载
        bundleFile = Path.Combine(Application.persistentDataPath, _characterResPath, task.bundleName);
        if (File.Exists(bundleFile))
        {
            LoggerUtils.Log($"bundle {task.bundleName}|{task.templateName} load from persistentDataPath {bundleFile}");
            try
            {
                assetBundle = AssetBundle.LoadFromFile(bundleFile);
            }
            catch (Exception e)
            {
                LoggerUtils.LogError($"CharacterResLoader::TryLoadFromLocal {e}");
            }

            if (assetBundle != null)
            {
                task.bw = new BundleWrapper(assetBundle);
                task.localLoadSucc = true;
            }
        }
    }

    private IEnumerator LoadRemoteResBundle(LoadingTask task)
    {
        int requestCount = 0;
        if (string.IsNullOrEmpty(task.url))
        {
            task.url = Path.Combine(_romoteUrl, task.bundleName);
        }

        while (requestCount < 3 && task.bundleLoadStatus != BundleLoadStatus.Success)
        {
            yield return RequestRemoteBundle(task);
            requestCount++;
        }

        if (task.bundleLoadStatus != BundleLoadStatus.Success)
        {
            OnLoadFinish(false, task);
        }
    }

    private IEnumerator RequestRemoteBundle(LoadingTask task)
    {

        using (var req = UnityWebRequest.Get(task.url))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                task.bundleLoadStatus = BundleLoadStatus.Error;
                LoggerUtils.LogError($"Load {task.templateName} fail: {req.error} ");
            }
            else
            {
                task.bundleLoadStatus = BundleLoadStatus.Success;
                BundleWrapper ab = null;
                ab = new BundleWrapper(AssetBundle.LoadFromMemory(req.downloadHandler.data));
                task.bw = ab;
                OnLoadFinish(true, task);
                SaveBundle(task.bundleName, req.downloadHandler.data);
            }
        }
    }

    private void CacheBundle(LoadingTask task)
    {
        if (!_bundleCache.ContainsKey(task.templateName))
        {
            _bundleCache.Add(task.templateName, task.bw);
            if (_bundleCache.Count >= _maxCacheBundleLen)
            {
                int c = (int)(_maxCacheBundleLen * 0.3);
                ReleaseBundle(c);
            }
        }

        LoggerUtils.Log($"Load bundle {task.templateName} success!");
    }

    private BundleMappingInfo GetBundleInfo(LoadingTask task)
    {
        if (_bundleFileList.TryGetValue(task.cp.ToString(), out var v))
        {
            var bundleMappingInfo = v.Find(i => i.fileName == task.templateName);
            if (bundleMappingInfo != null) return bundleMappingInfo;
        }

        LoggerUtils.LogError($"{task.cp.ToString()} {task.templateName} Bundle Not Found!");
        return null;
    }

    private void SaveBundle(string bundleName, byte[] rawData)
    {
        var bDir = Path.Combine(Application.persistentDataPath, _characterResPath);
        if (!Directory.Exists(bDir)) Directory.CreateDirectory(bDir);


        if (_fileInfos.Count >= _maxCacheBundleLen)
        {
            var c = (int)(_fileInfos.Count * 0.3);
            for (int i = 0; i < c; ++i)
            {
                try
                {
                    var f = Path.Combine(bDir, _fileInfos[i]);
                    if (File.Exists(f))
                    {
                        File.Delete(f);
                    }
                }
                catch (Exception e)
                {
                    LoggerUtils.LogError($"RemoveBundle err {e}");
                }
            }

            _fileInfos.RemoveRange(0, c);

            Debug.Log($"Delete {c}files form {bDir}");
        }

        var abFilePath = Path.Combine(bDir, bundleName);
        try
        {
            File.WriteAllBytes(abFilePath, rawData);
            _fileInfos.Add(Path.GetFileName(abFilePath));
        }
        catch (Exception e)
        {
            LoggerUtils.LogError($"SaveBundle {e}");
        }
        File.WriteAllText(Path.Combine(bDir, _cacheFileListJson), JsonConvert.SerializeObject(_fileInfos));
        LoggerUtils.Log($"savebundle in {abFilePath}");
    }

    private void ReleaseBundle(int c)
    {
        int i = 0;
        List<string> t = new List<string>(256);
        foreach (var item in _bundleCache)
        {
            if (i < c)
            {
                t.Add(item.Key);
                i++;
            }
            else
            {
                break;
            }
        }

        foreach (var item in t)
        {
            _bundleCache[item].Release(false);
            _bundleCache.Remove(item);
        }

        LoggerUtils.Log($"Exec ReleaseBundle {c}");
    }

    private void OnLoadFinish(bool succ, LoadingTask task)
    {
        try
        {
            if (succ)
            {
                OnLoadSucc(task);
            }
            else
            {
                OnLoadFail(task);
            }
        }
        catch (Exception e)
        {
            LoggerUtils.LogError($"CharacterResLoader::OnLoadFinish {e}");
        }
        finally
        {
            if (_loadTasksHandles.ContainsKey(task.templateName)) _loadTasksHandles.Remove(task.templateName);
        }
    }

    private void OnLoadSucc(LoadingTask task)
    {
        try
        {
            task.succ?.Invoke(task.bw);
        }
        catch (Exception e)
        {
            LoggerUtils.LogError($"CharacterResLoader::OnLoadSucc {e}");
        }
        finally
        {
            CacheBundle(task);
        }
    }


    private void OnLoadFail(LoadingTask task)
    {
        try
        {
            task.fail?.Invoke();
        }
        catch (Exception e)
        {
            LoggerUtils.LogError($"CharacterResLoader::OnLoadFail {e}");
        }
    }


    private IEnumerator CheckIfFileExist(FileExist fileExist, string file)
    {
        fileExist.res = false;
#if UNITY_ANDROID
        using (var req = UnityWebRequest.Get(file))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                fileExist.res = true;
            }
        }
#else
        if (File.Exists(file))
        {
            fileExist.res = true;
        }
#endif
        yield return null;
    }

    private void OnDestroy()
    {
        foreach (var item in _bundleCache)
        {
            if (item.Value != null) item.Value.Release(true);
        }

        _bundleCache?.Clear();
        inst = null;
    }
}

public class BundleWrapper
{
    private string localResPath = "Assets/Res/Bundle";
    private string _resFolder;

    public BundleWrapper(BundlePart part, string templateName)
    {
        _resFolder = Path.Combine(localResPath, part.ToString(), templateName);
    }

    private AssetBundle _ab;

    public BundleWrapper(AssetBundle ab)
    {
        _ab = ab;
    }


    public T LoadAsset<T>(string name) where T : Object
    {
#if !LOAD_FROM_BUNDLE && UNITY_EDITOR

        var assets = AssetDatabase.FindAssets("", new string[1] { _resFolder }).Select(AssetDatabase.GUIDToAssetPath)
            .ToArray();
        ;
        foreach (var at in assets)
        {
            if (name == Path.GetFileNameWithoutExtension(at))
            {
                var g = AssetDatabase.LoadAssetAtPath<T>(at);
                if (g != null) return g;
            }
        }

        LoggerUtils.Log($"not found {typeof(T).Name} {name} in {_resFolder}!");
        return null;
#else
        return _ab.LoadAsset<T>(name);
#endif
    }

    public void Release(bool unLoadAllLoadedObj)
    {
#if !LOAD_FROM_BUNDLE && UNITY_EDITOR

#else
        if (_ab != null) _ab.Unload(unLoadAllLoadedObj);
#endif
    }
}


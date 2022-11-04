using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using SavingData;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.U2D;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public enum VersionConfigType
{
    Resources,
    PersistentData,
    Remote
}

public class ConfigVersionPair
{
    public VersionConfigType versionConfigType;
    public ConfigVersion cv;
}

public class ResManager : CInstance<ResManager>
{
    private Dictionary<string, Object> resPool = new Dictionary<string, Object>();
    private SpriteAtlas gameAltas;
    private Dictionary<string, string> configInPersistent = new Dictionary<string, string>();

    
    public IEnumerator LoadConfig(string latestConfigVersionsString, Action onSucc = null, Action onFail = null)
    {
        bool res = true;
        var versionPairs = new List<ConfigVersionPair>();
        var localConfigVersionOnly = LoadJsonRes<object>("Configs/LocalConfigVersionOnly") != null;
        LoggerUtils.Log($"latestConfigVersionsString:{latestConfigVersionsString} localConfigVersionOnly:{localConfigVersionOnly}");
        var streamingAssetsConfig = LoadJsonRes<List<ConfigVersion>>("Configs/ConfigVersions");
        
        try
        {
            foreach (var smConfig in streamingAssetsConfig)
            {
                versionPairs.Add(new ConfigVersionPair(){cv = smConfig, versionConfigType = VersionConfigType.Resources});
            }
        
            var persistentConfigFile = ResPathTool.GetConfigPersistentDataPathFile();
            if (File.Exists(persistentConfigFile))
            {
                var persistentConfigVersions = 
                    JsonConvert.DeserializeObject<List<ConfigVersion>>(File.ReadAllText(persistentConfigFile));
                ReplaceLatestVersionConfig(versionPairs, persistentConfigVersions, VersionConfigType.PersistentData);
            }
        
            if (!string.IsNullOrEmpty(latestConfigVersionsString) && !localConfigVersionOnly)
            {
                var latestVersionConfig = JsonConvert.DeserializeObject<List<ConfigVersion>>(latestConfigVersionsString);
                ReplaceLatestVersionConfig(versionPairs, latestVersionConfig, VersionConfigType.Remote);
            }
            else
            {
                res = false;
                LoggerUtils.LogError("ResManager::LoadConfig latestConfigVersions is null, use local configVersions");
            }
        }
        catch (Exception e)
        {
            LoggerUtils.LogError(e);
            versionPairs.Clear();
            res = false;
            throw;
        }
        
        foreach (var pair in versionPairs)
        {
            switch (pair.versionConfigType)
            {
                case VersionConfigType.Resources:
                    break;
                case VersionConfigType.PersistentData:
                    LoadAllConfigPathToCache(Path.Combine(ResPathTool.GetConfigPersistentDataPath(), pair.cv.configGroupName));
                    break;
                case VersionConfigType.Remote:
                    var loadingTask = new ConfigLoadingTask() { cv = pair.cv, loadSuccess = true };
                    yield return DownloadRemoteConfig(loadingTask);
                    if (loadingTask.loadSuccess == false)
                    {
                        res = false;
                    }
                    break;
            }
        }

        if (res)
        {
            if (!Directory.Exists(ResPathTool.GetConfigPersistentDataPath())) Directory.CreateDirectory(ResPathTool.GetConfigPersistentDataPath());
            File.WriteAllText(ResPathTool.GetConfigPersistentDataPathFile(), latestConfigVersionsString); 
            onSucc?.Invoke();
        }
        else
        {
            onFail?.Invoke();
        }

    }
    
    public bool IsResInConfigInPersistent(string k)
    {
        return configInPersistent.ContainsKey(k);
    }
    public string GetConfigInPersistentPath(string k)
    {
        if (configInPersistent.ContainsKey(k)) return configInPersistent[k];
        return null;
    }
    private void ReplaceLatestVersionConfig(List<ConfigVersionPair> prePairs, List<ConfigVersion> curConfig, VersionConfigType configType)
    {
        if (prePairs == null)
        {
            LoggerUtils.LogError("prePairs == null");
            return;
        }
        if (curConfig == null)
        {
            LoggerUtils.LogError("curConfig == null");
            return;
        }

        foreach (var item in curConfig)
        {
            if (item == null)
            {
                LoggerUtils.LogError("ReplaceLatestVersionConfig::item == null");
                continue;
            }
            var preItem = prePairs.Find(i => i.cv.ID == item.ID);
            if (preItem != null)
            {
                if (preItem.cv.IsYoungerThan(item))
                {
                    preItem.cv = item;
                    preItem.versionConfigType = configType;
                }
            }
            else
            {
                prePairs.Add(new ConfigVersionPair{cv = item, versionConfigType = VersionConfigType.PersistentData});
            }
        }
    }
    private void LoadAllConfigPathToCache(string dir)
    {
        var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var k = file.Replace(dir, "").Replace("\\","/").Substring(1);
            var fName = Path.GetFileName(file);
            var fNameWithoutExt = Path.GetFileNameWithoutExtension(file);
            k = Path.Combine(k.Replace(fName, ""), fNameWithoutExt); 
            if (!configInPersistent.ContainsKey(k))
            {
                configInPersistent.Add(k, file);
            }
        }
    }
    
    private IEnumerator DownloadRemoteConfig(ConfigLoadingTask clt)
    {
        var cv = clt.cv;
        var fileName = $"{cv.configGroupName}_{cv.hash}_{cv.ver}.zip";
        var url = $"{ResPathTool.GetConfigUrlPrefix()}{fileName}";

        using (var req = UnityWebRequest.Get(url))
        {
            req.timeout = 3;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                clt.loadSuccess = false;
                LoggerUtils.LogError($"Load config {cv.configGroupName} fail: {url}|{req.error} ");
            }
            else
            {
                
                var dir = Path.Combine(ResPathTool.GetConfigPersistentDataPath(), cv.configGroupName);
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
                Directory.CreateDirectory(dir);
                var zipFile = Path.Combine(dir, fileName);
                File.WriteAllBytes(zipFile, req.downloadHandler.data);
                ZipUtils.UnZipDirectory(zipFile, dir);
                File.Delete(zipFile);
                clt.loadSuccess = true;
                LoadAllConfigPathToCache(dir);
                LoggerUtils.Log($"config {cv.configGroupName} update to ver {cv.ver} success!");
            }
        }

        yield return null;
    }

    public T LoadJsonRes<T>(string path)
    {
        string content = "";
        if(configInPersistent.ContainsKey(path)) content = File.ReadAllText(configInPersistent[path]);
        else
        {
            var f = LoadRes<TextAsset>(path);
            if (f != null)
            {
                content = f.text;
            }
            else
            {
                return default;
            }
        }
        return JsonConvert.DeserializeObject<T>(content);
    }

    public T LoadRes<T>(string path) where T : Object
    {
#if !UNITY_EDITOR
        T r = AssetBundleLoaderMgr.Inst.LoadRes<T>(path);
        if (r) return r;
#endif
        if (resPool.ContainsKey(path))
        {
            return (T)resPool[path];
        }

        var res = Resources.Load<T>(path);
        resPool.Add(path, res);
        return res;
    }

    public T LoadResNoCache<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    public Sprite GetGameAtlasSprite(string spriteName)
    {
        if (gameAltas == null)
        {
            gameAltas = LoadRes<SpriteAtlas>("Atlas/GameAtlas");
        }

        var sprite = gameAltas.GetSprite(spriteName);
        return sprite;
    }

    public void WriteJsonRes<T>(string path, T data) where T : class
    {
        string content = JsonConvert.SerializeObject(data);
        File.WriteAllText(path, content);
    }

    public T LoadCharacterRes<T>(string path) where T : Object
    {
        if (resPool.ContainsKey(path))
        {
            return (T)resPool[path];
        }

        var res = Resources.Load<T>(path);
        resPool.Add(path, res);
        return res;
    }

    public IEnumerator GetContent(string url, UnityAction<string> onSuccess, UnityAction<string> onFailure)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = 30;
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(www.downloadHandler.text);
        }
    }

    public IEnumerator GetZipContent(string url, UnityAction<string> onSuccess, UnityAction<string> onFailure)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = 30;
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            onFailure.Invoke(www.error);
        }
        else
        {
            LoggerUtils.LogReport("ZipUtils.SaveZipFromByte", "SaveZipFromByte");
            string jsonStr = ZipUtils.SaveZipFromByte(www.downloadHandler.data);
            if (string.IsNullOrEmpty(jsonStr))
            {
                onFailure.Invoke(null);
                yield break;
            }

            onSuccess.Invoke(jsonStr);
        }
    }

    public IEnumerator GetTexture(string url, UnityAction<Texture2D> onSuccess, UnityAction onFailure)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            onFailure?.Invoke();
        }
        else
        {
            var tex = DownloadHandlerTexture.GetContent(www);
            onSuccess?.Invoke(tex);
        }
    }

    public IEnumerator GetAudioClip(string url, UnityAction<AudioClip> onSuccess, UnityAction onFailure)
    {
        using (var uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            var suc = onSuccess;
            var fail = onFailure;
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                LoggerUtils.LogError("Unity uwr.result == " + (uwr.result));
                fail?.Invoke();
            }
            else
            {
                var clip = DownloadHandlerAudioClip.GetContent(uwr);
                LoggerUtils.Log("Unity onSuccess == null" + (onSuccess == null));
                suc?.Invoke(clip);
            }
        }
    }
}
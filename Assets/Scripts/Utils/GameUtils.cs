using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

// 需要替换的动画片段类型
public enum AnimClipType
{
    idle,
    Pickup,
    Discard,
    Walk,
    Fast_Run,
    Jump,
    pvp_attack,
    pvp_runattack,
    pvp_beattack,
    pvp_beattackback,
    mutual_run_left,
    mutual_run_right,
    Run_Jump,
    selfiestick_start,
    selfiestick_centre,
    selfiestick_end,
    BouncePlank,
    pvp_runbeattackback,
    trapbox_hit
}

[Serializable]
public class ClipItem
{
    public AnimClipType clipKey;
    public AnimationClip clipValue;
}

public class TypeDescAttribute : System.Attribute
{
    public int tDesc;
    public TypeDescAttribute(int desc)
    {
        tDesc = desc;
    }
}

/**
* 为了进行批量替换配合 AnimatorOverrideController.ApplyOverrides 方法的参数，自定义的片段重写列表类
*/
public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
            {
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
            }
        }
    }
}

public class GameUtils
{
    //需要替换的动画类型和动画名称对应的 dict
    public static Dictionary<AnimClipType, string> AnimTypeAndNameDict = new Dictionary<AnimClipType, string>()
    {
        {AnimClipType.idle, "idle"},
        {AnimClipType.Pickup, "collect"},
        {AnimClipType.Discard, "discard"},
        {AnimClipType.Walk, "run"},
        {AnimClipType.Fast_Run, "run_fast"},
        {AnimClipType.Jump, "jump"},
        {AnimClipType.pvp_attack, "pvp_attack"},
        {AnimClipType.pvp_runattack, "pvp_runattack"},
        {AnimClipType.pvp_beattack, "pvp_beattack"},
        {AnimClipType.pvp_beattackback, "pvp_beattackback"},
        {AnimClipType.mutual_run_left, "pull_run_left"},
        {AnimClipType.mutual_run_right, "pull_run_right"},
        {AnimClipType.Run_Jump, "runjump24"},
        {AnimClipType.selfiestick_start, "selfiestick_start"},
        {AnimClipType.selfiestick_centre, "selfiestick_centre"},
        {AnimClipType.selfiestick_end, "selfiestick_end"},
        {AnimClipType.BouncePlank, "bounceplank"},
        {AnimClipType.pvp_runbeattackback, "pvp_runbeattackback"},
        {AnimClipType.trapbox_hit, "trapbox_hit"},
    };

    private static Vector3 selectScale = new Vector3(0.66f, 0.66f, 0.66f);

    //下载队列参数
    private static int loadingCount;
    private const int loadingQueue = 6;

    public static void Clear()
    {
        LoggerUtils.Log("GameUtils clear");
        loadingCount = 0;
    }

    private static bool IsCanEnterLoadQueue()
    {
        if (loadingCount < loadingQueue)
        {
            loadingCount++;
            return true;
        }
        return false;
    }

    private static void ExitLoadQueue()
    {
        loadingCount--;
    }

    public static long GetTimeStamp()
    {
        var untilNow = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return (long)untilNow.TotalSeconds;
    }

    public static string GetTimeDay()
    {
        return System.DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static string GetCurTimeStr()
    {
        return DateTime.Now.ToString("HH:mm:ss:fff", CultureInfo.InvariantCulture);
    }

    public static string GetTimeStrByStamp(double stamp)
    {
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); //当地时区  
        var dt = startTime.AddMilliseconds(stamp);
        return dt.ToString("HH:mm:ss:fff", CultureInfo.InvariantCulture);
    }

    
    public static long GetUtcTimeStamp()
    {
        var untilNow = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return (long)untilNow.TotalSeconds;
    }

    public static TimeSpan GetUtcTimeStampAsSpan()
    {
        return DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
    }

    public static long GetMilliTimeStamp()
    {
        var untilNow = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return (long)untilNow.TotalMilliseconds;
    }

    public static int GetDeltaTime(TimeSpan startTime, TimeSpan endTime)
    {
        return (int)(endTime - startTime).TotalSeconds;
    }

    public static long GetSystemTime() {
        return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
    }
    // 按照某个长度，截取文本。多余的显示为 ...
    public static string SetText(string val, int textLimit = 20)
    {
        if (string.IsNullOrEmpty(val))
        {
            return val;
        }
        string text = val.Length > textLimit ? val.Substring(0, textLimit) + "..." : val;
        return text;
    }
    
    public static T GetAttr<T>(int key,List<BehaviorKV> behavs)
    {
        var kv = behavs.Find(x => x.k == key);
        if (kv != null)
        {
            return JsonConvert.DeserializeObject<T>(kv.v);
        }
        return default(T);
    }


    // 通过 uidList 批量获取玩家信息
    public static void GetUserInfoByUid(List<string> uidList, UnityAction<string> onReceive, UnityAction<string> onFail)
    {
        string uidStr = "";
        for (int i = 0; i < uidList.Count; i++)
        {
            if (i == 0)
            {
                uidStr = uidList[i];
            }
            else
            {
                uidStr = uidStr + "," + uidList[i];
            }
        }

        LoggerUtils.Log("GameUtils GetUserInfoByUid uidStr: " + uidStr);
        BatchGetUserInfo req = new BatchGetUserInfo();
        req.uids = uidList.ToArray();
        LoggerUtils.Log("GameUtils GetUserInfoByUid JsonUtility.ToJson(req) = " + JsonUtility.ToJson(req));
        HttpUtils.MakeHttpRequest("/image/batchImage", (int)HTTP_METHOD.POST, JsonUtility.ToJson(req), onReceive, onFail);
    }

    public static HttpResponseRaw GetHttpResponseRaw(string content)
    {
        try
        {
            HttpResponseRaw responseDataRaw = JsonConvert.DeserializeObject<HttpResponseRaw>(content);
            return responseDataRaw;
        }
        catch
        {
            return new HttpResponseRaw() {
                result = -1
            };
        }
    }


    //获取当前位置 地形使用的texture的下标数组
    public static float[] GetTerrainTextureMix(Vector3 worldPos)
    {
        Terrain terrain = Terrain.activeTerrain;
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;
        int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
        float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];
        for (int n = 0; n < cellMix.Length; ++n)
        {
            cellMix[n] = splatmapData[0, 0, n];
        }
        return cellMix;
    }

    //根据当前位置获取地形对应texure 的index
    public static int GetTrrainTextureIndex(Vector3 worldPos)
    {
        float[] mix = GetTerrainTextureMix(worldPos);
        float maxMix = 0;
        int maxIndex = 0;
        // loop through each mix value and find the maximum
        for (int n = 0; n < mix.Length; ++n)
        {
            if (mix[n] > maxMix)
            {
                maxIndex = n;
                maxMix = mix[n];
            }
        }
        return maxIndex;
    }

    public static IEnumerator LoadTextureByQueue(string url, Action<Texture> onSuccess, Action<string> onFailure)
    {
        yield return new WaitUntil(IsCanEnterLoadQueue);
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }
        UnityWebRequest www = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        www.downloadHandler = texDl;
        www.timeout = 15;
        yield return www.SendWebRequest();
        ExitLoadQueue();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log("LoadSpriteError" + www.error);
            onFailure?.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(texDl.texture);
        }
        texDl.Dispose();
        www.Dispose();
    }

    public static IEnumerator LoadTexture(string url, Action<Texture> onSuccess, Action<string> onFailure)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }
        UnityWebRequest www = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        www.downloadHandler = texDl;
        www.timeout = 45;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log("LoadSpriteError" + www.error);
            onFailure?.Invoke(www.error); 
        }
        else
        {
            onSuccess.Invoke(texDl.texture);
        }
        texDl.Dispose();
        www.Dispose();
    }

    public static IEnumerator LoadTexture2D(string url, Action<Texture2D> onSuccess, Action<string> onFailure)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }
        UnityWebRequest www = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        www.downloadHandler = texDl;
        www.timeout = 45;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log("LoadSpriteError" + www.error);
            onFailure?.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(texDl.texture);
        }
        texDl.Dispose();
        www.Dispose();
    }

    public static IEnumerator GetText(string url, Action<string> onSuccess, Action<string> onFailure)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = 45;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log(www.error);
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(www.downloadHandler.text);
        }
    }

    public static IEnumerator GetByte(string url, Action<byte[]> onSuccess, Action<string> onFailure)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.timeout = 45;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log(www.error);
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(www.downloadHandler.data);
        }
    }

    public static UserInfo CloneUserInfo(UserInfo source)
    {
        UserInfo newUserInfo = new UserInfo();
        newUserInfo.uid = source.uid;
        newUserInfo.userNick = source.userNick;
        newUserInfo.userName = source.userName;
        newUserInfo.gender = source.gender;
        newUserInfo.portraitUrl = source.portraitUrl;
        newUserInfo.imageJson = source.imageJson;
        newUserInfo.birthday = source.birthday;
        newUserInfo.officialCert = source.officialCert;
        newUserInfo.clothesId = source.clothesId;
        newUserInfo.clothesIsBan = source.clothesIsBan;
        newUserInfo.facePaintingIsBan = source.facePaintingIsBan;
        newUserInfo.dcUgcInfos = source.dcUgcInfos;
        newUserInfo.dcPgcInfos = source.dcPgcInfos;
        return newUserInfo;
    }

    //按60FPS 计算速度
    public static float GetFixSpeed()
    {
        return Time.deltaTime / 0.0167f;
    }

    /// <summary>
    /// 按字节数截取字符串的方法(比SubString好用)
    /// </summary>
    /// <param name="source">要截取的字符串（可空）</param>
    /// <param name="NumberOfBytes">要截取的字节数</param>
    /// <param name="encoding">System.Text.Encoding</param>
    /// <param name="suffix">结果字符串的后缀（超出部分显示为该后缀）</param>
    /// <returns></returns>
    public static string SubStringByBytes(string source, int NumberOfBytes, System.Text.Encoding encoding, string suffix = "...")
    {
        if (string.IsNullOrWhiteSpace(source) || source.Length == 0)
            return source;

        if (encoding.GetBytes(source).Length <= NumberOfBytes)
            return source;

        long tempLen = 0;
        StringBuilder sb = new StringBuilder();
        foreach (var c in source)
        {
            Char[] _charArr = new Char[] { c };
            byte[] _charBytes = encoding.GetBytes(_charArr);
            if ((tempLen + _charBytes.Length) > NumberOfBytes)
            {
                if (!string.IsNullOrWhiteSpace(suffix))
                    sb.Append(suffix);
                break;
            }
            else
            {
                tempLen += _charBytes.Length;
                sb.Append(encoding.GetString(_charBytes));
            }
        }
        return sb.ToString();
    }

    public static void ChangeToTargetLayer(string targetLayer, Transform parentTF, bool includeChild = true)
    {
        if (LayerMask.NameToLayer(targetLayer) == -1)
        {
            LoggerUtils.LogError("targetLayer is not exist");
            return;
        }
        if (parentTF == null)
        {
            LoggerUtils.LogError("ChangeToTargetLayer --> parentTF == null");
            return;
        }
        if (includeChild)
        {
            Transform[] childTFs = parentTF.GetComponentsInChildren<Transform>(true);
            foreach (Transform childTF in childTFs)
            {
                childTF.gameObject.layer = LayerMask.NameToLayer(targetLayer);
            }
        }
        else
        {
            parentTF.gameObject.layer = LayerMask.NameToLayer(targetLayer);
        }
    }
    public static Vector2 GetUIPointByScreenPoint(Canvas canvas,Vector2 dstPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, dstPos, canvas.worldCamera, out Vector2 result);
        return result;
    }

    //获取屏幕自动适配缩放值
    public static float GetAutoFixScale()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        LoggerUtils.Log("screen:" + screenWidth + "/" + screenHeight);
        float screenRatio = screenWidth / screenHeight;//屏幕宽高比
        float minRatio = 1.78f;//缩放刚好合适时的宽高比=iphone6宽高比
        float zoomScale = screenRatio / minRatio;//缩放比例
        if(screenRatio > minRatio){
            zoomScale = 1;
        }
        return zoomScale;
    }
    
    //格式化json字符串
    public static string ConvertStringToFormatJson(string str)
    {
        JsonSerializer serializer = new JsonSerializer();
        TextReader tr = new StringReader(str);
        JsonTextReader jtr = new JsonTextReader(tr);
        object obj = serializer.Deserialize(jtr);
        if (obj != null)
        {
            StringWriter textWriter = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 1,
                IndentChar = '\t',
            };
            serializer.Serialize(jsonWriter, obj);
            return textWriter.ToString();
        }
        else
        {
            return str;
        }
    }

    /// <summary>
    /// 将版本号转为 int 类型的值方便后续与后端传来的版本值(int)进行比较
    /// 约定是版本号为：major.minor.patch 三段式(如：1.36.0)
    /// </summary>
    /// <returns>转换后的 int 值</returns>
    public static int GetVersionIntByStr(string version)
    {
        var versionList = version.Split('.');
        if (versionList.Length != 3)
        {
            return 0;
        }
        int major, minor, patch;
        if (!int.TryParse(versionList[0], out major))
        {
            return 0;
        }

        if (!int.TryParse(versionList[1], out minor))
        {
            return 0;
        }

        if (!int.TryParse(versionList[2], out patch))
        {
            return 0;
        }

        var targetVersion = major << 24 | minor << 16 | patch << 8;
        return targetVersion;
    }

    /// <summary>
    /// 将后端给的版本值(int)转回三段式版本号字符串
    /// </summary>
    /// <param name="version"> 版本值</param>
    /// <returns>转换后的字符串</returns>
    public static string GetVersionStrByInt(int version)
    {
        string versionStr = string.Format("{0}.{1}.{2}", version >> 24, ((255 << 16) & version) >> 16, ((255 << 8) & version) >> 8);
        return versionStr;
    }

    public static Transform FindChildByName(Transform parent,string name)
    {
        Transform child = parent.Find(name);
        if(child != null)
        {
            return child;
        }
        for(int i = 0;i<parent.childCount;i++)
        {
            child = FindChildByName(parent.GetChild(i),name);
            if(child !=null)
            {
                break;
            }
        }
        return child;
    }

    public static Transform FindChildByName(GameObject parent,string name)
    {
        return FindChildByName(parent.transform,name);
    }
    public static int JavaHashCodeIgnoreCase( string s)
    {
        return JavaHashCode(s,s.Length);
    }
    public static int JavaHashCodeIgnoreCaseEraseExt( string s)
    {
        int fIndex = s.LastIndexOf('.');
        int len = fIndex > 0 ? fIndex : s.Length;
        return JavaHashCode(s,len);
    }
    static int JavaHashCode(string s, int len)
    {
        int h = 0;
        if (len > 0)
        {
            int off = 0;

            for (int i = 0; i < len; i++)
            {
                char c = s[off++];
                if (c >= 'A' && c <= 'Z')
                {
                    c += (char)('a' - 'A');
                }
                h = 31 * h + c;
            }
        }
        return h;
    }

    /// <summary>
    /// 平行光，点光源，聚光灯等颜色选择方法
    /// </summary>
    /// <param name="index"></param>
    /// <param name="gameObjects"></param>
    public static void SetSelect(int index, List<GameObject> gameObjects)
    {
        if (index < 0 || index >= gameObjects.Count)
        {
            LoggerUtils.LogError("Mat ID is Error");
            return;
        }
        gameObjects.ForEach(x =>
        {
            x.SetActive(false);
            var item = x.transform.parent;
            if (item)
            {
                var bg = item.transform.Find("bg").gameObject;
                bg.transform.localScale = Vector3.one;
            }
        });
        gameObjects[index].SetActive(true);
        var item = gameObjects[index].transform.parent;
        if (item)
        {
            var bg = item.transform.Find("bg").gameObject;
            bg.transform.localScale = selectScale;
        }
    }

    public static float stayTime = 0; // 累计拖动的时间

    public static void ResetStayTime()
    {
        stayTime = 0;

        PlayerBaseControl.Inst.canUseAutoMateMode = false;
    }


    //判断世界坐标是否在 Collider 范围内(考虑物体的旋转)
    public static bool Contains( BoxCollider boxCollider, Vector3 worldPosition)
    {
        Vector3 tgtPos = worldPosition;
        Vector3 rootPos = boxCollider.transform.position;
        //以collider为中心计算目标坐标点的向量，反向旋转之后再转换为自己的真正坐标。
        Vector3 posVec = tgtPos - rootPos;
        Quaternion rot = boxCollider.transform.rotation;
        rot = Quaternion.Inverse(rot);
        //旋转(rot * posVec)之后转换为目标点的世界坐标（+ rootPos），之后再判断包含关系
        return ContainsAABB(boxCollider,(rot * posVec) + rootPos);
    }

    //判断一个点是否包含在自身所限定的aabb盒内 
    public static bool ContainsAABB(BoxCollider boxCollider, Vector3 worldPosition)
    {
        //计算盒子的中心点世界坐标
        Vector3 center = boxCollider.transform.position + boxCollider.center;
        //计算中心点到每个方向边界的大小（半径）
        //这里同时考虑了collider上设置的尺寸以及物体的全局缩放值
        Vector3 hafSize = Mul(boxCollider.size, boxCollider.transform.lossyScale) * 0.5f;
        Vector3 max = center + hafSize;
        Vector3 min = center - hafSize;
        Vector3 tgt = worldPosition;
        //如果小于等于最大值并且大于等于最小值，说明这个点在盒子范围内
        return LessThanMax(max, tgt) && MoreThanMin(min, tgt);
    }

    static bool LessThanMax(Vector3 max, Vector3 target)
    {
        return (target.x <= max.x && target.y <= max.y && target.z <= max.z);
    }

    static bool MoreThanMin(Vector3 min, Vector3 target)
    {
        return (target.x >= min.x && target.y >= min.y && target.z >= min.z);
    }

    static Vector3 Mul(Vector3 size, Vector3 lossyScale)
    {
        return new Vector3(size.x * lossyScale.x, size.y * lossyScale.y, size.z * lossyScale.z);
    }
    
    public static List<T> GetBehaviourInFirstLayer<T>(Transform node)
    {
        List<T> behaviours = new List<T>();
        for (int i = 0; i < node.childCount; i++)
        {
            T nBehaviour = node.GetChild(i).GetComponent<T>();
            if (nBehaviour != null)
            {
                behaviours.Add(nBehaviour);
            }
        }
        return behaviours;
    }

    public static void CloseAllMesh(NodeBaseBehaviour baseBev)
    {
        var meshList = baseBev.GetComponentsInChildren<MeshRenderer>();
        foreach(var mesh in meshList)
        {
            mesh.enabled = false;
        }
    }
}

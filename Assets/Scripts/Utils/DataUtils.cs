using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class DataUtils
{
    private static Vector3 colorMultiplier = new Vector3(1.3f, 1.3f, 1.3f);
    public static string dataDir => Application.persistentDataPath + "/U3D/";
    public static string crashDataDir => Application.persistentDataPath + "/U3DCrash/";
    public static string ExceptionReportFileName => "ExceptionReport.json";
#if UNITY_EDITOR
    public static string ugcClothesDataDir => Application.streamingAssetsPath + "/U3D/UGCClothes/";
    public static string ugcDataDir => Application.streamingAssetsPath + "/U3D/UGCData/";
    public static string DraftPath => Application.streamingAssetsPath + "/U3D/UGCClothes/";
#else
    public static string ugcClothesDataDir => Application.persistentDataPath + "/U3D/UGCClothes/";
    public static string ugcDataDir => Application.persistentDataPath + "/U3D/UGCData/";
    public static string DraftPath => GameManager.Inst.ugcUntiyMapDataInfo.draftPath + "/";
#endif
    public static string logDataDir => Application.persistentDataPath +"/UnityLog/";
    
    private static string configName = "config.json";
    private static string infoName = "info.json";
    private static string clothJsonName = "clothJson.json";
    private static string dataJsonName = "dataJson.json";
    public static string dataUrlName = "dataTex.png";
    private static string localInfo = "localInfo.json";
    private static string emoDataLocalInfo = "emoDataLocalInfo.json";
    public static string InfoDir => Application.persistentDataPath + "/LocalInfo/";
    private static string settingLocalInfo = "settingLocalInfo.json";
    public static string ScissorsName = "alpha";
    public static string Vector3ToString(Vector3 target)
    {
        string str = target.ToString("f4");
        str = str.Substring(1, str.Length - 2);
        return str;
    }

    public static Vector3 LimitVector3(Vector3 target, float min = 0.0001f)
    {
        for (int i = 0; i < 3; i++)
        {
            target[i] = Mathf.Max(target[i],min);
        }
        return target;
    }

    public static string Vector2ToString(Vector2 target)
    {
        string str = target.ToString("f4");
        str = str.Substring(1, str.Length - 2);
        return str;
    }
    public static string Vector2IntToString(Vector2Int target)
    {
        string str = target.ToString();
        str = str.Substring(1, str.Length - 2);
        return str;
    }

    public static Vector2Int  DeSerializeVector2Int(string target)
    {
        string[] split = target.Split(',');
        int x = int.Parse(split[0], CultureInfo.InvariantCulture);
        int y = int.Parse(split[1], CultureInfo.InvariantCulture);
        return new Vector2Int(x, y);

    }

    public static Vector3 DeSerializeVector3(string target)
    {
        string[] split = target.Split(',');
        float x = float.Parse(split[0], CultureInfo.InvariantCulture);
        float y = float.Parse(split[1], CultureInfo.InvariantCulture);
        float z = float.Parse(split[2], CultureInfo.InvariantCulture);
        return new Vector3(x, y, z);
    }

    public static Vector2 DeSerializeVector2(string target)
    {
        string[] split = target.Split(',');
        float x = float.Parse(split[0], CultureInfo.InvariantCulture);
        float y = float.Parse(split[1], CultureInfo.InvariantCulture);
        return new Vector2(x, y);
    }

    public static string ColorToString(Color target)
    {
        return ColorUtility.ToHtmlStringRGB(target);
    }
    public static string ColorRGBAToString(Color target)
    {
        return ColorUtility.ToHtmlStringRGBA(target);
    }
    public static Color DeSerializeColor(string target)
    {
        bool parseSuccess = ColorUtility.TryParseHtmlString("#" + target, out Color color);
        if (parseSuccess)
        {
            return color;
        }
        return default;
    }
    public static Color DeSerializeColorByHex(string target)
    {
        bool parseSuccess = ColorUtility.TryParseHtmlString(target, out Color color);
        if (parseSuccess)
        {
            return color;
        }
        return default;
    }

    public static int GetProgress(float realVal, float min, float max, float pMin, float pMax)
    {
        return (int)(pMin + (realVal - min) / (max - min) * (pMax - pMin));
    }

    public static float GetRealValue(float progress, float min, float max, float pMin, float pMax)
    {
        return min + (progress - pMin) / (pMax - pMin) * (max - min);
    }

    public static Color GetHighlightColor(Color oldColor)
    {
        Color newColor = oldColor;
        if (newColor.r < 0.2f)
        {
            newColor.r += 0.1f;
        }
        newColor.r *= colorMultiplier.x;

        if (newColor.g < 0.2f)
        {
            newColor.g += 0.1f;
        }
        newColor.g *= colorMultiplier.y;

        if (newColor.b < 0.2f)
        {
            newColor.b += 0.1f;
        }
        newColor.b *= colorMultiplier.z;

        if (newColor.a < 0.5f) newColor.a *= 1.4f;

        return newColor;
    }

    public static Vector3 GetCenterPoint(List<SceneEntity> entitys)
    {
        if (entitys.Count < 2)
        {
            LoggerUtils.Log("Combine Object Num Error");
            return Vector3.zero;
        }

        Vector3 min = entitys[0].Get<GameObjectComponent>().bindGo.transform.position;
        Vector3 max = min;
        entitys.ForEach(x =>
        {
            var pos = x.Get<GameObjectComponent>().bindGo.transform.position;
            min.x = Mathf.Min(min.x, pos.x);
            min.y = Mathf.Min(min.y, pos.y);
            min.z = Mathf.Min(min.z, pos.z);
            max.x = Mathf.Max(max.x, pos.x);
            max.y = Mathf.Max(max.y, pos.y);
            max.z = Mathf.Max(max.z, pos.z);
        });
        return (min + max) / 2;
    }

    public static void RandomShuffle<T>(List<T> list)
    {
        //Fisher–Yates Shuffle
        for (int i = 0; i < list.Count - 1 ; ++i)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            T temp = list[r];
            list[r] = list[i];
            list[i] = temp;
        }
    }

    public static string FilterNonStandardText(string content, string defCont = "")
    {
        string str = content;
        List<string> patten = new List<string>();
        patten.Add(@"\p{Cs}");
        patten.Add(@"\p{Co}");
        patten.Add(@"\p{Cn}");
        patten.Add(@"[\u2070-\u24ff]");
        patten.Add(@"[\u2580-\u2bff]");
        patten.Add(@"[\ud800-\uf8ff]");
        patten.Add(@"[\ufff0-\uffff]");
        for (int i = 0; i < patten.Count; i++)
        {
            str = Regex.Replace(str, patten[i], defCont);
        }
        return str;
    }

    /// <summary>
    /// 图片裁剪压缩
    /// </summary>
    /// <param name="tex">原始图片</param>
    /// <param name="nRect">裁剪范围</param>
    /// <param name="ratio">压缩比例</param>
    /// <returns></returns>
    public static Texture2D TextureCompress(Texture2D tex, Vector2 nRect, float ratio = 0.8f)
    {
        if (tex == null || ratio <= 0)
        {
            return tex;
        }
        if (nRect == null || nRect.x > tex.width || nRect.y > tex.height || nRect.x <= 0 || nRect.y <= 0)
        {
            nRect = new Vector2(tex.width, tex.height);
        }
        //限定ratio上限
        ratio = Mathf.Min(ratio, 1);
        int width = (int)(nRect.x * ratio);
        int height = (int)(nRect.y * ratio);
        Texture2D newTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        //截取像素点信息
        Color[] pixels = tex.GetPixels((int)(tex.width - nRect.x) / 2, (int)(tex.height - nRect.y) / 2, (int)nRect.x, (int)nRect.y);
        Color[] newPixels = new Color[newTexture.width * newTexture.height];
        UnityEngine.Object.Destroy(tex);
        //降低分辨率
        for (int i = 0; i < newTexture.height; i++)
        {
            for (int j = 0; j < newTexture.width; j++)
            {
                newPixels[j + newTexture.width * i] = pixels[(int)(j * (1 / ratio)) + (int)(i * (1 / ratio)) * (int)nRect.x];
            }
        }
        newTexture.SetPixels(newPixels);
        newTexture.Apply();
        newTexture.Compress(false);
        return newTexture;
    }

    public static string SaveImg(byte[] img)
    {
        string fileName = GetImgName();
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        LoggerUtils.Log("dataDir + fileName==" + dataDir + fileName);

        if (File.Exists(dataDir + fileName))
        {
            File.Delete(dataDir + fileName);
        }
        FileStream stream = new FileStream(dataDir + fileName,FileMode.Create);
        stream.Write(img,0,img.Length);
        stream.Flush();
        stream.Close();
        return fileName;
    }

    public static string SaveResImg(byte[] img)
    {
        string fileName = GetResImgName();
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        LoggerUtils.Log("dataDir + fileName==" + dataDir + fileName);

        if (File.Exists(dataDir + fileName))
        {
            File.Delete(dataDir + fileName);
        }
        FileStream stream = new FileStream(dataDir + fileName, FileMode.Create);
        stream.Write(img, 0, img.Length);
        stream.Flush();
        stream.Close();
        return fileName;
    }

    public static void SaveCoverLocal(byte[] img, CoverType type)
    {
        string fileName = "cover." + type.ToString().ToLower();
        if (!Directory.Exists(DraftPath))
        {
            Directory.CreateDirectory(DraftPath);
        }
        if (File.Exists(DraftPath + fileName))
        {
            File.Delete(DraftPath + fileName);
        }
        FileStream stream = new FileStream(DraftPath + fileName, FileMode.Create);
        stream.Write(img, 0, img.Length);
        LoggerUtils.Log("local save cover success -- desPath = " + DraftPath + fileName);

        stream.Flush();
        stream.Close();
        GameManager.Inst.editSaveInfo.sCover = 1;
    }

    public static string SaveProfile(byte[] json)
    {

        string fileName = GetProfileName();
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        if (File.Exists(dataDir + fileName))
        {
            File.Delete(dataDir + fileName);
        }
        LoggerUtils.Log("dataDir + fileName====="+ dataDir + fileName);
        FileStream stream = new FileStream(dataDir + fileName, FileMode.Create);
        stream.Write(json, 0, json.Length);
        LoggerUtils.Log("dataDir + json.Length=====" + json.Length);

        stream.Flush();
        stream.Close();
        return fileName;
    }

    public static string SaveUgcClothCoverImg(byte[] img)
    {
        string fileName = GetUgcClothCoverName();
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        LoggerUtils.Log("dataDir + fileName==" + dataDir + fileName);

        if (File.Exists(dataDir + fileName))
        {
            File.Delete(dataDir + fileName);
        }
        FileStream stream = new FileStream(dataDir + fileName, FileMode.Create);
        stream.Write(img, 0, img.Length);
        stream.Flush();
        stream.Close();
        return fileName;
    }


    public static string SaveAudio(string audioName, byte[] audio)
    {
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        if (File.Exists(dataDir + audioName))
        {
            File.Delete(dataDir + audioName);
        }
        LoggerUtils.Log("dataDir + fileName====="+ dataDir + audioName);
        FileStream stream = new FileStream(dataDir + audioName, FileMode.Create);
        stream.Write(audio, 0, audio.Length);
        LoggerUtils.Log("dataDir + audio.Length=====" + audio.Length);

        stream.Flush();
        stream.Close();
        return dataDir + audioName;
    }

    public static void SetConfigLocal(CoverType type)
    {
        if (!Directory.Exists(DraftPath))
        {
            Directory.CreateDirectory(DraftPath);
        }
        LocalSaveConfig localSaveConfig = new LocalSaveConfig()
        {
            draftId = GameManager.Inst.gameMapInfo.draftId,
            coverType = (int)type,
            saveCover = GameManager.Inst.editSaveInfo.sCover,
            saveMap = GameManager.Inst.editSaveInfo.sMap,
            saveProp = GameManager.Inst.editSaveInfo.sProp,
            saveCloth = GameManager.Inst.editSaveInfo.sCloth,
            saveMaterial = GameManager.Inst.editSaveInfo.sMaterial,
        };
        string json = JsonConvert.SerializeObject(localSaveConfig);
        LoggerUtils.Log("LocalSave -- Config = " + json);
        File.WriteAllText(DraftPath + configName, json);
    }

    public static void SetMapInfoLocal(OperationType operationType)
    {
        if (!Directory.Exists(DraftPath))
        {
            Directory.CreateDirectory(DraftPath);
        }
        UpLoadMapBody upLoadMapBody = new UpLoadMapBody
        {
            mapInfo = GameManager.Inst.gameMapInfo,
            operationType = (int)operationType,
            templateId = GameManager.Inst.unityConfigInfo.templateId,
        };
        string json = JsonConvert.SerializeObject(upLoadMapBody);
        LoggerUtils.Log("LocalSave -- UpLoadMapBody = " + json);
        File.WriteAllText(DraftPath + infoName, json);
    }

    public static MapInfo GetMapInfoLocal()
    {
        string filePath = DraftPath + infoName;
        if (!File.Exists(filePath))
        {
            LoggerUtils.Log("local map info not exist! => " + filePath);
            return null;
        }
        string jsonStr = File.ReadAllText(filePath);
        LoggerUtils.Log("LocalRead -- UpLoadMapBody = " + jsonStr);
        UpLoadMapBody upLoadMapBody = JsonConvert.DeserializeObject<UpLoadMapBody>(jsonStr);
        return upLoadMapBody.mapInfo;
    }

    public static void InitEditSaveInfo()
    {
#if UNITY_EDITOR
        GameManager.Inst.unityConfigInfo = new UnityConfigInfo();
        GameManager.Inst.ugcUntiyMapDataInfo = new UgcUntiyMapDataInfo();
        GameManager.Inst.ugcUntiyMapDataInfo.draftPath = Application.persistentDataPath + "/U3D/Local";
#endif
        GameManager.Inst.editSaveInfo = new EditSaveInfo()
        {
            sMap = File.Exists(DraftPath + "map.zip") ? 1 : 0,
            sProp = File.Exists(DraftPath + "prop.zip") ? 1 : 0,
            sCloth = File.Exists(DraftPath + "clothJson.json") ? 1 : 0,
            sCover = File.Exists(DraftPath + "cover.jpg") || File.Exists(DraftPath + "cover.png") ? 1 : 0,
            sMaterial = File.Exists(DraftPath + "dataJson.json")? 1 : 0
        };
    }

    public static string SaveJsonAndGetPath(string json)
    {
        string fileName = GetJsonName();
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        if (File.Exists(dataDir + fileName))
        {
            fileName = fileName.Replace(".json", "_1.json");
        }
        if (!File.Exists(dataDir + fileName))
        {
            File.WriteAllText(dataDir + fileName, json);
        }
        string FullPath = dataDir + fileName;
        return FullPath;
    }

    public static void SaveJson(string filePath, string content)
    {
        string path = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        File.WriteAllText(filePath, content);
    }

    public static string SavePropJsonAndGetPath(string json)
    {
        string fileName = GetPropJsonName();
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        if (File.Exists(dataDir + fileName))
        {
            fileName = fileName.Replace("m.json", "_1m.json");
        }
        if (!File.Exists(dataDir + fileName))
        {
            File.WriteAllText(dataDir + fileName, json);
        }
        string FullPath = dataDir + fileName;
        return FullPath;
    }

    public static string SaveUgcClothJsonAndGetPath(string json)
    {
        string fileName = GetClothJsonName();
        if (!Directory.Exists(ugcClothesDataDir))
        {
            Directory.CreateDirectory(ugcClothesDataDir);
        }
        File.WriteAllText(ugcClothesDataDir + fileName, json);
        string FullPath = ugcClothesDataDir + fileName;
        return FullPath;
    }

    public static string SaveUgcClothJson(string json)
    {
        string fileName = GetClothJsonName();
        if (!Directory.Exists(ugcClothesDataDir))
        {
            Directory.CreateDirectory(ugcClothesDataDir);
        }
        File.WriteAllText(ugcClothesDataDir + fileName, json);
        return fileName;
    }

    public static string SaveUgcResJsonToLocal(string json,MapSaveType mapSaveType)
    {
        if (!Directory.Exists(DraftPath))
        {
            Directory.CreateDirectory(DraftPath);
        }
        string name = "";
        switch (mapSaveType)
        {
            case MapSaveType.UGCRes:
                File.WriteAllText(DraftPath + clothJsonName, json);
                GameManager.Inst.gameMapInfo.draftId++;
                GameManager.Inst.editSaveInfo.sCloth = 1;
                name = clothJsonName;
                break;
            case MapSaveType.UGCMaterial:
                File.WriteAllText(DraftPath + dataJsonName, json);
                GameManager.Inst.gameMapInfo.draftId++;
                GameManager.Inst.editSaveInfo.sMaterial = 1;
                name = dataJsonName;
                break;
        }
        return name;
    }
   
    public static string GetJsonName()
    {
        return GameInfo.Inst.myUid + "_"  + GameUtils.GetMilliTimeStamp() + ".json";
    }

    private static string GetPropJsonName()
    {
        return GameInfo.Inst.myUid + "_" + GameUtils.GetMilliTimeStamp() + "m.json";
    }

    public static string GetImgName()
    {
        return GameInfo.Inst.myUid + "_" + GameUtils.GetTimeStamp() + ".jpg";
    }

    public static string GetResImgName()
    {
        return GameInfo.Inst.myUid + "_" + GameUtils.GetTimeStamp() + ".png";
    }

    public static string GetResNameWithoutEx()
    {
        return GameInfo.Inst.myUid + "_" + GameUtils.GetTimeStamp();
    }

    public static string GetProfileName()
    {
        return "Profile" + GameInfo.Inst.myUid + "_" + GameUtils.GetTimeStamp() + ".png";
    }

    public static int GetColorSelect(string curColor, List<Color> colorLib)
    {
        for (int i = 0; i < colorLib.Count; i++)
        {
            string color = DataUtils.ColorToString(colorLib[i]);
            if (color == curColor)
            {
                return i;
            }
        }
        return -1;
    }

    public static string GetUgcClothCoverName()
    {
        return GetResNameWithoutEx() + "_CoverImg_clothes.png";
    }

    public static string GetClothJsonName()
    {
        return GetResNameWithoutEx() + "_json_clothes.json";
    }

    public static string GetClothImageZipName()
    {
        return GetResNameWithoutEx() + "_tex_clothes.zip";
    }

    public static string GetLocalTimeZone()
    {
        //GMT+08:00 GMT+00:00 GMT-05:00
        int offset = TimeZoneInfo.Local.BaseUtcOffset.Hours;
        string sign = offset >= 0 ? "+" : "-";
        return "GMT" + sign + TimeZoneInfo.Local.BaseUtcOffset.ToString(@"hh\:mm");
    }

    /// <summary>
    /// 获取文件MD5值
    /// </summary>
    /// <param name="fileName">文件绝对路径</param>
    /// <returns>MD5值</returns>
    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile Fail! Error : " + ex.Message);
        }
    }

    /// <summary>
    /// 获得正确的旋转值
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static Vector3 GetInspectorRotationValueMethod(Transform transform)
    {
        // 获取原生值
        System.Type transformType = transform.GetType();
        PropertyInfo m_propertyInfo_rotationOrder = transformType.GetProperty("rotationOrder", BindingFlags.Instance | BindingFlags.NonPublic);
        object m_OldRotationOrder = m_propertyInfo_rotationOrder.GetValue(transform, null);
        MethodInfo m_methodInfo_GetLocalEulerAngles = transformType.GetMethod("GetLocalEulerAngles", BindingFlags.Instance | BindingFlags.NonPublic);
        object value = m_methodInfo_GetLocalEulerAngles.Invoke(transform, new object[] { m_OldRotationOrder });
        string temp = value.ToString();
        //将字符串第一个和最后一个去掉
        temp = temp.Remove(0, 1);
        temp = temp.Remove(temp.Length - 1, 1);
        //用‘，’号分割
        string[] tempVector3;
        tempVector3 = temp.Split(',');
        //将分割好的数据传给Vector3
        Vector3 vector3 = new Vector3(float.Parse(tempVector3[0]), float.Parse(tempVector3[1]), float.Parse(tempVector3[2]));
        return vector3;
    }

    /// <summary>
    /// 计算生成包围盒
    /// </summary>
    /// <param name="box"></param>
    /// <returns></returns>
    public static Bounds CalculateBoundingBox(Transform box)
    {
        Vector3 postion = box.position;
        Quaternion rotation = box.rotation;
        box.position = Vector3.zero;
        box.rotation = Quaternion.Euler(Vector3.zero);
        Vector3 center = Vector3.zero;
        Renderer[] renders = box.GetComponentsInChildren<Renderer>();
        foreach (Renderer child in renders)
        {
            center += child.bounds.center;
        }
        center /= renders.Length;
        Bounds bounds = new Bounds(center, Vector3.zero);
        foreach (Renderer child in renders)
        {
            bounds.Encapsulate(child.bounds);
        }
        box.position = postion;
        box.rotation = rotation;
        return bounds;
    }
    
    public static void SetLocalNotification(string id, bool v)
    {
        var info = GetLocalInfo();
        var playInfo=GetPlayerInfoById(id,info);
        playInfo.SocialNotification = v;
        info.PlayerLocalInfoDic[id] = playInfo;
        SetLocalInfo(info);
    }
    public static void SetUgcClothsColors(string id, List<string> colors)
    {
        var info = GetLocalInfo();

        var playInfo=GetPlayerInfoById(id,info);
        playInfo.UgcColorDatas=colors;
        info.PlayerLocalInfoDic[id] = playInfo;
        SetLocalInfo(info);
    }
    public static void SetCustomizeColorInfo(string id,List<string> list)
    {
        var info = GetLocalInfo();
        var playInfo = GetPlayerInfoById(id, info);
        playInfo.CustomizeColor = list;
        info.PlayerLocalInfoDic[id] = playInfo;
        SetLocalInfo(info);
    }
    public static PlayerLocalInfo GetPlayerInfoById(string id,LocalInfo info)
    {
        if (info.PlayerLocalInfoDic == null)
        {
            info.PlayerLocalInfoDic = new Dictionary<string, PlayerLocalInfo>();
        }
        PlayerLocalInfo playInfo;
        if (info.PlayerLocalInfoDic.ContainsKey(id))
        {
            playInfo = info.PlayerLocalInfoDic[id];
        }
        else
        {
            playInfo = new PlayerLocalInfo();
        }
        return playInfo;
    }

    public static bool GetLocalNotification(string id)
    {
        var info = GetLocalInfo();
        if (info.PlayerLocalInfoDic != null && info.PlayerLocalInfoDic.ContainsKey(id))
        {
            return info.PlayerLocalInfoDic[id].SocialNotification;
        }

        return true;
    }
    public static List<string> GetLocalUgcColors(string id)
    {
        var info = GetLocalInfo();
        if (info.PlayerLocalInfoDic != null && info.PlayerLocalInfoDic.ContainsKey(id))
        {
            return info.PlayerLocalInfoDic[id].UgcColorDatas;
        }
        return new List<string>();
    }
    public static List<string> GetCustomizeColorInfo(string id)
    {
        var info = GetLocalInfo();
        if (info.PlayerLocalInfoDic != null && info.PlayerLocalInfoDic.ContainsKey(id))
        {
            return info.PlayerLocalInfoDic[id].CustomizeColor;
        }
        return new List<string>();
    }
    private static void SetLocalInfo(LocalInfo info)
    {
        string filePath = dataDir + localInfo;
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        string json = JsonConvert.SerializeObject(info);
        File.WriteAllText(filePath, json);
    }
    
    private static LocalInfo GetLocalInfo()
    {
        string filePath = dataDir + localInfo;
        LocalInfo info = null;
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        if (File.Exists(filePath))
        {
            string jsonStr = File.ReadAllText(filePath);
            if(!string.IsNullOrEmpty(jsonStr))
            {
                info = JsonConvert.DeserializeObject<LocalInfo>(jsonStr);
            }
        }

        if (info == null)
        {
            info = new LocalInfo();
 
        }
        return info;
    }
    
    public static void SetLocalLockMove(string id, int v)
    {
        var info = GetLocalInfo();
        if (info.PlayerLocalInfoDic == null)
        {
            info.PlayerLocalInfoDic = new Dictionary<string, PlayerLocalInfo>();
        }

        PlayerLocalInfo playInfo;
        if (info.PlayerLocalInfoDic.ContainsKey(id))
        {
            playInfo = info.PlayerLocalInfoDic[id];
        }
        else
        {
            playInfo = new PlayerLocalInfo();
        }

        playInfo.LockMoveStick = v;
        info.PlayerLocalInfoDic[id] = playInfo;
        SetLocalInfo(info);
    }

    public static void SetGlobalSetting(string id, GlobalSettingData data)
    {

        var info = ReadSettingInfo();
        info.PlayerSettingInfoDict[id] = data;
        SaveSettingInfo(info);
    }

    public static GlobalSettingData GetGlobalSetting(string id)
    {
        var info = GetLocalInfo();
        var settingInfo = ReadSettingInfo();
        GlobalSettingData data;
        if (!settingInfo.PlayerSettingInfoDict.TryGetValue(id, out data))
        {
            data = new GlobalSettingData();
            settingInfo.PlayerSettingInfoDict[id] = data;
        }
        bool isNeverSave = false;
        if (data.lockMoveStick == -1)
        {
            //没有保存过数据的情况下，检查以前保存的摇杆设置
            //如果以前是锁定关闭，现在就是锁定关闭，其他情况都是开
            //注意以前的0是关闭，现在的0是开启
            if (info.PlayerLocalInfoDic != null
            && info.PlayerLocalInfoDic.ContainsKey(id)
            && info.PlayerLocalInfoDic[id].LockMoveStick == 0)
            {
                data.lockMoveStick = 1;
            }
            else
            {
                data.lockMoveStick = 0;
            }
            isNeverSave = true;
        }
        if (data.friendRequest == -1)
        {
            //检查以前保存的好友申请设置
            if (info.PlayerLocalInfoDic != null
            && info.PlayerLocalInfoDic.ContainsKey(id)
            && !info.PlayerLocalInfoDic[id].SocialNotification)
            {
                data.friendRequest = 1;
            }
            else
            {
                data.friendRequest = 0;
            }
            isNeverSave = true;
        }

        //从未保存过数据，保存一下
        if (isNeverSave)
        {
            SetGlobalSetting(id, data);
        }
        //视角暂时永远设置为第三人称
        data.gameView = GameView.ThirdPerson;
        return data;
    }

    /// <summary>
    /// 全局设置数据保存
    /// </summary>
    /// <param name="info">待保存的数据</param>

    public static void SaveSettingInfo(SettingLocalInfo info)
    {
        string filePath = InfoDir + settingLocalInfo;
        if (!Directory.Exists(InfoDir))
        {
            Directory.CreateDirectory(InfoDir);
        }
        string json = JsonConvert.SerializeObject(info);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 全局设置数据读取
    /// </summary>
    /// <returns>读取的数据</returns>

    public static SettingLocalInfo ReadSettingInfo()
    {
        string filePath = InfoDir + settingLocalInfo;
        SettingLocalInfo info;
        if (!Directory.Exists(InfoDir))
        {
            Directory.CreateDirectory(InfoDir);
        }

        if (File.Exists(filePath))
        {
            string jsonStr = File.ReadAllText(filePath);
            info = JsonConvert.DeserializeObject<SettingLocalInfo>(jsonStr);
        }
        else
        {
            info = new SettingLocalInfo();
        }
        return info;
    }
    public static int GetLocalLockMoveStick(string id)
    {
        var info = GetLocalInfo();
        if (info.PlayerLocalInfoDic != null && info.PlayerLocalInfoDic.ContainsKey(id))
        {
            return info.PlayerLocalInfoDic[id].LockMoveStick;
        }

        return 1;
    }
    private static void SetEmoRedLocalInfo(EmoRedLocalInfo info)
    {
        string filePath = crashDataDir + emoDataLocalInfo;
        if (!Directory.Exists(crashDataDir))
        {
            Directory.CreateDirectory(crashDataDir);
        }
        string json = JsonConvert.SerializeObject(info);
        File.WriteAllText(filePath, json);
    }
    private static EmoRedLocalInfo GetEmoRedLocalInfo()
    {
        string filePath = crashDataDir + emoDataLocalInfo;
        EmoRedLocalInfo info;
        if (!Directory.Exists(crashDataDir))
        {
            Directory.CreateDirectory(crashDataDir);
        }
        if (File.Exists(filePath))
        {
            string jsonStr = File.ReadAllText(filePath);
            info = JsonConvert.DeserializeObject<EmoRedLocalInfo>(jsonStr);
        }
        else
        {
            info = new EmoRedLocalInfo();
        }
        return info;
    }
    public static void SetEmoReds(string id, Dictionary<int, int> emoReds)
    {
        var info = GetEmoRedLocalInfo();
        if (info.playerEmoRedLocalInfo == null)
        {
            info.playerEmoRedLocalInfo = new Dictionary<string, PlayerEmoRedInfo>();
        }
        PlayerEmoRedInfo playInfo;
        if (info.playerEmoRedLocalInfo.ContainsKey(id))
        {
            playInfo = info.playerEmoRedLocalInfo[id];
        }
        else
        {
            playInfo = new PlayerEmoRedInfo();
        }
        playInfo.emoRedDics = emoReds;
        info.playerEmoRedLocalInfo[id] = playInfo;
        SetEmoRedLocalInfo(info);
    }
    public static Dictionary<int, int> GetLocalEmoReds(string id)
    {
        var info = GetEmoRedLocalInfo();
        if (info.playerEmoRedLocalInfo != null && info.playerEmoRedLocalInfo.ContainsKey(id))
        {
            return info.playerEmoRedLocalInfo[id].emoRedDics;
        }
        return new Dictionary<int, int>();
    }
    public static string ReplaceRichText(string content)
    {
        return Regex.Replace(content, @"(<size|</size>|<color|</color>|<b>|</b>|<i>|</i>|<material|</material>|<quad>|</quad>)", "", RegexOptions.IgnoreCase);
    }
}

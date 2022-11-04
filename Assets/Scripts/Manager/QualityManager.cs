using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class QualityManager : CInstance<QualityManager>
{

    public enum QualityLevel
    {
        Low,
        High,
    }
    
    [Serializable]
    public class FpsInfo
    {
        public string mapId;
        public float avgFps;
        public int count;
        public QualityLevel quality;
    }
    
    [Serializable]
    public class QualityInfo
    {
        public string mapId;
        public QualityLevel quality;
    }
    
    

    private QualityLevel nowQualityLevel = QualityLevel.Low;
    private bool isOpenShadow = false;
    private List<FpsInfo> fpsInfos;
    private List<QualityInfo> qualityInfos;

    private string fpsInfosPath => DataUtils.dataDir + "FpsInfos.json";
    private string qualityInfoPath => DataUtils.dataDir  + "QualityInfos.json";
    private const int MaxFpsLowQuality = 28;
    private const int MinFpsHighQuality = 20;
    public override void Release()
    {
        base.Release();
        fpsInfos?.Clear();
        qualityInfos?.Clear();
        nowQualityLevel = QualityLevel.Low;
    }

    public void Init()
    {
        if (File.Exists(fpsInfosPath))
        {
            var infoData = File.ReadAllText(fpsInfosPath);
            try
            {
                fpsInfos = JsonConvert.DeserializeObject<List<FpsInfo>>(infoData);
            }
            catch (Exception e)
            {
                fpsInfos = new List<FpsInfo>();
                LoggerUtils.Log("QualityManager Init FpsInfos Error:" + e.Message);
            }
        }
        else
        {
            fpsInfos = new List<FpsInfo>();
        }
        
        if (File.Exists(qualityInfoPath))
        {
            var infoData = File.ReadAllText(qualityInfoPath);
            try
            {
                qualityInfos = JsonConvert.DeserializeObject<List<QualityInfo>>(infoData);
            }
            catch (Exception e)
            {
                qualityInfos = new List<QualityInfo>();
                LoggerUtils.Log("QualityManager Init QualityInfos Error:" + e.Message);
            }
        }
        else
        {
            qualityInfos = new List<QualityInfo>();
        }
    }

    public void SetFps(int fps)
    {
        Application.targetFrameRate = fps;
        FPSController.Inst.ResetTime();
    }
    public void SetAvgFps(string mapId, float avgFps)
    {
        var fpsInfo = fpsInfos.Find(tmp => tmp.mapId == mapId && tmp.quality == nowQualityLevel);
        if (fpsInfo == null)
        {
            fpsInfo = new FpsInfo()
            {
                mapId = mapId,
                quality = nowQualityLevel,
            };
            fpsInfos.Add(fpsInfo);
        }

        var allFps = fpsInfo.avgFps * fpsInfo.count;
        allFps += avgFps;
        fpsInfo.count++;
        fpsInfo.avgFps = allFps / fpsInfo.count;
        SaveFpsInfo();
    }

    public QualityLevel CheckQuality()
    {
        if(GlobalFieldController.CurMapInfo == null || qualityInfos == null)return QualityLevel.Low;
        string mapId = GlobalFieldController.CurMapInfo.mapId;
        var tmpQualityInfo = qualityInfos.Find(tmp => tmp.mapId == mapId);
        if (tmpQualityInfo == null)
        {
            // 无该地图游玩记录，则根据历史游玩记录，确定当前地图画质, 若游玩过的地图中，有超过半数是高画质，则默认为高画质
            var HighCount = qualityInfos.FindAll(tmp => tmp.quality == QualityLevel.High).Count;
            isOpenShadow &= (HighCount > (qualityInfos.Count * 0.5f));
            return HighCount > (qualityInfos.Count * 0.5f) ? QualityLevel.High : QualityLevel.Low;
        }
        nowQualityLevel = tmpQualityInfo.quality;
        var fpsInfo = fpsInfos.Find(tmp => tmp.mapId == mapId && tmp.quality == nowQualityLevel);
        if (fpsInfo == null)
        {
            isOpenShadow &= (nowQualityLevel == QualityLevel.High);
            return nowQualityLevel;
        }
        if (nowQualityLevel == QualityLevel.Low)
        {
            //当前画质为Low, 则判断平均fps，若超过阈值, 则将画质 调整为 High
            isOpenShadow &= (fpsInfo.avgFps >= MaxFpsLowQuality);
            return fpsInfo.avgFps >= MaxFpsLowQuality ? QualityLevel.High : QualityLevel.Low;
        }
        //当前画质为High, 则判断平均fps，若低于阈值, 则将画质 调整为 Low
        isOpenShadow &= !(fpsInfo.avgFps <= MinFpsHighQuality);
        return fpsInfo.avgFps <= MinFpsHighQuality ? QualityLevel.Low : QualityLevel.High;
    }

    public QualityLevel GetQualityLevel()
    {
        return nowQualityLevel;
    }

    public void SetTargetQualityShadow(bool isOpenShadow)
    {
        QualitySettings.shadows = isOpenShadow ?ShadowQuality.All:ShadowQuality.Disable;
 
        this.isOpenShadow = isOpenShadow;
    }
    public void SetQualityLevel(QualityLevel level)
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            // 非Android平台, 该接口无效
            return;
        }
        nowQualityLevel = level;

        switch (level)
        {
            case QualityLevel.Low:
                QualitySettings.SetQualityLevel(0, true);
                // QualitySettings.lodBias = 0.3f;
                // QualitySettings.pixelLightCount = 0;
                // QualitySettings.shadows = ShadowQuality.Disable;
                // QualitySettings.antiAliasing = 0;
                break;
            case QualityLevel.High:
                QualitySettings.SetQualityLevel(3, true);
                // QualitySettings.lodBias = 1.0f;
                // QualitySettings.antiAliasing = 2;
                // QualitySettings.pixelLightCount = 4;
                // QualitySettings.shadows = ShadowQuality.All;
                break;
        }
        QualitySettings.lodBias = 1.0f;
        QualitySettings.vSyncCount = 0;
        QualitySettings.skinWeights = SkinWeights.TwoBones;
        QualitySettings.shadows = isOpenShadow ?ShadowQuality.All:ShadowQuality.Disable;
        SaveQualityInfo();
    }

    private void SaveQualityInfo()
    {
        if (!Directory.Exists(DataUtils.dataDir))
        {
            Directory.CreateDirectory(DataUtils.dataDir);
        }
        var qualityInfo = qualityInfos.Find(tmp => tmp.mapId == GlobalFieldController.CurMapInfo.mapId);
        if (qualityInfo == null)
        {
            qualityInfo = new QualityInfo()
            {
                mapId = GlobalFieldController.CurMapInfo.mapId,
                quality = nowQualityLevel
            };
            qualityInfos.Add(qualityInfo);
        }
        else
        {
            qualityInfo.quality = nowQualityLevel;
        }
        try
        {
            File.WriteAllText(qualityInfoPath, JsonConvert.SerializeObject(qualityInfos));
        }
        catch (Exception e)
        {
            LoggerUtils.Log("SaveQualityInfo Error:" + e.Message);
        }
    
    }

    private void SaveFpsInfo()
    {
        if (!Directory.Exists(DataUtils.dataDir))
        {
            Directory.CreateDirectory(DataUtils.dataDir);
        }
        try
        {
            File.WriteAllText(fpsInfosPath, JsonConvert.SerializeObject(fpsInfos));
        }
        catch (Exception e)
        {
            LoggerUtils.Log("SaveFpsInfo Error:" + e.Message);
        }

    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Funly.SkyStudio;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Author: Shaocheng
/// Description: 天空盒行为
/// Date: 2022-9-6 23:11:01
/// </summary>
public class SkyboxBehaviour : NodeBaseBehaviour
{
    public const string SKYPROFILEPATH = "SkyProfile/SkySystemController_";
    public int TotalDaySecs = 24 * 3600;
    public bool IsDayNightSkyRunning = false;
    private GameObject tcObj;
    private TimeOfDayController tc;
    private Material normalSkyboxMat;
    private SkyboxComponent skyboxComponent;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        normalSkyboxMat = Resources.Load<Material>("Material/Skybox/Skybox");
        if (normalSkyboxMat == null)
        {
            LoggerUtils.LogError("!!!!!!!! 404: Skybox not found !!!!!!!!!!");
        }

        skyboxComponent = entity.Get<SkyboxComponent>();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        IsDayNightSkyRunning = false;
    }

    #region 普通天空盒

    public void SetNormalSky(int skyId)
    {
        RemoveDayNightSkybox();
        SetNormalSkyMat();

        SetSkySetting(skyId);
        SetSkyboxTexture(skyId);
        SetSkyboxCubemap(skyId);

        if (tc)
        {
            tc.ResetLastTimeOfDay();
        }
        tc = null;
        IsDayNightSkyRunning = false;
        ControlDirLight();
        ControlDayNightAnimPanelShow();
    }

    public void SetNormalSkyMat()
    {
        if (normalSkyboxMat && RenderSettings.skybox != normalSkyboxMat)
        {
            RenderSettings.skybox = normalSkyboxMat;
        }
    }

    public static void SetSkySetting(int id)
    {
        var skySetting = GameConsts.settings[id];
        RenderSettings.ambientMode = (UnityEngine.Rendering.AmbientMode) skySetting.gradientType;
        RenderSettings.ambientLight = skySetting.sky;
        RenderSettings.ambientSkyColor = skySetting.sky;
        RenderSettings.ambientEquatorColor = skySetting.equator;
        RenderSettings.ambientGroundColor = skySetting.ground;
        RenderSettings.reflectionIntensity = skySetting.reflectionIntensity;

        var dirBehv = SceneBuilder.Inst.DirLight;
        dirBehv.SetIntensity(skySetting.intensity);
        dirBehv.SetAngleX(skySetting.anglex);
        dirBehv.SetAngleY(skySetting.angley);
        dirBehv.SetColor(skySetting.dirctional);
        var comp = dirBehv.entity.Get<DirLightComponent>();
        comp.anglex = skySetting.anglex;
        comp.angley = skySetting.angley;
        comp.intensity = skySetting.intensity;
        comp.color = skySetting.dirctional;
    }

    public static void SetSkyGradient(AmbientMode mode, Color sky, Color equa, Color ground)
    {
        RenderSettings.ambientMode = mode;
        RenderSettings.ambientSkyColor = sky;
        RenderSettings.ambientEquatorColor = equa;
        RenderSettings.ambientGroundColor = ground;
    }

    public static void SetSkyboxTexture(int id)
    {
        var textureNames = GameManager.Inst.skyboxDatas[id].textures;
        List<Texture2D> textures = new List<Texture2D>();
        for (int i = 0; i < textureNames.Length; i++)
        {
            Texture2D tex = null;
#if UNITY_EDITOR
            tex = ResManager.Inst.LoadRes<Texture2D>(GameConsts.SkyTexPath + textureNames[i]);
#else
            tex = AssetBundleLoaderMgr.Inst.LoadSkybox(GameConsts.SkyTexPath + textureNames[i]);
#endif
            textures.Add(tex);
        }

        RenderSettings.skybox.SetTexture("_FrontTex", textures[0]);
        RenderSettings.skybox.SetTexture("_BackTex", textures[1]);
        RenderSettings.skybox.SetTexture("_LeftTex", textures[2]);
        RenderSettings.skybox.SetTexture("_RightTex", textures[3]);
        RenderSettings.skybox.SetTexture("_UpTex", textures[4]);
        RenderSettings.skybox.SetTexture("_DownTex", textures[5]);
    }

    public static void SetSkyboxCubemap(int id)
    {
        var cubemapName = GameManager.Inst.skyboxDatas[id].cubemap;
        if (!string.IsNullOrEmpty(cubemapName))
        {
            var cubemap = ResManager.Inst.LoadRes<Cubemap>(GameConsts.CubemapPath + cubemapName);
            RenderSettings.customReflection = cubemap;
        }
        else
        {
            RenderSettings.customReflection = null;
        }
    }

    #endregion

    #region 昼夜天空盒

    private void InitTcObj(int skyboxId)
    {
        if (tcObj == null)
        {
            var skySysCtrObj = ResManager.Inst.LoadRes<GameObject>(SKYPROFILEPATH + skyboxId);
            tcObj = Instantiate(skySysCtrObj, transform);
            tc = tcObj.GetComponent<TimeOfDayController>();
            
            var cloudTrans = tcObj.transform.Find("BudCloud");
            BudCloudManager.Inst.Init(cloudTrans);
        }
    }

    public void SetDayNightSkybox(int skyboxId)
    {
        SetNormalSky(SkyboxManager.Inst.defaultSkyId);

        InitTcObj(skyboxId);
        tc.gameObject.SetActive(true);
        tc.automaticTimeIncrement = false;
        SetSkyboxTime();
        IsDayNightSkyRunning = false;
        ControlDirLight();
        tc.ResetLastTimeOfDay();
        
        //美术要求，调整昼夜天空盒环境光参数
        RenderSettings.ambientSkyColor = new Color(0.54f,0.7316f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.97f,0.7663f, 0.7730f);
        RenderSettings.ambientGroundColor = new Color(0.91f,0.6988f, 0.3822f);
    }

    public void RemoveDayNightSkybox()
    {
        if (tcObj)
        {
            DestroyImmediate(tcObj);
            tcObj = null;
            tc = null;
        }
    }

    public void OnWaitForFrameFinish()
    {
        EnterSkyboxGuestMode();
    }

    public void EnterSkyboxGuestMode(bool isEnterForeGround = false)
    {
        if (skyboxComponent == null || skyboxComponent.skyboxType != SkyboxType.DayNight)
        {
            LoggerUtils.Log("EnterSkyboxGuestMode return, not DAY NIGHT skybox!");
            return;
        }

        if (Global.Room == null || Global.Room.RoomInfo == null || GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            LoggerUtils.Log("EnterSkyboxGuestMode return, Room may be null!");
            return;
        }

        var curUnixTime = (ulong) GameUtils.GetUtcTimeStamp();
        var roomCreateTime = Global.Room.RoomInfo.CreateTime;
        float timePassed = curUnixTime - roomCreateTime;
        //如果用户时间错误，出现异常值，则从0开始
        if (timePassed < 0f)
        {
            LoggerUtils.LogError("User unix time error:" + curUnixTime);
            timePassed = 0f;
        }
        
        float timePassedDay = timePassed / (skyboxComponent.dayLength * 60);

        var dayTimeHour = skyboxComponent.daytimeHour;
        var dayTimeMin = skyboxComponent.daytimeMin;
        var startTims = dayTimeHour * 3600 + dayTimeMin * 60;
        float startSkyTime = (float) startTims / TotalDaySecs;

        timePassedDay += startSkyTime;

        SetSkyboxTime(timePassedDay);
        LoggerUtils.Log($"EnterSkyboxGuestMode Skybox： curUnixTime:{curUnixTime}, roomCreateTime:{roomCreateTime}, " +
                        $"roomOwner:{Global.Room.RoomInfo.Owner}, timePassed:{timePassed}, timePassedDay:{timePassedDay}");

        ControlDirLight();
        IsDayNightSkyRunning = true;

        if (!isEnterForeGround)
        {
            DayNightSkyboxAnimPanel.Show();
        }
    }

    public void EnterSkyboxPlayMode()
    {
        if (skyboxComponent.skyboxType != SkyboxType.DayNight)
        {
            return;
        }

        SetSkyboxTime();
        IsDayNightSkyRunning = true;
        ControlDirLight();
        DayNightSkyboxAnimPanel.Show();
    }

    public void EnterSkyboxEditMode()
    {
        if (skyboxComponent.skyboxType != SkyboxType.DayNight)
        {
            return;
        }

        IsDayNightSkyRunning = false;
        SetSkyboxTime();
        DayNightSkyboxAnimPanel.Hide();
    }

    public void OnFixedTimePassed()
    {
        if (!IsDayNightSkyRunning || skyboxComponent == null || skyboxComponent.skyboxType != SkyboxType.DayNight)
        {
            return;
        }

        if (tc)
        {
            float ugcDayLength = skyboxComponent.dayLength * 60;
            float dayPassed = Time.fixedDeltaTime / ugcDayLength;
            tc.skyTime += dayPassed;

            //云的旋转和变色
            BudCloudManager.Inst.OnFixedTimePassed(tc.skyTime);

            //左下角动画
            if (DayNightSkyboxAnimPanel.Instance)
            {
                DayNightSkyboxAnimPanel.Instance.OnFixedTimePassed(tc.skyTime);
            }
        }
    }

    public void SetSkyboxTime(float inputSkyTime = 0f)
    {
        if (skyboxComponent == null || skyboxComponent.skyboxType != SkyboxType.DayNight)
        {
            return;
        }

        var dayTimeHour = skyboxComponent.daytimeHour;
        var dayTimeMin = skyboxComponent.daytimeMin;

        if (inputSkyTime != 0f)
        {
            tc.skyTime = inputSkyTime;
        }
        else
        {
            var curSces = dayTimeHour * 3600 + dayTimeMin * 60;
            float startTime = (float) curSces / TotalDaySecs;

            tc.skyTime = startTime;
            LoggerUtils.Log($"SetSkyboxTime dayTimeHour:{dayTimeHour}, dayTimeMin:{dayTimeMin}");
            LoggerUtils.Log($"SetSkyboxTime curSces:{curSces}, totalSce:{TotalDaySecs}, CurSkyTime;{tc.skyTime}");
        }

        BudCloudManager.Inst.SetCurrentTiming(tc.skyTime);
    }

    public float GetCurSkyTime()
    {
        if (tc)
        {
            return tc.skyTime;
        }

        return 0f;
    }

    public void ControlDirLight()
    {
        if (SceneBuilder.Inst.DirLight != null && skyboxComponent != null)
        {
            SceneBuilder.Inst.DirLight.gameObject.SetActive(skyboxComponent.skyboxType != SkyboxType.DayNight);
        }
    }

    public void ControlDayNightAnimPanelShow()
    {
        if (skyboxComponent != null && skyboxComponent.skyboxType != SkyboxType.DayNight)
        {
            DayNightSkyboxAnimPanel.Hide();
        }
    }

    #endregion
}
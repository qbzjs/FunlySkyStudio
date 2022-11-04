using System;
using System.Collections.Generic;
using System.Timers;
using Amazon.SecurityToken.Internal;
using UnityEngine;

/// <summary>
/// Author: Shaocheng
/// Description: 昼夜天空盒云的管理
/// Date: 2022-9-15 13:27:12
/// </summary>
public class BudCloudManager : CInstance<BudCloudManager>
{
    private class CloudStage
    {
        public int enterTime;
        public bool needLerp; //是否需要lerp过渡
        public int lerpTime;
        public int lastStageKey;

        public Color rimColor;
        public Color rimColor1;
        public Color brightColor;
        public Color brightColor1;
        public Color darkColor;
        public Color darkColor1;
        public Color secDarkColor;
        public Color secDarkColor1;
    }

    //key-触发变色的时间点
    private Dictionary<int, CloudStage> cloudStages = new Dictionary<int, CloudStage>();
    private List<Material> cloudMats;
    private Transform cloudTrans;
    private const float cloudRotateSpeed = 1f;
    private CloudStage curStage;

    public void Init(Transform t)
    {
        if (cloudStages != null)
        {
            cloudStages.Clear();
        }
        else
        {
            cloudStages = new Dictionary<int, CloudStage>();
        }

        curStage = null;
        cloudTrans = t;
        InitCloudLerpTime();

        #region 时间配置 https: //pointone.feishu.cn/sheets/shtcnJNv2tIyMltv2QFeAYxDd0g

        cloudStages.Add(0, new CloudStage()
        {
            enterTime = 0,

            rimColor = DataUtils.DeSerializeColorByHex("#9996A4"),
            rimColor1 = DataUtils.DeSerializeColorByHex("#92BEE9"),
            brightColor = DataUtils.DeSerializeColorByHex("#618ABE"),
            brightColor1 = DataUtils.DeSerializeColorByHex("#396DA4"),
            darkColor = DataUtils.DeSerializeColorByHex("#34659F"),
            darkColor1 = DataUtils.DeSerializeColorByHex("#93C7FF"),
            secDarkColor = DataUtils.DeSerializeColorByHex("#5989D1"),
            secDarkColor1 = DataUtils.DeSerializeColorByHex("#5E90D5"),
        });

        cloudStages.Add(3, new CloudStage()
        {
            lastStageKey = 0,
            enterTime = 3,
            needLerp = true,
            lerpTime = 2,

            rimColor = DataUtils.DeSerializeColorByHex("#B7B7B7"),
            rimColor1 = DataUtils.DeSerializeColorByHex("#EEEEAA"),
            brightColor = DataUtils.DeSerializeColorByHex("#7DB7B2"),
            brightColor1 = DataUtils.DeSerializeColorByHex("#2E6986"),
            darkColor = DataUtils.DeSerializeColorByHex("#4A7EBE"),
            darkColor1 = DataUtils.DeSerializeColorByHex("#79A4A0"),
            secDarkColor = DataUtils.DeSerializeColorByHex("#5989D1"),
            secDarkColor1 = DataUtils.DeSerializeColorByHex("#5E90D5"),
        });

        cloudStages.Add(5, new CloudStage()
        {
            lastStageKey = 3,
            enterTime = 5,
            needLerp = true,
            lerpTime = 1,

            rimColor = DataUtils.DeSerializeColorByHex("#FDC893"),
            rimColor1 = DataUtils.DeSerializeColorByHex("#FFDFC9"),
            brightColor = DataUtils.DeSerializeColorByHex("#D5B29F"),
            brightColor1 = DataUtils.DeSerializeColorByHex("#BDA9A9"),
            darkColor = DataUtils.DeSerializeColorByHex("#A09AAA"),
            darkColor1 = DataUtils.DeSerializeColorByHex("#EDBAA4"),
            secDarkColor = DataUtils.DeSerializeColorByHex("#B6A5AD"),
            secDarkColor1 = DataUtils.DeSerializeColorByHex("#A29CB1"),
        });

        cloudStages.Add(6, new CloudStage()
        {
            lastStageKey = 5,
            enterTime = 6,
            needLerp = true,
            lerpTime = 1,

            rimColor = DataUtils.DeSerializeColorByHex("#FDC893"),
            rimColor1 = DataUtils.DeSerializeColorByHex("#FFDFC9"),
            brightColor = DataUtils.DeSerializeColorByHex("#D5B29F"),
            brightColor1 = DataUtils.DeSerializeColorByHex("#BDA9A9"),
            darkColor = DataUtils.DeSerializeColorByHex("#A09AAA"),
            darkColor1 = DataUtils.DeSerializeColorByHex("#EDBAA4"),
            secDarkColor = DataUtils.DeSerializeColorByHex("#B6A5AD"),
            secDarkColor1 = DataUtils.DeSerializeColorByHex("#A29CB1"),
        });

        cloudStages.Add(7, new CloudStage()
        {
            lastStageKey = 6,
            enterTime = 7,
            needLerp = true,
            lerpTime = 1,

            rimColor = DataUtils.DeSerializeColorByHex("#8BB4B4"),
            rimColor1 = DataUtils.DeSerializeColorByHex("#848484"),
            brightColor = DataUtils.DeSerializeColorByHex("#FBFFFF"),
            brightColor1 = DataUtils.DeSerializeColorByHex("#C3E6E6"),
            darkColor = DataUtils.DeSerializeColorByHex("#C3F6FA"),
            darkColor1 = DataUtils.DeSerializeColorByHex("#EDE7E7"),
            secDarkColor = DataUtils.DeSerializeColorByHex("#E6F2F8"),
            secDarkColor1 = DataUtils.DeSerializeColorByHex("#75C5D9"),
        });

        cloudStages.Add(8, new CloudStage()
        {
            lastStageKey = 7,
            enterTime = 8,
            needLerp = true,
            lerpTime = 1,

            rimColor = DataUtils.DeSerializeColorByHex("#8BB4B4"),
            rimColor1 = DataUtils.DeSerializeColorByHex("#848484"),
            brightColor = DataUtils.DeSerializeColorByHex("#FBFFFF"),
            brightColor1 = DataUtils.DeSerializeColorByHex("#C3E6E6"),
            darkColor = DataUtils.DeSerializeColorByHex("#C3F6FA"),
            darkColor1 = DataUtils.DeSerializeColorByHex("#EDE7E7"),
            secDarkColor = DataUtils.DeSerializeColorByHex("#E6F2F8"),
            secDarkColor1 = DataUtils.DeSerializeColorByHex("#75C5D9"),
        });

        cloudStages.Add(9, new CloudStage()
        {
            enterTime = 9,

            rimColor = DataUtils.DeSerializeColorByHex("#8BB4B4"),
            rimColor1 = DataUtils.DeSerializeColorByHex("#848484"),
            brightColor = DataUtils.DeSerializeColorByHex("#FBFFFF"),
            brightColor1 = DataUtils.DeSerializeColorByHex("#C3E6E6"),
            darkColor = DataUtils.DeSerializeColorByHex("#C3F6FA"),
            darkColor1 = DataUtils.DeSerializeColorByHex("#EDE7E7"),
            secDarkColor = DataUtils.DeSerializeColorByHex("#E6F2F8"),
            secDarkColor1 = DataUtils.DeSerializeColorByHex("#75C5D9"),
        });

        cloudStages.Add(16, new CloudStage()
        {
            lastStageKey = 9,
            enterTime = 16,
            needLerp = true,
            lerpTime = 2,

            rimColor = DataUtils.DeSerializeColorByHex("#FFACAC"),
            rimColor1 = DataUtils.DeSerializeColorByHex("#E2CFC5"),
            brightColor = DataUtils.DeSerializeColorByHex("#FFF18A"),
            brightColor1 = DataUtils.DeSerializeColorByHex("#FFE677"),
            darkColor = DataUtils.DeSerializeColorByHex("#FF9C4C"),
            darkColor1 = DataUtils.DeSerializeColorByHex("#FFAD47"),
            secDarkColor = DataUtils.DeSerializeColorByHex("#FFC67E"),
            secDarkColor1 = DataUtils.DeSerializeColorByHex("#FF8966"),
        });

        cloudStages.Add(18, new CloudStage()
        {
            enterTime = 18,

            rimColor = DataUtils.DeSerializeColorByHex("#FFACAC"),
            rimColor1 = DataUtils.DeSerializeColorByHex("#E2CFC5"),
            brightColor = DataUtils.DeSerializeColorByHex("#FFF18A"),
            brightColor1 = DataUtils.DeSerializeColorByHex("#FFE677"),
            darkColor = DataUtils.DeSerializeColorByHex("#FF9C4C"),
            darkColor1 = DataUtils.DeSerializeColorByHex("#FFAD47"),
            secDarkColor = DataUtils.DeSerializeColorByHex("#FFC67E"),
            secDarkColor1 = DataUtils.DeSerializeColorByHex("#FF8966"),
        });

        cloudStages.Add(20, new CloudStage()
        {
            lastStageKey = 18,
            enterTime = 20,
            needLerp = true,
            lerpTime = 1,

            rimColor = DataUtils.DeSerializeColorByHex("#9996A4"),
            rimColor1 = DataUtils.DeSerializeColorByHex("#92BEE9"),
            brightColor = DataUtils.DeSerializeColorByHex("#618ABE"),
            brightColor1 = DataUtils.DeSerializeColorByHex("#396DA4"),
            darkColor = DataUtils.DeSerializeColorByHex("#34659F"),
            darkColor1 = DataUtils.DeSerializeColorByHex("#93C7FF"),
            secDarkColor = DataUtils.DeSerializeColorByHex("#5989D1"),
            secDarkColor1 = DataUtils.DeSerializeColorByHex("#5E90D5"),
        });

        cloudStages.Add(21, new CloudStage()
        {
            enterTime = 21,

            rimColor = DataUtils.DeSerializeColorByHex("#9996A4"),
            rimColor1 = DataUtils.DeSerializeColorByHex("#92BEE9"),
            brightColor = DataUtils.DeSerializeColorByHex("#618ABE"),
            brightColor1 = DataUtils.DeSerializeColorByHex("#396DA4"),
            darkColor = DataUtils.DeSerializeColorByHex("#34659F"),
            darkColor1 = DataUtils.DeSerializeColorByHex("#93C7FF"),
            secDarkColor = DataUtils.DeSerializeColorByHex("#5989D1"),
            secDarkColor1 = DataUtils.DeSerializeColorByHex("#5E90D5"),
        });

        #endregion
    }

    /// <summary>
    /// 云的消散控制 规则：
    /// 每2s一次，在2s内time从-1.5~1线性插值
    /// </summary>
    private void InitCloudLerpTime()
    {
        if (cloudMats == null)
        {
            cloudMats = new List<Material>();
        }
        else
        {
            cloudMats.Clear();
        }

        var allClouds = cloudTrans.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < allClouds.Length; i++)
        {
            var cloud = allClouds[i];
            if (cloud == null)
            {
                continue;
            }

            // if (i == 0)
            // {
            //     // TimerManager.Inst.Run("cloudTimer",2f, 2f, () =>
            //     // {
            //     //     cloud.material.SetFloat("_time", cloud.material.GetFloat("_time"));
            //     // });
            // }

            // cloud.material.SetFloat("_time", Mathf.Clamp(i * 0.1f, 0.1f, 0.9f));
            cloud.material.SetMatrix("_LToW", cloudTrans.localToWorldMatrix);
            cloudMats.Add(cloud.material);
        }
    }

    private void CloudRotate()
    {
        if (cloudTrans)
        {
            cloudTrans.Rotate(Vector3.up * cloudRotateSpeed * Time.fixedDeltaTime);
        }
    }

    private void CloudColorTrans(float skyTime)
    {
        var timeOfDay = skyTime - (int) skyTime;
        var timeHourOfDay = timeOfDay * 24;
        var hour = (int) timeHourOfDay;
        var needChangeColor = false;

        // LoggerUtils.Log($"fsc---timeOfDay:{timeOfDay}, timeHourOfDay:{timeHourOfDay}, hour:{hour}");

        //只有在强刷/过渡/stage发生改变才需要改材质球
        if (curStage == null)
        {
            curStage = GetcurStage(hour);
            needChangeColor = true;
            // LoggerUtils.Log($"fsc---强刷 找到当前匹配hour:{hour}, curStage enterTime:{curStage.enterTime}");
        }
        else if (cloudStages.ContainsKey(hour) && cloudStages[hour] != curStage)
        {
            curStage = cloudStages[hour];
            needChangeColor = true;
            // LoggerUtils.Log($"fsc---进入新状态：hour:{hour}, curStage:{curStage.enterTime}, isLerp:{curStage.needLerp}");
        }

        if (curStage == null)
        {
            return;
        }

        Color rimColor = default,
            rimColor1 = default,
            brightColor = default,
            brightColor1 = default,
            darkColor = default,
            darkColor1 = default,
            secDarkColor = default,
            secDarkColor1 = default;
        if (curStage.needLerp)
        {
            var lerpTime = (timeHourOfDay - curStage.enterTime) / curStage.lerpTime;
            var lastStage = cloudStages[curStage.lastStageKey];
            rimColor = Color.Lerp(lastStage.rimColor, curStage.rimColor, lerpTime);
            rimColor1 = Color.Lerp(lastStage.rimColor1, curStage.rimColor1, lerpTime);
            brightColor = Color.Lerp(lastStage.brightColor, curStage.brightColor, lerpTime);
            brightColor1 = Color.Lerp(lastStage.brightColor1, curStage.brightColor1, lerpTime);
            darkColor = Color.Lerp(lastStage.darkColor, curStage.darkColor, lerpTime);
            darkColor1 = Color.Lerp(lastStage.darkColor1, curStage.darkColor1, lerpTime);
            secDarkColor = Color.Lerp(lastStage.secDarkColor, curStage.secDarkColor, lerpTime);
            secDarkColor1 = Color.Lerp(lastStage.secDarkColor1, curStage.secDarkColor1, lerpTime);

            needChangeColor = true;
        }
        else if (needChangeColor)
        {
            rimColor = curStage.rimColor;
            rimColor1 = curStage.rimColor1;
            brightColor = curStage.brightColor;
            brightColor1 = curStage.brightColor1;
            darkColor = curStage.darkColor;
            darkColor1 = curStage.darkColor1;
            secDarkColor = curStage.secDarkColor;
            secDarkColor1 = curStage.secDarkColor1;
        }

        if (needChangeColor)
        {
            foreach (var material in cloudMats)
            {
                material.SetColor("_RimColor", rimColor);
                material.SetColor("_RimColor1", rimColor1);
                material.SetColor("_BrightColor", brightColor);
                material.SetColor("_BrightColor1", brightColor1);
                material.SetColor("_DarkColor", darkColor);
                material.SetColor("_DarkColor1", darkColor1);
                material.SetColor("_SecDarkColor", secDarkColor);
                material.SetColor("_SecDarkColor1", secDarkColor1);
            }
        }
    }

    private float deltaTime;

    private void CloudDisappear()
    {
        deltaTime += Time.fixedDeltaTime;
        if (deltaTime >= float.MaxValue)
        {
            deltaTime = 0f;
        }

        var sinV = deltaTime / 4f * Mathf.PI / 2;
        var sinR = Mathf.Sin(sinV);
        var newTValue = -1.5f + 2.5f * sinR;
        newTValue = Mathf.Clamp(newTValue, -1.5f, 2.5f);
        // LoggerUtils.Log($"fsc---------newTValue:{newTValue}");

        for (int i = 0; i < cloudMats.Count; i++)
        {
            if (i % 4 == 0)
            {
                var cMat = cloudMats[i];
                cMat.SetFloat("_time", newTValue);
            }
        }
    }

    private CloudStage GetcurStage(int hour)
    {
        while (!cloudStages.ContainsKey(hour))
        {
            hour--;
        }

        return cloudStages[hour];
    }

    public void OnFixedTimePassed(float skyTime)
    {
        CloudRotate();
        CloudColorTrans(skyTime);
        CloudDisappear();
    }

    public void SetCurrentTiming(float skyTime)
    {
        curStage = null;
        deltaTime = 0f;
        CloudColorTrans(skyTime);
    }
}
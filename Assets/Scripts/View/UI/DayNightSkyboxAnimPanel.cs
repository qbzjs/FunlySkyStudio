using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

/// <summary>
/// Author:Shaocheng
/// Description:昼夜天空盒左下角动画
/// Date: 2022-9-16 10:36:01
/// </summary>
public class DayNightSkyboxAnimPanel : BasePanel<DayNightSkyboxAnimPanel>
{
    public ScrollRect dayScroll;
    public int curDayNum;
    private Tweener dayTweener;

    public Image iconImg;
    public RectTransform iconCtrl;
    private float sunStartTime;
    private float moonStartTime;
    private Sprite sunSp;
    private Sprite moonSp;
    private Quaternion imgQuara;
    
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        sunStartTime = 6f / 24f; //早上六点开始显示太阳
        moonStartTime = 18f / 24f; //晚上六点开始显示月亮
        imgQuara = Quaternion.Euler(Vector3.zero);
        
        sunSp = ResManager.Inst.GetGameAtlasSprite("ic_sun");
        moonSp = ResManager.Inst.GetGameAtlasSprite("ic_moon");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (dayTweener != null)
        {
            dayTweener.Kill();
            dayTweener = null;
        }
    }

    public override void OnBackPressed()
    {
        if (dayTweener != null)
        {
            dayTweener.Kill();
            dayTweener = null;
        }

        ResetAllAnim();

        base.OnBackPressed();
    }


    public void OnFixedTimePassed(float skyTime)
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }
        SunMoonRotate(skyTime);
        ChangeDayNum(skyTime);
    }

    private void ResetAllAnim()
    {
        //天数重置
        dayScroll.content.GetChild(0).GetComponent<Text>().text = "0";
        dayScroll.content.GetChild(1).GetComponent<Text>().text = "1";
        dayScroll.verticalNormalizedPosition = 1;
        curDayNum = 0;

        //旋转sun moon
        iconCtrl.eulerAngles = Vector3.zero;
    }


    //下方天数动画
    private void ChangeDayNum(float skyTime)
    {
        var dayNum = (int) skyTime;
        //只在天数变化时执行，避免频繁调用
        if (curDayNum == dayNum)
        {
            return;
        }

        curDayNum = dayNum;

        var dNew = dayScroll.content.GetChild(1).GetComponent<Text>();
        dNew.text = curDayNum.ToString();
        dayTweener = DOTween.To(() => dayScroll.verticalNormalizedPosition,
            x => dayScroll.verticalNormalizedPosition = x, 0, 1f);
        dayTweener.onComplete += () =>
        {
            var curD = dayScroll.content.GetChild(0).GetComponent<Text>();
            curD.text = curDayNum.ToString();
            dayScroll.verticalNormalizedPosition = 1;
        };
    }

    //太阳月亮旋转动画
    private void SunMoonRotate(float skyTime)
    {
        var timeOfDay = SkyboxManager.Inst.GetTimeOfDay(skyTime);
        float rotateV = 0f;
        if (timeOfDay >= sunStartTime && timeOfDay <= moonStartTime)
        {
            if (iconImg.sprite != sunSp)
            {
                iconImg.sprite = sunSp;
            }
            rotateV = -(float) ((timeOfDay - sunStartTime) / 0.5 * 215) - 5;
        }
        else
        {
            if (iconImg.sprite != moonSp)
            {
                iconImg.sprite = moonSp;
            }
            if (timeOfDay > moonStartTime && timeOfDay < 1)
            {
                // 18 ~ 24点之间
                rotateV = -(float) ((timeOfDay - moonStartTime) / 0.25 * (215/2f)) - 5;
            }
            else if (timeOfDay > 0 && timeOfDay < sunStartTime)
            {
                // 24 ~ 6点之间
                rotateV = -(float) ((215/2f) + (timeOfDay / 0.25 * (215/2f))) - 5;
            }
        }
        // LoggerUtils.Log($"fsc SunMoonRotate: timeOfDay:{timeOfDay}, sunStart:{sunStartTime}, moonStart:{moonStartTime}, rotateV:{rotateV}");
        var ctrlEuler = iconCtrl.eulerAngles;
        ctrlEuler.z = rotateV;
        iconCtrl.eulerAngles = ctrlEuler;
        iconImg.transform.rotation = imgQuara;
    }

    public void SetAnimShow(bool isShow)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Edit)
        {
            return;
        }

        gameObject.SetActive(isShow);
    }
}
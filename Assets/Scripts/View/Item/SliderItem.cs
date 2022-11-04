using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Author : Tee Li
/// 描述：滑条与展示数字组合Item
/// 日期：2022/10/10
/// </summary>

public class SliderItem : MonoBehaviour
{

    public Slider slideBar;
    public Text numberTxt;
    public NumberSetting numberSetting; //只负责配置如何展示数字，不处理回调值

    private void Awake()
    {
        InitOnAwake();
    }

    public virtual void InitOnAwake()
    {
        AddBuildinListeners();
    }

    protected virtual void AddBuildinListeners()
    {
        slideBar?.onValueChanged.AddListener(SetText);
    }

    public void AddListener(UnityAction<float> onChange)
    {
        slideBar?.onValueChanged.AddListener(onChange);
    }

    public void SetValue(float value)
    {
        slideBar?.Set(value);
    }

    public void SetValueWithoutNotify(float value)
    {
        slideBar?.SetValueWithoutNotify(value);
        SetText(value);
    }

    private void SetText(float value)
    {
        if (numberSetting.enabled && numberTxt)
        {
            string txt = numberSetting.roundToIntType switch
            {
                RoundType.NoRound => value.ToString("F" + numberSetting.keepDigit),
                RoundType.Round => Mathf.RoundToInt(value).ToString(),
                RoundType.RoundUp => Mathf.CeilToInt(value).ToString(),
                RoundType.RoundDown => Mathf.FloorToInt(value).ToString(),
                _ => ""
            };
            numberTxt.text = txt;
        }
    }
}

[System.Serializable]
public class NumberSetting
{
    public bool enabled;
    public RoundType roundToIntType;
    public int keepDigit;    
}

public enum RoundType
{
    NoRound = 0,
    Round = 1,
    RoundUp = 2,
    RoundDown = 3   
}

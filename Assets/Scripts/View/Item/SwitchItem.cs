using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Author : Tee Li
/// 描述：单个Toggle开关Item
/// 日期：2022/10/10
/// </summary>

public class SwitchItem : MonoBehaviour
{
    public Toggle toggle;
    
    public void AddListener(UnityAction<bool> onChange)
    {
        toggle.onValueChanged.AddListener(onChange);
    }

    public void SetValue(bool isOn)
    {
        toggle.Set(isOn);
    }

    public void SetValueWithoutNotify(bool isOn)
    {
        toggle.SetIsOnWithoutNotify(isOn);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Author : Tee Li
/// 描述：用Toggle选择多种类型的面板Item
/// 日期：2022/10/10
/// </summary>

public class ToggleGroupItem : MonoBehaviour
{
    public GameObject toggleParent;
    public List<Toggle> Toggles { get; private set; }

    private void Awake()
    {
        InitOnAwake();
    }

    public virtual void InitOnAwake()
    {
        Toggles = new List<Toggle>(toggleParent.GetComponentsInChildren<Toggle>());
    }

    public void AddListener(string name, UnityAction<bool> onChange)
    {
        Toggle tgl = FindWithGoName(name);
        tgl?.onValueChanged.AddListener(onChange);
    }

    public void SetValue(string name, bool isOn)
    {
        Toggle tgl = FindWithGoName(name);
        tgl?.Set(isOn);
    }

    public void SetValueWithoutNotify(string name, bool isOn)
    {
        Toggle tgl = FindWithGoName(name);
        tgl?.SetIsOnWithoutNotify(isOn);
    }

    public Toggle FindWithGoName(string name)
    {
        return Toggles.Find(t => t.gameObject.name == name);
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description:显隐控制Component
/// Date: 2022-3-30 19:43:08
/// </summary>
[Serializable]
public class ShowHideData
{
    public int show; //0 - show ，1-hide
    public List<int> switchs = new List<int>();
}

/// <summary>
/// 只有开关和被开关控制的道具添加ShowHideComponent,以减少json大小
/// </summary>
public class ShowHideComponent : IComponent
{
    public int defaultShow = 0;//0-物体显隐初始可见，1-初始不可见
    public List<int> switchUids = new List<int>();

    public IComponent Clone()
    {
        var comp = new ShowHideComponent();
        comp.defaultShow = defaultShow;
        var tempList = new List<int>();
        foreach (var switchUid in switchUids)
        {
            tempList.Add(switchUid);
        }
        comp.switchUids = tempList;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        ShowHideData data = new ShowHideData
        {
            show = defaultShow,
            switchs = switchUids,
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.ShowHide,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
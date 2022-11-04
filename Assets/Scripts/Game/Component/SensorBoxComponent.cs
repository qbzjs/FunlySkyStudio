using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Author:JayWill
/// Description:感应盒Component,记录感应盒属性以及控制的物体uid
/// </summary>

[Serializable]
public class SensorBoxData
{
    public int index;
    public int times = -1;
    public List<int> visCtrs = new List<int>();
    public List<int> moveCtrs = new List<int>();
    public List<int> soundCtrs = new List<int>();
    public List<int> animCtrs = new List<int>();
    public List<int> fireworkCtrs = new List<int>();
}

public class SensorBoxComponent : IComponent
{
    public List<int> visibleCtrlUids = new List<int>(); //可见性控制实体uid列表
    public List<int> moveCtrlUids = new List<int>(); //移动控制实体uid列表
    public List<int> soundCtrlUids = new List<int>(); //声音控制实体uid列表
    public List<int> animCtrlUids = new List<int>(); // 旋转移动控制实体uid列表
    public List<int> fireworkCtrlUids = new List<int>(); // 烟花控制实体uid列表

    public int boxIndex = 0;
    public int boxTimes = -1;//感应盒能使用的次数，-1为无限次

    public IComponent Clone()
    {
        var comp = new SensorBoxComponent();
        comp.visibleCtrlUids = new List<int>();
        comp.moveCtrlUids = new List<int>();
        comp.soundCtrlUids = new List<int>();
        comp.animCtrlUids = new List<int>();
        comp.fireworkCtrlUids = new List<int>();
        comp.boxTimes = boxTimes;
        comp.boxIndex = SensorBoxManager.Inst.GetNewIndex();
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        SensorBoxData data = new SensorBoxData
        {
            times = boxTimes,
            index = boxIndex,
            visCtrs = visibleCtrlUids,
            moveCtrs = moveCtrlUids,
            soundCtrs = soundCtrlUids,
            animCtrs = animCtrlUids,
            fireworkCtrs = fireworkCtrlUids
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.SensorBox,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
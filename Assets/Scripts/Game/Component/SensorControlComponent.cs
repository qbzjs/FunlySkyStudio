using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Author:JayWill
/// Description:被感应盒控制的物体时添加SensorControlComponent
/// </summary>

[Serializable]
public class SensorControlData
{
    public List<int> moveUids = new List<int>();//控制移动的感应盒
    public List<int> visUids = new List<int>();//控制可见性的感应盒
    public List<int> soundUids = new List<int>();//控制声音的感应盒
    public List<int> animUids = new List<int>();//控制旋转移动的感应盒
    public List<int> fireworkUids = new List<int>();//控制烟花的感应盒
}
public class SensorControlComponent : IComponent
{
    public List<int> moveSensorUids = new List<int>();
    public List<int> visibleSensorUids = new List<int>();
    public List<int> soundSensorUids = new List<int>();
    public List<int> animSensorUids = new List<int>();
    public List<int> fireworkSensorUids = new List<int>();

    public IComponent Clone()
    {
        var comp = new SensorControlComponent();
        var moveList = new List<int>();
        foreach (var uid in moveSensorUids)
        {
            moveList.Add(uid);
        }
        comp.moveSensorUids = moveList;
        
        var visList = new List<int>();
        foreach (var uid in visibleSensorUids)
        {
            visList.Add(uid);
        }
        comp.visibleSensorUids = visList;

        var soundList = new List<int>();
        foreach (var uid in soundSensorUids)
        {
            soundList.Add(uid);
        }
        comp.soundSensorUids = soundList;

        var animList = new List<int>();
        foreach (var uid in animSensorUids)
        {
            animList.Add(uid);
        }
        comp.animSensorUids = animList;

        var fireworkList = new List<int>();
        foreach (var uid in fireworkSensorUids)
        {
            fireworkList.Add(uid);
        }
        comp.fireworkSensorUids = fireworkList;

        return comp;
    }

    public BehaviorKV GetAttr()
    {
        SensorControlData data = new SensorControlData()
        {
            moveUids = moveSensorUids,
            visUids = visibleSensorUids,
            soundUids = soundSensorUids,
            animUids = animSensorUids,
            fireworkUids = fireworkSensorUids,
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.SensorControl,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
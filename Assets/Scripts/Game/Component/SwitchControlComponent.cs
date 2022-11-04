using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class SwitchControlData
{
    public List<int> switchs = new List<int>();
    public List<int> soundUids = new List<int>(); // 开关控制声音播放的实体 uid List
    public List<int> animUids = new List<int>();
    public List<int> fireworkUids = new List<int>();
    public int switchCtrlType;
    public int playSound;
}

/// <summary>
/// Author:WenJia
/// Description:开关控制关联的 Component，主要用来保存由开关控制的物体的开关控制相关的数据和信息
/// 相当于将开关控制能力单独拆分出来
/// Date: 2022/1/20 16:10:17
/// </summary>

public class SwitchControlComponent : IComponent
{
    public List<int> switchUids = new List<int>();// 开关控制移动的实体 uid List
    public List<int> switchSoundUids = new List<int>(); // 开关控制声音播放的实体 uid List
    public List<int> switchAnimUids = new List<int>(); // 开关控制旋转移动的实体 uid List
    public List<int> switchFireworkUids = new List<int>(); // 开关控制烟花播放的实体 uid List
    [Obsolete("该字段属于老版本兼容字段, 现已弃用")]
    public int switchControlType; // 开关控制类型 [弃用，使用 uid List 来控制]  
    [Obsolete("该字段属于老版本兼容字段, 现已弃用")]
    public int controlPlaySound; // 开关是否支持音乐播放控制 1-支持音乐播放控制，0-不支持音乐控制 [弃用，使用 uid List 来控制]

    public IComponent Clone()
    {
        var comp = new SwitchControlComponent();
        var tempList = new List<int>();
        foreach (var switchUid in switchUids)
        {
            tempList.Add(switchUid);
        }

        var tempSoundList = new List<int>();
        foreach (var soundUid in switchSoundUids)
        {
            tempSoundList.Add(soundUid);
        }

        var tempAnimList = new List<int>();
        foreach (var animUid in switchAnimUids)
        {
            tempAnimList.Add(animUid);
        }

        var temFireworkList = new List<int>();
        foreach (var animUid in switchFireworkUids)
        {
            temFireworkList.Add(animUid);
        }

        comp.switchUids = tempList;
        comp.switchSoundUids = tempSoundList;
        comp.switchAnimUids = tempAnimList;
        comp.switchFireworkUids = temFireworkList;
        comp.switchControlType = switchControlType;
        comp.controlPlaySound = controlPlaySound;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        SwitchControlData data = new SwitchControlData()
        {
            switchs = switchUids,
            soundUids = switchSoundUids,
            animUids = switchAnimUids,
            fireworkUids = switchFireworkUids,
            switchCtrlType = switchControlType,
            playSound = controlPlaySound
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.SwitchControl,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
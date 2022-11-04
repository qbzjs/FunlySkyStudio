using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description:开关道具Component
/// Date: 2022-3-30 19:43:08
/// </summary>
[Serializable]
public class SwitchButtonData
{
    public int sid;
    public List<int> controlls = new List<int>();
    public List<int> moveControlls = new List<int>();
    public List<int> soundControlls = new List<int>();
    public List<int> animControlls = new List<int>();
    public List<int> fireworkControlls = new List<int>();
}

public class SwitchButtonComponent : IComponent
{
    // 显隐控制实体uid列表
    // 不能随便更改json文本原有字段，故注释说明。
    public List<int> controllUids = new List<int>();
    //移动控制实体uid列表
    public List<int> moveControllUids = new List<int>();
    //音乐播放控制实体uid列表
    public List<int> soundControllUids = new List<int>();
    //旋转移动控制实体uid列表
    public List<int> animControllUids = new List<int>();
    //烟花播放控制实体uid列表firework
    public List<int> fireworkControllUids = new List<int>();
    public int switchId = 0;

    public IComponent Clone()
    {
        var comp = new SwitchButtonComponent();
        comp.controllUids = new List<int>();
        comp.moveControllUids = new List<int>();
        comp.soundControllUids = new List<int>();
        comp.animControllUids = new List<int>();
        comp.fireworkControllUids = new List<int>();
        comp.switchId = SwitchManager.Inst.GetNewSwitchId();
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        SwitchButtonData data = new SwitchButtonData
        {
            sid = switchId,
            controlls = controllUids,
            moveControlls = moveControllUids,
            soundControlls = soundControllUids,
            animControlls = animControllUids,
            fireworkControlls = fireworkControllUids
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.SwitchButton,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
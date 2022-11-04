/// <summary>
/// Author:MeiMei—LiMei
/// Description: 音效道具存储数据的Component
/// Date: 2022-01-13
/// </summary>
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SoundButtonData
{
    public string sName;
    public string sUrl;
    public musicType musicType;
    public int musicId;
    public int isControl = 0; // 音乐按钮是否支持控制，默认支持控制: 0-支持控制 1-不支持
}
public enum musicType
{
    importMusic,
    noMusic,
}

public class SoundComponent : IComponent
{
    public string soundName = "";//导入的音效名
    public string soundUrl = "";//导入的音效url
    public musicType musicType;//设置的音乐类型
    public int isControl = (int)SoundControl.NOT_SUPPORT; // 音乐按钮是否支持控制，默认不支持

    public IComponent Clone()
    {
        var comp = new SoundComponent();
        comp.soundName = soundName;
        comp.soundUrl = soundUrl;
        comp.musicType = musicType;
        comp.isControl = isControl;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        SoundButtonData data = new SoundButtonData
        {
            sName = soundName,
            sUrl = soundUrl,
            musicType=musicType,
            isControl = isControl
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.Sound,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

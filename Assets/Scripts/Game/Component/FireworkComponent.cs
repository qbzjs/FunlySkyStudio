using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:烟花道具组件
/// Date: 2022/7/20 18:30:29
/// </summary>
public enum FireworkHeight
{
    Low = 0,
    Medium = 1,
    Heigh = 2,
}
public enum FireworkPointState
{
    Off = 0,
    On = 1
}
[Serializable]
public class FireworkData
{
    public string rId;
    public string fireworkcolor;
    public int fireworkHeight;
    public Vec3 anchorsPos;
    public int isCustomPoint;
    public int isControl = 0;// 烟花道具是否支持控制，默认支持控制: 0-支持控制 1-不支持
}

public class FireworkComponent : IComponent
{
    public string rId;//素材id
    public string fireworkcolor;
    public int fireworkHeight = 7;
    public Vec3 anchorsPos = Vector3.zero;//烟花发射点
    public int isCustomPoint = (int)FireworkPointState.Off;//是否编辑过准心
    public int isControl = (int)FireworkControl.NOT_SUPPORT;
    public IComponent Clone()
    {
        FireworkComponent component = new FireworkComponent();
        component.rId = rId;
        component.fireworkcolor = fireworkcolor;
        component.fireworkHeight = fireworkHeight;
        component.anchorsPos = anchorsPos;
        component.isCustomPoint = isCustomPoint;
        component.isControl = isControl;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        FireworkData data = new FireworkData
        {
            rId = rId,
            fireworkcolor = fireworkcolor,
            fireworkHeight = fireworkHeight,
            anchorsPos = anchorsPos,
            isCustomPoint = isCustomPoint,
            isControl = isControl,
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.Firework,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

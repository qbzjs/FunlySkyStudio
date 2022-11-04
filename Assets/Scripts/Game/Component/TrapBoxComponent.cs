using Newtonsoft.Json;
using System;
using UnityEngine;

[Serializable]
public struct TrapBoxData
{
    public int id;
    public int rPos;
    public int rTex;
    public string tex;
    public int hitS;
    public float hitV;
}

/// <summary>
/// Author: 熊昭
/// Description: 陷阱盒道具数据类
/// Date: 2021-11-26 14:50:28
/// </summary>
public class TrapBoxComponent : IComponent
{
    public int tId = 0;   //id : 1~99
    public int rePos = 0;   //0 : born point ; 1 : custom point  2:原地
    public int reTex = 0;   //0 : default text ; 1 : custom text
    public string text = "";   //custom text (if reTex = 0 , text is empty)
    public int pId = 0;//关联复活点的ID 目前仅供undo/redo 删除恢复时使用，请慎重使用
    public int tempRePos = 0;//用以记录陷阱盒删除时，是否带有复活点
    public int hitState = 0;//是否开启伤害 0:关闭； 1:打开
    public float hitValue = 20;//每次伤害值,默认20

    public IComponent Clone()
    {
        TrapBoxComponent component = new TrapBoxComponent();
        component.tId = tId;
        component.rePos = rePos;
        component.reTex = reTex;
        component.text = text;
        component.hitState = hitState;
        component.hitValue = hitValue;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        TrapBoxData data = new TrapBoxData
        {
            id = tId,
            rPos = rePos,
            rTex = reTex,
            tex = text,
            hitS = hitState,
            hitV = hitValue
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.TrapBox,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
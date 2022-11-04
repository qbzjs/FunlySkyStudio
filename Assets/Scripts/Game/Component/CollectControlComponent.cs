/*
* @Author: YangJie
 * @LastEditors: wenjia
* @Description: 收集道具
* @Date: ${YEAR}-${MONTH}-${DAY} ${TIME}
* @Modify:
*/


using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class CollectControlData
{
    public int isControl;
    public bool moveActive;
    public int playSound;
    public int animActive;
    public int playfirework;
}


// 只有道具被收集控制时需要添加该道具
public class CollectControlComponent  : IComponent
{

    public int isControl = 0;//0-不被星星控制 1-被星星控制
    public bool moveActive = false;
    public int triggerCount = 0;
    public int playSound;
    public int animActive;
    public int playfirework;

    public IComponent Clone()
    {
        var comp = new CollectControlComponent
        {
            isControl = isControl,
            moveActive = moveActive,
            playSound = playSound,
            animActive = animActive,
            playfirework = playfirework
        };
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        
        LoggerUtils.Log("CollectControlComponent: GetAttr" );
        CollectControlData data = new CollectControlData
        {
            isControl = isControl,
            moveActive = moveActive,
            playSound = playSound,
            animActive = animActive,
            playfirework = playfirework,
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.CollectControl,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
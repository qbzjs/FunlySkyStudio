using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

[SerializeField]
public class RPAnimData
{
    public int rsd;
    public int usd;
    public int rax;
    public int aState;
}

public class RPAnimComponent: IComponent
{
    public int rSpeed = 0;
    public int uSpeed = 0;
    
    /// <summary>
    /// 0 Y轴， 1 X轴， 2 Z轴 
    /// </summary>
    public int rAxis = 0;
    // 物体旋转状态使用 int 类型  0-旋转 1-静止
    public int animState;
    // 临时记录物体旋转状态的变量，用于在试玩和游玩中改变，但不保存到json，保证json数据的正确性
    public int tempAnimState;

    public IComponent Clone()
    {
        var comp = new RPAnimComponent();
        comp.rSpeed = rSpeed;
        comp.uSpeed = uSpeed;
        comp.rAxis = rAxis;
        comp.animState = animState;
        comp.tempAnimState = animState;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        if (rSpeed == 0 && uSpeed == 0)
            return null;
        RPAnimData data = new RPAnimData
        {
            rsd = rSpeed,
            usd = uSpeed,
            rax = rAxis,
            aState = animState,
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.RPAnim,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/7/21 20:26:11
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct DcData
{
    public int isDc;
    public string id;
    public string address;
    public string actId;//透传，申请详情页接口使用
}
public class DcComponent : IComponent
{
    public int isDc = 0;
    public string dcId = "";
    public string address = "";
    public string budActId = "";//透传，申请详情页接口使用
    public IComponent Clone()
    {
        DcComponent component = new DcComponent();
        component.isDc = isDc;
        component.dcId = dcId;
        component.address = address;
        component.budActId = budActId;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        DcData data = new DcData
        {
            isDc = isDc,
            id = dcId,
            address = address,
            actId = budActId
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.DC,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
public enum IsDC
{
    False,
    True
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: pzkunn
/// Description: 冰晶宝石组件
/// Date: 2022/10/21 13:15:36
/// </summary>
public class CrystalStoneComponent : IComponent
{
    public IComponent Clone()
    {
        return new CrystalStoneComponent();
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}

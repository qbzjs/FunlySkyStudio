/// <summary>
/// Author:LiShuZhan
/// Description:背包component
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaggageComponent : IComponent
{
    public int openBaggage;
    public IComponent Clone()
    {
        BaggageComponent component = new BaggageComponent();
        component.openBaggage = openBaggage;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWheelComponent : IComponent
{
    public IComponent Clone()
    {
        return new SteeringWheelComponent();
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}

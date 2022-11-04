using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:
/// Date: 2022/5/19 16:10:2
/// </summary>


public interface IBloodProp
{
    void OnCreate(NodeBaseBehaviour behv);

    void OnDisappear();
}

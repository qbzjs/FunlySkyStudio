using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
[Serializable]
public class MovementPropertyData
{
    public int ta;
    public int sd;
    public List<string> points;
    public int moveState;
}
public class MovementComponent:IComponent
{
    public int turnAround;
    public int speedId;
    public List<Vector3> pathPoints = new List<Vector3>();

    // 临时记录物体移动状态的变量，用于在试玩和游玩中改变，但不保存到json，保证json数据的正确性
    public int tempMoveState;

    // 为了兼容之前保存的数据，最好新加属性不要使用 bool 类型
    // 物体移动状态使用 int 类型  0-移动 1-静止
    public int moveState;

    public IComponent Clone()
    {
        var comp = new MovementComponent();
        comp.turnAround = turnAround;
        comp.speedId = speedId;
        comp.pathPoints = new List<Vector3>();
        comp.pathPoints.AddRange(pathPoints);
        comp.moveState = moveState;

        comp.tempMoveState = moveState;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        MovementPropertyData data = new MovementPropertyData()
        {
            ta = turnAround,
            sd = speedId,
            points = pathPoints.Select(x => DataUtils.Vector3ToString(x)).ToList(),
            moveState = moveState,
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.Movement,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
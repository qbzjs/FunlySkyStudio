using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:LiShuZhan
/// Description:排行榜组件，用来记录排行榜状态
/// Date: 2022.04.14
/// </summary>
public enum LeaderBoardModeType
{
    None,
    Win,
}

public struct LeaderBoardData
{
    public int curMode;
}

public class LeaderBoardComponent : IComponent
{
    public int curMode = (int)LeaderBoardModeType.None;

    public IComponent Clone()
    {
        LeaderBoardComponent component = new LeaderBoardComponent();
        component.curMode = curMode;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        LeaderBoardData data = new LeaderBoardData
        {
            curMode = curMode,
        };

        return new BehaviorKV
        {
            k = (int)BehaviorKey.LeaderBoard,
            v = JsonConvert.SerializeObject(data)
        };
    }
}

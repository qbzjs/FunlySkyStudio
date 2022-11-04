using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PVPWaitAreaComponent : IComponent
{
    public int gameMode;//1-Race 2-Survival 
    public RaceGameData raceData;
    public List<List<int>> teamList;

    public IComponent Clone()
    {
        return null;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}
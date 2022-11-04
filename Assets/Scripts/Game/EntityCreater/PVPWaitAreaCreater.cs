using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Game.Core;
using Newtonsoft.Json;
using UnityEngine;

public enum PVPWaitAreaTaskType
{
    None = 0,
    SwitchButton = 1,
    SensorBox = 2,
}

/// <summary>
/// 传给服务端的类型
/// </summary>
public enum PVPServerTaskType
{
    Race = 1, //所有不勾选，默认Race
    Survival = 2,
    SensorBox = 3
}

public class PVPWaitAreaCreater : SceneEntityCreater
{
    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.PVPWaitArea);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        if (!assetGo.TryGetComponent(out SpawnPointConstrainer adjustBehav))
        {
            adjustBehav = assetGo.AddComponent<SpawnPointConstrainer>();
            adjustBehav.minHeight = 0;
        }
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.PVPWaitArea;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.PVP;
        gameComponent.modelType = NodeModelType.PVPWaitArea;
        behav.OnInitByCreate();
        return behav;
    }

    public static void SetDefaultData(PVPWaitAreaBehaviour behaviour, Vector3 pos)
    {
        if (behaviour == null)
        {
            LoggerUtils.Log("PVPWaitAreaBehaviour is null");
            return;
        }
        var pvpComp = behaviour.entity.Get<PVPWaitAreaComponent>();
        
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = Vector3.zero;
        behaviour.transform.localScale = Vector3.one * 5;
        pvpComp.gameMode = (int)PVPServerTaskType.Race;
        pvpComp.raceData.pvpTime = 300;
    }

    public static void SetData(PVPWaitAreaBehaviour behaviour, PVPWaitAreaData data)
    {
        if (behaviour == null)
        {
            LoggerUtils.Log("PVPWaitAreaBehaviour is null");
            return;
        }

        Vector3 pos = DataUtils.DeSerializeVector3(data.p);
        Vector3 rot = DataUtils.DeSerializeVector3(data.r);
        Vector3 sca = DataUtils.DeSerializeVector3(data.s);
        behaviour.transform.position = pos;
        behaviour.transform.eulerAngles = rot;
        behaviour.transform.localScale = sca;
        
        var pvpComp = behaviour.entity.Get<PVPWaitAreaComponent>();
        var rData = JsonConvert.DeserializeObject<RaceGameData>(data.gameCondition);
        pvpComp.gameMode = data.gameMode;
        pvpComp.raceData.pvpTime = rData.pvpTime;
        pvpComp.raceData.taskArg = rData.taskArg;
        pvpComp.raceData.taskArga = rData.taskArga;
        if (data.teamList != null)
        {
            pvpComp.teamList = data.teamList;
        }
    }
}
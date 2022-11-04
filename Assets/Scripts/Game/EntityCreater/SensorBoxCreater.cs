using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author:JayWill
/// Description:感应盒Creater,用以创建并绑定感应盒数据
/// </summary>

public class SensorBoxCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.SensorBox);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.SensorBox;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Base;
        gameComponent.modelType = NodeModelType.SensorBox;
        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent = null)
    {
        LoggerUtils.Log("SensorBoxCreator SetData:"+data);
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        var newParent = parent ?? SceneBuilder.Inst.StageParent;
        behaviour.transform.SetParent(newParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;

        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        SetData(behaviour as SensorBoxBehaviour, GameUtils.GetAttr<SensorBoxData>((int)BehaviorKey.SensorBox, data.attr));
    }

    public static void SetData(SensorBoxBehaviour behaviour, SensorBoxData data)
    {
        var mComp = behaviour.entity.Get<SensorBoxComponent>();
        behaviour.Clear();
        mComp.boxIndex = data.index;
        mComp.boxTimes = data.times;
        mComp.visibleCtrlUids = data.visCtrs;
        mComp.moveCtrlUids = data.moveCtrs;
        mComp.soundCtrlUids = data.soundCtrs;
        mComp.animCtrlUids = data.animCtrs;
        mComp.fireworkCtrlUids = data.fireworkCtrs;
        behaviour.RefreshIndex();
        SensorBoxManager.Inst.UpdateMaxIndex(mComp.boxIndex);
        SensorBoxManager.Inst.AddSensorBox(behaviour);
    }
}
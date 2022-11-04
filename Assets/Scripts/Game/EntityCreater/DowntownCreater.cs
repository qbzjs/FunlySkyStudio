using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using HLODSystem;
using UnityEngine;

public class DowntownCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        GameObject assetGo = new GameObject("DowntownNode");
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.type = ResType.Downtown;
        return behav;
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Transform parent = null)
    {
        var downtownBev = behaviour as DowntownNodeBehaviour;
        if (downtownBev == null)
        {
            return;
        }

        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        var pos = DataUtils.DeSerializeVector3(data.p);
        sca = DataUtils.LimitVector3(sca);
        downtownBev.transform.SetParent(parent ? parent : SceneBuilder.Inst.StageParent);
        downtownBev.transform.localPosition = pos;
        downtownBev.transform.localEulerAngles = rot;
        downtownBev.transform.localScale = sca;
        downtownBev.data = data;
        var gameComp = downtownBev.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        gameComp.resId = data.rid;
        gameComp.modId = data.id;

        //var scenePrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/DownTown/bud_snowfield");
        //GameObject.Instantiate(scenePrefab, behaviour.transform);
        BundleMgr.Inst.LoadBundle(BundlePart.Respgc, "down_town_public", (bundle) =>
        {
            LoadAsset(downtownBev, data.id);
        });
    }

    public static void LoadAsset(DowntownNodeBehaviour downtownBev, int id, Action<Transform> onSucc = null, Action onFail = null)
    {
        if (!GameManager.Inst.priConfigData.TryGetValue(id, out var modelData))
        {
            onFail?.Invoke();
            return;
        };

        ModelCachePool.Inst.GetSync(downtownBev, id, (go) =>
        {
            var dt = SetAsset(downtownBev, go, id);
            if (dt != null)
            {
                onSucc?.Invoke(dt);
            }
            else
            {
                onFail?.Invoke();
            }
        }, onFail);
    }

    public static Transform SetAsset(DowntownNodeBehaviour downtownBev, GameObject assetObj, int id)
    {
        if (assetObj == null)
        {
            return null;
        }

        var parent = downtownBev.transform;
        if (parent != null)
        {
            assetObj.transform.SetParent(parent);
            assetObj.Normalize();
            downtownBev.assetObj = assetObj;
            downtownBev.OnInitByCreate();
        }

        return assetObj.transform;
    }
}
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using HLODSystem;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:PGC 特效创建
/// Date: 2022/10/25 14:43:40
/// </summary>
public class PGCEffectCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = new GameObject("PGCEffect");
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Special;
        gameComponent.modelType = NodeModelType.PGCEffect;

        AddConstrainer(behav);

        return behav as T;
    }

    //限制位置不能穿透到地底下
    public void AddConstrainer(NodeBaseBehaviour nBehav)
    {
        if (!nBehav.gameObject.TryGetComponent(out SpawnPointConstrainer adjustBehav))
        {
            adjustBehav = nBehav.gameObject.AddComponent<SpawnPointConstrainer>();
            adjustBehav.minHeight = 0.2f;;
        }
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static bool CanCloneTarget(GameObject target)
    {
        return true;
    }

    public static void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        var sBehav = nBehaviour as PGCEffectBehaviour;
        PGCEffectManager.Inst.AddItem(sBehav);
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vec3 pos, Transform parent = null)
    {
        var nodeBehaviour = behaviour as PGCEffectBehaviour;
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        var newParent = parent ? parent : SceneBuilder.Inst.StageParent;
        behaviour.transform.SetParent(newParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;
        ((PGCEffectBehaviour)behaviour).data = data;
        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        gameComp.modId = data.id;
        gameComp.handleType = NodeHandleType.Special;

        nodeBehaviour.OnInitByCreate();
        SetData(data.id, nodeBehaviour, GameUtils.GetAttr<PGCEffectData>((int)BehaviorKey.PGCEffect, data.attr));
    }

    public static void SetData(int id, PGCEffectBehaviour nBehaviour, PGCEffectData data)
    {
        var mComp = nBehaviour.entity.Get<PGCEffectComponent>();
        if (data != null)
        {
            mComp.effectColor = data.col;
            mComp.playSound = data.sound;
            mComp.useDefColor = data.def;
        }
        else
        {
            var effectConfig = PGCEffectManager.Inst.GetPGCEffectConfigData(id);
            mComp.effectColor = effectConfig.defColor;
        }

        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            nBehaviour.SetLODStatus(HLODState.High);
            nBehaviour.UpdateAssetObj(nBehaviour.assetObj, id);
        }
    }
}

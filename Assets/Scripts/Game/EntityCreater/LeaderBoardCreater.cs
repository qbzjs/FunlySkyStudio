/// <summary>
/// Author:LiShuZhan
/// Description:排行榜创建
/// Date: 2022-4-25 17:44:22
/// </summary>
using Assets.Scripts.Game.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderBoardCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.LeaderBoard);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        behav.gameObject.transform.localPosition = CameraUtils.Inst.GetCreatePosition();
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.LeaderBoard;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.SpecialCombine;
        gameComponent.modelType = NodeModelType.LeaderBoard;

        return behav;
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
        int uid = nBehaviour.entity.Get<GameObjectComponent>().uid;
        LeaderBoardManager.Inst.AddLeaderBoard(uid, nBehaviour);
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent = null)
    {
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
        SetData(behaviour as LeaderBoardBehaviour, GameUtils.GetAttr<LeaderBoardData>((int)BehaviorKey.LeaderBoard, data.attr));
    }

    public static void SetData(LeaderBoardBehaviour behaviour, LeaderBoardData data)
    {
        var mComp = behaviour.entity.Get<LeaderBoardComponent>();
        mComp.curMode = data.curMode;
        var gComp = behaviour.entity.Get<GameObjectComponent>();
        LeaderBoardManager.Inst.AddLeaderBoard(gComp.uid, behaviour);
    }
}

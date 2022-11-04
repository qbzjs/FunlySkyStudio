using Assets.Scripts.Game.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBoxCreater : SceneEntityCreater
{
    public GameObject followNodeBuilder;
    public override T Create<T>()
    {
        var entity = world.NewEntityNoRecord();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.FollowBox);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        behav.OnInitByCreate();
        behav.entity = entity;
        if (followNodeBuilder == null)
        {
            followNodeBuilder = new GameObject("FollowNodeBuilder");
        }
        behav.transform.SetParent(followNodeBuilder.transform);
        behav.gameObject.transform.localPosition = CameraUtils.Inst.GetCreatePosition();
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.FollowBox;
        gameComponent.type = ResType.Single;
        gameComponent.modelType = NodeModelType.FollowBox;
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

    public static void OnClone(SceneEntity sourceEntity, SceneEntity newEntity)
    {
        if (!sourceEntity.HasComponent<FollowableComponent>())
        {
            return;
        }
        if (sourceEntity.Get<FollowableComponent>().moveType == (int)MoveMode.Follow)
        {
            var gComp = newEntity.Get<GameObjectComponent>();
            var temp = gComp.bindGo.GetComponentInChildren<FollowModeBehaviour>();
            if (temp != null)
            {
                temp.gameObject.SetActive(false);
            }
            var nBehav = gComp.bindGo.GetComponent<NodeBaseBehaviour>();
            FollowModeManager.Inst.BuildFollowBox(nBehav);
        }
    }

    public static void SetData(FollowModeBehaviour behaviour, NodeBaseBehaviour target)
    {
        behaviour.transform.localPosition = target.transform.localPosition;
        behaviour.transform.localEulerAngles = target.transform.localEulerAngles;
        behaviour.SetFollowTarget(target);
        FollowModeManager.Inst.AddFolowBox(target.entity.Get<GameObjectComponent>().uid, behaviour);
    }
}

using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using Newtonsoft.Json;
using UnityEngine;

public class TransferCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.DowntownTransfer);
        if (!assetGo.TryGetComponent(out T behav)) {
            behav = assetGo.AddComponent<T>();
        }

        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.DowntownTransfer;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.DowntownTransfer;
        gameComponent.modelType = NodeModelType.DowntownTransfer;
        gameComponent.uid = UidManager.Inst.GetUid();
        return behav;
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Transform parent = null)
    {
        var transferBev = behaviour as TransferBehaviour;
        if (transferBev == null)
        {
            return;
        }
        var pos = DataUtils.DeSerializeVector3(data.p);
        transferBev.transform.position = pos;
        AddTransferAttribute(behaviour, data);
    }


    private static void AddTransferAttribute(NodeBaseBehaviour behaviour, NodeData data)
    {
        if (data != null)
        {
            var kv = data.attr.Find(x => x.k == (int)BehaviorKey.DowntownTransfer);
            if (kv != null)
            {
                var transData = JsonConvert.DeserializeObject<DowntownTransferData>(kv.v);
                DowntownTransferManager.Inst.AddTransferBev(behaviour, transData);
            }
        }
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public static void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        var tBehav = nBehaviour as TransferBehaviour;
        DowntownTransferManager.Inst.RecordTransBev(tBehav);
    }
}

/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/8/31 10:51:3
/// </summary>
using Assets.Scripts.Game.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class LadderCreater : SceneEntityCreater
{
    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int)GameResType.Ladder);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }
        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.Ladder;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Ladder;
        gameComponent.modelType = NodeModelType.Ladder;
        return behav;
    }
    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent = null)
    {
        LoggerUtils.Log("Ladder SetData:" + data);
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
        SetData(behaviour as LadderBehaviour, GameUtils.GetAttr<LadderData>((int)BehaviorKey.Ladder, data.attr));
    }

    public static void SetData(LadderBehaviour behaviour, LadderData data)
    {
        LadderComponent compt = behaviour.entity.Get<LadderComponent>();
        if (data != null)
        {
            
            compt.color = data.col;
            compt.mat = data.mat;
            compt.active = data.act;
            compt.tile = data.tile==null? new Vector2(1,1) : DataUtils.DeSerializeVector2(data.tile);
        }
        else
        {
          
            compt.color = DataUtils.ColorToString(AssetLibrary.Inst.colorLib.Get(0));
            compt.mat = 3;
            compt.active = 1;
            compt.tile =new Vector2(1, 1);
        }
       
        behaviour.SetColor(DataUtils.DeSerializeColor(compt.color));
        behaviour.SetMatetial(compt.mat);
        behaviour.SetHideModel(compt.active==1);
        behaviour.SetTiling(compt.tile);
        LadderManager.Inst.AddLadder(behaviour as LadderBehaviour);
    }
}


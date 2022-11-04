/// <summary>
/// Author:LiShuZhan
/// Description:新版3d文字创建
/// Date: 2022-5-27 17:44:22
/// </summary>
using Assets.Scripts.Game.Core;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HLODSystem;

/// <summary>
/// 修改此脚本时要考虑旧版3d文字DTextCreater
/// </summary>
public class NewDTextCreater : SceneEntityCreater
{
    public override GameObject Clone(GameObject target)
    {
        return null;
    }

    public override T Create<T>()
    {
        var entity = world.NewEntity();
        GameObject assetGo = new GameObject("NewDTextProp"); 

        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        behav.OnInitByCreate();
        behav.entity = entity;
        behav.transform.SetParent(SceneBuilder.Inst.StageParent);
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int)GameResType.NewDText;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Special;
        gameComponent.modelType = NodeModelType.NewDText;

        return behav;
    }

    public static void SetDefaultData(NewDTextBehaviour behaviour, Vector3 pos)
    {
        if (behaviour == null)
        {
            LoggerUtils.Log("NewDTextBehaviour is null");
            return;
        }
        var textComp = behaviour.entity.Get<NewDTextComponent>();

        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = Vector3.zero;
        behaviour.transform.localScale = Vector3.one;

        textComp.content = LocalizationConManager.Inst.GetLocalizedText("Enter text...");
        textComp.col = Color.white;
    }

    public static void SetData(NewDTextBehaviour behaviour, NodeData data, Vector3 pos, Transform parent = null)
    {
        if (behaviour == null)
        {
            LoggerUtils.Log("NewDTextBehaviour is null");
            return;
        }
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        var newParent = parent ?? SceneBuilder.Inst.StageParent;
        behaviour.transform.SetParent(newParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;
        behaviour.data = data;

        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);

        var mComp = behaviour.entity.Get<NewDTextComponent>();
        NewDTextData tData = GameUtils.GetAttr<NewDTextData>((int)BehaviorKey.NewDTextData, data.attr);
        mComp.content = tData.tex;
        mComp.col = DataUtils.DeSerializeColor(tData.textcol);
    }

    /// <summary>
    /// 因为插件在克隆时会丢失自己texmesh，所以调用显隐刷新组件状态
    /// </summary>
    /// <param name="oBehaviour"></param>
    /// <param name="nBehaviour"></param>
    public static void OnClone(NodeBaseBehaviour oBehaviour, NodeBaseBehaviour nBehaviour)
    {
        if (oBehaviour.gameObject.activeSelf)
        {
            oBehaviour.gameObject.SetActive(false);
            oBehaviour.gameObject.SetActive(true);
        }
    }
}

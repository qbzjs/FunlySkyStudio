/// <summary>
/// Author:YangJie
/// Description:
/// Date: 2022/5/16 18:12:29
/// </summary>
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using HLODSystem;
using Newtonsoft.Json;
using UnityEngine;

public class BaseModelCreater : SceneEntityCreater
{
    public override T Create<T>() 
    {
        var entity = world.NewEntity();
        var assetGo = new GameObject("BaseNodeModel");
        if (!assetGo.TryGetComponent(out NodeBehaviour nodeBehaviour))
        {
            nodeBehaviour = assetGo.AddComponent<NodeBehaviour>();
        }
        nodeBehaviour.OnInitByCreate();
        nodeBehaviour.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Base;
        gameComponent.modelType = NodeModelType.BaseModel;
        return nodeBehaviour as T;
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Transform parent = null)
    {
        var nodeBehaviour = behaviour as NodeBehaviour;
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        Vector3 pos = DataUtils.DeSerializeVector3(data.p);
        sca = DataUtils.LimitVector3(sca);
        nodeBehaviour.transform.SetParent(parent ? parent : SceneBuilder.Inst.StageParent);
        nodeBehaviour.transform.localPosition = pos;
        nodeBehaviour.transform.localEulerAngles = rot;
        nodeBehaviour.transform.localScale = sca;
        nodeBehaviour.data = data;
        var gameComp = nodeBehaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);
        gameComp.resId = data.rid;
        gameComp.modId = data.id;
        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            nodeBehaviour.SetLODStatus(HLODState.High);
        }
        
        nodeBehaviour.OnInitByCreate();
        SetData(nodeBehaviour, GameUtils.GetAttr<ColorMatData>((int) BehaviorKey.ColorMaterial, data.attr));
    }
    
    public static void SetData(NodeBaseBehaviour nBehaviour, ColorMatData matData)
    {
        if (matData == null)
        {
            return;
        }
        var mComp = nBehaviour.entity.Get<MaterialComponent>();
        mComp.matId = matData.mat;
        mComp.tile = DataUtils.DeSerializeVector2(matData.tile);
        mComp.color = DataUtils.DeSerializeColor(matData.cols);
        string uurl = null;
        if (!string.IsNullOrEmpty(matData.umat) && GlobalFieldController.ugcMatData.TryGetValue(matData.umat, out var value))
        {
            uurl = value.uurl;
        }
        mComp.uurl = uurl;
        mComp.umat = matData.umat;
        SceneObjectController.SetUGCBaseModelAtr(nBehaviour, mComp.matId, mComp.color, mComp.uurl);
        SceneObjectController.InitBaseModelTile(nBehaviour, mComp.tile);
    }

    public override GameObject Clone(GameObject target)
    {
        return null;
    }


}

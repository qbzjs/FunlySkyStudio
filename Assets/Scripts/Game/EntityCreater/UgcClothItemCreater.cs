using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description:UGC衣服道具创建
/// Date: 2022-4-22 16:01:37
/// </summary>
public class UgcClothItemCreater : SceneEntityCreater
{
    public override T Create<T>()
    {
        var entity = world.NewEntity();
        var assetGo = ModelCachePool.Inst.Get((int) GameResType.UgcCloth);
        if (!assetGo.TryGetComponent(out T behav))
        {
            behav = assetGo.AddComponent<T>();
        }

        behav.OnInitByCreate();
        behav.entity = entity;
        var gameComponent = entity.Get<GameObjectComponent>();
        gameComponent.bindGo = assetGo;
        gameComponent.modId = (int) GameResType.UgcCloth;
        gameComponent.type = ResType.Single;
        gameComponent.handleType = NodeHandleType.Special; //todo:ugc衣服handleType待定
        gameComponent.modelType = NodeModelType.UgcCloth;
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
        var sBehav = nBehaviour as UgcClothItemBehaviour;
        UgcClothItemManager.Inst.AddUgcClothItem(sBehav);
    }

    public static void SetData(NodeBaseBehaviour behaviour, NodeData data, Vector3 pos, Transform parent = null)
    {
        var rot = DataUtils.DeSerializeVector3(data.r);
        var sca = DataUtils.DeSerializeVector3(data.s);
        sca = DataUtils.LimitVector3(sca);
        var newParent = parent ? parent : SceneBuilder.Inst.StageParent;
        behaviour.transform.SetParent(newParent);
        behaviour.transform.localPosition = pos;
        behaviour.transform.localEulerAngles = rot;
        behaviour.transform.localScale = sca;
        
        var gameComp = behaviour.entity.Get<GameObjectComponent>();
        gameComp.uid = UidManager.Inst.GetUid(data);

        SetCanTransData(behaviour as UgcClothItemBehaviour, GameUtils.GetAttr<UGCPropData>((int) BehaviorKey.UGCProp, data.attr));
        SetData(behaviour as UgcClothItemBehaviour, GameUtils.GetAttr<UGCClothItemData>((int) BehaviorKey.UGCClothItem, data.attr));
    }

    public static void SetCanTransData(UgcClothItemBehaviour itemBehaviour, UGCPropData data)
    {
        var comp = itemBehaviour.entity.Get<UGCPropComponent>();
        comp.isTradable = data.isTradable;
        itemBehaviour.SetCanBuyInMap();
    }

    public static void SetData(UgcClothItemBehaviour itemBehaviour, UGCClothItemData data)
    {
        var mComp = itemBehaviour.entity.Get<UGCClothItemComponent>();
        mComp.templateId = data.tId;
        mComp.clothCover = data.cCover;
        mComp.clothMapId = data.cMapId;
        mComp.clothesUrl = data.cUrl;
        mComp.clothesJson = data.cJson;
        mComp.isDc = data.isDc;
        mComp.dcId = data.dCId;
        mComp.walletAddress = data.walAdd;
        mComp.budActId = data.actId;
        mComp.dataSubType = data.dataSubType;
        var cdata = new ClothStyleData()
        {
            templateId = mComp.templateId,
            clothesUrl = mComp.clothesUrl,
            clothMapId = mComp.clothMapId,
            clothesJson = mComp.clothesJson,
            classifyType = data.classifyType,
            pgcId = data.pgcId,
            dataSubType = mComp.dataSubType,
        };
        
        if (cdata.pgcId >= 100000)
        {
            itemBehaviour.loadAB(cdata);
        }else
        {
            itemBehaviour.LoadUgcCloth(cdata);
        }
        mComp.classifyType = data.classifyType;
        mComp.pgcId = data.pgcId;
        
        UgcClothItemManager.Inst.AddUgcClothItem(itemBehaviour);
    }
}
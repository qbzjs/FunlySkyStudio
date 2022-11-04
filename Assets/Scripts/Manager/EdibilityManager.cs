// EdibilityManager.cs
// Created by xiaojl Jul/22/2022
// 可食用属性管理器

using System.Collections.Generic;
using HLODSystem;
using Newtonsoft.Json;
using UnityEngine;

public struct FoodData
{
    public SceneEntity entity;
    public Transform parentNode;
    public Vector3 oriPos;
    public Vector3 oriRot;
    public Vector3 oriScale;
}

public class EdibilityManager : ManagerInstance<EdibilityManager>, IManager, IPVPManager, IUGCManager
{
    public Dictionary<int, FoodData> FoodDict = new Dictionary<int, FoodData>();
    public List<BudTimer> TimerList = new List<BudTimer>();

    private Dictionary<int, List<int>> FoodPropLayerDict = new Dictionary<int, List<int>>();
    private bool isInit = false; 

    // 判断道具是否具有食用属性
    public bool CheckEdibility(SceneEntity entity)
    {
        //判断是否包含特殊属性
        if (entity.HasComponent<ShootWeaponComponent>()
            || entity.HasComponent<AttackWeaponComponent>()
            || entity.HasComponent<FireworkComponent>()
            || entity.HasComponent<FreezePropsComponent>()
            || entity.HasComponent<ParachuteComponent>()
            || entity.HasComponent<FishingRodComponent>())
        {
            return false;
        }
        //判断是否包含特殊道具
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        var nodeBehvs = bindGo.GetComponentsInChildren<NodeBaseBehaviour>();
        for (var i = 0; i < nodeBehvs.Length; i++)
        {
            NodeModelType modeType = nodeBehvs[i].entity.Get<GameObjectComponent>().modelType;
            ResType resType = nodeBehvs[i].entity.Get<GameObjectComponent>().type;
            if (modeType != NodeModelType.BaseModel
                && modeType != NodeModelType.DText
                && modeType != NodeModelType.NewDText
                && resType != ResType.UGC
                && resType != ResType.PGC
                && resType != ResType.CommonCombine
                && modeType != NodeModelType.PGCPlant
                )
            {
                return false;
            }
        }
        return true;
    }

    // 是否具有可食用属性
    public bool HasEdibilityProp(SceneEntity entity)
    {
        return entity.HasComponent<EdibilityComponent>();
    }

    // 添加可食用属性
    public void AddEdibilityProp(SceneEntity entity)
    {
        HandleHLOD();
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        if (!FoodDict.ContainsKey(uid))
        {
            entity.Get<EdibilityComponent>();
            FoodData foodData = GetFoodData(entity);
            FoodDict.Add(uid, foodData);
            SetFoodLayer(true, entity);
        }
    }

    // 移除可食用属性
    public void RemoveEdibilityProp(SceneEntity entity)
    {
        // 如果存在可食用属性组件，移除
        if (HasEdibilityProp(entity))
        {
            entity.Remove<EdibilityComponent>();
            var gComp = entity.Get<GameObjectComponent>();
            var uid = gComp.uid;
            if (FoodDict.ContainsKey(uid))
            {
                FoodDict.Remove(uid);
            }
            if (FoodPropLayerDict.ContainsKey(uid))
            {
                SetFoodLayer(false, entity);
            }
        }
    }

    // 获取食用模式
    public EdibilityMode GetEdibilityMode(SceneEntity entity)
    {
        // 如果没有可食用属性组件，返回None
        if (!HasEdibilityProp(entity))
        {
            return EdibilityMode.None;
        }

        // 获取可食用属性组件，返回食用模式
        var com = entity.Get<EdibilityComponent>();
        return com.Mode;
    }

    // 设置食用模式
    public void SetEdibilityMode(SceneEntity entity, EdibilityMode mode)
    {
        // 如果没有可食用属性组件，返回
        if (! HasEdibilityProp(entity))
        {
            return;
        }

        // 获取可食用属性组件，并设置食用模式
        var com = entity.Get<EdibilityComponent>();
        com.Mode = mode;
    }

    #region 可食用道具的状态管理 - Author:Mingo
    public void OnChangeMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Edit:
                if (PlayerEatOrDrinkControl.Inst)
                {
                    PlayerEatOrDrinkControl.Inst.DropFood();
                }
                ResetFoodPropState();
                foreach(var timer in TimerList)
                {
                    if(timer != null)
                    {
                        TimerManager.Inst.Stop(timer);
                    }
                }
                break;
            case GameMode.Play:
            case GameMode.Guest:
                RefreshFoodData();
                break;
        }
    }

    /// <summary>
    /// 重置道具状态
    /// </summary>
    private void ResetFoodPropState()
    {
        foreach (var foodData in FoodDict.Values)
        {
            var entity = foodData.entity;
            var gCom = entity.Get<GameObjectComponent>();
            var foodComp = entity.Get<EdibilityComponent>();
            var bindGo = gCom.bindGo;
            var trans = bindGo.transform;
            foodComp.eatState = EateState.Free;
            if(GlobalFieldController.CurGameMode == GameMode.Edit)
            {
                trans.SetParent(SceneBuilder.Inst.StageParent);
                trans.localPosition = foodData.oriPos;
            }
            else
            {
                trans.SetParent(foodData.parentNode);
                if (foodData.parentNode.name == "moveNode")
                {
                    trans.localPosition = Vector3.zero;
                }
                else
                {
                    trans.localPosition = foodData.oriPos;
                }
            }
            trans.localEulerAngles = foodData.oriRot;
            trans.localScale = foodData.oriScale;
            bindGo.SetActive(true);
        }
    }

    /// <summary>
    /// 更新可食用道具的信息
    /// </summary>
    public void RefreshFoodData()
    {
        List<SceneEntity> tempList = new List<SceneEntity>();
        foreach (var foodData in FoodDict.Values)
        {
            tempList.Add(foodData.entity);
        }
        for (int i = 0; i < tempList.Count; i++)
        {
            var entity = tempList[i];
            var gCom = entity.Get<GameObjectComponent>();
            var uid = gCom.uid;
            var bindGo = gCom.bindGo;
            var trans = bindGo.transform;
            if (FoodDict.ContainsKey(uid))
            {
                var foodData = GetFoodData(entity);
                FoodDict[uid] = foodData;
            }
        }
    }

    public void GetParentData()
    {
        List<SceneEntity> tempList = new List<SceneEntity>();
        foreach (var foodData in FoodDict.Values)
        {
            tempList.Add(foodData.entity);
        }
        for (int i = 0; i < tempList.Count; i++)
        {
            var entity = tempList[i];
            var gCom = entity.Get<GameObjectComponent>();
            var uid = gCom.uid;
            var bindGo = gCom.bindGo;
            var trans = bindGo.transform;
            if (FoodDict.ContainsKey(uid))
            {
                var foodData = FoodDict[uid];
                foodData.parentNode = trans.parent;
                FoodDict[uid] = foodData;
            }
        }
    }

    /// <summary>
    /// 获取可食用道具的数据，并且存储下来
    /// </summary>
    private FoodData GetFoodData(SceneEntity entity)
    {
        var gComp = entity.Get<GameObjectComponent>();
        var bindGo = gComp.bindGo;
        var trans = bindGo.transform;
        FoodData foodData = new FoodData();
        foodData.entity = entity;
        foodData.oriPos = trans.localPosition;
        foodData.oriRot = trans.localEulerAngles;
        foodData.oriScale = trans.localScale;
        foodData.parentNode = trans.parent;
        return foodData;
    }

    public NodeBaseBehaviour GetNodeBaseBevByUid(int uid)
    {
        if (FoodDict.ContainsKey(uid))
        {
            var entity = FoodDict[uid].entity;
            var gComp = entity.Get<GameObjectComponent>();
            var bindGo = gComp.bindGo;
            var baseBev = bindGo.GetComponent<NodeBaseBehaviour>();
            return baseBev;
        }
        return null;
    }

    public void OnHandleClone(SceneEntity oEntity, SceneEntity nEntity)
    {
        if (oEntity.HasComponent<EdibilityComponent>())
         {
            var nGComp = nEntity.Get<GameObjectComponent>();
            var nUid = nGComp.uid;
            var oFoodComp = oEntity.Get<EdibilityComponent>();
            var nFoodComp = nEntity.Get<EdibilityComponent>();
            var Mode = oFoodComp.Mode;
            nFoodComp.Mode = Mode;
            if (!FoodDict.ContainsKey(nUid))
            {
                FoodData foodData = GetFoodData(nEntity);
                FoodDict.Add(nUid, foodData);
            }
         }
    }

    public void OnCombineNode(SceneEntity entity)
    {
        if (entity != null)
        {
            RemoveEdibilityProp(entity);
        }
    }

    public bool CheckCanBeControl(int uid) {
        if (FoodDict.ContainsKey(uid))
        {
            var entity = FoodDict[uid].entity;
            var foodComp = entity.Get<EdibilityComponent>();
            if (foodComp.eatState == EateState.HasEated)
            {
                return false;
            }
        }
        return true;
    }

    private void SetFoodLayer(bool canEat, SceneEntity entity)
    {
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        var binGo = gComp.bindGo;
        
        if (canEat)
        {
            if (!FoodPropLayerDict.ContainsKey(uid))
            {
                var childColliders = binGo.GetComponentsInChildren<Collider>();
                List<int> layerList = new List<int>();
                for (int i = 0; i < childColliders.Length; i++)
                {
                    layerList.Add(childColliders[i].gameObject.layer);
                    childColliders[i].gameObject.layer = LayerMask.NameToLayer("Touch");
                }
                FoodPropLayerDict.Add(uid, layerList);
            }
        }
        else
        {
            if (FoodPropLayerDict.ContainsKey(uid))
            {
                var childColliders = binGo.GetComponentsInChildren<Collider>();
                List<int> layerList = FoodPropLayerDict[uid];
                if(layerList != null && layerList.Count > 0)
                {
                    for (int i = 0; i < childColliders.Length; i++)
                    {
                        childColliders[i].gameObject.layer = layerList[i];
                    }
                    FoodPropLayerDict.Remove(uid);
                }
            }
        }
    }


    #endregion

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        var entity = behaviour.entity;
        if (entity != null)
        {
            var gComp = entity.Get<GameObjectComponent>();
            var uid = gComp.uid;
            if (FoodDict.ContainsKey(uid))
            {
                FoodDict.Remove(uid);
            }
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour.entity.HasComponent<EdibilityComponent>())
        {
            AddEdibilityProp(behaviour.entity);
        }
    }

    private void HandleHLOD()
    {
        if (!isInit)
        {
            HLOD.Inst.OnHLODBehaviourStatusChange = SetHLODCanTouchLayer;
            isInit = true;
        }
    }

    private void SetHLODCanTouchLayer(NodeBaseBehaviour baseBev)
    {
        SetBevFoodLayer(baseBev);
    }

    public void Clear()
    {
        // TODO
        FoodDict.Clear();
        FoodPropLayerDict.Clear();
        TimerList.Clear();
    }

    public void OnReset()
    {
        OnChangeMode(GameMode.Edit);
    }

    public void OnUGCChangeStatus(UGCCombBehaviour ugcCombBehaviour)
    {
        SetBevFoodLayer(ugcCombBehaviour);
    }

    private void SetBevFoodLayer(NodeBaseBehaviour baseBev)
    {
        var entity = baseBev.entity;
        if (entity.HasComponent<EdibilityComponent>())
        {
            var childColliders = baseBev.GetComponentsInChildren<Collider>();
            if (childColliders != null)
            {
                for (int i = 0; i < childColliders.Length; i++)
                {
                    childColliders[i].gameObject.layer = LayerMask.NameToLayer("Touch");
                }
            }
        }
    }
}

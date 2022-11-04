using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Author:Shaocheng
/// Description:场景构建帮助类，UGC克隆使用
/// Date: 2022-3-30 19:43:08
/// </summary>
public class SceneBuilderUtils : CInstance<SceneBuilderUtils>
{
    // key - data.rid
    public Dictionary<string, NodeBaseBehaviour> ugcBehavPool = new Dictionary<string, NodeBaseBehaviour>();

    #region UGC
    public void AddToUgcBehavPool(NodeBaseBehaviour behaviour, NodeData data)
    {
        if (data == null || behaviour == null) return;
        if (!string.IsNullOrEmpty(data.rid) && !ugcBehavPool.ContainsKey(data.rid) && behaviour.transform.childCount > 0)
        {
            ugcBehavPool.Add(data.rid, behaviour);
        }
    }

    public NodeBaseBehaviour CloneUgcNode(ECSWorld ecsWorld, NodeData data, Transform parent, Vector3 pos)
    {
        NodeBaseBehaviour behaviour;

        var ugcBevCache = ugcBehavPool[data.rid];
        var cNodeOld = ugcBevCache.gameObject;

        var par = parent == null ? SceneBuilder.Inst.StageParent : parent;
        var cNodeNew = GameObject.Instantiate(cNodeOld, par, true);
        cNodeNew.transform.localPosition = pos;

        CloneMatAfterClone(ecsWorld, cNodeOld, cNodeNew);

        behaviour = cNodeNew.GetComponent<UGCCombBehaviour>();
        
        var entity = ecsWorld.NewEntity();
        behaviour.entity = entity;
        behaviour.OnInitByCreate();
        var gameComp = entity.Get<GameObjectComponent>();
        gameComp.bindGo = cNodeNew;
        gameComp.modId = (int) GameResType.UGCComb;
        gameComp.type = ResType.UGC;
        gameComp.modelType = NodeModelType.CommonCombine;
        gameComp.handleType = NodeHandleType.SpecialCombine;
        
        return behaviour;
    }
    #endregion

    public void CloneMatAfterClone(ECSWorld ecsWorld, GameObject cNodeOld, GameObject cNodeNew)
    {
        var oComps = cNodeOld.GetComponentsInChildren<NodeBaseBehaviour>(true);
        var newComps = cNodeNew.GetComponentsInChildren<NodeBaseBehaviour>(true);
        var matProp = new MaterialPropertyBlock();

        //clone sub node
        for (var i = 0; i < newComps.Length; i++)
        {
            //entity in ugcBehavior
            if (newComps[i].gameObject == cNodeNew)
            {
                continue;
            }
            newComps[i].entity = ecsWorld.CloneEntity(oComps[i].entity);
            newComps[i].entity.Get<GameObjectComponent>().bindGo = newComps[i].gameObject;
            newComps[i].OnInitByCreate();
            
            //material propertyblock
            var oldRenders = oComps[i].GetComponentsInChildren<Renderer>();
            var newRenders = newComps[i].GetComponentsInChildren<Renderer>();
            
            for (int j = 0; j < newRenders.Length; j++)
            {
                var oldRender = oldRenders[j];
                var newRender = newRenders[j];
                if (oldRender != null && newRender != null)
                {
                    oldRender.GetPropertyBlock(matProp);
                    newRender.SetPropertyBlock(matProp);
                } 
            }

            //3dText位置clone后会错误
            if (newComps[i] is DTextBehaviour || newComps[i] is NewDTextBehaviour)
            {
                var rectTransNew = newComps[i].gameObject.GetComponent<RectTransform>();
                var rectTransOld = oComps[i].gameObject.GetComponent<RectTransform>();
                if (rectTransOld != null && rectTransNew != null)
                {
                    rectTransNew.localPosition = rectTransOld.localPosition;
                }
            }
        }
    }

    public void ClearAll()
    {
        if (ugcBehavPool != null)
        {
            ugcBehavPool.Clear();
        }
    }
    
    public int GetAllNodeCount()
    {
        // 场景中所有节点
        var childCount = SceneBuilder.Inst.allControllerBehaviours.Count;
        foreach (var tmpUgcBehaviour in SceneSystem.Inst.FilterNodeBehaviours<UGCCombBehaviour>(SceneBuilder.Inst.allControllerBehaviours))
        {
            // 由离线AB替换的UGC 需要单独处理
            if (tmpUgcBehaviour.GetComponentsInChildren<NodeBaseBehaviour>().Length <= 1)
            {
                var objComponent = tmpUgcBehaviour.entity.Get<GameObjectComponent>();
                if (!string.IsNullOrEmpty(objComponent.resId) && GlobalFieldController.ugcNodeData.ContainsKey(objComponent.resId))
                {
                    childCount += GlobalFieldController.ugcNodeData[objComponent.resId].Count;
                }
            }
        }
        return childCount;
    }
    
}
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

/// <summary>
/// Author:YangJie
/// Description:
/// Date: 2022/6/8 18:06:00
/// </summary>
public class UGCModelCachePool: InstMonoBehaviour<UGCModelCachePool>
{
     
    private Dictionary<string, Dictionary<UGCModelType, Queue<GameObject>>> ugcObjPools = new Dictionary<string, Dictionary<UGCModelType, Queue<GameObject>>>();
    private Dictionary<string, Dictionary<UGCModelType, GameObject>> ugcOriginObjs = new Dictionary<string, Dictionary<UGCModelType, GameObject>>();
    private int maxCacheCount = 50;
    public Dictionary<string, Dictionary<UGCModelType, GameObject>> UGCOriginObjs => ugcOriginObjs;
    private List<GameObject> prefabUGCObjs = new List<GameObject>();
    public int GetPoolCount()
    {
        var count = 0;
        foreach (var tmpOriginObj in ugcOriginObjs)
        {
            foreach (var tmpKeyValue in tmpOriginObj.Value)
            {
                if (tmpKeyValue.Key != UGCModelType.Json)
                {
                    count++;
                    break;
                }
            }
        }
        return count;
    }

    public bool IsContains(string rid, UGCModelType modelType)
    {
        return !string.IsNullOrEmpty(rid) && ugcOriginObjs.ContainsKey(rid)  && ugcOriginObjs[rid].ContainsKey(modelType) && ugcOriginObjs[rid][modelType] != null;
    }

    public GameObject Get(string rid, UGCModelType modelType, Transform parent = null)
    {
        if (string.IsNullOrEmpty(rid) )
        {
            return null;
        }
        GameObject tmpObj = null;
        if (ugcObjPools.ContainsKey(rid) && ugcObjPools[rid].ContainsKey(modelType))
        {
            GlobalFieldController.offlineRenderDataDic.TryGetValue(rid, out OfflineRenderData renderData);
            if (renderData!= null && renderData.IsSameModel() && modelType != UGCModelType.Json)
            {
                modelType = UGCModelType.High;
            }
            while (ugcObjPools[rid][modelType].Count > 0 && tmpObj == null)
            {
                tmpObj = ugcObjPools[rid][modelType].Dequeue();
                if (tmpObj == null)
                {
                    LoggerUtils.LogError("pool null:" + rid + ",modelType:" + modelType);
                }
            }
            if (tmpObj != null)
            {
                tmpObj.transform.SetParent(parent);
            }
        }

        if (tmpObj == null && ugcOriginObjs.ContainsKey(rid) && ugcOriginObjs[rid].ContainsKey(modelType))
        {
            var ugcOriginObj = ugcOriginObjs[rid][modelType];
            if(ugcOriginObj != null)
            {
                tmpObj = CloneTarget(ugcOriginObj, parent);
            }
        }
        
        if (tmpObj != null)
        {
            tmpObj.name = "NewUGCModel-" + rid + "-" + modelType;
            ResetObj(tmpObj);
            if (parent != null)
            {
                tmpObj.transform.localPosition = Vector3.zero;
                tmpObj.transform.localEulerAngles = Vector3.zero;
                tmpObj.transform.localScale = Vector3.one;
            }
        }
        return tmpObj;
    }

    private void CloneMatProp(GameObject org,GameObject clone)
    {
        var oRenders = org.GetComponentsInChildren<Renderer>(true);
        var cRenders = clone.GetComponentsInChildren<Renderer>(true);
        if (oRenders.Length != cRenders.Length)
        {
            LoggerUtils.Log("Clone Fail");
            return;
        }
        var matProp = new MaterialPropertyBlock();
        for (int i = 0; i < oRenders.Length; i++)
        {
            oRenders[i].GetPropertyBlock(matProp);
            cRenders[i].SetPropertyBlock(matProp);
        }
    }
    
    
    private GameObject CloneTarget(GameObject target, Transform parent)
    {
        var newTarget = GameObject.Instantiate(target, target.transform.parent);
        newTarget.name = target.name;
        var oComps = target.GetComponentsInChildren<NodeBaseBehaviour>(true);
        var newComps = newTarget.GetComponentsInChildren<NodeBaseBehaviour>(true);
        for (var i = 0; i < newComps.Length; i++)
        {
            newComps[i].entity = SceneBuilder.Inst.BindCreater<UGCCombCreater>().world.CloneEntity(oComps[i].entity);
            newComps[i].entity.Get<GameObjectComponent>().bindGo = newComps[i].gameObject;
            newComps[i].OnInitByCreate();
#if UNITY_EDITOR
            var gCmp = newComps[i].entity.Get<GameObjectComponent>();
            gCmp.bindGo.name = $"{target.name}_clone_{gCmp.uid}";
#endif
        }
        CloneMatProp(target,newTarget);
        return newTarget;
    }

    private void ResetObj(GameObject tmpObj)
    {
        tmpObj.SetActive(true);
        var tmpRenders = tmpObj.GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in tmpRenders)
        {
            meshRenderer.enabled = true;
            meshRenderer.forceRenderingOff = false;
        }
        NodeBaseBehaviour[] nbehaviours = tmpObj.GetComponentsInChildren<NodeBaseBehaviour>(true);
        foreach(var behaviour in nbehaviours){
            behaviour.enabled = true;
        }
    }

    public void Release(string rid,  UGCModelType modelType, GameObject tmpObj)
    {
        if (string.IsNullOrEmpty(rid) || tmpObj == null)
        {
            return;
        }
        if (!ugcObjPools.ContainsKey(rid))
        {
            ugcObjPools.Add(rid, new Dictionary<UGCModelType, Queue<GameObject>>());
        }
        GlobalFieldController.offlineRenderDataDic.TryGetValue(rid, out var renderData);
        if (renderData!= null && renderData.IsSameModel() && modelType != UGCModelType.Json)
        {
            modelType = UGCModelType.High;
        }
        if (!ugcObjPools[rid].ContainsKey(modelType))
        {
            ugcObjPools[rid].Add(modelType, new Queue<GameObject>());
        }

        if (ugcObjPools[rid][modelType].Count >= maxCacheCount)
        {
            Destroy(tmpObj);
        }
        else
        {
            if (ugcOriginObjs.ContainsKey(rid) && ugcOriginObjs[rid].ContainsKey(modelType) && ugcOriginObjs[rid][modelType] == null)
            {
                SetOriginObj(rid, modelType, tmpObj);
            }
            tmpObj.transform.SetParent(transform);
            tmpObj.gameObject.SetActive(false);
            ugcObjPools[rid][modelType].Enqueue(tmpObj);
        }
    }

    public GameObject SetOriginObj(string rid, List<UGCModelType> modelTypes, GameObject[] originObjs)
    {
        var tmpOriginRoot = new GameObject("Origin-UGCRoot-" + rid + "-" + "modelTypes");
        tmpOriginRoot.transform.SetParent(transform);
        tmpOriginRoot.SetActive(false);
        foreach (var tmpObj in originObjs)
        {
            Instantiate(tmpObj, tmpOriginRoot.transform);
            prefabUGCObjs.Add(tmpObj);
        }
        if (GlobalFieldController.ugcNodeData.TryGetValue(rid, out var prims))
        {
            GlobalFieldController.offlineRenderDataDic.TryGetValue(rid,out var offlineRenderData);
            var specials = prims.FindAll(tmp =>
            {
                var configData = GameManager.Inst.priConfigData[tmp.id];
                if(offlineRenderData !=null && offlineRenderData.version == OfflineRenderData.V41)      //V41版本不还原旧版3d文字
                    return UGCBehaviorManager.Inst.specileNodeModTypesDText.Contains(configData.modType);
                else
                    return UGCBehaviorManager.Inst.specileNodeModTypes.Contains(configData.modType);
            });
            SceneBuilder.Inst.BuildAllNodes(specials, tmpOriginRoot.transform);
            var nodeBehaviours = tmpOriginRoot.GetComponentsInChildren<NodeBaseBehaviour>(true);
            foreach (var tmpNodeBehaviour in nodeBehaviours)
            {
                SceneBuilder.Inst.RemoveNodeBehaviour(tmpNodeBehaviour);
                DestroyImmediate(tmpNodeBehaviour);
            }
        }
        foreach (var modelType in modelTypes)
        {
            SetOriginObj(rid, modelType, tmpOriginRoot);
        }
        Release(rid, modelTypes[0], tmpOriginRoot);
        return tmpOriginRoot;
    }

    public GameObject SetOriginObj(string rid,  UGCModelType modelType, GameObject[] originObjs)
    {
        return SetOriginObj(rid, new List<UGCModelType>() { modelType}, originObjs);
    }

    public GameObject SetOriginObj(string rid, UGCModelType modelType, GameObject originObj)
    {
        if (string.IsNullOrEmpty(rid) || originObj == null)
        {
            return null;
        }
        if (!ugcOriginObjs.ContainsKey(rid))
        {
            ugcOriginObjs.Add(rid, new Dictionary<UGCModelType, GameObject>());
        }
        
        if (!ugcOriginObjs[rid].ContainsKey(modelType))
        {
            ugcOriginObjs[rid].Add(modelType, originObj);
        }
        else
        {
            if (ugcOriginObjs[rid][modelType] != null && ugcOriginObjs[rid][modelType].transform.parent == transform)
            {
                SceneBuilder.Inst.DestroyEntity(ugcOriginObjs[rid][modelType]);
            }
            ugcOriginObjs[rid][modelType] = originObj;
        }
        return originObj;
    }

    public void DelOriginObj(string rid, UGCModelType modelType, GameObject originObj)
    {
        if (string.IsNullOrEmpty(rid) || originObj == null)
        {
            return;
        }

        if (ugcOriginObjs.ContainsKey(rid) && ugcOriginObjs[rid].ContainsKey(modelType))
        {
            if (ugcOriginObjs[rid][modelType] == originObj)
            {
                ugcOriginObjs[rid][modelType] = null;
            }
        }
        
    }

    private void OnDestroy()
    {
        ugcObjPools.Clear();
        ugcOriginObjs.Clear();
        foreach (var tmpPrefabUGCObj in prefabUGCObjs)
        {
            if (tmpPrefabUGCObj != null)
            {
                DestroyImmediate(tmpPrefabUGCObj, true);
            }
        }
        prefabUGCObjs.Clear();
        inst = null;
    }
}

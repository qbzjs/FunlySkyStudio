using System.Collections.Specialized;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Author:JayWill
/// Description:Undo/Redo 系统所使用的二级缓存池，undo/redo 15步骤中
/// 删除和创建会经过SecondCachePool，只到超出undo管理范畴会进入ModelCachePool
/// </summary>

public class CacheItem
{
    public GameObject target;
    public Transform originParent;
    public bool isDestroy = false;
    public int uid = 0;
}
public class SecondCachePool: CInstance<SecondCachePool>
{
    private OrderedDictionary _itemOrderDict = new OrderedDictionary();
    private Transform cacheNode;
    private int maxCount = 1000;
    public void DestroyEntity(GameObject gameObject)
    {
        if(gameObject == null)
        {
            LoggerUtils.Log("SecondCachePool DestroyEntity gameObject is null !!!");
            return;
        }
        AddItem(gameObject);
        var baseBehaivours = gameObject.GetComponentsInChildren<NodeBaseBehaviour>(true);
        for (int i = 0; i < baseBehaivours.Length; i++)
        {
            var nBehav = baseBehaivours[i];
            SceneBuilder.Inst.RemoveNodeBehaviour(nBehav);
        }
    }

    public void RevertEntity(GameObject gameObject)
    {
        RevertItem(gameObject);
        var baseBehaivours = gameObject.GetComponentsInChildren<NodeBaseBehaviour>(true);
        for (int i = 0; i < baseBehaivours.Length; i++)
        {
            var nBehav = baseBehaivours[i];
            SceneBuilder.Inst.RevertNodeBehaviour(nBehav);
        }
    }

    public void AddItem(GameObject go)
    {  
        LoggerUtils.Log("SecondCachePool AddItem:"+go?.transform.name);
        // if(!_itemDict.ContainsKey(go)){
        //     CacheItem item = new CacheItem();
        //     item.target = go;
        //     item.originParent = go.transform.parent;
        //     item.isDestroy = true;
        //     _itemDict.Add(go,item);
        //     go.transform.parent = GetCacheNode();
        //     SetItemEnAble(go,false);
        // }


        if(!_itemOrderDict.Contains(go)){
            if(_itemOrderDict.Count >= maxCount)
            {

                var removeItem = _itemOrderDict[0] as CacheItem;
                LoggerUtils.Log("CachePool超过最大限制个数111:"+ _itemOrderDict.Count);
                RemoveItemAt(0);
                LoggerUtils.Log("CachePool超过最大限制个数222:"+ _itemOrderDict.Count);
                if(removeItem.target){
                    SceneBuilder.Inst.DestroyEntity(removeItem.target);
                    LoggerUtils.Log("CachePool超过最大限制个数 销毁:"+removeItem.target.GetHashCode());
                }
            }
            CacheItem item = new CacheItem();
            item.target = go;
            item.originParent = go.transform.parent;
            item.isDestroy = true;
            var nodeBehav = go.GetComponent<NodeBaseBehaviour>();
            if (nodeBehav != null && nodeBehav.entity != null)
            {
                item.uid = nodeBehav.entity.Get<GameObjectComponent>().uid;
                LoggerUtils.Log("删除道具的ID:"+item.uid);
            }
            _itemOrderDict.Add(go,item);
            go.transform.parent = GetCacheNode();
            SetItemEnAble(go,false);
        }
    }

    public void RevertItem(GameObject go)
    {
        LoggerUtils.Log("SecondCachePool RevertItem:"+go?.transform.name);
        // if(_itemDict.ContainsKey(go)){
        //     var item = _itemDict[go];
        //     item.isDestroy = false;
        //     _itemDict.Remove(go);
        //     go.transform.parent = item.originParent;
        //     SetItemEnAble(go,true);
        // }

        if(_itemOrderDict.Contains(go)){
            var item = _itemOrderDict[go] as CacheItem;
            LoggerUtils.Log("#####RevertItem item:"+item.uid);
            item.isDestroy = false;
            _itemOrderDict.Remove(go);
            go.transform.parent = item.originParent;
            SetItemEnAble(go,true);
        }
    }

    public bool IsContains(GameObject go)
    {
        // return _itemDict.ContainsKey(go);
        return _itemOrderDict.Contains(go);
    }

    public GameObject GetGameObjectByUid(int uid)
    {
        if(uid <= 0)
        {
            return null;
        }
        foreach(var item in _itemOrderDict.Values)
        {
            CacheItem cacheItem = item as CacheItem;
            if(cacheItem.uid == uid)
            {
                return cacheItem.target;
            }
        }

        return null;
    }

    public void RemoveItem(GameObject go)
    {  
        if(go == null)
        {
            return;
        }

        // LoggerUtils.Log("SecondCachePool RemoveItem:"+go.transform.name);

        if(_itemOrderDict.Contains(go)){
            _itemOrderDict.Remove(go);
            SetItemEnAble(go,true);
        }
    }

    public void RemoveItemAt(int index)
    {
        CacheItem item = _itemOrderDict[index] as CacheItem;
        if(item!= null && item.target != null){
            RemoveItem(item.target);
        }
    }

    public Transform GetCacheNode()
    {
        if(cacheNode == null){
            cacheNode = new GameObject("SecondCacheNode").transform;
            cacheNode.gameObject.SetActive(false);
        }
        return cacheNode;
    }

    public int GetCount()
    {
        // return _itemDict.Count;
        return _itemOrderDict.Count;
    }

    private void SetItemEnAble(GameObject go, bool able)
    {
        MeshRenderer[] meshRenders = go.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mesh in meshRenders)
        {
            mesh.enabled = able;
        }

        NodeBaseBehaviour[] nbehaviours = go.GetComponentsInChildren<NodeBaseBehaviour>(true);
        foreach(var behaviour in nbehaviours){
            behaviour.enabled = able;
        }
    }

    public void ClearPool()
    {
        if(_itemOrderDict == null || _itemOrderDict.Count == 0)
        {
            return;
        }

        List<GameObject> toDestroyList = new List<GameObject>();

        foreach(GameObject gameObject in _itemOrderDict.Keys)
        {
            if(gameObject!=null){
                SetItemEnAble(gameObject,true);
                toDestroyList.Add(gameObject);
            }  
        }

        //避免出现_itemOrderDict遍历中出现removeItem
        foreach(GameObject gameObject in toDestroyList)
        {
            if(gameObject!=null)
            {
                SceneBuilder.Inst.DestroyEntity(gameObject);
            }
        }
        
        toDestroyList.Clear();
        _itemOrderDict.Clear();
    }

    public override void Release()
    {
        base.Release();
        _itemOrderDict.Clear();
    }
}
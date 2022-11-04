using System.Collections.Generic;
using UnityEngine;

public class ParticleObjPool : CInstance<ParticleObjPool>
{
    class DelayRecycle
    {
        public PooledParticleObjScript mRecycleObj = null;
        public int mTimeMillSecondsLeft = 0;
        public OnDelayRecycleDelegate callback = null;
    }
    public delegate void OnDelayRecycleDelegate(PooledParticleObjScript recycleObj);
    private Dictionary<int, Queue<PooledParticleObjScript>> mPooledParticleObjs = new Dictionary<int, Queue<PooledParticleObjScript>>();
    private LinkedList<DelayRecycle> mDelayRecycle = new LinkedList<DelayRecycle>();
    private GameObject mPoolRoot;
    private bool mClearPooledObjects = false;
    private int mClearPooledObjectsExecuteFrame = 0;
    private int mFrameCounter;
    public void Init()
    {
        mPoolRoot = new GameObject("ParticleObjPool");
        mPoolRoot.transform.SetParent(null);
    }
    public PooledParticleObjScript GetGameObject(string prefabFullPath)
    {
        return GetGameObject(prefabFullPath, Vector3.zero, Quaternion.identity, false);
    }
    private PooledParticleObjScript GetGameObject(string prefabFullPath, Vector3 pos, Quaternion rot, bool useRotation)
    {
        int hashKey = GameUtils.JavaHashCodeIgnoreCaseEraseExt(prefabFullPath);
        Queue<PooledParticleObjScript> pooledGameObjectScriptQueue = null;
        if (!mPooledParticleObjs.TryGetValue(hashKey, out pooledGameObjectScriptQueue))
        {
            pooledGameObjectScriptQueue = new Queue<PooledParticleObjScript>();
            mPooledParticleObjs.Add(hashKey, pooledGameObjectScriptQueue);
        }
        PooledParticleObjScript pooledGameObjectScript = null;
        //尝试从缓存的队列中获取GameObject
        while (pooledGameObjectScriptQueue.Count > 0)
        {
            pooledGameObjectScript = pooledGameObjectScriptQueue.Dequeue();

            if (pooledGameObjectScript != null && pooledGameObjectScript.mGameObject != null)
            {
                pooledGameObjectScript.mTransform.SetParent(null, true);

                pooledGameObjectScript.mTransform.position = pos;
                pooledGameObjectScript.mTransform.rotation = rot;

                //还原default值
                pooledGameObjectScript.mTransform.localScale = pooledGameObjectScript.mDefaultScale;

                break;
            }
            else
            {
                pooledGameObjectScript = null;
            }
        }
        //缓存中不能取到GameObject，尝试创建
        if (pooledGameObjectScript == null)
        {
            string prefabKey = (prefabFullPath);
            pooledGameObjectScript = CreateGameObject(prefabFullPath, pos, rot, useRotation, prefabKey);
        }
        if (pooledGameObjectScript == null)
        {
            return null;
        }
        pooledGameObjectScript.OnGet();
        return pooledGameObjectScript;
    }
    private PooledParticleObjScript CreateGameObject(string prefabFullPath, Vector3 pos, Quaternion rot, bool useRotation, string prefabKey)
    {
        PooledParticleObjScript pooledGameObjectScript = null;
        GameObject prefab = ResManager.Inst.LoadRes<GameObject>(prefabFullPath);
        if (prefab == null)
        {
            LoggerUtils.LogError($"prefab is null {prefabFullPath}");
            return null;
        }
        GameObject pooledGameObject = null;
        if (useRotation)
        {
            pooledGameObject = GameObject.Instantiate(prefab, pos, rot) as GameObject;
        }
        else
        {
            pooledGameObject = GameObject.Instantiate(prefab) as GameObject;
            pooledGameObject.transform.position = pos;
        }
        pooledGameObjectScript = pooledGameObject.GetComponent<PooledParticleObjScript>();
        if (pooledGameObjectScript == null)
        {
            pooledGameObjectScript = pooledGameObject.AddComponent<PooledParticleObjScript>();
        }
        //初始化参数
        pooledGameObjectScript.Initialize(prefabKey);
        //OnCreate
        pooledGameObjectScript.OnCreate();
        return pooledGameObjectScript;
    }
    public void Update()
    {
        mFrameCounter++;
        UpdateDelayRecycle();
        if (mClearPooledObjects && mClearPooledObjectsExecuteFrame == mFrameCounter)
        {
            ExecuteClearPooledObjects();
            mClearPooledObjects = false;
        }
    }
    private void UpdateDelayRecycle()
    {
        int timeElapsed = (int)(1000 * Time.deltaTime);
        var delayNode = mDelayRecycle.First;
        while (delayNode != null)
        {
            var currentNode = delayNode;
            delayNode = currentNode.Next;
            if (null == currentNode.Value.mRecycleObj)
            {
                mDelayRecycle.Remove(currentNode);
                continue;
            }
            currentNode.Value.mTimeMillSecondsLeft -= timeElapsed;
            if (currentNode.Value.mTimeMillSecondsLeft <= 0)
            {
                if (currentNode.Value.callback != null)
                {
                    currentNode.Value.callback(currentNode.Value.mRecycleObj);
                }
                RecycleGameObject(currentNode.Value.mRecycleObj);
                mDelayRecycle.Remove(currentNode);
                continue;
            }
        }
    }
    //发起清空对象池接口
    public void ClearPooledObjects()
    {
        mClearPooledObjects = true;
        mClearPooledObjectsExecuteFrame = mFrameCounter + 1;
    }
    public void ExecuteClearPooledObjects()
    {
        var delayNode = mDelayRecycle.First;
        while (delayNode != null)
        {
            if (null != delayNode.Value.mRecycleObj)
            {
                RecycleGameObject(delayNode.Value.mRecycleObj);
            }
            delayNode = delayNode.Next;
        }
        mDelayRecycle.Clear();

        var iter = mPooledParticleObjs.GetEnumerator();

        while (iter.MoveNext())
        {
            Queue<PooledParticleObjScript> pooledGameObjectQueue = iter.Current.Value;

            while (pooledGameObjectQueue.Count > 0)
            {
                PooledParticleObjScript pooledGameObjectScript = pooledGameObjectQueue.Dequeue();

                if (pooledGameObjectScript != null && pooledGameObjectScript.gameObject != null)
                {
                    GameObject.Destroy(pooledGameObjectScript.gameObject);
                }
            }
        }
        mPooledParticleObjs.Clear();
    }
    public void RecycleGameObject(PooledParticleObjScript pooledGameObject)
    {
        RecycleGameObject(pooledGameObject, false);
    }
    public void RecycleGameObjectDelay(PooledParticleObjScript pooledGameObject, int delayMillSeconds, OnDelayRecycleDelegate callback = null)
    {
        DelayRecycle delay = new DelayRecycle();
        delay.mRecycleObj = pooledGameObject;
        delay.mTimeMillSecondsLeft = delayMillSeconds;
        delay.callback = callback;
        mDelayRecycle.AddLast(delay);
    }
    public void RecycleDelayGameObject(int goInstanceID)
    {
        var delayNode = mDelayRecycle.First;
        while (delayNode != null)
        {
            if (goInstanceID == delayNode.Value.mRecycleObj.gameObject.GetInstanceID())
            {
                delayNode.Value.callback(delayNode.Value.mRecycleObj);

                RecycleGameObject(delayNode.Value.mRecycleObj, false);

                mDelayRecycle.Remove(delayNode);
                break;
            }
            delayNode = delayNode.Next;
        }
    }
    private void RecycleGameObject(PooledParticleObjScript pooledGameObjectScript, bool setIsInit)
    {
        if (pooledGameObjectScript == null)
        {
            return;
        }
        //尝试回收
        int hashKey = GameUtils.JavaHashCodeIgnoreCase(pooledGameObjectScript.mPrefabKey);
        Queue<PooledParticleObjScript> pooledGameObjectScriptQueue = null;
        if (!pooledGameObjectScript.mIsObjDestory && mPooledParticleObjs.TryGetValue(hashKey, out pooledGameObjectScriptQueue))
        {
            pooledGameObjectScriptQueue.Enqueue(pooledGameObjectScript);
            pooledGameObjectScript.OnRecycle();
            pooledGameObjectScript.transform.SetParent(mPoolRoot.transform, true);
            pooledGameObjectScript.mIsInit = setIsInit;
            return;
        }

        RecycleGameObject(pooledGameObjectScript.gameObject);
    }
    private void RecycleGameObject(GameObject pooledGameObject)
    {
        GameObject.Destroy(pooledGameObject);
    }
}


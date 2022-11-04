using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedDot
{
    public class RedDotNodeFactoryBase
    {
        private int mNodeId;
        private int NodeId => mNodeId++;
        public Dictionary<int, Type> mTypes;
        public RedDotNodeFactoryBase()
        {
            mTypes = new Dictionary<int, Type>();
        }
        public void Init()
        {
            OnInit();
        }
        protected virtual void OnInit()
        {

        }
        public void Register<T>(int nodeType) where T : Node
        {
            if (mTypes.ContainsKey(nodeType))
            {
                UnityEngine.Debug.LogError($"type has registed  nodetype={nodeType}");
            }
            else
            {
                mTypes.Add(nodeType, typeof(T));
            }
        }

        public Node Create(int nodeType)
        {
            Node node = null;
            Type type;
            if (!mTypes.TryGetValue(nodeType, out type))
            {

                type = typeof(Node);
            }
            node = Activator.CreateInstance(type) as Node;
            node.mNodeId = NodeId;
            node.mNodeType = nodeType;
            node.Init();
            return node;
        }
        public void Destroy(Node node)
        {
            node.Clear();
        }
    }
    public class Node
    {
        public GameObject mGameObject;
        private CountChangedNotifyFunc mChangedObserveFunc;
        public List<Node> mChildrens;
        public Node mParent;
        private int mCount;
        public int mNodeId;
        public int mNodeType;
        public RedDotTree mTree;
        public object mData;
        public bool IsLeap => mChildrens.Count <= 0;
        public Node()
        {
            mChildrens = new List<Node>();
        }
        public void SetTree(RedDotTree tree)
        {
            mTree = tree;
        }
        public int Count => mCount;
        public void Init()
        {
            mGameObject = new GameObject($"type={mNodeType}   count={Count}");
            OnInit();
        }
        protected void OnInit()
        {
           
        }
        public int ChildCount => mChildrens.Count;
        public void AddChild(Node child)
        {
            child.SetParent(this);
            child.AddListener(ChildChangedValueCallBack);
            mChildrens.Add(child);
            OnAddedChild(child);
        }
        protected virtual void OnAddedChild(Node child)
        {

        }
        public void RemoveChild(Node child)
        {
            mChildrens.Remove(child);
            child.mParent = null;
        }
        public void SetParent(Node parent)
        {
            mParent = parent;
            mGameObject.transform.parent = parent.mGameObject.transform;
        }
        //只要子节点有修改，直接暴力遍历刷新
        internal void ChildChangedValueCallBack(int count)
        {
            int newValue = 0;
            for (int i = 0; i < mChildrens.Count; i++)
            {
                newValue += mChildrens[i].mCount;
            }
            mGameObject.name = $"type={mNodeType}   count={newValue}";
            InternalChangeValue(newValue);
            OnChildChangedValueCallBack();
        }
        protected virtual void OnChildChangedValueCallBack()
        {

        }
        public void ChangeCount(int newCnt)
        {
            if (!IsLeap)
            {
                return;
            }

            InternalChangeValue(newCnt);
        }
        private void InternalChangeValue(int newValue)
        {
            if (newValue == Count)
            {
                return;
            }

            mCount = newValue;
            mGameObject.name = $"type={mNodeType}   count={Count}";
            InvokeChangedFunc(newValue);
        }
        public void InvokeChangedFunc(int count)
        {
            if (mChangedObserveFunc != null)
            {
                mChangedObserveFunc.Invoke(count);
            }
        }
        public Node GetNode(int nodeType)
        {
            Node ret = default;
            if (IsEqual(nodeType))
            {
                return this;
            }
            for (int i = 0; i < mChildrens.Count; i++)
            {
                Node child = mChildrens[i];
                if (child.IsEqual(nodeType))
                {
                    return child;
                }
                else
                {
                    ret = child.GetNode(nodeType);
                    if (ret != null)
                    {
                        break;
                    }
                }
            }
            return ret;
        }
        public bool IsEqual(Node other)
        {
            return mNodeType == other.mNodeType;
        }
        public bool IsEqual(int other)
        {
            return mNodeType == other;
        }
        public void AddListener(CountChangedNotifyFunc listener)
        {
            mChangedObserveFunc += listener;
        }
        public void RemoveListener(CountChangedNotifyFunc listener)
        {
            mChangedObserveFunc -= listener;
        }
        public void AddToWaittingDestroy()
        {
            mTree.AddNodeToWaittingDestroy(this);
        }
        public void Clear()
        {
            mChangedObserveFunc = null;
            if (mParent != null)
            {
                mParent.RemoveChild(this);
            }
            for (int i = 0; i < mChildrens.Count; i++)
            {
                Node child = mChildrens[i];
                child.RemoveListener(ChildChangedValueCallBack);
            }
            mChildrens.Clear();
            mChildrens = null;
            if (mGameObject != null)
            {
                GameObject.Destroy(mGameObject);
            }
            
        }
    }
}





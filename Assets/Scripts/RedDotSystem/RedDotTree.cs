using System.Collections.Generic;
using UnityEngine;
namespace RedDot
{

    public delegate void CountChangedNotifyFunc(int count);
   
    public class RedDotTree
    {
        public RedDotSystem mSystem;
        public Dictionary<int, int> mHierEnumDict;
        public Dictionary<int, Node> mNodes;
        public List<Node> mWaittingDestroys;

        public Node mRoot;
        public GameObject mGameObject;
        public string mName= "RedDotTree";
        public RedDotTree(RedDotSystem system)
        {
            mSystem = system;
            mHierEnumDict = new Dictionary<int, int>();
            mNodes = new Dictionary<int, Node>();
            mWaittingDestroys = new List<Node>();
            mGameObject = new GameObject(mName);
            mGameObject.transform.parent = system.mGameObject.transform;
        }
        public void Init()
        {

        }
        public void Register(int child, int parent)
        {
            if (mHierEnumDict.ContainsKey(child))
            {
                LoggerUtils.LogError($"key is registed  childType is ={child}");
            }
            else
            {
                mHierEnumDict.Add(child, parent);
            }
        }
        public void Construct(int rooType)
        {
            CreateRootNode(rooType);
            foreach (KeyValuePair<int, int> item in mHierEnumDict)
            {
                int parentType = item.Value;
                int childType = item.Key;
                Node parentNode = mRoot.GetNode(parentType);
                Node childNode = InternalAddNode(childType);
                if (parentNode == null)
                {
                    UnityEngine.Debug.LogError($"parentType is null ={parentType}   childType={childType}");
                }
                else
                {
                    parentNode.AddChild(childNode);
                }
            }
        }
        private void CreateRootNode(int nodeType)
        {
            mRoot = InternalAddNode(nodeType);
            mRoot.mGameObject.transform.parent = mGameObject.transform;
        }
        public void Update()
        {
            for (int i = (mWaittingDestroys.Count-1); i >=0; i--)
            {
                Node node = mWaittingDestroys[i];
                mSystem.NodeFactory.Destroy(node);
            }
            mWaittingDestroys.Clear();
        }
        private Node InternalAddNode(int nodeType)
        {
            Node node = mSystem.NodeFactory.Create(nodeType);
            node.SetTree(this);
            mNodes.Add(node.mNodeId, node);
            return node;
        }
        public Node AddNode(int nodeType)
        {
            int parentType = mRoot.mNodeType;
            if (mHierEnumDict.TryGetValue(nodeType, out parentType))
            {
                Node parentNode = mRoot.GetNode(parentType);
                Node childNode = InternalAddNode(nodeType);
                if (parentNode == null)
                {
                    UnityEngine.Debug.LogError($"parent node is null={parentNode}");
                }
                else
                {
                    parentNode.AddChild(childNode);
                }
                return childNode;
            }
            return null;
        }
        public Node AddNode(int parentType,int nodeType)
        {
            int type = mRoot.mNodeType;
            Node parentNode = mRoot.GetNode(parentType);
            Node childNode = InternalAddNode(nodeType);
            if (parentNode == null)
            {
                UnityEngine.Debug.LogError($"parent node is null={parentNode}  nodeType={nodeType}");
            }
            else
            {
                parentNode.AddChild(childNode);
            }
                
            return childNode;
        }
        public void RemoveNode(Node node)
        {
        }
        public Node GetNode(int nodeType)
        {
            return mRoot.GetNode(nodeType);
        }
        public void AddNodeToWaittingDestroy(Node node)
        {
            mWaittingDestroys.Add(node);
        }
    }
}





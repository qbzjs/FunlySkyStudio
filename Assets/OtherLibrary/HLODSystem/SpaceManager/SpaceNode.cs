using System.Collections.Generic;
using System.Linq;
using HLODSystem.Extensions;
using UnityEngine;

namespace HLODSystem.SpaceManager
{
    public class SpaceNode
    {
        public static SpaceNode CreateSpaceNodeWithBounds(Bounds bounds)
        {
            var spaceNode = new SpaceNode
            {
                Bounds = bounds
            };
            return spaceNode;
        }
        private Bounds m_bounds;
        private Bounds m_expandBounds;
        private SpaceNode m_parentNode;
        private List<SpaceNode> m_childTreeNodes = new List<SpaceNode>(); 
        private List<BaseHLODBehaviour> m_behaviours = new List<BaseHLODBehaviour>();
        

        public int index = 0;
        

        private string key;
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                {
                    key = Name;
                }
                return key;
            }
        }

        public Bounds Bounds
        {
            set
            {
                m_bounds = value;
                m_expandBounds = m_bounds;
                m_expandBounds.size *= 1.2f;
            }
            get => m_bounds;
        }

        public Bounds ExpandBounds
        {
            get => m_expandBounds;
        }

        private Renderer[] renderers = null;

        private Renderer[] tmpRenders = null;

        public Renderer[] TmpRenderers
        {
            get => tmpRenders;
            set => tmpRenders = value;
        }

        public Renderer[] Renderers
        {
            get
            {
                if (m_behaviours.Count == 0)
                {
                    return null;
                }
                else if (renderers == null)
                {
                    var tmpRenderers = new List<Renderer>();
                    foreach (var hlodBehaviour in m_behaviours)
                    {
                        if (hlodBehaviour is Behaviour behaviour)
                        {
                            tmpRenderers.AddRange( behaviour.GetComponentsInChildren<Renderer>());
                        }
                       
                    }
                    renderers = tmpRenderers.ToArray();
                }

                return renderers;
            }
            set
            {
                renderers = value;
            }
        }

        public List<BaseHLODBehaviour> Behaviours => m_behaviours;

        public SpaceNode ParentNode
        {
            set
            {
                m_parentNode?.m_childTreeNodes.Remove(this);
                m_parentNode = value;
                value?.m_childTreeNodes.Add(this);
            }
            get => m_parentNode;
        }

        public void Render()
        {
            if (!GetSpaceNodeRootObj().gameObject.activeInHierarchy)
            {
                return;
            }
            if (m_behaviours.Count > 0)
            {
                Gizmos.color = Color.red;   
                Gizmos.DrawWireCube(m_bounds.center, m_bounds.size);
            }
            foreach (var childTreeNode in m_childTreeNodes)
            {
                childTreeNode.Render();
            }
        }

        private bool isEmpty = true;

        public bool IsEmpty
        {
            get => isEmpty;
            set
            {
                
                if (isEmpty != value)
                {
                    isEmpty = value;
                    if (!isEmpty && ParentNode != null)
                    {
                        ParentNode.IsEmpty = false; 
                    }
                }
            }
        }

        public int Level
        {
            get
            {
                var level = 0;
                if (m_parentNode != null)
                {
                    var tmpParentNode = m_parentNode;
                    level++;
                    while (tmpParentNode.ParentNode != null)
                    {
                        level++;
                        tmpParentNode = tmpParentNode.ParentNode;
                    }
                }
                return level;
            }
        }

        public string Name
        {
            get
            {
                if (m_parentNode == null)
                {
                    return "0";
                }
                else
                {
                    return m_parentNode.Name + "_" + (m_parentNode.m_childTreeNodes.IndexOf(this) + 1);
                }
            }
        }

        public Transform GetSpaceNodeRootObj()
        {
            Transform tmpParent = null;
            if (m_parentNode == null)
            {

                var spaceRootObj = GameObject.Find("SpaceRoot");
                if (spaceRootObj != null)
                {
                    tmpParent = spaceRootObj.transform;
                }
                if (tmpParent == null)
                {
                    tmpParent = new GameObject("SpaceRoot").transform;
                }
            }
            else
            {
                tmpParent = m_parentNode.GetSpaceNodeRootObj();
            }

            Transform tmpObj = null;
            if (tmpParent != null)
            {
                tmpObj = tmpParent.Find(Name);
            }
            if (tmpObj == null)
            {
                tmpObj = new GameObject(Name).transform;
                tmpObj.transform.SetParent(tmpParent);
            }
            return tmpObj;
        }

        public SpaceNode GetChild(int index)
        {
            return m_childTreeNodes[index];
        }  
        public int GetChildCount()
        {
            return m_childTreeNodes.Count;
        }
        
        public SpaceNode[] ChildTreeNodes => m_childTreeNodes.ToArray();

        public void ForeachTmpRenderer(System.Action<Renderer> actionForRenderer)
        {
            if (tmpRenders == null)
            {
                return;
            }
            foreach (Renderer r in tmpRenders)
            {
#if UNITY_EDITOR
                if (r == null)
                {
                    Debug.LogError("Invalid renderer in bakeGroup");
                                
                    continue;
                }
#endif
                actionForRenderer.Invoke(r);
            }
        }

        public void ForeachRenderer(System.Action<Renderer> actionForRenderer)
        {
            foreach (Renderer r in renderers)
            {
#if UNITY_EDITOR
                if (r == null)
                {
                    Debug.LogError("Invalid renderer in bakeGroup");
                                
                    continue;
                }
#endif
                actionForRenderer.Invoke(r);
            }
        }
        
        public bool HasChild()
        {
            return m_childTreeNodes.Count > 0;
        }

        public bool IsContains(GameObject tmpObj)
        {
            var objBounds = tmpObj.transform.GetBounds();
            if (objBounds == null)
            {
                return false;
            }
            return objBounds.Value.IsPartOf(ref m_expandBounds);

        }
        
        public bool IsContains(BaseHLODBehaviour hlodBehaviour)
        {
            
            var objBounds = hlodBehaviour.GetBounds();
            if (objBounds == null)
            {
                return false;
            }
            return objBounds.Value.IsPartOf(ref m_expandBounds);
        }

        public void AddBehaviour(BaseHLODBehaviour hlodBehaviour)
        {
            if (!Application.isPlaying)
            {
                hlodBehaviour.transform.SetParent(GetSpaceNodeRootObj());
            }
            Behaviours.Add(hlodBehaviour);
        }


        public SpaceNode ParseBehaviour(BaseHLODBehaviour hlodBehaviour)
        {
            if (hlodBehaviour == null || !IsContains(hlodBehaviour))
            {
                LoggerUtils.Log("ParseBehaviour:" + hlodBehaviour.HLODID);
                return null;
            }

            var nearestDis = float.MaxValue;
            var nearestIndex = -1;
            for (int i = 0; i < ChildTreeNodes.Length; i++)
            {
                if (ChildTreeNodes[i].IsContains(hlodBehaviour))
                {
                    var dis = (hlodBehaviour.GetBounds().Value.center - ChildTreeNodes[i].Bounds.center).sqrMagnitude;
                    if (dis < nearestDis)
                    {
                        nearestDis = dis;
                        nearestIndex = i;
                    }
                }
            }

            SpaceNode tmpSpaceNode = null;
            if (nearestIndex == -1)
            {
                AddBehaviour(hlodBehaviour);
                tmpSpaceNode = this;
            }
            else
            {
                tmpSpaceNode = ChildTreeNodes[nearestIndex].ParseBehaviour(hlodBehaviour);
            }
            isEmpty = false;
            return tmpSpaceNode;
        }


        public void ParseBehaviours(List<BaseHLODBehaviour> behaviours)
        {
            foreach (var tmpBehaviour in behaviours)
            {
                var tmpSpaceNode = ParseBehaviour(tmpBehaviour);
                if (tmpSpaceNode == null && tmpBehaviour.GetBounds() != null)
                {
                    AddBehaviour(tmpBehaviour);
                    isEmpty = false;
                }
            }
        }

        public ulong GetAllBaseSpaceNodes(List<SpaceNode> nodes)
        {
            
            ulong count = 0;
            
            if (HasChild())
            {
                foreach (var childTree in m_childTreeNodes)
                {
                    count += childTree.GetAllBaseSpaceNodes(nodes);
                }
            }
            else
            {
                count++;
                nodes.Add(this);
            }
            return count;
        }

        public void GetRenderSpaceNodes(List<SpaceNode> nodes)
        {
            if (Renderers != null && Renderers.Length > 0)
            {
                nodes.Add(this);
            }
            if (HasChild())
            {
                foreach (var childTree in m_childTreeNodes)
                {
                    childTree.GetRenderSpaceNodes(nodes);
                }
            }
 
        }

        public SpaceNode GetBaseSpace(Vector3 pos)
        {
            if (Bounds.Contains(pos))
            {
                if (HasChild())
                {
                    foreach (var childTree in m_childTreeNodes)
                    {
                        if (childTree.Bounds.Contains(pos))
                        {

                            return childTree.GetBaseSpace(pos);
                        }
                    }
                    return null;
                }
                else
                {
                    return this;
                }
            }
            else
            {
                return null;
            }
            
        }


        public void HideRender()
        {
            if (Renderers == null) return;
            foreach (var tmpRender in Renderers)
            {
                tmpRender.forceRenderingOff = true;
            }
        }

        public void ToggleRender(bool isVisible)
        {
            foreach (var tmpBehaviour in Behaviours)
            {
                tmpBehaviour.IsOcclusion = !isVisible;
            }
        }

        public void HideAllRender()
        {
            if (isEmpty)
            {
                return;
            }

            foreach (var tmpBehaviour in Behaviours)
            {
                tmpBehaviour.IsOcclusion = true;
            }

            foreach (var childTreeNode in ChildTreeNodes)
            {
                childTreeNode.HideAllRender();
            }
        }

        public void ShowAllRender()
        {
            foreach (var tmpBehaviour in Behaviours)
            {
                tmpBehaviour.IsOcclusion = false;
            }
            
            foreach (var childTreeNode in ChildTreeNodes)
            {
                childTreeNode.ShowAllRender();
            }
        }

        public void ShowRender()
        {
            foreach (var tmpBehaviour in Behaviours)
            {
                tmpBehaviour.IsOcclusion = false;
            }
        }


        public SpaceNodeData GetData()
        {
            if (isEmpty)
            {
                return default;
            }
            var nodeData = new SpaceNodeData()
            {
                index = index,
                parent = ParentNode?.index ?? -1,
            };
            var childrenIndexes = new List<int>();
            foreach (var tmpChildSpace in ChildTreeNodes)
            {
                childrenIndexes.Add(tmpChildSpace.index);
            }
            nodeData.children = childrenIndexes.ToArray();
            var behaviourIds = new List<string>();
            foreach (var behaviour in Behaviours)
            {
                behaviourIds.Add(behaviour.HLODID);
            }
            nodeData.nodes = behaviourIds.ToArray();
            return nodeData;
        }
    }
}
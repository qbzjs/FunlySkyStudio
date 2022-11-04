/// <summary>
/// Author:YangJie
/// Description:
/// Date: 2022/5/16 21:1:1
/// </summary>
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using HLODSystem.Controller;
using UnityEngine;

namespace HLODSystem.Trees
{
    [Serializable]
    public class HLODTreeNode
    {
        [SerializeField] private int m_level;
        [SerializeField] private Bounds m_bounds;
        [NonSerialized] private HLODTreeNodeContainer m_container;
        [SerializeField] private List<int> m_childTreeNodeIds = new List<int>();


        [SerializeField] private List<string> m_objectIds = new List<string>();
        [SerializeField] private List<BaseHLODBehaviour> m_baseBehaviours = new List<BaseHLODBehaviour>();

        private Dictionary<string, BaseHLODBehaviour> m_objects = new Dictionary<string, BaseHLODBehaviour>();



        private DefaultHLODController m_controller;
        private QuadTreeSpaceManager m_spaceManager;



        public int Level
        {
            set => m_level = value;
            get => m_level;
        }

        public Bounds Bounds
        {
            set => m_bounds = value;
            get => m_bounds;
        }

        private HLODState m_expectedState = HLODState.Cull;

        public HLODState State => m_expectedState;
        public List<string> ObjectIds => m_objectIds;

        public void SetContainer(HLODTreeNodeContainer container)
        {
            m_container = container;

            foreach (var childId in m_childTreeNodeIds)
            {
                var childTreeNode = m_container.Get(childId);
                childTreeNode.SetContainer(container);
            }
        }

        public void ClearChildTreeNode()
        {
            foreach (var childId in m_childTreeNodeIds)
            {
                m_container.Remove(childId);
            }

            m_childTreeNodeIds.Clear();
        }

        public void SetChildTreeNode(List<HLODTreeNode> childNodes)
        {
            ClearChildTreeNode();
            m_childTreeNodeIds.Capacity = childNodes.Count;

            foreach (var childNode in childNodes)
            {
                var id = m_container.Add(childNode);
                m_childTreeNodeIds.Add(id);
                childNode.SetContainer(m_container);
            }
        }

        public int GetChildTreeNodeCount()
        {
            return m_childTreeNodeIds.Count;
        }

        public void Init(DefaultHLODController controller, QuadTreeSpaceManager spaceManager, HLODTreeNode parent)
        {
            foreach (var childId in m_childTreeNodeIds)
            {
                var childTreeNode = m_container.Get(childId);
                childTreeNode.Init(controller, spaceManager, this);
            }

            m_controller = controller;
            m_spaceManager = spaceManager;
            m_objects.Clear();
        }

        public void Initialize(DefaultHLODController controller, QuadTreeSpaceManager spaceManager, HLODTreeNode parent)
        {
            foreach (var childId in m_childTreeNodeIds)
            {
                var childTreeNode = m_container.Get(childId);
                childTreeNode.Initialize(controller, spaceManager, this);
            }   
        }

        public List<HLODTreeNode> GetHighTreeNodes()
        {
            if (State != HLODState.High)
            {
                return null;
            }

            List<HLODTreeNode> result = new List<HLODTreeNode>();
            if (State == HLODState.High)
            {
                result.Add(this);
            }
            foreach (var childId in m_childTreeNodeIds)
            {
                var childTreeNode = m_container.Get(childId);
                var childHighTreeNodes = childTreeNode.GetHighTreeNodes();
                if (childHighTreeNodes != null)
                {
                    result.AddRange(childTreeNode.GetHighTreeNodes());
                }
            }

            return result;
        }

        public bool IsContains(string objectId)
        {
            if (m_objectIds.Contains(objectId))
            {
                return true;
            }
            return false;
        }

        public void AddBehaviour(string hlodId, BaseHLODBehaviour behaviour)
        {
            if (m_objects.ContainsKey(hlodId))
            {
                m_objects[hlodId] = behaviour;
            }
            else
            {
                m_objects.Add(hlodId, behaviour);
            }
            m_baseBehaviours.Add(behaviour);
        }

        public void RemoveBehaviour(string hlodId, BaseHLODBehaviour behaviour)
        {
            if (m_objects.ContainsKey(hlodId))
            {
                m_objects.Remove(hlodId);
            }
            m_baseBehaviours.Remove(behaviour);
        }

        #region FSM functions



        void OnEnteredCull()
        {
            foreach (var mObjectKeyValue in m_objects)
            {
                mObjectKeyValue.Value.SetLODStatus(HLODState.Cull);
            }
        }

        void OnEnteredLow()
        {
            foreach (var mObjectKeyValue in m_objects)
            {
                mObjectKeyValue.Value.SetLODStatus(HLODState.Low);
            }
        }

        void OnEnteredHigh()
        {
            foreach (var mObjectKeyValue in m_objects)
            {
                mObjectKeyValue.Value.SetLODStatus(HLODState.High);
            }
        }

        public void Cull()
        {
            OnEnteredCull();
            foreach (var childNodeId in m_childTreeNodeIds)
            {
                var childTreeNode = m_container.Get(childNodeId);
                childTreeNode.Cull();
            }
        }

        #endregion

        public void Update(float lodDistance, float cullDistance)
        {
            HLODState beforeState = m_expectedState;
            bool isCull = m_spaceManager.IsCull(cullDistance, m_bounds);
            
            if (isCull)
            {
                m_expectedState = HLODState.Cull;
            }
            else
            {
                m_expectedState = m_spaceManager.IsHigh(lodDistance, m_bounds) ? HLODState.High : HLODState.Low;
            }

            switch (m_expectedState)
            {
                case HLODState.Cull:
                    Cull();
                    break;
                case HLODState.Low:
                    OnEnteredLow();
                    break;
                case HLODState.High:
                    OnEnteredHigh();
                    break;
            }

            if (m_expectedState != HLODState.Cull)
            {
                foreach (var childNodeId in m_childTreeNodeIds)
                {
                    var childTreeNode = m_container.Get(childNodeId);
                    childTreeNode.Update(lodDistance, cullDistance);
                }
            }
        }


        public void Cull(bool isCull)
        {
            if (isCull)
            {
                m_expectedState = HLODState.Cull;
                Cull();
            }
            else
            {
                m_expectedState = HLODState.Low;
                OnEnteredLow();
            }
        }



        static Material lineMaterial;

        static void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }



        public void RenderBounds()
        {
            if (m_expectedState == HLODState.Cull)
                return;

            for (int i = 0; i < m_childTreeNodeIds.Count; ++i)
            {
                m_container.Get(m_childTreeNodeIds[i]).RenderBounds();
            }

            //if this node has a child node, skipping render.
            if (m_expectedState == HLODState.High && m_childTreeNodeIds.Count > 0)
                return;

            Color color = Color.white;

            if (m_expectedState == HLODState.Low)
                color = Color.yellow;
            else
                color = Color.green;

            Vector3 min = m_bounds.min;
            Vector3 max = m_bounds.max;

            Vector3[] vertices = new Vector3[8];
            vertices[0] = new Vector3(min.x, min.y, min.z);
            vertices[1] = new Vector3(min.x, min.y, max.z);
            vertices[2] = new Vector3(max.x, min.y, max.z);
            vertices[3] = new Vector3(max.x, min.y, min.z);

            vertices[4] = new Vector3(min.x, max.y, min.z);
            vertices[5] = new Vector3(min.x, max.y, max.z);
            vertices[6] = new Vector3(max.x, max.y, max.z);
            vertices[7] = new Vector3(max.x, max.y, min.z);

            CreateLineMaterial();
            // Apply the line material
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.LINES);

            GL.Color(color);

            //bottom
            GL.Vertex(vertices[0]);
            GL.Vertex(vertices[1]);
            GL.Vertex(vertices[1]);
            GL.Vertex(vertices[2]);
            GL.Vertex(vertices[2]);
            GL.Vertex(vertices[3]);
            GL.Vertex(vertices[3]);
            GL.Vertex(vertices[0]);

            //center
            GL.Vertex(vertices[0]);
            GL.Vertex(vertices[4]);
            GL.Vertex(vertices[1]);
            GL.Vertex(vertices[5]);
            GL.Vertex(vertices[2]);
            GL.Vertex(vertices[6]);
            GL.Vertex(vertices[3]);
            GL.Vertex(vertices[7]);

            //top
            GL.Vertex(vertices[4]);
            GL.Vertex(vertices[5]);
            GL.Vertex(vertices[5]);
            GL.Vertex(vertices[6]);
            GL.Vertex(vertices[6]);
            GL.Vertex(vertices[7]);
            GL.Vertex(vertices[7]);
            GL.Vertex(vertices[4]);

            GL.End();
            GL.PopMatrix();
        }
    }
}
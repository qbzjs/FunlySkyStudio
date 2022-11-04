/// <summary>
/// Author:YangJie
/// Description:
/// Date: 2022/5/16 13:12:25
/// </summary>

using System;
using System.Collections.Generic;
using HLODSystem.Trees;
using OtherLibrary.HLODSystem;
using UnityEngine;

namespace HLODSystem.Controller
{
    using ControllerID = Int32;
    public class DefaultHLODController : MonoBehaviour
    {

        #region variables
        private QuadTreeSpaceManager m_spaceManager;

        [SerializeField] 
        private HLODTreeNodeContainer m_treeNodeContainer;
        [SerializeField]
        private HLODTreeNode m_root;

        [SerializeField]
        private float m_cullDistance = 0.3f;
        [SerializeField]
        private float m_lodDistance = 2f;

        [SerializeField] private bool m_runtimeDebug = false;

        public QuadTreeSpaceManager spaceManager => m_spaceManager;

        public HLODTreeNodeContainer Container
        {
            set
            {
                m_treeNodeContainer = value; 
                UpdateContainer();
            }
            get => m_treeNodeContainer;
        }
        public HLODTreeNode Root
        {
            set
            {
                m_root = value; 
                UpdateContainer();
            }
            get => m_root;
        }

        public float CullDistance
        {
            set => m_cullDistance = value;
            get => m_cullDistance;
        }

        public float LODDistance
        {
            set => m_lodDistance = value;
            get => m_lodDistance;
        }
        #endregion
        public void Awake()
        {
            m_spaceManager = new QuadTreeSpaceManager();
            UpdateContainer();
            HLODCameraManager.Inst.SpaceManager = m_spaceManager;
        }

        public void OnDestroy()
        {
            if (m_spaceManager != null)
            {
                m_spaceManager.Release();
            }
        }

        public void Init()
        {
            if (GameManager.Inst.unityConfigInfo.mapBlock != null)
            {
                this.m_cullDistance = GameManager.Inst.unityConfigInfo.mapBlock.runtimeCullDistance;
                this.m_lodDistance = GameManager.Inst.unityConfigInfo.mapBlock.runtimeLodDistance;
            }
            else
            {
                this.m_cullDistance = GameConsts.DefaultMapBlock.runtimeCullDistance;
                this.m_lodDistance = GameConsts.DefaultMapBlock.runtimeLodDistance;
            }
            if(Container != null && Container.Count > 0)
            {
                m_root = Container.Get(0);
                UpdateContainer();
            }
            m_root.Init(this, m_spaceManager, null);
            m_spaceManager.isCameraCull = HLOD.Inst.IsCameraCull;
        }

        public void Initialize()
        {
            m_root.Initialize(this, m_spaceManager, null);
        }
        
        public void OnRenderObject()
        {
            if (m_runtimeDebug == false)
                return;

            m_root.RenderBounds();
        }
        
        private void UpdateContainer()
        {
            m_root?.SetContainer(m_treeNodeContainer);
        }
        
        #region Method
        
        public void UpdateCull(Camera cam)
        {
            if (m_spaceManager == null)
                return;

            m_spaceManager.UpdateCamera(transform, cam);
            HLODCameraManager.Inst.UpdateCamera((otherCamera) =>
            {
                m_spaceManager.UpdateCamera(transform, otherCamera);
            });
            
            m_root.Update(m_lodDistance, m_cullDistance);
        }

        public List<HLODTreeNode> GetHighTreeNodes()
        {
            return m_root.GetHighTreeNodes();
        }

        #endregion

    }

}


/// <summary>
/// Author:Shaocheng
/// Description: 管理业务层动态创建的HLOD的相机，例如相机模式，人走出镜头外，就需要动态创建一个Camera用于加载分块
/// Date: 2022-6-27 10:42:08
/// </summary>

using System;
using System.Collections.Generic;
using HLODSystem;
using UnityEngine;

namespace OtherLibrary.HLODSystem
{
    public class HLODCameraManager : CInstance<HLODCameraManager>
    {
        private QuadTreeSpaceManager m_spaceManager;
        private List<Camera> hlodCameras = new List<Camera>();

        public QuadTreeSpaceManager SpaceManager
        {
            get => m_spaceManager;
            set => m_spaceManager = value;
        }

        public void UpdateCamera(Action<Camera> action)
        {
            if (hlodCameras != null && hlodCameras.Count > 0)
            {
                foreach (var hlodCamera in hlodCameras)
                {
                    action?.Invoke(hlodCamera);
                }
            }
        }

        public void CreateHLODCamera(Camera camera)
        {
            if (camera != null && !hlodCameras.Contains(camera))
            {
                hlodCameras.Add(camera);
                LoggerUtils.Log($"HLODCameraManager Add HLODCamera camera :{camera.name}");
            }
        }

        public void ReleaseHLODCamera(Camera camera)
        {
            if (m_spaceManager == null || camera == null || !hlodCameras.Contains(camera))
            {
                return;
            }

            hlodCameras.Remove(camera);
            m_spaceManager.RemoveCamera(camera);
            LoggerUtils.Log($"HLODCameraManager Release HLODCamera camera :{camera.name}");
        }

        public override void Release()
        {
            base.Release();
            if (hlodCameras != null)
            {
                this.hlodCameras.Clear();
            }
        }
    }
}
/// <summary>
/// Author:YangJie
/// Description:
/// Date: 2022/5/16 21:1:1
/// </summary>
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HLODSystem
{
    public class QuadTreeSpaceManager
    {

        public bool isCameraCull = true;
        
        private class CameraParams
        {
            public float preRelative;
            public Vector3 camPosition;
            public Plane[] cameraPlanes;

            // public override string ToString()
            // {
            //     return $"preRelative:{this.preRelative}, camPosition:{camPosition}, cameraPlanes:{cameraPlanes}";
            // }
        }

        private Dictionary<Camera, CameraParams> cameraDict = new Dictionary<Camera, CameraParams>();

        public void UpdateCamera(Transform hlodTransform, Camera cam)
        {
            if (!cameraDict.ContainsKey(cam))
            {
                cameraDict.Add(cam, new CameraParams()); 
                LoggerUtils.Log("UpdateCamera:" + cameraDict.Count);
            }

            float halfAngle = Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView * 0.5F);
            cameraDict[cam].preRelative = 0.5f / halfAngle;
            // preRelative *= QualitySettings.lodBias;
            cameraDict[cam].camPosition = hlodTransform.worldToLocalMatrix.MultiplyPoint(cam.transform.position);
            cameraDict[cam].cameraPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
            // LoggerUtils.Log("updateCamera update value:" + cameraDict[cam].ToString());
        }

        public void RemoveCamera(Camera camera)
        {
            if (camera != null && cameraDict != null && cameraDict.ContainsKey(camera))
            {
                cameraDict.Remove(camera);
            }
        }

        public void Release()
        {
            if (cameraDict != null)
            {
                cameraDict.Clear();
            }
        }

        //lod高低级判断：多个camera中满足一个为high就返回True
        public bool IsHigh(float lodDistance, Bounds bounds)
        {
            return cameraDict.Values.Any(tmp => IsHigh(tmp, lodDistance, bounds));
        }

        private bool IsHigh(CameraParams camPara, float lodDistance, Bounds bounds)
        {
            var distance = GetDistance(bounds.center, camPara.camPosition);
            var relativeHeight = bounds.size.x * camPara.preRelative / distance;
            return relativeHeight > lodDistance;
        }


        private bool IsCull(CameraParams camPara, float cullDistance, Bounds bounds)
        {
            if (isCameraCull)
            {
                var isCull = !GeometryUtility.TestPlanesAABB(camPara.cameraPlanes, bounds);
                if (isCull)
                {
                    return true;
                }
            }
            var distance = GetDistance(bounds.center, camPara.camPosition);
            var relativeHeight = bounds.size.x * camPara.preRelative / distance;
            return relativeHeight < cullDistance;
        }

        //是否裁剪判断：多个camera，同时需要Cull才返回True
        public bool IsCull(float cullDistance, Bounds bounds)
        {
            return cameraDict.Values.All(tmp => IsCull(tmp, cullDistance, bounds));
        }

        public float GetDistanceSquare(Bounds bounds, Camera cam)
        {
            var x = bounds.center.x - cameraDict[cam].camPosition.x;
            var z = bounds.center.z - cameraDict[cam].camPosition.z;

            var square = x * x + z * z;
            return square;
        }
        
        private float GetDistance(Vector3 boundsPos, Vector3 camPos)
        {
            var x = boundsPos.x - camPos.x;
            var z = boundsPos.z - camPos.z;
            var square = x * x + z * z;
            return Mathf.Sqrt(square);
        }
    } 
}


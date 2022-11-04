using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinemachine;
using UnityEngine;

public class CameraUtils:CInstance<CameraUtils>
{
    private Vector3 createAxisPos = new Vector3(0,1.1f,-4.31f);
    private Vector3 createOffset = new Vector3(0, 0.5f, 0);
    private Vector3 createDefault = new Vector3(0, 1f, 0);
    private float maxCamDist = 350;
    private Camera mainCamera;
    private CinemachineVirtualCamera cam;
    public CinemachineVirtualCamera VirtualCamera
    {
        get
        {
            if(cam == null)
            {
                cam = GameObject.Find("PlayModeCamera").GetComponent<CinemachineVirtualCamera>();
            }
            return cam;
        }
    }

    public void SetMainCamera(Camera mCamera)
    {
        mainCamera = mCamera;
    }

    public Vector3 GetCreatePosition()
    {
        if (mainCamera != null)
        {
            Vector3 screenPos = new Vector3(Screen.width / 2, Screen.height / 3, 0);
            Ray ray = mainCamera.ScreenPointToRay(screenPos);
            var layerMask = 1 << LayerMask.NameToLayer("PVPArea");
            bool isHit = Physics.Raycast(ray, out RaycastHit hit, maxCamDist, ~layerMask);
            Vector3 targetPosition = isHit? hit.point + createOffset:createDefault;
            return targetPosition;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Camera GetMainCamera()
    {
        return mainCamera;
    }
}
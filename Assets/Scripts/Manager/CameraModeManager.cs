using System.Collections;
using Cinemachine;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description: 相机模式管理
/// Date: 2022-6-13 10:52:24
/// </summary>
public enum CameraModeEnum
{
    NormalGuestCamera = 1, //游玩模式正常相机
    FreePhotoCamera = 2, //自由拍照相机模式
    RecordCamera = 3, //录屏相机模式
}

public enum CameraBodyType
{
    Transposer = 1, 
    HardLockToTarget = 2
}

public class CameraModeManager : CInstance<CameraModeManager>
{
    public BaseCameraMode CurMode;

    public BaseCameraMode NormalGuestCameraMode;
    public BaseCameraMode FreePhotoCameraMode;
    public BaseCameraMode RecordCameraMode;
    public const float DEFAULT_FOLLOW_Z = -7f;
    public const float CAMERA_FIELD_OF_VIEW = 50;
    public const float SELFIE_FIELD_OF_VIEW = 60;
    public void Init()
    {
        NormalGuestCameraMode = new NormalGuestCameraMode(CameraModeEnum.NormalGuestCamera);
        FreePhotoCameraMode = new FreePhotoCameraMode(CameraModeEnum.FreePhotoCamera);
        RecordCameraMode = new RecordCameraMode(CameraModeEnum.RecordCamera);
        CurMode = NormalGuestCameraMode; //默认是普通游玩相机
    }

    public override void Release()
    {
        base.Release();
        ExitCurrentMode();
        NormalGuestCameraMode = null;
        FreePhotoCameraMode = null;
        RecordCameraMode = null;
    }

    /// <summary>
    /// 模式切换：供外部调用
    /// </summary>
    /// <param name="cameraMode"></param>
    public void EnterMode(CameraModeEnum cameraMode)
    {
        ExitCurrentMode();

        switch (cameraMode)
        {
            case CameraModeEnum.NormalGuestCamera:
                CurMode = NormalGuestCameraMode;
                CurMode.Enter();
                SelfieModeManager.Inst.ExitSelfieMode();
                break;
            case CameraModeEnum.FreePhotoCamera:
                CurMode = FreePhotoCameraMode;
                CurMode.Enter();
                break;
            case CameraModeEnum.RecordCamera:
                CurMode = RecordCameraMode;
                CurMode.Enter();
                break;
            default: break;
        }
    }

    private void ExitCurrentMode()
    {
        if (CurMode != null)
        {
            CurMode.Exit();
            CurMode = null;
        }
    }

    public CinemachineVirtualCamera GetPlayVirCamera()
    {
        GameObject playModeCamera = GameObject.Find("PlayModeCamera");
        var virCamera = playModeCamera.GetComponent<CinemachineVirtualCamera>();
        return virCamera;
    }

    public CinemachineVirtualCamera GetCameraVirCamera()
    {
        GameObject ModeCameraObj = GameObject.Find("CameraModeVirCamera");
        if(!ModeCameraObj)
        {   
            //创建虚拟相机
            GameObject playModeCamera = GameObject.Find("PlayModeCamera");
            ModeCameraObj = GameObject.Instantiate(playModeCamera,playModeCamera.transform.parent);
            ModeCameraObj.name = "CameraModeVirCamera";
            GameObject cameraFollowNode = GetCameraModeNode();
           

            //设置虚拟相机参数,参照PlayModeCamera参数
            var virCamera = ModeCameraObj.GetComponent<CinemachineVirtualCamera>();
            if(virCamera == null)
            {
                virCamera = ModeCameraObj.AddComponent<CinemachineVirtualCamera>();
            }
            virCamera.Follow = cameraFollowNode.transform;
            virCamera.LookAt = cameraFollowNode.transform;
            virCamera.AddCinemachineComponent<CinemachineSameAsFollowTarget>();
            virCamera.m_Lens.FieldOfView = CAMERA_FIELD_OF_VIEW;
            SetVirCameraBodyType(virCamera,CameraBodyType.Transposer);
            var transposer = virCamera.GetCinemachineComponent<CinemachineTransposer>();
            transposer.m_FollowOffset = new Vector3(0,0,DEFAULT_FOLLOW_Z);
            
        }
        CinemachineVirtualCamera virtualCamera = ModeCameraObj.GetComponent<CinemachineVirtualCamera>();
        return virtualCamera;
    }

    public void SetVirCameraBodyType(CinemachineVirtualCamera virCamera,CameraBodyType type)
    {
        if(type == CameraBodyType.Transposer)
        {
            var transposer = virCamera.GetCinemachineComponent<CinemachineTransposer>();
            if(transposer == null)
            {
                transposer = virCamera.AddCinemachineComponent<CinemachineTransposer>();
            }
            transposer.m_BindingMode = CinemachineTransposer.BindingMode.LockToTarget;
            transposer.m_XDamping = 0;
            transposer.m_YDamping = 0;
            transposer.m_ZDamping = 0;
        }
        else if(type == CameraBodyType.HardLockToTarget)
        {
            virCamera.AddCinemachineComponent<CinemachineHardLockToTarget>();
        }
        
    }

    public GameObject GetCameraModeNode()
    {
        GameObject cameraFollowNode = GameObject.Find("CameraModeNode");
        if(cameraFollowNode == null)
        {
            cameraFollowNode = new GameObject("CameraModeNode");
        }
        return cameraFollowNode;
    }


    public CameraModeEnum GetCurrentCameraMode()
    {
        if (CurMode == null)
        {
            return CameraModeEnum.NormalGuestCamera;
        }

        return CurMode.Mode;
    }
}

using Cinemachine;
using OtherLibrary.HLODSystem;
using UnityEngine;

/// <summary>
/// 相机模式基类
/// </summary>
public abstract class BaseCameraMode
{
    public CameraModeEnum Mode;
    public InputHandler Handler;
    public GameObject ModeCameraObj;
    public CinemachineVirtualCamera VirtualCamera;

    protected BaseCameraMode(CameraModeEnum mode)
    {
        this.Mode = mode;
    }

    public abstract void Enter();

    public abstract void Exit();
}

/// <summary>
/// 自由拍照相机模式
/// </summary>
public class FreePhotoCameraMode : BaseCameraMode
{
    private Camera hlodCamera;
    
    public FreePhotoCameraMode(CameraModeEnum mode) : base(mode)
    {
    }

    public InputHandler GetCameraModeHandler()
    {   
        if(Handler == null)
        {
            var eHandler = new CameraModeHandler();
            var cam = GameManager.Inst.MainCamera;
            var vCam = CameraModeManager.Inst.GetCameraVirCamera();
            eHandler.SetCamera(cam, vCam);
            eHandler.joyStick = PlayModePanel.Instance.joyStick;
            eHandler.joyStick.JoystickReset();
            Handler = eHandler;
        }
       
        return Handler;
    }

    public Camera GetHlodCamera()
    {
        if (hlodCamera == null)
        {
            var cameraMain = Camera.main;
            var hlodCameraGo = new GameObject
            {
                name = "FreeCameraMode_HLODCamera",
                tag = "Untagged"
            };
            hlodCameraGo.SetActive(false);
            if (PlayerBaseControl.Inst == null || PlayerBaseControl.Inst.playerAnim == null)
            {
                return null;
            }
            hlodCameraGo.transform.parent = PlayerBaseControl.Inst.playerAnim.gameObject.transform;
            hlodCameraGo.transform.localPosition = Vector3.zero;
            hlodCameraGo.transform.localRotation = Quaternion.identity;
            var newCameraCmp = hlodCameraGo.AddComponent<Camera>();
            var sourceCameraCmp = cameraMain.GetComponent<Camera>();
            newCameraCmp.fieldOfView = sourceCameraCmp.fieldOfView;
            newCameraCmp.farClipPlane = sourceCameraCmp.farClipPlane;
            newCameraCmp.nearClipPlane = sourceCameraCmp.nearClipPlane;
            hlodCamera = hlodCameraGo.GetComponent<Camera>();
            hlodCamera.enabled = false;
        }

        return hlodCamera;
    }

    public void RemoveHlodCamera()
    {
        if (hlodCamera != null)
        {
            GameObject.Destroy(hlodCamera.gameObject);
        }
    }

    public override void Enter()
    {

        
        //TODO:进入某模式的UI显隐控制修改成配置
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.FreeCameraModeSwitch(true);
        }
        
        UIControlManager.Inst.CallUIControl("camera_mode_enter");


        //TODO:过渡动画
        // BlackPanel.Show();
        // BlackPanel.Instance.PlayTransitionAnimAct(() =>
        // {
        //     BlackPanel.Instance.transform.SetAsLastSibling();
        // });
        
        //虚拟相机切换
        VirtualCamera = CameraModeManager.Inst.GetCameraVirCamera();
        ModeCameraObj = VirtualCamera.gameObject;
        VirtualCamera.enabled = true;

        //切换相机模式 Handler
        CameraModeHandler handler = GetCameraModeHandler() as CameraModeHandler;
        handler.OnEnter();
        handler.joyStick.JoystickReset();
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.animCon.isPlaying == false)
        {
            PlayerControlManager.Inst.Move(Vector3.zero);
        }
        
        Handler = handler;
        InputReceiver.Inst.SetHandle(Handler);

        MessageHelper.Broadcast(MessageName.ReleaseTrigger);

        HLODCameraManager.Inst.CreateHLODCamera(GetHlodCamera());
    }

    public override void Exit()
    {
        //TODO:进入某模式的UI显隐控制修改成配置
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.FreeCameraModeSwitch(false);
        }
        
        UIControlManager.Inst.CallUIControl("camera_mode_exit");

        if (Handler != null)
        {
            var handler = Handler as CameraModeHandler;
            handler.joyStick.JoystickReset();
            if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.animCon.isPlaying == false)
            {
                PlayerControlManager.Inst.Move(Vector3.zero);
            }
        }
        
        VirtualCamera = CameraModeManager.Inst.GetCameraVirCamera();
        VirtualCamera.enabled = false;
        
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        
        HLODCameraManager.Inst.ReleaseHLODCamera(GetHlodCamera());
        RemoveHlodCamera();
    }
}

/// <summary>
/// 录屏模式相机 //TODO:后续版本使用
/// </summary>
public class RecordCameraMode : BaseCameraMode
{
    public RecordCameraMode(CameraModeEnum mode) : base(mode)
    {
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }
}

/// <summary>
/// 正常游玩模式相机
/// </summary>
public class NormalGuestCameraMode : BaseCameraMode
{
    public NormalGuestCameraMode(CameraModeEnum mode) : base(mode)
    {
        ModeCameraObj = GameObject.Find("PlayModeCamera");
        VirtualCamera = ModeCameraObj.GetComponent<CinemachineVirtualCamera>();
    }

    public override void Enter()
    {
        if (VirtualCamera != null)
        {
            VirtualCamera.enabled = true;
        }

        var playModeController = GameObject.Find("GameStart").GetComponent<GameController>().playController;
        if(playModeController != null)
        {
            playModeController.SetPlayHandler();
        }
    }

    public override void Exit()
    {
        if (VirtualCamera != null)
        {
            VirtualCamera.enabled = false;
        }
    }
}

using System.Collections;
using Cinemachine;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:相机模式--自拍模式管理
/// Date: 2022-8-10 17:39:24

public class SelfieModeManager : CInstance<SelfieModeManager>
{
    public bool isPlayingSelfieAnim = false; // 是否正在播放自拍进入动画
    // private Vector3 cameraDefaultAngles = new Vector3(10,180,0);
    // private Vector3 cameraDefaultPos = new Vector3(-0.05f,1f,1.03f);
    private Vector3 cameraDefaultAngles = new Vector3(13,-154,0);
    private Vector3 cameraDefaultPos = new Vector3(0,1f,1.03f);

    private BudTimer _enterTimer, _exitTimer, _playEnterTimer;

    /// <summary>
    /// 自拍模式互斥提示
    /// </summary>
    public void ShowSelfieModeToast()
    {
        TipPanel.ShowToast("Please quit selfie mode first.");
    }

    public void CreateSelfieTool()
    {
        var animCon = PlayerBaseControl.Inst.animCon;
        if (animCon.isLooping && animCon.loopingInfo != null && animCon.loopingInfo.id == (int)EmoName.EMO_SELFIE_MODE)
        {
            if (PlayerControlManager.Inst.effectTool == null)
            {
                var playerAnim = PlayerBaseControl.Inst.playerAnim;
                var roleCon = playerAnim.gameObject.GetComponent<RoleController>();
                var leftNode = roleCon.GetBandNode((int)BodyNode.LEffectNode);
                var oldEffect = animCon.moveEffect.expressionGameObject[0];
                if (oldEffect)
                {
                    var effect = GameObject.Instantiate(oldEffect, leftNode);
                    effect.transform.localRotation = oldEffect.transform.localRotation;
                    effect.transform.localPosition = oldEffect.transform.localPosition;
                    effect.transform.localScale = oldEffect.transform.localScale;
                    PlayerControlManager.Inst.effectTool = effect;
                }
            }
        }
        ClearSelfieTimer();
        _enterTimer = TimerManager.Inst.RunOnce("ResetIdle", 0.2f, () => { ResetIdle(); });
        PlayerControlManager.Inst.ChangeAnimClips();
    }

    public void RestoreSelfieModeAnim(int id)
    {
        var effectTool = PlayerControlManager.Inst.effectTool;
        var emoIconData = MoveClipInfo.GetAnimName(id);
        var bandBody = emoIconData.moveInfos[1].bandBody;
        if (effectTool == null)
        {
            string name = emoIconData.name.Split('_')[0];
            string path = "Prefabs/Emotion/Express/" + name; ;
            GameObject movePrefab = ResManager.Inst.LoadCharacterRes<GameObject>(path);

            if (movePrefab != null)
            {
                effectTool = GameObject.Instantiate(movePrefab);
                var playerAnim = PlayerBaseControl.Inst.playerAnim;
                var roleCon = playerAnim.gameObject.GetComponent<RoleController>();
                var parentNode = roleCon.GetBandNode(bandBody[0].bandNode);
                effectTool.transform.SetParent(parentNode);
                PlayerControlManager.Inst.effectTool = effectTool;
            }
        }

        effectTool.transform.localRotation = Quaternion.Euler(bandBody[0].r.x, bandBody[0].r.y, bandBody[0].r.z);
        effectTool.transform.localPosition = bandBody[0].p;
        effectTool.transform.localScale = bandBody[0].s;
    }

    public void ResetIdle()
    {
        var playerAnim = PlayerBaseControl.Inst.playerAnim;
        playerAnim.CrossFade("idle", 0.2f, 0, 0f);
    }

    public void EnterSelfieMode()
    {
        PlayEnterSelfieAnim();
        GameObject cameraModeNode = CameraModeManager.Inst.GetCameraModeNode();
        GameObject playerObj = PlayerManager.Inst.selfPlayer.gameObject;
        Transform playerRealNode = PlayerManager.Inst.selfPlayer.playerAnim.transform;
        cameraModeNode.transform.parent = playerRealNode.transform;
        cameraModeNode.transform.localEulerAngles = cameraDefaultAngles;
        cameraModeNode.transform.localPosition = cameraDefaultPos;


        CinemachineVirtualCamera virCamera = CameraModeManager.Inst.GetCameraVirCamera();
        CameraModeManager.Inst.SetVirCameraBodyType(virCamera,CameraBodyType.HardLockToTarget);
        var transposer = virCamera.GetCinemachineComponent<CinemachineTransposer>();
        virCamera.m_Lens.FieldOfView = CameraModeManager.SELFIE_FIELD_OF_VIEW;
        if (SwingManager.Inst != null)
        {
            SwingManager.Inst.Selfie();
        }
    }

    public void PlayEnterSelfieAnim()
    {
        //发起循环动作-->自拍
        PlayerBaseControl.Inst.animCon.PlayAnim((int)EmoName.EMO_SELFIE_MODE);

        SelfieModeManager.Inst.isPlayingSelfieAnim = true;
        RestoreSelfieModeAnim((int)EmoName.EMO_SELFIE_MODE);
        if (PlayerControlManager.Inst.effectTool)
        {
            PlayerControlManager.Inst.effectTool.SetActive(false);
        }

        ClearSelfieTimer();
        _playEnterTimer = TimerManager.Inst.RunOnce("enterSelfieMode", 1.8f, () =>
        {
            SelfieModeManager.Inst.CreateSelfieTool();
            PlayerControlManager.Inst.effectTool.SetActive(true);
            if (PlayerSwimControl.Inst)
            {
                PlayerSwimControl.Inst.ForceOutWater();
            }

            if (PlayerStandonControl.Inst)
            {
                PlayerStandonControl.Inst.ResetStandOn();
            }
            SelfieModeManager.Inst.isPlayingSelfieAnim = false;
        });
    }

    public void ExitSelfieMode()
    {
        if (!StateManager.IsInSelfieMode)
        {
            return;
        }
        PlayerBaseControl.Inst.isInSelfieMode = false;
        var playerAnim = PlayerBaseControl.Inst.playerAnim;
        playerAnim.Play("selfiestick_end", 0, 0);

        if (PlayerControlManager.Inst.effectTool)
        {
            PlayerControlManager.Inst.effectTool.GetComponent<Animator>().Play("selfiestick_end", 0, 0f);
        }
        PlayerControlManager.Inst.ChangeAnimClips();
        ClearSelfieTimer();
        _exitTimer = TimerManager.Inst.RunOnce("ClearSelfieMode", 0.8f, () => { ClearSelfieMode(); });


        //相机切换
        GameObject playModeCamera = GameObject.Find("PlayModeCamera");
        GameObject cameraModeNode = CameraModeManager.Inst.GetCameraModeNode();
        cameraModeNode.transform.parent = playModeCamera.transform.parent;
        
        CinemachineVirtualCamera virCamera = CameraModeManager.Inst.GetCameraVirCamera();
        CameraModeManager.Inst.SetVirCameraBodyType(virCamera,CameraBodyType.Transposer);

        var transposer = virCamera.GetCinemachineComponent<CinemachineTransposer>();
        transposer.m_FollowOffset = new Vector3(0,0,0);

        if(virCamera.m_Lens.FieldOfView > CameraModeManager.SELFIE_FIELD_OF_VIEW)
        {
            virCamera.m_Lens.FieldOfView = CameraModeManager.SELFIE_FIELD_OF_VIEW;
        }
        else if(virCamera.m_Lens.FieldOfView < CameraModeManager.CAMERA_FIELD_OF_VIEW)
        {
            virCamera.m_Lens.FieldOfView = CameraModeManager.CAMERA_FIELD_OF_VIEW;
        }
        if (CameraModeManager.Inst.FreePhotoCameraMode != null)
        {
            var handler = CameraModeManager.Inst.FreePhotoCameraMode.Handler as CameraModeHandler;
            handler.OnCameraMove();
        }
        if (PlayerControlManager.Inst.effectTool)
        {
            PlayerControlManager.Inst.effectTool.SetActive(false);
        }

        if (SwingManager.Inst != null)
        {
            SwingManager.Inst.ExitSelfie();
        }
    }
    
    private void ClearSelfieTimer()
    {
        TimerManager.Inst.Stop(_exitTimer);
        TimerManager.Inst.Stop(_enterTimer);
        TimerManager.Inst.Stop(_playEnterTimer);
    }

    public void ClearSelfieMode()
    {
        var playerAnim = PlayerBaseControl.Inst.playerAnim;
        if (PlayerControlManager.Inst.effectTool)
        {
            GameObject.Destroy(PlayerControlManager.Inst.effectTool);
        }
        playerAnim.Play("idle", 0, 0);
        if (PlayerStandonControl.Inst)
        {
            PlayerStandonControl.Inst.ResetStandOn();
        }
    }
}

/// <summary>
/// Author:WeiXin
/// Description: 摄像机位移动管理
/// Date: 2022/5/12 17:42:11
/// </summary>

using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class CameraZoomManager : CInstance<CameraZoomManager>
{
    private CinemachineVirtualCamera playerCamera;
    public void Init()
    {
        playerCamera = GameObject.Find("PlayModeCamera").GetComponent<CinemachineVirtualCamera>();
    }
    
    private Vector3 PVPcamPos;
    public void PVPZoomInPlayer()
    {
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.playerAnim)
        {
            playerCamera.LookAt = null;
            playerCamera.Follow = null;
            var trs = PlayerBaseControl.Inst.playerAnim.transform;
            trs.localRotation = Quaternion.Euler(new Vector3(0,-180,0));
            PVPcamPos = playerCamera.transform.position;
            playerCamera.transform.DOMove(trs.position + playerCamera.transform.forward.normalized * -5 
                                                       + playerCamera.transform.up.normalized, 0.6f).SetAutoKill(false);
        }
    }
    
    public void ZoomOutPlayer()
    {

        var c = GameObject.Find("Play Mode Camera Center");
        playerCamera.transform.DOMove(PVPcamPos, 0.6f).OnComplete(() =>
        {
            playerCamera.LookAt = c.transform;
            playerCamera.Follow = c.transform;
        });
        
    }
}

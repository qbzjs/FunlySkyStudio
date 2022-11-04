/// <summary>
/// Author:Mingo-LiZongMing
/// Description:
/// </summary>
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FishingCtrPanel : BasePanel<FishingCtrPanel>
{
    public GameObject BtnPanel;
    public Button BtnStartFishing;
    public Button BtnPullFishing;
    public Button BtnStopFishing;
    public Button BtnExitCamZoom;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        //联机模式下，需要等服务器回包之后才进行按钮状态切换
        MessageHelper.AddListener(MessageName.OnFishingStart, OnFishingStart);
        MessageHelper.AddListener(MessageName.OnFishingStop, OnFishingStop);
        BtnStartFishing.onClick.AddListener(OnBtnStartFishingClick);
        BtnPullFishing.onClick.AddListener(OnBtnPullFishingClick);
        BtnStopFishing.onClick.AddListener(OnBtnStopFishingClick);
        BtnExitCamZoom.onClick.AddListener(OnClickClose);

        lookPoint = GameObject.Find("Play Mode Camera Center");
        cam = GameObject.Find("PlayModeCamera").GetComponent<CinemachineVirtualCamera>();
    }

    private void OnDisable()
    {
        BtnPanel.SetActive(true);
        BtnStartFishing.gameObject.SetActive(true);
        BtnPullFishing.gameObject.SetActive(false);
        BtnStopFishing.gameObject.SetActive(false);
        BtnExitCamZoom.gameObject.SetActive(false);

        if (PortalPlayPanel.Instance)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(true);
        }
    }

    private void OnFishingStart()
    {
        BtnStartFishing.gameObject.SetActive(false);
        BtnPullFishing.gameObject.SetActive(true);
        if (PortalPlayPanel.Instance)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(false);
        }
    }

    private void OnFishingStop() {
        BtnStartFishing.gameObject.SetActive(true);
        BtnPullFishing.gameObject.SetActive(false);
        if (PortalPlayPanel.Instance)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(true);
        }

        BtnStopFishing.gameObject.SetActive(false);

    }

    private void OnBtnStartFishingClick()
    {
        if (!StateManager.Inst.CheckCanStartFishing())
        {
            return;
        }
        PlayerBaseControl.Inst.Move(Vector3.zero);
        FishingManager.Inst.StartFishing();
    }

    private void OnBtnPullFishingClick()
    {
        var playerNode = ClientManager.Inst.GetPlayerNode(GameManager.Inst.ugcUserInfo.uid);
        if (playerNode == null)
            return;

        if (!StateManager.Inst.CheckCanEndFishing())
        {
            return;
        }
        var playerFishingCtrl = playerNode.GetComponent<PlayerFishingController>();
        if (playerFishingCtrl && playerFishingCtrl.State == FishingState.Fishing)
        {
            FishingManager.Inst.PullFishingRod();
        }
    }

    private void OnBtnStopFishingClick()
    {
        FishingManager.Inst.StopFishing();
        EnterShowFishMode(false);
    }

    public void SetCtrlPanelVisible(bool isVisible)
    {
        BtnPanel.SetActive(isVisible);
        var playerNode = ClientManager.Inst.GetPlayerNode(GameManager.Inst.ugcUserInfo.uid);
        if (playerNode == null)
            return;
        var playerFishingCtrl = playerNode.GetComponent<PlayerFishingController>();
        if (playerFishingCtrl && playerFishingCtrl.State == FishingState.ShowFish)
        {
            BtnPanel.SetActive(false);
        }
    }

    protected override void OnDestroy()
    {
        MessageHelper.RemoveListener(MessageName.OnFishingStart, OnFishingStart);
        MessageHelper.RemoveListener(MessageName.OnFishingStop, OnFishingStop);
        base.OnDestroy();
    }

    public void EnterShowFishMode(bool isEnter)
    {
        BtnPanel.SetActive(!isEnter);
        BtnStopFishing.gameObject.SetActive(isEnter);
    }

    //特殊处理相机聚焦，显示展示鱼的动画
    private CinemachineVirtualCamera cam;
    private Vector3 Zoom_IN_CAM_FOLLOW_OFFSET = new Vector3(8, 180, 0);
    private Vector3 Zoom_OUT_CAM_FOLLOW_OFFSET = new Vector3(0, 0, 0);
    private Vector3 Zoom_OUT_CAM_Rot_OFFSET = new Vector3(0, 0, 0);
    private float zoomSpeed = 0.6f;
    private Tweener _tweener;
    private GameObject lookPoint;

    public void SetZoom()
    {
        Zoom_OUT_CAM_FOLLOW_OFFSET = cam.transform.position;
        Zoom_OUT_CAM_Rot_OFFSET = cam.transform.rotation.eulerAngles;
        cam.LookAt = null;
        cam.Follow = null;
        var playerRot = PlayerBaseControl.Inst.transform.localEulerAngles;
        var playerModelRot = PlayerBaseControl.Inst.playerAnim.transform.localEulerAngles;
        var targetCamPos = PlayerBaseControl.Inst.playerAnim.transform.Find("ShowFishPos").position;
        Tween tween = cam.transform.DOMove(targetCamPos, zoomSpeed);
        cam.transform.DORotate(playerRot + playerModelRot + Zoom_IN_CAM_FOLLOW_OFFSET, zoomSpeed);
        tween.onComplete += () =>
        {
            BtnExitCamZoom.gameObject.SetActive(true);
        };
    }

    private void OnClickClose()
    {
        BtnExitCamZoom.gameObject.SetActive(false);
        _tweener = cam.transform.DOMove(Zoom_OUT_CAM_FOLLOW_OFFSET, zoomSpeed);
        cam.transform.DORotate(Zoom_OUT_CAM_Rot_OFFSET, zoomSpeed);
        StartCoroutine("waitDoTween");
    }

    private IEnumerator waitDoTween()
    {
        yield return _tweener.WaitForCompletion();
        cam.LookAt = lookPoint.transform;
        cam.Follow = lookPoint.transform;
    }
}

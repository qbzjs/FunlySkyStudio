using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayModeController : BaseModeController
{
    private float maxCamDist = 350;
    //private string lastUid = "";
    private Camera mainCam;
    public JoyStick joyStick;

    private PlayModeHandler inputHandler;

    public void Init()
    {
    }

    public void SetCamera(Camera cam, CinemachineVirtualCamera vCam)
    {
        mainCam = cam;
        inputHandler = new PlayModeHandler();
        inputHandler.SetCamera(cam, vCam);
        inputHandler.joyStick = joyStick;
        inputHandler.joyStick.JoystickReset();
        inputHandler.OnClickTarget = OnClickTarget;
        SetPlayHandler();
    }

    public void SetPlayHandler()
    {
        if(inputHandler != null)
        {
            InputReceiver.Inst.SetHandle(inputHandler);
        }
    }

    public void SetPlayerState(PlayerBaseControl playerCom, GameMode gameMode)
    {
        PVPGameMode mode = PVPWaitAreaManager.Inst.PVPBehaviour != null? PVPGameMode.Race:PVPGameMode.Normal;
        MessageHelper.Broadcast(MessageName.ChangeMode, gameMode);
        playerCom.Move(Vector3.zero);
        playerCom.transform.gameObject.SetActive(true);
        playerCom.curGameMode = gameMode;
        switch (mode)
        {
            case PVPGameMode.Normal:
                playerCom.WaitForShow();
                //进入试玩/游玩模式初始化显示状态
                //PVPManager.Inst.UpdatePlayerHpShow(Player.Id);
                break;
            case PVPGameMode.Race:
                PVPWaitAreaManager.Inst.SetMeshAndBoxVisible(false, true);
                PlayerManager.Inst.ReturnPVPWaitArea();
                playerCom.ShowPlayerCharater();
                break;
        }

        if (gameMode == GameMode.Guest)
        {
#if UNITY_EDITOR
            UnityLocalTest_InitSyncData();
#else
            RoomChatPanel.Show();
            RoomChatPanel.Instance.SetRecChat(RecChatType.JoinRoom, GameManager.Inst.ugcUserInfo.userName);
#endif
        }
    }

    public void SetPVPPlayerState()
    {

    }

    private void OnClickTarget(Touch touch)
    {
        if (GlobalFieldController.isScreenShoting)
        {
            return;
        }
        if (StateManager.IsFishing)
        {
            return;
        }

        Ray ray = mainCam.ScreenPointToRay(touch.position);
        bool isHit = Physics.Raycast(ray, out RaycastHit hit, 2 * maxCamDist,
            1 << LayerMask.NameToLayer("OtherPlayer")
            | 1 << LayerMask.NameToLayer("Model")
            | 1 << LayerMask.NameToLayer("Touch"));
        if (!isHit)
        {
            //UserProfilePanel.Hide();
            return;
        }

        var hitGo = hit.collider.gameObject;
        // LoggerUtils.Log("hitGo.name=====>" + hitGo.name);

        VideoNodeManager.Inst.OnHitScreen(hitGo);

        var playerInfo = hitGo.GetComponent<LeaderBoardItem>();
        if (playerInfo != null)
        {
            UserProfilePanel.Show();
            UserProfilePanel.Instance.OnOpenPanel(playerInfo.uid);
            return;
        }


        #region 玩家信息面板展示


        var playerData = hitGo.GetComponent<PlayerData>();
        if (!playerData)
        {
            //UserProfilePanel.Hide();
            return;
        }

        var uid = playerData.playerInfo.Id;
        LoggerUtils.Log("TouchPlayerId -- " + uid);
        if (string.IsNullOrEmpty(uid))
        {
            return;
        }
        //if (UserProfilePanel.Instance != null && UserProfilePanel.Instance.gameObject.activeInHierarchy && lastUid == uid)
        //{
        //    return;
        //}

        ////暂存uid
        //lastUid = uid;
        UserProfilePanel.Show();
        UserProfilePanel.Instance.OnOpenPanel(uid);

        #endregion
    }

    #region Unity本地测试

#if UNITY_EDITOR
    private void UnityLocalTest_InitSyncData()
    {
        if (TestNetParams.Inst.CurrentConfig.isOpenNetTest)
        {
            ClientManager.Inst.RetryEnterRoom();
            ClientManager.Inst.InitSyncData();
        }
    }
#endif

    #endregion
}

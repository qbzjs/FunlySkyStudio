using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RoomMenuPanel : BasePanel<RoomMenuPanel>
{
    public Button BtnHide;
    public Button BtnExit;
    public Button BtnResume;
    public Animator loading;
    public GameObject exit;
    public Text RoomInfo;
    public Text DowntownRoomInfo;
    private Transform content;
    public Transform NormalContent;
    public Transform Team1Content;
    public Transform Team2Content;
    public GameObject playerInfoItem;
    public GameObject TeamUsersPanel;
    public GameObject NormalUsersPanel;
    public GameObject NormalRoomMenu;
    public GameObject DowntownRoomMenu;
    public DowntownRoomMenuPanel DowntownRoomMenuPanel;
    private List<Transform> allUserItem=new List<Transform>();
    private SCENE_TYPE curSceneType;

    // 账户类型
    enum AccountClassEnum
    {
        Normal = 0, // 普通账号
        Verified = 1, // 官方认证账号
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        GetEnterMode();
        SetUIEnterMode();
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        UpdatePlayerList();
    }

    #region 根据入口信息控制不同的UI显示 普通模式/大地图
    private void SetUIEnterMode()
    {
        if(GlobalFieldController.IsDowntownEnter)
        {
            NormalRoomMenu.SetActive(false);
            DowntownRoomMenu.SetActive(true);
            DowntownRoomMenuPanel.InitData();
        }
        else
        {
            NormalRoomMenu.SetActive(true);
            DowntownRoomMenu.SetActive(false);
            InitBtn();
            InitTeamInfo();
        }
    }

    private void GetEnterMode()
    {
        curSceneType = (SCENE_TYPE)GameManager.Inst.engineEntry.sceneType;
    }
    #endregion

    #region UI Func
    private void InitBtn()
    {
        BtnHide.onClick.AddListener(HidePanel);
        BtnExit.onClick.AddListener(OnExitBtnClick);
        BtnResume.onClick.AddListener(HidePanel);
    }

    public void OnExitBtnClick()
    {
        if (FPSController.Inst != null)
        {
            var avgFps = FPSController.Inst.GetAverageFPS();
            MobileInterface.Instance.LogEvent(LogEventData.unity_avg_fps, new SavingData.LogEventAvgFpsParam()
            {
                fps = Mathf.FloorToInt(avgFps),
            });
            QualityManager.Inst.SetAvgFps(GlobalFieldController.CurMapInfo?.mapId, avgFps );
            
        }

        Invoke("ExitGame", 0.1f);
        loading.gameObject.SetActive(true);
        exit.gameObject.SetActive(false);
    }

    public void HidePanel()
    {
        Hide();

        if (PlayModePanel.Instance != null)
        {
            PlayModePanel.Instance.gameObject.SetActive(true);
            PlayModePanel.Instance.ShowMenuBtn(true);
        }

        if (PortalPlayPanel.Instance != null)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(true);
        }

        if (CatchPanel.Instance != null)
        {
            CatchPanel.Instance.SetButtonVisible(true);
        }
        if (BaggagePanel.Instance != null)
        {
            BaggagePanel.Instance.gameObject.SetActive(true);
        }
        if (FPSPlayerHpPanel.Instance != null)
        {
            FPSPlayerHpPanel.Instance.SetHpPanelVisible(true);
        }

        if (AttackWeaponCtrlPanel.Instance != null)
        {
            AttackWeaponCtrlPanel.Instance.SetCtrlPanelVisible(true);
        }

        if (ShootWeaponCtrlPanel.Instance != null)
        {
            ShootWeaponCtrlPanel.Instance.SetCtrlPanelVisible(true);
        }

        if (EatOrDrinkCtrPanel.Instance != null)
        {
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(true);
        }

        if (PromoteCtrPanel.Instance && PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            PromoteCtrPanel.Instance.gameObject.SetActive(true);
        }

        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsInStateEmo() && StateEmoPanel.Instance)
        {
            StateEmoPanel.Instance.gameObject.SetActive(true);
        }
        
        if (ParachuteCtrlPanel.Instance && StateManager.IsParachuteGliding)
        {
            ParachuteCtrlPanel.Instance.gameObject.SetActive(true);
        }
        
        if (DayNightSkyboxAnimPanel.Instance && SkyboxManager.Inst.GetCurSkyboxType() == SkyboxType.DayNight)
        {
            DayNightSkyboxAnimPanel.Instance.gameObject.SetActive(true);
        }
        if (FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(true);
        }
        UIControlManager.Inst.CallUIControl("room_menu_exit");
    }

    private void ExitGame()
    {
        DataLogUtils.LogUnityLeaveRoomReq();
        if(Global.Room != null)
        {
            ClientManager.Inst.LeaveRoom();
        }
        MobileInterface.Instance.Quit();
    }
    #endregion

    private void UpdatePlayerList()
    {
        LoggerUtils.Log("UpdatePlayerList");
        if (Global.Room == null)
        {
            return;
        }
        foreach (Transform playerInfoItem in allUserItem)
        {
            Destroy(playerInfoItem.gameObject);     
        }
        allUserItem.Clear();
        InitPlayerInfoList();
    }
    public void InitTeamInfo()
    {
        if (PVPTeamManager.Inst.IsTeamMode())
        {
            SetTeamsPanelState(true);
        }
        else
        {
            SetTeamsPanelState(false);
        }
    }
    //根据分队信息设置父节点
    private Transform GetItemParent(int id)
    {
        if(GlobalFieldController.IsDowntownEnter)
        {
            content = DowntownRoomMenuPanel.downtownContent;
        }
        else
        {
            if (PVPTeamManager.Inst.IsTeamMode())
            {
                content = id == (int)TeamName.TeamA ? Team1Content : Team2Content;
            }
            else
            {
                content = NormalContent;
            }
        }
       return content;
    }
    /// <summary>
    /// 设置不同模式下TeamPanel的显隐
    /// </summary>
    private void SetTeamsPanelState(bool isVisible)
    {
        TeamUsersPanel.SetActive(isVisible);
        NormalUsersPanel.SetActive(!isVisible);
    }
    private void InitPlayerInfoList()
    {
        var playerList = Global.Room.RoomInfo.PlayerList;
        if (playerList == null || playerList.Count <= 0)
        {
            return;
        }
        var curPlayer = Global.Room.RoomInfo.PlayerList.Count;
        var maxPlayer = Global.Room.RoomInfo.MaxPlayers;
        RoomInfo.text = "(" + curPlayer + "/" + maxPlayer + ")";
        DowntownRoomInfo.text = "(" + curPlayer + "/" + maxPlayer + ")";
        for (int idx = 0; idx < curPlayer; ++idx)
        {
            var playerInfo = Global.Room.RoomInfo.PlayerList[idx];
            UserInfo syncPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(playerInfo.Id);
            if (syncPlayerInfo == null)
            {
                continue;
            }
            Transform infoItem = Instantiate(playerInfoItem).transform;
            var parent=GetItemParent((int.Parse(playerInfo.TeamId)));
            infoItem.SetParent(parent);
            allUserItem.Add(infoItem);
            infoItem.localPosition = Vector3.zero;
            infoItem.localRotation = Quaternion.identity;
            infoItem.localScale = Vector3.one;

            Text nick = infoItem.GetComponentInChildren<Text>();
            RawImage profile = infoItem.GetComponentInChildren<RawImage>();
            Button profileBtn = infoItem.GetComponentInChildren<Button>();
            GameObject deathGo = profileBtn.transform.GetChild(0).gameObject;
            Image verifiedIcon = nick.transform.GetComponentInChildren<Image>();
            verifiedIcon.gameObject.SetActive(false);
            var textEllipsis = nick.GetComponent<TextEllipsis>();
            textEllipsis.SetText(syncPlayerInfo.userName);

            // 没有官方认证信息的，默认是 null
            if (syncPlayerInfo.officialCert != null)
            {
                var verified = syncPlayerInfo.officialCert.accountClass;
                if (verified != (int)AccountClassEnum.Normal)
                {
                    verifiedIcon.gameObject.SetActive(true);
                    verifiedIcon.transform.localPosition = new Vector3(nick.preferredWidth + 3, 0, 0);
                }
            }

            bool isDeath = PlayerManager.Inst.GetPlayerDeathState(syncPlayerInfo.uid);
            deathGo.SetActive(isDeath);

            profileBtn.onClick.AddListener(() =>
            {
                OnHeadBtnClick(syncPlayerInfo.uid);
            });

            if (!string.IsNullOrEmpty(syncPlayerInfo.portraitUrl))
            {
                StartCoroutine(LoadSprite(syncPlayerInfo.portraitUrl, profile));
            }
        }
    }

    private void OnHeadBtnClick(string uid)
    {
        PlayerBaseControl.Inst.SetJoystickReset(() => { MobileInterface.Instance.OpenPersonalHomePage(uid); });
    }

    IEnumerator LoadSprite(string url, RawImage image)
    {
        UnityWebRequest wr = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        wr.downloadHandler = texDl;
        yield return wr.SendWebRequest();
        if (!wr.isNetworkError)
        {
            image.texture = texDl.texture;
        }
        texDl.Dispose();
        wr.Dispose();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: 熊昭
/// Description: 视频全屏UI面板
/// Date: 2022-05-05 10:39:47
/// </summary>
public class VideoFullPanel : BasePanel<VideoFullPanel>
{
    public GameObject ctrlPanel;

    public Button linkBackBtn;
    public Button playBtn;
    public Button pauseBtn;
    public Button linkBtn;
    public Button returnBtn;
    public Button smallScnBtn;

    public Button backGroudBtn;
    public Button topAreaBtn;
    public Button centerAreaBtn;
    public Button underAreaBtn;

    private VideoNodeBehaviour vBehav;
    private BGMusicBehaviour bgBehav;
    private bool lockState;
    private bool muteState;
    private float cVolume = 100; //默认Wise总音量100

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        playBtn.onClick.AddListener(OnPlayPauseClick);
        pauseBtn.onClick.AddListener(OnPlayPauseClick);
        linkBtn.onClick.AddListener(OnLinkClick);
        linkBackBtn.onClick.AddListener(OnLinkClick);
        returnBtn.onClick.AddListener(OnSmallScnClick);
        smallScnBtn.onClick.AddListener(OnSmallScnClick);

        backGroudBtn.onClick.AddListener(ShowCtrlPanel);
        topAreaBtn.onClick.AddListener(ShowCtrlPanel);
        centerAreaBtn.onClick.AddListener(CloseCtrlPanel);
        underAreaBtn.onClick.AddListener(ShowCtrlPanel);

        InitGetBGMAudioBehav();
    }

    public override void OnDialogBecameVisible()
    {
        //UIManager.Inst.CloseAllDialog();
        HideOrShowPanel(false);
        SteeringWheelManager.Inst.OnPanelReset();
        //禁用JoyStick操作
        lockState = InputReceiver.locked;
        InputReceiver.locked = true;
    }

    public override void OnBackPressed()
    {
        HideOrShowPanel(true);
        //设置全局声音
        VideoNodeManager.Inst.OnOpenCloseFull(vBehav, false);
        AudioController.Inst.SetBGAudioVolume(cVolume);
        SetBGMAudioMute(muteState);
        //复原JoyStick操作
        InputReceiver.locked = lockState;
    }

    public void SetEntity(SceneEntity entity)
    {
        vBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<VideoNodeBehaviour>();
        cVolume = AudioController.Inst.GetBGAudioVolume();
        muteState = GetBGMAudioMute();
        InitPanel();
        //设置全局声音
        VideoNodeManager.Inst.OnOpenCloseFull(vBehav, true);
        AudioController.Inst.SetBGAudioVolume(0);
        SetBGMAudioMute(true);
    }

    private void InitPanel()
    {
        ShowCtrlPanel();
        RefreshPlayBtn();
    }

    private void InitGetBGMAudioBehav()
    {
        if (SceneBuilder.Inst != null && SceneBuilder.Inst.BGMusicEntity != null)
        {
            var sceneEntity = SceneBuilder.Inst.BGMusicEntity;
            var bindGo = sceneEntity.Get<GameObjectComponent>().bindGo;
            bgBehav = bindGo.GetComponent<BGMusicBehaviour>();
        }
    }

    private bool GetBGMAudioMute()
    {
        bool initState = false;
        if (bgBehav != null)
        {
            initState = bgBehav.GetAudioMuteState();
        }
        return initState;
    }

    private void SetBGMAudioMute(bool isMute)
    {
        if (bgBehav == null)
        {
            LoggerUtils.LogError("SetBGMAudioMute --> bgBehav is null");
            return;
        }
        bgBehav.SetAudioMute(isMute);
    }

    public void HideOrShowPanel(bool Active)
    {
        if (PortalPlayPanel.Instance)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(Active);
        }
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.gameObject.SetActive(Active);
        }
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.BtnPanel.gameObject.SetActive(Active);
        }
        if (AttackWeaponCtrlPanel.Instance && PlayerAttackControl.Inst)
        {
            if (((PlayerOnBoardControl.Inst != null) && PlayerOnBoardControl.Inst.isOnBoard) ||
                ((PlayerSwimControl.Inst != null) && PlayerSwimControl.Inst.isInWater)||
                StateManager.IsOnLadder|| StateManager.IsOnSeesaw || StateManager.IsOnSwing
                ||StateManager.IsOnSlide)
            {
                return;
            }
            if (PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon != null)
            {
                AttackWeaponCtrlPanel.Instance.gameObject.SetActive(Active);
            }
        }
        if (ShootWeaponCtrlPanel.Instance && PlayerShootControl.Inst)
        {
            if (((PlayerOnBoardControl.Inst != null) && PlayerOnBoardControl.Inst.isOnBoard) ||
                ((PlayerSwimControl.Inst != null) && PlayerSwimControl.Inst.isInWater)||
                StateManager.IsOnLadder|| StateManager.IsOnSeesaw || StateManager.IsOnSwing
                || StateManager.IsOnSlide)
            {
                return;
            }
            if (PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
            {
                ShootWeaponCtrlPanel.Instance.gameObject.SetActive(Active);
            }
        }
        if (RoomChatPanel.Instance)
        {
            if (Active)
            {
                RoomChatPanel.Show();
            }
            else
            {
                RoomChatPanel.Hide();
            }
        }
        if (BaggagePanel.Instance)
        {
            BaggagePanel.Instance.gameObject.SetActive(Active);
        }
        if (FPSPlayerHpPanel.Instance)
        {
            FPSPlayerHpPanel.Instance.SetHpPanelVisible(Active);
        }
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsInStateEmo() && StateEmoPanel.Instance)
        {
            StateEmoPanel.Instance.gameObject.SetActive(Active);
        }
        if (EatOrDrinkCtrPanel.Instance != null)
        {
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(Active);
        }
        //TODO:隐藏PVP游戏面板

        if (ParachuteCtrlPanel.Instance && StateManager.IsParachuteGliding)
        {
            ParachuteCtrlPanel.Instance.gameObject.SetActive(Active);
        }

        if (DayNightSkyboxAnimPanel.Instance && SkyboxManager.Inst.GetCurSkyboxType() == SkyboxType.DayNight)
        {
            DayNightSkyboxAnimPanel.Instance.gameObject.SetActive(Active);
        }
        if (FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(Active);
        }
    }

    public void RePanelState()
    {
        if (PortalPlayPanel.Instance)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(true);
        }
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.BtnPanel.gameObject.SetActive(true);
        }
        if (AttackWeaponCtrlPanel.Instance && PlayerAttackControl.Inst)
        {
            if (PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon != null)
            {
                AttackWeaponCtrlPanel.Instance.gameObject.SetActive(true);
            }
        }
        if (ShootWeaponCtrlPanel.Instance && PlayerShootControl.Inst)
        {
            if (PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
            {
                ShootWeaponCtrlPanel.Instance.gameObject.SetActive(true);
            }
        }
        if (FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(true);
        }
    }

    public void RefreshPlayBtn()
    {
        playBtn.gameObject.SetActive(vBehav.GetPauseState());
        pauseBtn.gameObject.SetActive(!vBehav.GetPauseState());
    }

    private void ShowCtrlPanel()
    {
        ctrlPanel.gameObject.SetActive(true);
        linkBackBtn.gameObject.SetActive(false);
        CancelInvoke("CloseCtrlPanel");
        Invoke("CloseCtrlPanel", 3);
    }

    private void CloseCtrlPanel()
    {
        ctrlPanel.gameObject.SetActive(false);
        linkBackBtn.gameObject.SetActive(true);
    }

    private void OnPlayPauseClick()
    {
        vBehav.PlayPauseVideo();
        CancelInvoke("CloseCtrlPanel");
        Invoke("CloseCtrlPanel", 3);
    }

    private void OnSmallScnClick()
    {
        vBehav.ChangeCtrlPanelVisible(true);
        vBehav.SwitchFullScreen(() => { Hide(); });
    }

    private void OnLinkClick()
    {
        vBehav.ShowVideoLinkPage();
    }
}
using System;
using System.Collections;
using BudEngine.NetEngine;
using Cinemachine;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
public class PlayModePanel : BasePanel<PlayModePanel>
{
    public Button EditBtn;
    public Action OnEdit;
    public JoyStick joyStick;
    private Button interactBtn;
    private GameObject playerRole;
    private PlayerBaseControl playerCom;
    private GameObject playerObj;
    private GameObject playerModel;
    private GameObject collectStartPanel;
    private Transform playModeCamCenter;
    public Button jumpBtn;
    private Button setFlyBtn;
    private Button setDownBtn;
    private Button fpsBtn;
    private Button tpsBtn;
    private Button retryBtn;
    private Button selectClothBtn;
    private Button menuBtn;
    private Button shotBtn;
    public Button cameraModeBtn;
    private Button chatBtn;
    public Button emoBtn;
    private ImageToggle audioBtn;
    private ImageToggle micBtn;
    private Button shareBtn;
    private Button jumpOnBoardBtn;

    // 全局设置按钮
    public Button setingBtn;
    private Button unBindBoardBtn;
    // 牵手的人放手 按钮
    private Button releaseHandBtn;
    // 被牵的人放手 按钮
    private Button releaseBtn;
    private Text collectText;
    private GameObject flyModeBtns;
    private Button flyHighBtn;
    private Button flyDownBtn;
    private GameObject walkModeBtns;
    //private GameObject waterModeBtns;
    //private Button jumpInWaterBtn;
    //private Button setSwimBtn;
    //private GameObject swimModeBtns;
    //private Button stopSwimBtn;
    //private Button swimDownBtn;
    //private Button swimHighBtn;
    private GameObject magneticBoardModeBtns;
    private GameObject EmoModeBtns;
    private GameObject steeringWheelModeBtns;
    private EventTrigger steeringWheelForwardTrg;
    private EventTrigger steeringWheelBackwardTrg;
    private EventTrigger steeringWheelLeftTrg;
    private EventTrigger steeringWheelRightTrg;
    private Button steeringWheelGetOffBtn;
    private GameObject seesawModeBtns;
    private Button pushSeesawBtn, leaveSeesawBtn;
    private GameObject swingModeBtns;
    private Button PlaySwingBtn, StopSwingBtn;
    private Toggle footToggle;
    private GameObject footOpen;
    private GameObject footClose;
    // 操作按钮父节点，方便emo列表打开时的显隐
    private Transform OperateBtns;
    // chat 和 emo 的父节点，方便隐藏输入框和 emo 按钮
    private Transform ChatEmoBg;
    [HideInInspector]
    public bool isTps = true;
    private Vector3 initPos = new Vector3(0, 1.3f, 0);
    private Vector3 TpsVec = new Vector3(30, 0, 0);
    private Vector3 FpsVec = new Vector3(0, -2.5f, 0);
    private Vector3 FpsPos = new Vector3(0, 0.114f, -0.09f);
    private Vector3 TpsPos = new Vector3(0, 0.7f, 0);
    private bool CanSetView = true;

    private CinemachineVirtualCamera playModeCam;
    private CinemachineTransposer transposer;
    public WaterModePanel waterPanel;
    public LadderModePanel ladderPanel;
    public SceneRedDotManager mSceneRedDotManager;
    public OperationRedDotManager operationRedDotManager;
    private GameObject maxPlayerPanel;
    private Text maxPLayerText;
    [SerializeField] private GreatSnowfieldPanel greatSnowFieldPanel;
    public enum ButtonType
    {
        JoyStick,
        JumpBtn,
        FlyModeBtn
    }

    [HideInInspector]
    public bool isOpenFootSound = true;

    protected override void Awake()
    {
        base.Awake();
        OnInitByCreate();
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        InitBtn();
        EditBtn.onClick.AddListener(OnEditClick);
        collectStartPanel = transform.Find("Panel/TopLeftGroup/CollectPanel").gameObject;
        collectText = transform.Find("Panel/TopLeftGroup/CollectPanel/CollectText").GetComponent<Text>();
        collectText.transform.parent.gameObject.SetActive(false);
        var playerNode = GameObject.Find("PlayerNode").gameObject;
        playerRole = playerNode.transform.Find("Player/Player").gameObject;
        playerCom = playerNode.transform.Find("Player").GetComponentInChildren<PlayerBaseControl>();
        playModeCamCenter = playerCom.transform.Find("Play Mode Camera Center");
        playerModel = playerCom.playerAnim.gameObject;
        //playModeCam = GameObject.Find("GameStart").GetComponent<GameController>().PlayVirCamera;
        //transposer = playModeCam.GetCinemachineComponent<CinemachineTransposer>();
        //SetFlyButtonVisible();
        shareBtn.gameObject.SetActive(false);
        EmoModeBtns.SetActive(false);
        isOpenFootSound = true;
        waterPanel.InitBtn(playerCom);
        ladderPanel.InitBtn(playerCom);
        mSceneRedDotManager = new SceneRedDotManager(this);
        // SetFlyModeBtn(false);
        //mSceneRedDotManager.ConstructEmoRedDotSystem();
        //mSceneRedDotManager.RequestEmoRedDot();

        //operationRedDotManager = new OperationRedDotManager(this);
        //operationRedDotManager.ConstructOptRedDotSystem();
        //operationRedDotManager.RequestOptRedDot();
        //SpawnPointManager.Inst.SetMaxPlayerPanelAct = SetMaxPlayerPanel;
        //MessageHelper.AddListener(MessageName.OnEnterSnowfield, HandleEnterSnowfield);
        //MessageHelper.AddListener(MessageName.OnLeaveSnowfield, HandleLeaveSnowfield);
    }

    protected override void OnDestroy()
    {
        MessageHelper.RemoveListener(MessageName.OnEnterSnowfield, HandleEnterSnowfield);
        MessageHelper.RemoveListener(MessageName.OnLeaveSnowfield, HandleLeaveSnowfield);
    }

    private void HandleEnterSnowfield()
    {
        fpsBtn.transform.parent.gameObject.SetActive(false);
    }

    private void HandleLeaveSnowfield()
    {
        fpsBtn.transform.parent.gameObject.SetActive(true);
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        OnSetDownClick();
        SetView();
        SetFlyButtonVisible();
        SetWaterBtn();
        SetCollectGameObjActive();
        SetBaggageVisible();
        SetFlyModeBtn(GlobalSettingManager.Inst.GetFlyingMode() == FlyingMode.Original);
    }

    public void SetCollectGameObjActive()
    {
        if (CameraModeManager.Inst.GetCurrentCameraMode() != CameraModeEnum.FreePhotoCamera)
        {
            collectText.transform.parent.gameObject.SetActive(true);
        }
    }

    public void UpdateCollectText(int now, int count)
    {
        if (count <= 0)
        {
            collectText.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            SetCollectGameObjActive();
            collectText.text = $"{now}/{count}";
        }
        if (AudioLoadingPanel.Instance != null)
        {
            AudioLoadingPanel.Instance.SetLoadingPos();
        }
    }

    public bool IsShowCollectTip()
    {
        return collectText.transform.parent.gameObject.activeSelf;
    }



    public void OnEditClick()
    {
        waterPanel.ClearWaterBtn();
        ReferManager.Inst.OnChangeMode(GameMode.Edit);
        OnEdit?.Invoke();
        if (ReferManager.Inst.isReferPlay)
        {
            OnSetDownClick();
            ReferManager.Inst.EnterReferPlay();
            PlayerControlManager.Inst.isPickedProp = false;
        }
        if (!playerCom.isTps)
        {
            OnChangeViewBtnClick();
        }
    }

    private void InitBtn()
    {
        OperateBtns = transform.Find("Panel/OperateBtns").gameObject.transform;
        retryBtn = OperateBtns.Find("TopLeft/RetryBtn").GetComponent<Button>();
        selectClothBtn = OperateBtns.Find("TopLeft/SelectClothBtn").GetComponent<Button>();
        fpsBtn = OperateBtns.Find("TopLeft/ChangeViewButton/FpsBtn").GetComponent<Button>();
        tpsBtn = OperateBtns.Find("TopLeft/ChangeViewButton/TpsBtn").GetComponent<Button>();
        shotBtn = OperateBtns.Find("TopLeft/ScreenShotBtn").GetComponent<Button>();
        cameraModeBtn = OperateBtns.Find("TopLeft/ScreenModeBtn").GetComponent<Button>();
        shareBtn = OperateBtns.Find("TopLeft/ShareBtn").GetComponent<Button>();
        interactBtn = OperateBtns.Find("InteractBtn").GetComponent<Button>();
        footToggle = OperateBtns.Find("FootToggle").GetComponent<Toggle>();
        setingBtn = OperateBtns.Find("TopLeft/SettingBtn").GetComponent<Button>();
        flyModeBtns = OperateBtns.Find("FlyModeBtns").gameObject;
        walkModeBtns = OperateBtns.Find("WalkModeBtns").gameObject;
        releaseBtn = OperateBtns.Find("ReleaseBtn").GetComponent<Button>();
        magneticBoardModeBtns = OperateBtns.Find("MagneticBoardModeBtns").gameObject;
        seesawModeBtns = OperateBtns.Find("SeesawModeBtns").gameObject;
        swingModeBtns = OperateBtns.Find("SwingModeBtns").gameObject;
        EmoModeBtns = OperateBtns.Find("EmoModeBtns").gameObject;
        //waterModeBtns = OperateBtns.Find("WaterModeBtns").gameObject;
        //swimModeBtns = OperateBtns.Find("SwimModeBtns").gameObject;


        //jumpInWaterBtn = waterModeBtns.transform.Find("JumpInWaterBtn").GetComponent<Button>();
        //setSwimBtn  = waterModeBtns.transform.Find("SetSwimBtn").GetComponent<Button>();
        //stopSwimBtn = swimModeBtns.transform.Find("StopSwimBtn").GetComponent<Button>();
        //swimDownBtn = swimModeBtns.transform.Find("SwimDownBtn").GetComponent<Button>();
        //swimHighBtn = swimModeBtns.transform.Find("SwimHighBtn").GetComponent<Button>();
        jumpOnBoardBtn = magneticBoardModeBtns.transform.Find("JumpOnBoardBtn").GetComponent<Button>();
        unBindBoardBtn = magneticBoardModeBtns.transform.Find("UnBindBoardBtn").GetComponent<Button>();
        pushSeesawBtn = seesawModeBtns.transform.Find("PushSeesawBtn").GetComponent<Button>();
        leaveSeesawBtn = seesawModeBtns.transform.Find("LeaveSeesawBtn").GetComponent<Button>();
        PlaySwingBtn = swingModeBtns.transform.Find("PlayBtn").GetComponent<Button>();
        StopSwingBtn = swingModeBtns.transform.Find("StopBtn").GetComponent<Button>();

        releaseHandBtn = EmoModeBtns.transform.Find("ReleaseHandBtn").GetComponent<Button>();
        jumpBtn = walkModeBtns.transform.Find("JumpBtn").GetComponent<Button>();
        setFlyBtn = walkModeBtns.transform.Find("SetFlyBtn").GetComponent<Button>();
        setDownBtn = flyModeBtns.transform.Find("SetDownBtn").GetComponent<Button>();
        flyHighBtn = flyModeBtns.transform.Find("FlyHighBtn").GetComponent<Button>();
        flyDownBtn = flyModeBtns.transform.Find("FlyDownBtn").GetComponent<Button>();
        footOpen = footToggle.transform.Find("SoundOpen").gameObject;
        footClose = footToggle.transform.Find("SoundClose").gameObject;
        menuBtn = transform.Find("Panel/MenuBtn").GetComponent<Button>();
        ChatEmoBg = transform.Find("Panel/ChatEmoBg").transform;
        chatBtn = ChatEmoBg.Find("ChatBtn").GetComponent<Button>();
        emoBtn = ChatEmoBg.Find("EmoBtn").GetComponent<Button>();
        micBtn = ChatEmoBg.Find("MicBtn").GetComponent<ImageToggle>();
        audioBtn = ChatEmoBg.Find("AudioBtn").GetComponent<ImageToggle>();

        footToggle.onValueChanged.AddListener(OnFootToggleClick);
        micBtn.onValueChanged.AddListener(OnMicStatusClick);
        audioBtn.onValueChanged.AddListener(OnAudioStatusClick);
        retryBtn.onClick.AddListener(OnRetryBtnClick);
        selectClothBtn.onClick.AddListener(OnClosetBtnClick);
        tpsBtn.onClick.AddListener(OnChangeViewBtnClick);
        fpsBtn.onClick.AddListener(OnChangeViewBtnClick);
        shotBtn.onClick.AddListener(OnShotBtnClick);
        cameraModeBtn.onClick.AddListener(OnCameraModeBtnBtnClick);
        jumpBtn.onClick.AddListener(OnJumpClick);
        setFlyBtn.onClick.AddListener(OnSetFlyClick);
        setDownBtn.onClick.AddListener(OnSetDownClick);
        menuBtn.onClick.AddListener(OnMenuBtnClick);
        emoBtn.onClick.AddListener(OnEmoBtnClick);
        shareBtn.onClick.AddListener(OnShareBtnClick);
        unBindBoardBtn.onClick.AddListener(UnBindMagneticBoard);
        jumpOnBoardBtn.onClick.AddListener(PlayerJumpOnBoard);
        releaseHandBtn.onClick.AddListener(PlayerRelaseHand);
        releaseBtn.onClick.AddListener(PlayerRelaseHand);
        chatBtn.onClick.AddListener(OnChatBtnClick);
        setingBtn.onClick.AddListener(OnSettingBtnClick);
        pushSeesawBtn.onClick.AddListener(PlayerPushSeesaw);
        leaveSeesawBtn.onClick.AddListener(PlayerLeaveSeesaw);
        PlaySwingBtn.onClick.AddListener(PlaySwing);
        StopSwingBtn.onClick.AddListener(StopSwing);

        steeringWheelModeBtns = transform.Find("Panel/OperateBtns/SteeringWheelModeBtns").gameObject;
        steeringWheelForwardTrg = transform.Find("Panel/OperateBtns/SteeringWheelModeBtns/VLG/forward").GetComponent<EventTrigger>();
        steeringWheelBackwardTrg = transform.Find("Panel/OperateBtns/SteeringWheelModeBtns/VLG/backward").GetComponent<EventTrigger>();
        steeringWheelLeftTrg = transform.Find("Panel/OperateBtns/SteeringWheelModeBtns/HLG/left").GetComponent<EventTrigger>();
        steeringWheelRightTrg = transform.Find("Panel/OperateBtns/SteeringWheelModeBtns/HLG/right").GetComponent<EventTrigger>();
        steeringWheelGetOffBtn = transform.Find("Panel/OperateBtns/SteeringWheelModeBtns/getOff").GetComponent<Button>();
        maxPlayerPanel = transform.Find("Panel/TopLeftGroup/PlayerCountPanel").gameObject;
        maxPLayerText = maxPlayerPanel.transform.Find("CollectText").GetComponent<Text>();
        steeringWheelGetOffBtn.onClick.AddListener(OnGetOffClick);
        var entry = new EventTrigger.Entry() {eventID = EventTriggerType.PointerDown};
        var triggers = steeringWheelForwardTrg.GetComponent<EventTrigger>().triggers;
        entry.callback.AddListener(_=>OnForwardClick(true));
        triggers.Add(entry);
        entry = new EventTrigger.Entry() {eventID = EventTriggerType.PointerUp};
        entry.callback.AddListener(_=>OnForwardClick(false));
        triggers.Add(entry);
        
        entry = new EventTrigger.Entry() {eventID = EventTriggerType.PointerDown};
        triggers = steeringWheelBackwardTrg.GetComponent<EventTrigger>().triggers;
        entry.callback.AddListener(_=>OnBackwardClick(true));
        triggers.Add(entry);
        entry = new EventTrigger.Entry() {eventID = EventTriggerType.PointerUp};
        entry.callback.AddListener(_=>OnBackwardClick(false));
        triggers.Add(entry);
        
        entry = new EventTrigger.Entry() {eventID = EventTriggerType.PointerDown};
        triggers = steeringWheelLeftTrg.GetComponent<EventTrigger>().triggers;
        entry.callback.AddListener(_=>OnLeftClick(true));
        triggers.Add(entry);
        entry = new EventTrigger.Entry() {eventID = EventTriggerType.PointerUp};
        entry.callback.AddListener(_=>OnLeftClick(false));
        triggers.Add(entry);
        
        entry = new EventTrigger.Entry() {eventID = EventTriggerType.PointerDown};
        triggers = steeringWheelRightTrg.GetComponent<EventTrigger>().triggers;
        entry.callback.AddListener(_=>OnRightClick(true));
        triggers.Add(entry);
        entry = new EventTrigger.Entry() {eventID = EventTriggerType.PointerUp};
        entry.callback.AddListener(_=>OnRightClick(false));
        triggers.Add(entry);
    }
    private void OnGetOffClick()
    {
        SteeringWheelManager.Inst.SendGetOffCar();
    }

    private void OnRightClick(bool isDown)
    {
        SteeringWheelManager.Inst.OnRightClick(isDown);
    }

    private void OnLeftClick(bool isDown)
    { 
        SteeringWheelManager.Inst.OnLeftClick(isDown);
    }

    private void OnBackwardClick(bool isDown)
    {
        SteeringWheelManager.Inst.OnBackwardClick(isDown);
    }

    private void OnForwardClick(bool isDown)
    {
        SteeringWheelManager.Inst.OnForwardClick(isDown);
    }

    private void SetRoomChatPanelLater()
    {
        CoroutineManager.Inst.StartCoroutine(OnSetRoomChatPanel());
    }
    public void SetOnLadderMode(bool isOn)
    {
       
        joyStick.gameObject.SetActive(!isOn);
        if (isOn)
        {
            joyStickStatus = joyStick.enabled;
            joyStick.enabled = false;
        }
        else
        {
            joyStick.enabled = joyStickStatus;
        }
        ladderPanel.SetLadderModePanelShow(isOn);

    }
    public void SetOnSlidePipeMode(bool isOn)
    {
        joyStick.gameObject.SetActive(!isOn);
        walkModeBtns.SetActive(!isOn);
        if (isOn)
        {
            joyStickStatus = joyStick.enabled;
            joyStick.enabled = false;
            waterPanel.waterModeBtns.gameObject.SetActive(false);
            waterPanel.swimModeBtns.gameObject.SetActive(false);
            flyModeBtns.gameObject.SetActive(false);
        }
        else
        {
            joyStick.enabled = joyStickStatus;
            SetWaterBtn();
        }
    }
    private IEnumerator OnSetRoomChatPanel()
    {
        yield return new WaitForSeconds(0.2f);
        RoomChatPanel.Instance.ChangeSiblingIndex(transform.GetSiblingIndex() - 1);
    }
    
    public void EntryMode(GameMode gameMode)
    {
        //cameraModeBtn.gameObject.SetActive(true);
        //switch (gameMode)
        //{
        //    case (GameMode.Guest):
        //        EditBtn.gameObject.SetActive(false);
        //        menuBtn.gameObject.SetActive(true);
        //        retryBtn.gameObject.SetActive(true);
        //        chatBtn.gameObject.SetActive(true);
        //        emoBtn.gameObject.SetActive(true);
        //        selectClothBtn.gameObject.SetActive(true);
        //        RoomChatPanel.Show();
        //        SetRoomChatPanelLater();
        //        var bgBehv = SceneBuilder.Inst.BgBehaviour;
        //        if (bgBehv.isLoading)
        //        {
        //            AudioLoadingPanel.Show();
        //        }
        //        shareBtn.gameObject.SetActive(true);
        //        cameraModeBtn.gameObject.SetActive(true);
        //        break;
        //    case (GameMode.Play):
        //        EditBtn.gameObject.SetActive(true);
        //        menuBtn.gameObject.SetActive(false);
        //        retryBtn.gameObject.SetActive(true);
        //        chatBtn.gameObject.SetActive(false);
        //        emoBtn.gameObject.SetActive(false);
        //        shareBtn.gameObject.SetActive(false);
        //        selectClothBtn.gameObject.SetActive(false);
        //        if (retryBtn.gameObject.activeSelf)
        //        {
        //            retryBtn.transform.localPosition = cameraModeBtn.transform.localPosition;
        //        }
        //        cameraModeBtn.gameObject.SetActive(false);
        //        RoomChatPanel.Close();
        //        break;
        //}

        StartPVPGame(gameMode);
        
#if UNITY_EDITOR
        //emoBtn.gameObject.SetActive(true);
        //chatBtn.gameObject.SetActive(true);
        //RoomChatPanel.Show();
        //SetRoomChatPanelLater();
#endif
    }

    private void StartPVPGame(GameMode gameMode)
    {
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null)
        {
            var comp = PVPWaitAreaManager.Inst.PVPBehaviour.entity.Get<PVPWaitAreaComponent>();
            switch ((PVPServerTaskType)comp.gameMode)
            {
                case PVPServerTaskType.Race:
                case PVPServerTaskType.SensorBox:
                    PVPWinConditionGamePlayPanel.Show();
                    PVPWinConditionGamePlayPanel.Instance.EnterGameByMode(gameMode,comp.raceData.pvpTime);
                    break;
                case PVPServerTaskType.Survival:
                    PVPSurvivalGamePlayPanel.Show();
                    PVPSurvivalGamePlayPanel.Instance.EnterGameByMode(gameMode,comp.raceData.pvpTime);
                    break;
            }
        }
    }

    public RectTransform GetButtonRTF(ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ButtonType.JumpBtn:
                return jumpBtn.transform as RectTransform;
            case ButtonType.FlyModeBtn:
                return flyModeBtns.transform as RectTransform;
            case ButtonType.JoyStick:
                return joyStick.transform as RectTransform;
        }
        return null;
    }

    public void EntryPortalMode(bool isGuest)
    {
        EditBtn.gameObject.SetActive(!isGuest);
        menuBtn.gameObject.SetActive(isGuest);
    }
    private void OnMicStatusClick(bool isOn)
    {
        if (!audioBtn.isOn&&isOn)
        {
            OnAudioStatusClick(isOn);
            SetAudioUI(isOn);
        }
        RealTimeTalkManager.Inst.OnMicSwitch(isOn);
    }
    private void OnAudioStatusClick(bool isOn)
    {
        RealTimeTalkManager.Inst.AudioSwitch(isOn);
        if (!isOn)
        {
            RealTimeTalkManager.Inst.MicSwitch(isOn);
            SetMicUI(isOn);  
        }
    }
    public void SetMicUI(bool isOn)
    {
        if (micBtn.gameObject.activeSelf)
        {
            micBtn.isOn = isOn;
            micBtn.SetToggle(isOn);
        }
      
    }
    public void SetAudioUI(bool isOn)
    {
        if (audioBtn.gameObject.activeSelf)
        {
            audioBtn.isOn = isOn;
            audioBtn.SetToggle(isOn);
        }
            
    }
    public void ShowMicAudioUI()
    {
        audioBtn.gameObject.SetActive(true);
        micBtn.gameObject.SetActive(true);
    }
    private void OnJumpClick() {
        if (StateManager.IsOnLadder)
        {
            LadderManager.Inst.PlayerSendDownLadder();
            return;
        }
        playerCom.Jump();
    }
    
    
    private void OnSetFlyClick()
    {
        // 降落伞不允许飞行
        if (StateManager.IsParachuteUsing)
        {
            return;
        }
        if (PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        // 牵手中，不能飞行
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("You could not fly while Hand-in-hand");
            return;
        }
        if (StateManager.IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return;
        }
        if (playerCom.animCon != null && playerCom.animCon.isEating)
        {
            return;
        }
        if (playerCom.animCon != null && (playerCom.animCon.isLooping || playerCom.animCon.isInteracting))
        {
            playerCom.animCon.StopLoop();
            return;
        }

        //带货中-不可交互
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            TipPanel.ShowToast("You could not fly while promoting");
            return;
        }
        if (StateManager.IsOnLadder)
        {
            LadderManager.Inst.ShowTips();
            return;
        }

        if (StateManager.IsFishing)
        {
            return;
        }

        if (SwordManager.Inst.IsSelfInSword())
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return;
        }

        walkModeBtns.SetActive(false);
        flyModeBtns.SetActive(true);
        if (PlayerBaseControl.Inst.isOriginalFlyMode == false)
        {
            playerCom.SetFreeFlyPlayerPos();
        }
        playerCom.SetFly(true);
    }

    private void OnSetDownClick()
    {
        if (PlayerBaseControl.Inst!=null&& PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (playerCom.animCon != null && (playerCom.animCon.isLooping || playerCom.animCon.isInteracting))
        {
            playerCom.animCon.StopLoop();
            return;
        }
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return;
        }
        walkModeBtns.SetActive(true);
        flyModeBtns.SetActive(false);
        playerCom.SetFly(false);
    }
    public void OnSetDownButton(bool isActive)
    {
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }

        if (StateManager.IsParachuteUsing)
        {
            return;
        }
        if(StateManager.IsOnSeesaw)
        {
            return;
        }
        if(StateManager.IsOnSwing)
        {
            return;
        }
        
        walkModeBtns.SetActive(isActive && !(PlayerSwimControl.Inst && PlayerSwimControl.Inst.isInWater));
        flyModeBtns.SetActive(!isActive);
    }

    public void OnFlyHighClickDown()
    {
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return;
        }
        playerCom.FlySpeed(FlyStatus.Up);
        RectTransform rectTransform = flyModeBtns.transform.Find("FlyHighBtn").gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(218, 160);
        rectTransform.anchoredPosition = new Vector2(-109.36f, 237f);
    }

    public void OnFlyDownClickDown()
    {
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return;
        }
        playerCom.FlySpeed(FlyStatus.down);
        RectTransform rectTransform = flyModeBtns.transform.Find("FlyDownBtn").gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(218, 160);
        rectTransform.anchoredPosition = new Vector2(-109.36f, -229f);
    }

    public void OnFlyBtnClickUp()
    {
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return;
        }
        playerCom.FlySpeed(FlyStatus.stop);
        RectTransform flyHighBtnRT = flyModeBtns.transform.Find("FlyHighBtn").gameObject.GetComponent<RectTransform>();
        flyHighBtnRT.sizeDelta = new Vector2(218, 160);
        flyHighBtnRT.anchoredPosition = new Vector2(-109.36f, 206.2f);
        RectTransform flyDownBtnRT = flyModeBtns.transform.Find("FlyDownBtn").gameObject.GetComponent<RectTransform>();
        flyDownBtnRT.sizeDelta = new Vector2(218, 160);
        flyDownBtnRT.anchoredPosition = new Vector2(-109.36f, -201.2f);
    }

    public void OnSwimHighClickDown()
    {
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return;
        }
        PlayerSwimControl.Inst.SwimSpeed(PlayerSwimControl.SwimStatus.Up);
        RectTransform rectTransform = waterPanel.swimModeBtns.transform.Find("SwimHighBtn").gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(218, 160);
        rectTransform.anchoredPosition = new Vector2(-109.36f, 237f);
    }

    public void OnSwimDownClickDown()
    {
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return;
        }
        PlayerSwimControl.Inst.SwimSpeed(PlayerSwimControl.SwimStatus.down);
        RectTransform rectTransform = waterPanel.swimModeBtns.transform.Find("SwimDownBtn").gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(218, 160);
        rectTransform.anchoredPosition = new Vector2(-109.36f, -229f);
    }

    public void OnSwimBtnClickUp()
    {
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return;
        }
        PlayerSwimControl.Inst.SwimSpeed(PlayerSwimControl.SwimStatus.stop);
        RectTransform flyHighBtnRT = waterPanel.swimModeBtns.transform.Find("SwimHighBtn").gameObject.GetComponent<RectTransform>();
        flyHighBtnRT.sizeDelta = new Vector2(218, 160);
        flyHighBtnRT.anchoredPosition = new Vector2(-109.36f, 206.2f);
        RectTransform flyDownBtnRT = waterPanel.swimModeBtns.transform.Find("SwimDownBtn").gameObject.GetComponent<RectTransform>();
        flyDownBtnRT.sizeDelta = new Vector2(218, 160);
        flyDownBtnRT.anchoredPosition = new Vector2(-109.36f, -201.2f);
    }

    public void OnFootToggleClick(bool isToggle)
    {
        if (AudioController.Inst != null)
        {
            AudioController.Inst.CloseAudio(isToggle);
        }
        AKSoundManager.Inst.isOpenFootSound = isToggle;
        isOpenFootSound = isToggle;
        footOpen.gameObject.SetActive(isToggle);
        footClose.gameObject.SetActive(!isToggle);
        if (playerCom.isFlying)
        {
            AudioController.Inst.PlayFlyAudio();
        }

        if (playerCom.isGround && playerCom.isMoving)
        {
            playerCom.PlayFootSound();
        }

        if (PlayerSwimControl.Inst && PlayerSwimControl.Inst.isSwimming
        && PlayerSwimControl.Inst.curSwimSound == SwimSound.Swim)
        {
            PlayerSwimControl.Inst.PlaySwimSound();
        }

        SteeringWheelManager.Inst.SteeringWheelSound(isToggle);
    }

    private void OnRetryBtnClick()
    {
        if (StateManager.IsFishing)
        {
            TipPanel.ShowToast("You could not use this feature in the current state.");
            return;
        }

        if (playerCom.animCon != null && playerCom.animCon.isInteracting)
        {
            return;
        }

        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && (!PVPWaitAreaManager.Inst.IsPVPGameStart||PVPWaitAreaManager.Inst.IsSelfDeath))
        {
            TipPanel.ShowToast("You could not get back to spawn point in Waiting Zone");
            return;
        }
        
        if (StateManager.IsParachuteUsing)
        {
            PlayerParachuteControl.Inst.ForceStopParachute();
        }
        else if (StateManager.IsSnowCubeSkating)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
        if (StateManager.IsOnSlide)
        {
            PlayerSlidePipeControl iCtrl = PlayerControlManager.Inst.GetPlayerCtrlMgrAs<PlayerSlidePipeControl>(PlayerControlType.SlidePipe);
            if (iCtrl != null) iCtrl.ForceAbortSlideAction();
        }
        if (PlayerBaseControl.Inst!=null&&PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        MessageHelper.Broadcast(MessageName.PosMove, false);
        BlackPanel.Show();
        BlackPanel.Instance.PlayTransitionAnimAct(MoveToSpawnPoint);
    }


    public void MoveToSpawnPoint()
    {
        playerCom.SetPosToSpawnPoint();
    }

    private const float ONE_CAM_FOLLOW_OFFSET = 0.1f;
    private const float THIRD_CAM_FOLLOW_OFFSET = -7f;
    private void SetView()
    {
        if (isTps == playerCom.isTps || playerCom.animCon.isPlaying)
        {
            return;
        }
        playerCom.IsTps = isTps;
        Quaternion modleEuler = playerModel.transform.rotation;
        if (!isTps) {
            CanSetView = false;
            PlayModeHandler.fpvRotMax = 20;
            playerCom.SetPlayerHeadVisible(false);
            playModeCamCenter.localPosition = FpsPos;
            playModeCam.AddCinemachineComponent<CinemachineHardLockToTarget>();
            DOTween.To(() => transposer.m_FollowOffset.z, x => transposer.m_FollowOffset.z = x, ONE_CAM_FOLLOW_OFFSET, 0.3f).onComplete += () =>
            {
                CanSetView = true;
                CameraModeBtnControl();
                MessageHelper.Broadcast(MessageName.ChangeTps);
            };
            playModeCamCenter.DOLocalRotate(FpsVec, 0.3f);
            playerCom.playerAnim.transform.rotation = new Quaternion(0, 0, 0, 0);
            if (!PlayerBaseControl.Inst.isOriginalFlyMode && PlayerBaseControl.Inst.isFlying)
            {
                modleEuler.eulerAngles = Vector3.zero;
            }
            playerCom.transform.DORotateQuaternion(modleEuler, 0.3f).onComplete += () =>
            {
                MagneticBoardManager.Inst.SetView(modleEuler);
                if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
                {
                    SteeringWheelManager.Inst.ResetPlayerLookSteering();
                }
            };
            if (StateManager.IsOnSwing)
            {
                SwingManager.Inst.SetView();
            }
        }
        else
        {
            PlayModeHandler.fpvRotMax = 60;
            CanSetView = false;
            playerCom.SetPlayerHeadVisible(true);
            playModeCamCenter.localPosition = TpsPos;
            transposer = playModeCam.AddCinemachineComponent<CinemachineTransposer>();
            transposer.m_BindingMode = CinemachineTransposer.BindingMode.LockToTarget;
            transposer.m_XDamping = 0;
            transposer.m_YDamping = 0;
            transposer.m_ZDamping = 0;
            DOTween.To(() => transposer.m_FollowOffset.z, x => transposer.m_FollowOffset.z = x, THIRD_CAM_FOLLOW_OFFSET, 0.1f).onComplete += () =>
            {
                MessageHelper.Broadcast(MessageName.ChangeTps);
                CanSetView = true;
                CameraModeBtnControl();
            };
            // playerModel.transform.rotation = playerCom.transform.rotation;
            playModeCamCenter.DOLocalRotate(TpsVec, 0.3f);

            if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
            {
                SteeringWheelManager.Inst.ResetPlayerLookSteering();
            }

            if (StateManager.IsOnSeesaw)
            {
                SeesawManager.Inst.SetView();
            }
            if (StateManager.IsOnSwing)
            {
                SwingManager.Inst.ReSetView();
            }
        }
        PlayerControlManager.Inst.SetPlayerActive(isTps);

        PlayerBaseControl.Inst.SetPlayerRoleActive();

        playerCom.UpdateAnimatorCon();
    }

    public void SetFlyButtonVisibleByPortal()
    {
        OnSetDownClick();
        SetFlyButtonVisible();
    }

    private void SetFlyButtonVisible()
    {
        if (SceneBuilder.Inst.CanFlyEntity.Get<CanFlyComponent>().canFly == 0) 
        {
            setFlyBtn.gameObject.SetActive(true);
        }
        else
        {
            setFlyBtn.gameObject.SetActive(false);
        }
    }

    private void SetBaggageVisible()
    {
        if(SceneParser.Inst.GetBaggageSet() == 1)
        {
            BaggagePanel.Show();
        }
        else
        {
            BaggagePanel.Hide();
        }
    }

    public void OnChangeViewBtnClick()
    {
        // 牵手中，不能切换视角
        if (StateManager.Inst.IsHodingFishingRod() && isTps)
        {
            TipPanel.ShowToast("You could not switch view in the current state");
            return;
        }
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("You can not switch view during hand-in-hand");
            return;
        }
        if (!isTps&&PlayerShootControl.Inst && PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
        {
            TipPanel.ShowToast("You could not switch view in shooting mode");
            return;
        }
        // 降落伞使用中不允许切视角
        if (StateManager.IsParachuteUsing)
        {  
            TipPanel.ShowToast("You could not switch view in the current state");
            return;
        }
        if (StateManager.IsOnLadder)
        {
            LadderManager.Inst.ForceSetRot();
        }
        if (playerCom.animCon != null && playerCom.animCon.isLooping)
        {
            playerCom.animCon.StopLoop();
            return;
        }
        if (CanSetView == false || playerCom.animCon.isPlaying || playerCom.animCon.isFishing)
        {
            return;
        }
        isTps = !isTps;
        GlobalSettingManager.Inst.SyncGameView(isTps ? GameView.ThirdPerson : GameView.FirstPerson);
        fpsBtn.gameObject.SetActive(!isTps);
        tpsBtn.gameObject.SetActive(isTps);
        ShowFpsPlayerHpPanel(true);
        SetView();
        
    }

    private void OnMenuBtnClick() {
        RoomMenuPanel.Show();
        ShowMenuBtn(false);
        HideSomePanel();
    }

    private void OnSettingBtnClick()
    {
        GlobalSettingPanel.Show();
        HideSomePanel();
        if (RoomChatPanel.Instance != null)
        {
            RoomChatPanel.Instance.gameObject.SetActive(false);
        }
        
    }
   
    /**
    * 打开某些 Panel（如房间列表、全局设定）需要隐藏一些界面，让界面更简洁
    */
    private void HideSomePanel()
    {
        if (PlayModePanel.Instance != null)
        {
            PlayModePanel.Instance.gameObject.SetActive(false);
        }
        if (PortalPlayPanel.Instance != null)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(false);
        }
        if (CatchPanel.Instance != null)
        {
            CatchPanel.Instance.SetButtonVisible(false);
        }
        if (BaggagePanel.Instance != null)
        {
            BaggagePanel.Instance.gameObject.SetActive(false);
        }

        if (FPSPlayerHpPanel.Instance)
        {
            FPSPlayerHpPanel.Instance.SetHpPanelVisible(false);
        }

        if (AttackWeaponCtrlPanel.Instance != null)
        {
            AttackWeaponCtrlPanel.Instance.SetCtrlPanelVisible(false);
        }

        if (ShootWeaponCtrlPanel.Instance != null)
        {
            ShootWeaponCtrlPanel.Instance.SetCtrlPanelVisible(false);
        }

        if (EatOrDrinkCtrPanel.Instance != null)
        {
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(false);
        }

        if (PromoteCtrPanel.Instance)
        {
            PromoteCtrPanel.Instance.gameObject.SetActive(false);
        }

        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsInStateEmo() && StateEmoPanel.Instance)
        {
            StateEmoPanel.Instance.gameObject.SetActive(false);
        }

        if (ParachuteCtrlPanel.Instance && StateManager.IsParachuteGliding)
        {
            ParachuteCtrlPanel.Instance.gameObject.SetActive(false);
        }
        
        if (DayNightSkyboxAnimPanel.Instance && SkyboxManager.Inst.GetCurSkyboxType() == SkyboxType.DayNight)
        {
            DayNightSkyboxAnimPanel.Instance.gameObject.SetActive(false);
        }

        if (FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(false);
        }

        UIControlManager.Inst.CallUIControl("play_mode_panel_exit");
    }

    private void OnShotBtnClick()
    {
        DataLogUtils.LogTakePhoto("shot_start", (int)PhotoType.QuickMode);
        StartCoroutine(ShotAnimation());
        AudioController.Inst.PlayShotAudio();

        var bytes = ScreenShotUtils.ScreenShot(GameManager.Inst.MainCamera, new Rect(0, 0, Screen.width, Screen.height), true);
        if (bytes.Length == 0)
        {
            OnFail();
            return;
        }

        string fileName = DataUtils.SaveImg(bytes);
        PhotoBusData photoBusData = new PhotoBusData();
        if (GlobalFieldController.IsDowntownEnter)
        {
            photoBusData = new PhotoBusData()
            {
                downtownId = GameManager.Inst.gameMapInfo.mapId,
                downtownName = GameManager.Inst.gameMapInfo.mapName,
                downtownDesc = GameManager.Inst.gameMapInfo.mapDesc,
                downtownCover = GameManager.Inst.gameMapInfo.mapCover
            };
        }
        else
        {
            photoBusData = new PhotoBusData() { ugcId = GameManager.Inst.gameMapInfo.mapId };
        }
        string busData = JsonConvert.SerializeObject(photoBusData);
        LoggerUtils.Log("OnShotBtnClick -- PhotoBusData : " + busData);
        AWSUtill.UpLoadToAlbum(fileName, busData, OnSuccess, OnFail, (int)PhotoType.QuickMode);
    }

    //进入第一人称，相机模式按钮置灰
    public void CameraModeBtnControl()
    {
        var playerIsTps = PlayerBaseControl.Inst && PlayerBaseControl.Inst.isTps;
        var alpha = 1f;
        if (!playerIsTps)
        {
            alpha = 0.4f;
        }
        cameraModeBtn.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
    }

    private void OnCameraModeBtnBtnClick()
    {
        if (PickabilityManager.Inst != null && PickabilityManager.Inst.isSelfPicking)
        {
            return;
        }
        //第一人称不允许进入相机模式
        if (PlayerBaseControl.Inst && !PlayerBaseControl.Inst.isTps)
        {
            TipPanel.ShowToast("Please change game view to third-person before entering camera mode.");
            return;
        }
        if (CanSetView == false)
        {
            return;
        }
        if (PlayerShootControl.Inst && PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
        {
            return;
        }

        if (PortalGateAnimPanel.Instance && PortalGateAnimPanel.Instance.gameObject.activeInHierarchy)
        {
            return;
        }
        if (StateManager.IsSnowCubeSkating)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
        CameraModeManager.Inst.EnterMode(CameraModeEnum.FreePhotoCamera);
    }

    private IEnumerator ShotAnimation()
    {
        BlackPanel.Show();
        RawImage image = BlackPanel.Instance.GetComponent<RawImage>();
        Image blackImage = BlackPanel.Instance.BlackImage;
        GameObject black = BlackPanel.Instance.Black;

        image.CrossFadeAlpha(0, 0, false);
        image.color = new Color(1, 1, 1, 1);
        blackImage.color = new Color(1, 1, 1, 0);
        black.SetActive(false);

        blackImage.DOFade(1, 0.4f).SetEase(Ease.InExpo).onComplete += () =>
        {
            blackImage.DOFade(0, 0.4f).SetEase(Ease.OutExpo).onComplete += () =>
            {
                StartCoroutine(WaitShotStop(image));
            };
        };

        yield return new WaitForEndOfFrame();
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();
        image.texture = screenShot;
        image.CrossFadeAlpha(1, 0, false);
    }

    IEnumerator WaitShotStop(RawImage image)
    {
        yield return new WaitForSeconds(0.5f);
        Object.Destroy(image.texture);
        image.texture = null;
        image.CrossFadeAlpha(0, 0, false);
        BlackPanel.Hide();
    }

    private void OnSuccess(string content = "")
    {
        TipPanel.ShowToast("Added photo to album!");
    }

    private void OnFail(string content = "")
    {
        DataLogUtils.LogTakePhoto("shot_fail", (int)PhotoType.QuickMode);
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }



    private void OnExitBtnClick()
    {
        //StartCoroutine("ExitGame");
        RoomMenuPanel.Show();
        ShowMenuBtn(false);
    }

    public void ShowMenuBtn(bool isActive)
    {
        //StartCoroutine("ExitGame");
        menuBtn.gameObject.SetActive(isActive);
    }

    public void EmoMenuPanelBecameVisible(bool isActive)
    {
        OperateBtns.gameObject.SetActive(!isActive);
        if (CatchPanel.Instance != null)
        {
            CatchPanel.Instance.SetButtonVisible(!isActive);
        }
        if (AttackWeaponCtrlPanel.Instance &&  PlayerAttackControl.Inst)
        {
            if (PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon != null)
            {
                AttackWeaponCtrlPanel.Instance.gameObject.SetActive(!isActive);
            }
            if (PlayerSwimControl.Inst && PlayerSwimControl.Inst.isInWater)
            {
                AttackWeaponCtrlPanel.Instance.gameObject.SetActive(false);
            }
            if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
            {
                AttackWeaponCtrlPanel.Instance.gameObject.SetActive(false);
            }
        }
        if (ShootWeaponCtrlPanel.Instance && PlayerShootControl.Inst)
        {
            if (PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
            {
                ShootWeaponCtrlPanel.Instance.gameObject.SetActive(!isActive);
            }
            if (PlayerSwimControl.Inst && PlayerSwimControl.Inst.isInWater)
            {
                ShootWeaponCtrlPanel.Instance.gameObject.SetActive(false);
            }
            if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
            {
                ShootWeaponCtrlPanel.Instance.gameObject.SetActive(false);
            }
        }

        if (CameraModePanel.Instance)
        {
            CameraModePanel.Instance.OnEmoPanelShow(!isActive);
        }
        if (BaggagePanel.Instance&& CameraModeManager.Inst.GetCurrentCameraMode() != CameraModeEnum.FreePhotoCamera)
        {
            BaggagePanel.Instance.gameObject.SetActive(!isActive);
        }

        if (FPSPlayerHpPanel.Instance)
        {
            FPSPlayerHpPanel.Instance.SetHpPanelVisible(!isActive);
        }

        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsInStateEmo() && StateEmoPanel.Instance)
        {
            StateEmoPanel.Instance.gameObject.SetActive(!isActive);
        }

        if (EatOrDrinkCtrPanel.Instance != null)
        {
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(!isActive);
        }

        if (PromoteCtrPanel.Instance && PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            PromoteCtrPanel.Instance.gameObject.SetActive(!isActive);
        }
        
        if (ParachuteCtrlPanel.Instance && StateManager.IsParachuteGliding)
        {
            ParachuteCtrlPanel.Instance.gameObject.SetActive(!isActive);
        }
        if (FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(!isActive);
        }
        if (isActive)
        {
            UIControlManager.Inst.CallUIControl("emo_menu_enter");
        }
        else
        {
            UIControlManager.Inst.CallUIControl("emo_menu_exit");
        }
    }

    public void SetChatEmoVisible(bool visible)
    {
        ChatEmoBg.gameObject.SetActive(visible);
    }

    public void SetInteractBtnAct(UnityAction action) {
        interactBtn.onClick.RemoveAllListeners();
        interactBtn.onClick.AddListener(action);
    }

    public void OnChatBtnClick()
    {
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = LocalizationConManager.Inst.GetLocalizedText("Enter text..."),
            inputMode = 2,
            maxLength = 60,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = "",
            returnKeyType = (int)ReturnType.Send
        };
        if (StateManager.IsSnowCubeSkating)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
        SetKeyBoardStatus(true);
        MobileInterface.Instance.AddClientRespose(MobileInterface.hideKeyboard, OnHideKeyBoard);
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowKeyBoard);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }

    private void OnEmoBtnClick()
    {
        if (StateManager.IsOnLadder)
        {
            LadderManager.Inst.ShowTips();
            return;
        }

        if (StateManager.IsOnSeesaw)
        {
            SeesawManager.Inst.ShowSeesawMutexToast();
            return ;
        }
        if (StateManager.IsOnSwing)
        {
            SwingManager.Inst.ShowSwingMutexToast();
            return ;
        }
        if (StateManager.IsFishing)
        {
            TipPanel.ShowToast("You could not send emote in the current state.");
			return;
		}
        if (StateManager.IsOnSlide)
        {
            TipPanel.ShowToast("Please quit slider first.");
            return;
        }
        EmoMenuPanel.Show();
        EmoMenuPanelBecameVisible(true);
    }

    private void SetKeyBoardStatus(bool status)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            var roomChatData = new RoomChatData()
            {
                msgType = (int)RecChatType.Custom,
                data = JsonConvert.SerializeObject(new RoomChatCustomData()
                {
                    type = (int)ChatCustomType.Keyboard,
                    data = status ? "1" : "0"
                })
            };
            ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
        }
        // 降落伞使用中不显示打字动画
        if (StateManager.IsParachuteUsing||
            StateManager.IsOnSeesaw||
            StateManager.IsOnSwing||
            StateManager.IsOnLadder||
            StateManager.IsFishing ||
            StateManager.IsOnSlide)
        {
            return;
        }
        MessageHelper.Broadcast(MessageName.TypeData, new AnimationController.TypeStatusData
        {
            isStart = status,
            playerId = Player.Id
        });

 
    }



    public void OnHideKeyBoard(string str)
    {
        SetKeyBoardStatus(false);
    }


    public void ShowKeyBoard(string str)
    {
    
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        RoomChatData roomchatdata = new RoomChatData()
        {
            msgType = (int)RecChatType.TextChat,
            data = str
        };

        var textChatBev = PlayerEmojiControl.Inst.textCharBev;

        if(textChatBev != null)
        {
            textChatBev.ResizeChatAxis(str);
            RoomChatPanel.Instance.SetRecChat(RecChatType.TextChat, GameManager.Inst.ugcUserInfo.userName,str);
            ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomchatdata));
        }
    }

    private void OnClosetBtnClick()
    {
        if (!CanOpenCloset())
        {
            return;
        }
        if (StateManager.IsSnowCubeSkating)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
        //显示UI主面板(初始化界面)
        UIControlManager.Inst.CallUIControl("choose_cloth_enter");
        ChooseClothPanel.Show();
        //发起循环动作-->照镜子
        PlayerControlManager.Inst.PlayMove((int)EmoName.EMO_LOOK_MIRROR);
    }

    private bool CanOpenCloset()
    {
        //双人交互进行时点击换装无效
        if (playerCom.animCon != null && playerCom.animCon.isInteracting)
        {
            return false;
        }
        //换装动画播放过程中点击换装无效
        if (playerCom.animCon != null && playerCom.animCon.IsChanging)
        {
            return false;
        }
        //牵手状态点击换装无效
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("You could not change your outfit while using interactive emotes.");
            return false;
        }
        //降落伞使用期间无法打开
        if (StateManager.IsParachuteUsing)
        {
            return false;
        }
        //钓鱼中无法相应
        if (StateManager.IsFishing)
        {
            return false;
        }
        if (StateManager.IsOnLadder)
        {
            LadderManager.Inst.ShowTips();
            return false;
        }

        if (StateManager.IsOnSeesaw)
        {
            SeesawManager.Inst.ShowSeesawMutexToast();
            return false;
        }
        
        if (StateManager.IsOnSwing)
        {
            SwingManager.Inst.ShowSwingMutexToast();
            return false;
        }
        
        if (StateManager.IsOnSlide)
        {
            return false;
        }

        if (StateManager.IsSelfPromoting()) {
            TipPanel.ShowToast("You can not change outfit while promoting");
            return false;
        }

        return true;
    }

    private void OnShareBtnClick()
    {
        if (string.IsNullOrEmpty(ClientManager.Inst.roomCode))
        {
            return;
        }
        if (GlobalFieldController.IsDowntownEnter)
        {
            DowntownNativeShareParams downtownNativeShareParams = new DowntownNativeShareParams()
            {
                roomCode = ClientManager.Inst.roomCode,
                downtownId = GameManager.Inst.gameMapInfo.mapId,
                downtownCover = GameManager.Inst.gameMapInfo.mapCover,
                downtownDesc = GameManager.Inst.gameMapInfo.mapDesc,
                downtownName = GameManager.Inst.gameMapInfo.mapName,
                downtownPngPrefix = GameManager.Inst.downtownInfo.downtownPngPrefix
            };
            MobileInterface.Instance.OpenNativeShareDialog(JsonConvert.SerializeObject(downtownNativeShareParams));
        }
        else
        {
            NativeShareParams nativeShareParams = new NativeShareParams()
            {
                roomCode = ClientManager.Inst.roomCode,
                mapId = GameManager.Inst.gameMapInfo.mapId,
                creatorIcon = GameManager.Inst.gameMapInfo.mapCreator.portraitUrl,
                creatorName = GameManager.Inst.gameMapInfo.mapCreator.userName,
                creatorUid = GameManager.Inst.gameMapInfo.mapCreator.uid,
                mapCover = GameManager.Inst.gameMapInfo.mapCover,
                mapName = GameManager.Inst.gameMapInfo.mapName,
                mapDesc = GameManager.Inst.gameMapInfo.mapDesc
            };
            MobileInterface.Instance.OpenNativeShareDialog(JsonConvert.SerializeObject(nativeShareParams));
        }
    }

    private IEnumerator ExitGame()
    {
        yield return new WaitForSeconds(0.1f);
        ClientManager.Inst.LeaveRoom();
    }
    public void PlayerJumpOnBoard()
    {
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            TipPanel.ShowToast("You could not jump while promoting");
            return;
        }
        MagneticBoardManager.Inst.SendPlayerJumpOnBoard();
    }
    public void UnBindMagneticBoard()
    {
        MagneticBoardManager.Inst.PlayerSendDownBoard();

    }

    public void PlayerPushSeesaw()
    {
        SeesawManager.Inst.SendPlayerPushDownSeeSaw();
    }
    public void PlayerLeaveSeesaw()
    {
        SeesawManager.Inst.PlayerSendLeaveSeesaw();
    }
    
    public void PlaySwing()
    {
        // PlaySwingBtn.interactable = false;
        SwingManager.Inst.PlayerSendPlay();
    }
    public void StopSwing()
    {
        SwingManager.Inst.PlayerSendStop();
    }

    public void SwingButtonReset()
    {
        PlaySwingBtn.interactable = true;
    }

    public void PlayerRelaseHand()
    {
        if (StateManager.IsSnowCubeSkating)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
        PlayerBaseControl.Inst.animCon.ReleaseHand();
        PlayerBaseControl.Inst.animCon.StopLoop();
        if (PlayerMutualControl.Inst)
        {
            PlayerMutualControl.Inst.EndMutual();
        }
    }

    /**
    * 玩家处于牵手状态时，被牵玩家隐藏表情按钮、跳跃/飞行按钮、隐藏回到出生点按钮
    * 主动牵手玩家显示解除牵手按钮
    */
    public void RefreshBtns()
    {
        bool isHoldingHands = PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual;
        bool isStartPlayer = PlayerMutualControl.Inst && PlayerMutualControl.Inst.isStartPlayer;

        // 牵手中，切换视角按钮会置灰
        float alpha = 1;
        if (isHoldingHands)
        {
            alpha = 0.4f;
            EmoModeBtns.gameObject.SetActive(isStartPlayer);
            emoBtn.gameObject.SetActive(isStartPlayer);
            walkModeBtns.gameObject.SetActive(isStartPlayer);
            if (CameraModeManager.Inst == null || CameraModeManager.Inst.GetCurrentCameraMode() != CameraModeEnum.FreePhotoCamera)
            {
                retryBtn.gameObject.SetActive(isStartPlayer);
            }
        }
        else
        {
            EmoModeBtns.gameObject.SetActive(false);
            emoBtn.gameObject.SetActive(true);
            walkModeBtns.gameObject.SetActive(!playerCom.isFlying
            && (!PlayerSwimControl.Inst || !PlayerSwimControl.Inst.isInWater));
            if (CameraModeManager.Inst == null || CameraModeManager.Inst.GetCurrentCameraMode() != CameraModeEnum.FreePhotoCamera)
            {
                retryBtn.gameObject.SetActive(true);
            }
            
        }
        fpsBtn.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
        tpsBtn.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
    }

    /**
    * 牵手动作被牵者显示放手按钮
    * 一秒之后隐藏放手按钮
    */
    public void ShowReleaseBtn()
    {
        if (!releaseBtn.gameObject.activeSelf)
        {
            releaseBtn.gameObject.SetActive(true);
            Invoke("HideReleaseBtn", 1);
        }

    }

    public void HideReleaseBtn()
    {
        releaseBtn.gameObject.SetActive(false);
    }
    private bool walkbtnsStatus = true;
    private bool flybtnsStatus;
    public void SetOnBoard(bool isOnBoard)
    {
        if (isOnBoard)
        {
            walkbtnsStatus = walkModeBtns.activeSelf;
            flybtnsStatus = flyModeBtns.activeSelf;
            walkModeBtns.SetActive(!isOnBoard);
            flyModeBtns.SetActive(!isOnBoard);
        }
        else
        {
            walkModeBtns.SetActive(walkbtnsStatus);
            flyModeBtns.SetActive(flybtnsStatus);
        }
       
        magneticBoardModeBtns.SetActive(isOnBoard);

        if (RoomChatPanel.Instance)
        {
            RoomChatPanel.Instance.ChangeSiblingIndex(transform.GetSiblingIndex() - 1);
        }
    }

    public void SetOnSeesaw(bool isOnSeesaw)
    {
        if (isOnSeesaw)
        {
            walkbtnsStatus = walkModeBtns.activeSelf;
            flybtnsStatus = flyModeBtns.activeSelf;
            walkModeBtns.SetActive(!isOnSeesaw);
            flyModeBtns.SetActive(!isOnSeesaw);
        }
        else
        {
            walkModeBtns.SetActive(walkbtnsStatus);
            flyModeBtns.SetActive(flybtnsStatus);
        }

        seesawModeBtns.SetActive(isOnSeesaw);

        if (RoomChatPanel.Instance)
        {
            RoomChatPanel.Instance.ChangeSiblingIndex(transform.GetSiblingIndex() - 1);
        }

        if (FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(!isOnSeesaw);
        }
    }
    
    public void SetOnSwing(bool isOnSwing)
    {
        if (isOnSwing)
        {
            walkbtnsStatus = walkModeBtns.activeSelf;
            flybtnsStatus = flyModeBtns.activeSelf;
            walkModeBtns.SetActive(!isOnSwing);
            flyModeBtns.SetActive(!isOnSwing);
        }
        else
        {
            walkModeBtns.SetActive(walkbtnsStatus);
            flyModeBtns.SetActive(flybtnsStatus);
        }

        swingModeBtns.SetActive(isOnSwing);

        if (RoomChatPanel.Instance)
        {
            RoomChatPanel.Instance.ChangeSiblingIndex(transform.GetSiblingIndex() - 1);
        }

        if (FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(!isOnSwing);
        }
    }

    private bool joyStickStatus;

    public void Driving(bool isDriving)
    {
        if (isDriving)
        {
            walkbtnsStatus = walkModeBtns.activeSelf;
            flybtnsStatus = flyModeBtns.activeSelf;
            joyStickStatus = joyStick.enabled;
            joyStick.gameObject.SetActive(!isDriving);
            joyStick.enabled = false;
            walkModeBtns.SetActive(false);
            flyModeBtns.SetActive(false);
        }
        else
        {
            joyStick.gameObject.SetActive(joyStickStatus);
            joyStick.enabled = joyStickStatus;
            walkModeBtns.SetActive(walkbtnsStatus);
            flyModeBtns.SetActive(flybtnsStatus);
        }
        steeringWheelModeBtns.SetActive(isDriving);
        if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            emoBtn.gameObject.SetActive(!isDriving);
            if (steeringWheelModeBtns.activeSelf)
            {
                RoomChatPanel.Instance.SetChatFormActive(false);
                RoomChatPanel.Instance.ChangeSiblingIndex(transform.GetSiblingIndex() - 1);
            }
        }

    }

    public void InitSetOnParachute()
    {
        if (walkModeBtns)
        {
            walkbtnsStatus = walkModeBtns.activeSelf;
        }

        if (flyModeBtns)
        {
            flybtnsStatus = flyModeBtns.activeSelf;
        }
    }
    
    public void SetOnParachute()
    {
        if (StateManager.IsParachuteUsing)
        {
            walkbtnsStatus = walkModeBtns.activeSelf;
            flybtnsStatus = flyModeBtns.activeSelf;
            walkModeBtns.SetActive(false);
            flyModeBtns.SetActive(false);
        }
        else
        {
            walkModeBtns.SetActive(walkbtnsStatus);
            flyModeBtns.SetActive(flybtnsStatus);
        }
        if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            emoBtn.gameObject.SetActive(!StateManager.IsParachuteUsing);
        }
        
        SetBtnGray(selectClothBtn, StateManager.IsParachuteUsing);
    }

    public void SetBtnGray(Button btn, bool isGray)
    {
        if (btn != null && btn.GetComponent<Image>() != null)
        {
            btn.GetComponent<Image>().color = new Color(1, 1, 1, isGray? 0.4f : 1f);
        }
    }
    
    public void SetFlyModeBtn(bool isActive)
    {
        flyHighBtn.gameObject.SetActive(isActive);
        flyDownBtn.gameObject.SetActive(isActive);
    }

    public void SetInWaterBtn(bool isActive)
    {
        waterPanel.waterModeBtns.gameObject.SetActive(isActive);
        walkModeBtns.gameObject.SetActive(!isActive);
        flyModeBtns.gameObject.SetActive(false);
        waterPanel.swimModeBtns.gameObject.SetActive(false);
    }
    
    public void SetWaterBtn()
    {
        if (PlayerSwimControl.Inst != null && PlayerSwimControl.Inst.isInWater)
        {
            waterPanel.waterModeBtns.gameObject.SetActive(true);
            waterPanel.swimModeBtns.gameObject.SetActive(false);
            flyModeBtns.gameObject.SetActive(false);
            walkModeBtns.gameObject.SetActive(false);
        }

    }

    public void PVPUIswitch()
    {
        joyStick.gameObject.SetActive(!joyStick.gameObject.activeSelf);
        joyStick.enabled = !joyStick.enabled;
        ChatEmoBg.gameObject.SetActive(!ChatEmoBg.gameObject.activeSelf);
        OperateBtns.gameObject.SetActive(!OperateBtns.gameObject.activeSelf);
	}
    public void SetFlyBtnActive(bool isActive)
    {
        var playerSwimCtrl = PlayerControlManager.Inst.playerControlNode.GetComponent<PlayerSwimControl>();
        if (SceneBuilder.Inst.CanFlyEntity.Get<CanFlyComponent>().canFly == 0 && playerSwimCtrl == null)
        {
            setFlyBtn.gameObject.SetActive(isActive);
            OnSetDownClick();
            if (isActive)
            {
                PlayerBaseControl.Inst.SetBagPlayerRoleActive();
            }
        }
    }

    public void LookFpsMode(bool isLook)
    {
        float alphaValue = isLook == true ? 0.6f : 1f;
        fpsBtn.image.color = new Color(1,1,1, alphaValue);
    }

    /// <summary>
    /// 控制玩家自己血条的显示
    /// </summary>
    /// <param name="isVisiable"></param>
    public void ShowFpsPlayerHpPanel(bool isVisiable)
    {
        if (SceneParser.Inst.GetHPSet() == 0)
        {
            return;
        }
        var charCtr = playerCom.GetComponent<CharBattleControl>();
        if (charCtr != null)
        {
            if (!isVisiable)
            {
                FPSPlayerHpPanel.Hide();
            }
            if (isVisiable && charCtr.GetBloodBarVisiable() && GlobalFieldController.CurGameMode != GameMode.Edit)
            {
                FPSPlayerHpPanel.Show();
                if ((GlobalSettingPanel.Instance && GlobalSettingPanel.Instance.gameObject.activeInHierarchy)
                || (CameraModePanel.Instance && CameraModePanel.Instance.gameObject.activeInHierarchy)
                || (RoomMenuPanel.Instance && RoomMenuPanel.Instance.gameObject.activeInHierarchy))
                {
                    FPSPlayerHpPanel.Instance.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 进入相机模式
    /// </summary>
    public void FreeCameraModeSwitch(bool isEnter)
    {
        bool isHoldingHands = PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual;
        bool isStartPlayer = PlayerMutualControl.Inst && PlayerMutualControl.Inst.isStartPlayer;
        menuBtn.gameObject.SetActive(!isEnter);
        // ChatEmoBg.gameObject.SetActive(!isEnter);
        
        shareBtn.gameObject.SetActive(!isEnter);
        if (isHoldingHands == false || isStartPlayer == true)
        {
            retryBtn.gameObject.SetActive(!isEnter);
        }
        setingBtn.gameObject.SetActive(!isEnter);
        shotBtn.gameObject.SetActive(!isEnter);
        fpsBtn.transform.parent.gameObject.SetActive(!isEnter);
        cameraModeBtn.gameObject.SetActive(!isEnter);
        if (CollectControlManager.Inst.IsCanCollectStar())
        {
            collectStartPanel.SetActive(!isEnter);
        }
        selectClothBtn.gameObject.SetActive(!isEnter);
    }

    /// <summary>
    /// 是否进入摆摊模式
    /// </summary>
    /// <param name="isEnter"></param>
    public void OnPromoteModeChange(bool isEnter)
    {
        PlayerBaseControl.Inst.Move(Vector3.zero);
        joyStick.JoystickReset();
        joyStick.enabled = !isEnter;
        SetButtonState(setFlyBtn, !isEnter);
        SetButtonState(jumpBtn, !isEnter);
        //SetButtonState(flyDownBtn, !isEnter);
        //SetButtonState(flyHighBtn, !isEnter);
        SetButtonState(setDownBtn, !isEnter);
        SetButtonState(waterPanel.setSwimBtn, !isEnter);
        //SetButtonState(waterPanel.stopSwimBtn, !isEnter);
        //SetButtonState(waterPanel.swimDownBtn, !isEnter);
        //SetButtonState(waterPanel.swimHighBtn, !isEnter);
    }

    private void SetButtonState(Button button, bool isEnable)
    {
        if(button != null)
        {
            var img = button.GetComponent<Image>();
            var color = isEnable ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.5f);
            img.color = color;
        }
    }

    public void SetMaxPlayerPanel(bool isActive)
    {
        if(maxPlayerPanel != null && maxPLayerText != null)
        {
            maxPlayerPanel.SetActive(isActive);
            maxPLayerText.text = GameManager.Inst.maxPlayer.ToString();
        }
    }

    #region GreatSnowfield - 收集冰晶
    /// <summary>
    /// 打开收集冰晶界面
    /// </summary>
    public void ShowGreatSnowfieldPanel(int maxCount = 1, int collectedCount = 0)
    {
        greatSnowFieldPanel.gameObject.SetActive(true);
        greatSnowFieldPanel.ShowIceCrystal(maxCount, collectedCount);
    }

    /// <summary>
    /// 隐藏冰晶界面
    /// </summary>
    public void HideGreatSnowfieldPanel()
    {
        greatSnowFieldPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// 收集一个冰晶
    /// </summary>
    public void CollectIceCrystal(Action completeCallback)
    {
        greatSnowFieldPanel.CollectIceCrystal(completeCallback);
    }

    /// <summary>
    /// 收集完成，显示已收集完成的按钮 (合成动画)
    /// </summary>
    public void ShowCollectedComplete()
    {
        greatSnowFieldPanel.ShowCollectedComplete();
    }

    /// <summary>
    /// 显示礼物按钮
    /// </summary>
    /// <param name="spriteName"></param>
    public void ShowRewardBtn()
    {
        greatSnowFieldPanel.gameObject.SetActive(true);
        greatSnowFieldPanel.ShowCrystalCollectCompleteBtn();
    }
    #endregion

    private void OnDisable()
    {
        OnForwardClick(false);
        OnBackwardClick(false);
        OnLeftClick(false);
        OnRightClick(false);
    }
}

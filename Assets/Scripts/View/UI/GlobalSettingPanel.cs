using System;
using UnityEngine;
using UnityEngine.UI;
using RedDot;
using OperationRedDotSystem;
using System.Collections.Generic;
using DG.Tweening.Core.Easing;

public class GlobalSettingPanel : BasePanel<GlobalSettingPanel>
{
    private string[] LEFT_TABS = {"General", "Graphics", "Sound"};
    private SettingItemData[] ITEMS_GENERAL;
    private SettingItemData[] ITEMS_GRAPHICS;
    private SettingItemData[] ITEMS_SOUND;
    private SettingItemData[][] TAB_ITEMS;

    private Transform leftTabParent;
    private GameObject[][] gos;
    private ItemShowStatus[][] itemShowStatus;
    private SettingLeftTab[] tabs;
    private Transform rightContent;
    public Dictionary<int, VNode> vNodeDict = new Dictionary<int, VNode>();

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        InitConfigs();
        leftTabParent = transform.Find("Left/Tabs");
        Button back = transform.Find("Back/Text/Icon/BackClickArea").GetComponent<Button>();
        back.onClick.AddListener(BackClick);
        rightContent = transform.Find("Right/Scroll View/Viewport/Content");
        InitLeftTabs();

        if (PlayModePanel.Instance.operationRedDotManager.IsInited)
        {
            RedDotInitedCallBack(true);
        }
        else
        {
            PlayModePanel.Instance.operationRedDotManager.AddListener(RedDotInitedCallBack);
        }

        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        GlobalSettingManager.Inst.Save();
    }

    private void OnChangeMode(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            Hide();
            ShowPanelBtn();
        }
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        SyncVoiceItemShowStatus();
        SyncFlyItemShowStatus();
        SyncGameView();
        ChooseFirstTab();
        ScrollToTop();
    }

    private void ScrollToTop()
    {
        rightContent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    private void ChooseFirstTab()
    {
        ClickTab(0);
    }

    private void SyncGameView()
    {
        GameObject gameViewItem = FindItem("Game View");
        if (gameViewItem != null)
        {
            gameViewItem.GetComponent<SettingTwoChooseItem>().SetSelected((int) GlobalSettingManager.Inst.GetGameView());
        }
        gameViewItem.SetActive(GlobalFieldController.curMapMode != MapMode.Downtown);
    }
    
    private GameObject FindItem(string title)
    {
        for (int i = 0; i < TAB_ITEMS.Length; i++)
        {
            for (int j = 0; j < TAB_ITEMS[i].Length; j++)
            {
                SettingItemData settingItemData = TAB_ITEMS[i][j];
                if (settingItemData.title == title)
                {
                    if (i < gos.Length && j < gos[i].Length && gos[i][j] != null)
                    {
                        return gos[i][j];
                    }
                }
            }
        }

        return null;
    }

    private void SyncFlyItemShowStatus()
    {
        bool isCanFly = SceneBuilder.Inst.CanFlyEntity.Get<CanFlyComponent>().canFly == 0;
        SetItemVisibility("Flying Mode", isCanFly);
    }

    private void SetItemVisibility(string title, bool status)
    {
        for (int i = 0; i < TAB_ITEMS.Length; i++)
        {
            for (int j = 0; j < TAB_ITEMS[i].Length; j++)
            {
                SettingItemData settingItemData = TAB_ITEMS[i][j];
                if (settingItemData.title == title)
                {
                    if (i < itemShowStatus.Length && j < itemShowStatus[i].Length)
                    {
                        itemShowStatus[i][j].canShowBySpecialControl = status;
                        if (i < gos.Length && j < gos[i].Length && gos[i][j] != null)
                        {
                            gos[i][j].SetActive(itemShowStatus[i][j].ShouldShow());
                        }
                    }
                }
            }
        }
    }

    public void SyncVoiceItemShowStatus()
    {
        if (RealTimeTalkManager.Inst == null)
        {
            return;
        }

        SetItemVisibility("Voice Volume", !RealTimeTalkManager.Inst.closeVoice);
        SetItemVisibility("Voice Changer", !RealTimeTalkManager.Inst.closeVoice);
    }

    private void BackClick()
    {
        Hide();
        if (PlayModePanel.Instance != null)
        {
            PlayModePanel.Instance.gameObject.SetActive(true);
        }

        if (SceneParser.Inst.GetBaggageSet() == 1 && BaggagePanel.Instance != null)
        {
            BaggagePanel.Instance.gameObject.SetActive(true);
        }

        if (FPSPlayerHpPanel.Instance != null)
        {
            FPSPlayerHpPanel.Instance.SetHpPanelVisible(true);
        }

        if (RoomChatPanel.Instance != null)
        {
            RoomChatPanel.Instance.gameObject.SetActive(true);
        }

        ShowPanelBtn();
    }

    /**
    * 恢复之前隐藏的界面交互按钮
    */
    private void ShowPanelBtn()
    {
        if (PortalPlayPanel.Instance != null)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(true);
        }

        if (CatchPanel.Instance != null)
        {
            CatchPanel.Instance.SetButtonVisible(true);
        }

        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsInStateEmo() && StateEmoPanel.Instance)
        {
            StateEmoPanel.Instance.gameObject.SetActive(true);
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
        
        if (DayNightSkyboxAnimPanel.Instance && SkyboxManager.Inst.GetCurSkyboxType() == SkyboxType.DayNight)
        {
            DayNightSkyboxAnimPanel.Instance.SetAnimShow(true);
        }
        
        if(FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(true);
        }
        UIControlManager.Inst.CallUIControl("global_setting_exit");
    }

    public void OnDisable()
    {
        AKSoundManager.Inst.StopVoiceDemoSound(gameObject);
    }

    #region 配置数据

    private void InitConfigs()
    {
        ITEMS_GENERAL = new SettingItemData[]
        {
            new SettingTwoChooseItemData
            {
                title = "Game View",
                firstChoose = "First-person",
                secondChoose = "Third-person",
                defaultChoose = GlobalSettingManager.Inst.GetGameView() == GameView.FirstPerson ? 0 : 1,
                intercept = CheckIfCanChangeGameView,
                OnChooseChange = GameViewChange
            },
            new SettingTwoChooseItemData
            {
                title = "Automatic Running",
                firstChoose = "On",
                secondChoose = "Off",
                defaultChoose = GlobalSettingManager.Inst.IsAutoRunningOpen() ? 0 : 1,
                OnChooseChange = AutomaticRunChange
            },
            new SettingTwoChooseItemData
            {
                title = "Flying Mode",
                firstChoose = "Original",
                secondChoose = "Free",
                defaultChoose = GlobalSettingManager.Inst.GetFlyingMode() == FlyingMode.Original ? 0 : 1,
                OnChooseChange = FlyingModeChange
            },
            new SettingTwoChooseItemData
            {
                title = "Lock Move Stick",
                textLength = 40,
                widthLimit = 480,
                firstChoose = "On",
                secondChoose = "Off",
                defaultChoose = GlobalSettingManager.Inst.IsLockMoveStick() ? 0 : 1,
                OnChooseChange = LockMoveStickChange
            },
            new SettingSliderItemData()
            {
                title = "Camera Pan Sensitivity",
                textLength = 40,
                widthLimit = 480,
                minValue = 0.1f,
                maxValue = 10f,
                tips = "Higher camera pan sensitivity will increase movement speed while panning camera.",
                defaultValue = GlobalSettingManager.Inst.GetCameraSensitive(),
                OnValueChange = CameraPanSensitivityChange
            },
            new SettingTwoChooseItemData
            {
                title = "Show Username",
                firstChoose = "On",
                secondChoose = "Off",
                defaultChoose = GlobalSettingManager.Inst.IsShowUserName() ? 0 : 1,
                OnChooseChange = ShowUsernameChange
            },
            new SettingTwoChooseItemData
            {
                title = "Friend Request",
                firstChoose = "On",
                secondChoose = "Off",
                defaultChoose = GlobalSettingManager.Inst.IsFriendRequestOpen() ? 0 : 1,
                OnChooseChange = FriendRequestChange
            },
        };
        ITEMS_GRAPHICS = new SettingItemData[]
        {
            new SettingTwoChooseItemData
            {
                title = "FPS",
                firstChoose = "High",
                secondChoose = "Low",
                defaultChoose = GlobalSettingManager.Inst.GetFps() == 60 ? 0 : 1,
                tips = "Higher FPS will bring smoother frame rate but may cause phone heat.",
                OnChooseChange = FPSChange
            },
            new SettingTwoChooseItemData
            {
                title = "Bloom",
                firstChoose = "On",
                secondChoose = "Off",
                defaultChoose = GlobalSettingManager.Inst.IsBloomOpen() ? 0 : 1,
                tips = "Turning Bloom on will increase luminosity of light sources but may cause phone heat.",
                OnChooseChange = BloomChange
            },
            new SettingTwoChooseItemData
            {
                title = "Shadow",
                firstChoose = "On",
                secondChoose = "Off",
                defaultChoose = GlobalSettingManager.Inst.IsShadowOpen() ? 0 : 1,
                tips = "Turning Shadow on will optimize shadow effects but may cause phone heat.",
                OnChooseChange = ShadowChange
            },
        };
        ITEMS_SOUND = new SettingItemData[]
        {
            new SettingTwoChooseItemData()
            {
                title = "Footstep",
                firstChoose = "On",
                secondChoose = "Off",
                defaultChoose = GlobalSettingManager.Inst.IsFootstepOpen() ? 0 : 1,
                OnChooseChange = FootStepChange
            },
            new SettingSliderItemData()
            {
                title = "Music",
                minValue = 0,
                maxValue = 100,
                defaultValue = GlobalSettingManager.Inst.GetBgmVolume(),
                OnValueChange = BgmChange
            },
            new SettingSliderItemData()
            {
                title = "Sound Effect",
                textLength = 28,
                widthLimit = 500,
                minValue = 0,
                maxValue = 100,
                defaultValue = GlobalSettingManager.Inst.GetSoundEffectVolume(),
                OnValueChange = SoundEffectChange
            },
            new SettingTwoSliderItemData()
            {
                title = "Voice Volume",
                firstTitle = "Speaker",
                textLength = 24,
                widthLimit = 270,
                firstMinValue = 0,
                firstMaxValue = 100,
                firstDefaultValue = GlobalSettingManager.Inst.GetSpeakerVolume(),
                firstValueChange = SpeakerChange,
                secondTitle = "Microphone",
                secondMinValue = 0,
                secondMaxValue = 100,
                secondDefaultValue = GlobalSettingManager.Inst.GetMicrophoneVolume(),
                secondValueChange = MicroPhoneChange
            },
            new SettingFiveChooseData()
            {
                title = "Voice Changer",
                textLength = 20,
                widthLimit = 295,
                choose = new[] {"Original", "High", "Medium", "Low", "Extra-low"},
                defaultChoose = (int) GlobalSettingManager.Inst.GetVoiceEffect(),
                OnChooseChange = VoiceChangerChange
            }
        };
        TAB_ITEMS = new SettingItemData[][]
        {
            ITEMS_GENERAL,
            ITEMS_GRAPHICS,
            ITEMS_SOUND
        };
        gos = new GameObject[][]
        {
            new GameObject[ITEMS_GENERAL.Length],
            new GameObject[ITEMS_GRAPHICS.Length],
            new GameObject[ITEMS_SOUND.Length],
        };
        itemShowStatus = new ItemShowStatus[][]
        {
            new ItemShowStatus[ITEMS_GENERAL.Length],
            new ItemShowStatus[ITEMS_GRAPHICS.Length],
            new ItemShowStatus[ITEMS_SOUND.Length],
        };
        for (int i = 0; i < itemShowStatus.Length; i++)
        {
            for (int j = 0; j < itemShowStatus[i].Length; j++)
            {
                itemShowStatus[i][j] = new ItemShowStatus();
                itemShowStatus[i][j].showByTab = false;
                itemShowStatus[i][j].canShowBySpecialControl = true;
            }
        }
    }

    private bool CheckIfCanChangeGameView(object arg)
    {
        SettingTwoChooseItem.SettingTwoItemInterceptData interceptData =
            arg as SettingTwoChooseItem.SettingTwoItemInterceptData;
        //当前是拿着武器的时候不能换第三人称
        if (interceptData != null
            && interceptData.toSet == (int) GameView.ThirdPerson
            && PlayerShootControl.Inst
            && PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
        {
            TipPanel.ShowToast("You could not switch view in shooting mode");
            return true;
        }

        if (StateManager.Inst.IsHodingFishingRod())
        {
            TipPanel.ShowToast("You could not switch view in the current state");
            return true;
        }

        return false;
    }

    #endregion

    #region 事件触发

    public void GameViewChange(int index)
    {
        GlobalSettingManager.Inst.GameViewChange((GameView) index);
    }

    public void AutomaticRunChange(int index)
    {
        GlobalSettingManager.Inst.AutomaticRunChange(index);
    }

    public void FlyingModeChange(int index)
    {
        GlobalSettingManager.Inst.FlyingModeChange(index);
    }

    public void LockMoveStickChange(int index)
    {
        GlobalSettingManager.Inst.LockMoveStickChange(index);
    }

    public void CameraPanSensitivityChange(float value)
    {
        GlobalSettingManager.Inst.CameraPanSensitivityChange(value);
    }

    public void ShowUsernameChange(int index)
    {
        GlobalSettingManager.Inst.ShowUserNameChange(index);
    }

    public void FriendRequestChange(int index)
    {
        GlobalSettingManager.Inst.FriendRequestChange(index);
    }

    public void FPSChange(int index)
    {
        GlobalSettingManager.Inst.FPSChange(index);
    }

    public void BloomChange(int index)
    {
        GlobalSettingManager.Inst.BloomChange(index);
    }

    public void ShadowChange(int index)
    {
        GlobalSettingManager.Inst.ShadowChange(index);
    }

    public void FootStepChange(int index)
    {
        GlobalSettingManager.Inst.FootStepChange(index);
    }

    public void BgmChange(float value)
    {
        GlobalSettingManager.Inst.BgmChange(value);
    }

    public void SoundEffectChange(float value)
    {
        GlobalSettingManager.Inst.SoundEffectChange(value);
    }

    public void MicroPhoneChange(float value)
    {
        GlobalSettingManager.Inst.MicroPhoneChange(value);
    }

    public void SpeakerChange(float value)
    {
        GlobalSettingManager.Inst.SpeakerChange(value);
    }

    public void VoiceChangerChange(int index)
    {
        GlobalSettingManager.Inst.VoiceChangerChange(index);
    }

    #endregion

    private void InitLeftTabs()
    {
        GameObject prefabTab = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/Panel/SettingLeftTab");
        tabs = new SettingLeftTab[LEFT_TABS.Length];
        for (int i = 0; i < LEFT_TABS.Length; i++)
        {
            GameObject tab = Instantiate(prefabTab, leftTabParent);
            SettingLeftTab settingTab = tab.GetComponent<SettingLeftTab>();
            tabs[i] = settingTab;
            settingTab.Init(LEFT_TABS[i]);
            int index = i;
            settingTab.OnClick += () => { ClickTab(index); };
            settingTab.StatusChange += (status) => { ChangeRightItems(index, status); };
        }

        if (tabs.Length > 0)
        {
            tabs[0].SetSelected(true);
        }
    }

    private void ClickTab(int index)
    {
        if (tabs == null)
        {
            return;
        }

        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetSelected(index == i);
        }

        if (index == 1)
        {
            RequestCleanRedDot((int) ENodeType.GraphicsBtn);
        }
        else if (index == 2)
        {
            RequestCleanRedDot((int) ENodeType.SoundBtn);
        }
    }

    private void ChangeRightItems(int index, bool status)
    {
        if (index < 0 || index >= TAB_ITEMS.Length)
        {
            return;
        }

        SettingItemData[] itemDatas = TAB_ITEMS[index];
        for (int i = 0; i < itemDatas.Length; i++)
        {
            if (index < 0 || index >= gos.Length || index >= itemShowStatus.Length)
            {
                continue;
            }

            if (i < 0 || i >= gos[index].Length || i >= itemShowStatus[index].Length)
            {
                continue;
            }

            itemShowStatus[index][i].showByTab = status;
            bool exist = gos[index][i] != null;
            if (!exist)
            {
                if (status)
                {
                    SettingItem settingItem = InitItems(index, i, itemDatas[i]);
                    if (settingItem == null)
                    {
                        continue;
                    }

                    settingItem.Init(itemDatas[i]);
                    exist = true;
                }
            }

            if (exist)
            {
                gos[index][i].SetActive(itemShowStatus[index][i].ShouldShow());
            }
        }

        //回到顶部
        if (status)
        {
            ScrollToTop();
        }
    }

    private SettingItem InitItems(int index, int i, SettingItemData itemData)
    {
        GameObject prefabItem = null;
        Type t = null;
        if (itemData is SettingTwoChooseItemData)
        {
            prefabItem = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/Panel/SettingTwoChooseItem");
            t = typeof(SettingTwoChooseItem);
        }
        else if (itemData is SettingSliderItemData)
        {
            prefabItem = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/Panel/SettingSliderItem");
            t = typeof(SettingSliderItem);
        }
        else if (itemData is SettingTwoSliderItemData)
        {
            prefabItem = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/Panel/SettingTwoSliderItem");
            t = typeof(SettingTwoSliderItem);
        }
        else if (itemData is SettingFiveChooseData)
        {
            prefabItem = ResManager.Inst.LoadRes<GameObject>("Prefabs/UI/Panel/SettingFiveChooseItem");
            t = typeof(SettingFiveChooseItem);
        }

        if (prefabItem == null)
        {
            return null;
        }

        GameObject item = Instantiate(prefabItem, rightContent);
        gos[index][i] = item;
        SettingItem settingItem = item.GetComponent(t) as SettingItem;
        return settingItem;
    }

    private void RequestCleanRedDot(int btnId)
    {
        VNode vNode;
        if (vNodeDict.TryGetValue(btnId, out vNode) && vNode.mLogic.Count > 0)
        {
            int oldValue = vNode.mLogic.Count;
            vNode.mLogic.ChangeCount(oldValue - 1);
        }

        PlayModePanel.Instance.operationRedDotManager.RequestCleanOptRedDot(btnId);
    }

    private void RedDotInitedCallBack(bool isInited)
    {
        if (isInited)
        {
            List<int> redDotId = PlayModePanel.Instance.operationRedDotManager.optBtnIds;
            for (int i = 0; i < redDotId.Count; i++)
            {
                int id = redDotId[i];
                if (id == (int) ENodeType.GraphicsBtn || id == (int) ENodeType.SoundBtn)
                {
                    AttachOptRedDot(id);
                }
            }
        }
    }

    public void AttachOptRedDot(int logicNodeType)
    {
        var vNode = InternalAttachRedDotNode(logicNodeType);
        if (vNode != null && vNode.mLogic != null)
        {
            vNode.mLogic.AddListener(ChangedCountCallBack);
            vNode.mLogic.ChangeCount(vNode.mLogic.Count);
            vNodeDict.Add(logicNodeType, vNode);
        }
    }

    private void ChangedCountCallBack(int count)
    {
    }

    private VNode InternalAttachRedDotNode(int logicNodeType)
    {
        RedDotTree tree = PlayModePanel.Instance.operationRedDotManager.Tree;
        GameObject target = tabs[1].gameObject;
        if (logicNodeType == (int) ENodeType.SoundBtn)
        {
            target = tabs[2].gameObject;
        }

        VNode dot = tree.CreateAndBindViewRedDot(target, logicNodeType, ERedDotPrefabType.Type4);
        return dot;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }


    public class ItemShowStatus
    {
        public bool showByTab;
        public bool canShowBySpecialControl;

        public bool ShouldShow()
        {
            return showByTab && canShowBySpecialControl;
        }
    }
}
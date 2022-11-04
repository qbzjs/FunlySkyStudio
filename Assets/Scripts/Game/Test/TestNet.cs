using BudEngine.NetEngine.src.Util;
using Cinemachine;
using SavingData;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:Shaocheng
/// Description:本地测试工具-联机测试
/// Date: 2022-3-30 19:43:08
/// </summary>
public partial class TestPanel : BasePanel<TestPanel>
{
#if UNITY_EDITOR
    private void InitTestNetOnStart()
    {
        var config = TestNetParams.Inst.CurrentConfig;

        if (config != null && config.localserverConfig != null)
        {
            serverIpInput.text = config.localserverConfig.ipAddr;

            netMapIdInput.text = config.testMapId;

            saveNetConfigBtn.onClick.AddListener(() =>
            {
                UpdateConfigFromUI();
                TestNetParams.Inst.SaveConfigJson();
            });
            
            openTestConfigFile.onClick.AddListener(TestNetParams.Inst.OpenTestNetConfig);

            isOpenDebuggerLogToggle.isOn = config.isOpenDebuggerLog;
            Debugger.Enable = config.isOpenDebuggerLog;
            isOpenDebuggerLogToggle.onValueChanged.AddListener((isOn) =>
            {
                Debugger.Enable = isOn;
                config.isOpenDebuggerLog = isOn;
                LoggerUtils.Log("当前Debugger日志:" + Debugger.Enable);
            });

            isOpenNetTestToggle.isOn = config.isOpenNetTest;
            isOpenNetTestToggle.onValueChanged.AddListener((isOn) =>
            {
                config.isOpenNetTest = isOn;
                LoggerUtils.Log("TestNetParams.Inst.CurrentConfig.isOpenNetTest:" + isOn);
            });

            netTestToggles[0].isOn = config.testEnvironment == TestNetParams.TestNetType.Master;
            netTestToggles[1].isOn = config.testEnvironment == TestNetParams.TestNetType.Alpha;
            netTestToggles[2].isOn = config.testEnvironment == TestNetParams.TestNetType.Local;
            for (int i = 0; i < netTestToggles.Length; i++)
            {
                int index = i;
                netTestToggles[i].onValueChanged.AddListener((isOn) =>
                {
                    if (index == 0 && netTestToggles[index].isOn)
                    {
                        config.testEnvironment = TestNetParams.TestNetType.Master;
                        LoggerUtils.Log("当前联机测试环境:Master");
                    }
                    else if (index == 1 && netTestToggles[index].isOn)
                    {
                        config.testEnvironment = TestNetParams.TestNetType.Alpha;
                        LoggerUtils.Log("当前联机测试环境:Alpha测试");
                    }
                    else if (index == 2 && netTestToggles[index].isOn)
                    {
                        config.testEnvironment = TestNetParams.TestNetType.Local;
                        LoggerUtils.Log("当前联机测试环境:本地服测试");
                    }
                });
            }
            
            leaveRoomBtn.onClick.AddListener(() =>
            {
                if (ClientManager.Inst.isOnline)
                {
                    LoggerUtils.Log("LeaveRoom~~");
                    ClientManager.Inst.LeaveRoom();
                    EditorApplication.isPlaying = false;
                }
                else
                {
                    LoggerUtils.Log("Not Online~~");
                }
            });

            if (config.testHeaders != null)
            {
                for (int i = 0; i < config.testHeaders.Length; i++)
                {
                    GameObject playerItem = Instantiate(playerItemObj, playerRoot.transform, true);
                    playerItem.name = "Player_" + i;
                    var text = playerItem.GetComponentInChildren<Text>();
                    text.text = playerItem.name;
                    playerItem.SetActive(true);
                    
                    var toggle = playerItem.GetComponentInChildren<Toggle>();
                    
                    int index = i;
                    toggle.isOn = i == config.cosPlayerIndex;
                    toggle.onValueChanged.AddListener((isOn) =>
                    {
                        if (isOn)
                        {
                            config.cosPlayerIndex = index;
                            TestNetParams.testHeader = config.testHeaders[index];
                            LoggerUtils.Log("当前选择Player:" + index);
                        }
                    });
                }
            }

            isPrivateToggle.isOn = config.isPrivate == 1;
            isPrivateToggle.onValueChanged.AddListener((isOn) =>
            {
                TestNetParams.Inst.CurrentConfig.isPrivate = isPrivateToggle.isOn ? 1 : 0;
                LoggerUtils.Log("isPrivate=>" + TestNetParams.Inst.CurrentConfig.isPrivate);
            });

            roomCodeInput.text = config.roomCode;

            isSaveNetLog.isOn = config.isSaveLog;
            Debugger.isSaveLog = config.isSaveLog;
            isSaveNetLog.onValueChanged.AddListener((isOn) =>
            {
                TestNetParams.Inst.CurrentConfig.isSaveLog = isSaveNetLog.isOn;
                Debugger.isSaveLog = TestNetParams.Inst.CurrentConfig.isSaveLog;
                LoggerUtils.Log("isSaveLog=>" + TestNetParams.Inst.CurrentConfig.isSaveLog);
            });
        }
    }

    private void UpdateConfigFromUI()
    {
        if (!string.IsNullOrEmpty(serverIpInput.text)) TestNetParams.Inst.CurrentConfig.localserverConfig.ipAddr = serverIpInput.text;
        if (!string.IsNullOrEmpty(netMapIdInput.text)) TestNetParams.Inst.CurrentConfig.testMapId = netMapIdInput.text;
        if (!string.IsNullOrEmpty(roomCodeInput.text)) TestNetParams.Inst.CurrentConfig.roomCode = roomCodeInput.text;
    }

    void OnLoadMapEnterRoomClick()
    {
        UpdateConfigFromUI();
        OnLoadMapBtn(TestNetParams.Inst.CurrentConfig.testMapId);
        OnEnterRoomClick();
    }

    void OnEnterRoomClick()
    {
        UpdateConfigFromUI();

        if (GameManager.Inst.gameMapInfo == null)
        {
            GameManager.Inst.gameMapInfo = new MapInfo()
            {
                mapId = TestNetParams.Inst.CurrentConfig.testMapId
            };

            GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
        }
        else
        {
            GameManager.Inst.gameMapInfo.mapId = TestNetParams.Inst.CurrentConfig.testMapId;
            GlobalFieldController.CurMapInfo.mapId = TestNetParams.Inst.CurrentConfig.testMapId;
        }

        //editController.gController.DisableGizmo();
        var gameMode = GameMode.Guest;
        GlobalFieldController.CurGameMode = gameMode;
        InputReceiver.locked = false;
        UIManager.Inst.CloseAllDialog();
        MovePathManager.Inst.CloseAndSave();
        MovePathManager.Inst.ReleaseAllPoints();
        PlayModePanel.Show();
        //PlayModePanel.Instance.OnEdit = EnterEditMode;
        PlayModePanel.Instance.EntryMode(gameMode);

        var playController = new PlayModeController();
        var mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        var EditVirCamera = GameObject.Find("EditModeCamera").GetComponent<CinemachineVirtualCamera>();
        var PlayVirCamera = GameObject.Find("PlayModeCamera").GetComponent<CinemachineVirtualCamera>();
        var playerNodeGo = GameObject.Find("PlayerNode");
        var playerCom = playerNodeGo.transform.Find("Player").GetComponent<PlayerBaseControl>();
        var joyStick = PlayModePanel.Instance.joyStick;
        playController.joyStick = joyStick;
        playController.SetCamera(mainCamera, PlayVirCamera);
        playController.SetPlayerState(playerCom, gameMode);
        GameObject.Find("GameStart").GetComponent<GameController>().playController = playController;
        EditVirCamera.enabled = false;
        PlayVirCamera.enabled = true;
        SceneBuilder.Inst.SpawnPoint.SetActive(false);
        SceneBuilder.Inst.BgBehaviour.Play(true);
        SceneBuilder.Inst.BgBehaviour.PlayEnr();
        SceneBuilder.Inst.SetEntityMeshsVisibleByMode(false);
        SceneSystem.Inst.StartSystem();

        OnExitClick();
    }

#endif
}
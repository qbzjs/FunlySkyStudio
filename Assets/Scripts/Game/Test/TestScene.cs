using Cinemachine;
using Newtonsoft.Json;
using SavingData;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description:本地测试工具-场景加载
/// Date: 2022-3-30 19:43:08
/// </summary>
public partial class TestPanel : BasePanel<TestPanel>
{
#if UNITY_EDITOR

    private void OnLoadJsonUrlClick()
    {
        LoggerUtils.Log("OnLoadJsonUrlClick");
        if (IsInputNull(jsonUrlInput)) return;
        ClearScene();
        string jsonStr = jsonUrlInput.text;
        SaveToJsonUrlHistory(HistoryType.MAP_JSON, jsonStr);
        InitSceneFromJson(jsonStr);
        LoggerUtils.Log("OnLoadFromJsonClick success");
    }

    public void OnLoadMapBtnClick()
    {
        OnLoadMapBtn();
    }

    public void OnLoadMapBtn(string mapIdPara = "")
    {
        ClearScene();
        OfflineResManager.Inst.Clear();
        LoggerUtils.Log("OnLoadMapBtnClick");

        string loadMapId = string.Empty;
        string loadMapName = string.Empty;
        if (string.IsNullOrEmpty(mapIdPara))
        {
            if (string.IsNullOrEmpty(mapIdInput.text) && string.IsNullOrEmpty(mapNameInput.text))
            {
                TipPanel.ShowToast("{0}", "Please enter map Id or name ~");
                return;
            }

            loadMapId = mapIdInput.text;
            loadMapName = mapNameInput.text;
        }
        else
        {
            loadMapId = mapIdPara;
            loadMapName = "";
        }

        GameManager.Inst.ugcUntiyMapDataInfo = new UgcUntiyMapDataInfo()
        {
            mapId = loadMapId,
            mapName = loadMapName,
            draftPath = Application.persistentDataPath + "/U3D/Local"
        };
        MapLoadManager.Inst.GetMapInfo(GameManager.Inst.ugcUntiyMapDataInfo, (getMapInfo) =>
        {
            GameManager.Inst.gameMapInfo = getMapInfo.mapInfo;
            GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
            // GlobalFieldController.whiteListMask.SetInWhiteList(WhiteListMask.WhiteListType.OfflineRender);
            if (!string.IsNullOrEmpty(GameManager.Inst.gameMapInfo.mapJson))
            {
                SaveToMapHistory();
                
                // InitSceneFromJson(GameManager.Inst.gameMapInfo.mapJson);
                OfflineResManager.Inst.PreloadAssetBundle(() =>
                {
                    LoggerUtils.Log("PreloadAssetBundle Over");
                    InitSceneFromJson(GameManager.Inst.gameMapInfo.mapJson);
                });
            }
            else
            {
                LoggerUtils.LogError("GetJson failed");
            }
        }, (error) => { LoggerUtils.LogError("GetMapInfo Fail" + error); });
    }

    private void OnSaveJsonClick()
    {
        if (string.IsNullOrEmpty(localJsonNameInput.text))
        {
            TipPanel.ShowToast("{0}", "Please enter Json Name ~");
            return;
        }

        string sceneJson = SceneParser.Inst.StageToMapJson();
        LoggerUtils.Log(sceneJson);
        if (string.IsNullOrEmpty(sceneJson))
        {
            TipPanel.ShowToast("{0}", "Scene is empty ~");
            return;
        }

        DirectoryCheck();
        string inputJsonName = localJsonNameInput.text;
        if (!inputJsonName.EndsWith(".json"))
        {
            inputJsonName += ".json";
        }

        string jsonFilePath = JSON_FILE_PATH + inputJsonName;
        File.WriteAllText(jsonFilePath, sceneJson);
        LoggerUtils.Log("Save Json File Success !=>" + jsonFilePath);
    }


    private void OnReadJsonClick()
    {
        if (string.IsNullOrEmpty(localJsonNameInput.text))
        {
            TipPanel.ShowToast("{0}", "Please enter Json Name ~");
            return;
        }

        string inputJsonName = localJsonNameInput.text;
        if (!inputJsonName.EndsWith(".json"))
        {
            inputJsonName += ".json";
        }

        string filePath = JSON_FILE_PATH + inputJsonName;
        LoggerUtils.Log("Read Json File=>" + filePath);

        if (!File.Exists(filePath))
        {
            TipPanel.ShowToast("{0}", "Json file not exist! => " + filePath);
            LoggerUtils.Log("Json file not exist! => " + filePath);
            return;
        }

        string jsonStr = File.ReadAllText(filePath);
        ClearScene();
        if (GameManager.Inst.gameMapInfo == null)
        {
            GameManager.Inst.gameMapInfo = new MapInfo()
            {
                mapJson = jsonStr,
            };
            GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo;
        }

        SceneBuilder.Inst.ParseAndBuild(jsonStr);
    }

    private void ClearScene()
    {
        LoggerUtils.Log("ClearScene");
        SceneBuilder.Inst.BgBehaviour.Stop();
        SceneBuilder.Inst.DestroyScene();
        GameManager.Inst.gameMapInfo = null;
        GlobalFieldController.CurMapInfo = null;

        //其他业务数据清空
        SwitchManager.Inst.ClearBevs();
        ShowHideManager.Inst.ClearBevs();
        CollectControlManager.Inst.ClearBevs();
        CollectControlManager.Inst.ClearRoomData();
        NodeBehaviourManager.Inst.ClearManagers();
    }

    private void OnIsOpenLogToggleChanged(bool isOn)
    {
        LoggerUtils.IsDebug = isOn;
    }

    private void InitSceneFromJson(string jsonStr)
    {
        if (jsonStr.Contains("ZipFile/") && jsonStr.Contains(".zip"))
        {
            CoroutineManager.Inst.StartCoroutine(GetByte(jsonStr, (content) =>
                {
                    string jsonStr = ZipUtils.SaveZipFromByte(content);
                    if (GameManager.Inst.gameMapInfo == null)
                    {
                        GameManager.Inst.gameMapInfo = new MapInfo()
                        {
                            mapJson = jsonStr,
                        };
                        GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo;
                    }

                    SceneBuilder.Inst.ParseAndBuild(jsonStr);
                },
                (error) => { Debug.LogError("Get Zip MapJson Fail"); }));
        }
        else
        {
            CoroutineManager.Inst.StartCoroutine(GetText(jsonStr, (content) =>
                {
                    if (GameManager.Inst.gameMapInfo == null)
                    {
                        GameManager.Inst.gameMapInfo = new MapInfo()
                        {
                            mapJson = jsonStr,
                        };
                        GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo;
                    }

                    SceneBuilder.Inst.ParseAndBuild(content);
                },
                (error) => { Debug.LogError("Get MapJson Fail"); }));
        }
    }

    private void OnPrintJsonClick()
    {
        LoggerUtils.Log(SceneParser.Inst.StageToMapJson());
    }

    void OnEnterPlayClick()
    {
        var playController = new PlayModeController();
        var gameMode = GameMode.Play;
        var mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        var EditVirCamera = GameObject.Find("EditModeCamera").GetComponent<CinemachineVirtualCamera>();
        var PlayVirCamera = GameObject.Find("PlayModeCamera").GetComponent<CinemachineVirtualCamera>();

        var playerNodeGo = GameObject.Find("PlayerNode");

        var playerCom = playerNodeGo.transform.Find("Player").GetComponent<PlayerBaseControl>();
        GlobalFieldController.CurGameMode = gameMode;
        InputReceiver.locked = false;
        UIManager.Inst.CloseAllDialog();
        MovePathManager.Inst.CloseAndSave();
        MovePathManager.Inst.ReleaseAllPoints();
        PlayModePanel.Show();
        // PlayModePanel.Instance.OnEdit = EnterEditMode;
        PlayModePanel.Instance.EntryMode(gameMode);

        var joyStick = PlayModePanel.Instance.joyStick;
        playController.joyStick = joyStick;
        playController.SetCamera(mainCamera, PlayVirCamera);
        playController.SetPlayerState(playerCom, gameMode);
        EditVirCamera.enabled = false;
        PlayVirCamera.enabled = true;
        SceneBuilder.Inst.SpawnPoint.SetActive(false);
        SceneBuilder.Inst.BgBehaviour.Play(true);
        SceneBuilder.Inst.SetEntityMeshsVisibleByMode(false);
        SceneSystem.Inst.StartSystem();
        ShowHideManager.Inst.EnterPlayMode();
    }

    void OnOpenJsonBtnClick()
    {
        jsonUrlDropdown.Show();
    }

    void OnOpenMapBtnClick()
    {
        mapDropdown.Show();
    }

    void SaveToMapHistory()
    {
        if (GameManager.Inst.gameMapInfo == null)
        {
            LoggerUtils.Log("GameMapInfo is null!");
            return;
        }

        string mapId = GameManager.Inst.gameMapInfo.mapId;
        string mapName = GameManager.Inst.gameMapInfo.mapName;

        string saveStr = mapName + ',' + mapId;
        SaveToJsonUrlHistory(HistoryType.MAP_NAME, saveStr);
    }

    void OnMapHistoryClear()
    {
        if (EditorUtility.DisplayDialog("Hey Dude!", "确定要清除MapName历史记录吗？？", "YES", "NO"))
        {
            PlayerPrefs.SetString(nameof(HistoryType.MAP_NAME), string.Empty);
            mapNameHistory = InitHistoryString(HistoryType.MAP_NAME);
            RefreshHistoryDropdown(HistoryType.MAP_NAME);
            LoggerUtils.Log("记录清除成功！");
        }
    }

    void OnJsonHistoryClear()
    {
        if (EditorUtility.DisplayDialog("Hey Dude!", "确定要清除JsonUrl历史记录吗？？", "YES", "NO"))
        {
            PlayerPrefs.SetString(nameof(HistoryType.MAP_JSON), string.Empty);
            jsonUrlHistory = InitHistoryString(HistoryType.MAP_JSON);
            RefreshHistoryDropdown(HistoryType.MAP_JSON);
            LoggerUtils.Log("记录清除成功！");
        }
    }

    void OnJsonUrlDropValueChanged(int i)
    {
        var choose = jsonUrlDropdown.options[i].text;
        LoggerUtils.Log(string.Format("选择了历史记录=>{0}:{1}", i, choose));

        jsonUrlInput.text = choose;
    }

    void OnMapDropValueChanged(int i)
    {
        var choose = mapDropdown.options[i].text;
        LoggerUtils.Log(string.Format("选择了历史记录=>{0}:{1}", i, choose));

        string[] mapStr = choose.Split(',');
        mapNameInput.text = mapStr[0];
        mapIdInput.text = mapStr[1];
    }
#endif
}
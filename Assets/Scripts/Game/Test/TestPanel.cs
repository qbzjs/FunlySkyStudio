using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BudEngine.NetEngine;
using BudEngine.NetEngine.src.Util;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
///
/// 目前能力:
///
/// 场景加载：
/// 1-根据Map Json Url 加载Scene
/// 2-根据MapId/MapName 加载Scene ,支持Master和Prod环境
/// 3-根据本地Json文件 加载Scene
/// 4-保存SceneJson到本地目录
/// 5-场景清除
/// 6-进入游玩模式
/// 7-保存历史加载数据
///
/// 素材加载：
/// 1-根据JsonUrl加载素材(支持master和prod环境)
///
/// 网络：
/// 本地联机和master联机测试功能
///
/// 其他：
/// 1-LoggerUtils打印控制
/// 2-场景Scene打印Log
/// https://cdn.joinbudapp.com/UgcJson/1457997285296115712/1457997285296115712_1638530570.json
///
/// test map id : 1457997285296115712_1638515350_1 (master)
///
/// test prop json : https://buddy-app-bucket.s3.us-west-1.amazonaws.com/PropsJson/1460531478844608512/1460531478844608512_1638995877m.json
/// </summary>
/// <summary>
/// Author:Shaocheng
/// Description:本地测试工具-UI
/// Date: 2022-3-30 19:43:08
/// </summary>
public partial class TestPanel : BasePanel<TestPanel>
{
#if UNITY_EDITOR

    #region Ui

    public Button offlineBtn;

    public Toggle[] toggles;
    public GameObject[] togglePanels;

    public Button exitBtn;
    public Button loadJsonUrlBtn;
    public Button loadMapBtn;
    public Button saveJsonBtn;
    public Button readJsonFileBtn;
    public Button printJsonBtn;
    public Button clearBtn;

    public InputField jsonUrlInput;
    public InputField mapIdInput;
    public InputField mapNameInput;
    public InputField localJsonNameInput;

    public Toggle isOpenLogToggle;
    public Button runCodeBtn;

    public InputField propJsonUrlInput;
    public Button propLoadBtn;
    public Button printPropJsonBtn;
    public Button enterPlayBtn;

    public Toggle[] getMapUrlToggles;

    public Button openJsonDropBtn;
    public Button clearJsonHistoryBtn;
    public Dropdown jsonUrlDropdown;

    public Button openMapDropBtn;
    public Button clearMapHistoryBtn;
    public Dropdown mapDropdown;

    public Button openPropDropBtn;
    public Button clearPropHistoryBtn;
    public Dropdown propDropdown;

    //联机测试相关
    public Button leaveRoomBtn;
    public Button enterRoomBtn;
    public Button loadMapEnterRoomBtn;
    public Button saveNetConfigBtn;
    public Button openTestConfigFile;

    public InputField netMapIdInput;

    public InputField serverIpInput;

    public Toggle[] netTestToggles;
    public Toggle isOpenDebuggerLogToggle;

    public Toggle isOpenNetTestToggle;
    public Toggle isSaveNetLog;

    public ToggleGroup playerToggleGroup;
    public GameObject playerItemObj;
    public GameObject playerRoot;

    public Toggle isPrivateToggle;
    public InputField roomCodeInput;

    #endregion

    #region Params

    private Action CloseCallback;
    private string JSON_FILE_PATH = System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Json" + Path.DirectorySeparatorChar;

    private List<string> jsonUrlHistory = null;
    private List<string> mapNameHistory = null;
    private List<string> propHistory = null;

    enum HistoryType
    {
        MAP_JSON,
        MAP_NAME,
        PROP_JSON
    }

    #endregion

    public void Start()
    {
        Debug.Log("TestPanel Start");
        // Debug.Log("HistoryType.MAP_JSON:" + PlayerPrefs.GetString("MAP_JSON"));
        // Debug.Log("HistoryType.MAP_NAME:" + PlayerPrefs.GetString("MAP_NAME"));
        // Debug.Log("HistoryType.PROP_JSON:" + PlayerPrefs.GetString("PROP_JSON"));

        LoggerUtils.IsDebug = true;
        HttpUtils.IsMaster = true;

        offlineBtn.onClick.AddListener(TestOfflineClick);

        exitBtn.onClick.AddListener(OnExitClick);
        loadJsonUrlBtn.onClick.AddListener(OnLoadJsonUrlClick);
        loadMapBtn.onClick.AddListener(OnLoadMapBtnClick);
        saveJsonBtn.onClick.AddListener(OnSaveJsonClick);
        readJsonFileBtn.onClick.AddListener(OnReadJsonClick);
        printJsonBtn.onClick.AddListener(OnPrintJsonClick);
        clearBtn.onClick.AddListener(ClearScene);
        enterRoomBtn.onClick.AddListener(OnEnterRoomClick);
        loadMapEnterRoomBtn.onClick.AddListener(OnLoadMapEnterRoomClick);
        runCodeBtn.onClick.AddListener(OnCodeRun);
        propLoadBtn.onClick.AddListener(OnLoadProp);
        printPropJsonBtn.onClick.AddListener(OnPrintPropJsonClick);
        enterPlayBtn.onClick.AddListener(OnEnterPlayClick);

        openJsonDropBtn.onClick.AddListener(OnOpenJsonBtnClick);
        clearJsonHistoryBtn.onClick.AddListener(OnJsonHistoryClear);
        jsonUrlDropdown.options.Add(new Dropdown.OptionData(""));
        jsonUrlDropdown.onValueChanged.AddListener(OnJsonUrlDropValueChanged);

        openMapDropBtn.onClick.AddListener(OnOpenMapBtnClick);
        clearMapHistoryBtn.onClick.AddListener(OnMapHistoryClear);
        mapDropdown.options.Add(new Dropdown.OptionData(""));
        mapDropdown.onValueChanged.AddListener(OnMapDropValueChanged);

        openPropDropBtn.onClick.AddListener(OnOpenPropBtnClick);
        clearPropHistoryBtn.onClick.AddListener(OnPropHistoryClear);
        propDropdown.options.Add(new Dropdown.OptionData(""));
        propDropdown.onValueChanged.AddListener(OnPropDropValueChanged);

        isOpenLogToggle.isOn = LoggerUtils.IsDebug;
        isOpenLogToggle.onValueChanged.AddListener(OnIsOpenLogToggleChanged);
        for (int i = 0; i < toggles.Length; i++)
        {
            int index = i;
            toggles[i].onValueChanged.AddListener((isOn) => { togglePanels[index].SetActive(isOn); });
        }

        for (int i = 0; i < getMapUrlToggles.Length; i++)
        {
            int index = i;
            getMapUrlToggles[i].onValueChanged.AddListener((isOn) =>
            {
                //默认是 prod (PR环境)
                if (index == 0 && getMapUrlToggles[index].isOn)
                {
                    HttpUtils.IsMaster = true;
                    LoggerUtils.Log("HttpUtils.IsMaster = true");
                }
                else if (index == 1 && getMapUrlToggles[index].isOn)
                {
                    HttpUtils.IsMaster = false;
                    LoggerUtils.Log("HttpUtils.IsMaster = false");
                }
            });
        }

        jsonUrlHistory = InitHistoryString(HistoryType.MAP_JSON);
        RefreshHistoryDropdown(HistoryType.MAP_JSON);
        mapNameHistory = InitHistoryString(HistoryType.MAP_NAME);
        RefreshHistoryDropdown(HistoryType.MAP_NAME);
        propHistory = InitHistoryString(HistoryType.PROP_JSON);
        RefreshHistoryDropdown(HistoryType.PROP_JSON);

        for (int i = 0; i < toggles.Length; i++)
        {
            int index = i;
            toggles[i].onValueChanged.AddListener((isOn) => { togglePanels[index].SetActive(isOn); });
        }

        InitTestNetOnStart();
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
    }

    public GameObject playerNode;

    public int codeIndex;

    void OnCodeRun()
    {
        Debug.Log("OnCodeRun");
        //TODO: Do Some Test Code
    
        LoggerUtils.Log("GetUtcTimeStamp: --- " + GameUtils.GetUtcTimeStamp());

    }

    private void TestUgcLoad()
    {
        string testJson =
            "{\"cloId\":1000,\"clothMapId\":\"1499630531066150912_1646374940_5\",\"clothesJson\":\"https://cdn.joinbudapp.com/U3D/UGCClothes/ClothJson/1499630531066150912/1499630531066150912_1646403725_json_clothes.json\",\"clothesUrl\":\"https://cdn.joinbudapp.com/U3D/UGCClothes/ClothTex/1499630531066150912/1499630531066150912_1646403724_tex_clothes.zip\"}";
        playerNode = GameObject.Find("Player");
        ClothStyleData clothData = JsonConvert.DeserializeObject<ClothStyleData>(testJson);

        ClothStyleData clothStyleData = RoleConfigDataManager.Inst.GetClothesById(1000);

        LoggerUtils.Log("clothStyleData====>" + clothStyleData.templateId);
        clothData.templateId = clothStyleData.templateId;

        RoleController roleCom = playerNode.GetComponentInChildren<RoleController>(true);
        roleCom.SetUGCClothStyle(clothData);
    }

    private void TestUseWeapon()
    {
        var testWeaponUid = 123;

        AttackWeaponAffectPlayerData affectData = new AttackWeaponAffectPlayerData();
        affectData.PlayerId = "666666";
        affectData.Damage = 20;
        //affectData.AttackDirection = "55555555";

        AttackWeaponItemData weaponItemData = new AttackWeaponItemData();
        weaponItemData.affectPlayers = new[]
        {
            affectData,
        };

        WeaponSystemController.Inst.SendWeaponAttackReq(ItemType.ATTACK_WEAPON, testWeaponUid, weaponItemData, (errorCode, msg) =>
        {
            LoggerUtils.Log("AttackWeapon SendWeaponAttackReq CallBack ==>" + errorCode);
            LoggerUtils.Log("AttackWeapon SendWeaponAttackReq CallBack==>" + msg);
        });
    }

    #region Utils Funcs

    public void ShowInMain(bool isInMain, Action closeCb)
    {
        toggles[0].gameObject.SetActive(isInMain);
        toggles[2].gameObject.SetActive(isInMain);
        toggles[3].gameObject.SetActive(isInMain);

        if (isInMain)
        {
            togglePanels[0].SetActive(true);
            togglePanels[1].SetActive(false);
        }
        else
        {
            togglePanels[0].SetActive(false);
            togglePanels[1].SetActive(true);
        }

        this.CloseCallback = closeCb;
    }

    private void DirectoryCheck()
    {
        if (!Directory.Exists(JSON_FILE_PATH))
        {
            Directory.CreateDirectory(JSON_FILE_PATH);
        }
    }

    private void OnExitClick()
    {
        jsonUrlHistory = null;
        Destroy(gameObject);
        if (CloseCallback != null)
        {
            CloseCallback.Invoke();
        }
    }

    public void TestOfflineClick()
    {
        ClearScene();
        GameManager.Inst.gameMapInfo = ResManager.Inst.LoadJsonRes<MapInfo>("Offline/1478919641010266112_1641567274");
        if (GameManager.Inst.gameMapInfo == null)
        {
            LoggerUtils.Log("No Test Offline File");
            return;
        }

        GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
        // GlobalFieldController.whiteListMask.SetInWhiteList(WhiteListMask.WhiteListType.OfflineRender);
        if (!string.IsNullOrEmpty(GameManager.Inst.gameMapInfo.mapJson))
        {
            LoggerUtils.Log("CurMapInfo:" + JsonConvert.SerializeObject(GlobalFieldController.CurMapInfo));
            InitSceneFromJson(GameManager.Inst.gameMapInfo.mapJson);
        }
        else
        {
            LoggerUtils.Log("GetJson failed");
        }
    }

    IEnumerator GetByte(string url, UnityAction<byte[]> onSuccess, UnityAction<string> onFailure)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log(www.error);
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(www.downloadHandler.data);
        }
    }

    IEnumerator GetText(string url, UnityAction<string> onSuccess, UnityAction<string> onFailure)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            onFailure.Invoke(www.error);
        }
        else
        {
            onSuccess.Invoke(www.downloadHandler.text);
        }
    }

    private bool IsInputNull(InputField inputField)
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            LoggerUtils.Log("Pls input something~");
            TipPanel.ShowToast("Pls input something~");
            return true;
        }

        return false;
    }

    List<string> InitHistoryString(HistoryType hisType)
    {
        var historyList = GetHistoryListByType(hisType);
        if (historyList != null)
        {
            historyList.Clear();
        }

        string hisStr = PlayerPrefs.GetString(hisType.ToString());
        if (string.IsNullOrEmpty(hisStr))
        {
            return new List<string>();
        }

        string[] hisArray = hisStr.Split(';');
        return hisArray.ToList();
    }

    List<string> GetHistoryListByType(HistoryType type)
    {
        switch (type)
        {
            case HistoryType.MAP_JSON: return jsonUrlHistory;
            case HistoryType.MAP_NAME: return mapNameHistory;
            case HistoryType.PROP_JSON: return propHistory;
        }

        return null;
    }

    void SaveToJsonUrlHistory(HistoryType hisType, string str)
    {
        StringBuilder sbRet = new StringBuilder();
        List<string> hisList = GetHistoryListByType(hisType);

        if (hisList.Contains(str))
        {
            LoggerUtils.Log(string.Format("历史记录中已存在{0},将不会保存", str));
        }
        else
        {
            hisList.Add(str);
            for (int i = 0; i < hisList.Count; i++)
            {
                var itemS = hisList[i];
                if (i == hisList.Count - 1)
                {
                    sbRet.Append(itemS);
                }
                else
                {
                    sbRet.Append(string.Format("{0}{1}", itemS, ';'));
                }
            }

            Debug.Log(string.Format("Save to PlayerPrefs{0}:{1}", hisType.ToString(), sbRet.ToString()));
            PlayerPrefs.SetString(hisType.ToString(), sbRet.ToString());
            PlayerPrefs.Save();
        }

        RefreshHistoryDropdown(hisType);
    }

    Dropdown GetDropDownByType(HistoryType type)
    {
        switch (type)
        {
            case HistoryType.MAP_JSON: return jsonUrlDropdown;
            case HistoryType.MAP_NAME: return mapDropdown;
            case HistoryType.PROP_JSON: return propDropdown;
            default: return null;
        }
    }

    void RefreshHistoryDropdown(HistoryType type)
    {
        var dropDown = GetDropDownByType(type);
        var historyList = GetHistoryListByType(type);

        dropDown.options.Clear();
        dropDown.options.Add(new Dropdown.OptionData(""));
        if (historyList == null || historyList.Count <= 0)
        {
            return;
        }

        historyList.Reverse();
        foreach (var s in historyList)
        {
            dropDown.options.Add(new Dropdown.OptionData()
            {
                text = s
            });
        }
    }

    #endregion

#endif
}
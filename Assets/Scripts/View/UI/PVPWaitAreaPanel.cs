using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;

public class PVPWaitAreaPanel : InfoPanel<PVPWaitAreaPanel>
{
    
    public Text GameDurationText;
    public Toggle TeamToggle;
    public Toggle WinToggle;
    public Toggle SurvivalToggle;
    public Toggle SensorToggle;
    public Text WinText;
    public Text SensorText;
    public Text SurvivalWinText;
    public Text TeamText;
    public Button SurvivalNoSwitchBtn;
    public Button TipButton;
    public Button NoSwitchBtn;
    public Button NoSensorBoxBtn;
    public Button NoTeamBtn;
   // public GameObject SwithPanel;
   // public GameObject SwitchParent;
    public GameObject PVPTipPanel;
    public RectTransform PVPLayScollRect;
    public Button TipCloseBtn;
    public Button InputBoardBtn;
    public PVPWaitAreaPoolPanel SwitchButtonPanel;
    public PVPWaitAreaPoolPanel SensorButtonPanel;
    public PVPWaitAreaTeamPanel teamButtonPanel;
    //private GameObject switchPrefab;
    //private List<CommonButtonItem> switchPool = new List<CommonButtonItem>();
    private List<int> switchIds = new List<int>();
    private List<int> sensorUids = new List<int>();
    private int pvpTime;
    private Color inactiveColor = new Color(1f, 1f, 1f,0.5f);
    private PVPWaitAreaBehaviour pvpBehaviour;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        // switchPool.Clear();
        // switchPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "PVPWaitAreaSwitchItem");
        WinToggle.onValueChanged.AddListener(OnWinToggleChange);
        SurvivalToggle.onValueChanged.AddListener(OnSurvivalToggleChange);
        SensorToggle.onValueChanged.AddListener(OnSensorToggleChange);
        TeamToggle.onValueChanged.AddListener(OnTeamToggleChange);
        TipButton.onClick.AddListener(() => PVPTipPanel.SetActive(true));
        TipCloseBtn.onClick.AddListener(OnCloseTip);
        InputBoardBtn.onClick.AddListener(OnShowKeyBoard); 
        NoSwitchBtn.onClick.AddListener(()=> { TipPanel.ShowToast("Please add a Switch Button first");});
        SurvivalNoSwitchBtn.onClick.AddListener(()=> { TipPanel.ShowToast("Please check Health Point first.");});
        NoSensorBoxBtn.onClick.AddListener(()=> { TipPanel.ShowToast("Please add a Sensor Box first");});
        NoTeamBtn.onClick.AddListener(()=> { TipPanel.ShowToast("At least 2 players");});
    }

    private void OnCloseTip()
    {
        PVPTipPanel.SetActive(false);
    }

    public override void OnDialogBecameVisible()
    {
        bool isHasSwitch = IsHasSwitchButton();
        if (isHasSwitch)
        {
            switchIds = SwitchManager.Inst.switchBevs.Keys.ToList();
            SwitchButtonPanel.SetItemList(switchIds,OnSwitchButtonClick);
        }
        else
        {
            SwitchButtonPanel.OnClearPanel();
            SwitchButtonPanel.gameObject.SetActive(false);
        }

        bool isHasSensor = IsHasSensorBox();
        if (isHasSensor)
        {
            sensorUids = SensorBoxManager.Inst.GetIndexList();
            SensorButtonPanel.SetItemList(sensorUids,OnSensorSelect);
        }
        else
        {
            SensorButtonPanel.OnClearPanel();
            SensorButtonPanel.gameObject.SetActive(false);
        }

        bool isTeamMode = IsTeamMode();
        if (isTeamMode)
        {
            teamButtonPanel.RefreshPanel();
        }
    }

    private void OnSensorSelect(int index)
    {
        var comp = pvpBehaviour.entity.Get<PVPWaitAreaComponent>();
        comp.raceData.taskArg = sensorUids[index];
    }
    
    public void SetEntity(SceneEntity entity)
    {
        var comp = entity.Get<PVPWaitAreaComponent>();
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        pvpBehaviour = bindGo.GetComponent<PVPWaitAreaBehaviour>();
        ResetGameMode(comp);
        InitGameMode(comp);
        InitTeamMode(comp);
        switch ((PVPServerTaskType)comp.gameMode)
        {
            case PVPServerTaskType.Race:
                SetSwitchButtonGameMode(comp);
                break;
            case PVPServerTaskType.Survival:
                SetSurvivalGameMode(comp);
                break;
            case PVPServerTaskType.SensorBox:
                SetSensorBoxGameMode(comp);
                break;
        }
    }
    
    private void ResetGameMode(PVPWaitAreaComponent comp)
    {
        var hpComp = SceneBuilder.Inst.HPEntity.Get<HPControlComponent>();
        if (hpComp.setHP == 0)
        {
            if (comp.gameMode == (int)PVPServerTaskType.Survival)
            {
                comp.gameMode = (int)PVPServerTaskType.Race;
            }
        }
    }
    private void InitGameMode(PVPWaitAreaComponent comp)
    {
        bool isHasSwitch = IsHasSwitchButton();
        WinToggle.interactable = isHasSwitch;
        WinText.color = isHasSwitch ? Color.white : inactiveColor;
        NoSwitchBtn.gameObject.SetActive(!isHasSwitch);
        
        bool isHasSensor = IsHasSensorBox();
        SensorToggle.interactable = isHasSensor;
        SensorText.color = isHasSensor ? Color.white : inactiveColor;
        NoSensorBoxBtn.gameObject.SetActive(!isHasSensor);
        
        var hpComp = SceneBuilder.Inst.HPEntity.Get<HPControlComponent>();
        bool isOpenHP = hpComp.setHP == 1;
        SurvivalToggle.interactable = isOpenHP;
        SurvivalNoSwitchBtn.gameObject.SetActive(!isOpenHP);
        SurvivalWinText.color = isOpenHP ? Color.white : inactiveColor;
        pvpTime = comp.raceData.pvpTime;
        GameDurationText.text = pvpTime.ToString();

        bool isTeam=GameManager.Inst.maxPlayer>1;
        TeamToggle.interactable=isTeam;
        NoTeamBtn.gameObject.SetActive(!isTeam);
        TeamText.color=isTeam ? Color.white : inactiveColor;
    }

    private void InitTeamMode(PVPWaitAreaComponent comp)
    {
        if (GameManager.Inst.maxPlayer <= 1 || (comp.teamList == null || comp.teamList.Count <= 0))
        {
            TeamToggle.isOn = false;
            return;
        }
        var teamList = comp.teamList;
        if (teamList != null && teamList.Count > 0)
        {
            TeamToggle.isOn = true;
            teamButtonPanel.InitTeamPanelByData(teamList);
        }
    }

    private void SetSwitchButtonGameMode(PVPWaitAreaComponent comp)
    {
        SurvivalToggle.isOn = false;
        SensorToggle.isOn = false;
        if (SwitchManager.Inst.switchBevs.Count > 0)
        {
            WinToggle.interactable = true;
            WinToggle.isOn = comp.raceData.taskArga == 1;
            SwitchButtonPanel.gameObject.SetActive(comp.raceData.taskArga == 1);
            NoSwitchBtn.gameObject.SetActive(false);
            if (SwitchManager.Inst.switchBevs.ContainsKey(comp.raceData.taskArg))
            {
                int index = switchIds.FindIndex(x => x == comp.raceData.taskArg);
                SwitchButtonPanel.OnButtonClick(index);
            }
        }
        else
        {
            WinToggle.isOn = false;
            WinToggle.interactable = false;
            SwitchButtonPanel.gameObject.SetActive(false);
            NoSwitchBtn.gameObject.SetActive(true);
        }
    }

    private void SetSurvivalGameMode(PVPWaitAreaComponent comp)
    {
        SurvivalToggle.isOn = true;
        SwitchButtonPanel.gameObject.SetActive(false);
    }

    private void SetSensorBoxGameMode(PVPWaitAreaComponent comp)
    {
        WinToggle.isOn = false;
        SurvivalToggle.isOn = false;
        if (SensorBoxManager.Inst.sensorBoxDict.Count > 0)
        {
            SensorToggle.interactable = true;
            SensorToggle.isOn = true;
            SensorButtonPanel.gameObject.SetActive(true);
            NoSensorBoxBtn.gameObject.SetActive(false);
            if (sensorUids.Contains(comp.raceData.taskArg))
            {
                int index = sensorUids.FindIndex(x => x == comp.raceData.taskArg);
                SensorButtonPanel.OnButtonClick(index);
            }
        }
        else
        {
            SensorToggle.interactable = false;
            SensorToggle.isOn = false;
            SensorButtonPanel.gameObject.SetActive(false);
            NoSensorBoxBtn.gameObject.SetActive(true);
        }
    }

    private void OnShowKeyBoard()
    {
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = pvpTime.ToString(),
            inputMode = 1,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = "",
            returnKeyType = (int)ReturnType.Return
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowKeyBoard);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }

    public void ShowKeyBoard(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        int pointNum;
        if (!int.TryParse(str, out pointNum))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        if (pointNum > 999 || pointNum <= 0)
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        pvpBehaviour.entity.Get<PVPWaitAreaComponent>().raceData.pvpTime = pointNum;
        pvpTime = pointNum;
        GameDurationText.text = pvpTime.ToString();
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }


    private void OnWinToggleChange(bool value)
    {
        var comp = pvpBehaviour.entity.Get<PVPWaitAreaComponent>();
        comp.raceData.taskArga = value ? (int)PVPWaitAreaTaskType.SwitchButton : 0;
        if (value)
        {
            SensorButtonPanel.gameObject.SetActive(false);
        }
        SwitchButtonPanel.gameObject.SetActive(value);
        UpdateSwitchPanel();
    }

    private void OnSurvivalToggleChange(bool value)
    {
        if (value)
        {
            SwitchButtonPanel.gameObject.SetActive(false);
            SensorButtonPanel.gameObject.SetActive(false);
        }
        var comp = pvpBehaviour.entity.Get<PVPWaitAreaComponent>();
        comp.gameMode = value ? (int)PVPServerTaskType.Survival : (int)PVPServerTaskType.Race;
    }
    
    private void OnSensorToggleChange(bool value)
    {
        var comp = pvpBehaviour.entity.Get<PVPWaitAreaComponent>();
        comp.gameMode = value?(int)PVPServerTaskType.SensorBox:(int)PVPServerTaskType.Race;
        if (value)
        {
            SwitchButtonPanel.gameObject.SetActive(false);
        }
        SensorButtonPanel.gameObject.SetActive(value);
        UpdateSwitchPanel();
    }

    private void OnTeamToggleChange(bool value)
    {
        var entity = pvpBehaviour.entity;
        var Comp = entity.Get<PVPWaitAreaComponent>();
        teamButtonPanel.SetPVPWaitAreaComponent(Comp);
        teamButtonPanel.gameObject.SetActive(value);
        teamButtonPanel.ShowTeamPanel(value);
        UpdateSwitchPanel();
    }

    void UpdateSwitchPanel()
    {
       LayoutRebuilder.ForceRebuildLayoutImmediate(PVPLayScollRect);
        // yield return new WaitForEndOfFrame();
        // PVPLayScollRect.verticalNormalizedPosition = 0;
    }

    private void OnSwitchButtonClick(int index)
    {
        var comp = pvpBehaviour.entity.Get<PVPWaitAreaComponent>();
        comp.raceData.taskArg = switchIds[index];
    }
    
    private bool IsHasSwitchButton()
    {
        return SwitchManager.Inst.switchBevs != null && SwitchManager.Inst.switchBevs.Count > 0;
    }

    private bool IsHasSensorBox()
    {
        return SensorBoxManager.Inst.sensorBoxDict != null && SensorBoxManager.Inst.sensorBoxDict.Count > 0;
    }

    private bool IsTeamMode()
    {
        return PVPTeamManager.Inst.IsTeamMode();
    }

    private void OnDisable()
    {
        PVPTipPanel.SetActive(false);
    }

}

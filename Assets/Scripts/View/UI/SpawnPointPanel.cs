/// <summary>
/// Author:Mingo-LiZongMing
/// Description:出生点设置面板
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GRTools.Localization;

public class SpawnPointPanel : InfoPanel<SpawnPointPanel>
{
    public Button InputBtn;
    public Button btnPlusHP;
    public Button btnSubHP;
    public Text HPText;
    public GameObject HPAdjust;
    public ScrollRect scrollRect;
    public Text HPTitle, BagTitle;
    private const int MAX_HP = 999;
    private const int defaultHp = 100;
    public Toggle ToggleHPPermision;
    public Toggle ToggleBaggagePermision;
    public Button isOnBtn;
    public Image clickImg;
    [SerializeField]
    private GameObject DamagePanel;
    [SerializeField]
    private MToggle PlayerToggle;

    [SerializeField]
    private MToggle TrapBoxToggle;
    [SerializeField]
    private MToggle FireToggle;
    public SpawnPointBehaviour curBehav;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        ToggleHPPermision.onValueChanged.AddListener(OnToggleClick);
        if (SceneParser.Inst.GetHPSet() != 0)
        {
            ToggleHPPermision.isOn = true;
        }

        HPAdjust.SetActive(ToggleHPPermision.isOn);
        InputBtn.onClick.AddListener(OnShowKeyBoard);
        btnPlusHP.onClick.AddListener(OnBtnPlusHPClick);
        btnSubHP.onClick.AddListener(OnBtnSubHPClick);
        isOnBtn.onClick.AddListener(OnDefaultSpawnClick);
        ToggleBaggagePermision.onValueChanged.AddListener(OnBaggageToggleClick);
        if (SceneParser.Inst.GetBaggageSet() != 0)
        {
            ToggleBaggagePermision.isOn = true;
        }

        PlayerToggle.ToggleBtn.onClick.AddListener(OnPlayerToggleClick);
        TrapBoxToggle.ToggleBtn.onClick.AddListener(OnTrapBoxToggleClick);
        FireToggle.ToggleBtn.onClick.AddListener(OnFireToggleClick);

        InitDamagePanel();
    }

    public void SetBehav(GameObjectComponent gComp)
    {
        var bindgo = gComp.bindGo;
        curBehav = bindgo.GetComponent<SpawnPointBehaviour>();
        clickImg.gameObject.SetActive(curBehav.id == SpawnPointManager.Inst.defaultSpawnId ? true : false);
        HPText.text = curBehav.hpValue.ToString();
    }

    private void InitDamagePanel()
    {
        List<int> dmgList = SceneParser.Inst.GetDamageSources();
        bool isPlayerHit = dmgList.Contains((int)DamageSource.Player);
        PlayerToggle.SetIsOn(isPlayerHit);

        bool isTrapBoxHit = dmgList.Contains((int)DamageSource.TrapBox);
        TrapBoxToggle.SetIsOn(isTrapBoxHit);

        bool isFireHit = dmgList.Contains((int)DamageSource.Fire);
        FireToggle.SetIsOn(isFireHit);
    }

    public void UpdateTeamInfo()
    {
        var teamList = PVPTeamManager.Inst.GetRefreshTeamList();
        SceneBuilder.Inst.UpdateBronPointTeamIDState(teamList);
    }

    private void CheckPvpTeamToast(Action action)
    {
        if (PVPTeamManager.Inst.isSwap == true)
        {
            SecondComfirmPanel.Show();
            SecondComfirmPanel.Instance.SetTitle("This operation will reset the team assignment. Are you sure you want to continue?", "Cancel", "Continue");
            SecondComfirmPanel.Instance.LeftBthClickAct = null;
            SecondComfirmPanel.Instance.RightBtnClickAct =
                () => {
                    action?.Invoke();
                    UpdateTeamInfo();
                };
        }
        else
        {
            action?.Invoke();
            UpdateTeamInfo();
        }
    }

    private void OnToggleClick(bool isToggle)
    {
        SceneBuilder.Inst.HPEntity.Get<HPControlComponent>().setHP = isToggle ? 1 : 0;
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null&&!isToggle)
        {
            var comp = PVPWaitAreaManager.Inst.PVPBehaviour.entity.Get<PVPWaitAreaComponent>();
            if (comp.gameMode == (int)PVPServerTaskType.Survival)
            {
                comp.gameMode = (int)PVPServerTaskType.Race;
            }
        }

        //根据产品要求清除伤害来源设置
        if(isToggle == false)
        {   
            SceneParser.Inst.ResetDamageSources();
            InitDamagePanel();
        }
        

        HPAdjust.SetActive(isToggle);
        if (isToggle)
        {
            //列表滑动到底部，显示自定义血量按钮
            scrollRect.verticalNormalizedPosition = 0;
        }
    }

    private void OnBtnPlusHPClick()
    {
        if (curBehav.hpValue >= MAX_HP)
        {
            TipPanel.ShowToast("Up to {0}", 999);
            return;
        }
        curBehav.hpValue += 1;
        HPText.text = curBehav.hpValue.ToString();
    }

    private void OnBtnSubHPClick()
    {
        if (curBehav.hpValue <= 1)
        {
            TipPanel.ShowToast("At least {0}", 1);
            return;
        }
        curBehav.hpValue -= 1;
        HPText.text = curBehav.hpValue.ToString();
    }

    private void OnShowKeyBoard()
    {
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = curBehav.hpValue.ToString(),
            inputMode = 1,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = "Oops! Exceed limit:(",
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
        int HPNum;
        if (!int.TryParse(str, out HPNum))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        if (HPNum > MAX_HP || HPNum <= 0)
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        curBehav.hpValue = HPNum;
        HPText.text = curBehav.hpValue.ToString();
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }

    private void OnPlayerToggleClick()
    {
        LoggerUtils.Log("OnPlayerToggleClick");
        if(PlayerToggle.isOn == true 
            && TrapBoxToggle.isOn == false
            && FireToggle.isOn == false)
        {
            ShowLimitSelectToast();
            return;
        }
        bool toState = !PlayerToggle.isOn;
        string ctrType = toState == true ? "Add":"Remove";
        PlayerToggle.SetIsOn(toState);
        SceneParser.Inst.UpdateDamageSources(ctrType,(int)DamageSource.Player);
    }

    private void OnTrapBoxToggleClick()
    {
        if(TrapBoxToggle.isOn == true 
            && PlayerToggle.isOn == false
             && FireToggle.isOn == false)
        {
            ShowLimitSelectToast();
            return;
        }
        bool toState = !TrapBoxToggle.isOn;
        string ctrType = toState == true ? "Add":"Remove";
        TrapBoxToggle.SetIsOn(toState);
        SceneParser.Inst.UpdateDamageSources(ctrType,(int)DamageSource.TrapBox);
    } 
    private void OnFireToggleClick()
    {
        if(TrapBoxToggle.isOn == false 
            && PlayerToggle.isOn == false
            &&FireToggle.isOn==true)
        {
            ShowLimitSelectToast();
            return;
        }
        bool toState = !FireToggle.isOn;
        string ctrType = toState == true ? "Add":"Remove";
        FireToggle.SetIsOn(toState);
        SceneParser.Inst.UpdateDamageSources(ctrType,(int)DamageSource.Fire);
    }

      //必须至少勾选一个选项
    private void ShowLimitSelectToast()
    {
        string tipsText = LocalizationConManager.Inst.GetLocalizedText("Please select at least one option");
        TipPanel.ShowToast("{0}", tipsText);
    }

    private void OnBaggageToggleClick(bool isToggle)
    {
        SceneBuilder.Inst.BaggageEntity.Get<BaggageComponent>().openBaggage = isToggle ? 1 : 0;
    }

    /// <summary>
    /// 火焰道具会自动打开按钮
    /// </summary>
    /// <param name="isOn"></param>
    /// <returns></returns>
    public bool TurnFireToggleOn()
    {
        bool stateChanged = false;
        if (!ToggleHPPermision.isOn)
        {
            ToggleHPPermision.isOn = true;
            stateChanged = true;
        }
        if (!FireToggle.isOn)
        {
            OnFireToggleClick();
            stateChanged = true;
        }
        return stateChanged;      
    }

    public void OnDefaultSpawnClick()
    {
        if(curBehav.id == SpawnPointManager.Inst.defaultSpawnId)
        {
            return;
        }
        if (curBehav != null)
        {
            SpawnPointManager.Inst.defaultSpawnId = curBehav.id;
            clickImg.gameObject.SetActive(true);
        }
    }
}

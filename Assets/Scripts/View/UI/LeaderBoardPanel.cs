using Newtonsoft.Json;
using SavingData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardPanel : InfoPanel<LeaderBoardPanel>
{
    public Toggle winToggle;
    public GameObject CloseWin;
    public SceneEntity curEntity;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        winToggle.onValueChanged.AddListener(OnWinSelect);
        CloseWin.GetComponent<Button>().onClick.AddListener(OnCloseWinClick);
    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        var comp = curEntity.Get<LeaderBoardComponent>();
        var temp = comp.curMode == (int)LeaderBoardModeType.Win ? true : false;
        SetToggleSwitch(PVPWaitAreaManager.Inst.PVPBehaviour);
        winToggle.isOn = temp;
    }

    private void OnWinSelect(bool isOn)
    {
        var temp = isOn == true ? (int)LeaderBoardModeType.Win : (int)LeaderBoardModeType.None;
        curEntity.Get<LeaderBoardComponent>().curMode = temp;
        var hasLeaderBoard = LeaderBoardManager.Inst.DetectHasLeaderBoard() == true ? 1 : 0;
    }



    /// <summary>
    /// true为开，false为关
    /// </summary>
    /// <param name="isActive"></param>
    public void SetToggleSwitch(bool isActive)
    {
        CloseWin.gameObject.SetActive(!isActive);
        winToggle.gameObject.SetActive(isActive);
    }

    private void OnCloseWinClick()
    {
        TipPanel.ShowToast("Please add Waiting Zone first");
    }
}

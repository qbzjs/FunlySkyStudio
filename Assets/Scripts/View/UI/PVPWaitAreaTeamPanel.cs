/// <summary>
/// Author:Mingo-LiZongMing
/// Description:PVP对局分队-控制分队信息和数据保存
/// Date: 2022-6-24 14:08:22
/// </summary>
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public enum TeamName
{
    TeamA = 0,
    TeamB = 1,
}

public class PVPWaitAreaTeamPanel : MonoBehaviour
{
    public GameObject TeamAContent;
    public GameObject TeamBContent;

    public GameObject buttonPrefab;
    public GameObject txtTeamA;
    public GameObject txtTeamB;
    public GameObject buttonSwap;

    public GameObject BtnSwapIsSelect;
    public GameObject BtnSwapUnSelect;

    public RectTransform rect;

    private PVPWaitAreaComponent Comp;

    private Dictionary<int, MemberButtonItem> buttonPool = new Dictionary<int, MemberButtonItem>();
    private List<List<int>> teamInfoList = new List<List<int>>();

    private List<int> curSelectTeamA = new List<int>();
    private List<int> curSelectTeamB = new List<int>();

    private void Awake()
    {
        buttonSwap.GetComponent<Button>().onClick.AddListener(SwapTeamMember);
        SetBtnSwapState(false);
    }

    public void RefreshPanel()
    {
        if (buttonPool.Count > 0 && buttonPool.Count != GameManager.Inst.maxPlayer)
        {
            InitTeamPanel();
        }
    }

    public void SetPVPWaitAreaComponent(PVPWaitAreaComponent comp)
    {
        Comp = comp;
    }

    public void ShowTeamPanel(bool isActive)
    {
        if (isActive)
        {
            InitTeamPanel();
        }
        else
        {
            ResetTeamPanel();
        }
    }

    public void InitTeamPanel()
    {
        var tInfoList = PVPTeamManager.Inst.GetRefreshTeamList();
        InitTeamPanelByData(tInfoList);
    }

    public void InitTeamPanelByData(List<List<int>> teamList)
    {
        ResetMemberList();
        teamInfoList = teamList;
        Comp.teamList = teamInfoList;
        InitMemberButton();
        SceneBuilder.Inst.UpdateBronPointTeamIDState(teamList);
    }

    private void ResetTeamPanel()
    {
        ResetMemberList();
        SceneBuilder.Inst.UpdateBronPointTeamIDState(null);
    }

    private void InitMemberButton()
    {
        InitMemberList();
        UpdateSwitchPanel();
    }

    private void InitMemberList()
    {
        foreach (var member in teamInfoList[(int)TeamName.TeamA])
        {
            InitMemberItem(member,TeamName.TeamA);
        }
        foreach (var member in teamInfoList[(int)TeamName.TeamB])
        {
            InitMemberItem(member, TeamName.TeamB);
        }
    }

    private void ResetMemberList()
    {
        foreach (var obj in buttonPool.Values)
        {
            Destroy(obj.gameObject);
        }
        buttonPool.Clear();
        curSelectTeamA.Clear();
        curSelectTeamB.Clear();
        Comp.teamList = null;
    }

    private void InitMemberItem(int index,TeamName team)
    {
        MemberButtonItem itemScript = null;
        var item = Instantiate(buttonPrefab);
        itemScript = item.GetComponent<MemberButtonItem>();
        itemScript.Init();
        itemScript.gameObject.SetActive(true);
        itemScript.SetText(index.ToString());
        itemScript.AddClick(() => OnButtonClick(index));
        buttonPool.Add(index, itemScript);
        switch (team)
        {
            case TeamName.TeamA:
                item.transform.SetParent(TeamAContent.transform);
                break;
            case TeamName.TeamB:
                item.transform.SetParent(TeamBContent.transform);
                break;
        }
        var rectComp = itemScript.GetComponent<RectTransform>();
        rectComp.localScale = Vector3.one;
        rectComp.anchoredPosition3D = new Vector3(rectComp.anchoredPosition3D.x, rectComp.anchoredPosition3D.y, 0);
    }

    private void UpdateSwitchPanel()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    private void OnButtonClick(int index)
    {
        var isSelected = buttonPool[index].GetSelectState();
        if (isSelected)
        {
            buttonPool[index].SetSelectState(false);
            RemoveCurSelectMember(index);
        }
        else
        {
            buttonPool[index].SetSelectState(true);
            RecordCurSelectMember(index);
        }
        var swapState = (curSelectTeamA.Count > 0 || curSelectTeamB.Count > 0) ? true : false;
        SetBtnSwapState(swapState);
    }

    private void RecordCurSelectMember(int index)
    {
        if (teamInfoList[(int)TeamName.TeamA].Contains(index) && !curSelectTeamA.Contains(index))
        {
            curSelectTeamA.Add(index);
        }
        else if (teamInfoList[(int)TeamName.TeamB].Contains(index) && !curSelectTeamB.Contains(index))
        {
            curSelectTeamB.Add(index);
        }
    }

    private void RemoveCurSelectMember(int index)
    {
        if (teamInfoList[(int)TeamName.TeamA].Contains(index) && curSelectTeamA.Contains(index))
        {
            curSelectTeamA.Remove(index);
        }
        else if (teamInfoList[(int)TeamName.TeamB].Contains(index) && curSelectTeamB.Contains(index))
        {
            curSelectTeamB.Remove(index);
        }
    }

    private void ClearSelectState()
    {
        foreach(var member in buttonPool.Values)
        {
            member.SetSelectState(false);
        }
        SetBtnSwapState(false);
    }

    private void SwapTeamMember()
    {
        var TeamAList = teamInfoList[(int)TeamName.TeamA];
        var TeamBList = teamInfoList[(int)TeamName.TeamB];
        if ((TeamAList.Count + curSelectTeamB.Count) - curSelectTeamA.Count > 0)
        {
            foreach (var member in curSelectTeamA)
            {
                TeamAList.Remove(member);
                TeamBList.Add(member);
                buttonPool[member].transform.SetParent(TeamBContent.transform);
            }
            PVPTeamManager.Inst.isSwap=true;
        }
        else
        {
            TipPanel.ShowToast("At least 1 player each team");
        }

        if ((TeamBList.Count + curSelectTeamA.Count) - curSelectTeamB.Count > 0)
        {
            foreach (var member in curSelectTeamB)
            {
                TeamBList.Remove(member);
                TeamAList.Add(member);
                buttonPool[member].transform.SetParent(TeamAContent.transform);
            }
            PVPTeamManager.Inst.isSwap=true;
        }
        else
        {
            TipPanel.ShowToast("At least 1 player each team");
        }

        teamInfoList = new List<List<int>>();
        teamInfoList.Add(TeamAList); 
        teamInfoList.Add(TeamBList);
        SortTeamList(teamInfoList);
        Comp.teamList = teamInfoList;
        LoggerUtils.Log("teamInfoList = " + JsonConvert.SerializeObject(teamInfoList));
        curSelectTeamA.Clear();
        curSelectTeamB.Clear();
        ClearSelectState();
        UpdateSwitchPanel();
        SceneBuilder.Inst.UpdateBronPointTeamIDState(Comp.teamList);
    }

    /// <summary>
    /// 将分队名单升序排序
    /// </summary>
    private void SortTeamList(List<List<int>> teamList)
    {
        foreach (var team in teamList)
        {
            team.Sort();
            for (int i = 0; i < team.Count; i++)
            {
                buttonPool[team[i]].transform.SetSiblingIndex(i);
            }
        }
    }

    private void SetBtnSwapState(bool isActive)
    {
        BtnSwapUnSelect.SetActive(!isActive);
        BtnSwapIsSelect.SetActive(isActive);
    }
}

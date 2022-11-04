/// <summary>
/// Author:Mingo-LiZongMing
/// Description:分队信息管理Manager
/// Date: 2022-6-24 14:08:22
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using UnityEngine;

public class PVPTeamManager : CInstance<PVPTeamManager>
{
    public bool isSwap = false;

    public Dictionary<string, int> playerTeamData = new Dictionary<string, int>();

    /// <summary>
    /// 获取分队信息
    /// </summary>
    public List<List<int>> GetRefreshTeamList()
    {
        isSwap = false;
        //此次为出生点序号，不是数组下标
        var maxPlayer = GameManager.Inst.maxPlayer;
        if(maxPlayer == GameConsts.MIN_PLAYER)
        {
            CloseTeamMode();
            return null;
        }
        List<List<int>>  teamInfoList = new List<List<int>>();
        var TeamAList = new List<int>();
        var TeamBList = new List<int>();
        var teamBCount = maxPlayer / 2;
        var teamACount = maxPlayer - teamBCount;
        for (int i = 1; i <= teamACount; i++)
        {
            TeamAList.Add(i);
        }
        for (int i = teamACount + 1; i <= maxPlayer; i++)
        {
            TeamBList.Add(i);
        }
        teamInfoList.Add(TeamAList);
        teamInfoList.Add(TeamBList);
        SetTeamListData(teamInfoList);
        return teamInfoList;
    }

    private void CloseTeamMode()
    {
        var pvpBev = PVPWaitAreaManager.Inst.PVPBehaviour;
        if (pvpBev != null)
        {
            var entity = pvpBev.entity;
            var pvpComp = entity.Get<PVPWaitAreaComponent>();
            pvpComp.teamList = null;
        }
    }

    private void SetTeamListData(List<List<int>> teamList)
    {
        var pvpBev = PVPWaitAreaManager.Inst.PVPBehaviour;
        if (pvpBev != null)
        {
            var entity = pvpBev.entity;
            var pvpComp = entity.Get<PVPWaitAreaComponent>();
            pvpComp.teamList = teamList;
        }
    }

    /// <summary>
    /// 是否开启了分队模式
    /// </summary>
    /// <returns></returns>
    public bool IsTeamMode()
    {
        var pvpBev = PVPWaitAreaManager.Inst.PVPBehaviour;
        if (pvpBev != null)
        {
            var entity = pvpBev.entity;
            var pvpComp = entity.Get<PVPWaitAreaComponent>();
            if(pvpComp.teamList != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 判断当前玩家是否和自己是一队的，是则返回True不是则返回False
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public bool CheckOtherIsTeammates(string otherPlayerId)
    {
        var selfTeamId = Player.TeamId;
        var otherPlayerInfo = PlayerInfoManager.GetMgobePlayerInfoById(otherPlayerId);
        if(otherPlayerInfo != null)
        {
            return selfTeamId == otherPlayerInfo.TeamId;
        }
        return false;
    }

    /// <summary>
    /// 判断自己是否为最后的幸存者
    /// </summary>
    /// <returns></returns>
    public bool IsLastSurvivor() {
        var selfTeamId = Player.TeamId;
        var TeamMateList = new List<string>();
        for (int idx = 0; idx < Global.Room.RoomInfo.PlayerList.Count; idx++)
        {
            var playerInfo = Global.Room.RoomInfo.PlayerList[idx];
            if(playerInfo.TeamId == selfTeamId) {
                TeamMateList.Add(playerInfo.Id);
            }
        }
        foreach (var teamMate in TeamMateList)
        {
            if (!PlayerManager.Inst.GetPlayerDeathState(teamMate))
            {
                return false;
            }
        }
        return true;
    }

    public void UpdateTeamInfo()
    {
        var teamList = GetRefreshTeamList();
        SceneBuilder.Inst.UpdateBronPointTeamIDState(teamList);
        TipPanel.ShowToast("The team assignment has been reset.");
    }

}

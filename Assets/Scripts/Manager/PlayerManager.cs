using BudEngine.NetEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class PlayerManager : CInstance<PlayerManager>, IPVPManager
{
    private PlayerBaseControl _selfPlayer;
    public PlayerBaseControl selfPlayer
    {
        get
        {
            if (_selfPlayer == null)
            {
                GameObject playerObj = GameObject.Find("PlayerNode").gameObject.transform.Find("Player").gameObject;
                _selfPlayer = playerObj.GetComponent<PlayerBaseControl>();
            }

            return _selfPlayer;
        }
    }
    
    public Dictionary<string, OtherPlayerCtr> otherPlayerDataDic = new Dictionary<string, OtherPlayerCtr>();
    public Dictionary<string, float> allPlayerHp = new Dictionary<string, float>();
    public Dictionary<string, bool> playerDeath = new Dictionary<string, bool>();
    public void OnReset()
    {
        List<string> handInHandPlayer = new List<string>();
        Dictionary<string,string> tempHanderDic = new Dictionary<string, string>();

        if (selfPlayer != null && PlayerEmojiControl.Inst.textCharBev != null && MutualManager.Inst.holdingHandsPlayersDict != null)
        {
            foreach (var kValue in MutualManager.Inst.holdingHandsPlayersDict)
            {
                tempHanderDic.Add(kValue.Key,kValue.Value);
            }

            foreach (var kValue in tempHanderDic)
            {
                if (!handInHandPlayer.Contains(kValue.Key))
                {
                    handInHandPlayer.Add(kValue.Key);
                }

                if (!handInHandPlayer.Contains(kValue.Value))
                {
                    handInHandPlayer.Add(kValue.Value);
                }
                //PlayersReleaseHands方法会移除holdingHandsPlayersDict数据,导致代码后续不执行
                MutualManager.Inst.PlayersReleaseHands(kValue.Key, kValue.Value);
            }
        }
        foreach (var otherPlayerData in otherPlayerDataDic)
        {
            if (!handInHandPlayer.Contains(otherPlayerData.Key))
            {
                otherPlayerData.Value.OnRestart();
            }
        }
        selfPlayer.PlayerResetIdle();
    }


    public void ReturnPVPWaitArea()
    {
        var waitPosition = PVPWaitAreaManager.Inst.PVPBehaviour.transform.position;
        SelfReturnWaitAreaByDealth();
        foreach (var otherPlayerCtr in otherPlayerDataDic.Values)
        {
            otherPlayerCtr.transform.position = waitPosition;
            otherPlayerCtr.transform.rotation = Quaternion.identity;
            otherPlayerCtr.m_PlayerPos = waitPosition;
        }
    }

    public void ReturnInitPos(PVPGameState gameState)
    {
        if (gameState == PVPGameState.Start)
        {
            ReturnSpawnPoint();
        }
        else
        {
            SelfReturnWaitAreaByDealth();
        }
    }

    public void ReturnSpawnPoint()
    {
        selfPlayer.WaitForShow();
        foreach (var otherPlayerCtr in otherPlayerDataDic)
        {
            var spawnId = SpawnPointManager.Inst.GetPlayerSpawnId(otherPlayerCtr.Key);
            var spawnBehav = SpawnPointManager.Inst.GetSpawnPointBehavById(spawnId);
            var spwanPoint = spawnBehav.transform.localPosition;
            otherPlayerCtr.Value.transform.position = spwanPoint;
            otherPlayerCtr.Value.transform.rotation = Quaternion.identity;
            otherPlayerCtr.Value.m_PlayerPos = spwanPoint;
        }
    }
    
    public void SelfReturnWaitAreaByDealth()
    {
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && selfPlayer != null)
        {
            selfPlayer.SetPlayerPosAndRot(PVPWaitAreaManager.Inst.PVPBehaviour.transform.position,
                Quaternion.identity);
        }
    }


    //开启所有人头顶状态显示
    public void StartShowPlayerState()
    {
        ShowPlayerState(Player.Id);
        foreach (var other in otherPlayerDataDic.Keys)
        {
            ShowPlayerState(other);
        }
    }

    public void ShowPlayerState(string id, bool isInit = true)
    {
        if (SceneParser.Inst.GetHPSet() != 0)//开启血条
        {
            SetHPVisiable(id, isInit);
        }
        else if (PVPTeamManager.Inst.IsTeamMode())//未开启血条且为分队模式
        {
            SetTeamLogoVisiable(id, true);
        }
    }
    /// <summary>
    /// 设置血条显隐
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isInit"></param>
    public void SetHPVisiable(string id, bool isInit = true)
    {
        if (id == Player.Id)
        {
            if (selfPlayer != null)
            {
                var selfCon = selfPlayer.GetComponentInChildren<CharBattleControl>(true);
                if (selfCon == null)
                {
                    selfCon = selfPlayer.gameObject.AddComponent<CharBattleControl>();
                    selfCon.playerType = PlayerType.self;
                }
                selfCon.ShowState(id,isInit);
                var teamState = GetPlayerTeamState(id);
                selfCon.SetHPColor(teamState);
            }
        }
        else if (otherPlayerDataDic.ContainsKey(id))
        {
            var otherCon = otherPlayerDataDic[id].GetComponentInChildren<CharBattleControl>(true);
            if (otherCon == null)
            {
                otherCon = otherPlayerDataDic[id].gameObject.AddComponent<CharBattleControl>();
                otherCon.playerType = PlayerType.other;
            }
            otherCon.ShowState(id,isInit);
            var teamState = GetPlayerTeamState(id);
            otherCon.SetHPColor(teamState);
        }
    }
    /// <summary>
    /// 设置分队模式下且未开启血条时的标志显隐
    /// </summary>
    public void SetTeamLogoVisiable(string id, bool isShow = true)
    {
        if (id == Player.Id)
        {
            if (selfPlayer != null)
            {
                var selfteamLogo = selfPlayer.GetComponentInChildren<TeamLogo>(true);
                if (selfteamLogo != null)
                {
                    selfteamLogo.SetVisiable(isShow);
                    var teamState = GetPlayerTeamState(id);
                    selfteamLogo.SetTeamLogoColor(teamState);
                }
            }
        }
        else if (otherPlayerDataDic.ContainsKey(id))
        {
            var otherteamLogo = otherPlayerDataDic[id].GetComponentInChildren<TeamLogo>(true);
            if (otherteamLogo != null)
            {
                otherteamLogo.SetVisiable(isShow);
                var teamState = GetPlayerTeamState(id);
                otherteamLogo.SetTeamLogoColor(teamState);
            }
        }
    }
    /// <summary>
    /// 获取队员的是队友还是敌人
    /// </summary>
    public int GetPlayerTeamState(string playerId)
    {
        if (PVPTeamManager.Inst.IsTeamMode())
        {
            if (playerId == Player.Id)
            {
                return (int)TeamMemberState.Self;
            }
            //判断OtherPlayer是否是队友
            if (PVPTeamManager.Inst.CheckOtherIsTeammates(playerId))
            {
                return (int)TeamMemberState.TeamMate;
            }
            else
            {
                return (int)TeamMemberState.Enem;
            }
        }
        else
        {
            return (int)TeamMemberState.Normal;
        }
    }

    public void UpdatePlayerHPVisibleByReconnect()
    {
        foreach (var playerState in playerDeath)
        {
            SetPlayerHPVisible(playerState.Key,!playerState.Value);
        }
    }

    public void SetPlayerHPVisible(string playerId, bool isVisible)
    {
        if (SceneParser.Inst.GetHPSet() == 0 || selfPlayer == null)
        {
            return;
        }

        if (playerId == Player.Id)
        {
            var selfCon = selfPlayer.GetComponent<CharBattleControl>();
            if (selfCon != null)
            {
                selfCon.SetHPVisible(isVisible);
                //selfCon.SetHPValue(CharBattleControl.maxHP,false);//这里不是重置血条的地方。
                if (PlayModePanel.Instance)
                {
                    PlayModePanel.Instance.ShowFpsPlayerHpPanel(isVisible);
                }
            }
            return;
        }

        if (otherPlayerDataDic.ContainsKey(playerId))
        {
            var otherCon = otherPlayerDataDic[playerId].GetComponent<CharBattleControl>();
            if (otherCon != null)
            {
                otherCon.SetHPVisible(isVisible);
            }
        }
    }


    //关闭所有人头顶状态显示
    public void ExitShowPlayerState()
    {
        if (SceneParser.Inst.GetHPSet() != 0)//开启血条
        {
            ExitShowHP();
        }
        else if (PVPTeamManager.Inst.IsTeamMode())
        {
            ExitShowTeamLogo();
        }

    }
    public void ExitShowHP()
    {
        if (selfPlayer != null)
        {
            var selfCon = selfPlayer.GetComponent<CharBattleControl>();
            if (selfCon != null)
            {
                selfCon.ExitShowBattle(Player.Id);
            }
        }
        foreach (var other in otherPlayerDataDic.Keys)
        {
            var otherCon = otherPlayerDataDic[other].GetComponent<CharBattleControl>();
            if (otherCon != null)
            {
                otherCon.ExitShowBattle(other);
            }
        }
    }

    /// <summary>
    /// 关闭所有分分队标志显示
    /// </summary>
    public void ExitShowTeamLogo()
    {
        if (selfPlayer != null)
        {
            var selfteamlLogo = selfPlayer.GetComponentInChildren<TeamLogo>(true);
            if (selfteamlLogo != null)
            {
                selfteamlLogo.ExitShowTeamLogo();
            }
        }
        foreach (var other in otherPlayerDataDic.Keys)
        {
            var otherteamlLogo = otherPlayerDataDic[other].GetComponentInChildren<TeamLogo>(true);
            if (otherteamlLogo != null)
            {
                otherteamlLogo.ExitShowTeamLogo();
            }
        }
    }

    public void OnPlayerDeath(string playerID)
    {
        PromoteManager.Inst.OnPlayerDeath(playerID);
        FreezePropsManager.Inst.OnPlayerDeath(playerID);
        var handState = MutualManager.Inst.SearchHandStateOnPlayers(playerID);
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null)
        {
            var comp = PVPWaitAreaManager.Inst.PVPBehaviour.entity.Get<PVPWaitAreaComponent>();
            switch ((PVPServerTaskType)comp.gameMode)
            {
                case PVPServerTaskType.SensorBox:
                case  PVPServerTaskType.Race:
                    //被牵手位置不重置
                    if(handState != PlayerHandleState.PassiveHand)
                        {
                            if (playerID == Player.Id)
                            {
                                PlayerBaseControl.Inst.SetPosToSpawnPoint();
                            }
                            else if (playerID != Player.Id && otherPlayerDataDic.ContainsKey(playerID))
                            {
                                var otherCon = otherPlayerDataDic[playerID];
                                if (otherCon != null)
                                {
                                    otherCon.m_PlayerPos = SpawnPointManager.Inst.GetSpawnPoint().transform.localPosition;
                                    otherCon.OnRestart();
                                }
                            }
                        }
                        break;
                case PVPServerTaskType.Survival:
                        //被牵手位置不重置
                        if (handState != PlayerHandleState.PassiveHand)
                        {
                            if (playerID == Player.Id)
                            {
                                selfPlayer.SetPlayerPosAndRot(PVPWaitAreaManager.Inst.PVPBehaviour.transform.position, Quaternion.identity);
                                PVPManager.Inst.OnSelfDeath();

                            }
                            else if (playerID != Player.Id && otherPlayerDataDic.ContainsKey(playerID))
                            {
                                var otherCon = otherPlayerDataDic[playerID];
                                if (otherCon != null)
                                {
                                    otherCon.m_PlayerPos = PVPWaitAreaManager.Inst.PVPBehaviour.transform.position;
                                    otherCon.OnRestart();
                                }
                            }
                        }
                        SetPlayerHPVisible(playerID,false);
                        UpdatePlayerDeathState(playerID,true);
                        break;
            }
        }
        else
        {
                //被牵手位置不重置
                if (handState != PlayerHandleState.PassiveHand)
                {
                    if (playerID == Player.Id)
                    {
                        PlayerBaseControl.Inst.SetPosToSpawnPoint();
                    }
                    else if (playerID != Player.Id && otherPlayerDataDic.ContainsKey(playerID))
                    {
                        var otherCon = otherPlayerDataDic[playerID];
                        if (otherCon != null)
                        {
                            otherCon.m_PlayerPos = SpawnPointManager.Inst.GetSpawnPoint().transform.localPosition;
                            otherCon.OnRestart();
                        }
                    }
                }
            }
    }


    public void ClearPlayersDeathState()
    {
        playerDeath.Clear();
    }

    public bool GetPlayerDeathState(string playerID)
    {
        if (playerDeath.ContainsKey(playerID))
        {
            return playerDeath[playerID];
        }
        return false;
    }

    public void UpdatePlayerDeathState(string playerID,bool isDeath)
    {
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null)
        {
            var comp = PVPWaitAreaManager.Inst.PVPBehaviour.entity.Get<PVPWaitAreaComponent>();
            if (comp.gameMode == (int)PVPServerTaskType.Survival)
            {
                if (!playerDeath.ContainsKey(playerID))
                {
                    playerDeath.Add(playerID,isDeath);
                }
                else
                {
                    playerDeath[playerID] = isDeath;
                }
            }
        }
    }
}
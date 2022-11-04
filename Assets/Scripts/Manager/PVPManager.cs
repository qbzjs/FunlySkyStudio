
using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using BudEngine.NetEngine.src.BattleGame;
using Newtonsoft.Json;
using UnityEngine;
using static ClientManager;
/// <summary>
/// Author:Mingo-LiZongMing
/// Description:PVPManager-处理PVP对局广播的收发和逻辑处理
/// </summary>
public class PVPManager  : CInstance<PVPManager>
{
    public enum PVPGameMode
    {
        GameState = 1,
    }
    public Action<PVPGameConnectEnum,PVPSyncData> OnGameState;
    
    public Action<SurvivalSyncData> OnSelfDealth;

    public bool IsBackgroundMsg = false;
    /// <summary>
    /// 获取PVP房间状态
    /// </summary>
    public void OnReceiveServer(SendGameBst bst)
    {
        if (PVPWaitAreaManager.Inst.PVPBehaviour == null || IsBackgroundMsg)
        {
            return;
        }
        if (bst == null || string.IsNullOrEmpty(bst.GameData))
        {
            LoggerUtils.Log("PVPManager gameData is Null!");
            return;
        }
        LoggerUtils.Log("bst.GameData===="+bst.GameData);
        PVPGameMode reqType = (PVPGameMode)bst.ReqType;
        switch (reqType)
        {
            case PVPGameMode.GameState:
                PVPSyncData rspData = JsonConvert.DeserializeObject<PVPSyncData>(bst.GameData);
                GlobalFieldController.PVPRound = rspData.round;
                OnGameState?.Invoke(PVPGameConnectEnum.Normal,rspData);
                break;
          
        }
    }
    
    /// <summary>
    /// U3d主动向服务端发送请求，获取房间状态
    /// 1-游戏准备 2-游戏开始，3-游戏结束，4-时间校准
    /// </summary>
    /// <param name="gameData"></param>
    public void SendPVPGameDataReq(string gameData)
    {
        LoggerUtils.Log("SendPVPGameDataReq gameData = " + gameData);
        ClientManager.Inst.Battle_SendGameData(BattleReqType.SendGameData, gameData, null);
    }
    
    public void PVPGetGameInfo(Action callback = null)
    {
        if(PVPWaitAreaManager.Inst.PVPBehaviour == null)
            return;
        var restoreMap = callback;
        ClientManager.Inst.Battle_GetGameInfo(BattleGameType.PVP, BattleCommand.BATTLESTATE, (info) =>
        {
            IsBackgroundMsg = false;
            PVPWaitAreaManager.Inst.AddResetComplete(restoreMap);
            if (info.Retcode == 0 && !string.IsNullOrEmpty(info.GameInfo) && info.Command ==  BattleCommand.BATTLESTATE)
            {
                PVPSyncData data = JsonConvert.DeserializeObject<PVPSyncData>(info.GameInfo);
                GlobalFieldController.PVPRound = data.round;
                OnGameState?.Invoke(PVPGameConnectEnum.ReConnect,data);
            }
            else
            {
                Debug.LogError("PVP Reconnect Fail ErrorMsg:" + info.Rmsg);
            }
        });
    }

    public void OnSelfDeath()
    {
        OnSelfDealth?.Invoke(null);
    }

    #region HealthPoint
    public void Init()
    {
        MessageHelper.AddListener(MessageName.PlayerCreate, PlayerOnCreate);
    }

    public override void Release()
    {
        base.Release();
        MessageHelper.RemoveListener(MessageName.PlayerCreate, PlayerOnCreate);
    }

    private void PlayerOnCreate()
    {
        if (Global.Room.RoomInfo.PlayerList == null)
        {
            return;
        }
        for (int idx = 0; idx < Global.Room.RoomInfo.PlayerList.Count; idx++)
        {
            var playerInfo = Global.Room.RoomInfo.PlayerList[idx];
            //有新玩家进房初始化状态
            if (PVPWaitAreaManager.Inst.PVPBehaviour == null)
            {
                PlayerManager.Inst.ShowPlayerState(playerInfo.Id);
            }
        }
    }

    public void UpdatePlayerHp(string id, float value)
    {
        if (!PlayerManager.Inst.allPlayerHp.ContainsKey(id))
        {
            PlayerManager.Inst.allPlayerHp.Add(id, value);
            return;
        }
        PlayerManager.Inst.allPlayerHp[id] = value;
    }

    public void UpdatePlayerHpShow(string id)
    {
        if (PlayerManager.Inst.allPlayerHp.ContainsKey(id))
        {
            UpdatePlayerHpShow(id, PlayerManager.Inst.allPlayerHp[id]);
        }
        else
        {
            PlayerManager.Inst.ShowPlayerState(id);
        }
    }

    public void UpdatePlayerHpShow(string id, float value)
    {
        if (SceneParser.Inst.GetHPSet() == 0)
        {
            return;
        }
        if (id == Player.Id)
        {
            var selfCon = PlayerBaseControl.Inst.GetComponentInChildren<CharBattleControl>(true);
            if (selfCon == null)
            {
                selfCon = PlayerBaseControl.Inst.gameObject.AddComponent<CharBattleControl>();
                selfCon.ShowState(id);
            }
            selfCon.UpdateHpValue(value,Player.Id);
            if(FPSPlayerHpPanel.Instance)
            {
                FPSPlayerHpPanel.Instance.SetBlood(value);
            }
        }
        else if (PlayerManager.Inst.otherPlayerDataDic.ContainsKey(id))
        {
            var otherCon = PlayerManager.Inst.otherPlayerDataDic[id].GetComponentInChildren<CharBattleControl>(true);
            if (otherCon == null)
            {
                otherCon = PlayerManager.Inst.otherPlayerDataDic[id].gameObject.AddComponent<CharBattleControl>();
                otherCon.ShowState(id);
            }
            otherCon.UpdateHpValue(value,id);
        }
    }

    public void AddPlayerHpShow(string id, float addValue)
    {
        if (SceneParser.Inst.GetHPSet() == 0)
        {
            return;
        }
        if (id == Player.Id)
        {
            var selfCon = PlayerBaseControl.Inst.GetComponentInChildren<CharBattleControl>(true);
            if (selfCon == null)
            {
                selfCon = PlayerBaseControl.Inst.gameObject.AddComponent<CharBattleControl>();
                selfCon.ShowState(id);
            }
            selfCon.AddHp(addValue,Player.Id);

        }
        else if (PlayerManager.Inst.otherPlayerDataDic.ContainsKey(id))
        {
            var otherCon = PlayerManager.Inst.otherPlayerDataDic[id].GetComponentInChildren<CharBattleControl>(true);
            if (otherCon == null)
            {
                otherCon = PlayerManager.Inst.otherPlayerDataDic[id].gameObject.AddComponent<CharBattleControl>();
                otherCon.ShowState(id);
            }
            otherCon.AddHp(addValue,id);
        }
    }

    public void OnGetItemsCallback(string dataJson)
    {
        if(SceneParser.Inst.GetHPSet() == 0)
            return;
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null)
        {
            if (!string.IsNullOrEmpty(dataJson))
            {
                GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
                if (getItemsRsp == null)
                {
                    LoggerUtils.Log("[PVPManager.OnGetItemsCallback] getItemsRsp is null");
                    return;
                }
                foreach (var data in getItemsRsp.playerBlood)
                {
                    UpdatePlayerHp(data.playerId, data.value);
                    UpdatePlayerHpShow(data.playerId, data.value);
                    if (GlobalFieldController.pvpData.winType == (int)PVPServerTaskType.Survival)
                    {
                        PlayerManager.Inst.UpdatePlayerDeathState(data.playerId,data.value == 0);
                    }
                }
            }
            UpdatePlayerHPStateByPVP();
            
            return;
        }
        //断线重连同步血量显示
        LoggerUtils.Log("===========PVPManager===>OnGetItems:" + dataJson);
        if (!string.IsNullOrEmpty(dataJson))
        {
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (getItemsRsp == null)
            {
                LoggerUtils.Log("[PVPManager.OnGetItemsCallback] getItemsRsp is null");
                return;
            }
            if (!PlayerBaseControl.Inst.GetComponentInChildren<BattleState>())
            {
                PlayerManager.Inst.StartShowPlayerState();
            }
            foreach (var data in getItemsRsp.playerBlood)
            {
                UpdatePlayerHp(data.playerId, data.value);
                UpdatePlayerHpShow(data.playerId, data.value);
                PlayerManager.Inst.UpdatePlayerDeathState(data.playerId,data.value == 0);
            }
        }
    }

    private void UpdatePlayerHPStateByPVP()
    {
        if (!PVPWaitAreaManager.Inst.IsPVPGameStart)
        {
            //断线重连后对战结束，离开对战状态
            PlayerManager.Inst.ExitShowPlayerState();
        }
        else
        {
            PlayerManager.Inst.UpdatePlayerHPVisibleByReconnect();
        }
    }

    #endregion
}

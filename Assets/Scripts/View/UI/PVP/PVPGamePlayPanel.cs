using System;
using System.Collections.Generic;
using BudEngine.NetEngine.src.Util;
using UnityEngine;

public class PVPGamePlayPanel<T> : BasePanel<T> where T:BasePanel<T>
{
    public PVPWaitPanel WaitState;
    public PVPReadyPanel ReadyState;
    public PVPStartPanel StartState;
    public PVPGameOverPanel GameOverState;
    protected BasePVPGamePanel CurState;
    private PVPGameState curPVPState = PVPGameState.Calibration;
    private int curRound = -1;// current round
    private Dictionary<PVPGameState, BasePVPGamePanel> AllPanels = new Dictionary<PVPGameState, BasePVPGamePanel>();
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        AllPanels.Add(PVPGameState.Ready,ReadyState);
        AllPanels.Add(PVPGameState.Wait,WaitState);
        AllPanels.Add(PVPGameState.Start,StartState);
        AllPanels.Add(PVPGameState.End,GameOverState);
    }
    
    
    
    protected void OnChangeModeByNet(PVPSyncData data,PVPGameConnectEnum connect)
    {
        var state = (PVPGameState)data.gameStatus;
        if(state == PVPGameState.Calibration)
            return;

        if (CurState !=  AllPanels[state])
        {
            OnChangeMode(state,connect);
        }
        curRound = data.round;
        curPVPState = state;
    }
    
    /// <summary>
    /// 断线重连或者切后台
    /// </summary>
    /// <param name="data"></param>
    /// <param name="connect"></param>
    protected void OnChangeModeByReconnect(PVPSyncData data,PVPGameConnectEnum connect)
    {
        var state = (PVPGameState)data.gameStatus;
        if(state == PVPGameState.Calibration)
            return;
        if (curRound == data.round && ((curPVPState == PVPGameState.Wait && state == PVPGameState.Ready) || curPVPState == state))
        {
            PVPWaitAreaManager.Inst.OnMapResetComplete();
        }
        else
        {
            OnResetMapByRound(state);
            OnChangeMode(state, connect);
        }
        curRound = data.round;
        curPVPState = state;
    }

    
    protected void OnChangeMode(PVPGameState state,PVPGameConnectEnum connect = PVPGameConnectEnum.Normal)
    {
        if(state == PVPGameState.Calibration)
            return;
        if (CurState != null)
        {
            CurState.Leave();
        }
        CurState = AllPanels[state];
        CurState.Enter(connect);
    }
    
    public void EnterGameByMode(GameMode gameMode, int durTime)
    {
        if (gameMode == GameMode.Play)
        {
            EnterGameByPlay(durTime);
        }
        else
        {
            EnterGameByGuest(durTime);
        }

    }

    protected virtual void EnterGameByPlay(int durTime)
    {
    }
    
    protected virtual void EnterGameByGuest(int durTime)
    {
    }

    protected virtual void OnGameState(PVPGameConnectEnum connect,PVPSyncData data)
    {
        if (connect == PVPGameConnectEnum.Normal)
        {
            OnChangeModeByNet(data, connect);
        }
        else
        {
            OnChangeModeByReconnect(data, connect);
        }

        switch ((PVPGameState)data.gameStatus)
        {
            case PVPGameState.Wait:
                OnGameWait(data);
                break;
            case PVPGameState.Start:
                OnGameStart(data);
                break;
            case PVPGameState.Ready:
                OnGameReady(data);
                break;
            case PVPGameState.Calibration:
                OnGameCalibration(data);
                break;
            case PVPGameState.End:
                OnGameEnd(data);
               break;
        }
    }

    protected virtual void OnGameWait(PVPSyncData data)
    {
        PVPWaitAreaManager.Inst.IsPVPGameStart = false;
    }
  
    protected virtual void OnGameStart(PVPSyncData data)
    {
        StartState.StartCountDown(data.afterStarted);
    }
    protected virtual void OnGameCalibration(PVPSyncData data)
    {
        StartState.OnResetTime(data.afterStarted);
    }
    
    protected virtual void OnGameEnd(PVPSyncData data)
    {
        PlayerManager.Inst.ExitShowPlayerState();
    }

    protected virtual void OnGameReady(PVPSyncData data)
    {
        PlayerManager.Inst.ExitShowPlayerState();
    }


    protected void OnResetMap()
    {
        PVPWaitAreaManager.Inst.OnReset();
        PlayerManager.Inst.ReturnPVPWaitArea();
        SceneSystem.Inst.StopSystem();
        SceneSystem.Inst.StartSystem();
    }
    /// <summary>
    /// 新的对局需要重置地图状态
    /// </summary>
    /// <param name="connect"></param>
    /// <param name="???"></param>
    public void OnResetMapByRound(PVPGameState gameState)
    {
        PVPWaitAreaManager.Inst.OnReset();
        SceneSystem.Inst.StopSystem();
        SceneSystem.Inst.StartSystem();
        PlayerBaseControl.Inst.SetEndFlyPlayerPos();
        PlayerManager.Inst.ReturnInitPos(gameState);
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        CurState.Leave();
        CurState = null;
        curRound = -1;
        SceneSystem.Inst.StopSystem();
    }
}

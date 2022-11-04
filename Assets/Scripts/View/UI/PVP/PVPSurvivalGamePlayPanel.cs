using System;
using UnityEngine;

public class PVPSurvivalGamePlayPanel : PVPGamePlayPanel<PVPSurvivalGamePlayPanel>
{
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        PVPManager.Inst.OnGameState = OnGameState;
        PVPManager.Inst.OnSelfDealth = OnSelfDealth;
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        PVPManager.Inst.OnGameState = null;
        PVPManager.Inst.OnSelfDealth = null;
    }

    #region Survival On playMode

    protected override void EnterGameByPlay(int durTime)
    {
        base.EnterGameByPlay(durTime);
        if (ReadyState.ReadyEndAction == null)
        {
            ReadyState.ReadyEndAction = () =>
            {
                OnChangeMode(PVPGameState.Start);
                StartState.StartCountDown();
            };
            StartState.StartEndAction = OnSurvivalGameOver;
            GameOverState.GameOverAction = OnSurvivalGameEnd;
        }
        StartState.durationTime = durTime;
        OnChangeMode(PVPGameState.Ready);
    }

    private void OnSurvivalGameOver()
    {
        GameOverState.SetWinner(PVPGameOverPanel.GameOverStateEnum.Win);
        OnChangeMode(PVPGameState.End);
    }

    public void SetWinner(PVPGameOverPanel.GameOverStateEnum  state)
    {
        OnChangeMode(PVPGameState.End);
        GameOverState.SetWinner(state);
    }

    private void OnSurvivalGameEnd()
    {
        CurState.Leave();
        PlayModePanel.Instance.OnEditClick();
    }

    #endregion

    #region Survival On Guest

    protected override void EnterGameByGuest(int durTime)
    {
        base.EnterGameByGuest(durTime);
        if (GameOverState.GameOverAction == null)
        {
            GameOverState.GameOverAction =  OnPVPGameOverByGuest;
        }
        StartState.durationTime = durTime;
        OnChangeMode(PVPGameState.Wait);
    }

    private void OnPVPGameOverByGuest()
    {
        PVPWaitAreaManager.Inst.IsPVPGameStart = false;
        BlackPanel.Show();
        BlackPanel.Instance.PlayTransitionAnimAct(OnPVPEndToWait);
        PVPWaitAreaManager.Inst.SetMeshAndBoxVisible(false, true);
    }
    
    private void OnPVPEndToWait()
    {
        OnChangeMode(PVPGameState.Wait);
        OnResetMap();
        PlayerManager.Inst.ReturnPVPWaitArea();
        PlayerManager.Inst.ClearPlayersDeathState();
    }

    protected override void OnGameEnd(PVPSyncData data)
    {
        base.OnGameEnd(data);
        if (string.IsNullOrEmpty(data.winner))
        {
            Debug.LogError("PVP Survival Data Server Error");
            if (PVPTeamManager.Inst.IsTeamMode())
            {
                GameOverState.SetWinnerPanel(PVPGameOverPanel.GameOverStateEnum.TimeUp);
                Invoke("OnPVPGameOverByGuest", 1.7f);
            }
            else
            {
                OnPVPGameOverByGuest();
            }
            return;
        }
        var selfUid = GameManager.Inst.ugcUserInfo.uid;
        var players = data.winner.Split(',');
        int index = Array.FindIndex(players, x => x == selfUid);
        if (index >= 0)
        {
            GameOverState.SetWinner(PVPGameOverPanel.GameOverStateEnum.Win);
        }
        else
        {
            if (PVPTeamManager.Inst.IsTeamMode())
            {
                GameOverState.SetWinnerPanel(PVPGameOverPanel.GameOverStateEnum.Loss);
                Invoke("OnPVPGameOverByGuest", 1.7f);
            }
            else
            {
                OnPVPGameOverByGuest();
            }
        }
        PlayerManager.Inst.ClearPlayersDeathState();
    }

    protected override void OnGameWait(PVPSyncData data)
    {
        base.OnGameWait(data);
        PVPWaitAreaManager.Inst.IsSelfDeath = false;
    }

    protected override void OnGameReady(PVPSyncData data)
    {
        base.OnGameReady(data);
        PVPWaitAreaManager.Inst.IsSelfDeath = false;
        PlayerManager.Inst.ClearPlayersDeathState();
    }

    protected override void OnGameStart(PVPSyncData data)
    {
        base.OnGameStart(data);
        PVPWaitAreaManager.Inst.IsSelfDeath = false;
        PlayerManager.Inst.ClearPlayersDeathState();
    }
    
    public void OnSelfDealth(SurvivalSyncData data)
    {
        //分队模式下，若玩家不是队伍中最后一个死亡的，则不弹出Lose界面
        //非分队模式下，玩家死亡后直接弹出Lose界面
        PVPWaitAreaManager.Inst.IsSelfDeath = true;
        if (!PVPTeamManager.Inst.IsTeamMode()) {
            GameOverState.SetWinnerPanel(PVPGameOverPanel.GameOverStateEnum.Loss);
        }
    }

    #endregion

}
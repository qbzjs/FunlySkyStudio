public class PVPWinConditionGamePlayPanel:PVPGamePlayPanel<PVPWinConditionGamePlayPanel>
{
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        PVPManager.Inst.OnGameState = OnGameState;
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        PVPManager.Inst.OnGameState = null;
    }
    #region  WinCondition On playMode 

    protected override void EnterGameByPlay(int durTime)
    {
        base.EnterGameByPlay(durTime);
        if (ReadyState.ReadyEndAction == null)
        {
            ReadyState.ReadyEndAction = ()=>
            {
                OnChangeMode(PVPGameState.Start);
                StartState.StartCountDown();
            };
            StartState.StartEndAction = OnWinConditionGameOver;
            GameOverState.GameOverAction =  OnWinConditionGameEnd;
        }
        StartState.durationTime = durTime;
        OnChangeMode(PVPGameState.Ready);
    }

    public void SetWinner(PVPGameOverPanel.GameOverStateEnum  state)
    {
        OnChangeMode(PVPGameState.End);
        GameOverState.SetWinner(state);
    }

    private void OnWinConditionGameOver()
    {
        GameOverState.SetWinner(PVPGameOverPanel.GameOverStateEnum.TimeUp);
        OnChangeMode(PVPGameState.End);
    }


    private void OnWinConditionGameEnd()
    {
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        CurState.Leave();
        PlayModePanel.Instance.OnEditClick(); 
    }
    #endregion
    
    #region WinCondition OnGuest
    
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
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
    }
    protected override void OnGameState(PVPGameConnectEnum connect,PVPSyncData data)
    {
        base.OnGameState(connect,data);
        if ((PVPGameState)data.gameStatus != PVPGameState.Calibration)
        {
            MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        }
    }

    protected override void OnGameEnd(PVPSyncData data)
    {
        base.OnGameEnd(data);
        if (string.IsNullOrEmpty(data.winner))
        {
            GameOverState.SetWinner(PVPGameOverPanel.GameOverStateEnum.TimeUp);
        }
        else
        {
            var selfUid = GameManager.Inst.ugcUserInfo.uid;
            GameOverState.SetWinner(data.winner.Contains(selfUid)
                ? PVPGameOverPanel.GameOverStateEnum.Win
                : PVPGameOverPanel.GameOverStateEnum.Loss);
        }
    }
    #endregion
}
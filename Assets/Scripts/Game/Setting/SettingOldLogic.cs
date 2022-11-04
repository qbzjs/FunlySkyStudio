public class SettingOldLogic
{
    public void OldLogic()
    {
        GlobalSettingManager.Inst.OnFootStepChange += OldFootStepChange;
        GlobalSettingManager.Inst.OnAutomaticRunChange += OldAutomaticRunChange;
        GlobalSettingManager.Inst.OnFlyingModeChange += OldFlyingModeChange;
        GlobalSettingManager.Inst.OnGameViewChange += OldGameViewChange;
        GlobalSettingManager.Inst.OnMicrophoneChange += OldMicrophoneChange;
        GlobalSettingManager.Inst.OnSpeakerChange += OldSpeakerChange;
        GlobalSettingManager.Inst.OnVoiceChangerChange += OldVoiceChangerChange;
        GlobalSettingManager.Inst.OnFriendRequestChange += OldFriendRequestChange;
    }

    private void OldFriendRequestChange(bool open)
    {
        SocialNotificationManager.Inst.openNotification = open;
    }

    private void OldFootStepChange(bool open)
    {
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.OnFootToggleClick(open);
        }
    }

    private void OldAutomaticRunChange(bool open)
    {
        if (PlayerBaseControl.Inst)
        {
            PlayerBaseControl.Inst.isOriginalWalkMode = !open;
        }
    }

    private void OldFlyingModeChange(FlyingMode flyingMode)
    {
        if (flyingMode == FlyingMode.Free
            && PlayerBaseControl.Inst
            && PlayerBaseControl.Inst.isOriginalFlyMode)
        {
            if (PlayModePanel.Instance)
            {
                PlayerBaseControl.Inst.isOriginalFlyMode = false;
                PlayerBaseControl.Inst.SetFreeFlyPlayerPos();
                PlayModePanel.Instance.SetFlyModeBtn(false);
            }
        }else if (flyingMode == FlyingMode.Original
                  && PlayerBaseControl.Inst
                  && !PlayerBaseControl.Inst.isOriginalFlyMode)
        {
            if (PlayModePanel.Instance)
            {
                PlayerBaseControl.Inst.isOriginalFlyMode = true;
                PlayerBaseControl.Inst.SetEndFlyPlayerPos();
                PlayModePanel.Instance.SetFlyModeBtn(true);
            }
        }
    }

    private void OldGameViewChange(GameView gameView)
    {
        if (gameView == GameView.FirstPerson
            && PlayModePanel.Instance 
            && PlayModePanel.Instance.isTps)
        {
            PlayModePanel.Instance.OnChangeViewBtnClick();
        }
        if (gameView == GameView.ThirdPerson
            && PlayModePanel.Instance 
            && !PlayModePanel.Instance.isTps)
        {
            PlayModePanel.Instance.OnChangeViewBtnClick();
        }
    }

    private void OldMicrophoneChange(float value)
    {
        if (RealTimeTalkManager.Inst != null)
        {
            RealTimeTalkManager.Inst.ChangeMicLevel((int)value);
        }
    }
    
    private void OldSpeakerChange(float value)
    {
        if (RealTimeTalkManager.Inst != null)
        {
            RealTimeTalkManager.Inst.ChangeVoiceLevel((int)value);
        }
    }
    
    private void OldVoiceChangerChange(VoiceEffect voiceEffect)
    {
        AKSoundManager.Inst.StopVoiceDemoSound(GlobalSettingPanel.Instance.gameObject);
        if (voiceEffect!=VoiceEffect.Original)
        {
            AKSoundManager.Inst.PlayVoiceDemoSound((voiceEffect).ToString(), GlobalSettingPanel.Instance.gameObject);
        }
        if (RealTimeTalkManager.Inst != null)
        {
            RealTimeTalkManager.Inst.SetVoiceChange(voiceEffect);
        }
    }
}
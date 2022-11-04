using System;
using UnityEngine;

public class GlobalSettingManager : CInstance<GlobalSettingManager>
{
    private GlobalSettingData data;
    private BGMusicBehaviour bgBehv;

    public void ReadSaveParams()
    {
        data = DataUtils.GetGlobalSetting(GameInfo.Inst.myUid ?? string.Empty);
    }

    public void Init()
    {
        ReadSaveParams();
        SettingOldLogic oldLogic = new SettingOldLogic();
        oldLogic.OldLogic();
        InitSoundVolume();
    }

    public void Save()
    {
        if (data != null)
        {
            DataUtils.SetGlobalSetting(GameInfo.Inst.myUid, data);
        }
    }

    public void SyncGameView(GameView gameView)
    {
        data.gameView = gameView;
    }
    
    #region 单一获取属性方法

    private void EnsureData()
    {
        if (data == null)
        {
            ReadSaveParams();
        }

        //以防万一
        if (data == null)
        {
            data = new GlobalSettingData();
        }
    }
    
    public GameView GetGameView()
    {
        EnsureData();
        return data.gameView;
    }

    public bool IsAutoRunningOpen()
    {
        EnsureData();
        return data.automaticRunning == 0;
    }

    public FlyingMode GetFlyingMode()
    {
        EnsureData();
        return data.flyingMode;
    }

    public bool IsLockMoveStick()
    {
        EnsureData();
        return data.lockMoveStick == 0;
    }

    public float GetCameraSensitive()
    {
        EnsureData();
        return data.cameraPanSensitivity;
    }
    
    public bool IsShowUserName()
    {
        EnsureData();
        return data.showUserName == 0;
    }

    public bool IsFriendRequestOpen()
    {
        EnsureData();
        return data.friendRequest == 0;
    }

    public int GetFps()
    {
        EnsureData();
        return data.FPS == 0 ? 60 : 30;
    }

    public bool IsBloomOpen()
    {
        EnsureData();
        return data.bloom == 0;
    }

    public bool IsShadowOpen()
    {
        EnsureData();
        return data.shadow == 0;
    }

    public bool IsFootstepOpen()
    {
        EnsureData();
        return data.footStep == 0;
    }

    public float GetBgmVolume()
    {
        EnsureData();
        return data.bgm;
    }

    public float GetSoundEffectVolume()
    {
        EnsureData();
        return data.soundEffect;
    }

    public float GetMicrophoneVolume()
    {
        EnsureData();
        return data.microPhone;
    }

    public float GetSpeakerVolume()
    {
        EnsureData();
        return data.speaker;
    }

    public VoiceEffect GetVoiceEffect()
    {
        EnsureData();
        return data.voiceEffect;
    }
    #endregion

    #region 监听事件

    public Action<GameView> OnGameViewChange;
    public Action<bool> OnAutomaticRunChange;
    public Action<FlyingMode> OnFlyingModeChange;
    public Action<bool> OnLockMoveStickChange;
    public Action<float> OnCameraPanSensitivityChange;
    public Action<bool> OnShowUserNameChange;
    public Action<bool> OnFriendRequestChange;
    public Action<int> OnFPSChange;
    public Action<bool> OnBloomChange;
    public Action<bool> OnShadowChange;
    public Action<bool> OnFootStepChange;
    public Action<float> OnBgmChange;
    public Action<float> OnSoundEffectChange;
    public Action<float> OnMicrophoneChange;
    public Action<float> OnSpeakerChange;
    public Action<VoiceEffect> OnVoiceChangerChange;

    #endregion

    #region 给Panel调用的方法

    public void GameViewChange(GameView gameView)
    {
        data.gameView = gameView;
        OnGameViewChange?.Invoke(gameView);
        //不保存这个
        //Save();
    }

    public void AutomaticRunChange(int index)
    {
        data.automaticRunning = index;
        OnAutomaticRunChange?.Invoke(index == 0);
        Save();
    }

    public void FlyingModeChange(int index)
    {
        data.flyingMode = (FlyingMode) index;
        OnFlyingModeChange?.Invoke(data.flyingMode);
        Save();
    }

    public void LockMoveStickChange(int index)
    {
        data.lockMoveStick = index;
        OnLockMoveStickChange?.Invoke(index == 0);
        Save();
    }

    public void CameraPanSensitivityChange(float value)
    {
        data.cameraPanSensitivity = value;
        OnCameraPanSensitivityChange?.Invoke(value);
    }

    public void ShowUserNameChange(int index)
    {
        data.showUserName = index;
        OnShowUserNameChange?.Invoke(index == 0);
        Save();
    }

    public void FriendRequestChange(int index)
    {
        data.friendRequest = index;
        OnFriendRequestChange?.Invoke(index == 0);
        Save();
    }

    public void FPSChange(int index)
    {
        data.FPS = index;
        OnFPSChange?.Invoke(index == 0 ? 60 : 30);
        //TODO test
        QualityManager.Inst.SetFps(index == 0 ? 60 : 30);
        Save(); 
    }

    public void BloomChange(int index)
    {
        data.bloom = index;
        OnBloomChange?.Invoke(index == 0);
        SceneBuilder.Inst.PostProcessBehaviour.SetPostProcessActive(index == 0);
        Save();
    }

    public void ShadowChange(int index)
    {
        data.shadow = index;
        OnShadowChange?.Invoke(index == 0);
        QualityManager.Inst.SetTargetQualityShadow(index == 0);
        Save();
    }

    public void FootStepChange(int index)
    {
        data.footStep = index;
        OnFootStepChange?.Invoke(index == 0);
        Save();
    }

    public void BgmChange(float value)
    {
        data.bgm = value;
        OnBgmChange?.Invoke(value);

        //设置 WWise BGM 音量
        AudioController.Inst.SetBGMAudioVolume(value);
        // 设置自定义 BGM 音量
        SetBGMBehaviorVolume(value / 100.0f);
    }

    public void SoundEffectChange(float value)
    {
        data.soundEffect = value;
        OnSoundEffectChange?.Invoke(value);
        float volume = value / 100;
        // 设置 H5 视频声音
        VideoNodeManager.Inst.SetVideoVolume(volume);
        // 设置声音按钮声音
        SoundManager.Inst.SetSoundVolume(volume);
        //设置 WWise 音效音量
        AudioController.Inst.SetSFXAudioVolume(value);
    }

    public void MicroPhoneChange(float value)
    {
        data.microPhone = value;
        OnMicrophoneChange?.Invoke(value);
    }

    public void SpeakerChange(float value)
    {
        data.speaker = value;
        OnSpeakerChange?.Invoke(value);
    }

    public void VoiceChangerChange(int index)
    {
        data.voiceEffect = (VoiceEffect)index;
        OnVoiceChangerChange?.Invoke(data.voiceEffect);
        Save();
    }

    #endregion
    
    private BGMusicBehaviour GetBgBav()
    {
        if (bgBehv == null)
        {
            if (SceneBuilder.Inst != null && SceneBuilder.Inst.BGMusicEntity != null)
            {
                var sceneEntity = SceneBuilder.Inst.BGMusicEntity;
                var bindGo = sceneEntity.Get<GameObjectComponent>().bindGo;
                if (bindGo != null)
                {
                    bgBehv = bindGo.GetComponent<BGMusicBehaviour>();
                }
            }
        }
        return bgBehv;
    }

    public void SetBGMBehaviorVolume(float volume)
    {
        var bgmBehv = GetBgBav();
        if (bgmBehv != null)
        {
            bgmBehv.SetAudioVolume(volume);
        }
    }

    /// <summary>
    /// 根据设置数据初始化音量
    /// </summary>
    public void InitSoundVolume()
    {
        AudioController.Inst.SetBGMAudioVolume(data.bgm);
        SetBGMBehaviorVolume(data.bgm / 100.0f);
        AudioController.Inst.SetSFXAudioVolume(data.soundEffect);
        float volume = data.soundEffect / 100.0f;
        SoundManager.Inst.SetSoundVolume(volume);
        VideoNodeManager.Inst.SetVideoVolume(volume);
    }

}
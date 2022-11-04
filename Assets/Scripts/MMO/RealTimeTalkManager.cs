/// <summary>
/// Author:Zhouzihan
/// Description:实时语音管理类
/// Date: 2022/4/24 15:42:29
/// </summary>
using System.Collections;
using System.Collections.Generic;
using agora_gaming_rtc;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Android;
public class RealTimeTalkManager : CInstance<RealTimeTalkManager>
{
    private static IRtcEngine mRtcEngine;
    private GameMediaEngine mGameMediaEngine;
   
    private const string APP_ID = "22121671373545f683ec2665d6bc8c13";//BUD_appid
    
    private string channelName = "";
    private string token = "";
    private bool isInRoom;
    //超过该音量视为正在讲话播放动效
    private const int TALKING_MIC_LEVEL = 60;
    private bool isTalking;
    private bool isFirstRequstPermission = true;
    private bool isPermissionAllow;
    private Dictionary<string, OtherPlayerInfo> otherPlayer = new Dictionary<string, OtherPlayerInfo>();
    private BGMusicBehaviour bgBehv;
    //语音开关
    public bool closeVoice = true;
    //空间语音开关
    public bool closeSpatialSound = true;
    //麦克风开关状态
    public bool isMicOn; 
    //扩音器开关状态
    public bool isAudioOn = true;
    //判断是否进房
    private bool isJoin = false;
    //扩音器和麦克风使用时间计算
    public double micTime, audioTime;
    public void Init()
    {
        GetSwitch();
        if (closeVoice)
        {
            return;
        }
      
        if (PlayModePanel.Instance != null)
        {
            Log("PlayModePanel.Instance");
            PlayModePanel.Instance.ShowMicAudioUI();
        }
        Log("RealTimeTalkManager  Init");
        // 填入 App ID 并初始化 IRtcEngine。
        
        mRtcEngine = IRtcEngine.GetEngine(APP_ID);
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY,AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
        mRtcEngine.EnableAudioVolumeIndication(500,3,true);
        mRtcEngine.SetParameters("{\"che.audio.keep.audiosession\":true}");
        //mRtcEngine.SetParameters("{\"che.audio.start_debug_recording\":\"all\"}");
        mRtcEngine.EnableDeepLearningDenoise(true);
        //修改音频输出方式为扬声器
        mRtcEngine.SetDefaultAudioRouteToSpeakerphone(true);
      
        //本地音频输出转换回调
        mRtcEngine.OnAudioRouteChanged = OnAudioRouteChanged;
        // 注册 OnJoinChannelSuccessHandler 回调。 
        // 本地用户成功加入频道时，会触发该回调。
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccessHandler;
        // 注册 OnJoinChannelSuccessHandler 回调。 
        // 本地用户成功加入频道时，会触发该回调。
        mRtcEngine.OnLeaveChannel = OnLeaveChannel;
        // 注册 OnUserJoinedHandler 回调。
        // SDK 接收到第一帧远端音频并成功解码时，会触发该回调。
        mRtcEngine.OnUserJoined = OnUserJoinedHandler;
        // 注册 OnUserOfflineHandler 回调。 
        // 远端用户离开频道或掉线时，会触发该回调。
        mRtcEngine.OnUserOffline = OnUserOfflineHandler;
        mRtcEngine.OnVolumeIndication = OnVolumeIndication;
    
        mRtcEngine.OnLocalAudioStateChanged = OnLocalAudioStateChanged;
        mRtcEngine.OnUserMutedAudio = OnUserMutedAudio;
        if (!closeSpatialSound)
        {
            SetGameVoice();
        }
        if (mRtcEngine != null)
        {
            mRtcEngine.MuteLocalAudioStream(true);
            AudioSwitch(false);
        }
        if (PlayModePanel.Instance != null)
        {
            PlayModePanel.Instance.SetMicUI(false);
            PlayModePanel.Instance.SetAudioUI(false);
        }

    }
    public void FixedUpdate()
    {
        if (mRtcEngine!=null)
        {
            if (isMicOn)
            {
                micTime += Time.deltaTime;
            }
            if (isAudioOn)
            {
                audioTime += Time.deltaTime;
            }
        }
       

    }
    public void SetGameVoice()
    {
        //创建和初始化游戏语音
        mGameMediaEngine = GameMediaEngine.CreateAgoraGameMediaEngine();
        mGameMediaEngine.Initialize(APP_ID);
        //设置小队语音
        mGameMediaEngine.SetRangeAudioTeamID(0);
        mGameMediaEngine.SetRangeAudioMode(0);
        mGameMediaEngine.SetMaxHearAudioCount(GameConsts.MAX_PLAYER);
        mGameMediaEngine.SetAudioRecvRange(30);
        mGameMediaEngine.SetDistanceUnit(1);

        SetVoiceChange(GlobalSettingManager.Inst.GetVoiceEffect());
        ChangeMicLevel((int)GlobalSettingManager.Inst.GetMicrophoneVolume());
        ChangeVoiceLevel((int)GlobalSettingManager.Inst.GetSpeakerVolume());

        mGameMediaEngine.OnEnterRoomSuccess = OnEnterRoomSuccess;
        mGameMediaEngine.OnEnterRoomFail = OnEnterRoomFail;
        mGameMediaEngine.OnConnectionStateChange = OnConnectionStateChange;
        mGameMediaEngine.OnLostSync = OnLostSync;
        mGameMediaEngine.OnGetSync = OnGetSync;
        ////打开3d音效
        mGameMediaEngine.EnableSpatializer(true, true);
    }
    private void GetSwitch()
    {
        if (GameManager.Inst.unityConfigInfo == null|| GameManager.Inst.unityConfigInfo.featSwitch==null)
        {
            return;
        }
        if (Global.Room.RoomInfo.IsPrivate)
        {
            closeVoice = GameManager.Inst.unityConfigInfo.featSwitch.privateServer.closeVoice;
            closeSpatialSound = GameManager.Inst.unityConfigInfo.featSwitch.privateServer.closeSpatialSound;
        }
        else
        {
            closeVoice = GameManager.Inst.unityConfigInfo.featSwitch.publicServer.closeVoice;
            closeSpatialSound = GameManager.Inst.unityConfigInfo.featSwitch.publicServer.closeSpatialSound;
        }
        
    }
    private uint playerUid;
    public void JoinInRoom()
    {
        if (closeVoice||isJoin)
        {
            return;
        }
        isJoin = true;
        GetChannelName();
        playerUid = GetId(Player.Timestamp,Player.Id);
        Log("playerUid"+ playerUid);
        RCTTokenInfo info = new RCTTokenInfo();
        info.channelName = channelName;
        info.role = 1;
        info.uid = playerUid;
        HttpUtils.MakeHttpRequest("/other/rtcToken", (int)HTTP_METHOD.POST, JsonUtility.ToJson(info), GetTokenSuccess, GetTokenFail);
    }
    
    public void LeaveRoom()
    {
        if (mRtcEngine == null)
            return;
        DataLogUtils.LogAudioPlayDuration(audioTime,micTime);
        OnLeaveRoom();
    }
    public void  OnLeaveRoom() {
        // 离开频道。
        Log("calling leave");
        if (mRtcEngine == null)
            return;
        mRtcEngine.LeaveChannel();
        isInRoom = false;
        if (mGameMediaEngine == null)
            return;
        mGameMediaEngine.ExitRoom();

    }

    public void OnMicSwitch(bool isOn)
    {
        MicSwitch(isOn);
        if (!isPermissionAllow)
        {
            CheckPermission(true);
        }

    }
    public void MicSwitch(bool isOn)
    {
        Log("MicSwitch  " + isOn);
        if (mRtcEngine == null)
            return;
        mRtcEngine.MuteLocalAudioStream(!isOn);
        isMicOn = isOn;
        if (!isOn)
        {
            SetTalkingAnim(isOn);
        }
        float volume = isOn ? 50 : 100;
        AudioController.Inst.SetBGAudioVolume(volume);
        volume = isOn ? 0.1f : 0.5f;
        GetBgBav()?.SetAudioVolume(volume);
    }
    private BGMusicBehaviour GetBgBav()
    {
        if (bgBehv==null)
        {
            if (SceneBuilder.Inst != null && SceneBuilder.Inst.BGMusicEntity != null)
            {
                var sceneEntity = SceneBuilder.Inst.BGMusicEntity;
                var bindGo = sceneEntity.Get<GameObjectComponent>().bindGo;
                bgBehv = bindGo.GetComponent<BGMusicBehaviour>();
            }
        }
        return bgBehv;
    }
    public void AudioSwitch(bool isOn)
    {
        Log("AudioSwitch  " + isOn);
        //点击扬声器按钮就会进入房间，如果已经进入则无效
        if (isOn)
        {
            JoinInRoom();
        }
        if (mRtcEngine == null)
            return;
        SetAudio(isOn);
        isAudioOn = isOn;
    }
    public void OnApplicationQuit()
    {
        if (mGameMediaEngine != null)
        {
            mGameMediaEngine.Release();
            mGameMediaEngine = null;
        }
        if (mRtcEngine != null)
        {
            // 销毁 IRtcEngine。
            IRtcEngine.Destroy();
            mRtcEngine = null;
        }
    }
    public void SetAudio(bool isOn)
    {
        if (mGameMediaEngine != null)
        {
            mGameMediaEngine.EnableSpeaker(isOn);
        }
        else
        {
            mRtcEngine.MuteAllRemoteAudioStreams(!isOn);
        }

    }

    public void GetTokenSuccess(string msg) {

        HttpResponDataStruct tokeninfo = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        GetToken info = JsonConvert.DeserializeObject<GetToken>(tokeninfo.data);
        token = info.token;
        Log("GetTokenSuccess:   " + token+"   "+ channelName+"   " + playerUid);
        if (mRtcEngine == null)
            return;
        isInRoom = mRtcEngine.JoinChannelByKey(token, channelName, null, playerUid) == 0;
       

    }
    public void OnLostSync(long lostSyncTimeMs)
    {
        Log("OnLostSync:  " + lostSyncTimeMs);
    }
    public void OnGetSync (long lostSyncTimeMs)
    {
        Log("OnGetSync:  " + lostSyncTimeMs);
    }
    public void OnConnectionStateChange(GME_CONNECTION_STATE_TYPE state, GME_CONNECTION_CHANGED_REASON_TYPE reason)
    {
        Log("OnConnectionStateChange" + state);
        Log("OnConnectionStateChange" + reason);
    }
    public void GetTokenFail(string msg)
    {
        Log("GetTokenFail" + msg);
    }
    public void OnLocalAudioStateChanged(LOCAL_AUDIO_STREAM_STATE state, LOCAL_AUDIO_STREAM_ERROR error)
    {
        Log("OnLocalAudioStateChanged" + state+ "  "+ error);
    }
    public void OnAudioRouteChanged(AUDIO_ROUTE route)
    {
        Log("OnAudioRouteChanged" + route);
    }
    public void OnLeaveChannel(RtcStats stats)
    {
        Log("OnLeaveChannel" + stats);
    }
    public void OnEnterRoomSuccess()
    {
        Log("OnEnterRoomSuccess");
    }
    public void OnEnterRoomFail()
    {
        Log("OnEnterRoomFail");
    }
    public void OnUserMutedAudio(uint uid, bool muted)
    {
        Log("OnUserMutedAudio"+ muted);

    }
    public void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        Log("OnJoinChannelSuccessHandler:   " + token+"   "+ channelName+"   "+ playerUid);
        if (closeSpatialSound||mGameMediaEngine == null)
            return;
        mGameMediaEngine.EnterRoom(token, channelName, playerUid, SPACIAL_AUDIO_SYNC_MODE.SPACIAL_AUDIO_SYNC_MODE_LOCAL);
    }
    public void OnUserJoinedHandler(uint uid, int elapsed)
    {
        Log("OnUserJoinedHandler");
    }
    public void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        Log("OnUserOfflineHandler");
    }
    //时隔0.5秒检测一次麦克风，如果状态变化则广播（为了不同异常情况矫正，每4次检测发送一次麦克风状态）
    private int micCheckTimes = 0;

    public void OnVolumeIndication(AudioVolumeInfo[] speakers, int speakerNumber, int totalVolume)
    {

        if (speakerNumber == 1 && speakers[0].uid == 0)
        {
            if (micCheckTimes<4)
            {
                micCheckTimes++;
            }
            else
            {
                micCheckTimes = 0;
            }
            bool curTalking = totalVolume > TALKING_MIC_LEVEL;
            if (curTalking!=isTalking|| micCheckTimes == 0)
            {
                isTalking = curTalking;
                SetTalkingAnim(isTalking);
            }
        }
    }
    private void SetTalkingAnim(bool isAct) {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            CustomData data = new CustomData();
            data.type = (int)ChatCustomType.Talk;
            data.data = (isAct ? 0 : 1).ToString();
            RoomChatData roomChatData = new RoomChatData()
            {
                msgType = (int)RecChatType.Custom,
                data = JsonConvert.SerializeObject(data),
            };
            ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
        }

    }


    private void CheckPermission(bool isChack)
    {
#if !UNITY_EDITOR
        MobileInterface.Instance.AddClientRespose(MobileInterface.requestNativePermission, OnRequestNativePermissionSuccess);
        MobileInterface.Instance.AddClientFail(MobileInterface.requestNativePermission, OnRequestNativePermissionFail);
        CheckPermission permission = new CheckPermission();
        permission.isCheck = isChack? 0:1;
        permission.permissionType = 0;
        MobileInterface.Instance.RequestNativePermission(JsonConvert.SerializeObject(permission));
       
#endif
        Log("CheckPermission: " + isChack);
    }
    public void OnRequestNativePermissionSuccess(string content)
    {
        if (mRtcEngine==null||closeVoice)
        {
            return;
        }
        MobileInterface.Instance.DelClientResponse(MobileInterface.requestNativePermission);

        Log("OnRequestNativePermissionSuccess: " + content);
        RecievePermission permission = JsonConvert.DeserializeObject<RecievePermission>(content);
        if (permission.permissionType == 0)
        {
            switch (permission.grantType)
            {

                case (int)PermissionType.DIDNT_SET:
                    CheckPermission(false);
                    break;
                case (int)PermissionType.DONT_ALLOW:
                case (int)PermissionType.DONT_ALLOW_AND_DONT_ASK:
                    MicSwitch(false);
                    if (PlayModePanel.Instance != null)
                    {
                        PlayModePanel.Instance.SetMicUI(false);
                    }
                    Log("OnRequestNativePermissionSuccess: " + isFirstRequstPermission);
                    if (!isFirstRequstPermission)
                    {
                        PermissionPanel.Show();
                    }
                    isFirstRequstPermission = false;
                    break;
                case (int)PermissionType.ALLOW:
                    isPermissionAllow = true;
                    if (isMicOn)
                    {
                        mRtcEngine.DisableAudio();
                        mRtcEngine.EnableAudio();
                        mRtcEngine.EnableLocalAudio(true);

                        Log("EnableAudio");
                    }
                    if (isAudioOn)
                    {
                        SetAudio(true);
                    }
                    break;
            }

        }


    }
    public void OnRequestNativePermissionFail(string content)
    {
        Log("OnRequestNativePermissionFail: " + content);
        MobileInterface.Instance.DelClientResponse(MobileInterface.requestNativePermission);
    }
    
    private Dictionary<string, OtherPlayerCtr> otherPlayerDic;
    public Dictionary<string, OtherPlayerCtr> GetOtherPlayerDataDic
    {
        get
        {
            if (otherPlayerDic == null && ClientManager.Inst != null)
            {
                otherPlayerDic = ClientManager.Inst.otherPlayerDataDic;
            }
            return otherPlayerDic;
        }
       
    }
    //立体声玩家位置
    public void SetRemoteVoicePosition()
    {
        
        if (closeSpatialSound)
        {
            return;
        }
        if (isInRoom && PlayerBaseControl.Inst != null && mGameMediaEngine != null)
        {
            Transform selftrans = PlayerBaseControl.Inst.transform;
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, selftrans.rotation, Vector3.one);
            int[] position = new int[3] { (int)selftrans.position.z, (int)selftrans.position.x, (int)selftrans.position.y };
            float[] axisForward = new float[3] { matrix.m22, matrix.m02, matrix.m12 };
            float[] axisRight = new float[3] { matrix.m20, matrix.m00, matrix.m10 };
            float[] axisUp = new float[3] { matrix.m21, matrix.m01, matrix.m11 };
            // Log("mian"+JsonConvert.SerializeObject(position));
            int q = mGameMediaEngine.UpdateSelfPosition(position, axisForward, axisRight, axisUp);
            if (GetOtherPlayerDataDic != null && GetOtherPlayerDataDic.Count > 0 && otherPlayer.Count > 0)
            {
                foreach (var item in otherPlayer)
                {

                    if (GetOtherPlayerDataDic.ContainsKey(item.Key) && GetOtherPlayerDataDic[item.Key] != null)
                    {
                        Transform trans = GetOtherPlayerDataDic[item.Key].transform;
                        int[] pos = new int[3] { (int)trans.position.z, (int)trans.position.x, (int)trans.position.y + SetCurHigh(item.Value.isCurMap) };
                        mGameMediaEngine.UpdateRemotePosition_gameEngine(item.Value.uid, pos);
                        //  Log("other" + JsonConvert.SerializeObject(pos));

                    }
                }
            }
        }


    }
    private int SetCurHigh(bool isCurMap)
    {
        return isCurMap ? 0 : 500;
    }
    private void  GetChannelName()
    {
        channelName = Global.Room.RoomInfo.Id;
    }

    public void ChangeMicLevel(int level)
    {
        
        if (mRtcEngine!=null)
        {
            mRtcEngine.AdjustRecordingSignalVolume(level);
        }
    }
    public void ChangeVoiceLevel(int level)
    {
       
        if (mRtcEngine != null)
        {
            mRtcEngine.AdjustPlaybackSignalVolume(level);
        }
    }


    public void OnPause(bool isPause)
    {
        if (mRtcEngine!=null)
        {
            if (isMicOn || isPause)
            {
                mRtcEngine.MuteLocalAudioStream(isPause);
            }

            if (isAudioOn || isPause)
            {
                SetAudio(!isPause);
            }
            
        }
    }

    public void SetVoiceChange(VoiceEffect voiceEffect)
    {
        Log("SetVoiceChange   " + voiceEffect);

        if (mRtcEngine != null)
        {
            double pitch = 0;

            switch (voiceEffect)
            {
                case VoiceEffect.Original:
                    pitch = 1.0f;
                    int[] eq = new int[10] {0,0,0,0,0,0,0,0,0,0 };
                    SetEQ(eq);
                    break;
                case VoiceEffect.High:
                    pitch = 1.45f;
                    eq = new int[10] { 10, 6, 1, 1, -6, 13, 7, -14, 13, -13 };
                    SetEQ(eq);
                    break;
                case VoiceEffect.Medium:
                    pitch = 1.23f;
                    eq = new int[10] { 15, 11, -3, -5, -7, -7, -9, -15, -15, -15 };
                    SetEQ(eq);
                    break;
                case VoiceEffect.Low:
                    pitch = 0.8f;
                    eq = new int[10] { -15, 0, 6, 1, -4, 1, -10, -5, 3, 3 };
                    SetEQ(eq);
                    break;
                case VoiceEffect.Extra_Low:
                    pitch = 0.6f;
                    eq = new int[10] { -15, 3, -9, -8, -6, -4, -3, -2, -1, 1 };
                    SetEQ(eq);
                    break;
            }
            mRtcEngine.SetLocalVoicePitch(pitch);

        }

    }
    public void SetEQ(int[] eq)
    {
        for (int i = 0; i < eq.Length; i++)
        {
            mRtcEngine.SetLocalVoiceEqualization((AUDIO_EQUALIZATION_BAND_FREQUENCY)i, eq[i]);
        }

    }
    public bool OnPlayerTalking(string senderPlayerId, string msg)
    {
        //Log("OnPlayerTalking   " + senderPlayerId);
        CustomData data = JsonConvert.DeserializeObject<CustomData>(msg);
        if (data != null)
        {

            if (Player.Id == senderPlayerId)
            {

                if (PlayerBaseControl.Inst!=null)
                {
                    PlayerBaseControl.Inst.OnTalkSend(int.Parse(data.data)==0);
                }
            }
            else
            {
                OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(senderPlayerId);
                if (otherCtr != null)
                {
                    otherCtr.GetComponent<OtherPlayerCtr>().OnTalkSend(int.Parse(data.data) == 0);
                }
            }

        }

        return true;
    }

   
    public override void Release()
    {
        OnLeaveRoom();
        OnApplicationQuit();
        base.Release();

    }
    public uint GetId(long timestemp, string playerID)
    {

        return uint.Parse(playerID.Remove(0, playerID.Length - 2) + (timestemp % 10000000).ToString());
    }
    public void AddOtherPlayer(string playerId,long timestemp)
    {
        if (!otherPlayer.ContainsKey(playerId))
        {
           
            OtherPlayerInfo info = new OtherPlayerInfo();
            info.uid = GetId(timestemp, playerId);
            info.isCurMap = true;
            otherPlayer.Add(playerId, info);
        }

    }
    public void RemoveOtherPlayer(string playerId)
    {
        if (otherPlayer.ContainsKey(playerId))
        {
            
            otherPlayer.Remove(playerId);
        }

    }
    public void SetOtherPlayerIsCurMap(string playerId, bool isCurMap)
    {
        if (otherPlayer.ContainsKey(playerId))
        {
            
            otherPlayer[playerId].isCurMap = isCurMap;
        }
    }


    private void LogError(string t)
    {
        LoggerUtils.LogError("UpdateRT*******  " + t);
    }
    private void Log(string t) {
        LoggerUtils.Log("realTime*******  "+t);
    }
    
}
public class OtherPlayerInfo
{
    public uint uid;
    public bool isCurMap;
}
public class RCTTokenInfo{
    public uint uid;
    public string channelName;
    public int role;
}
public class GetToken
{
    public string token;
}
public enum VoiceEffect
{
    Original,
    High,
    Medium,
    Low,
    Extra_Low,
   
}
public enum PermissionType
{
    DIDNT_SET,//为获取
    DONT_ALLOW,//拒绝
    ALLOW,//允许
    DONT_ALLOW_AND_DONT_ASK//拒绝并且不再问

}

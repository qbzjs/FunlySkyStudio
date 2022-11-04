using UnityEngine;
using System;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using AOT;

namespace agora_gaming_rtc {
public class GameMediaEngine : IRtcEngineNative{
    public OnGMEngineRequestTokenHandler OnGMEngineRequestToken = null;
    public OnEnterRoomSuccessHandler OnEnterRoomSuccess = null;
    public OnEnterRoomFailHandler OnEnterRoomFail = null;
    public OnConnectionStateChangeHandler OnConnectionStateChange = null;
    public OnLostSyncHandler OnLostSync = null;
    public OnGetSyncHandler OnGetSync = null;
    public OnTeamMateChangeHandler OnTeamMateChange = null;
    private const string agoraGameObjectName = "agora_gameEngine_CallBackGamObject";
    private AgoraCallbackObject _AgoraCallbackObject = null;
    private static GameMediaEngine instance = null;

    private GameMediaEngine() {
        createAgoraGameMediaEngine_gameEngine();
        InitGameObject();
        InitEngineCallback();
    }

    public static GameMediaEngine CreateAgoraGameMediaEngine() 
    {
        if (instance == null)
        {
            instance = new GameMediaEngine();
        }
        return instance;
    }

    private void InitEngineCallback()
    {
        initEventHandler_gameEngine(OnGMEngineRequestTokenCallback,
                        OnEnterRoomSuccessCallback,
                        OnEnterRoomFailCallback,
                        OnConnectionStateChangeCallback,
                        OnLostSyncCallback, 
                        OnGetSyncCallback,
                        OnTeamMateChangeCallback);
    }

    private void InitGameObject()
    {
        _AgoraCallbackObject = new AgoraCallbackObject(agoraGameObjectName);
    }

    private void DeInitGameObject()
    {
        _AgoraCallbackObject.Release();
        _AgoraCallbackObject = null;
    }

    public int Initialize(string appId, AREA_CODE areaCode = AREA_CODE.AREA_CODE_AS) 
    {
        return initialize_gameEngine(appId, (uint)areaCode);
    }

    public void Release() 
    {
        release_gameEngine();
        DeInitGameObject();
        instance = null;
    }
       
    public int EnableSpatializer(bool enable, bool applyToTeam)
    {
        return enableSpatializer_gameEngine(enable, applyToTeam);
    }
       
    public int SetRangeAudioTeamID(int teamID)
    {
        return setRangeAudioTeamID_gameEngine(teamID);
    }
       
    public int SetRangeAudioMode(int rangeAudioMode)
    {
        return setRangeAudioMode_gameEngine(rangeAudioMode);
    }
       
    public int SetMaxHearAudioCount(int maxCount)
    {
        return setMaxHearAudioCount_gameEngine(maxCount);
    }
       
    public int SetAudioRecvRange(int range)
    {
        return setAudioRecvRange_gameEngine(range);
    }

    public int SetDistanceUnit(float unit)
    {
        return setDistanceUnit_gameEngine(unit);
    }
       
    public int UpdateSelfPosition(int[] position, float[] axisForward,
                         float[] axisRight, float[] axisUp)
    {
        return updateSelfPosition_gameEngine(position, axisForward, axisRight, axisUp);
    }

    public int UpdateRemotePosition_gameEngine(uint uid, int[] position)
    {
        return updateRemotePosition_gameEngine(uid, position);
    }
      
    public int SetParameters(string parames) 
    {
        return setParameters_gameEngine(parames);
    }
      
    public int EnterRoom(string token, string roomName, uint uid, SPACIAL_AUDIO_SYNC_MODE mode)
    {
        return enterRoom_gameEngine(token, roomName, uid, mode);
    }
       
    public bool IsRoomEntered()
    {
        return isRoomEntered_gameEngine();
    }
       
    public int RenewToken(string token)
    {
        return renewToken_gameEngine(token);
    }

    public int ExitRoom()
    {
        return exitRoom_gameEngine();
    }
       
    public int EnableMic(bool enable)
    {
        return enableMic_gameEngine(enable);
    }
       
    public int EnableSpeaker(bool enable)
    {
        return enableSpeaker_gameEngine(enable);
    }
    
    [MonoPInvokeCallback(typeof(OnGMEngineRequestTokenHandler))]
    private static void OnGMEngineRequestTokenCallback()
    {
        if (instance != null && instance.OnGMEngineRequestToken != null && instance._AgoraCallbackObject != null)
        {
            AgoraCallbackQueue queue = instance._AgoraCallbackObject._CallbackQueue;
            if (!ReferenceEquals(queue, null))
            {
                queue.EnQueue(()=> {
                    if (instance != null && instance.OnGMEngineRequestToken != null)
                    { 
                        instance.OnGMEngineRequestToken();
                    }
                });
            }
        }
    }

    [MonoPInvokeCallback(typeof(OnEnterRoomSuccessHandler))]
    private static void OnEnterRoomSuccessCallback()
    {
        if (instance != null && instance.OnEnterRoomSuccess != null && instance._AgoraCallbackObject != null)
        {
            AgoraCallbackQueue queue = instance._AgoraCallbackObject._CallbackQueue;
            if (!ReferenceEquals(queue, null))
            {
                queue.EnQueue(()=> {
                    if (instance != null && instance.OnEnterRoomSuccess != null)
                    { 
                        instance.OnEnterRoomSuccess();
                    }
                });
            }
        }
    }

   [MonoPInvokeCallback(typeof(OnEnterRoomFailHandler))]
    private static void OnEnterRoomFailCallback()
    {
        if (instance != null && instance.OnEnterRoomFail != null && instance._AgoraCallbackObject != null)
        {
            AgoraCallbackQueue queue = instance._AgoraCallbackObject._CallbackQueue;
            if (!ReferenceEquals(queue, null))
            {
                queue.EnQueue(()=> {
                    if (instance != null && instance.OnEnterRoomFail != null)
                    { 
                        instance.OnEnterRoomFail();
                    }
                });
            }
        }
    }

   [MonoPInvokeCallback(typeof(OnConnectionStateChangeHandler))]
    private static void OnConnectionStateChangeCallback(GME_CONNECTION_STATE_TYPE state, GME_CONNECTION_CHANGED_REASON_TYPE reason)
    {
        if (instance != null && instance.OnConnectionStateChange != null && instance._AgoraCallbackObject != null)
        {
            AgoraCallbackQueue queue = instance._AgoraCallbackObject._CallbackQueue;
            if (!ReferenceEquals(queue, null))
            {
                queue.EnQueue(()=> {
                    if (instance != null && instance.OnConnectionStateChange != null)
                    { 
                        instance.OnConnectionStateChange(state, reason);
                    }
                });
            }
        }
    }

   [MonoPInvokeCallback(typeof(OnLostSyncHandler))]
    private static void OnLostSyncCallback(Int64 lostSyncTimeMs)
    {
        if (instance != null && instance.OnLostSync != null && instance._AgoraCallbackObject != null)
        {
            AgoraCallbackQueue queue = instance._AgoraCallbackObject._CallbackQueue;
            if (!ReferenceEquals(queue, null))
            {
                queue.EnQueue(()=> {
                    if (instance != null && instance.OnLostSync != null)
                    { 
                        instance.OnLostSync(lostSyncTimeMs);
                    }
                });
            }
        }
    }

   [MonoPInvokeCallback(typeof(OnGetSyncHandler))]
    private static void OnGetSyncCallback(Int64 lostSyncTimeMs)
    {
        if (instance != null && instance.OnGetSync != null && instance._AgoraCallbackObject != null)
        {
            AgoraCallbackQueue queue = instance._AgoraCallbackObject._CallbackQueue;
            if (!ReferenceEquals(queue, null))
            {
                queue.EnQueue(()=> {
                    if (instance != null && instance.OnGetSync != null)
                    { 
                        instance.OnGetSync(lostSyncTimeMs);
                    }
                });
            }
        }
    }

   [MonoPInvokeCallback(typeof(OnTeamMateChangeHandler))]
    private static void OnTeamMateChangeCallback(IntPtr uids, int userCount)
    {
        if (instance != null && instance.OnTeamMateChange != null && instance._AgoraCallbackObject != null)
        {
            AgoraCallbackQueue queue = instance._AgoraCallbackObject._CallbackQueue;
            if (!ReferenceEquals(queue, null))
            {
                queue.EnQueue(()=> {
                    if (instance != null && instance.OnTeamMateChange != null)
                    { 
                        instance.OnTeamMateChange(uids, userCount);
                    }
                });
            }
        }
    }
}
}

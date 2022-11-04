using BudEngine.NetEngine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Author: shaocheng
/// Description: 数据上报工具,例如统计游玩时长等
/// Date: 2022-5-16 15:54:45
/// </summary>
public class DataLogUtils
{
    public static float totalPlayTime = 0; //游客模式总游玩时长(S)

    private static float totalPingTime; //总联机ping延时
    private static int totalPingCount; //总联机ping次数
    private static bool firstPingLock; //初次ping过滤锁
    private static float averagePingTime; //平均ping延时
    private static float maxPingTime; //最大ping延时
    public static TimeSpan startRestoreTime;
    public static TimeSpan endRestoreTime;
    public static TimeSpan subMapStartPlayTime;
    public static TimeSpan subMapEndPlayTime;

    public static bool IsOpenTotalPlayTimeLog()
    {
#if UNITY_EDITOR
        return true;

#else
        return GameManager.Inst != null && (GameManager.Inst.sceneType == SCENE_TYPE.MAP_SCENE|| GameManager.Inst.sceneType == SCENE_TYPE.MYSPACE_SCENE) && GlobalFieldController.CurGameMode == GameMode.Guest && GameManager.Inst.loadingPageIsClosed;
#endif
    }

    //未来有其他数据统计，也一并clear
    public static void Clear()
    {
        LoggerUtils.Log("DataLogUtils clear");
        totalPlayTime = 0;
        totalPingTime = 0;
        totalPingCount = 0;
        firstPingLock = false;
        averagePingTime = 0;
        maxPingTime = 0;
        startRestoreTime = default;
        endRestoreTime = default;
        subMapStartPlayTime = default;
        subMapEndPlayTime = default;
    }

    #region 进房时长统计

    public static void UpdateTotalPlayTime()
    {
        if (IsOpenTotalPlayTimeLog())
        {
            totalPlayTime += Time.deltaTime;
        }
    }

    //enterMap :进入地图：0 离开地图：1  被T出:2
    public static void LogTotalPlayTime(int enterMap)
    {
        if (IsOpenTotalPlayTimeLog() == false)
        {
            return;
        }

        if (GameManager.Inst.unityConfigInfo == null || GameManager.Inst.ugcUserInfo == null || GameManager.Inst.gameMapInfo == null)
        {
            return;
        }

        LoggerUtils.Log($"LogTotalPlayTime: {enterMap}, {totalPlayTime}");
        MobileInterface.Instance.LogCustomEventData(LogEventData.MAP_EXPERIENCE_INFO, (int)Log_Platform.Backend, new Dictionary<string, object>()
        {
            {"seq", GameManager.Inst.unityConfigInfo.seq},
            {"uid", GameManager.Inst.ugcUserInfo.uid},
            {"map_id", GameManager.Inst.gameMapInfo.mapId},
            {"event_type", enterMap.ToString()},
            {"play_time", enterMap == 0 ? "0" : Math.Round(totalPlayTime).ToString()},
            {"event_timestamp", GameUtils.GetUtcTimeStamp().ToString()},
        });
    }
    public static void LogAudioPlayDuration(double speakDuration , double micDuration)
    {
        if (GameManager.Inst.unityConfigInfo == null || GameManager.Inst.ugcUserInfo == null || GameManager.Inst.gameMapInfo == null)
        {
            return;
        }

        LoggerUtils.Log($"LogAudioPlayDuration: {speakDuration}, {micDuration},{totalPlayTime}");
        MobileInterface.Instance.LogCustomEventData(LogEventData.LEAVE_EXPERIENCE, (int)Log_Platform.ThinkingData, new Dictionary<string, object>()
        {
            {"map_id", GameManager.Inst.gameMapInfo.mapId},
            {"user_id", GameManager.Inst.ugcUserInfo.uid},
            {"speaker_duration", Math.Round(speakDuration).ToString()},
            {"microphone_duration",Math.Round(micDuration).ToString()},
            {"map_duration", Math.Round(totalPlayTime).ToString()},
            {"leave_timestamp", GameUtils.GetUtcTimeStamp().ToString()},
        });
    }

    #endregion

    #region 联机网络延时统计

    public static void ResponseCount(float deltaTime)
    {
        if (!firstPingLock)
        {
            //过滤第一次ping延时计算
            firstPingLock = true;
            return;
        }
        if (deltaTime >= 600000)
        {
            //超过10分钟回包，为异常情况
            LoggerUtils.LogError("Ping deltaTime(ms) error --> " + deltaTime);
            return;
        }

        totalPingCount++;
        totalPingTime += deltaTime;
        //计算平均网络延时
        averagePingTime = totalPingTime / totalPingCount;
        //计算最大网络延时
        if (deltaTime > maxPingTime)
        {
            maxPingTime = deltaTime;
        }
    }

    //上报场景 Emote 使用情况
    public static void LogEmoteClickEvent(EmoIconData emoData, int collectStatus)
    {
        MobileInterface.Instance.LogCustomEventData(LogEventData.SENT_EMOTE, (int)Log_Platform.ThinkingData, new Dictionary<string, object>()
        {
            {"emote_id", emoData.id},
            {"collection_status", collectStatus},
            {"emote_name", emoData.spriteName},
            {"type_name", emoData.emoType},
            {"map_id", GameManager.Inst.gameMapInfo.mapId},
            {"seq", GameManager.Inst.unityConfigInfo.seq},
            {"event_timestamp", GameUtils.GetUtcTimeStamp().ToString()},
            {"uid", GameManager.Inst.ugcUserInfo.uid},
        });
    }

    #endregion

    // 事件组合上报(Firebase + ThinkingData)
    public static void LogCustomEventData(string eventName, Dictionary<string, object> param)
    {
        // 上报Firebase平台
        MobileInterface.Instance.LogCustomEventData(eventName, (int)Log_Platform.Firebase, param);

        // 上报ThinkingData平台，事件名全部转换成大写
        MobileInterface.Instance.LogCustomEventData(eventName.ToUpper(), (int)Log_Platform.ThinkingData, param);
    }
    /// <summary>
    /// 仅上报数数平台
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="param"></param>
    public static void LogCustomEventDataToThinkingData(string eventName, Dictionary<string, object> param)
    {
        MobileInterface.Instance.LogCustomEventData(eventName.ToUpper(), (int)Log_Platform.ThinkingData, param);
    }

    #region 进房流程事件上报
    public static void LogUnityDownLoadABStart(string abFileName)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("abFileName", abFileName);
        LogCustomEventData(LogEventData.unity_downLoadABStart, parameters);
    }

    public static void LogUnityDownLoadABEnd(string abFileName,int abSize)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("abFileName", abFileName);
        parameters.Add("abSize",abSize);
        LogCustomEventData(LogEventData.unity_downLoadABEnd, parameters);
    }

    public static void LogUnityDownLoadABError(string abFileName, string error)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("abFileName", abFileName);
        parameters.Add("error", error);
        LogCustomEventData(LogEventData.unity_downLoadABError, parameters);
    }

    public static void LogUnityDownLoadABFinish(int cache, int total, int error, int useTime)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("cache", cache);
        parameters.Add("total", total);
        parameters.Add("error", error);
        parameters.Add("useTime", useTime);
        LogCustomEventData(LogEventData.unity_downLoadABFinish, parameters);
    }

    public static void LogUnityStartGame()
    {
        var parameters = GetLogCommonParameters();
        LogCustomEventData(LogEventData.unity_startGame, parameters);
    }

    public static void LogUnityGetGameJsonReq()
    {
        var parameters = GetLogCommonParameters();
        LogCustomEventData(LogEventData.unity_getGameJson_req, parameters);
    }

    public static void LogUnityGetGameJsonRsp()
    {
        var parameters = GetLogCommonParameters();
        LogCustomEventData(LogEventData.unity_getGameJson_rsp, parameters);
    }

    public static void LogUnityGetEngineEntyReq()
    {
        var parameters = GetLogCommonParameters();
        LogCustomEventData(LogEventData.unity_getEngineEnty_req, parameters);
    }

    public static void LogUnityGetEngineEntyRsp()
    {
        var parameters = GetLogCommonParameters();
        LogCustomEventData(LogEventData.unity_getEngineEnty_rsp, parameters);
    }

    public static void LogUnityGetMapInfoReq()
    {
        var parameters = GetLogCommonParameters();
        LogCustomEventData(LogEventData.unity_getMapInfo_req, parameters);
    }

    public static void LogUnityGetMapInfoRsp(string code)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("code", code);
        LogCustomEventData(LogEventData.unity_getMapInfo_rsp, parameters);
    }
    
    public static void LogUnityGetMapOfflineABReq()
    {
        var parameters = GetLogCommonParameters();
        LogCustomEventData(LogEventData.unity_getMapOfflineAB_req, parameters);
    }
    public static void LogUnityGetMapOfflineABRsp(string code)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("code", code);
        LogCustomEventData(LogEventData.unity_getMapOfflineAB_rsp, parameters);
    }

    public static void LogUnityGetGameSessionReq(int retry)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("retry", retry);
        LogCustomEventData(LogEventData.unity_getGameSession_req, parameters);
    }

    public static void LogUnityGetGameSessionRsp(string code, int retry)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("code", code);
        parameters.Add("retry", retry);
        LogCustomEventData(LogEventData.unity_getGameSession_rsp, parameters);
    }

    public static void LogUnityRestoreJsonStart()
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("total_memory", (int)(Profiler.GetTotalAllocatedMemoryLong() / 1048576)); // 以前有上传，但是文档上没有标明上传
        LogCustomEventData(LogEventData.unity_restoreJson_start, parameters);
    }

    public static void LogUnityRestoreJsonEnd()
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("total_memory", (int)(Profiler.GetTotalAllocatedMemoryLong() / 1048576)); // 以前有上传，但是文档上没有标明上传
        LogCustomEventData(LogEventData.unity_restoreJson_end, parameters);
    }

    public static void LogUnityMapLight()
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("point", SceneSystem.Inst.FilterNodeBehaviours<PointLightBehaviour>(SceneBuilder.Inst.allControllerBehaviours).Count);
        parameters.Add("spot", SceneSystem.Inst.FilterNodeBehaviours<SpotLightBehaviour>(SceneBuilder.Inst.allControllerBehaviours).Count);
        LogCustomEventData(LogEventData.unity_map_light, parameters);
    }

    /// <summary>
    /// 地图游玩影响因素
    /// </summary>
    public static void LogMapInfo()
    {
        if (GameManager.Inst.unityConfigInfo == null || GameManager.Inst.ugcUserInfo == null || GameManager.Inst.gameMapInfo == null)
        {
            return;
        }
        //获取数据
        int point = SceneSystem.Inst.FilterNodeBehavioursCount<PointLightBehaviour>(SceneBuilder.Inst.allControllerBehaviours);//点光源
        int spot = SceneSystem.Inst.FilterNodeBehavioursCount<SpotLightBehaviour>(SceneBuilder.Inst.allControllerBehaviours);//聚光灯
        int flash = SceneSystem.Inst.FilterNodeBehavioursCount<FlashLightBehaviour>(SceneBuilder.Inst.allControllerBehaviours);
        int fire = SceneSystem.Inst.FilterNodeBehavioursCount<FirePropBehaviour>(SceneBuilder.Inst.allControllerBehaviours);
        int switchbutton = SceneSystem.Inst.FilterNodeBehavioursCount<SwitchButtonBehaviour>(SceneBuilder.Inst.allControllerBehaviours); ;//开关
        int sensorbox = SceneSystem.Inst.FilterNodeBehavioursCount<SensorBoxBehaviour>(SceneBuilder.Inst.allControllerBehaviours); ;//感应盒
        int propstar = SceneSystem.Inst.FilterNodeBehavioursCount<PropStarBehaviour>(SceneBuilder.Inst.allControllerBehaviours); ;//收集
        int maxplayers = GameManager.Inst.gameMapInfo.maxPlayer;
        bool isPvp = (GlobalFieldController.pvpData.pvpMode == 1) ? true : false;//是否PVP
        var ugcbehavList = SceneSystem.Inst.FilterNodeBehaviours<UGCCombBehaviour>(SceneBuilder.Inst.allControllerBehaviours);
        var ugcpropkinds = GlobalFieldController.ugcNodeData != null ? GlobalFieldController.ugcNodeData.Count : 0;
        int attackCount = 0, shotCount = 0, bloodCount = 0;//攻击，射击，回血
        foreach (var ugcbehav in ugcbehavList)
        {
            if (ugcbehav.entity.HasComponent<AttackWeaponComponent>())
            {
                attackCount++;
            }
            if (ugcbehav.entity.HasComponent<ShootWeaponComponent>())
            {
                shotCount++;
            }
            if (ugcbehav.entity.HasComponent<BloodPropComponent>())
            {
                bloodCount++;
            }
        }
        var ugcclothList = SceneSystem.Inst.FilterNodeBehaviours<UgcClothItemBehaviour>(SceneBuilder.Inst.allControllerBehaviours);
        Dictionary<string, int> ugcclothDic = new Dictionary<string, int>();
        foreach (var behv in ugcclothList)
        {
            if (behv.entity.HasComponent<UGCClothItemComponent>())
            {
                var comp = behv.entity.Get<UGCClothItemComponent>();
                if (!string.IsNullOrEmpty(comp.clothMapId))
                {
                    if (ugcclothDic.ContainsKey(comp.clothMapId))
                    {
                        ugcclothDic[comp.clothMapId]++;
                    }
                    else
                    {
                        ugcclothDic.Add(comp.clothMapId, 1);
                    }
                }
            }
        }
        //封装数据
        var parameters = GetLogCommonParameters();
        parameters.Add("light", point + spot + flash + fire);
        parameters.Add("watercube", SceneSystem.Inst.FilterNodeBehavioursCount<WaterCubeBehaviour>(SceneBuilder.Inst.allControllerBehaviours));
        parameters.Add("ugcpropcount", ugcbehavList.Count);
        parameters.Add("ugcpropkinds", ugcpropkinds);
        parameters.Add("steeringWheel", SceneSystem.Inst.FilterNodeBehavioursCount<SteeringWheelBehaviour>(SceneBuilder.Inst.allControllerBehaviours));
        parameters.Add("ugccloth", ugcclothDic.Count);
        parameters.Add("attackweapon", attackCount);
        parameters.Add("shootweapon", shotCount);
        parameters.Add("bloodprop", bloodCount);
        parameters.Add("controllprop", switchbutton + sensorbox + propstar);
        parameters.Add("ispvp", isPvp);
        parameters.Add("maxplayer", maxplayers);
        parameters.Add("averagePing", Math.Round(averagePingTime, 2));
        parameters.Add("averageFPS", Math.Round(FPSController.Inst.GetAverageFPS(), 2));
        parameters.Add("nodeCount", SceneBuilderUtils.Inst.GetAllNodeCount());
        parameters.Add("targetFPS", Application.targetFrameRate);
        double microphoneTime = 0;
        double speakerTime = 0; 
        if(RealTimeTalkManager.Inst != null)
        {
            microphoneTime = RealTimeTalkManager.Inst.micTime;
            speakerTime = RealTimeTalkManager.Inst.audioTime;
        }
        parameters.Add("speaker_duration", Math.Round(microphoneTime));
        parameters.Add("microphone_duration",Math.Round(speakerTime));
        parameters.Add("map_duration", Math.Round(totalPlayTime));
        parameters.Add("dTextCount", SceneBuilder.Inst.Get3DTextCount());
        parameters.Add("isHLOD", HLODSystem.HLOD.Inst.IsValid ? 1:0);
        parameters.Add("isOcclusion", MapRenderManager.Inst.isOcclusionEnable ? 1:0);
        parameters.Add("totalAB", GlobalFieldController.ugcNodeData.Count);
        parameters.Add("particle", SceneSystem.Inst.GetParticlesCount(SceneBuilder.Inst.allControllerBehaviours));
        MobileInterface.Instance.LogCustomEventData(LogEventData.LEAVE_ROOM, (int)Log_Platform.ThinkingData, parameters);
    }
    public static void LogUnityInitSdkStart()
    {
        var parameters = GetLogCommonParameters();
        LogCustomEventData(LogEventData.unity_initSDK_start, parameters);
    }

    public static void LogUnityInitSdkSuccess(int retry)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("retry", retry);
        LogCustomEventData(LogEventData.unity_initSDK_success, parameters);
    }

    public static void LogUnityEnterRoomReq(int retry)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("retry", retry);
        LogCustomEventData(LogEventData.unity_enterRoom_req, parameters);
    }

    public static void LogUnityEnterRoomRsp(string code, int retry)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("code", code);
        parameters.Add("retry", retry);
        parameters.Add("entrance", GameManager.Inst.onLineDataInfo != null ? GameManager.Inst.onLineDataInfo.entrance : "");
        LogCustomEventData(LogEventData.unity_enterRoom_rsp, parameters);
    }

    public static void LogUnityStartFrameStepSend(int retry)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("retry", retry);
        LogCustomEventData(LogEventData.unity_startFrameStep_send, parameters);
    }

    public static void LogUnityStartFrameStepRecv(string code, int retry)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("code", code);
        parameters.Add("retry", retry);
        LogCustomEventData(LogEventData.unity_startFrameStep_recv, parameters);
    }

    public static void LogUnityCloseLoadingPage()
    {
        var parameters = GetLogCommonParameters();
        LogCustomEventData(LogEventData.unity_closeLoadingPage, parameters);
    }

    public static void LogUnityCloseLoadingPageSuccess(bool isSuccess)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("code", isSuccess ? "0" : "1");
        LogCustomEventData(LogEventData.unity_closeLoadingPageSuccess, parameters);
    }

    public static void LogEnterRoom()
    {
        // 房间信息不存在则不上报
        if (Global.Room == null || Global.Room.RoomInfo == null)
            return;

        var room_type = GameManager.Inst.onLineDataInfo != null ? GameManager.Inst.onLineDataInfo.isPrivate : 0;
        var mapid = GameManager.Inst.gameMapInfo != null ? GameManager.Inst.gameMapInfo.mapId : "";
        var roomcode = Global.Room.RoomInfo.Id;
        var category = GameManager.Inst.sceneType == SCENE_TYPE.MYSPACE_SCENE ? "myspace" : "experience";
        var server_language = Global.Room.RoomInfo.Lang != null ? Global.Room.RoomInfo.Lang : "";//房间语言码
        var server_capacity = Global.Room.RoomInfo.MaxPlayers;
        var server_ppl_count = Global.Room.RoomInfo.PlayerList != null ? Global.Room.RoomInfo.PlayerList.Count : 0;
        var user_language = (GameManager.Inst.baseGameJsonData != null && GameManager.Inst.baseGameJsonData.baseInfo != null) ? GameManager.Inst.baseGameJsonData.baseInfo.lang : "";
        var user_ip = ClientManager.Inst != null ? ClientManager.Inst.CountryCode : "";
        var entrance = GameManager.Inst.onLineDataInfo != null ? GameManager.Inst.onLineDataInfo.entrance : "";
        var seq = GameManager.Inst.unityConfigInfo != null ? GameManager.Inst.unityConfigInfo.seq : "";
        var server_ip = Global.Room.RoomInfo.Locale != null ? Global.Room.RoomInfo.Locale : "";//房间地区码
        List<string> ppl_in_room_ip_list = new List<string>();//用户加入server时，server 内所有用户的 IP 列表
        List<string> ppl_in_room_language_list = new List<string>();//用户加入server时，server 内所有用户的语言码列表
        foreach (var player in Global.Room.RoomInfo.PlayerList)
        {
            ppl_in_room_ip_list.Add(player.Locale);
            ppl_in_room_language_list.Add(player.Lang);
        }

        var parameters = new Dictionary<string, object>();
        parameters.Add("room_type", room_type);
        parameters.Add("mapid", mapid);
        parameters.Add("roomcode", roomcode);
        parameters.Add("category", category);
        parameters.Add("server_language", server_language);
        parameters.Add("server_capacity", server_capacity);
        parameters.Add("server_ppl_count", server_ppl_count);
        parameters.Add("user_language", user_language);
        parameters.Add("user_ip", user_ip);
        parameters.Add("entrance", entrance);
        parameters.Add("seq", seq);
        parameters.Add("server_ip", server_ip);
        parameters.Add("ppl_in_room_ip_list", string.Join(",", ppl_in_room_ip_list));
        parameters.Add("ppl_in_room_language_list", string.Join(",", ppl_in_room_language_list));
        LogCustomEventData(LogEventData.ENTER_ROOM, parameters);
    }

    public static void LogUnityLeaveRoomReq()
    {
        var parameters = GetLogCommonParameters();
        LogCustomEventData(LogEventData.unity_leaveRoom_req, parameters);
    }

    public static void LogUnityPingTimeSend()
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("region", GameManager.Inst.baseGameJsonData != null ? GameManager.Inst.baseGameJsonData.baseInfo.locale : "cn_unity");
        parameters.Add("maxPing", ((int)maxPingTime).ToString());
        parameters.Add("averagePing", ((int)averagePingTime).ToString());
        LogCustomEventData(LogEventData.unity_pingTime_send, parameters);
    }

    public static void LogUnityUserInfoReq()
    {
        var parameters = GetLogCommonParameters();
        LogCustomEventData(LogEventData.unity_userInfo_req, parameters);
    }

    public static void LogUnityUserInfoRsp(string code)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("code", code);
        LogCustomEventData(LogEventData.unity_userInfo_rsp, parameters);
    }
    #endregion

    // 获取上报通用参数的数据
    private static Dictionary<string, object> GetLogCommonParameters()
    {
        // 获取数据
        var scene = GameManager.Inst.engineEntry != null ? GameManager.Inst.engineEntry.sceneType : 0;
        var subType = GameManager.Inst.engineEntry != null ? GameManager.Inst.engineEntry.subType : 0;
        var roomMode = GameManager.Inst.onLineDataInfo != null ? GameManager.Inst.onLineDataInfo.roomMode : 0;
        var isPrivate = GameManager.Inst.onLineDataInfo != null ? GameManager.Inst.onLineDataInfo.isPrivate : 0;
        var roomCode = (ClientManager.Inst != null && !string.IsNullOrEmpty(ClientManager.Inst.roomCode)) ? ClientManager.Inst.roomCode :
            (GameManager.Inst.onLineDataInfo != null && !string.IsNullOrEmpty(GameManager.Inst.onLineDataInfo.roomCode)) ? GameManager.Inst.onLineDataInfo.roomCode : "";
        var mapId = GameManager.Inst.ugcUntiyMapDataInfo != null ? GameManager.Inst.ugcUntiyMapDataInfo.mapId : "";
        var seq = GameManager.Inst.unityConfigInfo != null ? GameManager.Inst.unityConfigInfo.seq : "";
        var sessionId = (ClientManager.Inst != null && !string.IsNullOrEmpty(ClientManager.Inst.SessionId)) ? ClientManager.Inst.SessionId.Replace("arn:aws:gamelift:us-west-1::gamesession/", "") : "";

        // 封装数据
        var parameters = new Dictionary<string, object>();
        parameters.Add("scene", scene);
        parameters.Add("subType", subType);
        parameters.Add("roomMode", roomMode);
        parameters.Add("isPrivate", isPrivate);
        parameters.Add("roomCode", roomCode);
        parameters.Add("mapId", mapId);
        parameters.Add("seq", seq);
        parameters.Add("sessionId", sessionId);

        // 返回数据
        return parameters;
    }

    //新用户关注数
    public static void NewUserFollowers()
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("category", "unity");
        LogCustomEventData(LogEventData.FOLLOW_SUCCESS, parameters);
    }

    /// <summary>
    /// 新用户好友数
    /// </summary>
    /// <param name="acceptUid">接收者uid</param>
    public static void NewUserFriends(string acceptUid,string startUid)
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("accept_uid", acceptUid);//接收者
        parameters.Add("add_uid", startUid);//发起者
        parameters.Add("category", "unity");
        LogCustomEventData(LogEventData.ADD_FRIEND_SUCCESS, parameters);
    }


    /// <summary>
    /// 点击添加好友
    /// </summary>
    /// <param name="addUid">发起好友申请的UID</param>
    public static void AddNewUserFriend()
    {
        var parameters = GetLogCommonParameters();
        parameters.Add("category", "unity");
        LogCustomEventData(LogEventData.ADD_FRIEND, parameters);
    }
    /// <summary>
    /// 3D素材预览
    /// </summary>
    /// <param name="isDc">该素材是否为DC素材（0-不是，1-是）</param>
    public static void View3DProps()
    {
        if (GameManager.Inst.gameMapInfo == null)
        {
            return;
        }
        string item_id = "";
        string budactid = "";
        if (GameManager.Inst.gameMapInfo.dcInfo != null)
        {
            item_id = GameManager.Inst.gameMapInfo.dcInfo.itemId;
            budactid = GameManager.Inst.gameMapInfo.dcInfo.budActId;
        }
        var parameters = new Dictionary<string, object>();
        parameters.Add("dc", GameManager.Inst.gameMapInfo.isDC);
        parameters.Add("item_id", item_id);
        parameters.Add("bud_act_id", budactid);
        parameters.Add("map_id", GameManager.Inst.gameMapInfo.mapId);
        LogCustomEventDataToThinkingData(LogEventData.VIEW_3D, parameters);
    }
    /// <summary>
    /// 场景内素材、衣服详情页预览
    /// </summary>
    /// <param name="mapid"></param>
    /// <param name="dCInfo">dc信息</param>
    /// <param name="item">详情页类型（clothing、prop）</param>
    public static void DetailPageView(string mapid, DCInfo dCInfo, string item)
    {
        string budactid = "";
        string item_id = "";
        string dc_type = "";
        int dc = dCInfo != null ? 1 : 0;
        if (dCInfo != null)
        {
            var dctype = dCInfo.dcType;//owned/listing/resell
            item_id = dCInfo.itemId;
            budactid = dCInfo.budActId;//dc批次id
            dc_type = ((DetailPageDCType)dctype).ToString();
        }
        var parameters = new Dictionary<string, object>();
        parameters.Add("dc", dc);
        parameters.Add("dc_type", dc_type);
        parameters.Add("category", "unity");
        parameters.Add("item", item);
        parameters.Add("item_id", item_id);
        parameters.Add("bud_act_id", budactid);
        parameters.Add("map_id", mapid);
        LogCustomEventDataToThinkingData(LogEventData.DETAIL_PAGE_VIEW, parameters);
    }

    /// <summary>
    /// 上报衣服试穿
    /// </summary>
    /// <param name="classtype">物品类型</param>
    /// <param name="dcType">资源类型（0-非官方DC,1-官方DC,2-非DC）</param>
    /// <param name="pgcid">官方物品id(仅官方物品有)</param>
    /// <param name="mapid">ugcy衣服mapid（仅UGC衣服有）</param>
    public static void LogAvatarWear(string classtype, int dcType, int pgcid = -1, string mapid = "")
    {
        var parameters = new Dictionary<string, object>();
        parameters.Add("dcitem_type", classtype);
        parameters.Add("official", dcType);
        parameters.Add("pgcdc_id", pgcid);
        parameters.Add("map_id", mapid);
        LogCustomEventDataToThinkingData(LogEventData.AVATAR_DC_WEAR, parameters);
    }

    /// <summary>
    ///  通过RoleData上报衣服试穿（适用于搭配，场景内换装）
    /// </summary>
    public static void LogWearByRoleData(RoleData roleData)
    {
        AvatarPGCWear(ClassifyType.headwear, roleData.hatId);
        AvatarPGCWear(ClassifyType.glasses, roleData.glId);
        AvatarPGCWear(ClassifyType.hand, roleData.hdId);
        AvatarPGCWear(ClassifyType.shoes, roleData.shoeId);
        AvatarPGCWear(ClassifyType.bag, roleData.bagId);
        AvatarPGCWear(ClassifyType.bag, roleData.cbId);
        AvatarPGCWear(ClassifyType.eyes, roleData.eId);
        AvatarPGCWear(ClassifyType.brows, roleData.bId);
        AvatarPGCWear(ClassifyType.nose, roleData.nId);
        AvatarPGCWear(ClassifyType.mouth, roleData.mId);
        AvatarPGCWear(ClassifyType.blush, roleData.bluId);
        AvatarPGCWear(ClassifyType.hair, roleData.hId);
        AvatarPGCWear(ClassifyType.accessories, roleData.acId);
        AvatarPGCWear(ClassifyType.special, roleData.saId);
        if (!string.IsNullOrEmpty(roleData.clothMapId))
        {
            int isDC = roleData.ugcClothType == (int)UGCClothesResType.DC ? 1 : 0;
            AVatarUGCWear(roleData.clothMapId, isDC, ClassifyType.ugcCloth);
        }
        else
        {
            AvatarPGCWear(ClassifyType.outfits, roleData.cloId);
        }
        if (!string.IsNullOrEmpty(roleData.ugcFPData.ugcMapId))
        {
            int isDC = roleData.ugcClothType == (int)UGCClothesResType.DC ? 1 : 0;
            AVatarUGCWear(roleData.ugcFPData.ugcMapId, isDC, ClassifyType.ugcPatterns);
        }
        else
        {
            AvatarPGCWear(ClassifyType.patterns, roleData.fpId);
        }
    }

    /// <summary>
    /// UGCTryOn-试穿
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="isDc">是否为DC</param>
    /// <param name="type"></param>
    public static void LogTryOnWear(string mapId, int isDc, ClassifyType type)
    {
        int isdc = isDc > 0 ? 1 : 0;
        AVatarUGCWear(mapId, isdc, type);
    }
    /// <summary>
    /// PGC试穿(点击Item,TryOn官方DC,换装搭配)
    /// </summary>
    /// <param name="type">物品类型</param>
    /// <param name="id">物品id</param>
    /// <param name="grading">物品权限</param>
    public static void AvatarPGCWear(ClassifyType type, int id, int grading = -1)
    {
        if (grading == -1)
        {
            var data = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId(type, id);
            if (data != null)
            {
                grading = data.grading;
            }
        }
        var resType = grading == (int)RoleResGrading.DC ? (int)AvatarResType.PGCDC : (int)AvatarResType.Normal;
        LogAvatarWear(type.ToString(), resType, id);
    }
    /// <summary>
    /// UGC试穿(点击UGCItem、TryOn、搭配换装)
    /// </summary>
    /// <param name="clothmapid">UGCID</param>
    /// <param name="type">UGC类型</param>
    public static void AVatarUGCWear(string clothmapid, int isDC, ClassifyType type)
    {
        var resType = isDC == (int)RoleResGrading.DC ? (int)AvatarResType.UGCDC : (int)AvatarResType.Normal;
        LogAvatarWear(type.ToString(), resType, -1, clothmapid);
    }

    /// <summary>
    /// 拍照上传
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="clickmethod">拍照模式（0-快拍，1-相机模式，2-自拍模式）</param>
    public static void LogUploadAlbumStart(string fileName, int clickmethod)
    {
        if (GameManager.Inst.gameMapInfo == null)
        {
            return;
        }
        var category = GameManager.Inst.sceneType == SCENE_TYPE.MYSPACE_SCENE ? "myspace" : "experience";
        var parameters = GetLogCommonParameters();
        parameters.Add("file", fileName);
        parameters.Add("click_method", clickmethod);
        parameters.Add("map_id", GameManager.Inst.gameMapInfo.mapId);
        parameters.Add("category", category);
        LogCustomEventData(LogEventData.UNITY_UPLOAD_ALBUM_START, parameters);
    }

    /// <summary>
    /// 拍照状态埋点上传
    /// </summary>
    /// <param name="state"> 拍照状态 </param>
    /// <param name="click_method"> 拍照模式（0-快拍，1-相机模式，2-自拍模式）</param>
    public static void LogTakePhoto(string state, int click_method)
    {
        if (GameManager.Inst.gameMapInfo == null)
        {
            return;
        }
        var category = GameManager.Inst.sceneType == SCENE_TYPE.MYSPACE_SCENE ? "myspace" : "experience";
        var parameters = GetLogCommonParameters();
        parameters.Add("state", state);
        parameters.Add("click_method", click_method);
        parameters.Add("map_id", GameManager.Inst.gameMapInfo.mapId);
        parameters.Add("category", category);
        LogCustomEventData(LogEventData.UNITY_TAKE_PHOTO_STATE, parameters);
    }

    /// <summary>
    /// 用户成功触发传送进入子地图时上报
    /// </summary>
    public static void LogEnterSubMapSuccess()
    {
        if (GameManager.Inst.gameMapInfo == null)
        {
            return;
        }
        var parameters = GetLogCommonParameters();
        var subMapId = GlobalFieldController.CurMapInfo.mapId;
        var mainSeq = GameManager.Inst.unityConfigInfo != null ? GameManager.Inst.unityConfigInfo.seq : "";
        var sub_seq = GameManager.Inst.curSubSeq;
        parameters.Add("main_map_id", GameManager.Inst.gameMapInfo.mapId);
        parameters.Add("sub_map_id", subMapId);
        parameters.Add("main_seq", mainSeq);
        parameters.Add("sub_seq", sub_seq);
        LogCustomEventData(LogEventData.ENTER_EXPV2_SELECTED, parameters);
    }

    /// <summary>
    /// 用户进入被大世界配置的子地图关闭 Loading 页时上报
    /// </summary>
    public static void LogRestoreSubMapTime()
    {
        if (GameManager.Inst.gameMapInfo == null)
        {
            return;
        }
        GetDowntownSubMapSeq();
        var parameters = GetLogCommonParameters();
        var subMapId = GlobalFieldController.CurMapInfo.mapId;
        var mainSeq = GameManager.Inst.unityConfigInfo != null ? GameManager.Inst.unityConfigInfo.seq : "";
        var sub_seq = GameManager.Inst.curSubSeq;
        var enter_usetime = GameUtils.GetDeltaTime(startRestoreTime, endRestoreTime);
        parameters.Add("main_map_id", GameManager.Inst.gameMapInfo.mapId);
        parameters.Add("sub_map_id", subMapId);
        parameters.Add("main_seq", mainSeq);
        parameters.Add("sub_seq", sub_seq);
        parameters.Add("enter_usetime", enter_usetime);
        LogCustomEventData(LogEventData.ENTER_EXPV2_SELECTED_SUCCESS, parameters);
    }

    /// <summary>
    /// 用户从离开被大世界配置的子地图时上报（包含主动离开和被动离开）
    /// </summary>
    public static void LEAVE_EXPV2_SELECTED()
    {
        if (GameManager.Inst.gameMapInfo == null)
        {
            return;
        }
        var parameters = GetLogCommonParameters();
        var subMapId = GlobalFieldController.CurMapInfo.mapId;
        var mainSeq = GameManager.Inst.unityConfigInfo != null ? GameManager.Inst.unityConfigInfo.seq : "";
        var sub_seq = GameManager.Inst.curSubSeq;
        var play_time = GameUtils.GetDeltaTime(subMapStartPlayTime, subMapEndPlayTime);
        var fps = Math.Round(FPSController.Inst.GetAverageFPS(), 2);
        var ping = averagePingTime;

        parameters.Add("main_map_id", GameManager.Inst.gameMapInfo.mapId);
        parameters.Add("sub_map_id", subMapId);
        parameters.Add("main_seq", mainSeq);
        parameters.Add("sub_seq", sub_seq);
        parameters.Add("play_time", play_time);
        parameters.Add("fps", fps);
        parameters.Add("ping", ping);
        LogCustomEventData(LogEventData.LEAVE_EXPV2_SELECTED, parameters);
    }

    public static void GetDowntownSubMapSeq()
    {
        GameManager.Inst.curSubSeq = GameUtils.GetUtcTimeStamp().ToString();
    }
}
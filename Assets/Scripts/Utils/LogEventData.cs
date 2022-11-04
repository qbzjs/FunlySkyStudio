using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogEventData
{
    public static string unity_startGame = "unity_startGame";
    public static string unity_getGameJson_req = "unity_getGameJson_req";
    public static string unity_getGameJson_rsp = "unity_getGameJson_rsp";
    public static string unity_getEngineEnty_req = "unity_getEngineEnty_req";
    public static string unity_getEngineEnty_rsp = "unity_getEngineEnty_rsp";
    public static string unity_getMapInfo_req = "unity_getMapInfo_req";
    public static string unity_getMapInfo_rsp = "unity_getMapInfo_rsp";
    public static string unity_getMapOfflineAB_rsp = "unity_getMapOfflineAB_rsp";
    public static string unity_getMapOfflineAB_req = "unity_getMapOfflineAB_req";
    public static string unity_downloadJson_req = "unity_downloadJson_req";
    public static string unity_downloadJson_rsp = "unity_downloadJson_rsp";
    public static string unity_restoreJson_start = "unity_restoreJson_start";
    public static string unity_restoreJson_end = "unity_restoreJson_end";
    public static string unity_downTempJson_req = "unity_downTempJson_req";
    public static string unity_downTempJson_rsp = "unity_downTempJson_rsp";
    public static string unity_restoreTemp_start = "unity_restoreTemp_start";
    public static string unity_restoreTemp_end = "unity_restoreTemp_end";
    public static string unity_userInfo_req = "unity_userInfo_req";
    public static string unity_userInfo_rsp = "unity_userInfo_rsp";
    public static string unity_closeLoadingPage = "unity_closeLoadingPage";
    public static string unity_closeLoadingPageSuccess = "unity_closeLoadingPageSuccess";
    public static string unity_getGameSession_req = "unity_getGameSession_req";
    public static string unity_getGameSession_rsp = "unity_getGameSession_rsp";
    public static string unity_initSDK_start = "unity_initSDK_start";
    public static string unity_initSDK_success = "unity_initSDK_success";
    // public static string unity_getMyRoom_req = "unity_getMyRoom_req";
    // public static string unity_getMyRoom_rsp = "unity_getMyRoom_rsp";
    // public static string unity_matchRoom_req = "unity_matchRoom_req";
    // public static string unity_matchRoom_rsp = "unity_matchRoom_rsp";
    // public static string unity_createRoom_req = "unity_createRoom_req";
    // public static string unity_createRoom_rsp = "unity_createRoom_rsp";
    // public static string unity_joinRoom_req = "unity_joinRoom_req";
    // public static string unity_joinRoom_rsp = "unity_joinRoom_rsp";

    public static string unity_enterRoom_req = "unity_enterRoom_req";
    public static string unity_enterRoom_rsp = "unity_enterRoom_rsp";
    public static string unity_timeout_kickout = "unity_timeout_kickout";
    public static string unity_leaveRoom_req = "unity_leaveRoom_req";
    public static string unity_leaveRoom_rsp = "unity_leaveRoom_rsp";
    public static string unity_startFrameStep_send = "unity_startFrameStep_send";
    public static string unity_startFrameStep_recv = "unity_startFrameStep_recv";
    public static string unity_clickSave = "unity_clickSave";
    public static string unity_zipJson_start = "unity_zipJson_start";
    public static string unity_zipJson_success = "unity_zipJson_success";
    public static string unity_uploadJson_start = "unity_uploadJson_start";
    public static string unity_uploadJson_success = "unity_uploadJson_success";
    public static string unity_uploadPng_start = "unity_uploadPng_start";
    public static string unity_uploadPng_success = "unity_uploadPng_success";
    public static string unity_saveMap_req = "unity_saveMap_req";
    public static string unity_saveMap_rsp = "unity_saveMap_rsp";

    public static string unity_avg_fps = "unity_avg_fps";
    public static string unity_startOffline = "unity_startOffline";
    public static string unity_endOffline = "unity_endOffline";
    public static string unity_downLoadABStart = "unity_downLoadABStart";
    public static string unity_downLoadABEnd = "unity_downLoadABEnd";
    public static string unity_downLoadABError = "unity_downLoadABError";
    public static string unity_downLoadABFinish = "unity_downLoadABFinish";
    public static string unity_map_light = "unity_map_light";
    
    public static string unity_roomchat_req = "unity_roomchat_req";
    public static string unity_roomchat_rsp = "unity_roomchat_rsp";
    public static string unity_roomchat_broadcast = "unity_roomchat_broadcast";
    public static string unity_frame_sended = "unity_frame_sended";
    public static string unity_frame_broadcast = "unity_frame_broadcast";
    public static string unity_pingTime_send = "unity_pingTime_send";

    public static string unity_background = "unity_background";
    public static string unity_foreground = "unity_foreground";

    public static string AVATAR_SUBMIT = "AVATAR_SUBMIT";
    public static string IM_SEND_3D = "IM_SEND_3D";

    public static string UNITY_AVATAR_SET_SCREEN = "UNITY_AVATAR_SET_SCREEN";
    public static string UNITY_SELFIE_CHOOSE_SCREEN = "UNITY_SELFIE_CHOOSE_SCREEN";

    public static string login_new_creation_done = "login_new_creation_done";
    public static string login_new_selfie = "login_new_selfie";
    public static string AVATAR_PAGE_VIEW = "AVATAR_PAGE_VIEW";
    public static string AVATAR_CLICK = "AVATAR_CLICK";
    public static string SELFIE_PAGE_VIEW = "SELFIE_PAGE_VIEW";
    public static string SELFIE_CLICK = "SELFIE_CLICK";
    
    public static string MAP_EXPERIENCE_INFO = "map_experience_info";
    public static string unity_getRoleData_error = "unity_getRoleData_error";
    public static string SENT_EMOTE = "SENT_EMOTE";
    public static string LEAVE_EXPERIENCE = "LEAVE_EXPERIENCE";
    public static string ENTER_ROOM = "ENTER_ROOM";
    public static string ADD_FRIEND_SUCCESS = "ADD_FRIEND_SUCCESS";
    public static string FOLLOW_SUCCESS = "FOLLOW_SUCCESS";
    public static string ADD_FRIEND = "ADD_FRIEND";
    public static string LEAVE_ROOM = "LEAVE_ROOM";
    public static string VIEW_3D = "VIEW_3D";
    public static string AVATAR_DC_WEAR = "AVATAR_DC_WEAR";
    public static string DETAIL_PAGE_VIEW = "DETAIL_PAGE_VIEW";
    public static string UNITY_UPLOAD_ALBUM_START = "UNITY_UPLOAD_ALBUM_START";
    public static string UNITY_TAKE_PHOTO_STATE = "UNITY_TAKE_PHOTO_STATE";

    public static string UNITY_MAINSCENE_START = "UNITY_MAINSCENE_START";

    public static string ENTER_EXPV2_SELECTED = "ENTER_EXPV2_SELECTED";
    public static string ENTER_EXPV2_SELECTED_SUCCESS = "ENTER_EXPV2_SELECTED_SUCCESS";
    public static string LEAVE_EXPV2_SELECTED = "LEAVE_EXPV2_SELECTED";
}

using System;
using System.Collections.Generic;
using System.Globalization;
using Google.Protobuf.Collections;
using BudEngine.NetEngine.src.Util;

namespace BudEngine.NetEngine {
    /** Properties of a Frame. */
    [Serializable]
    public partial class Frame {
        public string RoomId { get; set; }
        public long Time { get; set; }
        public bool IsReplay { get; set; }
        public Frame(Frame frame, string id) {
            RoomId = id;
            Ext.Seed = frame.Ext.Seed;
            Id = frame.Id;
            Items.AddRange(frame.Items);
            Time = 0;
            IsReplay = false;
        }
    }
    /** 房间信息meta 

    /** 帧同步消息广播 */
    public class Signature {
        public Signature() {
            Timestamp = SdkUtil.GetCurrentTimeSeconds();
        }

        public string Sign { get; set; }

        public ulong Nonce { get; set; }

        public ulong Timestamp { get; set; }
    }
    public class BroadcastEvent : IFormattable {
        public BroadcastEvent(Object data, string seq) {
            this.Data = data;
            this.Seq = seq;
        }

        public object Data { get; set; }

        public string Seq { get; set; }

        public string ToString(string format, IFormatProvider provider)
        {
            string str = "{\"Seq\": \"" + this.Seq +
                         "\", \"Data\": " + this.Data?.ToString()
                         + "}";
            return str;
        }
    }

    public class ResponseEvent : IFormattable {
        public ResponseEvent(int code) {
            this.Code = code;
            this.Msg = "";
            this.Seq = "";
            this.Data = null;
        }
        public ResponseEvent(int code, string msg, string seq, Object data) {
            this.Code = code;
            this.Msg = msg;
            this.Seq = seq;
            this.Data = data;
        }

        public int Code { get; set; }

        public string Msg { get; set; }

        public string Seq { get; set; }

        public object Data { get; set; }

        public string ToString(string format, IFormatProvider provider)
        {
            string str = "{\"Code\": " + this.Code +
                         ", \"Seq\": \"" + this.Seq +
                         "\", \"Msg\": \"" + this.Msg +
                         "\", \"Data\": " + this.Data?.ToString() +
                         "}";
            return str;
        }

    }

    public class InitRsp {
        public InitRsp(ulong serverTime) {
            this.ServerTime = serverTime;
        }

        public ulong ServerTime { get; set; }
    }

    // public delegate Action CreateSignature(Signature signature);

    public class ConfigPara {
        public int ReconnectMaxTimes { get; set; }

        public int ReconnectInterval { get; set; }

        public string Url { get; set; }

        public int ResendInterval { get; set; }

        public int ResendTimeout { get; set; }

        public bool IsAutoRequestFrame { get; set; }

        public string CacertNativeUrl { get; set; }

        public int Port { get; set; }
        public int FramePort { get; set; }
    }

    public class ChangeCustomPlayerStatusPara {
        public ulong CustomPlayerStatus { get; set; }
    }

    public class ChangeRoomPlayerProfilePara {
        public string CustomProfile { get; set; }
    }

    public class RequestFramePara {
        public long BeginFrameId { get; set; }

        public long EndFrameId { get; set; }
    }

    /**
     * @doc types.RecvType
     * @name 消息接收者类型
     * @field {1} ROOM_ALL 全部玩家
     * @field {2} ROOM_OTHERS 除自己外的其他玩家
     * @field {3} ROOM_SOME 房间中部分玩家
     */
    public enum RecvType {
        RoomAll = 1,
        RoomOthers = 2,
        RoomSome = 3
    }

    public class GameInfoPara {
        public string GameId { get; set; }

        public string OpenId { get; set; }

        public string SecretKey { get; set; }

        public Action CreateSignature { get; set; }
    }

    public class PlayerInfoPara {
        public string Name { get; set; }

        public ulong CustomPlayerStatus { get; set; }

        public string CustomProfile { get; set; }

        public string ImageChosenDataJson { get; set; }

        public string Lang { get; set; }
        public string Locale { get; set; }
        public string WalletAddr { get; set; }
    }
    
    public class EnterRoomPara
    {
        public string RoomName { get; set; }

        public string RoomType { get; set; }

        public uint MaxPlayers { get; set; }

        public bool IsPrivate { get; set; }

        public string CustomProperties { get; set; }

        public PlayerInfoPara PlayerInfo { get; set; }
        
        public string SessionId { get; set; }

        public string RoomId { get; set; }

        public string GameData { get; set; }

        public string PlayerSessionId { get; set; }

        public string Region { get; set; }
    }

    public class EnterRoomGameInfo
    {
        public string IsPvp { get; set; }

        public uint ReadyTime { get; set; }

        public uint GameDuration { get; set; }

        public uint WinType { get; set; }

        public bool OpenBlood { get; set; }

        public bool HasLeaderBoard { get; set; }
        public float InitBlood { get; set; }
        public bool IsOpenBaggage { get; set; }

        public string Team { get; set; }
        public List<int> damageType { get; set; }
        public string Seq { get; set; }
    }

    public class CreateRoomPara {
        public string RoomName { get; set; }

        public string RoomType { get; set; }

        public uint MaxPlayers { get; set; }

        public bool IsPrivate { get; set; }

        public string CustomProperties { get; set; }

        public PlayerInfoPara PlayerInfo { get; set; }

        public string RoomId { get; set; }

        public string SessionId { get; set; }
    }

    public class CreateTeamRoomPara {
        public string RoomName { get; set; }

        public string RoomType { get; set; }

        public uint MaxPlayers { get; set; }

        public bool IsPrivate { get; set; }

        public string CustomProperties { get; set; }

        public PlayerInfoPara PlayerInfo { get; set; }

        public uint TeamNumber { get; set; }

        public string RoomId { get; set; }

        public string SessionId { get; set; }
    }

    public class JoinRoomPara {
        public PlayerInfoPara PlayerInfo { get; set; }

        public string SessionId { get; set; }

        public string RoomId { get; set; }
    }
    public class JoinTeamRoomPara {
        public PlayerInfoPara PlayerInfo { get; set; }

        public string TeamId { get; set; }

    }

    public class LeaveRoomPara
    {
        public string RoomId { get; set; }

        public string PlayerId { get; set; }
    }

    public class ChangeRoomPara {
        public string RoomName { get; set; }

        public string Owner { get; set; }

        public bool IsPrivate { get; set; }

        public string CustomProperties { get; set; }

        public bool IsForbidJoin { get; set; }
    }

    public class RemovePlayerPara {
        public string RemovePlayerId { get; set; }
    }

    /// <summary>
    ///  获取房间列表参数
    /// </summary>
    public class GetRoomListPara {
        public int PageNo { get; set; }

        public int PageSize { get; set; }

        public string RoomType { get; set; } = "";

        public bool IsDesc { get; set; } = false;
    }

    /// <summary>
    ///  获取房间参数
    /// </summary>
    public class GetRoomByRoomIdPara {
        public string RoomId { get; set; }

        public string SessionId { get; set; }

        public string PlayerId { get; set; }
    }

    public class MatchPlayersPara {
        /** 匹配的玩家信息 */
        private MatchPlayerInfoPara playerInfoPara;

        public string MatchCode { get; set; }

        /** 匹配的玩家信息 */
        public MatchPlayerInfoPara PlayerInfoPara {
            get => playerInfoPara;
            set => playerInfoPara = value;
        }
    }

    public class MatchPlayerInfoPara {
        public string Name { get; set; }

        public ulong CustomPlayerStatus { get; set; }

        public string CustomProfile { get; set; }
    }
    
    public class MatchRoomPara {
        public PlayerInfoPara PlayerInfo { get; set; }

        public ulong MaxPlayers { get; set; }

        public string RoomType { get; set; }

        public string SessionId { get; set; }
    }

    public class MatchGroupPlayerInfoPara {
        public string Id { get; set; }

        public string Name { get; set; }

        public ulong CustomPlayerStatus { get; set; }

        public string CustomProfile { get; set; }
    }

    public class MatchGroupPara {
        public List<MatchGroupPlayerInfoPara> PlayerInfoList { get; set; }

        public string MatchCode { get; set; }
    }

    public class MatchBst {
        public int ErrCode { get; set; }

        public RoomInfo RoomInfo { get; set; }
    }

    public partial class RecvFrameBst {
        public RecvFrameBst (Frame frame, string id) {
            Frame = new Frame (frame, id);
        }
    }

    public class SendFramePara {
        public object Data { get; set; }
    }

    public class SendToClientPara {

        public string RoomId { get; set; }

        public string PlayerId { get; set; }

        public List<string> RecvPlayerList { get; set; }

        public string Msg { get; set; }

        public RecvType RecvType { get; set; }
    }

    [Serializable]
    public class SendToGameSvrPara {
        public object Data { get; set; }
    }

    public class PlayerStateMsg
    {
        public uint BackgroundState { get; set; } //玩家前后台状态 0-前台 1-后台
    }

    public class GetGroupByGroupIdPara {
        public string GroupId { get; set; }
    }
    
    public class GroupPlayerInfoPara {
        public string Name { get; set; }
        public uint CustomGroupPlayerStatus { get; set; }
        public string CustomGroupPlayerProfile { get; set; }
    }
    
    public class CreateGroupPara {
        public string GroupName { get; set; }
        public uint MaxPlayers { get; set; }
        public string CustomProperties { get; set; }
        public GroupPlayerInfoPara PlayerInfo { get; set; }
        public bool IsForbidJoin { get; set; }
        public bool IsPersistent { get; set; }
    }
    
    public class JoinGroupPara {
        public GroupPlayerInfoPara PlayerInfo { get; set; }
    }
    
    public class ChangeGroupPara {
        public string GroupName { get; set; }
        public string Owner { get; set; }
        public string CustomProperties { get; set; }
        public bool IsForbidJoin { get; set; }
    }
    
    public class RemoveGroupPlayerPara {
        public string RemovePlayerId { get; set; }
    }
    
    public class ChangeCustomGroupPlayerStatusPara {
        public uint CustomGroupPlayerStatus { get; set; }
    }
    
    public class ChangeGroupPlayerProfilePara {
        public string CustomProfile { get; set; }
    }
    
    public class SendToGroupClientPara {
        public List<string> RecvPlayerList { get; set; }
        public string Msg { get; set; }
    }

    public class GetGameInfoPara
    {
        public string RoomId { get; set; }
        public int GameType { get; set; }  //游戏类型 1 - pvp
        public int Command { get; set; }
    }

    public class SendGameDataPara
    {
        public string RoomId { get; set; }
        public int ReqType { get; set; }
        public string GameData { get; set; }
    }
}
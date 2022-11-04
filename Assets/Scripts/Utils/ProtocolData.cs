using SavingData;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EngineEntry
{
    public int sceneType = 0;
    public int subType = 0;
}

[Serializable]
public class HttpResponDataStruct
{
    public int result = 0;
    public string rmsg = "";
    public string data = "";
}
[Serializable]
public class HttpResponDataStructBatch
{
    public int result = 0;
    public string rmsg = "";
    public string[] data;
}

[Serializable]
public class UnityImageBean
{
    public string imageUrl = "";
    public int action = 0;
}

[Serializable]
public class RoleResponData
{
    public UserInfo userInfo;
}

[Serializable]
public class UserInfo
{
    public string uid = "";
    public string userNick = "";
    public string userName = "";
    public int gender = 0;
    public string portraitUrl = "";
    public string imageJson = "";//为roledata的序列化字符串
    public Int64 birthday;
    public OfficialCert officialCert; // 官方认证
    public string clothesId = "";//用于传给后端进行审核玩家身上的UGC物品是否被下架("clothesMapid,patternMapid")
    public int clothesIsBan = 0;
    public int facePaintingIsBan = 0;
    public DCUGCItemInfo[] dcUgcInfos; //用户ugc部件信息
    public DCPGCItemInfo[] dcPgcInfos; //用户pgc部件信息
    public PGCInfo[] rewards; //用户奖励部件信息..(奖励不存在转卖情况, 拥有后不会失去, 只需在Avatar编辑页做校验即可)
}

[Serializable]
public class UserDetails
{
    public string uid = "";
    public string userNick = "";
    public string userName = "";
    public string portraitUrl = "";
    public string bio = "";
    public OfficialCert officialCert; // 官方认证
}

// 官方认证
[Serializable]
public class OfficialCert
{
    // 账号类别（0 普通 1 官方账号）
    public int accountClass;
    public Certifications[] certifications;
}

// 官方认证信息
[Serializable]
public class Certifications
{
    public string certName = "";
    public string certIcon = "";
}

[Serializable]
public class RoleUpLoadBody
{
    public UserInfo userInfo;
    public int operationType; //0:非DC部件  1:含DC部件
    public string portraitUrl = "";
}

[Serializable]
public class RoleClosetData
{
    public string imageJson = "";
    public string clothesId = "";
}

[Serializable]
public class RoleSocialData
{
    public UserDetails userInfo;
    public Relationship relation;
}

// 社交关系
[Serializable]
public class Relationship
{
    public int subscribed;  //订阅关注关系（0: 无关系 1 关注 2 被对方关注中 3 互关）
    public int subscribers;  //订阅人数
    public int transactions;  //素材获取次数
    public int likes;  //获赞次数
    public int friendship;  //好友关系（0: 无关系 1 申请中 2 被对方申请中 3 好友）
}

[Serializable]
public class SetSubscribeParam
{
    public string toUid = "";  //关注人uid
    public int operationType;  //行为类型0(follow),1(unfollow),2(addFriend),3(cancelFriend)
    public int clickPage = 1;  //点击页面0(个人主页)1(U3D场景)
    public string recommendId = "";  //推荐id
}

[Serializable]
public class SetLikeParm
{
    public SetLikeMapInfo mapInfo;
    public int operationType;
}

[Serializable]
public class SetLikeMapInfo
{
    public string mapId;
    public int dataType;
    public DCInfo dcInfo;
}

[Serializable]
public class StoreResData
{
    public ResData mapInfo;
}

[Serializable]
public class PVPLeaderBoardData
{
    public RankingTopData[] rankingTops; //排行榜内所有玩家列表
}

[Serializable]
public class RankingTopData
{
    public UserInfo userInfo; //玩家信息
    public int times; //该id对应的胜场
}

[Serializable]
public class ResData
{
    public string mapId = "";//物品id
    public string mapName = "";//物品名
    public string mapDesc = "";//物品详细信息
    public string mapCover = "";//物品图片
    public int isLimit;//购买素材是否受限
    public int lastModifiedTime;//更新日期
    public ResInteractStatus interactStatus; //物品信息
    public UserInfo mapCreator;//创作者信息
    public Relationship relation;//是否关注信息
    public LimitList limitList;//购买权限
    public PropsBuyLevel propsBuyLevel;//权限信息
    public int buyNumLimit;//可购买的总数
    public DCInfo dcInfo;//dc信息
    public int isDC;//是否为dc
    public int nftType; //0:dc 1:airdrop
    public string banner;
}

[Serializable]
public class DCInfo
{
    public string itemId = "";
    public string itemName = "";
    public string itemDesc = "";
    public string itemCover = "";
    public string coverBgValue = "";
    public string itemMetadata = "";
    public string blockchain = "";
    public string supply = "";
    public string soldNum = "";
    public string price = "";
    public string royalties = "";
    public string contractAddress = "";
    public string walletAddress = "";
    public string tokenStandard = "";
    public string tokenId = "";
    public string budActId = "";
    public int itemUgcType;
    public int itemStatus;
    public string maticPrice;
    public int dcType;
    public int nftType; //0:dc 1:airdrop
    public DCUserInfo creatorInfo;
    public DCUserInfo ownerInfo;
}

[Serializable]
public class DCPromoteInfo
{
    public string itemId = "";
    public string walletAddress = "";
    public string budActId = "";
}

[Serializable]
public class SoldNum
{
    public long sold;
}
public class DCGetSoldNumInfo
{
    public string tokenId;
    public string supply;
    public string contractAddress;
    public string walletAddress;
}

[Serializable]
public class DcSaveInfo
{
    public string dcId;
    public string address;
    public int isSoldOut;
}

[Serializable]
public class DcSoldOutListInfo
{
    public List<DcSaveInfo> dcInfos;
}

[Serializable]
public class LimitList
{
    public int numLimit;//是否到购买人数上限  0为不够 1为够
    public int shareWithLimit;//是否设置购买权限  0为未设置  1为设置
}

[Serializable]
public class DCUserInfo
{
    public string uid = "";
    public string userNick = "";
    public string userName = "";
    public string portraitUrl = "";
    public UserRelation relation;
}

[Serializable]
public class UserRelation
{
    public int subscribed;
}

[Serializable]
public class PropsBuyLevel
{
    public int propsType;//权限信息  0公开，1粉丝，2部分好友，3私人
}

[Serializable]
public class ResInteractStatus
{
    public int liked;//是否喜欢
    public int likes;//获赞数
    public int isOwned;//是否购买过
    public int purchasesNum;//购买数
}

# region MMO Data

public enum ProtoCommand
{
    PlayerInfo = 1,
    CharMsg = 2,
    PlayMove = 4
}

enum SERVER_ROOM_TYPE
{
    ROOM = 1,//Normal Room
    ACT = 2,//Action Room
}

enum ROOM_MODE
{
    MATCH = 0,//点加入公共房间
    CREATE = 1,//点加入私人房间
    JOIN = 2,
}


public enum EnterRoomMode {
    Public = 0, //public Room
    Private = 1 //private Room
}

//道具类型标识，和国内不一样,对应GameResType
public enum ItemType
{
    SWITCH = 1017,
    PROP_STAR = 1023,
    MAGNETIC_BOARD = 1020,
    STEERING_WHEEL = 1040,
    PICK_PROP = 1030,
    SENSOR_BOX = 1031,
    TRAP_BOX = 1032,
    FOOD_PROP = 1035,
    
    
    FIREWORK = 1045,
    FREEZEPROPS = 1050,
    FIREPROP = 1055,

    FISHING_PROP = 1060,

    //////////////////////武器道具使用//////////////////////
    ATTACK_WEAPON = 2000, //攻击道具
    BLOOD_PROP = 2001, // 回血道具
    SHOOT_WEAPON = 2002, //射击道具
    WEAPON_OPERATE = 2003, //更新子弹和射击动画


    Ladder = 3001,//梯子

    SLIDE_PIPE= 3002,//滑梯
    VIP_ZONE = 3003, // VIP 区域
    SWORD = 4001,//舞刀
}


enum RoomAttrType
{
    FULL_SWITCH = 1,
    SPAWN = 2,
    COLLECT_ENTITY = 3,
}

public enum OPERATE_TYPE {
    OnBtnDown = 1,
    OnBtnUp = 2,
    StartReload = 3,
    EndReload = 4,
    BulletCalibration = 5,
}

/// <summary>
/// 自定义数据类型
/// </summary>
public enum ChatCustomType
{
    Keyboard,
    JumpOnBoard,
    ChangeCloth,
    Talk,
    ChangeImage,
    DCClothSoldOut,
    DCResSoldOut,
    Bounceplank,
    PGCResSoldOut,
    DowntownTransfer
}

[Serializable]
public class ServerPacket
{
    public int retcode;
    public string rmsg;
    public int msgType;
    public string content;
    public string data; //todo:修改content为data,待测试
}

[Serializable]
public class OnLineDataInfo
{
    public int    roomMode  = 0;  // 0 项目详情点加入公共房间 1 项目详情点加入私人房间 2 分享的地图
    public int    isPrivate = 0;  // 0 公共房间 1 私人房间
    public string roomCode  = ""; // 房间码
    public string entrance  = ""; // 进房来源
}

//帧同步数据
public class UgcFrameData
{
    public Vector3 playerPos;
    public Quaternion playerRot;
    public bool IsMoving;
    public bool IsGround;
    public bool IsFlying;
    public string mapId;
    public bool IsFastRun;
    public bool IsInWater;
    public bool IsSwimming;
    public int AnimType;
    public int StateType;
}

public class RolesImageReq
{
    public string uids;
}

public class BatchGetUserInfo
{
    public string[] uids;
}

[Serializable]
public class RolesImageRsp
{
    public UserInfo[] images;
}

public class GetSessionReq
{
    public string roomType = "";
    public int isPrivate = 0;
    public string roomCode = "";
    public int maxPlayerCount = 8;
    public string isPvp = "0";
}
public class closeSessionParam
{
    public string roomType = "";
    public int isPrivate = 0;
    public string roomCode = "";
    public int maxPlayerCount = 8;
    public string isPvp = "0";
    public string sessionId = "";
}
public class SessionInfo
{
    public string sessionId = "";
    public string ipAddress = "";
    public int port = 0;
    public int framePort = 0;
    public string roomId = "";
    public string roomLang = "";
    public int spawnIdx = 1;
    public string countryCode = "";
    public string playerSessionId = "";
    public string region = "";
}

public class RoomChatResp
{
    public string RoomId = "";
    public string SendPlayerId = "";
    public string Msg = "";
}

public class RoomChatData
{
    public int msgType = 0;
    public string data = "";
    public string requestSeq = ""; //唯一RoomChat请求号，由客户端发req时上报，服务器进行透传
}

[Serializable]
public class ServerRsp
{
    public int retcode;
    public string rmsg;
    // public int msgType;
    public string requestSeq;
    public Item item;
}

public class RoomChatCustomData
{
    public int type;
    public string data;
}

public class NativeShareParams
{
    public string roomCode = "";
    public string mapId = "";
    public string creatorIcon = "";
    public string creatorName = "";
    public string creatorUid = "";
    public string mapCover = "";
    public string mapName = "";
    public string mapDesc = "";
}

public class DowntownNativeShareParams
{
    public string roomCode = "";
    public string downtownId = "";
    public string downtownCover = "";
    public string downtownDesc = "";
    public string downtownName = "";
    public string downtownPngPrefix = "";
}

[Serializable]
public class GetItemsReq
{
    public string mapId;
    public int bigMap; //0.普通地图  1.大世界地图
}

[Serializable]
public class GetItemsRsp 
{
    public string mapId;
    public int msgType;
    public Item[] items;
    public PlayerBloodData[] playerBlood;
    public RoomAttr[] roomAttrs;
    public PlayerCustomData[] playerCustomDatas;
    public PlayerLastFrame[] playerLatestFrames;
    public ActivityData[] activityDatas;
    public string requestSeq = "";
}


[Serializable]
public class Item
{
    public int id;
    public int type;
    public string data;
}

[Serializable]
public class HoldItemCallRsp
{
    public string mapId;
    public Item lastItem;
    public Item curItem;
}

[Serializable]
public class SwordCallRsp
{
    public int part;
    public int opType;// 1 穿戴 2 退出
}

[Serializable]
public class CustomData
{
    public int type;
    public string data;
}

[Serializable]
public class PromoteData
{
    public string mapId;
    public int status; //1.选货 2.开始摆摊 3.结束摆摊 4.吆喝 5.介绍 6.服务器通知播放动画
    public string extraData; //广播摆摊内容数据
}

[Serializable]
public class FishingData
{
    public string mapId;
    public int option; // 1.开始钓鱼 2.结束钓鱼
    public string position;
    public Item item;
    public int code;
}

[Serializable]
public class ActivityData
{
    public string activityName;
    public int activityId;
    public string data;
}

[Serializable]
public class PlayerBloodData
{
    public string playerId;
    public float value;
    public int alive;  //1存活 2死亡
}


[Serializable]
public class RoomAttr
{
    public int type;
    public string data;
}


[Serializable]
public class PlayerCustomData
{
    public string playerId;
    public Item[] items; //目前只有动画
    public Item[] props;
    public string[] baggageItems;
    public string curHoldItem;
    public ActivityData[] activitiesData;
}
[Serializable]
public class DefeatMsg
{
    public string msg;
    public int chatId;
}

[Serializable]
public class PurchasedTextData
{
    public string source;
    public string goods;
    public string imgUrl;
}

[Serializable]
public class GetMsgsReq
{
    public int lastChatId;
}
[Serializable]
public class GetMsgsRsp
{
    public chatMsgs[] chatMsgs;
}
[Serializable]
public class chatMsgs
{
    public long timestamp;//兼容旧版本
    public string playerName = "";
    public string msg = "";
    public int type = 0;
    public int id = 0;
}

public class PlayerLastFrame
{
    public string player_id;
    public string data;
    public long timestamp;
}

[Serializable]
public class SendRoomAttrReq
{
    public string roomId;
    public List<RoomAttr> roomAttrs;

}

[Serializable]
public class SyncItemsReq
{
    public string retcode;
    public string rmsg;
    public string mapId;
    public Item[] items;
}


[Serializable]
public class SyncBaggageItemReq
{
    public string retcode;
    public string rmsg;
    public string mapId;
    public Item item;
}

[Serializable]
public class SwitchPack
{
    public int status; // 1-开关打开，0-开关关闭
}

[Serializable]
public class MagneticData
{
    public int status;//0:下磁力板，1:上磁力板
    public string playerId;//操作玩家
}

[Serializable]
public class SeesawSendData
{
    public string mapId;
    public int itemId; // 道具Id
    public string uidLeft; // 左边座椅玩家的Id
    public string uidRight; // 右边座椅的玩家Id
    public int side; // 0-左，1-右
    public int option; // 1-使用，2-释放，3-施力
    public float angle; // 当前旋转角度
    public float speed; // 当前速度
    public float minAngle; // 旋转最小角度
    public float maxAngle; // 旋转最大角度
    public long lastUpdateTime; // 上次操作的时间（ms）
}

[Serializable]
public class SwingSeverData
{
    public int opType;//1.上秋千 2.荡秋千 3.下秋千
    public SwingData data;
}

[Serializable]
public class SwingData
{
    public string mapId;
    public int itemId;
    public string playerId;
    public float angle;
    public float targetAngle;
    public int time;
}

[Serializable]
public class IceGemSendData
{
    public string playerId;
    public string mapId; //主图mapId
    public string subMapId;
    public int itemId; // 道具uid
    public int option; // 0-收集, 1-播放合成动画
    public int status; // 0-失败, 1-单个道具收集成功, 2-全部道具收集成功
}

[Serializable]
public class SeesawErrorData
{
    public int msgType;
    public int retcode;
    public string  rmsg;
}

[Serializable]
public class SnowfieldCollectInfo
{
    public string mapId; //主地图id
    public int isCompleted; // 0-未集齐 1-收集齐
    public bool isPlayedAnimation; //是否已经播放过合成动画
    public List<IceGemCollectData> collects;
}

[Serializable]
public class IceGemCollectData
{
    public string subMapId; //小地图id
    public int itemId; //
    public int status; //1-收集完成
}

[Serializable]
public class LadderSendData
{
    public int status;//0:上梯子，1:下梯子
    public string playerId;//操作玩家
    public int opType;//1:上面爬出，0:下面爬出
}
[Serializable]
public class SteeringWhellData
{
    public const int DataMultiple = 10000;
    public int status;//0:下车，1:上车
    public string playerId;//操作玩家
    public string position;//位置
}

[Serializable]
public class SensorBoxProtoData
{
    public int status;// 1-开关打开，0-开关关闭
    public int times;//该感应盒可以使用次数
    public int count;//感应盒已使用次数
}

/// <summary>
/// 踩中陷阱盒数据item
/// </summary>
[Serializable]
public class TrapBoxAffectPlayerData
{
    public string playerId;
    public int canDamage;//0-不开启，1-开启
    public float damage;//伤害值
    public float curBlood; //玩家当前血量
    public int alive; //1-存活 2-死亡
}

/// <summary>
/// 踩中陷阱盒回包数据
/// </summary>
[Serializable]
public class TrapBoxAffectData
{
    public TrapBoxAffectPlayerData[] affectPlayers;
}

[Serializable]
public class IndexData
{
    public int area;
    public int index;
    public int spawnType;
    public string position;
}


[Serializable]
public class CollectStarReq
{
    public string mapId;
    public Item[] items;
}

[Serializable]
public class HoldItemReq
{
    public string mapId;
    public Item item;
}

[Serializable]
public class CollectStarPack
{
    /// <summary>
    /// 1 为已经收集
    /// </summary>
    public int isCollect;
}

[Serializable]
public class RoomAttrCollectControl
{

    public List<string> collectedUsers;
    public List<CollectControlObj> collectControlObjs;
}

[Serializable]
public class CollectControlObj
{
    public int uid;
    public int triggerCount;
    public string mapId;

}

[Serializable]
public struct CustomProfile
{
    public string userName;
}


[Serializable]
public class EmoItemData
{
    public int random; //随机数(客户端生成)
    public int opt; //opt = 1:取消动作 0-释放动作 2-完成动作(双人) 3-双人动作进行中
    public string startPlayerId = ""; //双人动作发起者(双人动作分发起者/完成者)
    public string followPlayerId = ""; //双人动作接收者(例如:牵手中的被牵者/跟随者)
    public string extraData;
}
[Serializable]
public struct EmoItemDataExtra
{
    public int doAction; //是否播放动画  0-播动画，1-不播动画
    public int actionProtect; //动作保护时间（单位：毫秒）
    public long lastActionTime;//最后动作时间戳
    public int isFriend;//0 之前不是，1：已经是好友 
    public string token;
}
#endregion

[Serializable]
public struct EventParam
{
    public int scene;
    public int subType;
    public int roomMode;
    public string roomCode;
    public string mapId;
    public string seq;
    public string code;
    public string sessionId;
    public int retry;
}

[Serializable]
public struct LogEventParam
{
    public string eventName;
    public EventParam parameters;
}

[Serializable]
public struct RoomChatLogEventParam
{
    public string eventName;
    public RoomChatEventParam parameters;
}

[Serializable]
public struct RoomChatEventParam
{
    public int scene;
    public int subType;
    public int roomMode;
    public string roomCode;
    public string mapId;
    public string seq;
    public string code;
    public string sessionId;
    public int retry;
    
    public string requestSeq;
    public int msgType;
} 

[Serializable]
public struct FrameCountLogEventParam
{
    public string eventName;
    public FrameCountEventParam parameters;
}

[Serializable]
public struct FrameCountEventParam
{
    public int scene;
    public int subType;
    public int roomMode;
    public string roomCode;
    public string mapId;
    public string seq;
    public string code;
    public string sessionId;
    public int retry;

    public string frameCount;
} 

[Serializable]
public struct PingTimeLogEventParam
{
    public string eventName;
    public PingTimeEventParam parameters;
}

[Serializable]
public struct PingTimeEventParam
{
    public int scene;
    public int subType;
    public int roomMode;
    public string roomCode;
    public string mapId;
    public string seq;
    public string code;
    public string sessionId;
    public int retry;

    public string region;
    public string maxPing;
    public string averagePing;
}

public enum NATIVE_TYPE
{
    ROLECALL = 1,
    MAPCALL = 2,
}

public enum OptType
{
    Release, //发起动作
    Cancel, // 取消动作
    MutualFin, // 双人动作结束
    Interacting, // 双人循环动作进行中
}

public struct PVPData
{
    public int pvpMode;
    public int winType;
    public int gameTime;
    public List<List<int>> teamList;//分队信息
}

public enum PVPGameState
{
    //1-游戏准备 2-游戏开始，3-游戏结束，4-时间校准
    Wait = 0,
    Ready = 1,
    Start = 2,
    End = 3,
    Calibration = 4,
}

public class PVPSyncData
{
    //1-游戏准备 2-游戏开始，3-游戏结束，4-时间校准
    public int gameStatus;
    public string winner;
    public string afterStarted;
    public int round;
}

public class PVPSyncDataOnServer
{
    public int winType;
    public string winner;
    public int round;
}

public class SurvivalSyncData
{
    public string killer;
    public string loser;
    public int round;
}

public struct BloodPropSyncData
{
    public string playerId;
    public float restore;
    public float curBlood;
}

public struct CustomEventParam
{
    public int platform;
    public string eventName;
    public Dictionary<string, object> parameters;
}

public enum Log_Platform
{
    Firebase = 0,
    // Mixpanel = 1,
    Custom = 2,
    ThinkingData = 3,//数数
    Backend = 4,//后端
}

[Serializable]
public class FriendFollowMsg
{
    public int broadcastType;
    public string broadcastData;
}

public class FriendFollowData
{
    public string playerId;
    public string opPlayerId;
    public int opType; //1-加好友、2-关注
    public int opStatus; //1-主动添加好友、关注 2-被添加好友、关注 3-双方互为好友、
}

#region 
/// <summary>
/// VIP 区域数据
/// </summary>
[Serializable]
public class VIPZoneSendData
{
    public string mapId; // 当前地图 mapId
    public string playerId; //玩家Id
    public int option; //玩家操作 1.检测 2.进入 3.退出 4.广播区域内非法玩家 5. DcToken检测结果失效
    public Item item; // 区域对象信息
    public List<string> playerIds; // 非法玩家列表
    public int status; // 玩家操作结果 -1.失败 1.成功 2.玩家没有绑定钱包
}

[Serializable]
public class VIPTokenData
{
    public List<int> areaIds; // token 有效的 VIP 区域
    public int curAreaId; // 玩家当前所在的 VIP 区域
}

#endregion


/// <summary>
/// 回血道具网络数据包
/// </summary>
[Serializable]
public class BloodPropItemData
{
    public BloodPropAffectPlayerData[] affectPlayers;
}

/// <summary>
/// 回血道具网络数据包，包装到affectPlayers中发送
/// </summary>
[Serializable]
public class BloodPropAffectPlayerData
{
    public string PlayerId { get; set; } //回血玩家Id
    public float restore { get; set; } //回血值
    public float CurBlood { get; set; } //玩家当前血量
}

[Serializable]
public class PortalDataReq
{
    public string curMapId;
    public TargetMapData targetMap;
}

[Serializable]
public class PortalDataRsp
{
    public string code;
    public string playerId;
    public string curMapId;
    public string targetMapId;
    public float curBlood;
    public int spawnId;
}

[Serializable]
public class LatencyData
{
    public int latency;
}

[Serializable]
public class TargetMapData
{
    public string mapId;
    public bool isOpenBlood;
    public bool hasLeaderBoard;
    public bool isOpenBaggage;
    public float initBlood;
}

#region 武器道具网络消息结构

/// <summary>
/// 攻击道具网络数据包
/// </summary>
[Serializable]
public class AttackWeaponItemData
{
    public AttackWeaponAffectPlayerData[] affectPlayers;
}

/// <summary>
/// 攻击道具网络数据包，包装到affectPlayers中发送
/// </summary>
[Serializable]
public class AttackWeaponAffectPlayerData
{
    public string AttackPlayerId { get; set; } //攻击方ID
    public string PlayerId { get; set; } //受击方ID
    public float Damage { get; set; } //伤害值
    public Vec3 AttackDir { get; set; } //受击方向
    public PlayerAttackBase.AttrackDirection AnimDir { get; set; } //受击动画方向
    public int Alive { get; set; } //1-存活 2-死亡
    public float CurBlood { get; set; } //玩家当前血量
    public float CurDurability { get; set; } // 道具当前耐久值
    public int AttackPart { get; set; } // 受击部位，用于播放受击特效
}

#endregion

// 活动ID
public class ActivityID
{
    public const int Promote = 1001; // 场景内带货
    public const int FreezeItem = 1002; // 冻结道具
    public const int Fishing = 1003; // 钓鱼
    public const int LadderItem = 1004; // 梯子道具
    public const int SeesawItem = 1005; // 跷跷板道具
    public const int SlidePipe = 1006; // 滑梯道具
    public const int VipZone = 1008; // VIP 区域
    public const int Sword = 1009; // 舞刀道具
    public const int Swing = 1010; // 秋千
    public const int IceGem = 1011; // 冰晶宝石
}

#region Bud-Downtown
public class DowntownSpawnData
{
    public string playerId;
    public string mapId;
    public string roomId;
    public string position;
    public int area;
}
#endregion

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum GameResType
{
    UGCComb = -2,
    CombEmpty = -1,
    BaseMode = 0,
    BornPoint = 1001,
    Sky = 1002,
    Ground = 1003,
    DirLight = 1004,
    PointLight = 1005,
    SpotLight = 1006,
    PortalPoint = 1007,
    PortalButton = 1008,
    BGMusic = 1009,
    DText = 1010, //旧版3d文字
    PortalGate  = 1011,
    CanFly = 1012,
    Like = 1013,
    Attention = 1014,
	NudeModel = 1015,
    TrapBox = 1016,
    Switch = 1017,
    PropStar = 1018,
    EnrMusic = 1019,
    MusicBoard = 1020,
    DisplayBoard = 1021,
    TrapSpawn = 1022,
    PostProcess = 1023,
    MagneticBoard = 1024,
    Favorite = 1025,
    Sound=1026,
    ShotPhoto = 1027,
    Video = 1028,
    PVPWaitArea = 1029,
    AttackWeapon = 1030,
    SensorBox = 1031,
    FollowBox = 1032,
    WaterCube = 1033,
    LeaderBoard = 1034,
    SteeringWheel = 1040,
    UgcCloth = 1041,
    ShootWeapon = 1042,
    BloodRestore = 1043, // 回血道具
    IceCube = 1044, //冰方块
    Firework = 1045,
    Bounceplank = 1046, //bengchuang
    PGCPlant = 1047, //PGC植物
    Parachute = 1048,//降落伞
    ParachuteBag = 1049,//降落伞包
    FreezeProps = 1050, //冻结道具
    Weather = 1051,//天气
    SnowCube = 1052,//雪方块
    FireProp = 1055, //火道具
    Ladder = 1057,//梯子
    
    FishingRod = 1058, //钓鱼竿
    FishingHook = 1059, //鱼钩
    FishingModel = 1060,//鱼竿模型结构 包含钓鱼竿和鱼钩
    SlidePipe = 1061,//滑梯
    SlideItem = 1062,//滑梯子节点
    
    SeeSaw = 1067,//跷跷板
    SeeSawSeat = 1068,//跷跷板
    
    Swing = 1069,//秋千
    VIPZone = 1070,//VIP区域
    VIPArea = 1071,//VIP区域范围
    VIPDoor = 1072,//VIP区域门
    VIPDoorWrap = 1073,//VIP区域门父节点
    VIPDoorEffect = 1074,//VIP区域门特效
    VIPCheck = 1075,//VIP区域检测台
    VIPDoor2 = 1076,//VIP区域门
    VIPCheck2 = 1077,//VIP区域检测台
    VIPDoorEffect2 = 1078,//VIP区域门特效
    VIPDoorEffect3 = 1079,//VIP区域门特效
    VIPDoorEffect4 = 1080,//VIP区域门特效
    FlashLight = 1081,//手电灯
    PGCEffect = 1082, // PGC 特效
    DowntownTransfer = 1083,//Downtown传送点
    CrystalStone = 1085, //冰晶宝石

    EditMovePoint = 5000,
    NewDText = 2501, //新版3d文字
}


public class GameConsts
{
    public static string TerrainMatPath = "Material/Ground/";
    public static string SkyTexPath = "Texture/Skybox/";
    public static string BaseTexPath = "Texture/BaseTexture/";
    public static string CubemapPath = "Texture/Cubemap/";
    public static string PanelPath = "Prefabs/UI/Panel/";
    public static string SpecialModelPath = "Prefabs/Model/Special/";
    public static string BaseModelPath = "Prefabs/Model/";
    public static string PluginPath = "Prefabs/Plugin/";
    public static string OfflineCachePath = "Offline/";
    public static string NativeCachePath = "ABCache/data/";
    public static float averageFPS = 25;
    public static float TimeScale = 0.0167f;
    public static readonly int defTerrainMatId = 8;
    public static float PlayerNodeHigh = 0.95f;
    public static int MAX_PLAYER = 16;
    public static int MIN_PLAYER = 1;
    public static int DEFAULT_PLAYER = 16;
    public static int PLAYER_LAYER = 9;
    // 透明材质 Id
    public static int TRANSPARENT_MAT_ID = 1;
    //TODO: FEAT 发光材质
    // 发光材质 Id
    // public static int EMISSION_MAT_ID = 157;
    public static List<int> GameEditIds = new List<int>()
    {
        1001, //born point

        1012, //can fly button
        1051, //weather
        1002, //sky
        1003, //terrain
        1009, //bg music
        1019, //white noise
        1004, //direct light
        1023, //post process

        1029, //PVP
        1070, //VIP区域
        1085, //IceGem
        1083, //DowntownTransfer
        1030, //Attack Item
        1042, //Shoot Item
        1048, //Parachute
        1058, //FishingRod
        1059, //FishingHook
        1060, //FishingModel
        1043, //Blood Restore
        1040, //steeringWheel
        1024, //magneticBoard
        1046,//bounceplank
        1057,//梯子
        1061,//SlidePipe 滑梯
        1067, //Seesaw
        1069, //Swing
        1050,//Freeze props
        1045,//fireworks
        1017, //switch button
        1018, //prop star
        1031, //sensor box
        1016, //trap box
        1007, //portal point
        1011, //portal gate
        1020, //music board
        1026,//Sound
        1034, //LeaderBoard
        1013, //like
        1014, //attention
        1025, //favorite

        1047,//PGCPlant
        1005, //point light
        1006, //spot light
        1081, //flash light
        1021, //3D Display Board 
        1027, //shot photo
        1028, //3D Video
        //1010, //3D text
        2501,//New 3D text
		1041, //Ugc Cloth
        1033, //water cube
        1044, //ice cube
        1052,//Snow Cube
        1082, // PGC 特效
        1055, //fire prop
        
        13,
        14,
        12,
        15,
        16,
        29,
        17,
        6,
        18,
        19,
        30,
        20,
        21,
        27,
        22,
        23,
        24,
        25,
        26,
        28
    };

    public static SavingData.MapBlock DefaultMapBlock = new SavingData.MapBlock()
    {
        spawnLodDistance = 2,
        runtimeCullDistance = 0.3f,
        runtimeLodDistance = 2
    };

    public static List<int> ResEditIds = new List<int>()
    {
        //1010, //3D text
        2501,//New 3D text
        13,
        14,
        12,
        15,
        16,
        29,
        17,
        6,
        18,
        19,
        30,
        20,
        21,
        27,
        22,
        23,
        24,
        25,
        26,
        28
    };

    public static GameResType GetResType(int id)
    {
        if (id < 1000 && id >= 0)
        {
            return GameResType.BaseMode;
        }
        return (GameResType) id;
    }

    //PS:该setting只能添加，不能调整顺序
    public static LightSetting[] settings = new LightSetting[]
    {
        //默认
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(1f, 1f, 1f),
            equator = new Color(0.95f,0.95f,0.95f),
            ground = new Color(0.5f, 0.5f, 0.5f),
            dirctional = new Color(1f, 1f, 1f),
            intensity = 0.5f,
            anglex = 60,
            angley = 333,
            reflectionIntensity = 0.5f,
        },
        //夜晚
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(0.09f, 0.22f, 0.48f),
            equator = new Color(0.04f,0.04f,0.04f),
            ground = new Color(0.17f, 0.26f, 0.41f),
            dirctional = new Color(0.15f, 0.21f, 0.47f),
            intensity = 1,
            anglex = 50,
            angley = 330,
            reflectionIntensity = 0.5f,
        },
        //清晨
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(0.72f, 0.80f, 0.92f),
            equator = new Color(0.82f, 0.80f, 0.77f),
            ground = new Color(0.04f, 0.37f, 0.64f),
            dirctional =  new Color(1f,0.84f,0.47f),
            intensity = 0.7f,
            anglex = 25,
            angley = 20,
            reflectionIntensity = 0.5f,
        },
        //黄昏
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(1f,0.67f,0.53f),
            equator = new Color(1f, 0.79f, 0.66f),
            ground = new Color(0.75f, 0.15f ,0.02f),
            dirctional = new Color(0.86f,0.68f,0.37f),
            intensity = 0.7f,
            anglex = 20,
            angley = 200,
            reflectionIntensity = 0.5f,
        },
        //粉色
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(1f,0.64f,0.73f),
            equator = new Color(0.85f,0.57f,0.57f),
            ground = new Color(0.62f,0f,0.30f),
            dirctional = new Color(1f,0.6f,0.55f),
            intensity = 1f,
            anglex = 45,
            angley = 35,
            reflectionIntensity = 0.5f,
        },
        //moon
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(0.43f, 0.53f, 0.35f),
            equator = new Color(0.24f, 0.24f, 0.24f),
            ground = new Color(0.30f, 0.05f, 0.35f),
            dirctional = new Color(0.62f,0.43f,0.59f),
            intensity = 0.7f,
            anglex = 60,
            angley = 333,
            reflectionIntensity = 0.5f,
        },
        //orange
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(0.92f, 0.42f, 0.23f),
            equator = new Color(0.80f, 0.35f, 0.15f),
            ground = new Color(0.50f, 0.04f, 0.11f),
            dirctional = new Color(0.62f,0.43f,0.59f),
            intensity = 0.72f,
            anglex = 40,
            angley = 32,
            reflectionIntensity = 0.5f,
        },
        //purple
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(1.0f, 0.35f, 0.95f),
            equator = new Color(0.74f, 0.39f, 0.46f),
            ground = new Color(0.73f, 0.02f, 0.71f),
            dirctional = new Color(1.0f,0.7f,0.96f),
            intensity = 0.75f,
            anglex = 40,
            angley = 32,
            reflectionIntensity = 0.5f,
        },
        //绿色星空（green）
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(0.37f, 0.69f, 0.19f),
            equator = new Color(0.66f, 0.2f, 0.13f),
            ground = new Color(0.73f, 0.73f, 0.73f),
            dirctional = new Color(0.62f,0.43f,0.59f),
            intensity = 1,
            anglex = 70,
            angley = 187,
            reflectionIntensity = 0.5f,
        },
        //深紫星空(darkpurple)
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(0.57f, 0.22f, 0.95f),
            equator = new Color(0.65f, 0.33f, 0.76f),
            ground = new Color(0.28f, 0.06f, 0.53f),
            dirctional = new Color(0.8f,0.77f,0.25f),
            intensity = 0.45f,
            anglex = 148,
            angley = 40,
            reflectionIntensity = 0.5f,
        },
        //夏日沙滩（water）
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(1f, 0.94f, 0.82f),
            equator = new Color(0.85f, 0.5f, 0.33f),
            ground = new Color(0.73f, 0.38f, 0.35f),
            dirctional = new Color(0.94f,0.93f,0.68f),
            intensity = 0.8f,
            anglex = 30,
            angley = 229,
            reflectionIntensity = 0.65f,
        },
        //阴天（yingtian）
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(0.6f, 0.6f, 0.6f),
            equator = new Color(0.27f, 0.26f, 0.26f),
            ground = new Color(0.31f, 0.31f, 0.31f),
            dirctional = new Color(1f,1f,1f),
            intensity = 0.1f,
            anglex = 50,
            angley = 300,
            reflectionIntensity = 0.5f,
        },
        //My Space(default)
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(0.82f, 0.82f, 0.82f),
            equator = new Color(0.94f, 0.94f, 0.94f),
            ground = new Color(0.58f, 0.58f, 0.58f),
            dirctional = new Color(1f, 1f, 1f),
            intensity = 0.7f,
            anglex = 58,
            angley = 321,
            reflectionIntensity = 0.5f,
        },
        //雾霾天（wumaitian）
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(0.73f, 0.58f, 0.45f),
            equator = new Color(0.54f, 0.34f, 0.48f),
            ground = new Color(0.15f, 0.09f, 0.06f),
            dirctional = new Color(1f, 1f, 1f),
            intensity = 0.03f,
            anglex = 50,
            angley = 150,
            reflectionIntensity = 0.5f,
        },
        //极光(jiguang)
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(0.25f, 0.39f, 0.4f),
            equator = new Color(0.14f, 0.2f, 0.09f),
            ground = new Color(0.36f, 0.36f, 0.36f),
            dirctional = new Color(0.86f, 0.86f, 0.86f),
            intensity = 1f,
            anglex = 70,
            angley = 187,
            reflectionIntensity = 0.5f,
        },
        //夕阳红(dusk)
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(1f, 0.72f, 0.37f),
            equator = new Color(0.62f, 0.25f, 0f),
            ground = new Color(0.92f, 0.37f, 0.41f),
            dirctional = new Color(0.9f, 0.55f, 0.16f),
            intensity = 0.85f,
            anglex = 12,
            angley = 184,
            reflectionIntensity = 0.5f,
        },
        //雪地（大地图）
        new LightSetting
        {
            gradientType = (int)UnityEngine.Rendering.AmbientMode.Trilight,
            sky = new Color(0.73f, 0.84f, 0.89f),
            equator = new Color(0.23f, 0.44f, 0.7f),
            ground = new Color(0.2f, 0.47f, 0.67f),
            dirctional = new Color(0.78f,0.95f,1f),
            intensity = 0.36f,
            anglex = 155,
            angley = 350,
            reflectionIntensity = 0.65f,
        },
    };

    public static Ease[] upEases = { Ease.InOutCubic, Ease.OutCubic, Ease.OutCubic };
    public static Ease[] downEases = { Ease.InOutCubic, Ease.InCubic, Ease.InCubic };
    public static float[] updownDurations = { 1.2f, 0.2f, 0.1f };
    public static float[] rotSpeeds = { 15, 30, 45 };
    public static float[] moveSpeed = { 0, 1, 4, 8 };
    public static float[] rotDelTime = { 0, 0.4f, 0.6f, 1.2f };


    public static int[] ambineMusicIds = {0,2007,2008,2001, 2002, 2003, 2004,2005,2006};

    //环境音配置表<ambientId,eventName>
    public static Dictionary<int, string> ambientEventDict = new Dictionary<int, string>{
        {2001,"Play_white_noise_day"},//清晨时分
        {2002,"Play_white_noise_night"},//夜间蝉鸣
        {2003,"Play_White_noise_city"},
        {2004,"Play_White_noise_beach"},
        {2005,"Play_White_noise_room"},
        {2006,"Play_White_noise_seaisland"},
        {2007,"Play_White_noise_rain"},
        {2008,"Play_White_noise_snow"},
    };
}


public enum TerrainSizeConfigs
{
    defaultTerrainSize = 1, // 不扩大 默认100*100
    fiveTimesTerrainSize = 5, // 5倍 500*500
}

public enum HTTP_METHOD
{
    GET = 0,
    POST = 1
}

//请求状态
public enum HttpReqState
{
    FirstEntry,
    Refreshing,
    Failed,
    Success,
}

//图片加载状态
public enum ImgLoadState
{
    Loading,
    Complete,
    Failed
}

public enum ResBundleType
{ 
    Icon = 1,
    AbFile = 2,
    Banner = 3,
}

//跳转端上详情页类型
public enum DetailPageType
{
    Prop = 1,
    Nft = 2,
}

public enum NftType
{
    Dc = 0,
    Airdrop = 1,
}

public enum LabelType
{
    NONE,
    AIR, //airdrop
    DC, //ugc-dc
    PGC, //官方nft
    RW, //rewards
    UGCAIR, //ugc-airdrop
}

public enum ActivityStatus
{
    NotStart = 0,
    Active = 1,
    Finished = 2
}

// 开关控制的类型
public enum SwitchControlType
{
    VISIBLE_CONTROL = 0, //显隐控制
    MOVEMENT_CONTROL = 1, // 移动控制
    SOUNDPLAY_CONTROL = 2, // 声音播放控制
    ANIMATION_CONTROL = 3, // 旋转移动控制
    FIREWORK_CONTROL = 4,//烟花道具控制
}

//道具控制的类型
public enum PropControlType
{
    VISIBLE_CONTROL, //显隐控制
    MOVEMENT_CONTROL, // 移动控制
    SOUNDPLAY_CONTROL, // 声音播放控制
    ANIMATION_CONTROL, // 旋转移动控制
    FIREWORK_CONTROL,//烟花道具控制
}

/** 
    * 双人交互动作类型枚举
    * 和 EmoConfigData.json 文件中的 emoId 保持一致
    */
public enum EmoName
{
    None,
    //EMO_HIGH_FIVE = 101, // 击掌  (暂不用)
    EMO_JOIN_HAND = 102, // 牵手
    EMO_LOOK_MIRROR = 10001, // 照镜子
    EMO_CHANGE_CLOTH = 10002, // 换装
    EMO_SELFIE_MODE = 10003, // 自拍动画
    EMO_SEESAW_ANIM = 10004, // 跷跷板动画
    EMO_SEESAW_PUSH = 10005, // 跷跷板下压动画

    EMO_ADD_FRIEND = 125, // 发起请求加好友 
    EMO_PROMOTE = 140, // 场景内带货 
    EMO_SWORD = 10006, //舞刀
    EMO_CRYSTAL_GET = 10011, //收集冰晶宝石
}
public enum HandleMode
{
    Move,
    Rotate,
    Scale
}

//UGC衣服Undo/Redo数据区分
public enum UGCElementType
{
    Trans,
    Color,
}

public enum MaterialSelectType
{
    Material,
    Color,
    Tile,
    Velocity
}
public enum PGCSelectType
{
    Type,
    Color,
}
public enum TextSelectType
{
    Text,
    Color
}
public enum LockHideType
{
    Lock,
    Hide,
    Show
}

public enum SnowCubeType
{
    Color,
    Tile
}

// 声音控制类型
public enum SoundControl
{
    NOT_SUPPORT = 0, // 支持声音控制
    SUPPORT_CTRL_MUSIC = 1, // 不支持声音控制
}
public enum FireworkControl
{
    NOT_SUPPORT = 0, // 支持烟花控制
    SUPPORT_CTRL_Firework = 1, // 不支持控制
}


// 旋转移动控制类型
public enum AnimControl
{
    NOT_SUPPORT = 0, // 支持控制
    SUPPORT_CTRL_ANIM = 1, // 不支持控制
}
//逻辑道具操作错误码
public enum PropOptErrorCode
{
    Successed=20000,//操作成功
    Clash=20002,//操作冲突
    Exception=20005,//操作异常
    SeeSawError = 20007, // 跷跷板异常
}

//http请求操作错误码
public enum HttpOptErrorCode
{
    None = 0, //操作成功
    DC_NOT_OWNED = 10116, //包含未拥有DC
    ITEM_NOT_OWNED = 10117, //包含未拥有部件(Rewards)
}

//伤害来源
public enum DamageSource
{
    Player = 0,//玩家伤害
    TrapBox = 1,//陷阱盒伤害
    Fire = 2,//火焰伤害
}

public enum TrapBoxTrans
{
    MapSpawn = 0,//地图出生点
    CustomSpawn = 1,//自定义出生点
    NoTrans = 2//原地
}

//材质类型
public enum MaterialGroupTypes
{
    Wooden = 0,//木头
    Stone = 1,//石头
    Grass = 2,//草地
    Metal = 3,//金属
    Pattern = 4,//花纹
    Others = 5,//不好分类
}

/// <summary>
/// 编辑器道具列表分类
/// </summary>/
public enum PrimitiveItemType
{
    NONE = 0,
    Character = 1,
    General = 2,
    GamePlay = 3,
    Scene = 4
}

//天气系统类型
public enum EWeatherType
{
    None,
    Rain,
    Snow
}

//天气系统等级
public enum EWeatherEffectQuality
{
    LowQuality,
    HighQuality,
}

public enum EWeatherEffectId
{
    None,
    Rain,
    Snow
}

public enum SlideResType
{
    Forward = 1062,
    Left = 1063,
    Right = 1064,
    Up = 1065,
    Down = 1066
    
}

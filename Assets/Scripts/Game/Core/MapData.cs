using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class BehaviorKV
{
    public int k;
    public string v;
}

public enum MapSaveType
{
    Map = 0,
    Prop = 1,
    UGCRes = 2,
    Space = 3,
    UGCMaterial = 4,
}


public enum BehaviorKey
{
    //Mat
    ColorMaterial = 1,
    PointLight = 2,
    SpotLight = 3,
    PortalPoint = 4,
    DText = 5,
    PortalGate = 6,
    RPAnim = 7,
    Movement = 8,
    ShowHide = 9,
    SwitchButton = 10,
    CollectControl = 11,
    MusicBoard = 12,
    UGCProp = 13,
    DisplayBoard = 14,
    TrapBox = 15,
    TrapSpawn = 16,
    Sound = 17,
    SwitchControl = 18, // 开关控制
    ShotPhoto = 19,
    Pickablity = 20,
    VideoNode = 21,
    AttackWeapon = 22, //攻击道具
    SensorBox = 23,
    SensorControl = 24,

    // ！！！废弃k=25！！！
    //FollowBox = 25, //在1.24版本中，跟随功能Componnt数据异常，该ID禁止使用
    // ！！！废弃k=25！！！
    FollowBox = 26,
    LeaderBoard = 27,
    UGCClothItem = 27,
    ShootWeapon = 30, //射击道具
    BloodRestoreProp = 31, // 回血道具
    NewDTextData = 32, // 新版3d文字 
    WaterData = 33,
    IceCube = 34,
    DC = 35, //dcinfo
    Firework = 36,
    Edibility = 37,
    Bounceplank = 38,//蹦床
    PGCPlant = 39,//PGC植物
    Parachute = 40,//降落伞
    ParachuteBag = 41,//降落伞包
    FreezeProps=42,//冻结道具
    FireProp = 43,//火焰
    SnowCube = 44, //雪方块
    Ladder = 45,//梯子
    FishingRod = 46, //鱼竿
    FishingHook = 47, //鱼钩
    Catchability = 48,  //可捕捉
    FishingModel = 49, //鱼竿整体
    PgcScene = 50,
    SlidePipe = 51,//滑梯父节点
    SlideItem = 52,//滑梯item
    SeeSaw = 53,//跷跷板
    SeeSawSeat = 54,//跷跷板
    VIPZone = 55,//VIP区域
    VIPDoor = 56,//VIP门
    VIPCheck = 57,//VIP检测台
    VIPDoorEffect = 58,//VIP门特效
    FlashLight = 59,//手电灯
    Swing = 60,//秋千
    PGCEffect = 61,//PGC特效
    DowntownTransfer = 62,//Downtown传送点
    DowntownNode = 63,//DowntownPGC节点
    CrystalStone = 64,//冰晶宝石
}


[System.Serializable]
public class ColorMatData
{
    public string cols;
    public int mat;
    public string tile;
    public string umat;//新增存储ugc mapid
}


[System.Serializable]
public class UGCMatSaveData
{
    public string uurl;//ugc材质 图片url
}
[System.Serializable]
public class NodeData
{
    public int uid;
    public int id;// mod id
    public string rid = "";
    public int type;
    public string p;
    public string r;
    public string s;
    public List<BehaviorKV> attr = new List<BehaviorKV>();
    public List<NodeData> prims = new List<NodeData>();

    private string hash;
    public string ToHash()
    {
        if (string.IsNullOrEmpty(hash))
        {
            hash = HashUtils.HashString(uid, id, rid, type, p, r, s);
        }
        return hash;
    }
}


public class MatNodeData {
    public NodeData data;
    public int version;
    public Dictionary<string, UGCMatSaveData> ugcMatDic = new Dictionary<string, UGCMatSaveData>();

}

public struct SkyData
{
    public int skyId; //天空盒id
    public int type; // 1 - gradient, 3 - color 
    public string scol;
    public string ecol;
    public string gcol;
    
    public int skyboxType; //天空盒类型 0-普通天空盒，1-昼夜天空盒
    public int dayLength; //昼夜天空盒一天的时长(min)
    public int daytimeHour; //昼夜天空盒初始时间(hour)
    public int daytimeMin; //昼夜天空盒初始时间(min)
}

[Serializable]
public struct DirLightData
{
    public float inte; //intensity
    public float anx; //x asix angle
    public float any; //y asix angle
    public string lico; //light color
}

public class PostProcessData
{
    public int bActive;
    public float bInte;
    //public int amActive;
}

public class PVPWaitAreaData
{
    public string p;
    public string r;
    public string s;
    public int gameMode;
    public string gameCondition;
    public List<List<int>> teamList;
}

public struct RaceGameData
{
    public int taskType;//区分获胜条件，目前暂不使用
    public int taskArg;
    public int taskArga;//开关是否勾选(仅判断是否设置开关获胜使用)
    public int pvpTime;
}

[Serializable]
public class SpotLightData
{
    public float inte;
    public float rng;
    public float spoa;
    public string lico;
}

public class GameTerrainData
{
    public int matId;
    public string uurl;
    public string umat;
    public string cols;
    public int terrainSize;
}

public struct BGMusicData
{
    public string bgName;
    public string bgUrl;
    public int musicType;
    public int musicId;
    public int eId;
}


[System.Serializable]
public class MapData
{
    public int version;
    public SpawnData pspawn; //play or guest mode camera pos,rotation
    public SpawnData espawn;//edit mode camera pos,rotation
    public SpawnData[] spawns;//spawn array instead of "pspawn"
    public int maxPlayers;
    public string nudeModPos;//nude model pos
    public SkyData sky;//skybox type index
    public DirLightData dir; //direct light
    public GameTerrainData ter;//terrain data
    public BGMusicData bgmusic;
    public List<NodeData> pref = new List<NodeData>();
    public int canFly; // 0 ==> can fly  1=>can no fly
    public PostProcessData postprocess;
    public Dictionary<string, List<NodeData>> resList;
    public Dictionary<string, UGCMatSaveData> uMatList; //存储ugc材质信息列表（mapid，含url结构体）
    public int editTime;//map edit Time
    public PVPWaitAreaData pvpData;
    public int setHP; // 0=> no HP    1=> has HP
    public int setLeaderBoard; // 0=> no LeaderBoard  1=> has LeaderBoard
    public int customHP;
    public int setBaggage;
    public WeatherSaveParams weather;
    public List<int> dmgSrcs;//伤害来源，如开启了setHP至少存在一个
    public int defaultSpawnId;//默认出生点Id
}

[System.Serializable]
public class SpawnData
{
    public string p;
    public string r;
    public int id;
    public int hp;
}

public struct MusicIDData
{
    public int lID; //left board audioID   (0~36)
    public int mID; //middle board audioID   (0~36)
    public int rID; //right board audioID   (0~36)
}

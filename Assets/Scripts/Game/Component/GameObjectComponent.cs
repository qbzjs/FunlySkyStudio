using System;
using UnityEngine;

public enum NodeModelType
{
    Special = -1,//Terrain,Skybox,Direct light
    BaseModel = 0,
    BornPoint = 1,
    PointLight = 2,
    SpotLight = 3,
    PortalPoint = 4,
    PortalButton = 5,
    DText = 6,
    PortalGate = 7,
    Like = 8,
    Attention = 9,  
    NudeModel = 10,
    TrapBox = 11,
    Switch = 12,
    MagneticBoard = 13,
    TrapSpawn = 14,
    Favorite = 15,
    Sound=16,
    ShotPhoto = 17,
    SteeringWheel = 18,//方向盘
    MusicBoard = 20,

    Video = 21, 
    PVPWaitArea =22,
    SensorBox = 23,
    WaterCube = 24,
    FollowBox = 25,
    LeaderBoard = 26,
    NewDText = 27, //新版3d文字
    CommonCombine = 100,
    Movement = 102,
    PropStar = 103,
    DisplayBoard = 104,
    UgcCloth = 105,
    IceCube = 106,
    Bounceplank = 107,
    PGCPlant = 108,
    Parachute = 109,//降落伞
    ParachuteBag = 110,//降落伞包
    FreezeProps = 111,
    FireProp = 112,
    SnowCube = 113,//雪方块
    Ladder = 114,//梯子
    FishingRod = 115,
    FishingHook = 116,
    FishingModel = 117,
    SlidePipe = 118,//滑梯
    SlideItem = 119,//滑梯子节点
    SeeSaw = 120,//跷跷板
    SeeSawSeat = 121,//跷跷板座位
    VIPZone = 122,//VIP区域
    VIPArea = 123,//VIP区域范围
    VIPDoor = 124,//VIP区域门
    VIPDoorWrap = 125,//VIP区域门父节点
    VIPDoorEffect = 126,//VIP区域门特效
    VIPCheck = 127,//VIP区域检测台
    FlashLight = 128,//手电光
    Swing = 129,//秋千
    PGCEffect = 130, // PGC 特效
    DowntownTransfer = 131,//Dotowntown
    CrystalStone = 133,//冰晶宝石
  
    //武器道具 200顺延
    AttackWeapon = 200, //攻击道具
    ShootWeapon = 201,//射击道具
    BloodRestore = 202, // 回血道具
    Firework = 203,//烟花道具

    Downtown = 400,//大世界资源

    //PS:PGC序号500，如无特殊需求,不要往500后添加
    PGC = 500,
}

public enum NodeHandleType
{
    Base = 0,
    Born = 1,//for bornPoint
    PointLight = 2, //for point light
    Combine = 3,//Combine Type
    SpecialCombine = 4,//Contain Special Model, For Example:Light,Portal
    SpotLight = 5,
    Special = 6, //Normal Scale
    RotAxisY = 7,
    NudeMod = 8,
    TrapSpawn = 9,
    MagneticBoard = 10,//磁力版
    PGC = 11,
    SteeringWheel = 12,//方向盘
    Video = 13,// 3d视频
    PVP = 14,
    WaterCube = 15,// 水方块
	AttackWeapon = 16,// 攻击道具
    ShootWeapon = 17,
    BloodRestore = 18, // 回血道具
    IceCube = 19,
    Firework = 20,//烟花
    Bounceplank = 21, // 蹦床
    Parachute = 22,//降落伞
    ParachuteBag = 23,//降落伞包
    FreezeProps = 24, // 冰冻道具
    SnowCube = 25,//雪方块
    Ladder = 26,//梯子
    FishingRod = 27,
    FishingHook = 28,
    FishingModel = 29,
    SlidePipe = 30,//滑梯
    SeeSaw = 31,//跷跷板
    SeeSawSeat = 32,//跷跷板座位
    SeesawCombine = 33,//UGC组合，这个名字最好换一下
    VIPZone = 34,//VIP区域
    Swing = 35,//秋千
    DowntownTransfer = 36,//Downtown传送点
    CrystalStone = 38,//冰晶宝石
}

public enum NodeUploadType
{
    Unable = 0,
    Able = 1,
}

public enum ResType
{
    Special = -2,//Terrain、BornPoint
    Single = -1,//Light,Base
    CommonCombine = 0,
    UGC = 1,
    PGC = 2,
    Downtown = 3
}

[Serializable]
public class GameObjectComponent:IComponent
{
    public int uid;
    public int modId;
    public string resId;
    public ResType type; //-2 special -1-basemodel 0-common pack //1-UGC
    public NodeHandleType handleType;
    public NodeModelType modelType;
    public GameObject bindGo;

    public IComponent Clone()
    {
        GameObjectComponent component = new GameObjectComponent();
        component.uid = UidManager.Inst.GetUid();
        component.modId = modId;
        component.resId = resId;
        component.type = type;
        component.handleType = handleType;
        component.modelType = modelType;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}

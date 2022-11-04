using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Object = System.Object;

namespace SavingData
{

    [Serializable]
    public struct HTTPRequest
    {
        public string path;
        public int requestType;
        public string paramStr;
        public string identifier;
        public string headerStr;
    }

    [Serializable]
    public struct HttpResponse
    {
        public string identifier;
        public int isSuccess; //1 success
        public string data;
    }

    [Serializable]
    public struct GetMapInfo
    {
        public MapInfo mapInfo;
        public int isInWhiteList;
        public Perm perm;
    }

    [Serializable]
    public struct GetDowntownInfo
    {
        public DowntownInfo downtownInfo;
    }
    [Serializable]
    public class Perm
    {
        public Groups[] groups;
        public bool granted;
    }
    [Serializable]
    public class Groups
    {
        public string name;
    }



    [Serializable]
    public struct GetBatchMapInfo
    {
        public MapInfo[] mapInfos;
    }

    [Serializable]
    public class HttpResponseRaw
    {
        public int result;
        public string rmsg;
        public object data;
    }

    public struct ClientResponse
    {
        public int isSuccess;
        public string data;
        public string funcName;
    }

    [Serializable]
    public class BaseGameJson
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public UnityConfigInfo configInfo = new UnityConfigInfo();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public UnityBaseInfo baseInfo = new UnityBaseInfo();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public UserInfo unityUserInfo = new UserInfo();

        public string u3dSourcesConfigVersion;
    }

    [Serializable]
    public class GetGameJson: BaseGameJson
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public UgcUntiyMapDataInfo ugcMapDataInfo = new UgcUntiyMapDataInfo();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public OnLineDataInfo onLineDataInfo = new OnLineDataInfo();
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public UnityMapInfo unityMapInfo = new UnityMapInfo();
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public UGCClothInfo unityUGCClothInfo = new UGCClothInfo();
    }

    [Serializable]
    public class GetDowntownJson : BaseGameJson
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DowntownDataInfo downtownDataInfo = new DowntownDataInfo();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public OnLineDataInfo onLineDataInfo = new OnLineDataInfo();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public UnityDowntownInfo unityDowntownInfo = new UnityDowntownInfo();
    }

    [Serializable]
    public class ConfigVersion
    {
        public int ID;
        public string configGroupName;
        public string ver;
        public string hash;
        
        public bool IsYoungerThan(ConfigVersion v1)
        {
            VersionNumber.CheckFormat(ver, out var vv2);
            VersionNumber.CheckFormat(v1.ver, out var vv1);

            return IsYoungerThan(vv2, vv1);
        }
    
        private bool IsYoungerThan(ushort[] v2, ushort[] v1)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (v2[i] < v1[i]) return true;
                if (v2[i] > v1[i]) return false;
            }
            return false;
        }
        
    }
    
    [Serializable]
    public class UgcUntiyMapDataInfo
    {
        public string mapId = "";
        public string mapName = "";
        public string draftPath = "";
    }

    [Serializable]
    public class DowntownDataInfo
    {
        public string downtownId = "";
        public string downtownName = "";
        public string draftPath = "";
    }

    [Serializable]
    public class HttpDowntownDataInfo
    {
        public string downtownId = "";
    }

    [Serializable]
    public class HttpMapDataInfo
    {
        public string mapId = "";
        public string mapName = "";
    }
    
    [Serializable]
    public class HttpBatchMapDataInfo
    {
        public string[] mapIds;
        public int dataType; //0(地图),1(素材),2(衣服),3(space)
    }

    [Serializable]
    public class ExitEditParams
    {
        public string mapId = "";
        public string draftPath = ""; //本地草稿路径(指向本地文件夹)
    }

    /// <summary>
    /// 通知端上接口类, 仅仅作为类型识别使用
    /// </summary>
    public interface IMobileNotify
    {
        
    }


    [Serializable]
    public class LoadProgressParams : IMobileNotify
    {
        public int total; //ab文件总数
        public int now; //当前已下载文件数
    }

    [Serializable]
    public class EditSaveInfo
    {
        public int sCover; //0 不保存封面 1 保存封面
        public int sMap; //0 不保存 1 保存
        public int sProp; //0 不保存 1 保存
        public int sCloth; //0 不保存 1 保存
        public int sMaterial; //0 不保存 1 保存
    }

    [Serializable]
    public class LocalSaveConfig
    {
        public int draftId; // 草稿id
        public int coverType; // 0 jpg 1 png
        public int saveCover; // 0 默认不保存封面 1 保存封面
        public int saveMap; // 0 默认不保存 1 保存
        public int saveProp; // 0 默认不保存 1 保存
        public int saveCloth; // 0 默认不保存 1 保存(为1时需上传.zip和.json两个文件)
        public int saveMaterial; // 0 默认不保存 1 保存(为1时需上传.png和.json两个文件)
    }

    [Serializable]
    public class UnitySaveData
    {
        public int openBlood = 0; //0:不开启血量  1:开启血量
        public int hasLeaderboard = 0;//0:没有开启排行榜  1:开启了排行榜
        public int customBlood = 100; // 自定义血量，默认 100
        public int openBaggage = 0; //0:没有开启背包  1:开启了背包
        public List<int> dmgSrcs;//伤害来源
        public List<int> crystalList;//冰晶宝石列表
    }

    [Serializable]
    public class NativeResInfo
    {
        public int type;  //1 : UGC Store 2: ugcCloth
        public string mapId = "";
    }
    [Serializable]
    public class DCResInfo
    {
        public string itemId = "";  
        public string budActId = "";
        public int classifyType = 0;  
        public int pgcId = 0;
    }
    [Serializable]
    public class UnityConfigInfo
    {
        public string appVersion = "";
        public string dataDir = "";
        public int templateId = 0;
        public string templateUrl = "";
        public string seq = "";
        public int isQueen = 0; //  是否为可上传pgc素材的账号（如官妈
        public string callUnityTimeStamp = "";
        public FeatSwitch featSwitch;
        public MapBlock mapBlock;
        public int dataSubType;//判断模版为衣服或面部彩绘或其他
        public bool isWhiteUser;
    }

    [Serializable]
    public class MapBlock
    {
        public float spawnLodDistance;
        public float runtimeCullDistance;
        public float runtimeLodDistance;
    }

    [Serializable]
    public class FeatSwitch
    {
        public RealTimeVoiceSwitch publicServer;
        public RealTimeVoiceSwitch privateServer;
        public bool enableLogUpload = false;
    }
    [Serializable]
    public class RealTimeVoiceSwitch
    {
        public bool closeVoice;
        public bool closeSpatialSound;
    }
    [Serializable]
    public class UnityBaseInfo
    {
        public string uid = "";
        public string baseUrl = "";
        public string environment = "";
        public string device = "";
        public string platform = "";
        public string generation = "";
        public string token = "";
        public string locale = "";
        public string lang = "";
        public string walletAddress = "";
        public string timezone = "";
    }

    [Serializable]
    public class UnityMapInfo
    {
        public MapInfo mapInfo;
        public int isInWhiteList;
    }

    [Serializable]
    public class UnityDowntownInfo
    {
        public DowntownInfo downtownInfo;
        public int isInWhiteList;
    }

    [Serializable]
    public class UGCClothInfo
    {
        public string clothesJson;
        public string clothesUrl;
        public string mapId;
        public int templateId;
        public PGCInfo dcPgcInfo;
        public PGCInfo[] dcPgcInfos;
        public int dataSubType;
    }

    [Serializable]
    public class WearClothInfo
    {
        public string clothMapId;
        public int templateId;
        public string clothesJson;
        public string clothesUrl;
        public int sceneType;
        public int subType;
        public int dataSubType;
    }

    [Serializable]
    public class CreateMapInfo
    {
        public string mapId = "";
        public int isInWhiteList = 0;

    }

    [Serializable]
    public class ReqNftData
    {
        public List<PGCInfo> itemDatas;
    }

    [Serializable]
    public class RespNftData
    {
        public List<DCResInfo> itemDatas;
    }

    [Serializable]
    public class DCUGCItemInfo
    {
        public int classifyType; //角色部件分类，同ClassifyType枚举值
        public string ugcId; //ugc资源的id
        public int hasCount; //拥有数量：0表示未拥有
    }

    [Serializable]
    public class DCPGCItemInfo
    {
        public int classifyType; //角色部件分类，同ClassifyType枚举值
        public int pgcId; //配置表内pgc资源id，100000-200000
        public int hasCount; //拥有数量：0表示未拥有
        public int openStatus; //dc部件活动是否开启: 0 关闭 1 打开
    }

    [Serializable]
    public class PGCInfo
    {
        public int classifyType;
        public int pgcId;
    }

    [Serializable]
    public class PGCClothesInfo
    {
        public string mapId = "";
        public int dataType = 0;//0:map 1:prop 2:clot 3:space
        public MapStatus mapStatus = new MapStatus();
        public PGCInfo pgcInfo; //pgc资产
    }

    public class DCUGCClothesInfo
    {
        public string mapId = "";
        public int dataType = 0;//0:map 1:prop 2:clot 3:space
        public string mapCover = "";
        public string mapName = "";
        public string mapJson = "";
        public string clothesJson = "";
        public string clothesUrl = "";
        public string mapDesc = "";
        public int templateId = 1;
        public int isDC = 0;
        public int isPGC = 0;
        public DCInfo dcInfo = new DCInfo();
        public MapStatus mapStatus = new MapStatus();
        public DCPGCItemInfo dcPgcInfo; //pgc资产
        public int dataSubType;
    }

    [Serializable]
    public class MapInfo
    {
        public string mapId = "";
        public int dataType = 0;//0:map 1:prop 2:clot 3:space 4:material
        public string mapCover = "";
        public string mapName = "";
        public string mapJson = "";
        public string jsonUrl = "";
        public string dataUrl = "";
        public string clothesJson = "";
        public string clothesUrl = "";
        public string propsJson = "";
        public string dcJson = "";
        public string mapDesc = "";
        public string[] propList;
        public DcSaveInfo[] dcList;
        public string audioUrl = "";
        public string[] audioList;
        public MapCreator mapCreator = new MapCreator();
        public MapStatus mapStatus = new MapStatus();
        public Relation relation;
        public InteractStatus interactStatus;
        public OfflineRenderListObj[] renderList;
        
        [JsonConverter(typeof(MapRenderInfoConverter))]
        public List<MapRenderInfo> renderJson;
        public string[] highestResIds;
        public int templateId = 1;
        public int maxPlayer = 8;
        public int draftId = 0;
        public int editTime = 1; //创作时长 /s -- 从1开始计时
        public string pvpData;
        public string unityData; //u3d地图信息(只用于u3d数据保存)
        public int isDC;
        public int isPGC;
        public string bannerName;
        public DCInfo dcInfo;
        public int editorVersion;
        public string[] imgs;
        public DCPGCItemInfo dcPgcInfo;
        public int dataSubType;
        
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime lastModifiedTime;
        public MapInfo Clone()
        {
            MapInfo newMapInfo = new MapInfo();
            newMapInfo.mapId = mapId;
            newMapInfo.dataType = dataType;
            newMapInfo.mapCover = mapCover;
            newMapInfo.mapName = mapName;
            newMapInfo.mapJson = mapJson;
            newMapInfo.clothesJson = clothesJson;
            newMapInfo.clothesUrl = clothesUrl;
            newMapInfo.jsonUrl = jsonUrl;
            newMapInfo.dataUrl = dataUrl;
            newMapInfo.propsJson = propsJson;
            newMapInfo.mapDesc = mapDesc;
            newMapInfo.propList = propList;
            newMapInfo.dcList = dcList;
            newMapInfo.audioList = audioList;
            newMapInfo.mapCreator = mapCreator;
            newMapInfo.mapStatus = mapStatus;
            newMapInfo.relation = relation;
            newMapInfo.interactStatus = interactStatus;
            newMapInfo.renderList = renderList;
            newMapInfo.highestResIds = highestResIds;
            newMapInfo.templateId = templateId;
            newMapInfo.maxPlayer = maxPlayer;
            newMapInfo.draftId = draftId;
            newMapInfo.editTime = editTime;
            newMapInfo.pvpData = pvpData;
            newMapInfo.unityData = unityData;
            newMapInfo.isDC = isDC;
            newMapInfo.isPGC = isPGC;
            newMapInfo.bannerName = bannerName;
            newMapInfo.dcInfo = dcInfo;
            newMapInfo.editorVersion = editorVersion;
            newMapInfo.imgs = imgs;
            newMapInfo.dcPgcInfo = dcPgcInfo;
            newMapInfo.dataSubType = dataSubType;
            newMapInfo.renderJson = renderJson;
            newMapInfo.lastModifiedTime = lastModifiedTime;
            return newMapInfo;
        }

        public bool IsScenePgc()
        {
            return dcPgcInfo != null && dcPgcInfo.pgcId > 10000 && dcPgcInfo.classifyType == (int)BundlePart.Respgc;
        }

        public static bool IsScenePgc(int classifyID, int pgcID)
        {
            return pgcID > 10000 && classifyID == (int)BundlePart.Respgc;
        }
    }

    [Serializable]
    public class DowntownInfo
    {
        public string downtownId = "";
        public string downtownCover = "";
        public string downtownName = "";
        public string downtownJson = "";
        public string downtownDesc = "";
        public string downtownPngPrefix = "";
        public string editorVersion;
        public OfflineRenderListObj[] renderList;
        public DowntownInfo Clone()
        {

            DowntownInfo newDowntownInfo = new DowntownInfo();
            newDowntownInfo.downtownId = downtownId;
            newDowntownInfo.downtownCover = downtownCover;
            newDowntownInfo.downtownName = downtownName;
            newDowntownInfo.downtownJson = downtownJson;
            newDowntownInfo.downtownDesc = downtownDesc;
            newDowntownInfo.downtownPngPrefix = downtownPngPrefix;
            newDowntownInfo.editorVersion = editorVersion;
            newDowntownInfo.renderList = renderList;
            return newDowntownInfo;
        }
    }
    [Serializable]
    public class DowntownExtend
    {
        public bool isWhiteUser;
    }

    [Serializable]
    public class ResItemData
    {
        public MapInfo mapInfo;
        public Texture mapCover;
        public string mapJsonContent;
    }
    [Serializable]
    public class UGCMatData
    {
        public MapInfo mapInfo;
        public string mapId;
        public Texture2D matCover;
        public Texture2D matTex;
        public string coverUrl;
        public string matUrl;
    }

    [Serializable]
    public class BoundsInfo
    {
        public string center;
        public string size;


        public BoundsInfo(string center, string size)
        {
            this.center = center;
            this.size = size;
        }

        public Bounds Get()
        {
            return new Bounds(DataUtils.DeSerializeVector3(center), DataUtils.DeSerializeVector3(size));
        }

    }

    [Serializable]
    public class WaterCubeData
    {
        public int id;
        public string iconName;
        public string surfaceDiffuse;//水面漫反射
        public int surfaceDiffuseAlpha;//水面透明度
        public string surfaceEmission;//水面环境反射
        public int surfaceEmissionAlpha;//水面环境透明度
        public string edgeAlbedo;//水边界的颜色
        public int edgeAlbedoAlpha;//水边界的透明度
    }
    [Serializable]
    public class OfflineRenderListObj
    {
        [Obsolete("该字段属于老版本兼容字段, 已移动到abList中")]
        public string mapId;
        
        [Obsolete("该字段属于老版本兼容字段, 已移动到abList中")]
        public string renderUrl;

        public string version;

        public OfflineRenderData[] abList;

    }

    [Serializable]
    public class OfflineUGCNode
    {
        public NodeData nodeData;
        public OfflineRenderData renderData;
    }
    
    
    [Serializable]
    public class MapCreator
    {
        public string uid = "";
        public string userNick = "";
        public string userName = "";
        public string portraitUrl = "";
    }

    [Serializable]
    public class MapStatus
    {
        public bool isSetCover;
        public int isNew;
        public int isFavorites; // UGC衣服是否被收藏
        //public int liked; // moved to InteractStatus after 1.4version
    }

    /// <summary>
    /// 收藏的服饰交互数据
    /// </summary>
    [Serializable]
    public struct ClothingData
    {
        public string id;
        public int type;
        public string data;
    }

    [Serializable]
    public struct ClothingDataList
    {
        public List<ClothingData> favoritesInfo;
    }

    [Serializable]
    public class SetMatchData
    {
        public string name = "";
        public string coverUrl = "";
        public string data = "";
        public string clothesId = "";//"clothMapid,patternMapid"
        public int isUgcClothes;//是否包含ugc
        public DCUGCItemInfo[] dcUgcInfos;
        public DCPGCItemInfo[] dcPgcInfos;
        public PGCInfo[] rewards; //用户奖励部件信息..(奖励不存在转卖情况, 拥有后不会失去, 只需在Avatar编辑页做校验即可)
    }

    /// <summary>
    /// 收藏的搭配交互数据
    /// </summary>
    [Serializable]
    public struct MatchData
    {
        public string name;
        public string coverUrl;
        public string data;
    }

    [Serializable]
    public class MatchDataList
    {
        public string cookie = "";
        public int isEnd = -1;
        public List<MatchData> collocationInfo;
    }

    [Serializable]
    public class InteractStatus
    {
        public int liked;
        public int isCollect;
    }

    [Serializable]
    public class Relation
    {
        public int subscribed;
    }

    [Serializable]
    public class UpLoadMapBody
    {
        public MapInfo mapInfo;
        public int operationType = -1;
        public int templateId = 0;
    }

    [Serializable]
    public class UpLoadLikeBody
    {
        public MapInfo mapInfo;
        public int operationType = -1;
    }

    [Serializable]
    public class UpLoadAttentionBody
    {
        public string toUid = "";
        public int operationType = -1;
        public int clickPage = -1;
    }

    [Serializable]
    public class UserReqInfo
    {
        public string toUid = "";
    }

    [Serializable]
    public class ResourceInfo
    {
        public string cookie = "";
        public int isEnd = -1;
        public List<MapInfo> mapInfos;
    }
    
    public class DCUGCClothesRepInfo
    {
        public string cookie = "";
        public int isEnd = -1;
        public List<DCUGCClothesInfo> itemList;
    }

    [Serializable]
    public class PGCClothesRepInfo
    {
        public string cookie = "";
        public int isEnd = -1;
        public List<PGCClothesInfo> itemList;
    }

    [Serializable]
    public class NFTSeriesRepInfo
    {
        public string cookie = "";
        public int isEnd = -1;
        public List<NFTSeriesInfo> seriesList;
    }

    [Serializable]
    public class AvatarClothesRepInfo
    {
        public string cookie = "";
        public int isEnd = -1;
        public List<AvatarClothesInfo> resources;
    }

    [Serializable]
    public class AvatarClothesInfo
    {
        public int id;
        public int resourceType;
        public int isDelete;
        public int newFilter; //新用户过滤: 0-不需要 1-过滤
        public int sort; //排序: 值越大越靠前
        public int isNew;
        public int isFavorites;
        public string icon;
        //以下官方DC专有
        public string seriesName;
        public int detailsType;
        public string shadowUrl;
        public string bannerUrl;
        public string backgroundUrl;
        public string itemId = "";
        public string budActId = "";
        public int isOwner;

        public bool IsIllegal()
        {
            return id < 0 || resourceType <= 0 || isDelete == 1 ||
                ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY && newFilter == (int)EntryFilterType.firstEntry);
        }
    }

    public class RoleItemInfo
    {
        public string mapId;
        public int pgcId;
        public int sort; //序列值
        public int isNew;
        public int isFavorites;
        public RoleStyleItem item;
    }

    public class NFTItemInfo
    {
        public int sort; //序列值
        public int detailsType;
        public string shadowUrl;
        public string bannerUrl;
        public string backgroundUrl;
        public string itemId = "";
        public string budActId = "";
        public RoleStyleItem item;
    }

    public struct NFTSeriesInfo
    {
        public int seriesId;
        public string seriesName;
        public string seriesIcon;
        public string bannerUrl;
        public string backgroundUrl;
        public string shadowUrl;
    }

    public struct ReqQuerry
    {
        public int pageSize;
        public string cookie;
        public string toUid;
    }

    public struct HttpReqQuerry
    {
        public string cookie;
        public string pageSize;
        public string toUid;
        public int dataType;
        public int dataSubType;
    }

    public struct AvatarReqQuerry
    {
        public string cookie;
        public string toUid;
        public int pageSize;
        public int parentType;
        public int subType;
        public int componentType;
    }

    public struct NFTHttpReqQuerry
    {
        public string cookie;
        public string toUid;
        public int pageSize;
        public string seriesName; //默认:all
    }

    public struct DCHttpReqQuerry
    {
        public string pageSize;
        public string cookie;
        public string toUid;
        public int listType; //1:listing列表 2:owned列表
        public int itemType; //1:prop 2:clothes形象搭配
        public int classifyType; //11:headwear[头饰] 12:glasses[面饰] 16:ugcCloth[ugc衣服]
        public int componentType;
    }

    public struct RWHttpReqQuerry
    {
        public string pageSize;
        public string cookie;
        public string toUid;
        public int type; //0:all 1:仅本次活动奖励
        public int ith; //0:all  其他:指定查询
        public int dataType; //0:map 1:prop 2:clot 3:space
    }

    public struct RewardsRepQuerry
    {
        public int type; //打卡签到类型 - 1
    }

    [Serializable]
    public struct RewardsActivityInfo
    {
        public ActivityDetails activity;
    }

    [Serializable]
    public class ActivityDetails
    {
        public int type;
        public int status; //活动状态 0:未开始 1:活动中 2:已结束
    }

    public struct SearchRepQuerry
    {
        public string searchWord;
        public string cookie;
    }

    public struct UgcCreator
    {
        public string uid;
        public string userNick;
        public string userName;
    }

    public struct PhotoUgcInfo
    {
        public string ugcId;
        public string ugcName;
        public string ugcCover;
        public string ugcDesc;
        public int ugcType;//0:map 1:prop
        public UgcCreator ugcCreator;
    }

    public struct PhotoInfo
    {
        public string photoId;
        public string photoCover;
        public PhotoUgcInfo photoUgcInfo;
    }

    [Serializable]
    public class AvatarRedInfo
    {
        public AvatarRedDots[] avatarRedDots;
    }

    [Serializable]
    public class AvatarRedDots
    {
        public string resourceKind;
        public int[] newItems;
    }

    [Serializable]
    public class ClearRedDots
    {
        public string[] resourceKinds;
        public int cleanKind;
    }
    public struct UpLoadShotImg
    {
        public PhotoInfo photoInfo;
        public int operationType;
    }

    public struct SaveMediaParams
    {
        public int mediaType;
        public string mediaUrl;
    }
    public struct CheckPermission
    {
        public int permissionType;
        public int isCheck;
    }
    public struct RecievePermission
    {
        public int permissionType;
        public int grantType;
    }
    public struct OpenProfilePageParams
    {
        public int albumType;
    }

    public enum SavePhotoType
    {
        CheckInPhoto = 0,
        SystemPhoto
    }

    public struct OnProfilePageResp
    {
        public string portraitUrl; //原图url
        public string compressUrl; //压缩图url
    }

    [Serializable]
    public struct UpdateDataParam
    {
        public int updateType;
        public string param;
    }

    public struct NativeProfileParam
    {
        public string imgName;
        public string imgPath;
    }

    [Serializable]
    public class NativeDetailParam
    {
        public int optType = 0; //1:素材购买详情(DetailPageType.Prop)  2:NFT详情(DetailPageType.Nft)
        public string mapId = ""; //(仅optType==1) 素材id
        public int dataType = 0; //(仅optType==2) 1:素材NFT 2:衣服NFT
        public string itemId = ""; //(仅optType==2)
        public string budActId = ""; //(仅optType==2)
    }

    public struct H5Params
    {
        public string url;
    }
    

    public class LogEventBaseParam
    {
        
        public string mapId;
        public int scene;
        public int subType;
        public string seq;

        public LogEventBaseParam()
        {
            mapId = GlobalFieldController.CurMapInfo?.mapId;
            if (GameManager.Inst.engineEntry != null)
            {
                scene = GameManager.Inst.engineEntry.sceneType;
                subType = GameManager.Inst.engineEntry.subType;
            }
            if (GameManager.Inst.unityConfigInfo != null && !string.IsNullOrEmpty(GameManager.Inst.unityConfigInfo.seq))
            {
                seq = GameManager.Inst.unityConfigInfo.seq;
            }
        }
    }

    public class LogEventMapLight : LogEventBaseParam
    {
        public int point;
        public int spot;
    }
    public class LogEventAvgFpsParam : LogEventBaseParam
    {
        public int fps;
        public string cpu;
        public int cpuCount;
        public int memorySize;
        public string graphics;
        public int graphicsMemorySize;
        public int nodeCount;
        public int quality;
        public int dTextCount;
        public int targetFrameRate;
        public int isOpenPostProcess; 
        public int isHLOD; 
        public int isOcclusion;

        public LogEventAvgFpsParam()
        {
            cpu = SystemInfo.processorType;
            cpuCount = SystemInfo.processorCount;
            graphics = SystemInfo.graphicsDeviceName;
            graphicsMemorySize = SystemInfo.graphicsMemorySize;
            memorySize = SystemInfo.systemMemorySize;
            nodeCount = SceneBuilderUtils.Inst.GetAllNodeCount();
            quality = (int)QualityManager.Inst.GetQualityLevel();
            dTextCount = SceneBuilder.Inst.Get3DTextCount();
            targetFrameRate = Application.targetFrameRate;
            isHLOD = HLODSystem.HLOD.Inst.IsValid ? 1 : 0;
            isOpenPostProcess = GlobalFieldController.isOpenPostProcess ? 1 : 0;
            isOcclusion = MapRenderManager.Inst.isOcclusionEnable ? 1 : 0;
        }

    }

    public class LogEventStartOffline : LogEventBaseParam
    {
        public int totalProp;
        public int offlineProp;
    }
    
    public class LogEventEndOffline : LogEventBaseParam
    {
        public int inLocalCache;
        public int totalUGCProp;
    }
    
    public class LogEventDownLoadABStart : LogEventBaseParam
    {
        public string abFileName;
    }
    
    public class LogEventDownLoadABEnd : LogEventBaseParam
    {
        public string abFileName;
        public int size;
    }
    public class LogEventDownLoadABError : LogEventBaseParam
    {
        public string abFileName;
        public string error;
    }
    public class LogEventDownLoadABFinish : LogEventBaseParam
    {
        public int cache;
        public int total;
        public int error;
        public int useTime;
    }

    public class LogEventRestoreJsonStart : LogEventBaseParam
    {
        public int total_memory;
        public int roomMode;
        public string roomCode;

        public LogEventRestoreJsonStart()
        {
            total_memory = (int)(Profiler.GetTotalAllocatedMemoryLong() / 1048576);
            if (GameManager.Inst.onLineDataInfo != null)
            {
                if (!string.IsNullOrEmpty(GameManager.Inst.onLineDataInfo.roomCode))
                {
                    roomCode = GameManager.Inst.onLineDataInfo.roomCode;
                }
                roomMode = GameManager.Inst.onLineDataInfo.roomMode;
            }
            if (ClientManager.Inst != null)
            {
                if (!string.IsNullOrEmpty(ClientManager.Inst.roomCode))
                {
                    roomCode = ClientManager.Inst.roomCode;
                }
            }
        }
    }
    
    public class LogEventRestoreJsonEnd : LogEventBaseParam
    {
        public int total_memory;
        public int roomMode;
        public string roomCode;
        public LogEventRestoreJsonEnd()
        {
            total_memory = (int)(Profiler.GetTotalAllocatedMemoryLong() / 1048576);
            if (GameManager.Inst.onLineDataInfo != null)
            {
                if (!string.IsNullOrEmpty(GameManager.Inst.onLineDataInfo.roomCode))
                {
                    roomCode = GameManager.Inst.onLineDataInfo.roomCode;
                }
                roomMode = GameManager.Inst.onLineDataInfo.roomMode;
            }
            if (ClientManager.Inst != null)
            {
                if (!string.IsNullOrEmpty(ClientManager.Inst.roomCode))
                {
                    roomCode = ClientManager.Inst.roomCode;
                }
            }
        }
    }

    public class LogEventRoomchat : LogEventBaseParam
    {
        public string requestSeq;
        public string msgType;
    }

    public class LogEventAvatar : LogEventBaseParam {
        public string uid;
        public string avatar_result;
    }


    public class LogEventInfo
    {
        public int platform = 0;
        public string eventName;
        public LogEventBaseParam parameters;
    }
    public enum IsQueen
    {
        False = 0,//不是queen账号
        True = 1,//是queen账号

    }

    public struct LeaveRoomParam
    {
        public string roomCode;
        public string timestamp;
        public string seq;
    }

    public enum Data_Type
    {
        Map = 0,
        Prop = 1,
        Cloth = 2,
        Material = 4,
    }
    
    public enum DCUGCCloResType
    {
        Create = 0,
        Listing = 1,
        Owned = 2
    }

    public enum DCItemType
    {
        Prop = 1,
        Clothes = 2 //人物形象搭配
    }

    public enum CoverType
    {
        JPG = 0,
        PNG = 1,
    }

    public class ZipThreadParams
    {
        public string filePath;
        public Data_Type type;
        public Action<object> onFinish;
    }

    public class ZipClothThreadParams
    {
        public List<string> files;
        public string extension;
        public string inPath;
        public string zipName;
        public Action<object> onFinish;
    }
    
    [Serializable]
    public class LocalInfo
    {
        public Dictionary<string, PlayerLocalInfo> PlayerLocalInfoDic;
    }
    [Serializable]
    public class PlayerLocalInfo
    {
        public bool SocialNotification = true;
        public List<string> UgcColorDatas=new List<string>();//Ugcy衣服颜色数据集合
        public int LockMoveStick = 0; //1-锁定 0-不锁定
        public List<string> CustomizeColor = new List<string>();//自定义颜色
    }

    [Serializable]
    public class SettingLocalInfo
    {
        public Dictionary<string, GlobalSettingData> PlayerSettingInfoDict = new Dictionary<string, GlobalSettingData>();
    }

    [Serializable]
    public class EmoRedLocalInfo
    {
        public Dictionary<string, PlayerEmoRedInfo> playerEmoRedLocalInfo;
    }
    [Serializable]
    public class PlayerEmoRedInfo
    {
        public Dictionary<int, int> emoRedDics = new Dictionary<int, int>();//表情红点信息，id/isNew
    }

    public enum ModeResType
    {
        Base = 0,
        PGC = 1,
        particular = 2
    }

    public enum LoadingstageType
    {
        BuildingExperience = 0, //还原重建场景
        SearchingServers = 1, //匹配服务器
        EnteringServer = 2, //进入服务器
        EnteringExperience = 3 //同步场景内状态
    }

    [Serializable]
    public class UnityLoadingstage
    {
        public int stage; //对应LoadingstageType里的内容
    }

    public class SkyboxData
    {
        public int id;
        public string iconName;
        public string[] textures;
        public string cubemap;
    }

    //昼夜天空盒
    public class SkyboxDayNightData
    {
        public int id;
        public string iconName;
    }
    public class AirdropRewards
    {
        public List<AirdropReward> airdropRewards;
    }
    public class AirdropReward
    {
        public string rewardName;
        public string coverUrl;
        public bool canClaim;
        public List<RewardItem> itemList;
    }
    public class RewardItem
    {
        public string itemId;
        public string budActId;
        public string supply;
        public string tokenId;
        public bool canClaim;
    }

}

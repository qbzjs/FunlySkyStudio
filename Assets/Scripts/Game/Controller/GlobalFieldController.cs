using System.Collections.Generic;

public class GlobalFieldController
{
    public static bool isScreenShoting = false;
    public static string orgMapContent = string.Empty;

    
    // 白名单掩码
    public static WhiteListMask whiteListMask;
    
    // UGC素材对应内部数据 key-value 参数
    public static Dictionary<string, List<NodeData>> ugcNodeData = new Dictionary<string, List<NodeData>>();
    // UGC材质对应内部数据 key-value 参数
    public static Dictionary<string, UGCMatSaveData> ugcMatData = new Dictionary<string, UGCMatSaveData>();

    public static Dictionary<string, OfflineRenderData> offlineRenderDataDic =
        new Dictionary<string, OfflineRenderData>();

    public static GameMode CurGameMode =  GameMode.Edit;
    public static SCENE_TYPE CurSceneType = SCENE_TYPE.MAP_SCENE;
    public static string portalGateId = string.Empty;
    public static SavingData.MapInfo OrgMapInfo = null;
    public static SavingData.MapInfo CurMapInfo = null; //为了兼容传送门，每次切换地图前都手动赋值。目前只有点赞、关注按钮使用
    public static TerrainSizeConfigs terrainSize = TerrainSizeConfigs.fiveTimesTerrainSize;
    public static bool isOpenPostProcess = true;
    public static MapMode curMapMode = MapMode.NormalMap;
    public static bool IsDowntownEnter = false;
    public static bool isGameProcessing = false;
    public static int CollectIceGem;
    public static int MaxIceGem;
    #region PVP_DATA
    public static int pvpReadyTime = 9;
    public static int PVPRound = 0;
    public static PVPData pvpData;
    #endregion

    public static void Clear()
    {
        isScreenShoting = false;
        orgMapContent = string.Empty;
        ugcMatData.Clear();
        whiteListMask = default;
        ugcNodeData.Clear();
        offlineRenderDataDic.Clear();
        CurGameMode = GameMode.Edit;
        CurSceneType = SCENE_TYPE.MAP_SCENE;
        portalGateId = string.Empty;
        OrgMapInfo = null;
        CurMapInfo = null; 
        terrainSize = TerrainSizeConfigs.fiveTimesTerrainSize;
        isOpenPostProcess = true;
        PVPRound = 0;
        pvpData = default;
        IsDowntownEnter = false;
        curMapMode = MapMode.NormalMap;
        isGameProcessing = false;
    }


}

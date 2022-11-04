using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedDot;
using SavingData;
//using UnityEditor.VersionControl;
using UnityEngine;
public class GamePropData
{
    public int id;
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public int modType;
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public int handleType;
    public int uploadType;
    public string iconName;
    public string prefabName;
    public string bannerName;
    public int propType;
    public bool loadFromBundle;
    public int isWhiteListOnly;//是否为白名单专用道具
    [JsonConverter(typeof(BoundsConverter))]
    public Bounds bounds;
}

public class GameMatData
{
    public int id;
    public string iconName;
    public string texName;
    public float alpha;
    public float smoothness;
    public float metallic;
    public int isDelete = 0;
    public int matGroupType;
}

public class MusicBoardData
{
    public int id; //0~36 color  101~103 area
    public string iconName;
    public string darkColor;
    public string lightColor;
    public string wiseEvent;
}
public class BgmConfigData
{
    public int id;
    public string showName;
    public string wwiseName;
}
public class GameManager : CInstance<GameManager>
{
    private int baseTexCount = 21;
    public bool IsGPUInstance = false;
    public Dictionary<int, GamePropData> priConfigData { private set; get; }
    /// <summary>
    /// index == id
    /// </summary>
    public List<GameMatData> matConfigDatas { private set; get; }
    public List<BgmConfigData> bgmConfigDatas { private set; get; }
    public Dictionary<int, BgmConfigData> bgmConfigDataDics { private set; get; }
    public List<GameMatData> allTerrainConfigDatas { private set; get; }//全部地面材质
    public List<GameMatData> terrainConfigDatas { private set; get; }//显示的地面材质（1.38版本将不适合做地面的材质从ＵＩ上进行删除）
    public List<Material> BaseModelMats { private set; get; }
    public List<SkyboxData> skyboxDatas { private set; get; }
    public List<SkyboxDayNightData> skyboxDayNightDatas { private set; get; }
    public List<MusicBoardData> musicBoardDatas { private set; get; }
    public List<WaterCubeData> waterCubeDatas
    {
        private set; get;
    }
    public List<GamePropData> priData { private set; get; }
    public List<GamePropData> PGCPlantDatas { private set; get; }
    public Dictionary<int, GamePropData> PGCPlantDatasDic { private set; get; }
    private Camera mCamera;
    public Camera MainCamera
    {
        get
        {
            if (mCamera == null)
            {
                mCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
            }

            return mCamera;
        }
    }

    public DTextData dTextData { get; private set; }
    public ColorMatData colorMatData { get; private set; }
    public SpotLightData spotLightData { get; private set; }
    public PointLightData pointLightData { get; private set; }


    #region server data
    public EngineEntry engineEntry { get; set; }
    public MapInfo gameMapInfo { get; set; }
    public int isInWhiteList { get; set; }
    public BaseGameJson baseGameJsonData { get; set; }
    public UnityConfigInfo unityConfigInfo { get; set; }
    public UgcUntiyMapDataInfo ugcUntiyMapDataInfo { get; set; }
    public EditSaveInfo editSaveInfo { get; set; }
    public UserInfo ugcUserInfo { get; set; }
    public UGCClothInfo ugcClothInfo { get; set; }
    public RoleData ugcRoleData { get; set; }
    public OnLineDataInfo onLineDataInfo = new OnLineDataInfo();
    public string curDiyMapId = "";
    public List<string> ugcPropList = new List<string>();
    public NativeProfileParam nativeProfileParam { get; set; }
    public int maxPlayer;
    public List<string> subMapsData { get; set; }
    #endregion

    #region Bud-Downtown
    public DowntownInfo downtownInfo { get; set; }
    public string downtownJson { get; set; }
    public string curSubSeq = "";
    #endregion

    #region scene type
    public SCENE_TYPE sceneType;
    public NATIVE_TYPE nativeType = NATIVE_TYPE.MAPCALL;
    #endregion

    public int PlayerSpawnId = 0;

    public int LoadMapAsyncCount = 0;
    public bool loadingPageIsClosed = false; // 进房的Loading页是否已经关闭
    public RedDotSystemManager mRedDotSystemManager;
    public void Init()
    {
        BaseModelMats = new List<Material>();
        matConfigDatas = new List<GameMatData>();
        bgmConfigDatas = new List<BgmConfigData>();
        priConfigData = new Dictionary<int, GamePropData>();
        skyboxDatas = new List<SkyboxData>();
        skyboxDayNightDatas = new List<SkyboxDayNightData>();
        musicBoardDatas = new List<MusicBoardData>();
        allTerrainConfigDatas = new List<GameMatData>();
        waterCubeDatas = new List<WaterCubeData>();
        loadingPageIsClosed = false;
        
        skyboxDatas = ResManager.Inst.LoadJsonRes<List<SkyboxData>>("Configs/Skybox");
        skyboxDayNightDatas = ResManager.Inst.LoadJsonRes<List<SkyboxDayNightData>>("Configs/SkyboxDayNight");
        musicBoardDatas = ResManager.Inst.LoadJsonRes<List<MusicBoardData>>("Configs/MusicBoard");
        bgmConfigDatas = ResManager.Inst.LoadJsonRes<List<BgmConfigData>>("Configs/BgmConfig");
        allTerrainConfigDatas = ResManager.Inst.LoadJsonRes<List<GameMatData>>("Configs/TerrainTexture");
        matConfigDatas = ResManager.Inst.LoadJsonRes<List<GameMatData>>("Configs/PrimitiveBaseTexture");
        waterCubeDatas = ResManager.Inst.LoadJsonRes<List<WaterCubeData>>("Configs/WaterCube");
        priData = ResManager.Inst.LoadJsonRes<List<GamePropData>>("Configs/PrimitiveBase");
        priData.ForEach(x => priConfigData.Add(x.id, x));
        AssetLibrary.Inst.Init();
        InitDefaultData();
        InitBaseMat();
        GetShowTerrainConfigDatas();
        GetPGCPlantDatas(priData);
        GetBgmConfigDics();

        mRedDotSystemManager = new RedDotSystemManager();
    }
    //过滤已经上线但是需要删除的地面材质,留下需要显示的地面材质
    private void GetShowTerrainConfigDatas()
    {
        terrainConfigDatas = new List<GameMatData>();
        for (int i = 0; i < allTerrainConfigDatas.Count; i++)
        {
            if (allTerrainConfigDatas[i].isDelete != 1)//isDelete为1表示被删除
            {
                terrainConfigDatas.Add(allTerrainConfigDatas[i]);
            }
        }
    }

    //获取PGC植物数据
    private void GetPGCPlantDatas(List<GamePropData> PrimitiveData)
    {
        PGCPlantDatas = new List<GamePropData>();
        PGCPlantDatasDic = new Dictionary<int, GamePropData>();
        for (int i = 0; i < PrimitiveData.Count; i++)
        {
            if (PrimitiveData[i].id > 11000 && PrimitiveData[i].id <= 11999)//PGC植物ID为11000——11999
            {
                PGCPlantDatas.Add(PrimitiveData[i]);
                PGCPlantDatasDic.Add(PrimitiveData[i].id, PrimitiveData[i]);
            }
        }
    }
    private void GetBgmConfigDics()
    {
        bgmConfigDataDics = new Dictionary<int, BgmConfigData>();
        for (int i = 0; i < bgmConfigDatas.Count; i++)
        {
            bgmConfigDataDics.Add(bgmConfigDatas[i].id, bgmConfigDatas[i]);
        }
    }
    private void InitBaseMat()
    {
        if (!IsGPUInstance)
        {
            var opaqua = ResManager.Inst.LoadResNoCache<Material>("Material/StandardOpaque");
            var transparent = ResManager.Inst.LoadResNoCache<Material>("Material/StandardTransparent");
            //TODO: FEAT 发光材质
            // var emission = ResManager.Inst.LoadResNoCache<Material>("Material/Pgc_emission");
#if UNITY_ANDROID
            opaqua = new Material(Shader.Find("Custom/CustomDiffuse"));
            transparent = new Material(Shader.Find("Custom/CustomDiffuseAlpha"));
            //TODO: FEAT 发光材质
            // emission = new Material(Shader.Find("Pgc_emisson"));
#endif
            BaseModelMats.Add(opaqua);
            BaseModelMats.Add(transparent);
            //TODO: FEAT 发光材质
            // BaseModelMats.Add(emission);
        }
        else
        {
            var opaqua = ResManager.Inst.LoadResNoCache<Material>("Material/GPUStandardOpaque");
            var transparent = ResManager.Inst.LoadResNoCache<Material>("Material/GPUStandardTransparent");
            Texture2D tex1 = ResManager.Inst.LoadRes<Texture2D>(GameConsts.BaseTexPath + "tex_1");
            var textureArray = new Texture2DArray(tex1.width, tex1.height, baseTexCount, tex1.format, false, false);
            for (int i = 0; i < baseTexCount; i++)
            {
                Texture2D tempTex = ResManager.Inst.LoadRes<Texture2D>(GameConsts.BaseTexPath + "tex_"+(i+1));
                Graphics.CopyTexture(tempTex, 0, 0, textureArray, i, 0); // i is the index of the texture
            }
            opaqua.SetTexture("_MainTex", textureArray);

            var transTextureArray = new Texture2DArray(tex1.width, tex1.height, 1, tex1.format, false, false);
            Graphics.CopyTexture(tex1, 0, 0, transTextureArray, 0, 0);
            transparent.SetTexture("_MainTex", transTextureArray);
            BaseModelMats.Add(opaqua);
            BaseModelMats.Add(transparent);
        }
    }

    private void InitDefaultData()
    {
        //colorMatData = new ColorMatData();
        //colorMatData.mat = 0;
        //colorMatData.cols = DataUtils.ColorToString(AssetLibrary.Inst.colorLib.Get(0));
        //colorMatData.tile = DataUtils.Vector2ToString(new Vector2(1, 1));

        pointLightData = new PointLightData();
        pointLightData.inte = 1.5f;
        pointLightData.rng = 6;
        pointLightData.lico = DataUtils.ColorToString(Color.white);

        spotLightData = new SpotLightData();
        spotLightData.inte = 4;
        spotLightData.rng = 5.5f;
        spotLightData.spoa = 51;
        spotLightData.lico = DataUtils.ColorToString(Color.white);

        dTextData = new DTextData();
        dTextData.tex = "Enter text...";
        dTextData.textcol = DataUtils.ColorToString(Color.white);
    }

    public string CheckLang()
    {
        if(baseGameJsonData == null || baseGameJsonData.baseInfo == null || string.IsNullOrEmpty(baseGameJsonData.baseInfo.lang))
        {
            return "";
        }
        return baseGameJsonData.baseInfo.lang;
    }

    public string CheckLocale()
    {
        if (baseGameJsonData == null || baseGameJsonData.baseInfo == null || string.IsNullOrEmpty(baseGameJsonData.baseInfo.locale))
        {
            return "";
        }
        return baseGameJsonData.baseInfo.locale;
    }
}

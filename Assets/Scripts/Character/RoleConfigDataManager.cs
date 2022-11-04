
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

//仅适用于衣服分类页
public enum UGCClothesResType {
    UGC = 0, //非dc
    DC = 1, //dc-ugc
    PGC = 2 //所有pgc
}
public enum DataSubType
{
    Clothes = 0,
    Patterns = 1,
}

public enum AvatarResType
{
    UGCDC = 0,
    PGCDC = 1,
    Normal = 2,//非DC
}

public enum RoleResGrading
{
    Normal = 0,
    DC = 1,
}

public enum RoleOriginType
{
    Normal = 0,
    Rewards = 1,
    Airdrop = 2,
}

public enum RoleSubType
{
    Default = 0,
    Neverafter = 1,
    Vetraska = 2,
}

public enum HandBipType
{
    Arm = 0,
    Glove = 1,
}

public enum HandLRType
{
    Default = 0,
    Left = 1,
    Right = 2,
    Both = 3
}

[Serializable]
public struct UgcResData
{
    public string ugcMapId;//UGCMapId，用于审核
    public string ugcJson;//UGCJson数据
    public string ugcUrl;//UGC贴图压缩包
    public int ugcType;
}

[Serializable]
public class RoleData:ICloneable
{
    public int sex;
    public int hId;//头发ID
    public string hCr = "#6B4F3C";//头发颜色
    public string sCr = "#e9c5c5";//肤色
    public int eId;//眼睛ID
    public string eCr = "#f4f4f4"; // 眼睛瞳孔颜色
    public Vec3 eP = new Vec3(-0.16f, 0.21f, 0.1f);//眼睛位置
    public Vec3 eR = new Vec3(0, 0, 0);//眼睛旋转
    public Vec3 eS = new Vec3(1, 1, 1);//眼睛缩放
    public int bId;//眉毛ID
    public string bCr = "#6B4F3C";//眉毛颜色
    public Vec3 bP = new Vec3(-0.24f, 0.21f, 0.09f);//眉毛位置
    public Vec3 bR = new Vec3(0, 0, 0);//眉毛旋转
    public Vec3 bS = new Vec3(1, 1, 1);//眉毛缩放

    public int nId = 1;//鼻子ID
    public Vec3 nP = new Vec3(-0.11f, 0.22f, 0);//鼻子位置
    public Vec3 nCS = new Vec3(1, 1, 1);//鼻子纵向缩放 nCS -> nose child Node Scale
    public Vec3 nPS = new Vec3(1, 1, 1);//鼻子整体缩放 nPS -> nose parent Node Scale

    public int mId;//嘴巴ID
    public Vec3 mP = new Vec3(-0.05f, 0.22f, 0);//嘴巴位置
    public Vec3 mS = new Vec3(1, 1, 1);//嘴巴缩放
    public Vec3 mR = new Vec3(-166, 90, 0);//嘴巴旋转
    public int bluId;//腮红ID
    public string bluCr = "#ffa9a6";//腮红颜色
    public Vec3 bluS = new Vec3(1, 1, 1);//腮红缩放
    public Vec3 bluP = new Vec3(-0.1f, 0.17f, 0.13f);//腮红位置


    public int ugcClothType = 0;//针对UGC衣服分类,默认为0：普通UGC衣服 1:DC UGC衣服
    public int cloId;//衣服ID UGC衣服 >=1000
    public string clothMapId = "";//UGC衣服的MapId，用于审核
    public string clothesJson = "";//UGC衣服Json数据
    public string clothesUrl = "";//UGC衣服贴图压缩包

    public int hatId = 0;//头饰ID
    public Vec3 hatP = new Vec3(-0.24f, 0, 0);//头饰位置
    public Vec3 hatS = new Vec3(1, 1, 1);//头饰缩放
    public Vec3 hatR = new Vec3(0, 0, 0);//头饰旋转
    public string hatCr = "#F4FEFF";//头饰颜色
    public int glId = 0;//眼镜ID
    public Vec3 glP = new Vec3(-0.16f, 0.22f, 0);//眼镜位置
    public Vec3 glS = new Vec3(1, 1, 1);//眼镜大小
    public Vec3 glR = new Vec3(0, -90, 180);//眼镜旋转
    public string glCr = "#301818";//眼镜颜色
    public int acId = 1;//围巾ID
    public int shoeId = 28;//鞋子ID 如果shoeId == 1001 说明未设置过鞋子
    public int bagId=0;//背包ID
    public int cbId = 0;//斜挎包ID
    public Vec3 bagP = new Vec3(-0.06f, -0.09f, 0);//背包位置
    public Vec3 bagS = new Vec3(1, 1, 1);//背包大小
    public Vec3 bagR = new Vec3(0, -90, -180);//背包旋转
    public string bagCr = "#F4FEFF";//背饰颜色
    public int hdId = 0;//手部挂饰ID
    public Vec3 hdP = new Vec3(0, 0, 0);//手部挂饰位置
    public Vec3 hdS = new Vec3(1, 1, 1);//手部挂饰缩放
    public Vec3 hdR = new Vec3(0, 0, 0);//手部挂饰旋转
    public int hdLR = 0;//左右手

    public int fpId;//面部彩绘ID
    public Vec3 fpP = new Vec3(0, 0, 0);//面部彩绘位置
    public Vec3 fpS = new Vec3(1, 1, 1);//面部彩绘缩放
    public UgcResData ugcFPData = new UgcResData();


    public int saId;//特殊挂饰ID
    public Vec3 saP = new Vec3(-0.05f, -0.01f, 0);//特殊挂饰位置
    public Vec3 saR = new Vec3(0, 0, 0);//特殊挂饰旋转
    public Vec3 saS = new Vec3(1, 1, 1);//特殊挂饰缩放

    public int effId;//特效ID
    public Vec3 effP = new Vec3(-0.24f, 0, 0);//特效位置
    public Vec3 effS = new Vec3(1, 1, 1);//特效缩放
    public Vec3 effR = new Vec3(0, 0, 0);//特效旋转

    public int sceneType; //0 换装场景 1 3d场景
    public object Clone()
    {
        RoleData roleData = this.MemberwiseClone() as RoleData;
        roleData.hId = hId;
        roleData.hCr = hCr;
        roleData.sCr = sCr;
        roleData.eId = eId;
        roleData.eCr = eCr;
        roleData.eP = eP.Clone();
        roleData.eR = eR.Clone();
        roleData.eS = eS.Clone();
        roleData.bId = bId;
        roleData.bCr = bCr;
        roleData.bP = bP.Clone();
        roleData.bR = bR.Clone();
        roleData.bS = bS.Clone();
        roleData.nId = nId;
        roleData.nP = nP.Clone();
        roleData.nCS = nCS.Clone();
        roleData.nPS = nPS.Clone();
        roleData.mId = mId;
        roleData.mP = mP.Clone();
        roleData.mS = mS.Clone();
        roleData.mR = mR.Clone();
        roleData.bluId = bluId;
        roleData.bluCr = bluCr;
        roleData.bluP = bluP.Clone();
        roleData.bluS = bluS.Clone();

        roleData.cloId = cloId;
        roleData.clothMapId = clothMapId;
        roleData.clothesJson = clothesJson;
        roleData.clothesUrl = clothesUrl;
        roleData.ugcClothType = ugcClothType;
        roleData.hatId = hatId;
        roleData.hatP = hatP.Clone();
        roleData.hatS = hatS.Clone();
        roleData.hatR = hatR.Clone();
        roleData.hatCr = hatCr;
        roleData.glId = glId;
        roleData.glCr = glCr;
        roleData.glP = glP.Clone();
        roleData.glS = glS.Clone();
        roleData.glR = glR.Clone();
        roleData.acId = acId;
        roleData.shoeId = shoeId;
        roleData.bagId = bagId;
        roleData.cbId = cbId;
        roleData.bagP = bagP.Clone();
        roleData.bagS = bagS.Clone();
        roleData.bagR = bagR.Clone();
        roleData.bagCr = bagCr;
        roleData.hdId = hdId;
        roleData.hdP = hdP.Clone();
        roleData.hdS = hdS.Clone();
        roleData.hdR = hdR.Clone();
        roleData.hdLR = hdLR;
        roleData.fpId = fpId;
        roleData.fpP = fpP.Clone();
        roleData.fpS = fpS.Clone();
        roleData.ugcFPData = ugcFPData;
        roleData.saId = saId;
        roleData.saP = saP.Clone();
        roleData.saS = saS.Clone();
        roleData.saR = saR.Clone();
        roleData.sceneType = sceneType;
        return roleData;
    }
}
[Serializable]
public class RoleIconData
{
    public int id;
    public string spriteName;
    public string texName;
    public string modelName;
    //public bool isNew; //47版本开始废弃, 由服务器更新
    public int grading; //0:Normal 1:DC
    public int origin; //0:Normal 1:Rewards 2:Airdrop
    public int subType; //0:Default 1:Neverafter 2:Vetraska
    public int specialType; // 1:sowrd
    [NonSerialized]
    public RoleStyleItem rc;
    //返回RoleIconData是否为普通部件
    public bool IsNormal()
    {
        return grading == (int)RoleResGrading.Normal && origin == (int)RoleOriginType.Normal && subType == (int)RoleSubType.Default;
    }
    //返回RoleIconData是否为通用部件(保存无校验逻辑)
    public bool IsOrigin()
    {
        return grading == (int)RoleResGrading.Normal && origin == (int)RoleOriginType.Normal;
    }
}
[Serializable]
public class RoleUGCIconData
{
    public string coverUrl;
    public string jsonUrl;
    public string zipUrl;
    public int templateId;
    public string mapId;
    public int isNew;
    public int isFavorites;
    public int classifyType; //45适配通用pgc信息(后续ugc需要给ugc类型, 不区分dc)
    public int pgcId; //临时添加
    public int grading; //0:Normal 1:DC(统一在此处区分dc)
    public int origin; //0:Normal 1:Rewards 2:Airdrop
}
[Serializable]
public class Vec3
{
    public float x;
    public float y;
    public float z;

    public Vec3(float vx, float vy, float vz)
    {
        x = vx;
        y = vy;
        z = vz;
    }

    public Vec3 Clone()
    {
        return new Vec3(x, y, z);
    }

    public static implicit operator Vector3(Vec3 vec)
    {
        return new Vector3(vec.x, vec.y, vec.z);
    }


    public static implicit operator Vec3(Vector3 vec)
    {
        return new Vec3(vec.x, vec.y, vec.z);
    }
}

[Serializable]
public class RoleColorConfigData
{
    public List<string> allColors; // 色盘全部颜色
    public List<string> commonColors; // 常用色
}

[Serializable]
public class RoleColorConfig
{
    public RoleColorConfigData hairColors;
    public RoleColorConfigData hatColors;
    public RoleColorConfigData faceStyleColors;
    public RoleColorConfigData skinColors;
    public RoleColorConfigData browColors;
    public RoleColorConfigData bagColors;
    public RoleColorConfigData glassesColors;
    public RoleColorConfigData eyeColors;
}

[Serializable]
public class EyeStyleData: RoleIconData
{
    public Vec3 pDef;
    public Vec3 sDef;
    public Vec3 rDef;
    public Vec3[] vLimit;
    public Vec3[] hLimit;
    public Vec3[] fLimit;
    public Vec3[] scaleLimit;
    public Vec3[] rotateLimit;
    public bool CantSetColor;
}

public class BrowStyleData : RoleIconData
{
    public Vec3 pDef;
    public Vec3 rDef;
    public Vec3 sDef;
    public Vec3[] vLimit;
    public Vec3[] hLimit;
    public Vec3[] fLimit;
    public Vec3[] rotateLimit;
    public Vec3[] scaLimit;
}
[Serializable]
public class MouseStyleData : RoleIconData
{
    public Vec3 pDef;
    public Vec3 sDef;
    public Vec3 rDef;
    public Vec3[] vLimit;
    public Vec3[] hLimit;
    public Vec3[] fLimit;
    public Vec3[] scaLimit;
    public Vec3[] rotLimit;
}
public class BlushStyleData : RoleIconData
{
    public Vec3 pDef;
    public Vec3 sDef;
    public Vec3[] vLimit;
    public Vec3[] hLimit;
    public Vec3[] fLimit;
    public Vec3[] scaLimit;
}
[Serializable]
public class HatStyleData : RoleIconData
{
    public Vec3 pDef;
    public Vec3 sDef;
    public Vec3 rDef;
    public Vec3[] vLimit;
    public Vec3[] scaleLimit;
    public Vec3[] fLimit;
    public Vec3[] hLimit;
    public Vec3[] xrotLimit;
    public Vec3[] yrotLimit;
    public Vec3[] zrotLimit;

    public bool CantSetColor;
}

[Serializable]
public class EffectStyleData : RoleIconData
{
    public Vec3 pDef;
    public Vec3 sDef;
    public Vec3 rDef;
    public Vec3[] vLimit;
    public Vec3[] scaleLimit;
    public Vec3[] fLimit;
    public Vec3[] hLimit;
    public Vec3[] xrotLimit;
    public Vec3[] yrotLimit;
    public Vec3[] zrotLimit;
}

[Serializable]
public class GlassesStyleData : RoleIconData
{
    public Vec3 pDef;
    public Vec3 sDef;
    public Vec3 rDef;
    public Vec3[] scaLimit;
    public Vec3[] vLimit;
    public Vec3[] fLimit;
    public Vec3[] hLimit;
    public Vec3[] xrotLimit;
    public Vec3[] yrotLimit;
    public Vec3[] zrotLimit;
    public bool CantSetColor;
}

[Serializable]
public class NoseStyleData : RoleIconData
{
    public Vec3 pDef;
    public Vec3 childSDef;
    public Vec3 parentSDef;
    public Vec3[] vLimit;
    public Vec3[] childHScaleLimit;
    public Vec3[] childVScaleLimit;
    public Vec3[] parentScaleLimit;
}
[Serializable]
public class ShoeStyleData : RoleIconData
{
}

[Serializable]
public class AccessoriesStyleData : RoleIconData
{
}

[Serializable]
public class ClothStyleData : RoleIconData
{
    public int templateId;//UGC衣服模版Id
    public string clothesJson = ""; //衣服JsonUrl
    public string clothesUrl = ""; //贴图Url
    public string clothMapId = ""; //UGC衣服的MapId，用于内容审核
    public int classifyType; //角色部件分类，同ClassifyType枚举值
    public int pgcId; //配置表内pgc资源id，100000-200000
    public string abUrl = "";//AB包Url 预留暂时为空 用不到
    public int dataSubType;//仅用于场景内加入UGC衣服
    public int extraType;
    //返回ClothStyleData是否为PGC
    public bool IsPGC()
    {
        return id < 1000 || grading == (int)RoleResGrading.DC || origin == (int)RoleOriginType.Rewards;
    }
}



[Serializable]
public class BagStyleData : RoleIconData
{
    public Vec3 pDef;
    public Vec3 sDef;
    public Vec3 rDef;
    public Vec3[] vLimit;
    public Vec3[] fLimit;
    public Vec3[] scaLimit;
    public Vec3[] hLimit;
    public Vec3[] xrotLimit;
    public Vec3[] yrotLimit;
    public Vec3[] zrotLimit;
    public bool CantSetColor;
    public int bagCompType;
}

[Serializable]
public class PatternStyleData : RoleIconData
{
    public Vec3 pDef;
    public Vec3 sDef;
    public Vec3[] vLimit;
    public Vec3[] scaleLimit;
    public int templateId;//UGC面部彩绘模版Id
    public string patternJson = ""; //面部彩绘JsonUrl
    public string patternUrl = ""; //贴图Url
    public string patternMapId = ""; //UGC面部彩绘的MapId，用于内容审核
    //返回是否为PGC
    public bool IsPGC()
    {
        return id < 2001 || grading == (int)RoleResGrading.DC || origin == (int)RoleOriginType.Rewards;
    }
}

[Serializable]
public class HandStyleData : RoleIconData
{
    public Vec3 pDef;
    public Vec3 rDef;
    public Vec3 sDef;
    public Vec3[] scaleLimit;
    public Vec3[] vLimit;
    public Vec3[] fLimit;
    public Vec3[] hLimit;
    public Vec3[] xrotLimit;
    public Vec3[] yrotLimit;
    public Vec3[] zrotLimit;
    public int handBipType; //0:Arm, 1:Glove
    public int leftRightType; //0:default 1:left 2:right 3:both
}

[Serializable]
public class SpecialStyleData : RoleIconData
{
    public Vec3 pDef;
    public Vec3 sDef;
    public Vec3 rDef;
    public Vec3[] scaleLimit;
    public Vec3[] vLimit;
    public Vec3[] fLimit;
    public Vec3[] hLimit;
    public Vec3[] xrotLimit;
    public Vec3[] yrotLimit;
    public Vec3[] zrotLimit;
}

[Serializable]
public class RoleColorData : RoleIconData
{
    public string hexColor = "";
}

[Serializable]
public class RoleConfigData
{
    public int sex;//0:未设置 1:男 2:女
    public List<RoleIconData> hairIcons = new List<RoleIconData>();
    public List<EyeStyleData> eyeStyles = new List<EyeStyleData>();
    public List<BrowStyleData> browStyles = new List<BrowStyleData>();
    public List<NoseStyleData> noseStyles = new List<NoseStyleData>();
    public List<MouseStyleData> mouseStyles = new List<MouseStyleData>();
    public List<BlushStyleData> faceStyles = new List<BlushStyleData>();
    public List<ClothStyleData> clothes = new List<ClothStyleData>();
    public List<HatStyleData> hatStyles = new List<HatStyleData>();
    public List<GlassesStyleData> glassesStyles = new List<GlassesStyleData>();
    public List<AccessoriesStyleData> accessoriesStyles = new List<AccessoriesStyleData>();
    public List<ShoeStyleData> shoesStyles = new List<ShoeStyleData>();
    public List<BagStyleData> bagStyles = new List<BagStyleData>();
    public List<PatternStyleData> patternStyles = new List<PatternStyleData>();
    public List<SpecialStyleData> specialStyles = new List<SpecialStyleData>();
    public List<RoleColorData> hairColors = new List<RoleColorData>();
    public List<RoleColorData> skinColors = new List<RoleColorData>();
    public List<RoleColorData> browColors = new List<RoleColorData>();
    public List<RoleColorData> faceStyleColors = new List<RoleColorData>();
    public List<HandStyleData> handStyles = new List<HandStyleData>();
    public List<EffectStyleData> effectStyles = new List<EffectStyleData>();
}

[Serializable]
public class DefClothData
{
    public int id;
    public string defShoeName;
}

[Serializable]
public class DefClothDataConfig
{
    public List<DefClothData> DefClothData = new List<DefClothData>();
}

public class RoleConfigDataManager:CInstance<RoleConfigDataManager>
{
    public RoleConfigData womanRoleConfigData;
    public RoleConfigData manRoleConfigData;
    public RoleColorConfig roleColorConfig;
    public RoleData defRoleConfigData;
    public Dictionary<ClassifyType, List<RoleIconData>> ConfigDataDic { private set; get; }
    public RoleData CurRoleData { private set; get; }
    public RoleConfigData CurConfigRoleData { private set; get; }
    public Dictionary<ClassifyType, ClassifyType> UgcToPgcDic { private set; get; }
    //管理动态下载替换的图片
    private Dictionary<Image, string> curImgName = new Dictionary<Image, string>();

    public void LoadRoleConfig()
    {
        int sex = 1;
        womanRoleConfigData = ResManager.Inst.LoadJsonRes<RoleConfigData>("Configs/RoleData/RoleConfigDataWoman");
        manRoleConfigData = ResManager.Inst.LoadJsonRes<RoleConfigData>("Configs/RoleData/RoleConfigDataMan");
        
        manRoleConfigData.clothes = ResManager.Inst.LoadJsonRes<List<ClothStyleData>>("Configs/RoleData/RoleConfigDataClothes");
        manRoleConfigData.bagStyles = ResManager.Inst.LoadJsonRes<List<BagStyleData>>("Configs/RoleData/RoleConfigDataBag");
        manRoleConfigData.glassesStyles = ResManager.Inst.LoadJsonRes<List<GlassesStyleData>>("Configs/RoleData/RoleConfigDataGlasses");
        manRoleConfigData.handStyles = ResManager.Inst.LoadJsonRes<List<HandStyleData>>("Configs/RoleData/RoleConfigDataHand");
        manRoleConfigData.hatStyles = ResManager.Inst.LoadJsonRes<List<HatStyleData>>("Configs/RoleData/RoleConfigDataHats");
        manRoleConfigData.effectStyles = ResManager.Inst.LoadJsonRes<List<EffectStyleData>>("Configs/RoleData/RoleConfigDataEffect");
        manRoleConfigData.specialStyles = ResManager.Inst.LoadJsonRes<List<SpecialStyleData>>("Configs/RoleData/RoleConfigDataSpecial");
        manRoleConfigData.accessoriesStyles = ResManager.Inst.LoadJsonRes<List<AccessoriesStyleData>>("Configs/RoleData/RoleConfigDataAccessoies");
        manRoleConfigData.hairIcons = ResManager.Inst.LoadJsonRes<List<RoleIconData>>("Configs/RoleData/RoleConfigDataHair");    
        manRoleConfigData.shoesStyles = ResManager.Inst.LoadJsonRes<List<ShoeStyleData>>("Configs/RoleData/RoleConfigDataShoe");
        manRoleConfigData.browStyles = ResManager.Inst.LoadJsonRes<List<BrowStyleData>>("Configs/RoleData/RoleConfigDataBrow");
        manRoleConfigData.eyeStyles = ResManager.Inst.LoadJsonRes<List<EyeStyleData>>("Configs/RoleData/RoleConfigDataEyes");
        manRoleConfigData.faceStyles = ResManager.Inst.LoadJsonRes<List<BlushStyleData>>("Configs/RoleData/RoleConfigDataFace");    
        manRoleConfigData.mouseStyles = ResManager.Inst.LoadJsonRes<List<MouseStyleData>>("Configs/RoleData/RoleConfigDataMouse");
        manRoleConfigData.noseStyles = ResManager.Inst.LoadJsonRes<List<NoseStyleData>>("Configs/RoleData/RoleConfigDataNose");
        manRoleConfigData.patternStyles = ResManager.Inst.LoadJsonRes<List<PatternStyleData>>("Configs/RoleData/RoleConfigDataPattern");
        CurConfigRoleData = sex == 1 ? manRoleConfigData : womanRoleConfigData;
        roleColorConfig = ResManager.Inst.LoadJsonRes<RoleColorConfig>("Configs/RoleData/RoleColorConfigData");
        defRoleConfigData = ResManager.Inst.LoadJsonRes<RoleData>("Configs/RoleData/DefConfigRoleData");
        InitConfigDataDic();
        InitClassifyTypeConfigDic();
    }

    private void InitConfigDataDic()
    {
        ConfigDataDic = new Dictionary<ClassifyType, List<RoleIconData>>();
        ConfigDataDic.Add(ClassifyType.hair, new List<RoleIconData>(CurConfigRoleData.hairIcons));
        ConfigDataDic.Add(ClassifyType.eyes, new List<RoleIconData>(CurConfigRoleData.eyeStyles));
        ConfigDataDic.Add(ClassifyType.brows, new List<RoleIconData>(CurConfigRoleData.browStyles));
        ConfigDataDic.Add(ClassifyType.nose, new List<RoleIconData>(CurConfigRoleData.noseStyles));
        ConfigDataDic.Add(ClassifyType.mouth, new List<RoleIconData>(CurConfigRoleData.mouseStyles));
        ConfigDataDic.Add(ClassifyType.blush, new List<RoleIconData>(CurConfigRoleData.faceStyles));
        ConfigDataDic.Add(ClassifyType.outfits, new List<RoleIconData>(CurConfigRoleData.clothes));
        ConfigDataDic.Add(ClassifyType.headwear, new List<RoleIconData>(CurConfigRoleData.hatStyles));
        ConfigDataDic.Add(ClassifyType.glasses, new List<RoleIconData>(CurConfigRoleData.glassesStyles));
        ConfigDataDic.Add(ClassifyType.accessories, new List<RoleIconData>(CurConfigRoleData.accessoriesStyles));
        ConfigDataDic.Add(ClassifyType.shoes, new List<RoleIconData>(CurConfigRoleData.shoesStyles));
        ConfigDataDic.Add(ClassifyType.bag, new List<RoleIconData>(CurConfigRoleData.bagStyles));
        ConfigDataDic.Add(ClassifyType.patterns, new List<RoleIconData>(CurConfigRoleData.patternStyles));
        ConfigDataDic.Add(ClassifyType.hand, new List<RoleIconData>(CurConfigRoleData.handStyles));
        ConfigDataDic.Add(ClassifyType.effects, new List<RoleIconData>(CurConfigRoleData.effectStyles));
        ConfigDataDic.Add(ClassifyType.special, new List<RoleIconData>(CurConfigRoleData.specialStyles));
    }

    private void InitClassifyTypeConfigDic()
    {
        //TODO: 提取到配置表
        UgcToPgcDic = new Dictionary<ClassifyType, ClassifyType>();
        UgcToPgcDic.Add(ClassifyType.ugcCloth, ClassifyType.outfits);
        UgcToPgcDic.Add(ClassifyType.ugcPatterns, ClassifyType.patterns);
    }

    public RoleIconData GetConfigDataByTypeAndId(ClassifyType type, int id)
    {
        if (!ConfigDataDic.ContainsKey(type))
        {
            LoggerUtils.LogError($"GetConfigDataByTypeAndId Failed! --> type:{type}  id:{id}");
            return null;
        }
        return ConfigDataDic[type].Find(a => a.id == id);
    }

    public string GetAvatarIconPath(string spriteName)
    {
        //示例：https://cdn.joinbudapp.com/U3D/AvatarIcon/caps/caps_07_01.png
        string head = "https://cdn.joinbudapp.com/U3D/AvatarIcon";
        var spriteStr = spriteName.Split('_');
        string folderName = spriteStr[0];
        return string.Format("{0}/{1}/{2}.png", head, folderName, spriteName);
    }

    public string GetAvatarIconPath(ClassifyType type, int id)
    {
        var data = GetConfigDataByTypeAndId(type, id);
        if (data != null && !string.IsNullOrEmpty(data.spriteName))
        {
            return GetAvatarIconPath(data.spriteName);
        }
        return null;
    }

    //目前仅适用于pgc部件Icon
    public void SetAvatarIcon(Image targetImg, string spriteName, SpriteAtlas spriteAtlas = null, Action<ImgLoadState> loadAct = null)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            loadAct?.Invoke(ImgLoadState.Failed);
            return;
        }

        //优先从本地读取
        if (spriteAtlas != null)
        {
            targetImg.sprite = spriteAtlas.GetSprite(spriteName); //找不到返回null
            if (targetImg.sprite != null)
            {
                loadAct?.Invoke(ImgLoadState.Complete);
                return;
            }
        }

        //本地读取失败, 从s3下载
        loadAct?.Invoke(ImgLoadState.Loading);
        var url = GetAvatarIconPath(spriteName);
        UGCResourcePool.Inst.DownloadAndGet(url, tex =>
        {
            if (tex != null)
            {
                Sprite sprite = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                targetImg.sprite = sprite;
                loadAct?.Invoke(ImgLoadState.Complete);
            }
        }, () => loadAct?.Invoke(ImgLoadState.Failed));
    }

    //目前仅适用于pgc部件Icon, 针对动态替换更新的情况
    public void SetAvatarIconDynamic(Image targetImg, string spriteName, SpriteAtlas spriteAtlas = null, Action<ImgLoadState> loadAct = null)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            loadAct?.Invoke(ImgLoadState.Failed);
            return;
        }

        //记录当前Img正在加载/显示的图片
        if (!curImgName.ContainsKey(targetImg))
        {
            curImgName.Add(targetImg, spriteName);
        }
        else
        {
            curImgName[targetImg] = spriteName;
        }

        //优先从本地读取
        if (spriteAtlas != null)
        {
            targetImg.sprite = spriteAtlas.GetSprite(spriteName); //找不到返回null
            if (targetImg.sprite != null)
            {
                loadAct?.Invoke(ImgLoadState.Complete);
                return;
            }
        }
        
        //本地读取失败, 从s3下载
        loadAct?.Invoke(ImgLoadState.Loading);
        var url = GetAvatarIconPath(spriteName);
        UGCResourcePool.Inst.DownloadAndGet(url, tex =>
        {
            if (tex != null && curImgName[targetImg] == spriteName)
            {
                Sprite sprite = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                targetImg.sprite = sprite;
                loadAct?.Invoke(ImgLoadState.Complete);
            }
        }, () => loadAct?.Invoke(ImgLoadState.Failed));
    }

    public void SetRoleData(RoleData data)
    {
        CurRoleData = data;
    }

    private int[] skinRange = { 0,1,2,3,6};

    public RoleData GetDefaultRoleData()
    {
        RoleData data = new RoleData();
        data.eP = CurConfigRoleData.eyeStyles[1].pDef;
        data.eR = CurConfigRoleData.eyeStyles[1].rDef;
        data.eS = CurConfigRoleData.eyeStyles[1].sDef;
        data.bP = CurConfigRoleData.browStyles[1].pDef;
        data.bR = CurConfigRoleData.browStyles[1].rDef;
        data.bS = CurConfigRoleData.browStyles[1].sDef;
        data.mP = CurConfigRoleData.mouseStyles[1].pDef;
        data.mR = CurConfigRoleData.mouseStyles[1].rDef;
        data.mS = CurConfigRoleData.mouseStyles[1].sDef;
        return data;
    }
    /// <summary>
    /// 判断该衣服是否为UGC
    /// </summary>
    /// <param name="patternid">衣服id</param>
    /// <returns></returns>
    public bool CurClothesIsUgc(int cloId)
    {
        var clothStyleData = GetClothesById(cloId);
        return !clothStyleData.IsPGC();
    }
    /// <summary>
    /// 判断该面部彩绘是否为UGC
    /// </summary>
    /// <param name="patternid">面部彩绘id</param>
    /// <returns></returns>
    public bool CurPatternIsUgc(int fpId)
    {
        var patternStyleData = GetPatternStylesDataById(fpId);
        return !patternStyleData.IsPGC();
    }
    /// <summary>
    /// 通过DataSubType获取ClassifyType(收藏，AirDrop区分ugc)
    /// </summary>
    /// <param name="dataSubType"></param>
    /// <returns></returns>
    public ClassifyType GetTypeByDataSubType(int dataSubType)
    {
        ClassifyType classifyType = ClassifyType.ugcCloth;
        switch (dataSubType)
        {
            case (int)DataSubType.Clothes:
                classifyType = ClassifyType.ugcCloth;
                break;
            case (int)DataSubType.Patterns:
                classifyType = ClassifyType.ugcPatterns;
                break;
                //TODO:未来补充其他部件
        }
        return classifyType;
    }
    /// <summary>
    /// 通过UGCtype获取同类型的PGCtype
    /// </summary>
    /// <param name="ugctype"></param>
    /// <returns></returns>
    public ClassifyType GetPGCTypeByUGCType(ClassifyType ugctype)
    {
        if (UgcToPgcDic.ContainsKey(ugctype))
        {
            return UgcToPgcDic[ugctype];
        }
        return ClassifyType.none;
    }
    /// <summary>
    /// 替换被ban的UGC物品
    /// </summary>
    /// <param name="roleData"></param>
    /// <returns></returns>
    public bool ReplaceUGC(RoleData roleData, UserInfo userInfo)
    {
        bool hasReplaced = false;
        if (userInfo != null)
        {
            if (userInfo.clothesIsBan == 1)
            {
                roleData.cloId = 1;
                hasReplaced = true;
            }
            if (userInfo.facePaintingIsBan == 1)
            {
                roleData.fpId = 0;
                hasReplaced = true;
            }
        }
        return hasReplaced;
    }
    private bool ReplaceUGCDC(DCUGCItemInfo[] DCUgcInfos, RoleData roleData)
    {
        if (DCUgcInfos == null)
        {
            return false;
        }

        bool hasReplaced = false;
        foreach (var dcInfo in DCUgcInfos)
        {
            if (dcInfo.hasCount > 0) continue; //拥有的DC不做替换

            switch (dcInfo.classifyType)
            {
                case (int)ClassifyType.ugcCloth:
                    if (!string.IsNullOrEmpty(dcInfo.ugcId) && roleData.clothMapId.Equals(dcInfo.ugcId) && CurClothesIsUgc(roleData.cloId))
                    {
                        roleData.cloId = 1;
                        hasReplaced = true;
                    }
                    break;
                case (int)ClassifyType.ugcPatterns:
                    if (!string.IsNullOrEmpty(dcInfo.ugcId) && roleData.ugcFPData.ugcMapId.Equals(dcInfo.ugcId) && CurPatternIsUgc(roleData.fpId))
                    {
                        roleData.fpId = 0;
                        hasReplaced = true;
                    }
                    break;
                //TODO:未来补充其他部件
            }
        }
        return hasReplaced;
    }

    private bool ReplacePGCDC(DCPGCItemInfo[] DCPgcInfos, RoleData roleData)
    {
        if (DCPgcInfos == null)
        {
            return false;
        }

        bool hasReplaced = false;
        foreach (var dcInfo in DCPgcInfos)
        {
            if (dcInfo.hasCount > 0) continue; //拥有的DC不做替换

            switch (dcInfo.classifyType)
            {
                case (int)ClassifyType.headwear:
                    ReplaceToDefPGC(dcInfo.pgcId, 0, ref roleData.hatId, ref hasReplaced);
                    break;
                case (int)ClassifyType.glasses:
                    ReplaceToDefPGC(dcInfo.pgcId, 0, ref roleData.glId, ref hasReplaced);
                    break;
                case (int)ClassifyType.outfits:
                    ReplaceToDefPGC(dcInfo.pgcId, 1, ref roleData.cloId, ref hasReplaced);
                    break;
                case (int)ClassifyType.shoes:
                    ReplaceToDefPGC(dcInfo.pgcId, 1, ref roleData.shoeId, ref hasReplaced);
                    break;
                case (int)ClassifyType.hand:
                    ReplaceToDefPGC(dcInfo.pgcId, 0, ref roleData.hdId, ref hasReplaced);
                    break;
                case (int)ClassifyType.bag:
                    ReplaceToDefPGC(dcInfo.pgcId, 0, ref roleData.bagId, ref hasReplaced);
                    break;
                case (int)ClassifyType.eyes:
                    ReplaceToDefPGC(dcInfo.pgcId, 1, ref roleData.eId, ref hasReplaced);
                    break;
                case (int)ClassifyType.effects:
                    ReplaceToDefPGC(dcInfo.pgcId, 0, ref roleData.effId, ref hasReplaced);
                    break;
                    //TODO:未来补充其他部件
            }
        }
        return hasReplaced;
    }

    private void ReplaceToDefPGC(int refId, int defId, ref int curId, ref bool hasReplaced)
    {
        hasReplaced = curId == refId ? true : hasReplaced;
        curId = curId == refId ? defId : curId;
    }

    //返回是否发生替换
    public bool ReplaceNotOwnedDC(UserInfo userInfo, RoleData roleData)
    {
        //UGC
        bool ugcRep = ReplaceUGCDC(userInfo.dcUgcInfos, roleData);
        //PGC
        bool pgcRep = ReplacePGCDC(userInfo.dcPgcInfos, roleData);
        return ugcRep || pgcRep;
    }

    public List<PGCInfo> GetContainRewardsList(RoleData roleData)
    {
        var pgcList = new List<PGCInfo>();
        var hatData = GetHatStyleDataById(roleData.hatId);
        if (hatData.origin == (int)RoleOriginType.Rewards)
        {
            pgcList.Add(new PGCInfo()
            {
                classifyType = (int)ClassifyType.headwear,
                pgcId = roleData.hatId
            });
        }
        var glassesData = GetGlassesStyleDataById(roleData.glId);
        if (glassesData.origin == (int)RoleOriginType.Rewards)
        {
            pgcList.Add(new PGCInfo()
            {
                classifyType = (int)ClassifyType.glasses,
                pgcId = roleData.glId
            });
        }
        var clothData = GetClothesById(roleData.cloId);
        if (clothData.origin == (int)RoleOriginType.Rewards)
        {
            pgcList.Add(new PGCInfo()
            {
                classifyType = (int)ClassifyType.outfits,
                pgcId = roleData.cloId
            });
        }
        var shoeData = GetShoeStylesDataById(roleData.shoeId);
        if (shoeData.origin == (int)RoleOriginType.Rewards)
        {
            pgcList.Add(new PGCInfo()
            {
                classifyType = (int)ClassifyType.shoes,
                pgcId = roleData.shoeId
            });
        }
        var bagData = GetBagStylesDataById(roleData.bagId);
        if (bagData.origin == (int)RoleOriginType.Rewards)
        {
            pgcList.Add(new PGCInfo()
            {
                classifyType = (int)ClassifyType.bag,
                pgcId = roleData.bagId
            });
        }
        var assessoriesData = GetAccessoriesStylesDataById(roleData.acId);
        if (assessoriesData.origin == (int)RoleOriginType.Rewards)
        {
            pgcList.Add(new PGCInfo()
            {
                classifyType = (int)ClassifyType.accessories,
                pgcId = roleData.acId
            });
        }
        var eyeData = GetEyeStyleDataById(roleData.eId);
        if (eyeData.origin == (int)RoleOriginType.Rewards)
        {
            pgcList.Add(new PGCInfo()
            {
                classifyType = (int)ClassifyType.eyes,
                pgcId = roleData.eId
            });
        }
        var effData = GetEffectStyleDataById(roleData.effId);
        if (effData.origin == (int)RoleOriginType.Rewards)
        {
            pgcList.Add(new PGCInfo()
            {
                classifyType = (int)ClassifyType.effects,
                pgcId = roleData.effId
            });
        }
        return pgcList;
    }

    public List<DCPGCItemInfo> GetContainDCList(RoleData roleData)
    {
        var pgcList = new List<DCPGCItemInfo>();
        var hatData = GetHatStyleDataById(roleData.hatId);
        if (hatData.grading == (int)RoleResGrading.DC)
        {
            pgcList.Add(new DCPGCItemInfo()
            {
                classifyType = (int)ClassifyType.headwear,
                pgcId = roleData.hatId
            });
        }
        var glassesData = GetGlassesStyleDataById(roleData.glId);
        if (glassesData.grading == (int)RoleResGrading.DC)
        {
            pgcList.Add(new DCPGCItemInfo()
            {
                classifyType = (int)ClassifyType.glasses,
                pgcId = roleData.glId
            });
        }
        var clothData = GetClothesById(roleData.cloId);
        if (clothData.grading == (int)RoleResGrading.DC)
        {
            pgcList.Add(new DCPGCItemInfo()
            {
                classifyType = (int)ClassifyType.outfits,
                pgcId = roleData.cloId
            });
        }
        var shoeData = GetShoeStylesDataById(roleData.shoeId);
        if (shoeData.grading == (int)RoleResGrading.DC)
        {
            pgcList.Add(new DCPGCItemInfo()
            {
                classifyType = (int)ClassifyType.shoes,
                pgcId = roleData.shoeId
            });
        }
        var handData = GetHandStyleDataById(roleData.hdId);
        if (handData.grading == (int)RoleResGrading.DC)
        {
            pgcList.Add(new DCPGCItemInfo()
            {
                classifyType = (int)ClassifyType.hand,
                pgcId = roleData.hdId
            });
        }
        var bagData = GetBagStylesDataById(roleData.bagId);
        if (bagData.grading == (int)RoleResGrading.DC)
        {
            pgcList.Add(new DCPGCItemInfo()
            {
                classifyType = (int)ClassifyType.bag,
                pgcId = roleData.bagId
            });
        }
        var effData = GetEffectStyleDataById(roleData.effId);
        if (effData.grading == (int)RoleResGrading.DC)
        {
            pgcList.Add(new DCPGCItemInfo()
            {
                classifyType = (int)ClassifyType.effects,
                pgcId = roleData.effId
            });
        }
        var eData = GetEyeStyleDataById(roleData.eId);
        if (eData.grading == (int)RoleResGrading.DC)
        {
            pgcList.Add(new DCPGCItemInfo()
            {
                classifyType = (int)ClassifyType.eyes,
                pgcId = roleData.eId
            });
        }
        return pgcList;
    }

    //由于OnDestroy调用先后不一致，导致部分OnDestroy去调用时CurConfigRoleData已经为空
    public EyeStyleData GetEyeStyleDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Eye—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.eyeStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("EyeDataNull—EyeId: " + id);
        }
        return data;
    }

    public BrowStyleData GetBrowStyleDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Brow—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.browStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("BrowDataNull—BrowId: " + id);
        }
        return data;
    }

    public MouseStyleData GetMouseStyleDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Mouth—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.mouseStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("MouseDataNull—MouseId: " + id);
        }
        return data;
    }

    //由于OnDestroy调用先后不一致，导致部分OnDestroy去调用时CurConfigRoleData已经为空，对此进行判空处理
    public NoseStyleData GetNoseStyleDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Nose—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.noseStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("NoseDataNull—NoseId: " + id);
        }
        return data;
    }

    public BlushStyleData GetBlusherStyleDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Blush—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.faceStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("BlushDataNull—BlushId: " + id);
        }
        return data;
    }

    public ClothStyleData GetClothesById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Cloth—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.clothes.Find(x => x.id == id);
        if (data == null && id != -1)
        {
            LoggerUtils.LogError("ClothDataNull—ClothId: " + id);
        }
        return data;
    }

    public ClothStyleData GetClothesByTemplateId(int templateId)
    {
        if (CurConfigRoleData == null || templateId == 0)
        {
            LoggerUtils.LogError("UGCCloth—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.clothes.Find(x => x.templateId == templateId);
        if (data == null)
        {
            LoggerUtils.LogError("UGCClothDataNull—UGCClothId: " + templateId);
        }
        return data;
    }

    public RoleIconData GetHairDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Hair—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.hairIcons.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("HairDataNull—HairId: " + id);
        }
        return data;
    }

    public HatStyleData GetHatStyleDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Hat—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.hatStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("HatDataNull—HatId: " + id);
        }
        return data;
    }

    public GlassesStyleData GetGlassesStyleDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Glasses—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.glassesStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("GlassesDataNull—GlassesId: " + id);
        }
        return data;
    }
    public AccessoriesStyleData GetAccessoriesStylesDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Accessories—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.accessoriesStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("AccessoriesDataNull—AccessoriesId: " + id);
        }
        return data;
    }
    public ShoeStyleData GetShoeStylesDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Shoe—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.shoesStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("ShoeDataNull—ShoeId: " + id);
        }
        return data;
    }
    public BagStyleData GetBagStylesDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Bag—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.bagStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("BagDataNull—BagId: " + id);
        }
        return data;
    }
    public PatternStyleData GetPatternStylesDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Pattern—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.patternStyles.Find(x => x.id == id);
        if (data == null && id != -1)
        {
            LoggerUtils.LogError("PatternDataNull—PatternId: " + id);
        }
        return data;
    }
    public PatternStyleData GetPatternByTemplateId(int templateId)
    {
        if (CurConfigRoleData == null || templateId == 0)
        {
            LoggerUtils.LogError("GetPattern—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.patternStyles.Find(x => x.templateId == templateId);
        if (data == null)
        {
            LoggerUtils.LogError("UGCClothDataNull—UGCPatternTemplateId: " + templateId);
        }
        return data;
    }
    public HandStyleData GetHandStyleDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Hand—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.handStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("HandDataNull—HandId: " + id);
        }
        return data;
    }
    public SpecialStyleData GetSpecialStylesDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Special—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.specialStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("SpecialDataNull—SpecialId: " + id);
        }
        return data;
    }

    public EffectStyleData GetEffectStyleDataById(int id)
    {
        if (CurConfigRoleData == null)
        {
            LoggerUtils.LogError("Effect—CurConfigRoleDataNull");
            return null;
        }
        var data = CurConfigRoleData.effectStyles.Find(x => x.id == id);
        if (data == null)
        {
            LoggerUtils.LogError("EffectDataNull—EffectId: " + id);
        }
        return data;
    }
}
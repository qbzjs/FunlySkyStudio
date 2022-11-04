using System;
using Newtonsoft.Json;

[Serializable]
public struct UGCClothItemData
{
    public int tId;
    public string cUrl; //贴图Url
    public string cMapId; //UGC衣服的MapId，用于内容审核
    public string cCover; //UGC衣服的封面图
    public string cJson; //衣服JsonUrl
    public int isDc;
    public string dCId;
    public string walAdd;//钱包地址
    public string actId;//dcInfo数据
    public int classifyType; //角色部件分类，同ClassifyType枚举值
    public int pgcId; //配置表内pgc资源id，100000-200000
    public int nftType; //0:dc 1:airdrop
    public int dataSubType;
}

/// <summary>
/// Author:Shaocheng
/// Description:UGC衣服道具comp
/// Date: 2022-4-21 15:03:11
/// </summary>
public class UGCClothItemComponent : IComponent
{
    public int templateId;
    public string clothesUrl; //贴图Url
    public string clothesJson = ""; //衣服JsonUrl
    public string clothMapId; //UGC衣服的MapId，用于内容审核
    public string clothCover; //UGC衣服的封面图
    public string imageJson = ""; // 人物形象(主要用于透传给联机端，不记录到 Json 文件)
    public string clothesId = ""; // UGC 部件 Id (主要用于透传给联机端，不记录到 Json 文件）！！1.47增加面部彩绘后可废弃
    public int isDc;//是否是dc
    public string dcId;
    public string walletAddress = "";
    public string budActId = "";
    public int classifyType; //角色部件分类，同ClassifyType枚举值
    public int pgcId; //配置表内pgc资源id，100000-200000
    public int dataSubType;
    public IComponent Clone()
    {
        UGCClothItemComponent component = new UGCClothItemComponent();
        component.templateId = templateId;
        component.clothesUrl = clothesUrl;
        component.clothMapId = clothMapId;
        component.clothCover = clothCover;
        component.clothesJson = clothesJson;
        component.isDc = isDc;
        component.dcId = dcId;
        component.walletAddress = walletAddress;
        component.budActId = budActId;
        component.classifyType = classifyType;
        component.pgcId = pgcId;
        component.dataSubType = dataSubType;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        UGCClothItemData data = new UGCClothItemData
        {
            tId = templateId,
            cUrl = clothesUrl,
            cMapId = clothMapId,
            cCover = clothCover,
            cJson = clothesJson,
            isDc = isDc,
            dCId = dcId,
            walAdd = walletAddress,
            actId = budActId,
            classifyType = classifyType,
            pgcId = pgcId,
            dataSubType = dataSubType,
        };

        return new BehaviorKV
        {
            k = (int) BehaviorKey.UGCClothItem,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
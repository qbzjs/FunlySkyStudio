using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Author:MeiMei
/// Description:随机人物形象
/// </summary>
public class RandomData
{
    public List<int> hairIcons;
    public List<int> clothesIcons;
    public List<int> shoeIcons;
}
public class FaceMatch
{
    public int eId = 4;
    public int bId = 7;
    public int nId = 1;
    public int mId = 3;
    public int bluId = 3;

    public void Copy(RoleData rData)
    {
        rData.eId = eId;
        rData.bId = bId;
        rData.mId = mId;
        rData.nId = nId;
        rData.bluId = bluId;
    }
}
public class OutfitsMatch
{
    public int cloId = 1;
    public int shoeId = 28;
    public int hatId = 0;
    public int glId = 0;
    public void Copy(RoleData rData)
    {
        rData.cloId = cloId;
        rData.shoeId = shoeId;
        rData.hatId = hatId;
        rData.glId = glId;
    }
}
public class RoleConfigRandomData
{
    public RandomData WomanRandomData;
    public List<FaceMatch> FaceMatch;
    public List<OutfitsMatch> OutfitsMatch;
    public List<string> hairColors;
    public List<string> skinColors;
    public List<string> blackSkinColors;
    public RandomData ManRandomData;
    public RandomData QueenRandomData;
}
public enum RandomType
{
    Match = 0,
    Random = 1,
}
public enum SkinRandomType
{
    Black = 0,
    Other = 1,
}
public enum Gender
{
    Man = 1,
    Woman = 2,
    Queen = 3,
}
public class RoleDataRandomMgr : CInstance<RoleDataRandomMgr>
{
    public RoleConfigRandomData randomConfigData;
    public int _sex = 2;
    public int Sex
    {
        get
        {
            return _sex;
        }
        set
        {
            if (value <= 0 || value > 3)
            {
                value = 2;
            }
            _sex = value;
        }
    }
    //概率数组
    int[] clothArea = new int[] { 3, 7 };
    int[] skinArea = new int[] { 1, 9 };

    public void LoadRandomConfig()
    {
        randomConfigData = ResManager.Inst.LoadJsonRes<RoleConfigRandomData>("Configs/RoleData/RoleConfigDataRandom");
    }
    /// <summary>
    /// 获取概率随机数
    /// </summary>
    /// <returns></returns>
    public int GetRandomType(int[] area)
    {
        int total = 0;
        int t = 0;
        for (int i = 0; i < area.Length; i++)
        {
            total += area[i];
        }
        int r = Random.Range(0, total);
        for (int i = 0; i < area.Length; i++)
        {
            t += area[i];
            if (r < t)
            {
                return i;
            }
        }
        return 0;
    }
    public RoleData GetRandomRoleData()
    {
        if (randomConfigData == null)
        {
            LoadRandomConfig();
        }
        Sex = GameManager.Inst.ugcUserInfo != null ? GameManager.Inst.ugcUserInfo.gender : 2;
        LoggerUtils.Log("GENDER:" + GameManager.Inst.ugcUserInfo.gender);
        RoleData data = new RoleData();
        FaceMatch faceData = GetRandomFaceData();
        faceData.Copy(data);
        data.hId = GetRandomHair(Sex);
        data.hCr = GetColorString(randomConfigData.hairColors);
        data.bCr = data.hCr;
        SkinRandomType skinRandomType = (SkinRandomType)GetRandomType(skinArea);
        data.sCr = GetSkinColor(skinRandomType);

        RandomType type = (RandomType)GetRandomType(clothArea);
        switch (type)
        {
            case RandomType.Random:
                data.cloId = GetRandomCloth(Sex);
                data.shoeId = GetRandomShoe(Sex);
                break;
            case RandomType.Match:
                OutfitsMatch matchData = GetOutfitsMatch();
                matchData.Copy(data);
                break;
        }
        return data;
    }
    public FaceMatch GetRandomFaceData()
    {
        FaceMatch data = new FaceMatch();
        int index = Random.Range(0, randomConfigData.FaceMatch.Count);
        data = randomConfigData.FaceMatch[index];
        return data;
    }
    public int GetRandomHair(int sex)
    {
        int hId = 1;
        int index = 0;
        switch (sex)
        {
            case (int)Gender.Man:
                index = Random.Range(0, randomConfigData.ManRandomData.hairIcons.Count);
                hId = randomConfigData.ManRandomData.hairIcons[index];
                break;
            case (int)Gender.Woman:
                index = Random.Range(0, randomConfigData.WomanRandomData.hairIcons.Count);
                hId = randomConfigData.WomanRandomData.hairIcons[index];
                break;
            case (int)Gender.Queen:
                index = Random.Range(0, randomConfigData.QueenRandomData.hairIcons.Count);
                hId = randomConfigData.QueenRandomData.hairIcons[index];
                break;
        }
        return hId;
    }
    public int GetRandomShoe(int sex)
    {
        int shoeId = 28;
        int index = 0;
        switch (sex)
        {
            case (int)Gender.Man:
                index = Random.Range(0, randomConfigData.ManRandomData.shoeIcons.Count);
                shoeId = randomConfigData.ManRandomData.shoeIcons[index];
                break;
            case (int)Gender.Woman:
                index = Random.Range(0, randomConfigData.WomanRandomData.shoeIcons.Count);
                shoeId = randomConfigData.WomanRandomData.shoeIcons[index];
                break;
            case (int)Gender.Queen:
                index = Random.Range(0, randomConfigData.QueenRandomData.shoeIcons.Count);
                shoeId = randomConfigData.QueenRandomData.shoeIcons[index];
                break;
        }
        return shoeId;
    }
    public int GetRandomCloth(int sex)
    {
        int cloId = 1;
        int index = 0;
        switch (sex)
        {
            case (int)Gender.Man:
                index = Random.Range(0, randomConfigData.ManRandomData.clothesIcons.Count);
                cloId = randomConfigData.ManRandomData.clothesIcons[index];
                break;
            case (int)Gender.Woman:
                index = Random.Range(0, randomConfigData.WomanRandomData.clothesIcons.Count);
                cloId = randomConfigData.WomanRandomData.clothesIcons[index];
                break;
            case (int)Gender.Queen:
                index = Random.Range(0, randomConfigData.QueenRandomData.clothesIcons.Count);
                cloId = randomConfigData.QueenRandomData.clothesIcons[index];
                break;
        }
        return cloId;
    }
    public OutfitsMatch GetOutfitsMatch()
    {
        OutfitsMatch data = new OutfitsMatch();
        int index = Random.Range(0, randomConfigData.OutfitsMatch.Count);
        data = randomConfigData.OutfitsMatch[index];
        return data;
    }
    public string GetColorString(List<string> colors)
    {
        int index = Random.Range(0, colors.Count);
        return colors[index];
    }
    public string GetSkinColor(SkinRandomType skinType)
    {
        int index = 0;
        string skinColor = "#FFDDDC";
        switch (skinType)
        {
            case SkinRandomType.Black:
                index = Random.Range(0, randomConfigData.blackSkinColors.Count);
                skinColor = randomConfigData.blackSkinColors[index];
                break;
            case SkinRandomType.Other:
                index = Random.Range(0, randomConfigData.skinColors.Count);
                skinColor = randomConfigData.skinColors[index];
                break;
        }
        return skinColor;
    }

    public override void Release()
    {
        base.Release();
    }
}

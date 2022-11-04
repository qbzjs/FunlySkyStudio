using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RTG;
using UnityEngine;
using UnityEngine.Rendering;
[Serializable]
public class TextData
{
    public string pos;
    public string rot;
    public string sizeDelta;
    public string color;
    public string content;
    public int hierarchy;
}
[Serializable]
public class PhotoData
{
    public string pos;
    public string rot;
    public string sizeDelta;
    public string photoUrl;
    public int hierarchy;
}


public class UGCClothesDataManager:CInstance<UGCClothesDataManager>
{
    public Dictionary<int,string> clothesConfigs = new Dictionary<int, string>()
    {
        { 1,"UGCClothes_1"},
        { 2,"UGCClothes_2"},
        { 3,"UGCClothes_3"},
        { 4,"UGCClothes_4"},
        { 1001,"UGCClothes_1001"},
        { 10001, "UGCMaterial_10001"}
    };

    public Dictionary<int, Vector3> ugcResShotDistance = new Dictionary<int, Vector3>()
    {
        { 1,new Vector3(0,0.3f,0.65f)},
        { 2,new Vector3(0,0.28f,0.72f)},
        { 3,new Vector3(0,0.27f,0.74f)},
        { 4,new Vector3(0,0.36f,0.61f)},
        { 1001,new Vector3(0,0.36f,0.7f)},
        { 10001, new Vector3(0,0.3f,0.65f)}
    };
    public SaveUGCClothesData saveClothesData = new SaveUGCClothesData();
    private Dictionary<int,List<UGCData>> allUGCResDatas = new Dictionary<int, List<UGCData>>();
    private Dictionary<int, Dictionary<Vector2Int, Color>> allClothesParts = new Dictionary<int, Dictionary<Vector2Int, Color>>();
    private Dictionary<int, List<Vector2Int>> allInoperableArea = new Dictionary<int, List<Vector2Int>>();
    public Action setHierarchy;
    public const string faceName = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 Head/ugc_face";
    public void Init()
    {
        allUGCResDatas.Clear();
        foreach (var keyValue in clothesConfigs)
        {
            var data = ResManager.Inst.LoadJsonRes<List<UGCData>>("Configs/UGCRes/" + keyValue.Value);
            allUGCResDatas.Add(keyValue.Key, data);
        }
    }

    public void InitDataByCreate(int templateId)
    {
        saveClothesData = new SaveUGCClothesData();
        if (allUGCResDatas.ContainsKey(templateId)) {
            var ugcDataList = allUGCResDatas[templateId];
            saveClothesData.Id = templateId;
            saveClothesData.parts = new List<SaveUGCClothesPartsData>();
            for (int i = 0; i < ugcDataList.Count; i++)
            {
                var ugcData = ugcDataList[i];
                var partData = new SaveUGCClothesPartsData();
                partData.uType = ugcData.ugcType;
                partData.pixels = GetDefaultPixelData();
                saveClothesData.parts.Add(partData);
            }
        }
    }

    public void InitDataByCreate(string content)
    {
        saveClothesData = JsonConvert.DeserializeObject<SaveUGCClothesData>(content);
    }

    private List<UGCPixelData> GetDefaultPixelData()
    {
        var tempList = new List<UGCPixelData>();
        for (int i = 0; i < 32; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                UGCPixelData data = new UGCPixelData();
                data.p = DataUtils.Vector2IntToString(new Vector2Int(i, j));
                data.col = DataUtils.ColorToString(Color.white);
                tempList.Add(data);
            }
        }
        return tempList;
    }

    public Dictionary<int, Dictionary<Vector2Int, Color>> GetClothesPartDicByID(int id)
    {
        allClothesParts.Clear();
        var data = saveClothesData;
        for (int i = 0; i < data.parts.Count; i++)
        {
            Dictionary <Vector2Int, Color > vColors = new Dictionary<Vector2Int, Color>();
            for (int j = 0; j < data.parts[i].pixels.Count; j++)
            {
                var pos = DataUtils.DeSerializeVector2Int(data.parts[i].pixels[j].p);
                var col = DataUtils.DeSerializeColor(data.parts[i].pixels[j].col);
                vColors.Add(pos,col);
            }
            allClothesParts.Add(data.parts[i].uType, vColors);
        }
        return allClothesParts;
    }

    public Dictionary<int, List<Vector2Int>> GetInoperableAreaByID(int id)
    {
        allInoperableArea.Clear();
        var data = allUGCResDatas[id];
        for (int i = 0; i < data.Count; i++)
        {
            var inoperableArea = new List<Vector2Int>();
            for (int j = 0; j < data[i].inoperableArea.Count; j++)
            {
                var pos = DataUtils.DeSerializeVector2Int(data[i].inoperableArea[j]);
                inoperableArea.Add(pos);
            }
            allInoperableArea.Add(data[i].ugcType, inoperableArea);
        }
        return allInoperableArea;
    }

    private Dictionary<int, List<TextData>> textDataDic = new Dictionary<int, List<TextData>>();
    private Dictionary<int, List<PhotoData>> photoDataDic = new Dictionary<int, List<PhotoData>>();
    public void CloneOrgSaveData()
    {
        textDataDic.Clear();
        for (int i = 0; i < saveClothesData.parts.Count; i++)
        {
            if (saveClothesData.parts[i].textData == null)
            {
                continue;
            }
            List<TextData> tDatas = new List<TextData>();
            for (int j = 0; j < saveClothesData.parts[i].textData.Count; j++)
            {
                TextData tData = new TextData();
                tData.pos = saveClothesData.parts[i].textData[j].pos;
                tData.rot = saveClothesData.parts[i].textData[j].rot;
                tData.sizeDelta = saveClothesData.parts[i].textData[j].sizeDelta;
                tData.color = saveClothesData.parts[i].textData[j].color;
                tData.content = saveClothesData.parts[i].textData[j].content;
                tData.hierarchy = saveClothesData.parts[i].textData[j].hierarchy;
                tDatas.Add(tData);
            }
            textDataDic.Add(i, tDatas);
        }
        
        
        photoDataDic.Clear();
        for (int i = 0; i < saveClothesData.parts.Count; i++)
        {
            if (saveClothesData.parts[i].photoData == null)
            {
                continue;
            }
            List<PhotoData> pDatas = new List<PhotoData>();
            for (int j = 0; j < saveClothesData.parts[i].photoData.Count; j++)
            {
                PhotoData pData = new PhotoData();
                pData.pos = saveClothesData.parts[i].photoData[j].pos;
                pData.rot = saveClothesData.parts[i].photoData[j].rot;
                pData.sizeDelta = saveClothesData.parts[i].photoData[j].sizeDelta;
                pData.photoUrl = saveClothesData.parts[i].photoData[j].photoUrl;
                pData.hierarchy = saveClothesData.parts[i].photoData[j].hierarchy;
                pDatas.Add(pData);
            }
            photoDataDic.Add(i, pDatas);
        }
    }

    public List<TextData> GetTextData(int index)
    {
        if (textDataDic.ContainsKey(index))
        {
            return textDataDic[index]; 
        }
        return new List<TextData>();
    }

    public List<PhotoData> GetPhotoData(int index)
    {
        if (photoDataDic.ContainsKey(index))
        {
            return photoDataDic[index]; 
        }
        return new List<PhotoData>();
    }


    public Dictionary<int, List<PhotoData>> LoadingPhotoData()
    {
        var data = saveClothesData;
        Dictionary<int, List<PhotoData>> pDic = new Dictionary<int, List<PhotoData>>();
        for (int i = 0; i < data.parts.Count; i++)
        {
            if (data.parts[i].photoData == null)
            {
                continue;
            }
            List<PhotoData> pDatas = new List<PhotoData>();
            for (int j = 0; j < data.parts[i].photoData.Count; j++)
            {
                PhotoData pData = new PhotoData();
                pData.pos = data.parts[i].photoData[j].pos;
                pData.rot = data.parts[i].photoData[j].rot;
                pData.sizeDelta = data.parts[i].photoData[j].sizeDelta;
                pData.photoUrl = data.parts[i].photoData[j].photoUrl;
                pDatas.Add(pData);
            }
            pDic.Add(i, pDatas);
        }
        return pDic;
    }

    public SaveUGCClothesData SaveAllClothesData()
    {
        setHierarchy?.Invoke();
        SaveUGCClothesData data = new SaveUGCClothesData();
        data.Id = saveClothesData.Id;
        data.parts = new List<SaveUGCClothesPartsData>();
        foreach (var parts in allClothesParts)
        {
            SaveUGCClothesPartsData partData = new SaveUGCClothesPartsData();
            partData.uType = parts.Key;
            List<UGCPixelData> pixels = new List<UGCPixelData>();
            List<TextData> tData = UGCClothesTextManager.Inst.GetPartTextDatas(parts.Key);
            List<PhotoData> pData = UGCClothesPhotoManager.Inst.GetPartPhotoDatas(parts.Key);
            foreach (var pixel in parts.Value)
            {
                UGCPixelData pixelData = new UGCPixelData();
                pixelData.col = DataUtils.ColorRGBAToString(pixel.Value);
                pixelData.p = DataUtils.Vector2IntToString(pixel.Key);
                pixels.Add(pixelData);
            }
            partData.pixels = pixels;
            partData.textData = tData;
            partData.photoData = pData;
            data.parts.Add(partData);
        }
        return data;
    }



    public List<UGCData> GetConfigClothesDataByID(int id)
    {
        return allUGCResDatas[id];
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UGCClothesData
{
}

public enum UGC_CLOTH_TYPE
{
    CLOTH = 1,
}
public enum PaintType
{
    Brush,
    OilDrum,
    Scissors,//剪刀
    Text,
    Photo,
}
public enum UGCClothesType
{
    LeftSleeve = 1,
    RightSleeve = 2,
    FrontClothes = 3,
    BackClothes = 4,
    FrontTrousers = 5,
    BackTrousers = 6
}
public class UGCData
{
    public int ugcType;
    public int rotAngle;
    public string partsName;
    public string iconSpriteName;
    public string maskSpriteName;
    public string meshSpriteName;
    public string faceMatTextureName;
    public List<string> inoperableArea;
}


public class SaveUGCClothesData
{
    public int Id;//不同服饰种类ID
    public List<SaveUGCClothesPartsData> parts;
}

[Serializable]
public class SaveUGCClothesPartsData
{
    public int uType;//服饰部件类型Type
    public List<UGCPixelData> pixels;
    public List<TextData> textData;
    public List<PhotoData> photoData;
}

[Serializable]
public struct UGCPixelData
{
    public string p;
    public string col;
}

public struct UGCClothesPixelData
{
    public Vector2Int pos;
    public Color color;
}


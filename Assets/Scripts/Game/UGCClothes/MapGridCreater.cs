using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Amazon.S3.Model;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using Debug = UnityEngine.Debug;

public class MapGridCreater : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    public enum TextureType
    {
        Cloth,
        Cloth_Alpha,
        Material
    }
    public Material XBRMaterial;
    public RawImage ShowRawImage;
    public RawImage DrawRawImage;
    public Transform lineParent;
    public Image linePrefab;
    public int GridCountX = 32;
    public int GridCountY = 32;
    public float lineHeight = 3;
    public Vector2Int GenerateTextureSize;
    public Vector2Int FinalTextureSize;
    public Action OnPointerDownEvent;
    public Action<Dictionary<Vector2Int, Color>, Dictionary<Vector2Int, Color>> OnAddRecordEvent;
    
    private RenderTexture finalTex;
    private Material clothesPartMat;
    private Material clonePartMat;
    private int pixelOffset;
    private float gridOffset;
    private Vector3 gridOffsetX;
    private Vector3 gridOffsetY;
    private Vector3 leftDownPosition;

    private Vector3 lineRotation = new Vector3(0,0,90);
    private bool isScissors;
    private Vector2 mapRowSize;
    private Vector2 mapColSize;

    private Texture2D genTexture;
    private RenderTexture filterTexture;
    private Texture2D genAlphaTexture;
    private RenderTexture filterAlphaTexture;
    private RectTransform rawRectTransform;
    private bool isStartDraw = false;
    private int texPass = 4;
    private Dictionary<Vector2Int, Color> partsPixels;
    private List<Vector2Int> inoperableArea;
    public ClotheDrawMode curMode = ClotheDrawMode.Normal;
    //与背景相同的颜色
    private Color backgroundColor = new Color(0.9372f, 0.9372f, 0.9372f, 0);
    private RawImage dyRawImage;
    private Dictionary<Vector2Int, Color> beforeDrowGridPairs = new Dictionary<Vector2Int, Color>();
    private Dictionary<Vector2Int, Color> afterDrowGridPairs = new Dictionary<Vector2Int, Color>();
    // Start is called before the first frame update
    void Awake()
    {
        Vector3 mapSize = DrawRawImage.rectTransform.sizeDelta;
        leftDownPosition = DrawRawImage.transform.localPosition - mapSize / 2;
        float gridSizeX = mapSize.x / GridCountX;
        float gridSizeY = mapSize.y / GridCountX;
        if (gridSizeX != gridSizeY)
        {
            Debug.LogError("RawImage is not");
        }

        pixelOffset = GenerateTextureSize.x / GridCountX;
        gridOffset = gridSizeX;
        gridOffsetX = new Vector3(gridOffset, 0,0);
        gridOffsetY = new Vector3(0, gridOffset, 0);
        mapRowSize = new Vector2(mapSize.x,lineHeight);
        mapColSize = new Vector2(mapSize.y, lineHeight);
        rawRectTransform = DrawRawImage.GetComponent<RectTransform>();
        DrawMapGrid();
    }


    public UGCClothTexture2D GenerateNoFilterTexture2D(Dictionary<Vector2Int, Color> datas)
    {

        UGCClothTexture2D tex = new UGCClothTexture2D();

        tex.tex = new Texture2D(GenerateTextureSize.x, GenerateTextureSize.y, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.texAlpha = new Texture2D(GenerateTextureSize.x, GenerateTextureSize.y, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        foreach (var keyValue in datas)
        {
            DrawSingleGrid(keyValue.Key, keyValue.Value, tex.tex, tex.texAlpha);
        }
        tex.tex.Apply();
        tex.texAlpha.Apply();
        return tex;
    }

    public void SetPartsTexture(GameObject parts,RenderTexture rt, TextureType textureType)
    {
        switch (textureType)
        {
            case TextureType.Cloth:
                var partsMat = parts.GetComponent<SkinnedMeshRenderer>().material;
                partsMat.SetTexture("_MainTex", rt);
                break;
            case TextureType.Cloth_Alpha:
                partsMat = parts.GetComponent<SkinnedMeshRenderer>().material;
                partsMat.SetTexture("_opacity_texmask", rt);
                break;
            case TextureType.Material:
                partsMat = parts.GetComponent<Renderer>().material;
                partsMat.SetTexture("_MainTex", rt);
                break;
        }
        
    }

    public void SetRawImage(RawImage rImage)
    {
        dyRawImage = rImage;
    }

    public void ChangeClothesPart(Texture2D tex,RenderTexture rt,RenderTexture fTex, Texture2D alphaTex, RenderTexture alphaRt, GameObject part,GameObject clonePart,List<Vector2Int> inoperable, Dictionary<Vector2Int, Color> datas)
    {
        inoperableArea = inoperable;
        partsPixels = datas;
        genTexture = tex;
        genAlphaTexture = alphaTex;
        filterAlphaTexture = alphaRt;
        filterTexture = rt;
        finalTex = fTex;
        clothesPartMat = part.GetComponent<Renderer>().material;
        if(clonePart != null)
        {
            clonePartMat = clonePart.GetComponent<Renderer>().material;
        }
        AbtuAliasing();
    }

    private void DrawMapGrid()
    {
        for (int x = 0; x < GridCountX + 1; x++)
        {
            var lineGo = GameObject.Instantiate(linePrefab,lineParent);
            lineGo.rectTransform.anchoredPosition = leftDownPosition + gridOffsetX * x;
            lineGo.rectTransform.sizeDelta = mapRowSize;
            lineGo.transform.localEulerAngles = lineRotation;
            lineGo.gameObject.SetActive(true);
        }

        for (int y = 0; y < GridCountY + 1; y++)
        {
            var lineGo = GameObject.Instantiate(linePrefab, lineParent);
            lineGo.rectTransform.anchoredPosition = leftDownPosition + gridOffsetY * y;
            lineGo.rectTransform.sizeDelta = mapColSize;
            lineGo.transform.localEulerAngles = Vector3.zero;
            lineGo.gameObject.SetActive(true);
        }
    }


    private void SetFullPixel()
    {
        Vector2Int pixel = Vector2Int.zero;
        for (int i = 0; i < GridCountX; i++)
        {
            for (int j = 0; j < GridCountY; j++)
            {
                pixel.x = i;
                pixel.y = j;
                if (IsCanDrawArea(pixel))
                {
                    SetPartsPixel(pixel, PaintTool.Current.pColor);
                    DrawSingleGrid(pixel, PaintTool.Current.pColor, genTexture,genAlphaTexture);
                }
            }
        }
        genTexture.Apply();
        genAlphaTexture.Apply();
        AbtuAliasing();
    }

    //仅工具使用
    public void SetInoperableArea(Dictionary<Vector2Int,Color> inoArea)
    {
        foreach (var keyValuePair in inoArea)
        {
            partsPixels[keyValuePair.Key] = keyValuePair.Value;
            DrawSingleGrid(keyValuePair.Key, keyValuePair.Value, genTexture,genAlphaTexture);
        }
        genTexture.Apply();
        genAlphaTexture.Apply();
        AbtuAliasing();
    }

    private void DrawSingleGrid(Vector2Int textureCoordinate, Color col,Texture2D tex2D, Texture2D tex2DAlpha)
    {
       
        for (int i = 0; i < pixelOffset; i++)
        {
            for (int j = 0; j < pixelOffset; j++)
            {
                if (col.a == 0)
                {
                    col = Color.black;
                    tex2DAlpha.SetPixel(pixelOffset * textureCoordinate.x + i, pixelOffset * textureCoordinate.y + j, col);
                    col = backgroundColor;
                    tex2D.SetPixel(pixelOffset * textureCoordinate.x + i, pixelOffset * textureCoordinate.y + j, col);
                }
                else {
                    tex2D.SetPixel(pixelOffset * textureCoordinate.x + i, pixelOffset * textureCoordinate.y + j, col);
                    col = Color.white;
                    tex2DAlpha.SetPixel(pixelOffset * textureCoordinate.x + i, pixelOffset * textureCoordinate.y + j, col);
                }

                
            }
        }
    }

    private Vector2Int GetCurrentPixelPosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (rawRectTransform, Input.mousePosition, Camera.main, out var localPoint);

        var textureCoordinate = new Vector2Int
        (
            Mathf.FloorToInt((localPoint.x + DrawRawImage.rectTransform.sizeDelta.x / 2f)/ gridOffset),
            Mathf.FloorToInt((localPoint.y + DrawRawImage.rectTransform.sizeDelta.y / 2f) / gridOffset)
        );
        return textureCoordinate; 
    }
    
    private void Update()
    {
        if (isStartDraw)
        {
            var curPixel = GetCurrentPixelPosition();
            if (IsCanDrawArea(curPixel))
            {
                OnDrawPaint(curPixel);
            }
        }
    }

    private bool IsCanDrawArea(Vector2Int pixel)
    {
        if (pixel.x >= 0 && pixel.x < GridCountX 
            && pixel.y >= 0 && pixel.y < GridCountY 
            && !inoperableArea.Contains(pixel))
            return true;
        return false;
    }
    public void OnUndoSetGrid(Dictionary<Vector2Int, Color> undoPairs)
    {
        
        if (undoPairs.Count>0)
        {
            foreach (var item in undoPairs)
            {
                partsPixels[item.Key] = item.Value;
                DrawSingleGrid(item.Key, item.Value, genTexture , genAlphaTexture);
            }
            genTexture.Apply();
            genAlphaTexture.Apply();
            AbtuAliasing();
        }
      
    }
    private void OnDrawPaint(Vector2Int textureCoordinate)
    {
        Color color = PaintTool.Current.pColor;
        if (isScissors)
        {
            color = backgroundColor;
        }
        SetPartsPixel(textureCoordinate, color);
        DrawSingleGrid(textureCoordinate , color, genTexture , genAlphaTexture);
        if (curMode == ClotheDrawMode.Mirror)
        {
            Vector2Int mirrorTarget = GetMirrorPos(textureCoordinate);

            SetPartsPixel(mirrorTarget, color);
            DrawSingleGrid(mirrorTarget, color, genTexture, genAlphaTexture);
          
        }
        genTexture.Apply();
        genAlphaTexture.Apply();
        AbtuAliasing();
        
    }
    private void SetPartsPixel(Vector2Int textureCoordinate,Color col) {
        if (!beforeDrowGridPairs.ContainsKey(textureCoordinate))
        {
            beforeDrowGridPairs.Add(textureCoordinate, partsPixels[textureCoordinate]);
        }
        partsPixels[textureCoordinate] = col;
        if (!afterDrowGridPairs.ContainsKey(textureCoordinate))
        {
            afterDrowGridPairs.Add(textureCoordinate, partsPixels[textureCoordinate]);
        }
       
    }

    public RenderTexture GenerateFilterTexture(Texture2D tex)
    {
        RenderTexture filterRenderTexture = new RenderTexture((int)(GenerateTextureSize.x * texPass), (int)(GenerateTextureSize.y * texPass), 0, RenderTextureFormat.ARGB32);
        filterRenderTexture.Create();
        UpdateFilterTexture(tex,ref filterRenderTexture);
        return filterRenderTexture;
    }

    public RenderTexture GenerateFinalTexture()
    {
        RenderTexture filterRenderTexture = new RenderTexture(FinalTextureSize.x, FinalTextureSize.y, 0, RenderTextureFormat.ARGB32);
        filterRenderTexture.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
        filterRenderTexture.Create();
        return filterRenderTexture;
    }


    public void UpdateFilterTexture(Texture2D tex,ref RenderTexture rt)
    {
        XBRMaterial.SetVector("texture_size", new Vector4(GenerateTextureSize.x, GenerateTextureSize.y, 0, 0));
        XBRMaterial.SetTexture("decal", tex);
        XBRMaterial.SetTexture("_BackgroundTexture", tex);
        XBRMaterial.SetTexture("_MainTex", tex);
        Graphics.Blit(tex, rt, XBRMaterial);
    }

    private void AbtuAliasing()
    {
        DrawRawImage.texture = genTexture;
        UpdateFilterTexture(genTexture,ref filterTexture);
        UpdateFilterTexture(genAlphaTexture, ref filterAlphaTexture);
        if (dyRawImage != null)
        {
            dyRawImage.texture = filterTexture;
        }
        ShowRawImage.texture = finalTex;
        clothesPartMat.SetTexture("_MainTex", finalTex);
        clothesPartMat.SetTexture("_opacity_texmask", filterAlphaTexture);
        if(clonePartMat != null)
        {
            clonePartMat.SetTexture("_MainTex", finalTex);
            clonePartMat.SetTexture("_opacity_texmask", filterAlphaTexture);

        }
    }

    private bool InCantClick()
    {
        if (PaintTool.Current.pType == PaintType.Text 
            || PaintTool.Current.pType == PaintType.Photo)
        {
            return true;
        }
        return false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (InCantClick()) { return; }
        if (eventData.pointerId == 0)
        {
            UGCClothesInputReceiver.Inst.enabled = false;
            OnPointerDownEvent?.Invoke();
            isScissors = false;
            switch (PaintTool.Current.pType)
            {
                case PaintType.Brush:
                    isStartDraw = true;
                    break;
                case PaintType.OilDrum:
                    OnOilDrumPointerDown();
                    break;
                case PaintType.Scissors:
                    isStartDraw = true;
                    isScissors = true;
                    break;
            }
        }
        else if(eventData.pointerId == 1) {
            if (isStartDraw)
            {
                isStartDraw = false;
                AddRecord();
               
            }
            if (!UGCClothesInputReceiver.Inst.enabled)
            {
                UGCClothesInputReceiver.Inst.enabled = true;
            }
            
        }
    }
    
    private void ClearDrawGridPairs()
    {
        beforeDrowGridPairs.Clear();
        afterDrowGridPairs.Clear();
    }
    private void OnOilDrumPointerDown()
    {
        var curPixel = GetCurrentPixelPosition();
        if (IsCanDrawArea(curPixel))
        {
            SetFullPixel();
            AddRecord();
       
            AbtuAliasing();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (InCantClick()) { return; }
        UGCClothesInputReceiver.Inst.enabled = false;

        if (isStartDraw)
        {
            AddRecord();
      
            isStartDraw = false;
        }
    }
    private Vec2Int GetMirrorPos(Vec2Int vec) {
        Vec2Int tar = vec;
        if (tar.x > GridCountX)
        {
            tar.x -= tar.x - GridCountX/2-1;
        }
        else if(tar.x < GridCountX)
        {
            tar.x += (GridCountX/2 - tar.x)*2-1;
        }

        return tar;
    }
   
    public void AddRecord()
    {
        if (beforeDrowGridPairs.Count>0|| afterDrowGridPairs.Count>0)
        {
            OnAddRecordEvent?.Invoke(beforeDrowGridPairs, afterDrowGridPairs);
            ClearDrawGridPairs();
        }
        

    }
    
}

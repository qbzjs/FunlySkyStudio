using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DG.Tweening;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
public class PaintTool
{
    private static PaintTool cur;

    public static PaintTool Current => cur ?? (cur = new PaintTool());
    public PaintType pType { private set; get; }
    public Color pColor { private set; get; }
    public CommonColorToggleItem PgcCurItem;//选中的PGC颜色
    public CommonColorToggleItem UgcCurItem;//选中的UGC颜色
    public ClotheDrawMode curMode { private set; get; }
    public void SetPaintType(PaintType type)
    {
        pType = type;
    }
    public void SetCurMode(ClotheDrawMode mode)
    {
        curMode = mode;
    }
    public void SetColor(Color col)
    {
        pColor = col;
    }

    public static void Release()
    {
        cur = null;
    }
}
//图片需要两张图，/第一张为普通颜色，第二张为透明图片
public class UGCClothTexture2D
{
    public Texture2D tex;
    public Texture2D texAlpha;
}

/// <summary>
/// /// UGCResUI面板
/// </summary>
public class MainUGCResPanel : BMonoBehaviour<MainUGCResPanel>
{
    private CommonColorToggleItem ColorPrefab;
    private Transform ColorParent;
    private Transform ColorParentBG;
    private RectTransform viewPort;

    private ScrollRect colorScroll;
    [HideInInspector]
    public Toggle PaintBtn;
    private Toggle OilDrumBtn;
    protected Toggle ScissorsBtn;
    private Toggle PaletteBtn;
    private Toggle mirrorBtn;
    [HideInInspector]
    public Toggle textBtn;
    [HideInInspector]
    public Toggle photoBtn;
    private Button ExitBtn;

    private Button SaveBtn;
    protected Image IconImage;
    protected RawImage maskImage;
    protected Image meshImage;
    protected Image smallMaskImage;
    private ImageToggle ShowColorBtn;
    protected MapGridCreater mapCreater;
    private UGCClothesHandler ugcHandler;
    protected Transform ClothesModelParent;
    protected Transform CloneModelParent;


    [HideInInspector]
    public ColorPinkerPanel colorPinkerPanel;
   [HideInInspector]
    public GameObject ClothesModel;
    [HideInInspector]
    public GameObject CloneModel;


    protected Camera screenShotCamera;
    private  Button UndoBtn;
    private Button RedoBtn;
    private Color maskColor = new Color(0,0,0,0.7f);
    protected ColorLibrary ugcAsset;
    protected SpriteAtlas ugcAtlas;
    private int colorLength = 14;
    protected List<UGCData> ugcDatas = new List<UGCData>();
    private List<CommonColorToggleItem> colorItems = new List<CommonColorToggleItem>();

    protected List<Texture2D> allUGCResTextures = new List<Texture2D>();
    protected List<Texture2D> allUGCResAlphaTextures = new List<Texture2D>();
    protected List<RenderTexture> allRenderTextures = new List<RenderTexture>();
    protected List<RenderTexture> allFinalRenderTextures = new List<RenderTexture>();
    protected List<DynamicDrawCanvas> allFinalCanvas = new List<DynamicDrawCanvas>();
    protected List<GameObject> allParts = new List<GameObject>();
    protected List<GameObject> allCloneParts = new List<GameObject>();
    protected List<RenderTexture> allRenderAlphaTextures = new List<RenderTexture>();
    protected Dictionary<int, Dictionary<Vector2Int, Color>> curUGCResParts;
    protected Dictionary<int, List<Vector2Int>> allInoperableArea;

    [HideInInspector]
    public GameObject curSelectPart;

    protected int curUGCResId = 2;//current only a suit of clothes
    protected int curClothesIndex = 0;
    protected string extension = ".png";
    private Tween undoClothTween;
    public Material material;
    private Transform framesPar;//所有外框的父节点
    [HideInInspector]
    public List<GameObject> frames = new List<GameObject>();//所有外框列表
    [HideInInspector]
    public RectTransform elementPanel;
    [HideInInspector]
    public Text textPrefabs;
    [HideInInspector]
    public RawImage photoPrefabs;
    private SelectPhotoPanel selectPhotoPanel;
    public Action<Color> SetElementColor;
    private UGCClothZoomHandler zoomHandler;
    protected CanvasScaler MainCanvas;
    protected RectTransform DrawBoard; 
    public Transform ImportCanvasParent;
    public DynamicDrawCanvas DrawCamera;
    protected GameObject cloneSelectPart;

    private GameObject noneArea;


    private enum FramesType
    {
        MIRROR,
        SCISSORS,
        MIRROR_AND_SCISSORS
    }
    public virtual void OnInit(MainUGCPanelResHandler handler)
    {
        
        InitLoadRes(handler);
        InitUI();
        InitListener();
        curUGCResId = UGCClothesDataManager.Inst.saveClothesData.Id;
        ugcAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/UGCClothes");
        ugcAsset = ResManager.Inst.LoadRes<ColorLibrary>("ConfigAssets/UGCClothesColorLibrary");
        SetModel();
        ugcDatas = UGCClothesDataManager.Inst.GetConfigClothesDataByID(curUGCResId);
        allInoperableArea = UGCClothesDataManager.Inst.GetInoperableAreaByID(curUGCResId);
        var noneAreaBehav = noneArea.AddComponent<ElementBaseBehaviour>();
        noneAreaBehav.type = BehaviourType.noneArea;
        noneAreaBehav.AddClickEvent();
        ugcHandler.OnSelectHander = OnSelectHandler;
        mapCreater.OnPointerDownEvent = On2DDrawPointerDown;
        mapCreater.OnAddRecordEvent = AddRecord;
        InitColorPanel();
        OnSelectColor(0);
        InitAllParts();
        Canvas uiCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        zoomHandler = new UGCClothZoomHandler();
        zoomHandler.SetData(uiCanvas, mapCreater.gameObject);
        ClothEditModeController.screenShotCamera = screenShotCamera;
        UGCClothesInputReceiver.Inst.SetHandle(zoomHandler);
        GetAllFrames();
        UGCClothesDataManager.Inst.CloneOrgSaveData();
        UGCClothesDataManager.Inst.setHierarchy = SaveHierarchy;
        UGCClothesTextManager.Inst.Init();
        UGCClothesPhotoManager.Inst.Init();

    }
    public void InitLoadRes(MainUGCPanelResHandler handler)
    { 
        material = handler.material;
        ImportCanvasParent = handler.ImportCanvasParent;
        DrawCamera = handler.DrawCamera;
        MainCanvas = handler.MainCanvas;
    }
    public void InitUI()
    {
        UndoBtn = transform.Find("Panel/UndoRedo/UndoButton").GetComponent<Button>();
        RedoBtn = transform.Find("Panel/UndoRedo/RedoButton").GetComponent<Button>();

        ExitBtn = transform.Find("Panel/MenuTool/ExitButton").GetComponent<Button>();

        SaveBtn = transform.Find("Panel/MenuTool/SaveButton").GetComponent<Button>();

        PaintBtn = transform.Find("Panel/DrawTool/Scroll View/Viewport/Content/PaintToggle").GetComponent<Toggle>();
        OilDrumBtn = transform.Find("Panel/DrawTool/Scroll View/Viewport/Content/OilDrumToggle").GetComponent<Toggle>();
        ScissorsBtn = transform.Find("Panel/DrawTool/Scroll View/Viewport/Content/ScissorsToggle").GetComponent<Toggle>();
        textBtn = transform.Find("Panel/DrawTool/Scroll View/Viewport/Content/TextToggle").GetComponent<Toggle>();
        photoBtn = transform.Find("Panel/DrawTool/Scroll View/Viewport/Content/ImageToggle").GetComponent<Toggle>();
        PaletteBtn = transform.Find("Panel/DrawTool/Scroll View/Viewport/Content/PaletteToggle").GetComponent<Toggle>();
        mirrorBtn = transform.Find("Panel/DrawTool/Scroll View/Viewport/Content/MirrorToggle").GetComponent<Toggle>();

        smallMaskImage = transform.Find("Panel/GenRawImageBG/GenRawImage/ClothesMask").GetComponent<Image>();


        ShowColorBtn = transform.Find("Panel/ColorTool/ColorBg/ColorButton").GetComponent<ImageToggle>();
        colorScroll = transform.Find("Panel/ColorTool/ColorBg/ColorGrid").GetComponent<ScrollRect>();
        viewPort = colorScroll.transform.Find("Viewport").GetComponent<RectTransform>();
        ColorParent = viewPort.Find("ColorGrid");
        ColorPrefab = ColorParent.Find("ColorItem").GetComponent<CommonColorToggleItem>();
        ColorParentBG = transform.Find("Panel/ColorTool/ColorBg/BG");
        DrawBoard = transform.Find("Panel/ZoomMask").GetComponent<RectTransform>();
        mapCreater = DrawBoard.transform.Find("RawImage").GetComponent<MapGridCreater>();
        maskImage = mapCreater.transform.Find("ClothesMask").GetComponent<RawImage>();
        meshImage = mapCreater.transform.Find("ClothesMesh").GetComponent<Image>();
        ugcHandler = transform.Find("Panel/Clothes").GetComponent<UGCClothesHandler>();
        ClothesModelParent = ugcHandler.transform.Find("UGCClothes");
        CloneModelParent = transform.Find("ScreenShot/CloneUGCClothes");
        screenShotCamera = CloneModelParent.Find("ScreenShotCamera").GetComponent<Camera>();
        colorPinkerPanel = transform.Find("Panel/ColorPinkerPanel").GetComponent<ColorPinkerPanel>();
        framesPar = transform.Find("Panel/frames");
        elementPanel = transform.Find("Panel/ElementPanel/Recovery GameObject (856466278)").GetComponent<RectTransform>();
        textPrefabs = elementPanel.transform.Find("UGCClothText").GetComponent<Text>();
        photoPrefabs = elementPanel.transform.Find("UGCClothImage").GetComponent<RawImage>();
        selectPhotoPanel = transform.Find("SelectPhoto").GetComponent<SelectPhotoPanel>();
        noneArea = transform.Find("Panel/ElementPanel/noneArea").gameObject;

    }
    public void InitListener()
    {
        UndoBtn.onClick.AddListener(OnUndoClick);
        RedoBtn.onClick.AddListener(OnRedoClick);
        ExitBtn.onClick.AddListener(OnExitClick);
        SaveBtn.onClick.AddListener(OnSaveBtnClick);
        PaintBtn.onValueChanged.AddListener(OnPaintChange);
        OilDrumBtn.onValueChanged.AddListener(OnOilDrumChange);
        ScissorsBtn.onValueChanged.AddListener(OnScissorsChange);
        PaletteBtn.onValueChanged.AddListener(OnPaletteChange);
        ShowColorBtn.onValueChanged.AddListener(OnShowColorChange);
        mirrorBtn.onValueChanged.AddListener(OnMirrorModeChange);
        textBtn.onValueChanged.AddListener(OnTextModeChange);
        photoBtn.onValueChanged.AddListener(OnPhotoModeChange);

    }
    protected virtual void InitAllParts()
    {
        allParts = new List<GameObject>();
        allCloneParts = new List<GameObject>();
        for (int i = 0; i < ugcDatas.Count; i++)
        {
            var parts = ClothesModel.transform.Find(ugcDatas[i].partsName).gameObject;
            var cloneParts = CloneModel.transform.Find(ugcDatas[i].partsName).gameObject;
            allParts.Add(parts);
            allCloneParts.Add(cloneParts);
        }
    }
    protected virtual void SetButtonShow()
    {

    }
    public virtual void GenerateUGCClothes()
    {
        allUGCResTextures = new List<Texture2D>();
        allUGCResAlphaTextures = new List<Texture2D>();
        allFinalRenderTextures = new List<RenderTexture>();
        curUGCResParts = UGCClothesDataManager.Inst.GetClothesPartDicByID(curUGCResId);

        for (var i = 0; i < ugcDatas.Count; i++)
        {
            UGCClothTexture2D tex = mapCreater.GenerateNoFilterTexture2D(curUGCResParts[ugcDatas[i].ugcType]);
            var filerTex = mapCreater.GenerateFilterTexture(tex.tex);

            var filerAlphaTex = mapCreater.GenerateFilterTexture(tex.texAlpha);
            var finalTex = mapCreater.GenerateFinalTexture();
            var drawCam = Instantiate(DrawCamera, ImportCanvasParent);
            drawCam.transform.localPosition = new Vector3(i * 30, 0, 0);
            GenerateImportUGCClothes(i, drawCam.mDrawPanel);
            drawCam.SetTargetTexture(DrawBoard, MainCanvas, finalTex);
            drawCam.SetRawImage(filerTex);
            SetPartTexture(i, finalTex, filerAlphaTex);
            allFinalCanvas.Add(drawCam);
            allUGCResTextures.Add(tex.tex);
            allRenderTextures.Add(filerTex);
            allFinalRenderTextures.Add(finalTex);
            allUGCResAlphaTextures.Add(tex.texAlpha);
            allRenderAlphaTextures.Add(filerAlphaTex);
        }
        OnSelectHandler(allParts[0]);
        HierarchicalSort();
    }
    protected virtual void SetPartTexture(int index ,RenderTexture tex,RenderTexture alphaTex)
    {
        
    }
    protected virtual void OnSelectHandler(GameObject hit)
    {
        if (curSelectPart == hit)
            return;
        var index = ugcDatas.FindIndex(x => x.partsName == hit.name);
        SetHightLight(curClothesIndex, index);
        var oldPart = curClothesIndex;
        curClothesIndex = index;
        curSelectPart = hit;
        cloneSelectPart = GetClonePart(curSelectPart);
        var data = ugcDatas[index];
        if (!string.IsNullOrEmpty(data.maskSpriteName))
        {
            maskImage.texture = ResManager.Inst.LoadRes<Sprite>("Texture/UGCClothes/Mask/" + data.maskSpriteName).texture;
            smallMaskImage.sprite = ResManager.Inst.LoadRes<Sprite>("Texture/UGCClothes/Mask/" + data.maskSpriteName);
        }
        else
        {
            maskImage.gameObject.SetActive(false);
            smallMaskImage.gameObject.SetActive(false);
        }
        if (!string.IsNullOrEmpty(data.meshSpriteName))
        {
            meshImage.gameObject.SetActive(true);
            meshImage.sprite = ugcAtlas.GetSprite(data.meshSpriteName);
        }
        else
        {
            meshImage.gameObject.SetActive(false);
        }
        mapCreater.SetRawImage(allFinalCanvas[index].mRawImage);
        mapCreater.ChangeClothesPart(allUGCResTextures[index], allRenderTextures[index], allFinalRenderTextures[index],
            allUGCResAlphaTextures[index], allRenderAlphaTextures[index]
            , curSelectPart, cloneSelectPart, allInoperableArea[data.ugcType], curUGCResParts[data.ugcType]);
        ResetMapZoom();
        UGCClothesTextManager.Inst.OnChangePart(index + 1);
        UGCClothesPhotoManager.Inst.OnChangePart(index + 1);
        if (TransformInteractorController.Inst.interActor)
        {
            TransformInteractorController.Inst.interActor.ResetInfo();
        }
    }
    protected virtual void SetHightLight(int lastIndex, int curIndex)
    {
    }
    private GameObject GetClonePart(GameObject curSelectPart)
    {
        var transList = CloneModel.GetComponentsInChildren<Transform>();
        foreach (var tran in transList)
        {
            if (tran.name == curSelectPart.name)
            {
                return tran.gameObject;
            }
        }
        return null;
    }
    protected void GenerateImportUGCClothes(int index,Transform par)
    {
        int part = index + 1;
        var tList = UGCClothesDataManager.Inst.GetTextData(index);
        foreach (var t in tList)
        {
            UGCClothesTextManager.Inst.CreatText(part, t, par);
        }
        UGCClothesTextManager.Inst.ShowTextByIndex(part);
        UGCClothesTextManager.Inst.ShowHideAllRayTarget(false);

        var pList = UGCClothesDataManager.Inst.GetPhotoData(index);
        foreach (var p in pList)
        {
            UGCClothesPhotoManager.Inst.CreatPhoto(part, p, par,selectPhotoPanel);
        }
        UGCClothesPhotoManager.Inst.ShowPhotoByIndex(part);
        UGCClothesPhotoManager.Inst.ShowHideAllRayTarget(false);
    }

    protected void HierarchicalSort()
    {
        List<ElementBaseBehaviour> gather = new List<ElementBaseBehaviour>();

        gather.AddRange(UGCClothesPhotoManager.Inst.photoList);
        gather.AddRange(UGCClothesTextManager.Inst.textList);
        gather.Sort(SortCompare);
        for (int i = 0; i < gather.Count; i++)
        {
            gather[i].rectTrans.SetSiblingIndex(i);
            gather[i].SetMirSiblingIndex();
        }
    }

    private static int SortCompare(ElementBaseBehaviour info1, ElementBaseBehaviour info2)
    {
        return info1.hierarchy.CompareTo(info2.hierarchy);
    }


    public virtual void OnUndo(UGCClothDrawUndoData helpData)
    {
        mapCreater.OnUndoSetGrid(helpData.drawGridPairs);
    }
   
    protected void StartUGCTween(int rot)
    {
        if (undoClothTween!=null)
        {
            undoClothTween.Kill();
            undoClothTween = null;
        }
        undoClothTween = ClothesModelParent.DORotate(new Vec3(0, rot, 0), 0.2f);
    }

    //外部切换面片时调用
    public virtual void ChangeParts(GameObject selectPart)
    {

    }
   


    protected void ResetMapZoom()
    {
        RectTransform rectComp = mapCreater.GetComponent<RectTransform>();
        rectComp.localScale = Vector3.one;
        rectComp.anchoredPosition = Vector3.zero;
    }

    protected virtual void On2DDrawPointerDown()
    {

    }



    private void InitColorPanel()
    {
        CreateColorPanel(0, ugcAsset.Size(), ColorParent);
        colorScroll.vertical = false;
    }

    private void CreateColorPanel(int startIndex,int count,Transform par)
    {
        for (int i = startIndex; i < count; i++)
        {
            var item = GameObject.Instantiate(ColorPrefab, par);
            item.gameObject.SetActive(true);
            item.SetColor( i,ugcAsset.Get(i), OnSelectColor);
            colorItems.Add(item);
        }
    }


    private int curIndex;
    private void OnSelectColor(int index)
    {
       


        curIndex = index;
        for (int i = 0; i < colorItems.Count; i++)
        {
            if (i != index)
            {
                colorItems[i].ColorCheckImage.SetActive(false);
            }
        }
        //自定义颜色面板打开时显示对应的三色值
        if (colorPinkerPanel.gameObject.activeSelf)
        {     
            colorPinkerPanel.SetColorHSV(ugcAsset.Get(index));
        }
        PaintTool.Current.SetColor(ugcAsset.Get(index));
        PaintTool.Current.PgcCurItem=colorItems[index];
        //将ugc颜色的选中态置空
        if (PaintTool.Current.UgcCurItem!=null)
        {
            PaintTool.Current.UgcCurItem.ColorCheckImage.SetActive(false);
            PaintTool.Current.UgcCurItem=null;
        }
        ShowColorBtn.isOn = true;
        ShowColorBtn.SetToggle(true);
        OnShowColorChange(true);
        SetElementColor?.Invoke(ugcAsset.Get(index));
    }


    private void OnPaintChange(bool isOn)
    {
        if (isOn)
        {
            PaintTool.Current.SetPaintType(PaintType.Brush);
        }
    }
    private void OnMirrorModeChange(bool isOn)
    {
        PaintTool.Current.SetCurMode(isOn?ClotheDrawMode.Mirror: ClotheDrawMode.Normal);
        SetMirrorMode(isOn);
    }

    private void OnTextModeChange(bool isOn)
    {
        if (isOn)
        {
            PaintTool.Current.SetPaintType(PaintType.Text);
            var textList = UGCClothesTextManager.Inst.textList;
            ElementButtonManager.Inst.isCanClickElement = true;
            noneArea.gameObject.SetActive(true);
            UGCClothesTextBehaviour newBehav = null;
            for (int i = textList.Count - 1; i >= 0 ; i--)
            {
                if(textList[i].part == (curClothesIndex + 1))
                {
                    newBehav = textList[i];
                    break;
                }
            }
            if (newBehav == null)
            {
                newBehav = UGCClothesTextManager.Inst.CreatText(curClothesIndex + 1, null, allFinalCanvas[curClothesIndex].mDrawPanel);
                if (newBehav)
                {
                    UGCClothesTextManager.Inst.AddCreateRecord(newBehav.gameObject, (int)newBehav.type);
                }
            }
            if (newBehav)
            {
                TransformInteractorController.Inst.GetInterActor().Settup(newBehav.rectTrans, newBehav.OnTransformChange, newBehav.Init);
                SetColorSelect(newBehav.self.color);
            }
        }
        else
        {
            QuitElementMode();
        }
        UGCClothesTextManager.Inst.ShowHideAllRayTarget(isOn);
        UGCClothesPhotoManager.Inst.ShowHideAllRayTarget(isOn);
    }

    private void OnPhotoModeChange(bool isOn)
    {
        if (isOn)
        {
            PaintTool.Current.SetPaintType(PaintType.Photo);
            var photoList = UGCClothesPhotoManager.Inst.photoList;
            ElementButtonManager.Inst.isCanClickElement = true;
            noneArea.gameObject.SetActive(true);
            UGCClothesPhotoBehaviour newBehav = null;
            for (int i = photoList.Count - 1; i >= 0; i--)
            {
                if (photoList[i].part == (curClothesIndex+1))
                {
                    newBehav = photoList[i];
                    break;
                }
            }
            if(newBehav == null)
            {
                newBehav = UGCClothesPhotoManager.Inst.CreatPhoto(curClothesIndex + 1, null, allFinalCanvas[curClothesIndex].mDrawPanel, selectPhotoPanel);
                if (newBehav)
                {
                    UGCClothesPhotoManager.Inst.AddCreateRecord(newBehav.gameObject, (int)newBehav.type);
                }
            }
            if (newBehav)
            {
                TransformInteractorController.Inst.GetInterActor().Settup(newBehav.rectTrans, newBehav.OnTransformChange, newBehav.Init);
            }
        }
        else
        {
            QuitElementMode();
        }
        UGCClothesTextManager.Inst.ShowHideAllRayTarget(isOn);
        UGCClothesPhotoManager.Inst.ShowHideAllRayTarget(isOn);
    }

    private void QuitElementMode()
    {
        if (TransformInteractorController.Inst.interActor)
        {
            TransformInteractorController.Inst.interActor.ResetInfo();
        }
        ElementButtonManager.Inst.isCanClickElement = false;
        noneArea.gameObject.SetActive(false);
    }

    private void OnOilDrumChange(bool isOn)
    {
        if (isOn)
        {
            PaintTool.Current.SetPaintType(PaintType.OilDrum);
        }
        
    }
    private void OnScissorsChange(bool isOn)
    {
        
        if (isOn)
        {
            PaintTool.Current.SetPaintType(PaintType.Scissors);
        }
        SetFrameShow(isOn);
    }
    
    private void OnPaletteChange(bool isOn)
    {
        colorPinkerPanel.gameObject.SetActive(isOn);
        if ( isOn==true)
        {
            if (ShowColorBtn.isOn==false)
            {
                ShowColorBtn.SetToggle(true);
                OnShowColorChange(true);
            }         
        }
    }
    private void OnShowColorChange(bool isOn)
    {
       
        if (isOn)
        {
            colorScroll.vertical = false;
            viewPort.offsetMin = new Vector2(viewPort.offsetMin.x, 132);
            ColorParentBG.transform.localPosition = new Vector3(0, 250, 0);
            ColorParent.localPosition = new Vector3(0, (curIndex / colorLength)*102, 0);
        }
        else
        {
            colorScroll.vertical = true;
            viewPort.offsetMin = new Vector2(viewPort.offsetMin.x, -118);
            ColorParentBG.transform.localPosition = Vector3.zero;
            ColorParent.localPosition = new Vector3(0, 0, 0);
            PaletteBtn.isOn=false;
        }
    }

    protected virtual void OnSaveBtnClick()
    {
        PlaySaveAnim(true);
        SaveUGCRes(OnSaveUGCResInfoSuccess, OnSaveUGCResInfoFail);
    }

    protected virtual void OnSaveAndQuitClick()
    {
        SaveUGCRes(OnSaveUGCResInfoSuccessAndQuit, OnSaveUGCResInfoFail);
    }

    private void OnSaveUGCResInfoSuccess()
    {
        PlaySaveAnim(false);
        ClothEditModeController.OnSaveClothInfoSuccess();
    }

    private void OnSaveUGCResInfoSuccessAndQuit()
    {
        PlaySaveAnim(false);
        ClothEditModeController.OnSavClothInfoeSuccessAndQuit();
    }

    private void OnSaveUGCResInfoFail()
    {
        PlaySaveAnim(false);
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }

    public virtual void OnSaveUGCResByFirst()
    {

    }

    protected virtual void SaveUGCRes(Action onSuccess, Action onFail)
    {
   
    }
    
    protected void GenTexturePNG(string filePath, RenderTexture rt)
    {
        Texture2D tex2D = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;
        tex2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex2D.Apply();
        var bytes = tex2D.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
    }

   
    string loadOrFailTips = "Photo is saving. Please wait for seconds. Quitting at this time may lose the photo.";
    string oriTips = "Do you want to save this clothing?";
    private void OnExitClick()
    {
        ComfirmPanel.Show();
        ComfirmPanel.Instance.SetMaskColor(maskColor);
        string content = UGCClothesPhotoManager.Inst.IsLoadingOrFail() ? loadOrFailTips : oriTips;
        ComfirmPanel.Instance.SetText(content);
        ComfirmPanel.Instance.OnDontClick = OnGameQuit;
        ComfirmPanel.Instance.OnSaveClick = () =>
        {
            ComfirmPanel.SetAnim(true);
            OnSaveAndQuitClick();
        };
    }

    protected void PlaySaveAnim(bool isActive)
    {
        var saveIcon = SaveBtn.transform.GetChild(0);
        var loader = SaveBtn.transform.GetChild(1);
        saveIcon.gameObject.SetActive(!isActive);
        loader.gameObject.SetActive(isActive);
    }


    private void OnGameQuit()
    {
        ExitEditParams exitEditParams = new ExitEditParams()
        {
            mapId = GameManager.Inst.ugcUntiyMapDataInfo.mapId,
            draftPath = GameManager.Inst.ugcUntiyMapDataInfo.draftPath,
        };
        string quitPara = JsonConvert.SerializeObject(exitEditParams);
        LoggerUtils.Log("exitEditParams == " + quitPara);
        MobileInterface.Instance.Quit(quitPara);
    }

    protected override void OnDestroy()
    {
        PaintTool.Release();
        if (undoClothTween!=null)
        {
            undoClothTween.Kill();
            undoClothTween = null;
        }
    }
    protected virtual void SetModel()
    {
        
    }
    protected void AddModel()
    {

    }
    
    private void SetMirrorMode(bool isMirrorMode)
    {
        if (isMirrorMode)
        {
            maskImage.material = material;
            mapCreater.curMode = ClotheDrawMode.Mirror;
            
        }
        else
        {
            maskImage.material = null;
            mapCreater.curMode = ClotheDrawMode.Normal;
           
        }
        SetFrameShow(PaintTool.Current.pType == PaintType.Scissors);
    }

    private void OnUndoClick()
    {
        UndoRedoManager.Inst.Undo();
        UpdateUndoBtnView();
    }

    private void OnRedoClick()
    {
        UndoRedoManager.Inst.Redo();
        UpdateUndoBtnView();
    }
    public void UpdateUndoBtnView()
    {
        LoggerUtils.Log("UpdateUndoBtnView");
        bool hasUndo = (UndoRecordPool.Inst.GetUndoCount() > 0);
        UndoBtn.transform.GetChild(0).gameObject.SetActive(!hasUndo);
       

        bool hasRedo = (UndoRecordPool.Inst.GetRedoCount() > 0);
        RedoBtn.transform.GetChild(0).gameObject.SetActive(!hasRedo);
    }
    public void AddRecord(Dictionary<Vector2Int, Color> beforeDrowGridPairs, Dictionary<Vector2Int, Color> afterDrowGridPairs)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.UGCClothDrawUndoHelper);
        record.BeginData = CreateUndoData(beforeDrowGridPairs);
        record.EndData = CreateUndoData(afterDrowGridPairs);

        UndoRecordPool.Inst.PushRecord(record);

        UpdateUndoBtnView();


    }
    private UGCClothDrawUndoData CreateUndoData(Dictionary<Vector2Int, Color> gridList)
    {
        UGCClothDrawUndoData data = new UGCClothDrawUndoData();
        Dictionary<Vector2Int, Color> pairs = new Dictionary<Vector2Int, Color>();
        foreach (var item in gridList)
        {
            pairs.Add(item.Key, item.Value);
        }
        data.drawGridPairs = pairs;
        data.selectPart = curSelectPart;

        return data;
    }
    
    private void GetAllFrames()
    {
        for (int i = 0; i < framesPar.childCount; i++)
        {
            frames.Add(framesPar.GetChild(i).gameObject);
        }
    }

    private void SetFrameShow(bool isScissors)
    {
        for (int i = 0; i < frames.Count; i++)
        {
            frames[i].SetActive(false);
        }
        if (isScissors)
        {
            if (mapCreater.curMode == ClotheDrawMode.Mirror)
            {
                frames[(int)FramesType.MIRROR_AND_SCISSORS].SetActive(true);
            }
            else
            {
                frames[(int)FramesType.SCISSORS].SetActive(true);
            }
        }
        else
        {
            if (mapCreater.curMode == ClotheDrawMode.Mirror)
            {
                frames[(int)FramesType.MIRROR].SetActive(true);
            }
        }
        
    }

    //外部调整了颜色后，设置颜色选中
    public void SetColorSelect(Color col)
    {
        for (int i = 0; i < colorPinkerPanel.UgcColorItems.Count; i++)
        {
            if (colorPinkerPanel.UgcColorItems[i].color == col)
            {
                colorPinkerPanel.OnSelectColor(colorPinkerPanel.UgcColorItems[i]);
                colorPinkerPanel.UgcColorItems[i].ColorCheckImage.SetActive(true);
                return;
            }
            else
            {
                colorPinkerPanel.UgcColorItems[i].ColorCheckImage.SetActive(false);
            }
        }
        for (int i = 0; i < colorItems.Count; i++)
        {
            if (colorItems[i].color == col)
            {
                OnSelectColor(colorItems[i].colorIndex);
                colorItems[i].ColorCheckImage.SetActive(true);
                return;
            }
            else
            {
                colorItems[i].ColorCheckImage.SetActive(false);
            }
        }
    }

    private void SaveHierarchy()
    {
        UGCClothesTextManager.Inst.ShowHideAllText(true);
        UGCClothesPhotoManager.Inst.ShowHideAllPhoto(true);
        var elementList = elementPanel.GetComponentsInChildren<ElementBaseBehaviour>();
        for (int i = 0; i < elementList.Length; i++)
        {
            elementList[i].hierarchy = i;
        }
        UGCClothesTextManager.Inst.OnChangePart(curClothesIndex + 1);
        UGCClothesPhotoManager.Inst.OnChangePart(curClothesIndex + 1);
    }


#if UNITY_EDITOR
    //设置UGC画板区域
    private string fileName = "UGCClothes_1";
    private bool isEditConfig = false;
    private void OnGUI()
    {
       
        GUIStyle btnStyle = new GUIStyle();  
        btnStyle.alignment=TextAnchor.MiddleCenter;
        btnStyle.fontSize=40;
        btnStyle.normal.textColor=Color.white;
        btnStyle.normal.background = Texture2D.grayTexture;
        btnStyle.hover.background = Texture2D.whiteTexture;
        string btnName = isEditConfig ? "Close" : "Edit";
        if (GUI.Button(new Rect(50,150,200,60),btnName,btnStyle))
        {
            isEditConfig = !isEditConfig;
            maskImage.color = isEditConfig ? new Color(1, 1, 1, 0.7f) : Color.white;
        }
        if(!isEditConfig)
            return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 40;
        style.imagePosition = ImagePosition.ImageAbove;
        GUI.Label(new Rect(50,200,500,100),"Config Name:",style);
        fileName = UGCClothesDataManager.Inst.clothesConfigs[curUGCResId];
        GUI.Label(new Rect(50,250,500,60),fileName,style);
        GUI.Label(new Rect(50,300,500,100),"Note: white is the paintable area",style);
        GUI.Label(new Rect(50,350,500,100),"Note: GameStart Node TempId sets the current UGC clothing template",style);
        if (GUI.Button(new Rect(50,400,200,60),"create",btnStyle))
        {
            List<string> noFiles = new List<string>();
            var data = ugcDatas[curClothesIndex];
            foreach (var pValue in curUGCResParts[data.ugcType])
            {
                if (pValue.Value != Color.white)
                {
                    noFiles.Add(DataUtils.Vector2IntToString(pValue.Key));
                }
            }
            data.inoperableArea = noFiles;
            File.WriteAllText(Application.dataPath + "/Resources/Configs/UGCRes/" + fileName + ".json",
                JsonConvert.SerializeObject(ugcDatas));
        }
        
        if (GUI.Button(new Rect(50,500,200,60),"Clear",btnStyle))
        {
            foreach (var data in ugcDatas)
            {
                data.inoperableArea = new List<string>();
            }
            File.WriteAllText(Application.dataPath + "/Resources/Configs/UGCRes/" + fileName + ".json",
                JsonConvert.SerializeObject(ugcDatas));
        }

        if (GUI.Button(new Rect(50,580,200,60),"inoperableArea",btnStyle))
        {
            if (isEditConfig)
            {
                Dictionary<Vector2Int, Color> tempDic = new Dictionary<Vector2Int, Color>();
                var datas = ugcDatas[curClothesIndex].inoperableArea;
                foreach (var data in datas)
                {
                    Vector2Int vec = DataUtils.DeSerializeVector2Int(data);
                    tempDic.Add(vec,Color.red);
                }
                mapCreater.SetInoperableArea(tempDic);
            }
        }
    }
#endif

}
public enum ClotheDrawMode
{
    Normal,//普通模式
    Mirror//镜像模式
}
public enum ClotheType
{
    Cloth,//衣服
    Face,//面部
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GRTools.Localization;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;
using UnityEngine.UI;

public static class ShowType
{
    public const string Material = "Material";
    public const string Color = "Color";
    public const string Text = "Text";
    public const string Type = "Type";
    public const string Settings = "Settings";
}
[Serializable]
public class OpenStorePageData
{
    public int dataType;
}

public abstract class CommonMatColorPanel<T> : InfoPanel<T> where T : InfoPanel<T>
{   
    public enum ColorSelectType
    {
        Normal,
        Customize,
    }
    public enum OpenUGCPage
    {
        UGCMaterial = 1
    }
    public virtual void GetMatDatas() { }
    public virtual void GetColorDatas() { }
    public virtual void SetColor(int index, ColorSelectType type) { }
    public virtual void SetColor(Color color) { }
    public virtual void SetMaterial(int index) { }
    
    public abstract void SetEntity(SceneEntity entity);
    public virtual void SetEntity(SceneEntity rootEntity, SceneEntity entity) { }
    public abstract void AddTabListener();
    public virtual void SetUGCMaterial(UGCMatData uData) { }
    protected TabSelectGroup tabGroup;
    protected ToggleGroup matTypeGroup;
    protected ScrollRect matNormalScroll;
    protected ScrollRect matExpandScroll;
    protected ScrollRect ugcMatExpandScroll;
    protected ScrollRect colorNormalScroll;
    protected ScrollRect colorExpandScroll;
    private Slider HueSlider;
    private Slider ChormaSlider;
    private Slider BrightSlider;
    private SliderGradient chormaGradient;
    private SliderGradient brightGradient;
    protected Button confirmBtn;
    protected Button delectBtn;
    private GameObject loadMatPrefab;
    private GameObject loadColorPrefab;

    private Vector3 normalVec = new Vector3(1, 1, 1);
    private Vector3 selectVec = new Vector3(0.66f, 0.66f, 1);
    private int curIndex = MAX_COUNT;
    private bool isSelectCustomize = false;
    protected int colorId;
    protected string colorStr = "";
    protected ColorSelectType colorType;
    protected List<GameObject> matItems = new List<GameObject>();
    protected List<GameObject> colorItems = new List<GameObject>();
    protected List<GameObject> customizeItems = new List<GameObject>();
    protected List<string> customizeColors;
    protected List<Toggle> matTypeToggles = new List<Toggle>();
    protected List<GameMatData> matDatas = new List<GameMatData>();
    protected ColorLibrary colorDatas;

    protected bool hasUGCMat;
    protected UGCMatView ugcMatView;
    private const int MAX_COUNT = 8;

    private bool _avoidSetColor = false;//slider获取值是不改变entity颜色，只做slider展示

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        customizeColors = DataUtils.GetCustomizeColorInfo(GameInfo.Inst.myUid ?? string.Empty);
        ResetSlider();
    }
    public void SetHasUGCMatPanel()
    {
        hasUGCMat = true;
      
    }
    public void AddCommonListener()
    {
        AddTabListener();
        AddSliderListener();
        AddCustomizeBtnListener();
    }
    public void AddCustomizeBtnListener()
    {
        confirmBtn.onClick.AddListener(OnConfirmBtnClick);
        delectBtn.onClick.AddListener(OnDelectBtnClick);
       
    }

    public void OnConfirmBtnClick()
    {
        if (customizeColors.Count < MAX_COUNT)
        {
            Color color = Color.HSVToRGB(HueSlider.value, BrightSlider.value, ChormaSlider.value);
            string str = DataUtils.ColorToString(color);
            customizeColors.Add(str);
            DataUtils.SetCustomizeColorInfo(GameInfo.Inst.myUid ?? string.Empty, customizeColors);
            UpdateCustomizePanel();
        }
        else
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
        }
    }
    public void OnDelectBtnClick()
    {
        if (curIndex >= customizeColors.Count) return;
        if (isSelectCustomize)
        {
            customizeColors.RemoveAt(curIndex);
            DataUtils.SetCustomizeColorInfo(GameInfo.Inst.myUid ?? string.Empty, customizeColors);
            UpdateCustomizePanel();
            curIndex = MAX_COUNT;//删除后默认选中最后一个空白位
            SetCurColor(ColorSelectType.Customize, curIndex);
        }
    }
    public void ResetSlider()
    {
        Slider[] sliders = GetComponentsInChildren<Slider>();
        if (sliders.Length <= 0)
        {
            return;
        }
        HueSlider = sliders[0];
        ChormaSlider = sliders[1];
        BrightSlider = sliders[2];
        chormaGradient = ChormaSlider.GetComponentInChildren<SliderGradient>();
        brightGradient = BrightSlider.GetComponentInChildren<SliderGradient>();
    }
    private void AddSliderListener()
    {
        HueSlider.onValueChanged.AddListener(OnHueSliderValueChanged);
        BrightSlider.onValueChanged.AddListener(OnBightSliderValueChanged);
        ChormaSlider.onValueChanged.AddListener(OnChormaSliderValueChanged);
    }
    private void OnHueSliderValueChanged(float value)
    {
        Color brightL = Color.HSVToRGB(value, 0, ChormaSlider.value);
        Color brightR = Color.HSVToRGB(value, 1, ChormaSlider.value);
        brightGradient.SetColor(brightL, brightR);
        Color chormaL = Color.black;
        Color chormaR = Color.HSVToRGB(value, BrightSlider.value, 1);
        chormaGradient.SetColor(chormaL, chormaR);
        if(!_avoidSetColor)
        {
            SetColor(GetColor());
        }
        
    }
    private void OnChormaSliderValueChanged(float value)
    {
        Color brightL = Color.HSVToRGB(HueSlider.value, 0, value);
        Color brightR = Color.HSVToRGB(HueSlider.value, 1, value);
        brightGradient.SetColor(brightL, brightR);
        if(!_avoidSetColor)
        {
            SetColor(GetColor());
        }
    }
    private void OnBightSliderValueChanged(float value)
    {
        Color chormaL = Color.black;
        Color chormaR = Color.HSVToRGB(HueSlider.value, value, 1);
        chormaGradient.SetColor(chormaL, chormaR);
        if(!_avoidSetColor)
        {
            SetColor(GetColor());
        }
        
    }
    private Color GetColor()
    {
        var color = Color.HSVToRGB(HueSlider.value, BrightSlider.value, ChormaSlider.value);
        return color;
    }
    public void SetSliderColor(float H, float S, float V)
    {   
        HueSlider.value = H;
        ChormaSlider.value = V;
        BrightSlider.value = S;
        Color brightL = Color.HSVToRGB(H, 0, V);
        Color brightR = Color.HSVToRGB(H, 1, V);
        Color chormaL = Color.black;
        Color chormaR = Color.HSVToRGB(H, S, 1);
        brightGradient.SetColor(brightL, brightR);
        chormaGradient.SetColor(chormaL, chormaR);
    }
    public void SetSliderColor(Color color)
    {
        Color.RGBToHSV(color, out float H, out float S, out float V);
        _avoidSetColor = true;
        HueSlider.value = H;
        ChormaSlider.value = V;
        BrightSlider.value = S;
        _avoidSetColor = false;
        Color brightL = Color.HSVToRGB(H, 0, V);
        Color brightR = Color.HSVToRGB(H, 1, V);
        Color chormaL = Color.black;
        Color chormaR = Color.HSVToRGB(H, S, 1);
        brightGradient.SetColor(brightL, brightR);
        chormaGradient.SetColor(chormaL, chormaR);
    }

    public void AddMatTypeListener()
    {
        
        Toggle[] toggles = matTypeGroup.GetComponentsInChildren<Toggle>();
        for(int i = 0; i < toggles.Length; i++)
        {
            int index = i;
            toggles[index].onValueChanged.AddListener((bool isOn) =>
            {
                OnMatTypeTabValueChanged(index, isOn);
            });
            toggles[index].transform.GetComponentInChildren<Text>().color = new Color(1, 1, 1, 0.4f);
            toggles[index].transform.GetComponentInChildren<LocalizationComponent>().OnChangeContent = OnLocalizationContent;
            matTypeToggles.Add(toggles[index]);
        }
    }
    public void OnLocalizationContent(LanguageCode languageCode, Component component, string text)
    {
        var content = GameUtils.SubStringByBytes(text, 14, Encoding.Unicode);
        component.GetComponent<Text>().text = content;
    }
    private void OnMatTypeTabValueChanged(int index,bool isOn)
    {
        Text text = matTypeToggles[index].GetComponentInChildren<Text>();
        if (!isOn)
        {
            text.color = new Color(1, 1, 1, 0.4f);
            return;
        }
        text.color = new Color(1, 1, 1, 1);
        RefreshScrollPanel(ShowType.Material, true);
        if (hasUGCMat)
        {
            if (index == 0)
            {
                ugcMatView.InitExperienceList(SetCurUGCMaterial);
            }
            else
            {
                UpdateMatExpandScroll(index-1);
            }
            SwithUgcMatPanel(index == 0);
        }
        else
        {
            UpdateMatExpandScroll(index);
        }
       
    }
    private void SwithUgcMatPanel(bool isUgcOn)
    {
        if (ugcMatExpandScroll != null&& matExpandScroll != null)
        {
            ugcMatExpandScroll.gameObject.SetActive(isUgcOn);
            matExpandScroll.gameObject.SetActive(!isUgcOn);
        }
    }
   
    public virtual void SetCurUGCMaterial(UGCMatData uData)
    {
        if (uData.mapInfo!=null)
        {
            SetUGCMatSelect(uData.mapInfo.mapId);
            SetUGCMaterial(uData);
        }
       
    }
    public void SetUGCMatSelect(string mapid)
    {
        if (string.IsNullOrEmpty(mapid))
        {
            LoggerUtils.LogError("matId is Error");
            return;
        }
        SetAllMatItemHide();
        ugcMatView.SetItemShow(mapid);
    }
    public void SetAllMatItemHide()
    {
        if (ugcMatView!=null)
        {
            ugcMatView.SetAllItemHide();
        }
      
        matItems.ForEach(x => x.transform.GetChild(0).gameObject.SetActive(false));
    }
    public void UpdateMatExpandScroll(int type)
    {
        if (type < 0)
        {
            for(int i = 0; i < matDatas.Count; i++)
            {
                matItems[i].SetActive(true);
            }
        }
        else
        {
            for(int i = 0; i < matDatas.Count;i++)
            {
                if (type == matDatas[i].matGroupType)
                {
                    matItems[i].SetActive(true);
                }
                else
                {
                    matItems[i].SetActive(false);
                }
            }
        }
    }
    public void SetCurMaterial(int index)
    {
        SetMatSelect(index);
        SetMaterial(index);
    }
    public void SetCurColor(ColorSelectType type,int index)
    {
        SetColorSelect(type, index);
        if (type == ColorSelectType.Customize)
        {
            isSelectCustomize = true;
            curIndex = index;
            if (index < customizeColors.Count)
            {
                var str = customizeColors[index];
                Color color = DataUtils.DeSerializeColor(str);
                SetSliderColor(color);
                SetColor(index,ColorSelectType.Customize);
                return;
            }
        }
        else if(type == ColorSelectType.Normal)
        {
            SetSliderColor(colorDatas.Get(index));
            isSelectCustomize = false;
            SetColor(index,ColorSelectType.Normal);
        }
    }
    public void SetMatSelect(int index)
    {
        if (index < 0 || index >= matItems.Count)
        {
            LoggerUtils.LogError("matId is Error");
            return;
        }
        SetAllMatItemHide();
        matItems[index].transform.GetChild(0).gameObject.SetActive(true);
    }
    public void SetColorSelect(ColorSelectType type, int index)
    {
        colorItems.ForEach(x => x.GetComponentInChildren<Image>().transform.localScale = normalVec);
        customizeItems.ForEach(x => x.GetComponentInChildren<Image>().transform.localScale = normalVec);
        if (type == ColorSelectType.Normal)
        {
            if (index < 0 || index >= colorItems.Count)
            {
                LoggerUtils.LogError("colorId is Error");
                return;
            }
            colorItems[index].GetComponentInChildren<Image>().transform.localScale = selectVec;
        }
        else if (type == ColorSelectType.Customize)
        {
            if (index < 0 || index >= customizeItems.Count)
            {
                return;
            }
            if (index < customizeColors.Count)
            {
                customizeItems[index].GetComponentInChildren<Image>().transform.localScale = selectVec;
            }
        }
    }
    public int GetCustomizeColorIndex(Color color)
    {
        for(int i = 0; i < customizeColors.Count; i++)
        {
            int index = i;
            var str = customizeColors[index];
            Color dataColor = DataUtils.DeSerializeColor(str);
            if(dataColor.Equals(color))
            {
                return index;
            }
        }
        return -1;
    }
    public void CreateMatItems()
    {
        matItems.Clear();
        var priAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/GameAtlas");
        loadMatPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "BaseMaterialItem");
        for (int i = 0; i < matDatas.Count; i++)
        {
            int index = i;
            var matData = matDatas[index];
            var matGo = Instantiate(loadMatPrefab, matNormalScroll.content);
            var matImage = matGo.transform.GetChild(1).GetComponent<Image>();
            var matBtn = matGo.GetComponentInChildren<Button>();
            matImage.sprite = priAtlas.GetSprite(matData.iconName);
            matBtn.onClick.AddListener(() => SetCurMaterial(index));
            matItems.Add(matGo);
        }
    }
    public void CreateColorItems()
    {
        colorItems.Clear();
        loadColorPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "CommonColorItem");
        for (int i = 0; i < colorDatas.List.Count; i++)
        {
            int index = i;
            var colorGo = Instantiate(loadColorPrefab, colorNormalScroll.content);
            var colorBtn = colorGo.GetComponentInChildren<Button>();
            var color = colorDatas.Get(index);
            Image[] images = colorGo.GetComponentsInChildren<Image>();
            foreach (var image in images)
            {
                image.color = color;
            }
            colorBtn.onClick.AddListener(() => SetCurColor(ColorSelectType.Normal, index));
            colorItems.Add(colorGo);
        }
        for (int i = 0; i < customizeItems.Count; i++)
        {
            int index = i;
            var btn = customizeItems[index].GetComponentInChildren<Button>();
            btn.onClick.AddListener(() => SetCurColor(ColorSelectType.Customize, index));
        }
    }
    public void UpdateCustomizePanel()
    {
        customizeColors = DataUtils.GetCustomizeColorInfo(GameInfo.Inst.myUid ?? string.Empty);
        foreach (var item in customizeItems)
        {
            Image[] images = item.GetComponentsInChildren<Image>();
            images[0].color = new Color(1, 1, 1, 0.4f);
            images[1].color = new Color(1, 1, 1, 0);
        }
        for (int i = 0; i < customizeColors.Count; i++)
        {
            int index = i;
            if (index >= customizeItems.Count) return;
            string str = customizeColors[index];
            Color custColor = DataUtils.DeSerializeColor(str);
            Color color = new Color(custColor.r, custColor.g, custColor.b, 1);
            Image[] images = customizeItems[index].GetComponentsInChildren<Image>();
            foreach(var image in images)
            {
                image.color = color;
            }
        }
    }
    public void UpdateScrollPanel(List<GameObject> items, RectTransform parent)
    {
        foreach (var item in items)
        {
            item.transform.SetParent(parent.transform);
        }
    }
    public void SetEntitySelectColor(int colorSelectIndex,string entityColorStr)
    {
        colorId = colorSelectIndex;
        if (colorId < 0)
        {
            for (int i = 0; i < customizeColors.Count; i++)
            {
                var cusColorStr = customizeColors[i];
                if (Equals(cusColorStr, entityColorStr))
                {
                    colorId = i;
                    break;
                }
            }
            Color color = DataUtils.DeSerializeColor(entityColorStr);
            SetSliderColor(color);
            colorType = ColorSelectType.Customize;
            SetColorSelect(ColorSelectType.Customize, colorId);
        }
        else
        {
            colorType = ColorSelectType.Normal;
            SetSliderColor(colorDatas.Get(colorId));
            SetColorSelect(ColorSelectType.Normal, colorId);
        }
    }
    public int GetMaterialType(int index)
    {
        var mat = matDatas[index];
        if (mat != null)
        {
            return mat.matGroupType;
        }
        return -1;
    }
    public void RefreshScrollPanel(string type, bool isExpand)
    {
        RefreshScrollPanel(type, isExpand,null,0);

    }
    public void RefreshScrollPanel(string type, bool isExpand, GameMatData matData)
    {
        RefreshScrollPanel(type, isExpand, matData, 0);
    }
    public void RefreshScrollPanel(string type, bool isExpand, int openUGCPage)
    {
        RefreshScrollPanel(type, isExpand, null, openUGCPage);
    }
    public void RefreshScrollPanel(string type, bool isExpand, GameMatData matData,int openUGCPage)
    {
        if (Equals(type,ShowType.Material))
        {
            if (isExpand)
            {
                UpdateScrollPanel(matItems, matExpandScroll.content);
                if (hasUGCMat)
                {
                    switch (openUGCPage)
                    {
                        case (int)OpenUGCPage.UGCMaterial:
                            matTypeToggles[0].isOn = true;
                            return; 
                    }
                    if (matData != null)
                    {
                        matTypeToggles[matData.matGroupType + 1].isOn = true;
                    }
                }
                else
                {
                    if (matData != null)
                    {
                        matTypeToggles[matData.matGroupType].isOn = true;
                    }
                }
                
                
            }
            else
            {
                UpdateScrollPanel(matItems, matNormalScroll.content);
                UpdateMatExpandScroll(-1);
            }
        }
        else if (Equals(type,ShowType.Color))
        {
            if (isExpand)
            {
                UpdateScrollPanel(colorItems, colorExpandScroll.content);
            }
            else
            {
                UpdateScrollPanel(colorItems, colorNormalScroll.content);
            }
        }
    }
    public GameMatData GetMatDataById(int matId)
    {
        var matData = matDatas.Find(x => x.id == matId);
        return matData;
    }
    public void SetCurColorSelect(Color color)
    {
        int selIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(color), colorDatas.List);
        if (selIndex >= 0)
        {
            colorType = ColorSelectType.Normal;
            SetColorSelect(ColorSelectType.Normal, selIndex);
        }
        else
        {
            colorType = ColorSelectType.Customize;
            int index = GetCustomizeColorIndex(color);
            SetColorSelect(ColorSelectType.Customize, index);
        }
    }
    
    public int GetColorIndex(Color targetColor)
    {
        for(int i = 0; i < colorDatas.List.Count; i++)
        {
            Color color = colorDatas.List[i];
            // Equals 有时候会返回不正确
            // if (color.Equals(targetColor))
            if (color == targetColor)
            {
                return i;
            }
        }
        return -1;
    }

    public Color GetCurColor(ColorSelectType type,int index)
    {
        if(type == ColorSelectType.Customize)
        {
            return DataUtils.DeSerializeColor(customizeColors[index]);
        }
        else if(type == ColorSelectType.Normal)
        {
            return colorDatas.Get(index);
        }
        return Color.white;
    }
    public void SetSelectColor(int index,ColorSelectType type)
    {
        if (type == ColorSelectType.Normal)
        {
            Color color = colorDatas.Get(index);
            colorStr = DataUtils.ColorToString(color);
            SetColor(color);
            SetSliderColor(color);
            SetColorSelect(ColorSelectType.Normal, index);
            colorId = index;
        }
        else if (type == ColorSelectType.Customize)
        {
            Color color = DataUtils.DeSerializeColor(colorStr);
            var colorIndex = GetCustomizeColorIndex(color);
            SetColor(color);
            SetSliderColor(color);
            SetColorSelect(ColorSelectType.Customize, colorIndex);
            colorId = colorIndex;
        }
        colorType = type;
    }

    public void SetSelectColor(Color color,ColorSelectType type)
    {
        int index = type == ColorSelectType.Normal ? GetColorIndex(color):GetCustomizeColorIndex(color);
        SetColor(color);
        SetSliderColor(color);
        SetColorSelect(type, index);
        colorId = index;
        colorType = type; 
    }

    public void HideSpecialMat(int matId)
    {
        int removeIndex = -1;
        for (int i = 0; i < matDatas.Count; i++)
        {
            var matData = matDatas[i];
            if(matData.id == matId)
            {
                removeIndex = i;
                break;
            }
        }
        if(removeIndex != -1)
        {
            matDatas.RemoveAt(removeIndex);
        }
        
    }
}

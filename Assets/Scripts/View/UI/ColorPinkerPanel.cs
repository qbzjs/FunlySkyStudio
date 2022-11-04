using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GRTools.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Author:Meimei-LiMei
/// Description:UGC衣服自定义颜色UI界面
/// Date: 2022/6/10 13:11:29
/// </summary>
public class ColorPinkerPanel : MonoBehaviour
{
  public Slider HueSlider;//H Slide
  public Slider ChormaSlider;//V Slide
  public Slider BrightSlider;//S Slide
  public Image HueImg;//HImg
  public Image ChormaImg;//VImg
  public Image BrightImg;//SImg
  public Text HueText;
  public Text ChormaText;
  public Text BrightText;
  public Image colorImg;//左侧颜色展示Image
  public CommonColorToggleItem ColorPrefab;
  public Transform ColorItemParent;
  public Button SaveBtn;
  public Button DeleteBtn;
  public Action<Color> SetElementColor;
  private Color ReservedColor = new Color(0.9f, 0.9f, 0.9f);//预留位颜色
  private int MaxUgcColorCount = 30;//UGC颜色最大数量
  private List<string> UgcColors;//UGC颜色值集合
  public List<CommonColorToggleItem> UgcColorItems = new List<CommonColorToggleItem>();
  private void Awake()
  {
    HueText.GetComponent<LocalizationComponent>().OnChangeContent = OnLocalizationContent;
    ChormaText.GetComponent<LocalizationComponent>().OnChangeContent = OnLocalizationContent;
    BrightText.GetComponent<LocalizationComponent>().OnChangeContent = OnLocalizationContent;
  }
  public void Start()
  {
    HueSlider.onValueChanged.AddListener(OnHueSliderValueChanged);
    BrightSlider.onValueChanged.AddListener(OnBightSliderValueChanged);
    ChormaSlider.onValueChanged.AddListener(OnChormaSliderValueChanged);
    SaveBtn.onClick.AddListener(OnSaveBtnClick);
    DeleteBtn.onClick.AddListener(OnDeleteBtnClick);
    this.gameObject.SetActive(false);
    UgcColors = DataUtils.GetLocalUgcColors(GameInfo.Inst.myUid ?? string.Empty);
    InitSprite(); 
    InitColorItems();
    UpdateUgcColors();
  }
  /// <summary>
  /// 本地化截取字符
  /// </summary>
  public void OnLocalizationContent(LanguageCode languageCode, Component component, string text)
  {
    var hueText = GameUtils.SubStringByBytes(text, 12, Encoding.Unicode);
    component.GetComponent<Text>().text = hueText;
  }
   public void InitSprite()
  {
    HueImg.sprite=Sprite.Create(ColorPink.colorPink.HueTex, new Rect(Vector2.zero,new Vector2(7,1)), Vector2.one*0.5f);
    ChormaImg.sprite=Sprite.Create(ColorPink.colorPink.ChromaTex, new Rect(new Vector2(0.5f,0),Vector2.one), Vector2.one*0.5f);
    BrightImg.sprite=Sprite.Create(ColorPink.colorPink.BrightTex, new Rect(new Vector2(0.5f,0),Vector2.one), Vector2.one*0.5f);
    ColorPink.colorPink.SetHueSprite();
  }
  public void OnEnable()
  {
    SetColorHSV(PaintTool.Current.pColor);
  }
  /// <summary>
  /// 初始化颜色预留位
  /// </summary>
  public void InitColorItems()
  {
    for (int i = 0; i < MaxUgcColorCount; i++)
    {
      var item = GameObject.Instantiate(ColorPrefab, ColorItemParent);
      item.gameObject.SetActive(true);
      item.ColorCheckImage.SetActive(false);
      item.SetColor(ReservedColor, null);
      UgcColorItems.Add(item);
    }
  }
  /// <summary>
  /// 更新色盘颜色展示
  /// </summary>
  private void UpdateUgcColors()
  {
    for (int i = 0; i < UgcColorItems.Count; i++)
    {
      if (i < UgcColors.Count)
      {
        Color color;
        bool isCus = ColorUtility.TryParseHtmlString(UgcColors[i], out color);
        if (isCus)
        {
          UgcColorItems[i].SetColor(color, OnSelectColor);
        }
      }
      else
      {
        UgcColorItems[i].SetColor(ReservedColor, null);
      }
    }
    if (UgcColors.Count > MaxUgcColorCount / 2)
    {
      SetColorItemShow(true);
    }
    else
    {
      SetColorItemShow(false);
    }
  }
  /// <summary>
  /// 设置色盘展示（一行/两行）
  /// </summary>
  private void SetColorItemShow(bool isShow)
  {
    for (int i = MaxUgcColorCount / 2; i < UgcColorItems.Count; i++)
    {
      UgcColorItems[i].gameObject.SetActive(isShow);
    }
  }
  /// <summary>
  /// 设置颜色值对应的HSV
  /// </summary>
  public void SetColorHSV(Color color)
  {
    float H, S, V;
    Color.RGBToHSV(color, out H, out S, out V);
    HueSlider.value = H;
    BrightSlider.value = S;
    ChormaSlider.value = V;
    ColorPink.colorPink.UpdateBrightSprite(H, V);
    ColorPink.colorPink.UpdateChormaSprite(H, S);
    colorImg.color = color;
  }
  private Color GetColor()
  {
    var color = Color.HSVToRGB(HueSlider.value, BrightSlider.value, ChormaSlider.value);
    return color;
  }
   public void OnSelectColor(CommonColorToggleItem item)
  {
    if (PaintTool.Current.PgcCurItem != null)
    {
      PaintTool.Current.PgcCurItem.ColorCheckImage.SetActive(false);
    }
    PaintTool.Current.SetColor(item.color);
    if (PaintTool.Current.UgcCurItem == item)
    {
      return;
    }
    if (PaintTool.Current.UgcCurItem != null)
    {
      PaintTool.Current.UgcCurItem.ColorCheckImage.SetActive(false);
    }
    PaintTool.Current.UgcCurItem = item;
    SetColorHSV(item.color);
    SetElementColor?.Invoke(item.color);
  }
  private void OnHueSliderValueChanged(float value)
  {
    ColorPink.colorPink.UpdateBrightSprite(value, ChormaSlider.value);
    ColorPink.colorPink.UpdateChormaSprite(value, BrightSlider.value);
    colorImg.color = GetColor();
  }
  private void OnChormaSliderValueChanged(float value)
  {
    ColorPink.colorPink.UpdateBrightSprite(HueSlider.value, value);
    colorImg.color = GetColor();
  }
  private void OnBightSliderValueChanged(float value)
  {
    ColorPink.colorPink.UpdateChormaSprite(HueSlider.value, value);
    colorImg.color = GetColor();
  }
  private void OnSaveBtnClick()
  {
    if (UgcColors.Count >= MaxUgcColorCount)
    {
      TipPanel.ShowToast("Oops! Exceed limit:(");
      return;
    }
    var index = UgcColors.Count;
    UgcColorItems[index].SetColor(GetColor(), OnSelectColor);
     UgcColorItems[index].OnToggleClick();
    UgcColors.Add("#" + ColorUtility.ToHtmlStringRGBA(GetColor()));
    DataUtils.SetUgcClothsColors(GameInfo.Inst.myUid ?? string.Empty, UgcColors);
    if (UgcColors.Count == (MaxUgcColorCount / 2) + 1)
    {
      SetColorItemShow(true);
    }
  }
  private void OnDeleteBtnClick()
  {
    var item = PaintTool.Current.UgcCurItem;
    if (item != null)
    {
      UgcColors.Remove("#" + ColorUtility.ToHtmlStringRGBA(item.color));
      DataUtils.SetUgcClothsColors(GameInfo.Inst.myUid ?? string.Empty, UgcColors);
      UpdateUgcColors();
      if (PaintTool.Current.PgcCurItem != null)
      {
        PaintTool.Current.PgcCurItem.OnToggleClick();
      }
    }
  }
}

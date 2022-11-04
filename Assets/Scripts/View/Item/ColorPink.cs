using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:Meimei-LiMei
/// Description:Ugc衣服自定义颜色滑动条贴图设置
/// Date: 2022/6/13 11:59:56
/// </summary>
public class ColorPink
{
  private static ColorPink _colorPink;
  public static ColorPink colorPink => _colorPink ?? (_colorPink = new ColorPink());
  public Texture2D HueTex=new Texture2D(7,1);
  public Texture2D BrightTex=new Texture2D(2,1);
  public Texture2D ChromaTex=new Texture2D(2,1);
  public void UpdateTexPixels(Texture2D tex, Color[] colors)
  {
    tex.SetPixels(colors);
    tex.Apply();
  }
  public void SetHueSprite()
  {
    Color[] colors = new Color[]{
      new Color(1, 0.5f, 0),//橙
      new Color(1, 1, 0),//黄
      new Color(0, 1, 0),//绿
      new Color(0, 1, 1),//青
      new Color(0, 0, 1),//蓝
      new Color(1, 0, 1),//紫
      new Color(1, 0, 0)};//红
      UpdateTexPixels(HueTex,colors);
  }
  public void UpdateChormaSprite(float H,float S)
  {
    Color[] colors = new Color[]{
      Color.black,
      Color.HSVToRGB(H,S,1),
      };
    UpdateTexPixels(ChromaTex,colors);
  }
  public void UpdateBrightSprite(float H,float V)
  {
    var leftColor=Color.HSVToRGB(H,0,V);
    var rightColor=Color.HSVToRGB(H,1,V);
    Color[] colors = new Color[]{
      leftColor,
      rightColor
      };
    UpdateTexPixels(BrightTex,colors);
  }
}

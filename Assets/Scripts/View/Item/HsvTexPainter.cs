using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: Tee Li
/// 描述：Hsv三色条颜色设置类
/// 日期：2022/9/13
/// </summary>
public class HsvTexPainter
{
    public Texture2D HueTex { get; set; }
    public Texture2D BrightTex { get; set; }
    public Texture2D ChromaTex { get; set; }

    public HsvTexPainter()
    {
        HueTex = new Texture2D(7, 1);
        BrightTex = new Texture2D(2, 1);
        ChromaTex = new Texture2D(2, 1);
        SetHueSprite();
    }

    public void BindImages(Image hue, Image bright, Image chroma)
    {
        hue.sprite = Sprite.Create(HueTex, new Rect(Vector2.zero, new Vector2(7, 1)), Vector2.one * 0.5f);
        bright.sprite = Sprite.Create(BrightTex, new Rect(new Vector2(0.5f, 0), Vector2.one), Vector2.one * 0.5f);
        chroma.sprite = Sprite.Create(ChromaTex, new Rect(new Vector2(0.5f, 0), Vector2.one), Vector2.one * 0.5f);
    }

    public void Set(float h, float s, float v)
    {
        UpdateChormaSprite(h, s);
        UpdateBrightSprite(h, v);
    }   

    private void UpdateTexPixels(Texture2D tex, Color[] colors)
    {
        tex.SetPixels(colors);
        tex.Apply();
    }
    private void SetHueSprite()
    {
        Color[] colors = new Color[]
        {
            new Color(1, 0.5f, 0),//橙
            new Color(1, 1, 0),//黄
            new Color(0, 1, 0),//绿
            new Color(0, 1, 1),//青
            new Color(0, 0, 1),//蓝
            new Color(1, 0, 1),//紫
            new Color(1, 0, 0)//红
        };
        UpdateTexPixels(HueTex, colors);
    }
    private void UpdateChormaSprite(float H, float S)
    {
        Color[] colors = new Color[]
        {
            Color.black,
            Color.HSVToRGB(H,S,1),
        };
        UpdateTexPixels(ChromaTex, colors);
    }
    private void UpdateBrightSprite(float H, float V)
    {
        var leftColor = Color.HSVToRGB(H, 0, V);
        var rightColor = Color.HSVToRGB(H, 1, V);
        Color[] colors = new Color[]
        {
            leftColor,
            rightColor
        };
        UpdateTexPixels(BrightTex, colors);
    }

}

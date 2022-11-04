using GRTools.Localization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: 熊昭
/// Description: 翻译文本裁剪组件
/// Date: 2022/7/28 14:22:17
/// </summary>
[RequireComponent(typeof(Text), typeof(LocalizationComponent))]
public class TextEllipsisByWidth : MonoBehaviour
{
    public int widthLimit = 110;
    public int textCut = 4;
    private LocalizationComponent localComp;

    private void Awake()
    {
        localComp = GetComponent<LocalizationComponent>();
        localComp.OnChangeContent += OnChangeContent;
    }

    private void OnChangeContent(LanguageCode code, Component comp, string text)
    {
        var textComp = comp as Text;
        textComp.text = textComp.preferredWidth > widthLimit ? text.Substring(0, textCut) + "..." : text;
    }
}
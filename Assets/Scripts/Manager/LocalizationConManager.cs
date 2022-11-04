using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GRTools.Localization;
using System;
using System.IO;
using UnityEngine.UI;

public class SpecLangFont
{
    public string langCode;
    public string localeCode;
    public string fontPath;
}

/// <summary>
/// Author: 熊昭
/// Description: 本地化工具管理器：用于初始化本地化工具、设置文本翻译内容
/// Date: 2022-03-01 12:00:38
/// </summary>
public class LocalizationConManager : CInstance<LocalizationConManager>
{
    private LanguageCode systemLanguageCode;

    private List<SpecLangFont> specLangList;

    private Dictionary<LanguageCode, Font> specLangFont;

    private string loaderPath = Path.Combine("Localizations", "CsvLocalizationManifest");

    private LocalizationTextType parserType = LocalizationTextType.Csv;

    public static LanguageCode GetLanguageCode(string lang, string locale = "")
    {
        //暂不考虑地区码(预留)
        LanguageCode langCode;
        if (Enum.TryParse(lang, false, out langCode))
        {
            return langCode;
        }
        else
        {
            return LanguageCode.en;
        }
    }

    public void Initialize(string sysLangCode, string localeCode = "")
    {
        systemLanguageCode = GetLanguageCode(sysLangCode, localeCode);
        LocalizationLoader loader = new LocalizationResourcesLoader(loaderPath);
        LocalizationManager.Init(loader, parserType, systemLanguageCode);
        InitSpecLangFont();
    }

    private void InitSpecLangFont()
    {
        specLangList = new List<SpecLangFont>();
        specLangList = ResManager.Inst.LoadJsonRes<List<SpecLangFont>>("Configs/SpecialLanguageFont");

        specLangFont = new Dictionary<LanguageCode, Font>();
        for (int i = 0; i < specLangList.Count; i++)
        {
            var key = GetLanguageCode(specLangList[i].langCode, specLangList[i].localeCode);
            var value = ResManager.Inst.LoadRes<Font>(specLangList[i].fontPath);
            if (value == null)
            {
                LoggerUtils.LogError("Can not find special language font asset -- key: " + key);
                continue;
            }
            specLangFont.Add(key, value);
        }
    }

    /// <summary>
    /// 用于动态获取文案：需要配合字体设置
    /// </summary>
    /// <param name="localizationKey">本地化键(英文原文案)</param>
    /// <param name="formatArgs">设置格式项</param>
    /// <returns>翻译文案</returns>
    public string GetLocalizedText(string localizationKey, params object[] formatArgs)
    {
        if (LocalizationManager.Singleton == null)
        {
            return string.Format(localizationKey, formatArgs);
        }
        return string.Format(LocalizationManager.Singleton.GetLocalizedText(localizationKey, localizationKey), formatArgs);
    }

    /// <summary>
    /// 用于设置字体(暂设定为一次性设置)
    /// </summary>
    /// <param name="textCom">Text字体组件</param>
    public void SetSystemTextFont(Text textCom)
    {
        if (specLangFont == null || !specLangFont.ContainsKey(systemLanguageCode))
        {
            return;
        }
        if (textCom.font == specLangFont[systemLanguageCode])
        {
            return;
        }
#if UNITY_IPHONE
        //IOS才做字体切换
        textCom.font = specLangFont[systemLanguageCode];
#endif
        textCom.fontStyle = FontStyle.Bold;
    }

    /// <summary>
    /// 用于设置字体(暂设定为一次性设置)
    /// </summary>
    /// <param name="textCom">SuperTextMesh字体组件</param>
    public void SetSystemTextFont(SuperTextMesh textCom)
    {
        if (specLangFont == null || !specLangFont.ContainsKey(systemLanguageCode))
        {
            return;
        }
        if (textCom.font == specLangFont[systemLanguageCode])
        {
            return;
        }
#if UNITY_IPHONE
        //IOS才做字体切换
        textCom.font = specLangFont[systemLanguageCode];
#endif
        textCom.style = FontStyle.Bold;
    }

    /// <summary>
    /// 用于动态设置文案(强烈推荐使用)
    /// </summary>
    /// <param name="textCom">Text字体组件</param>
    /// <param name="localizationKey">本地化键(英文原文案)</param>
    /// <param name="formatArgs">设置格式项</param>
    public void SetLocalizedContent(Text textCom, string localizationKey, params object[] formatArgs)
    {
        SetSystemTextFont(textCom);
        textCom.text = GetLocalizedText(localizationKey, formatArgs);
    }

    /// <summary>
    /// 用于动态设置文案(强烈推荐使用)
    /// </summary>
    /// <param name="textCom">SuperTextMesh字体组件</param>
    /// <param name="localizationKey">本地化键(英文原文案)</param>
    /// <param name="formatArgs">设置格式项</param>
    public void SetLocalizedContent(SuperTextMesh textCom, string localizationKey, params object[] formatArgs)
    {
        SetSystemTextFont(textCom);
        textCom.text = GetLocalizedText(localizationKey, formatArgs);
    }

    public override void Release()
    {
        base.Release();
        LocalizationManager.Singleton = null;
    }
}
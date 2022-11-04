using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: pzkunn
/// Description: 本地化翻译组件
/// Date: 2022-05-30 17:34:16
/// </summary>
namespace GRTools.Localization
{
    public class LocalizationComponent : MonoBehaviour
    {
        [Tooltip("localKey")] public string localizationKey;
        /// <summary>
        /// 内容翻译消息：当前语言码/文本组件/文本内容
        /// </summary>
        public Action<LanguageCode, Component, string> OnChangeContent;

        private LanguageCode curLangCode;
        private Text textCom;
        private SuperTextMesh superTM;

        private void Start()
        {
            InitComponent();
            if (LocalizationManager.Singleton != null && LocalizationManager.Singleton.CurrentLocalizationInfo != null)
            {
                OnLocalizationChanged(LocalizationManager.Singleton.CurrentLocalizationInfo);
            }
            LocalizationManager.LocalizationChangeEvent += OnLocalizationChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.LocalizationChangeEvent -= OnLocalizationChanged;
        }

        private void InitComponent()
        {
            textCom = GetComponent<Text>();
            if (textCom == null)
            {
                superTM = GetComponent<SuperTextMesh>();
            }
        }

        private void OnLocalizationChanged(LocalizationInfo localizationInfo)
        {
            //获取当前语言码
            curLangCode = localizationInfo.LanguageType;
            if (string.IsNullOrEmpty(localizationKey))
            {
                LoggerUtils.LogError("Localization --> Localization key is not set...");
                return;
            }

            if (textCom != null || superTM != null)
            {
                string value = LocalizationConManager.Inst.GetLocalizedText(localizationKey);
                ChangeText(value);
                ChangeTextMesh(value);
            }
            else
            {
                LoggerUtils.LogError("Localization --> Text component is not found...");
            }
        }

        private void ChangeText(string value)
        {
            if (textCom == null) return;
            LocalizationConManager.Inst.SetSystemTextFont(textCom);
            textCom.text = value;
            OnChangeContent?.Invoke(curLangCode, textCom, value);
        }

        private void ChangeTextMesh(string value)
        {
            if (superTM == null) return;
            LocalizationConManager.Inst.SetSystemTextFont(superTM);
            superTM.text = value;
            OnChangeContent?.Invoke(curLangCode, superTM, value);
        }
    }
}
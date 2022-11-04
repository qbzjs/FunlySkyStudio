using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description:本地测试工具-测试素材加载
/// Date: 2022-3-30 19:43:08
/// </summary>
public partial class TestPanel : BasePanel<TestPanel>
{
#if UNITY_EDITOR
    
    private void OnLoadProp()
    {
        if (!IsInputNull(propJsonUrlInput))
        {
            LoggerUtils.Log("OnLoadProp Clicked, jsonUrl=" + propJsonUrlInput.text);
            BuildPropFromJson(propJsonUrlInput.text);
        }
    }

    private void BuildPropFromJson(string jsonUrl)
    {
        ClearScene();
        IEnumerator mapLoader = null;

        void LoadSuccess(string content)
        {
            LoggerUtils.Log("BuildFromJson success ->" + content);
            SaveToJsonUrlHistory(HistoryType.PROP_JSON, jsonUrl);
            SceneBuilder.Inst.ParsePropAndBuild(content, Vector3.zero);
        }
                
        void LoadFail(string error)
        {
            LoggerUtils.LogError("Get PropJson Fail => " + error);
            TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        }

        if (jsonUrl.Contains("ZipFile/") && jsonUrl.Contains(".zip"))
        {
            mapLoader = ResManager.Inst.GetZipContent(jsonUrl, LoadSuccess, LoadFail);
        }
        else
        {
            mapLoader = ResManager.Inst.GetContent(jsonUrl, LoadSuccess, LoadFail);
        } 
        StartCoroutine(mapLoader);
        
    }

    private void OnPrintPropJsonClick()
    {
        LoggerUtils.Log(SceneParser.Inst.StageToPropJson());
    }

    void OnOpenPropBtnClick()
    {
        propDropdown.Show();
    }

    void OnPropHistoryClear()
    {
        if (EditorUtility.DisplayDialog("Hey Dude!", "确定要清除Prop历史记录吗？？", "YES", "NO"))
        {
            PlayerPrefs.SetString(nameof(HistoryType.PROP_JSON), string.Empty);
            propHistory = InitHistoryString(HistoryType.PROP_JSON);
            RefreshHistoryDropdown(HistoryType.PROP_JSON);
            LoggerUtils.Log("记录清除成功！");
        }
    }

    void OnPropDropValueChanged(int i)
    {
        var choose = propDropdown.options[i].text;
        LoggerUtils.Log(string.Format("选择了历史记录=>{0}:{1}", i, choose));
        propJsonUrlInput.text = choose;
    }
    
#endif

}
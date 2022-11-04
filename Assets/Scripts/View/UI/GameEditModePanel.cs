using BasePrimitiveRedDotSystem;
using EmoRedDotSystem;
using Newtonsoft.Json;
using RedDot;
using SavingData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameEditModePanel:EditModePanel<GameEditModePanel>
{
    
    protected override void SaveMapCover(Action<string> success, Action<string> fail)
    {
        base.SaveMapCover(success,fail);
        if (GameManager.Inst.gameMapInfo == null)
        {
            string jsonUrl = SceneParser.Inst.StageToMapJson();
            string jsonPath = Application.streamingAssetsPath + "/123.json";
            File.WriteAllText(jsonPath, jsonUrl);
            fail?.Invoke(jsonUrl);
            return;
        }
        GameManager.Inst.gameMapInfo.dataType = GlobalFieldController.CurSceneType == SCENE_TYPE.MYSPACE_SCENE ?
            (int)MapSaveType.Space : (int)MapSaveType.Map;

        var isHasSetCover = GameManager.Inst.gameMapInfo.mapStatus.isSetCover;
        if (isHasSetCover)
        {
            EditModeController.SaveMapJson((fileName) =>
            {
                OnSaveMapJsonSuccess(fileName, success, fail);
            }, fail);
        }
        else
        {
            EditModeController.SaveMapCover((fileName) =>
            {
                OnSaveMapCoverSuccess(fileName, success, fail);
            }, fail);
        }
    }

    private void OnSaveMapCoverSuccess(string fileName, Action<string> success, Action<string> fail)
    {
        EditModeController.SaveMapJson((fileName) =>
        {
            OnSaveMapJsonSuccess(fileName, success, fail);
        }, fail);
    }

    private void OnSaveMapJsonSuccess(string fileName, Action<string> success, Action<string> fail)
    {
        var optType = string.IsNullOrEmpty(GameManager.Inst.gameMapInfo.mapId) ? OperationType.ADD : OperationType.UPDATE;
        DataUtils.SetMapInfoLocal(optType);
        DataUtils.SetConfigLocal(CoverType.JPG);
        success?.Invoke("Save Success");
    }

    public override void OnSaveAndQuit()
    {
        if (FPSController.Inst != null)
        {
            MobileInterface.Instance.LogEvent(LogEventData.unity_avg_fps, new SavingData.LogEventAvgFpsParam()
            {
                fps = Mathf.FloorToInt(FPSController.Inst.GetAverageFPS()),
            });
        }
        base.OnSaveAndQuit();
    }

    public override void OnQuit()
    {
        if (FPSController.Inst != null)
        {
            MobileInterface.Instance.LogEvent(LogEventData.unity_avg_fps, new SavingData.LogEventAvgFpsParam()
            {
                fps = Mathf.FloorToInt(FPSController.Inst.GetAverageFPS()),
            });
        }
        base.OnQuit();
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
       
    }
    //    protected override void SaveMapAndCover()
    //    {
    //        base.SaveMapAndCover();
    //        var isHasSetCover = GameManager.Inst.gameMapInfo.mapStatus.isSetCover;
    //        if (isHasSetCover)
    //        {
    //            EditModeController.SaveMapJson(OnSaveJsonSuccess, OnSaveJsonFail);
    //            return;
    //        }
    //        EditModeController.SaveMapCover(OnSaveCoverSuccess, OnSaveCoverFail);
    //    }
    //
    //
    //    private void OnSaveCoverSuccess(string imageUrl)
    //    {
    //        GameManager.Inst.gameMapInfo.mapCover = imageUrl;
    //        LoggerUtils.Log("OnSaveCoverSuccess  url = " + imageUrl);
    //        EditModeController.SaveMapJson(OnSaveJsonSuccess, OnSaveJsonFail);
    //    }
    //
    //    private void OnSaveJsonSuccess(string jsonUrl)
    //    {
    //        GameManager.Inst.gameMapInfo.mapJson = jsonUrl;
    //        //0(add),1(delete),2(publish),3(update))
    //        LoadAndSave.Inst.SetMapInfo(3, (x) => {
    //            TipPanel.ShowToast("Save Success");
    //            CloseSaveAnim();
    //        }, (x) => {
    //            TipPanel.ShowToast("Save Fail");
    //            CloseSaveAnim();
    //        });
    //    }
    //
    //    private void OnSaveJsonFail(string msg)
    //    {
    //        CloseSaveAnim();
    //    }
    //
    //    private void OnSaveCoverFail(string msg)
    //    {
    //        LoggerUtils.LogError("OnSaveCoverFail");
    //        TipPanel.ShowToast("Save Fail");
    //        CloseSaveAnim();
    //    }
    //
    //    protected override void OnSaveAndQuit()
    //    {
    //        base.OnSaveAndQuit();
    //        var isHasSetCover = GameManager.Inst.gameMapInfo.mapStatus.isSetCover;
    //        if (isHasSetCover)
    //        {
    //            EditModeController.SaveMapJson(OnSaveJsonSuccessAndQuit, OnSaveCoverFailAndQuit);
    //            return;
    //        }
    //        EditModeController.SaveMapCover(OnSaveCoverSuccessAndQuit, OnSaveCoverFailAndQuit);
    //    }
    //
    //    private void OnSaveCoverSuccessAndQuit(string imageUrl)
    //    {
    //        LoggerUtils.LogError("Cover Save Success");
    //        GameManager.Inst.gameMapInfo.mapCover = imageUrl;
    //        EditModeController.SaveMapJson(OnSaveJsonSuccessAndQuit, OnSaveCoverFailAndQuit);
    //    }
    //
    //    private void OnSaveCoverFailAndQuit(string err)
    //    {
    //        TipPanel.ShowToast("Save Fail");
    //        ComfirmPanel.SetAnim(false);
    //        MaskGo.gameObject.SetActive(false);
    //    }
    //
    //    private void OnSaveJsonSuccessAndQuit(string jsonUrl)
    //    {
    //        LoggerUtils.LogError("MapJson Save Success");
    //        GameManager.Inst.gameMapInfo.mapJson = jsonUrl;
    //        //0(add),1(delete),2(publish),3(update))
    //        LoadAndSave.Inst.SetMapInfo(3, (x) => {
    //            TipPanel.ShowToast("Save Success");
    //            ComfirmPanel.SetAnim(false);
    //            ComfirmPanel.Hide();
    //            StartCoroutine("OnQuitGame");
    //        }, (x) =>
    //        {
    //            OnSaveCoverFailAndQuit("");
    //        });
    //    }
}
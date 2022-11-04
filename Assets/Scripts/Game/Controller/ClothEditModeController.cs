using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

public class ClothEditModeController
{
    public static Camera screenShotCamera;

    public static void OnSaveClothTexByFirst(MapSaveType mapSaveType)
    {
#if !UNITY_EDITOR
        GameManager.Inst.gameMapInfo = new MapInfo();
        GameManager.Inst.gameMapInfo.mapName = GameManager.Inst.ugcUntiyMapDataInfo.mapName;
        GameManager.Inst.gameMapInfo.dataType = (int)mapSaveType;
        GameManager.Inst.gameMapInfo.dataSubType = GetSubType();
        
#else
        GameManager.Inst.gameMapInfo = new MapInfo();
        GameManager.Inst.gameMapInfo.mapName = "Hello";
        GameManager.Inst.gameMapInfo.dataType = (int)mapSaveType;
       
#endif
        OnSaveClothJsonByFirst(mapSaveType);
    }

    public static void OnSaveClothJsonByFirst(MapSaveType mapSaveType)
    {
        var clothData = UGCClothesDataManager.Inst.SaveAllClothesData();
        string content = JsonConvert.SerializeObject(clothData);
        DataUtils.SaveUgcResJsonToLocal( content,mapSaveType);
        //����mapId
        GameManager.Inst.gameMapInfo.mapId = GameManager.Inst.ugcUntiyMapDataInfo.mapId;
        GlobalFieldController.CurMapInfo = GameManager.Inst.gameMapInfo.Clone();
        if (string.IsNullOrEmpty(GlobalFieldController.CurMapInfo.mapId))
        {
            GlobalFieldController.CurMapInfo.mapId = GameInfo.Inst.myUid + "_" + GameUtils.GetTimeStamp() + "_cloth";
        }
        DataUtils.SetMapInfoLocal(OperationType.ADD);
        DataUtils.SetConfigLocal(CoverType.PNG);
    }

    public static void OnSaveClothTexSuccess(MapSaveType mapSaveType, Action onSuccess, Action onFail)
    {
        var clothData = UGCClothesDataManager.Inst.SaveAllClothesData();
        string content = JsonConvert.SerializeObject(clothData);
        if (string.IsNullOrEmpty(content))
        {
            LoggerUtils.LogError("Local Save Cloth Json --> Json Content is Empty!");
            onFail?.Invoke();
            return;
        }
        DataUtils.SaveUgcResJsonToLocal(content, mapSaveType);
        SaveClothCover(mapSaveType ,onSuccess, onFail);
    }

    private static void SaveClothCover(MapSaveType mapSaveType, Action onSuccess, Action onFail)
    {
        var screenShotRec = new Rect(0, 0, screenShotCamera.pixelWidth, screenShotCamera.pixelHeight);
        var bytes = ScreenShotUtils.TakeUGCClothShot(screenShotCamera, screenShotRec);
        if (bytes.Length == 0)
        {
            LoggerUtils.LogError("Local Save Cloth Cover --> Cover File is Empty!");
            onFail?.Invoke();
            return;
        }
        DataUtils.SaveCoverLocal(bytes, CoverType.PNG);
        SaveMapInfo(mapSaveType, onSuccess, onFail);
    }

    private static void SaveMapInfo(MapSaveType mapSaveType ,Action onSuccess, Action onFail)
    {
        GameManager.Inst.gameMapInfo.dataType = (int)mapSaveType;
        GameManager.Inst.gameMapInfo.imgs = UGCClothesPhotoManager.Inst.GetUrlArr();
        var optType = string.IsNullOrEmpty(GameManager.Inst.gameMapInfo.mapId) ? OperationType.ADD : OperationType.UPDATE;
        DataUtils.SetMapInfoLocal(optType);
        DataUtils.SetConfigLocal(CoverType.PNG);
        onSuccess?.Invoke();
    }

    public static void OnSaveClothInfoSuccess()
    {
        TipPanel.ShowToast("Saved successfully:D");
    }

    public static void OnSavClothInfoeSuccessAndQuit()
    {
        TipPanel.ShowToast("Saved successfully:D");
        ExitEditParams exitEditParams = new ExitEditParams()
        {
            mapId = GameManager.Inst.ugcUntiyMapDataInfo.mapId,
            draftPath = GameManager.Inst.ugcUntiyMapDataInfo.draftPath,
        };
        string quitPara = JsonConvert.SerializeObject(exitEditParams);
        LoggerUtils.Log("exitEditParams == " + quitPara);
        MobileInterface.Instance.Quit(quitPara);
    }

    public static void OnSaveClothInfoFail()
    {
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }
    public static int GetSubType()
    {
        switch (GameManager.Inst.engineEntry.sceneType)
        {
            case (int)SCENE_TYPE.UGCClothes:
                return (int)DataSubType.Clothes;
            case (int)SCENE_TYPE.UGCPatterns:
                return (int)DataSubType.Patterns;
            default:
                return 0;
        }
    }
}

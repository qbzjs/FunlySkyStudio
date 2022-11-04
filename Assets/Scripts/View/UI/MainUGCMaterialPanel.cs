using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DG.Tweening;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class MainUGCMaterialPanel : MainUGCResPanel
{
    public override void OnInit(MainUGCPanelResHandler handler)
    {
        base.OnInit(handler);
        SetButtonShow();

    }
    protected override void SetModel()
    {
        var modle = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/UGCMaterial/UGCMaterial_" + curUGCResId);
        ClothesModel = Instantiate(modle, ClothesModelParent);
        ClothesModel.name = "UGCRes";
        CloneModel = Instantiate(modle, CloneModelParent);
        var cloneModels = CloneModel.transform.childCount;
        for (int i = 0; i < cloneModels; i++)
        {
            CloneModel.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("SpecialModel");
        }
        CloneModel.name = "CloneUGCRes";
        screenShotCamera.transform.SetParent(CloneModel.transform);
        screenShotCamera.transform.localPosition = UGCClothesDataManager.Inst.ugcResShotDistance[curUGCResId];
    }
    protected override void SetPartTexture(int index, RenderTexture tex, RenderTexture alphaTex)
    {
        mapCreater.SetPartsTexture(allParts[index], tex, MapGridCreater.TextureType.Material);
        mapCreater.SetPartsTexture(allCloneParts[index], tex, MapGridCreater.TextureType.Material);
    }
    protected override void SetButtonShow()
    {
       
        ScissorsBtn.gameObject.SetActive(false);
    }

    public override void OnSaveUGCResByFirst()
    {
        if (allRenderTextures.Count != ugcDatas.Count)
        {
            LoggerUtils.LogError("ugcClothes RenderTexture is error");
            return;
        }
        
        SetFilesName();
        ClothEditModeController.OnSaveClothTexByFirst(MapSaveType.UGCMaterial);
    }
    protected override void SaveUGCRes(Action onSuccess, Action onFail)
    {
        if (allRenderTextures.Count != ugcDatas.Count)
        {
            LoggerUtils.LogError("ugcClothes RenderTexture is error");
            return;
        }

       SetFilesName();

       ClothEditModeController.OnSaveClothTexSuccess(MapSaveType.UGCMaterial,onSuccess, onFail);
    }
    private void SetFilesName()
    {
        if (!Directory.Exists(DataUtils.DraftPath))
        {
            Directory.CreateDirectory(DataUtils.DraftPath);
        }
        if (File.Exists(DataUtils.DraftPath + DataUtils.dataUrlName))
        {
            File.Delete(DataUtils.DraftPath + DataUtils.dataUrlName);
        }
       
        for (int i = 0; i < allFinalRenderTextures.Count; i++)
        {
            GenTexturePNG(DataUtils.DraftPath + DataUtils.dataUrlName, allFinalRenderTextures[i]);
        }
    }
}

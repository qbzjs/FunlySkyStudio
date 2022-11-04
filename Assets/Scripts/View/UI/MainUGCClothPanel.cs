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

public class MainUGCClothPanel : MainUGCResPanel
{
    public RoleController playerModelRoleCon;
    public RoleData playerModelRoleData;
    public GameObject PlayerModel;
    private Transform PlayerModelPar;
    private Toggle playerModeBtn;
    private Button OnChangeParts;
    private ClotheType clothType = ClotheType.Cloth;
    public FaceSaveCanvas faceSaveCamera;
    [HideInInspector]
    public FaceSaveCanvas faceSaveCam;


    protected List<GameObject> clothesMeshModels;

    [HideInInspector]
    public GameObject ClothesMeshModel;
    public override void OnInit(MainUGCPanelResHandler handler)
    {
        base.OnInit(handler);
        clothType = GetType();
        InitClothLoadRes(handler);
        InitClothUI();
        InitClothListener();
        SetButtonShow();

    }
    private void InitClothLoadRes(MainUGCPanelResHandler handler)
    {
        PlayerModel = handler.PlayerModel;
        faceSaveCamera = handler.faceSaveCamera;
    }
    private void InitClothUI()
    {
        PlayerModelPar = ClothesModelParent.transform.Find("PlayerModeModle");
        playerModeBtn = transform.Find("Panel/MenuTool/playermodeToggle").GetComponent<Toggle>();
        OnChangeParts = transform.Find("Panel/GenRawImageBG/GenRawImage/ClothesIcon").GetComponent<Button>();
        IconImage = OnChangeParts.transform.GetComponent<Image>();
    }
    private void InitClothListener()
    {
        playerModeBtn.onValueChanged.AddListener(OnPlayerModeChange);
        OnChangeParts.onClick.AddListener(OnChangePartsClick);
    }
    protected override void SetButtonShow()
    {
        playerModeBtn.gameObject.SetActive(true);
        OnChangeParts.gameObject.SetActive(true);
    }
    public override void GenerateUGCClothes()
    {
        base.GenerateUGCClothes();

        SetFaceMatShow();
        SetPlayerModel();
    }
    protected override void SetPartTexture(int index, RenderTexture tex, RenderTexture alphaTex)
    {
        mapCreater.SetPartsTexture(allParts[index], tex, MapGridCreater.TextureType.Cloth);
        mapCreater.SetPartsTexture(allCloneParts[index], tex, MapGridCreater.TextureType.Cloth);
        mapCreater.SetPartsTexture(allParts[index], alphaTex, MapGridCreater.TextureType.Cloth_Alpha);
        mapCreater.SetPartsTexture(allCloneParts[index], alphaTex, MapGridCreater.TextureType.Cloth_Alpha);
    }

    protected override void SetModel()
    {
        var modle = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/UGCClothes/UGCClothes_" + curUGCResId);
        ClothesModel = Instantiate(modle, ClothesModelParent);
        ClothesModel.name = "UGCRes";
        CloneModel = Instantiate(modle, CloneModelParent);
        var cloneModels = CloneModel.transform.childCount;
        for (int i = 0; i < cloneModels; i++)
        {
            CloneModel.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("SpecialModel");

        }
        var face = CloneModel.transform.Find(UGCClothesDataManager.faceName);
        if (face != null)
        {
            face.gameObject.layer = LayerMask.NameToLayer("SpecialModel");
        }
        CloneModel.name = "CloneUGCRes";
        screenShotCamera.transform.SetParent(CloneModel.transform);
        screenShotCamera.transform.localPosition = UGCClothesDataManager.Inst.ugcResShotDistance[curUGCResId];
        var modleMesh = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/UGCClothes/UGCClothesMesh_" + curUGCResId);
        ClothesMeshModel = Instantiate(modleMesh, ClothesModelParent);
        ClothesMeshModel.name = "UGCResMesh";
    }
    protected override void InitAllParts()
    {
        base.InitAllParts();
        clothesMeshModels = new List<GameObject>();
        for (int i = 0; i < ugcDatas.Count; i++)
        {
            var clothesMesh = ClothesMeshModel.transform.Find(ugcDatas[i].partsName + "_mesh").gameObject;
            clothesMeshModels.Add(clothesMesh);
        }
    }
    public void OnChangePartsClick()
    {
        var nexIndex = (curClothesIndex + 1) % ugcDatas.Count;
        OnSelectHandler(allParts[nexIndex]);
    }
    //外部切换面片时调用
    public override void ChangeParts(GameObject selectPart)
    {
        OnSelectHandler(selectPart);
        StartUGCTween(ugcDatas[curClothesIndex].rotAngle);
    }
    protected override void OnSelectHandler(GameObject hit)
    {
        base.OnSelectHandler(hit);
        var index = ugcDatas.FindIndex(x => x.partsName == hit.name);
        var data = ugcDatas[index];
        IconImage.sprite = ugcAtlas.GetSprite(data.iconSpriteName);
    }
    protected override void SetHightLight(int lastIndex, int curIndex)
    {
        clothesMeshModels[lastIndex].SetActive(false);
        clothesMeshModels[curIndex].SetActive(true);
    }
 

    //将ui图的rt赋值给面片
    public void SetFaceMatShow()
    {
        if (clothType == ClotheType.Face)
        {
            SetFaceSaveCam();
            var partsMat = ClothesModel.transform.Find(UGCClothesDataManager.faceName).GetComponent<MeshRenderer>().material;
            partsMat.SetTexture("_patterns_tex", faceSaveCam.rt);
            partsMat = CloneModel.transform.Find(UGCClothesDataManager.faceName).GetComponent<MeshRenderer>().material;
            partsMat.SetTexture("_patterns_tex", faceSaveCam.rt);
        }

    }
    private void SetFaceSaveCam()
    {
        faceSaveCam = Instantiate(faceSaveCamera, ImportCanvasParent);
        faceSaveCam.transform.localPosition = new Vector3(0, 30, 0);
        faceSaveCam.SetRawTexture(allFinalRenderTextures, allRenderAlphaTextures, ugcDatas);
    }
    #region 人物模型参照
    private void OnPlayerModeChange(bool isOn)
    {
        PlayerModelPar.gameObject.SetActive(isOn);
        ClothesModel.SetActive(!isOn);
        ClothesMeshModel.SetActive(!isOn);
        if (isOn)
        {
            playerModelRoleCon.StartEyeAnimation(playerModelRoleData.eId);
        }
    }
    private void SetPlayerModel()
    {
        GameObject model = Instantiate(PlayerModel);
        model.transform.SetParent(PlayerModelPar);
        playerModelRoleCon = model.GetComponent<RoleController>();
      
        var modelCon = model.GetComponent<UGCClothModelController>();
        if (modelCon == null)
        {
            modelCon = model.AddComponent<UGCClothModelController>();
        }

#if !UNITY_EDITOR
        playerModelRoleData = JsonConvert.DeserializeObject<RoleData>(GameManager.Inst.ugcUserInfo.imageJson);
#else
        playerModelRoleData = JsonConvert.DeserializeObject<RoleData>(File.ReadAllText(Application.streamingAssetsPath + "/qqq.json"));
#endif
        if (playerModelRoleCon != null && playerModelRoleData != null)
        {

            var type = GetType();

            switch (type)
            {
                case ClotheType.Cloth:
                    playerModelRoleCon.InitRoleByData(playerModelRoleData, RoleController.InitRoleType.UGCPlayerModelCloth);
                    modelCon.SetModelUGCCloth(playerModelRoleCon, allRenderAlphaTextures, allFinalRenderTextures, curUGCResId);
                    model.transform.localPosition = Vector3.zero;
                    model.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                    break;
                case ClotheType.Face:
                    playerModelRoleCon.InitRoleByData(playerModelRoleData, RoleController.InitRoleType.UGCPlayerModelFace);
                    modelCon.SetModelUGCFace(model, faceSaveCam.rt);
                    model.transform.localPosition = new Vector3(0, -0.27f, 0);
                    model.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    break;
            }
        }
    }
    #endregion

    public override void OnUndo(UGCClothDrawUndoData helpData)
    {
        if (helpData.selectPart != curSelectPart)
        {
            OnSelectHandler(helpData.selectPart);
            StartUGCTween(ugcDatas[curClothesIndex].rotAngle);
        }
        base.OnUndo(helpData);
    }

    protected override void On2DDrawPointerDown()
    {
        clothesMeshModels[curClothesIndex].SetActive(false);
    }

    #region 保存

    public override void OnSaveUGCResByFirst()
    {
        if (!Directory.Exists(DataUtils.ugcClothesDataDir))
        {
            Directory.CreateDirectory(DataUtils.ugcClothesDataDir);
        }

        if (allRenderTextures.Count != ugcDatas.Count)
        {
            LoggerUtils.LogError("ugcClothes RenderTexture is error");
            return;
        }

        List<string> files = null;
        if (clothType == ClotheType.Face)
        {

            files = SetFaceFilesName();
        }
        else if (clothType == ClotheType.Cloth)
        {
            files = SetFilesName();
        }

        ZipUtils.SaveClothZipLocal(files, (obj) =>
        {
            string zipName = obj as string; // "clothTex.zip"
            if (string.IsNullOrEmpty(zipName))
            {
                LoggerUtils.LogError("Local Save Cloth Tex Zip By First --> Zip File Failed!");
                return;
            }
            ClothEditModeController.OnSaveClothTexByFirst( MapSaveType.UGCRes);
        });
    }

    protected override void SaveUGCRes(Action onSuccess, Action onFail)
    {
        if (!Directory.Exists(DataUtils.ugcClothesDataDir))
        {
            Directory.CreateDirectory(DataUtils.ugcClothesDataDir);
        }

        if (allRenderTextures.Count != ugcDatas.Count)
        {
            LoggerUtils.LogError("ugcClothes RenderTexture is error");
            return;
        }

        List<string> files = null;
        if (clothType == ClotheType.Face)
        {

            files = SetFaceFilesName();
        }
        else if (clothType == ClotheType.Cloth)
        {
            files = SetFilesName();
        }
        ZipUtils.SaveClothZipLocal(files, (obj) =>
        {
            string zipName = obj as string; // "clothTex.zip"
            if (string.IsNullOrEmpty(zipName))
            {
                LoggerUtils.LogError("Local Save Cloth Tex Zip --> Zip File Failed!");
                onFail?.Invoke();
                return;
            }
            ClothEditModeController.OnSaveClothTexSuccess(MapSaveType.UGCRes,onSuccess, onFail);
        });
    }
    private List<string> SetFilesName()
    {
        List<string> files = new List<string>();
        for (int i = 0; i < allFinalRenderTextures.Count; i++)
        {
            StringBuilder namebuilder = new StringBuilder();
            namebuilder.Append(DataUtils.GetResNameWithoutEx())
                .Append("_")
                .Append(curUGCResId)
                .Append("_")
                .Append(ugcDatas[i].ugcType)
                .Append(extension);
            files.Add(namebuilder.ToString());
            GenTexturePNG(DataUtils.ugcClothesDataDir + namebuilder.ToString(), allFinalRenderTextures[i]);
        }
        for (int i = 0; i < allRenderAlphaTextures.Count; i++)
        {
            StringBuilder namebuilder = new StringBuilder();
            namebuilder.Append(DataUtils.GetResNameWithoutEx())
                .Append("_")
                .Append(curUGCResId)
                .Append("_")
                .Append(ugcDatas[i].ugcType)
                .Append("_")
                .Append(DataUtils.ScissorsName)
                .Append(extension);
            files.Add(namebuilder.ToString());
            GenTexturePNG(DataUtils.ugcClothesDataDir + namebuilder.ToString(), allRenderAlphaTextures[i]);
        }
        return files;
    }
    protected List<string> SetFaceFilesName()
    {
        List<string> files = new List<string>();
        StringBuilder namebuilder = new StringBuilder();
        namebuilder.Append(DataUtils.GetResNameWithoutEx())
            .Append("_")
            .Append(curUGCResId)
            .Append(extension);
        files.Add(namebuilder.ToString());
        GetFaceGenTexturePNG(DataUtils.ugcClothesDataDir + namebuilder.ToString());
        return files;
    }
    private void GetFaceGenTexturePNG(string filePath)
    {
        var bytes = faceSaveCam.GetTexture();
        File.WriteAllBytes(filePath, bytes);
    }

    protected override void OnSaveBtnClick()
    {
        clothesMeshModels[curClothesIndex].SetActive(false);
        base.OnSaveBtnClick();
    }

    protected override void OnSaveAndQuitClick()
    {
        clothesMeshModels[curClothesIndex].SetActive(false);
        base.OnSaveAndQuitClick();
    }

    #endregion
    /// <summary>
    /// id为模板id
    /// </summary>
    /// <returns></returns>
    public ClotheType GetType()
    {
        if (curUGCResId < 1000)
        {
            return ClotheType.Cloth;
        }
        else if (curUGCResId > 1000 && curUGCResId < 2000)
        {
            return ClotheType.Face;
        }
        return ClotheType.Cloth;
    }
}

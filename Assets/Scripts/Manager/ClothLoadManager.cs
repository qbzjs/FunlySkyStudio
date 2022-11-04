using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SavingData;
using UnityEngine;
using UnityEngine.UI;
public class LoadUgcResData
{
    public string clothesUrl;
    public int templateId;
}

public class ClothLoadManager : InstMonoBehaviour<ClothLoadManager>
{
    private Vector2Int texSize = new Vector2Int(128, 128);
    private Dictionary<string, Dictionary<string, byte[]>> clothTexDic = new Dictionary<string, Dictionary<string, byte[]>>();
    private Dictionary<string, Texture2D> ugcTexDict = new Dictionary<string, Texture2D>(); // 缓存UGC衣服贴图资源
    
    public void LoadUGCClothRes(ClothStyleData data, RoleController roleComp,Action onSuccess = null, Action onFail = null)
    {
      
        var clothUrl = data.clothesUrl;
        var templateId = data.templateId;
        if (string.IsNullOrEmpty(clothUrl)) return;
        if (clothTexDic.ContainsKey(clothUrl) && clothTexDic[clothUrl] != null)
        {
            var texBytes = clothTexDic[clothUrl];
            SetUGCClothByBytes(roleComp, texBytes, templateId);
            onSuccess?.Invoke();
        }
        else
        {
            StartCoroutine(GameUtils.GetByte(clothUrl, (bytes) =>
            {
                var texBytes = ZipUtils.UnpackFiles(bytes);
                if (!clothTexDic.ContainsKey(clothUrl))
                {
                    clothTexDic.Add(clothUrl, texBytes);
                }
                else {
                    clothTexDic[clothUrl] = texBytes;
                }
                SetUGCClothByBytes(roleComp, texBytes, templateId);
                onSuccess?.Invoke();
            }, (error) =>
            {
                onFail?.Invoke();
                LoggerUtils.LogError("LoadUGCClothRes Fail err = " + error);
            }));
        }
    }

    private void SetUGCClothByBytes(RoleController roleComp, Dictionary<string, byte[]> texBytes, int templateId)
    {
        var ugcDatas = UGCClothesDataManager.Inst.GetConfigClothesDataByID(templateId);
        var ugcBoneParent = roleComp.InitUgcClothBone(templateId);
        if (ugcBoneParent == null)
        {
            LoggerUtils.LogError("SetUGCClothByBytes=>ugcBoneParent==null:" + templateId);
            return;
        }
        foreach (var item in ugcDatas)
        {
            var partsGo = ugcBoneParent.transform.Find(item.partsName);
            if (partsGo != null)
            {
                var partsMat = partsGo.GetComponent<SkinnedMeshRenderer>().material;
                partsMat.SetTexture("_opacity_texmask", null);
            }
        }
        foreach (var kValue in texBytes)
        {
            string nameWithoutEx = Path.GetFileNameWithoutExtension(kValue.Key);
            string[] nameData = nameWithoutEx.Split('_');
            int ugcType;
            string MatTextureName = "_MainTex";
            if (nameData[nameData.Length - 1].CompareTo(DataUtils.ScissorsName)!=0)
            {
                ugcType = int.Parse(nameData[nameData.Length - 1]);
            }
            else
            {
                ugcType = int.Parse(nameData[nameData.Length - 2]);
                MatTextureName = "_opacity_texmask";
            }
            var partsdata = ugcDatas.Find(x => x.ugcType == ugcType);
            if (partsdata != null)
            {
                var partsGo = ugcBoneParent.transform.Find(partsdata.partsName);
                var partsMat = partsGo.GetComponent<SkinnedMeshRenderer>().material;
                Texture2D tex;
                if (!ugcTexDict.TryGetValue(kValue.Key, out tex))
                {
                    tex = new Texture2D(texSize.x, texSize.y);
                    tex.LoadImage(kValue.Value);
                    ugcTexDict.Add(kValue.Key, tex);
                }
                partsMat.SetTexture(MatTextureName, tex);
            }
        }
        TryOnShowFrame(roleComp);
    }
    //！！只有TryOn场景用到了这个方法(加载完人物形象关闭第一帧)
    private void TryOnShowFrame(RoleController roleComp)
    {
#if !UNITY_EDITOR
        if (GameManager.Inst.engineEntry.sceneType == (int)SCENE_TYPE.CPreview || GameManager.Inst.engineEntry.sceneType == (int)SCENE_TYPE.FPPreview)
        {
            roleComp.ShowFrame?.Invoke();
        }
#endif
    }

    #region Add by Shaocheng : 给场景内UGC衣服道具换装

    public void LoadUGCClothRes(ClothStyleData data, GameObject go, Action onSuccess = null, Action onFail = null)
    {
        var clothUrl = data.clothesUrl;
        var templateId = data.templateId;
        if (string.IsNullOrEmpty(clothUrl)) return;
        if (clothTexDic.ContainsKey(clothUrl) && clothTexDic[clothUrl] != null)
        {
            var texBytes = clothTexDic[clothUrl];
            SetUGCClothByBytes(go, texBytes, templateId);
            onSuccess?.Invoke();
        }
        else
        {
            StartCoroutine(GameUtils.GetByte(clothUrl, (bytes) =>
            {
                var texBytes = ZipUtils.UnpackFiles(bytes);
                if (!clothTexDic.ContainsKey(clothUrl))
                {
                    clothTexDic.Add(clothUrl, texBytes);
                }
                else {
                    clothTexDic[clothUrl] = texBytes;
                }
                SetUGCClothByBytes(go, texBytes, templateId);
                onSuccess?.Invoke();
            }, (error) =>
            {
                onFail?.Invoke();
                LoggerUtils.LogError("LoadUGCClothRes Fail err = " + error);
            }));
        }
    }
    
    private void SetUGCClothByBytes(GameObject go, Dictionary<string, byte[]> texBytes, int templateId)
    {
        var ugcDatas = UGCClothesDataManager.Inst.GetConfigClothesDataByID(templateId);
        var ugcBoneParent = go;
        foreach (var kValue in texBytes)
        {
            string nameWithoutEx = Path.GetFileNameWithoutExtension(kValue.Key);
            string[] nameData = nameWithoutEx.Split('_');
            int ugcType;
            string MatTextureName = "_MainTex";
            if (nameData[nameData.Length - 1].CompareTo(DataUtils.ScissorsName) != 0)
            {
                ugcType = int.Parse(nameData[nameData.Length - 1]);
            }
            else
            {
                ugcType = int.Parse(nameData[nameData.Length - 2]);
                MatTextureName = "_opacity_texmask";
            }
            var partsdata = ugcDatas.Find(x => x.ugcType == ugcType);
            if (partsdata != null)
            {
                var partsGo = ugcBoneParent.transform.Find(partsdata.partsName);
                var partsMat = partsGo.GetComponent<SkinnedMeshRenderer>().material;
                Texture2D tex;
                if (!ugcTexDict.TryGetValue(kValue.Key, out tex))
                {
                    tex = new Texture2D(texSize.x, texSize.y);
                    tex.LoadImage(kValue.Value);
                    ugcTexDict.Add(kValue.Key, tex);
                }
                partsMat.SetTexture(MatTextureName, tex);
            }
        }
    }

    #endregion

    #region 加载UGC面部彩绘资源 场景内导入，avatar场景共用
    public void LoadUGCPatternsRes(LoadUgcResData data, GameObject go, Action onSuccess = null, Action onFail = null)
    {
        var clothUrl = data.clothesUrl;
        var templateId = data.templateId;
        if (string.IsNullOrEmpty(clothUrl)) return;
        if (clothTexDic.ContainsKey(clothUrl) && clothTexDic[clothUrl] != null)
        {
            var texBytes = clothTexDic[clothUrl];
            FindUgcFaceGo(go, texBytes);
            onSuccess?.Invoke();
        }
        else
        {
            StartCoroutine(GameUtils.GetByte(clothUrl, (bytes) =>
            {
                var texBytes = ZipUtils.UnpackFiles(bytes);
                if (!clothTexDic.ContainsKey(clothUrl))
                {
                    clothTexDic.Add(clothUrl, texBytes);
                }
                else
                {
                    clothTexDic[clothUrl] = texBytes;
                }
                FindUgcFaceGo(go, texBytes);
                onSuccess?.Invoke();
            }, (error) =>
            {
                onFail?.Invoke();
                LoggerUtils.LogError("LoadUGCClothRes Fail err = " + error);
            }));
        }
    }
    private void FindUgcFaceGo(GameObject go, Dictionary<string, byte[]> texBytes)
    {
        var partsGo = go.transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 Head/ugc_face");
        if (partsGo)
        {
            SetUGCPatternByBytes(partsGo.gameObject, texBytes);
        }
        //场景内导入时无roleComp
        RoleController rolecomp = go.GetComponent<RoleController>();
        if (rolecomp)
        {
            TryOnShowFrame(rolecomp);
        }
    }
    private void SetUGCPatternByBytes(GameObject ugc_face, Dictionary<string, byte[]> texBytes)
    {
        ugc_face.SetActive(true);
        foreach (var kValue in texBytes)
        {
            string MatTextureName = "_patterns_tex";
            var mesh = ugc_face.GetComponent<MeshRenderer>();
            Texture2D tex;
            if (!ugcTexDict.TryGetValue(kValue.Key, out tex))
            {
                tex = new Texture2D(texSize.x, texSize.y);
                tex.LoadImage(kValue.Value);
                ugcTexDict.Add(kValue.Key, tex);
            }
            tex.wrapMode = TextureWrapMode.Clamp;
            mesh.material.SetTexture(MatTextureName, tex);
        }
    }
    #endregion
    private void OnDestroy()
    {
        ugcTexDict.Clear();
        inst = null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UGCClothModelController : MonoBehaviour
{
    public void SetModelUGCCloth()
    {
        
    }
    public void SetModelUGCFace(GameObject go, RenderTexture tex)
    {
        var partsGo = go.transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 Head/ugc_face");
        if (partsGo)
        {
            SetUGCPattern(partsGo.gameObject, tex);
        }
    }
    private void SetUGCPattern(GameObject ugc_face, RenderTexture tex)
    {
        ugc_face.SetActive(true);
        string MatTextureName = "_patterns_tex";
        var mesh = ugc_face.GetComponent<MeshRenderer>();
        mesh.material.SetTexture(MatTextureName, tex);
       
    }
    public void SetModelUGCCloth(RoleController roleComp ,List<RenderTexture> alphaRenderTextures, List<RenderTexture>renderTextures, int templateId)
    {
        var ugcDatas = UGCClothesDataManager.Inst.GetConfigClothesDataByID(templateId);
        var ugcBoneParent = roleComp.InitUgcClothBone(templateId);
        if (ugcBoneParent == null)
        {
            LoggerUtils.LogError("SetUGCClothByBytes=>ugcBoneParent==null:" + templateId);
            return;
        }
        for (int i = 0; i < ugcDatas.Count; i++)
        {
            var partsGo = ugcBoneParent.transform.Find(ugcDatas[i].partsName);
            if (partsGo != null)
            {
                var partsMat = partsGo.GetComponent<SkinnedMeshRenderer>().material;
                partsMat.SetTexture("_MainTex", renderTextures[i]);
                partsMat.SetTexture("_opacity_texmask", alphaRenderTextures[i]);
            }
        }
    }
}

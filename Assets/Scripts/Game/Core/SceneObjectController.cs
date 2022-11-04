using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SceneObjectController
{
    public static void SetBaseModelColor(NodeBaseBehaviour baseBehaviour, Color color)
    {
        var behav = baseBehaviour as NodeBehaviour;
        behav.SetColor("_Color", color);
    }

    public static void SetBaseModelAtr(NodeBaseBehaviour baseBehaviour, int matId, Color color)
    {
        var behav = baseBehaviour as NodeBehaviour;
        var matData = GameManager.Inst.matConfigDatas.Find(x => x.id == matId);
        //var color = AssetLibrary.Inst.colorLib.Get(colorId);
        bool isTransparent = matId == 1;
        var mat = isTransparent ? GameManager.Inst.BaseModelMats[1] : GameManager.Inst.BaseModelMats[0];
        behav.SetMatetial(isTransparent, mat);
        //TODO: FEAT 发光材质
        // bool isEmission = matId == GameConsts.EMISSION_MAT_ID;
        // var mat = GameManager.Inst.BaseModelMats[0];
        // if(isTransparent)
        // {
        //     mat = GameManager.Inst.BaseModelMats[1];
        // }
        // else if(isEmission)
        // {
        //     mat = GameManager.Inst.BaseModelMats[2];
        // }
        // behav.SetMatetial(isTransparent, mat, isEmission);
        var tex = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + matData.texName);
        var normalMap = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + matData.texName + "_normal");
        if (!GameManager.Inst.IsGPUInstance)
        {
            behav.SetMatAttribute(tex, matData.smoothness, matData.metallic, color, normalMap);
        }
        else
        {
            behav.SetGPUMatAttribute(matId, matData.smoothness, matData.metallic, color);
        }
    }
    
    public static void SetUGCBaseModelAtr(NodeBaseBehaviour baseBehaviour, int matId, Color color, string uurl)
    {
        var behav = baseBehaviour as NodeBehaviour;
        if(string.IsNullOrEmpty(uurl))
        {
            SetBaseModelAtr(baseBehaviour, matId, color);
            return;
        }
        UGCTexManager.Inst.GetUGCTex(uurl, (tex)=>{
            behav.isTransparent = false;
            behav.SetMatAttribute(tex, 0.5f, 0, color, null);
        });
    }

    public static void InitBaseModelTile(NodeBaseBehaviour baseBehaviour, Vector2 tiling)
    {
        var behav = baseBehaviour as NodeBehaviour;
        behav.InitTiling(tiling);
    }

    public static SceneEntity GetCanControllerNode(GameObject hitGo)
    {
        var nodeBehav = hitGo.GetComponentInParent<NodeBaseBehaviour>();
        var entity = GetCanControllerEntity(nodeBehav);
        return entity;
    }

    private static SceneEntity GetCanControllerEntity(NodeBaseBehaviour nodeBehav)
    {
        if (nodeBehav == null)
        {
            //LoggerUtils.Log("node isError");
            return null;
        }
        var parent = nodeBehav.transform.parent;
        if(parent != null)
        {
            var parBehav = parent.GetComponentInParent<CombineBehaviour>();
            if (parBehav != null)
            {
                return GetCanControllerEntity(parBehav);
            }
            var fishingBev = parent.GetComponentInParent<FishingBehaviour>();
            if (fishingBev != null)
            {
                return GetCanControllerEntity(fishingBev);
            }
            var swingBev = parent.GetComponentInParent<SwingBehaviour>();
            if (swingBev != null)
            {
                return GetCanControllerEntity(swingBev);
            }
        }
        return nodeBehav.entity;
    }

}
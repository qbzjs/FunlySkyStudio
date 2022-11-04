using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Game.Core;
using HLODSystem;
using Leopotam.Ecs;
using UnityEngine;

public class NodeBehaviour : BaseHLODBehaviour
{
    public static MaterialPropertyBlock mpb;
    [HideInInspector]
    public Renderer[] renderers;
    [HideInInspector]
    public bool isTransparent = false;
    //TODO: FEAT 发光材质
    // public bool isEmission = false;
    private Color[] colors;
    private MaterialPropertyBlock defaultMpb;
    private float tAlpha = 0.36f;
    private Texture defNormalMap;
    private ColorMatData colorMatData;

    private Texture ugcTexture;

    public override void OnInitByCreate()
    {
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
        defaultMpb = new MaterialPropertyBlock();
        renderers = GetComponentsInChildren<Renderer>();
        allRenderers = renderers;
        defNormalMap = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + "default_normal");
    }

    public override void OnReset()
    {
        for (var i = 0; i < renderers.Length; i++)
        {
            renderers[i].SetPropertyBlock(defaultMpb);
        }
        colorMatData = null;
        base.OnReset();
    }

    public void SetMatetial(bool isTrans,Material mat)
    {
        isTransparent = isTrans;
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = mat;
        }
    }

    //TODO: FEAT 发光材质
    // public void SetMatetial(bool isTrans, Material mat, bool isEmi)
    // {
    //     isEmission = isEmi;
    //     SetMatetial(isTrans, mat);
    // }

    public void SetColor(string colorName,Color color)
    {
        if(renderers.Length <= 0)
            return;
        renderers[0].GetPropertyBlock(mpb);
        if (isTransparent)
        {
            color.a = tAlpha;
        }
        mpb.SetColor(colorName, color);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].SetPropertyBlock(mpb);
        }
    }

    public void SetScale(Vector3 scale, Vector3 lastScale)
    {
        if (renderers.Length <= 0)
            return;
        if (isTransparent || renderers.Length <= 0)
            return;
        Vector3 change = scale - lastScale;
        renderers[0].GetPropertyBlock(mpb);
        var tiling = mpb.GetVector("_MainTex_ST");
        tiling.y += change.y;
        tiling.x += Mathf.Abs(change.x) > Mathf.Abs(change.z) ? change.x : change.z;
        if (tiling.y < 0) tiling.y = 0;
        if (tiling.x < 0) tiling.x = 0;
        
        mpb.SetVector("_MainTex_ST", new Vector4(tiling.x, tiling.y, 0, 0));
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].SetPropertyBlock(mpb);
        }
    }

    public void InitTiling(Vector2 tiling)
    {
        //透明材质无效
        if (isTransparent || renderers.Length <= 0)
        {
            return;
        }
        
        renderers[0].GetPropertyBlock(mpb);
        mpb.SetVector("_MainTex_ST", new Vector4(tiling.x, tiling.y, 0, 0));
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].SetPropertyBlock(mpb);
        }
    }

    public void SetTiling(Vector2 tiling)
    {
        if (renderers.Length <= 0)
        {
            return;
        }
        renderers[0].GetPropertyBlock(mpb);
        mpb.SetVector("_MainTex_ST", new Vector4(tiling.x, tiling.y, 0, 0));
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].SetPropertyBlock(mpb);
        }
    }

    public void SetMatAttribute(Texture tex, float smooth, float metal, Color color,Texture normalMap = null)
    {
        if (renderers.Length <= 0)
            return;
        renderers[0].GetPropertyBlock(mpb);
        if (isTransparent)
        {
            color.a = tAlpha;
        }
		ugcTexture = tex;
        //TODO: FEAT 发光材质
        // if(!isEmission)
        // {
        
        mpb.SetTexture("_MainTex", tex);
        // }
        mpb.SetFloat("_Glossiness", smooth);
        mpb.SetFloat("_Metallic", metal);
        mpb.SetColor("_Color", color);
        if(normalMap != null) {
            mpb.SetTexture("_BumpMap", normalMap);
        }
        else
        {
            mpb.SetTexture("_BumpMap", defNormalMap);
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].SetPropertyBlock(mpb);
        }
    }
    public void SetUGCMat(Texture2D tex)
    {
        if (renderers.Length <= 0)
            return;
        renderers[0].GetPropertyBlock(mpb);
        mpb.SetTexture("_MainTex", tex);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].SetPropertyBlock(mpb);
        }
    }
    public void SetGPUMatAttribute(int index,float smooth,float metal,Color color)
    {
        if (renderers.Length <= 0)
            return;
        renderers[0].GetPropertyBlock(mpb);
        color.a = isTransparent ? tAlpha : color.a;
        index = isTransparent ? 0 : index;
        mpb.SetFloat("_Index", index);
        mpb.SetFloat("_Glossiness", smooth);
        mpb.SetFloat("_Metallic", metal);
        mpb.SetColor("_Color", color);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].SetPropertyBlock(mpb);
        }
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLight(isHigh,mpb,ref colors, renderers);
    }

    public override void SetLODStatus(HLODState state)
    {
        if (state == hlodState)
        {
            return;
        }

        hlodState = state;
        switch (hlodState)
        {
            case HLODState.Cull:
                if (assetObj != null)
                {
                    ModelCachePool.Inst.Release(nodeData.id, assetObj);
                    assetObj = null;
                }
                break;
            case HLODState.High:
            case HLODState.Low:
                if (assetObj == null)
                {
                    assetObj = ModelCachePool.Inst.Get(nodeData.id);
                    assetObj.transform.SetParent(transform);
                    assetObj.transform.localPosition = Vector3.zero;
                    assetObj.transform.localEulerAngles = Vector3.zero;
                    assetObj.transform.localScale = Vector3.one;
                    RefreshRender();
                }
                break;
        } 
    }

    private void RefreshRender()
    {
        renderers = GetComponentsInChildren<Renderer>();
        if (colorMatData == null)
        {
            colorMatData = GameUtils.GetAttr<ColorMatData>((int) BehaviorKey.ColorMaterial, nodeData.attr);
        }
        allRenderers = renderers;
        // 状态重置
        IsOcclusion = IsOcclusion;
        BaseModelCreater.SetData(this, colorMatData);
        HLOD.Inst.OnHLODBehaviourStatusChange?.Invoke(this);
    }
}

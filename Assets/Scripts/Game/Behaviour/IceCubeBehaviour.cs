using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Lishuzhan
/// Description: 
/// Date: 2022-07-14
/// </summary>
public class IceCubeBehaviour : NodeBaseBehaviour
{
    public static MaterialPropertyBlock mpb;
    public Material iceMat;
    private Color[] oldColor;
    private List<Renderer> blockRenders = new List<Renderer>();
    [HideInInspector]
    public Renderer renderer;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
        renderer = GetComponentInChildren<Renderer>();
        iceMat = renderer.material;
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLight(isHigh, mpb, ref oldColor, blockRenders.ToArray());
    }

    // 1.0:根据三向轴拉伸自动设置tiling 
    // 1.1:让用户自己设置tiling
    // public void SetScale(Vector3 scale, Vector3 lastScale)
    // {
    //     Vector3 change = scale - lastScale;
    //     renderer.GetPropertyBlock(mpb);
    //     var tiling = iceMat.GetTextureScale("_Texture");
    //     tiling.y += change.y;
    //     tiling.x += Mathf.Abs(change.x) > Mathf.Abs(change.z) ? change.x : change.z;
    //     if (tiling.y < 0) tiling.y = 0;
    //     if (tiling.x < 0) tiling.x = 0;
    //     Vector2 tilingsVec = new Vector2(tiling.x, tiling.y);
    //     iceMat.SetTextureScale("_Texture", tilingsVec);
    // }

    public void SetTiling(Vec2 tiling)
    {
        if (iceMat != null && tiling != null)
        {
            iceMat.SetTextureScale("_Texture", tiling);
        }
    }
}
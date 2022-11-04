using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: LiShuzhan
/// Description: 
/// Date: 2022-08-16
/// </summary>
public class SnowCubeBehaviour : NodeBaseBehaviour
{
    private static MaterialPropertyBlock mpb;
    private Color[] oldColor;
    private List<Renderer> blockRenders = new List<Renderer>();

    public Renderer[] renderer;
    public  List<Material> snowMat = new List<Material>();

    private Renderer[] renderers;

    private Renderer[] Renderers
    {
        get
        {
            if (renderers == null)
            {
                renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            }

            return renderers;
        }
    }

    private List<GameObject> shaps;

    private List<GameObject> Shaps
    {
        get
        {
            if (shaps == null)
            {
                shaps = new List<GameObject>();
                for (int i = 0; i < transform.childCount; i++)
                {
                    shaps.Add(transform.GetChild(i).gameObject);
                }
            }

            return shaps;
        }
    }

    private Animator animator;

    public Animator Anim
    {
        get
        {
            if (animator == null)
            {
                animator = gameObject.GetComponentInChildren<Animator>();
            }

            return animator;
        }
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }

        renderer = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderer.Length; i++)
        {
            snowMat.Add(renderer[i].material);
        }
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLight(isHigh, mpb, ref oldColor, blockRenders.ToArray());
    }

    public void SetColor(Color color)
    {
        if (Renderers.Length <= 0)
            return;
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderers[i].material.SetColor("_Color", color);
        }
        entity.Get<SnowCubeComponent>().color = DataUtils.ColorToString(color);
    }

    public void SetTiling(Vec2 tiling)
    {
        if(tiling == null)
        {
            return;
        }
        for (int i = 0; i < snowMat.Count; i++)
        {
            if(snowMat[i] != null)
            {
                snowMat[i].SetTextureScale("_MainTex", tiling);
            }
        }
    }

    public void SetShape(SnowShape shape)
    {
        HideAllShape();
        entity.Get<SnowCubeComponent>().shape = (int) shape;
        animator = null;
        switch (shape)
        {
            case SnowShape.Cube:
                Shaps[0].SetActive(true);
                return;
            case SnowShape.Cylinder:
                Shaps[1].SetActive(true);
                return;
        }
    }

    public void HideAllShape()
    {
        for (int i = 0; i < Shaps.Count; i++)
        {
            Shaps[i].SetActive(false);
        }
    }
}
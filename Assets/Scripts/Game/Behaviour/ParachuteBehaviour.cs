using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: LiShuzhan
/// Description: 
/// Date: 2022-08-01
/// </summary>
public class ParachuteBehaviour : NodeBaseBehaviour
{
    private static MaterialPropertyBlock mpb;
    private Color[] oldColor;
    private List<Renderer> blockRenders = new List<Renderer>();
    
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLight(isHigh, mpb, ref oldColor, blockRenders.ToArray());
    }
}
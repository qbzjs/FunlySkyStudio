using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParachuteBagBehaviour : NodeBaseBehaviour
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

using UnityEngine;

/// <summary>
/// Author: 熊昭
/// Description: 特殊PGC素材行为类：使用特殊高亮(预制件原有材质有颜色)
/// Date: 2022-03-18 22:07:43
/// </summary>
public class PGCSpecialBehaviour : PGCBehaviour
{
    private static MaterialPropertyBlock mpb;
    private Renderer[] renderers;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
        renderers = gameObject.GetComponentsInChildren<Renderer>(true);
    }

    public override void HighLight(bool isHigh)
    {
        HighLightUtils.HighLightOnHasColor(isHigh, mpb, ref colors, renderers);
    }
}
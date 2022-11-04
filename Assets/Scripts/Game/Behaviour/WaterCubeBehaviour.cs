/// <summary>
/// Author:zhouzihan
/// Description:水方块
/// Date: #CreateTime#
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using SavingData;

public class WaterCubeBehaviour : NodeBaseBehaviour
{
    public Material mpbSurface;
    public Material mpbEdge;
    private Color[] colors;
    public float test;

    private string texEmission02 = "_Emission_02";
    private string texEmission01 = "_Emission_01";
    private string texNoise02 = "_Noise_02";
    private string texNoise03 = "_Noise_03";
    private string floatTime01U = "_01_time_u";
    private string floatTime02V = "_02_time_v";
    private string floatTime03V = "_03_time_v";

    public Dictionary<string, float> velocityDict = new Dictionary<string, float>();
    public override void OnInitByCreate ()
    {
        base.OnInitByCreate();
        Renderer renderer = transform.Find("wetrix_02").GetComponent<Renderer>();
        mpbSurface = renderer.material;
        Renderer edgeRenderer = transform.Find("wetrix_01").GetComponent<Renderer>();
        mpbEdge = edgeRenderer.material;
    }
    #region 旧版本的自适应Tiling和Velocity
    public void SetupOldVersion ()
    {
        SetMatScale(transform.localScale);
        SetMaterial(0);
    }

    public void SetMatScale (Vector3 scale)
    {
        SetTextureScale(texEmission02, scale, 1);
        SetTextureScale(texEmission01, scale);
        SetTextureScale(texNoise02, scale);
        SetTextureScale(texNoise03, scale);
        SetFloat(floatTime01U, scale, 5);
        SetFloat(floatTime02V, scale, 3);
        SetFloat(floatTime03V, scale, 5);
    }
    public void SetTextureScale (string name, Vector3 change, float times = 0.5f)
    {
        var tiling = mpbSurface.GetTextureScale(name);
        tiling.y = change.x >= 5 ? change.x * 0.2f * times : times;
        tiling.x = change.z >= 5 ? change.z * 0.2f * times : times;
        if (tiling.y < 0) tiling.y = 0;
        if (tiling.x < 0) tiling.x = 0;
        mpbSurface.SetTextureScale(name, new Vector2(tiling.x, tiling.y));
    }
    #region 计算旧版本的tiling和velocity
    // Tiling 以（_Emission_02） 为基准  比例为1 
    public Vector2 OldTiling ()
    {
        Vector3 change = transform.localScale;
        float times = 1;
        var tiling = mpbSurface.GetTextureScale(texEmission02);
        tiling.y = change.x >= 5 ? change.x * 0.2f * times : times;
        tiling.x = change.z >= 5 ? change.z * 0.2f * times : times;
        if (tiling.y < 0) tiling.y = 0;
        if (tiling.x < 0) tiling.x = 0;
        return new Vector2(tiling.x, tiling.y);
    }
    public float OldVelocity ()
    {
        Vector3 change = transform.localScale;
        float times = 5;
        float changeFloat = change.x > change.z ? change.x : change.z;
        float time = changeFloat >= 5 ? 0.1f / changeFloat * times : times * 0.01f;
        return time;
    }
    #endregion
    #endregion
    public void SetFloat (string name, Vector3 change, int times)
    {
        float changeFloat = change.x > change.z ? change.x : change.z;
        float time = changeFloat >= 5 ? 0.1f / changeFloat * times : times * 0.01f;
        mpbSurface.SetFloat(name, time);
    }
    public void SetTiling (Vector2 tiling)
    {
        mpbSurface.SetTextureScale(texEmission02, tiling);
        mpbSurface.SetTextureScale(texEmission01, tiling * 0.5f);//沿用旧版本的比例
        mpbSurface.SetTextureScale(texNoise02, tiling * 0.5f);//沿用旧版本的比例
        mpbSurface.SetTextureScale(texNoise03, tiling * 0.5f);//沿用旧版本的比例
    }
    public void SetVelocity (float val)
    {
        mpbSurface.SetFloat(floatTime01U, val);//沿用旧版本的比例

        mpbSurface.SetFloat(floatTime02V, val * 0.6f);//沿用旧版本的比例
        mpbSurface.SetFloat(floatTime03V, val);//沿用旧版本的比例
    }
    public void SetSurfaceDiffuse (Color color)
    {
        SetColor("_Diffuse_Color", color);
    }
    public void SetSurfaceEmission (Color color)
    {
        SetColor("_Emission_Color", color);
    }
    public void SetEdgeAlbedo (Color color)
    {
        mpbEdge.color = color;
    }
    public void SetColor (string name, Color color)
    {
        mpbSurface.SetColor(name, color);
    }
    public void Setup ()
    {
        WaterComponent compt = entity.Get<WaterComponent>();
        SetTiling(compt.tiling);
        SetVelocity(compt.v);
        SetMaterial(compt.id);
    }
    private void SetMaterial (int cfgId)
    {
        int index = GameManager.Inst.waterCubeDatas.FindIndex(x => x.id == cfgId);
        WaterCubeData cfgData = GameManager.Inst.waterCubeDatas[index];
        //
        Color color = DataUtils.DeSerializeColorByHex(cfgData.surfaceDiffuse);
        color.a = ColorInt2Float(cfgData.surfaceDiffuseAlpha);
        SetSurfaceDiffuse(color);

        color = DataUtils.DeSerializeColorByHex(cfgData.surfaceEmission);
        color.a = ColorInt2Float(cfgData.surfaceEmissionAlpha);
        SetSurfaceEmission(color);

        color = DataUtils.DeSerializeColorByHex(cfgData.edgeAlbedo);
        color.a = ColorInt2Float(cfgData.edgeAlbedoAlpha);
        SetEdgeAlbedo(color);
    }
    private float ColorInt2Float (int i)
    {
        return ((float)i) / 255;
    }
    public override void HighLight (bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref colors);
    }
}

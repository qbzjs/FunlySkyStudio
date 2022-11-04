using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTG;
using UnityEngine;

public class HighLightUtils
{
    public static void HighLight(bool isHigh,MaterialPropertyBlock mpb,ref Color[] oldColor, Renderer[] renderers)
    {
        if (isHigh)
        {
            oldColor = new Color[renderers.Length];
            for (int i = 0; i < oldColor.Length; i++)
            {
                renderers[i].GetPropertyBlock(mpb);
                oldColor[i] = mpb.GetColor("_Color");
                mpb.SetColor("_Color", DataUtils.GetHighlightColor(oldColor[i]));
                renderers[i].SetPropertyBlock(mpb);
            }
        }
        else
        {
            if (oldColor != null && oldColor.Length == renderers.Length)
            {
                for (int i = 0; i < oldColor.Length; i++)
                {
                    renderers[i].GetPropertyBlock(mpb);
                    mpb.SetColor("_Color", oldColor[i]);
                    renderers[i].SetPropertyBlock(mpb);
                }
            }
        }
    }

    public static void HighLightOnSpecial(bool isHigh, GameObject go,ref Color[] oldColor)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        var mpb = new MaterialPropertyBlock();
        if (isHigh)
        {
            oldColor = new Color[renderers.Length];
            for (int i = 0; i < oldColor.Length; i++)
            {
                renderers[i].GetPropertyBlock(mpb);
                oldColor[i] = mpb.GetColor("_Color");
                Color hColor = new Color(1.3f, 1.3f, 1.3f, 1);
                mpb.SetColor("_Color", hColor);
                renderers[i].SetPropertyBlock(mpb);
            }
        }
        else
        {
            if (oldColor != null && oldColor.Length == renderers.Length)
            {
                for (int i = 0; i < oldColor.Length; i++)
                {
                    renderers[i].GetPropertyBlock(mpb);
                    Color oColor = new Color(1f, 1f, 1f, 1);
                    mpb.SetColor("_Color", oColor);
                    renderers[i].SetPropertyBlock(mpb);
                }
            }
        }
    }

    public static void HighLightOnOffLineUgc(bool isHigh, GameObject go, Dictionary<Material, Color> oColorDic, Dictionary<Material, Color> hColorDic)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        if (isHigh)
        {
            foreach (var render in renderers)
            {
                var materials = render.materials;
                foreach (var mat in materials)
                {
                    if (hColorDic.ContainsKey(mat))
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            mat.SetColor("_Color", hColorDic[mat]);
                        }
                    }
                }
            }
        }
        else {
            foreach (var render in renderers)
            {
                var materials = render.materials;
                foreach (var mat in materials)
                {
                    if (oColorDic.ContainsKey(mat))
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            mat.SetColor("_Color", oColorDic[mat]);
                        }
                    }
                }
            }
        }
    }

    public static void HighLightOnEdit(bool isHigh, GameObject go,ref Color[] oldColor,float highColor = 2f)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        var mpb = new MaterialPropertyBlock();
        if (isHigh)
        {
            oldColor = new Color[renderers.Length];
            for (int i = 0; i < oldColor.Length; i++)
            {
                renderers[i].GetPropertyBlock(mpb);
                oldColor[i] = mpb.GetColor("_Color");
                Color hColor = new Color(highColor, highColor, highColor, 1);
                mpb.SetColor("_Color", hColor);
                renderers[i].SetPropertyBlock(mpb);
            }
        }
        else
        {
            if (oldColor != null && oldColor.Length == renderers.Length)
            {
                for (int i = 0; i < oldColor.Length; i++)
                {
                    renderers[i].GetPropertyBlock(mpb);
                    Color oColor = new Color(1f, 1f, 1f, 1);
                    mpb.SetColor("_Color", oColor);
                    renderers[i].SetPropertyBlock(mpb);
                }
            }
        }
    }

    public static void HighLightOnHasColor(bool isHigh, MaterialPropertyBlock mpb, ref Color[] oldColor, Renderer[] renderers)
    {
        if (isHigh)
        {
            oldColor = new Color[renderers.Length];
            for (int i = 0; i < oldColor.Length; i++)
            {
                renderers[i].GetPropertyBlock(mpb);
                oldColor[i] = renderers[i].material.color;
                mpb.SetColor("_Color", DataUtils.GetHighlightColor(oldColor[i]));
                renderers[i].SetPropertyBlock(mpb);
            }
        }
        else
        {
            if (oldColor != null && oldColor.Length == renderers.Length)
            {
                for (int i = 0; i < oldColor.Length; i++)
                {
                    renderers[i].GetPropertyBlock(mpb);
                    mpb.SetColor("_Color", oldColor[i]);
                    renderers[i].SetPropertyBlock(mpb);
                }
            }
        }
    }
}
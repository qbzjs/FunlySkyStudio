using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:PGC植物Behavior：设置颜色、抖动值，植物类型
/// Date: 2022/8/4 16:43:6
/// </summary>
public class PGCPlantBehaviour : BaseHLODBehaviour
{
    private static MaterialPropertyBlock mpb;
    protected Color[] oldColor;
    private Renderer leafRender;
    public Renderer LeafRender
    {
        get
        {
            if (assetObj.transform.childCount > 0)
            {
                leafRender = assetObj.transform.GetChild(0).GetComponent<Renderer>();
            }
            else
            {
                leafRender = assetObj.GetComponent<Renderer>();
            }
            return leafRender;
        }
    }
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
        if (isHigh)
        {
            oldColor = new Color[Renderers.Length];
            for (int i = 0; i < oldColor.Length; i++)
            {
                if (Renderers[i].gameObject.name == "trunk")//树干
                {
                    oldColor[i] = Renderers[i].material.GetColor("_Color");
                    var hightColor = DataUtils.GetHighlightColor(oldColor[i]);
                    Renderers[i].material.SetColor("_Color", hightColor);
                }
                else
                {
                    Renderers[i].GetPropertyBlock(mpb);
                    oldColor[i] = mpb.GetColor("_Color");
                    var hightColor = DataUtils.GetHighlightColor(oldColor[i]);
                    mpb.SetColor("_Color", hightColor);
                    Renderers[i].SetPropertyBlock(mpb);
                }
            }
        }
        else
        {
            if (oldColor != null && oldColor.Length == Renderers.Length)
            {
                for (int i = 0; i < oldColor.Length; i++)
                {
                    if (Renderers[i].gameObject.name == "trunk")//树干
                    {
                        Renderers[i].material.SetColor("_Color", oldColor[i]);
                    }
                    else
                    {
                        Renderers[i].GetPropertyBlock(mpb);
                        mpb.SetColor("_Color", oldColor[i]);
                        Renderers[i].SetPropertyBlock(mpb);
                    }
                }
            }
        }
    }
    public void UpdateAssetObj(GameObject gameObject, int id)
    {
        gameObject.transform.SetParent(this.transform);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localEulerAngles = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;
        assetObj = gameObject;
        var gameComp = entity.Get<GameObjectComponent>();
        gameComp.modId = id;
        //根据模型的y轴缩放值设置抖动参数
        SetIntensity(this.transform.localScale.y, id);
    }
    public void UpdateNodeHandleType(int id)
    {
        var data = GameManager.Inst.PGCPlantDatasDic[id];
        var gameComp = entity.Get<GameObjectComponent>();
        //不同植物类型对应不同缩放操作
        if (gameComp.handleType != (NodeHandleType)data.handleType)
        {
            gameComp.handleType = (NodeHandleType)data.handleType;
            this.transform.localScale = Vector3.one;
            if (ModelHandlePanel.Instance != null)
            {
                ModelHandlePanel.Instance.EnterMode((NodeHandleType)data.handleType);
            }
        }
    }
    /// <summary>
    /// 设置颜色
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color)
    {
        LeafRender.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", color);
        LeafRender.SetPropertyBlock(mpb);
    }
    /// <summary>
    /// 设置抖动值
    /// </summary>
    /// <param name="scaleY">y轴大小值</param>
    public void SetIntensity(float scaleY, int id)
    {
        float intensity = (float)((scaleY * 0.05) - 0.03);
        var data = PGCPlantManager.Inst.GetPGCPlantConfigData(id);
        if (intensity < data.minIntensity)
        {
            intensity = data.minIntensity;
        }
        if (intensity > data.maxIntensity)
        {
            intensity = data.maxIntensity;
        }
        LeafRender.GetPropertyBlock(mpb);
        mpb.SetFloat("_intensity_02", intensity);
        LeafRender.SetPropertyBlock(mpb);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HLODSystem;
using Assets.Scripts.Game.Core;

public class PointLightBehaviour : BaseHLODBehaviour
{
    [HideInInspector]
    public Light curLight;
    [HideInInspector]
    public GameObject highGo;
    [HideInInspector]
    public GameObject lightMeshGo;
    public override void SetUp()
    {
        curLight = assetObj.GetComponentInChildren<Light>();
        highGo = assetObj.transform.GetChild(2).gameObject;
        lightMeshGo = assetObj.transform.GetChild(0).gameObject;

        var mComp = entity.Get<PointLightComponent>();
        SetIntensity(mComp.Intensity);
        SetRange(mComp.Range);
        SetColor(mComp.color);

    }

    public void SetIntensity(float inten)
    {
        curLight.intensity = inten;
    }

    public void SetRange(float range)
    {
        curLight.range = range;
    }

    public void SetColor(Color color)
    {
        curLight.color = color;
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        highGo.SetActive(isHigh);
    }

    public void SetMeshVisibel(bool isVisible)
    {
        var rComp = lightMeshGo.GetComponent<MeshRenderer>();
        if (rComp)
        {
            rComp.enabled = isVisible;
        }
    }
}

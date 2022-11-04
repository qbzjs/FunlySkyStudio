using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLibrary : CInstance<AssetLibrary>
{
    public ColorLibrary colorLib;
    public ColorLibrary lightLib;
    public TripleColorLibrary enrLib;

    public void Init()
    {
        colorLib = ResManager.Inst.LoadRes<ColorLibrary>("ConfigAssets/NewColorLibrary");
        lightLib = ResManager.Inst.LoadRes<ColorLibrary>("ConfigAssets/DirLightColors");
        enrLib = ResManager.Inst.LoadRes<TripleColorLibrary>("ConfigAssets/SceneGradient");
    }
}

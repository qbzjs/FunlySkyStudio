using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TerrainComponent : IComponent
{
    public int matId;
    public string umatUrl;
    public string umapId;
    public Color color;
    public int terrainSize;
    public IComponent Clone()
    {
        return null;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}
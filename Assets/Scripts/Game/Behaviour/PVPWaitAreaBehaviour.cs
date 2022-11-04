using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTG;
using UnityEngine;

public class PVPWaitAreaBehaviour : NodeBaseBehaviour
{
    public GameObject meshShow;
    public GameObject boxShow;
    private float minHeight = 0.1f;
    private float maxHeight => (int)GlobalFieldController.terrainSize * 100f;

    private float MinX => -maxHeight / 2 + 1;
    private float MaxX => maxHeight / 2 - 1 * (int)GlobalFieldController.terrainSize;
    private float MinZ => -maxHeight / 2 + 1;
    private float MaxZ => maxHeight / 2 - 1 * (int)GlobalFieldController.terrainSize;

    private Vector3 pos;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        meshShow = transform.GetChild(0).gameObject;
        boxShow = transform.GetChild(1).gameObject;
    }

    public override void OnReset()
    {
        base.OnReset();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }


    public bool OnCanBeTransformed(Gizmo transformGizmo)
    {
        return true;
    }

    public void OnTransformed(Gizmo transformGizmo)
    {
        pos.x = transform.position.x;
        pos.y = transform.position.y;
        pos.z = transform.position.z;
        if (pos.y <= minHeight)
        {
            pos.y = minHeight;
        }
        if (pos.y >= maxHeight)
        {
            pos.y = maxHeight;
        }
        if (pos.x <= MinX)
        {
            pos.x = MinX;
        }
        if (pos.x >= MaxX)
        {
            pos.x = MaxX;
        }
        if (pos.z <= MinZ)
        {
            pos.z = MinZ;
        }
        if (pos.z >= MaxZ)
        {
            pos.z = MaxZ;
        }
        transform.position = pos;
    }
}

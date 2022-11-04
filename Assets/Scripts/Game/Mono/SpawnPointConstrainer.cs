using System.Collections;
using System.Collections.Generic;
using RTG;
using UnityEngine;

public class SpawnPointConstrainer : MonoBehaviour, IRTTransformGizmoListener
{
    public float minHeight = 0.1f;
    private float maxHeight => (int)GlobalFieldController.terrainSize * 100f;
    private float MinX => -maxHeight / 2 + 1;
    private float MaxX => maxHeight / 2 - 1 * (int)GlobalFieldController.terrainSize;
    private float MinZ => -maxHeight / 2 + 1;
    private float MaxZ => maxHeight / 2 - 1 * (int)GlobalFieldController.terrainSize;

    Vector3 pos;

    public  bool OnCanBeTransformed(Gizmo transformGizmo)
    {
        return true;
    }

    public  void OnTransformed(Gizmo transformGizmo)
    {
        pos.x = transform.position.x;
        pos.y = transform.position.y;
        pos.z = transform.position.z;
        if (pos.y <= minHeight)
        {
            pos.y = minHeight;
        }
        if(pos.y >= maxHeight)
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

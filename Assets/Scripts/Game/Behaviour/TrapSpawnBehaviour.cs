using System.Collections;
using RTG;
using TMPro;
using UnityEngine;

/// <summary>
/// Author: 熊昭
/// Description: 陷阱盒传送点行为功能类
/// Date: 2022-01-03 21:24:42
/// </summary>
public class TrapSpawnBehaviour : NodeBaseBehaviour, IRTTransformGizmoListener
{
    public TextMeshPro textMesh;

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
        if (textMesh == null)
        {
            textMesh = this.GetComponentInChildren<TextMeshPro>(true);
        }
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public override void OnReset()
    {
        base.OnReset();
        textMesh.text = "";
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public void RefreshPointId()
    {
        var tComp = entity.Get<TrapSpawnComponent>();
        textMesh.text = tComp.tId.ToString();
    }

    private void OnChangeMode(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
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
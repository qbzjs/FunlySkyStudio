/// <summary>
/// Author:Mingo-LiZongMing
/// Description:传送点Behaviour
/// </summary>
using System;
using RTG;
using TMPro;
using UnityEngine;

public class PortalPointBehaviour : NodeBaseBehaviour, IRTTransformGizmoListener
{
    public TextMeshPro textMesh;

    public int pid;

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
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        if(textMesh == null)
        {
            textMesh = this.GetComponentInChildren<TextMeshPro>(true);
        }
    }


    public override void OnReset()
    {
        base.OnReset();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        pid = 0;
        textMesh.text = "";
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public void RefreshPointId()
    {
        //LoggerUtils.LogError("OnInitByCreate + pid = " + pid);
        textMesh.text = pid.ToString();
    }

    private void Start()
    {
        textMesh.text = pid.ToString();
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

    private void OnDestroy()
    {
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }
}

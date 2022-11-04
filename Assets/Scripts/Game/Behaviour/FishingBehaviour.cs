using System.Collections;
using System.Collections.Generic;
using RTG;
using UnityEngine;

public class FishingBehaviour : NodeBaseBehaviour, IRTTransformGizmoListener
{

    private float minHeight = 0.1f;
    private float maxHeight => (int)GlobalFieldController.terrainSize * 100f;

    private float MinX => -maxHeight / 2 + 1;
    private float MaxX => maxHeight / 2 - 1 * (int)GlobalFieldController.terrainSize;
    private float MinZ => -maxHeight / 2 + 1;
    private float MaxZ => maxHeight / 2 - 1 * (int)GlobalFieldController.terrainSize;

    private Dictionary<Transform, Vector3> _fishingRodNodePosDic = new Dictionary<Transform, Vector3>();

    private Vector3 pos;

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

    /// <summary>
    /// 由于鱼竿节点上有Animator，自节点位置重置有问题
    /// </summary>
    public void GetFishingRodNodeOriPos()
    {
        _fishingRodNodePosDic.Clear();
        var transforms = GetComponentsInChildren<Transform>();
        foreach (Transform trans in transforms)
            _fishingRodNodePosDic[trans] = trans.localPosition;
    }

    /// <summary>
    /// 重置鱼竿子节点的位置
    /// </summary>
    public void ResetFishingRodNodeOriPos()
    {
        foreach (var kvp in _fishingRodNodePosDic)
            kvp.Key.localPosition = kvp.Value;
    }

    public void EnableHookEffet(bool enable)
    {
        var hookEffect = GetHookEffect();
        if (hookEffect != null)
            hookEffect.SetActive(enable);
    }

    public Vector3 GetHookWorldPos()
    {
        var hookEffect = GetHookEffect();
        return hookEffect != null ? hookEffect.transform.position : Vector3.zero;
    }

    private GameObject GetHookEffect()
    {
        var hook = FishingEditManager.Inst.GetHook(this);
        if (hook == null)
            return null;

        var hookEffect = hook.transform.Find("Anchors");
        if (hookEffect == null)
        {
            var hookEffectPrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/Fishing/FishingHookEffect");
            hookEffect = GameObject.Instantiate(hookEffectPrefab, hook.transform).transform;
            hookEffect.name = "Anchors";
            hookEffect = hookEffect.transform;
            hookEffect.Normalize();
            hookEffect.transform.localPosition = hook.entity.Get<FishingHookComponent>().hookPosition;
        }

        return hookEffect.gameObject;
    }
}

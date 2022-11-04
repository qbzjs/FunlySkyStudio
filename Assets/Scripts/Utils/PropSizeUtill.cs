/// <summary>
/// Author:Mingo-LiZongMing
/// Description: 物体的实际尺寸 = 原始尺寸 * 缩放比例
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropSizeUtill : MonoBehaviour
{
    public static void SetPropSize(Vector3 targetSize, NodeBaseBehaviour baseBev)
    {
        var curPropSize = GetPropSize(baseBev);
        var targetSide = GetLongestSide(targetSize);
        var curSide = GetLongestSide(curPropSize);
        var scale = targetSide / curSide;
        if (curPropSize != Vector3.zero)
        {
            baseBev.transform.localScale *= scale;
        }
    }

    public static Vector3 GetPropSize(NodeBaseBehaviour baseBev)
    {
        BoxCollider propBoxCollider;
        MeshCollider[] propColliders;
        GameObject boxCheck = new GameObject("boxCheck");
        boxCheck.transform.SetParent(baseBev.transform);
        boxCheck.transform.localPosition = Vector3.zero;
        boxCheck.transform.localScale = Vector3.one;
        propColliders = boxCheck.GetComponentsInChildren<MeshCollider>(true);
        if (propColliders != null)
        {
            for (var i = 0; i < propColliders.Length; i++)
            {
                propColliders[i].enabled = true;
            }
        }
        propBoxCollider = boxCheck.AddComponent<BoxCollider>();
        UpdateBoundBox(propBoxCollider);
        var propSize = propBoxCollider.size;
        return propSize;
    }

    public static void UpdateBoundBox(BoxCollider bCollider)
    {
        var cNode = bCollider.transform.parent;
        Renderer[] renders = cNode.GetComponentsInChildren<Renderer>();
        if(renders.Length == 0)
        {
            LoggerUtils.Log("UpdateBoundBox renders.Length = " + renders.Length);
            return;
        }
        Vector3 postion = cNode.position;
        Quaternion rotation = cNode.rotation;
        Vector3 scale = cNode.localScale;
        cNode.position = Vector3.zero;
        cNode.rotation = Quaternion.Euler(Vector3.zero);
        Vector3 center = Vector3.zero;
        
        foreach (Renderer child in renders)
        {
            center += child.bounds.center;
        }
        center /= renders.Length;
        Bounds bounds = new Bounds(center, Vector3.zero);
        foreach (Renderer child in renders)
        {
            bounds.Encapsulate(child.bounds);
        }
        bCollider.center = bounds.center - cNode.position;
        bCollider.size = bounds.size;
        cNode.position = postion;
        cNode.rotation = rotation;
        cNode.localScale = scale;
    }

    private static float GetLongestSide(Vector3 vector3)
    {
        var x = vector3.x;
        var y = vector3.y;
        var z = vector3.z;
        var max = x > y ? x : y;
        max = z > max ? z : max;
        return max;
    }
}

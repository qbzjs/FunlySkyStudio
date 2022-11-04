// GameObjectUtils.cs
// Created by xiaojl Sep/15/2022
// GameObject工具类

using UnityEngine;

public static class GameObjectUtils
{
    public static void Normalize(this GameObject go)
    {
        Normalize(go.transform);
    }

    public static void Normalize(this Transform trans)
    {
        trans.localPosition = Vector3.zero;
        trans.localScale = Vector3.one;
        trans.localRotation = Quaternion.identity;
    }

    public static void ClearChildren(this GameObject go)
    {
        ClearChildren(go.transform);
    }

    public static void ClearChildren(this Transform trans)
    {
        foreach (Transform t in trans)
            GameObject.Destroy(t.gameObject);
    }
}

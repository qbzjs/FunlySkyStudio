using System;
using System.Collections.Generic;
using UnityEngine;

public static class DontDestroyUtils
{
    private static List<GameObject> dontDestroys = new List<GameObject>();

    public static void DontDestroy(this GameObject obj)
    {
        if (!dontDestroys.Contains(obj))
        {
            dontDestroys.Add(obj);
        }
        GameObject.DontDestroyOnLoad(obj);
    }


    public static void Dispose()
    {
        foreach (var obj in dontDestroys)
        {
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
        }
        dontDestroys.Clear();
    }
}
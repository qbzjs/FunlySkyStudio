using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Multithreading is not supported
/// </summary>
/// <typeparam name="T"></typeparam>
public class InstMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T inst;

    public static T Inst
    {
        get
        {
            if (inst == null)
            {
                inst = GameObject.FindObjectOfType<T>();
                if (inst == null)
                {
                    var go = new GameObject(typeof(T).Name);
                    inst = go.AddComponent<T>();
                }
            }
            return inst;
        }
    }
}

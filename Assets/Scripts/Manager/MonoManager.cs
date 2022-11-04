using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// Base On MonoBehaviour，Not Support Multithreading
/// </summary>
/// <typeparam name="T"></typeparam>
public class MonoManager<T> :MonoBehaviour where T: MonoBehaviour
{
    private static T _inst;
    public static T Inst
    {
        get
        {
            if (_inst == null)
            {
                GameObject tempGo = new GameObject(typeof(T).ToString());
                _inst = tempGo.AddComponent<T>();
            }

            return _inst;
        }
    }
    /// <summary>
    /// init manager
    /// </summary>
    public virtual void Init()
    { }
    public static bool IsInit()
    {
        return _inst != null;
    }

    protected virtual void OnDestroy()
    {
        _inst = null;
    }
}
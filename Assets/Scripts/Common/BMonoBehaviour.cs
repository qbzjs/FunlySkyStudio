using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BMonoBehaviour<T>:MonoBehaviour where T: MonoBehaviour
{
    public static T Inst;
    protected virtual void Awake()
    {
        Inst = this as T;
    }

    protected virtual void OnDestroy()
    {
        Inst = null;
    }
}
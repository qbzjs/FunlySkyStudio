using System;
using System.Collections;
using UnityEngine;

public class CoroutineManager : MonoManager<CoroutineManager>
{


    public Coroutine CallBack<T>(T instruction, Action<T> callback) where T : YieldInstruction
    {
        return StartCoroutine(CoroutineCallBack(instruction, callback));
    }

    private IEnumerator CoroutineCallBack<T>(T enumerator, Action<T> callBack) where T : YieldInstruction
    {
        yield return enumerator;
        callBack?.Invoke(enumerator);
    }
}
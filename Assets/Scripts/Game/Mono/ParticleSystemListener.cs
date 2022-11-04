using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemListener : MonoBehaviour
{
    public Action CompleteAction;

    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ParticleSystem.MainModule mainModule = ps.main;
            mainModule.stopAction = ParticleSystemStopAction.Callback;
        }
    }
    void OnParticleSystemStopped()
    {
        LoggerUtils.Log("ParticleSystemListener OnParticleSystemStopped");
        if(CompleteAction !=null)
        {
            CompleteAction.Invoke();
        }
        
    }

    public void SetCompleteCallback(Action action)
    {
        CompleteAction = action;
    }
}

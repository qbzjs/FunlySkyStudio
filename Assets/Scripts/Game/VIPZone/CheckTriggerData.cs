using UnityEngine;

public class CheckTriggerData
{
    public bool enterTrigger = false;
    public bool everTriggerCheck = false;

    public void ExitTrigger()
    {
        everTriggerCheck = false;
    }

    public void EnterTrigger()
    {
        enterTrigger = true;
    }

    public bool CanTriggerCheck()
    {
        return enterTrigger && !everTriggerCheck;
    }

    public void OnCheckTrigger()
    {
        everTriggerCheck = true;
    }
}
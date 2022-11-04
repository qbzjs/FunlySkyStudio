using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class RoleHandAdjustView : RoleAdjustView
{
    public GameObject switchHandBtn;
    private Action onSwitch;

    protected override void Start()
    {
        base.Start();
        switchHandBtn.GetComponentInChildren<Button>().onClick.AddListener(SwitchHand);
    }

    public void SetOnSwitch(Action action)
    {
        onSwitch = action;
    }

    public void SetBtnActive(bool isActive)
    {
        switchHandBtn.SetActive(isActive);
    }


    private void SwitchHand()
    {
        onSwitch?.Invoke();
    }

    
}

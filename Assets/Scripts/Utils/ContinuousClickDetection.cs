using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Author：LiShuZhan
/// 防止按键连点
/// </summary>
enum LockType
{
    Lock,
    UnLock
}
public class ContinuousClickDetection : MonoBehaviour
{
    public Button btn;
    public float waitTime;
    private LockType locktype;
    public Action act;

    public void AddListener(Action ourAct)
    {
        act = ourAct;
        locktype = LockType.UnLock;
        btn.onClick.AddListener(LockBtn);
    }

    //通过外部按钮控制此按钮的锁定状态
    public void BtnLock()
    {
        CancelInvoke();
        locktype = LockType.Lock;
        Invoke("UnLockBtn", waitTime);
    }

    private void LockBtn()
    {
        if(locktype == LockType.Lock)
        {
            return;
        }
        act?.Invoke();
        locktype = LockType.Lock;
        Invoke("UnLockBtn", waitTime);
    }

    private void UnLockBtn()
    {
        locktype = LockType.UnLock;
    }

    private void OnDestroy()
    {
        CancelInvoke();
        UnLockBtn();
    }

    private void OnEnable()
    {
        UnLockBtn();
    }
}

/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/5/10 16:32:19
/// </summary>
using System.Collections;
using System.Collections.Generic;
using agora_gaming_rtc;
using TMPro;
using UnityEngine;

public class VoiceItemPos : MonoBehaviour
{
    public GameObject nickGO;
    public SuperTextMesh nickTMP;
    private float nHeartPosOff = 0.45f;
    private bool state;

    void Awake()
    {
        GlobalSettingManager.Inst.OnShowUserNameChange += SetPosWithIfShowUserName;
    }

    void OnDestory()
    {
        if (GlobalSettingManager.Inst.OnShowUserNameChange != null)
            GlobalSettingManager.Inst.OnShowUserNameChange -= SetPosWithIfShowUserName;
    }

    void OnEnable()
    {
        bool showUserName = GlobalSettingManager.Inst.IsShowUserName();
        SetPosWithIfShowUserName(showUserName);
    }
    
    public void SetPos(bool state)
    {
        this.state = state;
        bool showUserName = GlobalSettingManager.Inst.IsShowUserName();
        SetPosWithIfShowUserName(showUserName);
    }

    private void SetPosWithIfShowUserName(bool showUserName)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(CSetPosWithIfShowUserName(showUserName));
        }
    }

    private IEnumerator CSetPosWithIfShowUserName(bool showUserName)
    {
        if (showUserName)
        {
            yield return null;
            nHeartPosOff = state ? 0.121f : 0.078f;
            Vector3 heartPos = transform.localPosition;
            heartPos.x = -(nickTMP.preferredWidth) / 20 - nHeartPosOff;
            transform.localPosition = heartPos;
        }
        else
        {
            Vector3 heartPos = transform.localPosition;
            heartPos.x = 0;
            transform.localPosition = heartPos;
        }
    }
}

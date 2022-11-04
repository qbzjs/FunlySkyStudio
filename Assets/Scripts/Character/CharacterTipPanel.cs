using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:WenJia
/// Description: Avatar 界面专用 Toast
/// Date: 2022/5/5 20:51:54
/// </summary>


public class CharacterTipPanel : BasePanel<CharacterTipPanel>
{

    private Animator anim;
    private Text Title;
    private bool isOnShow = false;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        Title = this.GetComponentInChildren<Text>();
        anim = this.transform.GetComponentInChildren<Animator>();
        anim.enabled = false;
    }

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        if (isOnShow)
            return;
        isOnShow = true;
        anim.enabled = true;
        anim.Play("ToastAnim", 0, 0);
        CancelInvoke("OnHide");
        Invoke("OnHide", 2f);
    }

    private void OnHide()
    {
        isOnShow = false;
        anim.enabled = false;
        Hide();
    }

    public static void ShowToast(string content, params object[] formatArgs)
    {
        CharacterTipPanel.Show(true);
        CharacterTipPanel.Instance.SetTitle(content, formatArgs);
        CharacterTipPanel.Instance.transform.SetAsLastSibling();
    }



    public void SetTitle(string content, params object[] formatArgs)
    {
        LocalizationConManager.Inst.SetLocalizedContent(Title, content, formatArgs);
    }
}

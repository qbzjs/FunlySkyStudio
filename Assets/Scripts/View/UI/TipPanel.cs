using System.Net.Mime;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel:BasePanel<TipPanel>
{
    private Animator anim;
    private Text Title;
    private Vector3 startPos = new Vector3(0,-1000,0);
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
        if(isOnShow)
            return;
        isOnShow = true;
        anim.enabled = true;
        anim.Play("ToastAnim",0,0);
        CancelInvoke("OnHide");
        Invoke("OnHide",2f);
    }

    private void OnHide()
    {
        isOnShow = false;
        anim.enabled = false;
        Hide();
    }

    public static void ShowToast(string content, params object[] formatArgs)
    {
        TipPanel.Show(true);
        TipPanel.Instance.SetTitle(content, formatArgs);
        TipPanel.Instance.transform.SetAsLastSibling();
    }



    public void SetTitle(string content, params object[] formatArgs)
    {
        LocalizationConManager.Inst.SetLocalizedContent(Title, content, formatArgs);
    }



}
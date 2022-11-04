using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:LiShuZhan
/// Description:专有属性关闭按钮，关闭按钮在此处获取，直接给按钮添加点击事件
/// Date: 2022.01.25
/// </summary>
public abstract class InfoPanel<T> : BasePanel<T> where T : BasePanel<T>
{
    protected Button closeBtn;

    public override void OnBackPressed()
    {
    }

    public override void OnDialogBecameVisible()
    {
    }

    public override void OnInitByCreate()
    {
        FindCloseBtn();
    }

    protected void FindCloseBtn()
    {
        Button[] btns = transform.GetComponentsInChildren<Button>();
        foreach (var item in btns)
        {
            if(item.name == "closeBtn")
            {
                closeBtn = item;
            }
        }
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(OnClickClooseBtn);
        }

    }

    protected void OnClickClooseBtn()
    {
        if (closeBtn != null)
        {
            var opanel = UIManager.Inst.uiCanvas.GetComponentsInChildren<IPanelOpposable>(true);
            for (int i = 0; i < opanel.Length; i++)
            {
                opanel[i].SetGlobalHide(true);
            }
            this.gameObject.SetActive(false);
        }
    }

}

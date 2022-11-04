/// <summary>
/// Author:Mingo-LiZongMing
/// Description:进入摆摊模式后的界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromoteCtrPanel : BasePanel<PromoteCtrPanel>
{
    public Button btnChosePromote;
    public Button btnExitPromote;
    public Transform btnPanel;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        btnChosePromote.onClick.AddListener(OnChosePromoteBtnClick);
        btnExitPromote.onClick.AddListener(OnExitBtnClick);
    }

    private void OnChosePromoteBtnClick()
    {
        PromoteManager.Inst.Select();
    }

    private void OnExitBtnClick()
    {
        PromoteManager.Inst.End();
    }

    public void SetBtnPanelVisible(bool isVisible)
    {
        btnPanel.gameObject.SetActive(isVisible);
    }
}

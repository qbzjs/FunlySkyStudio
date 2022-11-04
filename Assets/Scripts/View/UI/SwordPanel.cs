using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SwordPanel : BasePanel<SwordPanel>
{
    public Button attackBtn;
    public Button changeBtn;
    public Button closeBtn;
    public bool isLock;
    public void Init()
    {
        attackBtn.onClick.AddListener(OnAttackBtnClick);
        changeBtn.onClick.AddListener(OnChangeBtnClick);
        closeBtn.onClick.AddListener(OnCloseBtnClick);
    }

    public void OnAttackBtnClick()
    {
        if (isLock)
        {
            return;
        }
        if (!StateManager.Inst.IsCanPlayEmo())
        {
            return;
        }
        PlayerBaseControl.Inst.PlayerResetIdle();
        var swordInfo = SwordManager.Inst.swordDic[SwordManager.Inst.selfUid];
        SwordManager.Inst.PlaySwordAnim(SwordManager.Inst.selfUid, swordInfo.id, swordInfo.part);
        isLock = true;
    }

    public void OnChangeBtnClick()
    {
        if (!StateManager.Inst.IsCanPlayEmo())
        {
            return;
        }
        SwordManager.Inst.ShowSwordPanel();
    }

    public void OnCloseBtnClick()
    {
        if (!StateManager.Inst.IsCanPlayEmo())
        {
            return;
        }
        if (SwordManager.Inst.IsSelfInSword())
        {
            SwordManager.Inst.LeaveSword(SwordManager.Inst.selfUid, true);
        }
    }

    public void OnHide()
    {
        Hide();
        IsOnShow(true);
    }

    public void IsOnShow(bool isShow)
    {
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.jumpBtn.gameObject.SetActive(isShow);
        }
    }
}

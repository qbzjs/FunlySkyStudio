using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Author:Shaocheng
/// Description:攻击道具控制UI--攻击按钮
/// Date: 2022-4-14 17:44:22
/// </summary>
public class AttackWeaponCtrlPanel:BasePanel<AttackWeaponCtrlPanel>
{
    //TODO:未来可能拓展多个攻击动作
    public Button btnAttack;
    public Slider hitsSlider;
    public Transform ctrlPanel;
    public Image sliderFillImg;
    private const string defFillColor = "#FFFFFF";
    private const string hintFillColor = "#FF332F";

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        btnAttack.onClick.AddListener(OnAttackBtnClick);
        AttackWeaponManager.Inst.HideWeaponCtrPanel = Hide;
        AttackWeaponManager.Inst.ShowWeaponCtrPanel = ShowAttackCtrPanel;
    }

    private void ShowAttackCtrPanel()
    {
        if( PlayerAttackControl.Inst != null &&
            PlayerAttackControl.Inst.curAttackPlayer != null &&
            PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon != null)
        {
            Show();
        }
    }

    public override void OnDialogBecameVisible()
    {
        ShowWeaponHitsUI();
    }

    public void ShowWeaponHitsUI()
    {
        var weapon = PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon;
        var visible = weapon.OpenDurability == 1;
        hitsSlider.gameObject.SetActive(visible);
        if (visible)
        {
            hitsSlider.value = weapon.CurDurability / weapon.Durability;
            sliderFillImg.color = DataUtils.DeSerializeColorByHex(defFillColor);
            if (hitsSlider.value <= 0.2f)
            {
                sliderFillImg.color = DataUtils.DeSerializeColorByHex(hintFillColor);
            }
        }
    }

    public void CheckShowHide()
    {
        if(((PlayerOnBoardControl.Inst != null) && PlayerOnBoardControl.Inst.isOnBoard) ||
           ((PlayerSwimControl.Inst != null) && PlayerSwimControl.Inst.isInWater) ||
           StateManager.IsOnLadder || StateManager.IsOnSeesaw || StateManager.IsOnSwing
           || StateManager.IsOnSlide)
        {
            Hide();
        }
    }

    private void OnAttackBtnClick()
    {
        if (StateManager.IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return;
        }
        if (PlayerBaseControl.Inst!=null&&PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        //TODO: 控制Player攻击
        LoggerUtils.Log("OnAttackBtnClick");

        PlayerAttackControl.Inst.Attack();
    }

    public void SetCtrlPanelVisible(bool isVisible)
    {
        ctrlPanel.gameObject.SetActive(isVisible);
    }

}
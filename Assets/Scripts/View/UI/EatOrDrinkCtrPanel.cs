/// <summary>
/// Author:Mingo-LiZongMing
/// Description:
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EatOrDrinkCtrPanel : BasePanel<EatOrDrinkCtrPanel>
{
    public Button btnEatOrDrink;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        btnEatOrDrink.onClick.AddListener(OnBtnEatOrDrink);
    }

    private void OnBtnEatOrDrink()
    {
        if (StateManager.IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return;
        }
        if (StateManager.IsOnLadder)
        {
            LadderManager.Inst.ShowTips();
            return;
        }

        if (StateManager.IsOnSeesaw)
        {
            SeesawManager.Inst.ShowSeesawMutexToast();
            return ;
        }
        if (StateManager.IsOnSwing)
        {
            SwingManager.Inst.ShowSwingMutexToast();
            return;
        }
        if (StateManager.IsOnSlide)
        {
            return;
        }
        EdibilitySystemController.Inst.OnHandNodeFoodBtnClick();
    }

    public void SetCtrlPanelVisible(bool isVisible)
    {
        btnEatOrDrink.gameObject.SetActive(isVisible);
    }
    public bool GetCtrlPanelVisibleState()
    {
        return btnEatOrDrink.gameObject.activeSelf;
    }
}

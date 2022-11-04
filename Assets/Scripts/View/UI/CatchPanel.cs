/// <summary>
/// Author:Mingo-LiZongMing
/// Description:可拾起道具的UI控制
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CatchPanel:BasePanel<CatchPanel>
{
    public Button BtnCatch;
    public Button BtnDrop;
    public GameObject Mask;
    public GameObject BtnPanel;
    public ContinuousClickDetection catchBtnLock;
    public ContinuousClickDetection dropBtnLock;

    private NodeBaseBehaviour curBevh;

    private UnityAction catchAct;
    private UnityAction dropAct;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        catchBtnLock = BtnCatch.GetComponent<ContinuousClickDetection>();
        dropBtnLock = BtnDrop.GetComponent<ContinuousClickDetection>();
        catchBtnLock.AddListener(OnCatchBtnClick);
        dropBtnLock.AddListener(OnDropBtnClick);
        UiResolutionAdjustment();
    }

    public void SetCatchAction(UnityAction act)
    {
        catchAct = act;
    }

    private void OnCatchBtnClick()
    {
        if (!StateManager.Inst.CanCatchCurProp())
        {
            return;
        }
        SetMaskEnable(true,0.5f);
        catchAct?.Invoke();
        InputReceiver.locked = true;
        SetFoodCtrPanelActive(true);
    }

    private void OnDropBtnClick()
    {
        if (PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Pickability))
        {
            return;
        }
        if (StateManager.IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return;
        }
        if (!StateManager.Inst.CanDropCurProp())
        {
            return;
        }
        if (StateManager.IsOnLadder)//在梯子上
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
        if (StateManager.IsOnSlide)//在滑梯上
        {
            return;
        }
        SetMaskEnable(true, 1.3f);
        InputReceiver.locked = true;
        PickabilityManager.Inst.HandleDropProp();
        Hide();
    }

    public void SetCatchState(bool isCatched)
    {
        if (SceneParser.Inst.GetBaggageSet() == 1)
        {
            return;
        }
        BtnCatch.gameObject.SetActive(!isCatched);
        BtnDrop.gameObject.SetActive(isCatched);
    }

    public void SetBagCatchStateEnter(NodeBaseBehaviour curBehav)
    {
        if (BaggageManager.Inst.IsSelfBaggageFull())
        {
            BtnCatch.gameObject.SetActive(false);
            BtnDrop.gameObject.SetActive(true);
        }
        else
        {
            BtnCatch.gameObject.SetActive(true);
            BtnDrop.gameObject.SetActive(true);
        }
        if (BaggageManager.Inst.IsSelfBaggageNull())
        {
            BtnCatch.gameObject.SetActive(true);
            BtnDrop.gameObject.SetActive(false);
        }
    }

    public void SetBagCatchStateExit()
    {
        BtnCatch.gameObject.SetActive(false);
        if (BaggageManager.Inst.IsSelfBaggageNull())
        {
            BtnDrop.gameObject.SetActive(false);
        }
        else
        {
            BtnDrop.gameObject.SetActive(true);
        }
    }

    private void SetMaskEnable(bool isActive,float time)
    {
        if (isActive)
        {
            CancelInvoke("HideMask");
            Invoke("HideMask", time);
        }
        Mask.SetActive(isActive);
    }

    private void HideMask()
    {
        Mask.SetActive(false);
        InputReceiver.locked = false;
    }

    public void SetButtonVisible(bool isActive)
    {
        BtnPanel.SetActive(isActive);
    }
    public bool GetButtonVisibleState()
    {
        return BtnPanel.gameObject.activeSelf;
    }

    public void UiResolutionAdjustment()
    {
        var screenW = Screen.width;
        var screenH = Screen.height;
        if(screenW < 1920)
        {
            var rectCatch = BtnCatch.GetComponent<RectTransform>();
            var rectDrop = BtnDrop.GetComponent<RectTransform>();
            rectCatch.sizeDelta *= 0.85f;
            rectDrop.sizeDelta *= 0.85f;
        }
    }

    private void SetFoodCtrPanelActive(bool isActive)
    {
        if (isActive)
        {
            var selfUid = GameManager.Inst.ugcUserInfo.uid;
            var curHoldBev = PickabilityManager.Inst.GetBagHandleItemBevByPlayerId(selfUid);
            if(curHoldBev != null)
            {
                var entity = curHoldBev.entity;
                if (entity.HasComponent<EdibilityComponent>())
                {
                    EatOrDrinkCtrPanel.Show();
                }
            }
            else
            {
                EatOrDrinkCtrPanel.Hide();
            }
        }
        else
        {
            EatOrDrinkCtrPanel.Hide();
        }
    }
}

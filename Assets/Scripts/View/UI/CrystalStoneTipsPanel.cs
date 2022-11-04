using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrystalStoneTipsPanel : BasePanel<CrystalStoneTipsPanel>
{
    public GameObject FirstGo;
    public GameObject Step1Go;
    public GameObject Step2Go;
    public GameObject Step3Go;
    public Button HowToBtn;
    public Button Step1Btn;
    public Button Step2Btn;
    public Button StartBtn;
    private bool lockState;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        HowToBtn.onClick.AddListener(OnHowToBtnClick);
        Step1Btn.onClick.AddListener(OnStep1BtnClick);
        Step2Btn.onClick.AddListener(OnStep2BtnClick);
        StartBtn.onClick.AddListener(OnStartBtnClick);
    }
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        FirstGo.SetActive(true);
        Step1Go.SetActive(false);
        Step2Go.SetActive(false);
        Step3Go.SetActive(false);
        OnNextStepClick(null, FirstGo);
        //禁用JoyStick操作
        lockState = InputReceiver.locked;
        InputReceiver.locked = true;
    }
    public override void OnBackPressed()
    {
        base.OnBackPressed();
        //复原JoyStick操作
        InputReceiver.locked = lockState;
    }
    private void OnNextStepClick(GameObject upGo, GameObject nextGO)
    {
        if (upGo)
        {
            upGo.SetActive(false);
        }
        if (nextGO)
        {
            nextGO.SetActive(true);
        }
    }
    private void OnHowToBtnClick()
    {
        OnNextStepClick(FirstGo, Step1Go);
    }
    private void OnStep1BtnClick()
    {
        OnNextStepClick(Step1Go, Step2Go);
    }
    private void OnStep2BtnClick()
    {
        OnNextStepClick(Step2Go, Step3Go);
    }
    private void OnStartBtnClick()
    {
        HidePanel();
        PlayerPrefs.SetInt(CrystalStoneManager.Inst.GREATSNOW_FIRST_TIP_KEY, 1);
        PlayerPrefs.Save();
    }
    private void HidePanel()
    {
        UIControlManager.Inst.CallUIControl("snowfield_first_tip_exit");
    }
}

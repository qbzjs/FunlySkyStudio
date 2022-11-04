/// <summary>
/// Author:zhouzihan
/// Description:游泳面板
/// Date: #CreateTime#
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[Serializable]
public class WaterModePanel 
{
    public GameObject waterModeBtns;
    public Button jumpInWaterBtn;
    public Button setSwimBtn;
    public GameObject swimModeBtns;
    public Button stopSwimBtn;
    public Button swimDownBtn;
    public Button swimHighBtn;
    private PlayerBaseControl playerCom;
    public void InitBtn(PlayerBaseControl playerControl)
    {
        playerCom = playerControl;
        jumpInWaterBtn.onClick.AddListener(OnJumpInWaterClick);
        setSwimBtn.onClick.AddListener(SetSwimBtnClick);
        stopSwimBtn.onClick.AddListener(StopSwimBtnClick);
    }
    private void OnJumpInWaterClick()
    {
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return;
        }
        PlayerSwimControl.Inst.JumpInWater();
    }
    public void SetSwimBtnClick()
    {
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            TipPanel.ShowToast("You could not swim while promoting");
            return;
        }
        if (playerCom.animCon != null && (playerCom.animCon.isLooping || playerCom.animCon.isInteracting))
        {
            playerCom.animCon.StopLoop();

            if (PlayerMutualControl.Inst)
            {
                PlayerMutualControl.Inst.StopFollowerLoop();
            }
            return;
        }
        waterModeBtns.gameObject.SetActive(false);
        swimModeBtns.gameObject.SetActive(true);

        PlayerSwimControl.Inst.SetSwim();
    }
    public void StopSwimBtnClick()
    {
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            TipPanel.ShowToast("You could not swim while promoting");
            return;
        }
        if (playerCom.animCon != null && (playerCom.animCon.isLooping || playerCom.animCon.isInteracting))
        {
            playerCom.animCon.StopLoop();

            if (PlayerMutualControl.Inst)
            {
                PlayerMutualControl.Inst.StopFollowerLoop();
            }
            return;
        }
        waterModeBtns.gameObject.SetActive(true);
        swimModeBtns.gameObject.SetActive(false);

        PlayerSwimControl.Inst.StopSwim();
    }
    public void ClearWaterBtn()
    {
        waterModeBtns.gameObject.SetActive(false);
        swimModeBtns.gameObject.SetActive(false);
    }
}

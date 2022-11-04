/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/9/1 18:39:26
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
[Serializable]
public class LadderModePanel
{
    public GameObject LadderModeBtns;
    private GameObject moveBtns;
    private EventTrigger UpBtn;
    private EventTrigger DownBtn;
    private PlayerBaseControl playerCom;
    bool isOnLadder;
    public void InitBtn(PlayerBaseControl playerControl)
    {
        playerCom = playerControl;
      
    
        moveBtns = LadderModeBtns.transform.Find("moveBtn").gameObject;
        UpBtn = moveBtns.transform.Find("UpBtn").GetComponent<EventTrigger>();
        DownBtn = moveBtns.transform.Find("DownBtn").GetComponent<EventTrigger>();

        AddListener();
    }
    public void SetLadderModePanelShow(bool isShow)
    {
        moveBtns.SetActive(isShow);
    }

    public void OnPlayerMoveUpClickDown(BaseEventData data)
    {



        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return;
        }
        LadderManager.Inst.SetPlayerOnLadderMove(LadderManager.OnLadderMoveStatus.Up);
    }

    public void OnPlayerMoveDownClickDown(BaseEventData data)
    {

        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return;
        }
        LadderManager.Inst.SetPlayerOnLadderMove(LadderManager.OnLadderMoveStatus.Down);
    }

    public void OnPlayerMoveClickUp(BaseEventData data)
    {
      
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        {
            return;
        }
        LadderManager.Inst.SetPlayerOnLadderMove(LadderManager.OnLadderMoveStatus.Stay);
    }

    public void AddListener()
    {

        AddEvent(EventTriggerType.PointerDown, OnPlayerMoveUpClickDown, UpBtn);
        AddEvent(EventTriggerType.PointerDown, OnPlayerMoveDownClickDown, DownBtn);
        AddEvent(EventTriggerType.PointerUp, OnPlayerMoveClickUp, DownBtn);
        AddEvent(EventTriggerType.PointerUp, OnPlayerMoveClickUp, UpBtn);
    }
    public void AddEvent(EventTriggerType type, UnityAction<BaseEventData> action,EventTrigger trigger)
    {
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = type;
        enter.callback = new EventTrigger.TriggerEvent();
        UnityAction<BaseEventData> callback = new UnityAction<BaseEventData>(action);
        enter.callback.AddListener(callback);
        trigger.triggers.Add(enter);

    }
    public void PlayerDownLadderClick()
    {

        LadderManager.Inst.PlayerSendDownLadder();
    }
         
}

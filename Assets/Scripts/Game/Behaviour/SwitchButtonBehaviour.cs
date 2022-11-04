using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description: 开关道具行为
/// Date: 2022-3-30 19:43:08
/// </summary>
public class SwitchButtonBehaviour : NodeBaseBehaviour
{
    public bool isWork = false;//�����Ƿ�������
    private TextMeshPro textMesh;

    private Animator mAnimator;
    private Color[] colors;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        mAnimator = this.GetComponentInChildren<Animator>();
        textMesh = this.GetComponentInChildren<TextMeshPro>();
        mAnimator.Play("Inacbtn", 0, 0);
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener(MessageName.SaveCoverStateChange, OnSavePhoto);

    }

    public override void OnReset()
    {
        base.OnReset();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener(MessageName.SaveCoverStateChange, OnSavePhoto);

    }

    private void OnDestroy()
    {
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener(MessageName.SaveCoverStateChange, OnSavePhoto);
    }

    private void OnClickSwitch()
    {
        AKSoundManager.Inst.PostEvent("play_button", gameObject);
        mAnimator.Play("Inacbtn", 0, 0);
        OnPVPWinCondition();
        //test
        // #if UNITY_EDITOR
        //         if (GlobalFieldController.CurGameMode == GameMode.Guest && ClientManager.Inst.isOnline)
        //         {
        //             //mapId = "1465952717637517312_1638345577_1",
        //             MapInfo testMapInfo = new MapInfo()
        //             {
        //                 mapId = "1465952717637517312_1638345577_1"
        //             };
        //             GlobalFieldController.CurMapInfo = testMapInfo.Clone();
        //             Invoke("SendRequest", 0.5f);
        //         }
        //         else
        //         {
        //             isWork = !isWork;
        //             Invoke("OnSwitchClickedLocal", 0.7f);
        //         }
        //
        //         return;
        // #endif

        //本地先进行表现
        isWork = !isWork;
        Invoke("OnSwitchClickedLocal", 0.5f);
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            SendRequest();
        }
        

    }

    private void OnPVPWinCondition()
    {
        if (!PVPWaitAreaManager.Inst.IsSameGameMode(PVPServerTaskType.Race) || !PVPWaitAreaManager.Inst.IsPVPGameStart)
        {
            return;
        }
        if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            var switchID = entity.Get<SwitchButtonComponent>().switchId;
            if (PVPWaitAreaManager.Inst.IsCompleteCondition(switchID) && PVPWaitAreaManager.Inst.IsPVPGameStart)
            {
                if (GameManager.Inst.ugcUserInfo == null)
                {
                    LoggerUtils.LogError("PvpWinner GameManager.Inst.ugcUserInfo == null ");
                    return;
                }
                PVPSyncDataOnServer pvpData = new PVPSyncDataOnServer()
                {
                    winner = GameManager.Inst.ugcUserInfo.uid,
                    round = GlobalFieldController.PVPRound,
                    winType = (int)ClientManager.BattleWinCondition.Switch
                };
                PVPManager.Inst.SendPVPGameDataReq(JsonConvert.SerializeObject(pvpData));
            }
        }
        else
        {
            var switchID = entity.Get<SwitchButtonComponent>().switchId;
            if (PVPWaitAreaManager.Inst.IsCompleteCondition(switchID))
            {
                PVPWinConditionGamePlayPanel.Instance.SetWinner(PVPGameOverPanel.GameOverStateEnum.Win);
            }
        }
    }

    private void OnSwitchClickedLocal()
    {
        var uids = entity.Get<SwitchButtonComponent>().controllUids;
        foreach (var uid in uids)
        {
            ShowHideManager.Inst.OnSwitchClick(uid);
        }

        var moveUids = entity.Get<SwitchButtonComponent>().moveControllUids;
        foreach (var uid in moveUids)
        {
            SwitchControlManager.Inst.OnSwitchClick(uid);
        }

        var soundUids = entity.Get<SwitchButtonComponent>().soundControllUids;
        foreach (var uid in soundUids)
        {
            SwitchControlManager.Inst.OnSwitchPlaySound(uid);
        }

        var AnimUids = entity.Get<SwitchButtonComponent>().animControllUids;
        foreach (var uid in AnimUids)
        {
            SwitchControlManager.Inst.OnSwitchRefreshAnim(uid);
        }
        var FireworkUids = entity.Get<SwitchButtonComponent>().fireworkControllUids;
        foreach (var uid in FireworkUids)
        {
            SwitchControlManager.Inst.OnSwitchPlayFirework(uid);
        }
    }

    private void SendRequest()
    {
        SwitchPack s = new SwitchPack
        {
            status = isWork ? 1 : 0,
        };
        Item itemData = new Item()
        {
            id = entity.Get<GameObjectComponent>().uid,
            type = (int)ItemType.SWITCH,
            data = JsonConvert.SerializeObject(s),
        };
        Item[] itemsArray = { itemData };
        SyncItemsReq itemsReq = new SyncItemsReq()
        {
            mapId = GlobalFieldController.CurMapInfo.mapId,
            items = itemsArray,
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Items,
            data = JsonConvert.SerializeObject(itemsReq),
        };
        LoggerUtils.Log("SwitchButton SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }


    public override void OnRayEnter()
    {
        HighLight(true);
        PortalPlayPanel.Show();
        PortalPlayPanel.Instance.AddButtonClick(OnClickSwitch);
        PortalPlayPanel.Instance.SetTransform(transform);
        ChangePVPConditionIcon();
    }

    private void ChangePVPConditionIcon()
    {
        if (!PVPWaitAreaManager.Inst.IsSameGameMode(PVPServerTaskType.Race))
        {
            return;
        }
        var switchID = entity.Get<SwitchButtonComponent>().switchId;
        if (PVPWaitAreaManager.Inst.IsCompleteCondition(switchID) && PVPWaitAreaManager.Inst.IsPVPGameStart)
        {
            PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.PVP);
        }
        else
        {
            PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Hand);
        }
    }

    public override void OnRayExit()
    {
        HighLight(false);
        PortalPlayPanel.Hide();
    }


    public void ShowIndexNum()
    {
        if (textMesh != null)
        {
            textMesh.text = this.entity.Get<SwitchButtonComponent>().switchId.ToString();
        }
    }

    public void OnChangeMode(GameMode mode)
    {
        if (textMesh != null)
        {
            textMesh.gameObject.SetActive(mode == GameMode.Edit);
        }
        else
        {
            LoggerUtils.Log("textMesh is null");
        }
    }

    public void OnSavePhoto()
    {
        if (textMesh != null)
        {
            textMesh.gameObject.SetActive(!GlobalFieldController.isScreenShoting);
        }
        else
        {
            LoggerUtils.Log("textMesh is null");
        }
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref colors);
    }

}

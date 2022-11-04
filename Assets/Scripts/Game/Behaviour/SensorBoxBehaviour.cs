using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using BudEngine.NetEngine;

/// <summary>
/// Author:JayWill
/// Description:感应盒Behaviour
/// </summary>
public class SensorBoxBehaviour : NodeBaseBehaviour
{
    public int UsedTimes = 0;//当前已使用次数
    public int SensorStatus = 0;//当前触发状态 0:未触发 1:已触发
    private Color[] orginColors;
    private GameObject boxGameObject;
    private MeshRenderer[] meshRenders;
    private TextMeshPro textPro;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        boxGameObject = transform.GetChild(0).gameObject;
        meshRenders = GetComponentsInChildren<MeshRenderer>();
        if (textPro == null)
        {
            textPro = this.GetComponentInChildren<TextMeshPro>(true);
        }
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public void Clear()
    {
        SensorStatus = 0;
        UsedTimes = 0;
    }

    public override void OnReset()
    {
        base.OnReset();
        Clear();
        textPro.text = "";
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public void RefreshIndex()
    {
        var tComp = entity.Get<SensorBoxComponent>();
        textPro.text = tComp.boxIndex.ToString();
    }

    public void SetBoxVisiable(bool state)
    {
        if (meshRenders == null) return;
        foreach (var item in meshRenders)
        {
            item.enabled = state;
        }
    }

    private void OnChangeMode(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            SetBoxVisiable(true);
        }
        else
        {
            SetBoxVisiable(false);
        }
    }

    public override void OnTrigEnter()
    {
        base.OnTrigEnter();
        //without trigger protected
        if (ReferManager.Inst.isRefer)
        {
            return;
        }
        OnBoxEnter();
    }

    //触发感应盒
    private void OnBoxEnter()
    {

        var sComp = entity.Get<SensorBoxComponent>();
        int index = sComp.boxIndex;
        LoggerUtils.Log("SensorBoxBehaviour OnBoxEnter:" + index);

        //已达使用次数
        if (sComp.boxTimes > 0 && UsedTimes >= sComp.boxTimes)
        {
            LoggerUtils.Log("该感应盒为一次性，已使用");
            return;
        }

        if (SensorStatus == 0)
        {
            SensorStatus = 1;
        }
        else
        {
            SensorStatus = 0;
        }
        UsedTimes ++;

        LocalShow();
        OnPVPWinCondition();
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            SendRequest();
        }
    }
    
    private void OnPVPWinCondition()
    {
        if (!PVPWaitAreaManager.Inst.IsSameGameMode(PVPServerTaskType.SensorBox) || !PVPWaitAreaManager.Inst.IsPVPGameStart)
        {
            return;
        }
        if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            var sensorIndex = entity.Get<SensorBoxComponent>().boxIndex;
            if (PVPWaitAreaManager.Inst.IsCompleteCondition(sensorIndex))
            {
                if (GameManager.Inst.ugcUserInfo == null)
                {
                    LoggerUtils.LogError("Sensor PvpWinner  GameManager.Inst.ugcUserInfo == null ");
                    return;
                }
                PVPSyncDataOnServer pvpData = new PVPSyncDataOnServer()
                {
                    winner = GameManager.Inst.ugcUserInfo.uid,
                    round = GlobalFieldController.PVPRound,
                    winType = (int)ClientManager.BattleWinCondition.Sensor
                };
                PVPManager.Inst.SendPVPGameDataReq(JsonConvert.SerializeObject(pvpData));
            }
        }
        else
        {
            var sensorIndex = entity.Get<SensorBoxComponent>().boxIndex;
            if (PVPWaitAreaManager.Inst.IsCompleteCondition(sensorIndex))
            {
                PVPWinConditionGamePlayPanel.Instance.SetWinner(PVPGameOverPanel.GameOverStateEnum.Win);
            }
        }
    }
    

    //本地预测展示
    private void LocalShow()
    {
        LoggerUtils.Log("SebsorBoxBehaviour LocalShow");
        SensorBoxManager.Inst.HandleSensorBoxTouch(this);
        SensorBoxManager.Inst.LocalHandeFireworkTouch(this);
    }

    private void SendRequest()
    {
        var sComp = entity.Get<SensorBoxComponent>();
        SensorBoxProtoData s = new SensorBoxProtoData
        {
            status = SensorStatus,
            times = sComp.boxTimes,
        };
        
        Item itemData = new Item()
        {
            id = entity.Get<GameObjectComponent>().uid,
            type = (int)ItemType.SENSOR_BOX,
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
        LoggerUtils.Log("SensorBox SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }


    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnEdit(isHigh, boxGameObject, ref orginColors,3);
    }
}

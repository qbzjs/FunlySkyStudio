/// <summary>
/// Author:Mingo-LiZongMing
/// Description:大地图和子地图之间的传送管理器
/// Date: 2022-5-17 17:44:22
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using HLODSystem;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

public class DowntownTransferManager : CInstance<DowntownTransferManager>, IManager
{
    private List<NodeBaseBehaviour> _transferBevList = new List<NodeBaseBehaviour>();
    #region 设置地图数据
    public void AddTransferBev(NodeBaseBehaviour baseBev, DowntownTransferData data)
    {
        var entity = baseBev.entity;
        var transComp = entity.Get<TransferComponent>();
        transComp.transId = data.transId;
        transComp.transType = data.transType;
        _transferBevList.Add(baseBev);
    }

    public void SetTransferData(List<string> subMaps)
    {
        if (_transferBevList == null)
            return;
        GameManager.Inst.subMapsData = subMaps;
        for (int i = 0; i < subMaps.Count; i++)
        {
            var transBev = _transferBevList.Find(x => x.entity.Get<TransferComponent>().transId == i + 1);
            if (transBev != null)
            {
                var entity = transBev.entity;
                var transComp = entity.Get<TransferComponent>();
                transComp.subMapId = subMaps[i];
            }
        }
    }
    #endregion

    #region 联机控制
    public void SendMsgToSever(TransAnimType type)
    {
        var roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Custom,
            data = JsonConvert.SerializeObject(new RoomChatCustomData()
            {
                type = (int)ChatCustomType.DowntownTransfer,
                data = type.ToString()
            })
        };
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }

    // 接收服务器消息
    public bool OnReceiveServer(string senderPlayerId, string data)
    {
        LoggerUtils.Log(string.Format("PlayerTransferManager OnReceiveServer. senderPlayerId = {0}", senderPlayerId));
        if (senderPlayerId != GameManager.Inst.ugcUserInfo.uid)
        {
            var otherPlayerCtr = ClientManager.Inst.GetOtherPlayerComById(senderPlayerId);
            if (otherPlayerCtr != null)
            {
                switch (GetTransEnum(data))
                {
                    case TransAnimType.Up:
                        otherPlayerCtr.animCon.PlayAnim(null, "portal_up", -1);
                        break;
                    case TransAnimType.End:
                        otherPlayerCtr.animCon.PlayAnim(null, "portal_down", -1);
                        break;
                }
                TimerManager.Inst.RunOnce("TransferAnim", 1f, () =>
                {
                    otherPlayerCtr.animCon.RleasePrefab();
                    otherPlayerCtr.animCon.CancelLastEmo();
                });
            }
        }
        return true;
    }
    #endregion

    public TransAnimType GetTransEnum(string type)
    {
        return (TransAnimType)System.Enum.Parse(typeof(TransAnimType), type);
    }

    public TransferBehaviour CreateDowntownTransfer(Vector3 pos)
    {
        var node = SceneBuilder.Inst.CreateSceneNode<TransferCreater, TransferBehaviour>();
        node.transform.position = pos;
        var comp = node.entity.Get<TransferComponent>();
        comp.transType = (int)TransferType.UGCMapTransfer;
        RecordTransBev(node);
        SceneBuilder.Inst.allControllerBehaviours.Add(node);
        return node;
    }

    public void RecordTransBev(NodeBaseBehaviour baseBev)
    {
        _transferBevList.Add(baseBev);
    }

    private void RemoveTransBev(NodeBaseBehaviour baseBev)
    {
        if (_transferBevList.Contains(baseBev))
        {
            _transferBevList.Remove(baseBev);
        }
    }

    public void CreateDefaultTransferPoint()
    {
        if(GameManager.Inst.sceneType != SCENE_TYPE.MAP_SCENE && GameManager.Inst.sceneType != SCENE_TYPE.Downtown)
        {
            HideAllTransferPoint();
            return;
        }
        //情况1:当前为编辑/游玩模式
        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            //1.如果不是白名单用户
            if (GameManager.Inst.isInWhiteList == 0)
            {
                HideAllTransferPoint();
                return;
            }

            //2.如果此地图没有创建过传送点 并且 是白名单用户的话，默认创建一个传送点
            if (_transferBevList.Count > 0)
                return;

            CreateDowntownTransfer(Vector3.zero);
        }
        else if(GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            //情况2:当前为游玩模式，但是入口不是Downtown
            if (!GlobalFieldController.IsDowntownEnter)
            {
                HideAllTransferPoint();
            }
        }
    }

    private void HideAllTransferPoint()
    {
        foreach(var transBev in _transferBevList)
        {
            transBev.gameObject.SetActive(false);
        }
    }

    public bool IsCanDesTarget()
    {
        if (_transferBevList.Count > 1)
        {
            return true;
        }
        TipPanel.ShowToast("At least 1 teleport beam");
        return false;
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.DowntownTransfer)
        {
            RemoveTransBev(behaviour);
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.DowntownTransfer)
        {
            RecordTransBev(behaviour);
        }
    }

    public void Clear()
    {
        _transferBevList.Clear();
    }

    public void TransferToDowntown(string curMapId)
    {
        //将地图ID还原为Downtown地图的MapId
        GlobalFieldController.curMapMode = MapMode.Downtown;
        GameManager.Inst.curDiyMapId = GameManager.Inst.gameMapInfo.mapId;
        PlayerManager.Inst.ExitShowPlayerState();
        SceneBuilder.Inst.BgBehaviour.Stop();
        SceneBuilder.Inst.BgBehaviour.StopEnr();
        WeatherManager.Inst.PauseShowWeather();
        //背包模式传送时不执行拾取的逻辑
        if (SceneParser.Inst.GetBaggageSet() != 1)
        {
            PickabilityManager.Inst.OnDealPortal();
        }
        else
        {
            PickabilityManager.Inst.OnBaggageDealPortal();
        }
        SceneSystem.Inst.StopSystem();
        SceneBuilder.Inst.DestroyScene();
        BloodPropManager.Inst.ClearBloodPropDict();
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        PortalPlayPanel.Hide();
        SceneBuilder.Inst.DowntownParseAndBuild(GameManager.Inst.downtownJson);
        SceneBuilder.Inst.SpawnPoint.SetActive(false);
        SceneBuilder.Inst.BgBehaviour.Play(true);
        SceneBuilder.Inst.BgBehaviour.PlayEnr();
        WeatherManager.Inst.ShowCurrentWeather();
        FollowModeManager.Inst.OnChangeMode(GameMode.Guest);
        BaggageManager.Inst.InitBaggageVisiable();
        SceneBuilder.Inst.SetEntityMeshsVisibleByMode(false);
        SceneSystem.Inst.StartSystem();
        PlayerManager.Inst.StartShowPlayerState();
        MessageHelper.Broadcast(MessageName.ChangeMode, GlobalFieldController.CurGameMode);
        ClientManager.Inst.SendGetItems();
        PlayModePanel.Instance.EntryPortalMode(true);
        PlayModePanel.Instance.SetFlyButtonVisibleByPortal();
        SetPlayerPos(curMapId);
        DowntownLoadingPanel.Hide();
        MessageHelper.Broadcast(MessageName.OnEnterSnowfield);
    }

    private void SetPlayerPos(string subMapId)
    {
        var lastPos = GetTransPosBySubMapId(subMapId);
        PlayerBaseControl.Inst.SetPlayerPosAndRot(lastPos, Quaternion.identity);
    }

    private Vector3 GetTransPosBySubMapId(string curMapId)
    {
        foreach (var transBev in _transferBevList)
        {
            var entity = transBev.entity;
            var transComp = entity.Get<TransferComponent>();
            var subMapId = transComp.subMapId;
            if(subMapId == curMapId)
            {
                return transBev.transform.position + new Vector3(1, 0, 1);
            }
        }
        return SpawnPointManager.Inst.GetSpawnPoint().transform.localPosition;
    }
}
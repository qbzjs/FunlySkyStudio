using System.Collections.Generic;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;

public class FreezePropsSession
{
    public enum EFreezeOpt
    {
        UnFreeze,
        Freeze,
    }
    public class ReqPlayerData
    {
        public string playerId { get; set; }
        public float keepTime { get; set; }
        public int state { get; set; }
    }
    public class RespPlayerData
    {
        public string playerId { get; set; }
        public float keepTime { get; set; }
        public int state { get; set; }
        public long freezeTime { get; set; }
    }
    public class ReqItemData
    {
        public ReqPlayerData[] affectPlayers;
    }
    public class RespItemDataFreeze
    {
        public RespPlayerData[] affectPlayers;
    }
    public class RespItemDataUnFreeze
    {
        public RespPlayerData affectPlayer;
    }
    public FreezePropsManager mManager;
    public FreezePropsSession(FreezePropsManager manager)
    {
        mManager = manager;
    }
    public void ReqTriggerFreezeProps(SceneEntity entity)
    {
        int freezeTime = 0;
        if (entity.HasComponent<FreezePropsComponent>())
        {
            freezeTime = entity.Get<FreezePropsComponent>().mFreezeTime;
        }
        List<ReqPlayerData> affectDatas = new List<ReqPlayerData>();
        FillPlayerList(affectDatas, freezeTime);
        ReqItemData itemData = new ReqItemData();
        itemData.affectPlayers = affectDatas.ToArray();
        var uid = entity.Get<GameObjectComponent>().uid;
        Item[] itemsArray =
        {
            new Item()
            {
                id = uid,
                type = (int) ItemType.FREEZEPROPS,
                data = JsonConvert.SerializeObject(itemData),
            }
        };

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

        string jsonData = JsonConvert.SerializeObject(roomChatData);
        LoggerUtils.Log($"FreezePropsSession ReqTriggerFreezeProps ==> =>:{jsonData}");
        ClientManager.Inst.SendRequest(jsonData, null);
    }
    public void FillPlayerList(List<ReqPlayerData> affectDatas, int freezeTime)
    {
        List<string> players = new List<string>();
        //如果是双人动作就同时冻结
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            players.Add(PlayerMutualControl.Inst.followPlayerId);
            players.Add(PlayerMutualControl.Inst.startPlayerId);
        }
        else
        {
            players.Add(Player.Id);
        }

        for (int i = 0; i < players.Count; i++)
        {
            ReqPlayerData affectData = new ReqPlayerData();
            affectData.playerId = players[i];
            affectData.keepTime = freezeTime;
            affectData.state = (int)EFreezeOpt.Freeze;
            affectDatas.Add(affectData);
        }
    }
    public bool OnReceiveServerFreeze(string sendPlayerId, string msg)
    {
        LoggerUtils.Log($"FreezePropsSession OnReceiveServerFreeze ==> => senderPlayer:{sendPlayerId}, msg:{msg}");
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                if (item.type == (int)ItemType.FREEZEPROPS)
                {
                    if (string.IsNullOrEmpty(item.data))
                    {
                        LoggerUtils.Log("[FreezePropsSession.OnReceiveServerFreeze.Items.item.Data is null");
                        continue;
                    }

                    var uid = item.id;
                    var behaviour = mManager.GetNodeByUid(uid);
                    if (behaviour)
                    {
                        behaviour.gameObject.SetActive(false);
                        FreezePropsNodeAuxiliary nodeAuxiliary = mManager.GetNodeAuxiliary(behaviour);
                        if (nodeAuxiliary != null)
                        {
                            nodeAuxiliary.propIsUsed = true;
                        }
                    }

                    RespItemDataFreeze itemDatas = JsonConvert.DeserializeObject<RespItemDataFreeze>(item.data);

                    if (itemDatas != null && itemDatas.affectPlayers != null)
                    {
                        foreach (var playerData in itemDatas.affectPlayers)
                        {
                            if (playerData.state==(int)EFreezeOpt.Freeze)
                            {
                                mManager.FreezePlayerWithNet(playerData.playerId, playerData.keepTime);
                            }
                            else if (playerData.state==(int)EFreezeOpt.UnFreeze)
                            {
                                mManager.UnFreezePlayerWithNet(playerData.playerId);
                            }
                            

                        }
                    }
                }
            }
        }
        return true;
    }
    public void OnGetItemsCallback(string dataJson)
    {
        if (!string.IsNullOrEmpty(dataJson))
        {
            LoggerUtils.Log($"FreezeSettion. OnGetItemsCallback=={dataJson}");
            GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
            if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
            {
                LoggerUtils.Log("[FreezePropsManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
                return;
            }

            if (getItemsRsp.mapId == GlobalFieldController.CurMapInfo.mapId)
            {
                if (getItemsRsp.items == null)
                {
                    LoggerUtils.Log("[FreezePropsManager.OnGetItemsCallback]getItemsRsp.items is null");
                    return;
                }

                for (int i = 0; i < getItemsRsp.items.Length; i++)
                {
                    Item item = getItemsRsp.items[i];
                    if (item.type != (int)ItemType.FREEZEPROPS)
                    {
                        continue;
                    }

                    var uid = item.id;
                    // 冻结道具同步消失
                    var node = mManager.GetNodeByUid(uid);
                    if (node)
                    {
                        node.gameObject.SetActive(false);
                        FreezePropsNodeAuxiliary nodeAuxiliary = FreezePropsManager.Inst.GetNodeAuxiliary(node);
                        if (nodeAuxiliary != null)
                        {
                            nodeAuxiliary.propIsUsed = true;
                        }
                    }

                    if (string.IsNullOrEmpty(item.data))
                    {
                        LoggerUtils.Log("[FreezePropsManager.OnGetItemsCallback]getItemsRsp.item.Data is null");
                        continue;
                    }
                }
                PlayerCustomData[] playerCustomDatas = getItemsRsp.playerCustomDatas;
                for (int i = 0; i < playerCustomDatas.Length; i++)
                {
                    PlayerCustomData playerData = playerCustomDatas[i];
                    ActivityData[] activitiesData = playerData.activitiesData;
                    if (activitiesData==null)
                    {
                        continue;
                    }
                    for (int n = 0; n < activitiesData.Length; n++)
                    {
                        ActivityData activeData = activitiesData[n];
                        if (activeData.activityId == ActivityID.FreezeItem)
                        {
                            if (activeData != null)
                            {
                                RespPlayerData playerFreezeData = JsonConvert.DeserializeObject<RespPlayerData>(activeData.data);
                                //当前玩家的冻结状态
                                bool isFreezeInLocal = mManager.mNetFreezeManager.CheckerPlayerIsFreeze(playerData.playerId);
                                EFreezeOpt localState = isFreezeInLocal ? EFreezeOpt.Freeze : EFreezeOpt.UnFreeze;
                                if ((int)localState == playerFreezeData.state)
                                {
                                    //服务器和本地的冻结状态一致，(如果是冰冻状态，变更冻结时间)
                                    if (playerFreezeData.state == (int)EFreezeOpt.Freeze)
                                    {
                                        long longLeftFreezeTime = GameUtils.GetMilliTimeStamp() - playerFreezeData.freezeTime;
                                        float fLeftFreezeTime = playerFreezeData.keepTime- longLeftFreezeTime / 1000;
                                        //获取定时器
                                        FreezeTimer timer= mManager.mTimerManager.GetTimer(playerData.playerId);
                                        if (timer!=null)
                                        {
                                            timer.ForceSplitTime(fLeftFreezeTime);
                                        }
                                    }
                                }
                                else
                                {
                                    //如果是冰冻状态，，把玩家冰冻起来
                                    if (playerFreezeData.state== (int)EFreezeOpt.Freeze)
                                    {
                                        long longLeftFreezeTime = GameUtils.GetMilliTimeStamp() - playerFreezeData.freezeTime;
                                        float fLeftFreezeTime = playerFreezeData.keepTime-longLeftFreezeTime / 1000;
                                        //解冻
                                        if (playerData.playerId == Player.Id)
                                        {
                                            mManager.mNetFreezeManager.MainPlayerFreeze(playerData.playerId, fLeftFreezeTime);
                                        }
                                        else
                                        {
                                            mManager.mNetFreezeManager.OtherPlayerFreeze(playerData.playerId, fLeftFreezeTime);
                                        }
                                    }
                                    else
                                    {
                                        //解冻
                                        if (playerData.playerId == Player.Id)
                                        {
                                            mManager.mNetFreezeManager.MainPlayerUnFreeze(playerData.playerId);
                                        }
                                        else
                                        {
                                            mManager.mNetFreezeManager.OtherPlayerUnFreeze(playerData.playerId);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }
    }
}

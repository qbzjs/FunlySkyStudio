/// <summary>
/// Author:Mingo-LiZongMing
/// Description:
/// </summary>
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

public struct FoodSyncData
{
    public int uid;
    public string playerId;
    public int opType;
}

public class EdibilitySystemController : CInstance<EdibilitySystemController>
{
    private const int EatOpType = 1;
    private const int RestoreOpType = 2;
    private const float ActiveTimer = 15;
    private const string FoodTimerName = "FoodTimer";

    public bool isSelfEating = false;
    private BudTimer curtimer;


    /// <summary>
    /// 点击场景中可食用道具上的食用按钮
    /// </summary>
    /// <param name="baseBev"></param>
    public void OnSceneNodeFoodBtnClick(NodeBaseBehaviour baseBev)
    {
        if (!StateManager.Inst.CheckCanEat())
        {
            return;
        }
        if (isSelfEating)
        {
            return;
        }
        if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            SendMsgToClient(baseBev);
        }
        else
        {
            DoEatOrDrinkBehaviour(GameManager.Inst.ugcUserInfo.uid, baseBev);
        }

        if (PlayerSnowSkateControl.Inst)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
        InputReceiver.locked = true;
        isSelfEating = true;
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.joyStick.JoystickReset();
        }
        if (CatchPanel.Instance)
        {
            CatchPanel.Instance.SetButtonVisible(false);
        }
        PlayerBaseControl.Inst.Move(Vector3.zero);
        PlayerBaseControl.Inst.animCon.isEating = true;
        InputReceiver.locked = false;
        if (curtimer != null)
        {
            TimerManager.Inst.Stop(curtimer);
        }
        curtimer = TimerManager.Inst.RunOnce("CancelFoodLock", 4.5f, () => { CancelLock(); });
    }

    /// <summary>
    /// 点击右下角拾起食物后的吃东西按钮
    /// </summary>
    public void OnHandNodeFoodBtnClick()
    {
        if (!StateManager.Inst.CheckCanEat())
        {
            return;
        }
        if (isSelfEating)
        {
            return;
        }
        var selfUid = GameManager.Inst.ugcUserInfo.uid;
        var pickState = PickabilityManager.Inst.GetPlayerPickState(selfUid);
        if (pickState)
        {
            var curHoldBev = PickabilityManager.Inst.GetBagHandleItemBevByPlayerId(selfUid);
            if (curHoldBev != null)
            {
                EatOrDrinkCtrPanel.Hide();
                OnSceneNodeFoodBtnClick(curHoldBev);
            }
        }
    }

    public void HandlePickableFood(NodeBaseBehaviour baseBev, bool isPick, string playerId)
    {
        if(playerId != GameManager.Inst.ugcUserInfo.uid)
        {
            return;
        }
        var entity = baseBev.entity;
        var hasEdibility = entity.HasComponent<EdibilityComponent>();
        if (isPick && hasEdibility)
        {
            EatOrDrinkCtrPanel.Show();
        }
        else
        {
            EatOrDrinkCtrPanel.Hide();
        }
    }

    /// <summary>
    /// 控制对应玩家的的吃东西的行为
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="baseBev"></param>
    private void DoEatOrDrinkBehaviour(string playerId, NodeBaseBehaviour baseBev)
    {
        CoroutineManager.Inst.StartCoroutine(DoEatOrDrinkBev(playerId, baseBev));
    }

    private IEnumerator DoEatOrDrinkBev(string playerId, NodeBaseBehaviour baseBev)
    {
        yield return new WaitForSeconds(0.01f);
        var entity = baseBev.entity;
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        var gameObjecet = SceneBuilder.Inst.CloneTargetTemp(baseBev.gameObject);
        if (playerId == GameManager.Inst.ugcUserInfo.uid)
        {
            if (PlayerEatOrDrinkControl.Inst == null)
            {
                PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerEatOrDrinkControl>();
            }
            var curHoldBev = PickabilityManager.Inst.GetBagHandleItemBevByPlayerId(playerId);
            if (curHoldBev  == null)
            {
                EatOrDrinkCtrPanel.Hide();
            }
            isSelfEating = true;
            PlayerEatOrDrinkControl.Inst.IsEating = true;
            PlayerEatOrDrinkControl.Inst.animCon.isEating = true;
            PlayerEatOrDrinkControl.Inst.InitFoodData(baseBev, gameObjecet);
            PlayerEatOrDrinkControl.Inst.SetFoodAction(
                () => { OnStartHaveMeal(uid); },
                () => {
                    OnFinishMeal(uid);
                    isSelfEating = false;
                    if (gameObjecet != null)
                    {
                        GameObject.Destroy(gameObjecet);
                    }
                    var curSwitchBev = PickabilityManager.Inst.GetBagHandleItemBevByPlayerId(playerId);
                    if(curSwitchBev != null && curSwitchBev == baseBev)
                    {
                        PickabilityManager.Inst.HandlePlayerDropProp(playerId);
                        var nextSwitchBev = PickabilityManager.Inst.GetBagHandleItemBevByPlayerId(playerId);
                        if(nextSwitchBev != null)
                        {
                            BaggageManager.Inst.SendMsgToSever(playerId, nextSwitchBev);
                        }
                    }
                    HandleFoodPropActive(entity, false);
                });
            PlayerEatOrDrinkControl.Inst.EatOrDrink();
        }
        else
        {
            var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
            if (otherComp != null)
            {
                var foodCtr = otherComp.GetOtherPlayerEatCtr();
                foodCtr.InitFoodData(baseBev, gameObjecet);
                foodCtr.SetFoodAction(
                    () => { OnStartHaveMeal(uid); },
                    () => {
                        OnFinishMeal(uid);
                        if (gameObjecet != null)
                        {
                            GameObject.Destroy(gameObjecet);
                        }
                        var curSwitchBev = PickabilityManager.Inst.GetBagHandleItemBevByPlayerId(playerId);
                        if (curSwitchBev != null && curSwitchBev == baseBev)
                        {
                            PickabilityManager.Inst.HandlePlayerDropProp(playerId); 
                        }
                        HandleFoodPropActive(entity, false);
                    });
                foodCtr.EatOrDrink();
            }
        }
    }

    /// <summary>
    /// 开始吃东西的行为
    /// </summary>
    /// <param name="uid">道具UID</param>
    private void OnStartHaveMeal(int uid)
    {
        var FoodDict = EdibilityManager.Inst.FoodDict;
        if (FoodDict.ContainsKey(uid))
        {
            var foodData = FoodDict[uid];
            var entity = foodData.entity;
            HandleFoodPropActive(entity, false);
            HandleFoodPropActiveCountdown(entity);
        }
    }

    /// <summary>
    /// 完成吃东西后的行为
    /// </summary>
    /// <param name="uid">道具UID</param>
    private void OnFinishMeal(int uid)
    {
        var FoodDict = EdibilityManager.Inst.FoodDict;
        if (FoodDict.ContainsKey(uid))
        {
            var foodData = FoodDict[uid];
            var entity = foodData.entity;
            RestorePropByUid(foodData);
            if (CatchPanel.Instance)
            {
                CatchPanel.Instance.SetButtonVisible(true);
            }
        }
    }

    /// <summary>
    /// 将道具还原到场景原位置
    /// </summary>
    /// <param name="foodData"></param>
    private void RestorePropByUid(FoodData foodData)
    {
        var entity = foodData.entity;
        var gComp = entity.Get<GameObjectComponent>();
        var foodComp = entity.Get<EdibilityComponent>();
        var bindGo = gComp.bindGo;
        var trans = bindGo.transform;
        trans.SetParent(foodData.parentNode);
        if (foodData.parentNode.name == "moveNode")
        {
            trans.localPosition = Vector3.zero;
        }
        else
        {
            trans.localPosition = foodData.oriPos;
        }
        trans.localEulerAngles = foodData.oriRot;
        trans.localScale = foodData.oriScale;
    }

    /// <summary>
    /// 处理食物道具的显影-需要兼容开关
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="isActive"></param>
    private void HandleFoodPropActive(SceneEntity entity, bool isActive)
    {
        var gComp = entity.Get<GameObjectComponent>();
        var foodComp = entity.Get<EdibilityComponent>();
        var bindGo = gComp.bindGo;
        if (isActive)
        {
            //TODO兼容开关
            if (entity.HasComponent<ShowHideComponent>())
            {
                var shComp = entity.Get<ShowHideComponent>();
                var defaultShow = shComp.defaultShow;
                bindGo.SetActive(defaultShow == 0);
            }
            else
            {
                bindGo.SetActive(isActive);
            }
            foodComp.eatState = EateState.Free;
        }
        else
        {
            bindGo.SetActive(isActive);
        }
    }

    /// <summary>
    /// 处理可食用道具被食用之后的的15s重生逻辑
    /// </summary>
    /// <param name="entity"></param>
    private void HandleFoodPropActiveCountdown(SceneEntity entity)
    {
        if(GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            return;
        }
        var timer = TimerManager.Inst.RunOnce(FoodTimerName, ActiveTimer, () => { HandleFoodPropActive(entity, true); });
        EdibilityManager.Inst.TimerList.Add(timer);
    }

    #region 联机逻辑
    /// <summary>
    /// 向服务器发送消息
    /// </summary>
    public void SendMsgToClient(NodeBaseBehaviour baseBev)
    {
        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            return;
        }
        LoggerUtils.Log("SendOperateMsgToSever111 =>");
        var entity = baseBev.entity;
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;

        FoodSyncData foodSyncData = new FoodSyncData();
        foodSyncData.uid = uid;
        foodSyncData.playerId = GameManager.Inst.ugcUserInfo.uid;
        foodSyncData.opType = EatOpType;

        Item itemData = new Item()
        {
            id = uid,
            type = (int)ItemType.FOOD_PROP,
            data = JsonConvert.SerializeObject(foodSyncData),
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
        LoggerUtils.Log("EdibilitySystemController SendMsgToClient =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }

    /// <summary>
    /// 收到服务器广播消息
    /// </summary>
    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        if (ClientManager.Inst.IsBackground)
        {
            return true;
        }
        LoggerUtils.Log("EdibilitySystemController OnReceiveServer " + msg);
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                if (item.type != (int)ItemType.FOOD_PROP) {
                    continue;
                }
                FoodSyncData data = JsonConvert.DeserializeObject<FoodSyncData>(item.data);
                data.uid = item.id;
                data.playerId = senderPlayerId;
                HandleFoodPropBoardCast(data);
            }
        }
        return true;
    }

    /// <summary>
    /// 获取房间内拾取道具的状态
    /// </summary>
    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("EdibilitySystemController OnGetItemsCallback:" + dataJson);
        if (string.IsNullOrEmpty(dataJson)) return;

        GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
        if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
        {
            LoggerUtils.Log("[EdibilitySystemController.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
            return;
        }

        if (getItemsRsp.mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            HandleGetItems(getItemsRsp.items);
        }
    }

    private void HandleGetItems(Item[] items)
    {
        if (items == null)
        {
            LoggerUtils.Log("[EdibilitySystemController.OnGetItemsCallback]getItemsRsp.items is null");
            return;
        }
        for (int i = 0; i < items.Length; i++)
        {
            Item item = items[i];
            if (string.IsNullOrEmpty(item.data))
            {
                LoggerUtils.Log("[EdibilitySystemController.OnGetItemsCallback]getItemsRsp.item.Data is null");
                continue;
            }
            FoodSyncData data = JsonConvert.DeserializeObject<FoodSyncData>(item.data);
            data.uid = item.id;
            HandleGetItemsData(data);
        }
    }

    private void HandleFoodPropBoardCast(FoodSyncData data)
    {
        var opType = data.opType;
        switch (opType)
        {
            case EatOpType:
                HandleHaveMeal(data);
                break;
            case RestoreOpType:
                HandleFoodRestore(data);
                break;
        }
    }

    private void HandleGetItemsData(FoodSyncData data)
    {
        var opType = data.opType;
        switch (opType)
        {
            case EatOpType:
                var FoodDict = EdibilityManager.Inst.FoodDict;
                if (FoodDict.ContainsKey(data.uid))
                {
                    var foodData = FoodDict[data.uid];
                    var entity = foodData.entity;
                    HandlePickFoodState(data.uid, entity);
                    HandleFoodPropActive(entity, false);
                }
                break;
            case RestoreOpType:
                HandleFoodRestore(data);
                break;
        }
    }

    private void HandleHaveMeal(FoodSyncData data)
    {
        var uid = data.uid;
        var playerId = data.playerId;
        var baseBev = EdibilityManager.Inst.GetNodeBaseBevByUid(uid);
        if (baseBev != null)
        {
            DoEatOrDrinkBehaviour(playerId, baseBev);
        }
    }

    private void HandleFoodRestore(FoodSyncData data)
    {
        var FoodDict = EdibilityManager.Inst.FoodDict;
        if (FoodDict.ContainsKey(data.uid))
        {
            var foodData = FoodDict[data.uid];
            var entity = foodData.entity;
            RestorePropByUid(foodData);
            HandleFoodPropActive(entity, true);
        }
    }
    #endregion

    public void CancelLock()
    {
        if(GlobalFieldController.CurGameMode != GameMode.Edit)
        {
            if (CatchPanel.Instance)
            {
                CatchPanel.Instance.SetButtonVisible(true);
            }
        }
        isSelfEating = false;
        if (PlayerEatOrDrinkControl.Inst)
        {
            PlayerEatOrDrinkControl.Inst.IsEating = false;
            PlayerEatOrDrinkControl.Inst.animCon.isEating = false;
        }
    }

    private void HandlePickFoodState(int uid, SceneEntity curFoodEntity)
    {
        var playerId = PickabilityManager.Inst.GetPlayerIdByPropUid(uid);
        if(playerId != null)
        {
            var curSwitchBev = PickabilityManager.Inst.GetBagHandleItemBevByPlayerId(playerId);
            if (curSwitchBev.entity == curFoodEntity)
            {
                PickabilityManager.Inst.HandlePlayerDropProp(playerId);
                PickabilityManager.Inst.ChangeAnimClips(playerId, PICK_STATE.DROP);
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

// 钓鱼操作枚举
public enum FishingOption
{
    Start = 1, // 开始钓鱼
    PullRod,   // 收竿
    Stop,      // 结束钓鱼
}

// 钓鱼状态枚举
public enum FishingState
{
    None,
    Fishing,        // 钓鱼中
    FishingSuccess, // 钓鱼成功
    FishingFailed,  // 钓鱼失败

    // NOTE: 以下客户端专用
    ShowFish,       // 展示鱼
    ShowEmpty,      // 无鱼
    SendStart,      // 已发送开始钓鱼指令
    SendPullRod,    // 已发送收竿指令
    SendStop,       // 已发送结束钓鱼指令
}

// 服务器返回的钓鱼错误码
public class FishingCode
{
    public const int FISHING_SUCCESS              = 0;     // 钓鱼成功，钓到了鱼并且放入背包
    public const int FISHING_FAILED_BAG_FULL      = 10301; // 钓鱼失败，背包已经满了
    public const int FISHING_FAILED_NO_BAG        = 10305; // 钓鱼失败，该地图没有开启背包
    public const int FISHING_FAILED_NO_FISH       = 10306; // 钓鱼失败，没有钓到鱼
    public const int FISHING_FAILED_CONFLICT_FISH = 20005; // 钓鱼失败，没有钓到鱼(两人同时钓一条鱼会发生)
}

// 进房时服务器发的钓鱼数据
public class FishingActivityInfo
{
    public int state;       // 钓鱼的状态
    public Item item;       // 钓到的渔获
    public string position; // 浮漂掉落的位置
    public int code;        // 服务器返回的错误码
}

public class FishingManager : ManagerInstance<FishingManager>, IManager, IPVPManager
{
    private const float FISHING_HOOK_RADIUS = 0.35f; // 鱼钩上鱼的半径范围
    private string _myPlayerId { get { return GameManager.Inst.ugcUserInfo.uid; } }
    private Dictionary<int, NodeBaseBehaviour> _allFishDic = new Dictionary<int, NodeBaseBehaviour>();
    private Dictionary<int, Transform> _allFishParentDic = new Dictionary<int, Transform>();
    private List<string> _fishingPlayerList = new List<string>();

    #region 服务器消息通知
    // 接收服务器消息
    public bool OnReceiveServer(string playerId, string content)
    {
        LoggerUtils.Log(string.Format("Fishing OnReceiveServer. playerId = {0}, content={1}", playerId, content));
        var fishingData = JsonConvert.DeserializeObject<FishingData>(content);
        if (fishingData != null)
        {
            var status = (FishingOption)fishingData.option;
            switch (status)
            {
                case FishingOption.Start:
                    var offset = JsonConvert.DeserializeObject<Vec3>(fishingData.position);
                    HandleStartFishing(playerId, offset);
                    break;
                case FishingOption.PullRod:
                    HandlePullRod(playerId, fishingData.item, fishingData.code);
                    break;
                case FishingOption.Stop:
                    HandleStopFishing(playerId, fishingData.item, fishingData.code);
                    break;
            }
        }

        return true;
    }

    // 进房时同步房间内所有玩家的钓鱼状态
    public void OnGetItemsCallback(string content)
    {
        LoggerUtils.Log(string.Format("Fishing OnGetItemsCallback content = {0}", content));
        if (string.IsNullOrEmpty(content))
            return;

        if (GlobalFieldController.CurMapInfo == null)
            return;

        GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(content);
        if (getItemsRsp == null)
            return;

        if (getItemsRsp.mapId != GlobalFieldController.CurMapInfo.mapId)
            return;

        var playerCustomDatas = getItemsRsp.playerCustomDatas;
        if (playerCustomDatas == null)
            return;

        foreach (var playerCustomData in playerCustomDatas)
        {
            var playerId = playerCustomData.playerId;
            var activitiesDatas = playerCustomData.activitiesData;
            if (activitiesDatas == null)
                continue;

            foreach (var activitiesData in activitiesDatas)
            {
                if (activitiesData.activityId == ActivityID.Fishing)
                {
                    var fishingActivityInfo = JsonConvert.DeserializeObject<FishingActivityInfo>(activitiesData.data);
                    HandleReconnect(playerId, fishingActivityInfo);
                    break;
                }
            }
        }
    }

    // 重置
    public void OnReset()
    {
        LoggerUtils.Log("Fishing OnRest");
        foreach(var playerId in _fishingPlayerList)
        {
            RemovePlayerFishingController(playerId);
        }
        StopAllPlayerFishingSound();
        _fishingPlayerList.Clear();
    }
    #endregion

    // 为道具添加可捕捉属性
    public void AddCatchability(SceneEntity entity)
    {
        var gComp = entity.Get<GameObjectComponent>();
        var bindGo = gComp.bindGo;
        var baseBev = bindGo.GetComponent<NodeBaseBehaviour>();
        var uid = gComp.uid;
        entity.Get<CatchabilityComponent>();
        if (!_allFishDic.ContainsKey(uid))
        {
            _allFishDic.Add(uid, baseBev);
        }
    }

    // 移除道具的可捕捉属性
    public void RemoveCatchability(SceneEntity entity)
    {
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        if (entity.HasComponent<CatchabilityComponent>())
        {
            entity.Remove<CatchabilityComponent>();
        }
        if (_allFishDic.ContainsKey(uid))
        {
            _allFishDic.Remove(uid);
        }
    }

    public NodeBaseBehaviour[] GetAllFish()
    {
        return _allFishDic.Values.ToArray();
    }

    // 获取可捕捉道具
    public NodeBaseBehaviour GetFish(int uid)
    {
        NodeBaseBehaviour baseBev;
        _allFishDic.TryGetValue(uid, out baseBev);
        return baseBev;
    }

    //获取可捕捉道具的父节点
    public Transform GetFishParent(int uid)
    {
        Transform parent;
        _allFishParentDic.TryGetValue(uid, out parent);
        return parent;
    }

    // 处理拾取道具
    public void HandlePickup(NodeBaseBehaviour baseBev, bool isPick, string playerId)
    {
        //捡起的物体是否是钓鱼竿
        var fishingBehav = baseBev.transform.GetComponent<FishingBehaviour>();
        if (fishingBehav == null)
            return;

        if (isPick)
        {
            // 往玩家身上添加钓鱼组件
            var controller = AddPlayerFishingController(playerId);
            if (controller)
                controller.SetFishingRod(fishingBehav);
        }
        else
        {
            FishingEditManager.Inst.ResetFishingHookLocalPos(baseBev);
            // 移除玩家身上的钓鱼组件
            RemovePlayerFishingController(playerId);
        }

        if (isPick)
        {
            if (!_fishingPlayerList.Contains(playerId))
            {
                _fishingPlayerList.Add(playerId);
            }
            if (playerId == GameManager.Inst.ugcUserInfo.uid)
            {
                if (PlayModePanel.Instance)
                {
                    PlayModePanel.Instance.isTps = false;
                    PlayModePanel.Instance.OnChangeViewBtnClick();
                }
                PlayerControlManager.Inst.ChangeAnimClips();
                FishingCtrPanel.Show();
            }
            else
            {
                var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
                if (otherComp != null)
                {
                    otherComp.SwitchFishingAnimClips();
                }
            }
        }
        else
        {
            if (_fishingPlayerList.Contains(playerId))
            {
                _fishingPlayerList.Remove(playerId);
            }
            if (playerId == GameManager.Inst.ugcUserInfo.uid)
            {
                PlayerControlManager.Inst.ChangeAnimClips();
                FishingCtrPanel.Hide();
            }
            else
            {
                var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
                if (otherComp != null)
                {
                    otherComp.SwitchNormalAnimClips();
                }
            }
        }
    }

    /// <summary>
    /// 根据PlayerId判断当前玩家是否正在拿着鱼竿
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public bool IsPlayerHoldingFishingRod(string playerId)
    {
        var curHoldBev = PickabilityManager.Inst.GetBagHandleItemBevByPlayerId(playerId);
        if (curHoldBev != null)
        {
            var gComp = curHoldBev.entity.Get<GameObjectComponent>();
            var modelType = gComp.modelType;
            if(modelType == NodeModelType.FishingModel)
            {
                return true;
            }
        }
        return false;
    }

    // 开始钓鱼
    public void StartFishing()
    {
        var controller = GetPlayerFishingController(_myPlayerId);
        if (controller)
        {
            if (controller.State != FishingState.None)
                return;

            controller.State = FishingState.SendStart;

            var pos = controller.PreCalcHookDropOffset();
            if (GlobalFieldController.CurGameMode == GameMode.Guest)
                SendRequest(FishingOption.Start, pos);
            else if (GlobalFieldController.CurGameMode == GameMode.Play)
                HandleStartFishing(_myPlayerId, pos);
        }
    }

    // 拉竿
    public void PullFishingRod()
    {
        var controller = GetPlayerFishingController(_myPlayerId);
        if (controller)
        {
            if (controller.State != FishingState.Fishing)
                return;

            controller.State = FishingState.SendPullRod;

            Item item = null;
            var baseBev = TryGetFish(_myPlayerId);
            if (baseBev != null)
            {
                var entity = baseBev.entity;
                var gComp = entity.Get<GameObjectComponent>();

                PickPropSyncData pickPropSyncData = new PickPropSyncData();
                pickPropSyncData.uid = gComp.uid;
                pickPropSyncData.playerId = _myPlayerId;
                pickPropSyncData.propType = (int)PROP_TYPE.FISHING;
                pickPropSyncData.status = (int)PICK_STATE.CATCH;

                item = new Item();
                item.id = entity.Get<GameObjectComponent>().uid;
                item.type = (int)ItemType.FISHING_PROP;
                item.data = JsonConvert.SerializeObject(pickPropSyncData);
            }

            if (GlobalFieldController.CurGameMode == GameMode.Guest)
            {
                SendRequest(FishingOption.PullRod, controller.HookDropOffset, item);
            }
            else if (GlobalFieldController.CurGameMode == GameMode.Play)
            {
                if (item == null)
                {
                    HandlePullRod(_myPlayerId, item, FishingCode.FISHING_FAILED_NO_FISH);
                }
                else
                {
                    if (SceneParser.Inst.GetBaggageSet() != 1)
                    {
                        HandlePullRod(_myPlayerId, item, FishingCode.FISHING_FAILED_NO_BAG);
                    }
                    else
                    {
                        var isBagFull = BaggageManager.Inst.IsSelfBaggageFull();
                        var code = isBagFull ? FishingCode.FISHING_FAILED_BAG_FULL : FishingCode.FISHING_SUCCESS;
                        HandlePullRod(_myPlayerId, item, code);
                    }
                }
            }
        }
    }

    // 结束钓鱼
    public void StopFishing()
    {
        var controller = GetPlayerFishingController(_myPlayerId);
        if (controller)
        {
            if (controller.State != FishingState.ShowFish && controller.State != FishingState.ShowEmpty)
                return;

            controller.State = FishingState.SendStop;

            if (GlobalFieldController.CurGameMode == GameMode.Guest)
                SendRequest(FishingOption.Stop, controller.HookDropOffset, controller.Item, controller.Code);
            else if (GlobalFieldController.CurGameMode == GameMode.Play)
                HandleStopFishing(_myPlayerId, controller.Item, controller.Code);
        }
    }

    // 强制结束钓鱼
    public void ForceStopFishing()
    {
        var controller = GetPlayerFishingController(_myPlayerId);
        if (controller)
        {
            controller.State = FishingState.SendStop;

            if (GlobalFieldController.CurGameMode == GameMode.Guest)
                SendRequest(FishingOption.Stop, controller.HookDropOffset, controller.Item, controller.Code);
            else if (GlobalFieldController.CurGameMode == GameMode.Play)
                HandleStopFishing(_myPlayerId, controller.Item, controller.Code);

            controller.StopFishingAudio();
        }
    }

    // 开始钓鱼
    private void HandleStartFishing(string playerId, Vector3 offset)
    {
        var controller = GetPlayerFishingController(playerId);
        if (controller)
            controller.StartFishing(offset);
    }

    // 收竿
    private void HandlePullRod(string playerId, Item item, int code)
    {
        var controller = GetPlayerFishingController(playerId);
        if (controller)
            controller.PullFishingRod(code, item);
    }

    // 结束钓鱼
    private void HandleStopFishing(string playerId, Item item, int code)
    {
        var controller = GetPlayerFishingController(playerId);
        if (controller)
            controller.StopFishing(code, item);
    }

    // 获取鱼钩范围内的鱼
    private NodeBaseBehaviour TryGetFish(string playerId)
    {
        var controller = GetPlayerFishingController(playerId);
        if (controller)
        {
            var fishParentPos = controller.FishParentPos;
            var curHits = Physics.OverlapSphere(fishParentPos, FISHING_HOOK_RADIUS);
            foreach (var hit in curHits)
            {
                var nodeList = hit.transform.GetComponentsInParent<NodeBaseBehaviour>();
                if(nodeList != null)
                {
                    foreach(var node in nodeList)
                    {
                        var entity = node.entity;
                        var gComp = entity.Get<GameObjectComponent>();
                        var uid = gComp.uid;
                        if (entity.HasComponent<CatchabilityComponent>() && _allFishParentDic.ContainsKey(uid)) {
                            return node;
                        }
                    }
                }
            }
        }

        return null;
    }

    // 断线重连
    private void HandleReconnect(string playerId, FishingActivityInfo fishingActivityInfo)
    {
        var controller = GetPlayerFishingController(playerId);
        if (controller)
        {
            if (fishingActivityInfo.state == (int)FishingState.Fishing)
                controller.StartFishingImmediate(JsonConvert.DeserializeObject<Vec3>(fishingActivityInfo.position));
            else if (fishingActivityInfo.state == (int)FishingState.FishingSuccess || fishingActivityInfo.state == (int)FishingState.FishingFailed)
                controller.PullFishingRodImmediate(fishingActivityInfo.code, fishingActivityInfo.item);
        }
    }

    // 向服务器发送请求
    private void SendRequest(FishingOption option, Vec3 pos, Item item = null, int code = 0)
    {
        LoggerUtils.Log(string.Format("Fishing SendRequest. option = {0}", (int)option));

        // 封装数据
        var data = new FishingData() { mapId = GlobalFieldController.CurMapInfo.mapId, option = (int)option, position = JsonConvert.SerializeObject(pos), item = item, code = code };
        RoomChatData rcData = new RoomChatData() { msgType = (int)RecChatType.Fishing, data = JsonConvert.SerializeObject(data) };

        // 发送请求到服务器
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(rcData));
    }

    private PlayerFishingController GetPlayerFishingController(string playerId)
    {
        var playerNode = ClientManager.Inst.GetPlayerNode(playerId);
        if (playerNode == null)
            return null;

        return playerNode.GetComponent<PlayerFishingController>();
    }

    private PlayerFishingController AddPlayerFishingController(string playerId)
    {
        var playerNode = ClientManager.Inst.GetPlayerNode(playerId);
        if (playerNode == null)
            return null;

        var playerFishingCtrl = playerNode.GetComponent<PlayerFishingController>();
        if (playerFishingCtrl == null)
        {
            playerFishingCtrl = playerNode.AddComponent<PlayerFishingController>();
            playerFishingCtrl.Init(playerId);
        }

        return playerFishingCtrl;
    }

    private void RemovePlayerFishingController(string playerId)
    {
        var playerNode = ClientManager.Inst.GetPlayerNode(playerId);
        if (playerNode == null)
            return;

        var playerFishingCtrl = playerNode.GetComponent<PlayerFishingController>();
        if (playerFishingCtrl != null) {
            GameObject.DestroyImmediate(playerFishingCtrl);
        }
    }

    #region 可捕捉
    public void GetParentData()
    {
        foreach(var fish in _allFishDic)
        {
            _allFishParentDic[fish.Key] = fish.Value.transform.parent;
        }
        _fishingPlayerList.Clear();
    }

    public void OnChangeMode(GameMode mode) {
        switch (mode)
        {
            case GameMode.Edit:
                ResetFishParent();
                StopAllPlayerFishingSound();
                OnReset();
                break;
            case GameMode.Play:
            case GameMode.Guest:
                GetParentData();
                break;
        }
    }


    // 判断道具是否可以设置可捕捉属性
    public bool CheckCanSetCatchability(SceneEntity entity)
    {
        //判断是否包含特殊属性
        if (entity.HasComponent<ShootWeaponComponent>()
            || entity.HasComponent<AttackWeaponComponent>()
            || entity.HasComponent<FireworkComponent>()
            || entity.HasComponent<FreezePropsComponent>()
            || entity.HasComponent<ParachuteComponent>()
            || entity.HasComponent<FishingRodComponent>()
            || entity.HasComponent<BloodPropComponent>())
        {
            return false;
        }
        //判断是否包含特殊道具
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        var nodeBehvs = bindGo.GetComponentsInChildren<NodeBaseBehaviour>();
        for (var i = 0; i < nodeBehvs.Length; i++)
        {
            NodeModelType modeType = nodeBehvs[i].entity.Get<GameObjectComponent>().modelType;
            ResType resType = nodeBehvs[i].entity.Get<GameObjectComponent>().type;
            if (modeType != NodeModelType.BaseModel
                && modeType != NodeModelType.DText
                && modeType != NodeModelType.NewDText
                && resType != ResType.UGC
                && resType != ResType.PGC
                && resType != ResType.CommonCombine
                && modeType != NodeModelType.PGCPlant
                && modeType != NodeModelType.PointLight
                && modeType != NodeModelType.SpotLight
                )
            {
                return false;
            }
        }
        return true;
    }

    public bool HasCatchability(SceneEntity entity)
    {
        return entity.HasComponent<CatchabilityComponent>();
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        var entity = behaviour.entity;
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        if (_allFishDic.ContainsKey(uid))
        {
            _allFishDic.Remove(uid);
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var entity = behaviour.entity;
        if (entity.HasComponent<CatchabilityComponent>())
        {
            AddCatchability(entity);
        }
    }

    public void Clear()
    {
    }

    public void OnHandleClone(SceneEntity oEntity, SceneEntity nEntity)
    {
        if (oEntity.HasComponent<CatchabilityComponent>())
        {
            if (CheckCurCountCanSetCatchability())
            {
                AddCatchability(nEntity);
            }
            else
            {
                nEntity.Remove<PickablityComponent>();
            }
        }
    }

    public bool CheckCurCountCanSetCatchability() {
        return PickabilityManager.Inst.CheckCanSetPickability();
    }

    public void OnCombineNode(SceneEntity entity)
    {
        if (entity == null) return;
        RemoveCatchability(entity);
    }

    public bool GetPlayerFishingStateByPlayerId(string playerId)
    {
        var fishingCtr = GetPlayerFishingController(playerId);
        if(fishingCtr != null)
        {
            if(fishingCtr.State != FishingState.None)
            {
                return true;
            }
        }
        return false;
    }

    public void StopAllPlayerFishingSound()
    {
        foreach (var playerId in _fishingPlayerList)
        {
            var fishingCtr = GetPlayerFishingController(playerId);
            if(fishingCtr != null)
            {
                fishingCtr.StopFishingAudio();
            }
        }
    }

    private void ResetFishParent()
    {
        foreach(var fish in _allFishDic.Values)
        {
            var gComp = fish.entity;
            var uid = gComp.Get<GameObjectComponent>().uid;
            if (_allFishParentDic.ContainsKey(uid)) {
                fish.transform.SetParent(_allFishParentDic[uid]);
            }
        }
    }

    #endregion
}

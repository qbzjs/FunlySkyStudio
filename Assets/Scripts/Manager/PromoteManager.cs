using System;
using System.Collections.Generic;
using System.IO;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

[Serializable]
// 带货的商品数据
public class PromoteItemInfo
{
    public string mapId;   // 商品ID
    public string mapCover = "";
    public string mapName = "";
    public int dataType = 0;//0:map 1:prop 2:clot 3:space
    public string clothesUrl = "";
    public string propsJson = "";
    public int isDC;
    public DCPGCItemInfo dcPgcInfo;
    public DCPromoteInfo dcInfo;
    public OfflineRenderListObj[] renderList;
    public int templateId = 1;
    public int dataSubType;//区分UGC衣服、彩绘
    public Action<SceneEntity> onGet;
    public bool IsScenePgc()
    {
        return dcPgcInfo != null && dcPgcInfo.pgcId > 10000 && dcPgcInfo.classifyType == (int)BundlePart.Respgc;
    }

    public bool IsClothPgc()
    {
        return dcPgcInfo != null && dcPgcInfo.pgcId > 100000 && dcPgcInfo.classifyType != (int)BundlePart.Respgc;
    }

}

// 带货状态枚举
public enum PromoteStatus
{
    None = 0,
    Select,        // 选择商品
    Begin,         // 开始摆摊
    End,           // 收摊
    Peddle,        // 吆喝
    Introduce,     // 介绍商品
    CheckCustomer, // 检查带货区域内的顾客
    Purchased = 8  // get货物弹气泡框
}

public enum PromoteActivityState
{
    Select = 1,
    Begin
}

// GetItems时服务器发的带货数据
public class PromoteActivityInfo
{
    public int state;
    public List<PromoteItemInfo> goods;
}

public class PromoteManager : CInstance<PromoteManager>, IPVPManager, IUGCManager
{
    private Dictionary<string, List<PromoteItemInfo>> _itemInfoDic = new Dictionary<string, List<PromoteItemInfo>>();
    private List<NodeBaseBehaviour> promotePropList = new List<NodeBaseBehaviour>();

    private bool _getItemsLock = false;
    private bool _pvpRestLock = false;

    // 构造
    public PromoteManager()
    {
        // 添加监听
        MessageHelper.AddListener<string>(MessageName.PlayerLeave, HandlePlayerLeaveRoom);

#if !UNITY_EDITOR
        MobileInterface.Instance.AddClientRespose(MobileInterface.openPromotePage, OpenPromotePageCallback);
#endif
    }

    // 析构
    public override void Release()
    {
        base.Release();

        // 移除监听
        MessageHelper.RemoveListener<string>(MessageName.PlayerLeave, HandlePlayerLeaveRoom);
    }

    // 重置
    public void OnReset()
    {
#if UNITY_ANDROID
        if (_pvpRestLock == true && _getItemsLock == true)
        {
            _pvpRestLock = false;
            return;
        }
#endif
        LoggerUtils.Log("Promote OnRest");
        ResetAll();
    }

    // 接收服务器消息
    public bool OnReceiveServer(string playerId, string content)
    {
        LoggerUtils.Log(string.Format("Promote OnReceiveServer. playerId = {0}, content={1}", playerId, content));
        HandleServerMsg(playerId, content);
        return true;
    }

    // 进入地图时发生
    public void OnGetItemsCallback(string content)
    {
#if UNITY_ANDROID
        if (_getItemsLock == true)
        {
            _getItemsLock = false;
            return;
        }
#endif
        LoggerUtils.Log(string.Format("Promote OnGetItemsCallback content = {0}", content));
        HandleEnterRoom(content);
    }

    // 玩家死亡
    public void OnPlayerDeath(string playerId)
    {
        LoggerUtils.Log(string.Format("Promote OnPlayerDeath playerId = {0}", playerId));

        _itemInfoDic.Remove(playerId);
        var promoteCtr = ClientManager.Inst.GetPlayerPromoteController(playerId);
        if (promoteCtr)
        {
            promoteCtr.ResetToDefault();
        }
    }

    // 选择带货商品
    public void Select()
    {
        //判断是否可以进入带货模式
        if (!StateManager.Inst.CheckCanEnterPromoteMode())
            return;

        // 调用原生接口，打开端上选择带货商品页面
#if UNITY_ANDROID
        _getItemsLock = true;
        _pvpRestLock = true;
#endif

#if !UNITY_EDITOR
        MobileInterface.Instance.OpenPromotePage("");
#endif
    }

    // 收摊
    public void End()
    {
        SendRequest(PromoteStatus.End);
    }

    // 判断是否还有某个带货商品数据
    public bool HasItemInfo(string playerId, PromoteItemInfo itemInfo)
    {
        List<PromoteItemInfo> lst;
        if (_itemInfoDic.TryGetValue(playerId, out lst))
        {
            if (lst.Contains(itemInfo))
                return true;
        }

        return false;
    }

    public bool GetPlayerPromoteState(string playerId)
    {
        if (!(GlobalFieldController.CurGameMode == GameMode.Guest))
        {
            return false;
        }

        var promoteCtr = ClientManager.Inst.GetPlayerPromoteController(playerId);
        if (promoteCtr != null)
        {
            return promoteCtr.InPromote;
        }

        return false;
    }

    // 处理我自己进房
    private void HandleEnterRoom(string content)
    {
        ResetAll();

        if (string.IsNullOrEmpty(content))
            return;

        if (GlobalFieldController.CurMapInfo == null)
            return;

        GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(content);
        if (getItemsRsp == null)
            return;

        if (getItemsRsp.mapId != GlobalFieldController.CurMapInfo.mapId)
            return;

        // 层层遍历，只为寻找出带货数据...
        var playerCustomDatas = getItemsRsp.playerCustomDatas;
        if (playerCustomDatas != null)
        {
            foreach (var playerCustomData in playerCustomDatas)
            {
                var playerId = playerCustomData.playerId;
                var activitiesDatas = playerCustomData.activitiesData;
                if (activitiesDatas != null)
                {
                    foreach (var activitiesData in activitiesDatas)
                    {
                        if (activitiesData.activityId == ActivityID.Promote)
                        {
                            // 反序列化带货商品数据
                            var promoteActivityInfo = JsonConvert.DeserializeObject<PromoteActivityInfo>(activitiesData.data);
                            var state = (PromoteActivityState)promoteActivityInfo.state;
                            if (state == PromoteActivityState.Select)
                            {
                                // 如果我自己在选货状态中，且本地已经选完了，向联机服务器补发退出带货的消息
                                if (playerId == GameManager.Inst.ugcUserInfo.uid)
                                    End();
                                else
                                    HandleSelect(playerId);
                            }
                            else
                                HandleBegin(playerId, promoteActivityInfo.goods, true);

                            // 已经找到该玩家的带货数据，跳出当前循环
                            break;
                        }
                    }
                }
            }
        }
    }

    // 处理别的玩家退房
    private void HandlePlayerLeaveRoom(string playerId)
    {
        _itemInfoDic.Remove(playerId);
    }

    // 端上选择带货商品回调
    public void OpenPromotePageCallback(string content)
    {
#if UNITY_EDITOR
        content = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "111.json"));
#endif
        LoggerUtils.Log(string.Format("OpenPromotePageCallback content = {0}", content));

        
        // 反序列化数据
        var promoteItems = JsonConvert.DeserializeObject<List<PromoteItemInfo>>(content);

        // 没有带货数据，说明没有选货直接返回了
        if (promoteItems == null || promoteItems.Count == 0)
        {
            string playerId = GameManager.Inst.ugcUserInfo.uid;
            var promoteCtrl = ClientManager.Inst.GetPlayerPromoteController(playerId);

            // 选择商品中，退出带货装备
            if (promoteCtrl && promoteCtrl.InSelect)
                End();
        }
        else
        {
            // 发送给联机服务器
            SendRequest(PromoteStatus.Begin, JsonConvert.SerializeObject(promoteItems));
        }
    }

    // 向服务器发送请求
    public void SendRequest(PromoteStatus status, string extraData = "")
    {
        LoggerUtils.Log(string.Format("Promote SendRequest. status = {0}, extraData = {1}", (int)status, extraData));

        // 封装数据
        var data = new PromoteData() { mapId = GlobalFieldController.CurMapInfo.mapId, status = (int)status, extraData = extraData };
        RoomChatData rcData = new RoomChatData() { msgType = (int)RecChatType.Promote, data = JsonConvert.SerializeObject(data) };

        // 发送请求到服务器
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(rcData));
    }

    // 处理服务器消息
    private bool HandleServerMsg(string playerId, string content)
    {
        // 反序列化数据
        var data = JsonConvert.DeserializeObject<PromoteData>(content);
        HandlePromoteAction(playerId, data);
        return true;
    }

    // 处理带货动作
    private void HandlePromoteAction(string playerId, PromoteData data)
    {
        if (data == null)
            return;

        if(playerId == GameManager.Inst.ugcUserInfo.uid && StateManager.IsSnowCubeSkating)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
        var status = (PromoteStatus)data.status;
        switch (status)
        {
            case PromoteStatus.Select:
                HandleSelect(playerId);
                break;
            case PromoteStatus.Begin:
                var itemInfos = JsonConvert.DeserializeObject<List<PromoteItemInfo>>(data.extraData);
                HandleBegin(playerId, itemInfos);
                break;
            case PromoteStatus.End:
                HandleEnd(playerId);
                break;
            case PromoteStatus.Peddle:
                HandlePeddle(playerId);
                break;
            case PromoteStatus.Introduce:
                HandleIntroduce(playerId);
                break;
            // 此条消息是服务器只给我自己发的
            case PromoteStatus.CheckCustomer:
                bool hasCustomer = ClientManager.Inst.GetPlayerPromoteController(playerId).CheckCustomer();
                SendRequest(hasCustomer ? PromoteStatus.Introduce : PromoteStatus.Peddle);
                break;
            case PromoteStatus.Purchased:
                if (playerId == Player.Id)
                {
                    var textChatBev = PlayerEmojiControl.Inst.textCharBev;
                    if(textChatBev != null)textChatBev.SetPurchaseText(data.extraData);
                }
                else
                {
                    var otherCtr = ClientManager.Inst.GetOtherPlayerComById(playerId);
                    otherCtr.OnPurchasedChat(data.extraData);
                }
                break;
        }
    }

    // 选择商品
    private void HandleSelect(string playerId)
    {
        var promoteCtr = ClientManager.Inst.GetPlayerPromoteController(playerId);
        if (promoteCtr != null)
        {
            promoteCtr.Select();

            if (! _itemInfoDic.ContainsKey(playerId))
                _itemInfoDic.Add(playerId, new List<PromoteItemInfo>());
        }
    }

    // 开始摆摊
    public void HandleBegin(string playerId, List<PromoteItemInfo> itemInfos, bool isReconnect = false)
    {
        // 如果没有商品数据，返回
        if (itemInfos == null || itemInfos.Count == 0)
        {
            LoggerUtils.Log(string.Format("Promote begin, item is empty. reconnect = {0}", isReconnect));
            return;
        }

        
    
        // 移除旧的商品数据
        
        _itemInfoDic.Remove(playerId);

        // 记录数据
        _itemInfoDic.Add(playerId, itemInfos);

        // 显示带货商品
        var promoteCtr = ClientManager.Inst.GetPlayerPromoteController(playerId);
        if (promoteCtr != null)
        {
            // 断线重连或者正在带货中（不要动画）
            if (promoteCtr.InPromote || isReconnect)
                promoteCtr.BeginPromoteImmediate(itemInfos);
            // 正常流程（要动画）
            else
                promoteCtr.BeginPromote(itemInfos);
        }

        CommonSetRecChat(playerId);
    }

    // 收摊
    private void HandleEnd(string playerId)
    {
        RemoveItemInfo(playerId);
        _itemInfoDic.Remove(playerId);

        var promoteCtrl = ClientManager.Inst.GetPlayerPromoteController(playerId);
        if (promoteCtrl.InPromote)
            promoteCtrl.ExitPromote();
        else
            promoteCtrl.ExitSelect();
    }

    private void RemoveItemInfo(string playerId)
    {
        if (_itemInfoDic.TryGetValue(playerId, out var item))
        {
            foreach (var itemInfo in item)
            {
                StorePanel.onGet -= itemInfo.onGet;
            }
        }
    }

    // 介绍
    private void HandleIntroduce(string playerId)
    {
        var promoteCtr = ClientManager.Inst.GetPlayerPromoteController(playerId);
        if (promoteCtr)
        {
            promoteCtr.Introduce();
        }
    }

    // 吆喝
    private void HandlePeddle(string playerId)
    {
        var promoteCtr = ClientManager.Inst.GetPlayerPromoteController(playerId);
        if (promoteCtr)
        {
            promoteCtr.Peddle();
        }
    }

    // 重置所有玩家的带货商品及数据
    private void ResetAll()
    {
        foreach (var playerId in _itemInfoDic.Keys)
        {
            var promoteCtr = ClientManager.Inst.GetPlayerPromoteController(playerId);
            if (promoteCtr != null)
            {
                promoteCtr.ResetToDefault();
            }
        }

        _itemInfoDic.Clear();
    }

    // 收到带货消息-并且显示到左边信息栏
    private void CommonSetRecChat(string playerId)
    {
        UserInfo syncPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(playerId);
        if(syncPlayerInfo != null && RoomChatPanel.Instance)
        {
            var userName = syncPlayerInfo.userName;
            RoomChatPanel.Instance.SetRecChat(RecChatType.Promote, userName);
        }
    }

    public void AddPromoteProp(NodeBaseBehaviour baseBev)
    {
        if (!promotePropList.Contains(baseBev))
        {
            promotePropList.Add(baseBev);
        }
    }

    public void RemovePromoteProp(NodeBaseBehaviour baseBev)
    {
        if (promotePropList.Contains(baseBev))
        {
            promotePropList.Remove(baseBev);
        }
    }

    public void OnUGCChangeStatus(UGCCombBehaviour ugcCombBehaviour)
    {
        if (promotePropList.Contains(ugcCombBehaviour)){
            var _targetSize = new Vector3(0.75f, 0.75f, 0.75f);
            PropSizeUtill.SetPropSize(_targetSize, ugcCombBehaviour);
            SetCollidersEnable(ugcCombBehaviour);
        }
    }

    private void SetCollidersEnable(NodeBaseBehaviour baseBev)
    {
        var colliders = baseBev.transform.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        // 设置层级为Touch层，进行射线检测
        var boxCollider = baseBev.transform.GetComponentInChildren<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.gameObject.layer = LayerMask.NameToLayer("Touch");
            boxCollider.enabled = true;
        }
    }

    private List<PromoteItemInfo> RemovePGC(List<PromoteItemInfo> list)
    {
        if (list != null && list.Count > 0)
        {
            list.RemoveAll(i => string.IsNullOrEmpty(i.clothesUrl) && string.IsNullOrEmpty(i.propsJson));
        }
        return list;
    }
}
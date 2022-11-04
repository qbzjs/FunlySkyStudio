using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SavingData;
using Newtonsoft.Json;
using System;
using UnityEngine.Networking;
using UnityEngine.U2D;
/// <summary>
/// Author:LiShuZhan
/// Description:背包manager，负责处理数据记录和加载
/// </summary>
public class BaggageManager : ManagerInstance<BaggageManager>, IPVPManager, INetMessageHandler,IManager
{
    /// <summary>
    /// 记录背包内的物体
    /// </summary>
    public Dictionary<string, List<int>> playerBaggageDic = new Dictionary<string, List<int>>();
    /// <summary>
    /// 素材图片缓存 最大存10张
    /// </summary>
    private Dictionary<int, Texture> itemIconDIc = new Dictionary<int, Texture>();
    //最大背包容量
    private int maxItemNum = 3;
    //自身id
    public string selfUid;

    public int handleItemId = (int)BaggageItemType.none;

    public void OnChangeMode(GameMode mode)
    {
        if (SceneParser.Inst.GetBaggageSet() == 1&&mode == GameMode.Edit)
        {
            if (BaggagePanel.Instance)
            {
                BaggagePanel.Instance.ResetBaggage();
            }
            handleItemId = (int)BaggageItemType.none;
            ResetBaggageItem(true);
        }
    }

    public void InitBaggageVisiable()
    {
        if (SceneParser.Inst.GetBaggageSet() == 0)
        {
            BaggagePanel.Hide();
        }
        else
        {
            BaggagePanel.Show();
        }
    }

    private void ResetBaggageItem(bool isActive)
    {
        foreach (var item in playerBaggageDic)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                var behav = GetBaseBevByUid(item.Value[i]);
                if (behav != null)
                {
                    behav.gameObject.SetActive(isActive);
                }
            }
        }
        playerBaggageDic.Clear();
    }

    public void OnChangeItemClick(BaggageItem baggageItem)
    {
        if (GameManager.Inst.ugcUserInfo == null)
        {
            return;
        }
        selfUid = GameManager.Inst.ugcUserInfo.uid;
        if (GlobalFieldController.CurGameMode == GameMode.Play)
        {
            ChangeItem(selfUid, baggageItem.uid);
        }
        else if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            var baseBev = GetBaseBevByPlayerId(selfUid, baggageItem.uid);
            if (baseBev != null)
            {
                SendMsgToSever(selfUid, baseBev);
            }
        }
    }

    private NodeBaseBehaviour GetBaseBevByPlayerId(string playerId,int itemUid)
    {
        if (!playerBaggageDic.ContainsKey(playerId))
        {
            return null;
        }
        if (!playerBaggageDic[playerId].Contains(itemUid))
        {
            return null;
        }
        return GetBaseBevByUid(itemUid);
    }

    //切换道具
    public void ChangeItem(string playerId , int itemId)
    {
        if (playerId == GameManager.Inst.ugcUserInfo.uid)
        {
            handleItemId = itemId;
        }
        var itemList = playerBaggageDic[playerId];
        for (int i = 0; i < itemList.Count; i++)
        {
            var tempBev = GetBaseBevByUid(itemList[i]);
            if (tempBev != null)
            {
                tempBev.gameObject.SetActive(false);
                SetAttacheItem(tempBev);
            }
        }
        if (playerId == GameManager.Inst.ugcUserInfo.uid)
        {
            var playerBase = PlayerControlManager.Inst.playerBase;
            playerBase.ResEmoAnim();
        }
        var behav = GetBaseBevByUid(itemId);
        if (behav != null)
        {
            if (playerId == GameManager.Inst.ugcUserInfo.uid)
            {
                SetControl(behav);
            }
            behav.gameObject.SetActive(IsCanShowItem(behav));
            PickabilityManager.Inst.RePickItemPos(behav);
            PickabilityManager.Inst.SetBaggageChangeAnim(playerId, itemId, behav);
            PickabilityManager.Inst.SetPickNodeRot(playerId);
        }
    }

    //新加道具
    public void AddNewItem(string playerId, int itemUid, string mapId,bool isGetItemsCallBack = false)
    {
        if (!playerBaggageDic.ContainsKey(playerId))
        {
            List<int> tempList = new List<int>();
            playerBaggageDic.Add(playerId, tempList);
        }
        if (playerBaggageDic[playerId].Count > maxItemNum)
        {
            return;
        }
        if (!playerBaggageDic[playerId].Contains(itemUid))
        {
            playerBaggageDic[playerId].Add(itemUid);
        }
        if (BaggagePanel.Instance && playerId == GameManager.Inst.ugcUserInfo.uid)
        {
            if (handleItemId == (int)BaggageItemType.none)
            {
                handleItemId = itemUid;
            }
            var behav = GetBaseBevByUid(itemUid);
            BaggageItem bagItem;
            if (isGetItemsCallBack)
            {
                bagItem = BaggagePanel.Instance.FindEmptyBag();
            }
            else
            {
                bagItem = BaggagePanel.Instance.BaggageSort(-1, true);
            }
            if(bagItem == null)
            {
                return;
            }
            bagItem.ResetInfo();
            bagItem.uid = itemUid;
            bagItem.loading.SetActive(true);
            List<int> resIDs = GameConsts.GameEditIds;
            var comp = behav.entity.Get<GameObjectComponent>();
            if ((comp.handleType == NodeHandleType.Base 
                || comp.handleType == NodeHandleType.PointLight 
                || comp.handleType == NodeHandleType.SpotLight
                || comp.modelType == NodeModelType.FishingModel) 
                && resIDs.Contains(comp.modId))
            {
                GetBasePrimitiveImage(comp.modId, bagItem);
            }
            else if (comp.handleType == NodeHandleType.Combine)
            {
                OnGetImageFail(bagItem);
            }
            else if (comp.modelType == NodeModelType.PGCPlant)
            {
                OnGetPGCPlantImg(comp.modId, bagItem);
            }
            else
            {
                GetItemImage(itemUid, mapId, bagItem);
            }
            BaggagePanel.Instance.RefCutoverBag(BaggagePanel.Instance.baggageState);
        }

    }

    public void RemoveItem(string playerId, int itemUid,string curHoldItem)
    {
        if (!playerBaggageDic.ContainsKey(playerId))
        {
            return;
        }
        if (playerBaggageDic[playerId].Contains(itemUid))
        {
            playerBaggageDic[playerId].Remove(itemUid);
        }
        if (BaggagePanel.Instance && playerId == GameManager.Inst.ugcUserInfo.uid)
        {
            if (playerId == GameManager.Inst.ugcUserInfo.uid) { }
            var baggage = BaggagePanel.Instance.BaggageSort(itemUid, false);
            baggage.ResetInfo();
            var itemList = BaggagePanel.Instance.itemList;
            if(itemList[itemList.Count - 1].uid == -1)
            {
                handleItemId = (int)BaggageItemType.none;
            }
            else if(handleItemId == itemUid)
            {
                handleItemId = itemList[itemList.Count - 1].uid;
            }
            if (handleItemId != (int)BaggageItemType.none)
            {
                PickabilityManager.Inst.RecordPlayerPick(playerId, handleItemId);
            }
            BaggagePanel.Instance.RefCutoverBag(BaggagePanel.Instance.baggageState);
            ChangeItem(playerId, handleItemId);
            CatchPanel.Instance.SetCatchState(true);
        }
        if(!string.IsNullOrEmpty(curHoldItem) && playerId != GameManager.Inst.ugcUserInfo.uid)
        {
            int uid = int.Parse(curHoldItem.Split('_')[1]);
            ChangeItem(playerId, uid);
        }
        var behav = GetBaseBevByUid(itemUid);
        behav.gameObject.SetActive(true);
    }

    //联机发送切换道具
    public void SendMsgToSever(string sendPlayerId, NodeBaseBehaviour behav)
    {
        if (GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            return;
        }
        var entity = behav.entity;
        var gComp = entity.Get<GameObjectComponent>();

        PickPropSyncData pickPropSyncData = PickabilityManager.Inst.GetPickSyncData(entity);
        pickPropSyncData.playerId = sendPlayerId;
        pickPropSyncData.uid = gComp.uid;

        Item itemData = new Item()
        {
            id = behav.entity.Get<GameObjectComponent>().uid,
            type = (int)ItemType.PICK_PROP,
            data = JsonConvert.SerializeObject(pickPropSyncData),
        };
        HoldItemReq holdItemReq = new HoldItemReq()
        {
            mapId = GlobalFieldController.CurMapInfo.mapId,
            item = itemData,
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.SwitchHoldItem,
            data = JsonConvert.SerializeObject(holdItemReq),
        };
        LoggerUtils.Log("BaggageManager SendMsgToSever =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), SendPickPropCallBack);
    }

    private void SendPickPropCallBack(int code, string data)
    {
        if (code != 0)//底层错误，业务层不处理
        {
            return;
        }
    }

    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("PickabilityManager OnReceiveServer " + msg);
        HoldItemCallRsp itemsReq = JsonConvert.DeserializeObject<HoldItemCallRsp>(msg);
        Item item = itemsReq.curItem;
        ChangeItem(senderPlayerId, item.id);
        return true;
    }

    private void GetItemImage(int itemUid, string mapId, BaggageItem emptyGrid)
    {
        if (!BaggagePanel.Instance)
        {
            return;
        }

        if (itemIconDIc.ContainsKey(itemUid))
        {
            BaggagePanel.Instance.SetItemUI(emptyGrid, itemIconDIc[itemUid], itemUid);
            return;
        }

        var httpMapDataInfo = new HttpMapDataInfo
        {
            mapId = mapId,
        };
        HttpUtils.MakeHttpRequest("/ugcmap/info", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpMapDataInfo), (content) =>
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }
            var mapInfo = JsonConvert.DeserializeObject<HttpResponse>(content);
            var getMapInfo = JsonConvert.DeserializeObject<GetMapInfo>(mapInfo.data);
            var itemInfo = getMapInfo.mapInfo;
            CoroutineManager.Inst.StartCoroutine(GameUtils.LoadTexture2D(itemInfo.mapCover,
            (tex) =>
            {
                BaggagePanel.Instance.SetItemUI(emptyGrid, tex, itemUid);
                ClearItemIconDic();
                itemIconDIc[itemUid] = tex;
            }, (contnet)=> {
                LoggerUtils.LogError("Script:BaggageManager GetItemImage error = " + content);
                OnGetImageFail(emptyGrid);
            }));
        }, (contnet) => {
            OnGetImageFail(emptyGrid);
        });
    }

    private void ClearItemIconDic()
    {
        if (itemIconDIc.Count > 10)
        {
            foreach (var item in itemIconDIc)
            {
                itemIconDIc.Remove(item.Key);
                return;
            }
        }
    }

    //基础道具加载他对应的图片
    private void GetBasePrimitiveImage(int modId, BaggageItem emptyGrid)
    {
        var priAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/GameAtlas");
        List<int> resIDs = GameConsts.GameEditIds;
        for (int i = 0; i < resIDs.Count; i++)
        {
            int itemId = resIDs[i];
            if (modId == itemId)
            {
                string iconName = GameManager.Inst.priConfigData[itemId].iconName;
                BaggagePanel.Instance.SetItemUI(emptyGrid, priAtlas.GetSprite(iconName));
                return;
            }
        }

    }

    private void OnGetImageFail(BaggageItem emptyGrid)
    {
        if (emptyGrid.uid == (int)BaggageItemType.none)
        {
            return;
        }
        emptyGrid.loading.SetActive(false);
        emptyGrid.lostTips.SetActive(true);
    }
    private void OnGetPGCPlantImg(int modId, BaggageItem emptyGrid)
    {
        if (PGCPlantManager.Inst.IsPGCPlant(modId))//PGC植物ID为11000——11999)
        {
            var data = GameManager.Inst.PGCPlantDatasDic[modId];
            if (data != null)
            {
                string iconName = data.iconName;
                BaggagePanel.Instance.SetItemUI(emptyGrid, ResManager.Inst.GetGameAtlasSprite(iconName));
            }
        }
    }

    public bool IsSelfBaggageFull()
    {
        if (!playerBaggageDic.ContainsKey(GameManager.Inst.ugcUserInfo.uid))
        {
            return false;
        }
        if (playerBaggageDic[GameManager.Inst.ugcUserInfo.uid].Count >= maxItemNum)
        {
            return true;
        }
        return false;
    }

    public bool IsSelfBaggageNull()
    {
        selfUid = GameManager.Inst.ugcUserInfo.uid;
        if (!playerBaggageDic.ContainsKey(selfUid))
        {
            return true;
        }
        if (playerBaggageDic[selfUid].Count > 0)
        {
            return false;
        }
        return true;
    }

    //通过物体id找到物体behav
    private NodeBaseBehaviour GetBaseBevByUid(int propUid)
    {
        if (PickabilityManager.Inst.PickabilityDic.ContainsKey(propUid))
        {
            var entity = PickabilityManager.Inst.PickabilityDic[propUid].entity;
            var gComp = entity.Get<GameObjectComponent>().bindGo;
            var nBevh = gComp.GetComponent<NodeBaseBehaviour>();
            return nBevh;
        }
        return null;
    }

    public void OnReset()
    {
        if (SceneParser.Inst.GetBaggageSet() == 0)
        {
            return;
        }
        if (BaggagePanel.Instance)
        {
            BaggagePanel.Instance.ResetBaggage();
        }
        ResetBaggageItem(true);
        handleItemId = (int)BaggageItemType.none;
    }

    /// <summary>
    /// 获取房间内拾取道具的状态
    /// </summary>
    /// <param name="dataJson"></param>
    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========BaggageManager===>OnGetItemsCallback:" + dataJson);
        if (string.IsNullOrEmpty(dataJson)) return;

        GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
        if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
        {
            LoggerUtils.Log("[PickabilityManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
            return;
        }

        if (getItemsRsp.mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            if(getItemsRsp.playerCustomDatas == null)
            {
                return;
            }
            HandleGetItems(getItemsRsp.playerCustomDatas);
        }
    }

    private void HandleGetItems(PlayerCustomData[] playerCustomDatas)
    {
        for (int i = 0; i < playerCustomDatas.Length; i++)
        {
            CheckPlayerBagCallBack(playerCustomDatas[i]);
        }
    }

    //断线重连处理
    private void CheckPlayerBagCallBack(PlayerCustomData playerCustomData)
    {
        if (playerCustomData == null || playerCustomData.baggageItems == null)
        {
            return;
        }
        var baggageItems = playerCustomData.baggageItems;
        var props = playerCustomData.props;
        var handleItem = playerCustomData.curHoldItem;
        var playerId = playerCustomData.playerId;
        if (BaggagePanel.Instance && playerId == GameManager.Inst.ugcUserInfo.uid)
        {
            BaggagePanel.Instance.ReBaggage();
            if (playerBaggageDic.ContainsKey(playerId))
            {
                playerBaggageDic.Remove(playerId);
            }
        }
        for (int i = 0; i < props.Length; i++)
        {
            var item = props[i];
            var behav = GetBaseBevByUid(item.id);
            if (behav != null)
            {

                var gcomp = behav.entity.Get<GameObjectComponent>();
                AddNewItem(playerCustomData.playerId, item.id, gcomp.resId);
            }
        }
        ChangeItem(playerId, int.Parse(handleItem.Split('_')[1]));
    }

    public bool IsPlayerBaggageNull(string playerid)
    {
        if (!playerBaggageDic.ContainsKey(playerid))
        {
            return true;
        }
        if (playerBaggageDic[playerid].Count > 0)
        {
            return false;
        }
        return true;
    }
    //判断特殊条件下物体的显隐
    public bool IsCanShowItem(NodeBaseBehaviour nBehav)
    {
        if (nBehav.entity.HasComponent<ParachuteComponent>())
        {
            var bag = ParachuteManager.Inst.GetAttachedItem(nBehav.gameObject);
            if (bag != null && bag.entity.Get<ParachuteBagComponent>().rid != ParachuteManager.DEFAULT_MODEL)
            {
                bag.gameObject.SetActive(true);
            }
            return false;
        }
        return true;
    }
    //设置附属道具的显隐
    public void SetAttacheItem(NodeBaseBehaviour nBehav)
    {
        var bag = ParachuteManager.Inst.GetAttachedItem(nBehav.gameObject);
        if (bag != null)
        {
            bag.gameObject.SetActive(false);
        }
    }
    //设置玩家身上的control
    private void SetControl(NodeBaseBehaviour nBehav)
    {
        ParachuteManager.Inst.SetControl(nBehav);
    }

    public void HandlePlayerCreated()
    {
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
    }

    public void Clear()
    {
    }
}

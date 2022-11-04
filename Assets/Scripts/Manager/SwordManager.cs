using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordInfo
{
    public RoleIconData itemData;
    public int owner;
    public BundlePart part;
    public SpecialEmotePropItem propItem;
}

public class SwordCallbackData
{
    public int type;
    public int id;
    public int part;
    public int opType;
}

public class RoleCtrData
{
    public Transform curSword;
    public int typeTF;
}

public class SwordManager : CInstance<SwordManager>
{
    public string selfUid;
    public Dictionary<string, List<GameObject>> swordAvatarDic = new Dictionary<string, List<GameObject>>();//玩家id对应Avatar的武器
    public Dictionary<string, SpecialEmoteBehaviour> swordDic = new Dictionary<string, SpecialEmoteBehaviour>();//玩家id对应的手持中的武器
    List<SwordInfo> swordList = new List<SwordInfo>();

    private PlayerBaseControl playerCom;
    int initialSword = 100026;//默认小木棍
    SwordInfo selectProp;

    List<HandStyleData> handList;
    List<BagStyleData> bagList;
    public static string quitStateTips = "Please quit current state first.";
    public void Init(PlayerBaseControl playerBaseCom)
    {
        InitAssetPath();
        if (playerCom == null)
        {
            playerCom = playerBaseCom;
        }
#if UNITY_EDITOR
        selfUid = TestNetParams.testHeader.uid;
#else
        selfUid = GameManager.Inst.ugcUserInfo.uid;
#endif
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        SwordAnimDataManager.Inst.InitAnim();
        SwordPanel.Show();
        SwordPanel.Instance.Init();
        SwordPanel.Hide();
    }

    public void InitAssetPath()
    {
        handList = RoleConfigDataManager.Inst.manRoleConfigData.handStyles;
        bagList = RoleConfigDataManager.Inst.manRoleConfigData.bagStyles;
    }

    #region 本地数据操作
    public void AddSword(string pid, SpecialEmoteBehaviour data)
    {
        if (!swordDic.ContainsKey(pid))
        {
            swordDic.Add(pid, data);
        }
        else
        {
            swordDic[pid] = data;
        }
    }

    public void RemoveSword(string pid)
    {
        if (!swordDic.ContainsKey(pid))
        {
            return;
        }
        swordDic.Remove(pid);
    }

    public void AddAvatarSword(string pid, List<GameObject> data)
    {
        if(data == null)
        {
            return;
        }
        if (!swordAvatarDic.ContainsKey(pid))
        {
            swordAvatarDic.Add(pid, data);
        }
        else
        {
            swordAvatarDic[pid] = data;
        }
    }

    public void RemoveAvatarSword(string pid)
    {
        if (!swordAvatarDic.ContainsKey(pid))
        {
            return;
        }
        swordAvatarDic.Remove(pid);
    }
    #endregion

    public void OnChangeMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Edit:
                RemovePrefab(selfUid);
                if (SwordPanel.Instance)
                {
                    SwordPanel.Instance.OnHide();
                }
                break;
        }
    }
    public void OnEmoClick()
    {
        AvatarReqQuerry avatarReqQuerry = RequestPackage();
        HttpUtils.MakeHttpRequest("/other/getAvatarOrigins", (int)HTTP_METHOD.GET, 
            JsonConvert.SerializeObject(avatarReqQuerry), OnGetSwordSuccess, OnGetSwordFail);
    }

    public void OnGetSwordSuccess(string msg)
    {
        LoggerUtils.Log("OnGetSwordSuccess msg = " + msg);
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        if (string.IsNullOrEmpty(repData.data))
        {
            return;
        }
        AvatarClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<AvatarClothesRepInfo>(repData.data);
        var sbehav = GetLoadSwordInfo(resourceInfo.resources);
        if (CheckIsCurSword(sbehav.id,sbehav.part))
        {
            return;
        }
        SendSword(sbehav.id, (int)sbehav.part, 1);
    }

    public bool CheckIsCurSword(int id,BundlePart part)
    {
        if (!swordDic.ContainsKey(selfUid))
        {
            return false;
        }
        if(swordDic[selfUid].id == id && swordDic[selfUid].part == part)
        {
            return true;
        }
        return false;
    }

    public bool IsOwnerSword(List<AvatarClothesInfo> resources,int sid,int part)
    {
        foreach (var item in resources)
        {
            if (item.id == sid && item.resourceType == part && item.isOwner == 1)
            {
                return true;
            }
        }
        return false;
    }

    public void ShowSwordPanel()
    {
        UIControlManager.Inst.CallUIControl("special_emote_enter");
        LoadListData();
    }

    public void LoadListData()
    {
        SpecialEmotePropsPanel.Show();
        SpecialEmotePropsPanel.Instance.Init("Select Sword", OnDoneClick, SpecialEmotePropsType.sword);
        AvatarReqQuerry avatarReqQuerry = RequestPackage();
        HttpUtils.MakeHttpRequest("/other/getAvatarOrigins", (int)HTTP_METHOD.GET,
            JsonConvert.SerializeObject(avatarReqQuerry), OnGetSwordListSuccess, OnGetSwordFail);
    }

    public void OnGetSwordListSuccess(string msg)
    {
        LoggerUtils.Log("OnGetSwordSuccess msg = " + msg);
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        if (string.IsNullOrEmpty(repData.data))
        {
            return;
        }
        AvatarClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<AvatarClothesRepInfo>(repData.data);
        ClearList();
        AddSwordList(resourceInfo.resources);
        SpecialEmoteBehaviour behav = new SpecialEmoteBehaviour();

        if (swordDic.ContainsKey(selfUid))
        {
            behav.id = swordDic[selfUid].id;
            behav.part = swordDic[selfUid].part;
        }
        else
        {
            behav = GetLoadSwordInfo(resourceInfo.resources);
        }
        for (int i = 0; i < swordList.Count; i++)
        {
            if (swordList[i].itemData.id == behav.id && swordList[i].part == behav.part)
            {
                OnPropClick(swordList[i]);
            }
        }
    }
    //刷新清除选中然后拉取最新列表
    public void ClearList()
    {
        selectProp = null;
        for (int i = 0; i < swordList.Count; i++)
        {
            GameObject.DestroyImmediate(swordList[i].propItem.gameObject);
        }
        swordList.Clear();
    }

    public void OnGetSwordFail(string msg)
    {
        LoggerUtils.Log("OnGetSwordFaill msg = " + msg);
    }

    public AvatarReqQuerry RequestPackage()
    {
        AvatarReqQuerry avatarReqQuerry = new AvatarReqQuerry();
        avatarReqQuerry.subType = (int)SpecialEmoHttpReq.sword;
        avatarReqQuerry.parentType = 1;
        avatarReqQuerry.pageSize = 10;
        avatarReqQuerry.toUid = GameManager.Inst.ugcUserInfo.uid;
        avatarReqQuerry.cookie = "";
        return avatarReqQuerry;
    }

    public void OnDoneClick()
    {
        if(selectProp == null)
        {
            return;
        }
        if(selectProp.itemData.id == 0)
        {
            return;
        }
        if (!StateManager.Inst.IsCanPlayEmo())
        {
            return;
        }
        var swordInfo = GetSwordInfoById(selectProp.itemData.id, selectProp.part);
        if (swordInfo != null)
        {
            foreach (var item in swordDic)
            {
                if (item.Value.id == selectProp.itemData.id && item.Value.part == selectProp.part)
                {
                    SpecialEmotePropsPanel.Hide();
                    return;
                }
            }
            SendSword(swordInfo.itemData.id, (int)swordInfo.part, 1);
            SpecialEmotePropsPanel.Hide();
            return;
        }
    }

    public void RemovePrefab(string playerId)
    {
        if (swordDic.ContainsKey(playerId))
        {
            GameObject.Destroy(swordDic[playerId].selfObj);
            RemoveSword(playerId);
        }
    }

    /// <summary>
    /// 生成武器按钮，添加进list
    /// </summary>
    /// <param name="data"></param>
    /// <param name="swordData"></param>
    /// <param name="part"></param>
    public void AddSwordList(List<AvatarClothesInfo> swordData)
    {
        for (int i = 0; i < swordData.Count; i++)
        {
            BundlePart part = (BundlePart)swordData[i].resourceType;
            var roleData = GetRoleIconDataByList(swordData[i].id, part);
            var item = SpecialEmotePropsPanel.Instance.LoadProp(roleData, part);
            if (item == null)
            {
                continue;
            }
            SwordInfo sinfo = new SwordInfo
            {
                itemData = roleData,
                owner = swordData[i].isOwner,
                part = part,
                propItem = item
            };
            item.clickArea.onClick.AddListener(() => { OnPropClick(sinfo); });
            swordList.Add(sinfo);
        }
    }

    /// <summary>
    /// 选中武器的点击事件
    /// </summary>
    /// <param name="sinfo"></param>
    public void OnPropClick(SwordInfo sinfo)
    {
        if(sinfo.owner == 0)
        {
            NotOwned();
            return;
        }
        else
        {
            UncheckAllProp();
            sinfo.propItem.selectImg.gameObject.SetActive(true);
            selectProp = sinfo;
        }
    }

    /// <summary>
    /// 此方法供之后未拥有跳转时使用
    /// </summary>
    private void NotOwned()
    {
        TipPanel.ShowToast("Please claim the digital collectible first.");
    }

    /// <summary>
    /// 取消所有选中态
    /// </summary>
    private void UncheckAllProp()
    {
        for (int i = 0; i < swordList.Count; i++)
        {
            swordList[i].propItem.selectImg.gameObject.SetActive(false);
        }
    }
#region 获取各个list内信息
    private RoleController GetRoleCom(string playerId)
    {
        RoleController roleCom = null;
        if(playerId == selfUid)
        {
            roleCom = playerCom.transform.GetComponentInChildren<RoleController>(true);
        }
        else
        {
            var playerCom = ClientManager.Inst.GetAnimControllerById(playerId);
            if (playerCom != null)
            {
                roleCom = playerCom.transform.GetComponentInChildren<RoleController>(true);
            }
        }
        return roleCom;
    }

    public SwordInfo GetSwordInfoById(int id, BundlePart part)
    {
        for (int i = 0; i < swordList.Count; i++)
        {
            if (swordList[i].itemData.id == id && swordList[i].part == part)
            {
                return swordList[i];
            }
        }
        return null;
    }

    public RoleIconData GetRoleIconDataByList(int id, BundlePart part)
    {
        RoleIconData rData = new RoleIconData();
        switch (part)
        {
            case BundlePart.Bag:
                rData = bagList.Find(x => x.id == id);
                break;
            case BundlePart.Hand:
                rData = handList.Find(x => x.id == id);
                break;
        }
        return rData;
    }

    public SpecialEmoteBehaviour GetLoadSwordInfo(List<AvatarClothesInfo> resources)
    {
        SpecialEmoteBehaviour sBehav = new SpecialEmoteBehaviour();
        var avatarSwordIdGroup = GetAvatarSwordId(selfUid, resources);
        if (avatarSwordIdGroup.id != 0 && avatarSwordIdGroup.isOwner == 1)
        {
            AddAvatarDataToDic(selfUid);
            sBehav = avatarSwordIdGroup;
        }
        else if (!string.IsNullOrEmpty(PlayerPrefs.GetString("CurSwordId")))
        {
            string[] group = PlayerPrefs.GetString("CurSwordId").Split('_');
            int groupId = int.Parse(group[0]);
            int groupPart = int.Parse(group[1]);
            if (IsOwnerSword(resources, groupId, groupPart))
            {
                sBehav.id = groupId;
                sBehav.part = (BundlePart)groupPart;
            }
        }


        if(sBehav.id == 0)
        {
            sBehav.id = initialSword;
            sBehav.part = BundlePart.Hand;
        }
        return sBehav;
    }

    public void AddAvatarDataToDic(string playerId)
    {
        var roleCom = GetRoleCom(playerId);
        var rList = roleCom.GetAvatarObj();
        AddAvatarSword(playerId, rList);
    }


    public SpecialEmoteBehaviour GetAvatarSwordId(string playerId, List<AvatarClothesInfo> list)
    {
        var roleCom = GetRoleCom(playerId);
        var roleData = roleCom.customRoleData;
        SpecialEmoteBehaviour sGroup = new SpecialEmoteBehaviour();
        for (int i = 0; i < list.Count; i++)
        {
            if (roleData.hdId == list[i].id)
            {
                sGroup.id = roleData.hdId;
                sGroup.part = BundlePart.Hand;
                sGroup.isOwner = list[i].isOwner;
                return sGroup;
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (roleData.bagId == list[i].id)
            {
                sGroup.id = roleData.bagId;
                sGroup.part = BundlePart.Bag;
                sGroup.isOwner = list[i].isOwner;
                return sGroup;
            }
        }

        return sGroup;
    }

    #endregion


    public void PlaySwordAnim(string playerId, int swordId,BundlePart part)
    {
        PlayerBaseControl.Inst.PlayerResetIdle();
        PlayAnimShowAllHandle(false, playerId);
        var emoteId = SwordAnimDataManager.Inst.FindemoteId(swordId, part);
        PlayerControlManager.Inst.PlayMove(emoteId);
        SendSword(swordId, (int)part, 2);
    }

    public void OnAnimStopAct(string playerId)
    {
        PlayAnimShowAllHandle(true, playerId);
        if (SwordPanel.Instance)
        {
            SwordPanel.Instance.isLock = false;
        }
    }

    private void PlayAnimShowAllHandle(bool isShow,string playerId)
    {
        foreach (var item in swordDic)
        {
            if(item.Key == playerId && item.Value != null && item.Value.selfObj != null)
            {
                item.Value.selfObj.SetActive(isShow);
            }
        }
    }

    public void forceInterrupt()
    {
        if (IsSelfInSword())
        {
            LeaveSword(selfUid,true);
        }
    }

    public void LeaveSword(string playerId, bool isNeedSendMsg = false)
    {
        if (swordDic.ContainsKey(playerId))
        {
            var sword = swordDic[playerId];
            swordDic.Remove(playerId);
            GameObject.Destroy(sword.selfObj);
            if (playerId == selfUid)
            {
                if (SwordPanel.Instance)
                {
                    SwordPanel.Instance.OnHide();
                    SwordPanel.Instance.isLock = false;
                }
                PlayerBaseControl.Inst.PlayerResetIdle();
            }
            if (isNeedSendMsg)
            {
                SendSword(sword.id, (int)sword.part, 0);
            }
            ShowAvatarProp(true, playerId);
            StopAudio(playerId);
        }
    }

    public bool IsSelfInSword()
    {
        if (swordDic.ContainsKey(selfUid))
        {
            return true;
        }
        return false;
    }

    #region 联机
    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("SwordManager OnReceiveServer " + msg);
        SyncItemsReq syncReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = syncReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in syncReq.items)
            {
                if (item.type == (int)ItemType.SWORD)
                {
                    SwordCallRsp itemsdata = JsonConvert.DeserializeObject<SwordCallRsp>(item.data);
                    if (itemsdata.opType == 1)
                    {
                        DesCurSword(senderPlayerId);
                        LoadSword((BundlePart)itemsdata.part, senderPlayerId, item.id, false);
                    }
                    else if (itemsdata.opType == 0)
                    {
                        LeaveSword(senderPlayerId);
                    }
                    else if (itemsdata.opType == 2)
                    {
                        PlayAnimShowAllHandle(false, senderPlayerId);
                    }
                }
            }
        }
        return true;
    }

    private void SendSword(int uid, int part, int opType)
    {
        SwordCallRsp s = new SwordCallRsp
        {
            part = part,
            opType = opType,
        };
        Item itemData = new Item()
        {
            id = uid,
            type = (int)ItemType.SWORD,
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
        LoggerUtils.Log("Sword SendRequest =>" + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }

    /// <summary>
    /// 断线重连
    /// </summary>
    /// <param name="dataJson"></param>
    public void OnGetItemsCallback(string dataJson)
    {
        LoggerUtils.Log("===========SwordManager===>OnGetItemsCallback:" + dataJson);
        if (string.IsNullOrEmpty(dataJson)) return;

        GetItemsRsp getItemsRsp = JsonConvert.DeserializeObject<GetItemsRsp>(dataJson);
        if (GlobalFieldController.CurMapInfo == null || getItemsRsp == null)
        {
            LoggerUtils.Log("[SwordManager.OnGetItemsCallback]GlobalFieldController.CurMapInfo is null");
            return;
        }

        if (getItemsRsp.mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            if (getItemsRsp.playerCustomDatas == null)
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
            PlayerCustomData playerData = playerCustomDatas[i];

            ActivityData[] activitiesData = playerData.activitiesData;
            if (activitiesData == null)
            {
                continue;
            }
            for (int n = 0; n < activitiesData.Length; n++)
            {
                ActivityData activeData = activitiesData[n];
                if (activeData != null && activeData.activityId == ActivityID.Sword)
                {
                    CheckPlayerBagCallBack(activeData, playerData.playerId);
                }
            }     
        }
    }

    //断线重连处理
    private void CheckPlayerBagCallBack(ActivityData activeData, string playerId)
    {
        SwordCallbackData info = JsonConvert.DeserializeObject<SwordCallbackData>(activeData.data);
        if (info.type == (int)ItemType.SWORD)
        {
            if (info.opType == 1)
            {
                DesCurSword(playerId);
                LoadSword((BundlePart)info.part, playerId, info.id, false);
            }
            else if (info.opType == 0)
            {
                LeaveSword(playerId);
            }
        }
    }

    #endregion

    #region 加载武器
    public void LoadSword( BundlePart part, string playerId,int sid, bool isNeedSendMsg = false)
    {
        switch (part)
        {
            case BundlePart.Bag:
                for (int i = 0; i < bagList.Count; i++)
                {
                    if (sid == bagList[i].id)
                    {
                        LoadBagPrefab(bagList[i], playerId, isNeedSendMsg);
                        return;
                    }
                }
                break;
            case BundlePart.Hand:
                for (int i = 0; i < handList.Count; i++)
                {
                    if (sid == handList[i].id)
                    {
                        LoadHandPrefab(handList[i], playerId, isNeedSendMsg);
                        return;
                    }
                }
                break;
        }
    }

    public void DesCurSword(string playerId)
    {
        if (swordDic.ContainsKey(playerId))
        {
            var sword = swordDic[playerId];
            swordDic.Remove(playerId);
            GameObject.Destroy(sword.selfObj);
        }
    }

    public void LoadHandPrefab(HandStyleData data,string playerId, bool isNeedSendMsg = false)
    {
        BundleMgr.Inst.LoadBundle(BundlePart.Hand, data.texName, ab =>
        {
            var handFbx = ab.LoadAsset<GameObject>(data.modelName);
            var roleCom = GetRoleCom(playerId);
            var rData = roleCom.GetHand(data, handFbx);
            if (rData.curSword == null)
            {
                LoggerUtils.Log("model no Find");
                LeaveSword(playerId,true);
                return;
            }
            var parent_h = roleCom.GetHandTrs(data.handBipType, rData.typeTF);
            var curHand = roleCom.InstantiateHand(rData.curSword.gameObject, parent_h);
            if (roleCom.curPlayerType == RoleController.PlayerType.SelfPlayer)
            {
                GameUtils.ChangeToTargetLayer(LayerMask.LayerToName(roleCom.playerLayer), curHand.transform);
            }
            OnLoadSwordSucc(curHand, BundlePart.Hand, data, playerId, isNeedSendMsg);
        });
    }

    public void LoadBagPrefab(BagStyleData data, string playerId, bool isNeedSendMsg = false)
    {
        BundleMgr.Inst.LoadBundle(BundlePart.Bag, data.texName, ab =>
        {
            var fbxBag = ab.LoadAsset<GameObject>(data.modelName).transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/back").gameObject;
            GameObject bag = null;
            var roleCom = GetRoleCom(playerId);
            bag = roleCom.GetBag(data, fbxBag);
            if (bag == null)
            {
                LoggerUtils.Log("model no Find");
                return;
            }
            if (roleCom.curPlayerType == RoleController.PlayerType.SelfPlayer)
            {
                bag.layer = roleCom.playerLayer;
            }
            OnLoadSwordSucc(bag, BundlePart.Bag, data, playerId, isNeedSendMsg);
        });
    }

    public void OnLoadSwordSucc(GameObject sword,BundlePart part,RoleIconData data,string playerId,bool isNeedSendMsg)
    {
        SpecialEmoteBehaviour sbehav = new SpecialEmoteBehaviour();
        sbehav.SetBehavInfo(data.id, part, sword, data.modelName, 0, SpecialEmotePropsType.sword);
        string swordIdGroup = data.id + "_" + (int)part;
        if (playerId == GameManager.Inst.ugcUserInfo.uid)
        {
            PlayerPrefs.SetString("CurSwordId", swordIdGroup);
        }
        AddSword(playerId, sbehav);
        if (playerId == selfUid)
        {
            SwordPanel.Show();
            SwordPanel.Instance.IsOnShow(false);
            PlayerBaseControl.Inst.PlayerResetIdle();
        }
        AddAvatarDataToDic(playerId);
        ShowAvatarProp(false, playerId);
        CommonSetRecChat(playerId);
        StopAudio(playerId);
    }
    #endregion

    private void CommonSetRecChat(string playerId)
    {
        UserInfo syncPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(playerId);
        if (syncPlayerInfo != null && RoomChatPanel.Instance)
        {
            var userName = syncPlayerInfo.userName;
            RoomChatPanel.Instance.SetRecChat(RecChatType.Sword, userName);
        }
    }

    public void StopAudio(string playerId)
    {
        if (playerId == selfUid)
        {
            PlayerControlManager.Inst.StopPlaySpecialAudio();
        }
        else
        {
            OtherPlayerCtr otherCtr = ClientManager.Inst.GetOtherPlayerComById(playerId);
            otherCtr.animCon.StopAudio();
        }

    }

    public void ShowAvatarProp(bool isShow,string playerId)
    {
        if (swordAvatarDic.ContainsKey(playerId))
        {
            for (int i = 0; i < swordAvatarDic[playerId].Count; i++)
            {
                if(swordAvatarDic[playerId][i] != null)
                {
                    swordAvatarDic[playerId][i].gameObject.SetActive(isShow);
                }
            }
        }
    }

    public void EnterSwordMode(int emoId)
    {
        if (!StateManager.Inst.IsCanPlayEmo())
        {
            return;
        }
        if (PlayModePanel.Instance && PlayModePanel.Instance.mSceneRedDotManager.mEmoIds.Contains(emoId))
        {
            PlayModePanel.Instance.mSceneRedDotManager.mEmoIds.Remove(emoId);
            ShowSwordPanel();
        }
        else
        {
            OnEmoClick();
        }
        PlayerControlManager.Inst.StopPlaySpecialAudio();
    }

    public override void Release()
    {
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }
}

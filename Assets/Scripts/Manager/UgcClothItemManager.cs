using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:Shaocheng
/// Description:UGC衣服道具数据管理和UGC衣服在场景中的创建管理
/// Date: 2022-4-21 14:24:00
/// </summary>
public class UgcClothItemManager : ManagerInstance<UgcClothItemManager>, IManager
{
    public const string WEAR_BTN_TEXT = "Wear";
    public const string WORE_BTN_TEXT = "Wore";

    //每个ugc衣服占用350KB左右
    public const int MAX_COUNT = 99;
    public const string MAX_COUNT_TIP = "Only 99 clothes can be added in the experience.";

    public bool isNotifyRefreshPlayer = false; // 判断是否要退房时通知端上刷新人物数据，只在玩家进行过换装后赋值

    public List<UgcClothItemBehaviour> UgcClothBehaviours = new List<UgcClothItemBehaviour>();

    //玩家原衣服数据，用于编辑和试玩切换时恢复衣服
    public UserInfo sourceUserInfo;
    public Coroutine curAnimCoroutine;

    public void Init()
    {
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public override void Release()
    {
        base.Release();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        UgcClothBehaviours.Clear();
        sourceUserInfo = null;
    }

    public bool IsOverMaxCount()
    {
        if (UgcClothBehaviours.Count >= MAX_COUNT)
        {
            return true;
        }

        return false;
    }

    public bool CheckCanClone(GameObject curTarget)
    {
        if (curTarget.GetComponentInChildren<UgcClothItemBehaviour>() != null)
        {
            int CombineCount = curTarget.GetComponentsInChildren<UgcClothItemBehaviour>().Length;
            if (CombineCount > 1)
            {
                if (CombineCount + UgcClothBehaviours.Count > MAX_COUNT)
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
            else
            {
                if (IsOverMaxCount())
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
        }

        return true;
    }

    private void OnChangeMode(GameMode curMode)
    {
        SetSoldOutTouch(curMode);
        if (curMode == GameMode.Edit)
        {
            //还原人物衣服
            if (sourceUserInfo != null)
            {
                var playerNode = FindPlayerNode();
                ChangeClothLocal(playerNode, sourceUserInfo);
                UpdateUserinfoCloth(sourceUserInfo.imageJson, sourceUserInfo.clothesId);
            }

            //还原隐藏卡位道具
            foreach (var bev in UgcClothBehaviours)
            {
                SetDefaultShow(bev, true);
            }
        }
        else if (curMode == GameMode.Play || curMode == GameMode.Guest)
        {
            //试玩游玩隐藏卡位道具
            foreach (var bev in UgcClothBehaviours)
            {
                if (bev.entity.HasComponent<UGCClothItemComponent>())
                {
                    var cData = bev.entity.Get<UGCClothItemComponent>();
                    if (cData == null || (cData.templateId == 0 && cData.pgcId == 0))
                    {
                        SetDefaultShow(bev, false);
                    }
                }
                else
                {
                    SetDefaultShow(bev, false);
                }
            }
        }
    }

    public GameObject FindPlayerNode()
    {
        var pTrigger = GameObject.Find("PTrigger");
        if (pTrigger == null)
        {
            return null;
        }

        var playerNode = pTrigger.gameObject.transform.parent.gameObject;
        return playerNode;
    }

    private void SetDefaultShow(UgcClothItemBehaviour bev, bool isShow)
    {
        if (bev == null)
        {
            return;
        }

        var mRender = bev.gameObject.GetComponentInChildren<MeshRenderer>();
        if (mRender)
        {
            mRender.enabled = isShow;
        }

        var collider = bev.gameObject.GetComponentInChildren<BoxCollider>();
        if (collider)
        {
            collider.enabled = isShow;
        }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.UgcCloth)
        {
            RemoveUgcClothItem((UgcClothItemBehaviour) behaviour);
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.UgcCloth)
        {
            AddUgcClothItem((UgcClothItemBehaviour) behaviour);
        }
    }

    public void Clear()
    {
        UgcClothBehaviours.Clear();
    }

    //从cache创建模型时会有之前的ugc衣服模型
    public void ResetUgcClothItem(NodeBaseBehaviour behaviour)
    {
        var ugcClothObj = behaviour.gameObject.transform.Find("UGCClothes");
        if (ugcClothObj)
        {
            GameObject.Destroy(ugcClothObj.gameObject);
            var defaultNode = behaviour.gameObject.transform.Find("default");
            defaultNode.gameObject.SetActive(true);
        }
        
        var dcObj = behaviour.gameObject.transform.Find("dcPartObj");
        if (dcObj)
        {
            GameObject.Destroy(dcObj.gameObject);
            var defaultNode = behaviour.gameObject.transform.Find("default");
            defaultNode.gameObject.SetActive(true);
        }
        
    }

    public void AddUgcClothItem(UgcClothItemBehaviour bev)
    {
        if (!UgcClothBehaviours.Contains(bev))
        {
            UgcClothBehaviours.Add(bev);
        }
    }

    public void RemoveUgcClothItem(UgcClothItemBehaviour bev)
    {
        if (UgcClothBehaviours.Contains(bev))
        {
            UgcClothBehaviours.Remove(bev);
        }
    }

    public void SetSourceUserInfo(UserInfo userInfo)
    {
        if (sourceUserInfo == null)
        {
            sourceUserInfo = GameUtils.CloneUserInfo(userInfo);
            LoggerUtils.Log($"UgcClothItemManager SetSourceUserInfo:{JsonConvert.SerializeObject(sourceUserInfo)}");
        }
    }

    //恢复玩家衣服
    public void ChangeClothLocal(GameObject playerNode, UserInfo userInfo)
    {
        if (playerNode == null || userInfo == null)
        {
            return;
        }

        RoleController roleCom = playerNode.GetComponentInChildren<RoleController>(true);
        RoleData rd = JsonConvert.DeserializeObject<RoleData>(userInfo.imageJson);
        
        if (RoleConfigDataManager.Inst.GetClothesById(rd.cloId) == null)
        {
            LoggerUtils.LogError($"Source role cloth data is null !! Maybe get role cloth data failed when enter room.");
            return;
        }
        //恢复UGC衣服
        ClothStyleData styleData = new ClothStyleData()
        {
            templateId = RoleConfigDataManager.Inst.GetClothesById(rd.cloId).templateId,
            clothesUrl = rd.clothesUrl
        };

        var clothesData = RoleConfigDataManager.Inst.GetClothesById(rd.cloId);
        if (clothesData != null)
        {
            if (clothesData.IsPGC())
            {
                roleCom.SetClothesStyle(clothesData.texName); //非ugc衣服恢复
            }
            else
            {
                roleCom.SetUGCClothStyle(styleData);
            }
        }
        //恢复UGC面部彩绘
        PatternStyleData patternstyleData = new PatternStyleData()
        {
            templateId = RoleConfigDataManager.Inst.GetClothesById(rd.cloId).templateId,
            patternUrl = rd.ugcFPData.ugcUrl
        };

        var patternData = RoleConfigDataManager.Inst.GetPatternStylesDataById(rd.fpId);
        if (patternData != null)
        {
            if (patternData.IsPGC())
            {
                roleCom.SetPatternStyle(patternData.texName);
            }
            else
            {
                roleCom.SetUgcPatternStyle(patternstyleData);
            }
        }
    }

    //沉浸购买页面换装
    public void ChangeClothInStorePanel(GameObject playerNode, UGCClothItemComponent ugcClothItemCmp, GameObject wearObj, GameObject woreObj)
    {
        if (PlayerBaseControl.Inst.animCon.IsChanging)
        {
            return;
        }

        if (wearObj != null && woreObj != null)
        {
            wearObj.SetActive(false);
            woreObj.SetActive(true);
            Text woreText = woreObj.GetComponentInChildren<Text>();
            LocalizationConManager.Inst.SetLocalizedContent(woreText, WORE_BTN_TEXT);
        }

        //zoom in到人物，播放换装特效, 第一人称视角不zoom
        if (StorePanel.Instance && (PlayerBaseControl.Inst && PlayerBaseControl.Inst.isTps))
        {
            StorePanel.Instance.SetZoomInPlayer(playerNode.transform);
        }
        RoleWearClothes(ugcClothItemCmp, playerNode);
        //第一人称换装不播特效
        if (PlayerBaseControl.Inst && !PlayerBaseControl.Inst.isTps)
        {
            return;
        }
        // 跷跷板换装不播特效
        if(StateManager.IsOnSeesaw)
        {
            return;
        }
        if(StateManager.IsOnSwing)
        {
            return;
        }
        PlayerBaseControl.Inst.animCon.PlayAnim((int)EmoName.EMO_CHANGE_CLOTH);
    }

    public void UpdateUserinfoCloth(string newImageJson, string newClothId)
    {
        GameManager.Inst.ugcUserInfo.imageJson = newImageJson;
        GameManager.Inst.ugcUserInfo.clothesId = newClothId;
    }
    
    //通知端上刷新人物数据
    public void NotifyRefreshPlayerData()
    {
        if (isNotifyRefreshPlayer)
        {
            LoggerUtils.Log("UgcClothItemManager NotifyRefreshPlayerData");
            MobileInterface.Instance.NotifyRefreshPlayerInfo();
            isNotifyRefreshPlayer = false;
        }
    }
    /// <summary>
    /// 人物穿衣（衣服、面部彩绘）
    /// </summary>
    /// <param name="ugcClothItemCmp"></param>
    /// <param name="playerNode"></param>
    public void RoleWearClothes(UGCClothItemComponent ugcClothItemCmp, GameObject playerNode)
    {
        RoleController roleCom = PlayerManager.Inst.selfPlayer.playerAnim.GetComponent<RoleController>();
        switch (ugcClothItemCmp.dataSubType)
        {
            case (int)DataSubType.Clothes:
                ClothStyleData styleData = new ClothStyleData()
                {
                    templateId = ugcClothItemCmp.templateId,
                    clothesUrl = ugcClothItemCmp.clothesUrl
                };
                roleCom.SetUGCClothStyle(styleData);
                break;
            case (int)DataSubType.Patterns:
                PatternStyleData patStyData = new PatternStyleData()
                {
                    templateId = ugcClothItemCmp.templateId,
                    patternUrl = ugcClothItemCmp.clothesUrl
                };
                roleCom.SetUgcPatternStyle(patStyData);
                break;
        }
    }
    //穿衣服消息
    public void SendChangeClothMsg(UGCClothItemComponent cmp)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            var dataStr = JsonConvert.SerializeObject(cmp);
            if (string.IsNullOrEmpty(dataStr))
            {
                return;
            }

            var roomChatData = new RoomChatData()
            {
                msgType = (int)RecChatType.ChangeCloth,
                data = JsonConvert.SerializeObject(new RoomChatCustomData()
                {
                    type = (int) ChatCustomType.ChangeCloth,
                    data = dataStr
                })
            };

            LoggerUtils.Log($"UgcClothItem send SendChangeClothMsg : {JsonConvert.SerializeObject(roomChatData)}");
            ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
        }
    }

    //广播处理
    public void OnHandleChangeClothBst(string senderPlayerId, string msg)
    {
        LoggerUtils.Log($"UgcClothItem OnHandleChangeClothBst: sendPlayerId:{senderPlayerId}, msg:{msg}");
        if (senderPlayerId == GameManager.Inst.ugcUserInfo.uid || string.IsNullOrEmpty(msg))
        {
            return;
        }

        RoomChatCustomData customData = JsonConvert.DeserializeObject<RoomChatCustomData>(msg);
        var ugcClothItemCmp = JsonConvert.DeserializeObject<UGCClothItemComponent>(customData.data);
        if (ugcClothItemCmp == null)
        {
            return;
        }

        var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(senderPlayerId);
        if (otherComp != null)
        {
            var playerNode = otherComp.gameObject;
            RoleWearClothes(ugcClothItemCmp, playerNode);
            playerNode.GetComponent<AnimationController>().PlayAnim((int)EmoName.EMO_CHANGE_CLOTH);
        }
    }
    public List<DcSaveInfo> AddDCList(List<DcSaveInfo> dcList)
    {
      
        UGCClothItemComponent com;
        for (int i = 0; i < UgcClothBehaviours.Count; i++)
        {
            com = UgcClothBehaviours[i].entity.Get<UGCClothItemComponent>();
            if (com!=null)
            {
                DcSaveInfo dc = new DcSaveInfo() {
                    dcId = com.dcId,
                    address = com.walletAddress
                };
                dcList.Add(dc);

            }
        }
        
        return dcList;
    }
    UGCClothItemComponent com;
    public void OnDcSoldOut(string msg)
    {
        CustomData data = JsonConvert.DeserializeObject<CustomData>(msg);
        DcSaveInfo info = JsonConvert.DeserializeObject<DcSaveInfo>(data.data);
        SetSoldOut(info.dcId, info.address);
    }
    public void SetSoldOut(string id , string address)
    {
        for (int i = 0; i < UgcClothBehaviours.Count; i++)
        {
            com = UgcClothBehaviours[i].entity.Get<UGCClothItemComponent>();
            if (com != null && com.isDc == 1)
            {
                if (com.dcId == id && com.walletAddress == address)
                {
                    UgcClothBehaviours[i].SetSoldOut();
                }
            }
        }
    }
    public void SetSoldOutList(List<DcSaveInfo> dcInfos)
    {
        for (int i = 0; i < dcInfos.Count; i++)
        {
            if (dcInfos[i].isSoldOut != 0)
            {
                SetSoldOut(dcInfos[i].dcId, dcInfos[i].address);
            }
        }
    }
    public void SetSoldOutTouch(GameMode curMode)
    {
        for (int i = 0; i < UgcClothBehaviours.Count; i++)
        {
            UgcClothBehaviours[i].OnModeChange(curMode);
        }
    }
}
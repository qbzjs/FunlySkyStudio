using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;
using TMPro;
using UnityEngine;

public partial class ClientManager : MonoBehaviour
{
    public void CreatePlayers()
    {
        //LoggerUtils.LogError("onCreatePlayers");
        if (Global.Room == null)
        {
            LoggerUtils.LogError("ClientMgr CheckInitOtherPlayer error :Global.Room is null!");
            return;
        }

        for (int idx = 0; idx < Global.Room.RoomInfo.PlayerList.Count; idx++)
        {
            var playerInfo = Global.Room.RoomInfo.PlayerList[idx];

            if (playerInfo.Id != Player.Id)
            {
                bool isCreate = false;
                if (!otherPlayerDataDic.ContainsKey(playerInfo.Id))
                {
                    CreateNewPlayer(playerInfo.Id);
                    RealTimeTalkManager.Inst.AddOtherPlayer(playerInfo.Id, playerInfo.Timestamp);
                    isCreate = true;
                }

                if (otherPlayerDataDic[playerInfo.Id].gameObject != null)
                {
                    CheckNeedChangeClothes(otherPlayerDataDic[playerInfo.Id].gameObject, playerInfo.Id);
                }
                CheckShowHP(playerInfo.Id, isCreate);
            }
            else if (playerInfo.Id == Player.Id)
            {
                GameObject myPlayerNode = selfPlayerCom.transform.gameObject;
                //TODO:Save UserInfo 
                PlayerData PlayerDataCom = myPlayerNode.GetComponent<PlayerData>();
                PlayerDataCom.playerInfo = playerInfo;
                //TODO:ChangeClothes
                CheckNeedChangeClothes(myPlayerNode, playerInfo.Id);
                if (PlayerControlManager.Inst.isPickedProp)
                {
                    PlayerControlManager.Inst.ChangeAnimClips();
                }
                if (!selfPlayerCom.isTps)
                {
                    RoleController roleCom = myPlayerNode.GetComponentInChildren<RoleController>(true);
                    if(roleCom != null) {
                        GameObject playerModel = roleCom.transform.gameObject;
                        playerModel.SetActive(false);
                    }
                }
                CheckShowHP(playerInfo.Id, false);
                PlayerControlManager.Inst.playerBase.isChanged = true;
            }
        }
    }

    //创建新加入的玩家
    public void CreateNewPlayer(string playerId)
    {
        if (otherPlayerDataDic == null || playerId == Player.Id || string.IsNullOrEmpty(playerId))
        {
            return;
        }
        if (!otherPlayerDataDic.ContainsKey(playerId))
        {
            Vector3 spwanPoint = Vector3.zero;
            PlayerInfo playerInfo = PlayerInfoManager.GetMgobePlayerInfoById(playerId);
            if (PVPWaitAreaManager.Inst.PVPBehaviour != null)
            {
                spwanPoint = PVPWaitAreaManager.Inst.PVPBehaviour.transform.position;
            }
            else
            {
                var spwanBehav = SpawnPointManager.Inst.GetSpawnPointBehavById(playerInfo.Spawn);
                if(spwanBehav != null)
                {
                    spwanPoint = spwanBehav.transform.localPosition;
                }
            }
            UserInfo syncPlayerInfo = new UserInfo();
#if UNITY_EDITOR
            syncPlayerInfo = GameManager.Inst.ugcUserInfo;
#endif
            var otherPlayer = Instantiate(OtherPlayerPrefab,spwanPoint, Quaternion.identity);
            otherPlayer.transform.parent = OtherPlayerNode.transform;
            otherPlayerDataDic[playerId] = otherPlayer.GetComponent<OtherPlayerCtr>();
            PlayerManager.Inst.otherPlayerDataDic = otherPlayerDataDic;
            PlayerData PlayerDataCom = otherPlayerDataDic[playerId].GetComponent<PlayerData>();
            PlayerDataCom.playerInfo = playerInfo;
            PlayerDataCom.syncPlayerInfo = syncPlayerInfo;
            //初始化层级相关
            var roleCom = otherPlayer.GetComponent<RoleController>();
            roleCom.InitPlayerType(RoleController.PlayerType.OtherPlayer);
            roleCom.InitPlayerLayer();
            otherPlayer.SetActive(false);
        }
    }

    private void CheckShowHP(string playerId, bool isCreate)
    {
        if (isCreate)
        {
            PVPManager.Inst.UpdatePlayerHpShow(playerId);
        }
        else
        {
            PlayerManager.Inst.ShowPlayerState(playerId, false);
        }

        if (PVPWaitAreaManager.Inst.PVPBehaviour != null)
        {
            if (!PVPWaitAreaManager.Inst.IsPVPGameStart)
            {
                PlayerManager.Inst.ExitShowPlayerState();
            }
            else
            {
                PlayerManager.Inst.UpdatePlayerHPVisibleByReconnect();
            }
        }
        
    }

    public void DestroyLeftPlayer(string playerId)
    {
        if (otherPlayerDataDic == null)
        {
            return;
        }
        if (otherPlayerDataDic.ContainsKey(playerId))
        {
            // 如果牵手双方有一方退房，则取消牵手状态
            if (PlayerMutualControl.Inst &&
            (PlayerMutualControl.Inst.startPlayerId == playerId || PlayerMutualControl.Inst.followPlayerId == playerId))
            {
                PlayerMutualControl.Inst.animCon.ReleaseHand();
                PlayerMutualControl.Inst.animCon.StopLoop();
                PlayerMutualControl.Inst.EndMutual();
            }

            var otherPlayerCom = otherPlayerDataDic[playerId];

            var sPlayerId = MutualManager.Inst.SearchHoldingHandsPlayers(playerId);
            if (!string.IsNullOrEmpty(sPlayerId))
            {
                //第三方视角也需要取消牵手状态
                var fPlayerId = MutualManager.Inst.holdingHandsPlayersDict[sPlayerId];
                var sPlayer = ClientManager.Inst.GetOtherPlayerComById(sPlayerId);
                if (sPlayer)
                {
                    sPlayer.SetPlayerFollow(null);
                }
                var fPlayer = ClientManager.Inst.GetOtherPlayerComById(fPlayerId);
                if (fPlayer)
                {
                    fPlayer.ResetPlayerState();
                    fPlayer.isAvoidFrame = false;
                }
            }
            var touchBev = otherPlayerCom.GetComponent<PlayerTouchBehaviour>();
            //离开玩家交互态重置
            touchBev.SetCanTouch(false);
            GameObject otherPlayerObj = otherPlayerCom.gameObject;
            RealTimeTalkManager.Inst.RemoveOtherPlayer(playerId);
            otherPlayerDataDic.Remove(playerId);
            PlayerManager.Inst.allPlayerHp.Remove(playerId);
            Destroy(otherPlayerObj);
        }
    }

    //删除除自己外所有的玩家
    public void DestroyAllOtherPlayer()
    {
        if (otherPlayerDataDic == null)
        {
            LoggerUtils.Log("otherPlayerDataDic is null");
            return;
        }
        foreach (var otherPlayerCom in otherPlayerDataDic.Values)
        {
            GameObject otherPlayerObj = otherPlayerCom.gameObject;
            Destroy(otherPlayerObj);
        }
        otherPlayerDataDic.Clear();
        PlayerManager.Inst.allPlayerHp.Clear();
    }

    /// <summary>
    /// 校验当前玩家是否需要换装
    /// </summary>
    /// <param name="playerNode"></param>
    /// <param name="id"></param>
    public void CheckNeedChangeClothes(GameObject playerNode, string id)
    {
        UserInfo syncPlayerInfo = GetSyncPlayerInfoByBudId(id);
        PlayerData playerDataCom = playerNode.GetComponentInChildren<PlayerData>(true);
        if (playerDataCom == null)
        {
            playerDataCom = playerNode.transform.parent.GetComponentInChildren<PlayerData>(true);
        }

        // 人物形象没有更新，就不需要换装
        if (playerDataCom.syncPlayerInfo.imageJson == syncPlayerInfo.imageJson)
        {
            return;
        }

        if (syncPlayerInfo != null && !string.IsNullOrEmpty(syncPlayerInfo.imageJson))
        {
            playerDataCom.syncPlayerInfo = syncPlayerInfo;
        }
        ChangeClothes(playerNode, syncPlayerInfo);
    }

    public void ChangeClothes(GameObject playerNode, UserInfo userInfo)
    {
        RoleController roleCom = playerNode.GetComponentInChildren<RoleController>(true);
        if (!roleCom)
        {
            return;
        }
        GameObject playerModel = roleCom.transform.gameObject;
        if (userInfo == null || string.IsNullOrEmpty(userInfo.imageJson))
        {
            playerModel.SetActive(false);
            MobileInterface.Instance.LogEventByEventName(LogEventData.unity_getRoleData_error, userInfo.uid);
            LoggerUtils.Log($"userInfo.imageJson is  invalid. playerId :{userInfo.uid}");
        }
        else
        {
            RoleData roleData = JsonConvert.DeserializeObject<RoleData>(userInfo.imageJson);
            //替换被ban的ugc部件
            bool isUgcBan = RoleConfigDataManager.Inst.ReplaceUGC(roleData, userInfo);
            //替换未拥有的DC部件
            bool isDcReplaced = RoleConfigDataManager.Inst.ReplaceNotOwnedDC(userInfo, roleData);
            roleCom.InitRoleByData(roleData);

            if (userInfo.uid == Player.Id)
            {
                HandleMyPlayer(roleCom, userInfo, isDcReplaced, isUgcBan);
            }

            playerModel.SetActive(true);
            SuperTextMesh nick = playerModel.transform.Find("playerInfo/nick").GetComponent<SuperTextMesh>();
            bool showUserName = GlobalSettingManager.Inst.IsShowUserName();
            nick.gameObject.SetActive(showUserName);
            nick.text = userInfo.userName;
            //认证图标
            Transform verify = playerModel.transform.Find("playerInfo/nick/verify");
            bool isVerify = userInfo.officialCert != null && userInfo.officialCert.accountClass == 1;
            verify.gameObject.SetActive(showUserName && isVerify);
            AdjustVerifyPos(verify);

            playerModel.GetComponentInChildren<VoiceItemPos>(true).SetPos(false);
            AnimationController animCon = playerNode.GetComponentInParent<AnimationController>();
            if (animCon == null)
            {
                animCon = playerNode.GetComponent<AnimationController>();
            }
            if (animCon != null)
            {
                animCon.PlayEyeAnim(roleData.eId);
            }
        }
    }

    private void AdjustVerifyPos(Transform verify)
    {
        VerifyItemPos verifyItemPos = verify.GetComponent<VerifyItemPos>();
        verifyItemPos.RefreshPos();
    }

    /// <summary>
    ///  换装时处理一些 MyPlayer 的逻辑
    /// </summary>
    /// <param name="roleCom"></param>
    /// <param name="userInfo"></param>
    public void HandleMyPlayer(RoleController roleCom, UserInfo userInfo, bool isDcReplaced, bool isUgcBan)
    {
        //如果所穿的UGC衣服违规，弹toast提示
        if (isUgcBan)
        {
            TipPanel.ShowToast("Your clothing was removed for violating our community guidelines.");
        }
        //Dc部件被卖出，替换后弹出提示
        if (isDcReplaced)
        {
            TipPanel.ShowToast("The digital collectibles contained in your outfit have been sold.");
        }
        roleCom.InitPlayerLayer();

        if (GameManager.Inst.ugcUserInfo == null || string.IsNullOrEmpty(GameManager.Inst.ugcUserInfo.imageJson))
        {
            GameManager.Inst.ugcUserInfo = userInfo;
        }
    }

    /// <summary>
    ///  更新房间内的玩家
    /// </summary>
    public void UpdatePlayers()
    {
        var time1 = GameUtils.GetSystemTime();
        CheckLeftPlayer();
        CreatePlayers();
        MessageHelper.Broadcast(MessageName.PlayerCreate);
        NetMessageHelper.Inst.OnPlayerCreated();
        var time2 = GameUtils.GetSystemTime();
        LoggerUtils.Log("###解析人物形象耗时:" + (time2 - time1));
    }
    private void CheckLeftPlayer()
    {
        List<string> removeList = new List<string>();
        if ((Global.Room.RoomInfo.PlayerList.Count - 1) != otherPlayerDataDic.Count)
        {
            foreach (var playerId in otherPlayerDataDic.Keys)
            {
                bool isExist = false;
                for (int i = 0; i < Global.Room.RoomInfo.PlayerList.Count; i++)
                {
                    if (playerId == Global.Room.RoomInfo.PlayerList[i].Id)
                    {
                        isExist = true;
                        break;
                    }
                }
                //重连回来发现减少玩家
                if (isExist == false)
                {
                    removeList.Add(playerId);
                }
            }

            for (int i = 0; i < removeList.Count; i++)
            {
                string playerId = removeList[i];
                DestroyLeftPlayer(playerId);
            }
        }
    }

    public UserInfo GetSyncPlayerInfoByBudId(string budId)
    {
        if (Global.Room.RoomInfo.PlayerList.Count > 0)
        {
            LoggerUtils.Log("------------ GetSyncPlayerInfoByBudId ----------- Global.Room.RoomInfo.PlayerList.Count is " + Global.Room.RoomInfo.PlayerList.Count);
            foreach (var playerInfo in Global.Room.RoomInfo.PlayerList)
            {
                if (playerInfo.Id == budId)
                {
                    if (playerInfo.ImageChosenDataJson == null || string.IsNullOrEmpty(playerInfo.ImageChosenDataJson))
                    {
                        LoggerUtils.Log("------------ GetSyncPlayerInfoByBudId ----------- playerInfo.ImageChosenDataJson is null ");
                        return null;
                    }
                    LoggerUtils.Log("------------ GetSyncPlayerInfoByBudId ----------- playerInfo.Id : " + playerInfo.Id);
                    LoggerUtils.Log("------------ GetSyncPlayerInfoByBudId ----------- ImageChosenDataJson : " + playerInfo.ImageChosenDataJson);
                    var time1 = GameUtils.GetSystemTime();
                    UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(playerInfo.ImageChosenDataJson);
                    var time2 = GameUtils.GetSystemTime();
                    LoggerUtils.Log("### GetSyncPlayerInfoByBudId 解析 UserInfo 耗时:" + (time2 - time1));

                    if (string.IsNullOrEmpty(userInfo.imageJson))
                    {
                        LoggerUtils.Log("------------ GetSyncPlayerInfoByBudId ----------- userInfo.imageJson is invalid ");
                    }
                    return userInfo;
                }
            }
        }
        return null;
    }

    public OtherPlayerCtr GetOtherPlayerComById(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return null;
        }
        if (otherPlayerDataDic.ContainsKey(playerId))
        {
            return otherPlayerDataDic[playerId];
        }
        else
        {
            return null;
        }
    }

    public AnimationController GetAnimControllerById(string playerId)
    {
        if (playerId == Player.Id)
        {
            return selfPlayerCom.GetComponent<AnimationController>();
        }

        var otherPlayerCom = GetOtherPlayerComById(playerId);
        if (otherPlayerCom == null)
        {
            return null;
        }

        return otherPlayerCom.GetComponent<AnimationController>();
    }

    // TODO：抽象成泛型的方式来写
    public PlayerPromoteController GetPlayerPromoteController(string playerId)
    {
        PlayerPromoteController controller = null;

        var playerNode = GetPlayerNode(playerId);
        if (playerNode != null)
        {
            controller = playerNode.GetComponent<PlayerPromoteController>();
            if (controller == null)
            {
                controller = playerNode.AddComponent<PlayerPromoteController>();
                controller.Init(playerId);
            }
        }

        return controller;
    }

    public GameObject GetPlayerNode(string playerId)
    {
        GameObject playerNode = null;
        if (playerId == GameManager.Inst.ugcUserInfo.uid)
        {
            if (selfPlayerCom != null)
                playerNode = selfPlayerCom.gameObject;
        }
        else
        {
            var otherPlayerCom = GetOtherPlayerComById(playerId);
            if (otherPlayerCom != null)
                playerNode = otherPlayerCom.gameObject;
        }

        return playerNode;
    }

    private void BindSettingEvents()
    {
        GlobalSettingManager.Inst.OnShowUserNameChange += ShowUserNameChange;
    }

    private void ShowUserNameChange(bool open)
    {
        for (int idx = 0; idx < Global.Room.RoomInfo.PlayerList.Count; idx++)
        {
            var playerInfo = Global.Room.RoomInfo.PlayerList[idx];
            if (playerInfo.Id != Player.Id)
            {
                if (otherPlayerDataDic[playerInfo.Id].gameObject != null)
                {
                    ChangeNameShowStatus(otherPlayerDataDic[playerInfo.Id].gameObject,open,playerInfo.Id);
                }
            }
        }
    }

    private void ChangeNameShowStatus(GameObject go,bool open,string id)
    {
        SuperTextMesh nick = go.transform.Find("playerInfo/nick").GetComponent<SuperTextMesh>();
        nick.gameObject.SetActive(open);
        UserInfo syncPlayerInfo = GetSyncPlayerInfoByBudId(id);
        bool isVerify = syncPlayerInfo.officialCert != null && syncPlayerInfo.officialCert.accountClass == 1;
        go.transform.Find("playerInfo/nick/verify").gameObject.SetActive(open && isVerify);
    }

    private void UnBindSettingEvents()
    {
        if (GlobalSettingManager.Inst.OnShowUserNameChange != null)
            GlobalSettingManager.Inst.OnShowUserNameChange -= ShowUserNameChange;
    }
}

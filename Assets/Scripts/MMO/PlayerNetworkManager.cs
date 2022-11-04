using System;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description: 玩家网络状态管理
/// Date: 2022-5-26 13:25:29
/// </summary>
public class PlayerNetworkManager : CInstance<PlayerNetworkManager>
{
    private Transform _selfNetTip;
    private bool _isNetTipsShow;
    private bool _isNoNetPanelShow;

    public bool isForceExitPanelShow;

    #region 玩家自身

    public void Init()
    {
        MessageHelper.AddListener(MessageName.PlayerCreate, OnCreatePlayer);
        MessageHelper.AddListener(MessageName.ChangeTps, OnChangeTps);
    }

    public override void Release()
    {
        base.Release();
        isForceExitPanelShow = false;
        MessageHelper.RemoveListener(MessageName.PlayerCreate, OnCreatePlayer);
        MessageHelper.RemoveListener(MessageName.ChangeTps, OnChangeTps);
    }

    public void NetChangeDetect()
    {
        AddAction(() =>
        {
            var netState = Application.internetReachability;
            LoggerUtils.Log($"PlayerNetworkManager NetChangeDetect {netState}");

            if (netState == NetworkReachability.NotReachable)
            {
                _isNoNetPanelShow = true;
                if (isForceExitPanelShow == false)
                {
                    NoNetworkPanel.Show();
                }
            }
            else
            {
                _isNoNetPanelShow = false;
                NoNetworkPanel.Hide();
            }
        });
    }

    public void WeakNetDetect(Room room)
    {
        //弱网提示
        if (room.GetNetworkState(ConnectionType.Common) == false && room.GetNetworkState(ConnectionType.Relay) == false)
        {
            if (room.IsInRoom() && _isNetTipsShow == false)
            {
                AddAction(() =>
                {
                    _isNetTipsShow = true;
                    WeakNetTipShow(true);
                });
            }
        }
        else if (room.GetNetworkState(ConnectionType.Common) == true && room.GetNetworkState(ConnectionType.Relay) == true)
        {
            if (_isNetTipsShow)
            {
                AddAction(() =>
                {
                    _isNetTipsShow = false;
                    WeakNetTipShow(false);
                });
            }
        }
    }


    public void WeakNetTipShow(bool isShow)
    {
        LoggerUtils.Log($"PlayerNetworkManager WeakNetTipShow {isShow}");

        var isFps = PlayerBaseControl.Inst && !PlayerBaseControl.Inst.isTps;
        if (isFps)
        {
            // 弱网UI
            if (isShow)
            {
                //第一人称避免弱网ui和断网ui冲突
                if (!_isNoNetPanelShow)
                {
                    NetTipsPanel.Show();
                }
            }
            else
            {
                NetTipsPanel.Hide();
            }
        }
        else
        {
            // 人物头顶弱网标识
            if (_selfNetTip == null &&ClientManager.Inst != null &&ClientManager.Inst.selfPlayerCom!=null)
            {
                _selfNetTip = ClientManager.Inst.selfPlayerCom.transform.Find("Player/netTip");
            }

            if (_selfNetTip)
            {
                _selfNetTip.gameObject.SetActive(isShow);
            }
        }
    }

    private void OnChangeTps()
    {
        if (!_isNetTipsShow)
        {
            return;
        }

        var isFps = PlayerBaseControl.Inst && !PlayerBaseControl.Inst.isTps;
        if (isFps)
        {
            if (_selfNetTip && _selfNetTip.gameObject.activeSelf)
            {
                _selfNetTip.gameObject.SetActive(false);
                if (!_isNoNetPanelShow)
                {
                    NetTipsPanel.Show();
                }
            }
        }
        else
        {
            NetTipsPanel.Hide();
            if (_selfNetTip)
            {
                _selfNetTip.gameObject.SetActive(true);
            }
        }
    }

    public bool CheckPlayerCanSendReq(RoomChatData roomChatData)
    {
        if (roomChatData == null)
        {
            return false;
        }

        //双人表情发送检查
        if (roomChatData.msgType == (int) RecChatType.Emo)
        {
            var item = JsonConvert.DeserializeObject<Item>(roomChatData.data);
            if (item != null)
            {
                var isMultiPlayerEmo = item.type == (int) EmoType.LoopMutual || item.type == (int) EmoType.Mutual;
                if (isMultiPlayerEmo && Global.Room != null && !Global.Room.GetNetworkState(ConnectionType.Common))
                {
                    TipPanel.ShowToast("Weak network connection, fail to send emote");
                    return false;
                }
            }
        }

        //逻辑道具使用检查
        if (roomChatData.msgType == (int) RecChatType.Items)
        {
            if (Global.Room != null && !Global.Room.GetNetworkState(ConnectionType.Common))
            {
                TipPanel.ShowToast("Weak network connection, fail to interact with object");
                return false;
            }
        }

        return true;
    }

    #endregion

    #region 其他玩家

    public void HandleChangePlayerNetworkStateBst(ChangePlayerNetworkStateBst bst)
    {
        if (bst == null)
        {
            return;
        }

        LoggerUtils.Log($"PlayerNetworkManager handleOtherBst: player:{bst.ChangePlayerId}, netstate:{bst.NetworkState}");

        var playerId = bst.ChangePlayerId;

        if (string.IsNullOrEmpty(playerId) || playerId == Player.Id)
        {
            return;
        }

        switch (bst.NetworkState)
        {
            case NetworkState.CommonOnline:
                OtherPlayerNetTipShow(playerId, false);
                break;
            case NetworkState.CommonOffline:
            case NetworkState.IdleOnline:
                OtherPlayerNetTipShow(playerId, true);
                break;
            default: break;
        }
    }


    private void OnCreatePlayer()
    {
        if (Global.Room == null || Global.Room.RoomInfo == null || Global.Room.RoomInfo.PlayerList == null)
        {
            return;
        }

        foreach (var playerInfo in Global.Room.RoomInfo.PlayerList)
        {
            if (playerInfo.Id == Player.Id)
            {
                continue;
            }
            
            LoggerUtils.Log($"PlayerNetworkManager OncreatePlayer: {playerInfo.Name} -- {playerInfo.Id} -- {playerInfo.CommonNetworkState}");
            

            switch (playerInfo.CommonNetworkState)
            {
                case NetworkState.CommonOnline:
                    OtherPlayerNetTipShow(playerInfo.Id, false);
                    break;
                case NetworkState.CommonOffline:
                case NetworkState.IdleOnline:
                    OtherPlayerNetTipShow(playerInfo.Id, true);
                    break;
                default: break;
            }
        }
    }

    private void OtherPlayerNetTipShow(string playerId, bool isShow)
    {
        var otherPlayerCmp = ClientManager.Inst.GetOtherPlayerComById(playerId);
        if (otherPlayerCmp)
        {
            var netTip = otherPlayerCmp.transform.Find("netTip");
            if (netTip)
            {
                netTip.gameObject.SetActive(isShow);
            }
        }
        else
        {
            LoggerUtils.Log($"PlayerNetworkManager other player :{playerId} not found, maybe already left room.");
        }
    }

    private void AddAction(Action cb)
    {
        MainThreadDispatcher.Enqueue(cb);
    }

    #endregion

    #region 对外接口

    public NetworkState GetPlayerCommonNetState(string playerId)
    {
        if (Global.Room != null && Global.Room.RoomInfo != null && Global.Room.RoomInfo.PlayerList != null)
        {
            foreach (var playerInfo in Global.Room.RoomInfo.PlayerList)
            {
                if (playerInfo != null && playerInfo.Id == playerId)
                {
                    return playerInfo.CommonNetworkState;
                }
            }
        }

        return NetworkState.CommonOffline;
    }

    #endregion
}
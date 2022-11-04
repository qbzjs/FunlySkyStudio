using Newtonsoft.Json;
using System.Collections.Generic;
using BudEngine.NetEngine;

/**
* 通用的双人交互处理
* 主要用来监听回包处理业务报错情况
*/

// 交互错误码
public enum Retcode
{
    JOIN_HAND_FAIL = 20002,
}

public class MutualManager : CInstance<MutualManager>
{

    // 记录房间内所有牵手的玩家队组(key:startPlayerId, value: followPlayerId)
    public Dictionary<string, string> holdingHandsPlayersDict = new Dictionary<string, string>();
    public void OnGetServerRspCallback(string rsp)
    {
        LoggerUtils.Log("=========== MutualManager ===> OnGetServerRspCallback:" + rsp);

        ServerRsp serverRsp = JsonConvert.DeserializeObject<ServerRsp>(rsp);
        //牵手失败处理
        if (serverRsp != null && serverRsp.retcode == (int)Retcode.JOIN_HAND_FAIL)
        {
            LoggerUtils.Log("retcode = " + serverRsp.retcode);
            var playEmoData = JsonConvert.DeserializeObject<EmoItemData>(serverRsp.item.data);
            var stPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(playEmoData.startPlayerId);
            var stPlayerName = stPlayerInfo == null ? " " : stPlayerInfo.userName;
            TipPanel.ShowToast("{0} has cancelled or interacted with other players!", stPlayerName);
            return;
        }
    }

    /**
    * 添加房间内的牵手队组(玩家成功牵手时)
    */
    public void AddHoldingHandsPlayers(string startPlayerId, string followPlayerId)
    {
        if (!holdingHandsPlayersDict.ContainsKey(startPlayerId))
        {
            holdingHandsPlayersDict[startPlayerId] = followPlayerId;
        }
    }

    /**
    * 删除房间内的某一对牵手队组(玩家解除牵手时)
    */
    public void RemoveHoldingHandsPlayers(string startPlayerId)
    {
        if (holdingHandsPlayersDict.ContainsKey(startPlayerId))
        {
            holdingHandsPlayersDict.Remove(startPlayerId);
        }
    }

    /**
    * 查找某一个玩家所在的牵手队组
    * 返回队组的 key
    */
    public string SearchHoldingHandsPlayers(string playerId)
    {
        if (holdingHandsPlayersDict.ContainsKey(playerId))
        {
            return playerId;
        }
        if (holdingHandsPlayersDict.ContainsValue(playerId))
        {
            foreach (string key in holdingHandsPlayersDict.Keys)
            {
                if (holdingHandsPlayersDict[key] == playerId)
                {
                    return key;
                }
            }
        }
        return string.Empty;
    }

    /**
    * 清除房间内的牵手队组
    */
    public void ClearHoldingHandsPlayers()
    {
        holdingHandsPlayersDict.Clear();
    }

    /**
    *  播放其他玩家的动画
    */
    public void PlayOtherPlayersAnim(string playerId)
    {
        var sPlayer = ClientManager.Inst.GetOtherPlayerComById(playerId);
        // 牵手双方播放牵手相关动画
        if (holdingHandsPlayersDict.ContainsKey(playerId))
        {
            var fPlayer = ClientManager.Inst.GetOtherPlayerComById(holdingHandsPlayersDict[playerId]);

            if (sPlayer)
            {
                sPlayer.SetPlayerAnimParam(true, true);
            }
            if (fPlayer)
            {
                fPlayer.SetPlayerAnimParam(true, false);
            }
        }
        // 未牵手玩家播放正常动画
        else if (!holdingHandsPlayersDict.ContainsValue(playerId))
        {
            if (sPlayer)
            {
                sPlayer.SetPlayerAnimParam(false, false);
            }
        }
    }

    public PlayerHandleState SearchHandStateOnPlayers(string playerId)
    {
        if (holdingHandsPlayersDict.ContainsKey(playerId))
        {
            return PlayerHandleState.ActiveHand;
        }

        if (holdingHandsPlayersDict.ContainsValue(playerId))
        {
            return PlayerHandleState.PassiveHand;
        }

        return PlayerHandleState.None;
    }

    public override void Release()
    {
        base.Release();
        ClearHoldingHandsPlayers();
    }
    
    /**
    * 玩家牵手
    */
    public void PlayersHoldingHands(string sPlayerId, string fPlayerId)
    {
        if (sPlayerId == Player.Id || fPlayerId == Player.Id)
        {
            // 牵手成功，进入牵手模式

            var playerMutualCtrl = PlayerControlManager.Inst.playerControlNode.GetComponent<PlayerMutualControl>();
            if (!playerMutualCtrl)
            {
                // 添加牵手交互控制脚本
                playerMutualCtrl = PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerMutualControl>();
            }

            if (PlayerMutualControl.Inst)
            {
                PlayerMutualControl.Inst.StartMutual(sPlayerId, fPlayerId);
            }

            //牵手成功,发起者和跟随者都需要隐藏牵手交互按钮
            var stAnimController = ClientManager.Inst.GetAnimControllerById(sPlayerId);
            PortalPlayPanel.Hide();
        }

        MutualManager.Inst.AddHoldingHandsPlayers(sPlayerId, fPlayerId);
        // 将牵手的玩家设为不可交互
        EmoMsgManager.Inst.SetOtherPlayerTouchable(sPlayerId, 0, false);
        EmoMsgManager.Inst.SetOtherPlayerTouchable(fPlayerId, 0, false);
        var splayer = ClientManager.Inst.GetOtherPlayerComById(sPlayerId);
        var fPlayer = ClientManager.Inst.GetOtherPlayerComById(fPlayerId);
        if (splayer && fPlayer)
        {
            splayer.SetPlayerFollow(fPlayer);
            fPlayer.isAvoidFrame = true;
        }
    }

    /**
    * 玩家解除牵手
    */
    public void PlayersReleaseHands(string sPlayerId, string fPlayerId)
    {
        if (sPlayerId == Player.Id || fPlayerId == Player.Id)
        {
            //牵手取消，需要做取消处理
            if (PlayerMutualControl.Inst)
            {
                PlayerMutualControl.Inst.EndMutual();
            }
        }

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

        MutualManager.Inst.RemoveHoldingHandsPlayers(sPlayerId);
    }


}

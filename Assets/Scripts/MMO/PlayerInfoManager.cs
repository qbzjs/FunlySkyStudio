using System.Collections.Generic;
using BudEngine.NetEngine;
using Newtonsoft.Json;

public class PlayerInfoManager
{
    public static UserInfo GetUserInfoById(string playerId)
    {
        PlayerInfo playerInfo = GetMgobePlayerInfoById(playerId);
        if (playerInfo != null)
        {
            return JsonConvert.DeserializeObject<UserInfo>(playerInfo.CustomProfile);
        }
        return null;
    }

    public static PlayerInfo GetMgobePlayerInfoById(string playerId)
    {
        for (int i = 0; i < Global.Room.RoomInfo.PlayerList.Count; i++)
        {
            PlayerInfo playerInfo = Global.Room.RoomInfo.PlayerList[i];
            if (playerInfo.Id == playerId)
            {
                return playerInfo;
            }
        }
        return null;
    }

    public static string GetPlayerIdByObj(UnityEngine.GameObject other)
    {
        var playerData = other.GetComponentInChildren<PlayerData>();
        if(playerData != null)
        {
            var playerId = playerData.syncPlayerInfo.uid;
            return playerId;
        }
        return null;
    }

    public static OtherPlayerCtr GetOtherPlayerCtrByPlayerId(string playerId)
    {
        if (ClientManager.Inst.otherPlayerDataDic.ContainsKey(playerId)) {
            return ClientManager.Inst.otherPlayerDataDic[playerId];
        }
        return null;
    }

    public static CharBattleControl GetBattleCtr(string playerId)
    {
        if (playerId == Player.Id)
        {
            return PlayerBaseControl.Inst.GetComponent<CharBattleControl>();
        }
        else
        {
            var otherComp = GetOtherPlayerCtrByPlayerId(playerId);
            if (otherComp != null)
            {
                return otherComp.GetComponent<CharBattleControl>();
            }
        }
        return null;
    }
}
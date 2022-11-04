using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

public partial class ClientManager : MonoBehaviour
{
    private MatchRoomPara GetMatchRoomPara()
    {
        var gameInfo = GameManager.Inst.gameMapInfo;
        var configInfo = GameManager.Inst.unityConfigInfo;
        MatchRoomPara para = new MatchRoomPara()
        {
            PlayerInfo = GetPlayerInfoPara(),
            MaxPlayers = (ulong)MAX_PLAYER,
#if !UNITY_EDITOR
            RoomType = gameInfo.mapId + "|" + configInfo.appVersion,
#else
            RoomType = "104|4",
#endif
            SessionId = SessionId
        };
        return para;
    }

    private bool GetOpenBloodInfo()
    {
#if UNITY_EDITOR
        return SceneParser.Inst.GetHPSet() == 1;
#endif
        var gameInfo = GameManager.Inst.gameMapInfo;
        if (gameInfo == null || string.IsNullOrEmpty(gameInfo.unityData))
        {
            LoggerUtils.LogError("GetOpenBloodInfo : gameMapInfo.unityData == null");
            return false;
        }
        var data = JsonConvert.DeserializeObject<UnitySaveData>(gameInfo.unityData);
        return data.openBlood != 0;
    }

    private List<int> GetDamageSources()
    {
        List<int> dmgList = SceneParser.Inst.GetDamageSources();
        var gameInfo = GameManager.Inst.gameMapInfo;
        if (gameInfo == null || string.IsNullOrEmpty(gameInfo.unityData))
        {
            LoggerUtils.LogError("GetDamageSources : gameMapInfo.unityData == null");
            return dmgList;
        }
        var data = JsonConvert.DeserializeObject<UnitySaveData>(gameInfo.unityData);
        if(data.dmgSrcs != null)
        {
            dmgList = data.dmgSrcs;
        }
        return dmgList;
    }

    private float GetCustomBloodInfo()
    {
        var comp = SceneBuilder.Inst.HPEntity.Get<HPControlComponent>();
        //兼容没有设置自定义血量的旧版本
        if (comp.customHP <= 0)
        {
            comp.customHP = 100;
        }
        var customBlood = comp.customHP;
        var gameInfo = GameManager.Inst.gameMapInfo;
        if (gameInfo == null || string.IsNullOrEmpty(gameInfo.unityData))
        {
            LoggerUtils.LogError("GetOpenBloodInfo : gameMapInfo.unityData == null");
            return customBlood;
        }
        var data = JsonConvert.DeserializeObject<UnitySaveData>(gameInfo.unityData);
        return data.customBlood;
    }

    private bool GetHasLeaderBoardInfo()
    {
        var gameInfo = GameManager.Inst.gameMapInfo;
        if (gameInfo == null || string.IsNullOrEmpty(gameInfo.unityData))
        {
            LoggerUtils.LogError("GetLeaderBoardInfo : gameMapInfo.unityData == null");
            return false;
        }
        var data = JsonConvert.DeserializeObject<UnitySaveData>(gameInfo.unityData);
        var haslb = data.hasLeaderboard == 0 ? false : true;
        return haslb;
    }

    private string GetTeamInfo()
    {
        var teamInfoList = GlobalFieldController.pvpData.teamList;
        if (teamInfoList != null)
        {
            return JsonConvert.SerializeObject(teamInfoList);
        }
        return "";
    }

    private bool GetHasBaggageInfo()
    {
        var gameInfo = GameManager.Inst.gameMapInfo;
        if (gameInfo == null || string.IsNullOrEmpty(gameInfo.unityData))
        {
            LoggerUtils.LogError("GetHasBaggageInfo : gameMapInfo.unityData == null");
            return false;
        }
        var data = JsonConvert.DeserializeObject<UnitySaveData>(gameInfo.unityData);
        return data.openBaggage == 0 ? false : true;
    }

    private PlayerInfoPara GetPlayerInfoPara()
    {
        var userName = GameManager.Inst.ugcUserInfo.userName;
        PlayerInfoPara playerInfo = new PlayerInfoPara();
        playerInfo.Name = userName;
        playerInfo.CustomPlayerStatus = 1;
        playerInfo.CustomProfile = "";
        playerInfo.ImageChosenDataJson = CheckUserInfo();
        playerInfo.Lang = GameManager.Inst.CheckLang();
        playerInfo.Locale = CountryCode;
#if UNITY_EDITOR
        playerInfo.WalletAddr = TestNetParams.testHeader.walletAddress;
#else
        playerInfo.WalletAddr = HttpUtils.tokenInfo.walletAddress;
#endif
        return playerInfo;
    }

    /// <summary>
    /// 校验即将发送给联机端的 UserInfo
    /// 如果 UserInfo 没有或不正确，则不给
    /// </summary>
    /// <returns></returns>
    private string CheckUserInfo()
    {
        string userInfo = "";
        if (GameManager.Inst.ugcUserInfo != null && !string.IsNullOrEmpty(GameManager.Inst.ugcUserInfo.imageJson))
        {
            userInfo = JsonConvert.SerializeObject(GameManager.Inst.ugcUserInfo);
        }
        return userInfo;
    }

    //Tips : RoomType is Mean
    private CreateRoomPara GetCreateRoomPara()
    {
        var gameInfo = GameManager.Inst.gameMapInfo;
        var configInfo = GameManager.Inst.unityConfigInfo;
        CreateRoomPara para = new CreateRoomPara()
        {
            RoomName = "private_room",
            MaxPlayers = (uint)MAX_PLAYER,
            IsPrivate = (rMode == EnterRoomMode.Private),
            CustomProperties = "",
#if !UNITY_EDITOR
            RoomType = gameInfo.mapId + "|" + configInfo.appVersion,
#else
            RoomType = "104|4",
#endif
            PlayerInfo = GetPlayerInfoPara(),
            RoomId = "",
            SessionId = SessionId
        };
        return para;
    }

    private EnterRoomPara GetEnterRoomPara()
    {
        var mapId = GameManager.Inst.gameMapInfo.mapId;
        var configInfo = GameManager.Inst.unityConfigInfo;
        var pvpData = GlobalFieldController.pvpData;
        string isPvp = (pvpData.pvpMode == 1) ? "1" : "0";
        int pvpReadyTime = GlobalFieldController.pvpReadyTime;
        
        #region PVP data

        EnterRoomGameInfo pvpGameInfo = new EnterRoomGameInfo()
        {
            IsPvp = isPvp,
            WinType = (uint)pvpData.winType,
            GameDuration = (uint)pvpData.gameTime,
            ReadyTime = (uint)pvpReadyTime,
            OpenBlood = GetOpenBloodInfo(),
            InitBlood = GetCustomBloodInfo(),//此处数据只用于旧地图，1.47.0后废弃
            HasLeaderBoard = GetHasLeaderBoardInfo(),
            Team = GetTeamInfo(),
            IsOpenBaggage = GetHasBaggageInfo(),
            damageType = GetDamageSources(),
            Seq = configInfo.seq,
        };
        #endregion

        EnterRoomPara para = new EnterRoomPara()
        {
            RoomName = "enter_room",
            MaxPlayers = (uint)MAX_PLAYER,
            IsPrivate = (rMode == EnterRoomMode.Private),
            CustomProperties = "",
            PlayerInfo = GetPlayerInfoPara(),
            SessionId = SessionId,
            RoomId = ServerRoomId,
            PlayerSessionId = SessionInfo.playerSessionId,
            Region = SessionInfo.region,

            GameData = JsonConvert.SerializeObject(pvpGameInfo),

#if !UNITY_EDITOR
            RoomType = mapId + "|" + configInfo.appVersion,
#else
            RoomType = string.Format("{0}|{1}", TestNetParams.Inst.CurrentConfig.testMapId, TestNetParams.testHeader.version),
#endif
        };
        return para;
    }
}

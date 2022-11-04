using System;
using BudEngine.NetEngine;
using BudEngine.NetEngine.src;
using Newtonsoft.Json;

/// <summary>
/// 
/// 房间内游戏对局相关收发包控制
///
/// Author:Shaocheng
/// 2022年3月20日20:30:53
/// 
/// </summary>
public partial class ClientManager
{
    #region Battle Enum

    public enum BattleGameType
    {
        PVP = 1, //PVP对战类型
    }

    public enum BattleReqType
    {
        SendGameData = 1, //发送游戏数据
        WeaponAttackData = 2, //武器攻击数据
    }

    public enum BattleWinCondition
    {
        Switch = 1,
        Sensor = 3
    }
    #endregion

    /// <summary>
    /// 获取对局信息, Rsp协议结构为:
    /// message GetGameInfoRsp {
    ///    int32 retcode                       = 1;
    ///    string rmsg                         = 2;
    ///    string game_info                    = 3;  /// json 序列化后的游戏数据数据
    /// }
    /// 
    /// </summary>
    /// <param name="gameType">游戏类型 1 - pvp</param>
    /// <param name="callback">请求回调，业务端可根据需要处理Rsp</param>
    public void Battle_GetGameInfo(BattleGameType gameType,int command, Action<GetGameInfoRsp> callback = null)
    {
        if (isEnterRoom == false)
        {
            return;
        }

        GetGameInfoPara para = new GetGameInfoPara()
        {
            RoomId = Global.Room.RoomInfo.Id,
            GameType = (int) gameType,
            Command = command
        };

        #region TODO:PVP埋点

        #endregion

        LoggerUtils.Log("Battle_GetGameInfo==>" + para);

        Sdk.GetGameInfo(para, (eve) =>
        {
            AddAction(() =>
            {
                var rsp = JsonConvert.DeserializeObject<GetGameInfoRsp>(eve.Data.ToString());
                callback?.Invoke(rsp);
            });
        });
    }

    /// <summary>
    /// 向服务器上报游戏数据, Rsp回包结构为
    ///
    /// message SendGameDataRsp {
    ///     int32  retcode                      = 1;
    ///     string rmsg                         = 2;
    ///     int32  req_type                     = 3; /// 请求类型
    ///     string game_data                    = 4; /// 序列化的游戏数据
    /// }
    ///
    /// </summary>
    /// <param name="reqType">请求类型</param>
    /// <param name="gameData">发送的业务数据，调用方按需进行序列化</param>
    /// <param name="callback">请求回调，业务端可根据需要处理Rsp</param>
    public void Battle_SendGameData(BattleReqType reqType, string gameData, Action<SendGameDataRsp> callback)
    {
        if (isEnterRoom == false)
        {
            return;
        }

        SendGameDataPara para = new SendGameDataPara()
        {
            RoomId = Global.Room.RoomInfo.Id,
            ReqType = (int) reqType,
            GameData = gameData
        };

        #region TODO:PVP埋点

        #endregion

        LoggerUtils.Log("Battle_SendGameData==>" + para);
        Sdk.SendGameData(para, (eve) =>
        {
            AddAction(() =>
            {
                var rsp = JsonConvert.DeserializeObject<SendGameDataRsp>(eve.Data.ToString());
                callback?.Invoke(rsp);
            });
        });
    }
    
}
using System;
using Google.Protobuf;
using BudEngine.NetEngine.src.Net;
using BudEngine.NetEngine.src.Util;

namespace BudEngine.NetEngine.src.BattleGame
{
    public static class BattleCommand
    {
        public static readonly int BATTLESTATE = 0;
        public static readonly int SURVIVALPLAYERSTATE = 1004;
    }

    public class BattleGame : BaseNetUtil
    {
        private const ServerSendClientBstType BattleGameBstType = ServerSendClientBstType.EPushTypeGame;

        public BattleGame(BstCallbacks bstCallbacks) : base(bstCallbacks)
        {
            // 注册广播
            this.SetBroadcastHandler(BattleGameBstType, this.BattleGameBroadcast);
        }

        ///////////////////////////////// 请求 //////////////////////////////////

        // 获取房间内游戏数据信息
        public string GetGameInfo(GetGameInfoReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int) ClientSendServerReqCmd.ECmdGetGameInfoReq;
            var response = new NetResponseCallback(GetGameInfoResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("BattleGame GetGameInfo_Para {0} {1}", para, seq);
            return seq;
        }

        // 获取房间内游戏数据信息
        public string SendGameData(SendGameDataReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int) ClientSendServerReqCmd.ECmdSendGameDataReq;
            var response = new NetResponseCallback(SendGameDataResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("BattleGame SendGameData_Para {0} {1}", para, seq);
            return seq;
        }


        ///////////////////////////////// 响应 //////////////////////////////////

        private void GetGameInfoResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspPacket = res.RspPacket;
            var eve = new ResponseEvent(rspPacket.ErrCode, rspPacket.ErrMsg, rspPacket.Seq, res.Body);
            Debugger.Log("BattleGame GetGameInfo_Response {0}", eve);
            callback?.Invoke(eve);
            return;
        }

        private void SendGameDataResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspPacket = res.RspPacket;
            var eve = new ResponseEvent(rspPacket.ErrCode, rspPacket.ErrMsg, rspPacket.Seq, res.Body);
            Debugger.Log("BattleGame SendGameData_Response {0}", eve);
            callback?.Invoke(eve);
            return;
        }

        ////////////////////////////////////// 广播  /////////////////////////////////////////

        private void BattleGameBroadcast(DecodeBstResult bst, string seq)
        {
            var eve = new BroadcastEvent(bst.Body, seq);
            Debugger.Log("BattleGame Broadcast {0}", eve);
            this.bstCallbacks.Room.OnRecvBattleGameBst(eve);
        }
    }
}
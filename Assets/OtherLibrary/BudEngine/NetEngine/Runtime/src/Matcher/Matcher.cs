using System;
using Google.Protobuf;

using BudEngine.NetEngine.src.Net;
using BudEngine.NetEngine.src.Util;



namespace BudEngine.NetEngine.src.Matcher {
    public class Matcher : BaseNetUtil {
        //private const ServerSendClientBstType MatchTimeoutBroadcastType = ServerSendClientBstType.EPushTypeMatchTimeout;
        //private const ServerSendClientBstType MatchUsersBroadcastType = ServerSendClientBstType.EPushTypeMatchSuccess;
        //private const ServerSendClientBstType CancelMatchBroadcastType = ServerSendClientBstType.EPushTypeMatchCancel;

        public Matcher (BstCallbacks bstCallbacks) : base (bstCallbacks) {
            // 注册广播
            // BroadcastCallback matchTimeoutBst = new BroadcastCallback(this.matchUsersTimeoutBroadcast);
            // BroadcastCallback matchUserstBst = new BroadcastCallback(this.matchUsersBroadcast);

            //this.SetBroadcastHandler (MatchTimeoutBroadcastType, this.MatchUsersTimeoutBroadcast);
            //this.SetBroadcastHandler (MatchUsersBroadcastType, this.MatchUsersBroadcast);
            //this.SetBroadcastHandler (CancelMatchBroadcastType, this.CancelMatchBroadcast);

        }

        ///////////////////////////////// 请求 //////////////////////////////////

        // 多人复杂匹配
        //public string MatchUsersComplex (MatchPlayersReq para, Action<ResponseEvent> callback) {
        //    const int subcmd = (int) ClientSendServerReqWrap2Cmd.ECmdMatchPlayerComplexReq;
        //    var response = new NetResponseCallback (MatchUsersComplexResponse);
        //    var seq = this.Send (para, subcmd, response, callback);
        //    Debugger.Log ("MatchUsersComplex_Para {0} {1}", para, seq);
        //    return seq;
        //}
        // 组队匹配
        //public string MatchGroup (MatchGroupReq para, Action<ResponseEvent> callback) {
        //    const int subcmd = (int) ClientSendServerReqWrap2Cmd.ECmdMatchGroupReq;
        //    var response = new NetResponseCallback (MatchGroupResponse);
        //    var seq = this.Send (para, subcmd, response, callback);
        //    Debugger.Log ("MatchGroup_Para {0} {1}", para, seq);
        //    return seq;
        //}
        // 房间匹配
        public string MatchRoom (MatchRoomSimpleReq para, Action<ResponseEvent> callback) {
            const int subcmd = (int) ClientSendServerReqCmd.ECmdMatchRoomSimpleReq;
            var response = new NetResponseCallback (MatchRoomResponse);
            var seq = this.Send (para, subcmd, response, callback);
            Debugger.Log ("MatchRoom_Para {0} {1}", para, seq);
            return seq;
        }
        // 取消匹配
        //public string CancelMatch (CancelPlayerMatchReq para, Action<ResponseEvent> callback) {
        //    const int subcmd = (int) ClientSendServerReqWrap2Cmd.ECmdMatchCancelMatchReq;
        //    var response = new NetResponseCallback (CancelMatchResponse);
        //    var seq = this.Send (para, subcmd, response, callback);
        //    Debugger.Log ("CancelMatch_Para {0} {1}", para, seq);
        //    return seq;
        //}

        ///////////////////////////////// 响应 //////////////////////////////////

        // 多人复杂匹配
        //private void MatchUsersComplexResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
        //    var rspWrap1 = res.RspWrap1;
        //    var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
        //    Debugger.Log ("MatchUsersComplexResponse {0}", eve);
        //    callback?.Invoke (eve);
        //    return;
        //}

        // 组队匹配
        //private void MatchGroupResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
        //    var rspWrap1 = res.RspWrap1;
        //    var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
        //    Debugger.Log ("MatchGroupResponse {0}", eve);
        //    callback?.Invoke (eve);
        //    return;
        //}

        // 房间匹配
        private void MatchRoomResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var rspPacket = res.RspPacket;
            var eve = new ResponseEvent (rspPacket.ErrCode, rspPacket.ErrMsg, rspPacket.Seq, res.Body);
            Debugger.Log ("MatchRoomResponse {0}", eve);
            callback?.Invoke (eve);
            return;
        }

        // 取消匹配
        //private void CancelMatchResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
        //    var rspWrap1 = res.RspWrap1;
        //    var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
        //    Debugger.Log ("CancelMatchResponse {0}", eve);
        //    callback?.Invoke (eve);
        //    return;
        //}

        ////////////////////////////////////// 广播  /////////////////////////////////////////
        //private void MatchUsersTimeoutBroadcast (DecodeBstResult bst, string seq) {
        //    var eve = new BroadcastEvent (bst.Body, seq);
        //    Debugger.Log ("MatchUsersTimeoutBroadcast {0}", eve);
        //    this.bstCallbacks.Room.OnMatchTimeout (eve);
        //}

        //private void MatchUsersBroadcast (DecodeBstResult bst, string seq) {
        //    var eve = new BroadcastEvent (bst.Body, seq);
        //    Debugger.Log ("MatchUsersBroadcast {0}", eve);
        //    this.bstCallbacks.Room.OnMatchPlayers (eve);
        //}

        //private void CancelMatchBroadcast (DecodeBstResult bst, string seq) {
        //    var eve = new BroadcastEvent (bst.Body, seq);
        //    Debugger.Log ("CancelMatchBroadcast {0}", eve);
        //    this.bstCallbacks.Room.OnCancelMatch (eve);
        //}
    }
}
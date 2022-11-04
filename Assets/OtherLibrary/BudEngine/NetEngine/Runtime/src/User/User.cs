using System;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;

using BudEngine.NetEngine.src.Net;
using BudEngine.NetEngine.src.Util;
using BudEngine.NetEngine.src.Util.Def;

namespace BudEngine.NetEngine.src.User {
    public class LoginPara {
        public LoginPara () {
            this.GameId = GameInfo.GameId;
            this.OpenId = GameInfo.OpenId;
        }

        public string GameId { get; }

        public string OpenId { get; }
    }
    public class User : BaseNetUtil {
        public User (BstCallbacks bstCallbacks) : base (bstCallbacks) {

        }

        private static string CreateSignature (string key, string gameId, string openId, ulong timestamp, ulong nonce) {
            var str = $"game_id={gameId}&nonce={nonce}&open_id={openId}&timestamp={timestamp}";

            var hmac = new HMACSHA1 (Encoding.ASCII.GetBytes (key));
            var hashBytes = hmac.ComputeHash (Encoding.ASCII.GetBytes (str));

            var retStr = Convert.ToBase64String (hashBytes);
            return retStr;
        }

        ////////////////////////////////////// 请求 ////////////////////////////////////
        //public string Login (LoginPara para, string secretKey, Signature signature, Action<ResponseEvent> callback) {
        //    const int subcmd = (int) ClientSendServerReqCmd.ECmdLoginReq;
        //    ulong timestamp;
        //    ulong nonce;
        //    string sign;

        //    if (signature != null) {
        //        timestamp = signature.Timestamp;
        //        nonce = signature.Nonce;
        //        sign = signature.Sign;
        //    } else {
        //        timestamp = SdkUtil.GetCurrentTimeSeconds();
        //        var gRand = new Random ();
        //        var buffer = new byte[sizeof (UInt32)];
        //        gRand.NextBytes (buffer);
        //        nonce = BitConverter.ToUInt32 (buffer, 0);
        //        sign = CreateSignature (secretKey, para.GameId, para.OpenId, timestamp, nonce);
        //    }

        //    var loginReq = new LoginReq {
        //        GameId = para.GameId,
        //        OpenId = para.OpenId,
        //        Sign = sign,
        //        Timestamp = timestamp,
        //        Nonce = nonce,
        //        Platform = 0,
        //        Channel = 0,
        //        DeviceId = para.OpenId + "_" + SdkUtil.deviceId,
        //        Mac = "",
        //        Imei = ""
        //    };

        //    UserStatus.SetStatus (UserStatus.StatusType.Logining);
        //    var response = new NetResponseCallback (LoginResponse);
        //    var seq = Send (loginReq, subcmd, response, callback);
            
        //    Debugger.Log ("Login_Para {0} {1}", loginReq, seq);
            
        //    return seq;
        //}

        //public string Logout (LogoutReq para, Action<ResponseEvent> callback) {
        //    const int subcmd = (int) ClientSendServerReqCmd.ECmdLogoutReq;
        //    var reponse = new NetResponseCallback (LogoutResponse);
        //    var seq = this.Send (para, subcmd, reponse, callback);
        //    Debugger.Log ("Logout_Para {0} {1}", para, seq);

        //    return seq;
        //}

        //public string ChangeCustomPlayerStatus (ChangeCustomPlayerStatusReq para, Action<ResponseEvent> callback) {
        //    const int subcmd = (int) ClientSendServerReqCmd.ECmdChangePlayerStateReq;
        //    var reponse = new NetResponseCallback (ChangeCustomPlayerStatusResponse);
        //    var seq = this.Send (para, subcmd, reponse, callback);
        //    Debugger.Log ("ChangeCustomPlayerStatus_Para {0} {1}", para, seq);
        //    return seq;
        //}

        //public string ChangeRoomPlayerProfile (ChangeRoomPlayerProfileReq para, Action<ResponseEvent> callback) {
        //    const int subcmd = (int) ClientSendServerReqCmd.ECmdChangeRoomPlayerProfile;
        //    var reponse = new NetResponseCallback (ChangeRoomPlayerProfileResponse);
        //    var seq = this.Send (para, subcmd, reponse, callback);
        //    Debugger.Log ("ChangeRoomPlayerProfile_Para {0} {1}", para, seq);
        //    return seq;
        //}

        ////////////////////////////////////// 响应 ////////////////////////////////////
        //private void LoginResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
        //    if (send) {
        //        UserStatus.SetStatus (UserStatus.StatusType.Logout);
        //    }

        //    var rsp = (LoginRsp) res.Body;
        //    var eve = new ResponseEvent (res.RspPacket.ErrCode, res.RspPacket.ErrMsg, res.RspPacket.Seq, rsp);
            
        //    Debugger.Log ("LoginResponse {0}", eve);
            
        //    NetClient.HandleSuccess (eve.Code, () => {
        //        if (eve.Code == ErrCode.EcOk) {
        //            RequestHeader.AuthKey = rsp.Token;
        //            RequestHeader.PlayerId = rsp.PlayerId;
        //            var messageData = rsp;

        //            // 更新状态
        //            UserStatus.SetStatus (UserStatus.StatusType.Login);

        //            // 设置 PlayerInfo
        //            if (string.IsNullOrEmpty (GamePlayerInfo.GetInfo ().Id)) {
        //                GamePlayerInfo.SetInfo (messageData.PlayerId);
        //            }
        //        }
        //    });
        //    UserStatus.SetErrCode (eve.Code, eve.Msg);
        //    callback?.Invoke (eve);
        //}

        //private static void LogoutResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
        //    var packet = res.RspPacket;
        //    var eve = new ResponseEvent (packet.ErrCode, packet.ErrMsg, packet.Seq, null);
            
        //    Debugger.Log ("LogoutResponse {0}", eve);

        //    void HandleSuccess () {
        //        RequestHeader.AuthKey = null;
        //        RequestHeader.PlayerId = null;

        //        UserStatus.SetStatus (UserStatus.StatusType.Logout);

        //        var playerInfo = new PlayerInfo { Id = null };
        //        GamePlayerInfo.SetInfo (playerInfo);
        //    }

        //    NetClient.HandleSuccess (eve.Code, HandleSuccess);
        //    callback?.Invoke (eve);
        //}

        //private void ChangeCustomPlayerStatusResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
        //    var packet = res.RspPacket;
        //    var rsp = (ChangeCustomPlayerStatusRsp)res.Body;
        //    var eve = new ResponseEvent (packet.ErrCode, packet.ErrMsg, packet.Seq, rsp);
            
        //    Debugger.Log ("ChangeCustomPlayerStatusResponse {0}", eve);

        //    callback?.Invoke (eve);
        //}

        //private void ChangeRoomPlayerProfileResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
        //    var packet = res.RspPacket;
        //    var rsp = (ChangeRoomPlayerProfileRsp)res.Body;
        //    var eve = new ResponseEvent (packet.ErrCode, packet.ErrMsg, packet.Seq, rsp);
            
        //    Debugger.Log ("ChangeRoomPlayerProfileResponse {0}", eve);

        //    callback?.Invoke (eve);
        //}
    }
}
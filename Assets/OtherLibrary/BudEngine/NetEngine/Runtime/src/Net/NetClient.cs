using System;
using System.Collections.Generic;
using Google.Protobuf;
using GooglePB = global::Google.Protobuf;

using BudEngine.NetEngine.src.EventUploader;
using BudEngine.NetEngine.src.Util;
using BudEngine.NetEngine.src.Util.Def;

namespace BudEngine.NetEngine.src.Net {
    public class NetClient : Net {

        private readonly int _maxDataLength = Convert.ToInt32 (Math.Pow (2, 13));
        private readonly BstCallbacks _bstCallbacks;
        private static Dictionary<int, Action<byte[]>> _requestMap;
        
        public NetClient (BstCallbacks bstCallbacks) {
            this._bstCallbacks = bstCallbacks;
        }

        // 发送消息请求
        public string SendRequest (Object body, int subcmd, NetResponseCallback response, Action<ResponseEvent> callback, string cmd, string seq) {
            if (seq.Length == 0) {
                seq = Guid.NewGuid ().ToString ();
                var sendQueueVal = new SendQueueValue {
                    Time = DateTime.Now,
                        IsSocketSend = false,
                        Cmd = (int) subcmd,
                        resend = () => this.SendRequest (body, subcmd, response, callback, cmd, seq),
                        response = msg => {
                            response (true, msg, callback);
                            DeleteSendQueue (seq);
                        }
                };
                sendQueueVal.sendSuccess = () => {
                    // if(Socket.Id == 1) Debugger.Log("handle send success {0}", seq);
                    sendQueueVal.IsSocketSend = true;
                };
                sendQueueVal.remove = () => {
                    DeleteSendQueue (seq);
                };
                sendQueueVal.sendFail = (errCode, errMsg) => {
                    var errMessage = "消息发送失败，" + errMsg + "[" + errCode + "]";
                    var rspPacket = new ClientSendServerRsp {
                        Seq = seq,
                        ErrCode = errCode,
                        ErrMsg = errMessage
                    };
                    response (false, new DecodeRspResult {
                        RspPacket = rspPacket,
                    }, callback);
                    DeleteSendQueue (seq);
                };
                AddSendQueue (seq, sendQueueVal);
            }

            // PB request = new PB();

            var qAppRequest = new ClientSendServerReq {
                //Version = RequestHeader.Version,
                //AppName = RequestHeader.AppName,
                //ClientIp = RequestHeader.ClientIp,
                //ServiceIp = RequestHeader.ServiceIp,
                //Business = RequestHeader.Business,
                //AuthKey = RequestHeader.AuthKey,
                //AuthType = RequestHeader.AuthType,
                //AuthIp = RequestHeader.AuthIp,
                //GameId = RequestHeader.GameId,
                //Uid = RequestHeader.Uid,
                //PlayerId = RequestHeader.PlayerId,
                Cmd = (ClientSendServerReqCmd)subcmd,
                Seq = seq
            };
            qAppRequest.Metadata.Add("userId", Player.Id);
            //var accessReq = new ClientSendServerReqWrap2 ();
            //accessReq.Cmd = (ClientSendServerReqCmd) subcmd;
            var data = Pb.EncodeReq (qAppRequest, (GooglePB::IMessage)body);

            if (data.Length > _maxDataLength) {
                SendQueueValue val = null;
                SendQueue.TryGetValue(seq + "", out val);
                var timer = new Timer ();
                timer.SetTimeout (() => {
                    if (val != null) val.sendFail ((int) ProtoErrCode.EcSdkSendFail, "数据长度超限");
                    timer.Stop();
                    timer.Close();
                }, 1);
                return seq;
            }

            var reqData = BuildData (data);

            ////UnityEngine.Debug.Log(reqData.ToString());
            //string str = "";
            //foreach (var b in reqData)
            //{
            //    str = str + b.ToString() + ", ";
            //    //UnityEngine.Debug.Log(b.ToString() + " ");
            //}
            //UnityEngine.Debug.Log(str);

            return this.Send (reqData, seq, (ClientSendServerReqCmd) subcmd);
        }

        private static byte[] BuildData (byte[] data) {
            return BuildData ((byte) MessageDataTag.ClientPre, data, (byte) MessageDataTag.ClientEnd);
        }

        // 接收响应并处理
        public void HandleMessage (byte[] body) {
            try {
                var rsp = Pb.DecodeRsp (body, (_seq) =>
                {
                    SendQueueValue _val = null;
                    SendQueue.TryGetValue(_seq + "", out _val);

                    if (_val == null)
                    {
                        return -1;
                    }
                    
                    return _val.Cmd;
                });
                
                var seq = rsp.RspPacket.Seq;

                SendQueueValue val = null;
                SendQueue.TryGetValue(seq + "", out val);

                var callback = val?.response;

                if (val == null) return;
                // 处理错误码，并拦截 value.response

                EventUpload.PushRequestEvent (new ReqEventParam { RqCmd = val.Cmd, RqSq = rsp.RspPacket.Seq, RqCd = rsp.RspPacket.ErrCode, Time = Convert.ToInt64 ((DateTime.Now - val.Time).TotalMilliseconds) });

                // 心跳不拦截
                if (val.Cmd != (int)ClientSendServerReqCmd.ECmdHeartBeatReq && HandleErrCode (rsp.RspPacket))
                //if (HandleErrCode(rsp.RspPacket))
                {
                    return;
                }

                callback?.Invoke (rsp);
                return;
            } catch (Exception e) {
                Debugger.Log (e.ToString ());
            }
        }

        // 处理登录失败
        private void HandleTokenErr () {
            // 重登录
            UserStatus.SetStatus (UserStatus.StatusType.Logout);
            this.Socket.Emit ("autoAuth", null);
        }

        // 处理checklogin connect失败
        private void HandleRelayConnectErr () {
            Debugger.Log ("handle relay connect err");
            // 重checklogin
            CheckLoginStatus.SetStatus (CheckLoginStatus.StatusType.Offline);
            this.Socket.Emit ("autoAuth", null);

        }

        // 处理异常错误码
        // 返回 true 会拦截 responses 回调
        private bool HandleErrCode (ClientSendServerRsp res) {
            // Debugger.Log("handle errcode {0}", res.ErrCode);
            if (IsTokenError (res.ErrCode)) {
                this.HandleTokenErr ();
                Debugger.Log ("TOKEN_ERROR", res);
                return true;
            }

            if (IsRelayConnectError (res.ErrCode) && this.Socket.Id == (int) ConnectionType.Relay) {
                this.HandleRelayConnectErr ();
                Debugger.Log ("RELAY_CONNECT_ERROR", res);
                return true;
            }
            
            return false;
        }

        private static bool IsTokenError (int errCode) {
            var res = errCode == ErrCode.EcAccessCmdGetTokenErr ||
                errCode == ErrCode.EcAccessCmdTokenPreExpire ||
                errCode == ErrCode.EcAccessCmdInvalidToken ||
                errCode == ErrCode.EcAccessGetCommConnectErr;

            return res;
        }

        private static bool IsRelayConnectError (int errCode) {
            var res = errCode == ErrCode.EcAccessGetRelayConnectErr;
            return res;
        }

        // 如果返回码正确
        public static void HandleSuccess (int code, Action callback) {
            if (code == (int) ProtoErrCode.EcOk) {
                callback ();
            }
        }
    }
}
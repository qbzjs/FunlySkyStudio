using System;
using BudEngine.NetEngine.src.EventUploader;
using BudEngine.NetEngine.src.Net;
using BudEngine.NetEngine.src.Sender;
using BudEngine.NetEngine.src.Util;
using BudEngine.NetEngine.src.Util.Def;

namespace BudEngine.NetEngine.src.Ping {
    public class Pinger : BaseNetUtil {
        private const int _maxPingRetry = 2;

        private int Timeout {
            get {
                // if (this.Id == (int) ConnectionType.Common && Config.EnableUdp) return Config.PingTimeout / 2;
                // return Config.PingTimeout;
                
                //双心跳都规定3s超时时间
                return Config.PingTimeout / 2;
            }
        }

        public enum StateEnum { Resposne, Timeout };
        public StateEnum State { get; set; } = StateEnum.Resposne;

        public bool IsResposne()
        {
            return State == StateEnum.Resposne;
        }

        public bool IsTimeout()
        {
            return State == StateEnum.Timeout;
        }

        public Timer PingTimer { get; set; } = new Util.Timer ();

        public Timer PongTimer { get; set; } = new Util.Timer ();

        public string CurrentSeq { get; set; } = "";

        public int Id { get; }

        public FrameSender FrameSender { get; }

        public static int MaxPingRetry => _maxPingRetry;

        public int Retry { get; set; } = MaxPingRetry;

        public Pinger (BstCallbacks bstCallbacks, int id, FrameSender frameSender) : base (bstCallbacks) {
            this.Id = id;
            this.FrameSender = frameSender;
        }

        ///////////////////////////////// PONG //////////////////////////////////
        public void Ping (Action<ResponseEvent> callback) {
            PingTimer.Stop ();
            //if (string.IsNullOrEmpty (RequestHeader.AuthKey)) {
            //    return;
            //}
            var startTime = DateTime.Now;
            var routeId = FrameSender?.RoomInfo?.RouteId ?? "";
            var conType = this.Id == 1 ? ConnectionType.Relay : ConnectionType.Common;
            var body = new HeartBeatReq {
                ConType = conType,
                RouteId = routeId,
                PlayerId = Player.Id
            };

            void PongResposne (bool send, DecodeRspResult result, Action<ResponseEvent> cb) {
                this.HandlePong (send, result, startTime);
            }

            var seq = this.Send (body, (int) ClientSendServerReqCmd.ECmdHeartBeatReq, PongResposne, callback);
            
            Debugger.Log("Ping {0} {1}", this.Id, seq);
            
            CurrentSeq = seq;
            this.PongTimer.SetTimer (() => HandlePongTimeout (seq), this.Timeout);

            this.client.Socket.Emit("pingSend", new SocketEvent());
        }

        public void Stop () {
            PingTimer.Close ();
            PongTimer.Close ();
        }

        ///////////////////////////////// PONG //////////////////////////////////
        private void HandlePong (bool send, DecodeRspResult res, DateTime startTime) {
            PongTimer.Stop ();
            
            Debugger.Log("Pong {0} {1} {2}", this.Id, res.RspPacket.Seq, send);

            if (!send) {
                this.HandlePongTimeout (res.RspPacket.Seq);
                return;
            }

            this.Retry = MaxPingRetry;
            // 清空发送队列
            this.client.ClearQueue ();

            // 心跳的错误码单独处理
            var errCode = res.RspPacket.ErrCode;

            // 上报心跳时延
            if (this.Id == 1 && errCode == ErrCode.EcOk) {
                EventUpload.PushPingEvent (new PingEventParam (Convert.ToInt64 ((DateTime.Now - startTime).TotalMilliseconds)));
            }

            if (IsTokenError (errCode)) {
                UserStatus.SetStatus (UserStatus.StatusType.Logout);
                this.client.Socket.Emit ("autoAuth", new SocketEvent ());
                return;
            }

            if (IsRelayConnectError (errCode) && this.client.Socket.Id == (int) ConnectionType.Relay) {
                CheckLoginStatus.SetStatus (CheckLoginStatus.StatusType.Offline);
                this.client.Socket.Emit ("autoAuth", new SocketEvent ());
                return;
            }

            State = StateEnum.Resposne;
            this.client.Socket.Emit("pongResposne", new SocketEvent());

            this.PingTimer.SetTimer (() => this.Ping (null), this.Timeout);
        }

        //////////////////////////////// TIMEOUT ////////////////////////////////
        private void HandlePongTimeout (string seq) {
            Debugger.Log("HandlePongTimeout {0} {1}", this.Id, seq);

            State = StateEnum.Timeout;

            this.PongTimer.Stop ();
            this.client.DeleteSendQueue (seq);
            this.Retry--;
            if (!seq.Equals (this.CurrentSeq)) return;
            if (this.client.Socket == null) return;

            // 针对 KCP 的逻辑
            // if (this.Id == (int) ConnectionType.Relay && Config.EnableUdp) {
            // //if (this.Id == (int) ConnectionType.Common && Config.EnableUdp) {
            //     if (this.Retry >= 0) {
            //         // 重试
            //         this.PingTimer.SetTimer (() => this.Ping (null), this.Timeout);
            //         this.client.Socket.Emit("pongTimeout", new SocketEvent());
            //         return;
            //     } else {
            //         this.Retry = MaxPingRetry;
            //     }
            // }
            // else
            // {
            //     this.client.Socket.Emit("pongTimeout", new SocketEvent());
            // }
            
            //双心跳
            this.client.Socket.Emit("pongTimeout", new SocketEvent());

            this.client.ClearQueue ();
            this.client.Socket.ConnectNewSocketTask (this.client.Socket.Url);
        }

        private static bool IsTokenError (int code) {
            return code == ErrCode.EcAccessCmdGetTokenErr ||
                code == ErrCode.EcAccessCmdTokenPreExpire ||
                code == ErrCode.EcAccessCmdInvalidToken ||
                code == ErrCode.EcAccessGetCommConnectErr;
        }

        private static bool IsRelayConnectError (int code) {
            return code == ErrCode.EcAccessGetRelayConnectErr;
        }
    }
}
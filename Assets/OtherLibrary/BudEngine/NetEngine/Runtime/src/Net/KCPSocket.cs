using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using BudEngine.NetEngine.src.Util;
using BudEngine.NetEngine.src.Net.Kcp;
using BudEngine.NetEngine.src.Util.Def;
using WebSocketSharp;
using Timer = BudEngine.NetEngine.src.Util.Timer;
using Debugger = BudEngine.NetEngine.src.Util.Debugger; // using System.IO;
using System.Threading;

namespace BudEngine.NetEngine.src.Net {

    public abstract class SocketTask {
        public const int Connecting = 0;
        public const int Open = 1;
        public const int Closing = 2;
        public const int Closed = 3;

        public int ReadyState { get; set; } = 3;

        public abstract void Connect ();
        public abstract void Send (byte[] data, Action<int> fail, Action success);

        public Action onClose;
        public Action onOpen;
        public Action<SocketEvent> onMessage;
        public Action<SocketEvent> onError;
        public virtual void Close (Action success, Action fail) { }
    }
    
    public class CommSocket : SocketTask {
        private WebSocket _socket;
        private readonly int _port;
        private readonly string _url;
        public CommSocket (string url) {
            //this._url = url.ToLower ().Replace ("wss://", "").Replace ("ws://", "");
            //this._url = $"ws://{this._url}:{Port.TcpRelayPort.ToString()}/";

            this._url = url.ToLower ().Replace ("wss://", "").Replace ("ws://", "");
            var str = url.Split (':');
            this._port = Convert.ToInt32 (str.Length > 1 ? str[1] : Port.TcpRelayPort.ToString());
            this._url = $"ws://{str[0]}:{_port}/";
        }

        public CommSocket(string url, int port)
        {
            this._url = url.ToLower ().Replace ("wss://", "").Replace ("ws://", "");
            this._port = port;
            this._url = $"ws://{this._url}:{this._port}/";
        }

        public override void Connect()
        {  
           
            _socket = new WebSocket(_url);
            _socket.OnMessage += OnMessage;
            _socket.OnOpen += OnOpen;
            _socket.OnError += OnError;
            _socket.OnClose += OnClose;
            ReadyState = Connecting;
            SdkUtil.UnityLog("###websocket new task connect beigin"+ "   socket hashcode:" + _socket.GetHashCode() );
            Task.Run(() =>
            {
                _socket.Connect();
                if(SdkUtil.IsDebug)
                {
                    SdkUtil.UnityLog("###websocket 测试ip地址:" + _socket.Url);
                    int threadCount = Sdk.GetThreadCount();
                    SdkUtil.UnityLog("###websocket new task connect 线程ID:" + Thread.CurrentThread.ManagedThreadId + "   socket hashcode:" + _socket.GetHashCode() + "  threadCount:" + threadCount);
                }
               
            });
        }

        public override void Send (byte[] data, Action<int> fail, Action success) {
            try {
                _socket.Send (data);
                // Debugger.Log ("data length send {0}", data.Length);
                success?.Invoke ();
            } catch (Exception e) {
                // Debugger.Log ("send error {0}", e, ToString ());
                fail?.Invoke (0);
            }
        }

        private void OnMessage (object sender, MessageEventArgs e) {
            if (!e.IsBinary) return;
            var data = e.RawData;
            var bytesRead = e.RawData.Length;
            if (bytesRead <= 0) {
                return;
            }
            var eve = new SocketEvent {
                Data = data
            };
            onMessage (eve);
            return;
        }

        private void OnOpen (object sender, EventArgs e) {
            ReadyState = Open;
            base.onOpen ();
        }

        private void OnClose (object sender, CloseEventArgs e) {
            Debugger.Log ("ws socket close {0} {1} {2}", e.Code, e.Reason, e.WasClean);
            ReadyState = Closed;
            base.onClose ();
        }

        private void OnError (object sender, ErrorEventArgs e) {
            try {
                var eve = new SocketEvent
                {
                    Msg = e.Message
                };
                //Debugger.Log ("ws socket error {0} {1}", e.ToString (), e.Message);
                base.onError(eve);
            }
            catch (Exception exce) {
                SdkUtil.UnityLog("Bud:KCPSocket OnError:" + exce.ToString());
            }
           
        }

        public override void Close(Action success, Action fail)
        {
            SdkUtil.UnityLog("###websocket new task close beigin："+ "   socket hashcode:" + _socket.GetHashCode());
            ReadyState = Closing;
            Task.Run(() =>
            {
                // var time1 = Sdk.GetSystemTime();
                _socket.Close();
                ReadyState = Closed;
                base.onClose();
                if(SdkUtil.IsDebug)
                {
                    int threadCount = Sdk.GetThreadCount();
                    SdkUtil.UnityLog("###websocket new task close 线程ID:" + Thread.CurrentThread.ManagedThreadId + "   socket hashcode:" + _socket.GetHashCode() + "  threadCount:" + threadCount);
                }
                success?.Invoke();
                // var time3 = Sdk.GetSystemTime();
                // SdkUtil.UnityLog("###websocket close耗时：" + (time3 - time1));
            });
        }
    }

    public class KcpSocket : SocketTask {
        private class KcpClient : UdpClient {
            public KcpClient() : base()
            {

            }
            public KcpClient (int port) : base (port) {

            }
            public bool Connected () {
                return this.Active;
            }
        }

        private class SocketStateObject {
            public KcpClient udpClient = null;
            public IPEndPoint endPoint = null;
            private const int BufferSize = 1024;
            public byte[] buffer = new byte[BufferSize];
        }

        private Kcp.Kcp _kcp;
        private readonly IPEndPoint _endPoint;
        private ProtocolType _protocolType;
        private readonly int _port;
        private readonly string _url;

        private readonly KcpClient _udpClient = null;

        // recv buffer
        private byte[] _udpRcvBuf;
        private int _rcvBufSize;
        private readonly byte[] _kcpRcvBuf;
        private Queue<byte[]> _rcvQueue;
        private Queue<byte[]> _forGround;
        private readonly Queue<Exception> _errors;

        // time-out control
        private long _lastRecvTime = 0;
        private readonly int _recvTimeoutSec = 0;
        private bool _needUpdate = false;
        private uint _nextUpdateTime = 0;

        private static readonly Timer Timer = new Timer ();
        public KcpSocket (string url, bool enableUdp, int timeoutSec = 60) {
            this._url = url.ToLower ().Replace ("wss://", "").Replace ("ws://", "");
            var str = url.Split (':');
            this._port = Convert.ToInt32 (str.Length > 1 ? url.Split (':') [1] : Port.GetRelayPort().ToString());
            this._url = str[0];
            var address = Dns.GetHostAddresses (this._url) [0];

            this._endPoint = new IPEndPoint (address, this._port);
            this._protocolType = enableUdp ? ProtocolType.Udp : ProtocolType.Tcp;

            //this._udpClient = new KcpClient (this._port+1);
            //this._udpClient = new KcpClient(new System.Random().Next(10000, 50000));
            this._udpClient = new KcpClient();

            _recvTimeoutSec = timeoutSec;
            _udpRcvBuf = new byte[(Kcp.Kcp.IKCP_MTU_DEF + Kcp.Kcp.IKCP_OVERHEAD) * 3];
            _kcpRcvBuf = new byte[(Kcp.Kcp.IKCP_MTU_DEF + Kcp.Kcp.IKCP_OVERHEAD) * 3];
            _rcvQueue = new Queue<byte[]> (64);
            _forGround = new Queue<byte[]> (64);
            _errors = new Queue<Exception> (8);
        }
        public override void Connect () {
            try {
                // Debugger.Log ("socket2 begin connect");
                ReadyState = Connecting;
                var state = new SocketStateObject { udpClient = _udpClient, endPoint = _endPoint };

                _udpClient.Connect (this._url, this._port);
                if (_udpClient.Connected ()) {
                    _kcp = new Kcp.Kcp (123, OutputKcp);
                    //_kcp.SetOutput (OutputKcp);

                    // fast mode
                    //_kcp.NoDelay (1, 10, 2, 1);
                    _kcp.NoDelay(1, 1, 1, 1);
                    _kcp.WndSize (1024, 1024);

                    Timer.SetTimer (() => StartKcpUpdate (), Config.K);

                    Socket s = this._udpClient.Client;
                    SdkUtil.UnityLog("I am connected to " + IPAddress.Parse(((IPEndPoint)s.RemoteEndPoint).Address.ToString()) + " on port number " + ((IPEndPoint)s.RemoteEndPoint).Port.ToString());
                    SdkUtil.UnityLog("My local IpAddress is :" + IPAddress.Parse(((IPEndPoint)s.LocalEndPoint).Address.ToString()) + " I am connected on port number " + ((IPEndPoint)s.LocalEndPoint).Port.ToString());

                    this.StartReceive ();
                    ReadyState = Open;
                    base.onOpen ();
                }
            } catch (Exception e) {
                Debugger.Log (e.ToString ());
                PushError (e);
            }
        }

        public override void Close (Action success, Action fail) {
            ReadyState = Closing;
            _udpClient.Close ();
            //_kcp.Release ();
            Timer.Stop ();
            ReadyState = Closed;
            base.onClose ();
            success?.Invoke ();
        }

        private void OutputKcp (byte[] data, int size) {
            var binary = new byte[size];
            Buffer.BlockCopy (data, 0, binary, 0, size);
            _udpClient.BeginSend (binary, binary.Length, new AsyncCallback (SendCallback), _udpClient);
        }

        private void ConnectCallback (IAsyncResult ar) {
            try {
                var client = (Socket) ar.AsyncState;
                if (client.Connected) {
                    ReadyState = Open;
                }
            } catch (Exception e) {
                PushError (e);
            }
        }
        private void SendCallback (IAsyncResult ar) {
            try {
                var client = (KcpClient) ar.AsyncState;
                var bytesSent = client.EndSend (ar);
            } catch (Exception e) {
                PushError (e);
            }
        }

        private void StartReceive () {
            try {
                var state = new SocketStateObject { udpClient = this._udpClient, endPoint = this._endPoint };
                _udpClient.BeginReceive (new AsyncCallback (ReceiveCallback), state);
            } catch (Exception e) {
                Debugger.Log (e.ToString ());
            }
        }

        private void ReceiveCallback (IAsyncResult ar) {
            try {
                var state = (SocketStateObject) ar.AsyncState;

                var client = state.udpClient;
                var endPoint = state.endPoint;

                var data = client.EndReceive (ar, ref endPoint);
                var bytesRead = data.Length;

                if (bytesRead <= 0) {
                    var ex = new Exception ("socket closed by peer");
                    PushError (ex);
                    return;
                }
                PushToRecvQueue (data);
                client.BeginReceive (new AsyncCallback (ReceiveCallback), state);
            } catch (Exception e) {
                PushError (e);
            }
        }

        private void PushToRecvQueue (byte[] data) {
            lock (_rcvQueue) {
                _rcvQueue.Enqueue (data);
            }
        }

        // if `rcvqueue` is not empty, swap it with `forground`
        private Queue<byte[]> SwitchRecvQueue () {
            lock (_rcvQueue) {
                if (_rcvQueue.Count <= 0) return _forGround;
                var tmp = _rcvQueue;
                _rcvQueue = _forGround;
                _forGround = tmp;
            }
            return _forGround;
        }

        // dirty write
        private void PushError (Exception ex) {
            // Debugger.Log("KCP push error {0}", ex.ToString());
            lock (_errors) {
                _errors.Enqueue (ex);
            }
        }

        // dirty read
        private Exception GetError () {
            Exception ex = null;
            lock (_errors) {
                if (_errors.Count > 0) {
                    ex = _errors.Dequeue ();
                }
            }
            return ex;
        }

        // 业务消息发送事件，进入 KCP 模块
        public override void Send (byte[] data, Action<int> fail, Action success) {
            if (_kcp == null) {
                return;
            }
            var ret = _kcp.Send (data, 0, data.Length);
            _needUpdate = true;

            if (ret != 0) {
                fail?.Invoke (-1);
            } else {
                success?.Invoke ();
            }
        }

        private void CheckTimeout (uint current) {
            if (_lastRecvTime == 0) {
                _lastRecvTime = current;
            }

            if (current - _lastRecvTime <= _recvTimeoutSec * 1000) return;
            var ex = new TimeoutException ("socket recv timeout");
            PushError (ex);
        }

        private void ProcessRecv (uint current) {
            var queue = SwitchRecvQueue ();
            while (queue.Count > 0) {
                _lastRecvTime = current;
                var data = queue.Dequeue ();
                var r = _kcp.Input (data, 0, data.Length, true, true);
                Debug.Assert (r >= 0);
                _needUpdate = true;
                while (true) {
                    var size = _kcp.PeekSize ();
                    if (size > 0) {
                        r = _kcp.Recv (_kcpRcvBuf, 0, _kcpRcvBuf.Length);
                        if (r <= 0) {
                            break;
                        }
                        var binary = new byte[size];
                        Buffer.BlockCopy (_kcpRcvBuf, 0, binary, 0, size);
                        var eve = new SocketEvent {
                            Data = binary
                        };
                        onMessage (eve);
                    } else {
                        break;
                    }
                }
            }
        }

        private void Update (uint current) {
            ProcessRecv (current);
            var err = GetError ();
            if (err != null) {
                throw err;
            }
            if (_needUpdate || current > _nextUpdateTime) {
                _kcp.Update ();
                _nextUpdateTime = _kcp.Check ();
                _needUpdate = false;
            }
            CheckTimeout (current);
        }

        private void StartKcpUpdate () {
            var now = Convert.ToInt64 (DateTime.Now.Subtract (new DateTime (2000, 1, 1)).TotalMilliseconds);
            Update ((uint) (now & 0xFFFFFFFF));
        }
    }
}
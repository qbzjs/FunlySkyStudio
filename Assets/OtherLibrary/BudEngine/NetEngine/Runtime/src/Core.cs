
using BudEngine.NetEngine.src.Net;
using BudEngine.NetEngine.src.Net.Sockets;
using BudEngine.NetEngine.src.Ping;
using BudEngine.NetEngine.src.Sender;
using BudEngine.NetEngine.src.User;
using BudEngine.NetEngine.src.Util;
using BudEngine.NetEngine.src.Util.Def;

// using Minigame.SdkType;

namespace BudEngine.NetEngine.src
{
    public static class Core
    {
        public static Socket Socket1 { get; set; } = null;

        public static Socket Socket2 { get; set; } = null;

        public static Pinger Pinger1 { get; set; } = null;

        public static Pinger Pinger2 { get; set; } = null;

        public static User.User User { get; set; } = null;

        public static Matcher.Matcher Matcher { get; set; } = null;

        public static Room.Room Room { get; set; } = null;

        public static Sender.Sender Sender { get; set; } = null;

        public static FrameSender FrameSender { get; set; } = null;
        
        public static BattleGame.BattleGame BattleGame { get; set; } = null;

        private static void InitModules()
        {
            Core.User = new User.User(Sdk.BstCallbacks);
            Core.Matcher = new Matcher.Matcher(Sdk.BstCallbacks);
            Core.Sender = new Sender.Sender(Sdk.BstCallbacks);
            Core.Room = new Room.Room(Sdk.BstCallbacks);
            Core.BattleGame = new BattleGame.BattleGame(Sdk.BstCallbacks);

            Core.FrameSender = new FrameSender(Sdk.BstCallbacks);

            Core.Socket1 = new Socket(0, false, null);
            Core.Socket2 = new Socket(1, false, null);

            Core.Pinger1 = new Pinger(Sdk.BstCallbacks, 0, null);
            Core.Pinger2 = new Pinger(Sdk.BstCallbacks, 1, FrameSender);

            var route1 = new BaseNetUtil[6] { User, Room, Sender, Pinger1, Matcher, BattleGame };
            var route2 = new BaseNetUtil[3] { FrameSender.NetUtil1, FrameSender.NetUtil2, Pinger2 };
            // var route2 = new BaseNetUtil[2] { FrameSender.NetUtil1, FrameSender.NetUtil2 };

            foreach (var request in route1)
            {
                request.BindSocket(Core.Socket1);
            }
            foreach (var request in route2)
            {
                request.BindSocket(Core.Socket2);
            }
            
            Pb.Init();
            Sdk.UpdateSdk();
        }

        private static void UnInitModules()
        {
            Socket1?.DestroySocketTask();
            Socket2?.DestroySocketTask();
            //var route = new BaseNetUtil[8] { User, Room, Sender, Matcher, FrameSender.NetUtil1, FrameSender.NetUtil2, Pinger1, Pinger2 };
            var route = new BaseNetUtil[9] { User, Room, Sender, Matcher, FrameSender.NetUtil1, FrameSender.NetUtil2, Pinger1, BattleGame, Pinger2 };
            foreach (var request in route)
            {
                request?.UnbindSocket();
            }
        }

        public static void InitSdk()
        {
            if (!SdkStatus.IsUnInit()) return;
            // 正在初始化
            SdkStatus.SetStatus(SdkStatus.StatusType.Initing);

            Core.InitModules();
            BaseNetUtil.StopQueueLoop();
            BaseNetUtil.StartQueueLoop();

            // 设置 Socket 链接地址
            Socket1.Url = Config.Url;

            // loginEvent += onSocketConnect;
            ListenSocketConnect();

            Socket1.ConnectSocketTask("init Sdk");
        }

        public static void UnInitSdk()
        {
            if (SdkStatus.IsUnInit())
            {
                return;
            }
            
            Pinger1.Stop();
            Pinger2.Stop();
            
            BaseNetUtil.StopQueueLoop();
            Sdk.Instance.ClearResponse();
            
            Core.UnInitModules();
            
            SdkStatus.SetStatus(SdkStatus.StatusType.Uninit);
            UserStatus.SetStatus(UserStatus.StatusType.Logout);
            Sdk.Uninit();
        }

        private static void ListenSocketConnect()
        {
            // 联网
            Socket1.OnEvent("connect", (SocketEvent socketEvent) =>
            {
                // 联网时自动Login 
                //if (!UserStatus.IsStatus(UserStatus.StatusType.Logining))
                //{
                //    UserUtil.Login(null);
                //}
                Debugger.Log("socket1 on connect");
                SdkUtil.UnityLog("socket1 on connect");
                UserStatus.SetStatus(UserStatus.StatusType.Login);
                ulong serverTime = 0;
                ResponseEvent eveInit;
                var initRsp = new InitRsp(serverTime);
                eveInit = new ResponseEvent(ErrCode.EcOk, "", "", initRsp);
                Core.SdkInitCallback(true, eveInit);

                if (string.IsNullOrEmpty(Socket1.Url)) return;
                var eve = new ResponseEvent(ErrCode.EcOk) { Data = Socket1.Id };
                Sdk.BstCallbacks.OnNetwork(eve);
                Pinger1.Ping(null);
            });
            Socket2.OnEvent("connect", (SocketEvent socketEvent) =>
            {
                // check login 成功后发送业务数据
                //FrameSender.CheckLogin(null, "connect " + Socket2.IsSocketStatus("connect"));

                // Debugger.Log("socket2 on connect:"+Socket2.Url);
                Debugger.Log("socket2 on connect");
                SdkUtil.UnityLog("socket2 on connect");
                CheckLoginStatus.SetStatus(CheckLoginStatus.StatusType.Checked);
                if (!string.IsNullOrEmpty(Socket2.Url))
                {
                    var eve = new ResponseEvent(ErrCode.EcOk) { Data = Socket2.Id };
                    Sdk.BstCallbacks.OnNetwork(eve);
                }
                Pinger2.Ping(null);
            });

            // 断网
            Socket1.OnEvent("connectClose", (SocketEvent socketEvent) =>
            {
                Debugger.Log("socket1 on connect close");
                SdkUtil.UnityLog("socket1 on connect close");
                // 初始化失败
                SdkInitCallback(false, new ResponseEvent(ErrCode.EcSdkSocketClose));
                if (!SdkStatus.IsInited()) { return; }
                // 断网时自动 Logout
                UserStatus.SetStatus(UserStatus.StatusType.Logout);
                if (string.IsNullOrEmpty(Socket1.Url)) return;
                var eve = new ResponseEvent(ErrCode.EcSdkSocketClose, "Socket 断开", null, null);
                Sdk.BstCallbacks.OnNetwork(eve);
                Pinger1.Stop();
            });
            Socket2.OnEvent("connectClose", (SocketEvent socketEvent) =>
            {
                Debugger.Log("socket2 on connect close");
                SdkUtil.UnityLog("socket2 on connect close");
                if (!SdkStatus.IsInited()) { return; }
                CheckLoginStatus.SetStatus(CheckLoginStatus.StatusType.Offline);
                if (!string.IsNullOrEmpty(Socket2.Url))
                {
                    var eve = new ResponseEvent(ErrCode.EcSdkSocketClose, "Socket 断开", null, null);
                    Sdk.BstCallbacks.OnNetwork(eve);
                };
                Pinger2.Stop();
            });

            // socket 错误
            Socket1.OnEvent("connectError", (SocketEvent socketEvent) =>
            {
                Debugger.Log("socket1 connectError");
                SdkUtil.UnityLog("socket1 connectError");
                // 初始化失败
                SdkInitCallback(false, new ResponseEvent(ErrCode.EcSdkSocketError));
                if (!SdkStatus.IsInited()) return;
                if (string.IsNullOrEmpty(Socket1.Url)) return;
                var eve = new ResponseEvent(ErrCode.EcSdkSocketError, "Socket 错误", null, null);
                Sdk.BstCallbacks.OnNetwork(eve);
            });
            Socket2.OnEvent("connectError", (SocketEvent socketEvent) =>
            {
                Debugger.Log("socket2 connectError");
                SdkUtil.UnityLog("socket2 connectError");
                if (!SdkStatus.IsInited()) return;
                if (string.IsNullOrEmpty(Socket2.Url)) return;
                var eve = new ResponseEvent(ErrCode.EcSdkSocketError, "Socket 错误", null, null);
                Sdk.BstCallbacks.OnNetwork(eve);
            });

            // 需要自动登录
            Socket1.OnEvent("autoAuth", (SocketEvent socketEvent) =>
            {
                if (!SdkStatus.IsInited()) return;

                var isLogout = UserStatus.IsStatus(UserStatus.StatusType.Logout);
                if (!string.IsNullOrEmpty(Socket1.Url) && isLogout)
                {
                    //UserUtil.Login(null);
                };
            });
            Socket2.OnEvent("autoAuth", (SocketEvent socketEvent) =>
            {
                if (!SdkStatus.IsInited()) return;
                if (string.IsNullOrEmpty(Socket2.Url)) return;

                // Debugger.Log("auto auth check 1");
                // 检查是否需要重登录
                //if (UserStatus.IsStatus(UserStatus.StatusType.Logout)) UserUtil.Login(null);

                // 检查是否需要 checkLogin
                var info = FrameSender.RoomInfo ?? new RoomInfo { RouteId = "" };
                // Debugger.Log("auto auth check 2: {0}", CheckLoginStatus.GetRouteId() != info.RouteId);

                if (CheckLoginStatus.IsOffline() || CheckLoginStatus.GetRouteId() != info.RouteId)
                {
                    //FrameSender.CheckLogin((ResponseEvent eve) =>
                    //{
                    //    if (eve.Code == ErrCode.EcOk)
                    //    {
                    //        Pinger2.Ping(null);
                    //    }
                    //}, "autoAuth");
                }
            });

            // 心跳发包
            Socket1.OnEvent("pingSend", (SocketEvent socketEvent) =>
            {
                if (!SdkStatus.IsInited()) return;

                if (!string.IsNullOrEmpty(Socket1.Url))
                {
                    var eve = new ResponseEvent(ErrCode.EcOk) { Data = Socket1.Id, Msg = socketEvent.Tag };
                    Sdk.BstCallbacks.OnNetwork(eve);
                }
            });


            // 心跳回包正常
            Socket1.OnEvent("pongResposne", (SocketEvent socketEvent) =>
            {
                if (!SdkStatus.IsInited()) return;

                if (!string.IsNullOrEmpty(Socket1.Url))
                {
                    var eve = new ResponseEvent(ErrCode.EcOk) { Data = Socket1.Id, Msg = socketEvent.Tag };
                    Sdk.BstCallbacks.OnNetwork(eve);
                }
            });

            // 心跳回包超时
            Socket1.OnEvent("pongTimeout", (SocketEvent socketEvent) =>
            {
                if (!SdkStatus.IsInited()) return;

                if (!string.IsNullOrEmpty(Socket1.Url))
                {
                    var eve = new ResponseEvent(ErrCode.EcTimeOut) { Data = Socket1.Id, Msg = socketEvent.Tag };
                    Sdk.BstCallbacks.OnNetwork(eve);
                }
            });
        }

        // 初始化回调函数
        public static void SdkInitCallback(bool success, ResponseEvent eve)
        {
            // 修改Sdk状:
            if (!SdkStatus.IsIniting()) return;
            // 初始化成功
            if (success) SdkStatus.SetStatus(SdkStatus.StatusType.Inited);

            //  初始化失败
            if (!success) SdkStatus.SetStatus(SdkStatus.StatusType.Uninit);

            // 回调
            var code = SdkStatus.IsInited() ? ErrCode.EcOk : ErrCode.EcSdkUninit;
            if (!success && eve != null && eve.Code != ErrCode.EcOk)
            {
                code = eve.Code;
            }

            // 错误信息
            var msg = SdkStatus.IsInited() ? "初始化成功" : "初始化失败";
                
            // 服务器时间戳
            var initRsp = (InitRsp)eve?.Data ?? null;
            ulong serverTime = initRsp?.ServerTime ?? 0;

            var e = new ResponseEvent(code, msg, null, new InitRsp(serverTime));
            
            Sdk.Instance.InitRsp(e);
            if(!SdkStatus.IsInited()) Sdk.Uninit(); 
        }
    }
}
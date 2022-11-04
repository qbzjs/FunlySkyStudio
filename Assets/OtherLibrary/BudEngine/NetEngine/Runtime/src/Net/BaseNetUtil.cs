using System;
using System.Collections.Generic;
using Google.Protobuf;

using BudEngine.NetEngine.src.Net.Sockets;
using BudEngine.NetEngine.src.Util;

namespace BudEngine.NetEngine.src.Net
{
    public struct QueueRequest
    {
        public Object Body { get; set; }

        public int Subcmd { get; set; }

        public Action<ResponseEvent> Completed { get; set; }

        public string RequestCmd { get; set; }

        public bool Running { get; set; }

        public NetResponseCallback Response { get; set; }

        public Action BeforeRequest { get; set; }

        public Action<string> AfterRequest { get; set; }
    }

    public class BaseNetUtil
    {

        private static HashSet<ClientSendServerReqCmd> _roomCmd;
        private static Queue<QueueRequest> _checkLoginQueue;
        private static Queue<QueueRequest> _roomQueue;

        public static void StartQueueLoop()
        {
            BaseNetUtil._checkLoginQueue = new Queue<QueueRequest>();
            BaseNetUtil._roomQueue = new Queue<QueueRequest>();
            BaseNetUtil._roomCmd = new HashSet<ClientSendServerReqCmd>
            {
                ClientSendServerReqCmd.ECmdCreateRoomReq,
                ClientSendServerReqCmd.ECmdJoinRoomReq,
                ClientSendServerReqCmd.ECmdQuitRoomReq,
                ClientSendServerReqCmd.ECmdDissmissRoomReq,
                ClientSendServerReqCmd.ECmdRemoveMemberReq,
                ClientSendServerReqCmd.ECmdStartFrameSyncReq,
                ClientSendServerReqCmd.ECmdStopFrameSyncReq,
                ClientSendServerReqCmd.ECmdGetGameInfoReq,
                ClientSendServerReqCmd.ECmdSendGameDataReq,
            };
            // 创建房间
            // 加入房间
            // 离开房间
            // 解散房间
            // 房间变更
            // 移除房间内玩家
            // 修改用户状态
            // 开始帧同步
            // 停止帧同步
            Net.StartQueueLoop();
        }

        public static void StopQueueLoop()
        {
            BaseNetUtil._checkLoginQueue = new Queue<QueueRequest>();
            BaseNetUtil._roomQueue = new Queue<QueueRequest>();
            BaseNetUtil._checkLoginQueue.Clear();
            BaseNetUtil._roomQueue.Clear();
            Net.StopQueueLoop();
        }

        public readonly NetClient client;
        private readonly NetServer _server;
        protected readonly BstCallbacks bstCallbacks;

        //protected Channel channel;
        //protected RoomService.RoomServiceClient clientRpc;
        //protected string openId;
        //protected string playerId;
        //protected string sessionId;
        //protected string roomId;
        //protected string roomType;
        //protected string roomName;

        public BaseNetUtil(BstCallbacks bstCallbacks)
        {
            this.bstCallbacks = bstCallbacks;
            client = new NetClient(bstCallbacks);
            _server = new NetServer();

            //init();
        }

        //private void init()
        //{
        //    channel = new Channel("127.0.0.1:52121", ChannelCredentials.Insecure);
        //    clientRpc = new RoomService.RoomServiceClient(channel);

        //    openId = "1005";
        //    playerId = openId;
        //    sessionId = "qcs::gse:ap-shanghai:uin/100006590487:gameserversession/fleet-qp3g3caa-dweqcm2g/gssess-r4rpkpb2-ilq2pwdu";
        //    roomId = "123456789";
        //    roomType = "PDIYMap:1278217188603334656_761_1627883426_1627901825" + "|" + 1;
        //    roomName = "private_room,";
        //}

        public void BindSocket(Socket socket)
        {
            void HandleResponse(byte[] data) => client.HandleMessage(data);

            void HandleBroadcast(byte[] data) => NetServer.HandleMessage(data);

            client.BindSocket(socket, HandleResponse, HandleBroadcast);
            _server.BindSocket(socket, HandleResponse, HandleBroadcast);
        }
        public void UnbindSocket()
        {
            client.UnbindSocket();
            _server.UnbindSocket();
        }

        public void SetBroadcastHandler(ServerSendClientBstType type, BroadcastCallback handler)
        {
            _server.SetBroadcastHandler(type, handler);
        }

        public string Send(Object body, int subcmd, NetResponseCallback response, Action<ResponseEvent> callback)
        {
            // 第一层 cmd：通用连接 | 帧同步连接
            var requestCmd = "comm_cmd";

            if (client.Socket.Id == (int)ConnectionType.Relay)
            {
                requestCmd = "relay_cmd";
            }

            var queRequest = new QueueRequest
            {
                Body = body,
                Subcmd = (int) subcmd,
                Completed = callback,
                RequestCmd = requestCmd,
                Running = false,
                Response = response
            };

            // CheckLogin 队列化
            //if (subcmd == (int)ClientSendServerReqCmd.ECmdCheckLoginReq)
            //{
            //    queRequest.BeforeRequest = () =>
            //    {   
            //        CheckLoginStatus.SetStatus(CheckLoginStatus.StatusType.Checking);
            //    };
            //    queRequest.AfterRequest = (seq) =>
            //    {
            //        // Debugger.Log("CHECKLOGIN", seq);
            //    };
            //};

            // 房间操作队列化
            var queue = BaseNetUtil._roomCmd.Contains((ClientSendServerReqCmd)subcmd) ? BaseNetUtil._roomQueue : BaseNetUtil._checkLoginQueue;
            return queue.Count == 0 ? SendRequest(queRequest) : PushRequest(queRequest, queue);
        }
        private string SendRequest(QueueRequest queRequest)
        {
            queRequest.Running = true;
            queRequest.BeforeRequest?.Invoke();
            var seq = client.SendRequest(queRequest.Body, queRequest.Subcmd, queRequest.Response, queRequest.Completed, queRequest.RequestCmd, "");

            queRequest.AfterRequest?.Invoke(seq);

            return seq;
        }

        private string PushRequest(QueueRequest queRequest, Queue<QueueRequest> queue)
        {
            var callback = queRequest.Completed;
            Action<ResponseEvent> requestCompleted = (ResponseEvent seq) =>
            {
                callback(seq);
                queRequest.Running = false;
                queue.Dequeue();
                QueueLoop(queue);
            };
            queRequest.Completed = requestCompleted;
            queue.Enqueue(queRequest);
            return QueueLoop(queue);
        }

        private string QueueLoop(Queue<QueueRequest> queue)
        {
            if (queue.Count == 0 || queue.Peek().Running)
            {
                return "NO_SEQ";
            }
            var queRequest = queue.Peek();
            return SendRequest(queRequest);
        }

    }
}
using System;
using Google.Protobuf;

using BudEngine.NetEngine.src.Net;
using BudEngine.NetEngine.src.Util;

namespace BudEngine.NetEngine.src.Sender
{
    public class Sender : BaseNetUtil
    {
        private const ServerSendClientBstType _messageBroadcastType = ServerSendClientBstType.EPushTypeRoomChat;

        public Sender(BstCallbacks bstCallbacks) : base(bstCallbacks)
        {
            var bst = new BroadcastCallback(OnRecvFromClient);
            SetBroadcastHandler(_messageBroadcastType, bst);
        }

        ///////////////////////////////// 请求 //////////////////////////////////
        // 发送消息
        public string SendMessage(SendToClientReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ClientSendServerReqCmd.ECmdRoomChatReq;
            var response = new NetResponseCallback(SendMessageResponse);
            var seq = Send(para, subcmd, SendMessageResponse, callback);
            Debugger.Log("SendMessage_Para {0} {1}", para, seq);
            return seq;
        }

        ///////////////////////////////// 响应 //////////////////////////////////
        // 发送消息
        private void SendMessageResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {

            var rspPacket = res.RspPacket;
            var eve = new ResponseEvent(rspPacket.ErrCode, rspPacket.ErrMsg, rspPacket.Seq, res.Body);
            Debugger.Log("SendMessageResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }

        ///////////////////////////////// 广播 //////////////////////////////////
        // 收到普通消息
        private void OnRecvFromClient(DecodeBstResult res, string seq)
        {
            var bst = (RecvFromClientBst)res.Body;
            var eve = new BroadcastEvent(bst, seq);
            Debugger.Log("OnRecvFromClient {0}", eve);
            var roomId = bst.RoomId;
            this.bstCallbacks.Room.OnRecvFromClient(roomId, eve);
            return;
        }
    }
}
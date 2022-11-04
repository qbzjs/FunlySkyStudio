using System;
using System.Collections.Generic;
using Google.Protobuf;
using GooglePB = global::Google.Protobuf;

namespace BudEngine.NetEngine.src.Util {
    public struct DecodeBstResult {
        public ServerSendClientBst BstPacket { get; set; }

        public object Body { get; set; }
    }

    public struct DecodeRspResult {
        // public byte[] body;
        public DecodeRspResult (ClientSendServerRsp packet, object data) : this()
        {
            RspPacket = packet;
            Body = data;
        }

        public ClientSendServerRsp RspPacket { get; set; }

        public object Body { get; set; }
    }

    public class Pb {
        
        public static Dictionary<int, Func<ByteString, object>> rspDic = new Dictionary<int, Func<ByteString, object>> ();
        public static Dictionary<int, Func<ByteString, object>> bstDic = new Dictionary<int, Func<ByteString, object>> ();

        public static void Init () {

            if (rspDic.Count + bstDic.Count != 0)
            {
                return;
            }
            
            // 设置解包方法
            ///////////////////////////// 响应 /////////////////////////////
            rspDic.Add((int) ClientSendServerReqCmd.ECmdRelayClientSendtoGamesvrReq, (data) => convert(data, new SendToGameSvrRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdRelaySendFrameReq, (data) => convert(data, new SendFrameRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdRoomChatReq, (data) => convert(data, new SendToClientRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdRelayRequestFrameReq, (data) => convert(data, new RequestFrameRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdStartFrameSyncReq, (data) => convert(data, new StartFrameSyncRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdStopFrameSyncReq, (data) => convert(data, new StopFrameSyncRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdCreateRoomReq, (data) => convert(data, new CreateRoomRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdJoinRoomReq, (data) => convert(data, new JoinRoomRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdEnterRoomReq, (data) => convert(data, new EnterRoomRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdQuitRoomReq, (data) => convert(data, new LeaveRoomRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdDissmissRoomReq, (data) => convert(data, new DismissRoomRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdRemoveMemberReq, (data) => convert(data, new RemovePlayerRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdGetRoomDetailReq, (data) => convert(data, new GetRoomByRoomIdRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdHeartBeatReq, (data) => convert(data, new HeartBeatRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdMatchRoomSimpleReq, (data) => convert(data, new MatchRoomSimpleRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdGetGameInfoReq, (data) => convert(data, new GetGameInfoRsp()));
            rspDic.Add((int) ClientSendServerReqCmd.ECmdSendGameDataReq, (data) => convert(data, new SendGameDataRsp()));
           
            ///////////////////////////// 广播 /////////////////////////////
            bstDic.Add((int) ServerSendClientBstType.EPushTypeGamesvr, (data) => convert(data, new RecvFromGameSvrBst()));
            bstDic.Add((int) ServerSendClientBstType.EPushTypeRoomChat, (data) => convert(data, new RecvFromClientBst()));
            bstDic.Add((int) ServerSendClientBstType.EPushTypeStartGame, (data) => convert(data, new StartFrameSyncBst()));
            bstDic.Add((int) ServerSendClientBstType.EPushTypeStopGame, (data) => convert(data, new StopFrameSyncBst()));
            bstDic.Add((int) ServerSendClientBstType.EPushTypeRelay, (data) => convert(data, new RecvFrameBst()));
            bstDic.Add((int) ServerSendClientBstType.EPushTypeJoinRoom, (data) => convert(data, new JoinRoomBst()));
            bstDic.Add((int) ServerSendClientBstType.EPushTypeLeaveRoom, (data) => convert(data, new LeaveRoomBst()));
            bstDic.Add((int) ServerSendClientBstType.EPushTypeDismissRoom, (data) => convert(data, new DismissRoomBst()));
            bstDic.Add((int) ServerSendClientBstType.EPushTypeRemovePlayer, (data) => convert(data, new RemovePlayerBst()));
            bstDic.Add((int) ServerSendClientBstType.EPushTypeNetworkState, (data) => convert(data, new ChangePlayerNetworkStateBst()));
            bstDic.Add((int) ServerSendClientBstType.EPushTypeGame, (data) => convert(data, new SendGameBst()));
        }

        private static object convert(ByteString data, GooglePB::IMessage tmp)
        {
            tmp.MergeFrom ((ByteString) data);
            return tmp;
        }

        public static byte[] EncodeReq (ClientSendServerReq packet, GooglePB::IMessage data) {
            packet.Body = data.ToByteString();
            //var bb = new Byte[1];
            //bb[0] = 1;
            //packet.Body = ByteString.CopyFrom(bb);
            //byte[] bb = new byte[] { 34, 222, 2, 10, 219, 2, 10, 9, 49, 50, 51, 52, 53, 54, 55, 56, 57, 18, 13, 112, 114, 105, 118, 97, 116, 101, 95, 114, 111, 111, 109, 44, 26, 55, 80, 68, 73, 89, 77, 97, 112, 58, 49, 50, 55, 56, 50, 49, 55, 49, 56, 56, 54, 48, 51, 51, 51, 52, 54, 53, 54, 95, 55, 54, 49, 95, 49, 54, 50, 55, 56, 56, 51, 52, 50, 54, 95, 49, 54, 50, 55, 57, 48, 49, 56, 50, 53, 124, 49, 32, 8, 48, 1, 66, 124, 18, 6, 98, 117, 100, 95, 54, 53, 32, 1, 42, 112, 123, 34, 117, 105, 100, 34, 58, 34, 49, 52, 48, 57, 56, 55, 48, 49, 48, 55, 56, 56, 54, 50, 51, 49, 53, 53, 51, 34, 44, 34, 105, 109, 97, 103, 101, 74, 115, 111, 110, 34, 58, 34, 34, 44, 34, 103, 101, 110, 100, 101, 114, 34, 58, 48, 44, 34, 110, 105, 99, 107, 34, 58, 34, 49, 50, 51, 34, 44, 34, 104, 101, 97, 100, 85, 114, 108, 34, 58, 34, 49, 50, 51, 34, 44, 34, 105, 109, 97, 103, 101, 73, 100, 34, 58, 48, 44, 34, 105, 110, 105, 116, 105, 97, 108, 105, 122, 101, 100, 34, 58, 48, 125, 66, 124, 18, 6, 98, 117, 100, 95, 54, 53, 32, 1, 42, 112, 123, 34, 117, 105, 100, 34, 58, 34, 49, 52, 48, 57, 56, 55, 48, 49, 48, 55, 56, 56, 54, 50, 51, 49, 53, 53, 51, 34, 44, 34, 105, 109, 97, 103, 101, 74, 115, 111, 110, 34, 58, 34, 34, 44, 34, 103, 101, 110, 100, 101, 114, 34, 58, 48, 44, 34, 110, 105, 99, 107, 34, 58, 34, 49, 50, 51, 34, 44, 34, 104, 101, 97, 100, 85, 114, 108, 34, 58, 34, 49, 50, 51, 34, 44, 34, 105, 109, 97, 103, 101, 73, 100, 34, 58, 48, 44, 34, 105, 110, 105, 116, 105, 97, 108, 105, 122, 101, 100, 34, 58, 48, 125, 80, 15, 96, 225, 232, 163, 139, 6 };

            //var packett = new ClientSendServerRsp();
            //packett.MergeFrom(bb);

            //var tmp = new CreateRoomRsp();
            //tmp.MergeFrom((ByteString)packett.Body);

            return packet.ToByteArray ();
        }

        public static DecodeRspResult DecodeRsp (byte[] data, Func<string, int> getReqCmd) {
            var packet = new ClientSendServerRsp ();

            //byte[] bb = new byte[]{34, 222, 2, 10, 219, 2, 10, 9, 49, 50, 51, 52, 53, 54, 55, 56, 57, 18, 13, 112, 114, 105, 118, 97, 116, 101, 95, 114, 111, 111, 109, 44, 26, 55, 80, 68, 73, 89, 77, 97, 112, 58, 49, 50, 55, 56, 50, 49, 55, 49, 56, 56, 54, 48, 51, 51, 51, 52, 54, 53, 54, 95, 55, 54, 49, 95, 49, 54, 50, 55, 56, 56, 51, 52, 50, 54, 95, 49, 54, 50, 55, 57, 48, 49, 56, 50, 53, 124, 49, 32, 8, 48, 1, 66, 124, 18, 6, 98, 117, 100, 95, 54, 53, 32, 1, 42, 112, 123, 34, 117, 105, 100, 34, 58, 34, 49, 52, 48, 57, 56, 55, 48, 49, 48, 55, 56, 56, 54, 50, 51, 49, 53, 53, 51, 34, 44, 34, 105, 109, 97, 103, 101, 74, 115, 111, 110, 34, 58, 34, 34, 44, 34, 103, 101, 110, 100, 101, 114, 34, 58, 48, 44, 34, 110, 105, 99, 107, 34, 58, 34, 49, 50, 51, 34, 44, 34, 104, 101, 97, 100, 85, 114, 108, 34, 58, 34, 49, 50, 51, 34, 44, 34, 105, 109, 97, 103, 101, 73, 100, 34, 58, 48, 44, 34, 105, 110, 105, 116, 105, 97, 108, 105, 122, 101, 100, 34, 58, 48, 125, 66, 124, 18, 6, 98, 117, 100, 95, 54, 53, 32, 1, 42, 112, 123, 34, 117, 105, 100, 34, 58, 34, 49, 52, 48, 57, 56, 55, 48, 49, 48, 55, 56, 56, 54, 50, 51, 49, 53, 53, 51, 34, 44, 34, 105, 109, 97, 103, 101, 74, 115, 111, 110, 34, 58, 34, 34, 44, 34, 103, 101, 110, 100, 101, 114, 34, 58, 48, 44, 34, 110, 105, 99, 107, 34, 58, 34, 49, 50, 51, 34, 44, 34, 104, 101, 97, 100, 85, 114, 108, 34, 58, 34, 49, 50, 51, 34, 44, 34, 105, 109, 97, 103, 101, 73, 100, 34, 58, 48, 44, 34, 105, 110, 105, 116, 105, 97, 108, 105, 122, 101, 100, 34, 58, 48, 125, 80, 15, 96, 225, 232, 163, 139, 6};

            packet.MergeFrom (data);
            
            object rsp = null;
            int cmd = getReqCmd(packet.Seq);

            if (cmd > 0 && rspDic.ContainsKey(cmd) && packet.Body != null)
            {
                Func<ByteString,object> func = null;
                rspDic.TryGetValue(cmd, out func);

                if (func != null)
                {
                    rsp = func(packet.Body);
                }
            }

            var rspResult = new DecodeRspResult {
                RspPacket = new ClientSendServerRsp (packet),
                Body = rsp
            };
            return rspResult;
        }

        public static DecodeBstResult DecodeBst (byte[] data) {
            var packet = new ServerSendClientBst ();
            packet.MergeFrom (data);

            object rsp =null;

            if (bstDic.ContainsKey((int)packet.Type))
            {
                Func<ByteString,object> func = null;
                bstDic.TryGetValue((int)packet.Type, out func); 
                
                if (func != null)
                {
                    rsp = func(packet.Body);
                }
            }

            return new DecodeBstResult {
                BstPacket = packet,
                Body = rsp
            };
        }
    }
}
/// <summary>
/// Author:MeiMei—LiMei
/// Description: 聊天窗口管理，负责接收聊天窗口断线重连消息数据
/// Date: 2022-02-23
/// </summary>
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatPanelManager : CInstance<ChatPanelManager>
{
    public int lastChatId = -1;
    public void UpdataeLastChatId(int id)
    {
        if (id > 0)
        {
            this.lastChatId = id;
        }
    }
    public void OnGetMsgCallback(string dataJson)
    {
        LoggerUtils.Log("===========RoomChatPanel===>OnGetMsgs:" + dataJson);
        if (!string.IsNullOrEmpty(dataJson))
        {
            GetMsgsRsp getMsgsRsp = JsonConvert.DeserializeObject<GetMsgsRsp>(dataJson);
            if (getMsgsRsp == null)
            {
                return;
            }
            if (getMsgsRsp.chatMsgs == null)
            {
                return;
            }
            RoomChatPanel.Instance.GetChatMsgData(getMsgsRsp);
        }
    }
    public void SendGetMsgs()
    {
        if (this.lastChatId <= 0)
        {
            return;
        }
        GetMsgsReq getMsgsReq = new GetMsgsReq
        {
            lastChatId = this.lastChatId,
        };
        LoggerUtils.Log("=====>SendGetMsgslastChatID" + this.lastChatId);
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.GetMsgs,
            data = JsonConvert.SerializeObject(getMsgsReq),
        };
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }
}

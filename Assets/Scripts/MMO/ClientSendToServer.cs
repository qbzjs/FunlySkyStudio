using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Date:2022-03-30 23:25:27
/// Author:Shaocheng
/// Desc:客户端发送非业务数据给服务器，例如上报当前切后台状态等
/// </summary>
public partial class ClientManager : MonoBehaviour
{
    /// <summary>
    /// 客户端发送非业务数据给服务器
    /// </summary>
    /// <param name="data"></param>
    public void ClientSendMsgToServer(object data)
    {
        SendToGameSvrPara p = new SendToGameSvrPara()
        {
            Data = data
        };

        if(Global.Room == null)
        {
            return;
        }

        Global.Room.ClientSendToServer(p, (eve) =>
        {
            if (eve.Code == 0)
            {
                LoggerUtils.Log($"ClientSendMsgToServer Success: Data:{eve.Data}");
            }
            else
            {
                LoggerUtils.Log($"ClientSendMsgToServer Error: {eve.Code} {eve.Msg}");
            }
        });
    }

    /// <summary>
    /// 切后台上报
    /// </summary>
    public void SendBackgroundAction()
    {
        PlayerStateMsg s = new PlayerStateMsg()
        {
            BackgroundState = 1
        };
        ClientSendMsgToServer(s);
        LoggerUtils.Log($"SendBackgroundAction :{s}");
    }

    /// <summary>
    /// 切前台上报
    /// </summary>
    public void SendForegroundAction()
    {
        PlayerStateMsg s = new PlayerStateMsg()
        {
            BackgroundState = 0
        };
        ClientSendMsgToServer(s);
        LoggerUtils.Log($"SendForegroundAction :{s}");
    }
}
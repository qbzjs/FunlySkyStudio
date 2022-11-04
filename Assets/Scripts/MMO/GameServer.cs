using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameServer
{
    private static GameServer _instance;
    private static object lockObj = new object();
    public static GameServer Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (lockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new GameServer();
                    }   
                }
            }
            return _instance;
        }
    }
    private Dictionary<int, Action<ServerPacket>> resqDict = new Dictionary<int, Action<ServerPacket>>();

    public void AddGameResq(ProtoCommand type, Action<ServerPacket> callback)
    {
        if (resqDict.ContainsKey((int)type))
        {
            LoggerUtils.Log("Add Same CallBack");
        }
        else
        {
            resqDict.Add((int)type, callback);
        }
    }

    public Action<ServerPacket> GetGameResq(int type)
    {
        if (resqDict.ContainsKey(type))
        {
            return resqDict[type];
        }

        return null;
    }

    public static void  Release()
    {
        _instance = null;
    }
}
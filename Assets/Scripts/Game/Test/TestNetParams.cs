using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
/// <summary>
/// Author:Shaocheng
/// Description:本地测试工具-联机测试使用
/// Date: 2022-3-30 19:43:08
/// </summary>
public class TestNetParams : CInstance<TestNetParams>
{
#if UNITY_EDITOR

    [NonSerialized] public static string TestConfigsDir = Application.dataPath + Path.DirectorySeparatorChar + "TestConfigs" + Path.DirectorySeparatorChar;
    [NonSerialized] public string TestNetConfigPath = TestConfigsDir + "TestNetConfig.json";

    public enum TestNetType
    {
        Master = 0, //Master环境
        Alpha = 1, //Alpha环境
        Local = 2, //本地服环境
    }

    public class HttpTestHeader
    {
        public string uid;
        public string baseUrl;
        public string environment;
        public string device;
        public string platform;
        public string generation;
        public string token;
        public string locale;
        public string lang;
        public string version;
        public string walletAddress;
        public string timezone;
    }

    public class LocalServerConfig
    {
        public string ipAddr; //Ip + port格式
    }

    private static GetSessionReq _getSessionReq;


    public class TestNetConfig
    {
        public bool isOpenNetTest; //是否开启联机测试,默认False,False-试玩模式后则不会启动联机引擎
        public bool isOpenDebuggerLog; //是否开启联机引擎Debug
        public bool isSaveLog; //是否保存联机日志

        public TestNetType testEnvironment; //当前测试环境-- Master/Local
        public string testMapId; //测试地图ID,多端保证MapId和Version一致才能匹配到同一房间

        public int isPrivate; //是否是公共房间 0-公共 1-私人
        public string roomCode; //房间码

        public int cosPlayerIndex; //表示当前Unity端扮演哪个Player,值为testHeader的下标

        public LocalServerConfig localserverConfig; // 本地服配置
        public HttpTestHeader[] testHeaders; //当前Unity端的请求头，用于拉取角色信息
    }

    public void ForceSetNetConfig()
    {
        CurrentConfig = new TestNetConfig()
        {
            isOpenNetTest = true,
            isOpenDebuggerLog = true,
            testEnvironment = TestNetType.Master,
            testMapId = "123",

            localserverConfig = new LocalServerConfig()
            {
                ipAddr = "54.153.123.237:9003",
            },

            testHeaders = new[]
            {
                new HttpTestHeader()
                {
                    uid = "1476091808982634496",
                    baseUrl = "123",
                    environment = "master",
                    device = "123",
                    platform = "U3D",
                    generation = "123",
                    token =
                        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjQ3OTQzNjI2ODYsImlhdCI6MTY0MDc2MjY4NiwibmJmIjoxNjQwNzYyNjg2LCJ1aWQiOiIxNDc2MDkxODA4OTgyNjM0NDk2In0.kOFjTBMEHXSAOLHz7qLtU5scOrywy7WDd5-G3AZMqPY",
                    version = "1.12.0",
                },
                new HttpTestHeader()
                {
                    uid = "1476470202995970048",
                    baseUrl = "123",
                    environment = "master",
                    device = "123",
                    platform = "U3D",
                    generation = "123",
                    token =
                        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjQ3OTQ0NTI5MDIsImlhdCI6MTY0MDg1MjkwMiwibmJmIjoxNjQwODUyOTAyLCJ1aWQiOiIxNDc2NDcwMjAyOTk1OTcwMDQ4In0.NASta00MXlGFrB9kuqeAsIo6HXooewaL3YuoVoSM4Sw",
                    version = "1.12.0",
                },
                new HttpTestHeader()
                {
                    uid = "1478556941902286848",
                    baseUrl = "123",
                    environment = "master",
                    device = "123",
                    platform = "U3D",
                    generation = "123",
                    token =
                        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjQ3OTQ5NjQ2OTIsImlhdCI6MTY0MTM2NDY5MiwibmJmIjoxNjQxMzY0NjkyLCJ1aWQiOiIxNDc4NTU2OTQxOTAyMjg2ODQ4In0.msD7rACGiZE1_oDNr1RN8r0RPEBkhbjLgkHlLQFDUVw",
                    version = "1.12.0",
                },
                new HttpTestHeader()
                {
                    uid = "1448854981612187648",
                    baseUrl = "123",
                    environment = "master",
                    device = "123",
                    platform = "U3D",
                    generation = "123",
                    token =
                        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjQ3OTQ5NjQ4OTQsImlhdCI6MTY0MTM2NDg5NCwibmJmIjoxNjQxMzY0ODk0LCJ1aWQiOiIxNDQ4ODU0OTgxNjEyMTg3NjQ4In0.0-K3ed3lw4Vk0EcTqhAUNJ23YW7lItknm3O5g66KzxA",
                    version = "1.12.0",
                },
            }
        };
    }

    public TestNetConfig CurrentConfig;
    public static HttpTestHeader testHeader;

    public void CheckConfigDir()
    {
        if (!Directory.Exists(TestConfigsDir))
        {
            Directory.CreateDirectory(TestConfigsDir);
        }
    }

    public void LoadConfig()
    {
        if (File.Exists(TestNetConfigPath))
        {
            string jsonStr = File.ReadAllText(TestNetConfigPath);
            LoggerUtils.Log("读取联机测试配置：" + jsonStr);

            try
            {
                CurrentConfig = JsonConvert.DeserializeObject<TestNetConfig>(jsonStr);
                if (CurrentConfig != null)
                {
                    testHeader = CurrentConfig.testHeaders[CurrentConfig.cosPlayerIndex];

                    if (GameManager.Inst.ugcUserInfo == null)
                    {
                        GameManager.Inst.ugcUserInfo = new UserInfo()
                        {
                            uid = testHeader.uid
                        };
                    }
                }
            }
            catch (Exception e)
            {
                LoggerUtils.Log("读取联机测试配置失败:" + e.ToString());
                throw;
            }
        }
        else
        {
            CurrentConfig = new TestNetConfig();
            LoggerUtils.Log(TestNetConfigPath + "不存在，请检查配置文件！");
        }
    }

    public void OpenTestNetConfig()
    {
        Application.OpenURL(TestNetConfigPath);
    }

    public void SaveConfigJson()
    {
        CheckConfigDir();
        // ForceSetNetConfig();
        if (CurrentConfig != null)
        {
            string jsonStr = GameUtils.ConvertStringToFormatJson(JsonConvert.SerializeObject(CurrentConfig));
            File.WriteAllText(TestNetConfigPath, jsonStr, Encoding.UTF8);
            LoggerUtils.Log("Save TestNetConfig File Finish~");
        }
        else
        {
            LoggerUtils.Log("CurrentConfig is null , save failed~");
        }
    }

    public string GetMakeHttpUrl()
    {
        switch (CurrentConfig.testEnvironment)
        {
            case TestNetType.Master: return "https://api-test.joinbudapp.com";
            case TestNetType.Alpha: return "https://api-alpha.joinbudapp.com";
            case TestNetType.Local: return "http://" + CurrentConfig.localserverConfig.ipAddr;
            default: return string.Empty;
        }
    }

    public string GetConnectIp()
    {
        if (CurrentConfig.testEnvironment == TestNetType.Local)
        {
            if (CurrentConfig.localserverConfig.ipAddr.Contains(":"))
            {
                string[] sp = CurrentConfig.localserverConfig.ipAddr.Split(':');
                if (sp.Length > 1)
                {
                    return sp[0];
                }
            }
            else
            {
                return CurrentConfig.localserverConfig.ipAddr;
            }
        }

        return string.Empty;
    }

    public string GetTestHeaderEvir()
    {
        switch (CurrentConfig.testEnvironment)
        {
            case TestNetType.Master: return "master";
            case TestNetType.Alpha: return "pr";
            case TestNetType.Local: return "master";
            default: return string.Empty;
        }
    }
#endif
}
using RedDot;
using SavingData;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using BasePrimitiveRedDotSystem;
using AvatarRedDotSystem;

public class AvatarRedDotManager
{
    public RedDotSystem mRedDotSystem;
    public VNode mVNode;
    public RoleMenuView mPanel;
    public List<AvatarRedDots> mAvatarRedInfos;//后端发来有红点的PrimitiveId;
    private bool mIsInited = false;
    private Action<bool> mRedDotSystemInitedFunc;
    public AvatarRedDotManager(RoleMenuView panel)
    {
        mAvatarRedInfos = new List<AvatarRedDots>();
        mPanel = panel;
    }
    public bool IsInited => mIsInited;
    public class RedDot
    {
        public int operationType;//0 清理红点，1 清理new
        public int resourceKind;
        public string resourceId;
    }
    public void ConstructRedDotSystem()
    {
        RedDotSystemContext context = new RedDotSystemContext();
        context.SetRedDotTreeConstructerType<AvatarRedDotTreeConstructer>();
        context.SetNodeFactoryType<AvatarRedDotNodeFactory>();

        context.mSystemName = "Avatar";
        mRedDotSystem = mPanel.mRedDotSystemManager.CreateSystem(ERedDotSystemType.Avatar, context);
    }
    public void GetAvatarRedInfo()
    {
        LoggerUtils.Log("GetAvatarRedInfo");
        HttpUtils.MakeHttpRequest("/other/resourceRedDot", (int)HTTP_METHOD.GET, "", GetRedInfoSuccess, GetRedInfoFail);
    }
    public void GetRedInfoSuccess(string msg)
    {
        LoggerUtils.Log("GetAvatarRedInfoMsg:" + msg);
        HttpResponDataStruct responseData = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        AvatarRedInfo avatarRedInfo = JsonConvert.DeserializeObject<AvatarRedInfo>(responseData.data);
        LoggerUtils.Log("content = " + responseData.data);
        if (avatarRedInfo.avatarRedDots != null)
        {
            mAvatarRedInfos = new List<AvatarRedDots>(avatarRedInfo.avatarRedDots);
            LoggerUtils.Log("avatarRedInfosContent = " + mAvatarRedInfos.Count);
            //数据返回，驱动红点创建，这里需要加入监听
            mIsInited = true;
        }
        else
        {
            mIsInited = false;
        }
        mRedDotSystemInitedFunc?.Invoke(mIsInited);

    }
    public void GetRedInfoFail(string msg)
    {
        LoggerUtils.LogError("Script:AvatarRedDotManager GetRedInfoFail error = " + msg);
        mRedDotSystemInitedFunc?.Invoke(false);
    }
    public void ReqCleanRedDot(ClassifyType classifyType)
    {
        ReqCleanRedDot(classifyType.ToString());
    }
    public void ReqCleanRedDot(string type)
    {
        var resourceKinds = new List<string>();
        resourceKinds.Add(type);
        ClearRedDots clearRedDots = new ClearRedDots
        {
            cleanKind = 0,//0 清理红点，1 清理new
            resourceKinds = resourceKinds.ToArray()
        };
        HttpUtils.MakeHttpRequest("/other/cleanResourceRedDot", (int)HTTP_METHOD.POST, JsonUtility.ToJson(clearRedDots),
            ResponeCleanRedDotSuccess,
            ResponeCleanRedDotFail);
    }
    private void ResponeCleanRedDotSuccess(string content)
    {
        LoggerUtils.Log($"-----ResponeCleanRedDotSuccess.Msg={content}");
    }
    private void ResponeCleanRedDotFail(string content)
    {
        LoggerUtils.LogError("Script:AvatarRedDotManager ResponeCleanRedDotFail error = " + content);
    }
    public void AddListener(Action<bool> listener)
    {
        mRedDotSystemInitedFunc += listener;
    }
    public void RemoveListener(Action<bool> listener)
    {
        mRedDotSystemInitedFunc -= listener;
    }
    public RedDotTree Tree => mRedDotSystem.mTree;
}

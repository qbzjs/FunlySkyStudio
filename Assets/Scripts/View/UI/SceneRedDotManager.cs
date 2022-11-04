using System;
using System.Collections.Generic;
using EmoRedDotSystem;
using Newtonsoft.Json;
using RedDot;
using UnityEngine;

public class SceneRedDotManager
{
    public RedDotSystem mEmoRedDotSystem;
    public VNode mVEmoNode;
    public PlayModePanel mPanel;
    public List<int> mEmoIds;//后端发来有红点的EmoId;
    private bool mIsInited = false;
    private Action<bool> mRedDotSystemInitedFunc;
    public SceneRedDotManager(PlayModePanel panel)
    {
        mEmoIds = new List<int>();
        mPanel = panel;
    }
    public bool IsInited => mIsInited;
    public class RedDot
    {
        public int operationType;//0 清理红点，1 清理
        public int resourceKind;
        public string resourceId;
    }
    public class SceneRedDotResponData
    {
        public string[] resourceIds;
    }
    private void ReqSceneRedDotSuccess(string content)
    {
        HttpResponDataStruct json = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        LoggerUtils.Log($"-----sceneRedDot.json.data={content}");
        SceneRedDotResponData data = JsonConvert.DeserializeObject<SceneRedDotResponData>(json.data);
        if (data != null && data.resourceIds != null)
        {
            ConvertToInt(data.resourceIds);
            VNode vNode = AttachEmoRedDotNode();
            vNode.mLogic.ChangeCount(mEmoIds.Count);
            mIsInited = true;
        }
        else
        {
            mIsInited = false;
        }

        mRedDotSystemInitedFunc?.Invoke(mIsInited);
    }

    private void ConvertToInt(string[] ids)
    {
        int len = ids.Length;
        for (int i = 0; i < len; i++)
        {
            string strId = ids[i];
            int id = 0;
            if (int.TryParse(strId, out id))
            {
                mEmoIds.Add(id);
            }
        }
    }
    
    private void ReqSceneRedDotFail(string content)
    {
        LoggerUtils.Log($"-----sceneRedDotFail.Msg={content}");
        mRedDotSystemInitedFunc?.Invoke(false);
    }
    public void ConstructEmoRedDotSystem()
    {
        RedDotSystemContext context = new RedDotSystemContext();
        context.SetRedDotTreeConstructerType<EmoRedDotTreeConstructer>();
        context.SetNodeFactoryType<EmoRedDotNodeFactory>();

        context.mSystemName = "Emo";
        mEmoRedDotSystem = GameManager.Inst.mRedDotSystemManager.CreateSystem(ERedDotSystemType.Emo, context);
    }
    public void RequestEmoRedDot()
    {
        RedDot req = new RedDot
        {
            operationType = (int)ESceneRedDotOptTpye.Pull,
            resourceKind = (int)ESceneRedDotKind.Emo,
            resourceId = "",
        };
        //请求Emo红点
        HttpUtils.MakeHttpRequest("/other/sceneRedDot",
                    (int)HTTP_METHOD.POST, JsonUtility.ToJson(req),
                     ReqSceneRedDotSuccess,
                    ReqSceneRedDotFail);
    }
    public void RequestCleanEmoRedDot(int emoId)
    {
        RedDot req = new RedDot
        {
            operationType = (int)ESceneRedDotOptTpye.Clean,
            resourceKind = (int)ESceneRedDotKind.Emo,
            resourceId = emoId.ToString(),
        };
        string data = JsonUtility.ToJson(req);
        LoggerUtils.Log($"clean_data={data}");
        //请求Emo红点
        HttpUtils.MakeHttpRequest("/other/sceneRedDot",
                    (int)HTTP_METHOD.POST, data,
                     ResponeCleanRedDotSuccess,
                    ResponeCleanRedDotFail);
    }
    private void ResponeCleanRedDotSuccess(string content)
    {
        LoggerUtils.Log($"-----ResponeCleanRedDotSuccess.Msg={content}");
    }
    private void ResponeCleanRedDotFail(string content)
    {
        LoggerUtils.Log($"-----ResponeCleanRedDotFail.Msg={content}");
    }
    public VNode AttachEmoRedDotNode()
    {
        mVEmoNode = mEmoRedDotSystem.mTree.AddRedDot(mPanel.emoBtn.gameObject, (int)EmoRedDotSystem.ENodeType.Root, (int)EmoRedDotSystem.ENodeType.Emo,
        ERedDotPrefabType.Type1);
        mVEmoNode.mLogic.AddListener(RootNodeChangedValueCallBack);
        return mVEmoNode;
    }
    public void RootNodeChangedValueCallBack(int count)
    {

    }
    public void AddListener(Action<bool> listener)
    {
        mRedDotSystemInitedFunc += listener;
    }
    public void RemoveListener(Action<bool> listener)
    {
        mRedDotSystemInitedFunc -= listener;
    }
    public RedDotTree Tree => mEmoRedDotSystem.mTree;

}

using System;
using System.Collections.Generic;
using OperationRedDotSystem;
using Newtonsoft.Json;
using RedDot;
using UnityEngine;

public class OperationRedDotManager
{
    public RedDotSystem mOperationRedDotSystem;
    public VNode mVOptNode;
    public Dictionary<int, VNode> vNodeDict = new Dictionary<int, VNode>();
    public PlayModePanel mPanel;
    public List<int> optBtnIds;//后端发来有红点的操作按钮Id;
    private bool mIsInited = false;
    private Action<bool> mRedDotSystemInitedFunc;
    public OperationRedDotManager(PlayModePanel panel)
    {
        optBtnIds = new List<int>();
        mPanel = panel;
    }
    public bool IsInited => mIsInited;
    public class RedDot
    {
        public int operationType;//0 清理红点，1 清理
        public int resourceKind;
        public string resourceId;
    }
    public class OperationRedDotResponData
    {
        public string[] resourceIds;
    }
    private void ReqSceneRedDotSuccess(string content)
    {
        HttpResponDataStruct json = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        LoggerUtils.Log($"-----optRedDot.json.data={content}");
        OperationRedDotResponData data = JsonConvert.DeserializeObject<OperationRedDotResponData>(json.data);
        if (data != null && data.resourceIds != null)
        {
            ConvertToInt(data.resourceIds);
            for (var i = 0; i < optBtnIds.Count; i++)
            {
                var id = optBtnIds[i];
                // VNode vNode = 
                AttachOptRedDotNode(id);
                // vNode.mLogic.ChangeCount(optBtnIds.Count);
            }

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
                optBtnIds.Add(id);
            }
        }
    }

    private void ReqSceneRedDotFail(string content)
    {
        LoggerUtils.Log($"-----optRedDotFail.Msg={content}");
        mRedDotSystemInitedFunc?.Invoke(false);
    }
    public void ConstructOptRedDotSystem()
    {
        RedDotSystemContext context = new RedDotSystemContext();
        context.SetRedDotTreeConstructerType<OperationRedDotTreeConstructer>();
        context.SetNodeFactoryType<OperationRedDotNodeFactory>();

        context.mSystemName = "Opt";
        mOperationRedDotSystem = GameManager.Inst.mRedDotSystemManager.CreateSystem(ERedDotSystemType.OperationBtn, context);
    }
    public void RequestOptRedDot()
    {
        RedDot req = new RedDot
        {
            operationType = (int)ESceneRedDotOptTpye.Pull,
            resourceKind = (int)ESceneRedDotKind.OperationBtn,
            resourceId = "",
        };
        //请求红点
        HttpUtils.MakeHttpRequest("/other/sceneRedDot",
                    (int)HTTP_METHOD.POST, JsonUtility.ToJson(req),
                     ReqSceneRedDotSuccess,
                    ReqSceneRedDotFail);
    }
    public void RequestCleanOptRedDot(int btnId)
    {
        RedDot req = new RedDot
        {
            operationType = (int)ESceneRedDotOptTpye.Clean,
            resourceKind = (int)ESceneRedDotKind.OperationBtn,
            resourceId = btnId.ToString(),
        };
        string data = JsonUtility.ToJson(req);
        LoggerUtils.Log($"clean_data={data}");
        //请求红点
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
    public VNode AttachOptRedDotNode(int id)
    {
        GameObject target = null;
        int parentType = 0;
        int nodeType = 0;
        switch (id)
        {
            case (int)ENodeType.SelfieMode:
                target = mPanel.cameraModeBtn.gameObject;
                parentType = (int)ENodeType.CameraMode;
                nodeType = (int)ENodeType.SelfieMode;
                break;
            case (int)ENodeType.GraphicsBtn:
                target = mPanel.setingBtn.gameObject;
                parentType = (int)ENodeType.GlobalSetting;
                nodeType = (int)ENodeType.GraphicsBtn;
                break;
            case (int)ENodeType.SoundBtn:
                target = mPanel.setingBtn.gameObject;
                parentType = (int)ENodeType.GlobalSetting;
                nodeType = (int)ENodeType.SoundBtn;
                break;
        }
        var vOptNode = AttachOptLogicNode(target, parentType, nodeType);
        return vOptNode;
    }

    public VNode AttachOptLogicNode(GameObject target, int parentType, int nodeType = 0)
    {
        if (parentType == 0)
        {
            return null;
        }
        VNode vOptNode;
        if (!vNodeDict.TryGetValue(parentType, out vOptNode))
        {
            vOptNode = mOperationRedDotSystem.mTree.AddRedDot(target, (int)OperationRedDotSystem.ENodeType.Root, parentType,
            ERedDotPrefabType.Type4);
            vOptNode.mLogic.AddListener(RootNodeChangedValueCallBack);
            vNodeDict.Add(parentType, vOptNode);
        }
        if (nodeType != 0)
        {
            var mLogic = mOperationRedDotSystem.mTree.AddNode(parentType, nodeType);
            mLogic.ChangeCount(1);
        }

        return vOptNode;
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
    public RedDotTree Tree => mOperationRedDotSystem.mTree;

}

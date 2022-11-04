using System;
using System.Collections.Generic;
using BasePrimitiveRedDotSystem;
using Newtonsoft.Json;
using RedDot;
using UnityEngine;

public class PrimitiveRedDotManager
{
    public RedDotSystem mPrimitiveRedDotSystem;
    public VNode mVNode;
    public BasePrimitivePanel mPanel;
    public List<int> mPrimitiveIds;//后端发来有红点的PrimitiveId;
    private bool mIsInited = false;
    private Action<bool> mRedDotSystemInitedFunc;
    public Dictionary<PrimitiveItemType, ENodeType> NodeTypeDict = new Dictionary<PrimitiveItemType, ENodeType>()
    {
        {PrimitiveItemType.Character, ENodeType.Character},
        {PrimitiveItemType.General, ENodeType.General},
        {PrimitiveItemType.GamePlay, ENodeType.GamePlay},
        {PrimitiveItemType.Scene, ENodeType.Scene},
    };
    public PrimitiveRedDotManager(BasePrimitivePanel panel)
    {
        mPrimitiveIds = new List<int>();
        mPanel = panel;
    }
    public bool IsInited => mIsInited;
    public class RedDot
    {
        public int operationType;//0 清理红点，1 清理new
        public int resourceKind;
        public string resourceId;
    }
    public class PrimitiveRedDotResponData
    {
        public string[] resourceIds;
    }
    private void ReqRedDotSuccess(string content)
    {
        HttpResponDataStruct json = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        LoggerUtils.Log($"-----PrimitiveRedDotResponData.json={content}");
        PrimitiveRedDotResponData data = JsonConvert.DeserializeObject<PrimitiveRedDotResponData>(json.data);
        if (data != null && data.resourceIds != null)
        {
            ConvertToInt(data.resourceIds);
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
                mPrimitiveIds.Add(id);
            }
        }
    }

    private void ReqRedDotFail(string content)
    {
        LoggerUtils.Log($"-----primitiveRedDotFail.Msg={content}");
        mRedDotSystemInitedFunc?.Invoke(false);
    }
    public void ConstructRedDotSystem()
    {
        RedDotSystemContext context = new RedDotSystemContext();
        context.SetRedDotTreeConstructerType<BasePrimitiveRedDotTreeConstructer>();
        context.SetNodeFactoryType<BasePrimitiveRedDotNodeFactory>();

        context.mSystemName = "Primitive";
        mPrimitiveRedDotSystem = GameManager.Inst.mRedDotSystemManager.CreateSystem(ERedDotSystemType.Primitive, context);
    }
    public void RequestRedDot()
    {
        RedDot req = new RedDot
        {
            operationType = (int)ESceneRedDotOptTpye.Pull,
            resourceKind = (int)ESceneRedDotKind.Primitive,
            resourceId = "",
        };
        string data = JsonUtility.ToJson(req);
        LoggerUtils.Log($"-----PrimitiveRedDotManager.Msg={data}");
        HttpUtils.MakeHttpRequest("/other/sceneRedDot",
                    (int)HTTP_METHOD.POST, data,
                     ReqRedDotSuccess,
                    ReqRedDotFail);
    }
    public void RequestCleanRedDot(int id)
    {
        RedDot req = new RedDot
        {
            operationType = (int)ESceneRedDotOptTpye.Clean,
            resourceKind = (int)ESceneRedDotKind.Primitive,
            resourceId = id.ToString(),
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
    public void AddListener(Action<bool> listener)
    {
        mRedDotSystemInitedFunc += listener;
    }
    public void RemoveListener(Action<bool> listener)
    {
        mRedDotSystemInitedFunc -= listener;
    }
    public RedDotTree Tree => mPrimitiveRedDotSystem.mTree;

}

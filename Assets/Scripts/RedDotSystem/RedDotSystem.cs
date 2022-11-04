using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//供父节点监听用
namespace RedDot
{
    public enum ERedDotSystemType
    {
        Avatar,//avatar界面
        Emo,//emo界面
        Primitive,
        OperationBtn, // 操作按钮
    }
    public class RedDotSystemManager
    {
        public Dictionary<ERedDotSystemType, RedDotSystem> mSystems;
        public GameObject mGameObject;
        public RedDotSystemManager()
        {
            mSystems = new Dictionary<ERedDotSystemType, RedDotSystem>();
            mGameObject = new GameObject("RedDotSystemManager");
        }
        public void Init()
        {

        }
        public void Register()
        {

        }
        public RedDotSystem CreateSystem(ERedDotSystemType systemType, RedDotSystemContext context)
        {
            if (mSystems.ContainsKey(systemType))
            {
                LoggerUtils.LogError($"system is created,  systemType is  {systemType}");
                return mSystems[systemType];
            }
            RedDotSystem system = new RedDotSystem(this,context);
            system.Init();
            mSystems.Add(systemType, system);
            return system;
        }
    }
    public class RedDotSystemContext
    {
        public string mSystemName = "RedDotSystem";
        public Type mNodeFactoryType;
        public Type mTreeConstructerType;

        public RedDotSystemContext()
        {
            mNodeFactoryType = typeof(RedDotNodeFactoryBase);
        }
        public void SetNodeFactoryType<T>() where T : RedDotNodeFactoryBase
        {
            mNodeFactoryType = typeof(T);
        }
        public void SetRedDotTreeConstructerType<T>() where T : IRedDotTreeConstructer
        {
            mTreeConstructerType = typeof(T);
        }
    }
    public class RedDotSystem
    {
        public RedDotTree mTree;
        public RedDotSystemContext mContext;
        public IRedDotTreeConstructer mTreeConstructer;
        protected RedDotNodeFactoryBase mNodeFactory;
        public RedDotSystemManager mManager;
        public RedDotNodeFactoryBase NodeFactory => mNodeFactory;
        public string mSystemName;
        public GameObject mGameObject;
        public RedDotSystem(RedDotSystemManager manager, RedDotSystemContext context)
        {
            mContext = context;
            mManager = manager;
            mGameObject = new GameObject($"{mContext.mSystemName}");
            mGameObject.transform.parent = mManager.mGameObject.transform;
        }
        public void Init()
        {
            CreateNodeFactory();
            CreateTree();
            mTreeConstructer = CreateTreeConstructer();
            mTreeConstructer.Construct(mTree);
            OnInit();
        }
        protected virtual void OnInit()
        {

        }
        public void Update()
        {
            if (mTree!=null)
            {
                mTree.Update();
            }
        }
        public void CreateTree()
        {
            mTree = new RedDotTree(this);
            mTree.Init();
        }
        public void CreateNodeFactory()
        {
            mNodeFactory = Activator.CreateInstance(mContext.mNodeFactoryType) as RedDotNodeFactoryBase;
            mNodeFactory.Init();
        }
        public IRedDotTreeConstructer CreateTreeConstructer()
        {
            return Activator.CreateInstance(mContext.mTreeConstructerType) as IRedDotTreeConstructer;
        }
    }
}


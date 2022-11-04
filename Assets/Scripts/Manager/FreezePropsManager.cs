using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
/// <summary>
/// Author:weichanglin
/// Description:冻结道具管理器
/// Date: 2022.08.05
/// </summary>
public class FreezePropsManager : ManagerInstance<FreezePropsManager>, IManager, IUGCManager,IPVPManager
{
    public const string DEFAULT_MODEL = "DEFAULT_MODEL";//默认占位图的Key
    public const int MaxCount = 99;
    public const int mNormalFreezeTime = 2;
    public const int mFreezeTimeMin = 2;
    public const int mFreezeTimeMax = 30;
    private Dictionary<string, List<NodeBaseBehaviour>> mNodes = new Dictionary<string, List<NodeBaseBehaviour>>();
    private Dictionary<NodeBaseBehaviour, FreezePropsNodeAuxiliary> mNodesAuxiliary = new Dictionary<NodeBaseBehaviour, FreezePropsNodeAuxiliary>();
    
    public FreezePropsSession mSession;
    private UgcChooseItem mLastSelect;
    public EffectManger mEffectManager;
    public FreezeTimerManager mTimerManager;
    //试玩模式的冻结管理
    public FreezePlayerManagerBase mPlayFreezeManager;
    //联机模式的冻结管理
    public FreezePlayerManagerBase mNetFreezeManager;
    public UgcChooseItem LastSelect
    {
        get { return mLastSelect; }
        set { mLastSelect = value; }
    }
    public int Count
    {
        get
        {
            int count = 0;
            foreach (var kv in mNodes)
            {
                var elements = kv.Value;
                count += elements.Count;
            }
            return count;
        }
    }
    public void Init()
    {
        mSession = new FreezePropsSession(this);
        mEffectManager = new EffectManger(this);
        mTimerManager = new FreezeTimerManager(this);
        mPlayFreezeManager = new PlayModelFreezePlayerManager();
        mPlayFreezeManager.SetManager(this);
        mNetFreezeManager = new NetModelFreezePlayerManager();
        mNetFreezeManager.SetManager(this);
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
    }
    public void Clear()
    {
        mNodes.Clear();
    }
    public override void Release()
    {
        base.Release();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
    }
    protected void OnChangeMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Edit:
                SetMeshColliderEnable(true);
                SetDefaultModeShow(true);
                SetAllNodeVisible(true);
                mTimerManager.DestroyAllTimer();
                if (mPlayFreezeManager.CheckerPlayerIsFreeze(Player.Id))
                {
                    mPlayFreezeManager.MainPlayerUnFreeze(Player.Id);
                }
                mPlayFreezeManager.Clear();
                mNetFreezeManager.Clear();
                break;
            case GameMode.Play:
            case GameMode.Guest:
                SetMeshColliderEnable(false);
                SetDefaultModeShow(false);
                break;
        }
    }
    public void SetDefaultModeShow(bool isShow)
    {
        var defalutModels = GetAllDefaultNodeBev();
        if (defalutModels != null)
        {
            foreach (var attackModel in defalutModels)
            {
                attackModel.gameObject.SetActive(isShow);
            }
        }
    }
    public void SetAllNodeVisible(bool visible)
    {
        foreach (var list in mNodes.Values)
        {
            foreach (var node in list)
            {
                if (node != null)
                {
                    bool nodeIsVisible = visible;
                    // 被开关控制，且默认不可见，就隐藏
                    if (GetNodeAuxiliary(node) != null)
                    {
                        GetNodeAuxiliary(node).propIsUsed = !visible;

                        // 被开关控制，且默认不可见，就隐藏
                        if (node.entity.HasComponent<ShowHideComponent>()
                        && node.entity.Get<ShowHideComponent>().defaultShow == 1)
                        {
                            nodeIsVisible = false;
                        }
                    }
                    node.gameObject.SetActive(nodeIsVisible);
                }
            }
        }
    }
    public void SetMeshColliderEnable(bool enabled)
    {
        foreach (var list in mNodes.Values)
        {
            foreach (var node in list)
            {
                if (node != null)
                {
                    var nodeAuxiliary = GetNodeAuxiliary(node);
                    if (nodeAuxiliary != null)
                    {
                        nodeAuxiliary.SetMeshColliderEnable(enabled);
                    }
                }
            }
        }
    }
    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        if (!behaviour)
        {
            return;
        }

        string removeIndex = "";
        foreach (var kv in mNodes)
        {
            var elements = kv.Value;
            if (elements.Contains(behaviour))
            {
                removeIndex = kv.Key;
                break;
            }
        }

        if (!string.IsNullOrEmpty(removeIndex) && mNodes.ContainsKey(removeIndex))
        {
            var rmList = mNodes[removeIndex];
            rmList.Remove(behaviour);

            //场景内已无该UGC素材使用记录，清除数据
            if (rmList.Count <= 0)
            {
                mNodes.Remove(removeIndex);
            }
        }
    }
    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour.entity.HasComponent<FreezePropsComponent>())
        {
            var rid = behaviour.entity.Get<FreezePropsComponent>().rId;
            AddUgcItem(rid, behaviour);
        }
    }
    public bool IsCanClone(int count)
    {
        if (Count + count > MaxCount)
        {
            return false;
        }
        return true;
    }
    public bool IsLimitedCount()
    {
        return Count >= MaxCount;
    }
    public NodeBaseBehaviour CreateBySelected(Vector3 pos)
    {
        NodeBaseBehaviour nBev = null;
        if (LastSelect == null || string.IsNullOrEmpty(LastSelect.mapJsonContent))
        {
            //创建默认冻结道具
            nBev = CreateDefaultNode();
        }
        else
        {
            //创建上一个UGC素材并作为冻结道具
            nBev = CreateUgcNode(pos, LastSelect.mapJsonContent);
        }
        return nBev;
    }
    public NodeBaseBehaviour CreateUgcNode(Vector3 pos, string content)
    {
        var mapInfo = LastSelect.mapInfo;
        NodeBaseBehaviour behaviour= CreateUgcNodeByEditor(pos, mapInfo, mapInfo.mapId, content);
        AddAndSetComponent(behaviour, mapInfo.mapId);
        AddUgcItem(mapInfo.mapId, behaviour);
        return behaviour;
    }
    //创建UgcNode统一接口
    public NodeBaseBehaviour CreateUgcNodeByEditor(Vector3 pos, MapInfo mapInfo, string rId, string mapJsonContent)
    {
        NodeBaseBehaviour nBev = UgcChooseManager.Inst.CreateSingleUgcAsProp(pos, mapInfo, rId, mapJsonContent);
        AddConstrainer(nBev);

        var gameComponent = nBev.entity.Get<GameObjectComponent>();
        gameComponent.handleType = NodeHandleType.FreezeProps;
        gameComponent.modelType = NodeModelType.FreezeProps;
        FreezePropsNodeAuxiliary auxiliary = new FreezePropsNodeAuxiliary(nBev, this);
        AddNodeAuxiliary(nBev, auxiliary);
        auxiliary.UpdatePropBehaviour(nBev,false);
        return nBev;
    }
    public NodeBaseBehaviour CreateDefaultNode()
    {
        FreezePropsBehaviour newBev = SceneBuilder.Inst.CreateSceneNode<FreezePropsCraeter, FreezePropsBehaviour>();
        FreezePropsData data = new FreezePropsData(DEFAULT_MODEL, mNormalFreezeTime);
        FreezePropsCraeter.SetData(newBev, data);
        return newBev;
    }
    public FreezePropsNodeAuxiliary GetNodeAuxiliary(NodeBaseBehaviour node)
    {
        FreezePropsNodeAuxiliary nodeAuxiliary = null;
        mNodesAuxiliary.TryGetValue(node, out nodeAuxiliary);
        return nodeAuxiliary;
    }
    public void AddNodeAuxiliary(NodeBaseBehaviour node, FreezePropsNodeAuxiliary nodeAuxiliary)
    {
        if (!mNodesAuxiliary.ContainsKey(node))
        {
            mNodesAuxiliary.Add(node, nodeAuxiliary);
        }
    }
    //这个理论上是还原场景时候走的路径
    public void AddComponentToUGC(NodeBaseBehaviour behaviour, NodeData data)
    {
        var kv = data.attr.Find(x => x.k == (int)BehaviorKey.FreezeProps);
        if (kv != null)
        {
            FreezePropsData jsonData = JsonConvert.DeserializeObject<FreezePropsData>(kv.v);
            FreezePropsComponent compt = behaviour.entity.Get<FreezePropsComponent>();
            compt.rId = jsonData.id;
            compt.mFreezeTime = jsonData.mFreezeTime;
            var gameComponent = behaviour.entity.Get<GameObjectComponent>();
            gameComponent.modId = (int)GameResType.FreezeProps;
            gameComponent.handleType = NodeHandleType.FreezeProps;
            gameComponent.modelType = NodeModelType.FreezeProps;
            AddUgcItem(compt.rId, behaviour);
            //需要刷新包围盒
            FreezePropsNodeAuxiliary auxiliary = GetNodeAuxiliary(behaviour);
            if (auxiliary == null)
            {
                auxiliary = new FreezePropsNodeAuxiliary(behaviour, this);
            }
            AddNodeAuxiliary(behaviour, auxiliary);
            auxiliary.UpdatePropBehaviour(behaviour);
        }
    }
    public void AddUgcItem(string rId, NodeBaseBehaviour ugcBeav)
    {
        AddUgcList(rId);
        mNodes[rId].Add(ugcBeav);
    }
    public void AddUgcList(string rid)
    {
        if (!mNodes.ContainsKey(rid))
        {
            mNodes.Add(rid, new List<NodeBaseBehaviour>());
        }
    }
    public void AddAndSetComponent(NodeBaseBehaviour behaviour, string rId)
    {
        if (behaviour != null)
        {
            var compt = behaviour.entity.Get<FreezePropsComponent>();
            compt.rId = rId;
            compt.mFreezeTime = mNormalFreezeTime;
            var gameComponent = behaviour.entity.Get<GameObjectComponent>();
            gameComponent.modId = (int)GameResType.FreezeProps;
            gameComponent.handleType = NodeHandleType.FreezeProps;
            gameComponent.modelType = NodeModelType.FreezeProps;
        }
    }
    public List<string> GetAllUgcRidList()
    {
        if (mNodes != null)
        {
            var tempList = mNodes.Keys.ToList();
            if (tempList.Contains(DEFAULT_MODEL))
            {
                tempList.Remove(DEFAULT_MODEL);
            }
            return tempList;
        }
        return null;
    }
    protected virtual void HandlePackPanelShow(bool isShow)
    {
        LoggerUtils.Log("HandlePackPanelShow");
        foreach (var list in mNodes.Values)
        {
            foreach (var nodeBev in list)
            {
                if (nodeBev)
                {
                    nodeBev.gameObject.SetActive(!isShow);
                }
            }
        }
    }


    public void OnUGCChangeStatus(UGCCombBehaviour ugcCombBehaviour)
    {
        //更新包围盒
        FreezePropsNodeAuxiliary nodeAuxiliary = GetNodeAuxiliary(ugcCombBehaviour);
        if (nodeAuxiliary != null)
        {
            if (GlobalFieldController.CurGameMode != GameMode.Edit)
            {
                nodeAuxiliary.SetMeshColliderEnable(false);
            }
            if (nodeAuxiliary.mCollider != null&& nodeAuxiliary.mCollider.size == Vector3.zero)
            {
                nodeAuxiliary.UpdateBoundBox();
            }
        }
    }
    public void AddConstrainer(NodeBaseBehaviour nBehav)
    {
        if (!nBehav.gameObject.TryGetComponent(out SpawnPointConstrainer adjustBehav))
        {
            adjustBehav = nBehav.gameObject.AddComponent<SpawnPointConstrainer>();
            adjustBehav.minHeight = 0;
        }
    }
    public NodeBaseBehaviour GetNodeByUid(int uid)
    {
        foreach (var list in mNodes.Values)
        {
            foreach (var node in list)
            {
                if (node != null)
                {
                    var gComp = node.entity.Get<GameObjectComponent>();
                    if (uid == gComp.uid)
                    {
                        return node;
                    }
                }
            }
        }
        return null;
    }
    //冰冻广播
    public bool OnReceiveServerFreeze(string sendPlayerId, string msg)
    {
        return mSession.OnReceiveServerFreeze(sendPlayerId, msg);
    }
    //解冻广播
    public void OnGetItemsCallback(string dataJson)
    {
        mSession.OnGetItemsCallback(dataJson);
    }
    public void FreezePlyaerWithPlay(string playerId, float freezeTime)
    {
        mPlayFreezeManager.MainPlayerFreeze(playerId,freezeTime);
    }
    public void FreezePlayerWithNet(string playerId, float freezeTime)
    {
        //冻结自己
        if (playerId == Player.Id)
        {
            mNetFreezeManager.MainPlayerFreeze(playerId, freezeTime);
        }
        else
        {
            mNetFreezeManager.OtherPlayerFreeze(playerId, freezeTime);
        }
    }
    public void UnFreezePlayerWithNet(string playerId)
    {
        if (playerId == Player.Id)
        {
            mNetFreezeManager.MainPlayerUnFreeze(playerId);
        }
        else
        {
            mNetFreezeManager.OtherPlayerUnFreeze(playerId);
        }
    }
    public void Update(float deltaTime)
    {
        mTimerManager?.Update(deltaTime);
    }
    //玩家死亡
    public void OnPlayerDeath(string playerId)
    {
        LoggerUtils.Log(string.Format("FreezeManager OnPlayerDeath playerId = {0}", playerId));
        if (mNetFreezeManager.CheckerPlayerIsFreeze(playerId))
        {
            if (playerId == Player.Id)
            {
                mNetFreezeManager.MainPlayerUnFreeze(playerId);
            }
            else
            {
                mNetFreezeManager.OtherPlayerUnFreeze(playerId);
            }
        }
       
    }
    public bool IsPropUsed(NodeBaseBehaviour behaviour)
    {
        var bloodProp = GetNodeAuxiliary(behaviour);
        if (bloodProp != null && bloodProp.propIsUsed)
        {
            return true;
        }

        // 未添加 UGC 素材的道具的显隐不应该被控制
        var defalutModels = GetAllDefaultNodeBev();
        if (defalutModels != null && defalutModels.Count > 0)
        {
            if (defalutModels.Contains(behaviour))
            {
                return true;
            }
        }

        return false;
    }
    public List<NodeBaseBehaviour> GetAllDefaultNodeBev()
    {
        if (mNodes.ContainsKey(DEFAULT_MODEL))
        {
            return mNodes[DEFAULT_MODEL];
        }

        return null;
    }

    public void OnReset()
    {
        //玩家解冻
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            if (mNetFreezeManager!=null)
            {
                mNetFreezeManager.AllClientForceUnFreeze();
            }
        }
        else
        {
            if (mPlayFreezeManager!=null)
            {
                mPlayFreezeManager.AllClientForceUnFreeze();
            }
        }
        //冻结道具重新显示
        SetAllNodeVisible(true);
    }
}

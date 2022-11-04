using System.Collections.Generic;
using SavingData;
using UnityEngine;

public class FishingEditManager : ManagerInstance<FishingEditManager>, IManager
{
    public Vector3 DefaultHookPoint => Vector3.up * -0.3f;
    public List<string> UgcRodRecords { get { return _ugcRodRecords; } }
    public List<string> UgcHookRecords { get { return _ugcHookRecords; } }

    public string RodParentPath = "root_fish1/yugan/Model";
    public string HookParentPath = "root_fish1/FishingHookParent/FishingHook/YUBIAO1_Mesh/Model";

    private List<NodeBaseBehaviour> _fishingNodeLst = new List<NodeBaseBehaviour>();
    private List<string> _ugcRodRecords = new List<string>();
    private List<string> _ugcHookRecords = new List<string>();

    private const int MAX_COUNT = 99;

    private Dictionary<int, Vector3> _hookNodeLocalPos = new Dictionary<int, Vector3>();

    public void Init()
    {
        MessageHelper.AddListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
    }

    public void OnModeChange(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Edit:
                RestFishingRodPos();
                break;
            case GameMode.Play:
            case GameMode.Guest:
                GetFishingRodPos();
                GetHookNodeLocalPos();
                break;
        }
    }

    public void GetFishingRodPos()
    {
        foreach(var fishingRod in _fishingNodeLst)
        {
            var fishBev = fishingRod as FishingBehaviour;
            fishBev.GetFishingRodNodeOriPos();
        }
    }

    public void RestFishingRodPos()
    {
        foreach (var fishingRod in _fishingNodeLst)
        {
            var fishBev = fishingRod as FishingBehaviour;
            fishBev.ResetFishingRodNodeOriPos();
        }
    }

    public void Clear()
    {
        _fishingNodeLst.Clear();
        _ugcRodRecords.Clear();
        _ugcHookRecords.Clear();
    }

    public FishingBehaviour CreateFishingNode(Vector3 pos)
    {
        var node = SceneBuilder.Inst.CreateSceneNode<FishingModelCreater, FishingBehaviour>();
        node.transform.position = pos;
        //鱼竿默认添加可拾起属性
        var comp = node.entity.Get<PickablityComponent>();
        comp.canPick = 1;
        PickabilityManager.Inst.AddPickablityProp(node.entity, comp.anchors);
        CreatePgcRod(node);
        CreatePgcHook(node);
        return node;
    }

    public NodeBaseBehaviour CreatePgcRod(FishingBehaviour fishingNode)
    {
        var oldRod = GetRod(fishingNode);
        var oldPos = oldRod ? oldRod.transform.localPosition : Vector3.zero;

        var rod = SceneBuilder.Inst.CreateSceneNode<FishingRodCreator, FishingRodBehaviour>();
        FishingRodCreator.AddRodComponent(rod, "", 0);
        var rodParent = fishingNode.transform.Find(RodParentPath);
        rod.transform.SetParent(rodParent);
        rod.transform.Normalize();
        rod.transform.localPosition = oldPos;

        return rod;
    }

    public NodeBaseBehaviour CreatePgcHook(FishingBehaviour fishingNode)
    {
        var oldHook = GetHook(fishingNode);
        var oldPos = oldHook ? oldHook.transform.localPosition : Vector3.zero;

        var hook = SceneBuilder.Inst.CreateSceneNode<FishingHookCreator, FishingHookBehaviour>();
        FishingHookCreator.AddHookComponent(hook, "", oldPos, 0);
        var hookParent = fishingNode.transform.Find(HookParentPath);
        hook.transform.SetParent(hookParent);
        hook.transform.Normalize();
        hook.transform.localPosition = oldPos;

        return hook;
    }

    public NodeBaseBehaviour CreateUgcRod(FishingBehaviour fishingNode, MapInfo mapInfo, string rid, string mapJsonContent)
    {
        var oldRod = GetRod(fishingNode);
        var oldPos = oldRod ? oldRod.transform.localPosition : Vector3.zero;

        var rod = UgcChooseManager.Inst.CreateSingleUgcAsProp(Vector3.zero, mapInfo, rid, mapJsonContent);
        FishingRodCreator.AddRodComponent(rod, rid, 0);
        var rodParent = fishingNode.transform.Find(RodParentPath);
        rod.transform.SetParent(rodParent);
        rod.transform.localPosition = oldPos;

        RecordUgcRod(rid);

        return rod;
    }

    public NodeBaseBehaviour CreateUgcHook(FishingBehaviour fishingNode, MapInfo mapInfo, string rid, string mapJsonContent)
    {
        var localPos = Vector3.zero;
        var hookPosition = Vector3.zero;
        var isCoutomHook = 0;

        var oldHook = GetHook(fishingNode);
        if (oldHook)
        {
            var comp = oldHook.entity.Get<FishingHookComponent>();
            localPos = oldHook.transform.localPosition;
            hookPosition = comp.hookPosition;
            isCoutomHook = comp.isCustomHook;
        }

        var hook = UgcChooseManager.Inst.CreateSingleUgcAsProp(Vector3.zero, mapInfo, rid, mapJsonContent);
        FishingHookCreator.AddHookComponent(hook, rid, hookPosition, isCoutomHook);
        var hookParent = fishingNode.transform.Find(HookParentPath);
        hook.transform.SetParent(hookParent);
        hook.transform.localPosition = localPos;

        RecordUgcHook(rid);

        return hook;
    }

    public NodeBaseBehaviour GetRod(FishingBehaviour fishingNode)
    {
        var parent = fishingNode.transform.Find(RodParentPath);
        if (parent.childCount > 0)
            return parent.GetChild(0).GetComponent<NodeBaseBehaviour>();

        return null;
    }

    public NodeBaseBehaviour GetHook(FishingBehaviour fishingNode)
    {
        var parent = fishingNode.transform.Find(HookParentPath);
        if (parent.childCount > 0)
            return parent.GetChild(0).GetComponent<NodeBaseBehaviour>();

        return null;
    }

    public void AddComponentToUGC(NodeBaseBehaviour behaviour, NodeData data)
    {
        if (data.attr.Find(x => x.k == (int)BehaviorKey.FishingRod) != null)
        {
            FishingRodCreator.SetData(behaviour, data, behaviour.transform.localPosition, behaviour.transform.parent);
            RecordUgcRod(data.rid);
        }

        if (data.attr.Find(x => x.k == (int)BehaviorKey.FishingHook) != null)
        {
            FishingHookCreator.SetData(behaviour, data, behaviour.transform.localPosition, behaviour.transform.parent);
            RecordUgcHook(data.rid);
        }
    }

    public void AddFishingNode(NodeBaseBehaviour node)
    {
        if (!_fishingNodeLst.Contains(node))
            _fishingNodeLst.Add(node);
    }

    public void RemoveFishingNode(NodeBaseBehaviour node)
    {
        if (_fishingNodeLst.Contains(node))
            _fishingNodeLst.Remove(node);
    }

    public void RecordUgcRod(string rid)
    {
        if (!_ugcRodRecords.Contains(rid))
            _ugcRodRecords.Add(rid);
    }

    public void RecordUgcHook(string rid)
    {
        if (!_ugcHookRecords.Contains(rid))
            _ugcHookRecords.Add(rid);
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour is FishingBehaviour)
            RemoveFishingNode(behaviour);
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour is FishingBehaviour)
            AddFishingNode(behaviour);
    }

    public bool IsOverMaxCount()
    {
        return _fishingNodeLst.Count >= MAX_COUNT;
    }

    public bool IsCanClone(GameObject curTarget)
    {
        var entity = curTarget.GetComponent<NodeBaseBehaviour>().entity;
        var comp = entity.Get<GameObjectComponent>();
        switch (comp.modelType)
        {
            case NodeModelType.FishingRod:
            case NodeModelType.FishingHook:
            case NodeModelType.FishingModel:
                if (IsOverMaxCount())
                {
                    TipPanel.ShowToast("Up to 99 fishing rods can be added to the experience");
                    return false;
                }
                break;
        }

        return true;
    }

    private void HandlePackPanelShow(bool isOn)
    {
        foreach (var node in _fishingNodeLst)
            node.gameObject.SetActive(!isOn);
    }

    private void GetHookNodeLocalPos()
    {
        foreach (var node in _fishingNodeLst)
        {
            var modelEntity = node.entity;
            var modelGComp = modelEntity.Get<GameObjectComponent>();
            var modelUid = modelGComp.uid;
            var hookBev = node.transform.Find(HookParentPath).GetComponentInChildren<NodeBaseBehaviour>();
            _hookNodeLocalPos[modelUid] = hookBev.transform.localPosition;
        }
    }

    //重置鱼竿鱼票的相对位置
    public void ResetFishingHookLocalPos(NodeBaseBehaviour baseBev)
    {
        var entity = baseBev.entity;
        var gComp = entity.Get<GameObjectComponent>();
        var uid = gComp.uid;
        var oriPos = _hookNodeLocalPos[uid];
        var hookBev = baseBev.transform.Find(HookParentPath).GetComponentInChildren<NodeBaseBehaviour>();
        if(oriPos != null)
        {
            hookBev.transform.localPosition = oriPos;
        }
    }

    public override void Release()
    {
        MessageHelper.RemoveListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
        base.Release();
    }
}

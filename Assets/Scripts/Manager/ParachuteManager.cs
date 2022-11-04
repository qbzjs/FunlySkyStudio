using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Author: LiShuzhan
/// Description:
/// Date: 2022-08-01
/// </summary>
public enum ParaUgcType
{
    None,
    Parachute,
    Bag
}
public class ParachuteManager : ManagerInstance<ParachuteManager>, IManager, IPVPManager
{
    public int MAX_COUNT = 99;
    public const string MAX_COUNT_TIP = "Up to 99 parachutes can be added to the experience"; //TODO:fsc tip更换
    public Vector3 bagOffset = new Vector3(1, 0, 0);
    public const string DEFAULT_MODEL = "DEFAULT_MODEL";
    //降落伞道具内存数据维护 -- 包含已替换的ugc 和 默认占位图
    //默认占位图的key--- DEFAULT_MODEL
    //UGC道具的key--- Rid
    public Dictionary<string, List<NodeBaseBehaviour>> allParaUgcDict = new Dictionary<string, List<NodeBaseBehaviour>>();
    //伞包道具内存数据维护
    public Dictionary<string, List<NodeBaseBehaviour>> allBagUgcDict = new Dictionary<string, List<NodeBaseBehaviour>>();

    private GameObject mParachutePickEffectObj;
    private string mParachutePickEffectPath = "Effect/parachute/ParacPick/parachuting_shou";
    private Coroutine closeEffect;
    private Vector3 paraRot = new Vector3(52, -237, 212);
    private Vector3 oriParaRot = Vector3.zero;
    private Transform pickNode;
    public override void Release()
    {
        base.Release();
        Clear();
    }

    public bool IsOverMaxCount()
    {
        int curCount = 0;
        foreach (var ugcs in allParaUgcDict)
        {
            curCount += ugcs.Value.Count;
        }
        if (curCount >= MAX_COUNT)
        {
            return true;
        }
        return false;
    }
    
    public bool IsCanClone(GameObject curTarget)
    {
        var entity = curTarget.GetComponent<NodeBaseBehaviour>().entity;
        if (entity.HasComponent<ParachuteComponent>())
        {
            if (IsOverMaxCount())
            {
                TipPanel.ShowToast(MAX_COUNT_TIP);
                return false;
            }
        }

        return true;
    }

    public void OnChangeMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Guest:
                SetAllDefPrefabActive(false);
                break;
            case GameMode.Play:
                SetAllDefPrefabActive(false);
                HideAllBagActive();
                break;
            case GameMode.Edit:
                ShowAllParachute();
                HideAllBagActive();
                SetAllDefPrefabActive(true);
                break;
        }
        if (PlayerParachuteControl.Inst)
        {
            PlayerParachuteControl.Inst.OnChangeMode(mode);
        }

    }

    public void Init()
    {
        MessageHelper.AddListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
    }

    public void HandlePackPanelShow(bool isActive)
    {
        var nodeList = GetAllParachute();
        for (int i = 0; i < nodeList.Count; i++)
        {
            nodeList[i].gameObject.SetActive(!isActive);
        }
        HideAllBagActive();
    }

    public NodeBaseBehaviour ByIdFindBehav(int uid, Dictionary<string, List<NodeBaseBehaviour>> allUgcDict)
    {
        foreach (var item in allUgcDict)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                var entity = item.Value[i].entity;
                if (entity.Get<GameObjectComponent>().uid == uid)
                {
                    return item.Value[i];
                }
            }
        }
        return null;
    }

    public void AddParachuteComponent(NodeBaseBehaviour behaviour, string rId,int baguid = -1)
    {
        if (behaviour != null)
        {
            var cmp = behaviour.entity.Get<ParachuteComponent>();
            var gameComponent = behaviour.entity.Get<GameObjectComponent>();
            gameComponent.modId = (int)GameResType.Parachute;
            if(baguid != -1)
            {
                cmp.parachuteBagUid = baguid;
            }
            cmp.rid = gameComponent.resId;
            cmp.isCustomPoint = (int)CustomPointState.Off;
            cmp.anchors = Vector3.zero;
            AddUgcItem(cmp.rid, behaviour, ParaUgcType.Parachute);
        }
    }

    public void AddParachuteBagComponent(NodeBaseBehaviour behaviour, string rId,int parauid = -1)
    {
        if (behaviour != null)
        {
            var cmp = behaviour.entity.Get<ParachuteBagComponent>();
            var gameComponent = behaviour.entity.Get<GameObjectComponent>();
            if(parauid != -1)
            {
                cmp.parachuteUid = parauid;
            }
            cmp.rid = gameComponent.resId;
            AddUgcItem(cmp.rid, behaviour, ParaUgcType.Bag);
        }
    }

    //在编辑模式创建降落伞
    public NodeBaseBehaviour CreateParachuteBeavInEdit(Vector3 pos)
    {
        UgcChooseItem lastSelectUgcInfo = null;
        if (ParachutePanel.Instance != null)
        {
            lastSelectUgcInfo = ParachutePanel.Instance.ugcChooseParachuteItem.lastChooseItem;
        }
        NodeBaseBehaviour nBev = null;
        if (lastSelectUgcInfo == null || string.IsNullOrEmpty(lastSelectUgcInfo.mapJsonContent))
        {
            nBev = CreateDefaultNode(ParaUgcType.Parachute,Vector3.zero);
        }
        else
        {
            var mapInfo = lastSelectUgcInfo.mapInfo;
            nBev = UgcChooseManager.Inst.CreateSingleUgcAsProp(pos, mapInfo, mapInfo.mapId, lastSelectUgcInfo.mapJsonContent);
            AddParachuteComponent(nBev, mapInfo.mapId);
        }
        AddPickableComponent(nBev,ParaUgcType.Parachute);
        return nBev;
    }

    //在编辑模式创建降落伞包
    public NodeBaseBehaviour CreateParachuteBagBeavInEdit(Vector3 pos)
    {
        if (ParachutePanel.Instance == null)
        {
            return null;
        }
        NodeBaseBehaviour nBev = null;
        var lastSelectUgcInfo = ParachutePanel.Instance.UgcChoosebagItem.lastChooseItem;
        if (lastSelectUgcInfo == null || string.IsNullOrEmpty(lastSelectUgcInfo.mapJsonContent))
        {
            nBev = CreateDefaultNode(ParaUgcType.Bag, pos);
        }
        else
        {
            var mapInfo = lastSelectUgcInfo.mapInfo;
            nBev = UgcChooseManager.Inst.CreateSingleUgcAsProp(pos, mapInfo, mapInfo.mapId, lastSelectUgcInfo.mapJsonContent);
            AddParachuteBagComponent(nBev, mapInfo.mapId);
        }
        AddPickableComponent(nBev, ParaUgcType.Bag);
        return nBev;
    }

    //创建默认的模型
    public NodeBaseBehaviour CreateDefaultNode(ParaUgcType type,Vector3 bagPos)
    {
        NodeBaseBehaviour newBev = null;
        if (type == ParaUgcType.Parachute)
        {
            newBev = SceneBuilder.Inst.CreateSceneNode<ParachuteCreater, ParachuteBehaviour>();
            ParachuteCreater.SetData((ParachuteBehaviour)newBev, new ParachuteData()
            {
                rid = DEFAULT_MODEL,
            }
            ,new NodeData()
            {
                uid = 0,
            });
        }
        else if (type == ParaUgcType.Bag)
        {
            newBev = SceneBuilder.Inst.CreateSceneNode<ParachuteBagCreater, ParachuteBagBehaviour>();
            newBev.transform.position = bagPos;
            ParachuteBagCreater.SetData((ParachuteBagBehaviour)newBev, new ParachuteBagData()
            {
                rid = DEFAULT_MODEL,
            }
            , new NodeData()
            {
            uid = 0,
            });
        }

        return newBev;
    }

    /// <summary>
    /// 添加UGC节点
    /// </summary>
    /// <param name="rId"></param>
    /// <param name="ugcBeav"></param>
    public void AddUgcItem(string rId, NodeBaseBehaviour ugcBeav, ParaUgcType type)
    {
        switch (type)
        {
            case ParaUgcType.Parachute:
                AddUgcList(rId,ParaUgcType.Parachute);
                if (!allParaUgcDict[rId].Contains(ugcBeav))
                {
                    allParaUgcDict[rId].Add(ugcBeav);
                }
                break;
            case ParaUgcType.Bag:
                AddUgcList(rId, ParaUgcType.Bag);
                if (!allBagUgcDict[rId].Contains(ugcBeav))
                {
                    allBagUgcDict[rId].Add(ugcBeav);
                }
                break;
        }
    }

    /// <summary>
    /// 根据素材id,添加UGC素材list
    /// </summary>
    /// <param name="rid"></param>
    public void AddUgcList(string rid,ParaUgcType type)
    {
        switch (type)
        {
            case ParaUgcType.Parachute:
                if (!allParaUgcDict.ContainsKey(rid))
                {
                    allParaUgcDict.Add(rid, new List<NodeBaseBehaviour>());
                }
                break;
            case ParaUgcType.Bag:
                if (!allBagUgcDict.ContainsKey(rid))
                {
                    allBagUgcDict.Add(rid, new List<NodeBaseBehaviour>());
                }
                break;
        }
    }

    public void RemoveUgcItem(string rid,int uid,ParaUgcType type)
    {
        if (rid == null) return;
        switch (type)
        {
            case ParaUgcType.Parachute:
                if (allParaUgcDict.ContainsKey(rid))
                {
                    for (int i = 0; i < allParaUgcDict[rid].Count; i++)
                    {
                        if (allParaUgcDict[rid][i].entity.Get<GameObjectComponent>().uid == uid)
                        {
                            allParaUgcDict[rid].Remove(allParaUgcDict[rid][i]);
                        }
                    }
                }
                break;
            case ParaUgcType.Bag:
                if (allBagUgcDict.ContainsKey(rid))
                {
                    for (int i = 0; i < allBagUgcDict[rid].Count; i++)
                    {
                        if (allBagUgcDict[rid][i].entity.Get<GameObjectComponent>().uid == uid)
                        {
                            allBagUgcDict[rid].Remove(allBagUgcDict[rid][i]);
                        }
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 获取所有UGC Rid集合
    /// 排除了默认占位道具
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllUgcRidList(ParaUgcType type)
    {
        switch (type)
        {
            case ParaUgcType.Parachute:
                if (allParaUgcDict != null)
                {
                    var tempList = allParaUgcDict.Keys.ToList();
                    if (tempList.Contains(DEFAULT_MODEL))
                    {
                        tempList.Remove(DEFAULT_MODEL);
                    }
                    return tempList;
                }
                break;
            case ParaUgcType.Bag:
                if (allBagUgcDict != null)
                {
                    var tempList = allBagUgcDict.Keys.ToList();
                    if (tempList.Contains(DEFAULT_MODEL))
                    {
                        tempList.Remove(DEFAULT_MODEL);
                    }
                    return tempList;
                }
                break;
        }
        return null;
    }

    public Vector3 GetAnchors(SceneEntity entity)
    {
        if (entity.HasComponent<ParachuteBagComponent>())
        {
            var pCom = entity.Get<ParachuteBagComponent>();
            return pCom.anchors;
        }
        if (entity.HasComponent<ParachuteComponent>())
        {
            var pCom = entity.Get<ParachuteComponent>();
            return pCom.anchors;
        }
        return Vector3.zero;
    }

    public void SetAnchors(SceneEntity entity, Vector3 pos)
    {
        if (entity.HasComponent<ParachuteBagComponent>())
        {
            var pCom = entity.Get<ParachuteBagComponent>();
            pCom.anchors = pos;
        }
        if (entity.HasComponent<ParachuteComponent>())
        {
            var pCom = entity.Get<ParachuteComponent>();
            pCom.anchors = pos;
        }
        if (entity.HasComponent<PickablityComponent>())
        {
            entity.Get<PickablityComponent>().anchors = pos;
        }
    }

    public NodeBaseBehaviour OnParachuteBagSelect(SceneEntity entity)
    {
        if (entity.HasComponent<ParachuteBagComponent>())
        {
            ParachutePanel.Show();
            var uid = entity.Get<ParachuteBagComponent>().parachuteUid;
            var paraBehav = ByIdFindBehav(uid,allParaUgcDict);
            return paraBehav;
        }
        return null;
    }

    public NodeBaseBehaviour GetAttachedItem(GameObject target)
    {
        var tagetBehav = target.GetComponent<NodeBaseBehaviour>();
        if (tagetBehav && tagetBehav.entity.HasComponent<ParachuteComponent>())
        {
            var entity = target.GetComponent<NodeBaseBehaviour>().entity;
            var uid = entity.Get<ParachuteComponent>().parachuteBagUid;
            var behav = ByIdFindBehav(uid,allBagUgcDict);
            if(behav == null)
            {
                return null;
            }
            return behav;
        }
        return null;
    }

    //显示所有降落伞
    public void ShowAllParachute()
    {
        var dic = allParaUgcDict;
        foreach (var item in dic)
        {
            foreach (var ugcitem in item.Value)
            {
                ugcitem.gameObject.SetActive(true);
            }
        }
    }
    //隐藏所有背包
    public void HideAllBagActive()
    {
        var dic = allBagUgcDict;
        foreach (var item in dic)
        {
            foreach (var ugcitem in item.Value)
            {
                ugcitem.gameObject.SetActive(false);
            }
        }
    }

    //显示降落伞对应的背包
    public void ShowBag(SceneEntity entity)
    {
        var target = entity.Get<GameObjectComponent>().bindGo;
        if (entity.HasComponent<ParachuteBagComponent>())
        {
            target.SetActive(true);
            return;
        }
        var bag = GetAttachedItem(target);
        if(bag != null)
        {
            bag.gameObject.SetActive(true);
        }
    }
    //拾取降落伞后，需要执行的事件
    public void OnPickParacute(string playerId,NodeBaseBehaviour nBehav, Transform bandNode,Transform paraNode)
    {
        if (!nBehav.entity.HasComponent<ParachuteComponent>()) { return; }
        OnPickBandBag(nBehav, bandNode);
        nBehav.gameObject.SetActive(false);
        var gComp = nBehav.entity.Get<GameObjectComponent>();
        if (playerId == GameManager.Inst.ugcUserInfo.uid)
        {
            if (PlayerParachuteControl.Inst == null)
            {
                var handleItemId = BaggageManager.Inst.handleItemId;
                if (SceneParser.Inst.GetBaggageSet() == 1 && handleItemId != gComp.uid && handleItemId != (int)BaggageItemType.none)
                {
                    return;
                }
                PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerParachuteControl>();
            }
            PickabilityManager.Inst.ChangeAnimClips(playerId, PICK_STATE.CATCH);
            
        }
        else
        {
            var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(playerId);
            if(otherComp != null)
            {
                otherComp.SwitchNormalAnimClips();
            }

        }
    }

    //拾取时把伞包绑定在玩家身上
    public void OnPickBandBag(NodeBaseBehaviour nBehav,Transform bandNode)
    {
        var entity = nBehav.entity;
        var bag = GetAttachedItem(entity.Get<GameObjectComponent>().bindGo);
        if(bag != null)
        {
            var bComp = bag.entity.Get<ParachuteBagComponent>();
            var gComp = bag.entity.Get<GameObjectComponent>();
            if (bComp.rid == DEFAULT_MODEL)
            {
                return;
            }
            if (!PickabilityManager.Inst.PropParentDic.ContainsKey(gComp.uid))
            {
                PickabilityManager.Inst.PropParentDic.Add(gComp.uid, bag.transform.parent);
            }
            Vector3 bAnchors = bComp.anchors;
            bag.transform.SetParent(bandNode);
            bag.transform.localPosition = Vector3.zero;
            bag.transform.localEulerAngles = PickabilityManager.Inst.GetOriQuaternion(gComp.uid);
            bag.transform.position = bag.transform.TransformPoint(-bAnchors);
            if (SceneParser.Inst.GetBaggageSet() == 0)
            {
                bag.gameObject.SetActive(true);
            }
            PickabilityManager.Inst.SetComponentEnable(bag.gameObject, false);
        }
    }
    //丢弃时扔出背包
    public void OnUnPickBag(string playerId, NodeBaseBehaviour nBehav)
    {
        if (!nBehav.entity.HasComponent<ParachuteComponent>())
        {
            return;
        }
        var entity = nBehav.entity;
        var go = entity.Get<GameObjectComponent>().bindGo;
        var bag = GetAttachedItem(go);
        if(bag != null)
        {
            bag.transform.parent = go.transform.parent;
            bag.gameObject.SetActive(false);
            go.SetActive(true);
            PickabilityManager.Inst.SetComponentEnable(bag.gameObject, true);
        }

        if (playerId == GameManager.Inst.ugcUserInfo.uid)
        {
            var curPickBev = PickabilityManager.Inst.GetBaseBevByPlayerId(playerId);
            if (curPickBev != null && 
                SceneParser.Inst.GetBaggageSet() == 1 && 
                curPickBev.entity.HasComponent<ParachuteComponent>())
            {
                return;
            }
            
            if (PlayerParachuteControl.Inst)
            {
                PlayerParachuteControl.Inst.DropParachute();
            }
        }
    }

    public List<NodeBaseBehaviour> GetAllParachute()
    {
        List<NodeBaseBehaviour> nodeList = new List<NodeBaseBehaviour>();
        foreach (var ugc in allParaUgcDict)
        {
            nodeList.AddRange(ugc.Value);
        }
        return nodeList;
    }

    private void SetAllDefPrefabActive(bool isActive)
    {
        if (!allParaUgcDict.ContainsKey(DEFAULT_MODEL))
        {
            return;
        }
        for (int i = 0; i < allParaUgcDict[DEFAULT_MODEL].Count; i++)
        {
            allParaUgcDict[DEFAULT_MODEL][i].gameObject.SetActive(isActive);
        }
    }
    //添加可拾取属性
    public void AddPickableComponent(NodeBaseBehaviour nBev,ParaUgcType type)
    {
        if(nBev && !nBev.entity.HasComponent<PickablityComponent>())
        {
            var entity = nBev.entity;
            PickabilityManager.Inst.AddPickablityProp(entity, entity.Get<PickablityComponent>().anchors);
            entity.Get<PickablityComponent>().canPick = type == ParaUgcType.Parachute ? 1 : 0;
        }
    }

    public void AddComponentToUGC(NodeBaseBehaviour behaviour, NodeData data)
    {
        //降落伞
        var ugcKV = data.attr.Find(x => x.k == (int)BehaviorKey.Parachute);
        if (ugcKV != null)
        {
            var parachuteData = JsonConvert.DeserializeObject<ParachuteData>(ugcKV.v);
            var gameComponent = behaviour.entity.Get<GameObjectComponent>();
            gameComponent.modId = (int)GameResType.Parachute;
            var pComp = behaviour.entity.Get<ParachuteComponent>();
            pComp.anchors = parachuteData.anchorsPos;
            pComp.parachuteBagUid = parachuteData.bagUid;
            pComp.rid = parachuteData.rid;
            pComp.isCustomPoint = parachuteData.isCustomPoint;
            AddUgcItem(parachuteData.rid, behaviour, ParaUgcType.Parachute);
            ParachuteCreater.AddConstrainer(behaviour);
        }

        //背包
        var ugcBagKV = data.attr.Find(x => x.k == (int)BehaviorKey.ParachuteBag);
        if (ugcBagKV != null)
        {
            var parachuteBagData = JsonConvert.DeserializeObject<ParachuteBagData>(ugcBagKV.v);
            var bComp = behaviour.entity.Get<ParachuteBagComponent>();
            bComp.anchors = parachuteBagData.anchorsPos;
            bComp.parachuteUid = parachuteBagData.paraUid;
            bComp.rid = parachuteBagData.rid;
            bComp.isCustomPoint = parachuteBagData.isCustomPoint;
            AddUgcItem(parachuteBagData.rid, behaviour, ParaUgcType.Bag);
            AddPickableComponent(behaviour, ParaUgcType.Bag);
            behaviour.gameObject.SetActive(false);
            ParachuteBagCreater.AddConstrainer(behaviour);
        }
    }

    public void SwitchParachute(string playerId,bool isOpen)
    {
        if (PickabilityManager.Inst.PlayerPickDic.ContainsKey(playerId))
        {
            var paraUid = PickabilityManager.Inst.PlayerPickDic[playerId];
            var parachute = ByIdFindBehav(paraUid, allParaUgcDict);
            if (parachute!=null && parachute.entity.HasComponent<ParachuteComponent>())
            {
               var paraBagUid = parachute.entity.Get<ParachuteComponent>().parachuteBagUid;
               var paraBag = ByIdFindBehav(paraBagUid, allBagUgcDict);
                parachute.gameObject.SetActive(isOpen);
                if(paraBag != null && !(paraBag is ParachuteBagBehaviour))
                {
                    paraBag.gameObject.SetActive(!isOpen);
                }
            }
        }
    }

    public void SetControl(NodeBaseBehaviour nBehav)
    {
        if (!nBehav.entity.HasComponent<ParachuteComponent>())
        {
            if (PlayerParachuteControl.Inst)
            {
                PlayerParachuteControl.Inst.DropParachute();
            }
        }
        else
        {
            if (!PlayerParachuteControl.Inst)
            {
                PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerParachuteControl>();
            }
        }
    }

    public void HideBag(GameObject target)
    {
        var bag = GetAttachedItem(target);
        if(bag != null)
        {
            bag.gameObject.SetActive(false);
        }
    }

    #region Ondo/Redo

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        var entity = behaviour.entity;
        if (entity.HasComponent<ParachuteComponent>())
        {
            var gComp = entity.Get<GameObjectComponent>();
            var pComp = entity.Get<ParachuteComponent>();
            DestroyAttachedItem(gComp.bindGo);
            RemoveUgcItem(pComp.rid, gComp.uid, ParaUgcType.Parachute);
        }
        if (entity.HasComponent<ParachuteBagComponent>())
        {
            var gComp = entity.Get<GameObjectComponent>();
            var pComp = entity.Get<ParachuteBagComponent>();
            RemoveUgcItem(pComp.rid, gComp.uid, ParaUgcType.Bag);
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var entity = behaviour.entity;
        if (entity.HasComponent<ParachuteComponent>())
        {
            var pComp = entity.Get<ParachuteComponent>();
            AddUgcItem(pComp.rid, behaviour, ParaUgcType.Parachute);
            if (pComp.parachuteBagUid > 0)
            {
                GameObject paraGO = SecondCachePool.Inst.GetGameObjectByUid(pComp.parachuteBagUid);
                if (paraGO != null)
                {
                    SecondCachePool.Inst.RevertEntity(paraGO);
                    var paraBehav = paraGO.GetComponent<NodeBaseBehaviour>();
                    if (paraBehav != null)
                    {
                        var bComp = paraBehav.entity.Get<ParachuteBagComponent>();
                        AddUgcItem(bComp.rid, paraBehav, ParaUgcType.Bag);
                    }
                }
                else
                {
                    LoggerUtils.Log("ParachuteManager RevertNode ParachuteBag Can not find:" + pComp.parachuteBagUid);
                }
            }
        }
    }

    public void Clear()
    {
        allBagUgcDict.Clear();
        allParaUgcDict.Clear();
    }
    #endregion
    //在删除时，检测降落伞是否有对应伞包，如果有就删除伞包
    public void DestroyAttachedItem(GameObject target)
    {
        var attache = GetAttachedItem(target);
        if (ParachutePanel.Instance && ParachutePanel.Instance.ugcChooseParachuteItem.isChange)
        {
            ParachutePanel.Instance.ugcChooseParachuteItem.isChange = false;
            return;
        }
        if (attache != null)
        {
            var gComp = attache.entity.Get<GameObjectComponent>();
            RemoveUgcItem(attache.entity.Get<ParachuteBagComponent>().rid, gComp.uid, ParaUgcType.Bag);
            SecondCachePool.Inst.DestroyEntity(gComp.bindGo);
        }
    }

    public ParaUgcType GetParachuteType(SceneEntity entity)
    {
        if (entity.HasComponent<ParachuteComponent>())
        {
            return ParaUgcType.Parachute;
        }
        else if (entity.HasComponent<ParachuteBagComponent>())
        {
            return ParaUgcType.Bag;
        }
        return ParaUgcType.None;
    }

    //单独走降落伞克隆
    public void ParachuteClone(NodeBaseBehaviour oBehav, NodeBaseBehaviour nBehav)
    {
        if (nBehav.entity == null) return;
        var ugcType = GetParachuteType(nBehav.entity);
        switch (ugcType)
        {
            case ParaUgcType.Parachute:
                ParachuteCreater.OnClone(oBehav, nBehav);
                var pComp = nBehav.entity.Get<ParachuteComponent>();
                var bagBehav = ByIdFindBehav(pComp.parachuteBagUid, ParachuteManager.Inst.allBagUgcDict);
                if (bagBehav != null)
                {
                    var bag = SceneBuilder.Inst.CloneTarget(bagBehav.gameObject, false);
                    var newBag = bag.GetComponent<NodeBaseBehaviour>();
                    ParachuteBagCreater.OnClone(newBag);
                    if (newBag != null)
                    {
                        var bComp = newBag.entity.Get<ParachuteBagComponent>();
                        pComp.parachuteBagUid = newBag.entity.Get<GameObjectComponent>().uid;
                        bComp.parachuteUid = nBehav.entity.Get<GameObjectComponent>().uid;
                    }
                    OnCloneBagAddProp(newBag);
                }
                break;
        }
    }

    private void OnCloneBagAddProp(NodeBaseBehaviour behav)
    {
        if(behav!=null && behav.entity != null)
        {
            if (behav.entity.HasComponent<PickablityComponent>())
            {
                PickabilityManager.Inst.AddPickablityProp(behav.entity, behav.entity.Get<PickablityComponent>().anchors);
            }
        }
    }

    public void OnDisSelectTarget(GameObject target)
    {
        if(target != null)
        {
           var behav = target.GetComponent<NodeBaseBehaviour>();
            if(behav != null)
            {
                if (behav.entity.HasComponent<ParachuteComponent>())
                {
                    HideBag(target);
                }
            }
        }
    }

    public void SetPickNodeRot(Transform pickNode)
    {
        pickNode.localEulerAngles = paraRot;
    }

    #region 特效
    public void PlayPickEffect(string playerId, SceneEntity entity,PICK_STATE state)
    {
        if (!entity.HasComponent<ParachuteComponent>() 
            || playerId != GameManager.Inst.ugcUserInfo.uid 
            || GlobalFieldController.CurGameMode == GameMode.Edit)
        {
            return;
        }
        if (mParachutePickEffectObj == null)
        {
            mParachutePickEffectObj = GenEffectObj(mParachutePickEffectPath);
        }
        if (mParachutePickEffectObj != null)
        {
            SetEffectTransform(mParachutePickEffectObj.transform);
            mParachutePickEffectObj.SetActive(true);
            var anim = mParachutePickEffectObj.GetComponentInChildren<Animator>();
            switch (state)
            {
                case PICK_STATE.DROP:
                    anim.Play("parachuting_fan");
                    AKSoundManager.Inst.PostEvent("Play_Put_Objects", mParachutePickEffectObj);
                    break;
                case PICK_STATE.CATCH:
                    anim.Play("parachuting_shou");
                    AKSoundManager.Inst.PostEvent("Play_Pickup_Objects", mParachutePickEffectObj);
                    break;
            }
            if (closeEffect != null)
            {
                CoroutineManager.Inst.StopCoroutine(closeEffect);
            }
            closeEffect = CoroutineManager.Inst.StartCoroutine(CloseEffect(0.5f));
        }
    }

    private IEnumerator CloseEffect(float time)
    {
        yield return new WaitForSeconds(time);
        if (mParachutePickEffectObj != null)
        {
            mParachutePickEffectObj.SetActive(false);
        }
    }

    private GameObject GenEffectObj(string effectpath)
    {
        GameObject obj = null;
        GameObject runEffectPrefab = ResManager.Inst.LoadRes<GameObject>(effectpath);
        if (runEffectPrefab == null)
        {
            Debug.LogError($"res is null  path is {effectpath}");
        }
        else
        {
            obj = GameObject.Instantiate(runEffectPrefab);
        }
        return obj;
    }

    private void SetEffectTransform(Transform effectTran)
    {
        effectTran.SetParent(PlayerBaseControl.Inst.playerAnim.transform, false);
        effectTran.localPosition = Vector3.zero;
        effectTran.localRotation = Quaternion.identity;
        effectTran.localScale = Vector3.one;
    }
    #endregion

    #region 联机otherPlayer表现

    public const string StateStr_ParachuteStateParam = "Parachute";
    public const string StateStr_IdleState = "idle";
    public const string StateStr_ParachuteGlideIdle = "ParachuteGlideIdle";
    public const string StateStr_ParachuteGlideMove = "ParachuteGlideMove";
    public const string StateStr_ParachuteGlidePreLand = "ParachuteGlidePreLand";
    public const string StateStr_ParachuteFallReady = "ParachuteFallReady";
    public const string StateStr_ParachuteFallPreLand = "ParachuteFallPreLand";
    public const string StateStr_ParachuteFallingIdle = "ParachuteFallingIdle";
    public const string StateStr_ParachuteFallingMoveForward = "ParachuteFallingForward";
    public const string StateStr_ParachuteFallingMoveLeft = "ParachuteFallingLeft";
    public const string StateStr_ParachuteFallingMoveBackward = "ParachuteFallingBackward";
    public const string StateStr_ParachuteFallingMoveRight = "ParachuteFallingRight";

    public void SetParachuteState(Animator playerAnim, int para, string stateStr)
    {
        if (playerAnim == null)
        {
            return;
        }

        playerAnim.SetInteger(StateStr_ParachuteStateParam, para);
        playerAnim.Update(0f);
        playerAnim.CrossFadeInFixedTime(stateStr, 0.2f);
    }

    //处理otherPlayer帧状态切换
    public void HandleFrameState(OtherPlayerCtr otherPlayerCtr, OtherPlayerAnimStateManager animStateManager, Animator playerAnim, FrameStateType stateType)
    {
        if (otherPlayerCtr == null || animStateManager == null || playerAnim == null)
        {
            return;
        }
        var otherPlayerData = otherPlayerCtr.GetComponent<PlayerData>();
        switch (stateType)
        {
            case FrameStateType.NoState:
                SetParachuteState(playerAnim, (int) ParachuteAnimState.NoUse, StateStr_IdleState);
                if (otherPlayerData != null && otherPlayerData.syncPlayerInfo != null)
                {
                    SwitchParachute(otherPlayerData.syncPlayerInfo.uid, false);
                }
                break;
            case FrameStateType.ParachuteGlidingIdle:
                SetParachuteState(playerAnim, (int) ParachuteAnimState.GlidingIdle, StateStr_ParachuteGlideIdle);
                animStateManager.SwitchTo(EPlayerAnimState.ParachuteGlideIdle);
                break;
            case FrameStateType.ParachuteGlidingMove:
                SetParachuteState(playerAnim, (int) ParachuteAnimState.GlidingMove, StateStr_ParachuteGlideMove);
                animStateManager.SwitchTo(EPlayerAnimState.ParachuteGlideMove);
                break;
            case FrameStateType.ParachuteGlidingPreLand:
                SetParachuteState(playerAnim, (int) ParachuteAnimState.GlidingPreLand, StateStr_ParachuteGlidePreLand);
                PlaySound("FrontFlip", "Play_Parachute_Ground", "Parachute_Ground", otherPlayerCtr.gameObject);
                TimerManager.Inst.RunOnce("OtherGlidingPreLandTimer", 0.5f, () =>
                {
                    if (animStateManager != null)
                    { 
                        animStateManager.SwitchTo(EPlayerAnimState.ParachuteGlidePreLandEnd);
                    }
                });
                break;
            case FrameStateType.ParachuteFallingReady: //开伞
                if (otherPlayerData != null)
                {
                    SwitchParachute(otherPlayerData.syncPlayerInfo.uid, true);
                }
                SetParachuteState(playerAnim, (int) ParachuteAnimState.FallingReady, StateStr_ParachuteFallReady);
                animStateManager.SwitchTo(EPlayerAnimState.ParachuteOpenParachute);
                PlaySound("Play_Parachute_Open", otherPlayerCtr.gameObject);
                break;
            case FrameStateType.ParachuteFallingPreLand: //收伞
                if (otherPlayerData != null)
                {
                    SwitchParachute(otherPlayerData.syncPlayerInfo.uid, false);
                }
                animStateManager.SwitchTo(EPlayerAnimState.ParachuteCloseParachute);
                PlaySound("Play_Parachute_Disappear", otherPlayerCtr.gameObject);
                
                TimerManager.Inst.RunOnce("OtherCloseParachuteTimer", 0.3f, () =>
                {
                    //收伞后翻滚
                    SetParachuteState(playerAnim, (int) ParachuteAnimState.FallingPreLand, StateStr_ParachuteFallPreLand);
                    PlaySound("Roll", "Play_Parachute_Ground", "Parachute_Ground", otherPlayerCtr.gameObject);
        
                    TimerManager.Inst.RunOnce("OtherFallingPreLandEndTimer", 0.3f, () =>
                    {
                        animStateManager.SwitchTo(EPlayerAnimState.ParachuteFallingPreLandEnd);
                    });
                });
                break;
            case FrameStateType.ParachuteFallingIdle:
                SetParachuteState(playerAnim, (int) ParachuteAnimState.FallingIdle, StateStr_ParachuteFallingIdle);
                animStateManager.SwitchTo(EPlayerAnimState.ParachuteFalling);
                break;
            case FrameStateType.ParachuteFallingMoveForward:
                SetParachuteState(playerAnim, (int) ParachuteAnimState.FallingMoveForward, StateStr_ParachuteFallingMoveForward);
                animStateManager.SwitchTo(EPlayerAnimState.ParachuteFalling);
                break;
            case FrameStateType.ParachuteFallingMoveLeft:
                SetParachuteState(playerAnim, (int) ParachuteAnimState.FallingMoveLeft, StateStr_ParachuteFallingMoveLeft);
                animStateManager.SwitchTo(EPlayerAnimState.ParachuteFalling);
                break;
            case FrameStateType.ParachuteFallingMoveBackward:
                SetParachuteState(playerAnim, (int) ParachuteAnimState.FallingMoveBackward, StateStr_ParachuteFallingMoveBackward);
                animStateManager.SwitchTo(EPlayerAnimState.ParachuteFalling);
                break;
            case FrameStateType.ParachuteFallingMoveRight:
                SetParachuteState(playerAnim, (int) ParachuteAnimState.FallingMoveRight, StateStr_ParachuteFallingMoveRight);
                animStateManager.SwitchTo(EPlayerAnimState.ParachuteFalling);
                break;
        }
    }

    
    #endregion

    #region 声音控制

    public void PlaySound(string eventName, GameObject obj)
    {
        if (obj)
        {
            AKSoundManager.Inst.PostEvent(eventName, obj);
        }
    }

    public void PlaySound(string switchName, string eventName, string groupName, GameObject obj)
    {
        if (obj)
        {
            AKSoundManager.Inst.PlayAttackSound(switchName, eventName, groupName, obj);
        }
    }
    
    public void StopAllSound(GameObject gameObject)
    {
        if (gameObject)
        {
            PlaySound("Stop_Parachute_Gliding_Fly", gameObject);
            PlaySound("Stop_Parachute_Fall_Fly", gameObject);
        }
    }

    #endregion
    
    public void OnReset()
    {
        if (PlayerParachuteControl.Inst)
        {
            PlayerParachuteControl.Inst.ForceStopParachute();
        }
    }
}
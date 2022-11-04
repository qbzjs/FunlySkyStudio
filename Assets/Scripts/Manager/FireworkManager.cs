using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using Entitas;

/// <summary>
/// Author:Meimei-LiMei
/// Description:烟花道具管理类：限制数量、联机交互、UndoRedo,ugc素材
/// Date: 2022/7/20 16:59:53
/// </summary>
public class FireworkManager : ManagerInstance<FireworkManager>, IManager, IUGCManager
{
    public const string DEFAULT_MODEL = "DEFAULT_MODEL";//默认占位图的Key
    public UgcChooseItem lastSelectfirework;
    //记录所有烟花特效
    private Dictionary<NodeBaseBehaviour, FireworkEffectBehaviour> allFireworksDict = new Dictionary<NodeBaseBehaviour, FireworkEffectBehaviour>();
    public Dictionary<string, List<NodeBaseBehaviour>> ugcFireworksDict = new Dictionary<string, List<NodeBaseBehaviour>>();
    private int MaxCount = 99;
    public void Init()
    {
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
    }

    #region 烟花数量限制处理
    /// <summary>
    /// 烟花是否达到最大数量
    /// </summary>
    /// <returns></returns>
    public bool IsOverMaxCount()
    {
        if (GetFireworkCount() >= MaxCount)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 获取当前所有烟花数
    /// </summary>
    /// <returns></returns>
    public int GetFireworkCount()
    {
        int curNum = 0;
        foreach (var fireworkList in ugcFireworksDict.Values)
        {
            curNum += fireworkList.Count;
        }
        return curNum;
    }
    /// <summary>
    /// 判断是否能够继续克隆
    /// </summary>
    /// <param name="curTarget">当前选中的对象</param>
    /// <returns></returns>
    public bool IsCanClone(GameObject curTarget)
    {
        int curNum = 0;
        curNum = GetFireworkCount();
        foreach (var item in curTarget.GetComponentsInChildren<NodeBaseBehaviour>())
        {
            if (item.entity.HasComponent<FireworkComponent>())
            {
                curNum++;
                if (curNum > MaxCount)
                {
                    TipPanel.ShowToast("Oops! Exceed limit:(");
                    return false;
                }
            }
        }
        if (IsOverMaxCount())
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return false;
        }
        return true;
    }
    #endregion

    #region 烟花特效行为相关处理
    public void AddFireworksEffectBehv(NodeBaseBehaviour behaviour)
    {
        if (!allFireworksDict.ContainsKey(behaviour))
        {
            FireworkEffectBehaviour fireworkEffect = new FireworkEffectBehaviour(behaviour);
            allFireworksDict.Add(behaviour, fireworkEffect);
        }
    }
    public void RemoveFireworkEffectBehv(NodeBaseBehaviour behaviour)
    {
        if (allFireworksDict.ContainsKey(behaviour))
        {
            allFireworksDict.Remove(behaviour);
        }
    }
    public FireworkEffectBehaviour GetFireworkEffectBehv(NodeBaseBehaviour behaviour)
    {
        if (allFireworksDict.ContainsKey(behaviour))
        {
            return allFireworksDict[behaviour];
        }
        return null;
    }
    /// <summary>
    /// 重置烟花显隐
    /// </summary>
    /// <param name="visible"></param>
    public void SetAllFireworkVisible(bool visible)
    {
        foreach (var fireworkEffectBehav in allFireworksDict.Values)
        {
            if (fireworkEffectBehav.fireworkEffectTs != null)
            {
                var fireworkeffect = fireworkEffectBehav.fireworkEffectTs.GetComponent<FireworkEffect>();
                if (fireworkeffect != null)
                {
                    fireworkeffect.DestroyFirework();
                }
            }
        }
    }
    #endregion
    /// <summary>
    /// 添加初始化的烟花组件（创建上一个UGC素材及选择ugc素材后调用）
    /// </summary>
    /// <param name="behaviour"></param>
    /// <param name="rId">素材id</param>
    public void AddFireworkComponent(NodeBaseBehaviour behaviour, string rId)
    {
        if (behaviour != null)
        {
            var cmp = behaviour.entity.Get<FireworkComponent>();
            cmp.rId = rId;
            cmp.fireworkcolor = "#F4F4F4";
            cmp.fireworkHeight = 7;
            cmp.anchorsPos = Vector3.zero;
            cmp.isCustomPoint = (int)FireworkPointState.Off;
            cmp.isControl = (int)FireworkControl.NOT_SUPPORT;
            var gameComponent = behaviour.entity.Get<GameObjectComponent>();
            gameComponent.modId = (int)GameResType.Firework;
            gameComponent.handleType = NodeHandleType.Firework;
            gameComponent.modelType = NodeModelType.Firework;
            RefreshButtonCanTouch(behaviour);
        }
    }

    #region Ugc素材添加处理
    //在编辑模式创建烟花
    public NodeBaseBehaviour CreateFireworkBeavInEdit(Vector3 pos)
    {
        NodeBaseBehaviour nBev = null;
        var manager = FireworkManager.Inst;
        var lastSelectUgcInfo = manager.GetLastSelectUgcMapInfo();
        if (lastSelectUgcInfo == null || string.IsNullOrEmpty(lastSelectUgcInfo.mapJsonContent))
        {
            //创建默认烟花道具
            nBev = manager.CreateDefaultNode();
        }
        else
        {
            //创建上一个UGC素材并作为烟花道具
            var mapInfo = lastSelectUgcInfo.mapInfo;
            nBev = UgcChooseManager.Inst.CreateSingleUgcAsProp(pos, mapInfo, mapInfo.mapId, lastSelectUgcInfo.mapJsonContent);
            manager.AddUgcFireworkItem(mapInfo.mapId, nBev);
            manager.AddFireworkComponent(nBev, mapInfo.mapId);
        }
        return nBev;
    }
    /// <summary>
    /// 创建地图时向UGC素材添加烟花组件
    /// </summary>
    public void AddFireworkComponentToUGC(NodeBaseBehaviour behaviour, NodeData data)
    {
        var FireworkKV = data.attr.Find(x => x.k == (int)BehaviorKey.Firework);
        if (FireworkKV != null)
        {
            var fireworkData = JsonConvert.DeserializeObject<FireworkData>(FireworkKV.v);
            var comp = behaviour.entity.Get<FireworkComponent>();
            comp.rId = fireworkData.rId;
            comp.fireworkcolor = fireworkData.fireworkcolor;
            comp.fireworkHeight = fireworkData.fireworkHeight;
            comp.anchorsPos = fireworkData.anchorsPos;
            comp.isCustomPoint = fireworkData.isCustomPoint;
            comp.isControl = fireworkData.isControl;
            AddUgcFireworkItem(fireworkData.rId, behaviour);
            var gameComponent = behaviour.entity.Get<GameObjectComponent>();
            gameComponent.modId = (int)GameResType.Firework;
            gameComponent.handleType = NodeHandleType.Firework;
            gameComponent.modelType = NodeModelType.Firework;
            RefreshButtonCanTouch(behaviour);
        }
    }
    public void RefreshButtonCanTouch(NodeBaseBehaviour nodeBehav)
    {
        if (nodeBehav.entity.HasComponent<FireworkComponent>())
        {
            var childColliders = nodeBehav.transform.GetComponentsInChildren<Collider>();
            for (int i = 0; i < childColliders.Length; i++)
            {
                GameObject childGO = childColliders[i].gameObject;
                if (childGO != null)
                {
                    childGO.layer = LayerMask.NameToLayer("Touch");
                }
            }
        }
    }
    //添加ugc烟花节点
    public void AddUgcFireworkItem(string rId, NodeBaseBehaviour ugcBeav)
    {
        AddUgcFireworkList(rId);
        ugcFireworksDict[rId].Add(ugcBeav);
        LoggerUtils.Log($"FireworkManager: AddUgcFireworkItem --{rId}--{ugcBeav.gameObject.name}");
    }
    //根据素材id添加ugc素材list
    public void AddUgcFireworkList(string rid)
    {
        if (!ugcFireworksDict.ContainsKey(rid))
        {
            ugcFireworksDict.Add(rid, new List<NodeBaseBehaviour>());
        }
    }
    //记录最后一个选择的ugc烟花
    public void SetLastSelectFirework(UgcChooseItem fireworkItem)
    {
        if (fireworkItem != null)
        {
            lastSelectfirework = fireworkItem;
        }
    }
    //获取最后选择的ugc烟花
    public UgcChooseItem GetLastSelectUgcMapInfo()
    {
        return lastSelectfirework;
    }

    /// <summary>
    /// 获取所有正在用作烟花的UGC Rid集合排除了默认占位道具
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllUgcRidList()
    {
        if (ugcFireworksDict != null)
        {
            var tempList = ugcFireworksDict.Keys.ToList();
            if (tempList.Contains(DEFAULT_MODEL))
            {
                tempList.Remove(DEFAULT_MODEL);
            }
            return tempList;
        }
        return null;
    }
    /// <summary>
    /// 获取所有默认道具的Beav
    /// </summary>
    /// <returns></returns>
    public List<NodeBaseBehaviour> GetAllDefaultNodeBeav()
    {
        if (ugcFireworksDict.ContainsKey(DEFAULT_MODEL))
        {
            return ugcFireworksDict[DEFAULT_MODEL];
        }
        return null;
    }
    #endregion
    /// <summary>
    /// 监听组合面板打开，打开组合，隐藏默认烟花道具
    /// </summary>
    /// <param name="isShow"></param>
    protected void HandlePackPanelShow(bool isShow)
    {
        SetDefaultModeShow(!isShow);
    }
    /// <summary>
    /// 切换模式时控制烟花模型的显示
    /// </summary>
    /// <param name="mode"></param>
    protected void OnChangeMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Edit:
                SetDefaultModeShow(true);
                SetAllFireworkVisible(true);
                break;
            case GameMode.Play:
            case GameMode.Guest:
                SetDefaultModeShow(false);
                break;
        }
    }
    //控制默认烟花道具显隐
    public void SetDefaultModeShow(bool isShow)
    {
        var defalutModels = GetAllDefaultNodeBeav();
        if (defalutModels != null)
        {
            foreach (var fireworkModel in defalutModels)
            {
                fireworkModel.gameObject.SetActive(isShow);
            }
        }
    }
    public void Clear()
    {
        allFireworksDict.Clear();
    }
    //创建默认的烟花模型
    public NodeBaseBehaviour CreateDefaultNode()
    {
        var newBev = SceneBuilder.Inst.CreateSceneNode<FireworkCreater, FireworkBehaviour>();
        FireworkCreater.SetData((FireworkBehaviour)newBev, new FireworkData()
        {
            rId = DEFAULT_MODEL,
        });
        return newBev;
    }
    #region  烟花发射锚点
    public Vector3 GetAnchors(SceneEntity entity)
    {
        var pCom = entity.Get<FireworkComponent>();
        return pCom.anchorsPos;
    }
    public void SetAnchors(SceneEntity entity, Vector3 pos)
    {
        if (!entity.HasComponent<FireworkComponent>()) return;
        var pCom = entity.Get<FireworkComponent>();
        pCom.anchorsPos = pos;
    }
    #endregion
    /// <summary>
    /// 烟花的触发事件
    /// </summary>
    /// <param name="behaviour"></param>
    /// <param name="isSendReq"></param>
    public void OnTriggerFirework(NodeBaseBehaviour behaviour)
    {
        AddFireworksEffectBehv(behaviour);
        allFireworksDict[behaviour].OnTriggerFirework();
    }
    public void OnPlayFirework(NodeBaseBehaviour behaviour)
    {
        AddFireworksEffectBehv(behaviour);
        allFireworksDict[behaviour].PlayFirework();//直接播放烟花
    }
    #region  联机交互
    /// <summary>
    /// 发送烟花播放请求给服务端 
    /// </summary>
    /// <param name="fireworkpropUid"></param>
    public void SendFireworkPlayReq(int fireworkpropUid)
    {
        Item[] itemsArray =
       {
            new Item()
            {
                id = fireworkpropUid,
                type = (int) ItemType.FIREWORK,
            }
        };
        SyncItemsReq itemsReq = new SyncItemsReq()
        {
            mapId = GlobalFieldController.CurMapInfo.mapId,
            items = itemsArray,
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Firework,
            data = JsonConvert.SerializeObject(itemsReq),
        };
        LoggerUtils.Log($"SendFireworkRestoreReq => {JsonConvert.SerializeObject(roomChatData)}");
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }
    /// <summary>
    /// 接收服务端广播
    /// </summary>
    /// <param name="sendPlayerId"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public bool OnReceiveServer(string sendPlayerId, string msg)
    {
        LoggerUtils.Log("FirworkManager OnReceiveServer ==> => senderPlayer:" + sendPlayerId + "msg:" + msg);
        SyncItemsReq itemsReq = JsonConvert.DeserializeObject<SyncItemsReq>(msg);
        string mapId = itemsReq.mapId;
        if (GlobalFieldController.CurMapInfo != null && mapId == GlobalFieldController.CurMapInfo.mapId)
        {
            foreach (var item in itemsReq.items)
            {
                if (item.type == (int)ItemType.FIREWORK)
                {
                    if (string.IsNullOrEmpty(item.data))
                    {
                        LoggerUtils.Log("[FirworkManager.OnReceiveServer getItemsRsp.item.Data is null");
                    }
                    var uid = item.id;
                    // 烟花道具同步播放
                    var behaviour = GetFireworkByUid(uid);
                    if (behaviour)
                    {
                        LoggerUtils.Log("[FirworkManager.OnReceiveServer behaviour");
                        OnPlayFirework(behaviour);
                    }
                }
            }
        }
        return true;
    }
    public NodeBaseBehaviour GetFireworkByUid(int uid)
    {
        foreach (var list in ugcFireworksDict.Values)
        {
            foreach (var fireworkBev in list)
            {
                if (fireworkBev != null)
                {
                    var gComp = fireworkBev.entity.Get<GameObjectComponent>();
                    if (uid == gComp.uid)
                    {
                        return fireworkBev;
                    }
                }
            }
        }
        return null;
    }
    #endregion

    #region UNDO/REDO
    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        if (behaviour.entity.HasComponent<FireworkComponent>())
        {
            var rid = behaviour.entity.Get<FireworkComponent>().rId;
            AddUgcFireworkItem(rid, behaviour);
        }
    }
    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        if (!behaviour)
        {
            return;
        }
        if (behaviour.entity == null)
        {
            return;
        }
        if (behaviour.entity.HasComponent<FireworkComponent>())
        {
            string removeRid = behaviour.entity.Get<FireworkComponent>().rId;//ugc素材ID
            if (!string.IsNullOrEmpty(removeRid) && ugcFireworksDict.ContainsKey(removeRid))
            {
                var rmList = ugcFireworksDict[removeRid];//ugc素材对应的behaviour List
                rmList.Remove(behaviour);//移除该behaviour            
                if (rmList.Count <= 0)//场景内已无该UGC素材使用记录，清除数据
                {
                    ugcFireworksDict.Remove(removeRid);
                }
            }
        }
    }
    #endregion
    public override void Release()
    {
        base.Release();
        Clear();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
    }
    public void OnUGCChangeStatus(UGCCombBehaviour ugcCombBehaviour)
    {
        RefreshButtonCanTouch(ugcCombBehaviour);
    }
}


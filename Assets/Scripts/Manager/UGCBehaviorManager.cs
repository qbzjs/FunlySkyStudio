/// <summary>
/// Author:YangJie
/// Description: UGC Behavior Manager
/// Date: 2022/3/30 18:28:5
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HLODSystem;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

public class UGCBehaviorManager : ManagerInstance<UGCBehaviorManager>, IManager
{
    public int cannotRenderABResCount = 0;
 
    private Dictionary<string, List<UGCCombBehaviour>> ugcBehaviours = new Dictionary<string, List<UGCCombBehaviour>>();
    
    // 保存删除的 UGC节点数据，为undo，redo恢复使用
    private Dictionary<string, List<NodeData>> tmpUgcNodeData = new Dictionary<string, List<NodeData>>();
 
    public readonly List<int> specileNodeModTypes = new List<int>
    {
        (int)NodeModelType.DText , (int)NodeModelType.PGC, (int) NodeModelType.NewDText
    };
    public readonly List<int> specileNodeModTypesDText = new List<int>  //包含pgc和新版3d文字还原
    {
        (int)NodeModelType.PGC, (int) NodeModelType.NewDText
    };
    
    private List<IUGCManager> allUgcManagers = new List<IUGCManager>();
    public void Init()
    {
        allUgcManagers.Add(BloodPropManager.Inst);
        allUgcManagers.Add(FireworkManager.Inst);
        allUgcManagers.Add(FreezePropsManager.Inst);
        allUgcManagers.Add(PromoteManager.Inst);
        allUgcManagers.Add(EdibilityManager.Inst);
        allUgcManagers.Add(SeesawManager.Inst);
        allUgcManagers.Add(VIPZoneManager.Inst);
        MessageHelper.AddListener<bool>(MessageName.DebugStateChange, OnDebugStateChange);
        Clear();
    }

    public void OnUGCChangeStatus(UGCCombBehaviour ugcCombBehaviour)
    {
        allUgcManagers.ForEach(x=>x.OnUGCChangeStatus(ugcCombBehaviour));
    }
    
    public override void Release()
    {
        base.Release();
        Clear();
        allUgcManagers.Clear();
        MessageHelper.RemoveListener<bool>(MessageName.DebugStateChange, OnDebugStateChange);
    }
    public void AddNode(NodeBaseBehaviour baseBehaviour, NodeData data)
    {
        if (!(baseBehaviour is UGCCombBehaviour behaviour) || data == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(data.rid))
        {
            if (!ugcBehaviours.ContainsKey(data.rid))
            {
                ugcBehaviours.Add(data.rid, new List<UGCCombBehaviour>());
            }

            if (!ugcBehaviours[data.rid].Contains(behaviour))
            {
                ugcBehaviours[data.rid].Add(behaviour);
            }
            if (!GlobalFieldController.ugcNodeData.ContainsKey(data.rid))
            {
                GlobalFieldController.ugcNodeData.Add(data.rid, data.prims);
            }
            else
            {
                data.prims = GlobalFieldController.ugcNodeData[data.rid];
            }
        }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        if (!(behaviour is UGCCombBehaviour ugcBehaviour))
        {
            return;
        }
        var comp = behaviour.entity.Get<GameObjectComponent>();
        if (string.IsNullOrEmpty(comp.resId))
        {
            return;
        }
        ugcBehaviours.TryGetValue(comp.resId, out var tmpUgcBehaviours);
        if (tmpUgcBehaviours != null && tmpUgcBehaviours.Contains(behaviour))
        {
            tmpUgcBehaviours.Remove(ugcBehaviour);
            if (tmpUgcBehaviours.Count <= 0)
            {
                ugcBehaviours.Remove(comp.resId);
                if (GlobalFieldController.ugcNodeData.ContainsKey(comp.resId))
                {
                    if (!tmpUgcNodeData.ContainsKey(comp.resId))
                    {
                        tmpUgcNodeData.Add(comp.resId, GlobalFieldController.ugcNodeData[comp.resId]);
                    }
                    GlobalFieldController.ugcNodeData.Remove(comp.resId);
                }
            }
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
       if (!(behaviour is UGCCombBehaviour ugcCombBehaviour))
       {
           return;
       } 
       var resId = behaviour.entity.Get<GameObjectComponent>().resId; 
       if (!string.IsNullOrEmpty(resId))
       {
           if (!ugcBehaviours.ContainsKey(resId))
           {
               ugcBehaviours.Add(resId, new List<UGCCombBehaviour>());
           }            
           if (!ugcBehaviours[resId].Contains(ugcCombBehaviour))
           {
               ugcBehaviours[resId].Add(ugcCombBehaviour);
           }
           if (!GlobalFieldController.ugcNodeData.ContainsKey(resId) && tmpUgcNodeData.ContainsKey(resId))
           {
               GlobalFieldController.ugcNodeData[resId] = tmpUgcNodeData[resId];
           }
           if (GlobalFieldController.whiteListMask.IsInWhiteList(WhiteListMask.WhiteListType.OfflineRender)) {
               ugcCombBehaviour.SetLODStatus(HLODState.High);
           }
       }
    }



    public void Clear()
    {
        ClearOfflineData();
    }

    private void OnDebugStateChange(bool isShow)
    {
        foreach (var ugcBehaviours in ugcBehaviours)
        {
            foreach (var ugcBehaviour in ugcBehaviours.Value)
            {
                if (isShow)
                {
                    ugcBehaviour.SetRedRender();
                }
                else
                {
                    ugcBehaviour.SetNormalRender();
                }
            }
        }
    }

    public void OnHandleClone(SceneEntity oEntity, SceneEntity nEntity)
    {
        var comp = nEntity.Get<GameObjectComponent>();
        var ugcCombBehaviour = comp.bindGo.GetComponent<UGCCombBehaviour>();
        if (ugcCombBehaviour != null && !string.IsNullOrEmpty(comp.resId))
        {
            if (!ugcBehaviours.ContainsKey(comp.resId))
            {
                ugcBehaviours[comp.resId] = new List<UGCCombBehaviour>();
            }
            ugcBehaviours[comp.resId].Add(ugcCombBehaviour);
        }
    }


    public void InitLocalOfflineRes(MapData mapData)
    {
        Clear();
 #if UNITY_EDITOR
         GlobalFieldController.whiteListMask.SetInWhiteList(WhiteListMask.WhiteListType.OfflineRender);
 #endif
        OfflineResManager.Inst.LoadCacheInfoFile();
        GlobalFieldController.ugcNodeData = GetAllUGCData(mapData);
        var ugcNodeData = GlobalFieldController.ugcNodeData;
        foreach(var nodeData in ugcNodeData){
            bool isAllDText = true;
            bool isPGC = false;
            foreach (var item in nodeData.Value)
            {
                var configData = GameManager.Inst.priConfigData[item.id];
                if((NodeModelType) configData.modType != NodeModelType.DText){
                    isAllDText = false;
                }
                if(item.type == 2)isPGC = true;
            }
            if(isAllDText || isPGC)cannotRenderABResCount++;
        }
        InitOfflineRenderData();
        InitMapOfflineRenderData();
    }
    
    public void InitOfflineRenderData()
    {
        GlobalFieldController.offlineRenderDataDic.Clear();
        if (GlobalFieldController.CurMapInfo == null || GlobalFieldController.CurMapInfo.renderList == null)
        {
            return;
        }
        AddOfflineRenderData(GlobalFieldController.CurMapInfo.renderList);
    }

    public void AddOfflineRenderData(OfflineRenderListObj[] renderListObjs)
    {
        if (renderListObjs == null)
        {
            return;
        }
        foreach (var renderObjList in renderListObjs)
        {
            if (renderObjList.abList != null)
            {
                foreach (var renderData in renderObjList.abList)
                {
                    if (!GlobalFieldController.offlineRenderDataDic.ContainsKey(renderData.mapId))
                    {
                        renderData.version = renderObjList.version;
                        renderData.Init();
                        GlobalFieldController.offlineRenderDataDic.Add(renderData.mapId, renderData);
                    }
                }
            }
        }
    }
    
    private void InitMapOfflineRenderData()
    {
        LoggerUtils.Log("InitOfflineRenderData:" + GlobalFieldController.whiteListMask.IsInWhiteList(WhiteListMask.WhiteListType.OfflineRender));
        if (!GlobalFieldController.whiteListMask.IsInWhiteList(WhiteListMask.WhiteListType.OfflineRender) || GlobalFieldController.CurMapInfo == null || GlobalFieldController.CurMapInfo.renderList == null)
        {
            OfflineResManager.Inst.Clear();
            return;
        }

        foreach (var tmpKey in GlobalFieldController.offlineRenderDataDic.Keys.ToList())
        {
            if (tmpKey != GlobalFieldController.CurMapInfo.mapId && !GlobalFieldController.ugcNodeData.ContainsKey(tmpKey) )
            {
                GlobalFieldController.offlineRenderDataDic.Remove(tmpKey);
            }
        }
    }

    
    private void ClearOfflineData()
    {
        tmpUgcNodeData.Clear();
        OfflineResManager.Inst.loadingDic.Clear();
        GlobalFieldController.offlineRenderDataDic.Clear();
        cannotRenderABResCount = 0;


    }
    
    public Dictionary<string, List<NodeData>> GetAllUGCData(MapData mapData)
    {
        var ugcNodeData = new Dictionary<string, List<NodeData>>();
        if (mapData == null)
        {
            return ugcNodeData;
        }
        LoggerUtils.Log("MapData Version:" + mapData.version);
        if (mapData.version == 1 && mapData.resList != null)
        {
            return mapData.resList;
        }
        
        GetChildUGC(mapData.pref, ugcNodeData);
        return ugcNodeData;
    }

    private void GetChildUGC(List<NodeData> pref, Dictionary<string, List<NodeData>> ugcNodeData)
    {
        if (pref == null)
        {
            return;
        }
        foreach (var nodeData in pref)  
        {
            if ((ResType)nodeData.type == ResType.UGC)
            {
                if (!string.IsNullOrEmpty(nodeData.rid) && !ugcNodeData.ContainsKey(nodeData.rid))
                {
                    ugcNodeData.Add(nodeData.rid, nodeData.prims);
                }
            }
            else if (nodeData.prims != null)
            {
                GetChildUGC(nodeData.prims, ugcNodeData);
            }
        }
    }
    public void SetAllSoldOutList()
    {
        
        DcSaveInfo[] list = GameManager.Inst.gameMapInfo.dcList;
        if (list!=null)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].isSoldOut == 1)
                {
                    SetSoldOut(list[i]);
                }
            }
        }
        
    }
    public void OnDcSoldOut(string msg)
    {
        CustomData data = JsonConvert.DeserializeObject<CustomData>(msg);
        DcSaveInfo info = JsonConvert.DeserializeObject<DcSaveInfo>(data.data);
        SetSoldOut(info);
    }
    public void SetSoldOut(DcSaveInfo info)
    {
        foreach (var item in ugcBehaviours.Values)
        {
            for (int k = 0; k < item.Count; k++)
            {
                if (item[k].entity.HasComponent<DcComponent>())
                {
                    var com = item[k].entity.Get<DcComponent>();
                    if (com.dcId == info.dcId && com.address == info.address)
                    {
                        item[k].SetSoldOut();
                    }
                }
            }
        }
    }

}

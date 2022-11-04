
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HLODSystem;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

public class PGCBehaviorManager : ManagerInstance<PGCBehaviorManager>, IManager
{
    
    private Dictionary<string, List<PGCBehaviour>> pgcBehaviours = new Dictionary<string, List<PGCBehaviour>>();
    
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
        foreach (var item in pgcBehaviours.Values)
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

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        if (!(behaviour is PGCBehaviour pgcBehaviour))
        {
            return;
        }
        var comp = behaviour.entity.Get<GameObjectComponent>();
        if (string.IsNullOrEmpty(comp.resId))
        {
            return;
        }
        if (!pgcBehaviours.TryGetValue(comp.resId, out var tmpPgcBehaviours) ||
            !tmpPgcBehaviours.Contains(behaviour)) return;

        tmpPgcBehaviours.Remove(pgcBehaviour);
        if (tmpPgcBehaviours.Count <= 0)
        {
            pgcBehaviours.Remove(comp.resId);
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        AddNode(behaviour);
    }

    
    public void AddNode(NodeBaseBehaviour baseBehaviour)
    {
        if (!(baseBehaviour is PGCBehaviour behaviour))return;
        var resId = behaviour.entity.Get<GameObjectComponent>().resId;
        if (string.IsNullOrEmpty(resId)) return;

        if (!pgcBehaviours.ContainsKey(resId))
        {
            pgcBehaviours.Add(resId, new List<PGCBehaviour>());
        }

        if (!pgcBehaviours[resId].Contains(behaviour))
        {
            pgcBehaviours[resId].Add(behaviour);
        }
    }
    
    public void Clear()
    {
        
    }
}

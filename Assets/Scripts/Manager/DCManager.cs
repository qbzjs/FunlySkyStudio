/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/7/21 21:34:18
/// </summary>
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class DCManager : CInstance<DCManager>
{
    private List<DcSaveInfo> dcList = new List<DcSaveInfo>();
    public void AddDCComponentToUGC(UGCCombBehaviour behaviour, NodeData data)
    {
        if (AddDCComponent(behaviour, data))
        {
            behaviour.SetCanBuyInMap();
        }
    }
    
    public void AddDCComponentToPGC(PGCBehaviour behaviour, NodeData data)
    {
        if (AddDCComponent(behaviour, data))
        {
            behaviour.SetCanBuyInMap();
        }
    }

    private bool AddDCComponent(NodeBaseBehaviour behaviour, NodeData data)
    {
        var dcKV = data.attr.Find(x => x.k == (int)BehaviorKey.DC);
       
        if (dcKV != null)
        {
            var dcData = JsonConvert.DeserializeObject<DcData>(dcKV.v);
            behaviour.entity.Get<DcComponent>().dcId = dcData.id;
            behaviour.entity.Get<DcComponent>().isDc = dcData.isDc;
            behaviour.entity.Get<DcComponent>().address = dcData.address;
            behaviour.entity.Get<DcComponent>().budActId = dcData.actId;
            behaviour.entity.Get<UGCPropComponent>().isTradable = 1;
            return true;
        }
        return false;
    }
    
    
    public List<DcSaveInfo> GetDcList()
    {
        dcList.Clear();
        var ugcbehavList = SceneSystem.Inst.FilterNodeBehaviours<UGCCombBehaviour>(SceneBuilder.Inst.allControllerBehaviours);
        foreach (var ugcbehav in ugcbehavList)
        {
            if (ugcbehav.entity.HasComponent<DcComponent>())
            {
                var comp = ugcbehav.entity.Get<DcComponent>();
                var dcinfo = new DcSaveInfo()
                {
                    dcId = comp.dcId,
                    address = comp.address
                };
                dcList.Add(dcinfo);
            }
        }
        
        var pgcBehavList = SceneSystem.Inst.FilterNodeBehaviours<PGCBehaviour>(SceneBuilder.Inst.allControllerBehaviours);
        foreach (var pgcbehav in pgcBehavList)
        {
            if (pgcbehav.entity.HasComponent<DcComponent>())
            {
                var comp = pgcbehav.entity.Get<DcComponent>();
                var dcinfo = new DcSaveInfo()
                {
                    dcId = comp.dcId,
                    address = comp.address
                };
                dcList.Add(dcinfo);
            }
        }
        
        return dcList;
    }
    public List<DcSaveInfo> AddList( List<DcSaveInfo> list)
    {
        GetDcList();
        if (list==null)
        {
            return dcList;
        }
        for (int i = 0; i < dcList.Count; i++)
        {
            list.Add(dcList[i]);
        }
        return list;
    }
    public override void Release()
    {
        base.Release();
        dcList.Clear();
    }
    public void Clear()
    {
        
        dcList.Clear();
    }
}

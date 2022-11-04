using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:PGC植物管理
/// Date: 2022/8/4 13:8:36
/// </summary>

public enum PGCPlantType
{
    greenTree = 0,//绿树
    yellowTree = 1,//黄树
    greenShrub = 2,//绿色灌木丛
    greenGrass = 3,//绿色草丛
}
public class PGCPlantConfig
{
    public int id;
    public int PlantType;
    public string defColor;
    public float minIntensity;
    public float maxIntensity;
}
public class PGCPlantManager : ManagerInstance<PGCPlantManager>, IManager
{
    public int lastChooseID = 11001;//最后一次选择的植物id
    public string lastChooseColor = "16B91B";//最后一次选择的颜色
    private const int MaxCount = 999;
    public const string MAX_COUNT_TIP = "Oops! Exceed limit:(";
    public List<PGCPlantConfig> PGCPlantConfigs { private set; get; }
    public Dictionary<int, PGCPlantConfig> PGCPlantConfigsDic { private set; get; }
    public List<NodeBaseBehaviour> bevs = new List<NodeBaseBehaviour>();
    public bool IsOverMaxCount()//最大限制数量
    {
        if (bevs.Count >= MaxCount)
        {
            return true;
        }
        return false;
    }
    public bool IsCanClone(GameObject curTarget)
    {
        if (curTarget.GetComponentInChildren<PGCPlantBehaviour>() != null)
        {
            int CombineCount = curTarget.GetComponentsInChildren<PGCPlantBehaviour>().Length;
            if (CombineCount > 1)
            {
                if (CombineCount + bevs.Count > MaxCount)
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
            else
            {
                if (IsOverMaxCount())
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
        }
        return true;
    }
    public bool IsPGCPlant(int id)
    {
        if (id >= 11000 && id <= 11999)
        {
            return true;
        }
        return false;
    }
    public void AddItem(NodeBaseBehaviour b)
    {
        if (!bevs.Contains(b))
        {
            bevs.Add(b);
        }
    }
    public int GetLastChooseID()
    {
        return lastChooseID;
    }
    public void UpdateLastChooseID(int id)
    {
        if (id >= 11001 && id <= 11999)
        {
            this.lastChooseID = id;
        }
    }
    public void UpdatelastChooseColor(string color)
    {
        lastChooseColor = color;
    }
    /// <summary>
    /// 动态更改抖动参数
    /// </summary>
    /// <param name="target"></param>
    public void SetIntensity(GameObject target)
    {
        var behav = target.GetComponent<PGCPlantBehaviour>();
        if (behav)
        {
            var comp = behav.entity.Get<GameObjectComponent>();
            var scaleY = target.transform.localScale.y;
            behav.SetIntensity(scaleY, comp.modId);
        }
    }
    public List<PGCPlantConfig> GetPGCPlantConfigList()
    {
        if (PGCPlantConfigs == null)
        {
            PGCPlantConfigs = new List<PGCPlantConfig>();
            PGCPlantConfigs = ResManager.Inst.LoadJsonRes<List<PGCPlantConfig>>("Configs/PGCPlantConfig");
        }
        return PGCPlantConfigs;
    }
    public PGCPlantConfig GetPGCPlantConfigData(int pgcPlantId)
    {
        if (PGCPlantConfigsDic == null)
        {
            PGCPlantConfigsDic = new Dictionary<int, PGCPlantConfig>();
            List<PGCPlantConfig> pgcPlantConfigs = GetPGCPlantConfigList();
            for (int i = 0; i < pgcPlantConfigs.Count; i++)
            {
                PGCPlantConfigsDic.Add(pgcPlantConfigs[i].id, pgcPlantConfigs[i]);
            }
        }
        return PGCPlantConfigsDic[pgcPlantId];
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();

        if (goCmp.modelType == NodeModelType.PGCPlant)
        {
            bevs.Remove(behaviour);
        }
    }
    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.PGCPlant)
        {
            if (!bevs.Contains(behaviour))
            {
                bevs.Add(behaviour);
            }
        }
    }
    public void Clear()
    {
        if (bevs != null)
        {
            bevs.Clear();
        }
    }
    public override void Release()
    {
        base.Release();
        Clear();
    }
}

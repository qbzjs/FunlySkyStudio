using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:PGC特效管理
/// Date: 2022/10/25 14:20:36
/// </summary>
public class PGCEffectConfig
{
    public int id;
    public string soundName;
    public string stopName;
    public int useSound;
    public string defColor;
    public string iconName;
}

public class PGCEffectManager : ManagerInstance<PGCEffectManager>, IManager
{
    public int lastChooseID;//最后一次选择的特效 id
    public int defChooseId = 16001; // 默认选择的特效 id
    public string lastChooseColor = String.Empty;//最后一次选择的颜色
    private const int MaxCount = 99;
    public const string MAX_COUNT_TIP = "Up to 99 Particle Effects can be placed.";
    public List<PGCEffectConfig> PGCEffectConfigs { private set; get; }
    public Dictionary<int, PGCEffectConfig> PGCEffectConfigsDic { private set; get; }
    public List<NodeBaseBehaviour> bevs = new List<NodeBaseBehaviour>();

    public bool IsOverMaxCount()//最大限制数量
    {
        if (bevs.Count >= MaxCount)
        {
            TipPanel.ShowToast(MAX_COUNT_TIP);
            return true;
        }
        return false;
    }

    public bool IsCanClone(GameObject curTarget)
    {
        if (curTarget.GetComponentInChildren<PGCEffectBehaviour>() != null)
        {
            int CombineCount = curTarget.GetComponentsInChildren<PGCEffectBehaviour>().Length;
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
                    return false;
                }
            }
        }
        return true;
    }

    public void OnChangeMode(GameMode mode)
    {
        if (mode == GameMode.Play || mode == GameMode.Guest)
        {
            PlayAllSound();
            PlayAllParticleEffect();
        }
        if (mode == GameMode.Edit)
        {
            StopAllSound();
        }
    }

    private void PlayAllSound()
    {
        foreach (NodeBaseBehaviour b in bevs)
        {
            (b as PGCEffectBehaviour)?.PlaySound(true);
        }
    }

    private void StopAllSound()
    {
        foreach (NodeBaseBehaviour b in bevs)
        {
            (b as PGCEffectBehaviour)?.PlaySound(false);
        }
    }

    private void PlayAllParticleEffect()
    {
        foreach (NodeBaseBehaviour b in bevs)
        {
            (b as PGCEffectBehaviour)?.PlayParticleEffect();
        }
    }

    public bool IsPGCEffect(int id)
    {
        if (id >= 16001 && id <= 16100)
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
        if (id >= 16001 && id <= 16100)
        {
            this.lastChooseID = id;
        }
    }

    public void UpdatelastChooseColor(string color)
    {
        lastChooseColor = color;
    }

    public List<PGCEffectConfig> GetPGCEffectConfigList()
    {
        if (PGCEffectConfigs == null)
        {
            PGCEffectConfigs = new List<PGCEffectConfig>();
            PGCEffectConfigs = ResManager.Inst.LoadJsonRes<List<PGCEffectConfig>>("Configs/PGCEffectConfig");
        }
        return PGCEffectConfigs;
    }

    public PGCEffectConfig GetPGCEffectConfigData(int pgcEffectId)
    {
        if (PGCEffectConfigsDic == null)
        {
            PGCEffectConfigsDic = new Dictionary<int, PGCEffectConfig>();
            List<PGCEffectConfig> PGCEffectConfigs = GetPGCEffectConfigList();
            for (int i = 0; i < PGCEffectConfigs.Count; i++)
            {
                PGCEffectConfigsDic.Add(PGCEffectConfigs[i].id, PGCEffectConfigs[i]);
            }
        }
        return PGCEffectConfigsDic[pgcEffectId];
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();

        if (goCmp.modelType == NodeModelType.PGCEffect)
        {
            bevs.Remove(behaviour);
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.PGCEffect)
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

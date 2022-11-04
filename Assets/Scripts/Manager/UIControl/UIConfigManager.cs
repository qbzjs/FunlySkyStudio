using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description: ui配置管理
/// Date: 2022-6-16 20:00:28
/// </summary>
public class UIConfigManager : CInstance<UIConfigManager>
{
    private readonly string UIPanelConfigPath = "Configs/UIPanelConfig";
    private Dictionary<int, UIPanelConfig> _uiPanelConfigs = new Dictionary<int, UIPanelConfig>();

    public void Init()
    {
        if (_uiPanelConfigs == null)
        {
            _uiPanelConfigs = new Dictionary<int, UIPanelConfig>();
        }
        else
        {
            _uiPanelConfigs.Clear();
        }
        var uiPanels = ResManager.Inst.LoadJsonRes<List<UIPanelConfig>>(UIPanelConfigPath);
        if (uiPanels != null && uiPanels.Count > 0)
        {
            foreach (var panelConfig in uiPanels)
            {
                _uiPanelConfigs.Add(panelConfig.id, panelConfig);
            }
        }
    }

    public override void Release()
    {
        base.Release();
        _uiPanelConfigs.Clear();
        _uiPanelConfigs = null;
    }

    public UIPanelConfig GetUIPanelConfigById(int id)
    {
        if (_uiPanelConfigs.ContainsKey(id))
        {
            return _uiPanelConfigs[id];
        }

        return null;
    }

    public BasePanel GetPanelById(int id)
    {
        if (_uiPanelConfigs.ContainsKey(id))
        {
            return UIManager.Inst.GetPanelByName(_uiPanelConfigs[id].panelName);
        }

        return null;
    }

}

[Serializable]
public class UIPanelConfig
{
    public int id { get; set; }
    public string panelName { get; set; }
    public int isGlobal { get; set; }
}
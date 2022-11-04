using System;
using System.Collections.Generic;

[Serializable]
public class UIControlConfig
{
    public string eventName { get; set; }
    public List<UIControlConfigItem> controls { get; set; }
}

[Serializable]
public class UIControlConfigItem
{
    public int panelId { get; set; }
    public int visibleControl { get; set; } //可见性控制：1:临时隐藏(Active=false), 2：直接关闭(调用Hide) ,3：恢复显示Active=true ,4:直接打开(调用Show)
    public string visibleControlLogic { get; set; } //显隐控制的前置逻辑判定
}

/// <summary>
/// 显隐关系控制
/// </summary>
public enum UIVisibleCtrType
{
    SetActiveTrue = 1,
    SetActiveFalse = 2,
    CallShow = 3,
    CallHide = 4,
}

/// <summary>
/// Author:Shaocheng
/// Description: ui显隐控制管理,只进行UI关联关系之间的显隐控制
/// Date: 2022-6-16 19:33:36
/// </summary>
public class UIControlManager : CInstance<UIControlManager>
{
    private readonly string UIControlConfigPath = "Configs/UIControlConfig";
    private Dictionary<string, List<UIControlConfigItem>> _uiControls = new Dictionary<string, List<UIControlConfigItem>>();
    private Dictionary<string, UIPanelFuncs> _panelFuncs = new Dictionary<string, UIPanelFuncs>();

    /// <summary>
    /// 避免使用反射，本地内存维护String和Panel脚本的关系，方便调用BasePanel.Show()/Hide()
    /// </summary>
    private void InitPanelShowFuncs()
    {
        UIPanelRegFunc.Inst.InitUIPanelFuncs(_panelFuncs);
    }

    private void CheckParamsAndInit()
    {
        if (_uiControls == null)
        {
            _uiControls = new Dictionary<string, List<UIControlConfigItem>>();
        }
        else
        {
            _uiControls.Clear();
        }        
        
        if (_panelFuncs == null)
        {
            _panelFuncs = new Dictionary<string, UIPanelFuncs>();
        }
        else
        {
            _panelFuncs.Clear();   
        }
    }

    public class UIPanelFuncs
    {
        public Action<bool> showFunc { get; set; }
        public Action hideFunc { get; set; }

        public UIPanelFuncs(Action<bool> showFunc, Action hideFunc)
        {
            this.showFunc = showFunc;
            this.hideFunc = hideFunc;
        }
    }

    public void Init()
    {
        CheckParamsAndInit();
        InitPanelShowFuncs();
        var controlConfigs = ResManager.Inst.LoadJsonRes<List<UIControlConfig>>(UIControlConfigPath);
        if (controlConfigs != null && controlConfigs.Count > 0)
        {
            foreach (var config in controlConfigs)
            {
                _uiControls.Add(config.eventName, config.controls);
            }
        }

        UIVisibleControlLogic.Inst.Init();
    }

    public override void Release()
    {
        base.Release();
        _uiControls.Clear();
        _panelFuncs.Clear();
        UIVisibleControlLogic.Inst.Release();
    }

    /// <summary>
    /// 提供给外部调用，触发事件，根据配置控制UI
    /// </summary>
    /// <param name="eventName"></param>
    public void CallUIControl(string eventName)
    {
        if (_uiControls == null)
        {
            LoggerUtils.LogError($"[UIControlManager] _uiControls is null: {eventName}");
            return;
        }
        
        if (!_uiControls.ContainsKey(eventName))
        {
            LoggerUtils.LogError($"[UIControlManager] event name not exists: {eventName}");
            return;
        }

        var controls = _uiControls[eventName];
        if (controls == null || controls.Count <= 0)
        {
            LoggerUtils.LogError($"[UIControlManager] ui controls is null, event: {eventName}");
            return;
        }

#if UNITY_EDITOR
        LoggerUtils.Log($"UIControlManager---CallUIControl Start:{eventName}-{DateTime.Now.Millisecond}");
#endif

        foreach (UIControlConfigItem controlItem in controls)
        {
            if (controlItem == null || controlItem.panelId == 0)
            {
                continue;
            }

            var panelId = controlItem.panelId;
            var panelConfig = UIConfigManager.Inst.GetUIPanelConfigById(panelId);
            if (panelConfig == null)
            {
                LoggerUtils.LogError($"UI Panel not contains config , eventName is :{eventName}");
                continue;
            }

            var panelInstance = UIConfigManager.Inst.GetPanelById(panelId);


            //显隐控制
            UIVisibleControl(panelConfig, panelInstance, (UIVisibleCtrType) controlItem.visibleControl, controlItem.visibleControlLogic);

            //TODO:其他能力
        }


#if UNITY_EDITOR
        LoggerUtils.Log($"UIControlManager---CallUIControl End:{eventName}-{DateTime.Now.Millisecond}");
#endif
    }


    private void UIVisibleControl(UIPanelConfig panelConfig, BasePanel instance, UIVisibleCtrType visibleControl, string controlFuncName)
    {
        var panelClsName = panelConfig.panelName;

        var controlFunc = UIVisibleControlLogic.Inst.GetControlFuncByName(controlFuncName);
        if (controlFunc != null && controlFunc.Invoke() == false)
        {
            return;
        }

        switch (visibleControl)
        {
            case UIVisibleCtrType.SetActiveTrue:
                if (instance != null && instance.gameObject != null && !instance.gameObject.activeInHierarchy)
                {
                    instance.gameObject.SetActive(true);
                }

                break;
            case UIVisibleCtrType.SetActiveFalse:
                if (instance != null && instance.gameObject != null && instance.gameObject.activeInHierarchy)
                {
                    instance.gameObject.SetActive(false);
                }

                break;
            case UIVisibleCtrType.CallShow:
                if (_panelFuncs.ContainsKey(panelClsName))
                {
                    var panelFuncs = _panelFuncs[panelClsName];
                    var isGlobal = panelConfig.isGlobal == 1;
                    panelFuncs.showFunc?.Invoke(isGlobal);
                }

                break;
            case UIVisibleCtrType.CallHide:
                if (_panelFuncs.ContainsKey(panelClsName))
                {
                    var panelFuncs = _panelFuncs[panelClsName];
                    panelFuncs.hideFunc?.Invoke();
                }

                break;

            default: break;
        }
    }
}
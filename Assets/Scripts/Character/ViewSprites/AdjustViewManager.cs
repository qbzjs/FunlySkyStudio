using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Author:Meimei-LiMei
/// Description:调整按钮管理，主要负责上新合集和收藏合集界面的Item的调整按钮事件监听
/// Date: 2022/4/25 11:57:51
/// </summary>
public class AdjustViewManager : BMonoBehaviour<AdjustViewManager>
{
    private RoleAdjustView adjustView;
    private BaseView targetView;
    private BaseView baseView;
    private List<RoleAdjustView> allAdjustView = new List<RoleAdjustView>();
    private void Start()
    {
        foreach (var panel in RoleClassifiyView.Ins.RolePanels)
        {
            var adjustView = panel.GetComponentInChildren<RoleAdjustView>(true);
            if (adjustView)
            {
                allAdjustView.Add(adjustView);
            }
        }
    }
    public void CloseCurrentAdjustView(Action onClose)
    {
        var view = allAdjustView.Find(x => x.gameObject.activeInHierarchy);
        if (view)
        {
            view.OnReturnClick();
            view.ReturnBtn.onClick.RemoveListener(CloseAdjustView);
            onClose?.Invoke();
        }
    }
    public void OpenAdjustView(ClassifyType type, BaseView baseView)
    {
        foreach (var panel in RoleClassifiyView.Ins.RolePanels)
        {
            if (panel.classifyType == type)
            {
                panel.gameObject.SetActive(true);
                var styleView = panel.GetComponentInChildren<RoleStyleView>();
                if (styleView is RoleCustomStyleView)
                {
                    var view = styleView as RoleCustomStyleView;
                    view.ShowAdjustView();
                    adjustView = view.AdjuestView;
                }
                else if (styleView is RoleDigitalCustomView)
                {
                    var view = styleView as RoleDigitalCustomView;
                    view.ShowAdjustView();
                    adjustView = view.AdjuestView;
                }
                else
                {
                    //TODO: 提取ugc-StyleView的基类, 先临时处理彩绘特殊情况
                    adjustView = panel.GetComponentInChildren<RoleAdjustView>(true);
                    var togScript = panel.GetComponentInChildren<ClassifyTogItem>(true);
                    if (togScript)
                    {
                        var viewList = new List<GameObject>(togScript.Panels);
                        var sView = viewList.Find(x => x.activeSelf);
                        adjustView.Show(sView);
                    }
                }
                adjustView.ReturnBtn.onClick.AddListener(CloseAdjustView);
                targetView = panel;
                this.baseView = baseView;
                baseView.gameObject.SetActive(false);
            }
        }
    }
    public void CloseAdjustView()
    {
        adjustView.ReturnBtn.onClick.RemoveListener(CloseAdjustView);
        targetView.gameObject.SetActive(false);
        baseView.gameObject.SetActive(true);
    }
}

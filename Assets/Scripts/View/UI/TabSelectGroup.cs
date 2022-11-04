using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabSelectGroup : MonoBehaviour
{
    private List<TabSelect> tabs = new List<TabSelect>();
    private Dictionary<string, TabSelect> ntabs = new Dictionary<string, TabSelect>();
    private TabSelect preTab = null;
    [HideInInspector]
    public bool isExpand = false;
    [HideInInspector]
    public bool isMultiple = false;

    private void Awake()
    {
        TabSelect[] tabArr = GetComponentsInChildren<TabSelect>();
        foreach (var tab in tabArr)
        {
            tab.SetTabGroup(this);
            AddTab(tab);
        }
    }

    public void AddTab(TabSelect tab)
    {
        tabs.Add(tab);
    }
    public void AddTab(string tabType, TabSelect tab)
    {
        ntabs.Add(tabType, tab);
    }
    public TabSelect GetTab(int index)
    {
        if (index < 0) return null;
        return tabs[index];
    }
    public void SetExpand(TabSelect selectTab)
    {
        if(preTab == selectTab)
        {
            isExpand = !isExpand;
        }
    }
    public void ResetTab()
    {
        isExpand = true;//默认展开
        SelectCurTab(tabs[0]);
    }
    public void OpenPreTab()
    {
        if(preTab == null)
        {
            SelectCurTab(tabs[0]);
        }
        else
        {
            SelectCurTab(preTab);
        }
    }
    public void SelectCurTab(TabSelect selectTab)
    {
        if (selectTab == null) return;
        if (!isMultiple)
        {
            foreach (var tab in tabs)
            {
                tab.ResetSelect();
            }
        }
        selectTab.SetSelectTab(true);
        selectTab.SetSelectPanel(isExpand);
        selectTab.RotateExBtn(isExpand);
        preTab = selectTab;
    }
    public void SelectCurUndoTab(TabSelect curTab,bool expand)
    {
        if (curTab == null) return;
        foreach (var tab in tabs)
        {
            tab.ResetSelect();
        }
        var selectTab = curTab;
        selectTab.SetSelectTab(true);
        selectTab.SetSelectPanel(expand);
        selectTab.RotateExBtn(expand);
        isExpand = expand;
        preTab = curTab;
    }
}

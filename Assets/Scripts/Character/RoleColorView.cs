using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RoleColorView : MonoBehaviour
{
    public Transform colorParent;
    public GameObject colorItem;
    public RoleColorItem curItem;  
    public Action<string> OnSelect;
    private List<RoleColorItem> items = new List<RoleColorItem>();
    public void Init(List<string> datas, Action<string> select)
    {
        OnSelect = select;
        for (int i = 0; i < datas.Count; i++)
        {
            var go = Instantiate(colorItem, colorParent);
            var goScript = go.GetComponent<RoleColorItem>();
            goScript.Init(datas[i], OnSelectClick);
            items.Add(goScript);
        }
    }

    public void OnSelectClick(RoleColorItem item)
    {
        if (curItem == item)
        {
            return;
        }
        if (curItem != null)
        {
            curItem.SetSelectState(false);
        }
        curItem = item;
        curItem.SetSelectState(true);
        OnSelect?.Invoke(curItem.rcData);
    }

    public void SetSelect(string hexColor)
    {
        bool isExist=false;
        items.ForEach(x =>
        {
            x.SetSelectState(false);
            if (x.rcData.Equals(hexColor) || x.rcData.Equals(hexColor.ToLower()))
            {
                curItem = x;
                curItem.SetSelectState(true);
                OnSelect?.Invoke(curItem.rcData);
                isExist=true;
            }
        });
        if(isExist==false)
        {
            curItem=null;
        }
    }

}
using System;
using System.Collections.Generic;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
using UnityEngine;

public class PVPWaitAreaPoolPanel:MonoBehaviour
{
    public GameObject buttonParent;
    public GameObject buttonPrefab;
    private List<CommonButtonItem> buttonPool = new List<CommonButtonItem>();
    private int curIndex = -1;
    private Action<int> OnSelect;

    public void SetItemList(List<int> ids,Action<int> select)
    {
        curIndex = -1;
        OnSelect = select;
        OnClearPanel();
        for (int i = 0; i < ids.Count; i++)
        {
            CommonButtonItem itemScript = null;
            if (i < buttonPool.Count)
            {
                itemScript = buttonPool[i];
            }
            else
            {
                var item = GameObject.Instantiate(buttonPrefab, buttonParent.transform);
                itemScript = item.GetComponent<CommonButtonItem>();
                itemScript.Init();
                buttonPool.Add(itemScript);
            }
            itemScript.gameObject.SetActive(true);
            itemScript.SetText(ids[i].ToString());
            int index = i;
            itemScript.AddClick(() => OnButtonClick(index));
        }
    }
    
    public void OnButtonClick(int index)
    {
        if (curIndex == index)
            return;
        if (curIndex >= 0 && curIndex < buttonPool.Count)
        {
            buttonPool[curIndex].SetSelectState(false);
        }
        curIndex = index;
        buttonPool[curIndex].SetSelectState(true);
        OnSelect?.Invoke(index);
    }

    public void OnClearPanel()
    {
        buttonPool.ForEach(x =>
        {
            x.SetSelectState(false);
            x.gameObject.SetActive(false);
        });
    }
}
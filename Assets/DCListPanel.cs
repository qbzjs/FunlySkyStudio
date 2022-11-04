using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct BatchClaim
{
    public string itemId;
    public string budActId;
    public string supply;
}

[Serializable]
public struct BatchClaimData
{
    public List<BatchClaim> data;
}

public class DCListPanel : BasePanel<DCListPanel>
{
    public DCListItem dcListItem;
    public Transform content;
    public Button btnClaim;
    public Button btnToast;
    private List<DCListItem> curItemIns = new List<DCListItem>();
    
    public void UpdateList(List<DCItem> dcItems, Web3Status web3Status)
    {
        
        curItemIns.Clear();
        for (int i = content.childCount - 1; i >= 0; --i)
        {
            DestroyImmediate(content.GetChild(i).gameObject);
        }

        
        dcItems.Sort((d1, d2) => GetOrder(d2).CompareTo(GetOrder(d1)));
        
        foreach (var item in dcItems)
        {
            var g = Instantiate(dcListItem.gameObject, content).GetComponent<DCListItem>();
            g.tog.onValueChanged.AddListener((v)=>OnTogValueChange(v, g));
            g.Init(item, web3Status);
            
        }
        btnToast.gameObject.SetActive(web3Status == Web3Status.Close);
        CheckBtnClaimState();
        Canvas.ForceUpdateCanvases();
    }


    private long GetOrder(DCItem dcItem)
    {
        if (RoleConfigDataManager.Inst.ConfigDataDic.TryGetValue((ClassifyType)dcItem.resourceType, out var l))
        {
            var item = l.Find(m => m.id == dcItem.resourceId);
            if (item != null)
            {
                return RoleController.QueryWearTime((BundlePart)dcItem.resourceType, item.texName);
            }
        }

        return 0;
    }
    
    private void OnTogValueChange(bool v, DCListItem it)
    {
        if (v)
        {
            if(!curItemIns.Contains(it))curItemIns.Add(it);
        }
        else
        {
            if(curItemIns.Contains(it))curItemIns.Remove(it);
        }

        CheckBtnClaimState();
    }

    private void CheckBtnClaimState()
    {
        if (curItemIns.Count > 0)
        {
            btnClaim.interactable = true;
        }
        else
        {
            btnClaim.interactable = false;
        }
    }


    public void OnBtnClaim()
    {
        if (curItemIns.Count > 0)
        {
            BatchClaimData bcData = new BatchClaimData
            {
                data = curItemIns.Select(i =>
                {
                    var dcItem = i.GetDCItem();
                    return new BatchClaim { budActId = dcItem.budActId, itemId = dcItem.itemId, supply = "1" };
                }).ToList()
            }; 
            
            HttpUtils.MakeHttpRequest("/web3/airdrop/batchClaim", (int)HTTP_METHOD.POST,
                JsonConvert.SerializeObject(bcData),
                null, null);
            ShowNext();
        }
        
    }

    public void ShowToast()
    {
        CharacterTipPanel.ShowToast("This service is currently upgrading...");
    }
    
    private void ShowNext()
    {
        Hide();
        HalfConfirmBoxPanel.Show();
        HalfConfirmBoxPanel.Instance.Init("Airdrop digital collectible on your way!", "You just claimed airdrop digital collectibles. It should be confirmed shortly and you can check it on your BUD Orders page");
    }
    
    public void OnBtnClose()
    {
        Hide();
    }
}
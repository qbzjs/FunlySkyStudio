/// <summary>
/// Author:LiShuZhan
/// Description:背包panel，负责背包界面的逻辑处理
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BaggageState
{
    fold, //折叠
    unfold //展开
}

public class BaggagePanel : BasePanel<BaggagePanel>
{
    public GameObject itemParent;
    public List<BaggageItem> itemList = new List<BaggageItem>();
    public Button foldBtn;
    public BaggageState baggageState;
    public Image foldIcon;

    public override void OnInitByCreate()
    {
        itemList.AddRange(itemParent.GetComponentsInChildren<BaggageItem>());
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].Init();
        }
        RefCutoverBag(BaggageState.fold);
        foldBtn.onClick.AddListener(OnFoldBtnClick);
    }

    public void OnFoldBtnClick()
    {
        var tempState = baggageState == BaggageState.fold ? BaggageState.unfold : BaggageState.fold;
        RefCutoverBag(tempState);
    }

    /// <summary>
    /// 刷新背包展开折叠状态
    /// </summary>
    /// <param name="state"></param>
    public void RefCutoverBag(BaggageState state)
    {
        bool isfold = state == BaggageState.fold ? false : true;
        int reverse = state == BaggageState.fold ? -1 : 1;
        baggageState = state;
        foldIcon.transform.localScale = new Vector3(reverse, 1, 1);
        for (int i = 0; i < itemList.Count-1; i++)
        {
            itemList[i].gameObject.SetActive(isfold);
        }
        itemList[itemList.Count - 1].gameObject.SetActive(true);
    }

    /// <summary>
    /// 背包排序
    /// </summary>
    public BaggageItem BaggageSort(int itemUid, bool isAdd)
    {
        if (isAdd)
        {
            return AddSort(itemUid);
        }
        else
        {
            return RemoveSort(itemUid);
        }
    }

    private BaggageItem AddSort(int itemUid)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].uid == itemUid)
            {
                itemList[i].transform.SetSiblingIndex(itemParent.transform.childCount - 1);
                for (int j = i; j < itemList.Count - 1; j++)
                {
                    var tempItem = itemList[j];
                    itemList[j] = itemList[j + 1];
                    itemList[j + 1] = tempItem;
                }
                return itemList[itemList.Count - 1];
            }
        }
        return null;
    }

    private BaggageItem RemoveSort(int itemUid)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].uid == itemUid)
            {
                itemList[i].transform.SetSiblingIndex(0);
                for (int j = i; j > 0; j--)
                {
                    var tempItem = itemList[j];
                    itemList[j] = itemList[j - 1];
                    itemList[j - 1] = tempItem;
                }
                return itemList[0];
            }
        }
        return null;
    }

    /// <summary>
    /// 重置背包
    /// </summary>
    public void ResetBaggage()
    {
        RefCutoverBag(BaggageState.fold);
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].ResetInfo();
        }
    }

    public void SetSelect(string playerUid,int itemUid)
    {
        if (playerUid == GameManager.Inst.ugcUserInfo.uid)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if(itemList[i].uid != -1 && itemList[i].uid == itemUid)
                {
                    itemList[i].selectImg.gameObject.SetActive(true);
                }
                else
                {
                    itemList[i].selectImg.gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetItemUI(BaggageItem emptyGrid,Texture tex,int curItemUid)
    {
        if(curItemUid != emptyGrid.uid)
        {
            return;
        }
        emptyGrid.icon.texture = tex;
        emptyGrid.icon.gameObject.SetActive(true);
        emptyGrid.loading.SetActive(false);
    }

    public void SetItemUI(BaggageItem emptyGrid, Sprite sprite)
    {
        if (emptyGrid.uid == (int)BaggageItemType.none)
        {
            return;
        }
        emptyGrid.baseIcon.sprite = sprite;
        emptyGrid.baseIcon.gameObject.SetActive(true);
        emptyGrid.loading.SetActive(false);
    }

    public void ReBaggage()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].ResetInfo();
        }
    }

    public BaggageItem FindEmptyBag()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if(itemList[i].uid == (int)BaggageItemType.none)
            {
                return itemList[i];
            }
        }
        return null;
    }
}

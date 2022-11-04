using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class RoleStyleView : MonoBehaviour
{
    public Transform IconParent;
    public GameObject IconItem;
    [HideInInspector]
    public RoleStyleItem curItem;
    protected Action<RoleIconData> OnSelect;
    protected List<RoleItemInfo> items = new List<RoleItemInfo>();
    public int componentType;
    public ClassifyType type;
    public BodyPartType part;
    public int noneId = 0; //默认none id
    public Action<RoleStyleView, RoleItemInfo> OnItemLoadFinish;
    protected HttpReqState curState = HttpReqState.FirstEntry;
    private AvatarReqQuerry reqQuerry = new AvatarReqQuerry();

    public virtual void Init<T>(T data, Action<RoleIconData> select, SpriteAtlas spriteAtlas, RoleItemInfo info) where T : RoleIconData
    {
        OnSelect = select;
        //创建初始化
        var go = Instantiate(IconItem, IconParent);
        var goScript = go.GetComponent<RoleStyleItem>();
        goScript.type = type;
        goScript.Init(data, OnSelectClick, spriteAtlas);
        info.item = goScript;
        //dc部件单独在子分类下添加
        ItemDictType dictType = data.IsOrigin() ? ItemDictType.AllOwned : ItemDictType.SpecialShow;
        BaseView.Ins.AddItemList(dictType, type, goScript);
        //更新收藏/isNew状态, 注意: 新用户不显示收藏/isNew, 也无法进行收藏操作
        goScript.UpdateItemCollect(info.isFavorites == 1);
        goScript.UpdateItemIsNew(info.isNew == 1);
        //排序并存入列表
        SortWhenCreateItem(goScript, info);
    }

    private void SortWhenCreateItem(RoleStyleItem roleItem, RoleItemInfo info)
    {
        var index = items.FindIndex(x => x.sort > 0 && x.sort < info.sort); //sort = 0表示none
        if (index >= 0)
        {
            int sibling = items[index].item.transform.GetSiblingIndex();
            roleItem.transform.SetSiblingIndex(sibling);
            items.Insert(index, info);
        }
        else
        {
            items.Add(info);
        }
    }

    //初始化列表请求
    public virtual void GetAllItemList(Action<RoleStyleView, RoleItemInfo> updateList)
    {
        if (curState == HttpReqState.FirstEntry)
        {
            reqQuerry.parentType = (int)part;
            reqQuerry.subType = (int)type;
            reqQuerry.componentType = componentType;
            reqQuerry.pageSize = 300;
            reqQuerry.cookie = "";
            reqQuerry.toUid = GameManager.Inst.ugcUserInfo.uid;
            OnItemLoadFinish = updateList;
            RefreshItemList();
        }
        if (curState == HttpReqState.Failed)
        {
            //初始化失败, 仍需要刷新
            RefreshItemList();
        }
    }

    public void RefreshItemList()
    {
        curState = HttpReqState.Refreshing;
        HttpUtils.MakeHttpRequest("/other/getAvatarOrigins", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(reqQuerry), (content) =>
        {
            OnGetItemListSuccess(content);
        }, (error) =>
        {
            OnGetItemListFail(error);
        });
    }

    private void OnGetItemListSuccess(string content)
    {
        LoggerUtils.Log("OnGetItemListSuccess --> " + type);
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data))
        {
            curState = HttpReqState.Failed;
            LoggerUtils.LogError($"OnGetItemList - {type} : repData.data == null");
            return;
        }
        AvatarClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<AvatarClothesRepInfo>(repData.data);
        reqQuerry.cookie = resourceInfo.cookie;
        if (resourceInfo.resources != null)
        {
            //获取pgcId
            foreach (var data in resourceInfo.resources)
            {
                if (data.IsIllegal())
                {
                    continue;
                }
                //逐个创建item
                RoleItemInfo info = new RoleItemInfo()
                {
                    pgcId = data.id,
                    sort = data.sort,
                    isNew = data.isNew,
                    isFavorites = data.isFavorites
                };
                OnItemLoadFinish?.Invoke(this, info);
            }
        }

        if (resourceInfo.isEnd != 1)
        {
            RefreshItemList();
        }
        else
        {
            curState = HttpReqState.Success;
        }
    }

    private void OnGetItemListFail(string error)
    {
        curState = HttpReqState.Failed;
        LoggerUtils.LogError($"OnGetItemListFail - {type} error = " + error);
    }

    public virtual void OnSelectClick(RoleStyleItem item)
    {
        if (curItem == item && IconParent.gameObject.activeInHierarchy)
        {
            return;
        }

        if (curItem != null)
        {
            curItem.SetSelectState(false);
        }
        curItem = item;
        curItem.SetSelectState(true);
        curItem.rcData.rc = item;
        OnSelect?.Invoke(curItem.rcData);
    }

    public void SetSelect(int id)
    {
        items.ForEach(x =>
        {
            x.item.SetSelectState(false);
            if (x.item.rcData.id == id)
            {
                curItem = x.item;
                curItem.SetSelectState(true);
                OnSelect?.Invoke(curItem.rcData);
            }
        });
    }
}
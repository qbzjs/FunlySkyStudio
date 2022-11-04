using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:Meimei-LiMei
/// Description:上新合集列表界面
/// Date: 2022/4/24 13:37:11
/// </summary>
public class NewsView : BaseView
{
    public Transform iconParent;
    public Action OnItemListLoadFinish;
    private RoleStyleItem curItem;
    private HttpReqState curState = HttpReqState.FirstEntry;
    private AvatarReqQuerry reqQuerry = new AvatarReqQuerry();
    private List<RoleItemInfo> roleItemInfos = new List<RoleItemInfo>();

    private void OnDisable()
    {
        ClearSelectState();
    }

    public void ClearSelectState()
    {
        if (curItem != null)
        {
            curItem.SetSelectState(false);
            curItem = null;
        }
    }

    public override void OnSelect()
    {
        if (curState == HttpReqState.FirstEntry)
        {
            GetAllItemList();
        }
        if (curState == HttpReqState.Failed)
        {
            //初始化失败, 仍需要刷新
            RefreshItemList();
        }
    }

    public override void UpdateSelectState()
    {
        base.UpdateSelectState();
        ClearSelectState();
    }

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitNewsView);
    }

    public void InitNewsView()
    {
        this.bodyPart = BodyPartType.body;
        this.classifyType = ClassifyType.news;
    }

    //第一次点击进入标签页才会请求
    public void GetAllItemList()
    {
        reqQuerry.parentType = (int)bodyPart;
        reqQuerry.subType = (int)classifyType;
        reqQuerry.pageSize = 100;
        reqQuerry.cookie = "";
        reqQuerry.toUid = GameManager.Inst.ugcUserInfo.uid;
        RefreshItemList();
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
        LoggerUtils.Log("OnGetItemListSuccess --> " + classifyType);
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data))
        {
            curState = HttpReqState.Failed;
            LoggerUtils.LogError($"OnGetItemList - {classifyType} : repData.data == null");
            return;
        }
        AvatarClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<AvatarClothesRepInfo>(repData.data);
        reqQuerry.cookie = resourceInfo.cookie;
        if (resourceInfo.resources != null)
        {
            //获取pgcId
            foreach (var item in resourceInfo.resources)
            {
                if (item.IsIllegal())
                {
                    continue;
                }
                //创建本页面Items
                CreateResourceItem(item);
            }
        }

        if (resourceInfo.isEnd != 1)
        {
            RefreshItemList();
        }
        else
        {
            curState = HttpReqState.Success;
            //创建完成, 执行回调事件
            OnItemListLoadFinish?.Invoke();
        }
    }

    private void OnGetItemListFail(string error)
    {
        curState = HttpReqState.Failed;
        LoggerUtils.LogError($"OnGetItemListFail - {classifyType} error = " + error);
    }

    private void CreateResourceItem(AvatarClothesInfo item)
    {
        var type = (ClassifyType)item.resourceType;
        var rcData = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId(type, item.id);
        var rItem = CreateItemByData(type, iconParent, rcData, OnItemSelect);
        if (rItem == null)
        {
            return;
        }
        //更新收藏标识
        rItem.UpdateItemCollect(item.isFavorites == 1);
        //更新isNew标识
        rItem.UpdateItemIsNew(item.isNew == 1);
        //顺序排列
        SortWhenCreateItem(rItem, item.sort);
    }

    protected override RoleStyleItem CreateItemByData(ClassifyType type, Transform parentTF, RoleIconData rcData, Action<RoleStyleItem> select, BaseView headView = null)
    {
        var rItem = base.CreateItemByData(type, parentTF, rcData, select);
        if (rItem != null)
        {
            BaseView.Ins.AddItemList(ItemDictType.News, type, rItem);
        }
        return rItem;
    }

    private void SortWhenCreateItem(RoleStyleItem roleItem, int itemSort)
    {
        var refIndex = roleItemInfos.FindIndex(x => x.sort < itemSort);
        if (refIndex >= 0)
        {
            int sibling = roleItemInfos[refIndex].item.transform.GetSiblingIndex();
            roleItem.transform.SetSiblingIndex(sibling);
            roleItemInfos.Insert(refIndex, new RoleItemInfo() { sort = itemSort, item = roleItem });
        }
        else
        {
            roleItemInfos.Add(new RoleItemInfo() { sort = itemSort, item = roleItem });
        }
    }

    public void OnItemSelect(RoleStyleItem item)
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
    }
}

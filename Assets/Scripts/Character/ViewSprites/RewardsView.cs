using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: pzkunn
/// Description: 奖励页面
/// Date: 2022/8/23 14:45:43
/// </summary>
public class RewardsView : BaseView
{
    public Transform iconParent;
    public BaseView viewParent;
    private RoleStyleItem curItem;
    private HttpReqState curState = HttpReqState.FirstEntry;
    private RWHttpReqQuerry httpReqQuerry = new RWHttpReqQuerry();

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitRewardsView);
    }

    public void InitRewardsView()
    {
        this.classifyType = ClassifyType.rewards;
    }

    public void GetAllRewardsItemList()
    {
        if (curState == HttpReqState.FirstEntry)
        {
            httpReqQuerry.dataType = (int)Data_Type.Cloth;
            httpReqQuerry.cookie = "";
            httpReqQuerry.toUid = GameManager.Inst.ugcUserInfo.uid;
            RefreshRewardsList();
        }
    }

    public void RefreshRewardsList()
    {
        curState = HttpReqState.Refreshing;
        HttpUtils.MakeHttpRequest("/facade/checkins/rewards", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpReqQuerry), (content) =>
        {
            OnGetRewardsListSuccess(content);
        }, (error) =>
        {
            OnGetRewardsListFail(error);
        });
    }

    private void OnGetRewardsListSuccess(string content)
    {
        LoggerUtils.Log("OnGetRewardsListSuccess");
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data))
        {
            curState = HttpReqState.Failed;
            LoggerUtils.LogError("OnGetRewardsList : repData.data == null");
            return;
        }
        PGCClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<PGCClothesRepInfo>(repData.data);
        httpReqQuerry.cookie = resourceInfo.cookie;
        if (resourceInfo.itemList != null)
        {
            //获取pgcId
            foreach (var item in resourceInfo.itemList)
            {
                if (item.pgcInfo.classifyType < 1)
                {
                    continue;
                }
                //创建本页面奖励部件
                CreateRewardsItem(item);
            }
        }

        if (resourceInfo.isEnd != 1)
        {
            RefreshRewardsList();
        }
        else
        {
            curState = HttpReqState.Success;
            InitWearRewardsItem();
        }
    }

    private void CreateRewardsItem(PGCClothesInfo item)
    {
        var type = (ClassifyType)item.pgcInfo.classifyType;
        var rcData = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId(type, item.pgcInfo.pgcId);
        var rItem = CreateItemByData(type, iconParent, rcData, OnRewardsItemClick);
        if (rItem == null)
        {
            return;
        }
        //传递mapId(红点标记)
        rItem.redDotId = item.mapId;
        rItem.canClearRed = true;
        //更新收藏标识
        rItem.UpdateItemCollect(item.mapStatus.isFavorites == 1);
        //更新isNew标识
        bool isNew = item.mapStatus.isNew == 1;
        rItem.UpdateItemIsNew(isNew);
        if (isNew)
        {
            rItem.transform.SetSiblingIndex(0);
        }
    }

    protected override RoleStyleItem CreateItemByData(ClassifyType type, Transform parentTF, RoleIconData rcData, Action<RoleStyleItem> select, BaseView headView = null)
    {
        var rItem = base.CreateItemByData(type, parentTF, rcData, select, viewParent);
        if (rItem != null)
        {
            BaseView.Ins.AddItemList(ItemDictType.AllOwned, type, rItem);
        }
        return rItem;
    }

    private void InitWearRewardsItem()
    {
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.SET_REWARDS)
        {
            var dcLists = GetAllDcPgcInfos();
            if(dcLists == null)
            {
                return;
            }
            for (int i = 0; i < dcLists.Count; i++)
            {
                var dcInfo = dcLists[i];
                if (dcInfo != null && !dcInfo.Equals(default(PGCInfo)))
                {
                    var type = dcInfo.classifyType;
                    var id = dcInfo.pgcId;
                    LoggerUtils.Log($"InitWearRewardsItem --> classifyType:{type}, pgcId:{id}");
                    //选中指定部件并试穿
                    var item = BaseView.Ins.GetItem(ItemDictType.AllOwned, (ClassifyType)type, id);
                    if (item) item.OnSelectClick();
                    BaseView bView = RoleClassifiyView.Ins.GetViewByType((ClassifyType)type);
                    if (bView) bView.OnSelectItem(id, item);
                }
            }
        }
    }

    private void OnGetRewardsListFail(string error)
    {
        curState = HttpReqState.Failed;
        LoggerUtils.LogError("OnGetRewardsListFail error = " + error);
    }

    public void OnRewardsItemClick(RoleStyleItem item)
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

    public void ClearSelectState()
    {
        if (curItem != null)
        {
            curItem.SetSelectState(false);
            curItem = null;
        }
    }

    public override void UpdateSelectState()
    {
        ClearSelectState();
    }

    private void OnDisable()
    {
        ClearSelectState();
    }
}
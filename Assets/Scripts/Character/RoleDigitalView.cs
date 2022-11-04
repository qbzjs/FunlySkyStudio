using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Author: pzkunn
/// Description: DC分类界面管理
/// Date: 2022/7/13 20:42:41
/// </summary>
public class RoleDigitalView : RoleStyleView
{
    private DCHttpReqQuerry httpReqQuerry = new DCHttpReqQuerry();

    public override void Init<T>(T data, Action<RoleIconData> select, SpriteAtlas spriteAtlas, RoleItemInfo info)
    {
        OnSelect = select;
        //创建初始化
        var go = Instantiate(IconItem, IconParent);
        var goScript = go.GetComponent<RoleStyleItem>();
        goScript.type = type;
        goScript.Init(data, OnSelectClick, spriteAtlas);
        info.item = goScript;
        BaseView.Ins.AddItemList(ItemDictType.AllOwned, type, goScript);
        //更新收藏/isNew状态
        var collectionsView = RoleMenuView.Ins.GetView<CollectionsView>();
        var cItem = collectionsView.GetCollectItem(type, info.pgcId);
        goScript.UpdateItemCollect(cItem != null);
        goScript.UpdateItemIsNew(info.isNew == 1);
        //传递mapId(红点标记)
        goScript.redDotId = info.mapId;
        goScript.canClearRed = true;
        //存入列表
        items.Add(info);
    }

    //初始化列表请求
    public override void GetAllItemList(Action<RoleStyleView, RoleItemInfo> updateList)
    {
        if (curState == HttpReqState.FirstEntry)
        {
            httpReqQuerry.classifyType = (int)type;
            httpReqQuerry.itemType = (int)DCItemType.Clothes;
            httpReqQuerry.listType = (int)DCUGCCloResType.Owned;
            httpReqQuerry.componentType = componentType;
            httpReqQuerry.toUid = GameManager.Inst.ugcUserInfo.uid;
            httpReqQuerry.cookie = "";
            OnItemLoadFinish = updateList;
            RefreshDCResList();
        }
        if (curState == HttpReqState.Failed)
        {
            //初始化失败，仍需要刷新
            RefreshDCResList();
        }
    }

    //发起http请求，用户某一类型下所有拥有的DC资产
    public void RefreshDCResList()
    {
        curState = HttpReqState.Refreshing;
        HttpUtils.MakeHttpRequest("/ugcmap/userItemList", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpReqQuerry), (content) =>
        {
            OnGetDCResListSuccess(content);
        }, (error) =>
        {
            OnGetDCResListFail(error);
        });
    }

    private void OnGetDCResListSuccess(string content)
    {
        LoggerUtils.Log("OnGetDCResListSuccess->" + type);
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data))
        {
            LoggerUtils.LogError("OnGetDCResList : repData.data == null");
            curState = HttpReqState.Failed;
            return;
        }
        DCUGCClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<DCUGCClothesRepInfo>(repData.data);
        httpReqQuerry.cookie = resourceInfo.cookie;
        if (resourceInfo.itemList != null)
        {
            //获取pgcId
            foreach (var dcInfo in resourceInfo.itemList)
            {
                if (dcInfo.dcPgcInfo != null && dcInfo.dcPgcInfo.classifyType == (int)type && dcInfo.dcPgcInfo.hasCount > 0)
                {
                    RoleItemInfo info = new RoleItemInfo()
                    {
                        mapId = dcInfo.mapId,
                        pgcId = dcInfo.dcPgcInfo.pgcId,
                        isNew = dcInfo.mapStatus.isNew,
                        isFavorites = dcInfo.mapStatus.isFavorites
                    };
                    OnItemLoadFinish?.Invoke(this, info);
                }
            }
        }
        
        if (resourceInfo.isEnd != 1)
        {
            RefreshDCResList();
        }
        else
        {
            curState = HttpReqState.Success;
            //列表拉取完成, 添加none
            OnItemLoadFinish?.Invoke(this, new RoleItemInfo() { pgcId = noneId });
            var noneItem = items.FindLast(x => x.pgcId == noneId);
            noneItem?.item.transform.SetAsFirstSibling();
        }
    }

    private void OnGetDCResListFail(string error)
    {
        curState = HttpReqState.Failed;
        LoggerUtils.LogError("OnGetDCResListFail error = " + error);
    }
}
using DG.Tweening;
using Newtonsoft.Json;
using SavingData;
using SuperScrollView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Author: pzkunn
/// Description: Airdrop奖励DC页面
/// Date: 2022/8/30 20:15:36
/// </summary>
public class AirdropView : BaseView
{
    private bool isLock = true;
    private bool isEnd = false;
    public BaseView viewParent;
    public LoopGridView mLoopGridView;
    private RoleStyleUgcItem curItem;
    private TextureQueuedLoader textureBatchLoader;
    private List<RoleUGCIconData> airIconInfos = new List<RoleUGCIconData>();
    private List<RoleStyleUgcItem> airItemList = new List<RoleStyleUgcItem>();
    private HttpReqState curState = HttpReqState.FirstEntry;
    private DCHttpReqQuerry httpReqQuerry = new DCHttpReqQuerry();

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitAirdropView);
    }

    public void InitAirdropView()
    {
        this.classifyType = ClassifyType.airdrop;
    }

    public void GetAllAirdropItemList()
    {
        if (curState == HttpReqState.FirstEntry)
        {
            textureBatchLoader = TextureQueuedLoader.Create(maxLoaderNum: 3, maxMemoryCacheNum: 300, maxQueueSize: 30);
            mLoopGridView.OnPreLoadEvent = () => RefreshAirdropRes(UpdateResListSuccess, OnGetAirdropListFail);
            mLoopGridView.OnOverBottomEvent = () => RefreshAirdropRes(UpdateResListSuccess, OnGetAirdropListFail);
            InitAirdropRes(OnGetAirdropListSuccess, OnGetAirdropListFail);
        }
    }

    private void InitAirdropRes(UnityAction<string> onSuccess, UnityAction<string> onFail)
    {
        airIconInfos.Clear();
        httpReqQuerry.itemType = (int)DCItemType.Clothes;
        httpReqQuerry.listType = (int)DCUGCCloResType.Owned;
        httpReqQuerry.toUid = GameManager.Inst.ugcUserInfo.uid;
        httpReqQuerry.cookie = "";
        curState = HttpReqState.Refreshing;
        HttpUtils.MakeHttpRequest("/ugcmap/airDropList", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpReqQuerry), onSuccess, onFail);
    }

    private void UnLockRefresh()
    {
        isLock = true;
    }

    private void RefreshAirdropRes(UnityAction<string> onSuccess, UnityAction<string> onFail)
    {
        if (isLock && !isEnd)
        {
            isLock = false;
            Invoke("UnLockRefresh", 5);
            curState = HttpReqState.Refreshing;
            HttpUtils.MakeHttpRequest("/ugcmap/airDropList", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpReqQuerry), onSuccess, onFail);
        }
    }

    private void OnGetAirdropListSuccess(string content)
    {
        LoggerUtils.Log("OnGetAirdropListSuccess content :" + content);
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data))
        {
            curState = HttpReqState.Failed;
            LoggerUtils.LogError("OnGetAirdropList : repData.data == null");
            return;
        }
        curState = HttpReqState.Success;
        DCUGCClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<DCUGCClothesRepInfo>(repData.data);
        isEnd = resourceInfo.isEnd == 1;
        if (resourceInfo.itemList != null)
        {
            LoggerUtils.Log("Airdrop Get -- resourceInfo.itemList = " + JsonConvert.SerializeObject(resourceInfo.itemList));
            httpReqQuerry.cookie = resourceInfo.cookie;
            UpdateAirdropIconsInfo(resourceInfo.itemList);
            mLoopGridView.InitGridView(airIconInfos.Count, OnGetItemByRowColumn);
            InitWearAirdropItem();
        }
    }

    private void UpdateResListSuccess(string content)
    {
        UnLockRefresh();
        HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        if (string.IsNullOrEmpty(repData.data))
        {
            curState = HttpReqState.Failed;
            LoggerUtils.LogError("OnUpdateAirdropList : repData.data == null");
            return;
        }
        curState = HttpReqState.Success;
        DCUGCClothesRepInfo resourceInfo = JsonConvert.DeserializeObject<DCUGCClothesRepInfo>(repData.data);
        isEnd = resourceInfo.isEnd == 1;
        if (resourceInfo.itemList != null)
        {
            LoggerUtils.Log("Airdrop Update -- resourceInfo.itemList = " + JsonConvert.SerializeObject(resourceInfo.itemList));
            httpReqQuerry.cookie = resourceInfo.cookie;
            UpdateAirdropIconsInfo(resourceInfo.itemList);
            mLoopGridView.SetListItemCount(airIconInfos.Count, false);
        }
    }

    private void OnGetAirdropListFail(string error)
    {
        UnLockRefresh();
        curState = HttpReqState.Failed;
        LoggerUtils.LogError("Script:AirdropView OnGetAirdropListFail error = " + error);
    }

    private void UpdateAirdropIconsInfo(List<DCUGCClothesInfo> mapInfos)
    {
        for (var i = 0; i < mapInfos.Count; i++)
        {
            RoleUGCIconData data = new RoleUGCIconData()
            {
                mapId = mapInfos[i].mapId,
                isNew = mapInfos[i].mapStatus.isNew,
                isFavorites = mapInfos[i].mapStatus.isFavorites,
                grading = (int)RoleResGrading.DC,
                origin = (int)RoleOriginType.Airdrop,
            };

            if (mapInfos[i].isPGC > 0)
            {
                var pgcInfo = mapInfos[i].dcPgcInfo;
                if (pgcInfo == null || pgcInfo.hasCount == 0)
                {
                    continue;
                }
                //官方Airdrop(Icon封面链接特殊获取)
                data.coverUrl = RoleConfigDataManager.Inst.GetAvatarIconPath((ClassifyType)pgcInfo.classifyType, pgcInfo.pgcId);
                data.classifyType = pgcInfo.classifyType;
                data.pgcId = pgcInfo.pgcId;
            }
            else
            {
                //ugc-Airdrop
                var type = RoleConfigDataManager.Inst.GetTypeByDataSubType(mapInfos[i].dataSubType);
                data.classifyType = (int)type; //目前仅用作Zoom判断(TODO: ugc其他类型需要补充接口字段)
                data.coverUrl = mapInfos[i].mapCover;
                data.jsonUrl = mapInfos[i].clothesJson;
                data.zipUrl = mapInfos[i].clothesUrl;
                data.templateId = mapInfos[i].templateId;
            }

            if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.SET_REWARDS)
            {
                var dcLists = GetAllDcPgcInfos();
                if (dcLists != null && dcLists[0].classifyType == data.classifyType&& dcLists[0].pgcId == data.pgcId)
                {
                    //官方Airdrop跳转页面，需要把当前选中Item置顶
                    airIconInfos.Insert(0, data);
                    continue;
                }
            }
            airIconInfos.Add(data);
        }
    }

    LoopGridViewItem OnGetItemByRowColumn(LoopGridView gridView, int itemIndex, int row, int column, ScrollDirection sdir)
    {
        LoopGridViewItem item = gridView.GetItemByPool();
        item.gameObject.name = itemIndex.ToString();
        var ugcItem = item.GetComponent<RoleStyleUgcItem>();

        var itemData = airIconInfos[itemIndex];
        if (itemData == null)
        {
            return item;
        }

        if (itemData.pgcId > 0)
        {
            //官方Airdrop
            ugcItem.Init(itemData, OnPgcItemSelect);
            AddItemList(UGCClothesResType.PGC, ugcItem);
            //临时处理Airdrop
            if (itemData.classifyType == (int)ClassifyType.headwear
                || itemData.classifyType == (int)ClassifyType.hand
                || itemData.classifyType == (int)ClassifyType.bag
                || itemData.classifyType == (int)ClassifyType.glasses
                || itemData.classifyType == (int)ClassifyType.effects
                || itemData.classifyType == (int)ClassifyType.eyes
                )
            {
                ugcItem.CanAdjust = true;
                ugcItem.SetCustomView(() => { AdjustViewManager.Inst.OpenAdjustView((ClassifyType)itemData.classifyType, viewParent); });
            }
        }
        else
        {
            //ugc-Airdrop
            ugcItem.Init(itemData, OnUgcItemSelect);
            AddItemList(UGCClothesResType.UGC, ugcItem);
            if (itemData.classifyType == (int)ClassifyType.ugcPatterns)
            {
                ugcItem.CanAdjust = true;
                var pgctype = RoleConfigDataManager.Inst.GetPGCTypeByUGCType((ClassifyType)itemData.classifyType);
                ugcItem.SetCustomView(() => { AdjustViewManager.Inst.OpenAdjustView(pgctype, viewParent); });
            }
        }
        //加载封面图
        textureBatchLoader.m_OnImageLoadError = (err, detail) =>
        {
            Debug.LogError("Error url " + err + "----------" + detail.m_URL);
        };
        textureBatchLoader.m_OnImageLoaded = (result) =>
        {
            if (result.m_Texture)
            {
                //快速翻页会出现查找不到情况
                var item = gridView.GetShownItemByItemIndex(result.tempArg);
                if (item != null)
                {
                    var itemScript = item.GetComponent<RoleStyleUgcItem>();
                    itemScript.SetItemTexture(result.m_Texture);
                }
            }
        };
        var tex = textureBatchLoader.GetImageByUrl(itemData.coverUrl, itemIndex, sdir, loadIfNotFound: true);
        if (tex)
        {
            ugcItem.SetItemTexture(tex);
        }
        return item;
    }

    private void OnPgcItemSelect(RoleStyleUgcItem ugcItem)
    {
        OnAirdropItemClick(ugcItem);
        var bView = RoleClassifiyView.Ins.GetViewByType((ClassifyType)ugcItem.rcData.classifyType);
        if (bView != null)
        {
            //TODO: 整合Pgc/Ugc Item, 都具有加载预制loading显示
            bView.OnSelectItem(ugcItem.rcData.pgcId, null);
        }
    }

    private void OnUgcItemSelect(RoleStyleUgcItem ugcItem)
    {
        OnAirdropItemClick(ugcItem);
        var itemData = ugcItem.rcData;
        var typeView = RoleClassifiyView.Ins.GetViewByType((ClassifyType)itemData.classifyType);
        if (typeView)
        {
            var ugciconView = typeView.GetComponentInChildren<RoleUgcBaseView>(true);
            if (ugciconView)
            {
                ugciconView.OnUgcItemSelect(ugcItem);
            }
        }
    }

    public RoleStyleUgcItem GetPgcItem(int type, int id)
    {
        return airItemList.Find(x => x.rcData.classifyType.Equals(type) && x.rcData.pgcId.Equals(id));
    }

    public RoleStyleUgcItem GetUgcItem(string mapId)
    {
        return airItemList.Find(x => x.rcData.mapId.Equals(mapId));
    }

    private void AddItemList(UGCClothesResType type, RoleStyleUgcItem item)
    {
        RoleStyleUgcItem oItem = null;
        var rcData = item.rcData;
        switch (type)
        {
            case UGCClothesResType.PGC:
                if (rcData.pgcId == default(int))
                {
                    return;
                }
                oItem = GetPgcItem(rcData.classifyType, rcData.pgcId);
                break;
            case UGCClothesResType.UGC:
                if (string.IsNullOrEmpty(rcData.mapId))
                {
                    return;
                }
                oItem = GetUgcItem(rcData.mapId);
                break;
        }
        if (oItem == null)
        {
            airItemList.Add(item);
        }
    }

    private void InitWearAirdropItem()
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
                var pgcInfo = dcLists[i];
                if (pgcInfo != null && !pgcInfo.Equals(default(PGCInfo)))
                {
                    var type = pgcInfo.classifyType;
                    var id = pgcInfo.pgcId;
                    LoggerUtils.Log($"InitWearAirdropItem --> classifyType:{type}, pgcId:{id}");
                    //选中指定部件并试穿
                    var item = GetPgcItem(type, id);
                    if (item) OnPgcItemSelect(item);
                }
            }
        }
    }

    public void OnAirdropItemClick(RoleStyleUgcItem item)
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
using System.Net.Mime;
using System;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Author:Meimei-LiMei
/// Description:服饰收藏列表界面
/// Date: 2022/4/24 13:36:51
/// </summary>
public class CollectionsView : BaseView
{
    public GameObject Tips;
    public Transform IconParent;
    public List<GameObject> collectList = new List<GameObject>(); //收藏列表
    public RoleStyleUgcItem ugcItem;
    [HideInInspector]
    public GameObject curItem;
    protected List<RoleStyleItem> items = new List<RoleStyleItem>();
    // 所有收藏的普通服饰 item Dict  key:itemType   value:<key, value> [key: itemId, value: item]
    private Dictionary<ClassifyType, Dictionary<int, RoleStyleItem>> itemCollectListDict = new Dictionary<ClassifyType, Dictionary<int, RoleStyleItem>>();
    //所有收藏的UGC item key:mapId  value:item
    private Dictionary<string, RoleStyleUgcItem> itemCollectUgcListDict = new Dictionary<string, RoleStyleUgcItem>();

    private void Awake()
    {
        ClearCollectList();
    }

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitCollectionsView);
    }

    public void InitCollectionsView()
    {
        this.classifyType = ClassifyType.collections;
        if (collectList.Count <= 0)
        {
            Tips.SetActive(true);
        }
        else
        {
            Tips.SetActive(false);
        }
    }
    public override void UpdateSelectState()
    {
        base.UpdateSelectState();
        ClearSelectState();
    }
    private void OnDisable()
    {
        ClearSelectState();
    }

    public void ClearSelectState()
    {
        if (curItem != null)
        {
            SetSelectState(curItem, false);
            curItem = null;
        }
    }

    /**
    * 获取所有收藏列表
    */
    public void GetAllCollectClothingList()
    {
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType != ROLE_TYPE.FIRST_ENTRY)
        {
            HttpUtils.MakeHttpRequest("/image/getFavorites", (int)HTTP_METHOD.GET, "", GetCollectListSuccess, GetCollectListFail);
        }
    }

    public void GetCollectListSuccess(string msg)
    {
        ClearCollectList();
        LoggerUtils.Log("CollectionsView GetCollectListSuccess. msg is  " + msg);
        HttpResponDataStruct responseData = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        ClothingDataList clothingDataList = JsonConvert.DeserializeObject<ClothingDataList>(responseData.data);
        if (clothingDataList.favoritesInfo == null)
        {
            return;
        }
        List<ClothingData> clothingDatas = new List<ClothingData>(clothingDataList.favoritesInfo);
        foreach (var clothingData in clothingDatas)
        {
            if (clothingData.type != (int)ClassifyType.ugcCloth) //统一用ugcCloth, 仅用作区分是否ugc
            {
                int id;
                if (int.TryParse(clothingData.id, out id))
                {
                    //TODO：初始化收藏列表需要屏蔽未拥有的DC部件 -- 服务端
                    var rcData = RoleConfigDataManager.Inst.GetConfigDataByTypeAndId((ClassifyType)clothingData.type, id);
                    if (rcData != null)
                    {
                        //刷新DC收藏列表
                        CollectClothingItem((ClassifyType)clothingData.type, rcData);
                    }
                }
            }
            else
            {
                RoleUGCIconData ugcData = JsonConvert.DeserializeObject<RoleUGCIconData>(clothingData.data);
                CollectUgcClothingItem(ugcData);
            }
        }
    }

    public void GetCollectListFail(string err)
    {
        LoggerUtils.LogError("Script:CollectionsView GetCollectListFail error = " + err);
    }

    public void AddCollectItem(ClassifyType type, RoleStyleItem item)
    {
        var itemId = item.rcData.id;
        Dictionary<int, RoleStyleItem> itemDict;
        if (!itemCollectListDict.ContainsKey(type))
        {
            itemDict = new Dictionary<int, RoleStyleItem>();
            itemDict[itemId] = item;
            itemCollectListDict[type] = itemDict;
        }
        else
        {
            itemDict = itemCollectListDict[type];
            if (!itemDict.ContainsKey(itemId))
            {
                itemDict[itemId] = item;
            }
        }
    }

    public void RemoveCollectItem(ClassifyType type, int id)
    {
        Dictionary<int, RoleStyleItem> itemDict;
        if (itemCollectListDict.TryGetValue(type, out itemDict))
        {
            if (itemDict.ContainsKey(id))
            {
                itemDict.Remove(id);
            }
        }
    }

    public void ClearItemCollectListDict()
    {
        if (itemCollectListDict != null)
        {
            itemCollectListDict.Clear();
        }
    }

    public RoleStyleItem GetCollectItem(ClassifyType type, int id)
    {
        Dictionary<int, RoleStyleItem> itemDict = null;
        if (!itemCollectListDict.TryGetValue(type, out itemDict))
        {
            return null;
        }
        RoleStyleItem item = null;
        itemDict.TryGetValue(id, out item);
        return item;
    }

    public void AddCollectUgcItem(RoleStyleUgcItem item)
    {
        var mapId = item.rcData.mapId;
        if (!itemCollectUgcListDict.ContainsKey(mapId))
        {
            itemCollectUgcListDict.Add(mapId, item);
        }
    }

    public void ClearItemCollectUgcListDict()
    {
        if (itemCollectUgcListDict != null)
        {
            itemCollectUgcListDict.Clear();
        }
    }

    public void RemoveCollectItem(GameObject item)
    {
        collectList.Remove(item.gameObject);
        item.SetActive(false);
        Destroy(item);

        if (collectList.Count <= 0)
        {
            Tips.SetActive(true);
        }
    }

    //收藏普通服饰(即非UGC衣服)
    public void CollectClothingItem(ClassifyType type, RoleIconData rcData, bool addCollect = false)
    {
        var dcItem = CreateItemByData(type, IconParent, rcData, OnSelectClick);
        if (dcItem == null)
        {
            return;
        }
        if (addCollect)
        {
            collectList.Insert(0, dcItem.gameObject);
        }
        else
        {
            collectList.Add(dcItem.gameObject);
        }
        BaseView.Ins.UpdateItemCollect(type, rcData.id, true);
        Tips.SetActive(false);
        UpdateCollectList();
    }

    protected override RoleStyleItem CreateItemByData(ClassifyType type, Transform parentTF, RoleIconData rcData, Action<RoleStyleItem> select, BaseView headView = null)
    {
        var dcItem = base.CreateItemByData(type, parentTF, rcData, select);
        if (dcItem == null)
        {
            return null;
        }
        dcItem.UpdateItemIsNew(false);
        dcItem.UpdateItemCollect(true);
        AddCollectItem(type, dcItem);
        return dcItem;
    }

    public void CancelCollectClothingItem(ClassifyType type, int id)
    {
        var item = GetCollectItem(type, id);
        if (item)
        {
            RemoveCollectItem(item.gameObject);
            RemoveCollectItem(type, id);
            BaseView.Ins.UpdateItemCollect(type, id, false);
        }
    }

    public void CancelUgcClothingItem(RoleUGCIconData data)
    {
        RoleStyleUgcItem item = null;
        if (itemCollectUgcListDict.TryGetValue(data.mapId, out item))
        {
            itemCollectUgcListDict.Remove(data.mapId);
            RemoveCollectItem(item.gameObject);
        }
        //通过UGCType获取对应的BaseView
        var typeView = RoleClassifiyView.Ins.GetViewByType((ClassifyType)data.classifyType);
        //处理UGC的收藏态
        var iconView = typeView.GetComponentInChildren<RoleUgcBaseView>(true);
        if (iconView)
        {
            iconView.UpdateUgcDataCollect(data.mapId, false);
        }
        //处理DC的收藏态
        var digitalView = typeView.GetComponentInChildren<RoleDCBaseView>(true);
        if (digitalView)
        {
            digitalView.UpdateItemCollect(UGCClothesResType.UGC, 0, data.mapId, false);
        }
        //处理ugc-Airdrop的收藏状态
        var aItem = RoleMenuView.Ins.GetView<AirdropView>().GetUgcItem(data.mapId);
        if (aItem) aItem.UpdateItemCollect(false);
    }

    /**
    * 收藏UGC服饰(主要为UGC衣服)
    */
    //TODO: 提取UGC Item的创建流程, 提取UGC Item的收藏更新流程
    public void CollectUgcClothingItem(RoleUGCIconData data, bool addCollect = false)
    {
        var item = Instantiate(ugcItem, IconParent);
        if (data.classifyType == 0)
        {
            //为45版本前数据兼容(45版本前只有ugc衣服, classifyType == 0必然是如下类型)
            data.classifyType = (int)ClassifyType.ugcCloth;
        }
        //通过UGCType获取对应的BaseView
        var typeView = RoleClassifiyView.Ins.GetViewByType((ClassifyType)data.classifyType);
        //DC处理
        if (data.grading == (int)RoleResGrading.DC)
        {
            var dciconView = typeView.GetComponentInChildren<RoleDCBaseView>(true);
            if (dciconView)
            {
                item.Init(data, dciconView.OnUgcItemSelect);
                //处理ugc-Airdrop的收藏状态
                var oItem = dciconView.GetUgcItem(data.mapId);
                if (oItem) oItem.UpdateItemCollect(true);
            }
        }
        else
        {
            var ugciconView = typeView.GetComponentInChildren<RoleUgcBaseView>(true);
            if (ugciconView)
            {
                item.Init(data, ugciconView.OnUgcItemSelect);
            }
        }
        AddCollectUgcItem(item);
        if (addCollect)
        {
            collectList.Insert(0, item.gameObject);
        }
        else
        {
            collectList.Add(item.gameObject);
        }
        //处理ugc-Airdrop的收藏状态
        var aItem = RoleMenuView.Ins.GetView<AirdropView>().GetUgcItem(data.mapId);
        if (aItem) aItem.UpdateItemCollect(true);

        Tips.SetActive(false);
        item.isCollected = true;
        item.rcData.isFavorites = 1;
        item.rcData.isNew = 0;
        item.newImage.SetActive(false);
        item.SetCollectTagVisible();
        UpdateCollectList();
        item.StyleBtn.onClick.AddListener(() =>
        {
            OnSelectClick(item.gameObject);
        });
        item.LoadClothCover();
        item.gameObject.SetActive(true);
        if (data.classifyType == (int)ClassifyType.ugcPatterns)
        {
            item.CanAdjust = true;
            var pgctype = RoleConfigDataManager.Inst.GetPGCTypeByUGCType((ClassifyType)data.classifyType);
            item.SetCustomView(() => { AdjustViewManager.Inst.OpenAdjustView(pgctype, this); });
        }
    }

    public void UpdateCollectList()
    {
        for (int i = 0; i < collectList.Count; i++)
        {
            var item = collectList[i];
            if (item)
            {
                item.transform.SetSiblingIndex(i + 1);
            }
        }
        if (curItem != null)
        {
            SetSelectState(curItem, false);
            curItem = null;
        }
    }

    public void ClearCollectList()
    {
        ClearItemCollectListDict();
        ClearItemCollectUgcListDict();
        collectList.Clear();
    }

    private void OnSelectClick(GameObject item)
    {
        if (curItem == item)
        {
            return;
        }

        if (curItem != null)
        {
            SetSelectState(curItem, false);
        }
        curItem = item;
        SetSelectState(curItem, true);
    }

    private void OnSelectClick(RoleStyleItem item)
    {
        OnSelectClick(item.gameObject);
    }

    public void SetSelectState(GameObject item, bool isSelected)
    {
        var roleItem = item.GetComponent<RoleStyleItem>();
        if (roleItem)
        {
            roleItem.SetSelectState(isSelected);
        }
        else
        {
            var ugcItem = item.GetComponent<RoleStyleUgcItem>();
            if (ugcItem)
            {
                ugcItem.SetSelectState(isSelected);
            }
        }
    }

    private void OnDestroy()
    {
    }
}

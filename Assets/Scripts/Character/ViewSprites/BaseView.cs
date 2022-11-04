using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RedDot;
using SavingData;
using UnityEngine;
using UnityEngine.U2D;

public enum ItemDictType
{
    AllOwned, //所有已拥有: Original+DC+Rewards
    News, //上新合集
    SpecialShow, //DC+Rewards(仅展示): 此类型保存形象需要校验
    DCView, //DC(官方页): 此类型保存形象需要校验
    DCViewAll //DC(官方页): 此类型保存形象需要校验
}

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象各个面板UI显示的父类
/// Date: 2022/4/2 10:21:30
/// </summary>
public class BaseView : MonoBehaviour
{
    public SpriteAtlas sprite;
    public ClassifyType classifyType;
    public BodyPartType bodyPart = BodyPartType.body; //用于请求后端列表
    [HideInInspector]
    public static RoleController rController;
    [HideInInspector]
    public static RoleData roleData;
    [HideInInspector]
    public static RoleConfigData roleConfigData;
    [HideInInspector]
    public static RoleColorConfig roleColorConfigData;
    public static BaseView Ins;
    private static Dictionary<ItemDictType, Dictionary<ClassifyType, Dictionary<int, RoleStyleItem>>> itemListDict;

    protected List<AdjustItemContext> mAdjustItemContexts;
    private void Awake()
    {
        if (Ins == null)
        {
            Ins = this;
            itemListDict = new Dictionary<ItemDictType, Dictionary<ClassifyType, Dictionary<int, RoleStyleItem>>>();
            itemListDict.Add(ItemDictType.AllOwned, new Dictionary<ClassifyType, Dictionary<int, RoleStyleItem>>());
            itemListDict.Add(ItemDictType.News, new Dictionary<ClassifyType, Dictionary<int, RoleStyleItem>>());
            itemListDict.Add(ItemDictType.SpecialShow, new Dictionary<ClassifyType, Dictionary<int, RoleStyleItem>>());
            itemListDict.Add(ItemDictType.DCView, new Dictionary<ClassifyType, Dictionary<int, RoleStyleItem>>());
            itemListDict.Add(ItemDictType.DCViewAll, new Dictionary<ClassifyType, Dictionary<int, RoleStyleItem>>());
        }
        mAdjustItemContexts = new List<AdjustItemContext>();
        OnAwake();
    }
    public virtual void OnAwake()
    {

    }

    public static void InitData(RoleConfigData data, RoleData roledata,RoleColorConfig roleColorConfig,RoleController roleController)
    {
        roleConfigData = data;
        roleData = roledata;
        rController = roleController;
        roleColorConfigData=roleColorConfig;
    }

    public static void UpdateRoleData(RoleData roledata)
    {
        roleData = roledata;
    }
    public float GetSliderValue(Vec3[] limit, Vec3 curVec, VecAxis vAxis = VecAxis.None)
    {
        Vector3 max = limit[1];
        Vector3 min = limit[0];
        Vector3 cur = curVec;
        switch (vAxis)
        {
            case VecAxis.X:
                max.y = 0;
                max.z = 0;
                min.y = 0;
                min.z = 0;
                cur.y = 0;
                cur.z = 0;
                break;
            case VecAxis.XY:
                max.z = 0;
                min.z = 0;
                cur.z = 0;
                break;
            case VecAxis.YZ:
                max.x = 0;
                min.x = 0;
                cur.x = 0;
                break;
            case VecAxis.Z:
                max.y = 0;
                max.x = 0;
                min.y = 0;
                min.x = 0;
                cur.y = 0;
                cur.x = 0;
                break;
            case VecAxis.Y:
                max.x = 0;
                max.z = 0;
                min.x = 0;
                min.z = 0;
                cur.x = 0;
                cur.z = 0;
                break;
        }
        float length = (max - min).magnitude;
        float curLength = (cur - min).magnitude;
        float progress = 0;
        if (length == 0)
        {
            LoggerUtils.LogError("分母不能为0" + max + min + cur);
            progress = 1;
        }
        else
        {
            progress = (float)Math.Round(curLength / length, 2);
            progress = Mathf.Clamp01(progress);
        }
        return progress;
    }
    public Vec3 GetValueBySlider(Vec3 curVec, Vec3[] limit, float progress, VecAxis vAxis = VecAxis.None)
    {
        Vector3 max = limit[1];
        Vector3 min = limit[0];
        var cur = curVec.Clone();
        switch (vAxis)
        {
            case VecAxis.X:
                cur.x = Mathf.Lerp(min.x, max.x, progress);
                break;
            case VecAxis.XY:
                cur.x = Mathf.Lerp(min.x, max.x, progress);
                cur.y = Mathf.Lerp(min.y, max.y, progress);
                break;
            case VecAxis.YZ:
                cur.y = Mathf.Lerp(min.y, max.y, progress);
                cur.z = Mathf.Lerp(min.z, max.z, progress);
                break;
            case VecAxis.None:
                cur = Vector3.Lerp(min, max, progress);
                break;
            case VecAxis.Z:
                cur.z = Mathf.Lerp(min.z, max.z, progress);
                break;
            case VecAxis.Y:
                cur.y = Mathf.Lerp(min.y, max.y, progress);
                break;
        }

        cur.x = (float)Math.Round(cur.x, 4);
        cur.y = (float)Math.Round(cur.y, 4);
        cur.z = (float)Math.Round(cur.z, 4);
        return cur;
    }

    private void OnDestroy()
    {
        Ins = null;
        itemListDict = null;
    }

    public void AddItemList(ItemDictType dictType, ClassifyType type, RoleStyleItem item)
    {
        var itemId = item.rcData.id;
        Dictionary<int, RoleStyleItem> itemDict;
        if (!itemListDict[dictType].ContainsKey(type))
        {
            itemDict = new Dictionary<int, RoleStyleItem>();
            itemDict[itemId] = item;
            itemListDict[dictType][type] = itemDict;
        }
        else
        {
            itemDict = itemListDict[dictType][type];
            if (!itemDict.ContainsKey(itemId))
            {
                itemDict[itemId] = item;
            }
        }
    }
    public virtual GameObject GetIconItem(RoleIconData roleIconData, BaseView view)
    {
        return view.GetComponentInChildren<RoleStyleView>(true).IconItem;
    }
    public RoleStyleItem GetItem(ItemDictType dictType, ClassifyType type, int id)
    {
        Dictionary<int, RoleStyleItem> itemDict = null;
        if (!itemListDict[dictType].TryGetValue(type, out itemDict))
        {
            return null;
        }
        RoleStyleItem item = null;
        itemDict.TryGetValue(id, out item);
        return item;
    }

    public void UpdateItemCollect(ClassifyType type, int id, bool isCollected)
    {
        UpdateItemCollect(ItemDictType.AllOwned, type, id, isCollected);
        UpdateItemCollect(ItemDictType.News, type, id, isCollected);
        UpdateItemCollect(ItemDictType.SpecialShow, type, id, isCollected);
        UpdateItemCollect(ItemDictType.DCView, type, id, isCollected);
        UpdateItemCollect(ItemDictType.DCViewAll, type, id, isCollected);
        //暂特殊处理官方Airdrop
        var aItem = RoleMenuView.Ins.GetView<AirdropView>().GetPgcItem((int)type, id);
        if (aItem) aItem.UpdateItemCollect(isCollected);
        //暂特殊处理已拥有nft衣服
        if (type == ClassifyType.outfits)
        {
            var clothesData = RoleConfigDataManager.Inst.GetClothesById(id);
            if (clothesData.grading == (int)RoleResGrading.DC)
            {
                HandleNftClothCollect(id, isCollected);
            }
        }
    }

    private void UpdateItemCollect(ItemDictType dictType, ClassifyType type, int id, bool isCollected)
    {
        var item = GetItem(dictType, type, id);
        if (item)
        {
            item.UpdateItemCollect(isCollected);
        }
    }
    private void HandleNftClothCollect(int pgcId, bool isCollected)
    {
        var outfitsView = RoleMenuView.Ins.GetView<OutfitsView>();
        var iconView = outfitsView.GetComponentInChildren<RoleClothDigitalView>(true);
        if (iconView)
        {
            iconView.UpdateItemCollect(UGCClothesResType.PGC, pgcId, string.Empty, isCollected);
        }
    }
    public void InitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {
        OnInitRedDot(rootItem,datas, tree);
    }
    protected virtual void OnInitRedDot(RoleClassifyItem rootItem, List<AvatarRedDots> datas, RedDotTree tree)
    {

    }
    protected void NewUserUiSetting(GameObject toggleParent, GameObject[] Panels)
    {
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
        {
            toggleParent.gameObject.SetActive(false);
            for (int i = 0; i < Panels.Length; i++)
            {
                var rectTrans = Panels[i].GetComponent<RectTransform>();
                rectTrans.offsetMin = Vector2.zero;
                rectTrans.offsetMax = Vector2.zero;
            }
        }
    }

    public static List<T> GetConfigDataFilter<T>(List<T> oDataList, Func<T, bool> filter) where T : RoleIconData
    {
        var nDataList = new List<T>();
        oDataList.ForEach(data =>
        {
            if (filter(data))
            {
                nDataList.Add(data);
            }
        });
        return nDataList;
    }

    //适用于混排面板
    protected virtual RoleStyleItem CreateItemByData(ClassifyType type, Transform parentTF, RoleIconData rcData, Action<RoleStyleItem> select, BaseView headView = null)
    {
        var bView = RoleClassifiyView.Ins.GetViewByType(type);
        if (bView == null || rcData == null)
        {
            return null;
        }
        //背包临时特殊处理
        GameObject prefGO = bView.GetIconItem(rcData, bView);
        var itemGO = Instantiate(prefGO, parentTF);
        var item = itemGO.GetComponent<RoleStyleItem>();
        item.type = type;
        item.Init(rcData, select, bView.sprite);
        item.StyleBtn.onClick.AddListener(() =>
        {
            bView.OnSelectItem(rcData.id, item);
        });
        if (item is RoleCustomStyleItem)
        {
            var cItem = item as RoleCustomStyleItem;
            cItem.CustomBtn.onClick.AddListener(() =>
            {
                AdjustViewManager.Inst.OpenAdjustView(cItem.type, headView ? headView : this);
            });
        }
        return item;
    }

    public virtual void OnSelect()
    {

    }
    /// <summary>
    /// 刷新选中态
    /// </summary>
    public virtual void UpdateSelectState()
    {

    }
    public virtual void OnSelectItem(int itemId, RoleStyleItem roleStyleItem)
    {

    }
    public void ClearRed(VNode node, string redstring)
    {
        if (node != null && node.mLogic.Count > 0)
        {
            node.mLogic.ChangeCount(node.mLogic.Count - 1);
            RoleMenuView.Ins.mAvatarRedDotManager.ReqCleanRedDot(redstring);
        }
    }

    public void PlayLoadingAnime(RoleIconData rs, bool enable)
    {
        if(rs.rc != null)rs.rc.PlayLoadingAnim(enable);
    }

    public List<PGCInfo> GetAllDcPgcInfos()
    {
        var ugcClothInfo = GameManager.Inst.ugcClothInfo;
        if (ugcClothInfo == null)
        {
            return null;
        }
        List<PGCInfo> dcLists = new List<PGCInfo>();
        if (ugcClothInfo.dcPgcInfos != null && ugcClothInfo.dcPgcInfos.Length > 0)
        {
            dcLists.AddRange(ugcClothInfo.dcPgcInfos);
        }
        else if(!ugcClothInfo.dcPgcInfo.Equals(default(PGCInfo)))
        {
            dcLists.Add(ugcClothInfo.dcPgcInfo);
        }
        if(dcLists.Count <= 0)
        {
            return null;
        }
        return dcLists;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using AvatarRedDotSystem;
using RedDot;
using SavingData;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class RoleBagClassifyView : MonoBehaviour
{
    public GameObject bagTypeItem;
    public Transform ItemPat;
    private List<RoleBagClassifyItem> allTypeItems = new List<RoleBagClassifyItem>();
    public RoleStyleView[] allClassifyView;
    private BagCompType curSelectType = BagCompType.Backpack;
    private Dictionary<ClassifyType, List<RoleStyleItem>> allItemDict = new Dictionary<ClassifyType, List<RoleStyleItem>>();
    private List<BagCompTypeData> configDatas = new List<BagCompTypeData>();
    public Action<RoleStyleView> GetAllItemList;

    public void InitClassifyItem(List<BagCompTypeData> datas)
    {
        this.configDatas = datas;
        foreach (var item in configDatas)
        {
            var classifyItem = Instantiate(bagTypeItem, ItemPat);

            var itemScript = classifyItem.GetComponent<RoleBagClassifyItem>();
            itemScript.SetData(item);
            itemScript.SetAction(OnSelectClassifyItem);
            allTypeItems.Add(itemScript);
        }
    }
    public void SetReqAction(Action<RoleStyleView> getItem)
    {
        this.GetAllItemList = getItem;
    }
    public void AttachRedDot()
    {
        foreach (var item in allTypeItems)
        {
            item.AttachRedDot();
        }
    }
    /// <summary>
    /// 同步选中三级分类
    /// </summary>
    public void UpdateSelectTab()
    {
        SetSelectByType(curSelectType);
    }
    /// <summary>
    /// 选中对应Type(公共avatar)
    /// </summary>
    public void SetSelectByType(BagCompType type)
    {
        OnSelectClassifyItem(type);
    }
    private void OnSelectClassifyItem(BagCompType type)
    {
        curSelectType = type;
        OnUISelect(type);
        SetViewSelect(type);
        GetAllItemList?.Invoke(GetClassifyView(type));
    }
    private void OnUISelect(BagCompType type)
    {
        allTypeItems.ForEach(x => x.SetSelectState(type == (BagCompType)x.data.bagCompType));
    }
    private void SetViewSelect(BagCompType type)
    {
        Array.ForEach<RoleStyleView>(allClassifyView, (x) => x.gameObject.SetActive(x.componentType == (int)type));
    }
    private RoleStyleView GetClassifyView(BagCompType type)
    {
        int index = Array.FindIndex<RoleStyleView>(allClassifyView, (x) => x.componentType == (int)type);
        return allClassifyView[index];
    }
}

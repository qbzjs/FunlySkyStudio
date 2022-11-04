using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using SavingData;

/// <summary>
/// Author:Meimei-LiMei
/// Description:搭配保存界面
/// Date: 2022/4/24 13:37:33
/// </summary>
public class SavesView : BaseView
{
    public GameObject Tips;
    public List<RoleMatchItem> matchList = new List<RoleMatchItem>();
    public Transform IconParent;
    public RoleMatchItem matchItem;
    public RoleMatchItem curItem;
    public const int MaxCount = 99;
    private const int pageSize = 12;
    private ReqQuerry httpReq = new ReqQuerry();

    private void Awake()
    {
        ClearMatchList();
    }

    public void Start()
    {
        RoleMenuView.Ins.SetAction(InitSavesView);
    }

    public void InitSavesView()
    {
        this.classifyType = ClassifyType.saves;
        if (matchList.Count <= 0)
        {
            Tips.SetActive(true);
        }
        else
        {
            Tips.SetActive(false);
        }
    }

    public void GetAllSavedMatchList()
    {
        httpReq.toUid = GameManager.Inst.ugcUserInfo.uid;
        httpReq.pageSize = pageSize;
        httpReq.cookie = "";
        GetMatchListRequest();
    }

    private void GetMatchListRequest()
    {
        HttpUtils.MakeHttpRequest("/image/getCollocation", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(httpReq), GetSavedMatchListSuccess, GetSavedMatchListFail);
    }

    public void GetSavedMatchListSuccess(string msg)
    {
        LoggerUtils.Log("CollectionsView GetCollectListSuccess. msg is  " + msg);
        HttpResponDataStruct responseData = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        if (string.IsNullOrEmpty(responseData.data))
        {
            LoggerUtils.LogError("OnGetSavedMatchList : repData.data == null");
            return;
        }
        MatchDataList matchDataList = JsonConvert.DeserializeObject<MatchDataList>(responseData.data);
        httpReq.cookie = matchDataList.cookie;
        if (matchDataList.collocationInfo != null)
        {
            foreach (var matchData in matchDataList.collocationInfo)
            {
                var item = InitItem();
                item.imgName = matchData.name;
                item.coverUrl = matchData.coverUrl;
                item.roleData = JsonConvert.DeserializeObject<RoleData>(matchData.data);
                item.UpdateIconImg(item.coverUrl);
            }
        }
        if (matchDataList.isEnd != 1)
        {
            GetMatchListRequest();
        }
    }

    public void GetSavedMatchListFail(string err)
    {
        LoggerUtils.Log("GetSavedMatchListFail errInfo: " + err);
    }

    public RoleMatchItem InitItem(bool addSave = false)
    {
        var item = Instantiate(matchItem, IconParent);

        item.SetIconImgVisible(false);
        item.StyleBtn.onClick.AddListener(() =>
        {
            item.OnSelectMatchItem();
            OnSelectClick(item);
        });
        item.SetSelectState(false);
        AddMatchItem(item, addSave);
        return item;
    }

    public bool IsOverMaxCount()
    {
        if (matchList.Count >= MaxCount)
        {
            CharacterTipPanel.ShowToast("Oops! Exceed limit:(");
            return true;
        }
        return false;
    }

    public void AddMatchItem(RoleMatchItem item, bool addSave)
    {
        if (addSave)
        {

            matchList.Insert(0, item);
        }
        else
        {
            matchList.Add(item);
        }
        Tips.SetActive(false);
        UpdateMatchList();
    }

    public void RemoveMatchItem(RoleMatchItem item)
    {
        matchList.Remove(item);
        item.gameObject.SetActive(false);
        Destroy(item);
        if (matchList.Count <= 0)
        {
            Tips.SetActive(true);
        }
    }

    private void UpdateMatchList()
    {
        for (int i = 0; i < matchList.Count; i++)
        {
            var matchItem = matchList[i];
            if (matchItem)
            {
                matchItem.transform.SetSiblingIndex(i + 1);
            }
        }
    }

    public void ClearMatchList()
    {
        matchList.Clear();
    }

    private void OnDestroy()
    {
        ClearMatchList();
    }

    // 清除当前选中态
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
        base.UpdateSelectState();
        ClearSelectState();
    }
    public virtual void OnSelectClick(RoleMatchItem item)
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

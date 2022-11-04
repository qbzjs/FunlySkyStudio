using System;
using System.Collections;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public class ResStorePanel : BasePanel<ResStorePanel>
{
    public RectTransform content;
    public int viewPortH;
    public Button ResStoreBtn;
    public Button GetPropBtn;
    public GameObject HadProp;
    public GameObject NoProp;
    public GameObject LoadingPanel;

    [SerializeField]
    private GameObject title;
    [SerializeField]
    private GameObject searchPanel;
    [SerializeField]
    private GameObject propView;
    [SerializeField]
    private Button inputSrchBtn;
    [SerializeField]
    private Button openSrchBtn;
    [SerializeField]
    private Button cancelSrchBtn;
    [SerializeField]
    private Button cleerSrchBtn;
    [SerializeField]
    private Text searchText;
    [SerializeField]
    private Text noPropText;

    private Dictionary<int, GameObject> AlreadySowItem = new Dictionary<int, GameObject>();
    private int oldMinIndex = -1;
    private int oldMaxIndex = -1;
    private Vector2 ResItemSize = new Vector2(240, 240);
    private const int BagColumnNum = 5;
    private const int ResItemContentSize = 264;
    private Color oTextColor = new Color(0.48f, 0.48f, 0.48f);
    private bool isSearchState = false;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        //Init Panel
        IsHadProp(false);
        ResBagManager.Inst.RefreshResList();
        ResStoreBtn.onClick.AddListener(OnResStoreBtnClick);
        GetPropBtn.onClick.AddListener(OnResStoreBtnClick);

        inputSrchBtn.onClick.AddListener(OnSearchInputClick);
        openSrchBtn.onClick.AddListener(OnOpenSearchPanel);
        cancelSrchBtn.onClick.AddListener(OnCloseSearchPanel);
        cleerSrchBtn.onClick.AddListener(OnCleerSearch);

#if !UNITY_EDITOR
        MobileInterface.Instance.AddClientRespose(MobileInterface.updateSelfResourceStore, UpdateResourceStore);
#endif
    }
    
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        GameEditModePanel.Show();
        SceneGizmoPanel.Show();
        BasePrimitivePanel.Show();
        if (isSearchState)
        {
            GoToContentTop();
            OnCloseSearchPanel();
            return;
        }
        CheckShowOrHide();
#if UNITY_EDITOR
        HttpUtils.IsMaster = true;
        ResBagManager.Inst.UpdateSelfResList();
#endif
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        LockHideManager.Inst.CheckHidePanelVisable();
        itemClick = null;
    }

    public void IsHadProp(bool IsActive)
    {
        HadProp.SetActive(IsActive);
        NoProp.SetActive(!IsActive);
        LoadingPanel.SetActive(false);

        if (IsActive)
        {
            propView.SetActive(true);
        }
        else
        {
            string textStr = isSearchState ? "We didn't find anything..." : "No props..yet";
            LocalizationConManager.Inst.SetLocalizedContent(noPropText, textStr);
        }
    }

    public void RefreshContentSize()
    {
        var mapInfos = isSearchState ? ResBagManager.Inst.srchMapInfos : ResBagManager.Inst.mapInfos;

        int Row = mapInfos.Count / BagColumnNum;
        if(mapInfos.Count % BagColumnNum != 0)
        {
            ++Row;
        }
        content.sizeDelta = new Vector2(content.sizeDelta.x, Row * ResItemContentSize);
    }

    public void OnTestBtnClick()
    {
        //ResBagManager.Inst.RefreshMapInfos();
        //RefreshContentSize();
        ResBagManager.Inst.UpdateSelfResList();
    }

    public void OnResStoreBtnClick()
    {
        MobileInterface.Instance.OpenResourceStore();
    }

    private void UpdateResourceStore(string content)
    {
        if (isSearchState)
            OnCloseSearchPanel();
        ResBagManager.Inst.UpdateSelfResList();
    }

    private void OnSearchInputClick()
    {
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = LocalizationConManager.Inst.GetLocalizedText("Enter text..."),
            inputMode = 2,
            maxLength = 80,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = "",
            returnKeyType = (int)ReturnType.Search
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, StartSearch);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }

    private void OnOpenSearchPanel()
    {
        isSearchState = true;
        ShowSearchPanel(true);
        propView.SetActive(false);
        ResBagManager.Inst.srchMapInfos.Clear();
        ResBagManager.Inst.ActiveLoadAllCoroute(false);
    }

    private void OnCloseSearchPanel()
    {
        isSearchState = false;
        ShowSearchPanel(false);

        //case : did not find
        if (!propView.activeSelf)
        {
            propView.SetActive(true);
            //case : found no result
            if (NoProp.activeSelf)
            {
                //IsHadProp(true);
                HadProp.SetActive(true);
                NoProp.SetActive(false);
            }
            ResBagManager.Inst.ActiveLoadAllCoroute(true);
        }
        //case : found result
        else
            ResBagManager.Inst.RefreshResList();

        CheckShowOrHide();
    }

    private void OnCleerSearch()
    {
        LocalizationConManager.Inst.SetLocalizedContent(searchText, "Search");
        searchText.color = oTextColor;
        cleerSrchBtn.gameObject.SetActive(false);
    }

    private void OnEnterSearch(string str)
    {
        searchText.text = str;
        searchText.color = Color.white;
        cleerSrchBtn.gameObject.SetActive(true);
    }

    private void ShowSearchPanel(bool isShow)
    {
        title.SetActive(!isShow);
        ResStoreBtn.gameObject.SetActive(!isShow);
        openSrchBtn.gameObject.SetActive(!isShow);

        OnCleerSearch();
        searchPanel.SetActive(isShow);
        LoadingPanel.SetActive(false);
    }

    private void StartSearch(string str)
    {
        MobileInterface.Instance.DelClientResponse(MobileInterface.hideKeyboard);
        if (string.IsNullOrEmpty(str))
            return;
        ShowSearchPanel(true);
        OnEnterSearch(str);
        ResBagManager.Inst.RefreshSearchResList(str);
    }

    public void GoToContentTop()
    {
        Vector3 contentPos = content.localPosition;
        contentPos.y = 0;
        content.localPosition = contentPos;
    }

    public void CheckShowOrHide()
    {
        //LoggerUtils.Log("CheckShowOrHide");
        if (content.anchoredPosition.y < -1)
        {
            return;
        }

        int minIndex = (int)(content.anchoredPosition.y / ResItemContentSize) * BagColumnNum;
        int maxIndex = (int)(content.anchoredPosition.y + viewPortH) / ResItemContentSize * BagColumnNum + BagColumnNum - 1;

        var mapInfos = isSearchState ? ResBagManager.Inst.srchMapInfos : ResBagManager.Inst.mapInfos;

        if (maxIndex >= mapInfos.Count)
        {
            maxIndex = mapInfos.Count - 1;
            if (ResBagManager.Inst.isEnd != 1)
            {
                //LoggerUtils.Log("ResBagManager.Inst.isEnd != 1");
                if (!isSearchState)
                    ResBagManager.Inst.GetNextPageResList();
                else
                    ResBagManager.Inst.GetNextPageSearchResList();
            }
        }

        for (int i = oldMinIndex; i < minIndex; ++i)
        {
            if (AlreadySowItem.ContainsKey(i) && AlreadySowItem[i] != null)
            {
                ResBagManager.Inst.PushItem(AlreadySowItem[i]);
                AlreadySowItem.Remove(i);
            }
        }

        for (int i = maxIndex + 1; i <= oldMaxIndex; ++i)
        {
            if (AlreadySowItem.ContainsKey(i))
            {
                ResBagManager.Inst.PushItem(AlreadySowItem[i]);
                AlreadySowItem.Remove(i);
            }
        }

        //if (oldMinIndex != minIndex)
        //{
        //    Vector3 contentPos = content.localPosition;
        //    contentPos.y = contentPos.y + (oldMinIndex - minIndex) / 5 * 264;
        //    content.localPosition = contentPos;
        //}

        oldMaxIndex = maxIndex;
        oldMinIndex = minIndex;

        for (int i = minIndex; i <= maxIndex; ++i)
        {
            int index = i;
            if (index > mapInfos.Count)
            {
                return;
            }
            var menuInfo = mapInfos[index];
            if (menuInfo == null)
            {
                return;
            }

            if (AlreadySowItem.ContainsKey(index)) {
                //LoggerUtils.Log("ContainsKey");
                AlreadySowItem[index].GetComponent<ResBagItem>().SetBagItemInfo(menuInfo);
                AlreadySowItem[index].GetComponent<ResBagItem>().SetOutsideItemClick(itemClick);
                continue;
            }
            else {
                GameObject item = ResBagManager.Inst.GetItem();
                AlreadySowItem.Add(index, item);
                item.GetComponent<ResBagItem>().SetBagItemInfo(menuInfo);
                item.name = index.ToString();
                item.transform.SetParent(content);
                item.transform.localScale = Vector3.one;
                item.GetComponent<RectTransform>().sizeDelta = ResItemSize;
                item.transform.localPosition = new Vector3((index % BagColumnNum) * ResItemContentSize, -index / BagColumnNum * ResItemContentSize, 0);
                item.GetComponent<ResBagItem>().SetOutsideItemClick(itemClick);
            }
        }
    }

    #region 点击素材Item回调

    private static Action<MapInfo> itemClick;
    public static void SetItemCallback(Action<MapInfo> itemClickPara)
    {
        itemClick = itemClickPara;
        LoggerUtils.Log("ResStorePanel SetItemCallback");
    }

    #endregion

}

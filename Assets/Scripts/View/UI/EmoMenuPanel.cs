using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using RedDot;
using UnityEngine;
using UnityEngine.UI;

public class EmoMenuPanel : BasePanel<EmoMenuPanel>
{
    [HideInInspector]
    public List<EmoIconData> emoicons = new List<EmoIconData>();
    [HideInInspector]
    public List<EmoIconData> emoIconDataList
    {
        get
        {
            if (emoicons.Count == 0)
            {
                emoicons = ResManager.Inst.LoadJsonRes<EmoConfigData>("Configs/Emotion/EmoConfigData").emoIcons;
            }
            return emoicons;
        }
    }

    // 表情信息
    public class EmoItemData
    {
        public GameObject item; // 表情 Item
        public EmoIconData data; // Item 数据
    }
    public class EmoItem
    {
        public GameObject mGameObject;
        public EmoItemData mData;
        public EmoMenuPanel mPanel;
        public VNode mVNode;
        public EmoItem(GameObject obj, EmoItemData data, EmoMenuPanel panel)
        {
            mGameObject = obj;
            mData = data;
            mPanel = panel;
        }
        public void Init()
        {
            //附加红点
            mGameObject.GetComponent<Button>().onClick.AddListener(OnClick);
        }
        private void OnClick()
        {
            if (mPanel.isEditCollectList)
            {
                mPanel.EditEmoItem(mData);
            }
            else
            {
                mPanel.ClickEmoteBtn(mData.data.id);
            }
            if (mVNode != null && mVNode.mLogic.Count > 0)
            {
                int oldValue = mVNode.mLogic.Count;
                mVNode.mLogic.ChangeCount(oldValue - 1);
                RequestCleanRedDot();
            }
        }
        private void RequestCleanRedDot()
        {
            PlayModePanel.Instance.mSceneRedDotManager.RequestCleanEmoRedDot(mData.data.id);
        }
        public void AttachEmoItemRedDot()
{
            mVNode = AttachEmoItemRedDot(mData.data.emoType, mGameObject);

            if (mVNode != null && mVNode.mLogic != null)
            {
                mVNode.mLogic.mData = mData;
                mVNode.mLogic.AddListener(ChangedCountCallBack);
                mVNode.mLogic.ChangeCount(1);
            }
        }
        private VNode AttachEmoItemRedDot(int emoType, GameObject item)
        {
            VNode vNode = null;
            if (emoType == (int)EmoTypeEnum.EMOJI)
            {
                vNode = InternalAttachEmoItemRedDotNode(item, (int)EmoRedDotSystem.ENodeType.Emoji, (int)EmoRedDotSystem.ENodeType.EmojiItem);
            }
            else if (emoType == (int)EmoTypeEnum.SINGLE_EMO)
            {
                vNode = InternalAttachEmoItemRedDotNode(item, (int)EmoRedDotSystem.ENodeType.SingleEmo, (int)EmoRedDotSystem.ENodeType.SingleEmoItem);
            }
            else if (emoType == (int)EmoTypeEnum.DOUBLE_EMO)
            {
                vNode = InternalAttachEmoItemRedDotNode(item, (int)EmoRedDotSystem.ENodeType.DoubleEmo, (int)EmoRedDotSystem.ENodeType.DoubleEmoItem);
            }
            else if (emoType == (int)EmoTypeEnum.STATE_EMO)
            {
                vNode = InternalAttachEmoItemRedDotNode(item, (int)EmoRedDotSystem.ENodeType.StateEmo, (int)EmoRedDotSystem.ENodeType.StateEmoItem);
            }
            
            return vNode;
        }
        private VNode InternalAttachEmoItemRedDotNode(GameObject target, int parentType, int nodeType, ERedDotPos dotPos = ERedDotPos.TopRight)
        {
            RedDotTree tree = PlayModePanel.Instance.mSceneRedDotManager.Tree;
            VNode dot = tree.AddRedDot(target, parentType, nodeType, ERedDotPrefabType.Type1);
            return dot;
        }
        private void ChangedCountCallBack(int count)
        {
           
        }
    }
    // 收藏表情信息
    public class CollectEmoInfo
    {
        public int[] emojiId; // 要收藏的表情 Id List
    }

    [HideInInspector]
    public Dictionary<int, EmoItemData> EmoBtnDir = new Dictionary<int, EmoItemData>(); // 所有表情 List
    public List<EmoItemData> CollectEditEmoList = new List<EmoItemData>(); // 收藏编辑 List
    public List<EmoItemData> CollectedEmoList = new List<EmoItemData>(); //已收藏 List
    public Transform itemParent;
    public GameObject itemPrefab;
    public Button bgBtn;
    [SerializeField]
    private GameObject CollectScrollView; // 收藏编辑列表
    [SerializeField]
    private Transform collectItemParent;
    [SerializeField]
    private GameObject CollectedScrollView;// 已收藏展示列表
    [SerializeField]
    private Transform collectedItemParent;
    [SerializeField]
    private GameObject addItemPrefab;
    [SerializeField]
    private Button CollectBtn, EmoBtn, SingleEmoBtn, DoubleEmoBtn, FinishBtn, StateEmoBtn;
    private List<Button> btnList = new List<Button>();
    private List<int> SelectedIds = new List<int>();
    private List<int> CurSentIds = new List<int>();
    public List<int> CurCollectedEmoList = new List<int>(); //当前收藏的 Id List
    private bool isEditCollectList = false; // 是否为收藏编辑态
    private EmoTypeEnum _curEmoTypeEnum; // 当前选中的表情类型，用于刷新表情列表的 Item 显示
    public GameObject emojiRedDot, singleEmoRedDot, doubleEmoRedDot, stateEmoRedDot;
    private Dictionary<EmoTypeEnum, GameObject> tabRedDics = new Dictionary<EmoTypeEnum, GameObject>();
    private Dictionary<int, int> emoReds = new Dictionary<int, int>();
    private Coroutine clickStateEmoCor;
    private bool isStateEmoRequesting = false; //是否当前正在请求状态动画中，状态表情和原先其他表情的区别是需要等回包，期间不允许播放其他表情
    private Dictionary<int, EmoItem> mEmoItems;

    // 表情分类
    public enum EmoTypeEnum
    {
        NOT_IN_PANEL = 0, //不在UI面板显示的动作
        EMOJI = 1, //Emoji 情绪表情
        SINGLE_EMO = 2, //单人动作表情
        DOUBLE_EMO = 3, //双人交互动作表情
        COLLECT_EMO = 4, // 收藏列表
        STATE_EMO = 5, //状态表情
    };

    public enum BtnTypeEnum
    {
        BTN_COLLECT = 0,
        BTN_EMOJI = 1,
        BTN_SINGLE_EMO = 2,
        BTN_DOUBLE_EMO = 3,
        BTN_STATE_EMO = 4,
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        emoReds = DataUtils.GetLocalEmoReds(GameInfo.Inst.myUid ?? string.Empty);
        tabRedDics = new Dictionary<EmoTypeEnum, GameObject>()
        {
            {EmoTypeEnum.EMOJI,emojiRedDot},
            { EmoTypeEnum.SINGLE_EMO,singleEmoRedDot},
            { EmoTypeEnum.DOUBLE_EMO,doubleEmoRedDot},
            { EmoTypeEnum.STATE_EMO,stateEmoRedDot},
        };
        SetCollectEditListVisible(false);
        InitAddEmoItemPrefab();
        InitEmoItem();
        bgBtn.onClick.AddListener(OnBgBtnClick);
        InitBtnListeners();
        btnList = new List<Button>() { CollectBtn, EmoBtn, SingleEmoBtn, DoubleEmoBtn, StateEmoBtn };
        GetAllCollectedEmoIds();

        // 默认选中表情Tab
        OnEmoBtnClick();

        if (PlayModePanel.Instance.mSceneRedDotManager.IsInited)
        {
            EmoRedDotInitedCallBack(true);
        }
        else
        {
            PlayModePanel.Instance.mSceneRedDotManager.AddListener(EmoRedDotInitedCallBack);
        }
    }
    private void EmoRedDotInitedCallBack(bool isInited)
    {
        if (isInited)
        {
            AttachEmoTableRedDotNode();

            List<int> redDotId = PlayModePanel.Instance.mSceneRedDotManager.mEmoIds;
            for (int i = 0; i < redDotId.Count; i++)
            {
                int id=redDotId[i];
                EmoItem item ;
                if (mEmoItems.TryGetValue(id,out item))
                {
                    item.AttachEmoItemRedDot();
                }
            }
        }
    }
    //这里给标签节点绑定逻辑Node，树的创建在PlayModePanel中，
    public void AttachEmoTableRedDotNode()
    {
        InternalAttachEmoTableRedDotNode(EmoBtn.gameObject, (int)EmoRedDotSystem.ENodeType.Emoji);
        InternalAttachEmoTableRedDotNode(SingleEmoBtn.gameObject, (int)EmoRedDotSystem.ENodeType.SingleEmo);
        InternalAttachEmoTableRedDotNode(DoubleEmoBtn.gameObject, (int)EmoRedDotSystem.ENodeType.DoubleEmo);
        InternalAttachEmoTableRedDotNode(StateEmoBtn.gameObject, (int)EmoRedDotSystem.ENodeType.StateEmo);
    }
    private VNode InternalAttachEmoTableRedDotNode(GameObject target, int nodeType)
    {
        RedDotTree tree = PlayModePanel.Instance.mSceneRedDotManager.Tree;
        VNode dot = tree.AddRedDot(target, (int)EmoRedDotSystem.ENodeType.Emo, nodeType, ERedDotPrefabType.Type1);
        return dot;
    }
    
    public override void OnBackPressed()
    {
        base.OnBackPressed();
        StopClickStateEmoCor();
    }
    private void OnDestroy()
    {
        StopClickStateEmoCor();
    }

    private void InitBtnListeners()
    {
        CollectBtn.onClick.AddListener(OnCollectBtnClick);
        EmoBtn.onClick.AddListener(OnEmoBtnClick);
        SingleEmoBtn.onClick.AddListener(OnSingleEmoBtnClick);
        DoubleEmoBtn.onClick.AddListener(OnDoubleEmoBtnClick);
        FinishBtn.onClick.AddListener(OnFinishBtnClick);
        StateEmoBtn.onClick.AddListener(OnStateEmoBtnClick);
    }

    private void InitEmoItem()
    {
        mEmoItems = new Dictionary<int, EmoItem>();
        foreach (var emoData in emoIconDataList)
        {
            if (emoData.emoType == (int)EmoTypeEnum.NOT_IN_PANEL)
            {
                //过滤不在面板显示的emo
                continue;
            }
            EmoItemData emoItemData = new EmoItemData();
            GameObject item = Instantiate(itemPrefab, itemParent);
            Text emoText = item.GetComponentInChildren<Text>();
            UpdateItemTagState(item, emoData.collected);
            LocalizationConManager.Inst.SetLocalizedContent(emoText, emoData.spriteName);
            emoItemData.item = item;
            emoItemData.data = emoData;
            EmoBtnDir[emoData.id] = emoItemData;
            EmoItem itemScrript = new EmoItem(item,emoItemData,this);
            itemScrript.Init();
            mEmoItems.Add(emoData.id, itemScrript);
        }
    }
    /**
    * 收藏编辑状态下，编辑 emo 按钮
    */
    private void EditEmoItem(EmoItemData emoItemData)
    {
        GameObject item = emoItemData.item;
        Transform isSpecialTrans = item.transform.Find("IsSpecial");
        Transform isSelectedTrans = item.transform.Find("isSelected");

        List<int> collectList = new List<int>();
        collectList.Add(emoItemData.data.id);
        if (!isSpecialTrans.gameObject.activeInHierarchy)
        {
            UpdateItemSelectState(item, !isSelectedTrans.gameObject.activeInHierarchy);
            bool isSelect = isSelectedTrans.gameObject.activeInHierarchy;
            UpdateCollectedEditList(collectList, isSelect, isSelect);
        }
        else
        {
            UpdateItemTagState(item, false);
            CollectEditEmoList.Remove(emoItemData);
            UpdateCollectedEditList(collectList, false, false);
        }
    }

    /**
    * 初始化收藏添加按钮
    */
    private void InitAddEmoItemPrefab()
    {
        EmoItemData emoItemData = new EmoItemData();
        GameObject item = Instantiate(addItemPrefab, collectedItemParent);
        emoItemData.item = item;
        // 收藏添加按钮的表情数据为空
        emoItemData.data = null;
        item.GetComponent<Button>().onClick.AddListener(() =>
        {
            SetCollectEditListVisible(true);
            RefreshCollectEditList();
            ShowCollectedEditList();
        });
    }

    /**
    * 根据表情类型显示列表
    */
    private void ShowEmoList(EmoTypeEnum type)
    {
        _curEmoTypeEnum = type;
        bool isCollectType = (type == EmoTypeEnum.COLLECT_EMO);
        itemParent.gameObject.SetActive(!isCollectType);
        CollectedScrollView.SetActive(isCollectType);
        if (isCollectType)
        {
            ShowCollectedEmoList();
        }
        foreach (var id in EmoBtnDir.Keys)
        {
            var emoItemData = EmoBtnDir[id];
            if (emoItemData == null)
            {
                continue;
            }
            GameObject item = emoItemData.item;
            EmoIconData data = emoItemData.data;
            bool isVisible = (data.emoType == (int)type);
            item.SetActive(isVisible);

            if (!isEditCollectList)
            {
                UpdateItemTagState(item, data.collected);
                UpdateItemSelectState(item, false);
            }
        }
    }

    /**
    * 显示收藏列表
    */

    private void ShowCollectedEmoList()
    {
        var selectCollectCount = CurCollectedEmoList.Count;
        var collectedCount = CollectedEmoList.Count;

        for (int i = 0; i < selectCollectCount; i++)
        {
            var id = CurCollectedEmoList[selectCollectCount - 1 - i];
            var data = EmoBtnDir[id].data;
            if (CollectedEmoList.Count > 0 && i < CollectedEmoList.Count && CollectedEmoList[i] != null)
            {
                UpdateCollectedEmoItem(i, data);
            }
            else
            {
                InitCollectedEmoItem(data);
            }
        }

        if (collectedCount > selectCollectCount)
        {
            for (int i = collectedCount - 1; i >= selectCollectCount; i--)
            {
                CollectedEmoList[i].item.SetActive(false);
            }
        }
    }

    /**
    * 更新收藏列表的 Item 显示
    */
    private void UpdateCollectedEmoItem(int index, EmoIconData emoData)
    {
        var itemData = CollectedEmoList[index];
        GameObject item = itemData.item;
        item.SetActive(true);
        Text emoText = item.GetComponentInChildren<Text>();
        UpdateItemTagState(item, emoData.collected);
        UpdateItemSelectState(item, false);
        LocalizationConManager.Inst.SetLocalizedContent(emoText, emoData.spriteName);
        itemData.item = item;
        itemData.data = emoData;
        item.GetComponent<Button>().onClick.RemoveAllListeners();
        item.GetComponent<Button>().onClick.AddListener(() =>
        {
            ClickEmoteBtn(itemData.data.id);
        });
    }

    /**
    * 初始化收藏列表的 Item
    */
    private void InitCollectedEmoItem(EmoIconData emoData)
    {
        EmoItemData emoItemData = new EmoItemData();
        GameObject item = Instantiate(itemPrefab, collectedItemParent);
        Text emoText = item.GetComponentInChildren<Text>();
        item.SetActive(true);
        UpdateItemTagState(item, emoData.collected);
        UpdateItemSelectState(item, false);
        LocalizationConManager.Inst.SetLocalizedContent(emoText, emoData.spriteName);
        emoItemData.item = item;
        emoItemData.data = emoData;
        CollectedEmoList.Add(emoItemData);
        item.GetComponent<Button>().onClick.AddListener(() =>
        {
            ClickEmoteBtn(emoItemData.data.id);
        });
    }

    /**
    * 从收藏列表中删除 emo 按钮
    */
    private void RemoveCollectedEmoItem(int index)
    {
        var itemData = CollectedEmoList[index];
        CollectedEmoList.Remove(itemData);
        Destroy(itemData.item.gameObject);
    }

    private void OnBgBtnClick()
    {
        if (CollectScrollView.activeInHierarchy)
        {
            SetCollectEditListVisible(false);
            return;
        }
        HideMenuPanel();
    }

    private void OnCollectBtnClick()
    {
        RefreshEmoBtnState(BtnTypeEnum.BTN_COLLECT);
        ShowEmoList(EmoTypeEnum.COLLECT_EMO);
    }

    private void OnEmoBtnClick()
    {
        RefreshEmoBtnState(BtnTypeEnum.BTN_EMOJI);
        ShowEmoList(EmoTypeEnum.EMOJI);
    }

    private void OnSingleEmoBtnClick()
    {
        RefreshEmoBtnState(BtnTypeEnum.BTN_SINGLE_EMO);
        ShowEmoList(EmoTypeEnum.SINGLE_EMO);
    }

    private void OnDoubleEmoBtnClick()
    {
        RefreshEmoBtnState(BtnTypeEnum.BTN_DOUBLE_EMO);
        ShowEmoList(EmoTypeEnum.DOUBLE_EMO);
    }

    private void OnFinishBtnClick()
    {
        SendCollectEmoIds();
        SetCollectEditListVisible(false);
    }

    private void OnStateEmoBtnClick()
    {
        RefreshEmoBtnState(BtnTypeEnum.BTN_STATE_EMO);
        ShowEmoList(EmoTypeEnum.STATE_EMO);
    }

    /**
    * 发送收藏表情列表
    */
    private void SendCollectEmoIds()
    {
        CollectEmoInfo emoInfo = new CollectEmoInfo();
        SelectedIds.Clear();
        foreach (var itemData in CollectEditEmoList)
        {
            SelectedIds.Add(itemData.data.id);
        }
        CurSentIds = new List<int>(SelectedIds);
        emoInfo.emojiId = CurSentIds.ToArray();
        LoggerUtils.Log("SendCollectEmoIds emojiId is" + JsonUtility.ToJson(emoInfo));
        HttpUtils.MakeHttpRequest("/ugcmap/setFavoriteEmoji", (int)HTTP_METHOD.POST, JsonUtility.ToJson(emoInfo), CollectEmoSuccess, CollectEmoFail);
    }

    public void CollectEmoSuccess(string msg)
    {
        LoggerUtils.Log("EmoMenuPanel CollectEmoSuccess. msg is  " + msg);
        CurCollectedEmoList = new List<int>(CurSentIds);
        UpdateEmoData(CurCollectedEmoList, true);
        RefreshCollectEditList();
    }

    public void CollectEmoFail(string msg)
    {
        LoggerUtils.Log("SendCollectEmoIds Fail. msg is " + msg);
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }

    /**
    * 获取所有收藏的表情列表
    */
    private void GetAllCollectedEmoIds()
    {
        HttpUtils.MakeHttpRequest("/ugcmap/favoriteEmojiList", (int)HTTP_METHOD.GET, "", GetEmoIdsSuccess, GetEmoIdsFail);
    }

    private void GetEmoIdsSuccess(string msg)
    {
        LoggerUtils.Log("EmoMenuPanel GetEmoIdsSuccess. msg is  " + msg);
        HttpResponDataStruct responseData = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        CollectEmoInfo emoInfo = JsonConvert.DeserializeObject<CollectEmoInfo>(responseData.data);
        List<int> collectedIds = new List<int>(emoInfo.emojiId);
        CurCollectedEmoList = collectedIds;
        UpdateEmoData(collectedIds, true);
        UpdateCollectedEditList(collectedIds, true, false);
    }

    private void GetEmoIdsFail(string msg)
    {
        LoggerUtils.Log("GetEmoIdsFail Fail. msg is " + msg);
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }

    private void HideMenuPanel()
    {
        Hide();
        PlayModePanel.Instance.EmoMenuPanelBecameVisible(false);
        PlayModePanel.Instance.SetChatEmoVisible(true);
    }

    /**
    * 设置收藏编辑列表是否可见
    */
    private void SetCollectEditListVisible(bool visible)
    {
        isEditCollectList = visible;
        CollectBtn.transform.parent.gameObject.SetActive(!visible);
        CollectScrollView.SetActive(visible);
        PlayModePanel.Instance.SetChatEmoVisible(!visible);
        if (visible)
        {
            OnEmoBtnClick();
        }
        else
        {
            OnCollectBtnClick();
        }
    }

    /**
    * 刷新分类按钮显示状态
    */
    private void RefreshEmoBtnState(BtnTypeEnum btnType)
    {
        for (int i = 0; i < btnList.Count; i++)
        {
            var select = btnList[i].transform.parent.GetComponent<Image>();
            var alpha = 0;
            if ((int)btnType == i)
            {
                alpha = 1;
            }
            select.color = new Color(1, 1, 1, alpha);
        }
    }

    /**
    * 更新表情 Data 数据和列表显示
    */
    private void UpdateEmoData(List<int> ids, bool collected)
    {
        // 更新之前先将所有表情的收藏态重置
        foreach (var emoData in emoIconDataList)
        {

            emoData.collected = false;
        }

        foreach (var id in ids)
        {
            foreach (var emoData in emoIconDataList)
            {
                if (emoData.id == id)
                {
                    emoData.collected = collected;
                }
            }
        }
        // 更新显示列表
        ShowEmoList(_curEmoTypeEnum);
    }

    /**
    * 动态刷新显示收藏编辑列表
    */
    private void RefreshCollectEditList()
    {
        var selectCollectCount = CurCollectedEmoList.Count;
        var collectedCount = CollectEditEmoList.Count;

        for (int i = 0; i < selectCollectCount; i++)
        {
            var id = CurCollectedEmoList[i];
            var data = EmoBtnDir[id];
            if (CollectEditEmoList.Count > 0 && i < CollectEditEmoList.Count && CollectEditEmoList[i] != null)
            {
                UpdateCollectEidtEmoItem(i, data.data);
            }
            else
            {
                AddCollectEmoItem(data, false);
            }
        }

        if (collectedCount > selectCollectCount)
        {
            for (int i = collectedCount - 1; i >= selectCollectCount; i--)
            {
                var itemData = CollectEditEmoList[i];
                CollectEditEmoList.Remove(itemData);
                Destroy(itemData.item);
            }
        }
    }

    /**
    * 更新收藏编辑列表 Item
    */

    private void UpdateCollectEidtEmoItem(int index, EmoIconData emoData)
    {
        var itemData = CollectEditEmoList[index];
        GameObject item = itemData.item;
        item.SetActive(true);
        Text emoText = item.GetComponentInChildren<Text>();
        UpdateItemTagState(item, emoData.collected);
        UpdateItemSelectState(item, false);
        LocalizationConManager.Inst.SetLocalizedContent(emoText, emoData.spriteName);
        itemData.item = item;
        itemData.data = emoData;
        var btn = item.GetComponent<Button>();
        var delBtn = item.transform.Find("DelBtn").GetComponent<Button>();
        delBtn.gameObject.SetActive(false);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            bool delBtnVisible = delBtn.gameObject.activeSelf;
            delBtn.gameObject.SetActive(!delBtnVisible);
            RefreshCollectBtnState(itemData.data.id);
        });
        delBtn.onClick.AddListener(() =>
        {
            DeleteCollectEmoItem(itemData.data.id);
        });
    }

    /**
    * 编辑状态下更新收藏编辑列表
    */
    private void UpdateCollectedEditList(List<int> ids, bool isCollected, bool isSelected)
    {
        for (int i = 0; i < ids.Count; i++)
        {
            int id = ids[i];
            var emoData = EmoBtnDir[id];
            if (isCollected)
            {
                AddCollectEmoItem(emoData, isSelected);
            }
            else
            {
                DeleteCollectEmoItem(id);
            }
        }
    }

    /**
    * 添加 emo 按钮到收藏编辑列表中
    */
    private void AddCollectEmoItem(EmoItemData emoData, bool isSelected)
    {
        EmoItemData itemData = new EmoItemData();
        GameObject item = Instantiate(itemPrefab, collectItemParent);
        Text emoText = item.GetComponentInChildren<Text>();
        itemData.item = item;
        itemData.data = emoData.data;
        LocalizationConManager.Inst.SetLocalizedContent(emoText, itemData.data.spriteName);
        UpdateItemTagState(item, true);
        UpdateItemSelectState(item, isSelected);
        item.gameObject.SetActive(true);
        CollectEditEmoList.Add(itemData);
        var btn = item.GetComponent<Button>();
        var delBtn = item.transform.Find("DelBtn").GetComponent<Button>();
        delBtn.gameObject.SetActive(false);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            bool delBtnVisible = delBtn.gameObject.activeSelf;
            delBtn.gameObject.SetActive(!delBtnVisible);
            RefreshCollectBtnState(itemData.data.id);
        });
        delBtn.onClick.AddListener(() =>
        {
            DeleteCollectEmoItem(itemData.data.id);
        });
    }

    /**
    * 从收藏编辑列表中删除 emo 按钮
    */
    private void DeleteCollectEmoItem(int id)
    {
        foreach (var itemData in CollectEditEmoList)
        {
            if (itemData.data.id == id)
            {
                UpdateEmoList(itemData.data.id, false);
                CollectEditEmoList.Remove(itemData);
                Destroy(itemData.item.gameObject);
                break;
            }
        }
    }

    /**
    * 显示收藏列表，并将列表中的 emo 按钮状态重置
    */
    private void ShowCollectedEditList()
    {
        foreach (var emoData in CollectEditEmoList)
        {
            var item = emoData.item;
            UpdateItemTagState(item, true);
            UpdateItemSelectState(item, false);
            var delBtn = item.transform.Find("DelBtn").GetComponent<Button>();
            delBtn.gameObject.SetActive(false);
        }
    }

    /**
    * 刷新收藏编辑模式下，按钮选中显示状态
    */
    private void RefreshCollectBtnState(int selectId)
    {
        foreach (var collectItem in CollectEditEmoList)
        {
            var item = collectItem.item;
            item.transform.SetParent(collectItemParent, false);
            var delBtn = item.transform.Find("DelBtn").GetComponent<Button>();
            if (collectItem.data.id != selectId)
            {
                delBtn.gameObject.SetActive(false);
            }
        }
    }

    /**
    * 根据表情 id 更新表情列表中的某一个表情的显示状态
    */
    private void UpdateEmoList(int id, bool collected)
    {
        var emoData = EmoBtnDir[id];
        var itemId = emoData.data.id;
        var item = emoData.item;
        if (!collected && isEditCollectList)
        {
            UpdateItemSelectState(item, false);
        }
        UpdateItemTagState(item, collected);
    }

    /**
    * 更新表情 Item 的收藏态
    */
    private void UpdateItemTagState(GameObject item, bool collected)
    {
        Transform isSpecialTrans = item.transform.Find("IsSpecial");
        isSpecialTrans.gameObject.SetActive(collected);
    }

    /**
    * 在收藏编辑状态下更新表情 Item 的选中态
    */
    private void UpdateItemSelectState(GameObject item, bool select)
    {
        Transform isSelectedTrans = item.transform.Find("isSelected");
        isSelectedTrans.gameObject.SetActive(select);
    }

    /// <summary>
    /// 获取指定表情的收藏状态
    /// 0:收藏 1:不收藏
    /// </summary>
    /// <param name="emoteId"> 表情 Id</param>
    /// <returns></returns>
    public int GetEmoteIsCollected(int emoteId)
    {
        int collectionStatus = 1;
        if (CurCollectedEmoList.Contains(emoteId))
        {
            collectionStatus = 0;
        }
        return collectionStatus;
    }
    public bool IsCanNotPlay()
    {
        return StateManager.IsOnLadder||StateManager.IsOnSlide|| StateManager.IsOnSeesaw || StateManager.IsOnSwing;
    }
    public void ClickEmoteBtn(int emoId)
    {
        if (isStateEmoRequesting)
        {
            LoggerUtils.Log("isStateEmoRequesting is true, can not click other emos");
            return;
        }
        if (IsCanNotPlay())
        {
            return;
        }
        bool showNormalEmo = true;

        if(emoId == (int)EmoName.EMO_SWORD)
        {
            SwordManager.Inst.EnterSwordMode(emoId);
            showNormalEmo = false;
        }
        else
        {
            SwordManager.Inst.forceInterrupt();
        }

        // 带货表情
        if (emoId == (int)EmoName.EMO_PROMOTE)
        {
            // 选择带货商品
            showNormalEmo = false;
            PromoteManager.Inst.Select();
        }
        else
        {
            // 选货或带货中
            var promoteCtrl = ClientManager.Inst.GetPlayerPromoteController(GameManager.Inst.ugcUserInfo.uid);
            if (promoteCtrl != null && (promoteCtrl.InSelect || promoteCtrl.InPromote))
            {
                showNormalEmo = false;
                PromoteManager.Inst.End();
            }
        }

        if (showNormalEmo)
        {
            // 播放表情
            var emoIconData = MoveClipInfo.GetAnimName(emoId);
            if (emoIconData.GetEmoType() == EmoType.StateEmo)
            {
                PlayerControlManager.Inst.CallStateEmo(emoId);
            }
            else
            {
                PlayerControlManager.Inst.PlayMove(emoId);
            }
        }

        HideMenuPanel();
        ReportEmoteData(emoId);
    }
    public void ReportEmoteData(int emoId)
    {
        var emoData = MoveClipInfo.GetAnimName(emoId);
        var collectStatus = GetEmoteIsCollected(emoId);
        //发送 Emote 埋点上报
        DataLogUtils.LogEmoteClickEvent(emoData, collectStatus);
    }

    public void StartClickStateEmoCor()
    {
        StopClickStateEmoCor();
        isStateEmoRequesting = true;
        clickStateEmoCor = CoroutineManager.Inst.StartCoroutine(OnStartClickStateEmoCor());
    }

    private IEnumerator OnStartClickStateEmoCor()
    {
        yield return new WaitForSeconds(1f);
        isStateEmoRequesting = false;
    }

    public void StopClickStateEmoCor()
    {
        if (clickStateEmoCor != null)
        {
            CoroutineManager.Inst.StopCoroutine(clickStateEmoCor);
            clickStateEmoCor = null;
        }
        isStateEmoRequesting = false;
    }

    public bool GetIsStateEmoRequesting()
    {
        return isStateEmoRequesting;
    }
}


using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class RoleAdjustView : MonoBehaviour
{
    public Button ResetBtn;
    public string color;
    public Button ReturnBtn;
    public GameObject AdjustItem;
    public Transform AdjustParent;
    public Transform scrollRect;
    public GameObject tabView;
    private GameObject origStyleView;
    private bool isCanShowTab = true;
    [HideInInspector]
    public float screenRatio;

    private Text ResetText, BackText;
    private bool isSpecialHandle = false;
    private float defaultBtnWidth;
    private const float textWidthOffset = 40;

    AdjustItemFactory itemFactory;
    public Dictionary<EAdjustItemType, AdjustItem> mAdjustItems;
    private Action<int> mOnClickResetCallBack;
    private Action<bool> mShowOrHide;
    public int mCurrentId;
    public RoleController mRoleController;
    protected virtual void Start()
    {
        ResetBtn.onClick.AddListener(OnResetClick);
        ReturnBtn.onClick.AddListener(OnReturnClick);
    }
    public void CreateItems(List<AdjustItemContext> itemContexts, Action<EAdjustItemType, float> ItemChanged)
    {
        for (int i = 0; i < itemContexts.Count; i++)
        {
            var go = Instantiate(AdjustItem, AdjustParent);
            AdjustItemContext itemContext = itemContexts[i];
            AdjustItem item = itemFactory.Craete(itemContext, go);
            item.mItemValueChanged = ItemChanged;
            mAdjustItems.Add(item.mItemType, item);
        }
    }
    public void Init(List<AdjustItemContext> itemContexts, Action<EAdjustItemType, float> ItemChanged,Action<int> resetCallBack,Action<bool> showOrHide=null)
    {
        itemFactory = new AdjustItemFactory();
        mAdjustItems = new Dictionary<EAdjustItemType, AdjustItem>();
        ResetText = ResetBtn.GetComponentInChildren<Text>(true);
        BackText = ReturnBtn.GetComponentInChildren<Text>(true);
        defaultBtnWidth = ResetBtn.GetComponent<RectTransform>().sizeDelta.x;

        if (itemContexts.Count >= 4)
        {
            SetAdjustScale(700f);
        }
        CreateItems(itemContexts, ItemChanged);
        var parentView = transform.parent.GetComponent<BaseView>();
        if ((parentView.classifyType == ClassifyType.headwear
            || parentView.classifyType == ClassifyType.glasses
            || parentView.classifyType == ClassifyType.outfits
            || parentView.classifyType == ClassifyType.shoes
            || parentView.classifyType == ClassifyType.hand
            || parentView.classifyType == ClassifyType.patterns
            || parentView.classifyType == ClassifyType.bag
            || parentView.classifyType == ClassifyType.eyes
            || parentView.classifyType == ClassifyType.effects
            )
         && (ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
        {
            isCanShowTab = false;
        }

        mOnClickResetCallBack = resetCallBack;
        mShowOrHide = showOrHide;
    }

    private void OnEnable()
    {
        if (!isSpecialHandle)
        {
            HandleSpecialText(ResetText);
            HandleSpecialText(BackText);
            isSpecialHandle = true;
        }
    }

    public virtual void Show(GameObject origView)
    {
        gameObject.SetActive(true);
        if (tabView)
        {
            tabView.SetActive(false);
        }
        if (origView)
        {
            origStyleView = origView;
            origView.SetActive(false);
        }
        mShowOrHide?.Invoke(true);
    }

    public virtual void SetAdjustScale(float height)
    {
        screenRatio = (float)Screen.safeArea.height / (float)Screen.safeArea.width;//屏幕宽高比
        if (screenRatio < 1.8f && scrollRect)
        {
            scrollRect.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1125, height);
        }
    }
    public void SetDefalutValue(string color = null)
    {
        this.color = color;
    }
    public void SetSliderValueWithoutNotify(EAdjustItemType itemType, float normal)
    {
        AdjustItem item = null;
        if (mAdjustItems.TryGetValue(itemType, out item))
        {
            item.SetValueWithoutNotify(normal);
        }
    }
    public void SetSliderValue(EAdjustItemType itemType,float normal)
    {
        AdjustItem item = null;
        if (mAdjustItems.TryGetValue(itemType,out item))
        {
            item.SetValue(normal);
        }
    }
    public virtual void OnResetClick()
    {
        mOnClickResetCallBack?.Invoke(mCurrentId);
    }

    public void SetCurrentId(int id)
    {
        mCurrentId = id;
    }
    public virtual void OnReturnClick()
    {
        if (origStyleView)
        {
            origStyleView.SetActive(true);
        }
        if (isCanShowTab && tabView)
        {
            tabView.SetActive(true);
        }
        gameObject.SetActive(false);
        mShowOrHide?.Invoke(false);
    }

    public void HandleSpecialText(Text contentText)
    {
        var tempStr = GameUtils.SubStringByBytes(contentText.text, 24, Encoding.Unicode);
        contentText.text = tempStr;
        if (contentText.preferredWidth > defaultBtnWidth - textWidthOffset)
        {
            var contentBtn = contentText.transform.parent;
            var RectTrans = contentBtn.GetComponent<RectTransform>();
            var BtnSize = RectTrans.sizeDelta;
            var sizeWidth = contentText.preferredWidth + textWidthOffset;
            RectTrans.sizeDelta = new Vector2(sizeWidth, BtnSize.y);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SuperScrollView
{
   
    [System.Serializable]
    public class GridViewItemPrefabConfData
    {
        public GameObject mItemPrefab = null;
        public int mInitCreateCount = 0;
    }
    
    public enum ScrollDirection
    {
        DOWN = 0,
        UP = 1
    }

    public class LoopGridViewInitParam
    {
        public static LoopGridViewInitParam CopyDefaultInitParam()
        {
            return new LoopGridViewInitParam();
        }
    }


    public class LoopGridViewSettingParam
    {
        public object mItemSize = null;
        public object mPadding = null;
        public object mItemPadding = null;
        public object mGridFixedType = null;
        public object mFixedRowOrColumnCount = null;
    }


    public class LoopGridView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        class ItemRangeData
        {
            public int mMaxRow;
            public int mMinRow;
            public int mMaxColumn;
            public int mMinColumn;
            public Vector2 mCheckedPosition;
        }
        [SerializeField]
        public GameObject ItemPrefab;
        RectTransform mContainerTrans;
        ScrollRect mScrollRect = null;
        RectTransform mViewPortRectTransform = null;
        private int mItemTotalCount = 0;
        [SerializeField] 
        private bool mAdjustCount = true;
        [SerializeField]
        private int mFixedRowOrColumnCount = 0;
        [SerializeField]
        RectOffset mPadding = new RectOffset();
        [SerializeField]
        Vector2 mItemPadding = Vector2.zero;
        [SerializeField]
        Vector2 mItemSize = Vector2.zero;
        [SerializeField]
        Vector2 mItemRecycleDistance = new Vector2(50,50);
        Vector2 mItemSizeWithPadding = Vector2.zero;
        Vector2 mStartPadding;
        Vector2 mEndPadding;
        System.Func<LoopGridView,int,int,int,ScrollDirection, LoopGridViewItem> mOnGetItemByRowColumn;
        List<GridItemGroup> mItemGroupObjPool = new List<GridItemGroup>();

        //if GridFixedType is GridFixedType.ColumnCountFixed, then the GridItemGroup is one row of the GridView
        //so mItemGroupList is current all shown rows or columns
        List<GridItemGroup> mItemGroupList = new List<GridItemGroup>();
        // Scroll Direction
        private ScrollDirection sDirection = ScrollDirection.DOWN;
        private bool mIsDraging = false;
        private int mRowCount = 0;
        private int mColumnCount = 0;
        private float offset = 20;
        public int mPreRowCount = -1;
        public Action OnPreLoadEvent;
        public Action OnOverBottomEvent;
        private GridItemPool gridPool;
        public System.Action<PointerEventData> mOnBeginDragAction = null;
        public System.Action<PointerEventData> mOnDragingAction = null;
        public System.Action<PointerEventData> mOnEndDragAction = null;
        float mSmoothDumpVel = 0;
        [SerializeField]
        GridFixedType mGridFixedType = GridFixedType.ColumnCountFixed;
        //in this callback, use CurSnapNearestItemRowColumn to get cur snaped item row column.
        private int mLeftSnapUpdateExtraCount = 1;
     
        private bool mListViewInited = false;
        private int mListUpdateCheckFrameCount = 0;
        private ItemRangeData mCurFrameItemRangeData = new ItemRangeData();
        private int mNeedCheckContentPosLeftCount = 1;
        private ClickEventListener mScrollBarClickEventListener1 = null;
        private ClickEventListener mScrollBarClickEventListener2 = null;

        private RowColumnPair mCurSnapNearestItemRowColumn;
        
        public int ItemTotalCount
        {
            get
            {
                return mItemTotalCount;
            }
        }

        public RectTransform ContainerTrans
        {
            get
            {
                return mContainerTrans;
            }
        }

        public float ViewPortWidth
        {
            get { return mViewPortRectTransform.rect.width; }
        }

        public float ViewPortHeight
        {
            get { return mViewPortRectTransform.rect.height; }
        }

        public ScrollRect ScrollRect
        {
            get
            {
                return mScrollRect;
            }
        }

        public bool IsDraging
        {
            get
            {
                return mIsDraging;
            }
        }
        
        public Vector2 ItemSize
        {
            get
            {
                return mItemSize;
            }
            set
            {
                SetItemSize(value);
            }
        }

        public Vector2 ItemPadding
        {
            get
            {
                return mItemPadding;
            }
            set
            {
                SetItemPadding(value);
            }
        }

        public Vector2 ItemSizeWithPadding
        {
            get
            {
                return mItemSizeWithPadding;
            }
        }
        public RectOffset Padding
        {
            get
            {
                return mPadding;
            }
            set
            {
                SetPadding(value);
            }
        }

        

        /*
        LoopGridView method is to initiate the LoopGridView component. There are 4 parameters:
        itemTotalCount: the total item count in the GridView, this parameter must be set a value >=0 , then the ItemIndex can be from 0 to itemTotalCount -1.
        onGetItemByRowColumn: when a item is getting in the ScrollRect viewport, and this Action will be called with the item' index and the row and column index as the parameters, to let you create the item and update its content.
        settingParam: You can use this parameter to override the values in the Inspector Setting
        */
        public void InitGridView(int itemTotalCount, 
            System.Func<LoopGridView,int,int,int,ScrollDirection, LoopGridViewItem> onGetItemByRowColumn, 
            LoopGridViewSettingParam settingParam = null,
            LoopGridViewInitParam initParam = null)
        {
            if (mListViewInited == true)
            {
                Debug.LogError("LoopGridView.InitListView method can be called only once.");
                return;
            }

           
            mListViewInited = true;
            if (itemTotalCount < 0)
            {
                Debug.LogError("itemTotalCount is  < 0");
                itemTotalCount = 0;
            }
            if(settingParam != null)
            {
                UpdateFromSettingParam(settingParam);
            }

            mScrollRect = gameObject.GetComponent<ScrollRect>();
            if (mScrollRect == null)
            {
                Debug.LogError("ListView Init Failed! ScrollRect component not found!");
                return;
            }
            mContainerTrans = mScrollRect.content;
            if (mAdjustCount)
            {
                mFixedRowOrColumnCount = (int)(mContainerTrans.rect.width/mItemSize.x);
                mPadding.left = (int)(mContainerTrans.rect.width - mFixedRowOrColumnCount * mItemSize.x) / 2;
            }
            mViewPortRectTransform = mScrollRect.viewport;
            InitItemPool();
            mOnGetItemByRowColumn = onGetItemByRowColumn;
            mNeedCheckContentPosLeftCount = 4;
            mItemTotalCount = itemTotalCount;
            UpdateAllGridSetting();
            OnUpdate();
            ScrollRect.onValueChanged.AddListener(x =>
            {
                OnUpdate();
            });
        }


        /*
        This method may use to set the item total count of the GridView at runtime. 
        this parameter must be set a value >=0 , and the ItemIndex can be from 0 to itemCount -1.  
        If resetPos is set false, then the ScrollRect’s content position will not changed after this method finished.
        */
        public void SetListItemCount(int itemCount, bool resetPos = true)
        {
            if(itemCount < 0)
            {
                return;
            }
            if(itemCount == mItemTotalCount)
            {
                return;
            }
            mItemTotalCount = itemCount;
            UpdateColumnRowCount();
            UpdateContentSize();
            ForceToCheckContentPos();
            if (mItemTotalCount == 0)
            {
                RecycleAllItem();
                ClearAllTmpRecycledItem();
                return;
            }
            VaildAndSetContainerPos();
            UpdateGridViewContent();
            ClearAllTmpRecycledItem();
            if (resetPos)
            {
                MovePanelToItemByRowColumn(0,0);
            }
        }
        
        public void RefreshGridView(int itemCount, bool resetPos = true)
        {
            mItemTotalCount = itemCount;
            UpdateColumnRowCount();
            UpdateContentSize();
            RecycleAllItem();
            ClearAllTmpRecycledItem();
            VaildAndSetContainerPos();
            UpdateGridViewContent();
            if (resetPos)
            {
                MovePanelToItemByRowColumn(0,0);
            }
        }

       //fetch or create a new item form the item pool.
        public LoopGridViewItem GetItemByPool()
        {
            LoopGridViewItem item = gridPool.GetItem();
            RectTransform rf = item.GetComponent<RectTransform>();
            rf.SetParent(mContainerTrans);
            rf.localScale = Vector3.one;
            rf.anchoredPosition3D = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            item.ParentGridView = this;
            return item;
        }


        /*
        To update a item by itemIndex.if the itemIndex-th item is not visible, then this method will do nothing.
        Otherwise this method will call RefreshItemByRowColumn to do real work.
        */
        public void RefreshItemByItemIndex(int itemIndex)
        {
            if(itemIndex < 0 || itemIndex >= ItemTotalCount)
            {
                return;
            }
            int count = mItemGroupList.Count;
            if (count == 0)
            {
                return;
            }
            RowColumnPair val = GetRowColumnByItemIndex(itemIndex);
            RefreshItemByRowColumn(val.mRow, val.mColumn);
        }


        /*
        To update a item by (row,column).if the item is not visible, then this method will do nothing.
        Otherwise this method will call mOnGetItemByRowColumn(row,column) to get a new updated item. 
        */
        public void RefreshItemByRowColumn(int row,int column)
        {
            int count = mItemGroupList.Count;
            if (count == 0)
            {
                return;
            }
            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                GridItemGroup group = GetShownGroup(row);
                if (group == null)
                {
                    return;
                }
                LoopGridViewItem curItem = group.GetItemByColumn(column);
                if(curItem == null)
                {
                    return;
                }
                LoopGridViewItem newItem = GetNewItemByRowColumn(row, column);
                if (newItem == null)
                {
                    return;
                }
                Vector3 pos = curItem.CachedRectTransform.anchoredPosition3D;
                group.ReplaceItem(curItem, newItem);
                RecycleItemTmp(curItem);
                newItem.CachedRectTransform.anchoredPosition3D = pos;
                ClearAllTmpRecycledItem();
            }
            else
            {
                GridItemGroup group = GetShownGroup(column);
                if (group == null)
                {
                    return;
                }
                LoopGridViewItem curItem = group.GetItemByRow(row);
                if (curItem == null)
                {
                    return;
                }
                LoopGridViewItem newItem = GetNewItemByRowColumn(row, column);
                if (newItem == null)
                {
                    return;
                }
                Vector3 pos = curItem.CachedRectTransform.anchoredPosition3D;
                group.ReplaceItem(curItem, newItem);
                RecycleItemTmp(curItem);
                newItem.CachedRectTransform.anchoredPosition3D = pos;
                ClearAllTmpRecycledItem();
            }
        }
        

        //force to update the mCurSnapNearestItemRowColumn value
        public void ForceSnapUpdateCheck()
        {
            if (mLeftSnapUpdateExtraCount <= 0)
            {
                mLeftSnapUpdateExtraCount = 1;
            }
        }

        //force to refresh the mCurFrameItemRangeData that what items should be shown in viewport.
        public void ForceToCheckContentPos()
        {
            if (mNeedCheckContentPosLeftCount <= 0)
            {
                mNeedCheckContentPosLeftCount = 1;
            }
        }

        /*
        This method will move the panel's position to ( the position of itemIndex'th item + offset ).
        */
        public void MovePanelToItemByIndex(int itemIndex, float offsetX = 0, float offsetY = 0)
        {
            if(ItemTotalCount == 0)
            {
                return;
            }
            if(itemIndex >= ItemTotalCount)
            {
                itemIndex = ItemTotalCount - 1;
            }
            if (itemIndex < 0)
            {
                itemIndex = 0;
            }
            RowColumnPair val = GetRowColumnByItemIndex(itemIndex);
            MovePanelToItemByRowColumn(val.mRow, val.mColumn, offsetX, offsetY);
        }

        /*
        This method will move the panel's position to ( the position of (row,column) item + offset ).
        */
        public void MovePanelToItemByRowColumn(int row,int column, float offsetX = 0,float offsetY = 0)
        {
            mScrollRect.StopMovement();
            if (mItemTotalCount == 0)
            {
                return;
            }
            Vector2 itemPos = GetItemPos(row, column);
            Vector3 pos = mContainerTrans.anchoredPosition3D;
            if (mScrollRect.horizontal)
            {
                float maxCanMoveX = Mathf.Max(ContainerTrans.rect.width - ViewPortWidth, 0);
                if(maxCanMoveX > 0)
                {
                    float x = -itemPos.x + offsetX;
                    x = Mathf.Min(Mathf.Abs(x), maxCanMoveX) * Mathf.Sign(x);
                    pos.x = x;
                } 
            }
            if(mScrollRect.vertical)
            {
                float maxCanMoveY = Mathf.Max(ContainerTrans.rect.height - ViewPortHeight, 0);
                if(maxCanMoveY > 0)
                {
                    float y = -itemPos.y + offsetY;
                    y = Mathf.Min(Mathf.Abs(y), maxCanMoveY) * Mathf.Sign(y);
                    pos.y = y;
                }
            }
            if(pos != mContainerTrans.anchoredPosition3D)
            {
                mContainerTrans.anchoredPosition3D = pos;
            }
            VaildAndSetContainerPos();
            ForceToCheckContentPos();
        }
        
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            mIsDraging = true;
            if (mOnBeginDragAction != null)
            {
                mOnBeginDragAction(eventData);
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            mIsDraging = false;
            ForceSnapUpdateCheck();
            if (mOnEndDragAction != null)
            {
                mOnEndDragAction(eventData);
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            if (mOnDragingAction != null)
            {
                mOnDragingAction(eventData);
            }
        }


        public int GetItemIndexByRowColumn(int row, int column)
        {
            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                return row * mFixedRowOrColumnCount + column;
            }
            else
            {
                return column * mFixedRowOrColumnCount + row;
            }
        }


        public RowColumnPair GetRowColumnByItemIndex(int itemIndex)
        {
            if(itemIndex < 0)
            {
                itemIndex = 0;
            }
            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                int row = itemIndex / mFixedRowOrColumnCount;
                int column = itemIndex % mFixedRowOrColumnCount;
                return new RowColumnPair(row, column);
            }
            else
            {
                int column = itemIndex / mFixedRowOrColumnCount;
                int row = itemIndex % mFixedRowOrColumnCount;
                return new RowColumnPair(row, column);
            }
        }


        public Vector2 GetItemAbsPos(int row, int column)
        {
            float x = mStartPadding.x + column * mItemSizeWithPadding.x;
            float y = mStartPadding.y + row * mItemSizeWithPadding.y;
            return new Vector2(x, y);
        }


        public Vector2 GetItemPos(int row, int column)
        {
            Vector2 absPos = GetItemAbsPos(row, column);
            float x = absPos.x;
            float y = absPos.y;
            return new Vector2(x, -y);
        }

        //get the shown item of itemIndex, if this item is not shown,then return null.
        public LoopGridViewItem GetShownItemByItemIndex(int itemIndex)
        {
            if(itemIndex < 0 || itemIndex >= ItemTotalCount)
            {
                return null;
            }
            if(mItemGroupList.Count == 0)
            {
                return null;
            }
            RowColumnPair val = GetRowColumnByItemIndex(itemIndex);
            return GetShownItemByRowColumn(val.mRow, val.mColumn);
        }

        //get the shown item of (row, column), if this item is not shown,then return null.
        public LoopGridViewItem GetShownItemByRowColumn(int row, int column)
        {
            if (mItemGroupList.Count == 0)
            {
                return null;
            }
            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                GridItemGroup group = GetShownGroup(row);
                if (group == null)
                {
                    return null;
                }
                return group.GetItemByColumn(column);
            }
            else
            {
                GridItemGroup group = GetShownGroup(column);
                if (group == null)
                {
                    return null;
                }
                return group.GetItemByRow(row);
            }
        }

        public void UpdateAllGridSetting()
        {
            UpdateStartEndPadding();
            UpdateItemSize();
            UpdateColumnRowCount();
            UpdateContentSize();
            ForceSnapUpdateCheck();
            ForceToCheckContentPos();
        }

        //set mGridFixedType and mFixedRowOrColumnCount at runtime
        public void SetGridFixedGroupCount(GridFixedType fixedType,int count)
        {
            if(mGridFixedType == fixedType && mFixedRowOrColumnCount == count)
            {
                return;
            }
            mGridFixedType = fixedType;
            mFixedRowOrColumnCount = count;
            UpdateColumnRowCount();
            UpdateContentSize();
            if (mItemGroupList.Count == 0)
            {
                return;
            }
            RecycleAllItem();
            ForceSnapUpdateCheck();
            ForceToCheckContentPos();
        }
        //change item size at runtime
        public void SetItemSize(Vector2 newSize)
        {
            if (newSize == mItemSize)
            {
                return;
            }
            mItemSize = newSize;
            UpdateItemSize();
            UpdateContentSize();
            if (mItemGroupList.Count == 0)
            {
                return;
            }
            RecycleAllItem();
            ForceSnapUpdateCheck();
            ForceToCheckContentPos();
        }
        //change item padding at runtime
        public void SetItemPadding(Vector2 newPadding)
        {
            if (newPadding == mItemPadding)
            {
                return;
            }
            mItemPadding = newPadding;
            UpdateItemSize();
            UpdateContentSize();
            if (mItemGroupList.Count == 0)
            {
                return;
            }
            RecycleAllItem();
            ForceSnapUpdateCheck();
            ForceToCheckContentPos();
        }
        //change padding at runtime
        public void SetPadding(RectOffset newPadding)
        {
            if (newPadding == mPadding)
            {
                return;
            }
            mPadding = newPadding;
            UpdateStartEndPadding();
            UpdateContentSize();
            if (mItemGroupList.Count == 0)
            {
                return;
            }
            RecycleAllItem();
            ForceSnapUpdateCheck();
            ForceToCheckContentPos();
        }


        public void UpdateContentSize()
        {
            float width = mStartPadding.x + mColumnCount * mItemSizeWithPadding.x - mItemPadding.x + mEndPadding.x;
            float height = mStartPadding.y + mRowCount * mItemSizeWithPadding.y - mItemPadding.y + mEndPadding.y;
            if (mContainerTrans.rect.height != height)
            {
                mContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }
            if (mContainerTrans.rect.width != width)
            {
                mContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
        }


        public void VaildAndSetContainerPos()
        {
            Vector3 pos = mContainerTrans.anchoredPosition3D;
            mContainerTrans.anchoredPosition3D = GetContainerVaildPos(pos.x, pos.y);
        }

        public void ClearAllTmpRecycledItem()
        {
            gridPool.ClearTmpRecycledItem();
        }


        public void RecycleAllItem()
        {
            foreach (GridItemGroup group in mItemGroupList)
            {
                RecycleItemGroupTmp(group);
            }
            mItemGroupList.Clear();
        }

        public void UpdateGridViewContent()
        {
            mListUpdateCheckFrameCount++;
            if (mItemTotalCount == 0)
            {
                if (mItemGroupList.Count > 0)
                {
                    RecycleAllItem();
                }
                return;
            }
            UpdateCurFrameItemRangeData();
            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                int groupCount = mItemGroupList.Count;
                int minRow = mCurFrameItemRangeData.mMinRow;
                int maxRow = mCurFrameItemRangeData.mMaxRow;
                if (mItemGroupList.Count > 0)
                {
                    if (mItemGroupList[0].GroupIndex < minRow)
                    {
                        sDirection = ScrollDirection.DOWN;
                    }
                    else if(mItemGroupList[mItemGroupList.Count - 1].GroupIndex > maxRow)
                    {
                        sDirection = ScrollDirection.UP;
                    }
                }

                for (int i = groupCount - 1; i >= 0; --i)
                {
                    GridItemGroup group = mItemGroupList[i];
                    if (group.GroupIndex < minRow || group.GroupIndex > maxRow)
                    {
                        RecycleItemGroupTmp(group);
                        mItemGroupList.RemoveAt(i);
                    }
                }
                if (mItemGroupList.Count == 0)
                {
                    GridItemGroup group = CreateItemGroup(minRow);
                    mItemGroupList.Add(group);
                }
                while (mItemGroupList[0].GroupIndex > minRow)
                {
                    GridItemGroup group = CreateItemGroup(mItemGroupList[0].GroupIndex - 1);
                    mItemGroupList.Insert(0, group);
                }
                while (mItemGroupList[mItemGroupList.Count - 1].GroupIndex < maxRow)
                {
                    GridItemGroup group = CreateItemGroup(mItemGroupList[mItemGroupList.Count - 1].GroupIndex + 1);
                    mItemGroupList.Add(group);
                }
                int count = mItemGroupList.Count;
                for (int i = 0; i < count; ++i)
                {
                    UpdateRowItemGroupForRecycleAndNew(mItemGroupList[i],sDirection);
                }
                
                if (mItemGroupList[mItemGroupList.Count - 1].GroupIndex > mRowCount - 3 &&mPreRowCount != mRowCount)
                {
                    mPreRowCount = mRowCount;
                    OnPreLoadEvent?.Invoke();
                }
                
            }
            // else
            // {
            //     int groupCount = mItemGroupList.Count;
            //     int minColumn = mCurFrameItemRangeData.mMinColumn;
            //     int maxColumn = mCurFrameItemRangeData.mMaxColumn;
            //     for (int i = groupCount - 1; i >= 0; --i)
            //     {
            //         GridItemGroup group = mItemGroupList[i];
            //         if (group.GroupIndex < minColumn || group.GroupIndex > maxColumn)
            //         {
            //             RecycleItemGroupTmp(group);
            //             mItemGroupList.RemoveAt(i);
            //         }
            //     }
            //     if (mItemGroupList.Count == 0)
            //     {
            //         GridItemGroup group = CreateItemGroup(minColumn);
            //         mItemGroupList.Add(group);
            //     }
            //     while (mItemGroupList[0].GroupIndex > minColumn)
            //     {
            //         GridItemGroup group = CreateItemGroup(mItemGroupList[0].GroupIndex - 1);
            //         mItemGroupList.Insert(0, group);
            //     }
            //     while (mItemGroupList[mItemGroupList.Count - 1].GroupIndex < maxColumn)
            //     {
            //         GridItemGroup group = CreateItemGroup(mItemGroupList[mItemGroupList.Count - 1].GroupIndex + 1);
            //         mItemGroupList.Add(group);
            //     }
            //     int count = mItemGroupList.Count;
            //     for (int i = 0; i < count; ++i)
            //     {
            //         UpdateColumnItemGroupForRecycleAndNew(mItemGroupList[i]);
            //     }
            //
            //
            //
            // }
        }

        public void UpdateStartEndPadding()
        {
            mStartPadding.x = mPadding.left;
            mStartPadding.y = mPadding.top;
            mEndPadding.x = mPadding.right;
            mEndPadding.y = mPadding.bottom;
        }


        public void UpdateItemSize()
        {
            if (mItemSize.x > 0f && mItemSize.y > 0f)
            {
                mItemSizeWithPadding = mItemSize + mItemPadding;
                return;
            }
            do
            {
                if (ItemPrefab == null)
                {
                    break;
                }
                RectTransform rtf = ItemPrefab.GetComponent<RectTransform>();
                if (rtf == null)
                {
                    break;
                }
                mItemSize = rtf.rect.size;
                mItemSizeWithPadding = mItemSize + mItemPadding;

            } while (false);

            if (mItemSize.x <= 0 || mItemSize.y <= 0)
            {
                Debug.LogError("Error, ItemSize is invaild.");
            }

        }

        public void UpdateColumnRowCount()
        {
            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                mColumnCount = mFixedRowOrColumnCount;
                mRowCount = mItemTotalCount / mColumnCount;
                if (mItemTotalCount % mColumnCount > 0)
                {
                    mRowCount++;
                }
                if (mItemTotalCount <= mColumnCount)
                {
                    mColumnCount = mItemTotalCount;
                }
            }
            else
            {
                mRowCount = mFixedRowOrColumnCount;
                mColumnCount = mItemTotalCount / mRowCount;
                if (mItemTotalCount % mRowCount > 0)
                {
                    mColumnCount++;
                }
                if (mItemTotalCount <= mRowCount)
                {
                    mRowCount = mItemTotalCount;
                }
            }
        }




        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>


        bool IsContainerTransCanMove()
        {
            if (mItemTotalCount == 0)
            {
                return false;
            }
            if (mScrollRect.horizontal && ContainerTrans.rect.width > ViewPortWidth)
            {
                return true;
            }
            if (mScrollRect.vertical && ContainerTrans.rect.height > ViewPortHeight)
            {
                return true;
            }
            return false;
        }



        void RecycleItemGroupTmp(GridItemGroup group)
        {
            if (group == null)
            {
                return;
            }
            while(group.First != null)
            {
                LoopGridViewItem item = group.RemoveFirst();
                RecycleItemTmp(item);
            }
            group.Clear();
            RecycleOneItemGroupObj(group);
        }



        void RecycleItemTmp(LoopGridViewItem item)
        {
            gridPool.RecycleItem(item);
        }
        
        void InitItemPool()
        {
            gridPool = new GridItemPool();
            gridPool.Init(ItemPrefab, mContainerTrans);
        }


        LoopGridViewItem GetNewItemByRowColumn(int row,int column,ScrollDirection dir = ScrollDirection.DOWN)
        {
            int itemIndex = GetItemIndexByRowColumn(row, column);
            if(itemIndex < 0 || itemIndex >= ItemTotalCount)
            {
                return null;
            }
            LoopGridViewItem newItem = mOnGetItemByRowColumn(this,itemIndex,row,column,dir);
            if (newItem == null)
            {
                return null;
            }
            newItem.NextItem = null;
            newItem.PrevItem = null;
            newItem.Row = row;
            newItem.Column = column;
            newItem.ItemIndex = itemIndex;
            newItem.ItemCreatedCheckFrameCount = mListUpdateCheckFrameCount;
            return newItem;
        }


        RowColumnPair GetCeilItemRowColumnAtGivenAbsPos(float ax,float ay)
        {
            ax = Mathf.Abs(ax);
            ay = Mathf.Abs(ay);
            int row = Mathf.CeilToInt((ay - mStartPadding.y) / mItemSizeWithPadding.y)-1;
            int column = Mathf.CeilToInt((ax - mStartPadding.x) / mItemSizeWithPadding.x)-1;
            if(row < 0)
            {
                row = 0;
            }
            if(row >= mRowCount)
            {
                row = mRowCount - 1;
            }
            if(column < 0)
            {
                column = 0;
            }
            if(column >= mColumnCount)
            {
                column = mColumnCount - 1;
            }
            return new RowColumnPair(row,column);
        }

        void OnUpdate()
        {
            if(mListViewInited == false)
            {
                return;
            }
            OverMoveDistance();
            UpdateGridViewContent();
            ClearAllTmpRecycledItem();
        }
        

        GridItemGroup CreateItemGroup(int groupIndex)
        {
            GridItemGroup ret = GetOneItemGroupObj();
            ret.GroupIndex = groupIndex;
            return ret;
        }

        private void OverMoveDistance()
        {
            if (mGridFixedType == GridFixedType.ColumnCountFixed && ContainerTrans.rect.height > ViewPortHeight &&
                ContainerTrans.anchoredPosition3D.y > ContainerTrans.rect.height - ViewPortHeight + offset)
            {
                OnOverBottomEvent?.Invoke();
            }
            else if (mGridFixedType == GridFixedType.RowCountFixed && ContainerTrans.rect.height > ViewPortHeight &&
                     (ContainerTrans.anchoredPosition3D.x > ContainerTrans.rect.width - ViewPortWidth + offset))
            {
                OnOverBottomEvent?.Invoke();
            }
        }

        Vector2 GetContainerMovedDistance()
        {
            Vector2 pos = GetContainerVaildPos(ContainerTrans.anchoredPosition3D.x, ContainerTrans.anchoredPosition3D.y);
            return new Vector2(Mathf.Abs(pos.x), Mathf.Abs(pos.y));
        }


        Vector2 GetContainerVaildPos(float curX, float curY)
        {
            float maxCanMoveX = Mathf.Max(ContainerTrans.rect.width - ViewPortWidth, 0);
            float maxCanMoveY = Mathf.Max(ContainerTrans.rect.height - ViewPortHeight, 0);
            curX = Mathf.Clamp(curX, -maxCanMoveX, 0);
            curY = Mathf.Clamp(curY, 0, maxCanMoveY);
            return new Vector2(curX, curY);
        }


        void UpdateCurFrameItemRangeData()
        {
            Vector2 distVector2 = GetContainerMovedDistance();
            if (mNeedCheckContentPosLeftCount <= 0 && mCurFrameItemRangeData.mCheckedPosition == distVector2)
            {
               return;
            }
            if (mNeedCheckContentPosLeftCount > 0)
            {
                mNeedCheckContentPosLeftCount--;
            }
            float distX = distVector2.x - mItemRecycleDistance.x;
            float distY = distVector2.y - mItemRecycleDistance.y;
            if(distX < 0)
            {
                distX = 0;
            }
            if(distY < 0)
            {
                distY = 0;
            }
            RowColumnPair val = GetCeilItemRowColumnAtGivenAbsPos(distX, distY);
            mCurFrameItemRangeData.mMinColumn = val.mColumn;
            mCurFrameItemRangeData.mMinRow = val.mRow;
            distX = distVector2.x + mItemRecycleDistance.x + ViewPortWidth;
            distY = distVector2.y + mItemRecycleDistance.y + ViewPortHeight;
            val = GetCeilItemRowColumnAtGivenAbsPos(distX, distY);
            mCurFrameItemRangeData.mMaxColumn = val.mColumn;
            mCurFrameItemRangeData.mMaxRow = val.mRow;
            mCurFrameItemRangeData.mCheckedPosition = distVector2;
        }

       
        
        void UpdateRowItemGroupForRecycleAndNew(GridItemGroup group,ScrollDirection dir = ScrollDirection.DOWN)
        {
            int minColumn = mCurFrameItemRangeData.mMinColumn;
            int maxColumn = mCurFrameItemRangeData.mMaxColumn;
            int row = group.GroupIndex;
            while(group.First != null && group.First.Column < minColumn)
            {
                RecycleItemTmp(group.RemoveFirst());
            }
            while (group.Last != null && ( ( group.Last.Column > maxColumn ) || ( group.Last.ItemIndex >= ItemTotalCount ) ) )
            {
                RecycleItemTmp(group.RemoveLast());
            }
            if(group.First == null)
            {
                LoopGridViewItem item = GetNewItemByRowColumn(row, minColumn,dir);
                if(item == null)
                {
                    return;
                }
                item.CachedRectTransform.anchoredPosition3D = GetItemPos(item.Row, item.Column);
                group.AddFirst(item);
            }
            while (group.First.Column > minColumn)
            {
                LoopGridViewItem item = GetNewItemByRowColumn(row, group.First.Column-1,dir);
                if (item == null)
                {
                    break;
                }
                item.CachedRectTransform.anchoredPosition3D = GetItemPos(item.Row, item.Column);

                group.AddFirst(item);
            }
            while (group.Last.Column < maxColumn)
            {
                LoopGridViewItem item = GetNewItemByRowColumn(row, group.Last.Column + 1,dir);
                if (item == null)
                {
                    break;
                }
                item.CachedRectTransform.anchoredPosition3D = GetItemPos(item.Row, item.Column);

                group.AddLast(item);
            }
        }
        void UpdateFromSettingParam(LoopGridViewSettingParam param)
        {
            if (param == null)
            {
                return;
            }
            if (param.mItemSize != null)
            {
                mItemSize = (Vector2)(param.mItemSize);
            }
            if (param.mItemPadding != null)
            {
                mItemPadding = (Vector2)(param.mItemPadding);
            }
            if (param.mPadding != null)
            {
                mPadding = (RectOffset)(param.mPadding);
            }
            if (param.mGridFixedType != null)
            {
                mGridFixedType = (GridFixedType)(param.mGridFixedType);
            }
            if (param.mFixedRowOrColumnCount != null)
            {
                mFixedRowOrColumnCount = (int)(param.mFixedRowOrColumnCount);
            }
        }

        
        GridItemGroup GetShownGroup(int groupIndex)
        {
            if(groupIndex < 0)
            {
                return null;
            }
            int count = mItemGroupList.Count;
            if (count == 0)
            {
                return null;
            }
            if (groupIndex < mItemGroupList[0].GroupIndex || groupIndex > mItemGroupList[count - 1].GroupIndex)
            {
                return null;
            }
            int i = groupIndex - mItemGroupList[0].GroupIndex;
            return mItemGroupList[i];
        }
        
        
        GridItemGroup GetOneItemGroupObj()
        {
            int count = mItemGroupObjPool.Count;
            if (count == 0)
            {
                return new GridItemGroup();
            }
            GridItemGroup ret = mItemGroupObjPool[count - 1];
            mItemGroupObjPool.RemoveAt(count - 1);
            return ret;
        }
        void RecycleOneItemGroupObj(GridItemGroup obj)
        {
            mItemGroupObjPool.Add(obj);
        }


    }

}

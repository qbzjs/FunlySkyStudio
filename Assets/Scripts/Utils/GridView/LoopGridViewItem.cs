using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperScrollView
{

    public class LoopGridViewItem : MonoBehaviour
    {
        // indicates the item’s index in the list the mItemIndex can only be from 0 to itemTotalCount -1.
        private int mItemIndex = -1;
        // the row index, the item is in. starting from 0.
        private int mRow = -1;
        // the column index, the item is in. starting from 0.
        private int mColumn = -1;
        //indicates the item’s id. 
        //This property is set when the item is created or fetched from pool, 
        //and will no longer change until the item is recycled back to pool.
        private int mItemId = -1;
        private LoopGridView mParentGridView = null;
        private bool mIsInitHandlerCalled = false;
        private string mItemPrefabName;
        private RectTransform mCachedRectTransform;
        private int mItemCreatedCheckFrameCount = 0;

        private object mUserObjectData = null;
        private int mUserIntData1 = 0;
        private int mUserIntData2 = 0;
        private string mUserStringData1 = null;
        private string mUserStringData2 = null;

        private LoopGridViewItem mPrevItem;
        private LoopGridViewItem mNextItem;

        public int ItemCreatedCheckFrameCount
        {
            get { return mItemCreatedCheckFrameCount; }
            set { mItemCreatedCheckFrameCount = value; }
        }


        public RectTransform CachedRectTransform
        {
            get
            {
                if (mCachedRectTransform == null)
                {
                    mCachedRectTransform = gameObject.GetComponent<RectTransform>();
                }
                return mCachedRectTransform;
            }
        }

        public string ItemPrefabName
        {
            get
            {
                return mItemPrefabName;
            }
            set
            {
                mItemPrefabName = value;
            }
        }

        public int Row
        {
            get
            {
                return mRow;
            }
            set
            {
                mRow = value;
            }
        }
        public int Column
        {
            get
            {
                return mColumn;
            }
            set
            {
                mColumn = value;
            }
        }

        public int ItemIndex
        {
            get
            {
                return mItemIndex;
            }
            set
            {
                mItemIndex = value;
            }
        }
        public int ItemId
        {
            get
            {
                return mItemId;
            }
            set
            {
                mItemId = value;
            }
        }
        

        public LoopGridView ParentGridView
        {
            get
            {
                return mParentGridView;
            }
            set
            {
                mParentGridView = value;
            }
        }

        public LoopGridViewItem PrevItem
        {
            get { return mPrevItem; }
            set { mPrevItem = value; }
        }
        public LoopGridViewItem NextItem
        {
            get { return mNextItem; }
            set { mNextItem = value; }
        }

    }
}

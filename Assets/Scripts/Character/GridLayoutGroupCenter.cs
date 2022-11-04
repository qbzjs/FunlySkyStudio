using UnityEngine;
using UnityEngine.UI;

public class GridLayoutGroupCenter : MonoBehaviour
{
    // Start is called before the first frame update
    public GridLayoutGroup mGridLayoutGroup;
    private RectTransform mRect;
    public float mWidth = 0;
    public float mCellx = 0;
    public int mCount = 0;
    public int mNewStart = 0;
    protected RectTransform rectTransform
    {
        get
        {
            if (mRect == null)
            {
                mRect = GetComponent<RectTransform>();
            }
            return mRect;
        }
    }
    void Start()
    {
        mGridLayoutGroup = GetComponent<GridLayoutGroup>();
        if (mGridLayoutGroup!=null)
        {
            mCellx = mGridLayoutGroup.cellSize.x;
            mWidth = rectTransform.rect.size.x;
            mCount = 0;
            float space = mGridLayoutGroup.spacing.x;
            float cellx = space + mCellx;
            mCount = (int)(mWidth / cellx);
            float newStart = (mWidth - ((mCount - 1) * space + mCellx * mCount)) / 2;
            mNewStart = (int)newStart;
            mGridLayoutGroup.padding.left = mNewStart;
        }
    }
}

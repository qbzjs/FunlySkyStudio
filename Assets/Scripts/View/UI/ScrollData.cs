using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewData
{
    public float cellSize;
    public float normalSpacingX;
    public int normalPaddingL;
    public int normalPaddingT;
    public float expandSpacingX;
    public float expandSpacingY;
    public int expandPaddingL;
    public int expandPaddingT;
    public int expandPaddingB;
}

public class ScrollData : MonoBehaviour
{
    [HideInInspector]
    public GridLayoutGroup normalGrid;
    [HideInInspector]
    public GridLayoutGroup expandGrid;

    private void Awake()
    {
        GridLayoutGroup[] grids = GetComponentsInChildren<GridLayoutGroup>();
        normalGrid = grids[0];
        expandGrid = grids[1];
    }

    public void UpdateScrollSetting(ScrollViewData data)
    {
        normalGrid.cellSize = new Vector2(data.cellSize, data.cellSize);
        normalGrid.padding.left = data.normalPaddingL;
        normalGrid.padding.top = data.normalPaddingT;
        normalGrid.spacing = new Vector2(data.normalSpacingX, 0);
        expandGrid.cellSize = new Vector2(data.cellSize, data.cellSize);
        expandGrid.padding.left = data.expandPaddingL;
        expandGrid.padding.top = data.expandPaddingT;
        expandGrid.padding.bottom = data.expandPaddingB;
        expandGrid.spacing = new Vector2(data.expandSpacingX, data.expandSpacingY);
    }

    [HideInInspector]
    public ScrollViewData matScrollData = new ScrollViewData
    {
        cellSize = 100f,
        normalSpacingX = 30f,
        normalPaddingL = 30,
        normalPaddingT = 20,
        expandSpacingX = 20f,
        expandSpacingY = 15f,
        expandPaddingL = 30,
        expandPaddingT = 20,
        expandPaddingB = 20,
    };
    [HideInInspector]
    public ScrollViewData colorScrollData = new ScrollViewData
    {
        cellSize = 50f,
        normalSpacingX = 40f,
        normalPaddingL = 40,
        normalPaddingT = 45,
        expandSpacingX = 45f,
        expandSpacingY = 40f,
        expandPaddingL = 40,
        expandPaddingT = 40,
        expandPaddingB = 20,
    };
}

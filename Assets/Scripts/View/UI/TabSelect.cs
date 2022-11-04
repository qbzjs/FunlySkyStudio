using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void OnClickCurTab();

public class TabSelect : MonoBehaviour
{
    public GameObject normalPanel;
    public GameObject expandPanel = null;
    private TabSelectGroup group;
    private Transform unselect;
    private Text text;
    private Image image = null;
    private RectTransform exBtnRect = null;
    private Button button;
    private Button expandBtn = null;

    private Color selectColor = Color.white;
    private Color unselectColor = new Color(1f, 1f, 1f, 0.5f);

    private void Awake()
    {
        unselect = transform.Find("UnSelect");
        text = GetComponentInChildren<Text>();
        button = transform.Find("Button").GetComponent<Button>();
        var exBtn = transform.Find("Expand");
        if (exBtn != null && exBtn.gameObject.activeSelf)
        {
            expandBtn = exBtn.GetComponent<Button>();
            exBtnRect = exBtn.GetComponent<RectTransform>();
            image = exBtn.GetComponent<Image>();
        }
    }

    public void AddClickListener(OnClickCurTab onClickCurTab)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            OnClickTab();
            onClickCurTab();
        });

        if (expandBtn != null)
        {
            expandBtn.onClick.RemoveAllListeners();
            expandBtn.onClick.AddListener(() =>
            {
                OnClickTab();
                onClickCurTab();
            });
        }
    }
    private void OnClickTab()
    {
        if (group == null) return;
        group.SetExpand(this);
        group.SelectCurTab(this);
    }

    public void SetSelectTab(bool isSelect)
    {
        unselect.gameObject.SetActive(!isSelect);
        text.color = isSelect ? selectColor : unselectColor;
        if (image != null)
        {
            image.color = isSelect ? selectColor : unselectColor;
        }
    }
    public void ResetTabPanel()
    {
        if (expandPanel != null)
        {
            expandPanel.SetActive(false);
        }
        normalPanel.SetActive(false);
    }
    public void SetSelectPanel(bool isExpand)
    {
        if (expandPanel != null)
        {
            expandPanel.SetActive(isExpand);
            normalPanel.SetActive(!isExpand);
        }
        else
        {
            normalPanel.SetActive(true);
        }
    }
    public void RotateExBtn(bool isExpand)
    {
        Quaternion unexpand = Quaternion.Euler(0, 0, 180f);
        Quaternion expand = Quaternion.Euler(0, 0, 0);
        if (expandBtn != null)
        {
            exBtnRect.rotation = isExpand ? expand : unexpand;
        }
    }
    public void SetSelect(bool isExpand)
    {
        SetSelectTab(true);
        SetSelectPanel(isExpand);
        RotateExBtn(isExpand);
    }
    public void ResetSelect()
    {
        SetSelectTab(false);
        ResetTabPanel();
        RotateExBtn(false);
    }
    public void SetTabGroup(TabSelectGroup tabGroup)
    {
        group = tabGroup;
    }
}

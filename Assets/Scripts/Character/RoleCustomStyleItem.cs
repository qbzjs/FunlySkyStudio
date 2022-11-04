using System;
using GameData;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
public class RoleCustomStyleItem : RoleStyleItem
{
    public Button CustomBtn;
    private Action customClick;
    private bool CanAdjust = true;
    protected override void Start()
    {
        base.Start();
        CustomBtn.onClick.AddListener(OnCustomClick);
    }

    public override void Init(RoleIconData data, Action<RoleStyleItem> select, SpriteAtlas spriteAtlas)
    {
        base.Init(data, select,spriteAtlas);
        if (string.IsNullOrEmpty(data.texName))
        {
            CanAdjust = false;
            CustomBtn.gameObject.SetActive(false);
        }
    }

    public void SetCustomView(Action act)
    {
        customClick = act;
    }

    private void OnCustomClick()
    {
        customClick?.Invoke();
    }

    public override void SetSelectState(bool isVisible)
    {
        base.SetSelectState(isVisible);
        CustomBtn.gameObject.SetActive(isVisible && CanAdjust);
        SetCollectTagVisible();
    }

    public override void OnLongPressItem()
    {
        base.OnLongPressItem();

        // SetCollectTagVisible();
    }

    public override void SetCollectTagVisible()
    {
        if (CustomBtn.gameObject.activeSelf)
        {
            collectTag.SetActive(false);
        }
        else
        {
            collectTag.SetActive(isCollected);
        }
    }
}
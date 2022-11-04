using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonColorToggleItem : MonoBehaviour
{
    public Button ColorBtn;

    public Image ColorBGImage;

    public GameObject ColorCheckImage;

    public int colorIndex = 0;
    public Color color;

    private Action<int> OnSelectColor;
    private Action<CommonColorToggleItem> OnUgcSelectColor;

    private void Start()
    {
        ColorBtn.onClick.AddListener(OnToggleClick);
    }

    public void SetColor(int index, Color color,Action<int> onSelect)
    {
        colorIndex = index;
        this.color=color;
        ColorBGImage.color = color;
        OnSelectColor = onSelect;
    }
    public void SetColor(Color color,Action<CommonColorToggleItem> onSelect)
    {
        this.color=color;
        ColorBGImage.color = color;      
        OnUgcSelectColor = onSelect;
    }

    public void OnToggleClick()
    {
        ColorCheckImage.SetActive(true);
        OnSelectColor?.Invoke(colorIndex);
        OnUgcSelectColor?.Invoke(this);
    }
}

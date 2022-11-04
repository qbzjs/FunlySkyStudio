using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:shenchao
/// Description:刷新ui高度
/// Date: 2022/8/5 18:12:30
/// </summary>
public class HeightRefresh : MonoBehaviour
{
    private List<RectTransform> _childRect = new List<RectTransform>();

    private RectTransform _rectTransform;
    // Start is called before the first frame update
    void Start()
    {
        foreach (RectTransform rt in transform)
        {
            _childRect.Add(rt);
        }

        _rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        float f = 0;
        foreach (var r in _childRect)
        {
            f += r.rect.height;
        }

        _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, f);
    }
}

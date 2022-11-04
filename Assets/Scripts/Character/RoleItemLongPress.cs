using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Author:WenJia
/// Description: Item 长按功能 
/// Date: 2022/4/24 13:53:43
/// </summary>

public class RoleItemLongPress : Button, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private bool isDown; //是否按下
    private float longPressTime = 0.5f; //检测长按的时长
    private float lastDownTime; //最近按下的那一刻时间
    public Action OnLongPress; // 长按处理事件
    public bool isCanLongPress = true; // 是否可以长按
    private bool longPressTriggered = false; // 是否触发长按
    public Vector3 curPos; // 当前鼠标位置
    public Vector3 prevPos; // 上一次鼠标位置
    private float borderDis = 0.5f;
    public ScrollRect scrollRect;
    private bool isDrag = false;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!isDrag && !longPressTriggered)
        {
            onClick?.Invoke();
            isDown = false;
            isDrag = false;
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;
        prevPos = transform.parent.TransformPoint(Input.mousePosition);
        curPos = transform.parent.TransformPoint(Input.mousePosition);
        lastDownTime = Time.time;
        isDown = true;
        longPressTriggered = false;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        prevPos = Vector3.zero;
        curPos = Vector3.zero;
        isDown = false;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        isDown = false;
    }

    private void Update()
    {
        if (!interactable || !isCanLongPress) return;
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
        {
            return;
        }

        if (isDown && !longPressTriggered)
        {
            float duration = Time.time - lastDownTime;
            if (duration >= longPressTime)
            {
                longPressTriggered = true;
                OnLongPress?.Invoke();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        scrollRect.OnBeginDrag(eventData);
        isDrag = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        curPos = transform.parent.TransformPoint(eventData.position);
        var distance = Vector3.Distance(prevPos, curPos);
        if (distance > 0.5f)
        {
            isDown = false;
            isDrag = true;
        }
        scrollRect.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        scrollRect.OnEndDrag(eventData);
        isDrag = false;
    }
}

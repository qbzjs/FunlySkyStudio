using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ItemBtnType
{
    None,
    BaggageItem,
}

public class GameItemLongPress : Button, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private bool isDown; //是否按下
    private float longPressTime = 0.5f; //检测长按的时长
    private float lastDownTime; //最近按下的那一刻时间
    public Action OnLongPress; // 长按处理事件
    public Action playMaskAnim;//需要播放的遮罩动画
    public Action closeMaskAnim;//需要关闭的遮罩动画
    public bool isCanLongPress = true; // 是否可以长按
    public ItemBtnType btnType = ItemBtnType.None;
    private bool longPressTriggered = false; // 是否触发长按
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
        lastDownTime = Time.time;
        isDown = true;
        longPressTriggered = false;
        playMaskAnim?.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
        closeMaskAnim?.Invoke();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        isDown = false;
        closeMaskAnim?.Invoke();
    }

    private void Update()
    {
        if (!interactable || !isCanLongPress) return;

        switch (btnType)
        {
            case ItemBtnType.None:
                break;
            case ItemBtnType.BaggageItem:
                if (SceneParser.Inst != null && SceneParser.Inst.GetBaggageSet() == 1)
                {
                    LongPress();
                }
                break;
        }
    }

    public void LongPress()
    {
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
}

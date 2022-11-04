using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UGCClothesInputReceiver : BMonoBehaviour<UGCClothesInputReceiver>
{
    public static bool locked { get; set; }
    private bool singleTouchLocked = false;
    private int firstTouchId;
    private int lastTouchCount;
    private float firstTouchTime;
    private InputHandler handler;
    private readonly float shortTouchThreshold = 0.2f;

    protected override void Awake()
    {
        base.Awake();
        locked = false;
        handler = null;
    }

    public void SetHandle(InputHandler iHandler)
    {
        handler = iHandler;
    }

    void Update()
    {
        if (locked)
            return;

        if (handler == null)
            return;

        if (lastTouchCount < Input.touchCount)
        {
            HandleNewTouch();
        }

        if (Input.touchCount == 1)
        {
            HandleSingleTouch();
        }

        if (Input.touchCount > 1)
        {
            HandleMultipleTouches();
        }
        HandleAllTouches();
        lastTouchCount = Input.touchCount;
    }

    void HandleNewTouch()
    {
        Touch last = GetNewTouch();
        singleTouchLocked = false;
        if (Input.touchCount == 1)
        {
            firstTouchId = last.fingerId;
            firstTouchTime = Time.timeSinceLevelLoad;
        }

        handler.OnTouchBegin(last);
    }

    void HandleSingleTouch()
    {
        if (singleTouchLocked)
            return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Moved)
        {
            handler.OnMovementTouchStay(touch);
        }

        if (touch.fingerId == firstTouchId)
        {
            if (touch.phase == TouchPhase.Ended)
            {
                if (Time.timeSinceLevelLoad - firstTouchTime < shortTouchThreshold)
                {
                    handler.OnShortTouchEnd(touch);
                }
                else
                {
                    handler.OnLongTouchEnd(touch);
                }
            }
        }
    }

    void HandleMultipleTouches()
    {
        if (lastTouchCount <= 1)
        {
            handler.OnMultipleTouchesBegin(Input.touches);
        }
        else
        {
            handler.OnMultipleTouchesStay(Input.touches);
        }
    }

    void HandleAllTouches()
    {
        for (int i = 0; i < Input.touchCount; ++i)
        {
            handler.OnTouchStay(Input.GetTouch(i));
        }
    }

    Touch GetNewTouch()
    {
        for (int i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                return Input.GetTouch(i);
            }
        }
        return default;
    }

}

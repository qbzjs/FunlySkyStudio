using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum InteractorType
{
    none,
    move,
    rote,
    scale,
}
public class Interactor : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    /// <summary>
    /// 需要操作的组件
    /// </summary>
    public GameObject targetGameObject;

    public RectTransform targetRecTrans;

    BoxCollider targetCollider;


    /// <summary>
    /// 自己的recttransfrom
    /// </summary>
    public RectTransform recTrans;
    public BoxCollider clickArea;
    public InteractorType type;

    public bool isDrag = false;

    private Vector2 lastMousePosition;

    public Button copyBtn;

    public Button desBtn;

    Vector2 lastScaleTouchPos;

    ElementUndoData beginData;
    ElementUndoData endData;



    private float restrictX = 600;
    private float restrictY = 500;

    public void Init()
    {
        copyBtn.onClick.AddListener(OnCopy);
        desBtn.onClick.AddListener(OnDesTarget);
    }
    #region 事件注册
    public Action SetUndoRedoAct;
    private Action OnTransformChange;
    public Action OnCopyAct;
    public Action OnDistroyAct;
    public Action OnSelectAct;
    public Action OnEndDragAct;
    public Action OnTransUndoAct;
    public Action<ElementUndoData> CreatUndoDataAct;
    #endregion

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="target"></param>
    public void Settup(RectTransform rectTrans, Action onChange,Action init)
    {
        OnTransformChange = onChange;
        targetGameObject = rectTrans.gameObject;
        targetRecTrans = rectTrans;
        targetCollider = targetGameObject.GetComponent<BoxCollider>();
        targetCollider.size = rectTrans.sizeDelta;
        clickArea.size = rectTrans.sizeDelta;
        recTrans = transform.GetComponent<RectTransform>();
        transform.position = targetGameObject.transform.position;
        recTrans.sizeDelta = rectTrans.sizeDelta;
        transform.rotation = targetGameObject.transform.rotation;
        init?.Invoke();
        gameObject.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastMousePosition = eventData.pointerCurrentRaycast.worldPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(recTrans, eventData.position, eventData.pressEventCamera, out lastScaleTouchPos);
        beginData = CreateUndoData(UGCElementType.Trans, targetRecTrans, MainUGCResPanel.Inst.curSelectPart);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDrag)
        {
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000,
                1 << LayerMask.NameToLayer("Anchors")))
            {
                var obj = hit.collider.gameObject;
                if (obj.name == "MFbtn")
                {
                    type = InteractorType.scale;
                    isDrag = true;
                }
                else
                {
                    type = InteractorType.move;
                    isDrag = true;
                }
            }
        }


        switch (type)
        {
            case InteractorType.none:
                break;
            case InteractorType.move:
                MoveSelectedObject(eventData.delta);
                break;
            case InteractorType.rote:
            case InteractorType.scale:
                RotateAround(eventData.pointerCurrentRaycast.worldPosition);
                ScaleSelectedObject(eventData.position, eventData.pressEventCamera);
                break;
        }
        OnTransformChange?.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        if (targetGameObject)
        {
            OnEndDragAct?.Invoke();
        }
        endData = CreateUndoData(UGCElementType.Trans, targetRecTrans, MainUGCResPanel.Inst.curSelectPart);
        AddRecord(beginData, endData);
        beginData = null;
        endData = null;
        SetUndoRedoAct?.Invoke();
    }

    public void MoveSelectedObject(Vector2 translateAmount)
    {
        RestrictedArea();
        targetRecTrans.anchoredPosition += translateAmount;
        recTrans.anchoredPosition += translateAmount;
    }

    private void RestrictedArea()
    {
        if(targetRecTrans.localPosition.x > restrictX || targetRecTrans.localPosition.x < -restrictX
            || targetRecTrans.localPosition.y > restrictY || targetRecTrans.localPosition.y < -restrictY)
        {
            Vector2 maxSize = targetRecTrans.localPosition;
            if (targetRecTrans.localPosition.x > restrictX)
            {
                maxSize = new Vector2(restrictX, targetRecTrans.localPosition.y);
                return;
            }
            else if (targetRecTrans.localPosition.x < -restrictX)
            {
                maxSize = new Vector2(-restrictX, targetRecTrans.localPosition.y);
            }
            else if (targetRecTrans.localPosition.y > restrictY)
            {
                maxSize = new Vector2(targetRecTrans.localPosition.x, restrictY);
            }
            else if (targetRecTrans.localPosition.y < -restrictY)
            {
                maxSize = new Vector2(targetRecTrans.localPosition.x, -restrictY);
            }
            targetRecTrans.localPosition = maxSize;
            recTrans.localPosition = maxSize;
        }
    }

    public void RotateAround(Vector2 newPos)
    {
        Vector3 localEluer = Vector3.zero;
        float angle = Vector2.SignedAngle((lastMousePosition - new Vector2(transform.position.x, transform.position.y)),
        newPos - new Vector2(transform.position.x, transform.position.y));
        localEluer.z += angle;
        targetRecTrans.eulerAngles += localEluer;
        recTrans.eulerAngles += localEluer;
        lastMousePosition = newPos;
    }

    public void ScaleSelectedObject(Vector2 newPos, Camera pressEventCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(recTrans, newPos, pressEventCamera, out Vector2 localTouchPos);
        Vector2 tempt = localTouchPos - lastScaleTouchPos;
        LimitMinSize(targetRecTrans);
        if (targetRecTrans.sizeDelta.x > targetRecTrans.sizeDelta.y)
        {
            var ratio = targetRecTrans.sizeDelta.x / targetRecTrans.sizeDelta.y;
            var newX = targetRecTrans.sizeDelta.x + tempt.x;
            targetRecTrans.sizeDelta = new Vector2(newX, newX / ratio);
        }
        else
        {
            var ratio = targetRecTrans.sizeDelta.y / targetRecTrans.sizeDelta.x;
            var newY = targetRecTrans.sizeDelta.y + tempt.x;
            targetRecTrans.sizeDelta = new Vector2(newY/ ratio, newY);
        }
        recTrans.sizeDelta = targetRecTrans.sizeDelta;
        targetCollider.size = targetRecTrans.sizeDelta;
        clickArea.size = targetRecTrans.sizeDelta;
        lastScaleTouchPos = localTouchPos;
    }

    private void LimitMinSize(RectTransform rectTrans)
    {
        if (targetRecTrans.sizeDelta.x < targetRecTrans.sizeDelta.y && rectTrans.sizeDelta.y < 1)
        {
            if(targetRecTrans.sizeDelta.x == 0)
            {
                return;
            }
            var ratio = targetRecTrans.sizeDelta.y / targetRecTrans.sizeDelta.x;
            rectTrans.sizeDelta = new Vector2(1/ ratio , 1);
        }
        if (targetRecTrans.sizeDelta.x > targetRecTrans.sizeDelta.y && rectTrans.sizeDelta.x < 1)
        {
            if (targetRecTrans.sizeDelta.y == 0)
            {
                return;
            }
            var ratio = targetRecTrans.sizeDelta.x / targetRecTrans.sizeDelta.y;
            rectTrans.sizeDelta = new Vector2(1, 1/ ratio);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDrag && targetGameObject != null) 
        {
            OnSelectAct?.Invoke();
        }
    }

    public void OnCopy()
    {
        if (targetGameObject != null)
        {
            OnCopyAct?.Invoke();
        }
    }

    public void Release()
    {
    }

    public void OnDesTarget()
    {
        if (targetGameObject != null)
        {
            OnDistroyAct?.Invoke();
        }
        ResetInfo();
        gameObject.SetActive(false);
    }

    public void RefreshTransfrom(RectTransform oriRectrans)
    {
        if (targetRecTrans == oriRectrans)
        {
            transform.position = targetRecTrans.position;
            recTrans.sizeDelta = targetRecTrans.sizeDelta;
            transform.rotation = targetRecTrans.rotation;
        }
    }

    public void ResetInfo()
    {
        isDrag = false;
        targetCollider = null;
        targetGameObject = null;
        targetRecTrans = null;
        gameObject.SetActive(false);
        MainUGCResPanel.Inst.colorPinkerPanel.SetElementColor = null;
        MainUGCResPanel.Inst.SetElementColor = null;
        ClearAction();
    }
    private void ClearAction()
    {
        OnCopyAct = null;
        OnDistroyAct = null;
        OnSelectAct = null;
        OnEndDragAct = null;
        OnTransUndoAct = null;
        CreatUndoDataAct = null;
    }
    #region Undo/Redo
    private ElementUndoData CreateUndoData(UGCElementType type, RectTransform rectTrans,GameObject selectPart)
    {
        ElementUndoData data = new ElementUndoData();
        data.targetNode = rectTrans;
        data.postion = rectTrans.localPosition;
        data.eulerAngles = rectTrans.localEulerAngles;
        data.sizeDelta = rectTrans.sizeDelta;
        data.transformType = (int)type;
        data.selectPart = selectPart;
        CreatUndoDataAct?.Invoke(data);
        return data;
    }

    public void AddRecord(ElementUndoData beginData, ElementUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.UGCClothElementUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        UndoRecordPool.Inst.PushRecord(record);
    }

    public void SetTransUndo(RectTransform targetTrans,Vector2 position,Vector3 rotate,Vector2 sizeDelta)
    {
        if (targetTrans == null) { return; }
        var behav = targetTrans.GetComponent<ElementBaseBehaviour>();
        targetTrans.localPosition = position;
        targetTrans.localEulerAngles = rotate;
        targetTrans.sizeDelta = sizeDelta;
        behav.OnTransformChange();
        OnTransUndoAct?.Invoke();
    }



    public void AddDestroyRecord(GameObject gameObject,int type)
    {
        UGCClothesCreateDestroyUndoData beginData = new UGCClothesCreateDestroyUndoData();
        beginData.targetNode = gameObject;
        beginData.createUndoMode = (int)CreateUndoMode.Destroy;
        beginData.type = type;
        beginData.selectPart = MainUGCResPanel.Inst.curSelectPart;
        UGCClothesCreateDestroyUndoData endData = new UGCClothesCreateDestroyUndoData();
        endData.targetNode = null;
        endData.createUndoMode = (int)CreateUndoMode.Destroy;
        endData.type = type;
        endData.selectPart = MainUGCResPanel.Inst.curSelectPart;
        UndoRecord record = new UndoRecord(UndoHelperName.UGCClothesCreateDestroyUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        UndoRecordPool.Inst.PushRecord(record);
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum BehaviourType
{
    Photo,
    Text,
    noneArea,
}
public class ElementBaseBehaviour : MonoBehaviour
{
    public int part;
    public BehaviourType type;
    public RectTransform rectTrans;
    public Button clickArea;
    public Image clickImage;
    public float scalingRatio = 1.5f;
    public float minSize = 370f;
    public int hierarchy;//层级记录
    public Vector3 copyOffset = new Vector3(18, -18, 0);
    //非业务开发逻辑，禁止调用
    private Interactor interactor;

    public virtual void SetSelfData(){ }

    public virtual void Select() { }

    public virtual void OnTransformChange(){}

    public virtual GameObject GetDynamicMir() { return null; }

    public virtual void OnCopy() {}

    public virtual void OnDis() 
    {
        SecondCachePool.Inst.DestroyEntity(this.gameObject);
        interactor.AddDestroyRecord(this.gameObject, (int)type);
    }

    public virtual void OnClick(GameObject obj) 
    {
        if (!IsCanSelect())
        {
            return;
        }
        interactor = TransformInteractorController.Inst.GetInterActor();
        interactor.ResetInfo();
        interactor.Settup(rectTrans, OnTransformChange, Init);
    }

    public virtual GameObject ExcuteMirObj() { return null; }

    public virtual void RedoInfo() { }

    public virtual void UndoInfo() { }

    public virtual void Init() 
    {
        interactor = TransformInteractorController.Inst.GetInterActor();
        interactor.OnCopyAct = OnCopy;
        interactor.OnDistroyAct = OnDis;
        interactor.OnSelectAct = Select;
        interactor.OnEndDragAct = EndDrag;
        interactor.OnTransUndoAct = TransUndo;
        interactor.CreatUndoDataAct = CreatUndoData;
        rectTrans.SetSiblingIndex(rectTrans.transform.parent.childCount - 1);
        var mir = GetDynamicMir();
        if (mir)
        {
            mir.transform.SetSiblingIndex(rectTrans.transform.parent.childCount - 1);
        }
    }

    public virtual void EndDrag()
    {
        SetSelfData();
    }

    public virtual void TransUndo()
    {
        OnTransformChange();
        interactor.Settup(rectTrans,OnTransformChange, Init);
    }

    public void AddClickEvent()
    {
        clickArea = gameObject.GetComponentInChildren<Button>();
        clickImage = clickArea.GetComponent<Image>();
        if (clickArea != null)
        {
            clickArea.onClick.AddListener(()=> { ElementButtonManager.Inst.OnClick(type, this); });
        }
    }

    public bool IsCanSelect()
    {
        bool isCan = false;
        switch (type)
        {
            case BehaviourType.Photo:
            case BehaviourType.Text:
                isCan = true;
                break;
            case BehaviourType.noneArea:
                isCan = false;
                break;
            default:
                break;
        }
        return isCan;
    }

    public virtual void SetMirSiblingIndex(){}

    public virtual void CreatUndoData(ElementUndoData data) { }
}

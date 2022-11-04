using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Input = UnityEngine.Input;


public class UGCClothesHandler : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerClickHandler
{
    public Action<GameObject> OnSelectHander;
    private Transform roleClothes;
    private float speed = 1;
    private void Start()
    {
        roleClothes = this.transform.GetChild(0);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        roleClothes.Rotate(Vector3.up, -speed * eventData.delta.x);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000,
            1 << LayerMask.NameToLayer("Default")))
        {
            OnSelectHander?.Invoke(hit.collider.gameObject);
        }
    }
}

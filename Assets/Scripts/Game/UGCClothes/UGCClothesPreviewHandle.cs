using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UGCClothesPreviewHandle : MonoBehaviour, IDragHandler
{

    public RoleController rController;

    public void OnDrag(PointerEventData eventData)
    {
        GameObject selectObj = EventSystem.current.currentSelectedGameObject;
        if (selectObj)
        {
            if (selectObj.name == "ClickArea")
            {
                Vector2 move = 0.3f * Input.GetTouch(0).deltaPosition;
                rController.transform.Rotate(Vector3.up, -move.x, Space.World);
            }
        }
    }
}

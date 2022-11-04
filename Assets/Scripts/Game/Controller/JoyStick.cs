using UnityEngine;

public class JoyStick : MonoBehaviour
{
    [SerializeField]
    Transform stick;

    [SerializeField]
    float radius;
    [SerializeField]
    Vector3 origPos;
    Vector3 screenPos;
    Vector3 ScreenPos
    {
        get
        {
            if (screenPos == default)
            {
                screenPos = GameObject.Find("UICamera").GetComponent<Camera>().WorldToScreenPoint(transform.position);
                screenPos.z = 0;
            }
            return screenPos;
        }
    }

    Canvas mainCanvas;
    Canvas MainCanvas
    {
        get
        {
            if (!mainCanvas)
            {
                mainCanvas = FindObjectOfType<Canvas>();
            }
            return mainCanvas;
        }
    }

    Camera uiCamera;
    Camera UICamera
    {
        get
        {
            if(uiCamera == null)
            {
                uiCamera =  GameObject.Find("UICamera").GetComponent<Camera>();
            }
            return uiCamera;
        }  
    }

    public Vector3 Touch(Vector3 touchScreenPos)
    {
        if (GlobalSettingPanel.Instance && GlobalSettingPanel.Instance.gameObject.activeSelf)
        {
            JoystickReset();
            return Vector3.zero;
        }
        Vector3 offset = touchScreenPos - screenPos;
        if (offset.magnitude > radius)
        {
            stick.localPosition = offset.normalized * radius;
        }
        else
        {
            stick.localPosition = offset;
        }

        return offset;
    }

    public void JoystickReset()
    {
        stick.localPosition = Vector3.zero;
        (transform as RectTransform).anchoredPosition = origPos;
        screenPos = UICamera.WorldToScreenPoint(transform.position);
        screenPos.z = 0;//复位
    }

    public bool InRange(Vector3 pos)
    {
        return Vector3.Distance(pos, ScreenPos) <= radius;
    }

    public void GetHotAreaPos()
    {

    }

    public void SetToPos(Vector2 touchPos)
    {
        if(GlobalSettingManager.Inst.IsLockMoveStick())
        {
            return;
        }
        // JoystickReset();
        float pandingDiff = radius;
        Vector2 rectPos = GameUtils.GetUIPointByScreenPoint(MainCanvas,touchPos);
        float canvasWidth = MainCanvas.GetComponent<RectTransform>().rect.width;
        float canvasHeight = MainCanvas.GetComponent<RectTransform>().rect.height;
        float zoomScale = GameUtils.GetAutoFixScale();//因屏幕UI经过自动适配，所以需要考虑自动适配带来的影响
        float leftX = (-canvasWidth/2) + (pandingDiff + Screen.safeArea.xMin) * zoomScale;
        float bottomY = (-canvasHeight/2) + (pandingDiff +Screen.safeArea.yMin) * zoomScale;
        float topY = 0 + pandingDiff * zoomScale;
        float rightX = -canvasWidth / 4 + pandingDiff * zoomScale;
        if(rectPos.x > rightX || rectPos.y > topY)
        {
            return;
        }

        if(rectPos.x < leftX && rectPos.y < bottomY)
        {
            return;
        }

        if(rectPos.x < (-canvasWidth/2) + Screen.safeArea.xMin || rectPos.y < (-canvasHeight/2) + Screen.safeArea.yMin * zoomScale)
        {
            return;
        }

        rectPos.x = rectPos.x < leftX ? leftX : rectPos.x;
        rectPos.x = rectPos.x > rightX ? rightX : rectPos.x;
        rectPos.y = rectPos.y < bottomY ? bottomY : rectPos.y;
        rectPos.y = rectPos.y > topY ? topY : rectPos.y;
        transform.localPosition = rectPos / zoomScale;
        this.screenPos = UICamera.WorldToScreenPoint(rectPos / zoomScale);
        this.screenPos.z = 0;
    }
}

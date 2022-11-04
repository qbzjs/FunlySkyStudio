using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 处理嵌套ScrollView滑动事件冲突，将当前脚本挂到子ScrollView 并关联父ScrollView
/// </summary>
public class NestedScrollRect : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public ScrollRect parentScrollRect; //父框
    ScrollRect scrollRect; //子框
    bool isUpAndDown = true; //子框的滑动方向
    void Awake () {
        scrollRect = GetComponent<ScrollRect> ();
        isUpAndDown = scrollRect.vertical;
        if (parentScrollRect == null) {
            parentScrollRect = scrollRect.GetComponentsInParent<ScrollRect> () [1]; //查找父节点的Scrollview
        }
    }

    public void OnBeginDrag (PointerEventData eventData) {
        parentScrollRect.OnBeginDrag (eventData);
        float angle = Vector2.Angle (eventData.delta, Vector2.up); //拖动方向和up方向的夹角
        //根据夹角判断启用哪一个Scrollview
        if (angle > 45 && angle < 135) {
            scrollRect.enabled = !isUpAndDown;
            parentScrollRect.enabled = isUpAndDown;
        } else {
            scrollRect.enabled = isUpAndDown;
            parentScrollRect.enabled = !isUpAndDown;
        }
        LoggerUtils.Log("######开始拖拽");
    }

    public void OnDrag (PointerEventData eventData) {
        parentScrollRect.OnDrag (eventData);
    }

    //结束拖动,需要将2个滑动框都启用
    public void OnEndDrag (PointerEventData eventData) {
        parentScrollRect.OnEndDrag (eventData);
        scrollRect.enabled = true;
        parentScrollRect.enabled = true;
    }
}
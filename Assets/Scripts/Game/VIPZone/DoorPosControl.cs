using RTG;
using UnityEngine;

public class DoorPosControl :MonoBehaviour,IRTTransformGizmoListener
{
    private const float xMin = -2.6f;
    private const float xMax = 2.6f;
    private const float yMax = 5.3f;
    private const float yMin = 0f;
    private Vector3 pos = new Vector3();
    public bool OnCanBeTransformed(Gizmo transformGizmo)
    {
        return true;
    }

    public void OnTransformed(Gizmo transformGizmo)
    {
        pos = transform.localPosition;
        pos.y = Mathf.Clamp(pos.y, yMin, yMax);
        pos.x = Mathf.Clamp(pos.x, xMin, xMax);
        transform.localPosition = pos;
    }
}
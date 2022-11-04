using UnityEngine;

public class DoorScaleKeeper : MonoBehaviour
{
    private Vector3 srcScale;
    public void StashScale()
    {
        Vector3 ss = transform.localScale;
        Vector3 ps = transform.parent.localScale;
        srcScale = new Vector3(ss.x*ps.x,ss.y*ps.y,ss.z*ps.z);
    }

    public void KeepScale()
    {
        Vector3 ps = transform.parent.localScale;
        Vector3 nowScale = new Vector3(srcScale.x/ps.x,srcScale.y/ps.y,srcScale.z/ps.z);
        transform.localScale = nowScale;
    }
}
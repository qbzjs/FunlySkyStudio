using UnityEngine;

public class SymmetrySeat : MonoBehaviour
{
    private Transform anchorSeat;
    private bool isActive = false;

    public void Init(Transform anchorSeat)
    {
        this.anchorSeat = anchorSeat;
    }

    public void ChangeAnchor(Transform anchor)
    {
        this.anchorSeat = anchor;
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
    }
    
    void Update()
    {
        if (!isActive)
        {
            return;
        }
        if (anchorSeat == null)
        {
            return;
        }

        var anchorLocPos = anchorSeat.transform.localPosition;
        transform.localPosition = new Vector3(-anchorLocPos.x,anchorLocPos.y,anchorLocPos.z);

        //x相反 y相反 z相同
        var anchorRot = anchorSeat.transform.localRotation.eulerAngles;
        float srcY = 180;
        float nowY = 180 - anchorRot.y;
        if (nowY > 180)
        {
            nowY -= 180;
        }
        transform.localRotation = Quaternion.Euler(-anchorRot.x,nowY,anchorRot.z);

        transform.localScale = anchorSeat.localScale;
    }
}
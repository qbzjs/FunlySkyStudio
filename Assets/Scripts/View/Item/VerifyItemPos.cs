using System.Collections;
using UnityEngine;

public class VerifyItemPos : MonoBehaviour
{
    public SuperTextMesh nickTMP;
    private float posXOff = 0.65f;

    void OnEnable()
    {
        RefreshPos();
    }
    
    public void RefreshPos()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ERefreshPos());
        }
    }

    private IEnumerator ERefreshPos()
    {
        yield return null;
        Vector3 heartPos = transform.localPosition;
        heartPos.x = nickTMP.preferredWidth / 2 + posXOff;
        transform.localPosition = heartPos;
    }
}

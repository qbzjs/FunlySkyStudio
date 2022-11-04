using UnityEngine;

public class WeatherEffectPositionAdjust : MonoBehaviour
{
    public Transform anchor;
    private Quaternion rotationSrc;
    private bool rotationGet = false;
    private float offset = 5;
    
    void Update()
    {
        if (anchor != null)
        {
            Vector3 forward = anchor.transform.forward;
            transform.position = anchor.position + forward.normalized * offset;
        }

        if (rotationGet)
        {
            transform.rotation = rotationSrc;
        }
    }

    public void SetRotaSrc(Quaternion rotaSrc)
    {
        rotationGet = true;
        rotationSrc = rotaSrc;
    }
}

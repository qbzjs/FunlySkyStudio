using UnityEngine;

public class FixedCtrl : MonoBehaviour
{
    private Vector3 worldPos;

    private void Awake()
    {
        worldPos = transform.position;
    }

    private void Update()
    {
        transform.position = worldPos;
    }
}

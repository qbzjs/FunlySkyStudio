
using System;
using UnityEngine;

public class NodeTriggerChecker : MonoBehaviour
{
    public Action<GameObject> OnTrigger;

    private void OnTriggerEnter(Collider other)
    {
        OnTrigger?.Invoke(other.gameObject);
    }
}

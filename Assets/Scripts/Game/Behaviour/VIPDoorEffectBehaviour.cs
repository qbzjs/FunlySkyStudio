using DG.Tweening.Core.Easing;
using UnityEngine;

public class VIPDoorEffectBehaviour : ActorNodeBehaviour
{
    public void DisabelCollider()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }
    public void EnableCollider()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }
    }
}
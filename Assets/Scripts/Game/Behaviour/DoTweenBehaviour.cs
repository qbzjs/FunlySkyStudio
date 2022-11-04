using UnityEngine;
using System.Collections;
using DG.Tweening;

public class DoTweenBehaviour : MonoBehaviour
{
    private void OnDestroy()
    {
        transform.DOKill();
    }
}

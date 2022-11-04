using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    void OnEnable()
    {
        SetLookAtDir();
    }
    void Update()
    {
        SetLookAtDir();
    }

    private void SetLookAtDir()
    {
        Vector3 lookAt = mainCamera.transform.position;
        lookAt.y = this.transform.position.y;
        transform.LookAt(lookAt);
    }
}

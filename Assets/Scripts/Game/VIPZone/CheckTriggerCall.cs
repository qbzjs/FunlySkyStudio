using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RTG;
using UnityEngine;

public class CheckTriggerCall : MonoBehaviour
{
    private bool isEnter = false;
    private float smallAreaSize = 0.3f;
    private BoxCollider boxCollider;
    private VIPZoneBehaviour vipZoneBehaviour;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        vipZoneBehaviour = GetComponentInParent<VIPZoneBehaviour>();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer != GameConsts.PLAYER_LAYER)
        {
            return;
        }
        VIPZoneManager.Inst.OnPlayerEnterCheckTrigger(vipZoneBehaviour);
    }
    
    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer != GameConsts.PLAYER_LAYER)
        {
            return;
        }
        VIPZoneManager.Inst.OnPlayerExitCheckTrigger(vipZoneBehaviour);
    }
    
    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.layer != GameConsts.PLAYER_LAYER)
        {
            return;
        }
        Vector3 colliderPos = boxCollider.center;
        colliderPos.y = 0;
        Vector3 playerPos = collider.transform.position;
        playerPos.y = 0;
        var disHorVec = colliderPos - playerPos;
        float disHor = disHorVec.magnitude;
        if (disHor < smallAreaSize)
        {
            VIPZoneManager.Inst.OnPlayerEnterTriggerCenterSmallArea(vipZoneBehaviour);
        }
    }

    public void SetSize(Vector3 size)
    {
        smallAreaSize = Mathf.Max(size.x, size.z) / 2 * VIPZoneConstant.FACTOR_CHECK_EFFECT * VIPZoneConstant.FACTOR_CHECK_CENTER;
    }
}
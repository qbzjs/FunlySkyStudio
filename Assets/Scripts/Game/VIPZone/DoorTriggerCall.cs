using UnityEngine;

public class DoorTriggerCall : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer != GameConsts.PLAYER_LAYER)
        {
            return;
        }
        VIPZoneBehaviour vipZoneBehaviour = GetComponentInParent<VIPZoneBehaviour>();
        if (!VIPZoneManager.Inst.CanEnter(vipZoneBehaviour))
        {
            TipPanel.ShowToast("Please check first before entering the VIP Zone");
            return;
        }
        VIPZoneManager.Inst.OnPlayerReceiveDoor(vipZoneBehaviour);
    }
    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer != GameConsts.PLAYER_LAYER)
        {
            return;
        }
        VIPZoneBehaviour vipZoneBehaviour = GetComponentInParent<VIPZoneBehaviour>();
        VIPZoneManager.Inst.OnPlayerLeaveDoor(vipZoneBehaviour);
    }
}
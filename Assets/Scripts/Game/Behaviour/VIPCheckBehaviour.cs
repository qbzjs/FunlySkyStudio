using UnityEngine;

public class VIPCheckBehaviour : ActorNodeBehaviour
{

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        var vipCheckBoundControl = GetComponent<VIPCheckBoundControl>();
        if (vipCheckBoundControl == null)
        {
            vipCheckBoundControl = gameObject.AddComponent<VIPCheckBoundControl>();
        }
        vipCheckBoundControl.InitEffect();
        vipCheckBoundControl.UpdateEffectShow();
    }

    public override void OnReset()
    {
        base.OnReset();
        VIPCheckBoundControl vipCheckBoundControl = GetComponent<VIPCheckBoundControl>();
        vipCheckBoundControl?.OnReset();
    }
}
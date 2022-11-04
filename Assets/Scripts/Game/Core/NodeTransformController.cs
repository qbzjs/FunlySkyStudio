using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeTransformController : CInstance<NodeTransformController>
{
    public void OnHandleMode(GameObject target, HandleMode handleMode)
    {
        VIPZoneManager.Inst.OnTransformChange(target,handleMode);
        switch (handleMode)
        {
            case HandleMode.Scale:
                PGCPlantManager.Inst.SetIntensity(target);
                FirePropManager.Inst.SetLightRangeOnScale(target);
                FlashLightManager.Inst.SetFixScale(target);
                break;
        }
    }
    public void OnDragUpdate(GameObject target, HandleMode handleMode)
    {
        FollowModeManager.Inst.SetFollowBoxTrans(target);
        VideoNodeManager.Inst.SetRangeEffectPos(target);        
        OnHandleMode(target, handleMode);

    }

    public void OnUndoRedo(GameObject target, HandleMode handleMode)
    {
        FollowModeManager.Inst.SetFollowBoxTrans(target);
        VideoNodeManager.Inst.SetRangeEffectPos(target);
        OnHandleMode(target, handleMode);

    }

    public void OnStepUpdate(GameObject target, HandleMode handleMode)
    {
        FollowModeManager.Inst.SetFollowBoxTrans(target);
        VideoNodeManager.Inst.SetRangeEffectPos(target);
        OnHandleMode(target, handleMode);

    }

    public void OnSelectTarget(GameObject target)
    {
        VideoNodeManager.Inst.OnSelectNode(target);
    }

    public void OnDisSelectTarget(GameObject target)
    {
        VideoNodeManager.Inst.OnDisSelectNode(target);
        ParachuteManager.Inst.OnDisSelectTarget(target);
        SlidePipeManager.Inst.OnDisSelectTarget(target);
    }
}

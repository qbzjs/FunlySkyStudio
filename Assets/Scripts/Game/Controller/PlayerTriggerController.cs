using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: pzkunn
/// Description: 人物触发检测器：检测人物碰撞触发器并调用事件
/// Date: 2021-12-20 16:15:28
/// </summary>
public class PlayerTriggerController : BMonoBehaviour<PlayerTriggerController>
{
    private void OnTriggerEnter(Collider other)
    {
        //add layer judgement by temporarily
        if (other.transform.gameObject.layer == LayerMask.NameToLayer("SpecialModel")
            || other.transform.gameObject.layer == LayerMask.NameToLayer("TriggerModel")
            || other.transform.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
        {
            var nodeBehav = other.GetComponentInParent<NodeBaseBehaviour>();
            if (nodeBehav)
            {
                nodeBehav.OnTrigEnter();
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        //add layer judgement by temporarily
        if (other.transform.gameObject.layer == LayerMask.NameToLayer("SpecialModel")
            || other.transform.gameObject.layer == LayerMask.NameToLayer("TriggerModel")
            || other.transform.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
        {
            var nodeBehav = other.GetComponentInParent<NodeBaseBehaviour>();
            if (nodeBehav)
            {
                nodeBehav.OnTrigExit();
            }
        }
    }

    public void SetTriggerActive(bool state)
    {
        gameObject.SetActive(state);
    }
}
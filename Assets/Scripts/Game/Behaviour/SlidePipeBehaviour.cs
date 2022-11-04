using System;
using System.Collections.Generic;
using UnityEngine;

public class SlidePipeBehaviour : ActorNodeBehaviour
{
  
    public List<SlidePipeWaypoint> mWaypoints = new List<SlidePipeWaypoint>();
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
    }

    public void Clear()
    {
        
    }
    public int GetMaxIndex()
    {
        int childCount = transform.childCount;
        return childCount + 1;
    }
    public int GetItemCount()
    {
        return transform.childCount;
    }

    //设置虚化模型
    public void SetVirtualModel(bool isHide)
    {
        SlidePipeComponent comp = entity.Get<SlidePipeComponent>();
        comp.HideModel = isHide == true ? 1 : 0;

        SlideItemBehaviour[] itemList = gameObject.GetComponentsInChildren<SlideItemBehaviour>();
        for(int i = 0;i < itemList.Length;i++)
        {
            itemList[i].SetItemVirtual(comp.HideModel);
        }       
    }

    public bool IsVirtualMode()
    {
        SlidePipeComponent comp = entity.Get<SlidePipeComponent>();
        return (comp.HideModel == 1);
    }

    public void SetRenderVisible(bool isVisible)
    {
        SlideItemBehaviour[] itemList = gameObject.GetComponentsInChildren<SlideItemBehaviour>();
        for(int i = 0;i < itemList.Length;i++)
        {
            itemList[i].SetRenderVisible(isVisible);
            itemList[i].SetColliderTrigger(!isVisible);
        }       
    }

    public SlideItemBehaviour GetTailItem()
    {
        SlideItemBehaviour tailItem = null;
        int maxIndex = 0;
        for(int i = 0;i<transform.childCount;i++)
        {
            Transform childNode = transform.GetChild(i);
            SlideItemBehaviour itemBehaviour = childNode.GetComponent<SlideItemBehaviour>();
            if(itemBehaviour)
            {
                var itemComp = itemBehaviour.entity.Get<SlideItemComponent>();
                if(itemComp.ItemIndex > maxIndex){
                    maxIndex = itemComp.ItemIndex;
                    tailItem = itemBehaviour;
                }
            }
        }
        return tailItem;
    }
    public void TakeWaypointInfoEdit(List<SlidePipeWaypoint> waypionts,SlideItemBehaviour node,bool isNegDir)
    {
        SlideItemMono itemMono = node.GetComponentInChildren<SlideItemMono>();
        SlideItemComponent itemCompt = node.entity.Get<SlideItemComponent>();
        int lenMono = itemMono.mWaypoints.Length;
        int startIndex = waypionts.Count;
        for (int i = 0; i < lenMono; i++)
        {
            SlideItemWaypointMono waypointMono = itemMono.mWaypoints[i];
            SlidePipeWaypoint waypoint = new SlidePipeWaypoint();
            waypoint.mPosition = waypointMono.transform.position;
            waypoint.mPosition+=waypointMono.transform.TransformVector(Vector3.up).normalized*0.95f;
            waypoint.mRotation = waypointMono.transform.rotation;
            if (isNegDir)
            {
              Quaternion q= Quaternion.Euler(0,180,0);
              waypoint.mRotation *= q;
            }
            waypoint.mSpeed = SlidePipeManager.Inst.GetSpeed(itemCompt.SpeedType);
            waypoint.mIndex = startIndex++;
            waypionts.Add(waypoint);
        }
    }
    public void RefreshWaypointsList(bool isNegDir)
    {
        mWaypoints.Clear();
       
        SlideItemBehaviour[] itemList = GetSortItemList();
        
        if(itemList == null || itemList.Length <= 0) return;
        for (int i = 0; i < itemList.Length; i++)
        {
            SlideItemBehaviour itemBehaviour = itemList[i];
            if (itemBehaviour)
            {
                TakeWaypointInfoEdit(mWaypoints,itemBehaviour, isNegDir);
            }
        }
	}

    public SlideItemBehaviour[] GetSortItemList()
    {
        SlideItemBehaviour[] itemList = gameObject.GetComponentsInChildren<SlideItemBehaviour>();
        Array.Sort(itemList,(item1,item2)=>{
            SlideItemComponent itemComp1 = item1.entity.Get<SlideItemComponent>();
            SlideItemComponent itemComp2 = item2.entity.Get<SlideItemComponent>();
            return itemComp1.ItemIndex - itemComp2.ItemIndex;
        });
        return itemList;
    }

    public void UnSelectAllItem()
    {
        SlideItemBehaviour[] itemList = gameObject.GetComponentsInChildren<SlideItemBehaviour>();
        for(int i = 0;i < itemList.Length;i++)
        {
            itemList[i].SetSelect(false);
        }
    }
    //玩家请求上滑梯
    public void OnClick(SlideItemBehaviour node)
    {
        IPlayerCtrlMgr iCtrl= PlayerControlManager.Inst.GetPlayerCtrlMgr(PlayerControlType.SlidePipe);
        if (iCtrl==null)
        {
            iCtrl= PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerSlidePipeControl>();
        }
        PlayerSlidePipeControl ctrl = iCtrl as PlayerSlidePipeControl;
        ESlideInPosType inPosType = node.IsTail ? ESlideInPosType.Tail : ESlideInPosType.Head;
        ctrl.OnClickUpSlidePipe(inPosType, entity.Get<GameObjectComponent>().uid);
    }
    public void UpdateLayer()
    {
        SlideItemBehaviour[] itemList = gameObject.GetComponentsInChildren<SlideItemBehaviour>();
        for (int i = 0; i < itemList.Length; i++)
        {
            itemList[i].UpdateLayer(entity.Get<SlidePipeComponent>().WayType);
        }
    }
}
/// <summary>
/// Author:Mingo-LiZongMing
/// Description:当人物靠近可拾起道具时，对传过来的behavior进行处理
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class PickablityController : CInstance<PickablityController>
{
    private NodeBaseBehaviour curBev;

    public void OnRayEnter(NodeBaseBehaviour baseBev)
    {
        curBev = baseBev;
        if(GameManager.Inst.ugcUserInfo != null)
        {
            var selfUid = GameManager.Inst.ugcUserInfo.uid;
            PickabilityManager.Inst.selfUid = selfUid;
        }
        HandleCatchProp();
    }

    public void OnRayExit()
    {
        HandleDropProp();
    }

    public void OnReleaseTrigger()
    {
        if (curBev != null)
        {
            EntityHighLight(curBev, false);
            curBev = null;
        }
    }

    private void ShowCatchPanel()
    {
        if (SwordManager.Inst.IsSelfInSword())
        {
            CatchPanel.Hide();
            return;
        }
        CatchPanel.Show();
        CatchPanel.Instance.SetCatchAction(OnBtnCatchClick);
        CatchPanel.Instance.SetCatchState(false);
        CatchPanel.Instance.transform.SetAsFirstSibling();
    }

    private void HandleCatchProp()
    {
        //开启背包后不用判断可拾取状态
        if (!CheckCanPick())
        {
           return;
        }
        if (CatchPanel.Instance && SceneParser.Inst.GetBaggageSet() == 1)
        {
            CatchPanel.Instance.SetBagCatchStateEnter(curBev);
        }
        EntityHighLight(curBev, true);
        ShowCatchPanel();
    }

    private void HandleDropProp()
    {
        if (curBev != null)
        {
            EntityHighLight(curBev, false);
            curBev = null;
        }
        //如果在播放动画
        var isPlayingAnim = !PlayerBaseControl.Inst.animCon.isPlaying
        && (!PlayerMutualControl.Inst || !PlayerMutualControl.Inst.isInEumual) && !PlayerBaseControl.Inst.animCon.isFishing;
        if (!isPlayingAnim)
        {
            CatchPanel.Hide();
            return;
        }

        if (SceneParser.Inst.GetBaggageSet() == 1)
        {
            CatchPanel.Show();
            CatchPanel.Instance.SetBagCatchStateExit();
            return;
        }

        //如果玩家没有拿着道具，则隐藏panel
        var isSelfPick = PickabilityManager.Inst.CheckSelfPickState();
        var selfUid = PickabilityManager.Inst.selfUid;
        if (!string.IsNullOrEmpty(selfUid))
        {
            var isPicked = PickabilityManager.Inst.GetPlayerPickState(selfUid);
            if (!isPicked)
            {
                CatchPanel.Hide();
            }
        }
        if (isSelfPick)
        {
            CatchPanel.Show();
        }
    }

    /// <summary>
    /// 当前玩家状态是否可以进行拾取
    /// </summary>
    private bool CheckCanPick()
    {
        var selfUid = PickabilityManager.Inst.selfUid;
        bool isPicked = true;
        if (string.IsNullOrEmpty(selfUid)) {
           if(GameManager.Inst.ugcUserInfo != null)
            {
                selfUid = GameManager.Inst.ugcUserInfo.uid;
                PickabilityManager.Inst.selfUid = selfUid;
            }
        }
        if (!string.IsNullOrEmpty(selfUid))
        {
            isPicked = PickabilityManager.Inst.GetPlayerPickState(selfUid);
        }
        //如果是背包模式就不判断玩家的拾取状态
        if(SceneParser.Inst.GetBaggageSet() == 1)
        {
            isPicked = false;
        }

        //牵手中，不可进行拾取
        var canPick = !PlayerBaseControl.Inst.animCon.isPlaying
        && (!PlayerMutualControl.Inst || !PlayerMutualControl.Inst.isInEumual) && (!VideoFullPanel.Instance || !VideoFullPanel.Instance.gameObject.activeSelf) && (!PlayerEatOrDrinkControl.Inst || !PlayerEatOrDrinkControl.Inst.IsEating);
        var isPickableProp = curBev.entity.HasComponent<PickablityComponent>();

        //求加好友状态回包前 不可进行拾取
        if (EmoMenuPanel.Instance && EmoMenuPanel.Instance.GetIsStateEmoRequesting())
        {
            canPick = false;
        }
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsInStateEmo())
        {
            canPick = false; 
        }
        if (PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid))
        { 
            canPick = false;
        }
        if(PromoteCtrPanel.Instance && PromoteCtrPanel.Instance.gameObject.activeSelf)
        {
            canPick = false;
        }
        if (StateManager.IsOnLadder)
        {
            canPick = false;
        }
        if (StateManager.IsOnSeesaw)
        {
            canPick = false;
        }
        if (StateManager.IsOnSwing)
        {
            canPick = false;
        }
        if (StateManager.IsOnSlide)
        {
            canPick = false;
        }
        if (PlayerBaseControl.Inst.animCon.isFishing) {
            canPick = false;
        }
        if (curBev.entity.HasComponent<BloodPropComponent>())
        {
            isPickableProp = false;
        }

        SteeringWheelBehaviour steeringWheel = null;
        if (PlayerDriveControl.Inst)
        {
            steeringWheel = PlayerDriveControl.Inst.steeringWheel;
        }
        return canPick && !isPicked && isPickableProp && (steeringWheel == null);
    }

    private void OnBtnCatchClick()
    {
        if (curBev == null) return;
        PickabilityManager.Inst.HandleCatchProp(curBev);
    }

    private void EntityHighLight(NodeBaseBehaviour baseBev, bool isHigh)
    {
        var baseBevs = baseBev.GetComponentsInChildren<NodeBaseBehaviour>();
        for (int i = 0; i < baseBevs.Length; i++)
        {
            HighLight(baseBevs[i], isHigh);
        }
    }

    private void HighLight(NodeBaseBehaviour baseBehav, bool isHigh)
    {
        if (baseBehav != null)
        {
            baseBehav.HighLight(isHigh);
        }
    }

}

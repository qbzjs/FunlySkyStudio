/// <summary>
/// Author:WeiXin
/// Description:方向盘MonoBehaviour
/// Date: 2022-01-18
/// </summary>

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SteeringWheelBehaviour : NodeBaseBehaviour
{
    public Transform carTrs;
    public Transform carRgbTrs;
    public Rigidbody carRgb;
    public BoxCollider carBc;
    public MeshCollider[] carMCS;
    public Transform driverTrs;
    public int uid;
    public Transform follow;
    public bool isMoved = false;

    private Color[] colors;
    private int layer;
    private Vector3 orginPos;
    private Quaternion orginRot;


    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        carTrs = transform;
        carBc = GetComponentInChildren<BoxCollider>();
        layer = LayerMask.NameToLayer("Touch");
        ResetLayer();
        var obj = new GameObject("follow");
        follow = obj.transform;
        follow.SetParent(carTrs);
        var pos = carTrs.position - carTrs.forward * 0.5f + carTrs.up * 0.3f;
        follow.SetPositionAndRotation(pos, carTrs.rotation);
        // uid = entity.Get<GameObjectComponent>().uid;
        // SteeringWheelManager.Inst.AddCar(uid,this);
        bool isCombine = carTrs.parent.GetComponent<CombineBehaviour>() != null;
        orginPos = isCombine ? carTrs.parent.position : carTrs.position;
        orginRot = isCombine ? carTrs.parent.rotation : carTrs.rotation;
    }

    public override void OnRayEnter()
    {
        if (PlayerOnSeesawControl.Inst != null && PlayerOnSeesawControl.Inst.isOnSeesaw)
        {
            return;
        }
        if (PlayerOnSwingControl.Inst != null && PlayerOnSwingControl.Inst.isOnSwing)
        {
            return;
        }
        PortalPlayPanel.Show();
        PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.SteeringWheel);
        PortalPlayPanel.Instance.SetTransform(transform);
        PortalPlayPanel.Instance.AddButtonClick(OnClick, true);
    }

    private void OnClick()
    {
        //已经在吃东西的时候不能吃东西
        if (PlayerEatOrDrinkControl.Inst && PlayerEatOrDrinkControl.Inst.IsEating)
        {
            TipPanel.ShowToast("You could not eat food in the current state.");
            return;
        }
        // 牵手状态下，不能和方向盘交互
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("You could not interact with Steering wheel while hand-in-hand");
            return;
        }
        //拾取道具状态下，不能和方向盘交互
        if (PickabilityManager.Inst.isSelfPicking || PlayerControlManager.Inst.isPickedProp)
        {
            TipPanel.ShowToast("You could not interact with Steering wheel while holding object");
            return;
        }
        //求加好友状态回包前 不可进行操作
        if (EmoMenuPanel.Instance && EmoMenuPanel.Instance.GetIsStateEmoRequesting())
        {
            return;
        }
        //求加好友状态中，不可进行操作
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetCurStateEmoName() == EmoName.EMO_ADD_FRIEND)
        {
            TipPanel.ShowToast("Please quit the add friend emote first");
            return;
        }
        // 自拍模式不能和方向盘交互
        if (StateManager.IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return;
        }
        //冻结状态不允许开车
        if (PlayerBaseControl.Inst&&PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        SteeringWheelManager.Inst.PlayerSendOnSteering(uid);
        // PlayerControl.Inst.SetPosToNewPoint(pos, trs.rotation);
        PortalPlayPanel.Hide();
    }

    public override void OnRayExit()
    {
        PortalPlayPanel.Hide();
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref colors);
    }

    public void ResetLayer()
    {
        carBc.gameObject.layer = layer;
    }
    
    public void ResetPos()
    {
        if (carTrs.parent.GetComponent<CombineBehaviour>() == null)
        {
            carTrs.position = orginPos;
            carTrs.rotation = orginRot;
        }
        else
        {
            carTrs.parent.position = orginPos;
            carTrs.parent.rotation = orginRot;
        }
    }

    public void Anim(bool play)
    {
        DoTweenBehaviour anim;
        if (carTrs.parent.GetComponent<CombineBehaviour>() == null)
        {
            anim = carTrs.parent.GetComponent<DoTweenBehaviour>();
        }
        else
        {
            anim = carTrs.parent.parent.GetComponent<DoTweenBehaviour>();
        }
        if (anim == null) return;
        if (!isMoved)
        {
            var trs = anim.transform.GetChild(0);
            anim.transform.eulerAngles = trs.eulerAngles;
            trs.localEulerAngles = Vector3.zero;
        }
        if (play)
        {
            anim.transform.DOPlay();
        }
        else
        {
            anim.transform.DOPause();
        }
    }

    public void DriverFollowCar()
    {
        if (driverTrs != null)
        {
            driverTrs.SetPositionAndRotation(follow.position - new Vector3(0,0.9f,0), follow.rotation);
        }
    }

    public void OnModeChange(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            ResetPos();
            isMoved = false;
        }
        else
        {
            var localScale = carTrs.parent.localScale;
            follow.position = carTrs.position 
                              - carTrs.forward * 0.5f / localScale.x
                              + carTrs.up * 0.3f / localScale.y;
            bool isCombine = carTrs.parent.GetComponent<CombineBehaviour>() != null;
            orginPos = isCombine ? carTrs.parent.position : carTrs.position;
            orginRot = isCombine ? carTrs.parent.rotation : carTrs.rotation;
        }
    }
    
    public void OnDisable()
    {
        if (SteeringWheelManager.IsInit())
        {
            SteeringWheelManager.Inst.OnSteeringWheelDisable(uid);
        }
    }
}
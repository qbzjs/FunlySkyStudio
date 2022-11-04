using System;
using System.Collections;
using System.Collections.Generic;
using RTG;
using UnityEngine;
using UnityEngine.Animations;
using Axis = UnityEngine.Animations.Axis;

public class SwingBehaviour : NodeBaseBehaviour
{
    [SerializeField] private Transform board;
    [SerializeField] private Transform ropeL;
    [SerializeField] private Transform ropeR;
    [SerializeField] private Transform rope;
    [SerializeField] private Transform top;
    [SerializeField] private Transform bottom;
    [SerializeField] public Transform sit;
    private Transform center;

    private Transform customBoard;
    private bool playing;
    private bool needFinsh;
    private bool isSelf;
    private int forward = -1;
    public Vector3 startPos = Vector3.zero;
    public Vector3 TargetPos = Vector3.zero;
    private readonly float angleSpeed = 90f;
    private readonly float angleSlow = 10f;
    private readonly float stopSpeed = 20f;
    private readonly float acceleration = 160;
    private readonly float maxAngle = 80;
    private readonly float maxAngleSpeed = 10f;
    public float nowSpeed = 0;
    private float nowAcceleration = 0;
    private readonly Vector3 animationOffset = new Vector3(0, -0.6f, -0.1f);
    private readonly Vector3 playerOffset = new Vector3(0, 1f, 0);
    private Vector3 nowRota = Vector3.zero;
    private Transform beforeNode;
    private Transform player;
    private Action leaveAct;
    private OtherPlayerCtr otherPlayer;
    private ParentConstraint pc;
    private Color[] colors;

    public void PlayerLeaveSwing()
    {
        if (player != null)
        {
            nowSpeed = 0;
            playing = false;
            player.SetParent(beforeNode);
            player = null;
            beforeNode = null;
            ReSetView();
            MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        }

        isSelf = false;
        otherPlayer = null;
    }

    public void PlayerOnBoard(Transform trs, bool self = false)
    {
        if (trs != null)
        {
            isSelf = self;
            beforeNode = trs.parent;
            if (isSelf)
            {
                beforeNode.position = sit.position + playerOffset;
            }
            else
            {
                otherPlayer = trs.GetComponent<OtherPlayerCtr>();
            }
            trs.SetParent(sit.parent);
            trs.position = sit.position + animationOffset;
            trs.rotation = rope.rotation;
            player = trs;
            if (!PlayerBaseControl.Inst.isTps)
            {
                SetView();
            }
        }
    }

    public void OnModeChange(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            ToEditMode();
        }
        else
        {
            ToPlayMode();
        }
    }

    private void ToPlayMode()
    {
        if (center == null)
        {
            center = Instantiate(top);
        }

        center.position = top.position;
        center.rotation = top.rotation;
        center.localScale = Vector3.one;
        center.SetParent(transform.parent);
        transform.SetParent(center);
    }

    private void ToEditMode()
    {
        ForceStop();
        if (center == null)
        {
            return;
        }

        center.eulerAngles = Vector3.zero;
        transform.SetParent(center.parent);
        center.SetParent(transform);
    }

    public void Play()
    {
        nowSpeed = nowSpeed < -angleSpeed ? nowSpeed : -angleSpeed;
        nowRota.x = playing ? nowRota.x : -0.000001f;
        nowAcceleration = getAcceleration(forward, nowRota.x, acceleration);
        playing = true;
        needFinsh = false;
        if (isSelf && PlayerOnSwingControl.Inst && !StateManager.IsInSelfieMode)
        {
            PlayerOnSwingControl.Inst.Playfront();
        }
        else if (otherPlayer != null)
        {
            otherPlayer.Swingfront();
        }
    }

    public void Leave(Action act)
    {
        if (playing)
        {
            leaveAct = act;
            needFinsh = true;
        }
        else
        {
            PlayerLeaveSwing();
            act?.Invoke();
            leaveAct = null;
            if (isSelf)
            {
                PlayModePanel.Instance.SwingButtonReset();
            }
        }
    }

    public void ForceStop()
    {
        if (center != null)
        {
            center.eulerAngles = Vector3.zero;
            transform.SetParent(center.parent);
            center.SetParent(transform);
        }

        playing = false;
        needFinsh = true;
        forward = -1;
        Leave(leaveAct);
    }

    public void InitSwing(SwingNodeData sd)
    {
        Transform trs = board;
        if (!string.IsNullOrEmpty(sd.rId) && customBoard != null)
        {
            trs = customBoard;
        }

        trs.localPosition = sd.seatPos ?? Vector3.zero;
        trs.localEulerAngles = sd.seatRote ?? Vector3.zero;
        trs.localScale = sd.seatScale ?? Vector3.one;
        rope.localPosition = sd.ropePos ?? Vector3.zero;
        rope.localEulerAngles = sd.ropeRote ?? Vector3.zero;
        rope.localScale = sd.ropeScale ?? Vector3.one;
        sit.localPosition = sd.sitPos ?? Vector3.zero;
        rope.gameObject.SetActive(!sd.hide);
    }

    public void InitSwingState(float angle, float speed, int time)
    {
        if (time == 0 || time > 60000)
        {
            forward = -1;
            playing = false;
            if (otherPlayer != null)
            {
                otherPlayer.SwingIdle();
            }
        }
        else
        {
            nowRota.x = angle;
            nowSpeed = speed;
            playing = true;
            forward = Math.Sign(speed);
            nowAcceleration = forward;
            center.localEulerAngles = nowRota;
            if (otherPlayer != null)
            {
                if (forward > 0)
                {
                    otherPlayer.SwingBack();
                }
                else
                {
                    otherPlayer.Swingfront();
                }
            }
        }
    }

    private float getAcceleration(float forward, float nowPos, float acceleration)
    {
        float na = nowPos >= 0 ? -1 : 1;
        float nf = na == forward ? -na : na;
        na = (float) (na * acceleration * Math.Sin(Math.Abs(nowRota.x * Math.PI / 180)) + nf * angleSlow);
        return na;
    }

    private void Swing()
    {
        if (!playing)
        {
            return;
        }

        var deltaTime = Time.deltaTime;


        var move = deltaTime * nowSpeed;
        var nx = nowRota.x;
        var ns = nowSpeed;
        float na = getAcceleration(forward, nowRota.x, acceleration);

        nowSpeed += na * deltaTime;
        nowRota.x += move;

        if (Math.Abs(nowRota.x) >= maxAngle)
        {
            forward = -forward;
            nowSpeed = forward * maxAngleSpeed;
            nowRota.x = Math.Sign(nowRota.x) * maxAngle;
            nowAcceleration = getAcceleration(forward, nowRota.x, acceleration);
            if (isSelf && PlayerOnSwingControl.Inst && !StateManager.IsInSelfieMode)
            {
                if (forward > 0)
                {
                    PlayerOnSwingControl.Inst.PlayBack();
                }
                else
                {
                    PlayerOnSwingControl.Inst.Playfront();
                }
            }
            else if (otherPlayer != null)
            {
                if (forward > 0)
                {
                    otherPlayer.SwingBack();
                }
                else
                {
                    otherPlayer.Swingfront();
                }
            }
        }
        else if (Math.Sign(nowSpeed) != Math.Sign(ns))
        {
            float l = ns * ns / (2 * getAcceleration(forward, nx, acceleration));
            float t = deltaTime - l / ns;
            forward = -forward;
            nowAcceleration = getAcceleration(forward, nx, acceleration);
            nowSpeed = nowAcceleration * t;
            nowRota.x = nx + l * -forward + nowSpeed * t;
            if (isSelf && PlayerOnSwingControl.Inst && !StateManager.IsInSelfieMode)
            {
                if (forward > 0)
                {
                    PlayerOnSwingControl.Inst.PlayBack();
                }
                else
                {
                    PlayerOnSwingControl.Inst.Playfront();
                }
            }
            else if (otherPlayer != null)
            {
                if (forward > 0)
                {
                    otherPlayer.SwingBack();
                }
                else
                {
                    otherPlayer.Swingfront();
                }
            }
        }
        else if (Math.Sign(nowRota.x) != Math.Sign(nx) && nx != 0 && nowRota.x != 0)
        {
            if (Math.Abs(nowSpeed) <= stopSpeed || needFinsh)
            {
                nowRota.x = 0;
                playing = false;
                forward = -1;
                if (isSelf)
                {
                    PlayModePanel.Instance.SwingButtonReset();
                    if (PlayerOnSwingControl.Inst && !StateManager.IsInSelfieMode)
                    {
                        PlayerOnSwingControl.Inst.PlayIdle();
                    }
                }
                else if (otherPlayer != null)
                {
                    otherPlayer.SwingIdle();
                }

                if (needFinsh)
                {
                    needFinsh = false;
                    PlayerLeaveSwing();
                }

                if (leaveAct != null)
                {
                    leaveAct.Invoke();
                    leaveAct = null;
                }
            }
            else
            {
                float t = Math.Abs(nx / nowSpeed);
                float s = t * nowAcceleration;
                nowAcceleration = getAcceleration(forward, nowRota.x, acceleration);
                t = deltaTime - t;
                nowSpeed = ns + s + nowAcceleration * t;
                nowRota.x = nowSpeed * t;
            }
        }

        center.localEulerAngles = nowRota;
    }

    public void SetBoard(Transform trs = null)
    {
        Transform ot = customBoard;
        if (trs == null)
        {
            if (customBoard != null)
            {
                board.localPosition = customBoard.localPosition;
                board.localScale = customBoard.localScale;
                board.localRotation = customBoard.localRotation;
                Destroy(customBoard.gameObject);
                customBoard = null;
            }

            board.gameObject.SetActive(true);
            entity.Get<SeesawSeatComponent>().rId = "";
        }
        else
        {
            Transform nt = GetBoard();
            trs.SetParent(transform);
            trs.localPosition = nt.localPosition;
            trs.localScale = nt.localScale;
            trs.localRotation = nt.localRotation;
            customBoard = trs;
            board.gameObject.SetActive(false);
            var boxCollider = trs.GetComponentInChildren<Collider>();
            if (boxCollider != null)
            {
                boxCollider.gameObject.layer = LayerMask.NameToLayer("Touch");
                boxCollider.enabled = true;
            }
        }

        if (ot != null)
        {
            SceneBuilder.Inst.DestroyEntity(ot.gameObject);
        }
    }

    // public Transform getPlayer()
    // {
    //     return player;
    // }

    public Transform GetSit()
    {
        return sit;
    }

    public Transform GetBoard()
    {
        return customBoard == null ? board : customBoard;
    }

    public Transform GetRope()
    {
        return rope;
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        SwingManager.Inst.AddSwing(this);
    }

    public override void OnRayEnter()
    {
        base.OnRayEnter();
        if (!SwingManager.Inst.CanUseSwing() || player != null)
        {
            return;
        }

        PortalPlayPanel.Show();
        PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Swing);
        PortalPlayPanel.Instance.SetTransform(transform);
        PortalPlayPanel.Instance.AddButtonClick(PlayerOnSwing, true);
    }

    public override void OnRayExit()
    {
        base.OnRayExit();
        PortalPlayPanel.Hide();
    }

    public override void OnReset()
    {
        base.OnReset();
        rope.localScale = Vector3.one;
        rope.localEulerAngles = Vector3.zero;
        rope.localPosition = Vector3.zero;
        rope.gameObject.SetActive(true);
        board.localScale = Vector3.one;
        board.localEulerAngles = Vector3.zero;
        board.localPosition = Vector3.zero;
        rope.gameObject.SetActive(true);
        sit.localPosition = Vector3.zero;
        if (customBoard != null)
        {
            Destroy(customBoard.gameObject);
            customBoard = null;
        }
    }

    private void PlayerOnSwing()
    {
        if (!StateManager.Inst.CheckCanSitOnSwing())
        {
            return;
        }

        SwingManager.Inst.PlayerSendOnSwing(GetHashCode());
        PortalPlayPanel.Hide();
    }

    public void SetView()
    {
        pc = beforeNode.gameObject.GetComponent<ParentConstraint>();
        if (pc == null)
        {
            pc = beforeNode.gameObject.AddComponent<ParentConstraint>();
        }
        ConstraintSource constraintSource = new ConstraintSource() { sourceTransform = sit, weight = 1 };
        pc.SetSources(new List<ConstraintSource>() { constraintSource });
        pc.SetTranslationOffset(0, playerOffset);
        pc.SetRotationOffset(0, Vector3.zero);
        pc.rotationAxis = Axis.X;
        pc.constraintActive = true;
    }
    
    public void ReSetView()
    {
        if (pc != null)
        {
            pc.constraintActive = false;
            pc = null;
            PlayerControlManager.Inst.ChangeAnimClips();
        }
    }

    public void Selfie()
    {
        if (isSelf)
        {
            player.localPosition = Vector3.zero;
        }
    }

    public void ExitSelfie()
    {
        if (isSelf)
        {
            player.position = sit.position + animationOffset;
            player.rotation = rope.rotation;
            PlayerOnSwingControl.Inst.PlayIdle();
        }
    }
    
    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref colors);
    }

    public void OnRemoveNode()
    {
        rope.localScale = Vector3.one;
        rope.localEulerAngles = Vector3.zero;
        rope.localPosition = Vector3.zero;
        rope.gameObject.SetActive(true);
        board.localScale = Vector3.one;
        board.localEulerAngles = Vector3.zero;
        board.localPosition = Vector3.zero;
        rope.gameObject.SetActive(true);
        sit.localPosition = Vector3.zero;
        if (customBoard != null)
        {
            Destroy(customBoard.gameObject);
            customBoard = null;
        }
    }

    private void Update()
    {
        Swing();
    }
}
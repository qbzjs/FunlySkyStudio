using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using SavingData;
using UnityEngine;
using DG.Tweening;
using System;
using Object = System.Object;

public class SeesawBehaviour : NodeBaseBehaviour
{
    private const float xOffset = 2.6f;
    private const float yOffset = -0.035f;
    private Color colorSrc = new Color(0.8247f, 0.8078f, 0.7490f);
    public Transform carryTranL, carryTranR;
    private bool isNotEmpty;


    [HideInInspector]
    public Transform seesawTrans;
    public bool isSwing; // 跷跷板是否正在摆动
    public float curSpeed { get; private set; } // 当前旋转速度
    public float curAngleMax { get; private set; } // 当前最大旋转角
    public float curAngleMin { get; private set; }  // 当前最小旋转角
    float defaultAngleMax = 18f; // 默认最大旋转角
    float defaultAngleMin = -18f; // 默认最小旋转角
    float angular = 10f; // 加速度
    float deltaSpeed = 30f; // 每次下压增加的速度
    float maxSpeed = 60f; // 最大速度
    int dir = -1; // 跷跷板方向 右边下压方向：-1， 左边下压方向：1
    bool isRight = false; // 是否在跷跷板右边座椅
    public float angleZ { get; private set; } // 当前角度
    private const float MOVE_THRESHOLD = 4;//跷跷板旋转阈值，超过该值播放跷跷板旋转音效
    float beforeAngle;
    BudTimer onSeesawTimer, leaveSeesawTimer, changeAnimTimer;
    private Transform stablePart;
    private Transform stableSrcParent;
    private Vector3 stablePartPosition;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        SeesawManager.Inst.AddSeesaw(this);
        enabled = true;
    }
    
    public Vector3 GetLeftSeatSrcPosition()
    {
        return new Vector3(-xOffset,yOffset,0);
    }
    public Vector3 GetLeftSeatPosition()
    {
        return FindSeat(0).localPosition;
    }
    public Vector3 GetRightSeatPosition()
    {
        return FindSeat(1).localPosition;
    }
    public Vector3 GetRightSeatSrcPosition()
    {
        return new Vector3(xOffset,yOffset,0);
    }

    public void SetMat(int id)
    {
        var matData = GameManager.Inst.matConfigDatas.Find(x => x.id == id);
        Texture t = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + matData.texName);
        Renderer[] renderer = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderer.Length; i++)
        {
            renderer[i].material.SetTexture("_MainTex", t);
        }
    }

    public void SetColor(Color color)
    {
        Renderer[] renderer = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderer.Length; i++)
        {
            renderer[i].material.SetColor("_Color", color);
        }
    }
    
    public void ChangeSeat(NodeBaseBehaviour nBehav,int index,string rId,bool isSymmetry)
    {
        var seat = FindSeat(index);
        
        nBehav.transform.parent = transform.parent;
        nBehav.transform.position = seat.position;
        nBehav.transform.localScale = Vector3.one;
        nBehav.transform.localRotation = index == 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
        nBehav.entity.Get<SeesawSeatComponent>().index = index;
        nBehav.entity.Get<SeesawSeatComponent>().rId = rId;
        nBehav.norScale = true;
        
        //先把原来的移出去
        seat.SetParent(SceneBuilder.Inst.StageParent);
        SceneBuilder.Inst.DestroyEntity(seat.gameObject);
        
        if (index == 1)
        {
            //这里可能是从池子里拿出来的，可能本来挂了脚本了，先判断一下有没有
            SymmetrySeat symmetrySeat = nBehav.GetComponent<SymmetrySeat>();
            if (symmetrySeat == null)
            {
                symmetrySeat = nBehav.gameObject.AddComponent<SymmetrySeat>();
            }

            if (!symmetrySeat.enabled)
            {
                symmetrySeat.enabled = true;
            }
            symmetrySeat.Init(SeesawManager.Inst.FindLeftSeat(nBehav));
            if (isSymmetry)
            {
                symmetrySeat.SetActive(true);
            }
        }else if (index == 0)
        {
            //同步右边的SymmetrySeat参考Transform
            SeesawBehaviour seesawBehaviour = SeesawManager.Inst.FindSeesawBehaviour(nBehav);
            Transform rightSeat = seesawBehaviour.FindSeat(1);
            SymmetrySeat symmetrySeat = rightSeat.GetComponent<SymmetrySeat>();
            if (symmetrySeat != null)
            {
                symmetrySeat.ChangeAnchor(nBehav.transform);
            }
        }

        SeesawManager.Inst.AddSeatBoard(nBehav, this);
        SeesawManager.Inst.SetToTouchLayer(nBehav.transform);

        NodeBaseBehaviour seatBehv = seat.GetComponent<SeesawSeatBehaviour>();
        if (seatBehv)
        {
            SeesawManager.Inst.RemoveSeatBoard(seatBehv);
        }
        else
        {
            seatBehv = seat.GetComponent<UGCCombBehaviour>();
            if (seatBehv)
            {
                SeesawManager.Inst.RemoveSeatBoard(seatBehv);
            }
        }

    }


    public void SetTiling(Vector2 tiling)
    {
        Renderer[] renderer = GetComponentsInChildren<Renderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        renderer[0].GetPropertyBlock(mpb);
        mpb.SetVector("_MainTex_ST", new Vector4(tiling.x, tiling.y, 0, 0));
        for (int i = 0; i < renderer.Length; i++)
        {
            renderer[i].SetPropertyBlock(mpb);
        }
    }

    public Transform FindSeat(int seatIndex)
    {
        Transform parent = transform.parent;
        SeesawSeatBehaviour[] seats = parent.GetComponentsInChildren<SeesawSeatBehaviour>();
        for (int i = 0; i < seats.Length; i++)
        {
            int index = seats[i].entity.Get<SeesawSeatComponent>().index;
            if (index == seatIndex)
            {
                return seats[i].transform;
            }
        }
        
        UGCCombBehaviour[] ugcs = parent.GetComponentsInChildren<UGCCombBehaviour>();
        for (int i = 0; i < ugcs.Length; i++)
        {
            int index = ugcs[i].entity.Get<SeesawSeatComponent>().index;
            if (index == seatIndex)
            {
                return ugcs[i].transform;
            }
        }

        return null;
    }

    public NodeBaseBehaviour GetLeftSeatBehaviour()
    {
        return GetSeatBehaviour(0);
    }

    public NodeBaseBehaviour GetRightSeatBehaviour()
    {
        return GetSeatBehaviour(1);
    }

    private NodeBaseBehaviour GetSeatBehaviour(int seatIndex)
    {
        Transform parent = transform.parent;
        SeesawSeatBehaviour[] seats = parent.GetComponentsInChildren<SeesawSeatBehaviour>();
        for (int i = 0; i < seats.Length; i++)
        {
            int index = seats[i].entity.Get<SeesawSeatComponent>().index;
            if (index == seatIndex)
            {
                return seats[i];
            }
        }
        
        UGCCombBehaviour[] ugcs = parent.GetComponentsInChildren<UGCCombBehaviour>();
        for (int i = 0; i < ugcs.Length; i++)
        {
            int index = ugcs[i].entity.Get<SeesawSeatComponent>().index;
            if (index == seatIndex)
            {
                return ugcs[i];
            }
        }

        return null;
    }
    
    public void ResetLeftSeat()
    {
        Transform leftSeat = FindSeat(0);
        leftSeat.localPosition = GetLeftSeatSrcPosition();
        leftSeat.localRotation = Quaternion.identity;
        leftSeat.localScale = Vector3.one;
    }

    public void ResetRightSeat()
    {
        Transform rightSeat = FindSeat(1);
        rightSeat.localPosition = GetRightSeatSrcPosition();
        rightSeat.localRotation = Quaternion.Euler(0,180,0);
        rightSeat.localScale = Vector3.one;
    }
    public void RefreshSeatState(int seatIndex, bool isFull)
    {
        Transform parent = transform.parent;
        SeesawSeatBehaviour[] seats = parent.GetComponentsInChildren<SeesawSeatBehaviour>();
        for (int i = 0; i < seats.Length; i++)
        {
            var seatCom = seats[i].entity.Get<SeesawSeatComponent>();
            int index = seatCom.index;
            if (index == seatIndex)
            {
                seats[i].SetCurStatu(isFull);
                seatCom.isFull = isFull;
                return;
            }
        }

        UGCCombBehaviour[] ugcs = parent.GetComponentsInChildren<UGCCombBehaviour>();
        for (int i = 0; i < ugcs.Length; i++)
        {
            var seatCom = ugcs[i].entity.Get<SeesawSeatComponent>();
            int index = seatCom.index;
            if (index == seatIndex)
            {
                seatCom.isFull = isFull;
                return;
            }
        }
    }

    public void ResetAllSeat()
    {
        var leftSeat = GetLeftSeatBehaviour();
        var rightSeat = GetRightSeatBehaviour();
        ResetSeatStatus(leftSeat);
        ResetSeatStatus(rightSeat);
    }

    public void ResetSeatStatus(NodeBaseBehaviour seat)
    {
        if (seat != null)
        {
            if(seat is SeesawSeatBehaviour)
            {
                SeesawSeatBehaviour bv = seat as SeesawSeatBehaviour;
                bv.SetCurStatu(false);
            }

            var seatCom = seat.entity.Get<SeesawSeatComponent>();
            seatCom.isFull = false;
        }
    }

    public void OnSeesawSuccess()
    {
        SetCurStatu(true);
        seesawTrans = transform.parent;
        if (PortalPlayPanel.Instance != null
            && PortalPlayPanel.Instance.gameObject.activeSelf
            && PortalPlayPanel.Instance.GetCurTargetId() == entity.Get<GameObjectComponent>().uid.ToString())
        {
            PortalPlayPanel.Hide();
        }
    }
    public void LeaveSeesawSuccess()
    {
        SetCurStatu(false);
        MessageHelper.Broadcast(MessageName.ReleaseTrigger);
    }
    public void SetCurStatu(bool isNotEmpty)
    {
        this.isNotEmpty = isNotEmpty;
        Collider[] cs = transform.parent.GetComponentsInChildren<Collider>();
        for (int i = 0; i < cs.Length; i++)
        {
            cs[i].enabled = !isNotEmpty;
        }
    }

    public void OnModeChange(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            ResetSeesaw();
            InitSymmetry();
            ResumeBottomSupportPart();
        }
        else
        {
            SymmetrySeat symmetrySeat = transform.parent.GetComponentInChildren<SymmetrySeat>();
            if (symmetrySeat != null)
            {
                Destroy(symmetrySeat);
            }
            MoveBottomSupportPartOut();
        }
    }

    private void ResumeBottomSupportPart()
    {
        var ifHas = GameUtils.FindChildByName(transform.parent, "seesaw01");
        if (ifHas)
        {
            return;
        }

        if (stablePart == null)
        {
            return;
        }
        stablePart.SetParent(stableSrcParent);
        stablePart.localPosition = stablePartPosition;
    }

    private void MoveBottomSupportPartOut()
    {
        stablePart = GameUtils.FindChildByName(transform.parent, "seesaw01");
        if (stablePart == null)
        {
            return;
        }
        stableSrcParent = stablePart.parent;
        stablePartPosition = stablePart.localPosition;
        stablePart.SetParent(SceneBuilder.Inst.StageParent);
    }

    public void SetPlayerNode(bool isRight)
    {
        Transform carryNodeTran;
        if (isRight)
        {
            if (carryTranR == null)
            {
                CreateCarryNode(isRight);
            }
            carryNodeTran = carryTranR;
        }
        else
        {
            if (carryTranL == null)
            {
                CreateCarryNode(isRight);
            }
            carryNodeTran = carryTranL;
        }
        RefreshSeatPos(carryNodeTran, isRight);
    }

    public void CreateCarryNode(bool isRight)
    {
        Transform carryNodeTran;
        if (isRight)
        {
            if (carryTranR == null)
            {
                carryTranR = new GameObject("carryNodeR").transform;
            }
            carryNodeTran = carryTranR;
        }
        else
        {
            if (carryTranL == null)
            {
                carryTranL = new GameObject("carryNodeL").transform;
            }
            carryNodeTran = carryTranL;
        }
        if (carryNodeTran)
        {
            carryNodeTran.SetParent(transform.parent, true);
            carryNodeTran.localRotation = Quaternion.identity;
            RefreshSeatPos(carryNodeTran, isRight);
        }
    }

    public void RefreshSeatPos(Transform carryNodeTran, bool isRight)
    {
        var pos = GetLeftSitPos();
        if (isRight)
        {
            pos = GetRightSitPos();
        }

        carryNodeTran.localPosition = pos;
    }

    private SeesawComponent GetSeeSawComponent()
    {
        Transform parent = transform.parent;
        SeesawComponent seesawComponent = parent.GetComponent<NodeBaseBehaviour>().entity.Get<SeesawComponent>();
        return seesawComponent;
    }

    public Vector3 GetLeftSitPos()
    {
        SeesawComponent seesawComponent = GetSeeSawComponent();
        if (seesawComponent.setLeftSitPoint == 1)
        {
            return seesawComponent.leftSitPoint;
        }

        return GetLeftSeatSrcPosition() + Vector3.up * 0.1f;
    }

    public Vector3 GetRightSitPos()
    {
        SeesawComponent seesawComponent = GetSeeSawComponent();
        if (seesawComponent.setRightSitPoint == 1)
        {
            return seesawComponent.rightSitPoint;
        }

        return GetRightSeatSrcPosition() + Vector3.up * 0.1f;
    }

    public override void OnReset()
    {
        base.OnReset();
        ResetSeesaw();
        ResumeBottomSupportPart();
        ResetParams();
    }

    private void ResetParams()
    {
        SetMat(0);
        SetColor(colorSrc);
        SetTiling(Vector2.one);
    }

    private void FixedUpdate()
    {
        if (isSwing  && seesawTrans)
        {
            RotateVertical(curSpeed * Time.fixedDeltaTime);
            angleZ = CheckAngle(seesawTrans.eulerAngles.z);

            // 衰减最大/最小角度
            curAngleMax -= 2 * Time.fixedDeltaTime;
            curAngleMax = Mathf.Clamp(curAngleMax, 0, defaultAngleMax);
            curAngleMin += 2 * Time.fixedDeltaTime;
            curAngleMin = Mathf.Clamp(curAngleMin, defaultAngleMin, 0);

            // 速度衰减
            if (angleZ > 0)
            {
                dir = -1;

                if (angleZ >= MOVE_THRESHOLD && beforeAngle < MOVE_THRESHOLD)
                {
                    AKSoundManager.Inst.PlaySeesawSound("Play_Seesaw_Swing_Up", gameObject);
                }
            }
            else if (angleZ < 0)
            {
                dir = 1;
                if (Math.Abs(angleZ) >= MOVE_THRESHOLD && Math.Abs(beforeAngle) < MOVE_THRESHOLD)
                {
                    AKSoundManager.Inst.PlaySeesawSound("Play_Seesaw_Swing_Down", gameObject);
                }
            }
            curSpeed += dir * angular * Time.fixedDeltaTime;
            curSpeed = Mathf.Clamp(curSpeed, -maxSpeed, maxSpeed);

            // 跷跷板停止
            if (curAngleMin >= MOVE_THRESHOLD || curAngleMax <= MOVE_THRESHOLD)
            {
                curAngleMin = 0;
                curAngleMax = 0;
                ResetSeesaw();
            }

            if (Math.Abs(angleZ) > curAngleMax)
            {
                curSpeed = -curSpeed;
            }
            beforeAngle = angleZ;
        }
    }


    // 旋转校正
    private void RotateVertical(float angle)
    {
        // 获取当前角度
        Vector3 currentAngle = seesawTrans.localRotation.eulerAngles;
        float curAngleZ = currentAngle.z > 180f ? currentAngle.z - 360f : currentAngle.z;
        float targetAngle = curAngleZ + angle;
        targetAngle = Mathf.Clamp(targetAngle, curAngleMin, curAngleMax);
        float angleToRotate = targetAngle - curAngleZ;
        // 进行旋转
        seesawTrans.Rotate(seesawTrans.forward, angleToRotate, Space.World);
    }

    // 获取跷跷板正确的角度
    public float CheckAngle(float value)
    {
        float angle = value - 180;
        if (angle > 0)
            return angle - 180;
        return angle + 180;
    }


    //重置跷跷板状态
    public void ResetSeesaw()
    {
        curSpeed = 0;
        isSwing = false;
        curAngleMax = defaultAngleMax;
        curAngleMin = defaultAngleMin;
        angleZ = 0;
        if (seesawTrans)
        {
            var rot = seesawTrans.localEulerAngles;
            seesawTrans.localEulerAngles = new Vector3(rot.x, rot.y, 0);
        }
    }

    //下压跷跷板
    public void PushSeesaw(bool isRight)
    {
        if (seesawTrans == null)
        {
            seesawTrans = transform.parent;
        }
        if (isRight)
        {
            dir = -1;
        }
        else
        {
            dir = 1;
        }

        curSpeed = dir * deltaSpeed;
        curSpeed = Mathf.Clamp(curSpeed, -maxSpeed, maxSpeed);
        isSwing = true;
        curAngleMax = defaultAngleMax;
        curAngleMin = defaultAngleMin;
    }

    // 更新跷跷板状态
    public void RefreshSeesawState(SeesawSendData data)
    {
        curSpeed = data.speed;
        curAngleMin = data.minAngle;
        curAngleMax = data.maxAngle;
        if (seesawTrans == null)
        {
            seesawTrans = transform.parent;
        }
        if(seesawTrans)
        {
            var rot =  seesawTrans.localRotation.eulerAngles;
            seesawTrans.eulerAngles = new Vector3(rot.x, rot.y, data.angle);
            // seesawTrans.Rotate(seesawTrans.forward, data.angle, Space.World);
        }
        isSwing = true;
    }


    public Transform FindPlayerOnSeesaw(string playerId)
    {
        if (carryTranL)
        {
            var playerData = carryTranL.GetComponentInChildren<PlayerData>(true);
            if (playerData != null)
            {
                if (playerData.syncPlayerInfo.uid == playerId)
                {
                    return carryTranL.GetChild(0);
                }
            }
        }

        if(carryTranR)
        {
            var playerData = carryTranR.GetComponentInChildren<PlayerData>(true);
            if (playerData != null)
            {
                if (playerData.syncPlayerInfo.uid == playerId)
                {
                    return carryTranR.GetChild(0);
                }
            }
        }
        
        return null;
    }

    public Transform FindPlayerOnSeesaw(bool isRight)
    {
        Transform carryNode = null;
        if (isRight)
        {
            carryNode = carryTranR;
        }else
        {
            carryNode = carryTranL;
        }

        if (carryNode)
        {
            var playerData = carryNode.GetComponentInChildren<PlayerData>(true);
            if (playerData != null)
            {
                return carryNode.GetChild(0);  
            }
        }

        return null;
    }

    public void InitSymmetry()
    {
        Transform rightSeat = FindSeat(1);
        if (rightSeat.gameObject.GetComponent<SymmetrySeat>() != null)
        {
            return;
        }
        Transform leftSeat = FindSeat(0);
        SymmetrySeat symmetrySeat = rightSeat.gameObject.AddComponent<SymmetrySeat>();
        if (!symmetrySeat.enabled)
        {
            symmetrySeat.enabled = true;
        }
        symmetrySeat.Init(leftSeat);
        bool symmetry = transform.parent.GetComponent<NodeBaseBehaviour>().entity.Get<SeesawComponent>().symmetry == 1;
        symmetrySeat.SetActive(symmetry);
    }

    public string GetLeftSeatUgcId()
    {
        Transform findSeat = FindSeat(0);
        return findSeat.GetComponent<NodeBaseBehaviour>().entity.Get<SeesawSeatComponent>().rId;
    }

    public string GetRightSeatUgcId()
    {
        Transform findSeat = FindSeat(1);
        return findSeat.GetComponent<NodeBaseBehaviour>().entity.Get<SeesawSeatComponent>().rId;
    }

    public void ChangeSeatOrigin(bool left,bool isSymmetry)
    {
        SeesawSeatBehaviour seat = SceneBuilder.Inst.CreateSceneNode<SeesawSeatCreater, SeesawSeatBehaviour>();
        if (!left)
        {
            ChangeSeat(seat,1,SeesawManager.SEAT_DEFAULT,isSymmetry);
        }
        else
        {
            ChangeSeat(seat,0,SeesawManager.SEAT_DEFAULT,isSymmetry);
        }
    }
}
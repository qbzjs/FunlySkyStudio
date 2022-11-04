/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/8/31 13:14:6
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarryNodeInfo
{
    public Transform carryTran;
    public bool isCarry;
}
public class LadderBehaviour : NodeBaseBehaviour
{
    private bool canNotOnLadder;//用于该梯子下梯子后的冷却判断
    private Color[] colors;
    public List<CarryNodeInfo> carryTrans = new List<CarryNodeInfo>();
    public BoxCollider boxCollider;
    public GameObject model;
    public Renderer[] renderers;
    public Vector3 moveOnLadderSpeed = new Vector3(0, 0.05f, 0);
    private Transform point_1;//监测点，初始状态为上方
    private Transform point_2;//监测点，初始状态为下方
    private float UpOutPosOffest = 1f;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        // LadderManager.Inst.AddLadder(GetHashCode(), this);
        boxCollider = GetComponentInChildren<BoxCollider>();
        model = transform.Find("ladder").gameObject;
        renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        point_1 = transform.Find("point_1");
        point_2 = transform.Find("point_2");
    }
    //public override void OnRayEnter()
    //{
    //    if (StateManager.PlayerOnCar
    //        || (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
    //        || StateManager.IsOnSeesaw)
    //    {
    //        return;
    //    }
    //    if (StateManager.IsOnSlide)
    //    {
    //        return;
    //    }
    //    if (PlayModePanel.Instance)
    //    {
   //      PlayModePanel.Instance.SetLadderModeShow();
    //        PlayModePanel.Instance.ladderPanel.SetOnLadderBtnAction(PlayerOnLadder);
    //    }
    //}
    public override void OnColliderHit()
    {
        Debug.Log("OnColliderHit");
        if (StateManager.PlayerOnCar|| canNotOnLadder
            || (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
            || StateManager.IsOnSeesaw || StateManager.IsOnSwing||StateManager.IsOnLadder)
        {
            return;
        }
        if (StateManager.IsOnSlide)
        {
            return;
        }
       
        if (PlayerIsForward() && PlayerLookAtLadder())
        {
            PlayerOnLadder();
        }

    }
    public bool PlayerIsForward()
    {
        if (PlayerBaseControl.Inst != null)
        {
            Vector3 direction = PlayerBaseControl.Inst.transform.position - transform.position;
            //点积运算
            var dot = Vector3.Dot(direction.normalized, transform.forward);
     
            return dot < 0;
        }
        return false;
    }
    public bool PlayerLookAtLadder()
    {
        if (PlayerBaseControl.Inst != null)
        {
            return Vector3.Angle(PlayerBaseControl.Inst.animCon.playerModle.transform.forward, transform.forward)<=70;
        }
        return false;
    }
    public void SetHideModel(bool isHide)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetFloat("_Opacity", isHide ? 0.5f : 1);
        }
        entity.Get<LadderComponent>().active = isHide ? 1 : 0;
        boxCollider.isTrigger = entity.Get<LadderComponent>().active == 1;

    }
    public void SetColor(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetColor("_Color", color);
        }
        entity.Get<LadderComponent>().color = DataUtils.ColorToString(color);

    }
    public void SetMatetial(int id)
    {

        var matData = GameManager.Inst.matConfigDatas.Find(x => x.id == id);
        Texture t = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + matData.texName);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetTexture("_MainTex", t);
        }
        entity.Get<LadderComponent>().mat = matData.id;
    }
    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref colors);
    }
    public void PlayerOnLadder()
    {

        // 牵手中，不能和梯子交互
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("Please finish interactive emote first.");
            return;
        }
        //对局准备过程中不能与梯子交互
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && (!PVPWaitAreaManager.Inst.IsPVPGameStart || PVPWaitAreaManager.Inst.IsSelfDeath))
        {
            TipPanel.ShowToast("Please wait for game mode to start.");
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
        //冻结状态不允许上梯子
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (StateManager.IsParachuteUsing)
        {
            TipPanel.ShowToast("Please finish landing first.");
            return;

        }
        //摆摊
        var promoteCom = PlayerBaseControl.Inst.transform.GetComponent<PlayerPromoteController>();
        if (promoteCom!=null&&promoteCom.Status != PromoteStatus.None)
        {
            return;
        }
        if (StateManager.IsInSelfieMode)
        {
            return;
        }
        if (StateManager.Inst.IsHodingFishingRod())
        {
            return;
        }
        SwordManager.Inst.forceInterrupt();
        MessageHelper.Broadcast(MessageName.PosMove, false);
        LadderManager.Inst.PlayerSendOnLadder(GetHashCode());

    }
    public LadderInfo GetFreeCarryNodeInfo(Transform player)
    {
        for (int i = 0; i < carryTrans.Count; i++)
        {
            if (!carryTrans[i].isCarry)
            {
                var carryNode = carryTrans[i].carryTran;
                carryTrans[i].isCarry = true;
                return SetNodePosAndRot(carryNode, player);
            }
        }
        return CreateCarryNode(player);
    }
    public LadderInfo CreateCarryNode(Transform player)
    {
        GameObject point = new GameObject("point");
        Transform carryNode = point.transform;
        CarryNodeInfo carryNodeInfo = new CarryNodeInfo()
        {
            carryTran = carryNode,
            isCarry = true,
        };
        carryTrans.Add(carryNodeInfo);
        carryNode.SetParent(transform);


        return SetNodePosAndRot(carryNode, player);
    }
    public LadderInfo SetNodePosAndRot(Transform carryNode, Transform player)
    {

        bool isTurn = false;
        bool isAbove = false;
        carryNode.localRotation = Quaternion.identity;
        carryNode.position = player.position;

        if (carryNode.position.y > point_1.position.y && carryNode.position.y > point_2.position.y)
        {
            isAbove = true;
        }
        if (point_1.position.y < point_2.position.y)
        {
            carryNode.localEulerAngles = new Vector3(0, 0, 180);
            isTurn = true;
        }
        if (carryNode.localPosition.y > point_1.localPosition.y)
        {
            carryNode.localPosition = new Vector3(0, point_1.localPosition.y, -0.4f);
        }
        else if (carryNode.localPosition.y < point_2.localPosition.y)
        {
            carryNode.localPosition = new Vector3(0, point_2.localPosition.y, -0.4f);
        }
        else
        {
            carryNode.localPosition = new Vector3(0, carryNode.localPosition.y, -0.4f);
        }
        carryNode.localScale = Vector3.one;

        LadderInfo info = new LadderInfo();
        info.ladderBehaviour = this;
        info.carryTrans = carryNode;
        info.isTurn = isTurn;
        info.isAboveLadder = isAbove;
        return info;
    }
    public void OnLadderSuccess()
    {
        SetCurStatu(true);

    }
    public void DownLadderSuccess(Transform carryTran,bool isPlayer)
    {
        if (isPlayer)
        {
            SetCurStatu(false);
            canNotOnLadder = true;
            Invoke("SetLadderCD",0.5f);
        }
        SetFreeNode(carryTran);
        isDowning = false;
    }
    public void SetLadderCD(){
        canNotOnLadder = false;
    }
    public void OnModeChange(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            if (HasCurCarryNode())
            {
                LadderManager.Inst.PlayerDownLadder();
                SetCurStatu(false);
                FreeAllNode();
            }
            model.SetActive(true);

        }
        else
        {
            model.SetActive(entity.Get<LadderComponent>().active == 0);
        }

    }
    public void SetFreeNode(Transform carryTran)
    {
        for (int i = 0; i < carryTrans.Count; i++)
        {
            if (carryTrans[i].carryTran == carryTran)
            {
                carryTrans[i].isCarry = false;
            }
        }
    }
    public void FreeAllNode()
    {
        for (int i = 0; i < carryTrans.Count; i++)
        {
            carryTrans[i].isCarry = false;
        }
    }
    public bool HasCurCarryNode()
    {
        for (int i = 0; i < carryTrans.Count; i++)
        {
            if (carryTrans[i].isCarry == true)
            {
                return true;
            }
        }
        return false;
    }
    //正在请求下梯子，处理反复请求
    private bool isDowning;
    private float deTime = 0.033f;
    public void SetPlayerCarryNodeMove(LadderManager.OnLadderMoveStatus status, LadderInfo carryNodeInfo)
    {
        //冻结状态不允许
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
       
        var speed = moveOnLadderSpeed / transform.localScale.y* (Time.deltaTime / deTime);
        if (carryNodeInfo.isTurn)
        {
            speed = -speed;
        }
        if (status == LadderManager.OnLadderMoveStatus.Up)
        {
            carryNodeInfo.carryTrans.localPosition += speed;
        }
        else if (status == LadderManager.OnLadderMoveStatus.Down)
        {
            carryNodeInfo.carryTrans.localPosition -= speed;
        }
        if (!isDowning)
        {
            if (!carryNodeInfo.isTurn)
            {
                if (carryNodeInfo.carryTrans.localPosition.y > point_1.localPosition.y + UpOutPosOffest / transform.localScale.y)
                {
                    StartCoroutine(PlayerMveDownLadder(true));
                }
                else if (carryNodeInfo.carryTrans.localPosition.y < point_2.localPosition.y)
                {
                    StartCoroutine(PlayerMveDownLadder(false));
                }
            }
            else
            {
                if (carryNodeInfo.carryTrans.localPosition.y < point_2.localPosition.y - UpOutPosOffest / transform.localScale.y)
                {
                    StartCoroutine(PlayerMveDownLadder(true));
                }
                else if (carryNodeInfo.carryTrans.localPosition.y > point_1.localPosition.y)
                {
                    StartCoroutine(PlayerMveDownLadder(false));
                }
            }
        }
    }
    public void SetTiling(Vector2 tiling)
    {
        if (renderers.Length <= 0)
        {
            return;
        }
    
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetVector("_MainTex_tilling", new Vector4(1, tiling.y, 0, 0));
        }
        entity.Get<LadderComponent>().tile = tiling;
    }
    public Vector3 SetOffsetNodePos(LadderInfo info)
    {
     
        return info.carryTrans.localPosition + new Vector3(0, (info.isTurn ? 1 : -1) * GameConsts.PlayerNodeHigh / transform.localScale.y+ GameConsts.PlayerNodeHigh, 0);
    }
    IEnumerator PlayerMveDownLadder(bool isDownLadderAbove)
    {
        yield return new WaitForEndOfFrame();
        isDowning = true;
        LadderManager.Inst.PlayerSendDownLadder(isDownLadderAbove);
       
    }
    public void SetCurStatu(bool isFull)
    {
        boxCollider.enabled = !isFull;
    }
    public void OnDisable()
    {   
        canNotOnLadder = false;
        SetCurStatu(false);
        if (LadderManager.Inst!=null)
        {
            LadderManager.Inst.OnLadderDisable(this, entity.Get<GameObjectComponent>().uid);
        }
    }
    public override void OnReset()
    {   
        base.OnReset();
    }
    public float GetRayDis(Vector3 pos)
    {
        var localPos = transform.InverseTransformPoint(pos);
        Vector3 newPos = new Vector3();
        if (localPos.y<=point_1.localPosition.y&&localPos.y>=point_2.localPosition.y)
        {
            newPos = new Vector3(0, localPos.y, 0);
        }
        else
        {
            newPos = localPos.y > point_1.localPosition.y ? point_1.localPosition : point_2.localPosition;
        }
        return Vector3.Distance(localPos, newPos);
    }
}

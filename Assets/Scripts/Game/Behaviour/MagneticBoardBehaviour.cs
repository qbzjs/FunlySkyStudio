/// <summary>
/// Author:zhouzihan
/// Description:磁力版
/// Date: #CreateTime#
/// </summary>
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MagneticBoardBehaviour : NodeBaseBehaviour
{
    private Color[] colors;
    public MeshRenderer[] meshRenderers;
    public BoxCollider boxCollider;
    public Transform carryTran;
    private bool isFull;
   
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
       
        MagneticBoardManager.Inst.AddBoard(GetHashCode(), this);
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        boxCollider = GetComponentInChildren<BoxCollider>();
        carryTran = transform.Find("carryNode");
        InitBroad();
    }

    public override void OnRayEnter()
    {
        if (!isFull)
        {
            PortalPlayPanel.Show();
            PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.MagneticBoard);
            PortalPlayPanel.Instance.AddButtonClick(PlayerOnBoard,true);
            PortalPlayPanel.Instance.SetTransform(transform);
            PortalPlayPanel.Instance.SetCurTargetId(entity.Get<GameObjectComponent>().uid.ToString());
        }
    
    }
    public override void OnRayExit()
    {
        if (!isFull)
        {
            PortalPlayPanel.Hide();
        }
    }
    public override void OnReset()
    {
        base.OnReset();
        MagneticBoardManager.Inst.RemoveBoard(this);
    }
    public Tween jumptween;
    public void JumpOnBoard()
    {
        if (jumptween != null)
        {
            return;
        }
        float defY = carryTran.localPosition.y;
        jumptween = carryTran.DOLocalMoveY(defY + 1, 0.4f).SetEase(Ease.OutSine);
        jumptween.onComplete += () => {
            if (transform != null)
            {
                jumptween = carryTran.DOLocalMoveY(defY, 0.4f).SetEase(Ease.InSine);
                jumptween.onComplete += () => {
                    jumptween = null;
                    MagneticBoardManager.Inst.LandOnBoard(carryTran);
                };

            }
        };
    }



    public void PlayerOnBoard()
    {
        // 牵手中，不能和磁力板交互
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("You could not interact with adhesive surface while Hand-in-hand");
            return;
        }
        //对局准备过程中不能与磁力板交互
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && (!PVPWaitAreaManager.Inst.IsPVPGameStart ||PVPWaitAreaManager.Inst.IsSelfDeath))
        {
            TipPanel.ShowToast("You could not interact with adhesive surface in Waiting Zone");
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
        //冻结状态不允许上磁力板
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        MagneticBoardManager.Inst.PlayerSendOnBoard(GetHashCode());
        PortalPlayPanel.Hide();
    }
    public void OnBoardSuccess()
    {
        SetCurStatu(true);
        ReSetCarryNode();
        if (PortalPlayPanel.Instance != null
            && PortalPlayPanel.Instance.gameObject.activeSelf
            && PortalPlayPanel.Instance.GetCurTargetId() == entity.Get<GameObjectComponent>().uid.ToString())
        {
            PortalPlayPanel.Hide();
        }
    }
    public void DownBoardSuccess()
    {
        SetCurStatu(false);
        ReSetCarryNode();

    }
    public void SetCurStatu(bool isFull)
    {
        this.isFull = isFull;
        boxCollider.enabled = !isFull;
    }

    public void OnModeChange(GameMode mode)
    {
       
        if (mode == GameMode.Edit)
        {
            if (isFull)
            {
                MagneticBoardManager.Inst.PlayerDownBoard();
                DownBoardSuccess();
            }
            foreach (var item in meshRenderers)
            {
                item.enabled = true;
            }
        }
        else
        {
            foreach (var item in meshRenderers)
            {
                item.enabled = false;
            }
        }
    }

    public void OnRestart()
    {
        if (isFull)
        {
            DownBoardSuccess();
        }
    }
    private void InitBroad()
    {
        foreach (var item in meshRenderers)
        {
            item.enabled = true;
        }
    }
    public void ReSetCarryNode()
    {
        carryTran.localRotation = Quaternion.identity;
        carryTran.localPosition = Vector3.zero;
        carryTran.DOKill();
        jumptween = null;
    }
    public void SetPlayerNode()
    {
        carryTran.localPosition = new Vector3(0, GameConsts.PlayerNodeHigh, 0);
    }
    public void OnDisable()
    {
        SetCurStatu(false);
        if (MagneticBoardManager.IsInit())
        {
            MagneticBoardManager.Inst.OnBoardDisable(carryTran, entity.Get<GameObjectComponent>().uid);
        }
        
        
    }
    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject,ref colors);
    }
}

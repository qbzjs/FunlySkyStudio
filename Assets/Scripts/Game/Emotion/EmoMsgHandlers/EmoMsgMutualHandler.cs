using BudEngine.NetEngine;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description: 双人表情动作广播处理 原双人动作@xiongzhao
/// Date: 2022-7-7 18:17:46
/// </summary>
public class EmoMsgMutualHandler : EmoMsgHandlerBase
{
    public EmoMsgMutualHandler()
    {
    }

    public EmoMsgMutualHandler(bool isSelf, IPlayerController pCtrl, PlayerBaseControl pBase, Item iData, EmoItemData emoItemData, AnimationController animCtrl, TextChatBehaviour textChatBev)
        : base(isSelf, pCtrl, pBase, iData, emoItemData, animCtrl, textChatBev)
    {
    }

    public override bool OnEmoOptRelease()
    {
        //双人动作发起客户端直接表现，所以回包时只用处理其他人的
        if (IsSelf == false && animCon != null)
        {
            animCon.PlayAnim(itemData.id, playEmoData.random);
        }

        return true;
    }

    public override bool OnEmoOptCancel()
    {
        if (IsSelf == false && animCon != null)
        {
            animCon.RecStopLoop();
        }
        CleanCurEmoData();

        return true;
    }

    public override bool OnEmoOptMutualFin()
    {
        if (IsSelf)
        {
            /********************************收到自己点别人的交互按钮的广播*******************************/

            if (animCon.isInteracting)
            {
                TipPanel.ShowToast("You have already interacted with someone!");
                return false;
            }

            if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
            {
                TipPanel.ShowToast("You could not interacted with others when locked with adhesive surface!");
                return false;
            }
            if (StateManager.IsOnLadder)
            {
                LadderManager.Inst.ShowTips();
                return false;
            }

            if (StateManager.IsOnSeesaw)
            {
                SeesawManager.Inst.ShowSeesawMutexToast();
                return false;
            }
            
            if (StateManager.IsOnSwing)
            {
                SwingManager.Inst.ShowSwingMutexToast();
                return false;
            }
           
            if (StateManager.IsOnSlide)
            {
                return false;
            }
            var otherPlayerCon = ClientManager.Inst.GetOtherPlayerComById(playEmoData.startPlayerId);
            if (otherPlayerCon == null)
            {
                return false;
            }

            if (!otherPlayerCon.GetEmoInteractState())
            {
                var stPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(playEmoData.startPlayerId);
                var stPlayerName = stPlayerInfo == null ? " " : stPlayerInfo.userName;
                TipPanel.ShowToast("{0} has cancelled or interacted with other players!", stPlayerName);
                return false;
            }

            JumpAndPlayAnimSelf(otherPlayerCon, itemData.id, playEmoData);
            
        }
        else
        {
            /********************************收到其他人点击交互按钮做双人动作的广播*******************************/

            if (animCon.isInteracting)
            {
                return false;
            }

            if (MagneticBoardManager.Inst.IsOtherPlayerOnBoard(playerCtrl as OtherPlayerCtr))
            {
                return false;
            }

            if (SeesawManager.Inst.IsOtherPlayerOnSeesaw(playerCtrl as OtherPlayerCtr))
            {
                return false;
            }

            var iPlayerController = playEmoData.startPlayerId == Player.Id ? (IPlayerController) PlayerEmojiControl.Inst : ClientManager.Inst.GetOtherPlayerComById(playEmoData.startPlayerId);
            if (iPlayerController == null || !iPlayerController.GetEmoInteractState())
            {
                return false;
            }

            var info = MoveClipInfo.GetMutualFinAnim(itemData.id);
            if (info == null)
            {
                LoggerUtils.LogError("Oth Play Anim Failed : MutualFinEmoData == null");
                return false;
            }

            playEmoData.followPlayerId = animCon.GetComponent<PlayerData>().playerInfo.Id;
            //其他玩家自身完成双人动作
            animCon.PlayMutualFinAnim(itemData.id, info.finEndName, playEmoData);
            var stAnimController = ClientManager.Inst.GetAnimControllerById(playEmoData.startPlayerId);
            if (stAnimController)
            {
                //完成双人动作，需要对方也完成
                stAnimController.PlayMutualFinAnim(itemData.id, info.strEndName, playEmoData);
                //完成双人动作，位置瞬闪到面前
                var stTF = stAnimController.GetComponentInChildren<TextChatBehaviour>().transform;
                Vector3 pos = stTF.position + stTF.TransformDirection(DataUtils.DeSerializeVector3(info.interactPos));
                Quaternion rot = Quaternion.LookRotation(-stTF.forward);
                rot = Quaternion.Euler(DataUtils.DeSerializeVector3(info.interactRot)) * rot;
                textCharBev.transform.SetPositionAndRotation(pos, rot);
            }

            if (playEmoData.startPlayerId == Player.Id)
            {
                //完成双人动作，发起者为自身，也需要隐藏交互按钮
                if (PortalPlayPanel.Instance != null)
                {
                    PortalPlayPanel.Instance.SetPlayBtnVisible(false);
                }

                stAnimController.AnimFinCallBack = InteractAnimFinCallBackOther;
            }
        }

        return true;
    }

    #region 双人动作交互执行

    private void InteractAnimFinCallBackOther()
    {
        if (PortalPlayPanel.Instance != null)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(true);
        }
    }

    private void JumpAndPlayAnimSelf(OtherPlayerCtr otherCon, int itemId, EmoItemData emoItemData)
    {
        var info = MoveClipInfo.GetMutualFinAnim(itemId);
        if (info == null)
        {
            LoggerUtils.LogError("Play Anim Failed : MutualFinEmoData == null");
            return;
        }

        Vector3 pos = otherCon.transform.position + otherCon.transform.TransformDirection(DataUtils.DeSerializeVector3(info.interactPos));
        pos.y += playerBase.transform.position.y - textCharBev.transform.position.y;
        Quaternion rot = Quaternion.LookRotation(-otherCon.transform.forward);
        rot = Quaternion.Euler(DataUtils.DeSerializeVector3(info.interactRot)) * rot;
        //rot *= transform.rotation;
        //SetPlayerPositionAndRotation(pos, rot);
        playerBase.SetPlayerPositionAndRotation(pos, playerBase.transform.rotation);
        textCharBev.transform.rotation = rot;

        emoItemData.followPlayerId = emoItemData.startPlayerId == Player.Id
            ? otherCon.GetComponent<PlayerData>().playerInfo.Id
            : Player.Id;
        animCon.PlayMutualFinAnim(itemId, info.finEndName, emoItemData);
        otherCon.animCon.PlayMutualFinAnim(itemId, info.strEndName, emoItemData);

        //other special handle
        InteractAnimHandle(true);
        animCon.AnimFinCallBack = InteractAnimFinCallBack;
    }

    private void InteractAnimFinCallBack()
    {
        InteractAnimHandle(false);
    }

    private void InteractAnimHandle(bool isStart)
    {
        PlayerTriggerController.Inst.SetTriggerActive(!isStart);
        if (PortalPlayPanel.Instance != null)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(!isStart);
        }

        playerBase.waitPosChange = isStart || (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard) || // 在磁力板上结束动作不能复位
            (PlayerLadderControl.Inst && PlayerLadderControl.Inst.isOnLadder || StateManager.IsOnSeesaw || StateManager.IsOnSwing); // 在梯子上结束动作不能复位

        if (isStart)
        {
            playerBase.isMoving = false;
            playerBase.PlayAnimation(AnimId.IsMoving, false);
            playerBase.moveVec = Vector3.zero;
            if (playerBase.mAnimStateManager != null)
            {
                playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.Idle);
            }
        }
        else
        {
            playerBase.ResetUpwardVec();
        }
    }

    #endregion
}
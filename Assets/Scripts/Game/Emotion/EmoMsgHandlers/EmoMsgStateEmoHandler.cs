using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;
using static EmoMenuPanel;

/// <summary>
/// Author:Shaocheng
/// Description: 状态表情动作(如加好友Emo)的广播处理
/// Date: 2022-7-7 18:19:49
/// </summary>
public class EmoMsgStateEmoHandler : EmoMsgHandlerBase
{
    public const int PLAYER_NICKNAME_LIMIT = 10;
    public EmoMsgStateEmoHandler()
    {
    }

    public EmoMsgStateEmoHandler(bool isSelf, IPlayerController pCtrl, PlayerBaseControl pBase, Item iData, EmoItemData emoItemData, AnimationController animCtrl, TextChatBehaviour textChatBev)
        : base(isSelf, pCtrl, pBase, iData, emoItemData, animCtrl, textChatBev)
    {

    }


    private void ClickCancelStateEmo()
    {
        PlayerEmojiControl.Inst.CancelStateEmo(itemData.id);
    }


    public override bool OnEmoOptRelease()
    {
        //已经在状态中
        if (animCon.curStateEmo != EmoName.None)
        {
            return false;
        }

        if (IsSelf)
        {
            //收到状态表情发起广播，进入某种状态，显示 UI
            StateEmoPanel.Show();
            //StateEmoPanel.Instance.SetIconHide();
            StateEmoPanel.Instance.SetIcon((EmoName)itemData.id);
            StateEmoPanel.Instance.SetCancelStateBtnClick(ClickCancelStateEmo);
            StateEmoPanel.Instance.SetIsTps(playerBase.isTps);
            //隐藏拾取按钮
            if (CatchPanel.Instance)
            {
                CatchPanel.Hide();
            }
            playerBase.AddPlayerStateChangedObserver(PlayerStateChanged);

            //如果不是StateEmo。退出之前的动画
            if (PlayerEmojiControl.Inst.mCurEmoData!=null
                && PlayerEmojiControl.Inst.mCurEmoData.emoType != (int)EmoTypeEnum.STATE_EMO)
            {
                animCon?.StopLoop();
            }
            PlayerEmojiControl.Inst.SetCurEmoData(itemData.id);
        }
        else
        {
            //其他玩家显示头顶状态表情icon
            EmoMsgManager.Inst.SetOtherPlayerTouchable(playEmoData.startPlayerId, itemData.id, true);
        }

        animCon.SetStateEmo((EmoName)itemData.id);

        return true;
    }
    public void PlayerStateChanged(IPlayerCtrlMgr playerController)
    {
        //如果是第三人称则显示头顶加好友，如果是第一人称则不显示AddFriend图标s
        if (StateEmoPanel.Instance != null)
        {
            StateEmoPanel.Instance.SetIsTps(playerBase.isTps);
        }
    }

    public override bool OnEmoOptCancel()
    {
        if (animCon.curStateEmo == EmoName.None)
        {
            return false;
        }

        if (IsSelf)
        {
            if (StateEmoPanel.Instance != null)
            {
                StateEmoPanel.Hide();
                playerBase.RemovePlayerStateChangedObserver(PlayerStateChanged);
            }
            //清理当前的Emo数据,如果已经被替换则不处理，
            CleanCurEmoData();
        }
        else
        {
            EmoMsgManager.Inst.SetOtherPlayerTouchable(playEmoData.startPlayerId, itemData.id, false);
        }

        animCon.SetStateEmo(EmoName.None);

        return true;
    }
   
    public override bool OnEmoOptMutualFin()
    {
        LoggerUtils.Log("EmoMsgStateEmoHandler: OnEmoOptMutualFin " + IsSelf);
        //if (playEmoData.extraData)
        //{
        //    LoggerUtils.Log("StateEmoHandler: OnEmoOptMutualFin data is null!!! Pls contact server");
        //    return false;
        //}
        //EmoItemDataExtra extraData = playEmoData.extraData;
        EmoItemDataExtra extraData = JsonConvert.DeserializeObject<EmoItemDataExtra>(playEmoData.extraData);

            //Tips弹窗处理
        HandleTipNotify(extraData);

        //同一时间收到多个交互请求，服务器进行CD时间判定，客户端根据doAction判断是否要播放动作
        if (extraData.doAction == 1)
        {
            return false;
        }
        if (IsSelf)
        {
            /********************************收到自己点别人的交互按钮的广播*******************************/

            // if (animCon.isInteracting)
            // {
            //     TipPanel.ShowToast("You have already interacted with someone!");
            //     return false;
            // }
            if(extraData.isFriend == 0)
            {
                DataLogUtils.NewUserFriends(playEmoData.followPlayerId, playEmoData.startPlayerId);
            }

            if (IsCanHandleStateEmoMsg() == false)
            {
                return false;
            }

            if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
            {
                // TipPanel.ShowToast("You could not interacted with others when locked with adhesive surface!");
                return false;
            }
            
            if (PlayerLadderControl.Inst && PlayerLadderControl.Inst.isOnLadder)
            {
                // TipPanel.ShowToast("You could not interacted with others when locked with adhesive surface!");
                return false;
            }
            if(StateManager.IsOnSeesaw)
            {
                SeesawManager.Inst.ShowSeesawMutexToast();
                return false;
            }
            
            if(StateManager.IsOnSwing)
            {
                SeesawManager.Inst.ShowSeesawMutexToast();
                return false;
            }

            if (StateManager.IsOnSlide)
            {
                return false;
            }
            //发起者状态判断
            var otherPlayerCon = ClientManager.Inst.GetOtherPlayerComById(playEmoData.startPlayerId);
            if (otherPlayerCon == null)
            {
                return false;
            }

            if (otherPlayerCon.animCon != null && otherPlayerCon.animCon.curStateEmo == EmoName.None)
            {
                return false;
            }

            JumpAndPlayAnimSelf(otherPlayerCon, itemData.id, playEmoData);
           
        }
        else
        {
            /********************************收到其他人点击交互按钮做双人动作的广播*******************************/

            // if (animCon.isInteracting)
            // {
            //     return false;
            // }

            if (MagneticBoardManager.Inst.IsOtherPlayerOnBoard(playerCtrl as OtherPlayerCtr))
            {
                return false;
            }

            if (SeesawManager.Inst.IsOtherPlayerOnSeesaw(playerCtrl as OtherPlayerCtr))
            {
                return false;
            }

            //发起者状态判断
            if (playEmoData.startPlayerId == Player.Id)
            {
                if (PlayerEmojiControl.Inst == null || PlayerEmojiControl.Inst.animCon == null || PlayerEmojiControl.Inst.animCon.curStateEmo == EmoName.None)
                {
                    return false;
                }
            }
            else
            {
                var otherPlayerCtrl = ClientManager.Inst.GetOtherPlayerComById(playEmoData.startPlayerId);
                if (otherPlayerCtrl == null || otherPlayerCtrl.animCon == null || otherPlayerCtrl.animCon.curStateEmo == EmoName.None)
                {
                    return false;
                }
            }


            var info = MoveClipInfo.GetMutualFinAnim(itemData.id);
            if (info == null)
            {
                LoggerUtils.LogError("EmoMsgStateEmoHandler Oth Play Anim Failed : MutualFinEmoData == null");
                return false;
            }

            playEmoData.followPlayerId = animCon.GetComponent<PlayerData>().playerInfo.Id;
            //其他玩家自身完成双人动作
            //todo:其他玩家移动停下
            animCon.PlayStateEmoMutualFinAnim(itemData.id, info.finEndName, playEmoData);
            var stAnimController = ClientManager.Inst.GetAnimControllerById(playEmoData.startPlayerId);
            if (stAnimController)
            {
                //完成双人动作，需要对方也完成
                stAnimController.PlayStateEmoMutualFinAnim(itemData.id, info.strEndName, playEmoData);
                //完成双人动作，位置瞬闪到面前
                var stTF = stAnimController.GetComponentInChildren<TextChatBehaviour>().transform;
                Vector3 pos = stTF.position + stTF.TransformDirection(DataUtils.DeSerializeVector3(info.interactPos));
                Quaternion rot = Quaternion.LookRotation(-stTF.forward);
                rot = Quaternion.Euler(DataUtils.DeSerializeVector3(info.interactRot)) * rot;
                textCharBev.transform.SetPositionAndRotation(pos, rot);
            }
        }

        return true;
    }

    private void HandleTipNotify(EmoItemDataExtra emoItemDataExtra)
    {
        if (itemData.id == (int)EmoName.EMO_ADD_FRIEND)
        {
            if (TipNotifyPanel.Instance && TipNotifyPanel.Instance.gameObject.activeInHierarchy)
            {
                LoggerUtils.Log("EmoMsgStateEmoHandler: TipNotifyPanel already showed!");
                return;
            }

            var showTipPlayer = string.Empty;
            if (playEmoData.startPlayerId == Player.Id)
            {
                //别人点了“我”的按钮
                showTipPlayer = playEmoData.followPlayerId;
            }
            else if (playEmoData.followPlayerId == Player.Id)
            {
                //“我”点了别人的按钮
                showTipPlayer = playEmoData.startPlayerId;
            }

            if (string.IsNullOrEmpty(showTipPlayer))
            {
                return;
            }
            UserInfo syncPlayerInfo = ClientManager.Inst.GetSyncPlayerInfoByBudId(showTipPlayer);
            if (syncPlayerInfo != null)
            {
                //已经是好友，表情发起者不显示tip
                if (emoItemDataExtra.isFriend == 1 && playEmoData.followPlayerId == Player.Id)
                {
                    TipNotifyPanel.ShowToastWithPlayer(showTipPlayer, "You and {0} are already friends.", GameUtils.SetText( syncPlayerInfo.userName, PLAYER_NICKNAME_LIMIT));
                    PlayerEmojiControl.Inst.IsStateEmoTipShow = true;
                    HideSocialNotify();

                    TipNotifyPanel.SetHideCallback(() =>
                    {
                        PlayerEmojiControl.Inst.IsStateEmoTipShow = false;
                    });
                }
                else if (emoItemDataExtra.isFriend == 0)
                {
                    TipNotifyPanel.ShowToastWithPlayer(showTipPlayer, "You and {0} become friends.", GameUtils.SetText(syncPlayerInfo.userName, PLAYER_NICKNAME_LIMIT)); 
                    PlayerEmojiControl.Inst.IsStateEmoTipShow = true;
                    HideSocialNotify();

                    TipNotifyPanel.SetHideCallback(() =>
                    {
                        PlayerEmojiControl.Inst.IsStateEmoTipShow = false;
                    });
                }
            }
        }
    }

    public override bool OnEmoOptInteracting()
    {
        return true;
    }

    //加好友emote弹窗和社交好友模块的弹窗不能同时显示,同时不能打断原SocialNotify的逻辑。所以这里采用移出屏幕外
    private void HideSocialNotify()
    {
        if (SocialNotificationPanel.Instance == null || SocialNotificationPanel.Instance.gameObject == null)
        {
            return;
        }
        var rectTrans = SocialNotificationPanel.Instance.gameObject.transform.GetComponent<RectTransform>();
        if (rectTrans == null)
        {
            return;
        }
        rectTrans.anchoredPosition = PlayerEmojiControl.Inst.v3OutScreen;
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
        animCon.PlayStateEmoMutualFinAnim(itemId, info.finEndName, emoItemData);
        otherCon.animCon.PlayStateEmoMutualFinAnim(itemId, info.strEndName, emoItemData);

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
        // PlayerTriggerController.Inst.SetTriggerActive(!isStart);
        // PortalPlayPanel.Instance.SetPlayBtnVisible(!isStart);
        // MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        var isOnBoard = PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard;
        var isOnLadder = PlayerLadderControl.Inst && PlayerLadderControl.Inst.isOnLadder;
        playerBase.waitPosChange = isStart || isOnBoard|| isOnLadder || StateManager.IsOnSeesaw || StateManager.IsOnSwing; // 在磁力板上结束动作不能复位

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

    private bool IsCanHandleStateEmoMsg()
    {
        // 牵手状态下
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            //TipPanel.ShowToast("You could not interact with Steering wheel while hand-in-hand");
            return false;
        }
        //拾取道具状态下
        if (PlayerControlManager.Inst.isPickedProp)
        {
            TipPanel.ShowToast("You could not interact with Steering wheel while holding object");
            return false;
        }
        //求加好友动画播放中，不可进行操作
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetEmoInteractState())
        {
            //TipPanel.ShowToast("Please quit the add friend emote first");
            return false;
        }
        //方向盘上不能响应
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel != null)
        {
            //TipPanel.ShowToast("You could not interact with other players while using steering wheel");
            return false;
        }
        //磁力版上不能响应
        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
        {
            //TipPanel.ShowToast("You could not interact with other players while adhesive surface");
            return false;
        }
        //梯子上不能响应
        if (PlayerLadderControl.Inst && PlayerLadderControl.Inst.isOnLadder)
        {
            //TipPanel.ShowToast("You could not interact with other players while adhesive surface");
            return false;
        }
        // 跷跷板上不能响应
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
        return true;
    }
    public override bool IsNeedShowInChatWnd()
    {
        if (string.IsNullOrEmpty( playEmoData.extraData))
        {
            return false;
        }
        EmoItemDataExtra extraData = JsonConvert.DeserializeObject<EmoItemDataExtra>(playEmoData.extraData);
        //检查是否重复加好友
        bool isAddFriendRepeat = false;
        if (itemData.id == (int)EmoName.EMO_ADD_FRIEND)
        {
            if (extraData.isFriend == 1)//已经是好友
            {
                isAddFriendRepeat = true;
            }
            else if (extraData.isFriend == 0)//之前不是好友，这次是
            {
                isAddFriendRepeat = false;
            }
        }

        return !isAddFriendRepeat;
    }
}
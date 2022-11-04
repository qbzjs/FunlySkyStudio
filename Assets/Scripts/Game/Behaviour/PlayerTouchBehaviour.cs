using Newtonsoft.Json;
using UnityEngine;
using BudEngine.NetEngine;
using System.Collections.Generic;

/// <summary>
/// Author: 熊昭
/// Description: 人物双人表情交互行为类：控制显示头顶交互按钮，点击事件等
/// Date: 2022-01-16 19:59:55
/// </summary>
public class PlayerTouchBehaviour : NodeBaseBehaviour
{
    public Transform touchPos;
    private int emoId;
    private EmoIconData currentEmo;

    public override void OnRayEnter()
    {
        base.OnRayEnter();
        if (currentEmo == null)
        {
            LoggerUtils.LogError("Current EmoIconData == null");
            return;
        }

        PortalPlayPanel.Show();
        PortalPlayPanel.Instance.SetTransform(touchPos);
        if (IsStateEmo())
        {
            PortalPlayPanel.Instance.AttachPlayBtnEffect("Effect/Please_add_friends/Please_add_friends");
            PortalPlayPanel.Instance.DisablePlayBtnImage();
        }
        else
        {
            PortalPlayPanel.Instance.SetIcon((PortalPlayPanel.IconName)currentEmo.emoPortalIcon);
        }
       
        PortalPlayPanel.Instance.AddButtonClick(OnClickEmo);
    }
    private void OnClickEmo()
    {
        
        if (currentEmo == null)
        {
            LoggerUtils.LogError("Current EmoIconData == null");
            return;
        }

        var type = currentEmo.GetEmoType();
        if (type == EmoType.Mutual)
        {
            if (CanClickNormalMutualBtn())
            {
                //退出当前的状态
                PlayerEmojiControl emojiCtrl = PlayerEmojiControl.Inst;
                if (emojiCtrl.mCurEmoDataNormal != null && emojiCtrl.mCurEmoDataNormal.emoType == (int)EmoMenuPanel.EmoTypeEnum.STATE_EMO)
                {
                    emojiCtrl.CancelStateEmo(emojiCtrl.mCurEmoDataNormal.id);
                }
                OnClickHighFive();
            }
        }
        else if (type == EmoType.LoopMutual)
        {
            OnClickJoinHand();
        }
        else if (type == EmoType.StateEmo)
        {
            if (CanClickStateEmoBtn())
            {
                //取消当前的状态
                PlayerEmojiControl emojiCtrl = PlayerEmojiControl.Inst;
                if (emojiCtrl.mCurEmoDataNormal != null && emojiCtrl.mCurEmoDataNormal.emoType == (int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO)
                {
                    emojiCtrl.animCon.StopLoop();
                }
                OnClickAddFriend();
            }
        }
    }

    private bool CanClickStateEmoBtn()
    {
        if (PlayerEatOrDrinkControl.Inst && PlayerEatOrDrinkControl.Inst.IsEating)
        {
            TipPanel.ShowToast("You could not use other interactive emotes while eating");
            return false;
        }

        //拾取道具状态下不能响应
        if (PlayerControlManager.Inst.isPickedProp)
        {
            TipPanel.ShowToast("You could not interact with other players while holding object");
            return false;
        }

        //方向盘上不能响应
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel != null)
        {
            TipPanel.ShowToast("You could not interact with other players while using steering wheel");
            return false;
        }

        //磁力版上不能响应
        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
        {
            TipPanel.ShowToast("You could not interact with other players while adhesive surface");
            return false;
        }

        if (PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsSnowSkating())
        {
            return false;
        }

        //梯子上不能响应
        if (StateManager.IsOnLadder)
        {
            LadderManager.Inst.ShowTips();
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
        return true;
    }

    private void OnClickHighFive()
    {
        EmoItemData emoItemData = new EmoItemData()
        {
            opt = (int)OptType.MutualFin,
            startPlayerId = GetComponent<PlayerData>().playerInfo.Id,
            followPlayerId = Player.Id
        };
        Item item = new Item()
        {
            id = emoId,
            type = (int)currentEmo.GetEmoType(),
            data = JsonConvert.SerializeObject(emoItemData)
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Emo,
            data = JsonConvert.SerializeObject(item),
        };
        LoggerUtils.Log("OnClickEmo -- Send -- " + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }
    private void OnClickAddFriend()
    {
        EmoItemDataExtra extraData = new EmoItemDataExtra()
        {
            actionProtect = currentEmo.actionProtect,
            doAction = 0,
            lastActionTime = 0,
            isFriend = 0,
            token = GetToken(),
        };
        string extraDataStr = JsonConvert.SerializeObject(extraData);
        EmoItemData emoItemData = new EmoItemData()
        {
            opt = (int)OptType.MutualFin,
            startPlayerId = GetComponent<PlayerData>().playerInfo.Id,
            followPlayerId = Player.Id,
            extraData = extraDataStr,
        };
        Item item = new Item()
        {
            id = emoId,
            type = (int)currentEmo.GetEmoType(),
            data = JsonConvert.SerializeObject(emoItemData)
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Emo,
            data = JsonConvert.SerializeObject(item),
        };
        LoggerUtils.Log("OnClickEmo -- Send -- " + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData), SendAddFriendCallBack);
    }
    private void SendAddFriendCallBack(int code, string content)
    {
       var startPlayerId = GetComponent<PlayerData>().playerInfo.Id;
       var followPlayerId = Player.Id;
       DataLogUtils.NewUserFriends(followPlayerId, startPlayerId);
    }
    private string GetToken()
    {
#if !UNITY_EDITOR
            return HttpUtils.tokenInfo.token;
#else
        return TestNetParams.testHeader.token;
#endif
    }
    /**
    * 是否可以点击响应其他双人交互动作
    */
    private bool CanClickNormalMutualBtn()
    {
        if (PlayerEatOrDrinkControl.Inst && PlayerEatOrDrinkControl.Inst.IsEating)
        {
            TipPanel.ShowToast("You could not use other interactive emotes while eating");
            return false;
        }

        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel != null)
        {
            TipPanel.ShowToast("You could not interact with interactive emote button while driving");
            return false;
        }

        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("You could not respond interactive emote while waiting hand-in-hand");
            return false;
        }

        //拾取道具状态下不能响应双人交互动作
        if (PlayerControlManager.Inst.isPickedProp)
        {
            TipPanel.ShowToast("You could not interact with other players while holding object");
            return false;
        }

        //双人牵手不允许点击加好友emo
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            LoggerUtils.Log("双人牵手不允许进入加好友emo");
            return false;
        }

        if (PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsSnowSkating())
        {
            return false;
        }
        return true;
    }

    private void OnClickJoinHand()
    {
        if (!CanClickJoinHandBtn())
        {
            return;
        }

        var startPlayerId = GetComponent<PlayerData>().playerInfo.Id;
        var followPlayerId = Player.Id;
        EmoItemData emoItemData = new EmoItemData()
        {
            opt = (int)OptType.Interacting,
            startPlayerId = startPlayerId,
            followPlayerId = followPlayerId
        };
        Item item = new Item()
        {
            id = emoId,
            type = (int)currentEmo.GetEmoType(),
            data = JsonConvert.SerializeObject(emoItemData)
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Emo,
            data = JsonConvert.SerializeObject(item),
        };
        LoggerUtils.Log("OnClickJoinHand -- Send -- " + JsonConvert.SerializeObject(roomChatData));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
        //SetCanTouch(false);
    }

    /**
    * 是否可以点击响应双人牵手交互动作
    */
    private bool CanClickJoinHandBtn()
    {
        if (PlayerEatOrDrinkControl.Inst && PlayerEatOrDrinkControl.Inst.IsEating)
        {
            TipPanel.ShowToast("You could not use other interactive emotes while eating");
            return false;
        }

        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("You could not interact with hand-in-hand button while hand-in-hand");
            return false;
        }

        if (PlayerBaseControl.Inst.isFlying)
        {
            TipPanel.ShowToast("You could not interact with hand-in-hand button while flying");
            return false;
        }

        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isWaitingHands)
        {
            TipPanel.ShowToast("You could not interact with hand-in-hand button while waiting hand-in-hand");
            return false;
        }

        //在磁力板上，不能响应牵手
        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
        {
            TipPanel.ShowToast("You could not interact with hand-in-hand button when locked with adhesive surface");
            return false;
        }
        //梯子上不能响应
        if (StateManager.IsOnLadder)
        {
            LadderManager.Inst.ShowTips();
            return false;
        }
        
        //滑梯上不能响应
        if (StateManager.IsOnSlide)
        {
            return false;
        }
        //驾驶方向盘时，不能响应牵手
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
        {
            TipPanel.ShowToast("You could not interact with hand-in-hand button while driving");
            return false;
        }

        //拾取道具状态下不能响应牵手
        if (PlayerControlManager.Inst.isPickedProp)
        {
            TipPanel.ShowToast("You could not interact with other players while holding object");
            return false;
        }

        if (!PlayerBaseControl.Inst.isTps)
        {
            TipPanel.ShowToast("You could not interact with hand-in-hand button in first person perspective");
            return false;
        }

        //加好友状态下无法响应其他双人动作
        if (PlayerEmojiControl.Inst != null && PlayerEmojiControl.Inst.IsInStateEmo())
        {
            return false;
        }

        if (StateManager.IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return false;
        }
        
        //滑雪中不允许交互
        if (PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsSnowSkating())
        {
            return false;
        }
        
        // 在跷跷板上不允许交互
        if(StateManager.IsOnSeesaw)
        {
            SeesawManager.Inst.ShowSeesawMutexToast();
            return false;
        }
        
        if (StateManager.IsOnSwing)
        {
            SwingManager.Inst.ShowSwingMutexToast();
            return false;
        }

        return true;
    }

    public void SetCanTouch(bool state, int id = 0)
    {
        string layer = state ? "Touch" : "OtherPlayer";
        int emo = state ? id : 0;

        gameObject.layer = LayerMask.NameToLayer(layer);
        if (emo != 0 && emoId != emo)
        {
            //切换发起双人动作时，交互按钮的icon需要刷新
            MessageHelper.Broadcast(MessageName.ReleaseTrigger);
        }
        emoId = emo;
        currentEmo = MoveClipInfo.GetAnimName(emoId);
    }

    public override void OnRayExit()
    {
        base.OnRayExit();
        PortalPlayPanel.Hide();
    }

    private bool IsStateEmo()
    {
        return currentEmo != null && (EmoMenuPanel.EmoTypeEnum) currentEmo.emoType == EmoMenuPanel.EmoTypeEnum.STATE_EMO;
    }
}
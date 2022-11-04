using System;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using UnityEngine;

public enum ESlideAction
{
    Down,
    Up,
    Start,//开始滑梯
    End,//结束滑行
}
public enum ESlideState
{
    OutTheSlide,//不在滑梯上
    InTheSlide,//滑梯上
    Slide,//滑行中
}
//玩家上滑梯头尾类型
public enum ESlideInPosType
{
    Head,
    Tail,
}
public class PlayerSlidePipeControl : MonoBehaviour, IPlayerCtrlMgr
{

    public class PlayerItemData
    {
        public string playerId { get; set; }
        public int opType { get; set; }
        public int inPos { get; set; }
    }
    public PlayerBaseControl playerBase;
    public SlidePipeMovementCompt mSlideMoveComponent;
    public FrameStateType CurFrameState = FrameStateType.NoState;
    public int mCurSlidePipeUid = 0;
    public bool mCatchPanelVisibleState = false;
    public bool mEatOrDrinkCtrPanelState = false;
    private void Awake()
    {
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.SlidePipe, this);

        playerBase = PlayerControlManager.Inst.playerBase;
    }
    void FixedUpdate()
    {
        if (mSlideMoveComponent != null)
        {
            mSlideMoveComponent.Update(Time.fixedDeltaTime);
        }
    }
    public void OnClickDownSlide()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            RequestOnSlide(ESlideAction.Down, ESlideInPosType.Head, mCurSlidePipeUid);
        }
        else
        {
            OnDownSlidePipe();
        }
    }
    public void OnClickUpSlidePipe(ESlideInPosType inPos,int uid)
    {
        if (!HandleNewAction())
        {
            return;
        }
        //冻结状态不允许上滑梯
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (GlobalFieldController.CurGameMode==GameMode.Guest&&Global.IsInRoom())
        {
            RequestOnSlide(ESlideAction.Up,inPos,uid);
        }
        else
        {
            OnUpSlidePipe(uid, inPos==ESlideInPosType.Tail);
        }
    }
    public bool HandleNewAction()
    {
        // 牵手中，不能和梯子交互
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("You could not interact with adhesive surface while Hand-in-hand");
            return false;
        }
        //对局准备过程中不能与梯子交互
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && (!PVPWaitAreaManager.Inst.IsPVPGameStart || PVPWaitAreaManager.Inst.IsSelfDeath))
        {
            TipPanel.ShowToast("You could not interact with adhesive surface in Waiting Zone");
            return false;
        }
        //求加好友状态回包前 不可进行操作
        if (EmoMenuPanel.Instance && EmoMenuPanel.Instance.GetIsStateEmoRequesting())
        {
            return false;
        }
        //求加好友状态中，不可进行操作
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetCurStateEmoName() == EmoName.EMO_ADD_FRIEND)
        {
            TipPanel.ShowToast("Please quit the add friend emote first");
            return false;
        }
        //冻结状态不允许上梯子
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return false;
        }
        if (StateManager.IsParachuteUsing)
        {
            return false;

        }
        //摆摊
        var promoteCom = PlayerBaseControl.Inst.transform.GetComponent<PlayerPromoteController>();
        if (promoteCom != null && promoteCom.Status != PromoteStatus.None)
        {
            return false;
        }
        if (StateManager.IsInSelfieMode)
        {
            if (SelfieModeManager.Inst!=null)
            {
                SelfieModeManager.Inst.ShowSelfieModeToast();
            }
            return false;
        }
        //if (StateManager.Inst.IsHodingFishingRod())
        //{
        //    return false;
        //}
        return true;
    }
    public void OnClickStarSlide()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            RequestOnSlide(ESlideAction.Start, ESlideInPosType.Head, mCurSlidePipeUid);
        }
        else
        {
            OnStartSlide();
        }
    }
    //滑行结束，进入Idle。发送一条消息
    public void ReqSlideEnd()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            RequestOnSlide(ESlideAction.End, ESlideInPosType.Head, mCurSlidePipeUid);
        }
    }
    public void RequestOnSlide(ESlideAction actionType, ESlideInPosType inPosType, int uid)
    {
        PlayerItemData itemData = new PlayerItemData();
        itemData.playerId = Player.Id;
        itemData.opType = (int)actionType;
        itemData.inPos = (int)inPosType;
        Item[] itemsArray =
        {
            new Item()
            {
                id = uid,
                type = (int) ItemType.SLIDE_PIPE,
                data = JsonConvert.SerializeObject(itemData),
            }
        };

        SyncItemsReq itemsReq = new SyncItemsReq()
        {
            mapId = GlobalFieldController.CurMapInfo.mapId,
            items = itemsArray,
        };

        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Items,
            data = JsonConvert.SerializeObject(itemsReq),
        };

        string jsonData = JsonConvert.SerializeObject(roomChatData);
        LoggerUtils.Log($"RequestOnSlide ==> =>:{jsonData}");
        ClientManager.Inst.SendRequest(jsonData, CallBack);
    }
    private void CallBack(int code,string msg)
    {
        LoggerUtils.Log($"RequestOnSlide.CallBack ==> =>:{code}      {msg}");
    }
    public void OnUpSlidePipe(int uid,bool isNegDir)
    {
        if (IsOnSlide())
        {
            //不允许重复上滑梯
            return;
        }
        PortalPlayPanel.Hide();
        playerBase.waitPosChange = true;
        playerBase.PlayAnimation(AnimId.IsGround, true);
        playerBase.PlayAnimation(AnimId.IsMoving, false);
        playerBase.isGround = true;
        playerBase.isMoving = false;
        playerBase.character.enabled = false;
        PlayerBaseControl.Inst.SetFly(false, true);
        AudioController.Inst.StopFlyAudio();

        PlayerEmojiControl.Inst.CancelInteractEmo();
        AttackWeaponManager.Inst.SetAttackCtrPanelActive(false);
        ShootWeaponManager.Inst.SetAttackCtrPanelActive(false);
        StateManager.Inst.SetFishingCtrPanelVisibile(false);
        playerBase.PlayerResetIdle();

        SlideControlPanel.Show();
        SlideControlPanel.Instance.SetrSlidePipeCtrl(this);
        CreateMovement(uid,isNegDir);

        PlayModePanel.Instance.SetOnSlidePipeMode(true);

        PlayerBaseControl.Inst.AddNoAbilityFlag(EObjAbilityType.Emo);

        if (CatchPanel.Instance!=null)
        {
            mCatchPanelVisibleState= CatchPanel.Instance.GetButtonVisibleState();
            CatchPanel.Instance.SetButtonVisible(false);
        }
        if (EatOrDrinkCtrPanel.Instance != null)
        {
            mEatOrDrinkCtrPanelState = EatOrDrinkCtrPanel.Instance.GetCtrlPanelVisibleState();
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(false);
        }
    }
    public void OnStartSlide()
    {
        ExcuteMove();
    }
    public void OnDownSlidePipe()
    {
        SlideControlPanel.Hide();
        mSlideMoveComponent.GotoState(ESlidePipeMoveState.End);
    }
    public void OnDownSlidePipeCompleted()
    {
        RecoverPlayerState();
        mSlideMoveComponent?.Exit();
        mSlideMoveComponent = null;
        mCurSlidePipeUid = 0;
    }
    public void ForceAbortSlideAction()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            RequestOnSlide(ESlideAction.Down, ESlideInPosType.Head, mCurSlidePipeUid);
        }
        SlideControlPanel.Hide();
        if (mSlideMoveComponent!=null)
        {
            mSlideMoveComponent.ForceAbort();
            mSlideMoveComponent.Exit();
            mSlideMoveComponent = null;
        }
        mCurSlidePipeUid = 0;
        RecoverPlayerState();
    }
    public void RecoverPlayerState()
    {
        playerBase.animCon.playerAnim.CrossFade("idle", 0.15f, 0);
        playerBase.animCon.ReleaseAndCancelLastEmo();
        playerBase.waitPosChange = false;
        playerBase.character.enabled = true;
        playerBase.transform.rotation = Quaternion.identity;
        playerBase.playerAnim.transform.rotation = Quaternion.identity;
        playerBase.SetLookCenter(Quaternion.identity);
        AttackWeaponManager.Inst.SetAttackCtrPanelActive(true);
        ShootWeaponManager.Inst.SetAttackCtrPanelActive(true);
        StateManager.Inst.SetFishingCtrPanelVisibile(true);
        PlayModePanel.Instance.SetOnSlidePipeMode(false);
        PlayerBaseControl.Inst.RemoveNoAbilityFlag(EObjAbilityType.Emo);

        MessageHelper.Broadcast(MessageName.ReleaseTrigger);

        if (CatchPanel.Instance != null)
        {
            CatchPanel.Instance.SetButtonVisible(mCatchPanelVisibleState);
        }
        if (EatOrDrinkCtrPanel.Instance != null)
        {
            EatOrDrinkCtrPanel.Instance.SetCtrlPanelVisible(mEatOrDrinkCtrPanelState);
        }
    }
    public void CreateMovement(int uid, bool isNegDir)
    {
        SlidePipeBehaviour slidePipe = SlidePipeManager.Inst.GetSlidePipe(uid);
        slidePipe.RefreshWaypointsList(isNegDir);
        mSlideMoveComponent = new SlidePipeMovementCompt(this);
        mSlideMoveComponent.Init();
        mSlideMoveComponent.SetMoveWaypoint(slidePipe.mWaypoints, isNegDir);
        mSlideMoveComponent.GotoState(ESlidePipeMoveState.Start);
        mCurSlidePipeUid = uid;
    }
    public void ExcuteMove()
    {
        if (mSlideMoveComponent != null)
        {
            mSlideMoveComponent.ExcuteMove();
        }
    }
    public bool IsOnSlide()
    {
        return mSlideMoveComponent != null;
    }
}
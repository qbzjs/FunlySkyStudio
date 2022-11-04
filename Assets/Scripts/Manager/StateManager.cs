/// <summary>
/// Author:WeiXin
/// Description:状态判断管理
/// Date: 2022/5/12 18:33:34
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : CInstance<StateManager>
{
    public void Init()
    {
    }

    public bool PVPFeedBack
    {
        get
        {
            bool[] PVPFeedBackList = new bool[]
            {
                PlayerOnCar,
                PlayerIsMutual,
                VideoFullPanelShow,
                StorePanelShow,
                !PlayerIsTps
            };
            bool value = false;
            foreach (var v in PVPFeedBackList)
            {
                value = value || v;
            }

            return value;
        }
    }

    //第三人称视角
    public static bool PlayerIsTps
    {
        get => PlayerBaseControl.Inst && PlayerBaseControl.Inst.isTps;
    }

    //方向盘
    public static bool PlayerOnCar
    {
        get => PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel;
    }

    //牵手
    public static bool PlayerIsMutual
    {
        get => PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual;
    }

    //全屏视频
    public static bool VideoFullPanelShow
    {
        get => VideoFullPanel.Instance && VideoFullPanel.Instance.gameObject.activeSelf;
    }

    //沉浸购买
    public static bool StorePanelShow
    {
        get => StorePanel.Instance && StorePanel.Instance.gameObject.activeSelf;
    }
    //梯子上
    public static bool IsOnLadder
    {
        get => PlayerLadderControl.Inst != null && PlayerLadderControl.Inst.isOnLadder;
    }
    public static bool IsOnSlide
    {
        get
        {
            if (PlayerControlManager.Inst!=null)
            {
                PlayerSlidePipeControl ctrl = PlayerControlManager.Inst.GetPlayerCtrlMgrAs<PlayerSlidePipeControl>(PlayerControlType.SlidePipe);
                if (ctrl != null)
                {
                    return ctrl.IsOnSlide();
                }
            }
            return false;
        }
       
    }
    //回血道具是否可以触发
    public static bool IsBloodTrigger
    {
        get => (SceneParser.Inst.GetHPSet() != 0 && !StateManager.IsPVPWait);
    }
    //在钓鱼
    public static bool IsFishing
    {
        get => PlayerBaseControl.Inst && PlayerBaseControl.Inst.animCon.isFishing;
    }

    //是否是PVP对局准备状态
    public static bool IsPVPWait
    {
        get => PVPWaitAreaManager.Inst.PVPBehaviour != null
        && (!PVPWaitAreaManager.Inst.IsPVPGameStart
        || PVPWaitAreaManager.Inst.IsSelfDeath);
    }

    public static bool IsInSelfieMode
    {
        get => PlayerManager.Inst.selfPlayer.isInSelfieMode;
    }

    //是否在跷跷板上
    public static bool IsOnSeesaw
    {
        get => PlayerOnSeesawControl.Inst != null && PlayerOnSeesawControl.Inst.isOnSeesaw;
    }
    
    public static bool IsOnSwing
    {
        get => PlayerOnSwingControl.Inst != null && PlayerOnSwingControl.Inst.isOnSwing;
    }

    public bool CheckCanSitOnSeesaw()
    {
        // 自拍模式不能和跷跷板交互
        if (StateManager.IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return false;
        }
        // 牵手中，不能和跷跷板交互
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("Please finish interactive emote first.");
            return false;
        }
        //正在做双人动作无法使用跷跷板
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetIsInInteracting())
        {
            TipPanel.ShowToast("Please finish interactive emote first.");
            return false;
        }
        // 求加好友状态中，不能和跷跷板交互
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetCurStateEmoName() == EmoName.EMO_ADD_FRIEND)
        {
            TipPanel.ShowToast("Please finish interactive emote first.");
            return false;
        }

        // 选货或带货中，不能和跷跷板交互
        var promoteCtrl = ClientManager.Inst.GetPlayerPromoteController(GameManager.Inst.ugcUserInfo.uid);
        if (promoteCtrl != null && (promoteCtrl.InSelect || promoteCtrl.InPromote))
        {
            TipPanel.ShowToast("Please quit promoting first.");
            return false;
        }

        //对局准备过程中不能与跷跷板交互
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && (!PVPWaitAreaManager.Inst.IsPVPGameStart || PVPWaitAreaManager.Inst.IsSelfDeath))
        {
            TipPanel.ShowToast("Please wait for game mode to start.");
            return false;
        }

        // 冻结状态下不允许和跷跷板交互
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return false;
        }

        return true;
    }
    
    public bool CheckCanSitOnSwing()
    {
        // 自拍模式不能和跷跷板交互
        if (IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return false;
        }
        // 牵手中，不能和跷跷板交互
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("Please finish interactive emote first.");
            return false;
        }
        //正在做双人动作无法使用跷跷板
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetIsInInteracting())
        {
            TipPanel.ShowToast("Please finish interactive emote first.");
            return false;
        }
        // 求加好友状态中，不能和跷跷板交互
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetCurStateEmoName() == EmoName.EMO_ADD_FRIEND)
        {
            TipPanel.ShowToast("Please finish interactive emote first.");
            return false;
        }

        // 选货或带货中，不能和跷跷板交互
        var promoteCtrl = ClientManager.Inst.GetPlayerPromoteController(GameManager.Inst.ugcUserInfo.uid);
        if (promoteCtrl != null && (promoteCtrl.InSelect || promoteCtrl.InPromote))
        {
            TipPanel.ShowToast("Please quit promoting first.");
            return false;
        }

        //对局准备过程中不能与跷跷板交互
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null && (!PVPWaitAreaManager.Inst.IsPVPGameStart || PVPWaitAreaManager.Inst.IsSelfDeath))
        {
            TipPanel.ShowToast("Please wait for game mode to start.");
            return false;
        }

        // 冻结状态下不允许和跷跷板交互
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return false;
        }

        return true;
    }

    //当前玩家状态是否可以吃东西

    public bool CheckCanEat()
    {
        //正在做双人动作无法进入
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetIsInInteracting())
        {
            TipPanel.ShowToast("You could not eat food in the current state.");
        }
        //已经在吃东西的时候不能吃东西
        if (PlayerEatOrDrinkControl.Inst && PlayerEatOrDrinkControl.Inst.IsEating)
        {
            TipPanel.ShowToast("You could not eat food in the current state.");
            return false;
        }
        //已经在吃东西的时候不能吃东西
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.isBounceplankJumping)
        {
            TipPanel.ShowToast("You could not eat food in the current state.");
            return false;
        }
        //开车时不能吃东西
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
        {
            TipPanel.ShowToast("You could not eat food in the current state.");
            return false;
        }
        //双人牵手时不能吃东西
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("You could not eat food in the current state.");
            return false;
        }
        if (PlayerBaseControl.Inst&&PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return false;
        }
        //持有武器时不能吃东西
        var curHoldBev = PickabilityManager.Inst.GetBagHandleItemBevByPlayerId(GameManager.Inst.ugcUserInfo.uid);
        if (curHoldBev != null)
        {
            var entity = curHoldBev.entity;
            if (entity.HasComponent<ShootWeaponComponent>() || entity.HasComponent<AttackWeaponComponent>())
            {
                TipPanel.ShowToast("You could not eat food in the current state.");
                return false;
            }
        }
        //自拍模式不能吃东西
        if (StateManager.IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return false;
        }
        //降落伞和吃东西互斥
        if (IsParachuteUsing || IsParachuteFalling)
        {
            TipPanel.ShowToast("You could not eat food in the current state.");
            return false;
        }
        //拾起过程中互斥
        if (PickabilityManager.Inst.isSelfPicking)
        {
            return false;
        }
        //跷跷板和吃东西互斥
        if(StateManager.IsOnSeesaw)
        {
            SeesawManager.Inst.ShowSeesawMutexToast();
            return false;
        }
        //秋千和吃东西互斥
        if(IsOnSeesaw)
        {
            SwingManager.Inst.ShowSwingMutexToast();
            return false;
        }
        return true;
    }

    public bool CheckCanEnterPromoteMode()
    {
        //开车时不能进入带货
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
        {
            return false;
        }
        //正在做双人动作无法进入
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetIsInInteracting())
        {
            return false;
        }
        //求加好友状态中，不可进行操作
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetCurStateEmoName() == EmoName.EMO_ADD_FRIEND)
        {
            TipPanel.ShowToast("You could not promote in the current state");
            return false;
        }
        //双人牵手时无法进入
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            return false;
        }
        //持有武器时不能吃东西
        var curHoldBev = PickabilityManager.Inst.GetBagHandleItemBevByPlayerId(GameManager.Inst.ugcUserInfo.uid);
        if (curHoldBev != null)
        {
            TipPanel.ShowToast("You could not promote while picking up something");
            return false;
        }
        //自拍模式不能带货
        if (StateManager.IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return false;
        }
        if (PlayerBaseControl.Inst&&PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Emo))
        {
            TipPanel.ShowToast("You could not use emote in the current state");
            return false;
        }
        return true;
    }

    public bool IsCanPlayEmo()
    {
        //拾取道具状态下不能响应
        if (PlayerControlManager.Inst.isPickedProp)
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        if (PlayerStandonControl.Inst.IsStandOnIceCube())
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        if (EdibilitySystemController.Inst.isSelfEating)
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        if (PlayerStandonControl.Inst.IsStandOnSnowCube())
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        //方向盘上不能响应
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel != null)
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        //磁力版上不能响应
        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        if (PlayerSwimControl.Inst && PlayerSwimControl.Inst.isInWater)
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
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
        //滑梯上不能响应
        if (StateManager.IsOnSlide)
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        if (PlayerBaseControl.Inst.isFlying)
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        //正在做双人动作无法进入
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetIsInInteracting())
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        //冻结状态不允许
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        //摆摊
        var promoteCom = PlayerBaseControl.Inst.transform.GetComponent<PlayerPromoteController>();
        if (promoteCom != null && promoteCom.Status != PromoteStatus.None)
        {
            TipPanel.ShowToast(SwordManager.quitStateTips);
            return false;
        }

        return true;
    }


    public bool CheckCanEnterSelfieMode()
    {
        //驾驶方向盘和牵手跟随者不可进入自拍模式
        if (StateManager.PlayerOnCar)
        {
            TipPanel.ShowToast("Selfie mode cannot be used while driving.");
            return false;
        }
        //正在做双人动作无法进入或者作为牵手跟随者不可进入自拍模式
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetIsInInteracting()
        || (StateManager.PlayerIsMutual && PlayerMutualControl.Inst.isFollowPlayer))
        {
            TipPanel.ShowToast("Selfie mode cannot be used with interactive emote.");
            return false;
        }

        if (PlayerBaseControl.Inst.isFlying)
        {
            TipPanel.ShowToast("Selfie mode cannot be used while flying.");
            return false;
        }

        // 选货或带货中，不能进入自拍模式
        var promoteCtrl = ClientManager.Inst.GetPlayerPromoteController(GameManager.Inst.ugcUserInfo.uid);
        if (promoteCtrl != null && (promoteCtrl.InSelect || promoteCtrl.InPromote))
        {
            TipPanel.ShowToast("Selfie mode cannot be used while promoting.");
            return false;
        }
        //吃东西和自拍互斥
        if (EdibilitySystemController.Inst.isSelfEating)
        {
            TipPanel.ShowToast("Selfie mode cannot be used while eating.");
            return false;
        }
        //降落伞和自拍互斥
        if (IsParachuteUsing || IsParachuteFalling)
        {
            TipPanel.ShowToast("Selfie mode cannot be used while using parachute.");
            return false;
        }

        //冻结和自拍互斥
        if (PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.SelfieMode))
        {
            TipPanel.ShowToast("Selfie mode cannot be used while freezing.");
            return false;
        }
        //梯子和自拍互斥
        if (IsOnLadder)
        {
            LadderManager.Inst.ShowTips();
            return false;
        }

        if (StateManager.IsOnSeesaw)
        {
            SeesawManager.Inst.ShowSeesawMutexToast();
            return false;
        }

        //和钓鱼互斥
        if (IsFishing)
        {
            TipPanel.ShowToast("You could not take a selfie in the current state.");
			return false;
        }

        if (IsOnSlide)
        {
            SlidePipeManager.Inst.ShowLockTips();
            return false;
        }
        return true;
    }

    //是否处于降落伞滑翔和开伞
    public static bool IsParachuteUsing
    {
        get => PlayerParachuteControl.Inst && PlayerParachuteControl.Inst.IsParachuteUsing();
    }

    //是否处于降落伞开伞降落状态
    public static bool IsParachuteFalling
    {
        get => PlayerParachuteControl.Inst && PlayerParachuteControl.Inst.IsParachuteFalling();
    }
    
    //是否处于降落伞开伞滑翔状态
    public static bool IsParachuteGliding
    {
        get => PlayerParachuteControl.Inst && PlayerParachuteControl.Inst.IsParachuteGliding();
    }
    
    //是否处于雪方块内滑雪状态
    public static bool IsSnowCubeSkating
    {
        get => PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsSnowSkating();
    }
    
    //是否正在上下滑雪板
    public static bool IsSnowCubeBoardAnim
    {
        get => PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsPlayEnterOrLeaveAnim() ;
    }

    //处于更换子弹的状态下不允许丢出武器
    private bool IsReloading()
    {
        if (ShootWeaponCtrlPanel.Instance)
        {
            return ShootWeaponCtrlPanel.Instance.reloading;
        }
        return false;
    }

    public bool CanDropCurProp()
    {
        if (IsReloading())
        {
            TipPanel.ShowToast("You could not throw shooting item while reloading");
            return false;
        }
        if (IsFishing)
        {
            return false;
        }
        return true;
    }

    public bool CanCatchCurProp()
    {
        if (PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Pickability))
        {
            return false;
        }
        if (IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return false;
        }
        if (PlayerOnCar)
        {
            return false;
        }
        return true;
    }

    //是否处于吃东西状态
    public static bool IsEating
    {
        get => EdibilitySystemController.Inst != null && EdibilitySystemController.Inst.isSelfEating;
    }

    public void SetFishingCtrPanelVisibile(bool isVisible)
    {
        if (FishingCtrPanel.Instance != null)
        {
            FishingCtrPanel.Instance.SetCtrlPanelVisible(isVisible);
        }
    }

    public bool CheckCanStartFishing()
    {
        if(IsFishing || IsOnLadder || !PlayerIsTps || IsSnowCubeSkating ||IsOnSlide )
        {
            return false;
        }
        //冻结和钓鱼互斥
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.SelfieMode))
        {
            return false;
        }
        if(PlayerBaseControl.Inst && PlayerBaseControl.Inst.isBounceplankJumping)
        {
            return false;
        }
        //如果在空中则不能钓鱼
        if (!PlayerBaseControl.Inst.isGround)
        {
            return false;
        }
        if (IsInSelfieMode)
        {
            return false;
        }
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetIsInInteracting()
        || (StateManager.PlayerIsMutual && PlayerMutualControl.Inst.isFollowPlayer))
        {
            return false;
        }
        return true;
    }

    public bool IsHodingFishingRod()
    {
        var curHodeBev = PickabilityManager.Inst.GetBagHandleItemBevByPlayerId(GameManager.Inst.ugcUserInfo.uid);
        if (curHodeBev != null)
        {
            var entity = curHodeBev.entity;
            var gComp = entity.Get<GameObjectComponent>();
            if (gComp.modelType == NodeModelType.FishingModel)
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckCanEndFishing()
    {
        //冻结和钓鱼互斥
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.SelfieMode))
        {
            return false;
        }
        return true;
    }

    public static bool IsSelfPromoting()
    {
        var promoteCtr = ClientManager.Inst.GetPlayerPromoteController(GameManager.Inst.ugcUserInfo.uid);
        if (promoteCtr != null && (promoteCtr.Status != PromoteStatus.None))
        {
            return true;
        }
        return false;
	}

    //是否能够传送到子地图
    public bool CheckCanTransfer()
    {
        if (CameraModeManager.Inst != null && CameraModeManager.Inst.GetCurrentCameraMode() == CameraModeEnum.FreePhotoCamera)
        {
            return false;
        }

        if (IsInSelfieMode)
        {
            SelfieModeManager.Inst.ShowSelfieModeToast();
            return false;
        }
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            //被冻结时候不能使用传送门
            return false;
        }
        //游戏未开始，在游戏等待区，不能和传送门交互
        if (PVPWaitAreaManager.Inst.PVPBehaviour != null)
        {
            if (PVPWaitAreaManager.Inst.IsPVPGameStart)
            {
                TipPanel.ShowToast("You could not interact with teleport door in game mode");
            }
            else
            {
                TipPanel.ShowToast("You could not interact with teleport door in Waiting Zone");
            }
            return false;
        }

        // 牵手中，不能和传送门交互
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("You could not interact with teleport door while Hand-in-hand");
            return false;
        }

        if (PlayerEatOrDrinkControl.Inst && PlayerEatOrDrinkControl.Inst.IsEating)
        {
            TipPanel.ShowToast("You could not use teleport door while eating food.");
            return false;
        }
        return true;
    }

    //处理特殊状态动画(收集宝石)
    public static bool CanPlayCharacterAnim()
    {
        //第一人称不播动画
        if (!PlayerBaseControl.Inst.isTps || !PlayerBaseControl.Inst.gameObject.activeSelf)
        {
            return false;
        }
        //飞行状态不能发起动作
        if (PlayerBaseControl.Inst.isFlying /*|| !PlayerBaseControl.Inst.isGround*/)
        {
            return false;
        }
        //蹦床时不能发起动作
        if (PlayerBaseControl.Inst && PlayerBaseControl.Inst.isBounceplankJumping)
        {
            return false;
        }
        //动作被冻结不能发起动作(冰冻/滑梯/收集宝石)
        if (PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Emo))
        {
            return false;
        }
        //不能发起动作: 跷跷板上/秋千上/驾驶状态/降落伞/上下滑雪板/自拍/钓鱼/牵手中/梯子上/吃东西
        if (IsOnSeesaw || IsOnSwing || PlayerOnCar || IsParachuteUsing || IsSnowCubeBoardAnim ||
            IsInSelfieMode || IsFishing || PlayerIsMutual || IsOnLadder || IsEating)
        {
            return false;
        }
        //选货或带货中不能发起动作
        if (IsSelfPromoting())
        {
            return false;
        }
        //正在做双人动作不能发起动作
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetIsInInteracting())
        {
            return false;
        }
        //求加好友状态中不能发起动作
        if (PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.GetCurStateEmoName() == EmoName.EMO_ADD_FRIEND)
        {
            return false;
        }
        //拾取道具状态下不能发起动作
        if (PlayerControlManager.Inst.isPickedProp)
        {
            return false;
        }
        //磁力板上不能发起动作
        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
        {
            return false;
        }
        //游泳状态不能发起动作
        if (PlayerSwimControl.Inst && PlayerSwimControl.Inst.isInWater)
        {
            return false;
        }
        //滑冰状态不能发起动作
        if (PlayerStandonControl.Inst.IsStandOnIceCube())
        {
            return false;
        }
        //滑雪状态不能发起动作
        if (PlayerStandonControl.Inst.IsStandOnSnowCube())
        {
            return false;
        }
        return true;
    }
}

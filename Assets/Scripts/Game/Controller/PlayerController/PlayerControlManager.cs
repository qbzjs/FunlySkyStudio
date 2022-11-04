using System.Collections.Generic;
using UnityEngine;

//Player 动画管理器的动画Id
public enum AnimId
{
    IsGround = 0,
    IsMoving = 1,
    IsFlying = 2,
    IsInDoubleEumual = 3,
    IsStartPlayer = 4,
    IsOnSteering = 5,
    IsFastRun = 6,
    IsInWater = 7,
    IsSwimming = 8,
    IsJump = 9,
    Parachute = 10,
    RunOffset = 11,
}

public enum PlayerControlType
{
    Base, //基础能力
    Emoji, // 表情
    Swim, // 游泳（水方块）
    OnBoard, // 磁力板
    Drive, // 驾驶（方向盘）
    Mutual, // 双人表情交互（牵手）
    Attack, //攻击道具
    Shoot,//射击道具
    StandOn,//玩家站在某道具上
    EatOrDrink,//食用道具
    Parachute,//降落伞
    OnLadder,//梯子
    SnowSkate, //滑雪
    OnSeesaw, // 跷跷板
    SlidePipe,//滑梯
    Swing,//秋千
}

/// <summary>
/// Author:WenJia
/// Description:PlayerControl 管理器
/// 管理 Player所有能力 和 和角色包围盒数据
/// 多个能力需要连接处理时，可在此处组合联结
/// 尽量减少各个能力模块间的耦合
/// Date: 2022/3/31 11:14:20
/// </summary>

public class PlayerControlManager : MonoBehaviour
{
    // Player 动画管理器的动画参数名和枚举的对应字典，后续需要的可自行动态添加
    public Dictionary<int, string> AnimNameDict = new Dictionary<int, string>(){
        {(int)AnimId.IsGround , "IsGround"},
        {(int)AnimId.IsMoving , "IsMoving"},
        {(int)AnimId.IsFlying , "IsFlying"},
        {(int)AnimId.IsInDoubleEumual , "IsInDoubleEumual"},
        {(int)AnimId.IsStartPlayer , "IsStartPlayer"},
        {(int)AnimId.IsOnSteering , "IsOnSteering"},
        {(int)AnimId.IsFastRun , "IsFastRun"},
        {(int)AnimId.IsJump , "IsJump"},
        {(int)AnimId.Parachute , "Parachute"},
        {(int)AnimId.RunOffset , "RunOffset"},
    };
    public static PlayerControlManager Inst;
    public PlayerBaseControl playerBase;
    public PlayerEmojiControl playerEmoji;
    public bool isPickedProp;
    public bool isGrabState = false;
    // 所有的 PlayerControl 字典集，需要在使用时动态添加和删除（留作后续扩展管理）
    public Dictionary<PlayerControlType, IPlayerCtrlMgr> playerCtrlMgrDict = new Dictionary<PlayerControlType, IPlayerCtrlMgr>();

    //人物其他能力脚本挂载节点，各个能力控制脚本动态添加和删除
    public GameObject playerControlNode;
    [HideInInspector]
    public Vector3 waterTriggerSize;
    [HideInInspector]
    public Vector3 waterTriggerCenter;
    public Vector3 normalBoxSize = new Vector3(0.4f, 1.3f, 0.4f);
    public Vector3 normalBoxCenter = new Vector3(0, -0.2f, 0);

    public GameObject effectTool; // 挂在人物身上的道具
    // 普通状态下的片段列表（第三人称视角下）
    public List<ClipItem> normalAnimClipList = new List<ClipItem>();
    // 拾取状态需要替换的动画片段列表（第三人称视角下）
    public List<ClipItem> pickupAnimClipList = new List<ClipItem>();

    public List<ClipItem> attackAnimClipList = new List<ClipItem>();
    public List<ClipItem> shootAnimClipList = new List<ClipItem>();
    public List<ClipItem> iceCubeAttackAnimClipList = new List<ClipItem>();
    public List<ClipItem> iceCubeShootAnimClipList = new List<ClipItem>();
    public List<ClipItem> selfieNormalAnimClipList = new List<ClipItem>();
    public List<ClipItem> selfieAttackAnimClipList = new List<ClipItem>();
    public List<ClipItem> selfiePickupAnimClipList = new List<ClipItem>();
    //滑雪动画片段
    public List<ClipItem> SnowSkateAttackAnimClipList = new List<ClipItem>();
    public List<ClipItem> SnowSkateShootAnimClipList = new List<ClipItem>();
    //跷跷板动画片段
    public List<ClipItem> SeesawAnimClipList = new List<ClipItem>();
    //秋千
    public List<ClipItem> SwingAnimClipList = new List<ClipItem>();
    //钓鱼动画片段
    public List<ClipItem> FishingAnimClipList = new List<ClipItem>();
    public List<ClipItem> FishingIceAnimClipList = new List<ClipItem>();
    public List<ClipItem> FishingSnowAnimClipList = new List<ClipItem>();
    private void Awake()
    {
        Inst = this;
        waterTriggerSize = normalBoxSize;
        waterTriggerCenter = normalBoxCenter;
    }

    public void AddAnimName(int id, string name)
    {
        if (!AnimNameDict.ContainsKey(id))
        {
            AnimNameDict.Add(id, name);
        }
    }

    public void AddPlayerCtrlMgr(PlayerControlType type, IPlayerCtrlMgr mgr)
    {
        if (!playerCtrlMgrDict.ContainsKey(type))
        {
            playerCtrlMgrDict.Add(type, mgr);
        }
    }

    public IPlayerCtrlMgr GetPlayerCtrlMgr(PlayerControlType type)
    {
        if (playerCtrlMgrDict.ContainsKey(type))
        {
            return playerCtrlMgrDict[type];
        }
        return null;
    }
    public T GetPlayerCtrlMgrAs<T>(PlayerControlType type) where T : IPlayerCtrlMgr
    {
        IPlayerCtrlMgr iCtrl = GetPlayerCtrlMgr(type); 
        if (iCtrl != null)
        {
            return (T)iCtrl;
        } 
        return default(T);
    }
    public void RemovePlayerCtrlMgr(PlayerControlType type)
    {
        if (playerCtrlMgrDict.ContainsKey(type))
        {
            playerCtrlMgrDict.Remove(type);
        }
    }
    private void OnDestroy()
    {
        playerCtrlMgrDict.Clear();
        Inst = null;
    }

    /**
    * 摇杆控制移动，由于和其他模块容易耦合，故放到 Manager 中组合实现
    */
    public void Move(Vector3 screenOffset)
    {
        if (Inst != null
         && playerBase.GetNoAbilityFlag(EObjAbilityType.Move)
         && screenOffset != Vector3.zero)
        {
            return;
        }
        var playerMutual = PlayerMutualControl.Inst;
        //跟随者移动遥控杆，显示放手
        if (playerMutual && playerMutual.isFollowPlayer)
        {
            if (screenOffset != Vector3.zero)
            {
                PlayModePanel.Instance.ShowReleaseBtn();
            }
            return;
        }

        // 调用基本能力进行摇杆控制移动
        playerBase.Move(screenOffset);

        // 如果自己是牵手发起者，就执行牵手的移动
        if (playerMutual && playerMutual.isInEumual && playerMutual.isStartPlayer)
        {
            playerMutual.Move(screenOffset);
        }
    }

    public void SetPlayerActive(bool isActive)
    {
        playerBase.SetPlayerActive(isActive);

        if (playerBase.isTps)
        {
            if (PlayerDriveControl.Inst)
            {
                // 切换视角后重新握起方向盘
                PlayerDriveControl.Inst.ChangeViewResetDriveState();
            }

            if (PlayerSwimControl.Inst)
            {
                PlayerSwimControl.Inst.ChangeViewResetSwimState();
            }
        }
    }

    /**
    * 玩家发起表情和动作处理
    * 由于双人表情和一些其他模块互斥，故提取到 Manager，方便做联结
    */
    public void PlayMove(int i)
    {
        var emoData = MoveClipInfo.GetAnimName(i);

        if (!CanPlayMove(emoData))
        {
            return;
        }
        //如果当前是播放状态中。取消AddFriend
        if (playerEmoji.mCurEmoData!=null
            &&playerEmoji.mCurEmoData.emoType==(int)EmoMenuPanel.EmoTypeEnum.STATE_EMO
            &&emoData.emoType==(int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO)
        {
            playerEmoji.CancelStateEmo((int)EmoName.EMO_ADD_FRIEND);
        }
        playerEmoji.PlayMove(i);
    }

    /**
    * 是否能发起动作
    */
    private bool CanPlayMove(EmoIconData emoData)
    {
        if (playerBase.animCon.isInteracting || playerBase.isMoving || playerBase.moveY != (int)FlyStatus.stop)
        {
            return false;
        }
        //
        if (playerBase.GetNoAbilityFlag(EObjAbilityType.Emo))
        {
            TipPanel.ShowToast("You could not use emote in the current state");
            return false;
        }
        //在磁力板上不能发起双人动作
        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard
        && emoData.emoType == (int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO)
        {
            TipPanel.ShowToast("You could not use interactive emotes when locked with adhesive surface");
            return false;
        }
        //在梯子上不能发起双人动作
        if (StateManager.IsOnLadder
        && emoData.emoType == (int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO)
        {
            LadderManager.Inst.ShowTips();
            return false;
        }
        //跷跷板上不能发起任何动作
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

        //在滑梯上不能发起双人动作
        if (StateManager.IsOnSlide && emoData.emoType == (int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO)
        {
            return false;
        }
        // 牵手时不能发起双人动作
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual
        && emoData.emoType == (int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO)
        {
            TipPanel.ShowToast("You could not use interactive emotes while Hand-in-hand");
            return false;
        }

        // 飞行状态下不能牵手
        if (playerBase.isFlying && emoData.id == (int)EmoName.EMO_JOIN_HAND)
        {
            TipPanel.ShowToast("You could not use hand-in-hand emote while flying");
            return false;
        }

        //拾取道具状态下不能发起双人动作
        if (isPickedProp && emoData.emoType == (int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO)
        {
            TipPanel.ShowToast("You could not use interactive emotes while holding object");
            return false;
        }

        //驾驶方向盘时，不能发起双人动作
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel
        && emoData.id == (int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO)
        {
            TipPanel.ShowToast("You could not use interactive emote while driving");
            return false;
        }

        // 第一人称视角不能牵手
        if (!playerBase.isTps && emoData.id == (int)EmoName.EMO_JOIN_HAND)
        {
            TipPanel.ShowToast("You could not use hand-in-hand emote in first person perspective");
            return false;
        }

        //驾驶状态不播放照镜子动作
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel && emoData.id == (int)EmoName.EMO_LOOK_MIRROR)
        {
            return false;
        }

        //状态动作中无法发起其他双人动作
        //if (emoData.emoType == (int)EmoMenuPanel.EmoTypeEnum.DOUBLE_EMO && PlayerEmojiControl.Inst && PlayerEmojiControl.Inst.IsInStateEmo())
        //{
        //    return false;
        //}
        
        //正在使用降落伞 不能发起表情
        if (StateManager.IsParachuteUsing)
        {
            return false;
        }
        
        //正在播放上下滑雪板动画，不能发表情
        if (StateManager.IsSnowCubeBoardAnim)
        {
            return false;
        }

        return true;
    }

    public bool CanPlayStateEmo(int emoId)
    {
        var emoData = MoveClipInfo.GetAnimName(emoId);
        if (emoData == null)
        {
            return false;
        }

        if (emoData.emoType != (int)EmoMenuPanel.EmoTypeEnum.STATE_EMO)
        {
            return false;
        }

        //正在做双人动作无法进入
        if (PlayerEmojiControl.Inst &&  PlayerEmojiControl.Inst.GetIsInInteracting())
        {
            return false;
        }

        //拾取道具状态下不能发起
        if (isPickedProp)
        {
            TipPanel.ShowToast("You could not use interactive emotes while holding object");
            return false;
        }
        //在梯子上不能发起
        if (StateManager.IsOnLadder)
        {
            LadderManager.Inst.ShowTips();
            return false;
        }
        //在磁力板上不能发起
        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
        {
            TipPanel.ShowToast("You could not use interactive emotes while on adhesive surface");
            return false;
        }
        //在跷跷板不能发起
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
        //驾驶方向盘时，不能发起
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
        {
            TipPanel.ShowToast("You could not use interactive emotes while using steering wheel");
            return false;
        }

        //双人牵手不允许进入加好友emo
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            LoggerUtils.Log("双人牵手不允许进入加好友emo");
            return false;
        }
        
        //正在使用降落伞 不能发起表情
        if (StateManager.IsParachuteUsing)
        {
            return false;
        }
        if (PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Emo))
        {
            TipPanel.ShowToast("You could not use emote in the current state");
            return false;
        }
        return true;
    }

    public void CallStateEmo(int i)
    {
        if (!CanPlayStateEmo(i))
        {
            return;
        }
       
        playerEmoji.CallStateEmo(i);
        if (EmoMenuPanel.Instance)
        {
            EmoMenuPanel.Instance.StartClickStateEmoCor();
        }
    }

    /**
    * 更换动画剪辑片段（普通动画和拾取动画之间动态更换）
    */
    public void ChangeAnimClips()
    {
        if (!playerBase.isTps)
        {
            return;
        }

        if (StateManager.IsInSelfieMode)
        {
            ChangeAnimClipsInSelfieMode();
        }
        else
        {
            ChangeAnimClipsInNormalMode();
        }
    }

    public void ChangeAnimClipsInSelfieMode()
    {
        if (!playerBase.isTps)
        {
            return;
        }
        var overrideAnimClipList = selfieNormalAnimClipList;
        if (PlayerControlManager.Inst.isPickedProp)
        {
            overrideAnimClipList = selfiePickupAnimClipList;
        }
        if (PlayerAttackControl.Inst && PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon != null)
        {
            var weapon = PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon;
            overrideAnimClipList = selfieAttackAnimClipList;
        }
        playerBase.UpdateAnimClips(overrideAnimClipList);
    }

    public void ChangeAnimClipsInNormalMode()
    {
        if (!playerBase.isTps)
        {
            return;
        }
        var overrideAnimClipList = normalAnimClipList;
        if (PlayerControlManager.Inst.isPickedProp)
        {
            overrideAnimClipList = pickupAnimClipList;
        }
        if (PlayerAttackControl.Inst && PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon != null)
        {
            var weapon = PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon;
            overrideAnimClipList = attackAnimClipList;
        }
        if (PlayerShootControl.Inst && PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
        {
            var weapon = PlayerShootControl.Inst.curShootPlayer.HoldWeapon;
            overrideAnimClipList = shootAnimClipList;
        }
        if (PlayerParachuteControl.Inst)
        {
            overrideAnimClipList = normalAnimClipList;
        }
        if (PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsStandOnIceCube())
        {
            overrideAnimClipList = iceCubeAttackAnimClipList;

            if (PlayerAttackControl.Inst && PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon != null)
            {
                overrideAnimClipList = iceCubeAttackAnimClipList;
            }
            if (PlayerShootControl.Inst && PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
            {
                overrideAnimClipList = iceCubeShootAnimClipList;
            }
            if (FishingManager.Inst != null && FishingManager.Inst.IsPlayerHoldingFishingRod(GameManager.Inst.ugcUserInfo.uid))
            {
                overrideAnimClipList = FishingIceAnimClipList;
            }
        }

        if (SnowCubeManager.Inst.IsStandOnSnowCube() && PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsSnowSkating())
        {
            overrideAnimClipList = SnowSkateAttackAnimClipList;

            if (PlayerAttackControl.Inst && PlayerAttackControl.Inst.curAttackPlayer.HoldWeapon != null)
            {
                overrideAnimClipList = SnowSkateAttackAnimClipList;
            }
            if (PlayerShootControl.Inst && PlayerShootControl.Inst.curShootPlayer.HoldWeapon != null)
            {
                overrideAnimClipList = SnowSkateShootAnimClipList;
            }
            if (FishingManager.Inst != null && FishingManager.Inst.IsPlayerHoldingFishingRod(GameManager.Inst.ugcUserInfo.uid))
            {
                overrideAnimClipList = FishingSnowAnimClipList;
            }
        }

        //玩家处于钓鱼状态
        if (FishingManager.Inst != null && FishingManager.Inst.IsPlayerHoldingFishingRod(GameManager.Inst.ugcUserInfo.uid))
        {
            overrideAnimClipList = FishingAnimClipList;

            if(SnowCubeManager.Inst.IsStandOnSnowCube() && PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsSnowSkating())
            {
                overrideAnimClipList = FishingSnowAnimClipList;
            }

            if (PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsStandOnIceCube())
            {
                overrideAnimClipList = FishingIceAnimClipList;
            }
        }

        if (StateManager.IsOnSeesaw)
        {
            overrideAnimClipList = SeesawAnimClipList;
        }
        
        if (StateManager.IsOnSwing)
        {
            overrideAnimClipList = SwingAnimClipList;
        }

        playerBase.UpdateAnimClips(overrideAnimClipList);
    }


    public void StopPlaySpecialAudio()
    {
        if (playerBase != null && playerBase.animCon != null)
        {
            playerBase.animCon.StopAudio();
        }
    }
}

using System.Collections;
using UnityEngine;
using BudEngine.NetEngine;

public enum PlayerHandleState
{
    None,
    ActiveHand,
    PassiveHand
}

/// <summary>
/// Author:WenJia
/// Description:Player 交互控制
/// 主要包含双人牵手的相关逻辑
/// Date: 2022/3/31 11:14:20
/// </summary>

public class PlayerMutualControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector]
    public PlayerBaseControl playerBase;
    [HideInInspector]
    public static PlayerMutualControl Inst;
    [HideInInspector]
    public CharacterController character;
    [HideInInspector]
    public Animator playerAnim;
    [HideInInspector]
    public AnimationController animCon;
    // 双人牵手的跟随者
    [HideInInspector]
    public OtherPlayerCtr playerFollowerCtrl = null;
    [HideInInspector]
    public UgcFrameData followerFrameData;
    [HideInInspector]
    // 双人动作进行中
    public bool isInEumual = false;
    [HideInInspector]
    //是否为双人动作的发起者
    public bool isStartPlayer = false;
    [HideInInspector]
    //是否为双人动作的跟随者
    public bool isFollowPlayer = false;
    //保存记录双人牵手动作的发起者 ID
    [HideInInspector]
    public string startPlayerId = "";
    //保存记录双人牵手动作的发起者 ID
    [HideInInspector]
    public string followPlayerId = "";
    // 双人牵手状态下的两人公用的人物管理器
    public MutualPlayersControl mutualPlayersCtrl;
    //记录玩家是否处于牵手待机状态
    [HideInInspector]
    public bool isWaitingHands = false;
    private Transform selfPlayerParent;
    //记录玩家跟随之前的胶囊体的中心
    private Vector3 beforePlayerCenter;
    private Vector3 playerInitPos = new Vector3(0, -0.95f, 0);
    // 放手后玩家未移动，导致CharacterController判断isGrounded出错
    private bool isEndMutualIdle = false;
    private Vector3 followPlayerIdlePos = new Vector3(0.5f, 0, 0);
    private Vector3 followPlayerRunPos = new Vector3(0.37f, 0, -0.6f);
    public bool isHoldingHands = false;
    private void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.Mutual, Inst);

        playerBase = PlayerControlManager.Inst.playerBase;
        playerAnim = playerBase.playerAnim;
        selfPlayerParent = playerAnim.transform.parent;
        character = playerBase.Character;
        beforePlayerCenter = character.center;
        animCon = playerBase.animCon;

        MessageHelper.AddListener(MessageName.PlayerCreate, PlayerOnCreate);
    }

    private void OnDestroy()
    {
        if (PlayerControlManager.Inst)
        {
            PlayerControlManager.Inst.RemovePlayerCtrlMgr(PlayerControlType.Mutual);
        }
        MessageHelper.RemoveListener(MessageName.PlayerCreate, PlayerOnCreate);
        Inst = null;
    }

    private void PlayerOnCreate()
    {
        if (!isHoldingHands
         && !string.IsNullOrEmpty(startPlayerId)
         && !string.IsNullOrEmpty(followPlayerId))
        {
            StartMutual(startPlayerId, followPlayerId);
        }
    }

    private void Update()
    {
        if (isInEumual)
        {
            // 自己为牵手发起者
            if (isStartPlayer)
            {
                // 自己的跟随者同步地面状态
                PlayFollowerAnim(AnimId.IsGround, playerBase.isGround);
                PlayOtherFollowEffect();
            }
            else
            {
                // 自己为牵手跟随者
                UpdateFollowerFrameData();
            }
        }
    }

    /**
    * 开始牵手：牵手状态记录和处理
    */
    public void StartMutual(string sPlayerId, string fPlayerId)
    {
        if (isHoldingHands)
        {
            return;
        }
        isWaitingHands = false;
        isInEumual = true;
        startPlayerId = sPlayerId;
        followPlayerId = fPlayerId;
        isStartPlayer = (sPlayerId == Player.Id);
        isFollowPlayer = (fPlayerId == Player.Id);
        animCon.StopLoop();
        isHoldingHands = false;
        if (PlayerSwimControl.Inst)
        {
            PlayerSwimControl.Inst.ForceOutWater();
        }
        playerBase.PlayerResetIdle();

        if (CatchPanel.Instance)
        {
            CatchPanel.Hide();
        }

        // 如果自己不是发起者，发起者需要终止循环变为 idle 状态
        if (!isStartPlayer)
        {
            if (character)
            {
                character.center = new Vector3(0, -0.1f, 0);
            }

            if (ClientManager.Inst.otherPlayerDataDic.Count > 0)
            {
                var sPlayer = ClientManager.Inst.GetOtherPlayerComById(startPlayerId);
                if (sPlayer && sPlayer.animCon)
                {
                    sPlayer.animCon.RecStopLoop();
                }
            }
        }

        if (isStartPlayer)
        {
            mutualPlayersCtrl = playerBase.gameObject.GetComponent<MutualPlayersControl>();
            if (!mutualPlayersCtrl)
            {
                mutualPlayersCtrl = playerBase.gameObject.AddComponent<MutualPlayersControl>();
            }

            var fPlayer = ClientManager.Inst.otherPlayerDataDic[followPlayerId];
            if (fPlayer)
            {
                mutualPlayersCtrl.SetOtherPlayer(fPlayer.gameObject);
                mutualPlayersCtrl.StartMutual();
                fPlayer.StopFootSound();
                fPlayer.ForceOutWater();
            }
        }

        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.RefreshBtns();
        }
        isHoldingHands = true;
    }

    /**
    * 结束牵手：牵手状态解除处理
    */
    public void EndMutual()
    {
        isInEumual = false;
        GameUtils.ResetStayTime();
        if (isInEumual)
        {
            isWaitingHands = false;
        }
        if (!isStartPlayer && !string.IsNullOrWhiteSpace(startPlayerId))
        {
            // 将自己的父节点设回之前的父节点
            playerAnim.transform.SetParent(selfPlayerParent, true);
            playerAnim.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
            playerAnim.transform.localPosition = playerInitPos;
            var playerCharacterCtrl = playerBase.gameObject.AddComponent<CharacterController>();
            InitCharacterController(playerBase.Character);
            // 将自己的 Character Controller 组件的中心恢复
            playerBase.Character.center = beforePlayerCenter;

            playerBase.gravity = Physics.gravity.y;
            PlayModePanel.Instance.RefreshBtns();

            //如果自己不是发起者，需要恢复发起者的动作状态
            var startPlayerCtrl = ClientManager.Inst.otherPlayerDataDic[startPlayerId];
            if (startPlayerCtrl)
            {
                startPlayerCtrl.SetPlayerAnimParam(false, false);
                startPlayerCtrl.GetComponent<PlayerTouchBehaviour>().SetCanTouch(false);
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(followPlayerId))
            {
                var fPlayer = ClientManager.Inst.otherPlayerDataDic[followPlayerId];
                if (fPlayer)
                {
                    fPlayer.GetComponent<PlayerTouchBehaviour>().SetCanTouch(false);
                    fPlayer.SetPlayerAnimParam(false, false);
                }
            }
            if (PlayModePanel.Instance)
            {
                PlayModePanel.Instance.RefreshBtns();
            }
        }

        startPlayerId = "";
        followPlayerId = "";
        isStartPlayer = false;
        isFollowPlayer = false;
        playerFollowerCtrl = null;
        if (mutualPlayersCtrl)
        {
            mutualPlayersCtrl.EndMutual();
            Destroy(mutualPlayersCtrl);
        }

        // 恢复自己的状态
        playerBase.isMoving = false;
        playerBase.PlayAnimation(AnimId.IsMoving, false);
        playerBase.PlayAnimation(AnimId.IsInDoubleEumual, false);
        playerBase.PlayAnimation(AnimId.IsStartPlayer, false);
        playerBase.PlayerResetIdle();
        // StartCoroutine(SetEndMutualIdleCor());
        playerBase.ResetUpwardVec();
        if (playerBase.mAnimStateManager!=null)
        {
            playerBase.mAnimStateManager.SwitchTo(EPlayerAnimState.Idle);
        }
        // 移除牵手交互控制脚本
        Destroy(this);
    }

    // private IEnumerator SetEndMutualIdleCor()
    // {
    //     isEndMutualIdle = true;
    //     yield return null;
    //     //1帧后需要复位，否则原地离开地面后没有向下速度
    //     isEndMutualIdle = false;
    // }

    /**
    * 恢复自己的胶囊体各参数
    */
    private void InitCharacterController(CharacterController controller)
    {
        controller.slopeLimit = 90;
        controller.stepOffset = 1;
        controller.skinWidth = 0.05f;
        controller.minMoveDistance = 0;
        controller.radius = 0.5f;
        controller.center = new Vector3(0, -0.07f, 0);
        controller.height = 1.7f;
    }

    /**
    * 打断跟随者的循环动作
    */
    public void StopFollowerLoop()
    {
        if (isInEumual && playerFollowerCtrl)
        {
            playerFollowerCtrl.animCon.RecStopLoop();
        }
    }

    /**
    * 播放跟随者动作动画
    */
    public void PlayFollowerAnim(AnimId animId, bool state)
    {
        if (isInEumual && playerFollowerCtrl != null)
        {
            string animName = PlayerControlManager.Inst.AnimNameDict[(int)animId];
            playerFollowerCtrl.PlayAnim(animName, state);

            if (PlayerStandonControl.Inst)
            {
                // playerFollowerCtrl.m_AnimType = (int) PlayerStandonControl.Inst.GetStandOnType();
                playerFollowerCtrl.m_AnimType = FrameStateManager.Inst.GetCurFrameAnimType();
                if (playerFollowerCtrl.m_AnimType == (int)FrameAnimType.SelfieMode)
                {
                    playerFollowerCtrl.m_AnimType = (int)FrameAnimType.Normal;
                }
            }
        }
    }

    private void PlayOtherFollowEffect()
    {
        if(playerFollowerCtrl != null && playerFollowerCtrl.mPlayerStateManager != null && playerFollowerCtrl.mPlayerStateManager.mSkateEffectObj != null)
        {
            var isActive = playerBase.isGround && !playerBase.IsJump && playerBase.isMoving && PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsStandOnIceCube();
            EffectActive(playerFollowerCtrl.mPlayerStateManager.mSkateEffectObj, isActive);
        }
        else
        {
            return;
        }
    }

    private void PlaySelfFollowEffect()
    {
        if (playerBase != null && playerBase.mAnimStateManager != null && playerBase.mAnimStateManager.mSkateEffectObj != null)
        {
            var isActive = playerBase.isGround && !playerBase.IsJump &&  playerBase.isMoving  && PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsStandOnIceCube();
            EffectActive(playerBase.mAnimStateManager.mSkateEffectObj, isActive);
        }
        else
        {
            return;
        }
    }

    private void EffectActive(GameObject mSkateEffectObj,bool isActive)
    {
        mSkateEffectObj.SetActive(isActive);
    }

    /**
    * 设置跟随者玩家的动画参数
    */
    public void SetFollowPlayerAnimParam(bool isEumutal, bool isStartPlayer)
    {
        if (isInEumual && playerFollowerCtrl != null)
        {
            playerFollowerCtrl.SetPlayerAnimParam(isEumutal, isStartPlayer);
        }
    }

    /**
    * 当自己为跟随者时，需要跟随发起者发送同样的表情
    */
    public void SelfPlayerFollowEmote(int id)
    {
        if (isFollowPlayer)
        {
            var emoData = MoveClipInfo.GetAnimName(id);
            //不能解析的动作，先打断当前循环动作
            if (emoData == null)
            {
                if (animCon.isPlaying)
                {
                    animCon.StopLoop();
                }
                return;
            }
            PlayerEmojiControl.Inst.PlayMove(id);
        }
    }

    /**
    * 当自己为牵手双方的跟随者时,用发起者的帧数据来更新自己的位置和状态
    */
    private void UpdateFollowerFrameData()
    {
        // 自己为跟随者
        if (isInEumual && isFollowPlayer && followerFrameData != null)
        {
            playerBase.isMoving = followerFrameData.IsMoving;
            playerBase.isGround = followerFrameData.IsGround;
            playerBase.isFlying = followerFrameData.IsFlying;

            var startPlayerCtrl = ClientManager.Inst.otherPlayerDataDic[startPlayerId];
            if (playerAnim.transform.parent != startPlayerCtrl.transform)
            {
                // 将自己的父节点设为发起者
                playerAnim.transform.parent = startPlayerCtrl.transform;
                Destroy(character);
                playerBase.gravity = 0;
            }
            playerAnim.transform.rotation = new Quaternion(0, 0, 0, 0);

            if (playerBase.isMoving)
            {
                if (animCon.isPlaying)
                {
                    AudioController.Inst.audioState = MoveAudioState.None;
                    playerBase.StopFootSound();
                    animCon.StopLoop();
                }
                playerAnim.transform.localPosition = followPlayerRunPos;
            }
            else
            {
                playerAnim.transform.localPosition = followPlayerIdlePos;
            }
            var pos = playerAnim.transform.parent.TransformPoint(playerAnim.transform.localPosition);
            playerBase.transform.localPosition = new Vector3(pos.x, pos.y - 0.03f, pos.z);

            playerBase.PlayAnimation(AnimId.IsMoving, playerBase.isMoving);
            playerBase.PlayAnimation(AnimId.IsGround, playerBase.isGround);
            playerBase.PlayAnimation(AnimId.IsFlying, playerBase.isFlying);
            playerBase.PlayAnimation(AnimId.IsInDoubleEumual, isInEumual);
            playerBase.PlayAnimation(AnimId.IsStartPlayer, isStartPlayer);

            //此时发起者应切换为牵手状态下的动作
            startPlayerCtrl.SetPlayerAnimParam(true, true);
            PlaySelfFollowEffect();
        }
    }

    public void Move(Vector3 screenOffset)
    {
        if (!animCon.isPlaying)
        {
            if (playerAnim.gameObject.activeSelf)
            {
                if (screenOffset == Vector3.zero)
                {
                    PlayFollowerAnim(AnimId.IsMoving, false);
                }
                else
                {
                    playerBase.PlayAnimation(AnimId.IsInDoubleEumual, isInEumual);
                    playerBase.PlayAnimation(AnimId.IsStartPlayer, isStartPlayer);

                    PlayFollowerAnim(AnimId.IsMoving, true);
                    PlayFollowerAnim(AnimId.IsInDoubleEumual, isInEumual);
                    PlayFollowerAnim(AnimId.IsStartPlayer, false);
                }
            }
        }
        else
        {
            StopFollowerLoop();
        }
    }

    public void Jump()
    {
        if (animCon.isPlaying)
        {
            StopFollowerLoop();
        }
    }
}
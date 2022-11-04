using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using BudEngine.NetEngine;
using System.Collections;
public class OtherPlayerCtr : MonoBehaviour, IPlayerController
{

    public Vector3 m_PlayerPos = new Vector3();
    public Quaternion m_PlayerRot = new Quaternion();
    public bool m_IsMoving = false, m_IsGround = false, m_IsFlying, m_IsInWater = false,m_IsSwimming = false;
    public int m_AnimType; //动画片段类型
    public FrameAnimType CurrentAnimType;
    public int m_StateType; //状态类型

    private bool isFastRun = false;
    private GameObject m_Obj;
    public Animator m_PlayerAnim;
    public AnimationController animCon;
    private string m_MapId = "";
    private bool emoInterState;
    private GameObject hitGameObject;
    private bool isMoving = false;
    private bool isGround = false;
    private bool isFlying = false;
    public bool isJump = false;

    public bool isAvoidFrame = false;//是否跳过帧同步
    public int avoidLerpCount = 0;//跳过缓动的帧数
    public SteeringWheelBehaviour steeringWheel;
    public OtherPlayerAttackCtr otherPlayerAttackCtr;
    public OtherPlayerShootCtr otherPlayerShootCtr;
    public OtherPlayerFrameStateCtr otherFrameStateCtr;
    public OtherPlayerEatOrDrinkCtr otherPlayerEatOrDrinkCtr;
    private bool _isInEumual = false, _isStartPlayer = false;
    //牵手跟随者
    private OtherPlayerCtr followPlayerCtrl = null;
    private OtherPlayerWaterCtr otherPlayerWaterCtr = new OtherPlayerWaterCtr();
    public Transform beforePlayerParent;
    public AnimatorOverrideController overrideController;
    protected AnimationClipOverrides clipOverrides;
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
    public List<ClipItem> SnowSkateAttackAnimClipList = new List<ClipItem>();
    public List<ClipItem> SnowSkateShootAnimClipList = new List<ClipItem>();
    //跷跷板动画片段
    public List<ClipItem> SeesawAnimClipList = new List<ClipItem>();
    public List<ClipItem> FishingAnimClipList = new List<ClipItem>();
    public List<ClipItem> SwingAnimClipList = new List<ClipItem>();

    public bool IsInSelfieMode = false;
    public GameObject effectTool; // 挂在人物身上的道具

    public bool isOnSeesaw =  false;
    public bool isOnSwing =  false;

    public int lastAnimClipIndex = 0;

    public OtherPlayerAnimStateManager mPlayerStateManager;
    
    private EmoMsgHandlerBase emoMsgHandler;
    private BudTimer _restoreTimer;

    public bool isPlaySnowSkatingSound = false;
    public OtherPlayerSlideMoveCompt mSlideMovementCompt;
    private void SetEmoMsgHandler(EmoMsgHandlerBase newMsgHandler)
    {
        this.emoMsgHandler = newMsgHandler;
    }

    private void Start()
    {
        m_Obj = this.gameObject;
        m_PlayerAnim = this.gameObject.GetComponent<Animator>();
        m_MapId = GameManager.Inst.gameMapInfo.mapId;
        animCon = GetComponent<AnimationController>(); 
        m_PlayerPos = transform.position;
        otherPlayerWaterCtr.Init(this);
        BindAnimator();
        normalAnimClipList = PlayerControlManager.Inst.normalAnimClipList;
        pickupAnimClipList = PlayerControlManager.Inst.pickupAnimClipList;
        attackAnimClipList = PlayerControlManager.Inst.attackAnimClipList;
        shootAnimClipList = PlayerControlManager.Inst.shootAnimClipList;
        iceCubeAttackAnimClipList = PlayerControlManager.Inst.iceCubeAttackAnimClipList;
        iceCubeShootAnimClipList = PlayerControlManager.Inst.iceCubeShootAnimClipList;
        //自拍模式的动画片段
        selfieNormalAnimClipList = PlayerControlManager.Inst.selfieNormalAnimClipList;
        selfieAttackAnimClipList = PlayerControlManager.Inst.selfieAttackAnimClipList;
        selfiePickupAnimClipList = PlayerControlManager.Inst.selfiePickupAnimClipList;
        //滑雪动画片段
        SnowSkateAttackAnimClipList = PlayerControlManager.Inst.SnowSkateAttackAnimClipList;
        SnowSkateShootAnimClipList = PlayerControlManager.Inst.SnowSkateShootAnimClipList;
        
        // 跷跷板动画片段
        SeesawAnimClipList = PlayerControlManager.Inst.SeesawAnimClipList;
        
        //秋千
        SwingAnimClipList = PlayerControlManager.Inst.SwingAnimClipList;

        mPlayerStateManager = new OtherPlayerAnimStateManager(this);
        mPlayerStateManager.Init();
        mPlayerStateManager.SwitchTo(EPlayerAnimState.Idle);
        //钓鱼动画片段
        FishingAnimClipList = PlayerControlManager.Inst.FishingAnimClipList;

        otherFrameStateCtr = FrameStateManager.Inst.AddOtherPlayerFrameStateCtr(this.gameObject);
        otherFrameStateCtr.Init(this, mPlayerStateManager, m_PlayerAnim);
    }

    public string GetMapId()
    {
        return m_MapId;
    }
    
    public void OnFrame(UgcFrameData ugcFrameData)
    {
        m_PlayerPos = ugcFrameData.playerPos;
        m_PlayerRot = ugcFrameData.playerRot;
        m_IsMoving = ugcFrameData.IsMoving;
        m_IsGround = ugcFrameData.IsGround;
        m_IsFlying = ugcFrameData.IsFlying;
        isFastRun = ugcFrameData.IsFastRun;
        
        m_IsInWater = ugcFrameData.IsInWater;
        m_IsSwimming = ugcFrameData.IsSwimming;
        m_AnimType = ugcFrameData.AnimType;
        m_StateType = ugcFrameData.StateType;
        
        m_MapId = ugcFrameData.mapId;
        m_PlayerRot.Normalize();
        
    }

    public void OnRoomChat(RoomChatResp resp)
    {
        var textCharBev = transform.GetComponentInChildren<TextChatBehaviour>();
        var playerDataCom = transform.GetComponent<PlayerData>();
        var syncPlayerInfo = playerDataCom.syncPlayerInfo;

        var roomChatData = JsonConvert.DeserializeObject<RoomChatData>(resp.Msg);
        LoggerUtils.Log("OnRoomChat--other--->" + resp.Msg);
        
        switch ((RecChatType)roomChatData.msgType)
        {
            case RecChatType.Emo:
                var itemData = JsonConvert.DeserializeObject<Item>(roomChatData.data);
                var playEmoData = JsonConvert.DeserializeObject<EmoItemData>(itemData.data);
                SetEmoMsgHandler(EmoMsgManager.Inst.GetEmoMsgHandler(false, this, null, itemData, playEmoData, animCon, textCharBev));
                var emoResult = EmoMsgManager.Inst.CallEmoMsgHandler(emoMsgHandler, (OptType) playEmoData.opt);
                bool isNeedShowInChatWnd = emoMsgHandler.IsNeedShowInChatWnd();
                if (emoResult == false || !isNeedShowInChatWnd)
                {
                    return;
                }
                RoomChatPanel.Instance.SetRecChat((RecChatType)roomChatData.msgType, syncPlayerInfo.userName, roomChatData.data);
                break;
            case RecChatType.TextChat:
                DefeatMsg defeatMsg = JsonConvert.DeserializeObject<DefeatMsg>(roomChatData.data);
                if (defeatMsg != null)
                {
                    RoomChatPanel.Instance.SetRecChat((RecChatType)roomChatData.msgType, syncPlayerInfo.userName, defeatMsg.msg);
                }
                break;
            default:
                RoomChatPanel.Instance.SetRecChat((RecChatType)roomChatData.msgType, syncPlayerInfo.userName, roomChatData.data);
                break;
        }

        if (textCharBev != null)
        {
            textCharBev.OnRecChat(resp);
        }
    }

    public void OnPurchasedChat(string data)
    {
        var textCharBev = transform.GetComponentInChildren<TextChatBehaviour>();
        if (textCharBev != null)
        {
            textCharBev.SetPurchaseText(data);
        }
    }
    
    public void OnRoomCustom(string playerId, RoomChatCustomData customData)
    {
        switch (customData.type)
        {
            case (int)ChatCustomType.Keyboard:
                LoggerUtils.Log($"OnRoomCustom playerId: [{playerId}], data: [{customData.data}]");
                // 降落伞使用中不显示打字动画
                if (otherFrameStateCtr != null && otherFrameStateCtr.IsInParachuteUsing())
                {
                    LoggerUtils.Log("OnRoomCustom Keyboard, parachuteUsing, skip it!");
                    return;
                }
                // 钓鱼状态中不显示打字动画
                if (FishingManager.Inst.GetPlayerFishingStateByPlayerId(playerId))
                {
                    return;
                }
                //滑梯上不显示打字动画
                if (mSlideMovementCompt!=null&&mSlideMovementCompt.IsOnSlide())
                {
                    return;
                }
                //跷跷板上不显示打字动画
                if(SeesawManager.Inst.IsOtherPlayerOnSeesaw(this))
                {
                    return;
                }
                if(SwingManager.Inst.IsOtherPlayerOnSwing(this))
                {
                    return;
                }
                MessageHelper.Broadcast(MessageName.TypeData, new AnimationController.TypeStatusData
                {
                    isStart = customData.data == "1",
                    playerId = playerId,
                });
                break;
        }

    }

    public void OnGetPlayerCustomData(PlayerCustomData playerCustomData)
    {
        if (animCon)
        {
            EmoMsgManager.Inst.OnPlayerCustomData(playerCustomData, animCon);
        }
    }

    public void SetEmoInteractState(bool state)
    {
        emoInterState = state;
    }

    public bool GetEmoInteractState()
    {
        return emoInterState;
    }

    private Vector3 followPlayerCtrlMovingPos = new Vector3(0.37f, 0, -0.6f);
    private Vector3 followPlayerCtrlIdlePos = new Vector3(0.5f, 0, 0);
    
    private void Update()
    {
        if (followPlayerCtrl)
        {
            followPlayerCtrl.PlayAnim("IsMoving", m_IsMoving);

            followPlayerCtrl.PlayAnim("IsGround", m_IsGround);

            followPlayerCtrl.PlayAnim("IsFlying", m_IsFlying);

            followPlayerCtrl.PlayAnim("IsInDoubleEumual", true);
            followPlayerCtrl.PlayAnim("IsStartPlayer", false);

            if (m_IsMoving)
            {
                followPlayerCtrl.transform.localPosition = followPlayerCtrlMovingPos;
            }
            else
            {
                followPlayerCtrl.transform.localPosition = followPlayerCtrlIdlePos;
            }

            // frame anim type
            followPlayerCtrl.m_AnimType = m_AnimType;

            if (followPlayerCtrl.m_AnimType == (int)FrameAnimType.SelfieMode)
            {
                followPlayerCtrl.m_AnimType = (int)FrameAnimType.Normal;
            }
        }
    }

    private void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hitGameObject != hit.transform.gameObject)
            {
                hitGameObject = hit.transform.gameObject;
            }
        }

        if (m_IsMoving && animCon.isPlaying && animCon.isLooping)
        {
            animCon.RecStopLoop();
            // 如果有跟随者，跟随者也需要打断循环
            if (followPlayerCtrl && followPlayerCtrl.animCon.isPlaying && followPlayerCtrl.animCon.isLooping)
            {
                followPlayerCtrl.animCon.RecStopLoop();
            }
            return;
        }
        if (isAvoidFrame == true)
        {
            if (m_PlayerAnim.GetBool("IsMoving"))
            {
                m_PlayerAnim.SetBool("IsMoving", false);
            }
            m_PlayerAnim.SetBool("IsInWater", false);
            m_PlayerAnim.SetBool("IsSwimming", false);
            
            ChangeAnimationClips();

            return;
        }
        if (!IsOnLadderFrame())
        {
            if (avoidLerpCount <= 0)
            {
                if (steeringWheel != null && steeringWheel.carRgb != null)
                {
                    Vector3 OriPos = steeringWheel.carRgb.transform.position;
                    Quaternion OriRot = steeringWheel.carRgb.transform.rotation;
                    var np = m_PlayerPos;
                    np.y += 0.95f;
                    Quaternion lerpRot = Quaternion.Lerp(OriRot, m_PlayerRot, 0.15f);
                    Vector3 lerpPos = Vector3.Lerp(OriPos, np, 0.15f);
                    steeringWheel.carRgb.transform.SetPositionAndRotation(lerpPos, lerpRot);
                }
                else
                {
                    Vector3 OriPos = m_Obj.transform.position;
                    Quaternion OriRot = m_Obj.transform.rotation;

                    Quaternion lerpRot = Quaternion.Lerp(OriRot, m_PlayerRot, 0.15f);
                    Vector3 lerpPos = Vector3.Lerp(OriPos, m_PlayerPos, 0.15f);
                    this.gameObject.transform.SetPositionAndRotation(lerpPos, lerpRot);
                }
            }
            else
            {
                if (steeringWheel != null && steeringWheel.carRgb != null)
                {
                    var np = m_PlayerPos;
                    np.y += 0.95f;
                    steeringWheel.carRgb.transform.SetPositionAndRotation(np, m_PlayerRot);
                }
                else
                {
                    this.gameObject.transform.SetPositionAndRotation(m_PlayerPos, m_PlayerRot);
                }
                if (avoidLerpCount > 0)
                {
                    avoidLerpCount--;
                }
            }
        }
        
        

        m_PlayerAnim.SetBool("IsMoving", m_IsMoving);

        m_PlayerAnim.SetBool("IsGround", m_IsGround);

        m_PlayerAnim.SetBool("IsFlying", m_IsFlying);

        m_PlayerAnim.SetBool("IsInDoubleEumual", _isInEumual);
        m_PlayerAnim.SetBool("IsStartPlayer", _isStartPlayer);

        m_PlayerAnim.SetBool("IsFastRun", isFastRun);

        m_PlayerAnim.SetBool("IsInWater", m_IsInWater);
        m_PlayerAnim.SetBool("IsSwimming", m_IsSwimming);

        if (otherFrameStateCtr != null && otherFrameStateCtr.IsInParachuteUsing() == false)
        {
            FootSound(m_IsMoving, m_IsFlying);
            GroundSound(m_IsGround);
            var isJump = !m_IsFlying && !m_IsGround;
            JumpSound(isJump);
        }
        else
        {
            StopFootSound();
        }
        
        m_PlayerAnim.SetBool("IsJump", isJump);
        otherPlayerWaterCtr.SetSwimEffect();
        
        ChangeAnimationClips();
        if (otherFrameStateCtr)
        {
            otherFrameStateCtr.OnOtherPlayerFixUpdate(m_StateType);
        }

        ChangeEffectPerFixUpdate();
        
    }

    private void  ChangeEffectPerFixUpdate()
    {
        if (CurrentAnimType == FrameAnimType.IceCube && isMoving)
        {
            mPlayerStateManager.SwitchTo(EPlayerAnimState.Skate);
        }
        else if (CurrentAnimType == FrameAnimType.SelfieMode)
        {
            mPlayerStateManager.SwitchTo(EPlayerAnimState.Selfie);
            if (effectTool == null && _restoreTimer == null)
            {
                _restoreTimer = TimerManager.Inst.RunOnce("enterSelfieMode", 3f, () =>
                {
                    if (effectTool == null)
                    {
                        //自拍恢复
                        mPlayerStateManager.RestoreSelfieModeAnim((int)EmoName.EMO_SELFIE_MODE);
                        TimerManager.Inst.Stop(_restoreTimer);
                        _restoreTimer = null;
                    }
                });
            }
        }
        else if (CurrentAnimType == FrameAnimType.SnowCube && isMoving && isFastRun)
        {
            //TODO 雪方块特效？
            // mPlayerStateManager.SwitchTo(EPlayerAnimState.Skate);
        }
        else if (CurrentAnimType == FrameAnimType.Normal && m_IsMoving && isFastRun)
        {
            //播放烟雾
            //mPlayerStateManager.SwitchTo(EPlayerAnimState.FastRun);
        }
        else
        {
            if(SeesawManager.Inst.IsOtherPlayerOnSeesaw(this))
            {
                mPlayerStateManager.SwitchTo(EPlayerAnimState.Seesaw);
            }else
            {
                mPlayerStateManager.SwitchTo(EPlayerAnimState.Idle);
            }
            if(SwingManager.Inst.IsOtherPlayerOnSwing(this))
            {
                mPlayerStateManager.SwitchTo(EPlayerAnimState.Swing);
            }else
            {
                mPlayerStateManager.SwitchTo(EPlayerAnimState.Idle);
            }
        }
        
        mPlayerStateManager.Update();
    }

    public void OnRestart()
    {
        m_IsGround = true;
        m_IsMoving = false;
        m_IsFlying = false;
        _isInEumual = false;
        _isStartPlayer = false;
        OnResetStateEmo();
        //TODO:fsc 降落伞状态重置
    }

    public void PlayAnim(string aniName, bool value)
    {
        if (m_PlayerAnim == null)
        {
            return;
        }
        m_PlayerAnim.enabled = true;
        m_PlayerAnim.SetBool(aniName, value);
    }

    /**
    * 玩家是否为牵手态动画播放
    * isInEumual 玩家是否正在牵手
    * isStartPlayer 玩家是否为牵手发起者(发起者右手动画，跟随者左手动画)
    */
    public void SetPlayerAnimParam(bool isInEumual, bool isStartPlayer)
    {
        _isInEumual = isInEumual;
        _isStartPlayer = isStartPlayer;
    }

    public void SetPlayerFollow(OtherPlayerCtr fPlayer)
    {
        followPlayerCtrl = fPlayer;
        if (fPlayer)
        {
            animCon.RecStopLoop();
            fPlayer.transform.parent = transform;
            fPlayer.transform.rotation = new Quaternion(0, 0, 0, 0);
        }
    }

    public void ResetPlayerState()
    {
        transform.parent = beforePlayerParent;
    }

    //处理因本地坐标世界坐标转换，发帧延迟导致的瞬移问题
    public bool IsOnLadderFrame()
    {
        return LadderManager.Inst != null && !LadderManager.Inst.ContainsOtherCtr(this)
            && (m_StateType >= (int)FrameStateType.LadderUpIn && m_StateType <= (int)FrameStateType.LadderIdel);
    }
    public void PlayJumpOnBoard()
    {
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
        m_PlayerAnim.Play("jump 0");
    }

    public void DrivingCar(bool isDriving)
    {
        m_PlayerAnim.SetBool("IsOnSteering", isDriving);
    }
    
    public void OnEnable()
    {
        isAvoidFrame = false;
        beforePlayerParent = transform.parent;
    }
    
    private void JumpSound(bool isJump)
    {
        if ((isJump && !this.isJump))
        {
            AKSoundManager.Inst.PlayJumpSound("play_default_jump_3p", gameObject);
            
            //滑雪时跳跃，停掉滑雪声音，只播跳跃声
            if (IsInSnowCubeAndSkating() && isPlaySnowSkatingSound)
            {
                isPlaySnowSkatingSound = false;
                SnowCubeManager.Inst.StopSkatingSound(false, m_PlayerAnim.gameObject);
            }
        }
        
        this.isJump = isJump;
    }

    private void GroundSound(bool isGround)
    {
        if (isGround && !this.isGround)
        {
            AKSoundManager.Inst.OtherPlayGroundSound("ground_player_3p","ground_material",
                hitGameObject, gameObject, FrameStateManager.Inst.GetStandOnType(CurrentAnimType));
        }
        
        this.isGround = isGround;
    }

    private void FootSound(bool isMoving, bool isFlying)
    {
        if (otherFrameStateCtr && otherFrameStateCtr.IsInSnowSkating())
        {
            StopFootSound();
            
            //跳跃结束恢复滑雪声音
            if (isJump == false && isPlaySnowSkatingSound == false)
            {
                isPlaySnowSkatingSound = true;
                SnowCubeManager.Inst.PlaySkatingSound(false, m_PlayerAnim.gameObject, IsInSnowForwardSkating());
            }
        }
        if (isFlying)
        {
            if (!this.isFlying)
            {
                StopFootSound();
            }
        }
        else
        {
            if (isMoving)
            {
                if (!this.isMoving || (this.isFlying && !isFlying))
                {
                    PlayFootSound();
                }
            }
            else
            {
                if (this.isMoving)
                {
                    StopFootSound();
                }
            }
        }
        
        this.isFlying = isFlying;
        this.isMoving = isMoving;
    }
    
    private void PlayFootSound()
    {
        if (!AKSoundManager.Inst.isOpenFootSound)
            return;
        if (m_IsInWater)
        {
            CurrentAnimType = FrameAnimType.Water;
        }
        var info = AKSoundManager.Inst.GetOtherFootSoundInfo(FrameStateManager.Inst.GetStandOnType(CurrentAnimType));
        AKSoundManager.Inst.PlayFootSound("foot_player_3p","footstep_material",
            hitGameObject, gameObject, info.switchState);
        if (isFastRun && info.switchState == StandOnAudioType.defaultAudio)
        {
            info.deltaTime = 0.18f;
        }
        Invoke("PlayFootSound", info.deltaTime);
    }
   
    public void StopFootSound()
    {
        CancelInvoke("PlayFootSound");
        if (AudioController.Inst != null)
        {
            AudioController.Inst.StopStepAudio();
        }
    }
   
    public void ForceOutWater()
    {
        m_IsInWater = false;
        m_IsSwimming = false;
        if (m_PlayerAnim)
        {
            m_PlayerAnim.SetBool("IsInWater", false);
            m_PlayerAnim.SetBool("IsSwimming", false);
        }

    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

        if (hit.point.y - (transform.position.y - 0.9f) > 0.01f)
        {
            if (hit.point.y > transform.position.y)
            {
                hit.controller.stepOffset = 0;
            }
            else
            {
                hit.controller.stepOffset = 1;
            }
        }
        if (hitGameObject != hit.gameObject)
        {
            hitGameObject = hit.gameObject;
        }
    }
    public void PlaySwimSound()
    {
        otherPlayerWaterCtr.PlaySwimSound();

    }
    public void PlayWaterIdleSound()
    {
        otherPlayerWaterCtr.PlayWaterIdleSound();
    }

    public OtherPlayerAttackCtr GetOtherPlayerAttackCtl()
    {
        if (otherPlayerAttackCtr == null)
        {
            otherPlayerAttackCtr = gameObject.AddComponent<OtherPlayerAttackCtr>();
        }
        return otherPlayerAttackCtr;
    }

    public OtherPlayerShootCtr GetOtherPlayerShootCtl()
    {
        if (otherPlayerShootCtr == null)
        {
            otherPlayerShootCtr = gameObject.AddComponent<OtherPlayerShootCtr>();
        }
        return otherPlayerShootCtr;
    }

    public OtherPlayerEatOrDrinkCtr GetOtherPlayerEatCtr()
    {
        if (otherPlayerEatOrDrinkCtr == null)
        {
            otherPlayerEatOrDrinkCtr = gameObject.AddComponent<OtherPlayerEatOrDrinkCtr>();
        }
        return otherPlayerEatOrDrinkCtr;
    }
    // 获取这个玩家当前站在什么道具上
    public FrameAnimType GetFrameAnimType()
    {
        return (FrameAnimType) m_AnimType;
    }

    //根据玩家所站在的位置，切换不同动画片段
    public void ChangeAnimationClips()
    {
        if (SeesawManager.Inst.IsOtherPlayerOnSeesaw(this))
        {
            m_IsGround = true;
            m_PlayerAnim.SetBool("IsGround", m_IsGround);
            CurrentAnimType = FrameAnimType.SeeSaw;
            mPlayerStateManager.SwitchTo(EPlayerAnimState.Seesaw);
            return;
        }
        if (SwingManager.Inst.IsOtherPlayerOnSwing(this))
        {
            m_IsGround = true;
            m_PlayerAnim.SetBool("IsGround", m_IsGround);
            CurrentAnimType = FrameAnimType.Swing;
            mPlayerStateManager.SwitchTo(EPlayerAnimState.Swing);
            return;
        }
        var newAnimType = GetFrameAnimType();
        if (CurrentAnimType != newAnimType)
        {
            CurrentAnimType = newAnimType;
            switch (newAnimType)
            {
                case FrameAnimType.Normal:
                    RecoverAnimClipByIndex(lastAnimClipIndex);
                    mPlayerStateManager.SwitchTo(EPlayerAnimState.Idle);
                    break;
                case FrameAnimType.IceCube: 
                    SwitchIceCubeAnimClips();
                    break;
                case FrameAnimType.SelfieMode:
                    SwitchSelfieModeAnimClips();
                    mPlayerStateManager.SwitchTo(EPlayerAnimState.Selfie);
                    break;
                case FrameAnimType.SnowCube:
                    //雪方块上慢走不处理，上滑板再做动画切换等表现
                    break;
                default:
                    break;
            }
        }
    }

    /**
     * 
    * 绑定动画状态机和动画器重写控制器
    */
    private void BindAnimator()
    {
        if(m_PlayerAnim == null)
        {
            return;
        }
        overrideController = new AnimatorOverrideController(m_PlayerAnim.runtimeAnimatorController);
        m_PlayerAnim.runtimeAnimatorController = overrideController;
        clipOverrides = new AnimationClipOverrides(overrideController.overridesCount);
        //获取动画器重写控制器中当前定义的动画剪辑重写的列表
        overrideController.GetOverrides(clipOverrides);
    }

    /**
    * 批量更新动画片段
    */
    public void UpdateAnimClips(List<ClipItem> clipList)
    {
        if(m_PlayerAnim == null)
        {
            return;
        }
        foreach (var clip in clipList)
        {
            if (clip != null)
            {
                var animName = GameUtils.AnimTypeAndNameDict[clip.clipKey];
                clipOverrides[animName] = clip.clipValue;
            }
        }
        animCon.RleasePrefab();
        animCon.CancelLastEmo();

        if (gameObject.activeInHierarchy)
        {
            StopCoroutine("ChangeAnimator");
            StartCoroutine("ChangeAnimator");
        }
    }

    public IEnumerator ChangeAnimator()
    {
        yield return null;
        // 批量替换动画片段列表
        overrideController.ApplyOverrides(clipOverrides);
        m_PlayerAnim.runtimeAnimatorController = overrideController;
    }

    /**
    * 更新某一个动画片段
    */
    // public void UpdateOneAnimClip(AnimClipType type, AnimationClip clip)
    // {
    //     var animName = GameUtils.AnimTypeAndNameDict[type];
    //     clipOverrides[animName] = clip;
    //     // 批量替换动画片段列表
    //     overrideController.ApplyOverrides(clipOverrides);
    //     m_PlayerAnim.runtimeAnimatorController = overrideController;
    // }

    /**
    * 批量切换为普通动作
    */
    public void SwitchNormalAnimClips()
    {
        if (CurrentAnimType == FrameAnimType.IceCube)
        {
            lastAnimClipIndex = 0;
            SwitchIceCubeAnimClips();
            return;
        }
        if (CurrentAnimType == FrameAnimType.SelfieMode)
        {
            lastAnimClipIndex = 0;
            SwitchSelfieModeAnimClips();
            return;
        }
        if (IsInSnowCubeAndSkating())
        {
            lastAnimClipIndex = 0;
            SwitchSnowCubeAnimClips();
            return;
        }

        if(SeesawManager.Inst.IsOtherPlayerOnSeesaw(this))
        {
            lastAnimClipIndex = 0;
            SwitchSeesawAnimClips();
            return;
        }
        
        if(SwingManager.Inst.IsOtherPlayerOnSwing(this))
        {
            lastAnimClipIndex = 0;
            SwitchSwingAnimClips();
            return;
        }
        UpdateAnimClips(normalAnimClipList);
        lastAnimClipIndex = 0;
    }

    /**
    * 批量切换为拾取动作
    */
    public void SwitchPickupAnimClips()
    {
        if (CurrentAnimType == FrameAnimType.IceCube)
        {
            lastAnimClipIndex = 1;
            SwitchIceCubeAnimClips();
            return;
        }
        if (CurrentAnimType == FrameAnimType.SelfieMode)
        {
            lastAnimClipIndex = 1;
            SwitchSelfieModeAnimClips();
            return;
        }
        if (IsInSnowCubeAndSkating())
        {
            lastAnimClipIndex = 1;
            SwitchSnowCubeAnimClips();
            return;
        }
        UpdateAnimClips(pickupAnimClipList);
        lastAnimClipIndex = 1;
    }

    /**
    * 批量切换为攻击动作
    */
    public void SwitchAttackAnimClips()
    {
        if (CurrentAnimType == FrameAnimType.IceCube)
        {        
            lastAnimClipIndex = 2;
            SwitchIceCubeAnimClips();
            return;
        }
        if (CurrentAnimType == FrameAnimType.SelfieMode)
        {
            lastAnimClipIndex = 2;
            SwitchSelfieModeAnimClips();
            return;
        }
        if (IsInSnowCubeAndSkating())
        {
            lastAnimClipIndex = 2;
            SwitchSnowCubeAnimClips();
            return;
        }
        UpdateAnimClips(attackAnimClipList);
        lastAnimClipIndex = 2;
    }

    /**
    * 批量切换为射击动作
    */
    public void SwitchShootAnimClips()
    {
        if (CurrentAnimType == FrameAnimType.IceCube)
        {
            lastAnimClipIndex = 3;
            SwitchIceCubeAnimClips();
            return;
        }
        if (IsInSnowCubeAndSkating())
        {
            lastAnimClipIndex = 3;
            SwitchSnowCubeAnimClips();
            return;
        }
        UpdateAnimClips(shootAnimClipList);
        lastAnimClipIndex = 3;
    }

    ///////////////////////////////////////////////////// 各模块切换动画片段使用 todo:待移到外部统一管理 ///////////////////////////////////////
    
    /**
    * 批量切换为滑冰动作,又区分为滑冰攻击、滑冰射击
    */
    public void SwitchIceCubeAnimClips()
    {
        if ((FrameAnimType)m_AnimType != FrameAnimType.IceCube)
        {
            return;
        }
        
        if (lastAnimClipIndex == 0 || lastAnimClipIndex == 2)
        {
            UpdateAnimClips(iceCubeAttackAnimClipList);
        }
        else if (lastAnimClipIndex == 3)
        {
            UpdateAnimClips(iceCubeShootAnimClipList);
        }
    }

    /// <summary>
    /// 批量切换为自拍模式动作
    /// </summary>
    public void SwitchSelfieModeAnimClips()
    {
        if ((FrameAnimType)m_AnimType != FrameAnimType.SelfieMode)
        {
            return;
        }

        if (lastAnimClipIndex == 0)
        {
            UpdateAnimClips(selfieNormalAnimClipList);
        }
        else if (lastAnimClipIndex == 1)
        {
            UpdateAnimClips(selfiePickupAnimClipList);
        }
        else if (lastAnimClipIndex == 2)
        {
            UpdateAnimClips(selfieAttackAnimClipList);
        }
    }

    /// <summary>
    /// 批量切换为跷跷板动作
    /// </summary>
    public void SwitchSeesawAnimClips()
    {
        if(SeesawManager.Inst.IsOtherPlayerOnSeesaw(this))
        {
            UpdateAnimClips(SeesawAnimClipList);
        }
    }
    
    public void SwitchSwingAnimClips()
    {
        if(SeesawManager.Inst.IsOtherPlayerOnSeesaw(this))
        {
            UpdateAnimClips(SwingAnimClipList);
        }
    }

    //下压跷跷板
    public void PushSeesaw()
    {
        animCon.PlayAnim((int)EmoName.EMO_SEESAW_PUSH);
        AKSoundManager.Inst.PlaySeesawSound("Play_Seesaw_Pushdown", gameObject);
    }
    
    public void Swingfront()
    {
        PlayerControlManager.Inst.ChangeAnimClips();
        AKSoundManager.Inst.PlaySwingSound("Play_Swing_Forward", gameObject);
        animCon.playerAnim.CrossFade("swing_front", 0.2f, 0, 0f);
        // animCon.playerAnim.Play("swing_front", 1, 0f);
    }
    
    public void SwingBack()
    {
        AKSoundManager.Inst.PlaySwingSound("Play_Swing_Backward", gameObject);
        animCon.playerAnim.CrossFade("swing_back", 0.2f, 0, 0f);
        // animCon.playerAnim.Play("swing_back", 1, 0f);
    }
    
    public void SwingIdle()
    {
        animCon.playerAnim.CrossFade("swing_idle", 0.2f, 0, 0f);
        // animCon.playerAnim.Play("swing_idle", 1, 0f);
    }

    /**
    * 批量切换为滑雪动作,又区分为滑冰攻击、滑冰射击
    */
    public void SwitchSnowCubeAnimClips()
    {
        if (IsInSnowCubeAndSkating() == false)
        {
            return;
        }

        if (lastAnimClipIndex == 0 || lastAnimClipIndex == 2)
        {
            UpdateAnimClips(SnowSkateAttackAnimClipList);
        }
        else if (lastAnimClipIndex == 3)
        {
            UpdateAnimClips(SnowSkateShootAnimClipList);
        }
    }

    //离开滑冰时，用来恢复原来的animClip，不记录滑冰的currentAnimClipIndex
    public void RecoverAnimClipByIndex(int index)
    {
        switch (index)
        {
            case 0: SwitchNormalAnimClips(); break;
            case 1: SwitchPickupAnimClips(); break;
            case 2: SwitchAttackAnimClips(); break;
            case 3: SwitchShootAnimClips(); break;
            default: SwitchNormalAnimClips(); break;
        }
    }

    public void SwitchFishingAnimClips()
    {
        UpdateAnimClips(FishingAnimClipList);
    }
    
    //雪方块动画切换前判定
    public bool IsInSnowCubeAndSkating()
    {
        return CurrentAnimType == FrameAnimType.SnowCube && otherFrameStateCtr != null && otherFrameStateCtr.IsInSnowSkating();
    }

    public bool IsInSnowForwardSkating()
    {
        return IsInSnowCubeAndSkating() && otherFrameStateCtr.CurFramStateType == FrameStateType.SnowCubeFastRunForward;
    }

    public void OnTalkSend(bool isTalking)
    {
        animCon.OnTalkSend(isTalking);
    }
    public void OnResetStateEmo()
    {
        //pvp模式下结算时刻服务器把玩家状态清除了，需要直接关闭加好友发起界面、清除数据
        if (animCon.curStateEmo != EmoName.EMO_ADD_FRIEND)
        {
            return;
        }
        SetOtherPlayerTouchable((int)EmoName.EMO_ADD_FRIEND, false);
        animCon.SetStateEmo(EmoName.None);
    }
    public void SetOtherPlayerTouchable(int emoId,bool state)
    {
        var touchBev = GetComponent<PlayerTouchBehaviour>();
        touchBev.SetCanTouch(state, emoId);
    }
    public void OnDestroy()
    {
        if (mSlideMovementCompt!=null)
        {
            mSlideMovementCompt.Destroy();
        }
    }
}

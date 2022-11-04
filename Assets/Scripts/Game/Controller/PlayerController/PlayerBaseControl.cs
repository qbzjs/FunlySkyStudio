using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Cinemachine;
using Newtonsoft.Json;
using DG.Tweening;
using HLODSystem;
using Action = System.Action;
using BudEngine.NetEngine;
using System;

public enum EObjAbilityType
{
    Move,
    Pickability,
    SelfieMode,
    Emo,
    Max,
}
public enum FlyStatus
{
    Up = 5,
    down = -5,
    stop = 0
}
public delegate void PlayerStateChangedFunc(IPlayerCtrlMgr iPlayer);
/// <summary>
/// Author:WenJia
/// Description:Player 基本能力控制
/// 包含胶囊体、跳、飞、跑等基本的能力
/// 此文件一般情况不做太多改动
/// Date: 2022/3/31 11:14:20
/// </summary>
public class PlayerBaseControl : MonoBehaviour, IPlayerCtrlMgr
{
    public float speed;
    public float jumpForce;

    #region 冰方块移动参数

    private float ice_max_speed = 7f;
    private float ice_max_speed_sqr = 7f * 7f;
    private float ice_add_speed = 0.3f;
    private float ice_del_speed = 0.2f;

    #endregion
    [HideInInspector]
    public Vector3 moveVec;
    public Vector3 curmoveVec;
    public CharacterController character;
    public CharacterController Character
    {
        get
        {
            if (!character)
            {
                character = GetComponent<CharacterController>();
            }
            return character;
        }
    }

    public float gravity;
    public float jumpDamp;
    public float stepDamp;
    public Vector3 initPos = new Vector3(0, 1.3f, 0);
    [SerializeField]
    Transform lookCenter;
    [HideInInspector]
    public Vector3 upwardVec;
    public CinemachineVirtualCamera cam;
    public CinemachineTransposer transposer;
    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool isFlying = false;
    [HideInInspector]
    public bool isGround = true;
    [HideInInspector]
    public bool isInSelfieMode = false;
    public Animator playerAnim;
    public RuntimeAnimatorController playerFPVAnimCon;
    public RuntimeAnimatorController playerNormalAnimCon;
    public RuntimeAnimatorController playerShootAnimCon;
    public bool isChanged = false;

    [HideInInspector]
    public bool isTps = true;
    private GameObject hitGameObject;
    public AnimationController animCon;
    [HideInInspector]
    public GameMode curGameMode;
    [HideInInspector]
    public bool waitPosChange = false; //wait after changing position.(cannot move while waiting)
    public static PlayerBaseControl Inst;
    public bool isOriginalWalkMode = false; // 是否为默认的跑步模式 (模式新增了快走)
    [HideInInspector]
    public bool isOriginalFlyMode = true;
    [HideInInspector]
    public bool canUseAutoMateMode = false;
    public bool isFastRun = false, IsJump = false;
    private GameObject parentPlayer;
    private GameObject ptrigger;
    float flyDir;
    public PlayerRoleData playerRoleData;
    public float moveY;
    public AnimatorOverrideController overrideController;
    protected AnimationClipOverrides clipOverrides;
    public bool isInWater = false;//双人牵手时记录玩家是否在水方块

    private GameObject head, face, hair;
    private SkinnedMeshRenderer cloth;
    public PlayerAnimStateManager mAnimStateManager;
    //骨骼头部路径
    const string BONE_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 Head";

    public GameObject playerCenter;
    
    public event PlayerStateChangedFunc mPlayerStateChangedFunc;

   
    public bool IsTps
    {
        get{return isTps; }
        set 
        {
            isTps = value;
            if (mPlayerStateChangedFunc!=null)
            {
                mPlayerStateChangedFunc(this);
            }
        }
    }
    private void Awake()
    {
        Inst = this;
        isOriginalWalkMode = !GlobalSettingManager.Inst.IsAutoRunningOpen();
        isOriginalFlyMode = GlobalSettingManager.Inst.GetFlyingMode() == FlyingMode.Original;
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.Base, Inst);
        mNoAbilites = new int[(int)EObjAbilityType.Max];
        Array.Clear(mNoAbilites, 0, mNoAbilites.Length);
        character = GetComponent<CharacterController>();
        animCon = GetComponent<AnimationController>();
        gravity = Physics.gravity.y;
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        parentPlayer = GameObject.Find("FlyPoint");
        ptrigger = GameObject.Find("PTrigger");
        playerCenter = playerAnim.transform.Find("playerCenter").gameObject;
        head = playerAnim.transform.Find(BONE_PATH).gameObject;
        face = playerAnim.transform.Find("body_face").gameObject;
        hair = playerAnim.transform.Find("hair").gameObject;
        cloth = playerAnim.transform.Find("clothing_01").GetComponent<SkinnedMeshRenderer>();
        BindAnimator();

        mAnimStateManager = new PlayerAnimStateManager(this);
        mAnimStateManager.Init();
        mAnimStateManager.SwitchTo(EPlayerAnimState.Idle);

    }
    public void AddPlayerStateChangedObserver(PlayerStateChangedFunc observer)
    {
        mPlayerStateChangedFunc += observer;
    }
    public void RemovePlayerStateChangedObserver(PlayerStateChangedFunc observer)
    {
        mPlayerStateChangedFunc -= observer;
    }
    /**
       * 玩家恢复Idle状态
       */
    public void PlayerResetIdle()
    {
        BounceplankLand();
        isFastRun = false;
        isMoving = false;
        isFlying = false;
        isGround = true;
        gravity = Physics.gravity.y;
        moveY = 0;
        PlayAnimation(AnimId.IsFlying, false);
        PlayAnimation(AnimId.IsMoving, false);
        PlayAnimation(AnimId.IsInDoubleEumual, false);
        PlayAnimation(AnimId.IsStartPlayer, false);
        PlayAnimation(AnimId.IsGround, true);
        PlayAnimation(AnimId.IsInWater, false);
        PlayAnimation(AnimId.IsSwimming, false);
        PlayAnimation(AnimId.IsJump, false);
        ResetJoystick();
        StopFootSound();
        if (PlayModePanel.Instance != null)
        {
            PlayModePanel.Instance.OnSetDownButton(true);
        }

        if (animCon != null)
        {
            animCon.OnEmoKill();
            animCon.playerAnim.Play("idle", 0, 0);
        }
        mAnimStateManager.SwitchTo(EPlayerAnimState.Idle);
        PlayerEmojiControl.Inst.OnReset();
        //ClearNoAbilityFlag(EObjAbilityType.Move);
    }

    public void GoToSpawnPoint()
    {
        Invoke("SetPosToSpawnPoint", 2f);
    }

    public void SetPlayerPos(Vector3 pos, Quaternion rotation)
    {
        StartCoroutine(WaitForNewPosition(pos, rotation, 0.7f));
    }

    private IEnumerator WaitForNewPosition(Vector3 pos, Quaternion rot, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        SetPlayerPositionAndRotation(pos + initPos, rot);
        StartCoroutine(WaitPosJump());
    }

    public void SetPosToSpawnPoint()
    {
        if (ReferManager.Inst.isRefer)
        {
            return;
        }
        var spawnPos = SpawnPointManager.Inst.GetSpawnPoint().transform.localPosition;
        var spawnRot = SpawnPointManager.Inst.GetSpawnPoint().transform.localRotation;
        SetPlayerPositionAndRotation(spawnPos + initPos, spawnRot);
        playerAnim.transform.rotation = new Quaternion(0, 0, 0, 0);
        lookCenter.localRotation = Quaternion.identity;
        DontHandleMove();
        BounceplankLand();
    }

    public void DontHandleMove()
    {
        if (gameObject.activeSelf)
            StartCoroutine(WaitPosJump());
    }

    public void SetPosToNewPoint(Vector3 pos, Quaternion rot)
    {
        SetPlayerPositionAndRotation(pos + initPos, rot);
        playerAnim.transform.rotation = new Quaternion(0, 0, 0, 0);
        lookCenter.localRotation = Quaternion.identity;
    }
    public void SetPosToNewPointWithoutIniPos(Vector3 pos, Quaternion rot)
    {
        SetPlayerPositionAndRotation(pos, rot);
        playerAnim.transform.rotation = new Quaternion(0, 0, 0, 0);
        //lookCenter.localRotation = Quaternion.identity;
    }
    public void SetLookCenter(Quaternion rot)
    {
        lookCenter.localRotation = rot;
    }
    public void SetPlayerPositionAndRotation(Vector3 pos, Quaternion rot)
    {
        transform.SetPositionAndRotation(pos, rot);
        DeletePlayerPlaceHolder();
        Physics.SyncTransforms();

    if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            CoroutineManager.Inst.StartCoroutine(ResetHLODController());
        }
    }

    private IEnumerator ResetHLODController()
    {
        yield return null;
        HLOD.Inst.ResetController();
    }

    private void DeletePlayerPlaceHolder()
    {
        //TODO: 在设置角色位置时，避免陷入包围盒包裹中
        float minValue = !isTps ? 10 : 5;
        Collider[] curHits = Physics.OverlapSphere(this.transform.position, minValue, LayerMask.GetMask("Airwall"));
        foreach (var collider in curHits)
        {
            if (collider.gameObject.name.StartsWith("PlaceHolder"))
            {
                Destroy(collider);
            }
        }
    }

    private IEnumerator WaitPosJump()
    {
        waitPosChange = true;
        yield return new WaitForSeconds(0.06f);
        waitPosChange = false;
    }
    
    public void Move(Vector3 screenOffset, bool isNotRot = false)
    {
        //有些地方在PlayerBaseController还没有Awake就调用进来。
        if (Inst!=null
            &&GetNoAbilityFlag(EObjAbilityType.Move)
            &&screenOffset!=Vector3.zero)
        {
            isMoving = false;
            isFastRun = false;
            curmoveVec = Vector3.zero;
            moveVec = Vector3.zero;
            return;
        }

        if (animCon.CanPlayerMove())
        {
            if (screenOffset == Vector3.zero)
            {
                isMoving = false;
                isFastRun = false;
                if (playerAnim.gameObject.activeSelf)
                {
                    isMoving = false;
                    PlayAnimation(AnimId.IsMoving, false);
                    PlayAnimation(AnimId.IsFastRun, false);

                    //不使用此处切特效，由对应管理器自行接管
                    if (!StateManager.IsParachuteUsing && !StateManager.IsSnowCubeSkating)
                    {
                        mAnimStateManager.SwitchTo(EPlayerAnimState.Idle);
                    }
                }
            }
            else
            {
                isMoving = true;
                if (playerAnim.gameObject.activeSelf)
                {
                    isMoving = true;
                    PlayAnimation(AnimId.IsMoving, true);
                    PlayAnimation(AnimId.IsFastRun, isFastRun);
                }
            }

            //计算人物速度
            Vector3 move = screenOffset.y * transform.forward + screenOffset.x * transform.right;

            float curSpeed = speed;
            var canUseFastModel = PlayerCanUseFastRunMode();
            if (canUseFastModel)
            {
                curSpeed = speed * 1.5f;
                isFastRun = true;
            }
            else
            {
                isFastRun = false;
            }

            if (!isOriginalFlyMode && isFlying)
            {
                FreeFlyMode(screenOffset);
                curSpeed = speed * 1.8f;
            }

            //计算移动向量
            if (PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsShouldRunInIceCube())
            {
                moveVec = curmoveVec + ice_add_speed * move.normalized;
            }
            else
            {
                moveVec = curSpeed * move.normalized;
            }
            
            moveVec.y = moveY; 
            
            //雪方块处理摇杆输入
            if (PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsCanUseSnowMove())
            {
                PlayerSnowSkateControl.Inst.OnJoystickMove(ref move, ref curmoveVec, ref moveVec, ref upwardVec);
            }
            
            curmoveVec = moveVec;
            //控制人物旋转
            if (moveVec != Vector3.zero)
            {
                if (playerAnim.gameObject.activeSelf && (moveVec.x != 0 || moveVec.z != 0))
                {
                    if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard && !isInSelfieMode)
                    {
                        float rotY = Vector3.Angle(new Vector3(0, 1, 0), screenOffset.normalized);
                        if (screenOffset.normalized.x < 0)
                        {
                            rotY *= -1;
                        }
                        playerAnim.transform.localRotation = Quaternion.Euler(0, rotY, 0);
                    }
                    else
                    {
                        if (!isTps || (!isOriginalFlyMode && isFlying) || isInSelfieMode)
                        {
                            //自由飞行模式和第一人称视角下不用此移动
                            //相机自拍模式下按第一人称方式运动
                            //TODO:第一人称摇杆无法控制人物旋转，已知问题
                        }
                        else if (PlayerParachuteControl.Inst && PlayerParachuteControl.Inst.IsCanHandleRotate())
                        {
                            //降落伞旋转
                            if (move != Vector3.zero)
                            {
                                PlayerParachuteControl.Inst.OnFallingRotate(screenOffset);
                            }
                        }
                        else if (SnowCubeManager.Inst.IsStandOnSnowCube() && PlayerSnowSkateControl.Inst)
                        {
                            PlayerSnowSkateControl.Inst.OnPlayerRotate(curmoveVec, moveVec);
                        }
                        else
                        {
                            if (move != Vector3.zero)
                            {
                                playerAnim.transform.rotation = Quaternion.LookRotation(move.normalized);
                            }
                            else
                            {
                                //松开摇杆时，若是滑冰，保持松开摇杆前的人物朝向
                                playerAnim.transform.rotation = Quaternion.LookRotation(curmoveVec.normalized);
                            }
                        }
                    }
                }
                else if(moveVec.x != 0 || moveVec.z != 0)
                {
                    #region 雪方块支持第一视角旋转，人物保持垂直雪面 TODO:待第一人称优化不隐藏player时可去除

                    if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard && !isInSelfieMode)
                    {
                    }
                    else if (SnowCubeManager.Inst.IsStandOnSnowCube() && PlayerSnowSkateControl.Inst)
                    {
                        PlayerSnowSkateControl.Inst.OnPlayerRotate(curmoveVec, moveVec);
                    }

                    #endregion
                }
            }
            if (gameObject.activeSelf)
            {
                PlayStepSound(screenOffset);
            }
        }
        else
        {
            AudioController.Inst.audioState = MoveAudioState.None;
            StopFootSound();
            animCon.StopLoop();
        }
        if (mAnimStateManager!=null)
        {
            UpdateAnimStateMachine();
        }
    }
    
    private void UpdateAnimStateMachine()
    {
        if (isMoving && isFastRun)
        {
            //mAnimStateManager.SwitchTo(EPlayerAnimState.FastRun);
        }
        if (!isMoving && !StateManager.IsParachuteUsing &&  !StateManager.IsSnowCubeSkating)
        {
            mAnimStateManager.SwitchTo(EPlayerAnimState.Idle);
        }
    }
    /**
    * 玩家是否可以使用快走模式
    */
    private bool PlayerCanUseFastRunMode()
    {
        //冰方块上无视全局设定的快跑设置，强制打开快跑
        if (isOriginalWalkMode && !(PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsStandOnIceCube()))
        {
            return false;
        }
        if (isFlying || !canUseAutoMateMode)
        {
            return false;
        }
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            return false;
        }

        if (PlayerSwimControl.Inst && PlayerSwimControl.Inst.isInWater)
        {
            return false;
        }

        if (PlayerStandonControl.Inst && !PlayerStandonControl.Inst.IsCanFastRun())
        {
            return false;
        }
        if (isInSelfieMode)
        {
            return false;
        }
        return true;
    }

    private void OnDisable()
    {
        if (AudioController.Inst != null)
        {
            AudioController.Inst.StopFlyAudio();
            AudioController.Inst.StopStepAudio();
        }
        StopFootSound();
    }

    public void ResetJoystick()
    {
        Move(Vector3.zero);
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.joyStick.JoystickReset();
        }
    }

    public void SetJoystickReset(Action callback)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ResetJoystick(callback));
        }
    }

    private IEnumerator ResetJoystick(Action callback)
    {
        InputReceiver.locked = true;
        ResetJoystick();
        callback?.Invoke();
        yield return null;
        InputReceiver.locked = false;
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

            //add layer judgement by temporarily
            if (hitGameObject.layer == LayerMask.NameToLayer("Model"))
            {
                var nodeBehav = hitGameObject.GetComponentInParent<NodeBaseBehaviour>();
                if (nodeBehav)
                {
                    //invoke every time when first hit the game object.
                    nodeBehav.OnColliderHit();
                }
            }
        }

        if (hit.gameObject.layer == LayerMask.NameToLayer("Airwall"))
        {
            if (hit.gameObject.name.StartsWith("PlaceHolder"))
            {
                // 占位空气墙不显示 toast
                return;
            }
            if (hit.gameObject.name.Equals("pvpBox"))
            {
                if (PVPWaitAreaManager.Inst.IsPVPGameStart)
                {
                    TipPanel.ShowToast("You could not enter Waiting Zone since game starts");
                }
                else
                {
                    TipPanel.ShowToast("The game has not started yet. Please stay in the Waiting Zone.");
                }
                return;
            }
            if (hit.gameObject.name.StartsWith("vipAreaBox"))
            {
                var tips = "You could only enter the VIP Zone through the door";
                if(VIPZoneManager.Inst.IsInArea)
                {
                    tips = "You could only leave the VIP Zone through the door";
                }
                TipPanel.ShowToast(tips);
                return;
            }
            if (hit.gameObject.name.Equals("pvpBox_Bot"))
                return;
            TipPanel.ShowToast("You have reached the end:)");
        }
    }

    private void PlayGroundSound()
    {
        AKSoundManager.Inst.PlayFootSound("ground_player", "ground_material",
            hitGameObject, gameObject);
    }

    public void PlayFootSound()
    {
        if (!AKSoundManager.Inst.isOpenFootSound)
            return;
        var info = AKSoundManager.Inst.GetPlayerFootSoundInfo();
        AKSoundManager.Inst.PlayFootSound("foot_player", "footstep_material",
    hitGameObject, gameObject, info.switchState);
        if (isFastRun && info.switchState == StandOnAudioType.defaultAudio)
        {
            info.deltaTime = 0.18f;
        }
        
        if (!IsInvoking("PlayFootSound"))
        {
            Invoke("PlayFootSound", info.deltaTime);
        }
    }

    public void StopFootSound()
    {
        CancelInvoke("PlayFootSound");
        if (AudioController.Inst != null)
        {
            AudioController.Inst.StopStepAudio();
        }
    }

    private void PlayStepSound(Vector3 screenOffset)
    {
        if (ReferManager.Inst.isRefer)
        {
            return;
        }
        //如果在雪方块上板子，不使用脚步声，用循环声音代替
        if (PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsSnowSkating())
        {
            AudioController.Inst.audioState = MoveAudioState.None;
            StopFootSound();
            return;
        }
        if (PlayerIsGround())
        {
            if (screenOffset != Vector3.zero)
            {
                //if (AudioController.Inst.audioState == MoveAudioState.None)
                //{
                //    AudioController.Inst.audioState = MoveAudioState.Move;
                //    PlayFootSound();
                //}
            }
            else
            {
                //AudioController.Inst.audioState = MoveAudioState.None;
                //StopFootSound();
            }
        }
        else
        {
            StopFootSound();
        }
    }

    public bool PlayerIsGround()
    {
        if (PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsGroundDetect())
        {
            return PlayerStandonControl.Inst.GetIsGround();
        }
        
        return Character && Character.isGrounded;
    }

    //旧版的直接使用CC判断IsGround
    public bool PlayerIsGroundByCc()
    {
        return Character && Character.isGrounded;
    }

    public void Jump()
    {
        if (Inst != null && GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return;
        }
        if (animCon.CanPlayerMove())
        {
            if (PlayerIsGround())
            {
                upwardVec.y = jumpForce;
                isGround = false;
                IsJump = true;
                //AudioController.Inst.audioState = MoveAudioState.Jump;
                //AudioController.Inst.StopStepAudio();
                //AudioController.Inst.PlayJumpAudio();
                PlayAnimation(AnimId.IsJump, true);

                if (PlayerStandonControl.Inst)
                {
                    PlayerStandonControl.Inst.OnClickJump();
                }
                if (PlayerSnowSkateControl.Inst)
                {
                    PlayerSnowSkateControl.Inst.OnClickJump();
                }
                
                IsJump = false;
            }
        }
        else
        {
            animCon.StopLoop();

            if (PlayerMutualControl.Inst)
            {
                PlayerMutualControl.Inst.StopFollowerLoop();
            }
        }
    }
    public bool isBounceplankJumping;
    public Vector3 bounceplnakHVec;
    public void BounceplankJump(BounceplankBehaviour behaviour)
    {
        if (Character&& behaviour != null)
        {
            int height = behaviour.GetHeight();
            isBounceplankJumping = true;
            bounceplnakHVec = behaviour.gameObject.transform.up;
            bounceplnakHVec *= height;
            upwardVec.y = bounceplnakHVec.y;
            bounceplnakHVec.y = 0;
            isGround = false;
        }


    }
  
    public void SetFly(bool isFly, bool islanded = false)
    {
        if (isFly == isFlying && isFlying == false)
        {
            return;
        }
        if (isFly)
        {
            if (playerAnim.gameObject.activeSelf)
            {
                isFlying = true;
                isFastRun = false;
                PlayAnimation(AnimId.IsFlying, true);
            }
            if (PlayerIsGround())
            {
                if (isOriginalFlyMode)
                {
                    StartCoroutine(StartFly(isFly));
                }
                else
                {
                    SetFreeFlyHight(isFly);
                }
                AudioController.Inst.StopStepAudio();
                AudioController.Inst.PlayJumpAudio();
            }
            else
            {

                isFlying = isFly;
                upwardVec.y = 0;
                AudioController.Inst.PlayFlyAudio();
            }
            gravity = 0;
            if (AudioController.Inst.audioState != MoveAudioState.Fly)
            {
                AudioController.Inst.audioState = MoveAudioState.Fly;
                AudioController.Inst.PlayFlyAudio();
            }
        }
        else
        {
            if (playerAnim.gameObject.activeSelf)
            {
                isFlying = false;
                PlayAnimation(AnimId.IsFlying, false);
            }
            gravity = Physics.gravity.y;
            upwardVec.y = 0;
            isFlying = isFly;
            FlySpeed(FlyStatus.stop);
            if (isOriginalFlyMode == false)
            {
                SetEndFlyPlayerPos();
            }
            AudioController.Inst.StopFlyAudio();
        }
    }

    IEnumerator StartFly(bool isfly)
    {
        upwardVec.y = 12;
        yield return new WaitForFixedUpdate();
        isFlying = isfly;
        upwardVec.y = 0;

        if (!isTps)
        {
            StartCoroutine(SetPlayerRoleActive(0.3f, false));
        }

    }

    public void ResetUpwardVec()
    {
        StartCoroutine(ResetUpwardVecCor());
    }

    private IEnumerator ResetUpwardVecCor()
    {
        yield return null;
        //等待1帧，Character.isGrounded 判断角色在空中，才复位向下速度
        //复位 upwardVec 会导致 Character.isGrounded => false
        if (Character != null && !Character.isGrounded)
        {
            upwardVec.y = 0;
        }
    }

    public void FlySpeed(FlyStatus speed)
    {
        if (animCon.CanPlayerMove())
        {
            moveVec.y = (int)speed;
            moveY = (int)speed;
        }
        else
        {
            animCon.StopLoop();
        }
    }

    private void Update()
    {
        if (playerAnim.gameObject.activeSelf)
        {
            if (!PlayerOnBoardControl.Inst || !PlayerOnBoardControl.Inst.isOnBoard)
            {
                PlayAnimation(AnimId.IsGround, isGround);
            }
        }
        //if (transform.position != AudioController.Inst.transform.position)
        //{
        //    AudioController.Inst.gameObject.transform.position = transform.position;
        //}
        UpdatePlayerState();
        // if (!isTps)
        // {
        //     bool visible = PlayerRoleVisible();
        //     playerAnim.gameObject.SetActive(visible);
        // }
    }
    private void UpdatePlayerState()
    {
        if (!isGround && isFlying)
        {
            BounceplankLand();
            PlayerFly();
        }

        if (!GetNoAbilityFlag(EObjAbilityType.Move))
        {
            if (PlayerIsGround())
            {
                // 在雪方块上，由于存在斜面移动，不重置y的向量值
                if (PlayerSnowSkateControl.Inst == null || !PlayerSnowSkateControl.Inst.IsCanUseSnowMove())
                {
                    moveVec.y = 0;
                }
            }
            else
            {
                isGround = false;
                upwardVec.y += gravity * Time.deltaTime;
            }
        }

        RefreshPlayerDamp();
        if (!isOriginalFlyMode && isFlying && CheckFlyHight())
        {
            SetFly(false);
        }


        if (PlayerSwimControl.Inst)
        {
            PlayerSwimControl.Inst.JudgeSwimLimit();
        }
        if (!GetNoAbilityFlag(EObjAbilityType.Move))
        {
            PlayerMove();
        }
        PlayerLand();
        mAnimStateManager.Update();//这个必须再Move后面
    }

    /**
    * 玩家飞行
    */
    private void PlayerFly()
    {
    
        if (playerAnim.gameObject.activeSelf)
        {
            // 切换为起飞状态，播放起飞动画
            PlayAnimation(AnimId.IsGround, true);
            PlayAnimation(AnimId.IsFlying, true);
        }
        PlayAnimation(AnimId.IsGround, isGround);
    }


    public void BounceplankLand()
    {
        bounceplnakHVec = Vector3.zero;
        isBounceplankJumping = false;
    }

    /**
    * 玩家降落
    */
    private void PlayerLand()
    {
        if (PlayerIsGround() && !isGround)
        {
            isGround = true;
            BounceplankLand();
            upwardVec.y = -1f;
            //if (!ReferManager.Inst.isRefer && (!PlayerSwimControl.Inst || !PlayerSwimControl.Inst.isInWater))
            //{
            //    PlayModePanel.Instance.OnSetDownButton(true);
            //    SetFly(false, true);
            //}
            //AudioController.Inst.audioState = MoveAudioState.Ground;
            ////AudioController.Inst.PlayGroundAudio();
            //PlayGroundSound();
            //AudioController.Inst.StopFlyAudio();
            //CancelInvoke("OnGroundStartStep");
            //Invoke("OnGroundStartStep", AudioController.Inst.deltaTime);

            PlayAnimation(AnimId.IsJump, IsJump);

            if (!isTps && PlayerControlManager.Inst.isPickedProp)
            {

                StartCoroutine(SetPlayerRoleActive(0.01f, true));
            }
            
            if (PlayerStandonControl.Inst)
            {
                PlayerStandonControl.Inst.OnPlayerLanded();
            }
        }

        if (transform.position.y <= -0.5)
        {
            upwardVec.y = 10;
        }
    }

    private void PlayerMove()
    {
        if (Character && !waitPosChange)
        {
            // 冰方块移动处理
            if (PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsShouldRunInIceCube())
            {
                if (moveVec.magnitude > 0.1f)
                {
                    moveVec -= moveVec.normalized * ice_del_speed;
                    mAnimStateManager.SwitchTo(EPlayerAnimState.Skate);
                }
                else
                {
                    moveVec = Vector3.zero;
                    mAnimStateManager.SwitchTo(EPlayerAnimState.Idle);
                }
                
                if (moveVec.sqrMagnitude > ice_max_speed_sqr)
                {
                    moveVec = moveVec.normalized * ice_max_speed;
                }
            }
            
            // 降落伞移动处理
            if (PlayerParachuteControl.Inst)
            {
                PlayerParachuteControl.Inst.OnPlayerMove(ref moveVec, ref upwardVec);
            }

            // 雪方块移动处理
            if (PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsCanUseSnowMove())
            {
                PlayerSnowSkateControl.Inst.OnPlayerMove(ref moveVec, ref upwardVec);
            }

            curmoveVec = moveVec;
            if (isBounceplankJumping)
            {
                moveVec *= 0.5f;
            }
            extraVec = GetExtraVec();
            Character.Move((moveVec + upwardVec + extraVec) * Time.deltaTime);
        }
    }
    private Vector3 extraVec;
    //附加的向量增加在此处
    public Vector3 GetExtraVec()
    {
        return bounceplnakHVec;
    }
    private void RefreshPlayerDamp()
    {
        if (PlayerIsGround() && upwardVec.y > 0)
        {
            SetDamp(jumpDamp);
        }
        else
        {
            SetDamp(stepDamp);
        }
    }

    private void OnGroundStartStep()
    {
        AudioController.Inst.audioState = MoveAudioState.None;
    }

    void SetDamp(float damp)
    {
        if (transposer)
        {
            transposer.m_YDamping = damp;
        }
    }

    public void SetPlayerActive(bool isActive)
    {
        if (!isChanged)
        {
            playerAnim.gameObject.SetActive(false);
            return;
        }
        if (isTps == false)
        {
            // playerAnim.gameObject.SetActive(false);
            return;
        }
        else if (isTps == true)
        {
            playerAnim.gameObject.SetActive(isActive);
            animCon.PlayEyeAnim();//切换视角时调用
        }
    }

    public void WaitForShow()
    {
        //var spawnPos = SpawnPointManager.Inst.GetSpawnPoint().transform.localPosition;
        //var spawnRot = SpawnPointManager.Inst.GetSpawnPoint().transform.localRotation;
        //if (!ReferManager.Inst.isRefer)
        //{
        //    SetPlayerPositionAndRotation(spawnPos + initPos, spawnRot);
        //}
        SetPlayerPositionAndRotation(new Vector3(-300, 20, 200), Quaternion.identity);
        ShowPlayerCharater();
    }

    public void ShowPlayerCharater()
    {
        if (GlobalFieldController.CurGameMode == GameMode.Edit)
        {
            if (ReferManager.Inst.isRefer)
            {
                return;
            }
            gameObject.SetActive(false);
            return;
        }

        if (curGameMode == GameMode.Guest)
        {
            Invoke("ShowPlayer", 0.1f);
        }
        else
        {
            Invoke("ShowPlayer", 2f);
        }

        // 试玩模式拉取人物形象
        if (curGameMode != GameMode.Guest)
        {
            InitUserInfo();
        }
    }

    public void InitUserInfo()
    {
        if (!isChanged)
        {
            if (GameManager.Inst.ugcUserInfo != null && !string.IsNullOrEmpty(GameManager.Inst.ugcUserInfo.imageJson))
            {
                LoggerUtils.Log("GetUserInfo native userInfo is exist to init");
                playerRoleData.InitUserInfo();
            }
            else
            {
                LoggerUtils.Log("GetUserInfo native userInfo is not exist,request http");
                playerRoleData.GetUserInfo();
            }
        }
    }

    /**
    * 播放 Player 动画
    * animId 动画Id
    * state 动画状态
    */
    public void PlayAnimation(AnimId animId, bool state)
    {
        // 添加判空，防止异常报错
        if (playerAnim && playerAnim.gameObject.activeSelf)
        {
            // 由于后续动作列表字典会动态添加，故需要在使用时判断
            if (PlayerControlManager.Inst.AnimNameDict.ContainsKey((int)animId))
            {
                string animName = PlayerControlManager.Inst.AnimNameDict[(int)animId];
                playerAnim.SetBool(animName, state);
            }
        }
    }
    
    /// <summary>
    /// 根据int类型参数播放状态机
    /// </summary>
    /// <param name="animId"></param>
    /// <param name="state"></param>
    public void PlayAnimationById(AnimId animId, int stateValue)
    {
        // 添加判空，防止异常报错
        if (playerAnim && playerAnim.gameObject.activeSelf)
        {
            // 由于后续动作列表字典会动态添加，故需要在使用时判断
            if (PlayerControlManager.Inst.AnimNameDict.ContainsKey((int)animId))
            {
                playerAnim.SetInteger(PlayerControlManager.Inst.AnimNameDict[(int)animId], stateValue);
            }
        }
    }

    public int GetPlayerAnimStateIntValue(AnimId animId)
    {
        if (PlayerControlManager.Inst.AnimNameDict.ContainsKey((int)animId))
        {
            //Gc warning!!! not use it in update() !!
            string animName = PlayerControlManager.Inst.AnimNameDict[(int)animId];
            return playerAnim.GetInteger(animName);
        }
        return 0;
    }

    public void SetPlayerPosAndRot(Vector3 pos, Quaternion rot)
    {
        SetPlayerPositionAndRotation(pos + initPos, rot);
    }

    private void ShowPlayer()
    {
        if (isChanged)
        {
            PlayerControlManager.Inst.SetPlayerActive(true);
            SetPlayerRoleActive();
            return;
        }
    }

    public void SetEndFlyPlayerPos()
    {
        ptrigger.transform.parent = transform;
        playerAnim.transform.parent = transform;
        ptrigger.transform.localPosition = Vector3.zero;
        ptrigger.transform.localRotation = Quaternion.Euler(Vector3.zero);
        playerAnim.transform.localPosition = new Vector3(0, -0.95f, 0);
        playerAnim.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
        playerAnim.transform.DOLocalRotate(new Vector3(0, playerAnim.transform.localRotation.eulerAngles.y, 0), 0.6f);
    }

    public void SetFreeFlyPlayerPos()
    {
        playerAnim.transform.DOKill();
        parentPlayer.transform.localRotation = playerAnim.transform.localRotation;
        playerAnim.transform.parent = parentPlayer.transform;
        playerAnim.transform.localRotation = Quaternion.Euler(Vector3.zero);
        ptrigger.transform.parent = parentPlayer.transform;
    }

    public void SetFreeFlyHight(bool isfly)
    {
        upwardVec.y = 40;
        Character.Move((moveVec + upwardVec) * Time.deltaTime);
        isFlying = isfly;
        upwardVec.y = 0;
    }

    private void FreeFlyMode(Vector3 screenOffset)
    {
        var joyStick = PlayModePanel.Instance.joyStick;
        if (!joyStick)
        {
            return;
        }
        RectTransform stick = joyStick.transform.Find("Stick").GetComponent<RectTransform>();
        float rotY = Vector3.Angle(new Vector3(0, 1, 0), screenOffset.normalized);
        if (screenOffset.normalized.x < 0)
        {
            rotY *= -1;
        }
        if (!isOriginalFlyMode && isMoving && isFlying)
        {
            if (stick.localPosition.y >= 60 && stick.localPosition.y <= 140)
            {
                CheckFlyDirections(-1);
            }
            else if (stick.localPosition.y <= -60 && stick.localPosition.y >= -140)
            {
                CheckFlyDirections(1);
            }
            else
            {
                flyDir = 0;
                moveY = 0;
            }
        }
        if (!isMoving && isFlying)
        {
            moveY = 0;
        }
        playerAnim.transform.localRotation = Quaternion.Euler(Vector3.zero);
        parentPlayer.transform.localRotation = Quaternion.Lerp(parentPlayer.transform.localRotation, Quaternion.Euler(lookCenter.rotation.eulerAngles.x * flyDir + 30, rotY, 0), 5f * Time.deltaTime);
    }

    private bool CheckFlyHight()
    {
        RaycastHit hit;
        bool isHit = Physics.Raycast(transform.position, -Vector3.up, out hit, 0.5f,
            1 << LayerMask.NameToLayer("Terrain") |
            1 << LayerMask.NameToLayer("Model"));
        if (isHit)
        {
            return true;
        }
        return false;
    }

    private void CheckFlyDirections(int direction)
    {
        float flySpeed = 8;
        float tempRot = lookCenter.eulerAngles.x;

        if (tempRot >= 180)
        {
            tempRot -= 360;
        }
        moveY = direction * tempRot * flySpeed * Time.deltaTime;
        flyDir = -direction;
    }

    private void OnChangeMode(GameMode mode)
    {
        curGameMode = mode;
        if (mode == GameMode.Edit)
        {
            gameObject.SetActive(false);
            UpdateAnimatorCon();
            WaitForShow();
        }
    }

    private void OnDestroy()
    {
        mPlayerStateChangedFunc = null;
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        Inst = null;
        PlayModeHandler.fpvRotMax = 60;
    }

    /**
    * 动态替换 runtimeAnimatorController
    */
    public void UpdateAnimatorCon()
    {
        if (!isTps && PlayerControlManager.Inst.isPickedProp)
        {
            playerAnim.runtimeAnimatorController = playerFPVAnimCon;
        }
        else if (isTps && PlayerControlManager.Inst.isPickedProp)
        {
            playerAnim.runtimeAnimatorController = overrideController;
        }
        else if (isTps && PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsStandOnIceCube())
        {
            playerAnim.runtimeAnimatorController = overrideController;
        }
        else if (isTps && PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsStandOnSnowCube() && PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsSnowSkating())
        {
            playerAnim.runtimeAnimatorController = overrideController;
        }
        else if(isTps && StateManager.IsOnSeesaw)
        {
            playerAnim.runtimeAnimatorController = overrideController;
        }
        else
        {
            playerAnim.runtimeAnimatorController = playerNormalAnimCon;
        }
        //TODO：：拾取降落伞时，会切换整个动画状态机
        if (!isTps && PlayerParachuteControl.Inst)
        {
            playerAnim.runtimeAnimatorController = playerNormalAnimCon;
        }
        if (SceneParser.Inst.GetBaggageSet() == 1)
        {
            PlayerControlManager.Inst.ChangeAnimClips();
        }
    }

    public void SetPlayerRoleActive()
    {
        bool isVisible = !isTps && !isFlying && (PlayerControlManager.Inst.isPickedProp
        || (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel));
        bool isPromote = PromoteManager.Inst.GetPlayerPromoteState(GameManager.Inst.ugcUserInfo.uid);
        isVisible = isPromote || isVisible;
        CoroutineManager.Inst.StartCoroutine(SetPlayerRoleActive(0, isVisible));
    }

    public void SetBagPlayerRoleActive()
    {
        bool isVisible = !isTps && isGround && (PlayerControlManager.Inst.isPickedProp
        || (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel));
        CoroutineManager.Inst.StartCoroutine(SetPlayerRoleActive(0.01f, isVisible));
    }

    public IEnumerator SetPlayerRoleActive(float delayTime, bool isActive, string animName = "")
    {
        yield return new WaitForSeconds(delayTime);

        if (!isTps)
        {
            playerAnim.gameObject.SetActive(isActive);
            if (isActive && !string.IsNullOrEmpty(animName))
            {
                playerAnim.Play(animName);
            }
        }
    }

    public IEnumerator SetPlayerRoleActive(float delayTime, bool isActive, AnimId animId, bool state)
    {
        yield return new WaitForSeconds(delayTime);

        if (!isTps)
        {
            playerAnim.gameObject.SetActive(isActive);
            if (isActive)
            {
                PlayAnimation(animId, state);
            }
        }
    }

    /**
    * 绑定动画状态机和动画器重写控制器
    */
    private void BindAnimator()
    {
        overrideController = new AnimatorOverrideController(playerAnim.runtimeAnimatorController);
        playerAnim.runtimeAnimatorController = overrideController;
        clipOverrides = new AnimationClipOverrides(overrideController.overridesCount);
        //获取动画器重写控制器中当前定义的动画剪辑重写的列表
        overrideController.GetOverrides(clipOverrides);
    }

    public AnimationClip GetOverrideAnimClip(AnimClipType clipType)
    {
        if (clipOverrides == null)
        {
            return null;
        }
        var animName = GameUtils.AnimTypeAndNameDict[clipType];
        return clipOverrides[animName];
    }

    /*
    * 批量更新动画片段
    */

    public void UpdateAnimClips(List<ClipItem> clipList)
    {
        if (!isTps)
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
        animCon.ReleaseAndCancelLastEmo();
        //在编辑模式进入游玩模式时，会调用此处，所以加入外层player的显隐判断
        if (gameObject.activeInHierarchy)
        {
            overrideController.ApplyOverrides(clipOverrides);
            playerAnim.runtimeAnimatorController = overrideController;
        }
    }

    public void SetPlayerHeadVisible(bool isVisible)
    {
        head.SetActive(isVisible);
        face.SetActive(isVisible);
        hair.SetActive(isVisible);
        cloth.enabled = isVisible;
    }
    public void OnTalkSend(bool isTalking)
    {
        animCon.OnTalkSend(isTalking);
    }
    /// <summary>
    /// 当低于minValue时，调整相机位置为minValue
    /// </summary>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    public void SetLookCenterMin(float minValue, float maxValue)
    {
        if (lookCenter != null && lookCenter.eulerAngles.x > minValue && lookCenter.eulerAngles.x <= maxValue)
        {
            lookCenter.eulerAngles = new Vector3(minValue, lookCenter.eulerAngles.y, lookCenter.eulerAngles.z);
        }
    }
    /*
    * 更新某一个动画片段
    */
    // 暂不使用
    // public void UpdateOneAnimClip(AnimClipType type, AnimationClip clip)
    // {
    //     var animName = GameUtils.AnimTypeAndNameDict[type];
    //     clipOverrides[animName] = clip;
    //     // 批量替换动画片段列表
    //     overrideController.ApplyOverrides(clipOverrides);
    //     playerAnim.runtimeAnimatorController = overrideController;
    // }

    public void ResEmoAnim()
    {
        animCon.RleasePrefab();
        animCon.CancelLastEmo();
        animCon.OnEmoKill();
        animCon.playerAnim.Play("idle");
        if (animCon.isPlaying)
        {

        }
    }
    #region 限制行为能力
    public int[] mNoAbilites=new int[(int)EObjAbilityType.Max];
    public bool GetNoAbilityFlag(EObjAbilityType abt)
    {
        return mNoAbilites[(int)abt] > 0 ? true : false;
    }
    public int AddNoAbilityFlag(EObjAbilityType abt)
    {
        mNoAbilites[(int)abt]++;
        SetNoAbilityFlagChange(abt);
        return mNoAbilites[(int)abt];
    }
    public virtual int RemoveNoAbilityFlag(EObjAbilityType abt)
    {
        mNoAbilites[(int)abt]--;
        SetNoAbilityFlagChange(abt);
        return mNoAbilites[(int)abt];
    }

    public virtual int ClearNoAbilityFlag(EObjAbilityType abt)
    {
        mNoAbilites[(int)abt] = 0;
        SetNoAbilityFlagChange(abt);
        return 0;
    }
    void SetNoAbilityFlagChange(EObjAbilityType abt)
    {
    }
    #endregion  
}

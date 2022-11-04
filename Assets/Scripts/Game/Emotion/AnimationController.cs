using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using static EmoMenuPanel;
using BudEngine.NetEngine;

public class AnimationController : MonoBehaviour
{
    private PlayerData playerData;
    public Animator playerAnim;
    public GameObject playerModle;
    private AnimatorOverrideController animatorOverrideController;
    AnimationClip[] clips;
    private SoundCookie cookie;
    private RoleController roleCon;
    [HideInInspector]
    public bool isPlaying;
    public bool isEating;
    public bool isFishing;
    public bool isLooping;
    public bool isInteracting;
    public EmoName curStateEmo = EmoName.None; //当前状态动画
    public string defulFaceName = "idle";
    public string defaultEyeName;

    private EmoItemData emoItemData;

    public EmoIconData loopingInfo;
    public Action AnimFinCallBack;
    private VoiceItemPos voiceItem;

    private Action OnAnimChange;

    public class TypeStatusData
    {
        public bool isStart;
        public string playerId;
    }

    private enum PlayerType
    {
        SelfPlayer = 1,
        OtherPlayer = 2,
    }
    private PlayerType curPlayerType = PlayerType.SelfPlayer;

    //是否正在换装
    public bool IsChanging => playerAnim.GetCurrentAnimatorStateInfo(0).IsName("rehandling");

    private void Awake()
    {
        MessageHelper.AddListener<TypeStatusData>(MessageName.TypeData,PlayType);
        if (GetComponent<OtherPlayerCtr>())
        {
            curPlayerType = PlayerType.OtherPlayer;
        }
    }
    private void Start()
    {
        voiceItem = playerModle.GetComponentInChildren<VoiceItemPos>(true);
        playerAnim = playerModle.GetComponent<Animator>();
        roleCon = playerModle.GetComponent<RoleController>();
        clips = playerAnim.runtimeAnimatorController.animationClips;
    }

    public void OnTalkSend(bool istalking)
    {
        if (isPlaying)
        {
            return;

        }
        if (istalking)
        {
            PlaySpeckAnim();
        }
        else
        {
            StopSpeckAnim();
        }

    }
    public void PlayAnim(int anim)
    {
        PlayAnim(anim, 0);
    }

    public void PlayAnim(int anim, int randomId)
    {
        StopSpeckAnim();
        if (isInteracting)
        {
            return;
        }
        OnAnimChange?.Invoke();
        OnAnimChange = null;
        EmoIconData info = MoveClipInfo.GetAnimName(anim);
        // 不能解析的表情，直接打断循环
        if (info == null)
        {
            if (isPlaying)
            {
                RecStopLoop();
            }
        }
        if (isLooping)
        {
            isLooping = false;
            cookie?.Stop();
            if (PlayLoop!=null)
            {
                StopCoroutine(PlayLoop);
            }
            OnEmoKill();

        }
        if (info != null)
        {
            if (info.noLoop == 0)
            {
                PlayAudio(info.name);

                PlayEmoMove(info, randomId);
            }
            else
            {
                PlayLoopEmo(info);
            }
        }

        if(roleCon != null)
        {
            if (info.id != (int)EmoName.EMO_SELFIE_MODE && info.id != (int)EmoName.EMO_SEESAW_PUSH)
            {
                roleCon.OnEmoPlay();
            }

            MessageHelper.Broadcast(MessageName.OnEmoPlay,curPlayerType == PlayerType.SelfPlayer);
        }
    }

    public void PlayMutualFinAnim(int anim, string endName, EmoItemData emoItemData)
    {
        if (isInteracting)
        {
            return;
        }
        if (isLooping)
        {
            isLooping = false;
            cookie?.Stop();
            StopCoroutine(PlayLoop);
        }

        this.emoItemData = emoItemData;
        EmoIconData info = MoveClipInfo.GetAnimName(anim);
        //PlayAudio(info.name);
        if (string.IsNullOrEmpty(endName))
        {
            StartPlayMutualFin(info);
            return;
        }
        StartPlayMutualFin(info, endName);
    }
    
    #region 播放状态表情的动作（如加好友emo）
    public void PlayStateEmoMutualFinAnim(int anim, string endName, EmoItemData emoItemData)
    {
        if (isInteracting)
        {
            return;
        }
        if (isLooping)
        {
            isLooping = false;
            cookie?.Stop();
            StopCoroutine(PlayLoop);
        }

        this.emoItemData = emoItemData;
        EmoIconData info = MoveClipInfo.GetAnimName(anim);
        //PlayAudio(info.name);
        if (string.IsNullOrEmpty(endName))
        {
            StartPlayStateEmoMutualFin(info);
            return;
        }
        StartPlayStateEmoMutualFin(info, endName);
    }
    
    private void StartPlayStateEmoMutualFin(EmoIconData data, string endName = "_finish")
    {
        PlayStateEmoEndMove(data, endName);

        isInteracting = true && gameObject.activeInHierarchy;
        SetOtherPlayerAvoidFrame(true);
    }
    
    public void PlayStateEmoEndMove(EmoIconData info,string endName)
    {
        if (info != null && info.endMoveInfos!=null)
        {
            for (int i = 0; i < info.endMoveInfos.Length; i++)
            {
                if (info.endMoveInfos[i].name.CompareTo(info.name+ endName) == 0)
                {
                    info.endMoveInfos[i].moveEndTime = GetEmoTime(info.endMoveInfos[i].name);
                    PlayEmoMove(info.endMoveInfos[i]);
                    if (roleCon != null)
                    {
                        roleCon.OnEmoPlay();
                    }
                    PlayAudio(info.endMoveInfos[i].name, 1, info.endMoveInfos[i]);
                    return;
                }
            }
        }

        CancelLastEmo();
        playerAnim.Play("idle", 0,0);
        playerAnim.Play(defulFaceName, 1, 0f);
    }
    
    #endregion
    

    public void SetRandomMove(string name, int id)
    {
        if (expressionGameObject != null && expressionGameObject.Count > 0)
        {
            for (int i = 0; i < expressionGameObject.Count; i++)
            {
                RandomMoveGameObject[] ran = expressionGameObject[i].GetComponentsInChildren<RandomMoveGameObject>();
                if (ran != null && ran.Length > 0)
                {
                    for (int j = 0; j < ran.Length; j++)
                    {
                        if (ran[j].isChangeTexture)
                        {
                            ran[j].ChangeTexture(name, id);
                        }
                    }
                }
            }
        }
    }
    public void PlayLoopEmo(EmoIconData info)
    {
        RleasePrefab();
        for (int i = 0; i < info.moveInfos.Length; i++)
        {
            info.moveInfos[i].moveEndTime = GetEmoTime(info.moveInfos[i].name);
            if (i< info.moveInfos.Length-1)
            {
                info.moveInfos[i].nextEmoInfo = info.moveInfos[i + 1];
            }
          
        }
        PlayAudio(info.moveInfos[0].name);
        PlayLoop = PlayLoopAudio(info.moveInfos[0].moveEndTime, info.name + "_centre");
        PlayLoopCor = StartCoroutine(PlayLoop);
        PlayEmoMove(info.moveInfos[0]);
        isLooping = true;
        loopingInfo = info;
        SetInteractState(info.emoType == (int)EmoTypeEnum.DOUBLE_EMO, info.id);
    }
    public void PlayEndMove(EmoIconData info,string endName)
    {
        if (info != null&& info.endMoveInfos!=null)
        {
            for (int i = 0; i < info.endMoveInfos.Length; i++)
            {
                if (info.endMoveInfos[i].name.CompareTo(info.name+ endName) ==0)
                {
                    info.endMoveInfos[i].moveEndTime = GetEmoTime(info.endMoveInfos[i].name);
                    PlayEmoMove(info.endMoveInfos[i]);
                    PlayAudio(info.endMoveInfos[i].name, 1, info.endMoveInfos[i]);
                    return;
                }
                
            }
        }

        CancelLastEmo();
        playerAnim.Play("idle", 0,0);
        playerAnim.Play(defulFaceName, 1, 0f);
    }
    public void StartPlayLoop(EmoIconData info)
    {
        RleasePrefab();
        float length = 0;
        if(clips == null) { return; }
        foreach(AnimationClip clip in clips)
        {
            if (clip.name.Equals(info.name+"_start"))
            {
                length = clip.length;
                break;
            }
        }
        SetExpressionPrefab(info);
        PlayAudio(info.name + "_start");
        playerAnim.Play(info.name + "_start", 0, 0f);

        PlayLoop = PlayLoopAudio(length, info.name + "_centre");

        PlayLoopCor = StartCoroutine(PlayLoop);
        if (FaceMove != null)
        {
            StopCoroutine(FaceMove);
        }
        if (info.hasFaceAnim == 1)
        {
            roleCon.SetCustomDefaultPos();
            playerAnim.Play(defulFaceName, 1, 0f);
        }
        if (isPlaying && PlayMove != null)
        {
            StopCoroutine(PlayMove);
        }
        isPlaying = true;
        isLooping = true;
        loopingInfo = info;
        // if (info.id == (int)EmoName.EMO_JOIN_HAND)
        // {
        //     SetInteractState(!PlayerControl.Inst.isInEumual, info.id);
        // }
        // else
        // {
        SetInteractState(info.emoType == (int)EmoTypeEnum.DOUBLE_EMO, info.id);
        // }
    }

    private void SetInteractState(bool state, int id = 0)
    {
        var iPlayerCon = GetComponent<IPlayerController>();
        if (iPlayerCon != null)
        {
            iPlayerCon.SetEmoInteractState(state);
        }

        //状态过程中，播放其他动画不隐藏交互按钮
        if (curStateEmo == EmoName.None)
        {
            var playerTouchBehav = GetComponent<PlayerTouchBehaviour>();
            if (playerTouchBehav != null)
            {
                playerTouchBehav.SetCanTouch(state, id);
            }
        }
    }

    public void StopLoop()
    {
        if (isLooping && loopingInfo != null)
        {
            EmoItemData emoItemData = new EmoItemData()
            {
                opt = (int)OptType.Cancel,
                startPlayerId = Player.Id,
            };
            Item item = new Item()
            {
                id = loopingInfo.id,
                type = (int)loopingInfo.GetEmoType(),
                data = JsonConvert.SerializeObject(emoItemData)
            };
            RoomChatData roomChatData = new RoomChatData()
            {
                msgType = (int)RecChatType.Emo,
                data = JsonConvert.SerializeObject(item),
            };

            // #if !UNITY_EDITOR
            // 牵手中不发送动作取消的消息
            if (!PlayerMutualControl.Inst || !PlayerMutualControl.Inst.isInEumual
            || loopingInfo.id != (int)EmoName.EMO_JOIN_HAND)
            {
                ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
            }
            // #endif
            PlayEndMove(loopingInfo,"_end");
            //StartPlay(loopingInfo, loopingInfo.name + "_end");
        
            cookie?.StopLoop();
            //for (int i = 0; i < expressionGameObject.Count; i++)
            //{
            //    expressionGameObject[i].GetComponent<Animator>().Play(loopingInfo.name + "_end", 0, 0f);
            //}
            if (PlayLoop!=null)
            {
                StopCoroutine(PlayLoop);
            }
            isLooping = false;
            if (PlayerMutualControl.Inst)
            {
                PlayerMutualControl.Inst.isWaitingHands = false;
            }
        }
    }

    //结束状态动画
    public void StopStateEmo()
    {
        if (curStateEmo == EmoName.None)
        {
            return;
        }
        
        EmoIconData info = MoveClipInfo.GetAnimName((int) curStateEmo);
        EmoItemData emoItemData = new EmoItemData()
        {
            opt = (int)OptType.Cancel,
            startPlayerId = Player.Id,
        };
        Item item = new Item()
        {
            id = loopingInfo.id,
            type = (int)EmoTypeEnum.STATE_EMO,
            data = JsonConvert.SerializeObject(emoItemData)
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Emo,
            data = JsonConvert.SerializeObject(item),
        };

        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }

    public void SetStateEmo(EmoName emoName)
    {
        curStateEmo = emoName;
    }

    /**
    * 双人牵手放手
    */
    public void ReleaseHand()
    {
        SetInteractState(false);
        string sPlayerId = "";
        string fPlayerId = "";
        if (PlayerMutualControl.Inst)
        {
            sPlayerId = PlayerMutualControl.Inst.startPlayerId;
            fPlayerId = PlayerMutualControl.Inst.followPlayerId;
        }
        EmoItemData emoItemData = new EmoItemData()
        {
            opt = (int)OptType.Cancel,
            startPlayerId = sPlayerId,
            followPlayerId = fPlayerId
        };
        Item item = new Item()
        {
            id = (int)EmoName.EMO_JOIN_HAND,
            type = (int)EmoType.LoopMutual,
            data = JsonConvert.SerializeObject(emoItemData)
        };
        RoomChatData roomChatData = new RoomChatData()
        {
            msgType = (int)RecChatType.Emo,
            data = JsonConvert.SerializeObject(item),
        };

        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
    }

    //停止循环动画回包
    public void RecStopLoop()
    {
        if (!isLooping) return;
        PlayEndMove(loopingInfo,"_end");
        //StartPlay(loopingInfo, loopingInfo.name + "_end");
        //for (int i = 0; i < expressionGameObject.Count; i++)
        //{
        //    expressionGameObject[i].GetComponent<Animator>().Play(loopingInfo.name + "_end", 0, 0f);
        //}
       // PlayAudio(loopingInfo.name + "_end");
        cookie?.StopLoop();
        if (PlayLoop != null)
        {
            StopCoroutine(PlayLoop);
        }
        isLooping = false;
    }

    /// <summary>
    /// 校验玩家眼睛是否可以被解析，如果可被解析则直接返回
    /// 不能被解析，则返回默认的玩家眼睛Id
    /// </summary>
    /// <param name="eId">玩家的眼睛 Id</param>
    /// <returns></returns>
    public int GetValidEyeId(int eId)
    {
        EyeStyleData eyeStyleData = RoleConfigDataManager.Inst.GetEyeStyleDataById(eId);
        var defEyeId = eId;
        if (eyeStyleData == null)
        {
            var defRoleConfigData = RoleConfigDataManager.Inst.defRoleConfigData;
            defEyeId = defRoleConfigData.eId;
        }
        return defEyeId;
    }

    public void PlayEyeAnim(int eId)
    {
        eId = GetValidEyeId(eId);
        string name = "eye_" + eId;

        defulFaceName = name;

        defaultEyeName = name;
        if (curPlayerType == PlayerType.SelfPlayer && StateManager.IsOnSeesaw)
        {
            defulFaceName = "seesaw_centre";
        }

        if (playerAnim == null)
        {
            playerAnim = playerModle.GetComponent<Animator>();
        }

        playerAnim.Play(defulFaceName, 1, 0f);
        
    }
    public void PlayEyeAnim()
    {
        if (curPlayerType == PlayerType.SelfPlayer)
        {
            if (StateManager.IsOnSeesaw)
            {
                defulFaceName = "seesaw_centre";
            }
            else
            {
                defulFaceName = defaultEyeName;
            }
        }
        
        if (playerAnim!=null)
        {
            playerAnim.Play(defulFaceName, 1, 0f);
        }
    }
    public void OnEnable()
    {
        PlayEyeAnim();
    }

    public void PlaySpeckAnim()
    {
        if (voiceItem!=null)
        {
            voiceItem.gameObject.SetActive(true);
        }
        playerAnim.SetLayerWeight(2, 1.0f);
    }
    public void StopSpeckAnim()
    {
        if (voiceItem != null)
        {
            voiceItem.gameObject.SetActive(false);
        }
        playerAnim.SetLayerWeight(2, 0f);
    }
    private IEnumerator PlayMove;
    private IEnumerator FaceMove;
    private IEnumerator PlayLoop;
    private List<GameObject> expressionGameObject = new List<GameObject>();

    private Coroutine FaceMoveCor;
    private Coroutine PlayMoveCor;
    private Coroutine PlayLoopCor;
    private Coroutine CustomPlayAnimEndCor;


    private BaseBody moveBody;
    private BaseFace moveFace;
    public BaseEffect moveEffect;
    private SpecialBody speBody;

    public void PlayEmoMove(EmoIconData info)
    {
        PlayEmoMove(info, 0);
    }

    public void PlayEmoMove(EmoIconData info,int random)
    {
        if (isInteracting)
        {
            return;
        }
        RleasePrefab();
        KillEmo();
        isPlaying = true;
        info.moveEndTime = GetEmoTime(info.name);
        if (info.specialType > 0)
        {
            var OnAnimFin = GetSpecialAnimFinAct(info);
            speBody = new SpecialBody();
            BodyArgs bodyArgs = new BodyArgs(this, info);
            speBody.Init(OnAnimFin, playerData.playerInfo.Id);
            speBody.OnPlay(bodyArgs);

            if (info.hasFaceAnim == 0)
            {
                moveFace = new NormalFace();
                FaceArgs faceArgs = new FaceArgs(playerAnim, info, defulFaceName, roleCon);
                moveFace.OnPlay(faceArgs);
            }

            moveEffect = new NormalEffect();
            EffectArgs effectArgs = new EffectArgs(playerAnim, info, playerModle, roleCon);
            moveEffect.OnPlay(effectArgs);
        }
        else
        {
            switch (info.GetEmoType())
            {
                case EmoType.Normal:
                    moveBody = new NormalBody();
                    BodyArgs bodyArgs = new BodyArgs(this, info);
                    moveBody.OnPlay(bodyArgs);

                    if (info.hasFaceAnim == 0)
                    {
                        moveFace = new NormalFace();
                        FaceArgs faceArgs = new FaceArgs(playerAnim, info, defulFaceName, roleCon);
                        moveFace.OnPlay(faceArgs);
                    }

                    moveEffect = new NormalEffect();
                    EffectArgs effectArgs = new EffectArgs(playerAnim, info, playerModle, roleCon);
                    moveEffect.OnPlay(effectArgs);
                    break;

                case EmoType.Random:
                    moveBody = new NormalBody();
                    bodyArgs = new BodyArgs(this, info);
                    moveBody.OnPlay(bodyArgs);

                    if (info.hasFaceAnim == 0)
                    {
                        moveFace = new NormalFace();
                        FaceArgs faceArgs = new FaceArgs(playerAnim, info, defulFaceName, roleCon);
                        moveFace.OnPlay(faceArgs);
                    }

                    moveEffect = new RamdomEffect();
                    effectArgs = new EffectArgs(playerAnim, info, playerModle, roleCon, random);
                    moveEffect.OnPlay(effectArgs);
                    break;
            }
        }
        SetInteractState(false);
    }

    private Action<string> GetSpecialAnimFinAct(EmoIconData info)
    {
        Action<string> OnAnimFin = null;
        switch (info.id)
        {
            case (int)EmoName.EMO_SWORD:
                OnAnimFin = SwordManager.Inst.OnAnimStopAct;
                break;
            case (int)EmoName.EMO_CRYSTAL_GET:
                OnAnimFin = CrystalStoneManager.Inst.OnCollectAnimFinish;
                break;
        }
        return OnAnimFin;
    }

    public void OnEmoKill()
    {
        KillEmo();
        isPlaying = false;
        ReAvoidFrameOnMutualFin();
        isInteracting = false;
        PlayMoveCor = null;
        AnimFinCallBack?.Invoke();
        AnimFinCallBack = null;
    }
    public void KillEmo()
    {
        if (moveBody != null)
        {
            moveBody.OnKill();
            moveBody = null;
        }
        if (moveFace != null)
        {
            moveFace.OnKill();
            moveFace = null;
        }
        if (moveEffect != null)
        {
            moveEffect.OnKill();
            moveEffect = null;
        }
        if (roleCon != null)
        {
            roleCon.OnEmoEnd();
            MessageHelper.Broadcast(MessageName.OnEmoEnd, curPlayerType == PlayerType.SelfPlayer);
        }
        if(speBody != null)
        {
            speBody.OnKill();
            speBody = null;
        }
    }




    private void StartPlay(EmoIconData info)
    {
       // TestPlay(info);
        StartPlay(info, info.name);
    }
    private void StartPlay(EmoIconData info, string name)
    {
        if (isInteracting)
        {
            return;
        }
        RleasePrefab();
        SetExpressionPrefab(info);

        playerAnim.Play(name, 0, 0f);

        if (FaceMove != null)
        {
            StopCoroutine(FaceMove);
        }

        if (info.hasFaceAnim==1)
        {
            roleCon.SetCustomDefaultPos();
            playerAnim.Play(defulFaceName, 1, 0f);
        }
        else
        {
            roleCon.SetFacialDefaultPos();
            StopSpeckAnim();
            if (info.delateTime != 0)
            {
                playerAnim.Play(defulFaceName, 1, 0f);
            }
            FaceMove = PlayFaceMove(info.delateTime, info.faceEndTime, info.name);
            FaceMoveCor = StartCoroutine(FaceMove);
        }
        for (int i = 0; i < clips.Length; i++)
        {
            if (name == clips[i].name)
            {
                if (isPlaying && PlayMove != null)
                {
                    StopCoroutine(PlayMove);
                }
                PlayMove = SetCallBack(clips[i].length);
                PlayMoveCor = StartCoroutine(PlayMove);
                break;
            }
        }
        SetInteractState(false);
    }
    private float GetEmoTime(string name)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            if (name == clips[i].name)
            {
                return clips[i].length;
            }
        }
        return 0;
    }
    private void StartPlayMutualFin(EmoIconData data, string endName = "_finish")
    {
        PlayEndMove(data, endName);
        //EmoIconData info = new EmoIconData()
        //{
        //    id = data.id,
        //    name = data.name,
        //    bandBody = data.bandBody,
        //    delateTime = data.delateTime,
        //    hasFaceAnim = data.hasFaceAnim,
        //    effectCount = data.effectCount,
        //    emoIcon = data.emoIcon
        //};
        //StartPlay(info);
        isInteracting = true && gameObject.activeInHierarchy;
        SetOtherPlayerAvoidFrame(true);
    }
    private IEnumerator PlayFaceMove(float time,float endTime,string name)
    {
        yield return new WaitForSeconds(time);
        playerAnim.Play(name, 1, 0f);
        yield return new WaitForSeconds(endTime-time);
        playerAnim.Play(defulFaceName, 1, 0f);
        FaceMove = null;
        FaceMoveCor = null;
    }
    private void PlayAudio(string name, uint count = 1, EmoIconData data = null)
    {
        if (playerData == null)
        {
            playerData = GetComponent<PlayerData>();
        }
        
        bool isSelf;
        if (emoItemData != null && data != null && name == "clap_finish")
        {
            if (emoItemData.startPlayerId == playerData.playerInfo.Id)
            {
                if(emoItemData.followPlayerId == Player.Id) return;
                isSelf = playerData.playerInfo.Id == Player.Id;
            }
            else
            {
                if (Player.Id == emoItemData.followPlayerId)
                {
                    isSelf = true;
                }
                else
                {
                    return;
                }
            }
        }
        else
        {
            isSelf = playerData.playerInfo.Id == Player.Id;
        }
        
        cookie?.Stop();
        cookie = AKSoundManager.Inst.playEmoSound(name, gameObject,isSelf, count);
    }
    IEnumerator SetCallBack(float time)
    {
        isPlaying = true;
        yield return new WaitForSeconds(time - 0.15f);
        playerAnim.CrossFade("idle", 0.15f);
        yield return new WaitForSeconds(0.15f);
        RleasePrefab();
        ReAvoidFrameOnMutualFin();
        roleCon.SetCustomDefaultPos();
        playerAnim.Play(defulFaceName, 1, 0f);
        isPlaying = false;
        isInteracting = false;
        PlayMoveCor = null;
        AnimFinCallBack?.Invoke();
        AnimFinCallBack = null;
    }

    private void ReAvoidFrameOnMutualFin()
    {
        if (isInteracting)
        {
            SetOtherPlayerAvoidFrame(false);
        }
    }

    private void SetOtherPlayerAvoidFrame(bool state)
    {
        var playerCon = GetComponent<OtherPlayerCtr>();
        if (!playerCon || MagneticBoardManager.Inst.IsOtherPlayerOnBoard(playerCon))
        {
            return;
        }
        if (!playerCon || SeesawManager.Inst.IsOtherPlayerOnSeesaw(playerCon))
        {
            return;
        }
        playerCon.isAvoidFrame = state;
    }

    // cancel all face / body / audio of last emo
    public void CancelLastEmo()
    {
        StopSpeckAnim();
        ReAvoidFrameOnMutualFin();
        if (FaceMoveCor != null)
        {
            StopCoroutine(FaceMoveCor);
        }
        playerAnim.Play(defulFaceName, 1, 0f);

        if (PlayMoveCor != null)
        {
            StopCoroutine(PlayMoveCor);
        }
        if(PlayLoopCor != null)
        {
            StopCoroutine(PlayLoopCor);
        }

        if(CustomPlayAnimEndCor != null)
        {
            // PlayAnimEndCor
            StopCoroutine(CustomPlayAnimEndCor);
        }
        cookie?.Stop();
        isPlaying = false;
        isLooping = false;
        isInteracting = false;
        AnimFinCallBack?.Invoke();
        AnimFinCallBack = null;
        SetInteractState(false);
        KillEmo();
    }
    
    public void RleasePrefab() {
        if (expressionGameObject != null && expressionGameObject.Count > 0)
        {
            for (int i = 0; i < expressionGameObject.Count; i++)
            {
                Destroy(expressionGameObject[i]);
            }
            //Destroy(expressionGameObject);
            expressionGameObject.Clear();
            //     expressionGameObject = null;
        }
        isLooping = false;
        cookie?.StopLoop();
    }
    public void SetExpressionPrefab(EmoIconData info)
    {
        string path;
        for (int i = 0; i < info.effectCount; i++)
        {
            if (info.effectCount <= 1)
            {
                path = "Prefabs/Emotion/Express/" + info.name;
            }
            else
            {
                path = "Prefabs/Emotion/Express/" + info.name + "_" + (i + 1);
            }
            GameObject movePrefab = ResManager.Inst.LoadCharacterRes<GameObject>(path);
            if (movePrefab != null && playerModle.activeSelf)
            {

                expressionGameObject.Add(Instantiate(movePrefab));
                if (IsBandBody(i, info))
                {
                    expressionGameObject[i].transform.parent = playerModle.transform;
                    expressionGameObject[i].transform.localRotation = Quaternion.identity;
                    expressionGameObject[i].transform.localPosition = Vector3.zero;
                    expressionGameObject[i].transform.localScale = Vector3.one;
                }
                else
                {
                    expressionGameObject[i].transform.parent = roleCon.GetBandNode(info.bandBody[i].bandNode);
                    expressionGameObject[i].transform.localRotation = Quaternion.Euler(info.bandBody[i].r.x, info.bandBody[i].r.y, info.bandBody[i].r.z);
                    expressionGameObject[i].transform.localPosition = info.bandBody[i].p;
                    expressionGameObject[i].transform.localScale = info.bandBody[i].s;
                }
            }
        }
    }

    public bool IsBandBody(int id, EmoIconData info)
    {
        if (info.bandBody != null)
        {
            for (int i = 0; i < info.bandBody.Length; i++)
            {
                if (info.bandBody[i].id == id)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void PlayPickUp()
    {
        playerAnim.Play("pickup", 0, 0f);
        // PlayAudio("pickup");
    }
    public void PlayBounceplankJump(string name)
    {
        if (isPlaying || SelfieModeManager.Inst.isPlayingSelfieAnim)
        {
            return;
        }
        RleasePrefab();
        CancelLastEmo();
        if (playerAnim)
        {
            playerAnim.Play("bounceplank", 0, 0f);
            playerAnim.Play("trampoline_jump", 1, 0f);
            CustomPlayAnimEndCor = StartCoroutine(SetCustomAnimEndCallBack("trampoline_jump", 1, () => {
                ReleaseAndCancelLastEmo();
            }));
            AKSoundManager.Inst.PlayBounceplankSound(name, gameObject);
        }
       

       
        
    }

    public void PlayTrapHitAnim()
    {
        RleasePrefab();
        CancelLastEmo();
        if(playerAnim)
        {
            roleCon.SetFacialDefaultPos();
            playerAnim.Play("trapbox_hit", 0, 0f);
            playerAnim.Play("trapbox_hit", 1, 0f);
            CustomPlayAnimEndCor = StartCoroutine(SetCustomAnimEndCallBack("trapbox_hit",1,()=>{
                RleasePrefab();
                CancelLastEmo();
                roleCon.SetCustomDefaultPos();
            }));
        }
    } 
    //火焰伤害
    public void PlayFireHitAnim()
    {
        RleasePrefab();
        CancelLastEmo();
        if(playerAnim)
        {
            roleCon.SetFacialDefaultPos();
            playerAnim.Play("scald", 0, 0f);
            playerAnim.Play("scaldface", 1, 0f);
            CustomPlayAnimEndCor = StartCoroutine(SetCustomAnimEndCallBack("scaldface", 1,()=>{
                RleasePrefab();
                CancelLastEmo();
                roleCon.SetCustomDefaultPos();
            }));
        }
    }
    public void PlaySlideAnim(string clipName)
    {
        RleasePrefab();
        CancelLastEmo();
        if (playerAnim)
        {
            roleCon.SetFacialDefaultPos();
            playerAnim.Play(clipName, 1, 0f);
        }
    }
    public void ReleaseAndCancelLastEmo()
    {
        RleasePrefab();
        CancelLastEmo();
        if(roleCon)
        {
            roleCon.SetCustomDefaultPos();
        }
    }
    //上下梯子动画
    public void PlayLadderAnim(bool isOutAbove)
    {
        RleasePrefab();
        CancelLastEmo();
        if (playerAnim)
        {
            playerAnim.Play(isOutAbove? "climbing_up_out" : "climbing_down_out", 0, 0f);
           
        }
    }


    //自定义表情动作回调,如陷阱盒受击动画
    public IEnumerator SetCustomAnimEndCallBack(string animName,int layerIndex,Action callback)
    {   
        yield return new WaitForEndOfFrame();
        var info = playerAnim.GetCurrentAnimatorStateInfo(layerIndex);
        if(!info.IsName(animName)) yield break;
        yield return new WaitForSeconds(info.length);
        callback();
    }

    public bool isTyping;
    public void PlayType(TypeStatusData data)
    {
        // 牵手的玩家不显示打字动画
        var holdingHandsPlayerId = MutualManager.Inst.SearchHoldingHandsPlayers(data.playerId);
        if (isInteracting || !string.IsNullOrEmpty(holdingHandsPlayerId))
        {
            isTyping = false;
            return;
        }
        if (playerData == null)
        {
            playerData = GetComponent<PlayerData>();
        }
        if (playerData.playerInfo != null
            && data.playerId == playerData.playerInfo.Id )
        {
            OnAnimChange?.Invoke();
            OnAnimChange = null;
            RleasePrefab();
            CancelLastEmo();
            if (isLooping)
            {
                isLooping = false;
                cookie?.StopLoop();
                StopCoroutine(PlayLoop);
            }
            if (data.isStart)
            {
                playerAnim.CrossFade("type", 0.2F,  0, 0f);
                EmoIconData emoData = new EmoIconData();
                emoData.name = "type";
                emoData.bandBody = new BandBody[1];
                emoData.bandBody[0] = new BandBody();
                emoData.bandBody[0].bandNode = (int)BodyNode.RightHand;
                emoData.bandBody[0].p = Vector3.zero;
                emoData.bandBody[0].r = new Vector3(-90, 90, 0);
                emoData.bandBody[0].s = Vector3.one;
                SetExpressionPrefab(emoData);
                // audioSource.loop = true;
                // PlayAudio("type");
                isTyping = true;
                if (roleCon != null)
                {
                    roleCon.OnEmoPlay();
                    MessageHelper.Broadcast(MessageName.OnEmoPlay, curPlayerType == PlayerType.SelfPlayer);
                }
            }
            else if (isTyping == true)
            {
                playerAnim.CrossFade("idle", 0.2F,  0, 0f);
                // audioSource.Stop();
                // audioSource.loop = false;
                isTyping = false;
                isPlaying = false;
                if (roleCon != null)
                {
                    roleCon.OnEmoEnd();
                    MessageHelper.Broadcast(MessageName.OnEmoEnd, curPlayerType == PlayerType.SelfPlayer);
                }
            }
        }
       
    }

    public void SetOnAnimChangeAct(Action onAnimChange)
    {
        OnAnimChange = onAnimChange;
    }

    public void PlayAnim(Action onAnimChange, string stateName, int layer = 0, int normalizedTime = 0)
    {
        OnAnimChange?.Invoke();
        OnAnimChange = onAnimChange;
        playerAnim.Update(0);
        playerAnim.Play(stateName, layer, normalizedTime);
    }

    public float PlayAni(string animationName, int layer = 0, float normalizedTime = 0)
    {
        string tempClip = "tempClip" + layer.ToString();
        animatorOverrideController = new AnimatorOverrideController(playerAnim.runtimeAnimatorController);
        playerAnim.runtimeAnimatorController = animatorOverrideController;
        var clip = AssetBundleLoaderMgr.Inst.LoadAnimationClip(animationName);
        animatorOverrideController[tempClip] = clip;
        playerAnim.Play(tempClip, layer, normalizedTime);

        return clip.length;
    }

    public void OnDestroy()
    {
        MessageHelper.RemoveListener<TypeStatusData>(MessageName.TypeData, PlayType);
        OnEmoKill();
    }
    private IEnumerator PlayLoopAudio(float time,string name)
    {
        yield return new WaitForSeconds(time);
            PlayAudio(name, UInt32.MaxValue);
            if (cookie != null) cookie.max = uint.MaxValue;
            PlayLoopCor = null;
    }
    private void OnDisable()
    {
        isInteracting = false;
    }

    public bool CanPlayerMove()
    {
        if(isPlaying || isEating || isFishing)
        {
            return false;
        }
        return true;
    }

    public void StopAudio()
    {
        cookie?.Stop();
    }
}

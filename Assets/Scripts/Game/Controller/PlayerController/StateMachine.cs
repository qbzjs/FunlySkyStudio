using System.Collections.Generic;
using UnityEngine;
public enum EPlayerAnimState
{
    Idle,
    FastRun,
    Skate,
    Selfie,
    ParachuteGlideIdle, //降落伞滑翔静止
    ParachuteGlideMove, //降落伞滑翔移动
    ParachuteGlidePreLandEnd, //降落伞滑翔落地结束
    ParachuteOpenParachute, //降落伞开伞
    ParachuteFalling, //降落伞开伞
    ParachuteCloseParachute, //降落伞收伞
    ParachuteFallingPreLandEnd, //降落伞开伞落地结束
    SnowOpenSkateboard, //进入雪方块滑板
    SnowLeaveSkateboard, //离开雪方块滑板
    Seesaw, // 跷跷板
    Swing, // 秋千
}
public class OtherPlayerAnimStateManager : StateMachine<EPlayerAnimState>
{
    public OtherPlayerCtr mPlayerCtrl;
    public RoleController mPlayerRoleCtrl;

    public OtherPlayerAnimStateManager(OtherPlayerCtr ctrl)
    {
        mPlayerCtrl = ctrl;
        if (ctrl)
        {
            mPlayerRoleCtrl = mPlayerCtrl.transform.GetComponentInChildren<RoleController>(true);
        }
    }
    public void Init()
    {
        Add(EPlayerAnimState.Idle, null, null, null);
        Add(EPlayerAnimState.FastRun, null, null, null);
        Add(EPlayerAnimState.Skate, EnterSkateState, UpdateSkateRunState, LeaveSkateRunState);
        Add(EPlayerAnimState.Selfie, PlayEnterSelfieAnim, null, HandleExitSelfieMode);
        Add(EPlayerAnimState.ParachuteGlideIdle, EnterParachuteGlideIdle, null, LeaveParachuteGlideIdle);
        Add(EPlayerAnimState.ParachuteGlideMove, EnterParachuteGlideMove, null, LeaveParachuteGlideMove);
        Add(EPlayerAnimState.ParachuteGlidePreLandEnd, EnterParachuteGlidePreLandEnd, null, LeaveParachuteGlidePreLandEnd);
        Add(EPlayerAnimState.ParachuteOpenParachute, EnterParachuteOpenParachute, null, LeaveParachuteOpenParachute);
        Add(EPlayerAnimState.ParachuteFalling, EnterParachuteFalling, null, LeaveParachuteFalling);
        Add(EPlayerAnimState.ParachuteCloseParachute, EnterParachuteCloseParachute, null, LeaveParachuteCloseParachute);
        Add(EPlayerAnimState.ParachuteFallingPreLandEnd, EnterParachuteFallingPreLandEnd, null, LeaveParachuteFallingPreLandEnd);
        Add(EPlayerAnimState.SnowOpenSkateboard, EnterOpenSnowSkateState, UpdatemSnowSkatiingState, LeaveSnowLeaveSnowSkateState);
        Add(EPlayerAnimState.SnowLeaveSkateboard, EnterSnowLeaveSnowSkateState, null, LeaveSnowLeaveSnowSkateState);
        Add(EPlayerAnimState.Seesaw, PlayerOnSeesaw, null, PlayerLeaveSeesaw);
        Add(EPlayerAnimState.Swing, PlayerOnSwing, null, PlayerLeaveSwing);
    }
    #region 快跑
    private GameObject mRunEffectObj;
    private string mEffectPath = "Effect/run/run_smoke_01";
    public void EnterFastRunState()
    {
        if (mRunEffectObj == null)
        {
            //播放特效
            mRunEffectObj = GenEffectObj(mEffectPath);
        }
        if (mRunEffectObj != null)
        {
            SetEffectTransform(mRunEffectObj.transform);
            mRunEffectObj.SetActive(true);
        }
    }

    private void SetEffectTransform(Transform effectTran)
    {
        effectTran.SetParent(mPlayerCtrl.transform, false);
        effectTran.localPosition = Vector3.zero;
        effectTran.localRotation = Quaternion.identity;
        effectTran.localScale = Vector3.one;
    }
    private GameObject GenEffectObj(string effectpath)
    {
        GameObject obj = null;
        GameObject runEffectPrefab = ResManager.Inst.LoadRes<GameObject>(effectpath);
        if (runEffectPrefab == null)
        {
            Debug.LogError($"res is null  path is {effectpath}");
        }
        else
        {
            obj = GameObject.Instantiate(runEffectPrefab);
        }
        return obj;
    }
    public void UpdateFastRunState()
    {
        if (mPlayerCtrl.m_IsGround && mRunEffectObj != null && !mPlayerCtrl.m_IsInWater && !mPlayerCtrl.isJump)
        {
            mRunEffectObj.SetActive(true);
        }
        else
        {
            mRunEffectObj.SetActive(false);
        }
    }
    public void LeaveFastRunState()
    {
        //关闭特效
        if (mRunEffectObj != null)
        {
            mRunEffectObj.SetActive(false);
        }
    }
    #endregion

    #region 滑冰
    public GameObject mSkateEffectObj;
    private string mIceCubeEffectPath = "Effect/Skate/crushed_ice";
    public void EnterSkateState()
    {
        if (mSkateEffectObj == null)
        {
            //播放特效
            mSkateEffectObj = GenEffectObj(mIceCubeEffectPath);
        }
        if (mSkateEffectObj != null)
        {
            SetEffectTransform(mSkateEffectObj.transform);
            mSkateEffectObj.SetActive(true);
        }
    }

    public void UpdateSkateRunState()
    {
        if (mPlayerCtrl.m_IsGround && mSkateEffectObj != null && !mPlayerCtrl.m_IsInWater && !mPlayerCtrl.isJump)
        {
            mSkateEffectObj.SetActive(true);
        }
        else
        {
            mSkateEffectObj.SetActive(false);
        }
    }
    public void LeaveSkateRunState()
    {
        //关闭特效
        if (mSkateEffectObj != null)
        {
            mSkateEffectObj.SetActive(false);
        }
    }
    #endregion

    #region  自拍模式

    public void PlayEnterSelfieAnim()
    {
        if (mPlayerCtrl.IsInSelfieMode)
        {
            return;
        }
        mPlayerCtrl.IsInSelfieMode = true;
        //发起循环动作-->自拍
        mPlayerCtrl.animCon.PlayAnim((int)EmoName.EMO_SELFIE_MODE);
        TimerManager.Inst.RunOnce("enterSelfieMode", 1.3f, () =>
        {
            CreateSelfieTool();
        });
    }

    public void CreateSelfieTool()
    {
        if (mPlayerCtrl.animCon.isLooping && mPlayerCtrl.animCon.loopingInfo != null && mPlayerCtrl.animCon.loopingInfo.id == (int)EmoName.EMO_SELFIE_MODE)
        {
            if (mPlayerCtrl.effectTool == null)
            {
                var roleCon = mPlayerCtrl.m_PlayerAnim.gameObject.GetComponent<RoleController>();
                var leftNode = roleCon.GetBandNode((int)BodyNode.LEffectNode);
                var oldEffect = mPlayerCtrl.animCon.moveEffect.expressionGameObject[0];
                if (oldEffect)
                {
                    var effect = GameObject.Instantiate(oldEffect, leftNode);
                    effect.transform.localRotation = oldEffect.transform.localRotation;
                    effect.transform.localPosition = oldEffect.transform.localPosition;
                    effect.transform.localScale = oldEffect.transform.localScale;
                    mPlayerCtrl.effectTool = effect;
                    TimerManager.Inst.RunOnce("ResetIdle", 0.2f, () => { ResetIdle(); });
                }
            }
        }
    }

    public void RestoreSelfieModeAnim(int id)
    {
        var effectTool = mPlayerCtrl.effectTool;
        var emoIconData = MoveClipInfo.GetAnimName(id);
        var bandBody = emoIconData.moveInfos[1].bandBody;
        if (effectTool == null)
        {
            string name = emoIconData.name.Split('_')[0];
            string path = "Prefabs/Emotion/Express/" + name; ;
            GameObject movePrefab = ResManager.Inst.LoadCharacterRes<GameObject>(path);

            if (movePrefab != null)
            {
                effectTool = GameObject.Instantiate(movePrefab);
                var playerAnim = mPlayerCtrl.m_PlayerAnim;
                var roleCon = playerAnim.gameObject.GetComponent<RoleController>();
                var parentNode = roleCon.GetBandNode(bandBody[0].bandNode);
                effectTool.transform.SetParent(parentNode);
                mPlayerCtrl.effectTool = effectTool;
            }
        }

        effectTool.transform.localRotation = Quaternion.Euler(bandBody[0].r.x, bandBody[0].r.y, bandBody[0].r.z);
        effectTool.transform.localPosition = bandBody[0].p;
        effectTool.transform.localScale = bandBody[0].s;
    }


    public void ResetIdle()
    {
        mPlayerCtrl.animCon.RleasePrefab();
        mPlayerCtrl.animCon.CancelLastEmo();
        mPlayerCtrl.m_PlayerAnim.CrossFade("idle", 0.2f, 0, 0f);
    }

    public void HandleExitSelfieMode()
    {
        if (!mPlayerCtrl.IsInSelfieMode)
        {
            return;
        }
        mPlayerCtrl.IsInSelfieMode = false;
        mPlayerCtrl.m_PlayerAnim.Play("selfiestick_end", 0, 0);

        if (mPlayerCtrl.effectTool)
        {
            mPlayerCtrl.effectTool.GetComponent<Animator>().Play("selfiestick_end", 0, 0f);
        }
        // mPlayerCtrl.ChangeAnimationClips();
        TimerManager.Inst.RunOnce("ClearSelfieMode", 0.8f, () => { ClearSelfieMode(); });
    }

    public void ClearSelfieMode()
    {
        if (mPlayerCtrl.effectTool)
        {
            GameObject.Destroy(mPlayerCtrl.effectTool);
        }
        mPlayerCtrl.m_PlayerAnim.Play("idle", 0, 0);
    }

    #endregion
    
    #region 降落伞-滑翔
    
    public GameObject mParachuteGlideLeftHandEffectObj;
    public GameObject mParachuteGlideRightHandEffectObj;
    private string mParachuteGlideIdleEffectPath = "Effect/parachute/glide_hand_trail";
    
    public GameObject mParachuteGlideMoveEffectObj;
    private string mParachuteGlideMoveEffectPath = "Effect/parachute/glide_lina";

    public Transform mParachuteLeftHandTrans;
    public Transform mParachuteRightHandTrans;

    private void InitParachuteHandsEffects()
    {
        if (mParachuteGlideLeftHandEffectObj == null)
        {
            mParachuteGlideLeftHandEffectObj = GenEffectObj(mParachuteGlideIdleEffectPath);
        }
        if (mParachuteGlideRightHandEffectObj == null)
        {
            mParachuteGlideRightHandEffectObj = GenEffectObj(mParachuteGlideIdleEffectPath);
        }
        if (mParachuteLeftHandTrans == null && mPlayerRoleCtrl != null)
        {
            mParachuteLeftHandTrans = mPlayerRoleCtrl.GetBandNode((int)BodyNode.LeftHand);
        }
        if (mParachuteRightHandTrans == null && mPlayerRoleCtrl != null)
        {
            mParachuteRightHandTrans = mPlayerRoleCtrl.GetBandNode((int)BodyNode.RightHand);
        }

        
        if (mParachuteGlideLeftHandEffectObj != null && mParachuteLeftHandTrans != null)
        {
            SetParachuteEffectTransform(mParachuteGlideLeftHandEffectObj.transform, mParachuteLeftHandTrans);
            mParachuteGlideLeftHandEffectObj.SetActive(true);
        }
        if (mParachuteGlideRightHandEffectObj != null && mParachuteRightHandTrans != null)
        {
            SetParachuteEffectTransform(mParachuteGlideRightHandEffectObj.transform, mParachuteRightHandTrans);
            mParachuteGlideRightHandEffectObj.SetActive(true);
        }
    }

    private void HideParachuteHandsEffects()
    {
        if (mParachuteGlideLeftHandEffectObj != null)
        {
            mParachuteGlideLeftHandEffectObj.SetActive(false);
        }
        if (mParachuteGlideRightHandEffectObj != null)
        {
            mParachuteGlideRightHandEffectObj.SetActive(false);
        }
    }
    
    private void SetParachuteEffectTransform(Transform effectTran, Transform parent)
    {
        effectTran.SetParent(parent, false);
        effectTran.localPosition = Vector3.zero;
        effectTran.localRotation = Quaternion.identity;
        effectTran.localScale = Vector3.one;
    }

    private void EnterParachuteGlideIdle()
    {
        InitParachuteHandsEffects();
    }

    private void LeaveParachuteGlideIdle()
    {
        HideParachuteHandsEffects();
    }
    
    private void EnterParachuteGlideMove()
    {
        InitParachuteHandsEffects();
        if (mParachuteGlideMoveEffectObj == null)
        {
            mParachuteGlideMoveEffectObj = GenEffectObj(mParachuteGlideMoveEffectPath);
        }
        if (mParachuteGlideMoveEffectObj != null)
        {
            SetEffectTransform(mParachuteGlideMoveEffectObj.transform);
            mParachuteGlideMoveEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteGlideMove()
    {
        HideParachuteHandsEffects();
        if (mParachuteGlideMoveEffectObj != null)
        {
            mParachuteGlideMoveEffectObj.SetActive(false);
        }
    }

    #endregion
    
    
    #region 降落伞-滑翔落地结束

    public GameObject mParachuteGlidePreLandEndEffectObj;
    private string mParachuteGlidePreLandEndEffectPath = "Effect/parachute/glide_land_smoke_01";

    private void EnterParachuteGlidePreLandEnd()
    {
        if (mParachuteGlidePreLandEndEffectObj == null)
        {
            mParachuteGlidePreLandEndEffectObj = GenEffectObj(mParachuteGlidePreLandEndEffectPath);
        }

        if (mParachuteGlidePreLandEndEffectObj != null)
        {
            SetEffectTransform(mParachuteGlidePreLandEndEffectObj.transform);
            mParachuteGlidePreLandEndEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteGlidePreLandEnd()
    {
        if (mParachuteGlidePreLandEndEffectObj != null)
        {
            mParachuteGlidePreLandEndEffectObj.SetActive(false);
        }
    }
    
    #endregion
    
    #region 降落伞-开伞

    public GameObject mParachuteOpenParachuteEffectObj;
    private string mParachuteOpenParachuteEffectPath = "Effect/parachute/open_parachute_smoke";

    private void EnterParachuteOpenParachute()
    {
        if (mParachuteOpenParachuteEffectObj == null)
        {
            mParachuteOpenParachuteEffectObj = GenEffectObj(mParachuteOpenParachuteEffectPath);
        }

        if (mParachuteOpenParachuteEffectObj != null)
        {
            SetEffectTransform(mParachuteOpenParachuteEffectObj.transform);
            mParachuteOpenParachuteEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteOpenParachute()
    {
        if (mParachuteOpenParachuteEffectObj != null)
        {
            mParachuteOpenParachuteEffectObj.SetActive(false);
        }
    }
    
    #endregion

    #region 降落伞--降落中

    public GameObject mParachuteFallingEffectObj;
    private string mParachuteFallingEffectPath = "Effect/parachute/falling_trail";

    private void EnterParachuteFalling()
    {
        if (mParachuteFallingEffectObj == null)
        {
            mParachuteFallingEffectObj = GenEffectObj(mParachuteFallingEffectPath);
        }

        if (mParachuteFallingEffectObj != null)
        {
            SetEffectTransform(mParachuteFallingEffectObj.transform);
            mParachuteFallingEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteFalling()
    {
        if (mParachuteFallingEffectObj != null)
        {
            mParachuteFallingEffectObj.SetActive(false);
        }
    }

    #endregion
    
    #region 降落伞-收伞

    public GameObject mParachuteCloseParachuteEffectObj;
    private string mParachuteCloseParachuteEffectPath = "Effect/parachute/open_parachute_smoke";

    private void EnterParachuteCloseParachute()
    {
        if (mParachuteCloseParachuteEffectObj == null)
        {
            mParachuteCloseParachuteEffectObj = GenEffectObj(mParachuteCloseParachuteEffectPath);
        }

        if (mParachuteCloseParachuteEffectObj != null)
        {
            SetEffectTransform(mParachuteCloseParachuteEffectObj.transform);
            mParachuteCloseParachuteEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteCloseParachute()
    {
        if (mParachuteCloseParachuteEffectObj != null)
        {
            mParachuteCloseParachuteEffectObj.SetActive(false);
        }
    }
    
    #endregion
    
    
    #region 降落伞-开伞降落落地

    public GameObject mParachuteFallingPreLandEndEffectObj;
    private string mParachuteFallingPreLandEndEffectPath = "Effect/parachute/falling_land_smoke_02";

    private void EnterParachuteFallingPreLandEnd()
    {
        if (mParachuteFallingPreLandEndEffectObj == null)
        {
            mParachuteFallingPreLandEndEffectObj = GenEffectObj(mParachuteFallingPreLandEndEffectPath);
        }

        if (mParachuteFallingPreLandEndEffectObj != null)
        {
            SetEffectTransform(mParachuteFallingPreLandEndEffectObj.transform);
            mParachuteFallingPreLandEndEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteFallingPreLandEnd()
    {
        if (mParachuteFallingPreLandEndEffectObj != null)
        {
            mParachuteFallingPreLandEndEffectObj.SetActive(false);
        }
    }

    #endregion

    #region 雪方块-进入滑雪状态
    //滑板预制体
    public GameObject mskateboardObj;
    private string mskateboardObjPath = "Prefabs/Model/Special/SkateBoard";
    public Animator mskateboardAnim;
    //滑板特效
    public GameObject mSnowSkatiingEffectObj;
    private string mSnowSkatiingPath = "Effect/SnowSkate/skateboardEffect";

    public void EnterOpenSnowSkateState()
    {
        if (mskateboardObj == null)
        {
            mskateboardObj = GenEffectObj(mskateboardObjPath);
            SetEffectTransform(mskateboardObj.transform);
        }
        if (mskateboardObj != null)
        {
            mskateboardObj.SetActive(true);
            mskateboardAnim = mskateboardObj.GetComponent<Animator>();
            if (mskateboardAnim != null)
            {
                mskateboardAnim.Play(PlayerSnowSkateAnim.skiing_in, 0);
            }
            EntermSnowSkatiingState();
        }
    }
    #endregion

    #region 雪方块-离开滑雪状态
    public void EnterSnowLeaveSnowSkateState()
    {
        if (mskateboardObj == null)
        {
            mskateboardObj = GenEffectObj(mskateboardObjPath);
            SetEffectTransform(mskateboardObj.transform);
        }
        if (mskateboardObj != null && mskateboardAnim != null)
        {
            mskateboardAnim.Play(PlayerSnowSkateAnim.skiing_out, 0);
        }
    }

    public void LeaveSnowLeaveSnowSkateState()
    {
        if (mskateboardObj != null)
        {
            mskateboardObj.SetActive(false);
        }
        LeavemSnowSkatiingState();
    }
    #endregion

    #region 雪方块-滑动特效

    public void EntermSnowSkatiingState()
    {
        if (mSnowSkatiingEffectObj == null)
        {
            //播放特效
            mSnowSkatiingEffectObj = GenEffectObj(mSnowSkatiingPath);
        }
        if (mSnowSkatiingEffectObj != null)
        {
            SetEffectTransform(mSnowSkatiingEffectObj.transform);
            mSnowSkatiingEffectObj.SetActive(true);
        }
    }

    public void UpdatemSnowSkatiingState()
    {
        if (mPlayerCtrl.m_IsGround && mSnowSkatiingEffectObj != null && !mPlayerCtrl.m_IsInWater && !mPlayerCtrl.isJump)
        {
            mSnowSkatiingEffectObj.SetActive(true);
        }
        else
        {
            mSnowSkatiingEffectObj.SetActive(false);
        }
    }

    public void LeavemSnowSkatiingState()
    {
        //关闭特效
        if (mSnowSkatiingEffectObj != null)
        {
            mSnowSkatiingEffectObj.SetActive(false);
        }
    }
    #endregion

    #region 
    // 跷跷板动作

    BudTimer changeAnimTimer, leaveSeesawTimer, resetIdleTimer;
    public void PlayerOnSeesaw()
    {
        if(mPlayerCtrl.isOnSeesaw)
        {
            return;
        }
        mPlayerCtrl.isOnSeesaw = true;

        mPlayerCtrl.animCon.PlayAnim((int)EmoName.EMO_SEESAW_ANIM);
        mPlayerCtrl.animCon.defulFaceName = "seesaw_centre";
        mPlayerCtrl.animCon.PlayEyeAnim();
        AKSoundManager.Inst.PlaySeesawSound("Play_Seesaw_Sitdown", mPlayerCtrl.m_PlayerAnim.gameObject);
        ClearTimer();
        changeAnimTimer = TimerManager.Inst.RunOnce("changeAnim", 1f, () =>
        {
            mPlayerCtrl.SwitchSeesawAnimClips();
            mPlayerCtrl.m_PlayerAnim.Play("seesaw_centre", 1, 0f);
            ClearTimer();

            resetIdleTimer = TimerManager.Inst.RunOnce("resetIdle", 0.1f, () =>
            {
                mPlayerCtrl.m_PlayerAnim.CrossFade("idle", 0.2f, 0, 0f);
            });
        });
    }

    public void PlayerLeaveSeesaw()
    {
        if (!mPlayerCtrl.isOnSeesaw)
        {
            return;
        }
        mPlayerCtrl.isOnSeesaw = false;
        mPlayerCtrl.m_PlayerAnim.Play("seesaw_end", 0, 0);

        ClearTimer();
        leaveSeesawTimer = TimerManager.Inst.RunOnce("leaveSeesaw", 0.5f, () =>
        {
            AKSoundManager.Inst.PlaySeesawSound("Play_Seesaw_Standup", mPlayerCtrl.m_PlayerAnim.gameObject);
            if (PlayerStandonControl.Inst)
            {
                PlayerStandonControl.Inst.ResetStandOn();
            }
            mPlayerCtrl.animCon.defulFaceName = mPlayerCtrl.animCon.defaultEyeName;
            mPlayerCtrl.animCon.PlayEyeAnim();
            mPlayerCtrl.m_PlayerAnim.CrossFade("idle", 0.2f, 0, 0f);
        });
    }

    public void ClearTimer()
    {
        TimerManager.Inst.Stop(changeAnimTimer);
        TimerManager.Inst.Stop(leaveSeesawTimer);
        TimerManager.Inst.Stop(resetIdleTimer);
    }
    #endregion
    
    #region 
    // 秋千动作

    BudTimer SwingchangeAnimTimer, leaveSwingTimer;
    public void PlayerOnSwing()
    {
        if(mPlayerCtrl.isOnSwing)
        {
            return;
        }
        mPlayerCtrl.isOnSwing = true;
        mPlayerCtrl.animCon.defulFaceName = "swing_idle";
        
        AKSoundManager.Inst.PlaySwingSound("Play_Swing_Sitdown", mPlayerCtrl.m_PlayerAnim.gameObject);
        mPlayerCtrl.m_PlayerAnim.Play("swing_sitdown", 0, 0f);
        // mPlayerCtrl.m_PlayerAnim.Play("swing_sitdown", 1, 0f);
        ClearSwingTimer();
        SwingchangeAnimTimer = TimerManager.Inst.RunOnce("changeAnim", 1.2f, () =>
        {
            mPlayerCtrl.SwitchSwingAnimClips();
            mPlayerCtrl.m_PlayerAnim.CrossFade("swing_idle", 0.2f, 0, 0f);
            mPlayerCtrl.animCon.PlayEyeAnim();
        });
    }

    public void PlayerLeaveSwing()
    {
        if (!mPlayerCtrl.isOnSwing)
        {
            return;
        }
        mPlayerCtrl.isOnSwing = false;
        ClearSwingTimer();
        AKSoundManager.Inst.PlaySwingSound("Play_Swing_Standup", mPlayerCtrl.m_PlayerAnim.gameObject);
        mPlayerCtrl.m_PlayerAnim.Play("swing_getup", 0, 0);
        // mPlayerCtrl.m_PlayerAnim.Play("swing_getup", 1, 0);
        leaveSwingTimer = TimerManager.Inst.RunOnce("leaveSwing", 0.667f, () =>
        {
            if (PlayerStandonControl.Inst)
            {
                PlayerStandonControl.Inst.ResetStandOn();
            }
            mPlayerCtrl.m_PlayerAnim.CrossFade("idle", 0.2f, 0, 0f);
            mPlayerCtrl.animCon.PlayEyeAnim();
        });
    }

    public void ClearSwingTimer()
    {
        TimerManager.Inst.Stop(SwingchangeAnimTimer);
        TimerManager.Inst.Stop(leaveSwingTimer);
    }
    #endregion
}
public class PlayerAnimStateManager : StateMachine<EPlayerAnimState>
{
    public PlayerBaseControl mPlayerController;
    public RoleController mPlayerRoleCtrl;
    
    public PlayerAnimStateManager(PlayerBaseControl ctrl)
    {
        mPlayerController = ctrl;
        if (ctrl)
        {
            mPlayerRoleCtrl = mPlayerController.transform.GetComponentInChildren<RoleController>(true);
        }
    }
    public void Init()
    {
        Add(EPlayerAnimState.Idle, null, null, null);
        Add(EPlayerAnimState.FastRun, null, null, null);
        Add(EPlayerAnimState.Skate, EnterSkateState, UpdateSkateRunState, LeaveSkateRunState);
        Add(EPlayerAnimState.ParachuteGlideIdle, EnterParachuteGlideIdle, null, LeaveParachuteGlideIdle);
        Add(EPlayerAnimState.ParachuteGlideMove, EnterParachuteGlideMove, null, LeaveParachuteGlideMove);
        Add(EPlayerAnimState.ParachuteGlidePreLandEnd, EnterParachuteGlidePreLandEnd, null, LeaveParachuteGlidePreLandEnd);
        Add(EPlayerAnimState.ParachuteOpenParachute, EnterParachuteOpenParachute, null, LeaveParachuteOpenParachute);
        Add(EPlayerAnimState.ParachuteFalling, EnterParachuteFalling, null, LeaveParachuteFalling);
        Add(EPlayerAnimState.ParachuteCloseParachute, EnterParachuteCloseParachute, null, LeaveParachuteCloseParachute);
        Add(EPlayerAnimState.ParachuteFallingPreLandEnd, EnterParachuteFallingPreLandEnd, null, LeaveParachuteFallingPreLandEnd);
        Add(EPlayerAnimState.SnowOpenSkateboard, EnterOpenSnowSkateState, UpdatemSnowSkatiingState, LeaveSnowLeaveSnowSkateState);
        Add(EPlayerAnimState.SnowLeaveSkateboard, EnterSnowLeaveSnowSkateState, null, LeaveSnowLeaveSnowSkateState);
    }
    #region 快跑
    private GameObject mRunEffectObj;
    private string mEffectPath = "Effect/run/run_smoke_01";
    public void EnterFastRunState()
    {
        if (mRunEffectObj == null)
        {
            //播放特效
            mRunEffectObj = GenEffectObj(mEffectPath);
        }
        if (mRunEffectObj != null)
        {
            SetEffectTransform(mRunEffectObj.transform);
            mRunEffectObj.SetActive(true);
        }
    }

    private void SetEffectTransform(Transform effectTran)
    {
        effectTran.SetParent(mPlayerController.playerAnim.transform, false);
        effectTran.localPosition = Vector3.zero;
        effectTran.localRotation = Quaternion.identity;
        effectTran.localScale = Vector3.one;
    }
    private GameObject GenEffectObj(string effectpath)
    {
        GameObject obj = null;
        GameObject runEffectPrefab = ResManager.Inst.LoadRes<GameObject>(effectpath);
        if (runEffectPrefab == null)
        {
            Debug.LogError($"res is null  path is {effectpath}");
        }
        else
        {
            obj = GameObject.Instantiate(runEffectPrefab);
        }
        return obj;
    }
    public void UpdateFastRunState()
    {
        bool inWater = InWater();
        bool isTps = mPlayerController.isTps;//第三人称才播放
        if (mPlayerController.isGround && mRunEffectObj != null && !inWater && isTps && !mPlayerController.IsJump)
        {
            mRunEffectObj.SetActive(true);
        }
        else
        {
            mRunEffectObj.SetActive(false);
        }
    }
    private bool InWater()
    {
        var playerSwimCtrl = PlayerControlManager.Inst.GetPlayerCtrlMgr(PlayerControlType.Swim);
        if (playerSwimCtrl != null)
        {
            return (playerSwimCtrl as PlayerSwimControl).isInWater;
        }
        return false;
    }
    public void LeaveFastRunState()
    {
        //关闭特效
        if (mRunEffectObj != null)
        {
            mRunEffectObj.SetActive(false);
        }
    }
    #endregion

    #region 滑冰
    public GameObject mSkateEffectObj;
    private string mIceCubeEffectPath = "Effect/Skate/crushed_ice";
    public void EnterSkateState()
    {
        if (mSkateEffectObj == null)
        {
            //播放特效
            mSkateEffectObj = GenEffectObj(mIceCubeEffectPath);
        }
        if (mSkateEffectObj != null)
        {
            SetEffectTransform(mSkateEffectObj.transform);
            mSkateEffectObj.SetActive(true);
        }
    }

    public void UpdateSkateRunState()
    {
        bool inWater = InWater();
        bool isTps = mPlayerController.isTps;//第三人称才播放
        if (mPlayerController.isGround && mSkateEffectObj != null && !inWater && isTps)
        {
            mSkateEffectObj.SetActive(true);
        }
        else
        {
            mSkateEffectObj.SetActive(false);
        }
    }
    public void LeaveSkateRunState()
    {
        //关闭特效
        if (mSkateEffectObj != null)
        {
            mSkateEffectObj.SetActive(false);
        }
    }
    #endregion

    #region 降落伞-滑翔
    
    public GameObject mParachuteGlideLeftHandEffectObj;
    public GameObject mParachuteGlideRightHandEffectObj;
    private string mParachuteGlideIdleEffectPath = "Effect/parachute/glide_hand_trail";
    
    public GameObject mParachuteGlideMoveEffectObj;
    private string mParachuteGlideMoveEffectPath = "Effect/parachute/glide_lina";
    
    public Transform mParachuteLeftHandTrans;
    public Transform mParachuteRightHandTrans;

    private void InitParachuteHandsEffects()
    {
        if (mParachuteGlideLeftHandEffectObj == null)
        {
            mParachuteGlideLeftHandEffectObj = GenEffectObj(mParachuteGlideIdleEffectPath);
        }
        if (mParachuteGlideRightHandEffectObj == null)
        {
            mParachuteGlideRightHandEffectObj = GenEffectObj(mParachuteGlideIdleEffectPath);
        }
        if (mParachuteLeftHandTrans == null && mPlayerRoleCtrl != null)
        {
            mParachuteLeftHandTrans = mPlayerRoleCtrl.GetBandNode((int)BodyNode.LeftHand);
        }
        if (mParachuteRightHandTrans == null && mPlayerRoleCtrl != null)
        {
            mParachuteRightHandTrans = mPlayerRoleCtrl.GetBandNode((int)BodyNode.RightHand);
        }

        
        if (mParachuteGlideLeftHandEffectObj != null && mParachuteLeftHandTrans != null)
        {
            SetParachuteEffectTransform(mParachuteGlideLeftHandEffectObj.transform, mParachuteLeftHandTrans);
            mParachuteGlideLeftHandEffectObj.SetActive(true);
        }
        if (mParachuteGlideRightHandEffectObj != null && mParachuteRightHandTrans != null)
        {
            SetParachuteEffectTransform(mParachuteGlideRightHandEffectObj.transform, mParachuteRightHandTrans);
            mParachuteGlideRightHandEffectObj.SetActive(true);
        }
    }

    private void HideParachuteHandsEffects()
    {
        if (mParachuteGlideLeftHandEffectObj != null)
        {
            mParachuteGlideLeftHandEffectObj.SetActive(false);
        }
        if (mParachuteGlideRightHandEffectObj != null)
        {
            mParachuteGlideRightHandEffectObj.SetActive(false);
        }
    }
    
    private void SetParachuteEffectTransform(Transform effectTran, Transform parent)
    {
        effectTran.SetParent(parent, false);
        effectTran.localPosition = Vector3.zero;
        effectTran.localRotation = Quaternion.identity;
        effectTran.localScale = Vector3.one;
    }

    private void EnterParachuteGlideIdle()
    {
        InitParachuteHandsEffects();
    }
    
    private void LeaveParachuteGlideIdle()
    {
        HideParachuteHandsEffects();
    }
    
    private void EnterParachuteGlideMove()
    {
        InitParachuteHandsEffects();
        if (mParachuteGlideMoveEffectObj == null)
        {
            mParachuteGlideMoveEffectObj = GenEffectObj(mParachuteGlideMoveEffectPath);
        }
        if (mParachuteGlideMoveEffectObj != null)
        {
            SetEffectTransform(mParachuteGlideMoveEffectObj.transform);
            mParachuteGlideMoveEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteGlideMove()
    {
        HideParachuteHandsEffects();
        if (mParachuteGlideMoveEffectObj != null)
        {
            mParachuteGlideMoveEffectObj.SetActive(false);
        }
    }
    #endregion

    #region 降落伞-滑翔落地结束

    public GameObject mParachuteGlidePreLandEndEffectObj;
    private string mParachuteGlidePreLandEndEffectPath = "Effect/parachute/glide_land_smoke_01";

    private void EnterParachuteGlidePreLandEnd()
    {
        if (mParachuteGlidePreLandEndEffectObj == null)
        {
            mParachuteGlidePreLandEndEffectObj = GenEffectObj(mParachuteGlidePreLandEndEffectPath);
        }

        if (mParachuteGlidePreLandEndEffectObj != null)
        {
            SetEffectTransform(mParachuteGlidePreLandEndEffectObj.transform);
            mParachuteGlidePreLandEndEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteGlidePreLandEnd()
    {

        if (mParachuteGlidePreLandEndEffectObj != null)
        {
            mParachuteGlidePreLandEndEffectObj.SetActive(false);
        }
    }
    
    #endregion
    
    #region 降落伞-开伞

    public GameObject mParachuteOpenParachuteEffectObj;
    private string mParachuteOpenParachuteEffectPath = "Effect/parachute/open_parachute_smoke";

    private void EnterParachuteOpenParachute()
    {
        if (mParachuteOpenParachuteEffectObj == null)
        {
            mParachuteOpenParachuteEffectObj = GenEffectObj(mParachuteOpenParachuteEffectPath);
        }

        if (mParachuteOpenParachuteEffectObj != null)
        {
            SetParachuteEffectTransform(mParachuteOpenParachuteEffectObj.transform, mPlayerController.playerAnim.transform);
            mParachuteOpenParachuteEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteOpenParachute()
    {
        if (mParachuteOpenParachuteEffectObj != null)
        {
            mParachuteOpenParachuteEffectObj.SetActive(false);
        }
    }
    
    #endregion   
    
    #region 降落伞--降落中

    public GameObject mParachuteFallingEffectObj;
    private string mParachuteFallingEffectPath = "Effect/parachute/falling_trail";

    private void EnterParachuteFalling()
    {
        if (mParachuteFallingEffectObj == null)
        {
            mParachuteFallingEffectObj = GenEffectObj(mParachuteFallingEffectPath);
        }

        if (mParachuteFallingEffectObj != null)
        {
            SetEffectTransform(mParachuteFallingEffectObj.transform);
            mParachuteFallingEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteFalling()
    {
        if (mParachuteFallingEffectObj != null)
        {
            mParachuteFallingEffectObj.SetActive(false);
        }
    }

    #endregion
    
    #region 降落伞-收伞

    public GameObject mParachuteCloseParachuteEffectObj;
    private string mParachuteCloseParachuteEffectPath = "Effect/parachute/open_parachute_smoke";

    private void EnterParachuteCloseParachute()
    {
        if (mParachuteCloseParachuteEffectObj == null)
        {
            mParachuteCloseParachuteEffectObj = GenEffectObj(mParachuteCloseParachuteEffectPath);
        }

        if (mParachuteCloseParachuteEffectObj != null)
        {
            SetParachuteEffectTransform(mParachuteCloseParachuteEffectObj.transform, mPlayerController.playerAnim.transform);
            mParachuteCloseParachuteEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteCloseParachute()
    {
        if (mParachuteCloseParachuteEffectObj != null)
        {
            mParachuteCloseParachuteEffectObj.SetActive(false);
        }
    }
    
    #endregion

    #region 降落伞-开伞降落落地

    public GameObject mParachuteFallingPreLandEndEffectObj;
    private string mParachuteFallingPreLandEndEffectPath = "Effect/parachute/falling_land_smoke_02";

    private void EnterParachuteFallingPreLandEnd()
    {
        if (mParachuteFallingPreLandEndEffectObj == null)
        {
            mParachuteFallingPreLandEndEffectObj = GenEffectObj(mParachuteFallingPreLandEndEffectPath);
        }

        if (mParachuteFallingPreLandEndEffectObj != null)
        {
            SetEffectTransform(mParachuteFallingPreLandEndEffectObj.transform);
            mParachuteFallingPreLandEndEffectObj.SetActive(true);
        }
    }

    private void LeaveParachuteFallingPreLandEnd()
    {
        if (mParachuteFallingPreLandEndEffectObj != null)
        {
            mParachuteFallingPreLandEndEffectObj.SetActive(false);
        }
    }

    #endregion

    #region 雪方块-进入滑雪状态
    //滑板预制体
    public GameObject mskateboardObj;
    private string mskateboardObjPath = "Prefabs/Model/Special/SkateBoard";
    public Animator mskateboardAnim;
    //滑板特效
    public GameObject mSnowSkatiingEffectObj;
    private string mSnowSkatiingPath = "Effect/SnowSkate/skateboardEffect";

    public void EnterOpenSnowSkateState()
    {
        if (mskateboardObj == null)
        {
            mskateboardObj = GenEffectObj(mskateboardObjPath);
            SetEffectTransform(mskateboardObj.transform);
        }
        if (mskateboardObj != null)
        {
            mskateboardObj.SetActive(true);
            mskateboardAnim = mskateboardObj.GetComponent<Animator>();
            if (mskateboardAnim != null)
            {
                mskateboardAnim.Play(PlayerSnowSkateAnim.skiing_in, 0);
            }
            EntermSnowSkatiingState();
        }
    }
    #endregion

    #region 雪方块-离开滑雪状态
    public void EnterSnowLeaveSnowSkateState()
    {
        if (mskateboardObj == null)
        {
            mskateboardObj = GenEffectObj(mskateboardObjPath);
            SetEffectTransform(mskateboardObj.transform);
        }
        if (mskateboardObj != null && mskateboardAnim != null)
        {
            mskateboardAnim.Play(PlayerSnowSkateAnim.skiing_out, 0);
        }
    }

    public void LeaveSnowLeaveSnowSkateState()
    {
        if (mskateboardObj != null)
        {
            mskateboardObj.SetActive(false);
        }
        LeavemSnowSkatiingState();
    }
    #endregion

    #region 雪方块-滑动特效

    public void EntermSnowSkatiingState()
    {
        if (mSnowSkatiingEffectObj == null)
        {
            //播放特效
            mSnowSkatiingEffectObj = GenEffectObj(mSnowSkatiingPath);
        }
        if (mSnowSkatiingEffectObj != null)
        {
            SetEffectTransform(mSnowSkatiingEffectObj.transform);
            mSnowSkatiingEffectObj.SetActive(true);
        }
    }

    public void UpdatemSnowSkatiingState()
    {
        bool inWater = InWater();
        bool isTps = mPlayerController.isTps;//第三人称才播放
        if (mPlayerController.isGround && mSnowSkatiingEffectObj != null && !inWater && isTps)
        {
            mSnowSkatiingEffectObj.SetActive(true);
        }
        else
        {
            mSnowSkatiingEffectObj.SetActive(false);
        }
    }
    public void LeavemSnowSkatiingState()
    {
        //关闭特效
        if (mSnowSkatiingEffectObj != null)
        {
            mSnowSkatiingEffectObj.SetActive(false);
        }
    }
    #endregion
}
public class StateMachine<T>
{
    public delegate void StateFunc();
    State mCurrentState = null;
    Dictionary<T, State> mStates = new Dictionary<T, State>();
    public void Add(T id, StateFunc enter, StateFunc update, StateFunc leave)
    {
        mStates.Add(id, new State(id, enter, update, leave));
    }

    public T CurrentState()
    {
        return mCurrentState.Id;
    }

    public void Update()
    {
        if (mCurrentState != null && mCurrentState.Update != null)
        {
            mCurrentState.Update();
        }
    }
    public void SwitchTo(T state)
    {
        var newState = mStates[state];
        if (mCurrentState != null && mCurrentState.Id.Equals(newState.Id))
        {
            return;
        }
        if (mCurrentState != null && mCurrentState.Leave != null)
            mCurrentState.Leave();
        if (newState.Enter != null)
            newState.Enter();
        mCurrentState = newState;

    }
    public void Shutdown()
    {
        if (mCurrentState != null && mCurrentState.Leave != null)
            mCurrentState.Leave();
        mCurrentState = null;
    }
    class State
    {
        public State(T id, StateFunc enter, StateFunc update, StateFunc leave)
        {
            Id = id;
            Enter = enter;
            Update = update;
            Leave = leave;
        }
        public T Id;
        public StateFunc Enter;
        public StateFunc Update;
        public StateFunc Leave;
    }
}

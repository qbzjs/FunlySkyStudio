using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:Shaocheng
/// Description:玩家站在哪些道具上的检测和控制
/// Date: 2022-7-14 14:09:48
/// </summary>
public enum StandOnType
{
    Nothing = 0, //普通地形
    IceCube = 1, //冰方块
    Water = 2, //水方块 -- 水方块不通过射线检测,请使用PlayerSwimControl.Inst.isSwimming
    Bounceplank = 3, //蹦床
    SnowCube = 4, //雪方块
    Seesaw = 5, // 跷跷板
}

public static class StandOnAudioType
{
    public static string skateAudio = "skate";
    public static string mutualSkateAudio = "handle_skate";
    public static string waterAudio = "underwater";
    public static string defaultAudio = "default";
    public static string snowAudio = "snow";
}

public class StandOnWhat
{
    public StandOnType StandOnType;
    public bool IsGroundDetect; //是否使用射线进行IsGround检测代替CharacterController的IsGround
    public Action<StandOnType, GameObject> StandOnEnter; //踩上某种道具action
    public Action<StandOnType, GameObject> StandOnChange; //踩上某种道具后，在道具之间切换，但type未改变
    public Action<StandOnType, GameObject> StandOnLeave; //离开某种道具action
    public Action<StandOnType> StandOnAir; //在某种道具上飞出，例如冰方块雪方块等可以高速滑出

    public StandOnWhat(StandOnType standOnType, bool isGroundDetect,
        Action<StandOnType, GameObject> standOnEnter, 
        Action<StandOnType, GameObject> standOnChange, 
        Action<StandOnType, GameObject> standOnLeave, 
        Action<StandOnType> standOnAir = null)
    {
        IsGroundDetect = isGroundDetect;
        StandOnType = standOnType;
        StandOnEnter = standOnEnter;
        StandOnChange = standOnChange;
        StandOnLeave = standOnLeave;
        StandOnAir = standOnAir;
    }
}

public class PlayerStandonControl : MonoBehaviour, IPlayerCtrlMgr
{
    [HideInInspector] public static PlayerStandonControl Inst;
    [HideInInspector] public PlayerBaseControl playerBase;

    [HideInInspector] public Dictionary<string, StandOnWhat> tagStandOnDict = new Dictionary<string, StandOnWhat>();
    [HideInInspector] public Dictionary<string, Action<GameObject>> tagOnHitDict = new Dictionary<string, Action<GameObject>>(); //射线触发事件注册

    [NonSerialized] public float maxDetectDistance = 1.5f;
    private float curRayDistance = 1.5f;
    
    public StandOnWhat CurStandOnWhat = null;
    private GameObject curHitObject;
    private int skipRaycastDetect;
    
    //和playerBaseControl会有时序问题（例如Jump时就设为false），使用标记位控制是否能检测
    private bool _isCanDetectGround = true;
    private BudTimer isGroundTimer;
    public bool IsGround;
    
    public void Awake()
    {
        Inst = this;
        PlayerControlManager.Inst = new PlayerControlManager();
        PlayerControlManager.Inst.AddPlayerCtrlMgr(PlayerControlType.StandOn, Inst);
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        playerBase = PlayerControlManager.Inst.playerBase;
        skipRaycastDetect = ~ LayerMask.GetMask("Player", "OtherPlayer", "Weapon", "PVPArea", "TriggerModel", "Touch", "WaterCube", "Trigger");
        curRayDistance = maxDetectDistance;
        
        InitDict();
    }

    private void InitDict()
    {
        //人物站立在某物体上检测， Key-tag名
        tagStandOnDict.Add("Nothing", new StandOnWhat(StandOnType.Nothing, false, EnterNothing, null, LeaveNothing));
        tagStandOnDict.Add("Water", new StandOnWhat(StandOnType.Water, false, null, null, null));
        tagStandOnDict.Add("IceCube", new StandOnWhat(StandOnType.IceCube, false, IceCubeManager.Inst.EnterIceCube, null, IceCubeManager.Inst.LeaveIceCube));
        tagStandOnDict.Add("SnowCube", new StandOnWhat(StandOnType.SnowCube, true, SnowCubeManager.Inst.EnterSnowCube, SnowCubeManager.Inst.ChangeSnowCube, SnowCubeManager.Inst.LeaveSnowCube, SnowCubeManager.Inst.OnAir));
        tagStandOnDict.Add("Seesaw", new StandOnWhat(StandOnType.Seesaw, true, null, null, null));

        //人物朝下物体击中检测， Key-tag名
        tagOnHitDict.Add("Bounceplank", BounceplankManager.Inst.BouncePlankJump);

        CurStandOnWhat = tagStandOnDict["Nothing"];
    }

    public void OnDestroy()
    {
        if (isGroundTimer != null)
        {
            TimerManager.Inst.Stop(isGroundTimer);
        }
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        CurStandOnWhat = null;
        curHitObject = null;
        _isCanDetectGround = true;
        tagOnHitDict.Clear();
        Inst = null;
    }

    private void OnChangeMode(GameMode gameMode)
    {
        LoggerUtils.Log($"PlayerStandOnControl OnChangeMode gameMode:{gameMode}");
        ResetStandOn();
    }

    private bool IsCanDetect()
    {
        //player形象创建出来才开始检测
        if (playerBase == null || playerBase.isChanged == false)
        {
            return false;
        }

        if (playerBase.playerCenter == null)
        {
            return false;
        }
        
        //在空中时跳过检测，保持原有状态
        // if (playerBase && !playerBase.PlayerIsGround())
        // {
        //     return false;
        // }

        if (StateManager.IsParachuteUsing)
        {
            return false;
        }

        if (playerBase.animCon && playerBase.animCon.isPlaying)
        {
            return false;
        }

        //参照模式不检测
        if (ReferManager.Inst != null && ReferManager.Inst.isRefer)
        {
            return false;
        }

        // 自己为牵手状态中的跟随者，不进行检测，以牵手发起者的为主
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isFollowPlayer)
        {
            var sPlayer = ClientManager.Inst.GetOtherPlayerComById(PlayerMutualControl.Inst.startPlayerId);
            var sPlayerCurType = FrameStateManager.Inst.GetStandOnType(sPlayer.CurrentAnimType);
            if (sPlayer && CurStandOnWhat != null && sPlayerCurType != CurStandOnWhat.StandOnType)
            {
                CurStandOnWhat?.StandOnLeave?.Invoke(sPlayerCurType, curHitObject);
                CurStandOnWhat = GetStandOnWhatByType(sPlayerCurType);
                CurStandOnWhat?.StandOnEnter?.Invoke(sPlayerCurType, curHitObject);
            }

            return false;
        }

        if (StateManager.IsFishing)
        {
            return false;
        }

        //飞行则重置
        if (playerBase && playerBase.isFlying)
        {
            ResetStandOn();
            return false;
        }

        //蹦床则重置
        if (playerBase && playerBase.isBounceplankJumping)
        {
            ResetStandOn();
            return false;
        }

        //游泳时重置，使用默认动作
        if (PlayerSwimControl.Inst && PlayerSwimControl.Inst.isInWater)
        {
            // ResetStandOn();
            JustResetStandOnType();
            return false;
        }

        //磁力板方向盘重置
        if (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard)
        {
            ResetStandOn();
            return false;
        }
        
        if (StateManager.IsOnSeesaw)
        {
            ResetStandOn();
            return false;
        }
        
        if (StateManager.IsOnSwing)
        {
            ResetStandOn();
            return false;
        }

        //梯子方向盘重置
        if (StateManager.IsOnLadder)
        {
            ResetStandOn();
            return false;
        }
        //滑梯方向盘重置
        if (StateManager.IsOnSlide)
        {
            ResetStandOn();
            return false;
        }
        //驾驶方向盘时重置
        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel)
        {
            ResetStandOn();
            return false;
        }

        return true;
    }

    private void Update()
    {
        if (IsCanDetect() == false)
        {
            return;
        }

        var pTrans = playerBase.playerCenter.transform;
        Ray ray = new Ray(pTrans.position, -pTrans.up);
        if (Physics.Raycast(ray, out var raycastHit, curRayDistance, skipRaycastDetect))
        {
            // LoggerUtils.Log("fsc-------->射线检测到-----" + raycastHit.transform.gameObject);
            if (curHitObject != raycastHit.transform.gameObject)
            {
                curHitObject = raycastHit.transform.gameObject;
                CheckStandOnWhat(curHitObject);
                CheckOnHit(curHitObject);
            }
            
            if (IsGroundDetect() && _isCanDetectGround)
            {
                IsGround = true;
            }
        }
        else
        {
            // LoggerUtils.Log("fsc-------->射线未检测到任何东西");
            if (IsGroundDetect() && _isCanDetectGround)
            {
                IsGround = false;
            }

            if (playerBase.PlayerIsGround())
            {
                ResetStandOn();
            }
            else if (CurStandOnWhat != null && CurStandOnWhat.StandOnAir != null)
            {
                CurStandOnWhat.StandOnAir.Invoke(CurStandOnWhat.StandOnType);
            }
        }

#if UNITY_EDITOR
        // Debug.DrawRay(pTrans.position, -pTrans.up, Color.yellow);
        //Debug.DrawLine(pTrans.position, new Vector3(pTrans.position.x, pTrans.position.y - 1.5f, pTrans.position.z), Color.yellow);
#endif
    }

    private StandOnWhat GetStandOnWhatByType(StandOnType type)
    {
        if (tagStandOnDict == null || tagStandOnDict.Count <= 0)
        {
            return null;
        }

        foreach (var w in tagStandOnDict.Values)
        {
            if (w.StandOnType == type)
            {
                return w;
            }
        }

        return null;
    }

    public void ResetStandOn()
    {
        curHitObject = null;
        CheckStandOnWhat(null);
    }

    //只切换状态值，不切动画片段 从冰上进入水方块会使用到，防止进水方块时因为切动画片段而顿一下
    private void JustResetStandOnType()
    {
        curHitObject = null;
        CurStandOnWhat = tagStandOnDict["Water"];
    }

    private void CheckOnHit(GameObject standOnGo)
    {
        if (standOnGo != null)
        {
            if (tagOnHitDict.ContainsKey(standOnGo.tag))
            {
                tagOnHitDict[standOnGo.tag]?.Invoke(standOnGo);
            }
        }
    }

    private void CheckStandOnWhat(GameObject standOnGo)
    {
        var newStandOnTag = "Nothing";
        if (standOnGo != null &&
            !StateManager.IsInSelfieMode &&
            tagStandOnDict.ContainsKey(standOnGo.tag) &&
            tagStandOnDict[standOnGo.tag] != null)
        {
            newStandOnTag = standOnGo.tag;
        }

        var newStandOn = tagStandOnDict[newStandOnTag];
        if (newStandOn != null && CurStandOnWhat != null)
        {
            if (newStandOn.StandOnType != CurStandOnWhat.StandOnType)
            {
                CurStandOnWhat.StandOnLeave?.Invoke(newStandOn.StandOnType, curHitObject);
                CurStandOnWhat = newStandOn;
                CurStandOnWhat.StandOnEnter?.Invoke(newStandOn.StandOnType, curHitObject);
            }
            else
            {
                CurStandOnWhat.StandOnChange?.Invoke(newStandOn.StandOnType, curHitObject);
            }
        }
    }

    private void EnterNothing(StandOnType standOnType, GameObject standOnObj)
    {
        LoggerUtils.Log($"PlayerStandonControl: EnterNothing");

        if (PlayerControlManager.Inst && playerBase)
        {
            playerBase.Move(Vector3.zero);
            PlayerControlManager.Inst.ChangeAnimClips();
        }
    }

    private void LeaveNothing(StandOnType standOnType, GameObject standOnObj)
    {
        LoggerUtils.Log($"PlayerStandonControl: LeaveNothing");
    }


    # region 外部接口

    public bool IsGroundDetect()
    {
        return CurStandOnWhat != null && CurStandOnWhat.IsGroundDetect;
    }

    public bool GetIsGround()
    {
        return IsGround;
    }

    //这里需要手动设置下雪方块isGroundSnow,
    //否则和雪方块每帧的射线检测有时序问题导致IsGroundSnow没有及时刷成false就直接PlayerLand
    public void OnClickJump()
    {
        if (IsGroundDetect())
        {
            IsGround = false;
            _isCanDetectGround = false;

            TimerManager.Inst.Stop(isGroundTimer);
            isGroundTimer = TimerManager.Inst.RunOnce("isGroundTimer", GetCurJumpTime(), () =>
            {
                _isCanDetectGround = true;
            });
        }
    }

    // 拿到跳跃时间，跳跃期间不做检测
    private float GetCurJumpTime()
    {
        if (playerBase == null || playerBase.playerAnim == null || playerBase.playerAnim.runtimeAnimatorController == null)
        {
            return 0.3f;
        }

        if (playerBase.isFastRun)
        {
            var clip = playerBase.GetOverrideAnimClip(AnimClipType.Fast_Run);
            return clip ? clip.length - 0.35f : 0.3f;
        }
        else
        {
            var clip = playerBase.GetOverrideAnimClip(AnimClipType.Jump);
            return clip ? clip.length - 0.3f : 0.3f;
        }
    }

    public void OnPlayerLanded()
    {
        _isCanDetectGround = true;
        // curRayDistance = maxDetectDistance;
    }

    public bool IsStandOnIceCube()
    {
        return CurStandOnWhat != null && CurStandOnWhat.StandOnType == StandOnType.IceCube;
    }

    public StandOnType GetStandOnType()
    {
        return CurStandOnWhat == null ? StandOnType.Nothing : CurStandOnWhat.StandOnType;
    }

    public GameObject GetCurHitObj()
    {
        return curHitObject;
    }

    public bool CheckPlayerStandonAudio(FootSoundInfo info)
    {
        info.switchState = "default";
        if (IsStandOnIceCube())
        {
            info.switchState = "skate";
            return true;
        }

        return false;
    }

    public bool IsCanFastRun()
    {
        // if (IsStandOnIceCube())
        // {
        //     return false;
        // }
        return true;
    }

    /// <summary>
    /// 是否开启滑冰动作,参照模式不使用滑冰
    /// </summary> 
    /// <returns></returns>
    public bool IsShouldRunInIceCube()
    {
        var standOnIce = IsStandOnIceCube();
        var isRefer = ReferManager.Inst != null && ReferManager.Inst.isRefer;
        return standOnIce && !isRefer;
    }
    
    public bool IsStandOnSnowCube()
    {
        return CurStandOnWhat != null && CurStandOnWhat.StandOnType == StandOnType.SnowCube;
    }

    #endregion
}
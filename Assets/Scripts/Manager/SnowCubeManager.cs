using System.Collections.Generic;
using UnityEngine;

public struct PlayerSnowSkateAnim
{
    public const string SnowAnimPara_RunOffset = "RunOffset";
    
    public const string skiing_l = "skiing_l";
    public const string skiing_r = "skiing_r";
    public const string skiing_in = "skiing_in";
    public const string skiing_out = "skiing_out";
    public const string skiing = "run_fast";
    public const string skiing_jump = "skiing_jump";
}

/// <summary>
/// Author: LiShuzhan
/// Description:
/// Date: 2022-08-16
/// </summary>
public class SnowCubeManager : ManagerInstance<SnowCubeManager>, IManager, IPVPManager
{
    public const string SNOW_TAG = "SnowCube";
    public int MAX_COUNT = 999;
    public const string MAX_COUNT_TIP = "Up to 999 Snow Cubes can be placed.";
    public List<NodeBaseBehaviour> bevs = new List<NodeBaseBehaviour>();

    //当前站在某个雪方块上
    private SnowCubeBehaviour curStandSnowCubeBev;

    #region 声音

    public const string SnowSound_ForwardSkating1P_Start = "Play_Snow_Ski_Front_1P";
    public const string SnowSound_ForwardSkating1P_Stop = "Stop_Snow_Ski_Front_1P";
    public const string SnowSound_ForwardSkating3P_Start = "Play_Snow_Ski_Front_3P";
    public const string SnowSound_ForwardSkating3P_Stop = "Stop_Snow_Ski_Front_3P";

    public const string SnowSound_SideSkating1P_Start = "Play_Snow_Ski_Side_1P";
    public const string SnowSound_SideSkating1P_Stop = "Stop_Snow_Ski_Side_1P";
    public const string SnowSound_SideSkating3P_Start = "Play_Snow_Ski_Side_3P";
    public const string SnowSound_SideSkating3P_Stop = "Stop_Snow_Ski_Side_3P";
    
    public const string SnowSound_Play_Snow_Skateboard_Appear = "Play_Snow_Skateboard_Appear";
    public const string SnowSound_Play_Snow_Skateboard_Disappear = "Play_Snow_Skateboard_Disappear";
    

    public void StopSkatingSound(bool isSelf, GameObject gameObject)
    {
        if (isSelf)
        {
            AKSoundManager.Inst.PostEvent(SnowSound_ForwardSkating1P_Stop, gameObject);
            AKSoundManager.Inst.PostEvent(SnowSound_SideSkating1P_Stop, gameObject);
        }
        else
        {
            AKSoundManager.Inst.PostEvent(SnowSound_ForwardSkating3P_Stop, gameObject);
            AKSoundManager.Inst.PostEvent(SnowSound_SideSkating3P_Stop, gameObject);
        }
    }

    #endregion

    public void Init()
    {
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
        MessageHelper.AddListener(MessageName.ChangeTps, OnChangeTps);
        MessageHelper.AddListener(MessageName.OnForeground, OnForeGround);
    }

    public override void Release()
    {
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener(MessageName.ChangeTps, OnChangeTps);
        MessageHelper.RemoveListener<bool>(MessageName.OpenPackPanel, HandlePackPanelShow);
        MessageHelper.RemoveListener(MessageName.OnForeground, OnForeGround);
        base.Release();
        Clear();
        curStandSnowCubeBev = null;
    }

    protected void HandlePackPanelShow(bool isShow)
    {
        for (int i = 0; i < bevs.Count; i++)
        {
            if (!LockHideManager.Inst.hideList.Contains(bevs[i].entity))
            {
                bevs[i].gameObject.SetActive(!isShow);
            }
        }
    }

    private void OnForeGround()
    {
        if (StateManager.IsSnowCubeSkating)
        {
            PlayerSnowSkateControl.Inst.OnForeground();
        }
    }

    private void OnChangeMode(GameMode mode)
    {
        if (PlayerSnowSkateControl.Inst)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
    }

    private void OnChangeTps()
    {
        if (PlayerSnowSkateControl.Inst)
        {
            PlayerSnowSkateControl.Inst.OnChangeTps();
        }
    }

    public bool IsOverMaxCount()
    {
        if (bevs.Count >= MAX_COUNT)
        {
            return true;
        }

        return false;
    }

    public bool IsCanClone(GameObject curTarget)
    {
        if (curTarget.GetComponentInChildren<SnowCubeBehaviour>() != null)
        {
            int CombineCount = curTarget.GetComponentsInChildren<SnowCubeBehaviour>().Length;
            if (CombineCount > 1)
            {
                if (CombineCount + bevs.Count > MAX_COUNT)
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
            else
            {
                if (IsOverMaxCount())
                {
                    TipPanel.ShowToast(MAX_COUNT_TIP);
                    return false;
                }
            }
        }

        return true;
    }

    public void AddItem(NodeBaseBehaviour b)
    {
        if (!bevs.Contains(b))
        {
            bevs.Add(b);
        }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        GameObjectComponent goCmp = behaviour.entity.Get<GameObjectComponent>();

        if (goCmp.modelType == NodeModelType.SnowCube)
        {
            bevs.Remove(behaviour);
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.SnowCube)
        {
            if (!bevs.Contains(behaviour))
            {
                bevs.Add(behaviour);
            }
        }
    }

    public void Clear()
    {
        if (bevs != null)
        {
            bevs.Clear();
        }
    }

    public bool IsStandOnSnowCube()
    {
        return PlayerStandonControl.Inst &&
               PlayerStandonControl.Inst.CurStandOnWhat != null &&
               PlayerStandonControl.Inst.CurStandOnWhat.StandOnType == StandOnType.SnowCube;
    }

    public SnowCubeBehaviour GetCurStandOnSnowCubeBev()
    {
        return curStandSnowCubeBev;
    }

    private void RefreshCurStandSnowCubeBev(GameObject hitObj)
    {
        if (hitObj == null || hitObj.transform.parent == null || hitObj.transform.parent.parent == null)
        {
            return;
        }

        curStandSnowCubeBev = hitObj.transform.parent.parent.GetComponent<SnowCubeBehaviour>();
    }

    public void EnterSnowCube(StandOnType standOnType, GameObject standObj)
    {
        LoggerUtils.Log($"SnowCubeManager EnterSnowCube-->standOnType:{standOnType}, standObj:{standObj?.name}");

        // 刷当前站在的snowCube
        RefreshCurStandSnowCubeBev(standObj);
        
        //站到雪方块上
        if (PlayerSnowSkateControl.Inst == null)
        {
            PlayerControlManager.Inst.playerControlNode.AddComponent<PlayerSnowSkateControl>();
            SwordManager.Inst.forceInterrupt();
        }

        PlayerSnowSkateControl.Inst.EnterSnowCube();
        PlayerControlManager.Inst.ChangeAnimClips();

    }

    public void ChangeSnowCube(StandOnType standOnType, GameObject standOnObj)
    {
        LoggerUtils.Log($"SnowCubeManager ChangeSnowCube-->standOnType:{standOnType}, standObj:{standOnObj?.name}");
        RefreshCurStandSnowCubeBev(standOnObj);
    }

    public void LeaveSnowCube(StandOnType standOnType, GameObject standOnObj)
    {
        LoggerUtils.Log($"SnowCubeManager LeaveSnowCube-->standOnType:{standOnType}, standObj:{standOnObj?.name}");

        //离开雪方块
        if (PlayerSnowSkateControl.Inst != null)
        {
            PlayerSnowSkateControl.Inst.LeaveSnowCube();
        }

        curStandSnowCubeBev = null;
    }

    public void OnAir(StandOnType standOnType)
    {
        LoggerUtils.Log($"SnowCubeManager OnAir-->standOnType:{standOnType}");

        if (PlayerSnowSkateControl.Inst)
        {
            PlayerSnowSkateControl.Inst.OnStandAir();
        }
        
    }

    #region 滑板动画切换

    public void ChangeSkateboardState(Animator skateboardAnim, int playerAnimState)
    {
        if (skateboardAnim == null || !skateboardAnim.gameObject.activeSelf)
        {
            return;
        }

        skateboardAnim.SetInteger(PlayerSnowSkateAnim.SnowAnimPara_RunOffset, playerAnimState);
    }

    #endregion


    public void OnReset()
    {
        if (PlayerSnowSkateControl.Inst)
        {
            PlayerSnowSkateControl.Inst.ForceStopSkating();
        }
    }

    #region 声音相关

    //判断是单人脚步声还是双人牵手脚步声
    public FootSoundInfo GetSimpleOrMultSoundInfo(FootSoundInfo info)
    {
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            info.switchState = StandOnAudioType.snowAudio;
        }
        else
        {
            info.switchState = StandOnAudioType.snowAudio;
        }

        return info;
    }

    // 播放板子上滑雪声音，分直行和左右偏，左右偏的声音播放时要过一会再把直行声音停下来，音效support@曾鹏
    public void PlaySkatingSound(bool isSelf, GameObject go, bool isForward)
    {
        if (go == null)
        {
            return;
        }

        if (isForward)
        {
            AKSoundManager.Inst.PostEvent(isSelf ? SnowSound_SideSkating1P_Stop : SnowSound_SideSkating3P_Stop, go);
            AKSoundManager.Inst.PostEvent(isSelf ? SnowSound_ForwardSkating1P_Start : SnowSound_ForwardSkating3P_Start, go);
        }
        else
        {
            AKSoundManager.Inst.PostEvent(isSelf ? SnowSound_SideSkating1P_Start : SnowSound_SideSkating3P_Start, go);
            AKSoundManager.Inst.PostEvent(isSelf ? SnowSound_ForwardSkating1P_Stop : SnowSound_ForwardSkating3P_Stop, go);
        }
    }

    #endregion

    #region 联机

    public void HandleFrameState(OtherPlayerCtr otherPlayerCtr, OtherPlayerAnimStateManager animStateManager, Animator playerAnim, FrameStateType stateType)
    {
        if (otherPlayerCtr == null || animStateManager == null || playerAnim == null)
        {
            return;
        }

        switch (stateType)
        {
            case FrameStateType.NoState:
                otherPlayerCtr.SwitchNormalAnimClips();
                otherPlayerCtr.isPlaySnowSkatingSound = false;
                StopSkatingSound(false, playerAnim.gameObject);
                animStateManager.SwitchTo(EPlayerAnimState.Idle);
                break;
            case FrameStateType.SnowCubeGetOnBoard:
                otherPlayerCtr.SwitchSnowCubeAnimClips();
                playerAnim.Play(PlayerSnowSkateAnim.skiing_in);
                animStateManager.SwitchTo(EPlayerAnimState.SnowOpenSkateboard);
                AKSoundManager.Inst.PostEvent(SnowSound_Play_Snow_Skateboard_Appear, playerAnim.gameObject);
                break;
            case FrameStateType.SnowCubeGetOffBoard:
                playerAnim.Play(PlayerSnowSkateAnim.skiing_out);
                animStateManager.SwitchTo(EPlayerAnimState.SnowLeaveSkateboard);
                AKSoundManager.Inst.PostEvent(SnowSound_Play_Snow_Skateboard_Disappear, playerAnim.gameObject);
                break;
            case FrameStateType.SnowCubeFastRunForward:
                CrossFadeAnimState(playerAnim, (int) SnowAnimState.ForWoard, PlayerSnowSkateAnim.skiing);
                if (animStateManager.mskateboardAnim != null)
                {
                    ChangeSkateboardState(animStateManager.mskateboardAnim, (int) SnowAnimState.ForWoard);
                }

                OtherPlayerPlaySkatingSound(otherPlayerCtr, playerAnim.gameObject, true);
                break;
            case FrameStateType.SnowCubeFastRunLeft: 
                CrossFadeAnimState(playerAnim, (int) SnowAnimState.Left, PlayerSnowSkateAnim.skiing_l);
                if (animStateManager.mskateboardAnim != null)
                {
                    ChangeSkateboardState(animStateManager.mskateboardAnim, (int) SnowAnimState.Left);
                }

                OtherPlayerPlaySkatingSound(otherPlayerCtr, playerAnim.gameObject, false);
                break;
            case FrameStateType.SnowCubeFastRunRight: 
                CrossFadeAnimState(playerAnim, (int) SnowAnimState.Right, PlayerSnowSkateAnim.skiing_r);
                if (animStateManager.mskateboardAnim != null)
                {
                    ChangeSkateboardState(animStateManager.mskateboardAnim, (int) SnowAnimState.Right);
                }

                OtherPlayerPlaySkatingSound(otherPlayerCtr, playerAnim.gameObject, false);
                break;
            default: break;
        }
    }

    private void OtherPlayerPlaySkatingSound(OtherPlayerCtr otherPlayerCtr, GameObject go, bool isForward)
    {
        if (otherPlayerCtr != null && !otherPlayerCtr.isJump && otherPlayerCtr.isPlaySnowSkatingSound == false)
        {
            otherPlayerCtr.isPlaySnowSkatingSound = true;
            StopSkatingSound(false, go);
            PlaySkatingSound(false, go, isForward);
        }
    }
    
    public void CrossFadeAnimState(Animator playerAnim, int para, string stateStr)
    {
        if (playerAnim == null)
        {
            return;
        }

        playerAnim.SetInteger(PlayerSnowSkateAnim.SnowAnimPara_RunOffset, para);
        playerAnim.Update(0f);
        playerAnim.CrossFadeInFixedTime(stateStr, 0.15f);
    }

    #endregion
}

using System;
using UnityEngine;

/// <summary>
/// 控制帧数据中的状态字段
/// </summary>
public class OtherPlayerFrameStateCtr : MonoBehaviour
{
    public FrameStateType CurFramStateType;

    private OtherPlayerCtr otherPlayerCtr;
    private OtherPlayerAnimStateManager playerStateManager;
    private Animator playerAnim;
    private const string IdleStateName = "idle";

    public void Init(OtherPlayerCtr otherPlayerCtr, OtherPlayerAnimStateManager playerStateManager, Animator playerAnim)
    {
        this.otherPlayerCtr = otherPlayerCtr;
        this.playerStateManager = playerStateManager;
        this.playerAnim = playerAnim;
    }

    public void OnOtherPlayerFixUpdate(int stateTypeInt)
    {
       
        if (stateTypeInt == (int) CurFramStateType)
        {
            return;
        }

        CurFramStateType = (FrameStateType) stateTypeInt;
        //LoggerUtils.Log($"OnOtherPlayerFixUpdate: FrameStateChanged:{CurFramStateType}");

        switch (CurFramStateType)
        {
            case FrameStateType.NoState:
                HandleNoState();
                break;

            case FrameStateType.ParachuteGlidingIdle:
            case FrameStateType.ParachuteGlidingMove:
            case FrameStateType.ParachuteGlidingPreLand:
            case FrameStateType.ParachuteFallingReady:
            case FrameStateType.ParachuteFallingIdle:
            case FrameStateType.ParachuteFallingPreLand:
            case FrameStateType.ParachuteFallingMoveForward:
            case FrameStateType.ParachuteFallingMoveLeft:
            case FrameStateType.ParachuteFallingMoveBackward:
            case FrameStateType.ParachuteFallingMoveRight:
                ParachuteManager.Inst.HandleFrameState(otherPlayerCtr, playerStateManager, playerAnim, CurFramStateType);
                break;
            
            case FrameStateType.SnowCubeGetOnBoard:
            case FrameStateType.SnowCubeGetOffBoard:
            case FrameStateType.SnowCubeFastRunForward:
            case FrameStateType.SnowCubeFastRunLeft:
            case FrameStateType.SnowCubeFastRunRight:
                SnowCubeManager.Inst.HandleFrameState(otherPlayerCtr, playerStateManager, playerAnim, CurFramStateType);
                break;
            case FrameStateType.LadderDown:
            case FrameStateType.LadderDownIn:
            case FrameStateType.LadderDownOut:
            case FrameStateType.LadderUp:
            case FrameStateType.LadderUpIn:
            case FrameStateType.LadderUpOut:
            case FrameStateType.LadderIdel:
                LadderManager.Inst.HandleFrameState(otherPlayerCtr, playerStateManager, playerAnim, CurFramStateType);
                break;
            //TODO:其他模块state处理
        }
    }

    private void HandleDefault()
    {
        if (playerStateManager != null)
        {
            playerStateManager.SwitchTo(EPlayerAnimState.Idle);
        }
    }

    private void HandleNoState()
    {
        HandleDefault();
        ParachuteManager.Inst.HandleFrameState(otherPlayerCtr, playerStateManager, playerAnim, CurFramStateType);
        SnowCubeManager.Inst.HandleFrameState(otherPlayerCtr, playerStateManager, playerAnim, CurFramStateType);
        LadderManager.Inst.HandleFrameState(otherPlayerCtr, playerStateManager, playerAnim, CurFramStateType);
        //TODO:其他NoState处理
    }

    public bool IsInParachuteUsing()
    {
        if ((int) CurFramStateType >= (int) FrameStateType.ParachuteGlidingIdle &&
            (int) CurFramStateType <= (int) FrameStateType.ParachuteFallingPreLand)
        {
            return true;
        }

        return false;
    }

    public bool IsInSnowSkating()
    {
        if ((int) CurFramStateType >= (int) FrameStateType.SnowCubeGetOnBoard &&
            (int) CurFramStateType <= (int) FrameStateType.SnowCubeFastRunRight)
        {
            return true;
        }
        return false;
    }
}

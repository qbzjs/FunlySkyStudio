using UnityEngine;

/// <summary>
/// 换动画片段时根据此字段，放到帧数据中同步状态使用
/// </summary>
public enum FrameAnimType
{
    Normal = 0, //普通地形
    IceCube = 1, //冰方块
    Water = 2,
    SelfieMode, //自拍
    SnowCube = 4, //雪方块
    SeeSaw = 5, //跷跷板
    Swing = 6, //跷跷板
}

/// <summary>
/// 状态类型，用于放到帧数据中用于同步其他玩家状态
/// </summary>
public enum FrameStateType
{
    NoState = 0,

    ParachuteGlidingIdle = 1, //降落伞-滑翔静止
    ParachuteGlidingMove = 2, //降落伞-滑翔移动
    ParachuteGlidingPreLand = 3, //降落伞-滑翔落地翻滚
    ParachuteFallingReady = 4, //降落伞-降落静止状态
    ParachuteFallingIdle = 5, //降落伞-降落静止状态
    ParachuteFallingMoveForward = 6, //降落伞-降落时移动-摇杆向左上
    ParachuteFallingMoveBackward = 7, //降落伞-降落时移动-摇杆向右上
    ParachuteFallingMoveLeft = 8, //降落伞-降落时移动-摇杆向左下
    ParachuteFallingMoveRight = 9, //降落伞-降落时移动-摇杆向右下
    ParachuteFallingPreLand = 10, //降落伞-降落落地翻滚
    
    SnowCubeGetOnBoard = 11, //雪方块上板子
    SnowCubeGetOffBoard = 12, //雪方块下板子
    SnowCubeFastRunForward = 13, //雪方块往前滑雪
    SnowCubeFastRunLeft = 14, //雪方块左倾滑雪
    SnowCubeFastRunRight = 15, //雪方块右倾滑雪

    //爬梯动作
    LadderUpIn = 16, 
    LadderUpOut = 17,
    LadderUp = 18,
    LadderDownIn = 19,
    LadderDownOut = 20,
    LadderDown = 21,
    LadderIdel = 22,
}

/// <summary>
/// 拿到玩家当前状态字段，塞到帧数据中进行联机表现
/// 解析和处理状态字段在OtherPlayerFrameStateCtr.cs中
/// </summary>
public class FrameStateManager : CInstance<FrameStateManager>
{
    public OtherPlayerFrameStateCtr AddOtherPlayerFrameStateCtr(GameObject go)
    {
        if (go.GetComponent<OtherPlayerFrameStateCtr>() == null)
        {
            return go.AddComponent<OtherPlayerFrameStateCtr>();
        }
        else
        {
            return go.GetComponent<OtherPlayerFrameStateCtr>();
        }
    }

    public StandOnType GetStandOnType(FrameAnimType animType)
    {
        switch (animType)
        {
            case FrameAnimType.Normal: return StandOnType.Nothing;
            case FrameAnimType.IceCube: return StandOnType.IceCube;
            case FrameAnimType.Water: return StandOnType.Water;
            case FrameAnimType.SnowCube: return StandOnType.SnowCube;
        }
        
        return StandOnType.Nothing;
    }
    
    //////////////////////////////////// 帧数据取值 ///////////////////////////////////////


    public int GetCurFrameAnimType()
    {
        if (StateManager.IsInSelfieMode)
        {
            return (int)FrameAnimType.SelfieMode;
        }
        if (PlayerStandonControl.Inst)
        {
            switch (PlayerStandonControl.Inst.GetStandOnType())
            {
                case StandOnType.Nothing: return (int)FrameAnimType.Normal;
                case StandOnType.IceCube: return (int)FrameAnimType.IceCube;
                case StandOnType.Water: return (int)FrameAnimType.Water;
                case StandOnType.SnowCube: return (int)FrameAnimType.SnowCube;
            }
        }

        return (int)FrameAnimType.Normal;
    }

    public int GetCurFrameStateType()
    {
        //降落伞状态切换 //todo:fsc待测试降落伞是否受影响
        if (PlayerParachuteControl.Inst && PlayerParachuteControl.Inst.IsParachuteUsing())
        {
            return (int) PlayerParachuteControl.Inst.GetCurFrameState();
        }
        //雪方块上状态切换
        if (PlayerSnowSkateControl.Inst && PlayerSnowSkateControl.Inst.IsSnowSkating())
        {
            return (int) PlayerSnowSkateControl.Inst.GetCurFrameState();
        }
        //梯子上状态切换
        if (PlayerLadderControl.Inst && PlayerLadderControl.Inst.isOnLadder)
        {
            return (int)PlayerLadderControl.Inst.GetCurFrameState();
        }
        return (int) FrameStateType.NoState;
    }
}

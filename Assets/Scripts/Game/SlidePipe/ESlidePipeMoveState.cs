
//玩家沿着滑梯移动的组件
public enum ESlidePipeMoveState
{
    None,
    StartIdle,//躺着等待滑行
    EndIdle,//滑行结束的Idle
    Start,//上滑梯
    Slide,//滑行中
    End,//下滑梯
}

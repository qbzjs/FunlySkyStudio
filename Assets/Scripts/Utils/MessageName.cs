using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageName : MonoBehaviour
{
    public const string ChangeMode = "ChangeMode";

    public const string ChangeView = "ChangeView";

    public const string EnterEdit = "EnterEdit";
    public const string ReleaseTrigger = "ReleaseTrigger";

    /// <summary>
    /// 打字状态
    /// </summary>
    public const string TypeData = "TypeData";

    public const string SaveCoverStateChange = "SaveCoverStateChange"; //保存封面
    /// <summary>
    /// 移动位置交互
    /// </summary>
    public const string PosMove = "PosMove";

    /// <summary>
    /// 玩家退出
    /// </summary>
    public const string PlayerLeave = "PlayerLeave";

    /// <summary>
    /// 玩家进入房间（被创建后）
    /// </summary>
    public const string PlayerCreate = "PlayerCreate";
    
    /// <summary>
    /// Debug 面板显示或隐藏
    /// </summary>
    public const string DebugStateChange = "DebugStateChange";

    /// <summary>
    /// 当可拾取道具
    /// </summary>
    public const string OnPickablePropActive = "OnPickablePropActive";

    /// <summary>
    /// 打开组合UI面板
    /// </summary>
    public const string OpenPackPanel = "OpenPackPanel";

    public const string UpdateUndoView = "UpdateUndoView";

    public const string StartGameOnLine = "StartGameOnLine";

    public const string OnEmoPlay = "OnEmoPlay";
    public const string OnEmoEnd = "OnEmoEnd";

    /// <summary>
    /// 钓鱼相关
    /// </summary>
    public const string OnFishingStart = "OnFishingStart";
    public const string OnFishingStop = "OnFishingStop";

    /// <summary>
    /// 玩家视角切换
    /// </summary>
    public const string ChangeTps = "ChangeTps";

    /// <summary>
    /// 对局结果反馈
    /// </summary>
    public const string OnPVPResult = "OnPVPResult";
    
    /// <summary>
    /// 切回前台消息
    /// </summary>
    public const string OnForeground = "OnForeground";

    public const string OnEnterSnowfield = "OnEnterSnowfield";
    public const string OnLeaveSnowfield = "OnExitSnowfield";
}

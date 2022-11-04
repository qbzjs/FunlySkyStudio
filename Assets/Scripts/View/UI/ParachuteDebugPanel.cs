// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using DG.Tweening;
// using UnityEngine.EventSystems;
//
// /// <summary>
// /// 降落伞测试panel
// /// 给产品体验调整参数和手感使用
// /// </summary>
// public class ParachuteDebugPanel : BasePanel<ParachuteDebugPanel>
// {
//     public Toggle IsOpenFallingToggle;
//     public Text CurrentFallingSpeedTxt;
//     public Text CurrentFallingAddSpeedTxt;
//     public Text CurrentFallingTimeTxt;
//     public Text CurrentHoriMoveSpeedTxt;
//     public Text CurrentGlidingTime;
//
//     #region 滑翔
//
//     public Slider EnterGlidingSpeed;
//     public Text EnterGlidingSpeedTxt;
//     public InputField EnterGlidingSpeedInput;
//
//     public Slider GlidingIdleFactor;
//     public Text GlidingIdleFactorTxt;
//     public InputField GlidingIdleFactorInput;
//
//     public Slider GlidingMoveFactor;
//     public Text GlidingMoveFactorTxt;
//     public InputField GlidingMoveFactorInput;
//
//     public Slider GlidingMoveMaxSpeed; //滑翔水平移动速度
//     public Text GlidingMoveMaxSpeedTxt;
//     public InputField GlidingMoveMaxSpeedInput;
//
//     public Slider GlidingEnterPrelandHight; //进入滑翔降落动作的高度
//     public Text GlidingEnterPrelandHightTxt;
//     public InputField GlidingEnterPrelandHightInput;
//
//     #endregion
//
//     #region 降落
//
//     public Text FallingVerticalIdleSpeedTxt;
//     public InputField FallingVerticalIdleSpeedInput;
//
//     public Text FallingVerticalForwardSpeedTxt;
//     public InputField FallingVerticalForwardSpeedInput;
//
//     public Text FallingVerticalBackwardSpeedTxt;
//     public InputField FallingVerticalBackwardSpeedInput;
//
//     public Text FallingHorizontalForwardSpeedTxt;
//     public InputField FallingHorizontalForwardSpeedInput;
//
//     public Text FallingHorizontalBackwardSpeedTxt;
//     public InputField FallingHorizontalBackwardSpeedInput;
//
//     public Text FallingXRotateMaxAngleTxt;
//     public InputField FallingXRotateMaxAngleInput;
//     public Text FallingXRotateMinAngleTxt;
//     public InputField FallingXRotateMinAngleInput;
//     public Text FallingZRotateMaxAngleTxt;
//     public InputField FallingZRotateMaxAngleInput;
//     public Text FallingZRotateMinAngleTxt;
//     public InputField FallingZRotateMinAngleInput;
//     
//     public Text FallingYRotateMaxAngleTxt;
//     public InputField FallingYRotateMaxAngleInput;
//     public Text FallingYRotateMinAngleTxt;
//     public InputField FallingYRotateMinAngleInput;
//
//     public Text FallingPreLandHeightTxt;
//     public InputField FallingPreLandHeightInput;
//     #endregion
//
//
//     public override void OnInitByCreate()
//     {
//         base.OnInitByCreate();
//
//         IsOpenFallingToggle.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.OpenParachute();
//             // PlayerParachuteControl.Inst.CurrentState = ParachuteMoveState.Falling;
//         });
//         IsOpenFallingToggle.isOn = false;
//
//         // MaxFallingSpeedSlider.minValue = PlayerParachuteControl.Inst.MinFallingSpeed;
//         // MaxFallingSpeedSlider.maxValue = PlayerParachuteControl.Inst.MaxFallingSpeed;
//         // MaxFallingSpeedSlider.onValueChanged.AddListener((value) =>
//         // {
//         //     PlayerParachuteControl.Inst.CurFallingSpeed = -value;
//         //     MaxFallingSpeedTxt.text = $"下落速度:{value}";
//         // });
//         // MaxFallingSpeedSlider.value = PlayerParachuteControl.Inst.CurFallingSpeed;
//         // MaxFallingSpeedTxt.text = $"下落速度:{PlayerParachuteControl.Inst.CurFallingSpeed}";
//
//         CurrentFallingSpeedTxt.text = $"当前下落速度:{PlayerParachuteControl.Inst.CurFallingSpeed}";
//         CurrentFallingTimeTxt.text = $"当前已下落时间：{0} s";
//
//         #region 滑翔
//
//         // EnterGlidingSpeed.minValue = 10f;
//         // EnterGlidingSpeed.maxValue = 50f;
//         // EnterGlidingSpeed.onValueChanged.AddListener((value) =>
//         // {
//         //     PlayerParachuteControl.Inst.EnterGlidingYSpeed = -value;
//         //     EnterGlidingSpeedTxt.text = $"进入滑翔的速度阈值:{value}";
//         // });
//         // EnterGlidingSpeed.value = 20f;
//
//         EnterGlidingSpeedTxt.text = $"进入滑翔的速度阈值:{PlayerParachuteControl.Inst.EnterGlidingYSpeed}";
//         EnterGlidingSpeedInput.text = PlayerParachuteControl.Inst.EnterGlidingYSpeed.ToString();
//         EnterGlidingSpeedInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.EnterGlidingYSpeed = -float.Parse(v);
//             EnterGlidingSpeedTxt.text = $"进入滑翔的速度阈值:{PlayerParachuteControl.Inst.EnterGlidingYSpeed}";
//         });
//
//
//         // GlidingIdleFactor.minValue = 0.1f;
//         // GlidingIdleFactor.maxValue = 1.0f;
//         // GlidingIdleFactor.onValueChanged.AddListener((value) =>
//         // {
//         //     PlayerParachuteControl.Inst.GlidingIdle_Y_AddSpeed_Factor = value;
//         //     GlidingIdleFactorTxt.text = $"滑翔静止的加速度系数:{value}";
//         // });
//         // GlidingIdleFactor.value = 0.4f;
//
//         GlidingIdleFactorTxt.text = $"滑翔静止的加速度系数:{PlayerParachuteControl.Inst.GlidingIdle_Y_AddSpeed_Factor}";
//         GlidingIdleFactorInput.text = PlayerParachuteControl.Inst.GlidingIdle_Y_AddSpeed_Factor.ToString();
//         GlidingIdleFactorInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.GlidingIdle_Y_AddSpeed_Factor = float.Parse(v);
//             GlidingIdleFactorTxt.text = $"滑翔静止的加速度系数:{PlayerParachuteControl.Inst.GlidingIdle_Y_AddSpeed_Factor}";
//         });
//
//         // GlidingMoveFactor.minValue = 0.1f;
//         // GlidingMoveFactor.maxValue = 1.0f;
//         // GlidingMoveFactor.onValueChanged.AddListener((value) =>
//         // {
//         //     PlayerParachuteControl.Inst.GlidingMove_Y_AddSpeed_Factor = value;
//         //     GlidingMoveFactorTxt.text = $"滑翔时移动的加速度系数:{value}";
//         // });
//         // GlidingMoveFactor.value = 0.6f;
//
//         GlidingMoveFactorTxt.text = $"滑翔时移动的加速度系数:{PlayerParachuteControl.Inst.GlidingMove_Y_AddSpeed_Factor}";
//         GlidingMoveFactorInput.text = PlayerParachuteControl.Inst.GlidingMove_Y_AddSpeed_Factor.ToString();
//         GlidingMoveFactorInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.GlidingMove_Y_AddSpeed_Factor = float.Parse(v);
//             GlidingMoveFactorTxt.text = $"滑翔时移动的加速度系数:{PlayerParachuteControl.Inst.GlidingMove_Y_AddSpeed_Factor}";
//         });
//
//         // GlidingMoveMaxSpeed.minValue = 4;
//         // GlidingMoveMaxSpeed.maxValue = 30f;
//         // GlidingMoveMaxSpeed.onValueChanged.AddListener((value) =>
//         // {
//         //     PlayerParachuteControl.Inst.Gliding_Move_Speed = value;
//         //     GlidingMoveMaxSpeedTxt.text = $"滑翔时水平移动速度:{value}";
//         // });
//         // GlidingMoveMaxSpeed.value = 8f;
//
//         GlidingMoveMaxSpeedTxt.text = $"滑翔时水平移动速度:{PlayerParachuteControl.Inst.Gliding_Move_Speed}";
//         GlidingMoveMaxSpeedInput.text = PlayerParachuteControl.Inst.Gliding_Move_Speed.ToString();
//         GlidingMoveMaxSpeedInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Gliding_Move_Speed = float.Parse(v);
//             GlidingMoveMaxSpeedTxt.text = $"滑翔时水平移动速度:{PlayerParachuteControl.Inst.Gliding_Move_Speed}";
//         });
//
//         // GlidingEnterPrelandHight.minValue = 10f;
//         // GlidingEnterPrelandHight.maxValue = 100f;
//         // GlidingEnterPrelandHight.onValueChanged.AddListener((value) =>
//         // {
//         //     PlayerParachuteControl.Inst.Gliding_Preland_Hight = value;
//         //     GlidingEnterPrelandHightTxt.text = $"滑翔降落动作的高度:{value}";
//         // });
//         // GlidingEnterPrelandHight.value = 50f;
//
//         GlidingEnterPrelandHightTxt.text = $"滑翔降落动作的高度:{PlayerParachuteControl.Inst.Gliding_Preland_Height}";
//         GlidingEnterPrelandHightInput.text = PlayerParachuteControl.Inst.Gliding_Preland_Height.ToString();
//         GlidingEnterPrelandHightInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Gliding_Preland_Height = float.Parse(v);
//             GlidingEnterPrelandHightTxt.text = $"滑翔降落动作的高度:{PlayerParachuteControl.Inst.Gliding_Preland_Height}";
//         });
//
//         #endregion
//
//         #region 降落
//
//         FallingVerticalIdleSpeedTxt.text = $"降落静止时的下落速度:{PlayerParachuteControl.Inst.Falling_Vertical_Idle_Speed}";
//         FallingVerticalIdleSpeedInput.text = PlayerParachuteControl.Inst.Falling_Vertical_Idle_Speed.ToString();
//         FallingVerticalIdleSpeedInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_Vertical_Idle_Speed = -float.Parse(v);
//             FallingVerticalIdleSpeedTxt.text = $"降落静止时的下落速度:{PlayerParachuteControl.Inst.Falling_Vertical_Idle_Speed}";
//         });
//
//         FallingVerticalForwardSpeedTxt.text = $"降落前移的下落速度:{PlayerParachuteControl.Inst.Falling_Vertical_Forward_Speed}";
//         FallingVerticalForwardSpeedInput.text = PlayerParachuteControl.Inst.Falling_Vertical_Forward_Speed.ToString();
//         FallingVerticalForwardSpeedInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_Vertical_Forward_Speed = -float.Parse(v);
//             FallingVerticalForwardSpeedTxt.text = $"降落前移的下落速度:{PlayerParachuteControl.Inst.Falling_Vertical_Forward_Speed}";
//         });
//
//         FallingVerticalBackwardSpeedTxt.text = $"降落后移的下落速度:{PlayerParachuteControl.Inst.Falling_Vertical_Backward_Speed}";
//         FallingVerticalBackwardSpeedInput.text = PlayerParachuteControl.Inst.Falling_Vertical_Backward_Speed.ToString();
//         FallingVerticalBackwardSpeedInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_Vertical_Backward_Speed = -float.Parse(v);
//             FallingVerticalForwardSpeedTxt.text = $"降落后移的下落速度:{PlayerParachuteControl.Inst.Falling_Vertical_Backward_Speed}";
//         });
//
//         FallingHorizontalForwardSpeedTxt.text = $"降落前移的水平速度:{PlayerParachuteControl.Inst.Falling_Horizontal_Forward_Speed}";
//         FallingHorizontalForwardSpeedInput.text = PlayerParachuteControl.Inst.Falling_Horizontal_Forward_Speed.ToString();
//         FallingHorizontalForwardSpeedInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_Horizontal_Forward_Speed = float.Parse(v);
//             FallingHorizontalForwardSpeedTxt.text = $"降落前移的水平速度:{PlayerParachuteControl.Inst.Falling_Horizontal_Forward_Speed}";
//         });
//
//         FallingHorizontalBackwardSpeedTxt.text = $"降落后移的水平速度:{PlayerParachuteControl.Inst.Falling_Horizontal_Backward_Speed}";
//         FallingHorizontalBackwardSpeedInput.text = PlayerParachuteControl.Inst.Falling_Horizontal_Backward_Speed.ToString();
//         FallingHorizontalBackwardSpeedInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_Horizontal_Backward_Speed = float.Parse(v);
//             FallingHorizontalBackwardSpeedTxt.text = $"降落后移的水平速度:{PlayerParachuteControl.Inst.Falling_Horizontal_Backward_Speed}";
//         });
//
//         #endregion
//
//         #region 降落旋转角度
//
//         FallingXRotateMaxAngleTxt.text = $"降落前倾最大角度:{PlayerParachuteControl.Inst.Falling_X_MaxAngle}";
//         FallingXRotateMaxAngleInput.text = PlayerParachuteControl.Inst.Falling_X_MaxAngle.ToString();
//         FallingXRotateMaxAngleInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_X_MaxAngle = float.Parse(v);
//             FallingXRotateMaxAngleTxt.text = $"降落前倾最大角度:{PlayerParachuteControl.Inst.Falling_X_MaxAngle}";
//         });
//
//         FallingXRotateMinAngleTxt.text = $"降落后倾最大角度:{PlayerParachuteControl.Inst.Falling_X_MinAngle}";
//         FallingXRotateMinAngleInput.text = PlayerParachuteControl.Inst.Falling_X_MinAngle.ToString();
//         FallingXRotateMinAngleInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_X_MinAngle = -float.Parse(v);
//             FallingXRotateMinAngleTxt.text = $"降落后倾最大角度:{PlayerParachuteControl.Inst.Falling_X_MinAngle}";
//         });
//
//         FallingZRotateMaxAngleTxt.text = $"降落左倾最大角度:{PlayerParachuteControl.Inst.Falling_Z_MaxAngle}";
//         FallingZRotateMaxAngleInput.text = PlayerParachuteControl.Inst.Falling_Z_MaxAngle.ToString();
//         FallingZRotateMaxAngleInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_Z_MaxAngle = float.Parse(v);
//             FallingZRotateMaxAngleTxt.text = $"降落左倾最大角度:{PlayerParachuteControl.Inst.Falling_Z_MaxAngle}";
//         });
//
//         FallingZRotateMinAngleTxt.text = $"降落右倾最大角度:{PlayerParachuteControl.Inst.Falling_Z_MinAngle}";
//         FallingZRotateMinAngleInput.text = PlayerParachuteControl.Inst.Falling_Z_MinAngle.ToString();
//         FallingZRotateMinAngleInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_Z_MinAngle = -float.Parse(v);
//             FallingZRotateMinAngleTxt.text = $"降落右倾最大角度:{PlayerParachuteControl.Inst.Falling_Z_MinAngle}";
//         });
//
//         
//         FallingYRotateMaxAngleTxt.text = $"降落Y轴旋转最大角度:{PlayerParachuteControl.Inst.Falling_Y_MaxAngle}";
//         FallingYRotateMaxAngleInput.text = PlayerParachuteControl.Inst.Falling_Y_MaxAngle.ToString();
//         FallingYRotateMaxAngleInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_Y_MaxAngle = float.Parse(v);
//             FallingYRotateMaxAngleTxt.text = $"降落Y轴旋转最大角度:{PlayerParachuteControl.Inst.Falling_Y_MaxAngle}";
//         });
//
//         FallingYRotateMinAngleTxt.text = $"降落Y轴旋转最小角度:{PlayerParachuteControl.Inst.Falling_Y_MinAngle}";
//         FallingYRotateMinAngleInput.text = PlayerParachuteControl.Inst.Falling_Y_MinAngle.ToString();
//         FallingYRotateMinAngleInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_Y_MinAngle = -float.Parse(v);
//             FallingYRotateMinAngleTxt.text = $"降落Y轴旋转最小角度:{PlayerParachuteControl.Inst.Falling_Y_MinAngle}";
//         });
//
//         
//         FallingPreLandHeightTxt.text = $"降落落地翻滚高度:{PlayerParachuteControl.Inst.Falling_Preland_Height}";
//         FallingPreLandHeightInput.text = PlayerParachuteControl.Inst.Falling_Preland_Height.ToString();
//         FallingPreLandHeightInput.onValueChanged.AddListener((v) =>
//         {
//             PlayerParachuteControl.Inst.Falling_Preland_Height = float.Parse(v);
//             FallingPreLandHeightTxt.text = $"降落落地翻滚高度:{PlayerParachuteControl.Inst.Falling_Preland_Height}";
//         });
//         
//         #endregion
//     }
//
//     public override void OnDialogBecameVisible()
//     {
//         base.OnDialogBecameVisible();
//     }
// }
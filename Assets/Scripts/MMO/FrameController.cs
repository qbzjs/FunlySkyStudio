using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using BudEngine.NetEngine;
using UnityEngine;

public partial class ClientManager : MonoBehaviour
{
    private const float DataMultiple = 10000f;
    private const int FramDataCount = 16;

    private static bool isCanSendFrame = false;
    private static float lastFrameTime;
    private static int frameStep = 60;


    
    private void FixedUpdate()
    {

#if !UNITY_EDITOR
        if (selfPlayerCom == null || GlobalFieldController.CurGameMode != GameMode.Guest)
        {
            return;
        }
#endif

        if (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel != null)
        {
            var trs = PlayerDriveControl.Inst.steeringWheel.carRgb.transform;
            PlayerPos = trs.position;
            PlayerRot = trs.rotation;
        }
        else if(PlayerLadderControl.Inst&&PlayerLadderControl.Inst.isOnLadder)
        {
            PlayerPos = LadderManager.Inst.GetPlayerCarryNodePos();
        }
        else
        {
            PlayerPos = selfPlayerCom.gameObject.transform.position;
            // if (!selfPlayerCom.isTps)
            // {
            //     PlayerRot = selfPlayerCom.gameObject.transform.rotation;
            // }
            // else
            // {
            //     PlayerRot = selfPlayerCom.playerAnim.gameObject.transform.rotation;
            // }
            // Edit By shaocheng - 2022-8-29 18:48:06
            // 原第一人称传的是外层Player节点rot,经确认当时做第一人称从国内移植就如此，雪方块无法使用遂修改
            PlayerRot = selfPlayerCom.playerAnim.gameObject.transform.rotation;
        }

        IsMoving = selfPlayerCom.isMoving;
        IsGround = selfPlayerCom.isGround;
        IsFlying = selfPlayerCom.isFlying;
        IsFastRun = selfPlayerCom.isFastRun;

        IsInWater = false;
        IsSwimming = false;
        if (PlayerSwimControl.Inst)
        {
            IsInWater = PlayerSwimControl.Inst.isInWater;
            IsSwimming = PlayerSwimControl.Inst.isSwimming;
        }

        AnimType = FrameStateManager.Inst.GetCurFrameAnimType();
        StateType = FrameStateManager.Inst.GetCurFrameStateType();

        if (!string.IsNullOrEmpty(GameManager.Inst.curDiyMapId))
        {
            selfDiyMapId = GameManager.Inst.curDiyMapId;
        }
        //LoggerUtils.Log("GameManager.Inst.curDiyMapId = " + GameManager.Inst.curDiyMapId);

        lastFrameTime += (Time.fixedDeltaTime * 1000);
       
        if (isCanSendFrame && isEnterRoom && lastFrameTime >= frameStep)
        {
            // LoggerUtils.Log("lastFrameTime==Start==>" + lastFrameTime);
            SendStep();
            lastFrameTime %= frameStep;
            
            // LoggerUtils.Log("lastFrameTime==End==>" + lastFrameTime);
            // LoggerUtils.Log("***********SendStep");
        }
        
         
    }

    //解析帧数据
    public UgcFrameData handleFrameData(string framData)
    {
        UgcFrameData ugcFrameData = new UgcFrameData();
        string[] vals = framData.Split('|');
        if (vals.Length == FramDataCount)
        {
            float px, py, pz;
            float rx, ry, rz, rw;
            bool IsMoving, IsGround, IsFlying, IsFastRun,IsInWater, IsSwimming;
            int animType, stateType;
            if (float.TryParse(vals[0], out px) &&
                float.TryParse(vals[1], out py) &&
                float.TryParse(vals[2], out pz) &&
                float.TryParse(vals[3], out rx) &&
                float.TryParse(vals[4], out ry) &&
                float.TryParse(vals[5], out rz) &&
                float.TryParse(vals[6], out rw) &&
                bool.TryParse(vals[7], out IsMoving) &&
                bool.TryParse(vals[8], out IsGround) &&
                bool.TryParse(vals[10], out IsFlying) &&
                bool.TryParse(vals[11], out IsFastRun) &&
                bool.TryParse(vals[12], out IsInWater) &&
                bool.TryParse(vals[13], out IsSwimming) &&
                int.TryParse(vals[14], out animType) &&
                int.TryParse(vals[15], out stateType)
                )
            {
                ugcFrameData.playerPos.x = px / DataMultiple;
                ugcFrameData.playerPos.y = py / DataMultiple - 0.95f;
                ugcFrameData.playerPos.z = pz / DataMultiple;

                ugcFrameData.playerRot.x = rx / DataMultiple;
                ugcFrameData.playerRot.y = ry / DataMultiple;
                ugcFrameData.playerRot.z = rz / DataMultiple;
                ugcFrameData.playerRot.w = rw / DataMultiple;

                ugcFrameData.IsMoving = IsMoving;
                ugcFrameData.IsGround = IsGround;
                ugcFrameData.IsFlying = IsFlying;
                ugcFrameData.IsFastRun = IsFastRun;
                ugcFrameData.IsInWater = IsInWater;
                ugcFrameData.IsSwimming = IsSwimming;
                ugcFrameData.AnimType = animType;
                ugcFrameData.StateType = stateType;

                ugcFrameData.mapId = vals[9];

                ugcFrameData.playerRot.Normalize();
                return ugcFrameData;
            }
        }
        return null;
    }

    public void SendStep()
    {
        var para = new SendFramePara
        {
            Data =
            $"{PlayerPos.x * DataMultiple:0}|" +
            $"{PlayerPos.y * DataMultiple:0}|" +
            $"{PlayerPos.z * DataMultiple:0}|" +
            $"{PlayerRot.x * DataMultiple:0}|" +
            $"{PlayerRot.y * DataMultiple:0}|" +
            $"{PlayerRot.z * DataMultiple:0}|" +
            $"{PlayerRot.w * DataMultiple:0}|" +
            $"{IsMoving}|" +
            $"{IsGround}|" +
            $"{selfDiyMapId}|" +
            $"{IsFlying}|" +
            $"{IsFastRun}|" +
            $"{IsInWater}|" +
            $"{IsSwimming}|" + 
            $"{AnimType}|" + 
            $"{StateType}"
        };

        frameSendedCount++;

        Global.Room.SendFrame(para, eve =>
        {
            if (eve.Code == 0)
            {
                //LoggerUtils.Log("发送帧同步成功\r\n");
            }
            else {
                LoggerUtils.Log("发送帧同步失败\r\n");
            }
        });
    }

    public void DealProtal(RecvFrameBst bst)
    {
        var fr = bst.Frame;
        for (int i = 0; i < fr.Items.Count; i++)
        {
            if (Player.Id != fr.Items[i].PlayerId)
            {
                UgcFrameData ugcFrameData = handleFrameData((string)fr.Items[i].Data);
                if(ugcFrameData == null)
                {
                    continue;
                }
                string CurDiyMapId = ugcFrameData.mapId;
                if (selfDiyMapId != CurDiyMapId)
                {
                    //HidePlayer
                    if (otherPlayerDataDic.ContainsKey(fr.Items[i].PlayerId))
                    {
                        otherPlayerDataDic[fr.Items[i].PlayerId].gameObject.SetActive(false);
                        RealTimeTalkManager.Inst.SetOtherPlayerIsCurMap(fr.Items[i].PlayerId, false);
                    }
                }
                else
                {
                    if (otherPlayerDataDic.ContainsKey(fr.Items[i].PlayerId))
                    {
                        otherPlayerDataDic[fr.Items[i].PlayerId].gameObject.SetActive(true);
                        RealTimeTalkManager.Inst.SetOtherPlayerIsCurMap(fr.Items[i].PlayerId, true);
                    }
                }
            }
        }
    }


    #region 客户端主动控制发包

    private void StartClientSendFrame()
    {
        LoggerUtils.Log("***********StartClientSendFrame");
        isCanSendFrame = true;
        lastFrameTime = 0;
    }

    private void StopClientSendFrame()
    {
        LoggerUtils.Log("***********StopClientSendFrame");
        isCanSendFrame = false;
        lastFrameTime = 0; 
    }

    #endregion
}

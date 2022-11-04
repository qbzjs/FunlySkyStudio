using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static PVPGameOverPanel;

/// <summary>
/// Author: pzkunn
/// Description: 衣橱换装广播收发管理器
/// Date: 2022-06-24 17:37:35
/// </summary>
public class ClosetClientManager : CInstance<ClosetClientManager>, IPVPManager
{
    private bool isNotifyRefreshPlayer; // 判断是否要退房时通知端上刷新人物数据，只在玩家进行过换装后赋值
    private RoleController roleCon;

    public ClosetClientManager()
    {
        MessageHelper.AddListener<GameOverStateEnum>(MessageName.OnPVPResult, ResetClosetOnPVPResult);
    }

    //发送服务端换装消息
    private void SendChangeOutfitMsg(RoleData roleData)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            RoleClosetData closetData = new RoleClosetData()
            {
                imageJson = JsonConvert.SerializeObject(roleData),
                clothesId = roleData.cloId.ToString()
            };
            string dataStr = JsonConvert.SerializeObject(closetData);

            var roomChatData = new RoomChatData()
            {
                msgType = (int)RecChatType.ChangeCloth,
                data = JsonConvert.SerializeObject(new RoomChatCustomData()
                {
                    type = (int)ChatCustomType.ChangeImage,
                    data = dataStr
                })
            };

            LoggerUtils.Log($"ClosetClientManager -- SendChangeOutfitMsg : {JsonConvert.SerializeObject(roomChatData)}");
            ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(roomChatData));
        }
    }

    //播放换装动画(需要取消前置动画)
    private void PlayChangeAnim()
    {
        //取消照镜子循环动作
        ResetMove();
        //第一人称换装不播动画
        if (!PlayerBaseControl.Inst.isTps || !PlayerBaseControl.Inst.gameObject.activeSelf)
        {
            return;
        }
        PlayerBaseControl.Inst.animCon.PlayAnim((int)EmoName.EMO_CHANGE_CLOTH);
    }

    //替换人物形象
    private void ChangePlayerOutfit(RoleData roleData)
    {
        //获取人物形象控制器
        if (roleCon == null)
        {
            roleCon = PlayerBaseControl.Inst.GetComponentInChildren<RoleController>(true);
        }
        roleCon.ChangeRoleImage(roleData);
        PlayerBaseControl.Inst.animCon.PlayEyeAnim(roleData.eId);
    }

    private void UpdateUserinfoLocal(string newImageJson, string newClothId)
    {
        GameManager.Inst.ugcUserInfo.imageJson = newImageJson;
        GameManager.Inst.ugcUserInfo.clothesId = newClothId;
    }

    //通知端上刷新人物数据
    public void NotifyRefreshPlayerData()
    {
        if (isNotifyRefreshPlayer)
        {
            LoggerUtils.Log("ClosetClientManager --> NotifyRefreshPlayerData");
            MobileInterface.Instance.NotifyRefreshPlayerInfo();
            isNotifyRefreshPlayer = false;
        }
    }

    private bool HandleSpecialCaseOnChange()
    {
        //双人牵手状态下(异常情况)
        if (PlayerMutualControl.Inst && PlayerMutualControl.Inst.isInEumual)
        {
            TipPanel.ShowToast("You could not change your outfit while using interactive emotes.");
            return false;
        }
        //换装动画播放 / 双人动作交互 过程中
        if (PlayerBaseControl.Inst.animCon.IsChanging || PlayerBaseControl.Inst.animCon.isInteracting)
        {
            return false;
        }
        return true;
    }

    public void UploadOutfitData(RoleData roleData, UnityAction<string> onSuccess, UnityAction<string> onFail)
    {
        //TODO：开启loading状态动画
        if (!HandleSpecialCaseOnChange())
        {
            //TODO：关闭loading状态动画、关闭换装UI界面
            ChooseClothPanel.Instance.HidePanel();
            onFail?.Invoke(null);
            return;
        }

        roleData.sceneType = 1;
        string newImageJson = JsonConvert.SerializeObject(roleData);
        RoleUpLoadBody roleUpLoadBody = new RoleUpLoadBody();
        UserInfo paraUserinfo = new UserInfo()
        {
            imageJson = newImageJson,
            clothesId = RoleLoadManager.GetUgcMapIds(roleData), //需要审核的ugcMapId
            dcUgcInfos = RoleLoadManager.GetDCUGCItemList(roleData),
            dcPgcInfos = RoleLoadManager.GetDCPGCItemList(roleData),
        };
        roleUpLoadBody.userInfo = paraUserinfo;
        LoggerUtils.Log("Closet --> ChangeOutfit: " + JsonConvert.SerializeObject(roleUpLoadBody));
        HttpUtils.MakeHttpRequest("/image/setImage", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(roleUpLoadBody), (content) =>
        {
            LoggerUtils.Log($"change outfit success!:{content}");
            SendChangeOutfitMsg(roleData);
            ChangePlayerOutfit(roleData);
            PlayChangeAnim();
            UpdateUserinfoLocal(paraUserinfo.imageJson, paraUserinfo.clothesId);
            isNotifyRefreshPlayer = true;
            //TODO：关闭loading状态动画、关闭换装UI界面
            ChooseClothPanel.Instance.HidePanel();
            onSuccess?.Invoke(content);
        }
        , (err) =>
        {
            LoggerUtils.LogError("Script:ClosetClientManager UploadOutfitData error = " + err);
            LoggerUtils.Log($"change outfit failed!:{err}");
            HttpResponseRaw roleResponseData = JsonConvert.DeserializeObject<HttpResponseRaw>(err);
            switch (roleResponseData.result)
            {
                case (int)HttpOptErrorCode.DC_NOT_OWNED:
                    TipPanel.ShowToast("Oops! Your outfit contains digital collectibles you do not own.");
                    ChooseClothPanel.Instance.SetLoadingAnim(false);
                    break;
                default:
                    TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
                    ChooseClothPanel.Instance.HidePanel();
                    onFail?.Invoke(err);
                    break;
            }
        });
    }

    //广播处理
    public void OnHandleChangeImageBst(string senderPlayerId, string msg)
    {
        LoggerUtils.Log($"UgcClothItem OnHandleChangeImageBst: sendPlayerId:{senderPlayerId}, msg:{msg}");
        if (senderPlayerId == GameManager.Inst.ugcUserInfo.uid || string.IsNullOrEmpty(msg))
        {
            return;
        }

        RoomChatCustomData customData = JsonConvert.DeserializeObject<RoomChatCustomData>(msg);
        var closetData = JsonConvert.DeserializeObject<RoleClosetData>(customData.data);
        if (closetData == null)
        {
            return;
        }
        var roleData = JsonConvert.DeserializeObject<RoleData>(closetData.imageJson);
        if (roleData == null)
        {
            LoggerUtils.LogError("OnHandleChangeImageBst : roleData == null");
            return;
        }

        var otherComp = PlayerInfoManager.GetOtherPlayerCtrByPlayerId(senderPlayerId);
        if (otherComp != null)
        {
            //替换角色形象
            RoleController roleCom = otherComp.GetComponentInChildren<RoleController>(true);
            roleCom.ChangeRoleImage(roleData);
            //播放换装动画
            var otherAnimCon = otherComp.GetComponent<AnimationController>();
            otherAnimCon.PlayEyeAnim(roleData.eId);
            otherAnimCon.PlayAnim((int)EmoName.EMO_CHANGE_CLOTH);
        }
    }

    private void ResetClosetOnPVPResult(GameOverStateEnum stateEnum)
    {
        OnReset();
    }

    public void OnReset()
    {
        //TODO：强制关闭换装UI界面，复位UI相关数据
        if (ChooseClothPanel.Instance)
        {
            ChooseClothPanel.Instance.ForseHidePanel();
        }
        //复位照镜子循环动作状态 -- 联机服务器已有处理(pvp开始+pvp结束)
        ResetMove();
    }

    public void ResetMove()
    {
        var animCon = PlayerBaseControl.Inst.animCon;
        if (animCon.isLooping && animCon.loopingInfo != null && animCon.loopingInfo.id == (int)EmoName.EMO_LOOK_MIRROR)
        {
            animCon.StopLoop();
        }
    }

    public override void Release()
    {
        base.Release();
        MessageHelper.RemoveListener<GameOverStateEnum>(MessageName.OnPVPResult, ResetClosetOnPVPResult);
    }
}
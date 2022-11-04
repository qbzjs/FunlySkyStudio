using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class DisplayBoardData
{
    public string userId;
    public string userName;
    public string headUrl;
}

/// <summary>
/// Author:WenJia
/// Description:3D 展板关联的 NodeBehaviour，主要处理展板的展示逻辑
/// Date: 2022/1/5 17:48:26
/// </summary>

public class DisplayBoardBehaviour : NodeBaseBehaviour
{
    private Color[] colors;
    private MeshRenderer iconRender;
    private MeshRenderer nameRender;
    private TextStatus _texStatus = TextStatus.NONE;
    private MaterialPropertyBlock mpb;
    private Texture iconDefaultTexture;
    private Texture2D headTexture; // 避免被误释放，保存一下引用

    enum TextStatus
    {
        NONE,
        START,
        SUCCESS,
        FAIL
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
        iconRender = transform.Find("display_icon").GetComponent<MeshRenderer>();
        iconDefaultTexture = iconRender.material.GetTexture("_TextureSample0");
        DisplayBoardManager.Inst.UpdateDisplayBoardCurNumber(true);
    }

    public void ReqUserInfo()
    {
        var displayBoard = entity.Get<DisplayBoardComponent>();
        if (string.IsNullOrEmpty(displayBoard.userId))
        {
            return;
        }
        List<string> uidReqList = new List<string>();
        uidReqList.Add(displayBoard.userId);

        GameUtils.GetUserInfoByUid(uidReqList, (string msg) =>
        {
            LoggerUtils.Log("DisplayBoardBehaviour GetUserInfoByUid Success. userInfo is  " + msg);
            HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
            LoggerUtils.Log("DisplayBoardBehaviour GetUserInfoByUid Success. hResponse.data is  " + JsonConvert.SerializeObject(hResponse.data));
            UserInfo[] syncPlayerInfos = JsonConvert.DeserializeObject<UserInfo[]>(hResponse.data);
            var playerInfo = syncPlayerInfos[0];
            DisplayBoardData data = new DisplayBoardData();
            data.userId = playerInfo.uid;
            data.userName = playerInfo.userName;
            data.headUrl = playerInfo.portraitUrl;
            GetUserInfo(data);
        }, (string content) =>
        {
            LoggerUtils.LogError("OnBatchGetPlayerInfo Fail");
            TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        });
    }

    public void GetUserInfo(DisplayBoardData data)
    {
        _texStatus = TextStatus.START;
        CoroutineManager.Inst.StartCoroutine(ResManager.Inst.GetTexture(data.headUrl, GetTexSuccess, GetTexFail));
    }

    private void GetTexSuccess(Texture2D tex)
    {
        ShowHeadImage(tex);
    }

    private void GetTexFail()
    {
        _texStatus = TextStatus.FAIL;
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }
    public override void OnRayEnter()
    {
        base.OnRayEnter();

        var displayBoard = entity.Get<DisplayBoardComponent>();

        //展板不空，且显示玩头像时，展板可交互
        if (!string.IsNullOrEmpty(displayBoard.userId) && _texStatus == TextStatus.SUCCESS)
        {
            PortalPlayPanel.Show();
            PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.DisplayBoard);
            PortalPlayPanel.Instance.AddButtonClick(OpenPersonalProfile);
            PortalPlayPanel.Instance.SetTransform(transform);
        }
    }

    public override void OnRayExit()
    {
        base.OnRayExit();
        PortalPlayPanel.Hide();
    }

    private void OpenPersonalProfile()
    {
        //获取头像不成功时，在点击交互按钮时会尝试重新获取用户信息
        if (_texStatus != TextStatus.SUCCESS)
        {
            ReqUserInfo();
        }
        LoggerUtils.Log("OpenPersonalProfile  enter");
        var displayBoard = entity.Get<DisplayBoardComponent>();
        UserProfilePanel.Show();
        UserProfilePanel.Instance.OnOpenPanel(displayBoard.userId);
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref colors);
    }

    public void ShowHeadImage(Texture2D tex)
    {
        if (iconRender != null)
        {
            headTexture = tex;
            mpb.SetTexture("_TextureSample0", tex);
            iconRender.SetPropertyBlock(mpb);
            _texStatus = TextStatus.SUCCESS;
            return;
        }
    }

    public void ClearUserInfo(string uid = "")
    {
        LoggerUtils.Log("ClearUserInfo enter");
        mpb.Clear();
        mpb.SetTexture("_TextureSample0", iconDefaultTexture);
        iconRender.SetPropertyBlock(mpb);
        DisplayBoardManager.Inst.RemoveDisplayBev(uid, this);
        headTexture = null;
    }

    public override void OnReset()
    {
        base.OnReset();
        ClearUserInfo();
        DisplayBoardManager.Inst.UpdateDisplayBoardCurNumber(false);
    }
}

using System.Collections.Generic;
using Entitas;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class DisplayUserInfo
{
    public string uid;
}

/// <summary>
/// Author:WenJia
/// Description:3D 展示板的设置 Panel，用于选择要展示的玩家
/// Date: 2022/1/5 17:48:26
/// </summary>

public class DisplayBoardPanel : InfoPanel<DisplayBoardPanel>
{
    public RawImage UserHeadImage;
    public GameObject AddPanel;
    public GameObject HasPanel;
    public Button AddButton;
    public Button CloseBtn;
    public Text UserNameText;
    private SceneEntity pEntity;
    private DisplayBoardBehaviour pBehv;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        AddButton.onClick.AddListener(OnAddClick);
        CloseBtn.onClick.AddListener(OnCloseClick);

        UserHeadImage.gameObject.SetActive(false);
    }

    public void SetEntity(SceneEntity entity)
    {
        pEntity = entity;
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        pBehv = bindGo.GetComponent<DisplayBoardBehaviour>();
        var comp = entity.Get<DisplayBoardComponent>();
        ShowPanel(string.IsNullOrEmpty(comp.userId));
        if (!string.IsNullOrEmpty(comp.userId))
        {
            ShowPlayerName(comp.userName);
            CoroutineManager.Inst.StartCoroutine(ResManager.Inst.GetTexture(comp.headUrl, GetTexSuccess, GetTexFail));
        }
    }

    public void ShowPlayerName(string name)
    {
        var nameStr = GameUtils.SetText(name, 9);
        UserNameText.text = nameStr;
    }

    private void ShowPanel(bool isAdd)
    {
        AddPanel.SetActive(isAdd);
        HasPanel.SetActive(!isAdd);
    }

    private void OnAddClick()
    {
#if !UNITY_EDITOR
        MobileInterface.Instance.AddClientRespose(MobileInterface.skipNative, ReqUserInfo);
        MobileInterface.Instance.SkipNative(0);
#else
        OnTest();
#endif
    }

    private void ReqUserInfo(string content)
    {
        LoggerUtils.Log("DisplayBoardPanel ReqUserInfo content is " + content);
        DisplayUserInfo mInfo = JsonConvert.DeserializeObject<DisplayUserInfo>(content);
        string uid = mInfo.uid;
        DisplayBoardData data = new DisplayBoardData();
        data.userId = uid;
        List<string> uidList = new List<string>();
        uidList.Add(uid);
        GameUtils.GetUserInfoByUid(uidList, (string msg) =>
        {
            LoggerUtils.Log("GetUserInfoByUid Success. userInfo is  " + msg);
            HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
            LoggerUtils.Log("DisplayBoardBehaviour GetUserInfoByUid Success. hResponse.data is  " + JsonConvert.SerializeObject(hResponse.data));
            UserInfo[] syncPlayerInfos = JsonConvert.DeserializeObject<UserInfo[]>(hResponse.data);
            var playerInfo = syncPlayerInfos[0];
            data.userName = playerInfo.userName;
            data.headUrl = playerInfo.portraitUrl;
            GetUserInfo(data);
        }, (string content) =>
        {
            LoggerUtils.LogError("OnBatchGetPlayerInfo Fail" + content);
            TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        });
    }

    private void OnTest()
    {
        DisplayBoardData data = new DisplayBoardData();
        data.userId = "1471467658284134400";
        // data.userId = "1478215670264369152";
        data.userName = "hhhhhh";
        data.headUrl = "https://cdn.joinbudapp.com/UgcImage/1471467658284134400/128x128/Profile1471467658284134400_1639689129.png";
        GetUserInfo(data);

        // var uid = "1481600584988889088";
        // List<string> uidReqList = new List<string>();
        // uidReqList.Add(uid);
        // GameUtils.GetUserInfoByUid(uidReqList, (string msg) =>
        //     {
        //         LoggerUtils.Log("GetUserInfoByUid Success. userInfo is  " + msg);
        //         HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(msg);
        //         LoggerUtils.Log("DisplayBoardBehaviour GetUserInfoByUid Success. hResponse.data is  " + JsonConvert.SerializeObject(hResponse.data));
        //         UserInfo[] syncPlayerInfos = JsonConvert.DeserializeObject<UserInfo[]>(hResponse.data);
        //         DisplayBoardData data = new DisplayBoardData();
        //         data.userId = uid;
        //         var playerInfo = syncPlayerInfos[0];
        //         data.userName = playerInfo.userName;
        //         data.headUrl = playerInfo.portraitUrl;
        //         GetUserInfo(data);
        //     }, (string content) =>
        //     {
        //         LoggerUtils.LogError("OnBatchGetPlayerInfo Fail" + content);
        //         TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        //     });
    }

    private void GetUserInfo(DisplayBoardData data)
    {
        LoggerUtils.Log("DisplayBoardPanel GetUserInfo info is " + JsonConvert.SerializeObject(data));
        ShowPanel(false);
        UserHeadImage.gameObject.SetActive(false);
        ShowPlayerName(data.userName);
        pEntity.Get<DisplayBoardComponent>().userName = data.userName;
        pEntity.Get<DisplayBoardComponent>().userId = data.userId;
        pEntity.Get<DisplayBoardComponent>().headUrl = data.headUrl;
        CoroutineManager.Inst.StartCoroutine(ResManager.Inst.GetTexture(data.headUrl, GetTexSuccess, GetTexFail));
    }

    private void GetTexSuccess(Texture2D tex)
    {
        UserHeadImage.gameObject.SetActive(true);
        UserHeadImage.texture = tex;
        pBehv.ShowHeadImage(tex);
    }

    private void GetTexFail()
    {
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }

    private void OnCloseClick()
    {
        ShowPanel(true);
        var comp = pEntity.Get<DisplayBoardComponent>();
        var uid = comp.userId;
        comp.userId = string.Empty;
        comp.userName = string.Empty;
        comp.headUrl = string.Empty;
        UserHeadImage.texture = null;
        UserHeadImage.gameObject.SetActive(false);
        pBehv.ClearUserInfo(uid);
    }
}
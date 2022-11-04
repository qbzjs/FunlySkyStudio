using Newtonsoft.Json;
using SavingData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Author: 熊昭
/// Description: 人物形象简介弹窗展示面板
/// Date: 2022-02-15 11:15:14
/// </summary>
public class UserProfilePanel : BasePanel<UserProfilePanel>
{
    public Button ProfileBtn;
    public Transform FollowParent;
    public Transform AddFriendParent;
    public GameObject SelfBtn;
    public Text NickName;
    public Text UserName; //全小写id
    public GameObject OfficialCert;
    public Text Likes;
    public Text Followers;
    public Text Transactions;
    public Text Bio;
    public RawImage Photo;
    public Texture DefaultPhoto;
    public Button BackBtn;

    private string currentId = "";
    private UserReqInfo reqInfo = new UserReqInfo();
    private TextEllipsis textEllipsis;
    private Coroutine GetPhotoCor;
    private bool onLoading;

    // 账户类型
    enum AccountClassEnum
    {
        Normal = 0, // 普通账号
        Verified = 1, // 官方认证账号
    }

    //按钮类型
    enum BtnType
    {
        Follow,
        AddFriend,
    }

    // 订阅关注关系
    public enum SubscribedEnum
    {
        //0: 无关系 1 关注 2 被对方关注中 3 互关
        Self = -1,
        None,
        Requesting,
        IsRequested,
        Mutual,
    }

    // 好友关系
    public enum FriendshipEnum
    {
        //0: 无关系 1 申请中 2 被对方申请中 3 好友
        Self = -1,
        None,
        Requesting,
        IsRequested,
        Mutual,
    }

    // 行为类型
    enum OperationType
    {
        //0(follow),1(unfollow),2(addFriend),3(cancelFriend)
        Follow,
        Unfollow,
        AddFriend,
        CancelFriend
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        InitBtn();
        textEllipsis = Bio.GetComponent<TextEllipsis>();
    }

    public void OnOpenPanel(string uid)
    {
        currentId = uid;
        UpdatePlayerInfo();
    }

    private void InitBtn()
    {
        ProfileBtn.onClick.AddListener(OnOpenPersonalPage);
        FollowParent.GetChild(0).GetComponent<Button>().onClick.AddListener(() => { OnBtnClick(BtnType.Follow, (int)SubscribedEnum.None); });
        FollowParent.GetChild(2).GetComponent<Button>().onClick.AddListener(() => { OnBtnClick(BtnType.Follow, (int)SubscribedEnum.IsRequested); });
        AddFriendParent.GetChild(0).GetComponent<Button>().onClick.AddListener(() => { OnBtnClick(BtnType.AddFriend, (int)FriendshipEnum.None); });
        AddFriendParent.GetChild(2).GetComponent<Button>().onClick.AddListener(() => { OnBtnClick(BtnType.AddFriend, (int)FriendshipEnum.IsRequested); });
        AddFriendParent.GetChild(3).GetComponent<Button>().onClick.AddListener(OnMsgBtnClick);
        BackBtn.onClick.AddListener(() => { Hide(); });
    }

    private void UpdatePlayerInfo()
    {
        //重置面板
        InitPanel();
        reqInfo.toUid = currentId;
        LoggerUtils.Log("getUserInfo -- reqInfo : " + JsonUtility.ToJson(reqInfo));
        if (!string.IsNullOrEmpty(currentId))
        {
            //is loading
            onLoading = true;
            HttpUtils.MakeHttpRequest("/image/getUserInfo", (int)HTTP_METHOD.GET, JsonUtility.ToJson(reqInfo), OnGetPlayerInfoSuccess, OnGetPlayerInfoFail);
        }
    }

    private void OnOpenPersonalPage()
    {
        if (string.IsNullOrEmpty(currentId))
        {
            LoggerUtils.Log("Current Id Is Null Or Empty");
            return;
        }
        MobileInterface.Instance.OpenPersonalHomePage(currentId);
    }

    private void OnBtnClick(BtnType type, int currentState)
    {
        if (onLoading)
        {
            LoggerUtils.Log("Player Info On Loading");
            return;
        }
        if (string.IsNullOrEmpty(currentId))
        {
            LoggerUtils.Log("Current Id Is Null Or Empty");
            return;
        }
        //切换按钮状态
        RefreshRelationBtn(type, currentState + 1);
        //只请求一次
        SetSubscribeParam req = new SetSubscribeParam()
        {
            toUid = currentId,
            operationType = type == BtnType.Follow ? (int)OperationType.Follow : (int)OperationType.AddFriend,
        };
        LoggerUtils.Log("social -- setSubscribe : " + JsonUtility.ToJson(req));
        HttpUtils.MakeHttpRequest("/social/setSubscribe", (int)HTTP_METHOD.POST, JsonUtility.ToJson(req),
            (success) =>
            {
                LoggerUtils.Log("social set subscribe successed!");
                if (type == BtnType.Follow)
                {
                    DataLogUtils.NewUserFollowers();
                }
                if(type == BtnType.AddFriend)
                {
                    switch (currentState)
                    {
                        case (int)FriendshipEnum.None:
                            DataLogUtils.AddNewUserFriend();
                            break;
                        case (int)FriendshipEnum.IsRequested:
                            DataLogUtils.NewUserFriends(currentId, GameManager.Inst.ugcUserInfo.uid);
                            break;
                    }
                }
            },
            (fail) =>
            {
                LoggerUtils.Log("social set subscribe failed!");
            });
    }

    private void OnMsgBtnClick()
    {
        TipPanel.ShowToast("You need to leave this room to send direct messages!");
    }

    private void OnGetPlayerInfoSuccess(string content)
    {
        onLoading = false;
        HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        RoleSocialData socialPlayerInfo = JsonConvert.DeserializeObject<RoleSocialData>(hResponse.data);
        LoggerUtils.Log("content = " + hResponse.data);

        RefreshPanel(socialPlayerInfo);
        if (!string.IsNullOrEmpty(socialPlayerInfo.userInfo.portraitUrl) && gameObject.activeInHierarchy)
        {
            GetPhotoCor = StartCoroutine(LoadSprite(socialPlayerInfo.userInfo.portraitUrl, Photo));
        }
    }

    private void OnGetPlayerInfoFail(string content)
    {
        onLoading = false;
        LoggerUtils.LogError("Script:UserProfilePanel OnGetPlayerInfoFail error = " + content);
    }

    private void InitPanel()
    {
        RefreshUserName("", "", AccountClassEnum.Normal);
        RefreshRelationInfo(0, 0, 0, SubscribedEnum.None, FriendshipEnum.None);
        RefreshBio("");
        //初始化头像
        if (GetPhotoCor != null)
        {
            StopCoroutine(GetPhotoCor);
        }
        Photo.texture = DefaultPhoto;
    }

    private void RefreshPanel(RoleSocialData playerInfo)
    {
        var userInfo = playerInfo.userInfo;
        var relation = playerInfo.relation;
        var subscribed = (SubscribedEnum)relation.subscribed;
        var friendship = (FriendshipEnum)relation.friendship;
        if (userInfo != null && relation != null)
        {
            if (userInfo.officialCert == null)
            {
                userInfo.officialCert = new OfficialCert();
            }
            //3d show panel -- self
            if (userInfo.uid == GameInfo.Inst.myUid)
            {
                subscribed = SubscribedEnum.Self;
                friendship = FriendshipEnum.Self;
            }
            RefreshUserName(userInfo.userNick, userInfo.userName, (AccountClassEnum)userInfo.officialCert.accountClass);
            RefreshRelationInfo(relation.likes, relation.subscribers, relation.transactions, subscribed, friendship);
            RefreshBio(userInfo.bio);
        }
    }

    private void RefreshUserName(string nick, string userName, AccountClassEnum verified)
    {
        LocalizationConManager.Inst.SetSystemTextFont(NickName);
        NickName.text = DataUtils.FilterNonStandardText(nick);
        UserName.text = "@" + userName;
        OfficialCert.SetActive(verified == AccountClassEnum.Verified);
        OfficialCert.transform.localPosition = new Vector3(UserName.preferredWidth + 31, 0, 0);
    }

    private void RefreshRelationInfo(int likes, int subscribers, int transactions, SubscribedEnum subscribed, FriendshipEnum friendship)
    {
        Likes.text = NumToString(likes);
        Followers.text = NumToString(subscribers);
        Transactions.text = NumToString(transactions);
        RefreshRelationBtn(subscribed, friendship);
    }

    private void RefreshRelationBtn(SubscribedEnum subscribed, FriendshipEnum friendship)
    {
        RefreshRelationBtn(BtnType.Follow, (int)subscribed);
        RefreshRelationBtn(BtnType.AddFriend, (int)friendship);
        //if self --> Me
        SelfBtn.SetActive(subscribed == SubscribedEnum.Self && friendship == FriendshipEnum.Self);
    }

    private void RefreshRelationBtn(BtnType type, int state)
    {
        Transform btnTF = type == BtnType.Follow ? FollowParent : AddFriendParent;
        for (int i = 0; i < btnTF.childCount; i++)
        {
            btnTF.GetChild(i).gameObject.SetActive(state == i);
        }
    }

    private void RefreshBio(string bio)
    {
        var content = DataUtils.FilterNonStandardText(bio);
        textEllipsis.SetText(content);
        string nStr = Bio.text;

        int startIndex = nStr.Length - 1;
        int start = nStr.LastIndexOf('@', startIndex);

        while (start >= 0)
        {
            int finish = nStr.IndexOf(' ', start);
            finish = (finish == -1) ? nStr.Length : finish;
            nStr = nStr.Insert(finish, "</color>").Insert(start, "<color=#B5ABFF>");
            startIndex = start;
            start = nStr.LastIndexOf('@', startIndex);
        }
        LocalizationConManager.Inst.SetSystemTextFont(Bio);
        Bio.text = nStr;
    }

    private string NumToString(int num)
    {
        int b = num / 1000000000;
        int m = (num % 1000000000) / 1000000;
        int k = (num % 1000000) / 1000;
        int p = num % 1000;

        if (b > 0)
        {
            return string.Format("{0}.{1}B", b, m / 100);
        }
        else if (m > 0)
        {
            return string.Format("{0}.{1}M", m, k / 100);
        }
        else if (k > 0)
        {
            return string.Format("{0}.{1}K", k, p / 100);
        }
        else
        {
            return p.ToString();
        }
    }

    IEnumerator LoadSprite(string url, RawImage image)
    {
        UnityWebRequest wr = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        wr.downloadHandler = texDl;
        yield return wr.SendWebRequest();
        if (wr.result == UnityWebRequest.Result.Success)
        {
            image.texture = texDl.texture;
        }
        else
        {
            LoggerUtils.LogError("OnLoadSpriteFail !");
        }
        texDl.Dispose();
        wr.Dispose();
    }
}
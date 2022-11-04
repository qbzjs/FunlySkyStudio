/// <summary>
/// Author:WeiXin
/// Description:
/// Date: 2022/4/8 14:36:3
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BudEngine.NetEngine;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SocialNotificationData
{
    public string id;
    public bool showFriend;
    public bool showFollow;
    public bool enableFriend;
    public bool enableFollow;

    public bool showButton
    {
        get { return showFriend || showFollow; }
    }

    public string tips = String.Empty;
    public string friendStr = String.Empty;
    public string followStr = String.Empty;

    public float hight
    {
        get { return showButton ? 186f : 120f; }
    }

    public NotificationPanelType type;
    public FriendFollow ffdata;
    public bool show;
}

public class FriendFollow
{
    public UserProfilePanel.FriendshipEnum friend;
    public UserProfilePanel.SubscribedEnum follow;

    public bool meetNotification;

    // public bool acceptNotification;
    // public bool friendNotification;
    // public bool followNotification;
    public String name;
}

public enum NotificationPanelType
{
    none,
    meet,
    follow,
    friend,
    confirm
}

public class SocialNotificationManager : MonoManager<SocialNotificationManager>
{
    struct receiveStruct
    {
        public string id;
        public NotificationPanelType type;
    }

    private Dictionary<string, Texture2D> avatarDic = new Dictionary<string, Texture2D>();
    private Dictionary<string, RoleSocialData> infoDic = new Dictionary<string, RoleSocialData>();


    private SortedDictionary<string, FriendFollow> socialDic =
        new SortedDictionary<string, FriendFollow>();

    private List<SocialNotificationData> notificationList = new List<SocialNotificationData>();
    private List<SocialNotificationData> acceptList = new List<SocialNotificationData>();
    private SortedSet<string> meetList = new SortedSet<string>();

    private List<receiveStruct> receiveList = new List<receiveStruct>();

    private PlayerBaseControl player;
    private double distancePow = Math.Pow(3, 2);
    public bool openNotification = true;

    public void Init()
    {
        openNotification = GlobalSettingManager.Inst.IsFriendRequestOpen();
        player = GameObject.Find("GameStart")?.GetComponent<GameController>().playerCom;
    }

    private void Update()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (!openNotification)
        {
            return;
        }
        if (GlobalFieldController.CurGameMode == GameMode.Guest && Global.IsInRoom())
        {
            for (int i = 0; i < Global.Room.RoomInfo.PlayerList.Count; i++)
            {
                var id = Global.Room.RoomInfo.PlayerList[i].Id;
                if (socialDic.ContainsKey(id) && socialDic[id].meetNotification) continue;
                if (meetList.Contains(id))
                {
                    continue;
                }
                else
                {
                    OtherPlayerCtr op = ClientManager.Inst.GetOtherPlayerComById(id);
                    if (op != null && op.GetMapId() == GameManager.Inst.gameMapInfo.mapId)
                    {
                        var distance = player.transform.position - op.transform.position;
                        if (distance.sqrMagnitude <= distancePow)
                        {
                            meetList.Add(id);
                            AddNotification();
                        }
                    }
                }
            }
        }
    }

    private string GetName(string str)
    {
        if (str.Length > 10)
        {
            str = str.Substring(0, 10);
            str += "...";
        }

        return str;
    }

    public SocialNotificationData GetNotification()
    {
        SocialNotificationData n = null;
        if (acceptList.Count > 0)
        {
            n = acceptList[0];
            acceptList.Remove(n);
        }

        if (n == null && notificationList.Count > 0)
        {
            n = notificationList[0];
            notificationList.Remove(n);
        }

        return n;
    }

    public void RemoveList(SocialNotificationData data)
    {
        acceptList.Remove(data);
        notificationList.Remove(data);
    }

    public void NotificationBack(SocialNotificationData data)
    {
        if (data.type == NotificationPanelType.confirm)
        {
            if (acceptList.Count > 0)
            {
                acceptList.Insert(acceptList.Count - 1, data);
            }
            else
            {
                acceptList.Add(data);
            }
        }
        else
        {
            if (notificationList.Count > 0)
            {
                notificationList.Insert(notificationList.Count - 1, data);
            }
            else
            {
                notificationList.Add(data);
            }
        }
    }

    private bool wait = false;

    private void AddNotification()
    {
        // if (socialDic.ContainsKey(id))
        // {
        //     var data = socialDic[id];
        //     if ((data.acceptNotification && type == NotificationPanelType.confirm) ||
        //         (data.friendNotification && type == NotificationPanelType.friend) ||
        //         (data.followNotification && type == NotificationPanelType.follow))
        //         return;
        // }
        if (wait) return;

        string id = null;
        NotificationPanelType type = NotificationPanelType.none;
        if (meetList.Count > 0)
        {
            id = meetList.FirstOrDefault();
            type = NotificationPanelType.meet;
        }
        else if (receiveList.Count > 0)
        {
            var kv = receiveList[0];
            id = kv.id;
            type = kv.type;
        }

        if (id == null) return;

        wait = true;
        if (type == NotificationPanelType.meet)
        {
            GetPlayerInfo(id, () =>
            {
                notificationList.Add(GetMeetSocialNotificationData(id, socialDic[id]));
                SocialNotificationPanel.Show(true);
                SocialNotificationPanel.Instance.ShowNotification();
                meetList.Remove(id);
                wait = false;
                AddNotification();
            });
        }
        else
        {
            GetPlayerInfo(id, () =>
            {
                var ntf = GetPushSocialNotificationData(id, socialDic[id], type);
                if (type == NotificationPanelType.confirm)
                {
                    acceptList.Add(ntf);
                }
                else
                {
                    notificationList.Add(ntf);
                }

                SocialNotificationPanel.Show(true);
                SocialNotificationPanel.Instance.ShowNotification();
                if (receiveList.Count > 0) receiveList.RemoveAt(0);
                wait = false;
                AddNotification();
            });
        }
    }

    private void UpdateNotification(string id)
    {
        for (int i = 0; i < notificationList.Count; i++)
        {
            var v = notificationList[i];
            if (v.id == id && v.showButton)
            {
                notificationList[i] = v.type == NotificationPanelType.meet
                    ? GetMeetSocialNotificationData(id, socialDic[id], notificationList[i])
                    : GetPushSocialNotificationData(id, socialDic[id], notificationList[i].type, notificationList[i]);
            }
        }

        for (int i = 0; i < acceptList.Count; i++)
        {
            var v = acceptList[i];
            if (v.id == id && v.showButton)
            {
                var data = acceptList[i];
                data.show = socialDic[id].friend != UserProfilePanel.FriendshipEnum.Mutual;
            }
        }

        SocialNotificationData uiData = null;
        if (SocialNotificationPanel.Instance != null) uiData = SocialNotificationPanel.Instance.data;
        if (uiData != null && uiData.id == id)
        {
            if (uiData.type == NotificationPanelType.meet)
            {
                GetMeetSocialNotificationData(id, socialDic[id], uiData);
            }
            else if (uiData.type != NotificationPanelType.confirm)
            {
                GetPushSocialNotificationData(id, socialDic[id], uiData.type, uiData);
            }
            else if (uiData.type == NotificationPanelType.confirm)
            {
                uiData.show = socialDic[id].friend != UserProfilePanel.FriendshipEnum.Mutual;
            }

            SocialNotificationPanel.Instance.UpdateUI(uiData);
        }
    }

    public void SendRequest(string id, int type, int status)
    {
        FriendFollowData friendFollowData = new FriendFollowData()
        {
            playerId = id,
            opPlayerId = Player.Id,
            opType = type,
            opStatus = status
        };
        FriendFollowMsg friendFollowMsg = new FriendFollowMsg()
        {
            broadcastType = 1,
            broadcastData = JsonConvert.SerializeObject(friendFollowData),
        };
        RoomChatData data = new RoomChatData()
        {
            msgType = (int) RecChatType.PVPRoomState,
            data = JsonConvert.SerializeObject(friendFollowMsg),
        };
        LoggerUtils.Log("SocialNotificationManager SendRequest =>" + JsonConvert.SerializeObject(data));
        ClientManager.Inst.SendRequest(JsonConvert.SerializeObject(data));
    }

    public void SendRequestWeb(string id, int type)
    {
        SetSubscribeParam req = new SetSubscribeParam()
        {
            toUid = id,
            operationType = type,
        };
        LoggerUtils.Log("social -- setSubscribe : " + JsonUtility.ToJson(req));
        HttpUtils.MakeHttpRequest("/social/setSubscribe", (int) HTTP_METHOD.POST, JsonUtility.ToJson(req),
            (success) =>
            {
                LoggerUtils.Log("subscribe successed!");
                HttpResponDataStruct roleResponseData = JsonConvert.DeserializeObject<HttpResponDataStruct>(success);
                RelationData subscribeData = JsonConvert.DeserializeObject<RelationData>(roleResponseData.data);
                if (socialDic.ContainsKey(id))
                {
                    var data = socialDic[id];
                    if (type == 2)
                    {
                        data.friend = (UserProfilePanel.FriendshipEnum) subscribeData.friendship;
                        switch (data.friend)
                        {
                            case UserProfilePanel.FriendshipEnum.Requesting:
                                DataLogUtils.AddNewUserFriend();
                                break;
                            case UserProfilePanel.FriendshipEnum.Mutual:
                                DataLogUtils.NewUserFriends(id, GameManager.Inst.ugcUserInfo.uid);
                                break;
                        }
                    }
                    else
                    {
                        DataLogUtils.NewUserFollowers();
                        data.follow = (UserProfilePanel.SubscribedEnum) subscribeData.subscribed;
                    }
                }

                UpdateNotification(id);
            },
            (fail) =>
            {
                wait = false;
                AddNotification();
                LoggerUtils.Log("subscribe failed!");
            });
    }

    public void LoadRelation(string id, string name = "", Action act = null)
    {
        UserReqInfo req = new UserReqInfo()
        {
            toUid = id
        };
        HttpUtils.MakeHttpRequest("/social/userRelation", (int) HTTP_METHOD.GET, JsonUtility.ToJson(req),
            (success) =>
            {
                LoggerUtils.Log("userRelation successed!");
                HttpResponDataStruct roleResponseData = JsonConvert.DeserializeObject<HttpResponDataStruct>(success);
                RelationData relationData = JsonConvert.DeserializeObject<RelationData>(roleResponseData.data);

                if (socialDic.ContainsKey(id))
                {
                    var data = socialDic[id];
                    data.friend = (UserProfilePanel.FriendshipEnum) relationData.friendship;
                    data.follow = (UserProfilePanel.SubscribedEnum) relationData.subscribed;
                }
                else
                {
                    var data = new FriendFollow
                    {
                        friend = (UserProfilePanel.FriendshipEnum) relationData.friendship,
                        follow = (UserProfilePanel.SubscribedEnum) relationData.subscribed,
                        name = name,
                        meetNotification = false,
                        // acceptNotification = false,
                        // friendNotification = false,
                        // followNotification = false
                    };
                    socialDic.Add(id, data);
                }

                LoadSprite(id, null, act);
            },
            (fail) =>
            {
                wait = false;
                AddNotification();
                LoggerUtils.Log("userRelation failed!");
            });
    }

    public void LoadSprite(string id, RawImage image = null, Action act = null)
    {
        if (avatarDic.ContainsKey(id))
        {
            if (image != null) image.texture = avatarDic[id];
            act?.Invoke();
        }
        else
        {
            var info = infoDic[id];
            if (!string.IsNullOrEmpty(info.userInfo.portraitUrl))
            {
                CoroutineManager.Inst.StartCoroutine(LoadSpriteIEnumerator(info, image, act));
            }
        }
    }

    IEnumerator LoadSpriteIEnumerator(RoleSocialData info, RawImage image, Action act = null)
    {
        UnityWebRequest wr = new UnityWebRequest(info.userInfo.portraitUrl);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        wr.downloadHandler = texDl;
        yield return wr.SendWebRequest();
        if (!wr.isNetworkError)
        {
            if (image != null) image.texture = texDl.texture;
            if (!avatarDic.ContainsKey(info.userInfo.uid)) avatarDic.Add(info.userInfo.uid, texDl.texture);
            act?.Invoke();
        }
        else
        {
            wait = false;
            AddNotification();
            LoggerUtils.Log("load avatar failed!");
        }

        texDl.Dispose();
        wr.Dispose();
    }

    public bool OnReceiveServer(string senderPlayerId, string msg)
    {
        LoggerUtils.Log("SocialNotificationManager OnReceiveServer==>" + msg);
        if (!openNotification)
        {
            return true;
        }
        FriendFollowMsg ffmsg = JsonConvert.DeserializeObject<FriendFollowMsg>(msg);
        FriendFollowData ffdata = JsonConvert.DeserializeObject<FriendFollowData>(ffmsg.broadcastData);

        GetPlayerInfo(ffdata.opPlayerId, () =>
        {
            var ff = socialDic[ffdata.opPlayerId];
            NotificationPanelType type;
            if (ffdata.opType == 1 && ff.friend == UserProfilePanel.FriendshipEnum.IsRequested)
            {
                type = NotificationPanelType.confirm;
            }
            else if (ffdata.opType == 1 && ff.friend == UserProfilePanel.FriendshipEnum.Mutual)
            {
                type = NotificationPanelType.friend;
            }
            else if (ffdata.opType == 2)
            {
                type = NotificationPanelType.follow;
            }
            else
            {
                type = NotificationPanelType.none;
            }

            if (type != NotificationPanelType.none)
            {
                receiveList.Add(new receiveStruct() {id = ffdata.opPlayerId, type = type});
                AddNotification();
            }
        });
        return true;
    }

    private SocialNotificationData GetPushSocialNotificationData(string id, FriendFollow ffdata,
        NotificationPanelType type, SocialNotificationData data = null)
    {
        data ??= new SocialNotificationData();
        data.id = id;

        data.showFriend = type == NotificationPanelType.confirm;
        data.showFollow = type == NotificationPanelType.follow &&
                          (ffdata.follow == UserProfilePanel.SubscribedEnum.None ||
                           ffdata.follow == UserProfilePanel.SubscribedEnum.IsRequested);

        data.enableFriend = ffdata.friend == UserProfilePanel.FriendshipEnum.None ||
                            ffdata.friend == UserProfilePanel.FriendshipEnum.IsRequested;
        data.enableFollow = ffdata.follow == UserProfilePanel.SubscribedEnum.None ||
                            ffdata.follow == UserProfilePanel.SubscribedEnum.IsRequested;

        switch (ffdata.friend)
        {
            case UserProfilePanel.FriendshipEnum.None:
                data.friendStr = "Add Friend";
                break;
            case UserProfilePanel.FriendshipEnum.Requesting:
                data.friendStr = "Pending";
                break;
            case UserProfilePanel.FriendshipEnum.IsRequested:
                data.friendStr = "Accept";
                break;
            case UserProfilePanel.FriendshipEnum.Mutual:
                data.friendStr = "Message";
                break;
        }

        switch (ffdata.follow)
        {
            case UserProfilePanel.SubscribedEnum.None:
                data.followStr = "Follow";
                break;
            case UserProfilePanel.SubscribedEnum.Requesting:
                data.followStr = "Following";
                break;
            case UserProfilePanel.SubscribedEnum.IsRequested:
                data.followStr = "Follow Back";
                break;
            case UserProfilePanel.SubscribedEnum.Mutual:
                data.followStr = "Mutual";
                break;
        }

        if (type == NotificationPanelType.friend || type == NotificationPanelType.confirm)
        {
            if (ffdata.friend == UserProfilePanel.FriendshipEnum.None ||
                ffdata.friend == UserProfilePanel.FriendshipEnum.IsRequested)
            {
                data.tips = LocalizationConManager.Inst.GetLocalizedText("Friend request from <b>{0}</b>.", GetName(ffdata.name));
            }
            else
            {
                data.tips = LocalizationConManager.Inst.GetLocalizedText("<b>{0}</b> accepted your friend request.", GetName(ffdata.name));
            }
        }
        else if (type == NotificationPanelType.follow)
        {
            data.tips = LocalizationConManager.Inst.GetLocalizedText("<b>{0}</b> just followed you.", GetName(ffdata.name));
        }

        data.type = type;
        data.ffdata = ffdata;
        // ffdata.acceptNotification = type == NotificationPanelType.confirm;
        // ffdata.friendNotification = type == NotificationPanelType.friend;
        // ffdata.followNotification = type == NotificationPanelType.follow;
        if (type == NotificationPanelType.confirm) data.show = ffdata.friend != UserProfilePanel.FriendshipEnum.Mutual;
        if (type == NotificationPanelType.friend || type == NotificationPanelType.follow) data.show = true;
        return data;
    }

    private SocialNotificationData GetMeetSocialNotificationData(string id, FriendFollow ffdata,
        SocialNotificationData data = null)
    {
        data ??= new SocialNotificationData();
        data.id = id;

        data.showFriend = true;
        data.showFollow = true;
        data.enableFriend = ffdata.friend == UserProfilePanel.FriendshipEnum.None ||
                            ffdata.friend == UserProfilePanel.FriendshipEnum.IsRequested;
        data.enableFollow = ffdata.follow == UserProfilePanel.SubscribedEnum.None ||
                            ffdata.follow == UserProfilePanel.SubscribedEnum.IsRequested;

        switch (ffdata.friend)
        {
            case UserProfilePanel.FriendshipEnum.None:
                data.friendStr = "Add Friend";
                break;
            case UserProfilePanel.FriendshipEnum.Requesting:
                data.friendStr = "Pending";
                break;
            case UserProfilePanel.FriendshipEnum.IsRequested:
                data.friendStr = "Accept";
                break;
            case UserProfilePanel.FriendshipEnum.Mutual:
                data.friendStr = "Message";
                break;
        }

        switch (ffdata.follow)
        {
            case UserProfilePanel.SubscribedEnum.None:
                data.followStr = "Follow";
                break;
            case UserProfilePanel.SubscribedEnum.Requesting:
                data.followStr = "Following";
                break;
            case UserProfilePanel.SubscribedEnum.IsRequested:
                data.followStr = "Follow Back";
                break;
            case UserProfilePanel.SubscribedEnum.Mutual:
                data.followStr = "Mutual";
                break;
        }

        data.tips = LocalizationConManager.Inst.GetLocalizedText("You and <b>{0}</b> happen to meet.", GetName(ffdata.name));
        data.type = NotificationPanelType.meet;
        data.ffdata = ffdata;
        data.show = ffdata.follow == UserProfilePanel.SubscribedEnum.None ||
                    ffdata.follow == UserProfilePanel.SubscribedEnum.IsRequested ||
                    ffdata.friend == UserProfilePanel.FriendshipEnum.None ||
                    ffdata.friend == UserProfilePanel.FriendshipEnum.IsRequested;
        ffdata.meetNotification = true;
        return data;
    }

    private void GetPlayerInfo(string id, Action act = null)
    {
        if (socialDic.ContainsKey(id))
        {
            LoadRelation(id, socialDic[id].name, act);
        }
        else
        {
            UserReqInfo reqInfo = new UserReqInfo();
            reqInfo.toUid = id;
            LoggerUtils.Log("getUserInfo -- reqInfo : " + JsonUtility.ToJson(reqInfo));
            HttpUtils.MakeHttpRequest("/image/getUserInfo", (int) HTTP_METHOD.GET, JsonUtility.ToJson(reqInfo),
                (success) =>
                {
                    HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(success);
                    RoleSocialData socialPlayerInfo = JsonConvert.DeserializeObject<RoleSocialData>(hResponse.data);
                    LoggerUtils.Log("GetPlayerInfo successed!");
                    if (!infoDic.ContainsKey(id))
                    {
                        infoDic.Add(id, socialPlayerInfo);
                    }

                    LoadRelation(id, socialPlayerInfo.userInfo.userName, act);
                }, (fail) =>
                {
                    wait = false;
                    AddNotification();
                    LoggerUtils.LogError("Script:SocialNotificationData GetPlayerInfo error = " + fail);
                });
        }
    }

    private void OnDestroy()
    {
        wait = false;
    }
}


public class RelationData
{
    public int subscribed = 0;
    public int friendship = 0;
}

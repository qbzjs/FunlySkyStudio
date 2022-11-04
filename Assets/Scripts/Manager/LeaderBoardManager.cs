using Newtonsoft.Json;
using SavingData;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using static ClientManager;

/// <summary>
/// Author:LiShuZhan
/// Description:对整个场景中的排行榜进行管理
/// Date: 2022.04.14
/// </summary>
public class LeaderBoardManager : ManagerInstance<LeaderBoardManager>, IManager
{
    public int maxCount = 10;//最大数量
    private Dictionary<int, NodeBaseBehaviour> leaderboards = new Dictionary<int, NodeBaseBehaviour>();
    private UserReqInfo reqInfo = new UserReqInfo();
    private HttpMapDataInfo mapInfo = new HttpMapDataInfo();
    private const float maxUserNameWidth = 8.5f;
    private const float maxlNameWidth = 10.5f;
    public NodeBaseBehaviour GetLeaderBoard(int lid)
    {
        if (leaderboards.ContainsKey(lid))
        {
            return leaderboards[lid];
        }
        return null;
    }

    public void AddLeaderBoard(int lid, NodeBaseBehaviour go)
    {
        if (!leaderboards.ContainsKey(lid))
        {
            leaderboards.Add(lid, go);
        }
    }

    public void RemoveLeaderBoard(int lid)
    {
        if (leaderboards.ContainsKey(lid))
        {
            leaderboards.Remove(lid);
        }
    }

    public void OnChangeMode(GameMode gameMode)
    {
        string selfUid = "";
#if UNITY_EDITOR
        if (TestNetParams.testHeader != null)
        {
            selfUid = TestNetParams.testHeader.uid;
        }
#else
        if (GameManager.Inst.ugcUserInfo != null)
        {
            selfUid =  GameManager.Inst.ugcUserInfo.uid;
        }
#endif
        foreach (var item in leaderboards)
        {
            var behav = GetBehav(item.Value.entity);
            if (behav == null)
            {
                continue;
            }
            behav.ChangeGameMode(gameMode);
        }
        if (gameMode == GameMode.Play)
        {
            PlayModeLeaderBoardInfo(selfUid);
        }
    }

    public LeaderBoardBehaviour GetBehav(SceneEntity entity)
    {
        var bing = entity.Get<GameObjectComponent>().bindGo;
        if (bing == null)
        {
            return null;
        }
        var behav = bing.GetComponent<LeaderBoardBehaviour>();
        if (behav == null)
        {
            return null;
        }
        return behav;
    }

    public void OnPVPClose()
    {
        foreach (var item in leaderboards)
        {
            var comp = item.Value.entity.Get<LeaderBoardComponent>();
            if (comp != null)
            {
                comp.curMode = (int)LeaderBoardModeType.None;
            }
        }
    }

    public bool DetectHasLeaderBoard()
    {
        foreach (var item in leaderboards)
        {
            if (item.Value.entity.HasComponent<LeaderBoardComponent>())
            {
                var comp = item.Value.entity.Get<LeaderBoardComponent>();
                if (comp.curMode >= (int)LeaderBoardModeType.Win)
                {
                    return true;
                }
            }
        }
        return false;
    }

    #region 联网数据处理
    /// <summary>
    /// 编辑模式请求
    /// </summary>
    /// <param name="ugcJson"></param>
    public void PlayModeLeaderBoardInfo(string ugcJson)
    {
        reqInfo.toUid = ugcJson;
        LoggerUtils.Log("getUserInfo -- reqInfo : " + JsonUtility.ToJson(mapInfo));
        if (!string.IsNullOrEmpty(ugcJson))
        {
            HttpUtils.MakeHttpRequest("/image/getUserInfo", (int)HTTP_METHOD.GET, JsonUtility.ToJson(mapInfo), OnGetPlayModeLBSuccess, OnGetPlayModeLBFail);
        }
    }

    public void OnGetPlayModeLBSuccess(string content)
    {
        HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        RoleSocialData socialResInfo = JsonConvert.DeserializeObject<RoleSocialData>(hResponse.data);
        LoggerUtils.Log("content = " + hResponse.data);
        SetPlayModeInfo(socialResInfo.userInfo);
    }

    public void OnGetPlayModeLBFail(string content)
    {
        LoggerUtils.LogError("Script:LeaderBoardManager OnGetPlayModeLBFail error = " + content);
    }

    private void SetPlayModeInfo(UserDetails createrInfo)
    {
        foreach (var item in leaderboards)
        {
            var behav = GetBehav(item.Value.entity);
            if (behav == null)
            {
                continue;
            }
            if (createrInfo.officialCert == null)
            {
                createrInfo.officialCert = new OfficialCert();
            }
            behav.itemPanel.gameObject.SetActive(true);
            SetUserInfo(behav.itemPanel, createrInfo.userNick, createrInfo.userName, createrInfo.officialCert.accountClass, createrInfo.portraitUrl);
            SetSpecialInfo(behav.itemPanel, 0, 1, createrInfo.uid);
        }
    }

    /// <summary>
    /// 联机请求,排行榜数据解析
    /// </summary>
    /// <param name="bst"></param>
    public void OnReceiveServer(SendGameBst bst)
    {
        string gameData = bst.GameData;
        LoggerUtils.Log("LeaderBoardManager gameData = " + gameData);
        if (string.IsNullOrEmpty(gameData))
        {
            LoggerUtils.Log("LeaderBoardManager gameData is Null!");
            return;
        }
        PVPLeaderBoardData rspData = JsonConvert.DeserializeObject<PVPLeaderBoardData>(gameData);
        SetLeaderBoardsInfo(rspData.rankingTops);
    }
    private void SetLeaderBoardsInfo(RankingTopData[] rankingTop)
    {
        foreach (var item in leaderboards)
        {
            var behav = GetBehav(item.Value.entity);
            if (behav == null)
            {
                continue;
            }
            SetAllPlayerInfo(rankingTop, behav);
        }
    }
    #endregion

    public void AddItemEvent(string uid)
    {
        UserProfilePanel.Instance.OnOpenPanel(uid);
    }

    public int GetLeaderBoardSet()
    {
        foreach (var item in leaderboards)
        {
            if (item.Value.entity.Get<LeaderBoardComponent>().curMode == (int)LeaderBoardModeType.Win)
            {
                return 1;
            }
        }
        return 0;
    }

    public bool IsOverMaxBoardCount()//是否达到最大数量
    {
        if (leaderboards.Count >= maxCount)
        {
            return true;
        }
        return false;
    }

    public bool IsCanCloneLeaderBoard(GameObject curTarget)//是否能克隆
    {
        if (curTarget.GetComponentInChildren<LeaderBoardBehaviour>() != null)
        {
            int CombineCount = curTarget.GetComponentsInChildren<LeaderBoardBehaviour>().Length;
            if (CombineCount > 1)
            {
                if (CombineCount + leaderboards.Count > maxCount)
                {
                    TipPanel.ShowToast("Oops! Exceed limit:(");
                    return false;
                }
            }
            else
            {
                if (IsOverMaxBoardCount())
                {
                    TipPanel.ShowToast("Oops! Exceed limit:(");
                    return false;
                }
            }
        }
        return true;
    }

    public void SetAllPlayerInfo(RankingTopData[] tempInfos, LeaderBoardBehaviour behav)
    {
        behav.ClearList();
        int maxData = tempInfos.Length < 5 ? tempInfos.Length : 5;
        for (int i = 0; i < maxData; i++)
        {
            var userInfo = tempInfos[i].userInfo;
            var item = GameObject.Instantiate(behav.itemPanel);
            behav.itemLists.Add(item.gameObject);
            item.parent = behav.playPanel;
            item.localPosition = behav.posList[i];
            item.rotation = behav.transform.rotation;
            item.localScale = behav.size;
            item.gameObject.SetActive(true);
            if (userInfo.officialCert == null)
            {
                userInfo.officialCert = new OfficialCert();
            }
            SetUserInfo(item, userInfo.userNick, userInfo.userName, userInfo.officialCert.accountClass, userInfo.portraitUrl);
            SetSpecialInfo(item, i, tempInfos[i].times, userInfo.uid);
        }
    }

    public void SetUserInfo(Transform item, string userName, string lName, int isSupper, string portraitUrl)
    {
        var panel = item.GetComponent<LeaderBoardItem>();
        panel.userName.text = "";
        panel.lname.text = "";
        LocalizationConManager.Inst.SetSystemTextFont(panel.userName);
        CheckLength(userName,maxUserNameWidth, panel.userName);
        CheckLength(lName, maxlNameWidth, panel.lname);
        panel.lname.text = "@" + panel.lname.text;
        bool supper = isSupper == 0 ? false : true;
        panel.superIcon.gameObject.SetActive(supper);
        SetSpecialPos(panel.superIcon, panel.lname);
        GetPhoto(portraitUrl, panel.icon, panel.unIcon, panel);
    }

    private void SetSpecialPos(GameObject special, SuperTextMesh parent)
    {
        Vector3 pos = Vector3.zero;
        float offset = 1;
        pos.x = parent.preferredWidth + offset;
        pos.z = special.transform.localPosition.z;
        pos.y = special.transform.localPosition.y;
        special.transform.localPosition = pos;
    }


    public void SetSpecialInfo(Transform item, int level, int score, string uid)
    {
        var panel = item.GetComponent<LeaderBoardItem>();
        panel.level.text = (level + 1).ToString();
        panel.score.text = score.ToString();
        panel.selfIcon.gameObject.SetActive(IsSelfInfo(uid));
                SetSpecialPos(panel.selfIcon, panel.userName);
        panel.uid = uid;
    }

    private void GetPhoto(string photoUrl, SpriteRenderer image, SpriteRenderer unImage, LeaderBoardItem panel)
    {
        if (!string.IsNullOrEmpty(photoUrl))
        {
            CoroutineManager.Inst.StartCoroutine(GameUtils.LoadTexture2D(photoUrl,
             (tex) =>
             {
                 image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                 unImage.sprite = image.sprite;
                 panel.iconGroup.SetActive(true);
                 panel.notIconGroup.SetActive(false);
             },
             (error) =>
             {
                 LoggerUtils.Log("LoadTextureError");
             }));
        }
    }

    private bool IsSelfInfo(string uid)
    {
        if (GameManager.Inst.ugcUserInfo == null)
        {
            return true;
        }
        if (GameManager.Inst.ugcUserInfo.uid == uid)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CheckLength(string stringToSub, float length, SuperTextMesh text)
    {
        stringToSub = DataUtils.FilterNonStandardText(stringToSub);
        char[] stringChar = stringToSub.ToCharArray();
        for (int i = 0; i < stringChar.Length; i++)
        {
            if (!CalculateLength(stringChar[i], length, text))
            {
                return;
            }
        }
    }

    public bool CalculateLength(char content, float length, SuperTextMesh text)
    {
        text.text += content;
        if (text.preferredWidth > length)
        {
            text.text = text.text.Substring(0, text.text.Length - 1);
            text.text += "...";
            return false;
        }
        return true;
    }

    public void Clear()
    {
        leaderboards.Clear();
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        var curEntity = behaviour.entity;
        if (curEntity.HasComponent<LeaderBoardComponent>())
        {
            curEntity.Get<LeaderBoardComponent>().curMode = (int)LeaderBoardModeType.None;
        }
        RemoveLeaderBoard(behaviour.entity.Get<GameObjectComponent>().uid);
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        int uid = behaviour.entity.Get<GameObjectComponent>().uid;
        AddLeaderBoard(uid, behaviour);
    }
}

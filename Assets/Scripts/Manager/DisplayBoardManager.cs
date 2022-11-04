using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Author:WenJia
/// Description:3D 展示板集中管理器，用于在进入场景时批量拉取展板用户信息还原展示板UI
/// Date: 2022/1/7 17:25:34
/// </summary>

public class DisplayBoardManager : ManagerInstance<DisplayBoardManager>, IManager
{
    private Dictionary<string, List<NodeBaseBehaviour>> displayBevs = new Dictionary<string, List<NodeBaseBehaviour>>();
    private int MaxCount = 99;
    public int CurrentNum;

    public void Clear()
    {
        if (displayBevs != null)
        {
            displayBevs.Clear();
        }
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {

    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {

    }

    // 批量获取用户信息
    public void BatchRequestPlayerInfo()
    {
        if (displayBevs.Keys.Count <= 0)
        {
            return;
        }
        List<string> uidList = new List<string>();
        foreach (var uid in displayBevs.Keys)
        {
            uidList.Add(uid);
        }
        GameUtils.GetUserInfoByUid(uidList, ReqUserInfoSuccess, ReqUserInfoFail);
    }

    public void AddDisplayBev(string uid, DisplayBoardBehaviour bev)
    {
        if (displayBevs.ContainsKey(uid))
        {
            displayBevs[uid].Add(bev);
        }
        else
        {
            List<NodeBaseBehaviour> bevList = new List<NodeBaseBehaviour>();
            bevList.Add(bev);
            displayBevs[uid] = bevList;
        }
    }

    public void RemoveDisplayBev(string uid, DisplayBoardBehaviour bev)
    {
        if (displayBevs.ContainsKey(uid))
        {
            displayBevs[uid].Remove(bev);
        }
    }

    public void UpdateDisplayBoardCurNumber(bool isAdd)
    {
        if (isAdd)
        {
            ++CurrentNum;
        }
        else
        {
            --CurrentNum;
        }
    }

    public bool IsOverMaxCount()
    {
        if (CurrentNum >= MaxCount)
        {
            return true;
        }
        return false;
    }

    public bool IsCanCloneDisplayBoard(GameObject curTarget)
    {
        if (curTarget.GetComponentInChildren<DisplayBoardBehaviour>() != null)
        {
            int combineDisplayNum = curTarget.GetComponentsInChildren<DisplayBoardBehaviour>().Length;
            if (combineDisplayNum > 1)
            {
                // 多个道具组合的情况单独判断
                if (CurrentNum + combineDisplayNum > MaxCount)
                {
                    TipPanel.ShowToast("Oops, exceeds limit:(");
                    return false;
                }
            }
            else
            {
                if (IsOverMaxCount())
                {
                    TipPanel.ShowToast("Oops, exceeds limit:(");
                    return false;
                }
            }
        }
        return true;
    }

    public void ReqUserInfoSuccess(string content)
    {
        LoggerUtils.Log("DisplayBoardManager GetUserInfoByUid Success. userInfo is  " + content);
        HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
        LoggerUtils.Log("DisplayBoardManager GetUserInfoByUid Success. hResponse.data is  " + JsonConvert.SerializeObject(hResponse.data));
        UserInfo[] syncPlayerInfos = JsonConvert.DeserializeObject<UserInfo[]>(hResponse.data);
        for (int i = 0; i < syncPlayerInfos.Length; i++)
        {
            var playerInfo = syncPlayerInfos[i];
            DisplayBoardData data = new DisplayBoardData();
            var uid = playerInfo.uid;
            data.userId = uid;
            data.userName = playerInfo.userName;
            data.headUrl = playerInfo.portraitUrl;
            var displayBevList = displayBevs[uid];
            if (displayBevList.Count > 0)
            {
                foreach (DisplayBoardBehaviour bev in displayBevList)
                {
                    bev.GetUserInfo(data);
                }
            }
        }
    }

    public void ReqUserInfoFail(string msg)
    {
        LoggerUtils.LogError("OnBatchGetPlayerInfo Fail. msg is " + msg);
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }
}

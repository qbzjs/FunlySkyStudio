/// <summary>
/// Author:Mingo-LiZongMing
/// Description:传送点Manager
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPointManager : ManagerInstance<PortalPointManager>, IManager
{
    public static readonly int MaxCount = 99;

    public int portalBtnMax { get; private set; }
    public int portalPosMax { get; private set; }
    private Dictionary<int, SceneEntity> portalButtons = new Dictionary<int, SceneEntity>();
    private Dictionary<int, SceneEntity> portalPoints = new Dictionary<int, SceneEntity>();

    public PortalPointManager()
    {
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, PortalPointVisible);
    }

    public override void Release()
    {
        base.Release();
        ClearPortalList();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, PortalPointVisible);
    }

    public void ClearPortalList()
    {
        portalButtons.Clear();
        portalPoints.Clear();
    }

    public int GetCurMax()
    {
        return Math.Max(portalBtnMax, portalPosMax);
    }

    public int GetNextPid()
    {
        int maxVal = Math.Max(portalBtnMax, portalPosMax);
        return maxVal + 1;
    }

    public int GetBtnNextPid()
    {
        portalBtnMax += 1;
        return portalBtnMax;
    }

    public int GetPointNextPid()
    {
        portalPosMax += 1;
        return portalPosMax;
    }


    public void AddPortalButton(int pid, SceneEntity go)
    {
        if (!portalButtons.ContainsKey(pid))
        {
            portalBtnMax = Math.Max(portalBtnMax, pid);
            portalButtons.Add(pid, go);
        }
        else
        {
            LoggerUtils.Log("AddPortalButton");
        }
    }

    public void AddPortalPoint(int pid, SceneEntity go)
    {
        if (!portalPoints.ContainsKey(pid))
        {
            portalPosMax = Math.Max(portalPosMax, pid);
            portalPoints.Add(pid, go);
        }
        else
        {
            LoggerUtils.Log("AddPortalPoint");
        }
    }
    
    public void RemovePortalButton(int pid)
    {
        if (portalButtons.ContainsKey(pid))
        {
            portalButtons.Remove(pid);
        }
    }

    public void RemovePortalPoint(int pid)
    {
        if (portalPoints.ContainsKey(pid))
        {
            portalPoints.Remove(pid);
        }
    }

    private void PortalPointVisible(GameMode mode)
    {
        if (portalPoints.Count > 0)
        {
            foreach (var portal in portalPoints.Values)
            {
                if (portal.Get<GameObjectComponent>().bindGo != null)
                {
                    portal.Get<GameObjectComponent>().bindGo.transform.gameObject.SetActive(mode == GameMode.Edit);
                }
            }
        }
    }

    public SceneEntity GetPointGo(int pid)
    {
        if (portalPoints.ContainsKey(pid))
            return portalPoints[pid];
        return null;
    }

    public void RefreshPortalPointManagerDic()
    {
        portalPoints.Clear();
    }


    public void RemoveNode(NodeBaseBehaviour nBehaviour)
    {
        var modelType = nBehaviour.entity.Get<GameObjectComponent>().modelType;
        switch (modelType)
        {
            case NodeModelType.PortalButton:
                var bComp = nBehaviour as PortalButtonBehaviour;
                RemovePortalButton(bComp.pid);
                LoggerUtils.Log("PortalPointManager RemoveNode PortalButton:"+bComp.pid);
                break;

            case NodeModelType.PortalPoint:
                var pComp = nBehaviour as PortalPointBehaviour;
                RemovePortalPoint(pComp.pid);
                LoggerUtils.Log("PortalPointManager RemoveNode PortalPoint:"+pComp.pid);
                break;
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.PortalButton)
        {
            var bComp = behaviour as PortalButtonBehaviour;
            AddPortalButton(bComp.pid, behaviour.entity);
        }
        
        if(goCmp.modelType == NodeModelType.PortalPoint)
        {
            var pComp = behaviour as PortalPointBehaviour;
            AddPortalPoint(pComp.pid, behaviour.entity);
        }
    }

    public void Clear()
    {
       
    }
}

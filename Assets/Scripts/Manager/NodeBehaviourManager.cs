using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:WenJia
/// Description:实体 Manager 集中管理器,主要用于集中管理各实体 Manager
/// Date: 2021/12/30 19:00:54
/// </summary>

public class NodeBehaviourManager : MonoManager<NodeBehaviourManager>
{
    private static List<IManager> managerList = new List<IManager>();

    // 管理单例对象的列表，主要用于 Release 时，统一释放所有单例对象
    private static List<BaseInstance> instanceList = new List<BaseInstance>();

    public T CreateManagerInstance<T>() where T : BaseInstance, IManager, new()
    {
        var instance = new T();
        AddManager(instance);
        AddInstance(instance);
        return instance;
    }

    private void AddInstance(BaseInstance instance)
    {
        if (!instanceList.Contains(instance))
        {
            instanceList.Add(instance);
        }
    }

    private void AddManager(IManager mgr)
    {
        if (!managerList.Contains(mgr))
        {
            managerList.Add(mgr);
        }
    }

    /**
    * 清除列表中所有 Manager 的指定的 NodeBaseBehaviour 关联的实体
    * 当移除某个实体时，调用
    */
    public void ClearManagerNodeBehaviour(NodeBaseBehaviour nBehav)
    {
        for (int i = 0; i < managerList.Count; i++)
        {
            var x = managerList[i];
            if (x != null)
            {
                x.RemoveNode(nBehav);
            }
        }
    }

    public void RevertManagerNodeBehaviour(NodeBaseBehaviour nBehav)
    {
        for (int i = 0; i < managerList.Count; i++)
        {
            var x = managerList[i];
            if (x != null)
            {
                x.RevertNode(nBehav);
            }
        }
    }

    // 移除 Manager 的所有相关的 NodeBaseBehaviour 关联的实体
    public void RemoveManager(IManager mgr)
    {
        mgr.Clear();
    }

    /** 
    * 移除列表中每个 Manager 的所有相关的 NodeBaseBehaviour 关联的实体
    * 仅在重建地图时调用，（即 SceneBuilder.ParseAndBuild 方法） 
    */
    public void ClearManagers()
    {
        for (int i = 0; i < managerList.Count; i++)
        {
            var x = managerList[i];
            if (x != null)
            {
                RemoveManager(x);
            }
        }
    }

    // 释放列表中所有 Manager 的单例
    public static void Release()
    {
        if (instanceList.Count > 0)
        {
            for (int i = 0; i < instanceList.Count; i++)
            {
                var ins = instanceList[i];
                if (ins != null)
                {
                    ins.Release();
                }
            }
        }
        managerList.Clear();
        instanceList.Clear();
    }
}
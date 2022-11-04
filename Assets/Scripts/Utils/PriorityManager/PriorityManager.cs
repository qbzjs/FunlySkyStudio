using System;
using System.Collections.Generic;

public class CPriorityInstanceManager
{
    private static Dictionary<string, BaseInstance> allInstances = new Dictionary<string, BaseInstance>();
    public static PriorityManager<TAction> CreateInstance<TAction>() where TAction : BaseAction
    {
        string typeName = typeof(PriorityManager<TAction>).Name + typeof(TAction).Name; ;
        if (!allInstances.ContainsKey(typeName) || allInstances[typeName] == null)
        {
            var instance = new PriorityManager<TAction>();
            allInstances.Add(typeName, instance);
        }
        return allInstances[typeName] as PriorityManager<TAction>;
    }
    

    public static void Release()
    {
        if (allInstances.Count > 0)
        {
            foreach (var ins in allInstances.Values)
            {
                ins?.Release();
            }
        }
        allInstances.Clear();
    }
}

public class PriorityManager<TAction> : BaseInstance, IDisposable where TAction : BaseAction
{
    public const int MAX_DO_COUNT = 25;

    private SortedDictionary<int, List<TAction>> actionWaitingList;
    private List<TAction> actionDoingList;

    private bool isUsed = false;

    private static PriorityManager<TAction> _instance;

    public static PriorityManager<TAction> Inst
    {
        get
        {
            _instance = CPriorityInstanceManager.CreateInstance<TAction>();
            return _instance;
        }
    }

    public override void Release()
    {
        base.Release();
        _instance = null;
        this.Dispose();
    }

    public PriorityManager()
    {
        actionWaitingList = new SortedDictionary<int, List<TAction>>();
        actionDoingList = new List<TAction>();
        isUsed = true;
    }

    public TAction Do(TAction action)
    {
        action.CallBackManager = PriorityManagerCallBack;
        if (actionDoingList.Count < MAX_DO_COUNT)
        {
            actionDoingList.Add(action);
            action.Do();
        }
        else
        {
            AddWaitingList(action, action.Priority);
        }

        return action;
    }
    
    public void SetPriority(TAction action, int priority)
    {
        //修改未进行中的队列优先级
        if (actionWaitingList[action.Priority] != null && actionWaitingList[action.Priority].Contains(action))
        {
            actionWaitingList[action.Priority].Remove(action);
            AddWaitingList(action, priority);
            action.Priority = priority;
        }
        //修改正在进行中的队列优先级
        else if (actionDoingList.Contains(action))
        {
            //TODO
        }
    }

    public int GetWaitingCount()
    {
        var tmpCount = 0;
        foreach (var keyValuePair in actionWaitingList)
        {
            tmpCount += keyValuePair.Value.Count;
        }
        return tmpCount;
    }
    
    public int GetDoingCount()
    {
        return actionDoingList.Count;
    }

    private void PriorityManagerCallBack(BaseAction action, object data, string err)
    {
        if (!isUsed)
        {
            return;
        }
        if (action == null)
            return;
        action.CallBack(action, data, err);
        actionDoingList.Remove(action as TAction);

        TAction actionWaiting = null;

        foreach(int i in actionWaitingList.Keys)
        {
            if (actionWaitingList[i] != null && actionWaitingList[i].Count > 0)
            {
                for (int j = actionWaitingList[i].Count - 1; j >= 0; j++)
                {
                    if (!actionWaitingList[i][j].IsUsed)
                    {
                        LoggerUtils.Log("Is Used:" + actionWaitingList[i][j].GetType().Name);
                        actionWaitingList[i].RemoveAt(j);
                        continue;
                    }
                    actionWaiting = actionWaitingList[i][j];
                    break;
                }
                actionWaitingList[i].Remove(actionWaiting);
                if (actionWaiting != null)
                    break;
            }
        }

        if (actionWaiting != null)
        {
            actionDoingList.Add(actionWaiting);
            actionWaiting.Do();
        }

    }
    
    private void AddWaitingList(TAction action, int priority)
    {
        if (!actionWaitingList.ContainsKey(priority))
            actionWaitingList.Add(priority, new List<TAction>());

        actionWaitingList[priority].Add(action);
    }

    public void Clear()
    {
        TAction actionWaiting = null;
        foreach (int i in actionWaitingList.Keys)
        {
            if (actionWaitingList[i] != null && actionWaitingList[i].Count > 0)
            {
                actionWaiting = actionWaitingList[i][actionWaitingList[i].Count - 1];
                actionWaitingList[i].Remove(actionWaiting);
                if (actionWaiting != null)
                    actionWaiting.Dispose();
            }
        }

        TAction action = null;
        for (int i = 0; i < actionDoingList.Count; i++)
        {
            action = actionDoingList[i];
            if (action != null)
                action.Dispose();
        }
        actionDoingList.Clear();
    }

    public void Dispose()
    {
        Clear();
        isUsed = false;
    }
}

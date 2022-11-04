using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

public class TaskRunner
{
    private object result;
    private Action<object> msgResp;
    public TaskRunner(object val, Action<object> resp)
    {
        result = val;
        msgResp = resp;
    }

    public void Invoke()
    {
        msgResp?.Invoke(result);
        result = null;
    }
}


public class MainThreadDispatcher : MonoBehaviour
{
    private readonly List<TaskRunner> executionQueue = new List<TaskRunner>();
    private readonly List<Action> executionActionQueue = new List<Action>();
    private static bool initialized = false;
    private static MainThreadDispatcher _current;
    //private int _count;
    public static MainThreadDispatcher Current
    {
        get
        {
            Init();
            return _current;
        }
    }
    
    

    public static void Init()
    {
        if (!initialized)
        {

            if (!Application.isPlaying)
                return;
            initialized = true;
            var g = new GameObject("MainThreadDispatcher");
            _current = g.AddComponent<MainThreadDispatcher>();
#if !ARTIST_BUILD
            //UnityEngine.Object.DontDestroyOnLoad(g);
            //g.DontDestroy();
#endif
        }

    }

    // public static MainThreadDispatcher Instance()
    // {
    //     if (!Exists())
    //     {
    //         throw new Exception("UnityMainThreadDispatcher could not find the UnityMainThreadDispatcher object. Please ensure you have added the MainThreadExecutor Prefab to your scene.");
    //     }
    //     return _instance;
    // }


    public static bool Exists()
    {
        return _current != null;
    }


    void Awake()
    {
        _current = this;
        initialized = true;
        this.gameObject.DontDestroy();
    }
    // void Awake()
    // {
    //     if (_instance == null)
    //     {
    //         _instance = this;
    //         DontDestroyOnLoad(this.gameObject);
    //     }
    // }

    // private void Update()
    // {
    //     lock (executionQueue)
    //     {
    //         while (executionQueue.Count > 0)
    //         {
    //             try
    //             {
    //                 var excute = executionQueue.Dequeue();
    //                 excute?.Invoke();
    //             }
    //             catch (Exception e)
    //             {
    //                 LoggerUtils.LogError("MainThreadDispatcher executionQueue Error:" + e.ToString());
    //             }
    //
    //         }
    //     }
    //
    //     lock (executionActionQueue)
    //     {
    //         while (executionActionQueue.Count > 0)
    //         {
    //             try
    //             {
    //                 executionActionQueue.Dequeue().Invoke();
    //             }
    //             catch (Exception e)
    //             {
    //                 LoggerUtils.LogError("MainThreadDispatcher executionActionQueue Error:" + e.ToString());
    //             }
    //         }
    //     }
    // }

    List<Action> _currentActions = new List<Action>();
    List<TaskRunner> _currentTasks = new List<TaskRunner>();

    // Update is called once per frame
    void Update()
    {
        lock (executionActionQueue)
        {
            _currentActions.Clear();
            _currentActions.AddRange(executionActionQueue);
            executionActionQueue.Clear();
        }
        foreach(var act in _currentActions)
        {
            act?.Invoke();
        }
        lock(executionQueue)
        {
            _currentTasks.Clear();
            _currentTasks.AddRange(executionQueue);
            executionQueue.Clear();
        }
        foreach(var act in _currentTasks)
        {
            act?.Invoke();
        }
    }
    public static void Enqueue(TaskRunner task)
    {
        if (Current != null)
        {
            lock (Current.executionQueue)
            {
                if (Current)
                {
                    Current.executionQueue.Add(task);
                }
            }
        }
    }

    public static void Enqueue(Action action)
    {
        if (Current != null)
        {
            lock (Current.executionActionQueue)
            {
                if (Current)
                {
                    Current. executionActionQueue.Add(action);
                }
            }
        }
    }

    void OnDestroy()
    {
        executionQueue.Clear();
        executionActionQueue.Clear();
        _current = null;
        initialized = false;
    }
}
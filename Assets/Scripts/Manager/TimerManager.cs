// TimerManager.cs
// Created by xiaojl Jul/13/2022
// 定时器管理

using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoManager<TimerManager>
{
    // 私有变量
    private List<BudTimer> _runningLst;
    private List<BudTimer> _disposeLst;

    // 启动timer，循环触发不停止
    public BudTimer Run(string name, float delayTime, float duration, Action callback)
    {
        return CreateTimer(name, false, delayTime, duration, 0, callback);
    }

    // 启动timer，触发一次后停止
    public BudTimer RunOnce(string name, float delayTime, Action callback)
    {
        return CreateTimer(name, true, delayTime, 0, 1, callback);
    }

    // 启动timer，触发指定次数后停止
    public BudTimer RunDisposable(string name, float delayTime, float duration, int count, Action callback)
    {
        return CreateTimer(name, true, delayTime, duration, count, callback);
    }

    // 停止timer
    public void Stop(BudTimer timer)
    {
        // timer不存在
        if (timer == null)
            return;

        // timer已经析构
        if (timer.IsDisposed)
        {
            Debug.LogWarning(string.Format("Stop timer {0} was failed when it is disposed", timer.Name));
            return;
        }

        // 标记timer已经析构
        timer.IsDisposed = true;

        // 从timer列表中移除
        _runningLst.Remove(timer);
    }

    // 挂起timer
    public void Suspend(BudTimer timer)
    {
        // timer不存在
        if (timer == null)
            return;

        // timer已经处于suspend状态
        if (timer.IsSuspend)
        {
            Debug.LogWarning(string.Format("Suspend timer {0} was failed when it is suspended", timer.Name));
            return;
        }

        // 标记timer已经挂起
        timer.IsSuspend = true;
    }

    // 恢复timer
    public void Resume(BudTimer timer)
    {
        // timer不存在
        if (timer == null)
            return;

        // timer已经处于non-suspend状态
        if (!timer.IsSuspend)
        {
            Debug.LogWarning(string.Format("Resume timer {0} was failed when it is non-suspend", timer.Name));
            return;
        }

        // 恢复timer
        timer.IsSuspend = false;
    }

    private void Awake()
    {
        // 初始化
        _runningLst = new List<BudTimer>();
        _disposeLst = new List<BudTimer>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 析构
        _disposeLst = null;
        _runningLst = null;
    }

    private void Update()
    {
        // 清空timer析构列表
        _disposeLst.Clear();

        // 更新所有timer
        for (int i = 0; i < _runningLst.Count; i++)
        {
            var timer = _runningLst[i];

            // 跳过挂起状态的timer
            if (timer.IsSuspend)
                continue;

            // 获取timer的剩余时长和触发次数
            var time = timer.Time;
            var count = 0;

            // 减少timer的剩余时长
            time = time - Time.deltaTime;

            // 触发timer
            while (time < 0)
            {
                // 执行回调
                timer.Callback?.Invoke();

                // 基于timer下一次触发的剩余时长
                time = time + timer.Duration;
                count = count + 1;

                // timer已经被析构
                if (timer.IsDisposed)
                    break;

                // 将到达限制次数的可移除timer放入析构列表
                if (timer.IsDisposable && count >= timer.Count)
                {
                    _disposeLst.Add(timer);
                    break;
                }
            }

            // 更新timer的剩余时长和触发次数
            timer.Time = time;
            timer.Count = timer.Count - count;
        }

        // 析构可以移除的timer
        for (int i = 0; i < _disposeLst.Count; i++)
        {
            var timer = _disposeLst[i];
            Stop(timer);
        }
    }

    // 创建timer
    private BudTimer CreateTimer(string name, bool disposable, float delayTime, float duration, int count, Action callback)
    {
        // 参数检查
        Debug.Assert(delayTime >= 0 && duration >= 0 && count >= 0);

        // 初始化timer的数据
        var timer = new BudTimer();
        timer.Name = name;
        timer.IsSuspend = false;
        timer.IsDisposed = false;
        timer.IsDisposable = disposable;
        timer.Time = delayTime;
        timer.Duration = duration;
        timer.Count = count;
        timer.Callback = callback;

        // 插入timer列表
        _runningLst.Add(timer);

        // 创建完成
        return timer;
    }
}

// 定时器
public class BudTimer
{
    public string Name { get; set; } // 定时器名称
    public bool IsSuspend { get; set; } // 是否处于挂起状态
    public bool IsDisposed { get; set; } // 是否已析构
    public bool IsDisposable { get; set; } // 是否可移除
    public float Time { get; set; } // 到达下次触发的剩余时长
    public float Duration { get; set; } // 间隔时长
    public int Count { get; set; } // 限制次数
    public Action Callback { get; set; } // 回调
}

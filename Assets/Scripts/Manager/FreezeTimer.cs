using System;

public class FreezeTimer
{
    public float mTime;
    public float mCurTime;
    public Action<float, string> mTimerCallBack;
    public Action<string, float> mTimer3SEndCallBack;
    public Action<string> mTimer1SEndCallBack;
    public Action<string> mTimer1SCallBack;
    public Action<string> mTimerFinishedCallBack;
    public Action<string> mTimerForceFinishedCallBack;
    public bool mIsStop = false;
    public bool mIsStart = false;
    public string mPlayerId;
    public bool mIsDirty = false;
    //本地兜底时间，比服务器多2秒，如果服务器的解冻消息没有被客户端正确接收，就由这个时间强制解冻
    public float mForceFinishTime = 0;//
    public void Init(float time, string playerId)
    {
        mIsStop = false;
        mIsStart = true;
        mTime = time;
        mPlayerId = playerId;
        mIsDirty = false;
        mForceFinishTime = mTime + 2;
        mCurTime = 0;
    }
    public void Start()
    {
        mIsStart = true;
        mIsStop = false;
    }
    public void Stop()
    {
        mIsStop = true;
        mIsStart = false;
    }
    public void Finished()
    {
        Stop();
        mCurTime = 0;
        mIsDirty = true;
        mTimer1SCallBack = null;
        mTimer1SEndCallBack = null;
        mTimer3SEndCallBack = null;
        mTimerFinishedCallBack = null;
        mTimerForceFinishedCallBack = null;
    }
    public void Update(float deltaTime)
    {
        if (mIsDirty) return;
        if (mIsStart && !mIsStop)
        {
            mForceFinishTime -= deltaTime;
            mTime -= deltaTime;
            mCurTime += deltaTime;
            mTimerCallBack?.Invoke(mTime, mPlayerId);
            if (mTime >= 0)
            {
                if (mTimer1SCallBack != null && mCurTime >= 1.0f)
                {
                    mTimer1SCallBack?.Invoke(mPlayerId);
                    mTimer1SCallBack = null;
                }
                else if (mTimer1SEndCallBack != null && mTime <= 0.2f)
                {
                    mTimer1SEndCallBack?.Invoke(mPlayerId);
                    mTimer1SEndCallBack = null;
                }
                else if (mTimer3SEndCallBack != null && mTime <= 3.0f)
                {
                    mTimer3SEndCallBack?.Invoke(mPlayerId, mTime);
                    mTimer3SEndCallBack = null;
                }
            }
            else
            {
                if (mTimerFinishedCallBack != null)
                {
                    mTimerFinishedCallBack?.Invoke(mPlayerId);
                    mTimerFinishedCallBack = null;
                }
            }

            if (mForceFinishTime <= 0)
            {
                mTimerForceFinishedCallBack?.Invoke(mPlayerId);
                mTimerForceFinishedCallBack = null;
                Finished();
            }
        }
    }
    public void UnInit()
    {
        mCurTime = 0;
        mIsStop = true;
        mIsStart = false;
        mTimerCallBack = null;
        mTimer1SCallBack = null;
        mTimer1SEndCallBack = null;
        mTimer3SEndCallBack = null;
        mTimerFinishedCallBack = null;
    }
    public float GetCurrentTime()
    {
        return mTime;
    }
    public void Destroy()
    {
        mIsDirty = true;
    }
    public void ForceSplitTime(float time)
    {
        //跳过的时间 加回去
        mCurTime += (mTime - time);
        mTime = time;
        mForceFinishTime = mTime + 2;
    }
}

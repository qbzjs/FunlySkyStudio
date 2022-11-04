using System.Collections.Generic;
//TODO：需要重命名定义为BuffManager
public class FreezeTimerManager
{
    public FreezePropsManager mManager;
    public Dictionary<string, FreezeTimer> mTimers;
    public List<string> mWaittingDestroy;
    public FreezeTimerManager(FreezePropsManager manager)
    {
        mWaittingDestroy = new List<string>();
        mTimers = new Dictionary<string, FreezeTimer>();
        mManager = manager;
    }
    public FreezeTimer CreateTimer(string id, float time)
    {
        FreezeTimer timer = null;
        if (!mTimers.TryGetValue(id, out timer))
        {
            timer = new FreezeTimer();
            timer.Init(time, id);
            mTimers.Add(id, timer);
            return timer;
        }
        else
        {
            return null;
        }

    }
    public void DestroyTimer(string playerId)
    {
        FreezeTimer timer;
        if (mTimers.TryGetValue(playerId, out timer))
        {
            timer.Destroy();
            mWaittingDestroy.Add(playerId);
        }
    }
    public void Update(float deltaTime)
    {
        if (mTimers != null && mTimers.Count > 0)
        {
            foreach (var item in mTimers)
            {
                FreezeTimer timer = item.Value;
                if (timer.mIsDirty)
                {
                    continue;
                }
                timer?.Update(deltaTime);
            }
            for (int i = 0; i < mWaittingDestroy.Count; i++)
            {
                string key = mWaittingDestroy[i];
                if (mTimers.ContainsKey(key))
                {
                    mTimers.Remove(key);
                }
            }
            mWaittingDestroy.Clear();
        }
    }
    public FreezeTimer GetTimer(string playerId)
    {
        FreezeTimer timer = null;
        if (mTimers.TryGetValue(playerId,out timer))
        {
            if (timer.mIsDirty)
            {
                return null;
            }
        }
        return timer;
    }
    public void DestroyAllTimer()
    {
        mTimers.Clear();
    }
   
}

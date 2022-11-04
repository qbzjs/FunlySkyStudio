using UnityEngine;

public class NetModelFreezePlayerManager : FreezePlayerManagerBase
{
    public override void MainPlayerFreeze(string playerId, float keepTime)
    {
        FreezeTimer timer = mManager.mTimerManager.CreateTimer(playerId, keepTime);
        if (timer!=null)
        {
            timer.Start();
            timer.mTimer1SEndCallBack = PlayerTimer1SEndCallBack;
            timer.mTimer1SCallBack = PlayerTimer1SCallBack;
            //timer.mTimer3SEndCallBack = PlayerTimer3SEndCallBack;
            timer.mTimerForceFinishedCallBack = ClientForceUnFreeze;
            base.MainPlayerFreeze(playerId, keepTime);
        }
    }
}

using System.Collections.Generic;
using BudEngine.NetEngine;
using UnityEngine;

public class FreezePlayerManagerBase
{
    public FreezePropsManager mManager;
    private Dictionary<string, int> mFreezePlayerIds = new Dictionary<string, int>();
    public void SetManager(FreezePropsManager manager)
    {
        mManager = manager;
    }
    public virtual void MainPlayerFreeze(string playerId, float keepTime)
    {
        PlayerBaseControl player = PlayerBaseControl.Inst;
        player.playerAnim.speed = 0;
        Transform parent = player.playerAnim.transform;
        player.AddNoAbilityFlag(EObjAbilityType.Move);
        player.AddNoAbilityFlag(EObjAbilityType.Pickability);
        player.AddNoAbilityFlag(EObjAbilityType.SelfieMode);
        player.AddNoAbilityFlag(EObjAbilityType.Emo);
        mManager.mEffectManager.PlayEffect(playerId, parent);
        AKSoundManager.Inst.PostEvent("Play_Ice_Freeze", parent.gameObject);
        AddPlayerId(playerId);
        PlayerTimer3SEndCallBack(playerId, keepTime);
    }
    public virtual void MainPlayerUnFreeze(string playerId)
    {
        PlayerBaseControl player = PlayerBaseControl.Inst;
        player.playerAnim.speed = 1;
        player.RemoveNoAbilityFlag(EObjAbilityType.Move);
        player.RemoveNoAbilityFlag(EObjAbilityType.Pickability);
        player.RemoveNoAbilityFlag(EObjAbilityType.SelfieMode);
        player.RemoveNoAbilityFlag(EObjAbilityType.Emo);
        mManager.mTimerManager.DestroyTimer(playerId);
        mManager.mEffectManager.StopEffect(playerId);
        RemovePlayerId(playerId);
        FreezePanel.Hide();
    }
    public virtual void OtherPlayerFreeze(string playerId, float keepTime)
    {
        Transform parent = null;
        OtherPlayerCtr otherPlayer = ClientManager.Inst.GetOtherPlayerComById(playerId);
        if (otherPlayer != null)
        {
            parent = otherPlayer.transform;
            FreezeTimer timer = mManager.mTimerManager.CreateTimer(playerId, keepTime);
            if (timer != null)
            {
                timer.Start();
                timer.mTimer1SEndCallBack = PlayerTimer1SEndCallBack;
                timer.mTimer1SCallBack = PlayerTimer1SCallBack;
                timer.mTimerForceFinishedCallBack = ClientForceUnFreeze;
                otherPlayer.m_PlayerAnim.speed = 0;
                mManager.mEffectManager.PlayEffect(playerId, parent);

                AKSoundManager.Inst.PostEvent("Play_Ice_Freeze", parent.gameObject);
                AddPlayerId(playerId);
            }
        }
    }
    public virtual void OtherPlayerUnFreeze(string playerId)
    {
        OtherPlayerCtr otherPlayer = ClientManager.Inst.GetOtherPlayerComById(playerId);
        if (otherPlayer != null)
        {
            otherPlayer.m_PlayerAnim.speed = 1;
        }
        mManager.mTimerManager.DestroyTimer(playerId);
        mManager.mEffectManager.StopEffect(playerId);
        RemovePlayerId(playerId);
    }
    public void ClientForceUnFreeze(string playerId)
    {
        if (Player.Id == playerId)
        {
            MainPlayerUnFreeze(playerId);
        }
        else
        {
            OtherPlayerUnFreeze(playerId);
        }
    }
    //pvp重置时候，所有玩家都强制解冻，不需要发送网络请求，服务器已经清理玩家状态
    public void AllClientForceUnFreeze()
    {
        if (mFreezePlayerIds!=null)
        {
            List<string> playerids = new List<string>();
            foreach (var item in mFreezePlayerIds)
            {
                playerids.Add(item.Key);
            }
            for (int i = 0; i < playerids.Count; i++)
            {
                ClientForceUnFreeze(playerids[i]);
            }
        }
    }
    public void AddPlayerId(string playerId)
    {
        if (!mFreezePlayerIds.ContainsKey(playerId))
        {
            mFreezePlayerIds.Add(playerId, 0);
        }
    }
    public void RemovePlayerId(string playerId)
    {
        if (mFreezePlayerIds.ContainsKey(playerId))
        {
            mFreezePlayerIds.Remove(playerId);
        }
    }
    public void PlayerTimer1SCallBack(string playerId)
    {
        mManager.mEffectManager.PlayContinueEffect(playerId);
    }
    public void PlayerTimer1SEndCallBack(string playerId)
    {
        mManager.mEffectManager.PlayEndEffect(playerId);
        GameObject parent = null;
        if (playerId == Player.Id)
        {
            parent = PlayerBaseControl.Inst.playerAnim.gameObject;
        }
        else
        {
            OtherPlayerCtr otherPlayer = ClientManager.Inst.GetOtherPlayerComById(playerId);
            if (otherPlayer != null)
            {
                parent = otherPlayer.transform.gameObject;
            }
        }
        AKSoundManager.Inst.PostEvent("Play_Ice_Burst", parent);
    }
    public void PlayerTimer3SEndCallBack(string playerId, float time)
    {
        FreezePanel.Show();
        FreezePanel.Instance.OnTimerTick(time);
    }
    public bool CheckerPlayerIsFreeze(string playerId)
    {
        return mFreezePlayerIds.ContainsKey(playerId);
    }
    public void Clear()
    {
        mFreezePlayerIds.Clear();
    }
}

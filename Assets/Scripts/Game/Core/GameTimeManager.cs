using System;

public class GameTimeManager : BMonoBehaviour<GameTimeManager>
{
    private delegate void FixedTimePassed();

    private FixedTimePassed _mFixedTimePassed;

    protected override void Awake()
    {
        base.Awake();
        //添加Time流逝接收方法
        AddFixedTimePassed(SkyboxManager.Inst.OnFixedTimePassed);
        AddFixedTimePassed(FlashLightManager.Inst.OnFixedTime);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        //移除Time流逝接收方法
        RemoveFixedTimePassed(SkyboxManager.Inst.OnFixedTimePassed);
        RemoveFixedTimePassed(FlashLightManager.Inst.OnFixedTime);
        _mFixedTimePassed = null;
    }

    private void FixedUpdate()
    {
        _mFixedTimePassed?.Invoke();
    }

    public void AddFixedTimePassed(Action act)
    {
        _mFixedTimePassed += new FixedTimePassed(act);
    }

    public void RemoveFixedTimePassed(Action act)
    {
        _mFixedTimePassed -= new FixedTimePassed(act);
    }
}
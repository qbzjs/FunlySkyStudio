/// <summary>
/// Author: Shaocheng
/// Description: 昼夜天空盒管理
/// Date: 2022-9-7 16:27:24
/// </summary>
public enum SkyboxType
{
    Normal = 0, //普通天空盒
    DayNight = 1, //昼夜天空盒
}

public class SkyboxManager : CInstance<SkyboxManager>
{
    public int defaultSkyId = 0;

    #region 昼夜天空盒

    public void Init()
    {
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.AddListener(MessageName.OnForeground, OnForeGround);
    }

    public override void Release()
    {
        base.Release();
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        MessageHelper.RemoveListener(MessageName.OnForeground, OnForeGround);
    }

    private void OnChangeMode(GameMode mode)
    {
        if (mode == GameMode.Play)
        {
            SceneBuilder.Inst.SkyboxBev.EnterSkyboxPlayMode();
        }
        else if (mode == GameMode.Edit)
        {
            SceneBuilder.Inst.SkyboxBev.EnterSkyboxEditMode();
        }
        else if (mode == GameMode.Guest)
        {
            SceneBuilder.Inst.SkyboxBev.EnterSkyboxGuestMode();
        }
    }


    private void OnForeGround()
    {
        if (GameManager.Inst != null && GameManager.Inst.loadingPageIsClosed)
        {
            SceneBuilder.Inst.SkyboxBev.EnterSkyboxGuestMode(true);
        }
    }

    public void OnFixedTimePassed()
    {
        //天空盒刷skytime
        if (SceneBuilder.Inst.SkyboxBev != null)
        {
            SceneBuilder.Inst.SkyboxBev.OnFixedTimePassed();
        }
    }

    public SkyboxType GetCurSkyboxType()
    {
        return SceneBuilder.Inst.SkyboxBev.entity.Get<SkyboxComponent>().skyboxType;
    }

    //取skytime的小数点位
    public float GetTimeOfDay(float skyTime)
    {
        return skyTime - (int) skyTime;
    }

    #endregion
}
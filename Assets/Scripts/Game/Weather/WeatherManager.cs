using UnityEngine;

public class WeatherManager : CInstance<WeatherManager>
{
    private WeatherParams curParams = new WeatherParams();
    private EWeatherType last = EWeatherType.None;
    private WeatherEffectController wec;
    private WeatherSoundController wsc;
    private bool isFPSCollected = false;
    
    public void ChangeWeatherWithSaveParams(WeatherSaveParams weatherParams)
    {
        last = curParams.saveParams.weatherType;
        curParams.saveParams = weatherParams;
    }

    public void ChangeWeatherType(EWeatherType weatherType)
    {
        last = curParams.saveParams.weatherType;
        curParams.saveParams.weatherType = weatherType;
        wsc.SyncSound(curParams.saveParams.weatherType, last);
    }

    public void ShowCurrentWeather()
    {
        wec.SyncWeatherEffect(curParams.GetEffectId());
    }

    public void OnFpsCollected(float fps)
    {
        EWeatherEffectQuality vquality = fps > GameConsts.averageFPS
            ? EWeatherEffectQuality.HighQuality
            : EWeatherEffectQuality.LowQuality;
        isFPSCollected = true;
        curParams.quality = vquality;
    }

    //暂时不显示天气，比如自拍的时候
    public void PauseShowWeather()
    {
        wec?.PauseWeatherEffect();
    }

    //恢复显示天气，比如自拍退出的时候
    public void ResumeShowWeather()
    {
        wec.ResumeWeatherEffect();
    }

    public void Init()
    {
        wec = new WeatherEffectController();
        wsc = new WeatherSoundController();
        curParams = new WeatherParams();
        WeatherEffectLibrary.Init();
    }

    public override void Release()
    {
        base.Release();
        WeatherEffectLibrary.Release();
    }
}
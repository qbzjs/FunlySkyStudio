public class WeatherParams
{
    public WeatherSaveParams saveParams = new WeatherSaveParams();
    public EWeatherEffectQuality quality = EWeatherEffectQuality.LowQuality;
  
    public EWeatherEffectId GetEffectId()
    {
        return WeatherEffectLibrary.QueryEffectId(this);
    }

    public override string ToString()
    {
        return $"weather[type = {saveParams.weatherType} quality = {quality}]";
    }
}
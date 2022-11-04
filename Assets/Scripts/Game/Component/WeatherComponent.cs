using Newtonsoft.Json;

public class WeatherComponent : IComponent
{
    public EWeatherType weatherType = EWeatherType.None;

    public IComponent Clone()
    {
        return null;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}
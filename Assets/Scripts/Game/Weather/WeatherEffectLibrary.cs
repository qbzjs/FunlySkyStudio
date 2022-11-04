using System.Collections.Generic;

public class WeatherEffectLibrary
{
    private static Dictionary<EWeatherEffectId, string> paths;
    public static EWeatherEffectId QueryEffectId(WeatherParams weatherParams)
    {
        if (weatherParams.saveParams.weatherType == EWeatherType.Rain)
        {
            return EWeatherEffectId.Rain;
        }
        if (weatherParams.saveParams.weatherType == EWeatherType.Snow)
        {
            return EWeatherEffectId.Snow;
        }

        return EWeatherEffectId.None;
    }

    public static string QueryEffectPath(EWeatherEffectId effectType)
    {
        if (paths == null)
        {
            return "";
        }
        if (paths.ContainsKey(effectType))
        {
            return paths[effectType];
        }
        return "";
    }

    public static void Init()
    {
        InitPaths();
    }

    private static void InitPaths()
    {
        List<WeatherEffectPathData> configs = ResManager.Inst.LoadJsonRes<List<WeatherEffectPathData>>("Configs/WeatherEffectPathConfig");
        paths = new Dictionary<EWeatherEffectId, string>();
        foreach (var config in configs)
        {
            paths[config.effectId] = config.path;
        }
    }

    public static void Release()
    {
        paths = null;
    }
}
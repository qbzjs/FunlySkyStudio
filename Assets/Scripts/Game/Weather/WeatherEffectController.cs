using System.Collections.Generic;
using UnityEngine;

public class WeatherEffectController
{
    private Dictionary<EWeatherEffectId, GameObject> weatherCache;
    private GameObject showing;

    public WeatherEffectController()
    {
        weatherCache = new Dictionary<EWeatherEffectId, GameObject>();
    }

    public void SyncWeatherEffect(EWeatherEffectId effectId)
    {
        if (weatherCache.ContainsKey(effectId) 
            && weatherCache[effectId] == showing && showing.activeSelf)
        {
            return;
        }

        HideShowing();
        if (effectId == EWeatherEffectId.None)
        {
            return;
        }

        if (weatherCache.ContainsKey(effectId))
        {
            weatherCache[effectId].SetActive(true);
            showing = weatherCache[effectId];
            return;
        }

        string effectPath = WeatherEffectLibrary.QueryEffectPath(effectId);
        GameObject prefabEffect = ResManager.Inst.LoadRes<GameObject>(effectPath);
        if (prefabEffect == null)
        {
            LoggerUtils.LogError(
                $"Load Weather Effect Error effectId = {effectId}");
            return;
        }

        Quaternion rotaSrc = prefabEffect.transform.rotation;
        GameObject effect = Object.Instantiate(prefabEffect, CameraUtils.Inst.GetMainCamera().transform);
        WeatherEffectPositionAdjust adjust = effect.AddComponent<WeatherEffectPositionAdjust>();
        adjust.SetRotaSrc(rotaSrc);
        adjust.anchor = CameraUtils.Inst.GetMainCamera().transform;
        showing = effect;
        weatherCache[effectId] = effect;
    }

    private void HideShowing()
    {
        if (showing != null && showing.activeSelf)
        {
            showing.SetActive(false);
            showing = null;
        }
    }

    public void PauseWeatherEffect()
    {
        if (showing != null && showing.activeSelf)
        {
            showing.SetActive(false);
        }
    }

    public void ResumeWeatherEffect()
    {
        if (showing != null && !showing.activeSelf)
        {
            showing.SetActive(true);
        }
    }
}
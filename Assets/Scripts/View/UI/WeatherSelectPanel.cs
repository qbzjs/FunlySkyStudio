using System;
using UnityEngine;
using UnityEngine.UI;

public class WeatherSelectPanel : InfoPanel<WeatherSelectPanel>
{
    private ToggleGroup tg;
    private Toggle tRain;
    private Toggle tSnow;
    private Button close;
    public Action OnCloseClick;
    
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        InitViews();
        EWeatherType weatherType = SceneBuilder.Inst.WeatherEntity.Get<WeatherComponent>().weatherType;
        tRain.isOn = weatherType == EWeatherType.Rain;
        tSnow.isOn = weatherType == EWeatherType.Snow;
        tRain.onValueChanged.AddListener(OnWeatherChange);
        tSnow.onValueChanged.AddListener(OnWeatherChange);
        close.onClick.AddListener(CloseClick);
    }

    private void InitViews()
    {
        tg = transform.Find("Root/Bg").GetComponent<ToggleGroup>();
        tRain = transform.Find("Root/Bg/RainParent/Rain/ToggleRain").GetComponent<Toggle>();
        tSnow = transform.Find("Root/Bg/SnowParent/Snow/ToggleSnow").GetComponent<Toggle>();
        close = transform.Find("Root/CloseParent/CloseButton").GetComponent<Button>();
    }

    private void CloseClick()
    {
        OnCloseClick?.Invoke();
    }

    private void OnWeatherChange(bool value)
    {
        var activeToggle = tg.GetFirstActiveToggle();
        if (activeToggle == null)
        {
            ChangeWeather(EWeatherType.None);
        }
        else
        {
            if (activeToggle == tRain)
            {
                ChangeWeather(EWeatherType.Rain);
            }else if (activeToggle == tSnow)
            {
                ChangeWeather(EWeatherType.Snow);
            }
        }
    }

    private void ChangeWeather(EWeatherType weatherType)
    {
        SceneBuilder.Inst.SetWeatherType(weatherType);
    }
}

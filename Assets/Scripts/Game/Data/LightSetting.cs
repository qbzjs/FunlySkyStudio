using UnityEngine;
using Newtonsoft.Json;

public class LightSetting
{
    public int gradientType;
    [JsonConverter(typeof(ColorConverter))]
    public Color sky;
    [JsonConverter(typeof(ColorConverter))]
    public Color equator;
    [JsonConverter(typeof(ColorConverter))]
    public Color ground;
    public float ambientIntensity;
    [JsonConverter(typeof(ColorConverter))]
    public Color dirctional;
    public float intensity;
    public int anglex;
    public int angley;
    public float reflectionIntensity;

}

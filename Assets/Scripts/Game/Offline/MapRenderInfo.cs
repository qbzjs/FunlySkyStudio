using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class MapRenderInfo
{
    public RuntimePlatform platform;
    public string version;
    public string occlusionUrl;
    
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime renderTime;
    
    public static readonly string V10 = "1.0";
}

public class MapRenderInfoConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(JsonConvert.SerializeObject((List<MapRenderInfo>)value));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return reader.Value != null ? JsonConvert.DeserializeObject<List<MapRenderInfo>>(reader.Value.ToString()) : null;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(List<MapRenderInfo>);
    }
}
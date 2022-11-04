using System;
using Newtonsoft.Json;
using UnityEngine;

public class ColorConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(DataUtils.ColorToString((Color)value));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.Value != null) return DataUtils.DeSerializeColor(reader.Value.ToString());
        return null;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Color);
    }
}
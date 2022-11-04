using System;
using Newtonsoft.Json;
using UnityEngine;

public class CustomDateTimeConverter : JsonConverter
{
    
    private static DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var tmpDateTime = (DateTime) value;
        writer.WriteValue(Math.Max( (long)((tmpDateTime - dt1970).TotalSeconds), 0));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return reader.Value != null ? dt1970.AddSeconds((long)reader.Value) : dt1970;
    } 

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(long) || objectType == typeof(ulong) || objectType == typeof(int) || objectType == typeof(uint);
    }
}
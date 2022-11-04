using System;
using Newtonsoft.Json;
using UnityEngine;

public class BoundsConverter : JsonConverter
{
    [Serializable]
    private class BoundsData
    {
        [SerializeField]
        public string center;
    
        [SerializeField]
        public string extents;

        public BoundsData(Bounds bounds)
        {
            center = DataUtils.Vector3ToString(bounds.center);
            extents = DataUtils.Vector3ToString(bounds.extents);
        }
        public Bounds Get()
        {
            var centerPos = DataUtils.DeSerializeVector3(center);
            var extentsSize = DataUtils.DeSerializeVector3(extents);
            return new Bounds() { center = centerPos, extents = extentsSize};;
        }
    }
    
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        
        writer.WriteValue(JsonConvert.SerializeObject(new BoundsData((Bounds)value)));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return reader.Value != null ? JsonConvert.DeserializeObject<BoundsData>(reader.Value.ToString())?.Get() : null;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Bounds);
    }
}

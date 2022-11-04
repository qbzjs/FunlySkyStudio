using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using UnityEngine;

namespace GameData
{

    public struct JsonLine
    {
        public string key;
        public string value;

        public string ToJson()
        {
            return "\""+ key + "\":" + value + "";
        }
    }

    [SerializeField]
    public struct ImagedData
    {
        public Sprite img;
        public string data;
        public string diyId;
    }

    [SerializeField]
    public struct LightData
    {
        public int litype;
		public string pos;
        public string rot;
        public float Int;
        public int anx;
        public int any;
        public float rng;
        public float spoa;
        public string lico;
    }

    public class DataUtils
    {
        public static string ToJson(IEnumerable<JsonLine> lines)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            foreach(JsonLine line in lines)
            {
                sb.Append(line.ToJson());
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1); //remove last comma
            sb.Append("}");
            return sb.ToString();
        }

        public static string StructToString(string val)
        {
            string newval = "\"" + val + "\"";
            return newval;
        }
        public static string ToJsonArray(IEnumerable<string> jsons)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            foreach(string json in jsons)
            {
                sb.Append(json);
                sb.Append(",");
            }
            if (sb.Length == 1) return "[]";
            sb.Remove(sb.Length - 1, 1); //remove last comma
            sb.Append("]");
            return sb.ToString();
        }

        public static string SerializeVector3(Vector3 target)
        {
            //TODO
            string str = target.ToString("f4");
            str = str.Replace('(', '\"');
            str = str.Replace(')', '\"');
            return str;
        }

        public static string Vector3ToString(Vector3 target)
        {
            string str = target.ToString("f4");
            str = str.Substring(1, str.Length - 2);
            return str;
        }

        public static Vector3 DeSerializeVector3(string target)
        {
            string[] split = target.Split(',');
            float x = float.Parse(split[0], CultureInfo.InvariantCulture);
            float y = float.Parse(split[1], CultureInfo.InvariantCulture);
            float z = float.Parse(split[2], CultureInfo.InvariantCulture);
            return new Vector3(x, y, z);
        }

        public static string SerializeQuaternion(Quaternion target)
        {
            return SerializeVector3(target.eulerAngles);
        }

        public static string QuaternionToString(Quaternion target)
        {
            return Vector3ToString(target.eulerAngles);
        }

        public static Quaternion DeSerializeQuaternion(string target)
        {
            Vector3 eulars = DeSerializeVector3(target);
            return Quaternion.Euler(eulars);
        }

        public static string ColorToString(Color target)
        {
            return ColorUtility.ToHtmlStringRGB(target);
        }

        public static string SerializeColor(Color target)
        {
            return "\"" + ColorUtility.ToHtmlStringRGB(target) + "\"";
        }
        public static Color DeSerializeColorByHex(string target)
        {
            bool parseSuccess = ColorUtility.TryParseHtmlString(target, out Color color);
            if (parseSuccess)
            {
                return color;
            }
            else
            {
                return default;
            }

        }
        public static Color DeSerializeColor(string target)
        {
            bool parseSuccess = ColorUtility.TryParseHtmlString("#"+target, out Color color);
            if (parseSuccess)
            {
                return color;
            }
            else
            {
                return default;
            }

        }
        /*
        public static Texture ToSprite(byte[] data)
        {

            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(data);
            return texture;
        }*/

        public static Sprite ToSprite(byte[] data, int size = 500)
        {
            Texture2D texture = new Texture2D(size, size);
            texture.LoadImage(data);
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            //Object.Destroy(texture);
            return newSprite;
        }

    }

}

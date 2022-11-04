/// <summary>
/// Author: YangJie
/// Description:
/// Date: 2022/5/16 13:7:38
/// </summary>
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Author:YangJie
/// Description:
/// Date: Hash 公共类
/// </summary>
public class HashUtils
{
    /// <summary>
    /// for custom class need [Serializable]
    /// to ignore https://stackoverflow.com/questions/33489930/
    /// ignore-non-serialized-property-in-binaryformatter-serialization
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] Byte(object value)
    {
        /*https://stackoverflow.com/questions/1446547/
          how-to-convert-an-object-to-a-byte-array-in-c-sharp*/
        using (var ms = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, value == null ? "null" : value);
            return ms.ToArray();
        }
    }

    public static byte[] Hash(byte[] value)
    {
        /*https://support.microsoft.com/en-za/help/307020/
          how-to-compute-and-compare-hash-values-by-using-visual-cs*/
        /*https://andrewlock.net/why-is-string-gethashcode-
          different-each-time-i-run-my-program-in-net-core*/
        byte[] result = MD5.Create().ComputeHash(value);
        return result;
    }

    public static byte[] Combine(params byte[][] values)
    {
        /*https://stackoverflow.com/questions/415291/
          best-way-to-combine-two-or-more-byte-arrays-in-c-sharp*/
        byte[] rv = new byte[values.Sum(a => a.Length)];
        int offset = 0;
        foreach (byte[] array in values)
        {
            System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
            offset += array.Length;
        }

        return rv;
    }

    public static string String(byte[] hash)
    {
        /*https://stackoverflow.com/questions/1300890/
          md5-hash-with-salt-for-keeping-password-in-db-in-c-sharp*/
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2")); /*do not make it X2*/
        }

        var result = sb.ToString();
        return result;
    }

    public static byte[] Hash(params object[] values)
    {
        byte[][] bytes = new byte[values.Length][];
        for (int i = 0; i < values.Length; i++)
        {
            bytes[i] = Byte(values[i]);
        }

        byte[] combined = Combine(bytes);
        byte[] combinedHash = Hash(combined);
        return combinedHash;
    }

    /*https://stackoverflow.com/questions/5868438/c-sharp-generate-a-random-md5-hash*/
    public static string HashString(string value, Encoding encoding = null)
    {
        if (encoding == null)
        {
            encoding = Encoding.ASCII;
        }

        byte[] bytes = encoding.GetBytes(value);
        byte[] hash = Hash(bytes);
        string result = String(hash);
        return result;
    }

    public static string HashString(params object[] values)
    {
        var hash = Hash(values); /*Add more not constant properties as needed*/
        var value = String(hash);
        return value;
    }
}
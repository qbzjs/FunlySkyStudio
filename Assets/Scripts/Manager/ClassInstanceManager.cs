using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CInstanceManager
{
    private static Dictionary<string, BaseInstance> allInstances = new Dictionary<string, BaseInstance>();
    public static T CreateInstance<T>(string typeName) where T : BaseInstance, new()
    {
        if (!allInstances.ContainsKey(typeName) || allInstances[typeName] == null)
        {
            var instance = new T();
            allInstances.Add(typeName, instance);
        }
        return allInstances[typeName] as T;
    }

    public static void Release()
    {
        if (allInstances.Count > 0)
        {
            foreach (var ins in allInstances.Values)
            {
                ins?.Release();
            }
        }
        allInstances.Clear();
    }
}
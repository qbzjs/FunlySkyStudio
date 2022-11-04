using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Author:YangJie
/// Description:
/// Date: #CreateTime#
/// </summary>
public class LRUManager<T>: CInstance<LRUManager<T>> where T : BaseLRUInfo, new()
{
    private string fileName;
    private ulong curSize;
    private ulong maxSize = 1024 * 1024 * 50;
    private QuickDLinkList<T, string> qDList;
    public LRUManager()
    {
        fileName =  LRUFilePath();
        curSize = 0;
        qDList = new QuickDLinkList<T, string>();
    }

    public void Init(ulong size)
    {
        maxSize = size;
        var targetPath = Path.Combine(DataUtils.dataDir,fileName);
        Clear();
        if(!File.Exists(targetPath)) return;
        var strs = File.ReadAllText(targetPath);
        if(string.IsNullOrEmpty(strs)) 
        {
            LoggerUtils.LogError(string.Format("LRUManager {0} is empty !!!",fileName));
            SaveJson();
            return;
        }
        try
        {
            var lruInfos = JsonConvert.DeserializeObject<List<T>>(strs);
            for (var i = lruInfos.Count - 1; i >= 0; i--)
            {
                Put(lruInfos[i]);
            }
        }
        catch (System.Exception e)
        {
            LoggerUtils.LogError(string.Format("LRUManager {0} init deserialize exception !!! {1}",fileName,e));
            SaveJson();
        }
    }

    public void Put(T lruInfo)
    {
        if(qDList.Find(lruInfo.key))
        {   
            var tmpVal = qDList.GetVal(lruInfo.key);
            if (!tmpVal.Equals(lruInfo))
            {
                curSize += lruInfo.size - tmpVal.size;
                tmpVal.Delete();
                qDList.DeleteQDNode(qDList.GetNode(lruInfo.key));
            }
        }
        else
        {
            curSize += lruInfo.size;
        }
        qDList.AddFirstQDNode(lruInfo, lruInfo.key);
        while (curSize > maxSize)
        {
            var p = qDList.GetLast();
            if(p == null) break;
            qDList.DeleteQDNode(p);
            p.val.Delete();
            curSize -= p.val.size;
        }
    }

    public T Get(string key)
    {
        var val = qDList.GetVal(key);
        if(val == null)return null;

        qDList.DeleteQDNode(qDList.GetNode(key));
        qDList.AddFirstQDNode(val,key);
        return val;
    }

    public string LRUFilePath()
    {
        return "LRUCache_" + typeof(T).Name + ".json";
    }
    public void SaveJson()
    {
        if(!Directory.Exists(DataUtils.dataDir))
            Directory.CreateDirectory(DataUtils.dataDir);
        File.WriteAllText(Path.Combine(DataUtils.dataDir, fileName),JsonConvert.SerializeObject(qDList.ToList()));
    }

    public void Clear()
    {
        qDList?.Clear();
        curSize = 0;
    }
    public ulong GetCurSize()
    {
        return curSize;
    }
    public override void Release()
    {
        base.Release();
        SaveJson();
        Clear();
    }
}

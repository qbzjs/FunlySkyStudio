using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class UGCTexLRUInfo : BaseLRUInfo
{
    
    protected virtual string localSavePath => Application.persistentDataPath + "/Offline/UGCTex/";
    public const ulong MaxSize = 1024 * 1024 * 50;
    public string cacheFilePath;

    public override void Delete()
    {
        var path = Path.Combine(localSavePath, cacheFilePath);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
 
    public override bool Equals(object obj)
    {
        if (!base.Equals(obj))
        {
            return false;
        }
        return cacheFilePath == (obj as UGCTexLRUInfo)?.cacheFilePath;
    }
}